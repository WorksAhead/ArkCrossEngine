using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using ArkCrossEngine;
using ArkCrossEngineMessage;
using ArkCrossEngine.Network;
using System.Collections;

internal class MsgPongHandler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_Pong pong_msg = msg as Msg_Pong;
    if (pong_msg == null) {
      return;
    }
    long time = TimeUtility.GetLocalMilliseconds();
    networkSystem.OnPong(time, pong_msg.send_ping_time, pong_msg.send_pong_time);
  }
}

internal class MsgShakeHandsRetHandler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_ShakeHands_Ret ret_msg = msg as Msg_RC_ShakeHands_Ret;
    if (msg == null) {
      return;
    }
    if (ret_msg.auth_result == Msg_RC_ShakeHands_Ret.RetType.SUCCESS) {
      networkSystem.CanSendMessage = true;      
      LogSystem.Debug("{0} auth ok !!! {1}", networkSystem.Robot.LobbyNetworkSystem.User, LobbyRobot.Robot.GetDateTime());

      ArkCrossEngineMessage.Msg_CRC_Create build = new ArkCrossEngineMessage.Msg_CRC_Create();
      networkSystem.SendMessage(build);
      LogSystem.Debug("{0} send Msg_CRC_Create to roomserver {1}", networkSystem.Robot.LobbyNetworkSystem.User, LobbyRobot.Robot.GetDateTime());
    } else {
      LogSystem.Debug("{0} auth failed !!! {1}", networkSystem.Robot.LobbyNetworkSystem.User, LobbyRobot.Robot.GetDateTime());
      networkSystem.WaitDisconnect();
    }
  }
}

internal class Msg_CRC_Create_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_Create enter = msg as Msg_CRC_Create;
    if (null == enter) {
      return;
    }
    if (enter.is_player_self) {
      networkSystem.Robot.NotifyUserEnter();
      networkSystem.Robot.MyselfId = enter.role_id;
      networkSystem.Robot.ConfigId = enter.hero_id;
      networkSystem.Robot.MyselfCampId = enter.camp_id;
    } else {
      List<int> otherIds = networkSystem.Robot.OtherIds;
      if (CharacterRelation.RELATION_ENEMY == LobbyRobot.Robot.GetRelation(enter.camp_id, networkSystem.Robot.MyselfCampId)) {
        if (!otherIds.Contains(enter.role_id)) {
          otherIds.Add(enter.role_id);
        }
      }
    }
  }
}

internal class Msg_RC_Enter_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_Enter enter = msg as Msg_RC_Enter;
    if (null == enter) {
      return;
    }
  }
}

internal class Msg_RC_Disappear_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_Disappear disappear = msg as Msg_RC_Disappear;
    if (disappear == null) {
      return;
    }
  }
}

internal class Msg_RC_Dead_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_Dead dead = msg as Msg_RC_Dead;
    if (dead == null) {
      return;
    }
  }
}

internal class Msg_RC_Revive_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_Revive revive = msg as Msg_RC_Revive;
    if (revive == null) {
      return;
    }
  }
}

internal class Msg_CRC_Exit_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_Exit targetmsg = msg as Msg_CRC_Exit;
    if (null == targetmsg) {
      return;
    }
    List<int> otherIds = networkSystem.Robot.OtherIds;
    otherIds.Remove(targetmsg.role_id);

    if (networkSystem.Robot.MyselfId == targetmsg.role_id) {
      networkSystem.QuitBattlePassive();
    }
  }
}

internal class Msg_CRC_Move_Handler
{
  internal static void OnMoveStart(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_MoveStart move_msg = msg as Msg_CRC_MoveStart;
    if (null == move_msg)
      return;
  }

  internal static void OnMoveStop(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_MoveStop move_msg = msg as Msg_CRC_MoveStop;
    if (null == move_msg) {
      return;
    }
  }
}

internal class Msg_CRC_MoveMeetObstacle_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_MoveMeetObstacle obstacle_msg = msg as Msg_CRC_MoveMeetObstacle;
    if (null == obstacle_msg) {
      return;
    }
  }
}

