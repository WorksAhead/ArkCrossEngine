using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby {

  public enum MissionStateType
  {
    DEFAULT,
    LOCKED,
    UNCOMPLETED,
    COMPLETED,
  }
  internal class MissionStateInfo
  {
    internal ConcurrentDictionary<int, MissionInfo> MissionList
    {
      get { return m_Missions; }
    }

    internal MissionInfo GetMissionInfoById(int id, MissionStateType state = MissionStateType.DEFAULT)
    {
      MissionInfo info;
      if (m_Missions.TryGetValue(id, out info)) {
        if (MissionStateType.DEFAULT == state || info.State == state) {
          return info;
        } else {
          ArkCrossEngine.LogSystem.Warn("Try to get {0} mission {1} which is {2}", state, id, info.State);
        }
      }
      return info;
    }

    internal bool IsMissionFinished(int id)
    {
      MissionInfo info;
      if (m_Missions.TryGetValue(id, out info) && MissionStateType.COMPLETED == info.State) {
        return true;
      }
      return false;
    }

    internal bool AddMission(int id, MissionStateType type, int progressValue = 0) {
      bool result = false;
      switch (type) {
        case MissionStateType.LOCKED:
          result = AddLockedMissions(id, progressValue);
          break;
        case MissionStateType.COMPLETED:
          result = AddCompletedMission(id, progressValue);
          break;
        case MissionStateType.UNCOMPLETED:
          result = AddUnCompletedMission(id, progressValue);
          break;
      }
      return result;
    }

    internal bool CompleteMission(int missionId) {
      MissionInfo info;
      if (!m_Missions.TryGetValue(missionId, out info)) {
        LogSystem.Warn("CompleteMission:: want to completed unkonwed missionId {0}", missionId);
        return false;
      }else if(MissionStateType.UNCOMPLETED != info.State){
        LogSystem.Warn("CompleteMission:: want to completed a completed missionId {0}", missionId);
        return false;
      } else {
        info.State = MissionStateType.COMPLETED;
        return true;
      }
    }

    internal bool UnLockMission(int missionId) {
      bool result = false;
      MissionInfo info;
      if (!m_Missions.TryGetValue(missionId, out info)) {
        LogSystem.Warn("UnLockMission::Try to unlock an unknowned mission {0}", missionId);
      } else {
        info.State = MissionStateType.UNCOMPLETED;
      }
      return result;
    }

    internal bool RemoveMission(int missionId)
    {
      MissionInfo missionInfo;
      return m_Missions.TryRemove(missionId, out missionInfo);
    }
    internal bool AddUnCompletedMission(int id, int progressValue) {
      MissionInfo info;
      if (m_Missions.TryGetValue(id, out info)) {
        info.State = MissionStateType.UNCOMPLETED;
        info.CurValue = progressValue;
      } else {
        info = new MissionInfo(id);
        info.State = MissionStateType.UNCOMPLETED;
        info.CurValue = progressValue;
        m_Missions.TryAdd(id, info);
      }
      return true;
    }

    internal bool AddLockedMissions(int id, int progressValue) {
      MissionInfo info;
      if (m_Missions.TryGetValue(id, out info)) {
        info.State = MissionStateType.LOCKED;
        info.CurValue = progressValue;
      } else {
        info = new MissionInfo(id);
        info.State = MissionStateType.LOCKED;
        info.CurValue = progressValue;
        m_Missions.TryAdd(id, info);
      }
      return true;
    }

    internal bool AddCompletedMission(int id, int progressValue) {
      MissionInfo info;
      if (m_Missions.TryGetValue(id, out info)) {
        info.State = MissionStateType.COMPLETED;
        info.CurValue = progressValue;
      } else {
        info = new MissionInfo(id);
        info.State = MissionStateType.COMPLETED;
        info.CurValue = progressValue;
        m_Missions.TryAdd(id, info);
      }
      return true;
    }

    internal void ResetDailyMissions()
    {
      foreach (int missionId in MissionConfigProvider.Instance.GetDailyMissionId()) {
        MissionInfo info;
        if (m_Missions.TryGetValue(missionId, out info)) {
          info.Reset();
        } else {
          AddMission(missionId, MissionStateType.UNCOMPLETED);
        }
      }
    }
    internal int GetMissionsExpReward(int missionId, int userLevel)
    {
      int result = 0;
      MissionConfig mc = MissionConfigProvider.Instance.GetDataById(missionId);
      if (null != mc) {
        Data_SceneDropOut dropOutConfig = SceneConfigProvider.Instance.GetSceneDropOutById(mc.DropId);
        if (null != dropOutConfig) {
          if (mc.MissionType == (int)MissionType.DAILY && dropOutConfig.m_Exp > 0) {
            if (userLevel < 21) {
              // 21级以下
              result = 120;
            }else if (userLevel < 24) {
              // 24级以下
              result = userLevel * 15;
            } else {
              result = (int)((0.0097 * Math.Pow(userLevel, 4) - 1.6977 * Math.Pow(userLevel, 3) + 106.88 * Math.Pow(userLevel, 2) - 2523.5 * userLevel + 19699) * 1);
            }
          } else {
            result = dropOutConfig.m_Exp;
          }
        }
      }
      return result;
    }

    internal void Reset()
    {
      m_Missions.Clear();
    }
    internal Dictionary<int, MissionInfo> UnCompletedMissions
    {
      get
      {
        Dictionary<int, MissionInfo> result = new Dictionary<int, MissionInfo>();
        foreach (MissionInfo mi in m_Missions.Values) {
          if (MissionStateType.UNCOMPLETED == mi.State) {
            result.Add(mi.MissionId, mi);
          }
        }
        return result;
      }
    }
    internal Dictionary<int, MissionInfo> CompletedMissions
    {
      get
      {
        Dictionary<int, MissionInfo> result = new Dictionary<int, MissionInfo>();
        foreach (MissionInfo mi in m_Missions.Values) {
          if (MissionStateType.COMPLETED == mi.State) {
            result.Add(mi.MissionId, mi);
          }
        }
        return result;
      }
    }

    internal Dictionary<int, MissionInfo> LockedMissions
    {
      get
      {
        Dictionary<int, MissionInfo> result = new Dictionary<int, MissionInfo>();
        foreach (MissionInfo mi in m_Missions.Values) {
          if (MissionStateType.LOCKED == mi.State) {
            result.Add(mi.MissionId, mi);
          }
        }
        return result;
      }
    }
    private ConcurrentDictionary<int, MissionInfo> m_Missions = new ConcurrentDictionary<int, MissionInfo>();
  }
}
