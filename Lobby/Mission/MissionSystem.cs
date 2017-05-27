using System;
using DashFire;
using System.Collections.Generic;
using ArkCrossEngine;

namespace Lobby
{
  internal class StageClearInfo
  {
    internal int SceneId = 0;
    internal int Level = 0;
    internal int HitCount = 0;
    internal int MaxMultHitCount = 0;
    internal long Duration = 0;
    internal int DeadCount = 0;
    internal int SweepCount = 1;
  }
  internal class MissionSystem
  {
    private enum MissionHandlerId
    {
      LEVEL = 1,
      SCENE = 2,
      TIME = 3,
      HIT_TIME = 4,
      DEAD_COUNT = 5,
      AUTO_FINISH = 6,
      ANY_SCENE = 7,
      SCENE_STAR = 8,
    }
    private delegate bool UpdateMissionDelegate(UserInfo user, MissionInfo mission, StageClearInfo info = null);
    private delegate string GetProgressDelegate(UserInfo user, MissionInfo mission, bool isFinished);

    internal void OnStageClear(UserInfo user, StageClearInfo info, ref List<int> completedMissions)
    {
      // 任务完成,新任务解锁
      foreach (MissionInfo missionInfo in user.Mission.UnCompletedMissions.Values) {
        if (null != missionInfo && null != missionInfo.Config) {
          if (user.Level >= missionInfo.Config.LevelLimit) {
            UpdateMissionDelegate handler = GetHandlerById(missionInfo.FinishType);
            if (null != handler) {
              bool result = handler(user, missionInfo, info);
              if (result) {
                completedMissions.Add(missionInfo.MissionId);
              }
            }
          }
        }
      }
      for (int i = 0; i < completedMissions.Count; ++i)
        CompleteMission(user, completedMissions[i]);
    }

    internal void CheckAndSyncMissions(UserInfo user, StageClearInfo info = null)
    {
      List<int> completedMissions = new List<int>();
      OnStageClear(user, info, ref completedMissions);
      foreach (int missionId in completedMissions) {
        JsonMessageWithGuid missionCompletedMsg = new JsonMessageWithGuid(JsonMessageID.MissionCompleted);
        missionCompletedMsg.m_Guid = user.Guid;
        ArkCrossEngineMessage.Msg_LC_MissionCompleted missionProtoData = new ArkCrossEngineMessage.Msg_LC_MissionCompleted();
        if (null != missionProtoData) {
          missionProtoData.m_MissionId = missionId;
          missionProtoData.m_Progress = MissionSystem.Instance.GetMissionProgress(user, missionId, true);
          missionCompletedMsg.m_ProtoData = missionProtoData;
          JsonMessageDispatcher.SendDcoreMessage(user.NodeName, missionCompletedMsg);
        }
      }
    }
    internal  bool IsMissionFinish(UserInfo user, int missionId)
    {
      MissionInfo mi = user.Mission.GetMissionInfoById(missionId);
      if (null != mi) {
        if (MissionStateType.COMPLETED == mi.State) {
          return true;
        } else {
          UpdateMissionDelegate handler = GetHandlerById(mi.FinishType);
          if (null != handler) {
            return handler(user, mi);
          }
        }
      }
      return false;
    }
    internal void ResetDailyMissions(UserInfo user)
    {
      foreach (int missionId in MissionConfigProvider.Instance.GetDailyMissionId()) {
        MissionInfo mi = user.Mission.GetMissionInfoById(missionId);
        if (null != mi) {
          mi.Reset();
        } else {
          user.Mission.AddMission(missionId, MissionStateType.UNCOMPLETED);
        }
        if (IsMissionFinish(user, missionId)) {
          MissionInfo missionInfo = user.Mission.GetMissionInfoById(missionId);
          if (null != missionInfo) {
            missionInfo.State = MissionStateType.COMPLETED;
          }
        }
      }
    }
    internal void ResetMonthCardMissions(UserInfo user)
    {
      foreach (int missionId in MissionConfigProvider.Instance.GetMonthCardMissionId()) {
        if (user.IsHaveMonthCard()) {
          MissionInfo mi = user.Mission.GetMissionInfoById(missionId);
          if (null != mi) {
            mi.Reset();
          } else {
            user.Mission.AddMission(missionId, MissionStateType.UNCOMPLETED);
          }
          if (IsMissionFinish(user, missionId)) {
            MissionInfo missionInfo = user.Mission.GetMissionInfoById(missionId);
            if (null != missionInfo) {
              missionInfo.State = MissionStateType.COMPLETED;
            }
          }
        } else {
          user.Mission.RemoveMission(missionId);
        }
      }
    }
    
