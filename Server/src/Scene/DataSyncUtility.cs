using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArkCrossEngineMessage;
using ArkCrossEngine;

namespace DashFire
{
    internal sealed class DataSyncUtility
    {
        internal static void SyncNpcAiStateToUser(NpcInfo npc, User user)
        {
            Msg_RC_NpcMove npcMoveBuilder = BuildNpcMoveMessage(npc);
            Msg_RC_NpcTarget npcFaceTargetBuilder = BuildNpcTargetMessage(npc);

            if (null != npcMoveBuilder)
                user.SendMessage(npcMoveBuilder);
            if (null != npcFaceTargetBuilder)
                user.SendMessage(npcFaceTargetBuilder);
        }
        internal static void SyncNpcAiStateToCaredUsers(NpcInfo npc)
        {
            Scene scene = npc.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                Msg_RC_NpcMove npcMoveBuilder = BuildNpcMoveMessage(npc);
                Msg_RC_NpcTarget npcFaceTargetBuilder = BuildNpcTargetMessage(npc);

                if (null != npcMoveBuilder)
                    scene.NotifyAreaUser(npc, npcMoveBuilder, false);
                if (null != npcFaceTargetBuilder)
                    scene.NotifyAreaUser(npc, npcFaceTargetBuilder, false);
            }
        }
        internal static void SyncBuffListToObserver(CharacterInfo obj, Observer observer)
        {
            List<ImpactInfo> impacts = obj.GetSkillStateInfo().GetAllImpact();
            foreach (ImpactInfo info in impacts)
            {
                if (info.m_ImpactType == (int)ImpactType.BUFF)
                {
                    Msg_CRC_SendImpactToEntity bd = BuildSendImpactToEntityMessage(info.m_ImpactSenderId, obj.GetId(), info.m_ImpactId, info.m_SkillId);

                    observer.SendMessage(bd);
                }
            }
        }
        internal static void SyncBuffListToObservers(CharacterInfo obj, Scene scene)
        {
            List<ImpactInfo> impacts = obj.GetSkillStateInfo().GetAllImpact();
            foreach (ImpactInfo info in impacts)
            {
                if (info.m_ImpactType == (int)ImpactType.BUFF)
                {
                    Msg_CRC_SendImpactToEntity bd = BuildSendImpactToEntityMessage(info.m_ImpactSenderId, obj.GetId(), info.m_ImpactId, info.m_SkillId);

                    scene.NotifyAllObserver(bd);
                }
            }
        }
        internal static void SyncBuffListToUser(CharacterInfo obj, User user)
        {
            List<ImpactInfo> impacts = obj.GetSkillStateInfo().GetAllImpact();
            foreach (ImpactInfo info in impacts)
            {
                if (info.m_ImpactType == (int)ImpactType.BUFF)
                {
                    Msg_CRC_SendImpactToEntity bd = BuildSendImpactToEntityMessage(info.m_ImpactSenderId, obj.GetId(), info.m_ImpactId, info.m_SkillId);

                    user.SendMessage(bd);
                }
            }
        }
        internal static void SyncBuffListToCaredUsers(CharacterInfo obj)
        {
            Scene scene = obj.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                List<ImpactInfo> impacts = obj.GetSkillStateInfo().GetAllImpact();
                foreach (ImpactInfo info in impacts)
                {
                    if (info.m_ImpactType == (int)ImpactType.BUFF)
                    {
                        Msg_CRC_SendImpactToEntity bd = BuildSendImpactToEntityMessage(info.m_ImpactSenderId, obj.GetId(), info.m_ImpactId, info.m_SkillId);

                        scene.NotifyAreaUser(obj, bd, false);
                    }
                }
            }
        }

        internal static void SyncUserReliveToCaredUsers(UserInfo user)
        {
            Scene scene = user.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                Msg_RC_Revive bd = BuildReviveMessage(user);

                scene.NotifyAreaUser(user, bd, false);
            }
        }

        internal static void SyncUserPropertyToCaredUsers(UserInfo user)
        {
            Scene scene = user.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                Msg_RC_SyncProperty bd = BuildSyncPropertyMessage(user);
                scene.NotifyAllUser(bd);
            }
        }

        internal static Msg_RC_CreateNpc BuildCreateNpcMessage(NpcInfo npc)
        {
            Msg_RC_CreateNpc bder = new Msg_RC_CreateNpc();
            bder.npc_id = npc.GetId();
            bder.unit_id = npc.GetUnitId();
            Vector3 pos = npc.GetMovementStateInfo().GetPosition3D();
            ArkCrossEngineMessage.Position pos_bd = new ArkCrossEngineMessage.Position();
            pos_bd.x = (float)pos.X;
            pos_bd.z = (float)pos.Z;
            bder.cur_pos = pos_bd;
            bder.face_direction = (float)npc.GetMovementStateInfo().GetFaceDir();
            bder.link_id = npc.GetLinkId();
            if (npc.GetUnitId() <= 0)
            {
                bder.camp_id = npc.GetCampId();
            }
            if (npc.OwnerId > 0)
            {
                bder.owner_id = npc.OwnerId;
            }
            bder.level = npc.GetLevel();
            return bder;
        }
        internal static Msg_RC_DropNpc BuildDropNpcMessage(NpcInfo npc, int fromNpcId, int dropType, int dropNum, string model)
        {
            Msg_RC_DropNpc bder = new Msg_RC_DropNpc();
            bder.npc_id = npc.GetId();
            bder.link_id = npc.GetLinkId();
            bder.owner_id = npc.OwnerId;
            bder.from_npc_id = fromNpcId;
            bder.drop_type = dropType;
            bder.drop_num = dropNum;
            bder.camp_id = npc.GetCampId();
            if (!String.IsNullOrEmpty(model))
            {
                bder.model = model;
            }
            return bder;
        }


        internal static Msg_RC_NpcEnter BuildNpcEnterMessage(NpcInfo npc)
        {
            Msg_RC_NpcEnter bder = new Msg_RC_NpcEnter();
            bder.npc_id = npc.GetId();
            Vector3 pos = npc.GetMovementStateInfo().GetPosition3D();
            bder.cur_pos_x = (float)pos.X;
            bder.cur_pos_z = (float)pos.Z;
            bder.face_direction = (float)npc.GetMovementStateInfo().GetFaceDir();
            return bder;
        }

        internal static Msg_RC_SyncProperty BuildSyncPropertyMessage(CharacterInfo obj)
        {
            Msg_RC_SyncProperty builder = new Msg_RC_SyncProperty();
            builder.role_id = obj.GetId();
            builder.hp = obj.Hp;
            builder.np = obj.Energy;
            builder.state = obj.StateFlag;
            return builder;
        }

        internal static Msg_RC_SyncNpcOwnerId BuildSyncNpcOwnerIdMessage(NpcInfo npc)
        {
            Msg_RC_SyncNpcOwnerId builder = new Msg_RC_SyncNpcOwnerId();
            builder.npc_id = npc.GetId();
            builder.owner_id = npc.OwnerId;
            return builder;
        }

        internal static Msg_RC_CampChanged BuildCampChangedMessage(CharacterInfo obj)
        {
            Msg_RC_CampChanged msg = new Msg_RC_CampChanged();
            msg.obj_id = obj.GetId();
            msg.camp_id = obj.GetCampId();
            return msg;
        }

        internal static Msg_RC_NpcMove BuildNpcMoveMessage(NpcInfo npc)
        {
            Msg_RC_NpcMove npcMoveBuilder = new Msg_RC_NpcMove();
            if (npc.GetMovementStateInfo().IsMoving)
            {
                npcMoveBuilder.npc_id = npc.GetId();
                npcMoveBuilder.is_moving = true;
                npcMoveBuilder.move_direction = (float)npc.GetMovementStateInfo().GetMoveDir();
                npcMoveBuilder.face_direction = (float)npc.GetMovementStateInfo().GetFaceDir();
                npcMoveBuilder.cur_pos_x = npc.GetMovementStateInfo().GetPosition3D().X;
                npcMoveBuilder.cur_pos_z = npc.GetMovementStateInfo().GetPosition3D().Z;
                NpcAiStateInfo data = npc.GetAiStateInfo();
                npcMoveBuilder.target_pos_x = npc.GetMovementStateInfo().TargetPosition.X;
                npcMoveBuilder.target_pos_z = npc.GetMovementStateInfo().TargetPosition.Z;
                npcMoveBuilder.velocity_coefficient = (float)npc.VelocityCoefficient;
                npcMoveBuilder.velocity = npc.GetActualProperty().MoveSpeed;
                npcMoveBuilder.move_mode = (int)npc.GetMovementStateInfo().MovementMode;
            }
            else
            {
                npcMoveBuilder.npc_id = npc.GetId();
                npcMoveBuilder.is_moving = false;
                npcMoveBuilder.cur_pos_x = npc.GetMovementStateInfo().GetPosition3D().X;
                npcMoveBuilder.cur_pos_z = npc.GetMovementStateInfo().GetPosition3D().Z;
            }
            return npcMoveBuilder;
        }

        internal static Msg_RC_NpcFace BuildNpcFaceMessage(NpcInfo npc)
        {
            Msg_RC_NpcFace npcFaceBuilder = new Msg_RC_NpcFace();
            npcFaceBuilder.npc_id = npc.GetId();
            npcFaceBuilder.face_direction = (float)npc.GetMovementStateInfo().GetFaceDir();
            return npcFaceBuilder;
        }

        internal static Msg_RC_NpcTarget BuildNpcTargetMessage(NpcInfo npc)
        {
            Msg_RC_NpcTarget npcFaceTargetBuilder = null;
            NpcAiStateInfo data = npc.GetAiStateInfo();
            if (null != data && data.Target > 0)
            {
                npcFaceTargetBuilder = new Msg_RC_NpcTarget();
                npcFaceTargetBuilder.npc_id = npc.GetId();
                npcFaceTargetBuilder.target_id = data.Target;
            }
            return npcFaceTargetBuilder;
        }

        internal static Msg_RC_UserMove BuildUserMoveMessage(UserInfo user)
        {
            Msg_RC_UserMove userMoveBuilder = new Msg_RC_UserMove();
            if (user.GetMovementStateInfo().IsMoving)
            {
                userMoveBuilder.role_id = user.GetId();
                userMoveBuilder.is_moving = true;
                userMoveBuilder.move_direction = (float)user.GetMovementStateInfo().GetMoveDir();
                userMoveBuilder.face_direction = (float)user.GetMovementStateInfo().GetFaceDir();
                userMoveBuilder.cur_pos_x = user.GetMovementStateInfo().GetPosition3D().X;
                userMoveBuilder.cur_pos_z = user.GetMovementStateInfo().GetPosition3D().Z;
                UserAiStateInfo data = user.GetAiStateInfo();
                userMoveBuilder.target_pos_x = user.GetMovementStateInfo().TargetPosition.X;
                userMoveBuilder.target_pos_z = user.GetMovementStateInfo().TargetPosition.Z;
                userMoveBuilder.velocity_coefficient = (float)user.VelocityCoefficient;
            }
            else
            {
                userMoveBuilder.role_id = user.GetId();
                userMoveBuilder.is_moving = false;
                userMoveBuilder.cur_pos_x = user.GetMovementStateInfo().GetPosition3D().X;
                userMoveBuilder.cur_pos_z = user.GetMovementStateInfo().GetPosition3D().Z;
            }
            return userMoveBuilder;
        }

        internal static Msg_RC_UserFace BuildUserFaceMessage(UserInfo user)
        {
            Msg_RC_UserFace builder = new Msg_RC_UserFace();
            builder.face_direction = (float)user.GetMovementStateInfo().GetFaceDir();
            builder.role_id = user.GetId();
            return builder;
        }

        internal static Msg_CRC_SendImpactToEntity BuildSendImpactToEntityMessage(int senderId, int targetId, int impactId, int skillId)
        {
            Msg_CRC_SendImpactToEntity bd = new Msg_CRC_SendImpactToEntity();
            bd.sender_id = senderId;
            bd.target_id = targetId;
            bd.impact_id = impactId;
            bd.skill_id = skillId;
            return bd;
        }

        internal static Msg_RC_SyncCombatStatisticInfo BuildSyncCombatStatisticInfo(UserInfo user)
        {
            Msg_RC_SyncCombatStatisticInfo builder = new Msg_RC_SyncCombatStatisticInfo();
            CombatStatisticInfo info = user.GetCombatStatisticInfo();
            builder.role_id = user.GetId();
            builder.kill_hero_count = info.KillHeroCount;
            builder.assit_kill_count = info.AssitKillCount;
            builder.kill_npc_count = info.KillNpcCount;
            builder.dead_count = info.DeadCount;
            return builder;
        }

        internal static Msg_RC_ControlObject BuildControlObjectMessage(int controller, int controlled, bool isControl)
        {
            Msg_RC_ControlObject builder = new Msg_RC_ControlObject();
            builder.controller_id = controller;
            builder.controlled_id = controlled;
            builder.control_or_release = isControl;
            return builder;
        }

        internal static Msg_RC_Revive BuildReviveMessage(UserInfo user)
        {
            Msg_RC_Revive builder = new Msg_RC_Revive();
            builder.camp_id = user.GetCampId();
            builder.face_direction = user.GetMovementStateInfo().GetFaceDir();
            builder.hero_id = user.GetLinkId();
            builder.is_player_self = true;
            builder.position = new ArkCrossEngineMessage.Position();
            builder.position.x = user.GetMovementStateInfo().GetPosition3D().X;
            builder.position.z = user.GetMovementStateInfo().GetPosition3D().Z;
            builder.role_id = user.GetId();
            return builder;
        }
    }
}
