using System;
using System.Collections.Generic;
using ArkCrossEngineMessage;
using Google.ProtocolBuffers;
using ArkCrossEngineSpatial;
using ArkCrossEngine;

namespace DashFire
{
  internal sealed partial class Scene
  {
    internal void SightTick()
    {
      if (m_SceneState != SceneState.Sleeping) {
        TimeSnapshot.DoCheckPoint();
        //移动版本不需要计算视野
        //NOTE: sight manager 必须放在movementsystem之后，它需要等待玩家的碰撞形状wordposition更新      
        /*
        if (IsAllPlayerEntered()) {
          if (IsPvpScene) {
            m_SightManager.Tick();
          } else {
            for (LinkedListNode<UserInfo> node = UserManager.Users.FirstValue; null != node; node = node.Next) {
              UserInfo user = node.Value;
              if (null != user) {
                user.CurBlueCanSeeMe = true;
                user.CurRedCanSeeMe = true;
              }
            }
            for (LinkedListNode<NpcInfo> node = NpcManager.Npcs.FirstValue; null != node; node = node.Next) {
              NpcInfo npc = node.Value;
              if (null != npc) {
                npc.CurBlueCanSeeMe = true;
                npc.CurRedCanSeeMe = true;
              }
            }
          }
        }
        if (IsPvpScene && IsAllPlayerEntered()) {
          TickSight();
        }
        */
        m_SceneProfiler.SightTickTime = TimeSnapshot.DoCheckPoint();
      }
    }

    private void TickSight()
    {
      for (LinkedListNode<UserInfo> node = UserManager.Users.FirstValue; null != node; node = node.Next) {
        UserInfo user = node.Value;
        if (null != user) {
          UpdateUserSight(user);
        }
      }
      for (LinkedListNode<NpcInfo> node = NpcManager.Npcs.FirstValue; null != node; node = node.Next) {
        NpcInfo npc = node.Value;
        if (null != npc) {
          UpdateNpcSight(npc);
        }
      }
    }

    private void UpdateNpcSight(NpcInfo npc)
    {
      if (npc.CurRedCanSeeMe && !npc.LastRedCanSeeMe) {
        NpcEnterCampSight(npc, (int)CampIdEnum.Red);
      } else if (!npc.CurRedCanSeeMe && npc.LastRedCanSeeMe) {
        NpcLeaveCampSight(npc, (int)CampIdEnum.Red);
      }
      if (npc.CurBlueCanSeeMe && !npc.LastBlueCanSeeMe) {
        NpcEnterCampSight(npc, (int)CampIdEnum.Blue);
      } else if (!npc.CurBlueCanSeeMe && npc.LastBlueCanSeeMe) {
        NpcLeaveCampSight(npc, (int)CampIdEnum.Blue);
      }
    }

    private void NpcEnterCampSight(NpcInfo npc, int campid)
    {
      Msg_RC_NpcEnter bder = DataSyncUtility.BuildNpcEnterMessage(npc);
      NotifyCampUsers(campid, bder);
      Msg_RC_SyncProperty propBuilder = DataSyncUtility.BuildSyncPropertyMessage(npc);
      NotifyCampUsers(campid, propBuilder);
      Msg_RC_NpcMove npcMoveBuilder = DataSyncUtility.BuildNpcMoveMessage(npc);
      NotifyCampUsers(campid, npcMoveBuilder);
      Msg_RC_NpcTarget npcFaceTargetBuilder = DataSyncUtility.BuildNpcTargetMessage(npc);
      if (npcFaceTargetBuilder != null) {
        NotifyCampUsers(campid, npcFaceTargetBuilder);
      }
    }

    private void NpcLeaveCampSight(NpcInfo npc, int campid)
    {
      Msg_RC_NpcDisappear bder = new Msg_RC_NpcDisappear();
      bder.npc_id = npc.GetId();
      NotifyCampUsers(campid, bder);
    }