internal class Msg_CRC_Face_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_Face face_msg = msg as Msg_CRC_Face;
    if (null == face_msg) {
      return;
    }
  }
}

internal class Msg_CRC_Skill_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_Skill skill_msg = msg as Msg_CRC_Skill;
    if (null == skill_msg) {
      return;
    }
  }
}

internal class Msg_CRC_StopSkill_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_StopSkill skill_msg = msg as Msg_CRC_StopSkill;
    if (null == skill_msg) {
      return;
    }
  }
}

internal class Msg_RC_UserMove_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_UserMove targetmsg = msg as Msg_RC_UserMove;
    if (null == targetmsg) {
      return;
    }
  }
}

internal class Msg_RC_UserFace_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_UserFace targetmsg = msg as Msg_RC_UserFace;
    if (null == targetmsg) {
      return;
    }
  }
}

internal class Msg_RC_CreateNpc_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_CreateNpc targetmsg = msg as Msg_RC_CreateNpc;
    if (null == targetmsg) {
      return;
    }
    if (targetmsg.owner_id == networkSystem.Robot.MyselfId) {
      List<int> ownedNpcs = networkSystem.Robot.OwnedNpcs;
      if (!ownedNpcs.Contains(targetmsg.npc_id)) {
        ownedNpcs.Add(targetmsg.npc_id);
      }
    }
    List<int> otherIds = networkSystem.Robot.OtherIds;
    if (CharacterRelation.RELATION_ENEMY == LobbyRobot.Robot.GetRelation(targetmsg.camp_id, networkSystem.Robot.MyselfCampId)) {
      if (!otherIds.Contains(targetmsg.npc_id)) {
        otherIds.Add(targetmsg.npc_id);
      }
    }
  }
}

internal class Msg_RC_DestroyNpc_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_DestroyNpc destroyMsg = msg as Msg_RC_DestroyNpc;
    if (destroyMsg == null) {
      return;
    }
    List<int> ownedNpcs = networkSystem.Robot.OwnedNpcs;
    ownedNpcs.Remove(destroyMsg.npc_id);
    List<int> otherIds = networkSystem.Robot.OtherIds;
    otherIds.Remove(destroyMsg.npc_id);
  }
}

internal class Msg_RC_NpcEnter_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_NpcEnter targetmsg = msg as Msg_RC_NpcEnter;
    if (null == targetmsg) {
      return;
    }
  }
}

internal class Msg_RC_NpcMove_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_NpcMove targetmsg = msg as Msg_RC_NpcMove;
    if (null == targetmsg) {
      return;
    }
  }
}

internal class Msg_RC_NpcFace_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_NpcFace face_msg = msg as Msg_RC_NpcFace;
    if (null == face_msg) {
      return;
    }
  }
}

internal class Msg_RC_NpcTarget_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_NpcTarget targetmsg = msg as Msg_RC_NpcTarget;
    if (null == targetmsg) {
      return;
    }
    LogSystem.Debug("NpcTarget, npc:{0} target:{1} robot:{2} {3}", targetmsg.npc_id, targetmsg.target_id, networkSystem.Robot.LobbyNetworkSystem.User, LobbyRobot.Robot.GetDateTime());
  }
}

internal class Msg_RC_NpcSkill_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_NpcSkill targetmsg = msg as Msg_RC_NpcSkill;
    if (null == targetmsg) {
      return;
    }
    if (networkSystem.Robot.OwnedNpcs.Contains(targetmsg.npc_id)) {
      networkSystem.Robot.NotifyNpcSkill(targetmsg.npc_id, targetmsg.skill_id, targetmsg.stand_pos.x, targetmsg.stand_pos.z, targetmsg.face_direction);
    }
  }
}

internal class Msg_CRC_NpcStopSkill_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_NpcStopSkill targetmsg = msg as Msg_CRC_NpcStopSkill;
    if (null == targetmsg) {
      return;
    }
  }
}

internal class Msg_RC_NpcDead_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_NpcDead targetmsg = msg as Msg_RC_NpcDead;
    if (null == targetmsg) {
      return;
    }
  }
}