    internal void SyncMissionList(UserInfo user)
    {
      JsonMessageResetDailyMissions gm_msg = new JsonMessageResetDailyMissions();
      gm_msg.m_Missions = new MissionInfoForSync[user.Mission.CompletedMissions.Count + user.Mission.UnCompletedMissions.Count];
      int count = 0;
      foreach (MissionInfo mi in user.Mission.MissionList.Values) {
        MissionInfoForSync mis = new MissionInfoForSync();
        if (MissionStateType.COMPLETED == mi.State) {
          mis.m_IsCompleted = true;
        } else if (MissionStateType.UNCOMPLETED == mi.State) {
          mis.m_IsCompleted = false;
        } else {
          continue;
        }
        mis.m_MissionId = mi.MissionId;
        mis.m_Progress = MissionSystem.Instance.GetMissionProgress(user, mi, mis.m_IsCompleted);
        gm_msg.m_Missions[count] = mis;
        count++;
      }
      gm_msg.m_Guid = user.Guid;
      JsonMessageDispatcher.SendDcoreMessage(user.NodeName, gm_msg);
    }

    internal string GetMissionProgress(UserInfo user, int missionId, bool isFinished)
    {
      if (null != user && null != user.Mission) {
        MissionInfo mission = user.Mission.GetMissionInfoById(missionId);
        return GetMissionProgress(user, mission, isFinished);
      }
      return "";
    }
    internal string GetMissionProgress(UserInfo user, MissionInfo info, bool isFinished)
    {
      if (null != info) {
        GetProgressDelegate handler = GetProgressHandlerById(info.FinishType);
        if (null != handler) {
          return handler(user, info, isFinished);
        }
      }
      return "";
    }

    private void CompleteMission(UserInfo user, int missionId)
    {
      user.Mission.CompleteMission(missionId);
      LogSys.Log(LOG_TYPE.DEBUG, "user {0} completed mission {1}.", user.Guid, missionId);
    }


    #region Complete mission handler
    private bool MissionHandler_Level(UserInfo user, MissionInfo mission, StageClearInfo info = null)
    {
      if (null != mission) {
        mission.CurValue = user.Level;
        if (mission.Param0 <= user.Level) {
          return true;
        }
      }
      return false;
    }

    private bool MissionHandler_Scene(UserInfo user, MissionInfo mission, StageClearInfo info = null)
    {
      if (null != info) {
        if (null != mission && mission.Param0 == info.SceneId) {
          mission.CurValue = mission.CurValue + info.SweepCount;
          if (mission.CurValue >= mission.Param1) {
            return true;
          }
        }
      } else {
        if (null != mission && mission.FinishType != (int)MissionType.DAILY) {
          if (user.GetSceneInfo(mission.Param0) > 0) {
            return true;
          }
        }
      }
      return false;
    }

    private bool MissionHandler_Time(UserInfo user, MissionInfo mission, StageClearInfo info = null)
    {
      if (null != mission && null != info && mission.SceneId == info.SceneId && mission.Param0 >= info.Duration / 1000) {
        return true;
      }
      return false;
    }

    private bool MissionHandler_HitCount(UserInfo user, MissionInfo mission, StageClearInfo info = null)
    {
      if (null != mission && null != info && mission.SceneId == info.SceneId && mission.Param0 >= info.HitCount) {
        return true;
      }
      return false;
    }

    private bool MissionHandler_DeadCount(UserInfo user, MissionInfo mission, StageClearInfo info = null)
    {
      if (null != mission && null != info && mission.SceneId == info.SceneId && mission.Param0 >= info.DeadCount) {
        return true;
      }
      return false;
    }
    private bool MissionHandler_AutoFinish(UserInfo user, MissionInfo mission, StageClearInfo info = null)
    {
      return true;
    }
    private bool MissionHandler_AnyScene(UserInfo user, MissionInfo mission, StageClearInfo info = null)
    {
      if (null != mission && null != info) {
        mission.CurValue = mission.CurValue + info.SweepCount;
        if (mission.CurValue >= mission.Param0) {
          return true;
        } else {
          return false;
        }
      }
      return false;
    }
    private bool MissionHandler_SceneStar(UserInfo user, MissionInfo mission, StageClearInfo info = null)
    {
      if (null != user && null != mission) {
        if (user.GetSceneInfo(mission.Config.Args0) >= mission.Config.Args1) {
          return true;
        }
      }
      return false;
    }
    private UpdateMissionDelegate GetHandlerById(int id)
    {
      UpdateMissionDelegate ret;
      m_MissionHandlers.TryGetValue(id, out ret);
      return ret;
    }
    #endregion