    private void UpdateUserSight(UserInfo user)
    {
      if (user.CurRedCanSeeMe && !user.LastRedCanSeeMe) {
        UserEnterCampSight(user, (int)CampIdEnum.Red);
      } else if (!user.CurRedCanSeeMe && user.LastRedCanSeeMe) {
        UserLeaveCampSight(user, (int)CampIdEnum.Red);
      }
      if (user.CurBlueCanSeeMe && !user.LastBlueCanSeeMe) {
        UserEnterCampSight(user, (int)CampIdEnum.Blue);
      } else if (!user.CurBlueCanSeeMe && user.LastBlueCanSeeMe) {
        UserLeaveCampSight(user, (int)CampIdEnum.Blue);
      }
    }

    private void UserEnterCampSight(UserInfo enter_user_info, int campid)
    {
      User enter_user = enter_user_info.CustomData as User;
      if (enter_user == null) { return; }
      IList<UserInfo> camp_users = m_SightManager.GetCampUsers(campid);
      foreach (UserInfo user_info in camp_users) {
        User user = user_info.CustomData as User;
        if (user == null) { continue; }
        if (enter_user_info.GetId() != user_info.GetId()) {
          user.AddICareUser(enter_user);
          //send message
          Vector3 enter_user_pos = enter_user_info.GetMovementStateInfo().GetPosition3D();
          ArkCrossEngineMessage.Position pos_bd0 = new ArkCrossEngineMessage.Position();
          pos_bd0.x = enter_user_pos.X;
          pos_bd0.z = enter_user_pos.Z;
          Msg_RC_Enter bder = new Msg_RC_Enter();
          bder.role_id = enter_user.RoleId;
          bder.hero_id = enter_user.HeroId;
          bder.camp_id = enter_user.CampId;
          bder.position = pos_bd0;
          bder.face_dir = (float)enter_user_info.GetMovementStateInfo().GetFaceDir();
          bder.is_moving = enter_user_info.GetMovementStateInfo().IsMoving;
          bder.move_dir = (float)enter_user_info.GetMovementStateInfo().GetMoveDir();
          user.SendMessage(bder);

          Msg_RC_SyncProperty propBuilder = DataSyncUtility.BuildSyncPropertyMessage(enter_user_info);
          user.SendMessage(propBuilder);
          DataSyncUtility.SyncBuffListToUser(enter_user_info, user);
        }
      }
    }

    private void UserLeaveCampSight(UserInfo leave_user_info, int campid)
    {
      User leave_user = leave_user_info.CustomData as User;

      IList<UserInfo> camp_users = m_SightManager.GetCampUsers(campid);
      foreach (UserInfo user_impl in camp_users) {
        if (user_impl == null) {
          continue;
        }
        User user = user_impl.CustomData as User;
        if (leave_user_info.GetId() != user_impl.GetId()) {
          user.RemoveICareUser(leave_user);
          Msg_RC_Disappear bder = new Msg_RC_Disappear();
          bder.role_id = leave_user_info.GetId();
          user.SendMessage(bder);
        }
      }
    }

    private void NotifyCampUsers(int campid, object msg)
    {
      IList<UserInfo> camp_users = m_SightManager.GetCampUsers(campid);
      foreach (UserInfo user_so in camp_users) {
        User user = user_so.CustomData as User;
        if (null != user) {
          user.SendMessage(msg);
        }
      }
    }

    private void NotifySightUsers(CharacterInfo ch, object msg, bool exceptself)
    {
      if (null != m_SightManager) {
        m_SightManager.VisitWatchingObjUsers(ch, (UserInfo userInfo) => {
          User user = userInfo.CustomData as User;
          if (null != user) {
            if (exceptself && ch.IsUser && userInfo.GetId() == ch.GetId()) {
            } else {
              user.SendMessage(msg);
            }
          }
        });
      }
    }
  }
}