internal class Msg_RC_NpcDisappear_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_NpcDisappear disappear = msg as Msg_RC_NpcDisappear;
    if (disappear == null) {
      return;
    }
  }
}

internal class Msg_RC_SyncProperty_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_SyncProperty targetmsg = msg as Msg_RC_SyncProperty;
    if (null == targetmsg) {
      return;
    }
  }
}

internal class Msg_RC_DebugSpaceInfo_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_DebugSpaceInfo targetmsg = msg as Msg_RC_DebugSpaceInfo;
    if (null == targetmsg) return;
  }
}

internal class Msg_RC_SyncCombatStatisticInfo_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_SyncCombatStatisticInfo message = msg as Msg_RC_SyncCombatStatisticInfo;
    if (null == message) return;
  }
}

internal class Msg_RC_PvpCombatInfo_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_PvpCombatInfo message = msg as Msg_RC_PvpCombatInfo;
    if (null == message) return;
  }
}

internal class Msg_CRC_SendImpactToEntity_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_SendImpactToEntity message = msg as Msg_CRC_SendImpactToEntity;
    if (null == message) return;
  }
}

internal class Msg_CRC_StopGfxImpact_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_StopGfxImpact impact_msg = msg as Msg_CRC_StopGfxImpact;
    if (null != impact_msg) {
    }
  }
}

internal class Msg_RC_ImpactDamage_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_ImpactDamage damage_msg = msg as Msg_RC_ImpactDamage;
    if (null != damage_msg) {
    }
  }
}

internal class Msg_CRC_InteractObject_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_InteractObject _msg = msg as Msg_CRC_InteractObject;
    if (null == _msg)
      return;

    int initiatorId = _msg.initiator_id;
    int receiverId = _msg.receiver_id;

  }
}

internal class Msg_RC_ControlObject_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_ControlObject _msg = msg as Msg_RC_ControlObject;
    if (null == _msg)
      return;

  }
}

internal class Msg_RC_RefreshItemSkills_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_RefreshItemSkills _msg = msg as Msg_RC_RefreshItemSkills;
    if (null == _msg)
      return;
    
  }
}

internal class Msg_RC_HighlightPrompt_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_HighlightPrompt _msg = msg as Msg_RC_HighlightPrompt;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_UpdateUserBattleInfo_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_UpdateUserBattleInfo _msg = msg as Msg_RC_UpdateUserBattleInfo;
    if (null == _msg)
      return;
    List<int> skills = networkSystem.Robot.SkillIds;
    skills.Clear();
    if (null != _msg.skill_info && _msg.skill_info.Count > 0) {
      for (int i = 0; i < _msg.skill_info.Count; i++) {
        int skill_id = _msg.skill_info[i].skill_id;
        if (skill_id > 0 && !IsExSkill(skill_id)) {
          skills.Add(skill_id);
          AddSubSkill(skills, skill_id);
        }
      }
      Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(networkSystem.Robot.ConfigId);
      if (null != playerData && null != playerData.m_FixedSkillList && playerData.m_FixedSkillList.Count > 0) {
        foreach (int skill_id in playerData.m_FixedSkillList) {
          if (skill_id > 0 && !IsExSkill(skill_id)) {
            skills.Add(skill_id);
          }
        }
      }
    }
  }
  internal static void AddSubSkill(List<int> skills, int skill_id)
  {
    SkillLogicData skill_data = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skill_id) as SkillLogicData;
    if (null != skill_data && skill_data.NextSkillId > 0 && !IsExSkill(skill_data.NextSkillId)) {
      skills.Add(skill_data.NextSkillId);
      AddSubSkill(skills, skill_data.NextSkillId);
    }
  }
  private static bool IsExSkill(int skillId)
  {
    bool ret = true;
    SkillLogicData cfg = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skillId) as SkillLogicData;
    if (null != cfg) {
      ret = (cfg.Category == SkillCategory.kEx);
    }
    return ret;
  }
}