    #region Mission progress handler
    private string ProgressHandler_Level(UserInfo user, MissionInfo mission, bool isFinished)
    {
      /*
      if(null != mission)
      {
        if (isFinished) {
          return String.Format("{0} / {1}", user.Level, mission.Param0);
        } else {
          return String.Format("{0} / {1}", user.Level, mission.Param0);
        }
      }
       */
      return "";
    }

    private string ProgressHandler_Scene(UserInfo user, MissionInfo mission, bool isFinished)
    {
      /*
      if (null != mission) {
        if (isFinished) {
          return String.Format("{0} / {1}", mission.CurValue, mission.Param1);
        }
      }
       */
        return "";
    }

    private string ProgressHandler_Common(UserInfo user, MissionInfo mission, bool isFinished)
    {
      /*
      if (isFinished) {
        return "1 / 1";
      } else {
        return "0 / 1";
      }
       */
      if (mission.Type == MissionType.MonthCard) {
        TimeSpan deltaTime = user.MonthCardExpiredTime - DateTime.Now;
        return deltaTime.Days.ToString();
      }
      return "";
    }
    private string ProgressHandler_AnyScene(UserInfo user, MissionInfo mission, bool isFinished)
    {
      //return String.Format("{0} / {1}", mission.CurValue, mission.Param0);
      return "";
    }
    
    private GetProgressDelegate GetProgressHandlerById(int id)
    {
      GetProgressDelegate ret;
      m_MissionProgressHandlers.TryGetValue(id, out ret);
      return ret;
    }
    #endregion

    private MissionSystem()
    {
      // complete mission handler
      m_MissionHandlers.Add((int)MissionHandlerId.LEVEL, MissionHandler_Level);
      m_MissionHandlers.Add((int)MissionHandlerId.SCENE, MissionHandler_Scene);
      m_MissionHandlers.Add((int)MissionHandlerId.TIME, MissionHandler_Time);
      m_MissionHandlers.Add((int)MissionHandlerId.HIT_TIME, MissionHandler_HitCount);
      m_MissionHandlers.Add((int)MissionHandlerId.DEAD_COUNT, MissionHandler_DeadCount);
      m_MissionHandlers.Add((int)MissionHandlerId.AUTO_FINISH, MissionHandler_AutoFinish);
      m_MissionHandlers.Add((int)MissionHandlerId.ANY_SCENE, MissionHandler_AnyScene);
      m_MissionHandlers.Add((int)MissionHandlerId.SCENE_STAR, MissionHandler_SceneStar);
      // get progress handler
      m_MissionProgressHandlers.Add((int)MissionHandlerId.LEVEL, ProgressHandler_Level);
      m_MissionProgressHandlers.Add((int)MissionHandlerId.SCENE, ProgressHandler_Scene);
      m_MissionProgressHandlers.Add((int)MissionHandlerId.HIT_TIME, ProgressHandler_Common);
      m_MissionProgressHandlers.Add((int)MissionHandlerId.TIME, ProgressHandler_Common);
      m_MissionProgressHandlers.Add((int)MissionHandlerId.DEAD_COUNT, ProgressHandler_Common);
      m_MissionProgressHandlers.Add((int)MissionHandlerId.AUTO_FINISH, ProgressHandler_Common);
      m_MissionProgressHandlers.Add((int)MissionHandlerId.ANY_SCENE, ProgressHandler_AnyScene);
      m_MissionProgressHandlers.Add((int)MissionHandlerId.SCENE_STAR, ProgressHandler_Common);
    }

    internal double LastResetDaliyMissionsTimeStamp
    {
      get { return m_LastResetDaliyMissionsTimeStamp; }
      set { m_LastResetDaliyMissionsTimeStamp = value; }
    }
    internal static MissionSystem Instance
    {
      get { return s_Instance; }
    }
    private double m_LastResetDaliyMissionsTimeStamp = 0;
    private Dictionary<int, UpdateMissionDelegate> m_MissionHandlers = new Dictionary<int, UpdateMissionDelegate>();
    private Dictionary<int, GetProgressDelegate> m_MissionProgressHandlers = new Dictionary<int, GetProgressDelegate>();
    private static MissionSystem s_Instance = new MissionSystem();
  }
}
