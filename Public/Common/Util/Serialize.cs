using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.ProtocolBuffers;
using Google.ProtocolBuffers.DescriptorProtos;
using System.Reflection;
using DashFireMessage;
using Lobby_RoomServer;

namespace DashFire
{
  public class Serialize
  {
    private static MyDictionary<int, IMessage> m_DicIDMsg = new MyDictionary<int, IMessage>();
    private static MyDictionary<Type, int> m_DicIDName = new MyDictionary<Type, int>();
    public static bool Init()
    {
      RegisterIDName((int)MessageDefine.Msg_Ping, Msg_Ping.DefaultInstance, typeof(Msg_Ping));
      RegisterIDName((int)MessageDefine.Msg_Pong, Msg_Pong.DefaultInstance, typeof(Msg_Pong));
      RegisterIDName((int)MessageDefine.Msg_CR_ShakeHands, Msg_CR_ShakeHands.DefaultInstance, typeof(Msg_CR_ShakeHands));
      RegisterIDName((int)MessageDefine.Msg_RC_ShakeHands_Ret, Msg_RC_ShakeHands_Ret.DefaultInstance, typeof(Msg_RC_ShakeHands_Ret));
      RegisterIDName((int)MessageDefine.Msg_CR_Observer, Msg_CR_Observer.DefaultInstance, typeof(Msg_CR_Observer));
      RegisterIDName((int)MessageDefine.Msg_CRC_Create, Msg_CRC_Create.DefaultInstance, typeof(Msg_CRC_Create));
      RegisterIDName((int)MessageDefine.Msg_RC_Enter, Msg_RC_Enter.DefaultInstance, typeof(Msg_RC_Enter));
      RegisterIDName((int)MessageDefine.Msg_RC_Disappear, Msg_RC_Disappear.DefaultInstance, typeof(Msg_RC_Disappear));
      RegisterIDName((int)MessageDefine.Msg_RC_Dead, Msg_RC_Dead.DefaultInstance, typeof(Msg_RC_Dead));
      RegisterIDName((int)MessageDefine.Msg_RC_Revive, Msg_RC_Revive.DefaultInstance, typeof(Msg_RC_Revive));
      RegisterIDName((int)MessageDefine.Msg_CRC_Exit, Msg_CRC_Exit.DefaultInstance, typeof(Msg_CRC_Exit));
      RegisterIDName((int)MessageDefine.Msg_CRC_MoveStart, Msg_CRC_MoveStart.DefaultInstance, typeof(Msg_CRC_MoveStart));
      RegisterIDName((int)MessageDefine.Msg_CRC_MoveStop, Msg_CRC_MoveStop.DefaultInstance, typeof(Msg_CRC_MoveStop));
      RegisterIDName((int)MessageDefine.Msg_RC_MoveMeetObstacle, Msg_RC_MoveMeetObstacle.DefaultInstance, typeof(Msg_RC_MoveMeetObstacle));
      RegisterIDName((int)MessageDefine.Msg_CRC_Face, Msg_CRC_Face.DefaultInstance, typeof(Msg_CRC_Face));
      RegisterIDName((int)MessageDefine.Msg_CRC_Skill, Msg_CRC_Skill.DefaultInstance, typeof(Msg_CRC_Skill));
      RegisterIDName((int)MessageDefine.Msg_CRC_Effect, Msg_CRC_Effect.DefaultInstance, typeof(Msg_CRC_Effect));
      RegisterIDName((int)MessageDefine.Msg_CRC_Action, Msg_CRC_Action.DefaultInstance, typeof(Msg_CRC_Action));
      RegisterIDName((int)MessageDefine.Msg_CRC_NormalAttackBegin, Msg_CRC_NormalAttackBegin.DefaultInstance, typeof(Msg_CRC_NormalAttackBegin));
      RegisterIDName((int)MessageDefine.Msg_CRC_NormalAttackEnd, Msg_CRC_NormalAttackEnd.DefaultInstance, typeof(Msg_CRC_NormalAttackEnd));
      RegisterIDName((int)MessageDefine.Msg_CRC_Reload, Msg_CRC_Reload.DefaultInstance, typeof(Msg_CRC_Reload));
      RegisterIDName((int)MessageDefine.Msg_CRC_PrevWeapon, Msg_CRC_PrevWeapon.DefaultInstance, typeof(Msg_CRC_PrevWeapon));
      RegisterIDName((int)MessageDefine.Msg_CRC_NextWeapon, Msg_CRC_NextWeapon.DefaultInstance, typeof(Msg_CRC_NextWeapon));
      RegisterIDName((int)MessageDefine.Msg_RC_CreateNpc, Msg_RC_CreateNpc.DefaultInstance, typeof(Msg_RC_CreateNpc));
      RegisterIDName((int)MessageDefine.Msg_RC_DestroyNpc, Msg_RC_DestroyNpc.DefaultInstance, typeof(Msg_RC_DestroyNpc));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcEnter, Msg_RC_NpcEnter.DefaultInstance, typeof(Msg_RC_NpcEnter));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcMove, Msg_RC_NpcMove.DefaultInstance, typeof(Msg_RC_NpcMove));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcFace, Msg_RC_NpcFace.DefaultInstance, typeof(Msg_RC_NpcFace));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcTarget, Msg_RC_NpcTarget.DefaultInstance, typeof(Msg_RC_NpcTarget));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcShoot, Msg_RC_NpcShoot.DefaultInstance, typeof(Msg_RC_NpcShoot));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcSkill, Msg_RC_NpcSkill.DefaultInstance, typeof(Msg_RC_NpcSkill));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcDead, Msg_RC_NpcDead.DefaultInstance, typeof(Msg_RC_NpcDead));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcDisappear, Msg_RC_NpcDisappear.DefaultInstance, typeof(Msg_RC_NpcDisappear));
      RegisterIDName((int)MessageDefine.Msg_RC_Levelup, Msg_RC_Levelup.DefaultInstance, typeof(Msg_RC_Levelup));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncLevel, Msg_RC_SyncLevel.DefaultInstance, typeof(Msg_RC_SyncLevel));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncExpMoney, Msg_RC_SyncExpMoney.DefaultInstance, typeof(Msg_RC_SyncExpMoney));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncProperty, Msg_RC_SyncProperty.DefaultInstance, typeof(Msg_RC_SyncProperty));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncImpactEffect, Msg_RC_SyncImpactEffect.DefaultInstance, typeof(Msg_RC_SyncImpactEffect));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncItem, Msg_RC_SyncItem.DefaultInstance, typeof(Msg_RC_SyncItem));
      RegisterIDName((int)MessageDefine.Msg_RC_ItemChanged, Msg_RC_ItemChanged.DefaultInstance, typeof(Msg_RC_ItemChanged));
      RegisterIDName((int)MessageDefine.Msg_RC_CreateSceneLogic, Msg_RC_CreateSceneLogic.DefaultInstance, typeof(Msg_RC_CreateSceneLogic));
      RegisterIDName((int)MessageDefine.Msg_RC_DestroySceneLogic, Msg_RC_DestroySceneLogic.DefaultInstance, typeof(Msg_RC_DestroySceneLogic));
      RegisterIDName((int)MessageDefine.Msg_RC_SceneLogicImpact, Msg_RC_SceneLogicImpact.DefaultInstance, typeof(Msg_RC_SceneLogicImpact));
      RegisterIDName((int)MessageDefine.Msg_RC_ShootHurt, Msg_RC_ShootHurt.DefaultInstance, typeof(Msg_RC_ShootHurt));
      RegisterIDName((int)MessageDefine.Msg_CR_KeyPress, Msg_CR_KeyPress.DefaultInstance, typeof(Msg_CR_KeyPress));
      RegisterIDName((int)MessageDefine.Msg_RC_DebugSpaceInfo, Msg_RC_DebugSpaceInfo.DefaultInstance, typeof(Msg_RC_DebugSpaceInfo));
      RegisterIDName((int)MessageDefine.Msg_CR_SwitchDebug, Msg_CR_SwitchDebug.DefaultInstance, typeof(Msg_CR_SwitchDebug));
      RegisterIDName((int)MessageDefine.Msg_CR_BuyItem, Msg_CR_BuyItem.DefaultInstance, typeof(Msg_CR_BuyItem));
      RegisterIDName((int)MessageDefine.Msg_CR_SellItem, Msg_CR_SellItem.DefaultInstance, typeof(Msg_CR_SellItem));
      RegisterIDName((int)MessageDefine.Msg_CR_SwapItem, Msg_CR_SwapItem.DefaultInstance, typeof(Msg_CR_SwapItem));
      RegisterIDName((int)MessageDefine.Msg_CR_DiscardItem, Msg_CR_DiscardItem.DefaultInstance, typeof(Msg_CR_DiscardItem));
      RegisterIDName((int)MessageDefine.Msg_RC_PvpFinish, Msg_RC_PvpFinish.DefaultInstance, typeof(Msg_RC_PvpFinish));
      RegisterIDName((int)MessageDefine.Msg_RC_PveFinish, Msg_RC_PveFinish.DefaultInstance, typeof(Msg_RC_PveFinish));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncCombatStatisticInfo, Msg_RC_SyncCombatStatisticInfo.DefaultInstance, typeof(Msg_RC_SyncCombatStatisticInfo));
      RegisterIDName((int)MessageDefine.Msg_RC_PvpCombatInfo, Msg_RC_PvpCombatInfo.DefaultInstance, typeof(Msg_RC_PvpCombatInfo));
      RegisterIDName((int)MessageDefine.Msg_RC_SkillShootBarrage, Msg_RC_SkillShootBarrage.DefaultInstance, typeof(Msg_RC_SkillShootBarrage));
      RegisterIDName((int)MessageDefine.Msg_RC_WpnShootBarrage, Msg_RC_WpnShootBarrage.DefaultInstance, typeof(Msg_RC_WpnShootBarrage));
      RegisterIDName((int)MessageDefine.Msg_CRC_UpdateSkill, Msg_CRC_UpdateSkill.DefaultInstance, typeof(Msg_CRC_UpdateSkill));
      RegisterIDName((int)MessageDefine.Msg_RC_SkillCreateSceneLogicInfo, Msg_RC_SkillCreateSceneLogicInfo.DefaultInstance, typeof(Msg_RC_SkillCreateSceneLogicInfo));
      RegisterIDName((int)MessageDefine.Msg_RC_SceneLogicShootBarrage, Msg_RC_SceneLogicShootBarrage.DefaultInstance, typeof(Msg_RC_SceneLogicShootBarrage));
      RegisterIDName((int)MessageDefine.Msg_RC_SendImpactToEntity, Msg_RC_SendImpactToEntity.DefaultInstance, typeof(Msg_RC_SendImpactToEntity));
      RegisterIDName((int)MessageDefine.Msg_RC_RemoveImpactFromEntity, Msg_RC_RemoveImpactFromEntity.DefaultInstance, typeof(Msg_RC_RemoveImpactFromEntity));
      RegisterIDName((int)MessageDefine.Msg_CRC_BeginSlowDown, Msg_CRC_BeginSlowDown.DefaultInstance, typeof(Msg_CRC_BeginSlowDown));
      RegisterIDName((int)MessageDefine.Msg_CRC_EndSlowDown, Msg_CRC_EndSlowDown.DefaultInstance, typeof(Msg_CRC_EndSlowDown));
      RegisterIDName((int)MessageDefine.Msg_RC_CreateLandMark, Msg_RC_CreateLandMark.DefaultInstance, typeof(Msg_RC_CreateLandMark));
      RegisterIDName((int)MessageDefine.Msg_CRC_WeaponStatus, Msg_CRC_WeaponStatus.DefaultInstance, typeof(Msg_CRC_WeaponStatus));
      RegisterIDName((int)MessageDefine.Msg_CRC_MousePos, Msg_CRC_MousePos.DefaultInstance, typeof(Msg_CRC_MousePos));
      RegisterIDName((int)MessageDefine.Msg_RC_ChangeWeaponSync, Msg_RC_ChangeWeaponSync.DefaultInstance, typeof(Msg_RC_ChangeWeaponSync));
      RegisterIDName((int)MessageDefine.Msg_CRC_InteractObject, Msg_CRC_InteractObject.DefaultInstance, typeof(Msg_CRC_InteractObject));
      RegisterIDName((int)MessageDefine.Msg_RC_ControlObject, Msg_RC_ControlObject.DefaultInstance, typeof(Msg_RC_ControlObject));
      RegisterIDName((int)MessageDefine.Msg_RC_RefreshItemSkills, Msg_RC_RefreshItemSkills.DefaultInstance, typeof(Msg_RC_RefreshItemSkills));
      RegisterIDName((int)MessageDefine.Msg_RC_HighlightPrompt, Msg_RC_HighlightPrompt.DefaultInstance, typeof(Msg_RC_HighlightPrompt));
      RegisterIDName((int)MessageDefine.Msg_CRC_DescriptorSync, GM.Msg_CRC_DescriptorSync.DefaultInstance, typeof(GM.Msg_CRC_DescriptorSync));
      RegisterIDName((int)MessageDefine.Msg_CR_Execute, GM.Msg_CR_Execute.DefaultInstance, typeof(GM.Msg_CR_Execute));
      RegisterIDName((int)MessageDefine.Msg_RC_ExecuteResult, GM.Msg_RC_ExecuteResult.DefaultInstance, typeof(GM.Msg_RC_ExecuteResult));
      RegisterIDName((int)MessageDefine.Msg_RC_SearchLightMove, Msg_RC_SearchLightMove.DefaultInstance, typeof(Msg_RC_SearchLightMove));
      RegisterIDName((int)MessageDefine.Msg_RC_NotifyEarnMoney, Msg_RC_NotifyEarnMoney.DefaultInstance, typeof(Msg_RC_NotifyEarnMoney));
      RegisterIDName((int)MessageDefine.Msg_CR_QuitClient, Msg_CR_QuitClient.DefaultInstance, typeof(Msg_CR_QuitClient));
      RegisterIDName((int)MessageDefine.Msg_RC_UserMove, Msg_RC_UserMove.DefaultInstance, typeof(Msg_RC_UserMove));
      RegisterIDName((int)MessageDefine.Msg_RC_UserFace, Msg_RC_UserFace.DefaultInstance, typeof(Msg_RC_UserFace));
      RegisterIDName((int)MessageDefine.Msg_LR_CreateBattleRoom, Msg_LR_CreateBattleRoom.DefaultInstance, typeof(Msg_LR_CreateBattleRoom));
      RegisterIDName((int)MessageDefine.Msg_RL_ReplyCreateBattleRoom, Msg_RL_ReplyCreateBattleRoom.DefaultInstance, typeof(Msg_RL_ReplyCreateBattleRoom));
      return true;
    }
    private static void RegisterIDName(int id, IMessage msg, Type msgtype) {
      if (msg == null)
        return;
      m_DicIDMsg[id] = msg;
      m_DicIDName[msgtype] = id;
    }
    public static byte[] Encode(IMessage msg)
    {
      byte[] ret = new byte[2 + msg.SerializedSize];
      int id = m_DicIDName[msg.GetType()];
      ret[0] = (byte)(id >> 8);
      ret[1] = (byte)(id);
      byte[] msgarray = msg.ToByteArray();
      Buffer.BlockCopy(msgarray, 0, ret, 2, msgarray.Length);
      return ret;
    }
    public static IMessage Decode(byte[] msgbuf)
    {
      int id = (int)(((int)msgbuf[0] << 8) | ((int)msgbuf[1]));
      if (id < 0) { 
        LogSystem.Debug("decode error:message id({0}) error !!!",
                          id);
        return null;
      }
      
      IMessage msg = Serialize.CreateMessage(id);
      if (msg == null) {
        LogSystem.Debug("decode error:can't find id {0}  !!!", id);
        return null;
      }
      
      byte[] rmsg = new byte[msgbuf.Length - 2];
      Buffer.BlockCopy(msgbuf, 2, rmsg, 0, msgbuf.Length - 2);
      object[] param2 = new object[] { rmsg };
      Type[] param = new Type[] { rmsg.GetType() };
      MethodInfo method = msg.GetType().GetMethod("ParseFrom", param);
      if (method == null) {
        LogSystem.Debug("decode error:can't find ParseFrom!");
        return null;
      }
      return (IMessage)method.Invoke(msg, param2);
    }
    public static IMessage CreateMessage(int id)
    {
      IMessage msg;
      if (m_DicIDMsg.TryGetValue(id, out msg))
      {
        return msg;
      }
      return null;
    }
  }
}