internal class Msg_RC_MissionCompleted_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_MissionCompleted _msg = msg as Msg_RC_MissionCompleted;
    if (null == _msg)
      return;
    networkSystem.QuitBattlePassive();
    networkSystem.Robot.NotifyMissionCompleted(_msg.main_scene_id);
  }
}

internal class Msg_RC_ChangeScene_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_ChangeScene _msg = msg as Msg_RC_ChangeScene;
    if (null == _msg)
      return;
    networkSystem.QuitBattle();
    networkSystem.Robot.NotifyChangeScene(_msg.main_scene_id);
  }
}

internal class Msg_RC_CampChanged_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_CampChanged _msg = msg as Msg_RC_CampChanged;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_EnableInput_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_EnableInput _msg = msg as Msg_RC_EnableInput;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_ShowUi_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_ShowUi _msg = msg as Msg_RC_ShowUi;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_ShowWall_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_ShowWall _msg = msg as Msg_RC_ShowWall;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_ShowDlg_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_ShowDlg _msg = msg as Msg_RC_ShowDlg;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_CameraLookat_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_CameraLookat _msg = msg as Msg_RC_CameraLookat;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_CameraFollow_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_CameraFollow _msg = msg as Msg_RC_CameraFollow;
    if (null == _msg)
      return;
  }
}

internal class Msg_CRC_GfxControlMoveStart_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_GfxControlMoveStart _msg = msg as Msg_CRC_GfxControlMoveStart;
    if (null == _msg)
      return;
  }
}

internal class Msg_CRC_GfxControlMoveStop_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_CRC_GfxControlMoveStop _msg = msg as Msg_CRC_GfxControlMoveStop;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_UpdateCoefficient_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_UpdateCoefficient _msg = msg as Msg_RC_UpdateCoefficient;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_AdjustPosition_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_AdjustPosition _msg = msg as Msg_RC_AdjustPosition;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_LockFrame_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_LockFrame _msg = msg as Msg_RC_LockFrame;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_PlayAnimation_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_PlayAnimation _msg = msg as Msg_RC_PlayAnimation;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_CameraYaw_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_CameraYaw _msg = msg as Msg_RC_CameraYaw;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_CameraHeight_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_CameraHeight _msg = msg as Msg_RC_CameraHeight;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_CameraDistance_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_CameraDistance _msg = msg as Msg_RC_CameraDistance;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_SetBlockedShader_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_SetBlockedShader _msg = msg as Msg_RC_SetBlockedShader;
    if (null == _msg)
      return;

    uint rimColor1 = _msg.rim_color_1;
    float rimPower1 = _msg.rim_power_1;
    float rimCutValue1 = _msg.rim_cutvalue_1;
    uint rimColor2 = _msg.rim_color_2;
    float rimPower2 = _msg.rim_power_2;
    float rimCutValue2 = _msg.rim_cutvalue_2;
  }
}

internal class Msg_RC_StartCountDown_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_StartCountDown _msg = msg as Msg_RC_StartCountDown;
    if (null == _msg)
      return;
  }
}

internal class Msg_RC_PublishEvent_Handler
{
  internal static void Execute(object msg, NetConnection conn, NetworkSystem networkSystem)
  {
    Msg_RC_PublishEvent _msg = msg as Msg_RC_PublishEvent;
    if (null == _msg)
      return;
    try {
      bool isLogic = _msg.is_logic_event;
      string name = _msg.ev_name;
      string group = _msg.group;
      ArrayList args = new ArrayList();
      foreach (Msg_RC_PublishEvent.EventArg arg in _msg.args) {
        switch (arg.val_type) {
          case 0://null
            args.Add(null);
            break;
          case 1://int
            args.Add(int.Parse(arg.str_val));
            break;
          case 2://float
            args.Add(float.Parse(arg.str_val));
            break;
          default://string
            args.Add(arg.str_val);
            break;
        }
      }
      object[] objArgs = args.ToArray();
    } catch (Exception ex) {
      LogSystem.Error("Msg_RC_PublishEvent_Handler throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
    }
  }
}
