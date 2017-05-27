using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.Collections;
using ArkCrossEngine;
using ArkCrossEngineMessage;
using ArkCrossEngine.Network;

internal class MsgPongHandler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_Pong pong_msg = msg as Msg_Pong;
        if (pong_msg == null)
        {
            return;
        }
        long time = TimeUtility.GetLocalMilliseconds();
        NetworkSystem.Instance.OnPong(time, pong_msg.send_ping_time, pong_msg.send_pong_time);
    }
}

internal class MsgShakeHandsRetHandler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_ShakeHands_Ret ret_msg = msg as Msg_RC_ShakeHands_Ret;
        if (msg == null)
        {
            return;
        }
        if (ret_msg.auth_result == Msg_RC_ShakeHands_Ret.RetType.SUCCESS)
        {
            LogSystem.Debug("auth ok !!!");
            NetworkSystem.Instance.CanSendMessage = true;
            GameControler.NotifyRoomServerConnected();
        }
        else
        {
            LogSystem.Debug("auth failed !!!");
            NetworkSystem.Instance.WaitDisconnect();
        }
    }
}

internal class Msg_CRC_Create_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_Create enter = msg as Msg_CRC_Create;
        if (null == enter)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(enter.role_id);
        if (cb != null)
        {
            return;
        }
        LogSystem.Debug("Msg_CRC_Create, PlayerId={0} HeroId={1} Pos={2},{3}", enter.role_id, enter.hero_id, enter.position.x, enter.position.z);

        if (enter.is_player_self)
        {

            UserInfo user = WorldSystem.Instance.CreatePlayerSelf(enter.role_id, enter.hero_id);

            user.SetAIEnable(false);
            user.SetCampId(enter.camp_id);
            user.SetLevel(enter.role_level);
            user.GetMovementStateInfo().SetPosition2D(enter.position.x, enter.position.z);
            user.GetMovementStateInfo().SetFaceDir(enter.face_dirction);
            if (null != WorldSystem.Instance.SpatialSystem)
            {
                WorldSystem.Instance.SpatialSystem.AddObj(user.SpaceObject);
            }

            EntityManager.Instance.CreateUserView(enter.role_id);
            /*for (int index = 0; index < enter.skill_levels.Count; index++) {
              int skillId = 0;
              SkillInfo skillInfo = new SkillInfo(skillId);
              skillInfo.SkillLevel = enter.skill_levels[index];
              user.GetSkillStateInfo().AddSkill(index, skillInfo);
            }*/
            if (enter.scene_start_time > 0)
            {
                WorldSystem.Instance.SceneStartTime = enter.scene_start_time;
            }

            user.SetNickName(enter.nickname);

            UserView view = EntityManager.Instance.GetUserViewById(enter.role_id);
            if (view != null)
            {
                GfxSystem.SendMessage("GfxGameRoot", "CameraFollowImmediately", view.Actor);
            }

            if (WorldSystem.Instance.IsPvpScene())
            {
                int campId = WorldSystem.Instance.CampId;
                /*if (campId == (int)CampIdEnum.Blue) {
                  GfxSystem.SendMessage("GfxGameRoot", "CameraFixedYaw", Math.PI / 2);
                } else if (campId == (int)CampIdEnum.Red) {
                  GfxSystem.SendMessage("GfxGameRoot", "CameraFixedYaw", -Math.PI / 2);
                }*/

                SceneResource scene = WorldSystem.Instance.GetCurScene();
                scene.NotifyUserEnter();
            }
            else
            {
                SceneResource scene = WorldSystem.Instance.GetCurScene();
                scene.NotifyUserEnter();
            }

            WorldSystem.Instance.SyncGfxUserInfo(enter.role_id);
        }
        else
        {

            UserInfo other = WorldSystem.Instance.CreateUser(enter.role_id, enter.hero_id);

            other.SetAIEnable(false);
            other.SetCampId(enter.camp_id);
            other.SetLevel(enter.role_level);
            other.GetMovementStateInfo().SetPosition2D(enter.position.x, enter.position.z);
            other.GetMovementStateInfo().SetFaceDir(enter.face_dirction);

            EntityManager.Instance.CreateUserView(enter.role_id);
            /*for (int index = 0; index < enter.skill_levels.Count; index++) {
              int skillId = 0;
              SkillInfo skillInfo = new SkillInfo(skillId);
              skillInfo.SkillLevel = enter.skill_levels[index];
              other.GetSkillStateInfo().AddSkill(index, skillInfo);
            }*/
            other.SetNickName(enter.nickname);

            if (WorldSystem.Instance.IsObserver)
            {
                if (enter.scene_start_time > 0)
                {
                    WorldSystem.Instance.SceneStartTime = enter.scene_start_time;
                }

                LogSystem.Debug("User {0}({1}) create", enter.nickname, enter.role_id);
            }

            UserView view = EntityManager.Instance.GetUserViewById(enter.role_id);
            if (view != null)
            {
            }
            WorldSystem.Instance.SyncGfxUserInfo(enter.role_id);
        }
    }
}

internal class Msg_RC_Enter_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_Enter enter = msg as Msg_RC_Enter;
        if (null == enter)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(enter.role_id);
        if (cb == null)
        {
            return;
        }
        LogSystem.Debug("Msg_RC_Enter, PlayerId={0}", enter.role_id);

        UserInfo other = cb.CastUserInfo();
        if (null != other)
        {
            other.SetCampId(enter.camp_id);
            MovementStateInfo msi = other.GetMovementStateInfo();
            msi.SetPosition2D(enter.position.x, enter.position.z);
            UserView view = EntityManager.Instance.GetUserViewById(enter.role_id);
            if (null != view)
            {
                view.Visible = true;
                GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
            }
            msi.SetFaceDir(enter.face_dir);
            msi.SetMoveDir(enter.move_dir);
            msi.IsMoving = enter.is_moving;
            if (enter.is_moving)
            {
                other.GetMovementStateInfo().StartMove();
            }
        }
    }
}

internal class Msg_RC_Disappear_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_Disappear disappear = msg as Msg_RC_Disappear;
        if (disappear == null)
        {
            return;
        }
        CharacterInfo info = WorldSystem.Instance.GetCharacterById(disappear.role_id);
        if (null == info)
        {
            return;
        }
        LogSystem.Debug("Msg_RC_Disappear, PlayerId={0}", disappear.role_id);
        UserInfo player = info.CastUserInfo();
        if (null != player)
        {
            LogSystem.Debug("Hide Player {0} link id {1} position {2} for Msg_RC_Disappear", player.GetId(), player.GetLinkId(), player.GetMovementStateInfo().GetPosition3D().ToString());

            UserView view = EntityManager.Instance.GetUserViewById(disappear.role_id);
            if (null != view)
            {
                view.Visible = false;
            }
        }
    }
}

internal class Msg_RC_Dead_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_Dead dead = msg as Msg_RC_Dead;
        if (dead == null)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(dead.role_id);
        if (null == cb)
        {
            return;
        }

        UserInfo player = cb.CastUserInfo();
        if (null != player)
        {
            if (!player.IsDead())
            {
                player.SetHp(Operate_Type.OT_Absolute, 0);
                player.GetMovementStateInfo().IsMoving = false;
            }
            player.SetStateFlag(Operate_Type.OT_AddBit, CharacterState_Type.CST_BODY);

            player.GetMovementStateInfo().IsMoving = false;
            player.GetMovementStateInfo().IsSkillMoving = false;
            UserView view = EntityManager.Instance.GetUserViewById(dead.role_id);
            if (null != view)
            {
                view.ObjectInfo.IsSkillGfxAnimation = false;
                view.ObjectInfo.IsSkillGfxMoveControl = false;
                view.ObjectInfo.IsImpactGfxAnimation = false;
                view.ObjectInfo.IsImpactGfxMoveControl = false;
            }

            if (WorldSystem.Instance.PlayerSelfId == dead.role_id || WorldSystem.Instance.IsObserver && WorldSystem.Instance.IsFollowObserver && WorldSystem.Instance.FollowTargetId == dead.role_id)
            {
                Vector3 pos = player.GetMovementStateInfo().GetPosition3D();
                GfxSystem.SendMessage("GfxGameRoot", "CameraLookat", new float[] { pos.X, pos.Y, pos.Z });
                LogSystem.Debug("[Camera State] LookAtPos:{0}", pos.ToString());
            }
        }
    }
}

internal class Msg_RC_Revive_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_Revive revive = msg as Msg_RC_Revive;
        if (revive == null)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(revive.role_id);
        if (cb == null)
        {
            return;
        }
        UserInfo user = cb.CastUserInfo();
        if (null != user)
        {
            user.SetCampId(revive.camp_id);
            user.SetStateFlag(Operate_Type.OT_RemoveBit, CharacterState_Type.CST_BODY);
            user.GetMovementStateInfo().IsMoving = false;

            user.GetMovementStateInfo().SetPosition(revive.position.x, 0, revive.position.z);
            user.GetMovementStateInfo().SetFaceDir(revive.face_direction);
            user.GetMovementStateInfo().SetWantFaceDir(revive.face_direction);
            user.DeadTime = 0;
            ///
            int playerselfid = WorldSystem.Instance.PlayerSelfId;
            if (playerselfid == revive.role_id)
            {
                WorldSystem.Instance.RelivePlayer();
            }
            UserView view = EntityManager.Instance.GetUserViewById(revive.role_id);
            if (null != view)
            {
                view.Visible = true;
                GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, revive.position.x, revive.position.z);
                view.ObjectInfo.FaceDir = revive.face_direction;
                view.ObjectInfo.WantFaceDir = revive.face_direction;
                view.ObjectInfo.IsDead = false;
                LogSystem.Debug("Show Player {0} link id {1} position {2} for Msg_RC_Revive", user.GetId(), user.GetLinkId(), user.GetMovementStateInfo().GetPosition3D().ToString());
            }
            if (playerselfid == revive.role_id || WorldSystem.Instance.IsObserver && WorldSystem.Instance.IsFollowObserver && WorldSystem.Instance.FollowTargetId == revive.role_id)
            {
                if (null != view)
                {
                    GfxSystem.SendMessage("GfxGameRoot", "CameraFollow", view.Actor);
                }
                if (WorldSystem.Instance.IsPvpScene() && !WorldSystem.Instance.IsObserver)
                {
                    int campId = WorldSystem.Instance.CampId;
                    /*if (campId == (int)CampIdEnum.Blue) {
                      GfxSystem.SendMessage("GfxGameRoot", "CameraFixedYaw", Math.PI / 2);
                    } else if (campId == (int)CampIdEnum.Red) {
                      GfxSystem.SendMessage("GfxGameRoot", "CameraFixedYaw", -Math.PI / 2);
                    }*/
                }
                LogSystem.Debug("[Camera State] FollowCharacter:{0} {1}", revive.role_id, user.GetMovementStateInfo().GetPosition3D().ToString());
            }
        }

    }
}

internal class Msg_CRC_Exit_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_Exit targetmsg = msg as Msg_CRC_Exit;
        if (null == targetmsg)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
        if (cb == null)
        {
            return;
        }
        UserInfo other = cb.CastUserInfo();
        if (other.GetId() != WorldSystem.Instance.PlayerSelfId)
        {
            UserView view = EntityManager.Instance.GetUserViewById(other.GetId());
            if (view != null)
            {
                GfxSystem.PublishGfxEvent("ge_hide_name_plate", "ui", view.Actor);
            }
        }
        else
        {
            NetworkSystem.Instance.QuitBattlePassive();
        }
        EntityManager.Instance.DestroyUserView(other.GetId());
        WorldSystem.Instance.DestroyCharacterById(other.GetId());
    }
}

internal class Msg_CRC_Move_Handler
{
    internal static void OnMoveStart(object msg, NetConnection conn)
    {
        Msg_CRC_MoveStart move_msg = msg as Msg_CRC_MoveStart;
        if (null == move_msg)
            return;

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(move_msg.role_id);
        if (cb == null)
            return;
        //LogSystem.Debug("Msg_CRC_MoveStart, EntityId={0}, MoveDir={1}, IsMoving={2}, IsSkillMoving={3}", move_msg.role_id, move_msg.dir, cb.GetMovementStateInfo().IsMoving, cb.GetMovementStateInfo().IsSkillMoving);
        UserInfo other = cb.CastUserInfo();
        if (other != null)
        {
            MovementStateInfo msi = other.GetMovementStateInfo();
            if (!msi.IsSkillMoving)
            {//如果技能控制移动，移动消息的位置同步忽略
                ControlSystemOperation.AdjustCharacterPosition(move_msg.role_id, move_msg.position.x, move_msg.position.z, TimeUtility.AverageRoundtripTime, move_msg.dir);
            }
            if (move_msg.role_id != WorldSystem.Instance.PlayerSelfId)
            {
                msi.SetMoveDir(move_msg.dir);
                //ControlSystemOperation.AdjustCharacterMoveDir(move_msg.role_id, move_msg.dir);
                msi.StartMove();
                msi.TargetPosition = Vector3.Zero;
                other.IsMoving = true;
            }
        }
    }

    internal static void OnMoveStop(object msg, NetConnection conn)
    {
        Msg_CRC_MoveStop move_msg = msg as Msg_CRC_MoveStop;
        if (null == move_msg)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(move_msg.role_id);
        if (cb == null)
            return;
        //LogSystem.Debug("Msg_CRC_MoveStop, EntityId={0}", move_msg.role_id);
        UserInfo other = cb.CastUserInfo();
        if (other != null)
        {
            MovementStateInfo msi = other.GetMovementStateInfo();
            msi.StopMove();
            other.IsMoving = false;
        }
    }
}

internal class Msg_CRC_MoveMeetObstacle_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_MoveMeetObstacle obstacle_msg = msg as Msg_CRC_MoveMeetObstacle;
        if (null == obstacle_msg)
        {
            return;
        }
        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(obstacle_msg.role_id);
        if (cb == null)
            return;
        UserInfo other = cb.CastUserInfo();
        if (other != null)
        {
            MovementStateInfo msi = other.GetMovementStateInfo();
            msi.SetPosition2D(obstacle_msg.cur_pos_x, obstacle_msg.cur_pos_z);
            UserView view = EntityManager.Instance.GetUserViewById(obstacle_msg.role_id);
            if (null != view)
            {
                GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
            }
        }
    }
}

internal class Msg_CRC_Face_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_Face face_msg = msg as Msg_CRC_Face;
        if (null == face_msg)
        {
            return;
        }
        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(face_msg.role_id);
        if (cb == null)
            return;
        UserInfo other = cb.CastUserInfo();
        if (other != null)
        {
            if (other.IsHaveStateFlag(CharacterState_Type.CST_Sleep))
            {
                return;
            }
            other.GetMovementStateInfo().SetFaceDir(face_msg.face_direction);
            //ControlSystemOperation.AdjustCharacterFaceDir(face_msg.role_id, face_msg.face_direction);
            other.GetMovementStateInfo().SetWantFaceDir(face_msg.face_direction);
        }
    }
}

internal class Msg_CRC_Skill_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_Skill skill_msg = msg as Msg_CRC_Skill;
        if (null == skill_msg)
        {
            return;
        }
        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(skill_msg.role_id);
        if (cb != null)
        {
            //LogSystem.Debug("Msg_CRC_Skill, EntityId={0}, SkillId={1} Pos={2},{3}", skill_msg.role_id, skill_msg.skill_id, skill_msg.stand_pos.x, skill_msg.stand_pos.z);

            cb = cb.GetRealControlledObject();
            if (!cb.IsHaveStateFlag(CharacterState_Type.CST_Hidden))   // 如果角色处于隐身状态，不同步位置信息
            {
                MovementStateInfo msi = cb.GetMovementStateInfo();
                msi.SetPosition2D(skill_msg.stand_pos.x, skill_msg.stand_pos.z);
                msi.SetFaceDir(skill_msg.face_direction);
                msi.SetWantFaceDir(skill_msg.want_face_dir);

                UserView view = EntityManager.Instance.GetUserViewById(skill_msg.role_id);
                if (null != view)
                {
                    view.ObjectInfo.FaceDir = skill_msg.face_direction;
                    view.ObjectInfo.WantFaceDir = skill_msg.want_face_dir;
                    GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
                }

                //LogSystem.Debug("Msg_CRC_Skill EntityId={0}, SkillId={1}, just move to pos {2},{3}", skill_msg.role_id, skill_msg.skill_id, skill_msg.stand_pos.x, skill_msg.stand_pos.z);
            }
            if (skill_msg.role_id != WorldSystem.Instance.PlayerSelfId)
            {
                cb.SkillController.ForceStartSkill(skill_msg.skill_id);
            }
        }
    }
}

internal class Msg_CRC_StopSkill_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_StopSkill skill_msg = msg as Msg_CRC_StopSkill;
        if (null == skill_msg)
        {
            return;
        }
        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(skill_msg.role_id);
        if (cb != null)
        {
            //LogSystem.Debug("Msg_CRC_StopSkill, EntityId={0}, SkillId={1}", skill_msg.role_id, skill_msg.skill_id);

            cb = cb.GetRealControlledObject();
            cb.SkillController.ForceInterruptCurSkill();
        }
    }
}

internal class Msg_RC_UserMove_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_UserMove targetmsg = msg as Msg_RC_UserMove;
        if (null == targetmsg)
        {
            return;
        }
        CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
        if (null == charObj)
        {
            return;
        }
        UserInfo user = charObj.CastUserInfo();
        if (user == null)
        {
            return;
        }
        if (targetmsg.is_moving)
        {
            //LogSystem.Debug("UserMove, user:{0} pos:{1} move:{2}->{3}", user.GetId(), user.GetMovementStateInfo().GetPosition3D().ToString(), new Vector2(targetmsg.cur_pos_x, targetmsg.cur_pos_z).ToString(), new Vector2(targetmsg.target_pos_x, targetmsg.target_pos_z).ToString());

            MovementStateInfo msi = user.GetMovementStateInfo();
            /*
            msi.SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
            UserView view = EntityManager.Instance.GetUserViewById(targetmsg.role_id);
            if (null != view) {
              GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
            }*/

            msi.SetMoveDir(targetmsg.move_direction);
            ControlSystemOperation.AdjustCharacterPosition(targetmsg.role_id, targetmsg.cur_pos_x, targetmsg.cur_pos_z, TimeUtility.AverageRoundtripTime, targetmsg.move_direction);
            ControlSystemOperation.AdjustCharacterFaceDir(targetmsg.role_id, targetmsg.face_direction);
            msi.IsMoving = targetmsg.is_moving;
            user.VelocityCoefficient = targetmsg.velocity_coefficient;

            UserAiStateInfo data = user.GetAiStateInfo();
            msi.TargetPosition = new Vector3(targetmsg.target_pos_x, msi.PositionY, targetmsg.target_pos_z);
            if (data.AiLogic != (int)AiStateLogicId.PvpUser_General)
            {
                data.AiLogic = (int)AiStateLogicId.PvpUser_General;
            }
            data.ChangeToState((int)AiStateId.Move);
        }
        else
        {
            //LogSystem.Debug("UserMove stop, user:{0} pos:{1}", user.GetId(), user.GetMovementStateInfo().GetPosition3D().ToString());

            MovementStateInfo msi = user.GetMovementStateInfo();
            msi.SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
            UserView view = EntityManager.Instance.GetUserViewById(targetmsg.role_id);
            if (null != view)
            {
                GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
            }

            msi.IsMoving = false;
            UserAiStateInfo data = user.GetAiStateInfo();
            data.ChangeToState((int)AiStateId.Wait);
        }

        /*NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
        if (null != view && !view.Visible) {
          view.Visible = true;
        }*/
    }
}

internal class Msg_RC_UserFace_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_UserFace targetmsg = msg as Msg_RC_UserFace;
        if (null == targetmsg)
        {
            return;
        }
        CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
        if (null == charObj)
        {
            return;
        }
        UserInfo user = charObj.CastUserInfo();
        if (user == null)
        {
            return;
        }
        if (user.IsHaveStateFlag(CharacterState_Type.CST_Sleep))
        {
            return;
        }
        //user.GetMovementStateInfo().SetFaceDir(targetmsg.face_direction);
        user.GetMovementStateInfo().SetWantFaceDir(targetmsg.face_direction);
        ControlSystemOperation.AdjustCharacterFaceDir(targetmsg.role_id, targetmsg.face_direction);
    }
}

internal class Msg_RC_CreateNpc_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_CreateNpc targetmsg = msg as Msg_RC_CreateNpc;
        if (null == targetmsg)
        {
            return;
        }
        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
        if (cb != null)
        {
            LogSystem.Debug("NpcCreate obj already exist:" + targetmsg.npc_id + " unit:" + targetmsg.unit_id);
            return;
        }
        LogSystem.Debug("NpcCreate:" + targetmsg.npc_id + " unit:" + targetmsg.unit_id);
        NpcInfo npc = null;
        if (-1 == targetmsg.unit_id)
        {
            npc = WorldSystem.Instance.CreateNpcByLinkId(targetmsg.npc_id, targetmsg.link_id);
        }
        else
        {
            npc = WorldSystem.Instance.CreateNpc(targetmsg.npc_id, targetmsg.unit_id);
        }
        if (null != npc)
        {
            npc.SetLevel(targetmsg.level);
            npc.SetAIEnable(false);
            npc.GetMovementStateInfo().SetPosition2D(targetmsg.cur_pos.x, targetmsg.cur_pos.z);
            npc.GetMovementStateInfo().SetFaceDir(targetmsg.face_direction);
            npc.GetMovementStateInfo().IsMoving = false;
            if (targetmsg.camp_id > 0)
            {
                npc.SetCampId(targetmsg.camp_id);
            }
            if (targetmsg.owner_id > 0)
            {
                npc.OwnerId = targetmsg.owner_id;
            }
            if (targetmsg.add_attr_id > 0)
            {
                AddNpcAttrByAttrId(npc, targetmsg.add_attr_id);
            }
            EntityManager.Instance.CreateNpcView(targetmsg.npc_id);
            npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
            if ((int)NpcTypeEnum.Partner == npc.NpcType)
            {
                if (npc.OwnerId == WorldSystem.Instance.PlayerSelfId)
                {
                    UserInfo player = WorldSystem.Instance.GetPlayerSelf();
                    if (null != player)
                    {
                        player.PartnerId = npc.GetId();
                    }
                    RoleInfo roleInfo = LobbyClient.Instance.CurrentRole;
                    if (null != roleInfo)
                    {
                        roleInfo.PartnerStateInfo.ActivePartnerHpPercent = 1.0f;
                        PartnerInfo partnerInfo = roleInfo.PartnerStateInfo.GetActivePartner();
                        if (null != partnerInfo)
                        {
                            AppendAttributeConfig aac = AppendAttributeConfigProvider.Instance.GetDataById(partnerInfo.GetAppendAttrConfigId());
                            float inheritDefenceAttrPercent = partnerInfo.GetInheritDefenceAttrPercent();
                            if (null != aac)
                            {
                                // attack
                                npc.GetBaseProperty().SetHpMax(Operate_Type.OT_Absolute, (int)(WorldSystem.Instance.GetPlayerSelf().GetActualProperty().HpMax * inheritDefenceAttrPercent));
                                npc.SetHp(Operate_Type.OT_Absolute, npc.GetBaseProperty().HpMax);
                            }
                        }
                    }
                    GfxSystem.PublishGfxEvent("ge_partner_summon_result", "ui", true);
                }
                UserInfo user = WorldSystem.Instance.GetCharacterById(npc.OwnerId) as UserInfo;
                if (null != user)
                {
                    PartnerInfo partnerInfo = user.GetPartnerInfo();
                    if (null != partnerInfo)
                    {
                        SkillInfo skillInfo = new SkillInfo(partnerInfo.BornSkill);
                        npc.GetSkillStateInfo().AddSkill(skillInfo);
                        npc.SkillController.ForceStartSkill(partnerInfo.BornSkill);
                    }
                }
            }
        }
    }
    private static void AddNpcAttrByAttrId(NpcInfo npc, int attr_id)
    {
        if (null == npc)
            return;
        ExpeditionMonsterAttrConfig attr = ExpeditionMonsterAttrConfigProvider.Instance.GetExpeditionMonsterAttrConfigById(attr_id);
        if (null != attr && null != attr.m_AttrData)
        {
            int addHp = (int)attr.m_AttrData.GetAddHpMax(0, npc.GetLevel());
            int addEnergy = (int)attr.m_AttrData.GetAddEpMax(0, npc.GetLevel());
            int addAd = (int)attr.m_AttrData.GetAddAd(0, npc.GetLevel());
            int addAdp = (int)attr.m_AttrData.GetAddADp(0, npc.GetLevel());
            int addMdp = (int)attr.m_AttrData.GetAddMDp(0, npc.GetLevel());
            int addHpRecover = (int)attr.m_AttrData.GetAddHpRecover(0, npc.GetLevel());
            int addEpRecover = (int)attr.m_AttrData.GetAddEpRecover(0, npc.GetLevel());
            npc.GetBaseProperty().SetHpMax(Operate_Type.OT_Relative, addHp);
            npc.GetBaseProperty().SetEnergyMax(Operate_Type.OT_Relative, addEnergy);
            npc.GetBaseProperty().SetAttackBase(Operate_Type.OT_Relative, addAd);
            npc.GetBaseProperty().SetADefenceBase(Operate_Type.OT_Relative, addAdp);
            npc.GetBaseProperty().SetMDefenceBase(Operate_Type.OT_Relative, addMdp);
            npc.GetBaseProperty().SetHpRecover(Operate_Type.OT_Relative, addHpRecover);
            npc.GetBaseProperty().SetEnergyRecover(Operate_Type.OT_Relative, addEpRecover);
            ///
            int curHp = (int)attr.m_AttrData.GetAddHpMax(0, npc.GetLevel());
            int curEnergy = (int)attr.m_AttrData.GetAddEpMax(0, npc.GetLevel());
            npc.SetHp(Operate_Type.OT_Absolute, curHp);
            npc.SetEnergy(Operate_Type.OT_Absolute, curEnergy);
            NpcAttrCalculator.Calc(npc);
            npc.SetHp(Operate_Type.OT_Absolute, npc.GetActualProperty().HpMax);
            npc.SetEnergy(Operate_Type.OT_Absolute, npc.GetActualProperty().EnergyMax);
        }
    }
}

internal class Msg_RC_DestroyNpc_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_DestroyNpc destroyMsg = msg as Msg_RC_DestroyNpc;
        if (destroyMsg == null)
        {
            return;
        }
        CharacterInfo info = WorldSystem.Instance.GetCharacterById(destroyMsg.npc_id);
        if (null == info)
        {
            LogSystem.Debug("NpcDestroy can't find obj:" + destroyMsg.npc_id);
            return;
        }
        LogSystem.Debug("NpcDestroy:" + destroyMsg.npc_id);

        NpcInfo npc = info.CastNpcInfo();
        if (null != npc)
        {
            if ((int)NpcTypeEnum.Partner == npc.NpcType && npc.OwnerId == WorldSystem.Instance.PlayerSelfId)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    role_info.PartnerStateInfo.ActivePartnerHpPercent = 0.0f;
                }
            }
            if (destroyMsg.need_play_effect)
            {
                CharacterView view = EntityManager.Instance.GetCharacterViewById(npc.GetId());
                if (null != view)
                {
                    GfxSystem.SendMessage(view.Actor, "OnEventDead", npc.NpcType);
                }
            }
            EntityManager.Instance.DestroyNpcView(npc.GetId());
            WorldSystem.Instance.DestroyCharacterById(npc.GetId());
            return;
        }
    }
}

internal class Msg_RC_NpcEnter_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_NpcEnter targetmsg = msg as Msg_RC_NpcEnter;
        if (null == targetmsg)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
        if (cb == null)
        {
            LogSystem.Debug("NpcEnter obj don't exist:" + targetmsg.npc_id);
            return;
        }
        LogSystem.Debug("NpcEnter:" + targetmsg.npc_id + " unit:" + cb.GetUnitId() + " FaceDir:" + targetmsg.face_direction);

        NpcInfo npc = cb.CastNpcInfo();
        if (null != npc)
        {
            MovementStateInfo msi = npc.GetMovementStateInfo();
            msi.SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
            NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
            if (null != view)
            {
                GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
                view.Visible = true;
            }

            msi.SetFaceDir(targetmsg.face_direction);
            msi.IsMoving = false;
            npc.GetAiStateInfo().Reset();
        }
    }
}

internal class Msg_RC_NpcMove_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_NpcMove targetmsg = msg as Msg_RC_NpcMove;
        if (null == targetmsg)
        {
            return;
        }
        CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
        if (null == charObj)
        {
            return;
        }
        NpcInfo npc = charObj.CastNpcInfo();
        if (npc == null)
        {
            return;
        }
        if (targetmsg.is_moving)
        {
            //LogSystem.Debug("NpcMove, npc:{0} pos:{1} move:{2}->{3}", npc.GetId(), npc.GetMovementStateInfo().GetPosition3D().ToString(), new Vector3(targetmsg.cur_pos_x, 0, targetmsg.cur_pos_z).ToString(), new Vector3(targetmsg.target_pos_x, targetmsg.target_pos_y, targetmsg.target_pos_z).ToString());

            MovementStateInfo msi = npc.GetMovementStateInfo();
            /*
            msi.SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
            NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
            if (null != view) {
              GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
            }*/

            msi.SetMoveDir(targetmsg.move_direction);
            //ControlSystemOperation.AdjustCharacterPosition(targetmsg.npc_id, targetmsg.cur_pos_x, targetmsg.cur_pos_z, TimeUtility.AverageRoundtripTime, targetmsg.move_direction);
            ControlSystemOperation.AdjustCharacterFaceDir(targetmsg.npc_id, targetmsg.face_direction);
            msi.IsMoving = targetmsg.is_moving;
            npc.VelocityCoefficient = targetmsg.velocity_coefficient;
            msi.MovementMode = (MovementMode)targetmsg.move_mode;
            npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, targetmsg.velocity);

            msi.TargetPosition = new Vector3(targetmsg.target_pos_x, msi.PositionY, targetmsg.target_pos_z);
        }
        else
        {
            //LogSystem.Debug("NpcMove stop, npc:{0} pos:{1}", npc.GetId(), npc.GetMovementStateInfo().GetPosition3D().ToString());

            MovementStateInfo msi = npc.GetMovementStateInfo();
            msi.SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
            NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
            if (null != view)
            {
                GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
            }

            msi.MovementMode = (MovementMode)targetmsg.move_mode;
            npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, targetmsg.velocity);
            msi.IsMoving = false;
            NpcAiStateInfo data = npc.GetAiStateInfo();
            data.ChangeToState((int)AiStateId.Wait);
        }

        /*NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
        if (null != view && !view.Visible) {
          view.Visible = true;
        }*/
    }
}

internal class Msg_RC_NpcFace_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_NpcFace face_msg = msg as Msg_RC_NpcFace;
        if (null == face_msg)
        {
            return;
        }
        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(face_msg.npc_id);
        if (cb == null)
            return;
        NpcInfo other = cb.CastNpcInfo();
        if (other != null)
        {
            if (other.IsHaveStateFlag(CharacterState_Type.CST_Sleep))
            {
                return;
            }
            //other.GetMovementStateInfo().SetFaceDir(face_msg.face_direction);
            other.GetMovementStateInfo().SetWantFaceDir(face_msg.face_direction);
            ControlSystemOperation.AdjustCharacterFaceDir(face_msg.npc_id, face_msg.face_direction);
        }

        //LogSystem.Debug("NpcFace, npc:{0} face:{1}", other.GetId(), face_msg.face_direction);
    }
}

internal class Msg_RC_NpcTarget_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_NpcTarget targetmsg = msg as Msg_RC_NpcTarget;
        if (null == targetmsg)
        {
            return;
        }
        LogSystem.Debug("NpcTarget, npc:{0} target:{1}", targetmsg.npc_id, targetmsg.target_id);
        CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
        if (null == charObj)
        {
            return;
        }
        NpcInfo npc = charObj.CastNpcInfo();
        if (npc == null)
        {
            return;
        }
        NpcAiStateInfo data = npc.GetAiStateInfo();
        data.Target = targetmsg.target_id;
        data.Time = 0;
    }
}

internal class Msg_RC_NpcSkill_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_NpcSkill targetmsg = msg as Msg_RC_NpcSkill;
        if (null == targetmsg)
        {
            return;
        }
        CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
        if (null == charObj)
        {
            return;
        }
        NpcInfo npc = charObj.CastNpcInfo();
        if (npc == null)
        {
            return;
        }
        LogSystem.Debug("Receive Msg_RC_NpcSkill, EntityId={0}, SkillId={1}", targetmsg.npc_id, targetmsg.skill_id);

        MovementStateInfo msi = npc.GetMovementStateInfo();
        msi.SetPosition2D(targetmsg.stand_pos.x, targetmsg.stand_pos.z);
        msi.SetFaceDir(targetmsg.face_direction);
        msi.SetWantFaceDir(targetmsg.face_direction);

        NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
        if (null != view)
        {
            view.ObjectInfo.FaceDir = targetmsg.face_direction;
            view.ObjectInfo.WantFaceDir = targetmsg.face_direction;
            GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
        }
        if (null != npc.SkillController)
        {
            npc.SkillController.ForceStartSkill(targetmsg.skill_id);
        }
    }
}

internal class Msg_CRC_NpcStopSkill_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_NpcStopSkill targetmsg = msg as Msg_CRC_NpcStopSkill;
        if (null == targetmsg)
        {
            return;
        }
        CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
        if (null == charObj)
        {
            return;
        }
        NpcInfo npc = charObj.CastNpcInfo();
        if (npc == null)
        {
            return;
        }
        LogSystem.Debug("Receive Msg_RC_NpcStopSkill, EntityId={0}, SkillId={1}", targetmsg.npc_id, targetmsg.skill_id);

        npc.SkillController.ForceInterruptCurSkill();
    }
}

internal class Msg_RC_NpcDead_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_NpcDead targetmsg = msg as Msg_RC_NpcDead;
        if (null == targetmsg)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
        if (null == cb)
        {
            LogSystem.Debug("NpcDead can't find obj:" + targetmsg.npc_id);
            return;
        }
        LogSystem.Debug("NpcDead:" + targetmsg.npc_id);

        //死亡的是NPC
        NpcInfo npc = cb.CastNpcInfo();
        if (null != npc)
        {
            npc.SetHp(Operate_Type.OT_Absolute, 0);
            npc.GfxStateFlag = 0;
            npc.GetMovementStateInfo().IsMoving = false;
        }
    }
}

internal class Msg_RC_NpcDisappear_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_NpcDisappear disappear = msg as Msg_RC_NpcDisappear;
        if (disappear == null)
        {
            return;
        }
        CharacterInfo info = WorldSystem.Instance.GetCharacterById(disappear.npc_id);
        if (null == info)
        {
            LogSystem.Debug("NpcDisappear can't find obj:" + disappear.npc_id);
            return;
        }
        LogSystem.Debug("NpcDisappear:" + disappear.npc_id);

        NpcInfo npc = info.CastNpcInfo();
        if (null != npc)
        {
            npc.GetMovementStateInfo().IsMoving = false;
            npc.GetMovementStateInfo().IsSkillMoving = false;

            NpcView view = EntityManager.Instance.GetNpcViewById(disappear.npc_id);
            if (null != view)
            {
                view.ObjectInfo.IsSkillGfxAnimation = false;
                view.ObjectInfo.IsSkillGfxMoveControl = false;
                view.ObjectInfo.IsImpactGfxAnimation = false;
                view.ObjectInfo.IsImpactGfxMoveControl = false;
                view.Visible = false;
            }
        }
    }
}

internal class Msg_RC_SyncProperty_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_SyncProperty targetmsg = msg as Msg_RC_SyncProperty;
        if (null == targetmsg)
        {
            return;
        }

        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
        if (null == cb)
        {
            return;
        }
        LogSystem.Debug("---set {2} hp to {0}/{1}", targetmsg.hp, cb.GetActualProperty().HpMax, cb.GetId());
        cb.SetHp(Operate_Type.OT_Absolute, targetmsg.hp);
        cb.SetEnergy(Operate_Type.OT_Absolute, targetmsg.np);
        cb.StateFlag = targetmsg.state;
    }
}

internal class Msg_RC_DebugSpaceInfo_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_DebugSpaceInfo targetmsg = msg as Msg_RC_DebugSpaceInfo;
        if (null == targetmsg) return;

        EntityManager.Instance.MarkSpaceInfoViews();
        if (GlobalVariables.Instance.IsDebug)
        {
            lock (GfxSystem.SyncLock)
            {

                for (int i = 0; i < targetmsg.space_infos.Count; i++)
                {
                    Msg_RC_DebugSpaceInfo.DebugSpaceInfo info = targetmsg.space_infos[i];
                    EntityManager.Instance.UpdateSpaceInfoView(info.obj_id, info.is_player, info.pos_x, 0, info.pos_z, info.face_dir);
                }
                /*
                foreach (Msg_RC_DebugSpaceInfo.DebugSpaceInfo info in targetmsg.space_infos) {
                  EntityManager.Instance.UpdateSpaceInfoView(info.obj_id, info.is_player, info.pos_x, 0, info.pos_z, info.face_dir);
                }*/
            }
        }
        EntityManager.Instance.DestroyUnusedSpaceInfoViews();
    }
}

internal class Msg_RC_SyncCombatStatisticInfo_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {

        Msg_RC_SyncCombatStatisticInfo message = msg as Msg_RC_SyncCombatStatisticInfo;
        if (null == message) return;

        UserInfo user = WorldSystem.Instance.UserManager.GetUserInfo(message.role_id);
        if (null != user)
        {
            CombatStatisticInfo info = user.GetCombatStatisticInfo();
            info.KillHeroCount = message.kill_hero_count;
            info.AssitKillCount = message.assit_kill_count;
            info.KillNpcCount = message.kill_npc_count;
            info.DeadCount = message.dead_count;

        }
    }
}

internal class Msg_RC_PvpCombatInfo_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_PvpCombatInfo message = msg as Msg_RC_PvpCombatInfo;
        if (null == message) return;
    }
}

internal class Msg_CRC_SendImpactToEntity_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_SendImpactToEntity message = msg as Msg_CRC_SendImpactToEntity;
        if (null == message) return;
        CharacterInfo target = WorldSystem.Instance.SceneContext.GetCharacterInfoById(message.target_id);
        if (null == target)
        {
            LogSystem.Debug("Receive Msg_RC_SendImpactToEntity, message.target_id={0} is not available", message.target_id);
            return;
        }
        else
        {
            LogSystem.Debug("Receive Msg_RC_SendImpactToEntity, TargetId={0}, ImpactId={1}, SenderId={2}, SkillId={3}",
              message.target_id, message.impact_id, message.sender_id, message.skill_id);
        }
        Vector3 senderPos = new Vector3(message.sender_pos.x, message.sender_pos.y, message.sender_pos.z);
        CharacterInfo sender = WorldSystem.Instance.GetCharacterById(message.sender_id);
        ImpactSystem.Instance.SendImpactToCharacter(sender, message.impact_id, message.target_id, message.skill_id, message.duration, senderPos, message.sender_dir);
    }
}

internal class Msg_CRC_StopGfxImpact_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_StopGfxImpact impact_msg = msg as Msg_CRC_StopGfxImpact;
        if (null != impact_msg)
        {
            CharacterInfo target = WorldSystem.Instance.SceneContext.GetCharacterInfoById(impact_msg.target_Id);
            if (null != target)
            {
                CharacterView view = EntityManager.Instance.GetCharacterViewById(target.GetId());
                if (null != view)
                {
                    GfxSystem.QueueGfxAction(GfxModule.Impact.GfxImpactSystem.Instance.StopGfxImpact, view.Actor, impact_msg.impact_Id);
                }
            }
        }
    }
}

internal class Msg_RC_ImpactDamage_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_ImpactDamage damage_msg = msg as Msg_RC_ImpactDamage;
        if (null != damage_msg)
        {
            CharacterInfo entity = WorldSystem.Instance.SceneContext.GetCharacterInfoById(damage_msg.role_id);
            if (null != entity)
            {
                int hpDamage = damage_msg.hp;
                entity.SetHp(Operate_Type.OT_Relative, hpDamage);
                //LogSystem.Debug("---set {2} hp to {0}/{1}", entity.Hp, entity.GetActualProperty().HpMax, entity.GetId());
                entity.SetAttackerInfo(damage_msg.attacker_id, 0, damage_msg.is_killer, damage_msg.is_ordinary, damage_msg.is_critical,
                                       damage_msg.hp,
                                       damage_msg.energy);
            }
        }
    }
}
internal class Msg_RC_ImpactRage_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_ImpactRage rage_msg = msg as Msg_RC_ImpactRage;
        if (null != rage_msg)
        {
            CharacterInfo entity = WorldSystem.Instance.SceneContext.GetCharacterInfoById(rage_msg.role_id);
            if (null != entity)
            {
                int rage = rage_msg.rage;
                entity.SetRage(Operate_Type.OT_Absolute, rage);
            }
        }
    }
}

internal class Msg_CRC_InteractObject_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_InteractObject _msg = msg as Msg_CRC_InteractObject;
        if (null == _msg)
            return;

        int initiatorId = _msg.initiator_id;
        int receiverId = _msg.receiver_id;

        UserInfo initiator = WorldSystem.Instance.GetCharacterById(initiatorId) as UserInfo;
        NpcInfo receiver = WorldSystem.Instance.GetCharacterById(receiverId) as NpcInfo;

        if (null != initiator && null != receiver)
        {
        }
    }
}

internal class Msg_RC_ControlObject_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_ControlObject _msg = msg as Msg_RC_ControlObject;
        if (null == _msg)
            return;



        CharacterInfo controller = WorldSystem.Instance.GetCharacterById(_msg.controller_id);
        if (controller == null)
            return;
        CharacterInfo controlled = WorldSystem.Instance.GetCharacterById(_msg.controlled_id);
        if (_msg.control_or_release)
        {
            if (null != controlled && !controller.IsDead() && !controlled.IsDead())
            {
                CharacterInfo.ControlObject(controller, controlled);
                controller.GetMovementStateInfo().SetPosition2D(controlled.GetMovementStateInfo().GetPosition2D());
                controller.GetMovementStateInfo().SetFaceDir(controlled.GetMovementStateInfo().GetFaceDir());
            }
        }
        else
        {
            CharacterInfo.ReleaseControlObject(controller, controlled);
            if (null != controlled)
            {
                controlled.GetMovementStateInfo().IsMoving = false;
            }
            if (null != controller)
            {
                UserView view = EntityManager.Instance.GetUserViewById(_msg.controller_id);
                if (null != view)
                {
                    view.Visible = true;
                    LogSystem.Debug("Show Player {0} link id {1} position {2} for Msg_RC_ControlObject", controller.GetId(), controller.GetLinkId(), controller.GetMovementStateInfo().GetPosition3D().ToString());
                }
            }
        }
    }
}

internal class Msg_RC_RefreshItemSkills_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_RefreshItemSkills _msg = msg as Msg_RC_RefreshItemSkills;
        if (null == _msg)
            return;
        CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(_msg.role_id);
        if (null == charObj)
        {
            return;
        }
        UserInfo user = charObj.CastUserInfo();
        if (null != user)
        {
            user.RefreshItemSkills();
        }
    }
}

internal class Msg_RC_HighlightPrompt_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_HighlightPrompt _msg = msg as Msg_RC_HighlightPrompt;
        if (null == _msg)
            return;
        WorldSystem.Instance.HighlightPrompt(_msg.dict_id, _msg.argument.ToArray());
    }
}

internal class Msg_RC_UpdateUserBattleInfo_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_UpdateUserBattleInfo _msg = msg as Msg_RC_UpdateUserBattleInfo;
        if (null == _msg)
            return;
        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(_msg.role_id);
        if (cb == null)
        {
            return;
        }
        UserInfo user = cb as UserInfo;
        if (null != _msg.skill_info && _msg.skill_info.Count > 0)
        {
            List<SkillTransmitArg> skill_assit = new List<SkillTransmitArg>();
            for (int i = 0; i < _msg.skill_info.Count; i++)
            {
                SkillTransmitArg info_ = new SkillTransmitArg();
                info_.SkillId = _msg.skill_info[i].skill_id;
                info_.SkillLevel = _msg.skill_info[i].skill_level;
                skill_assit.Add(info_);
            }
            ///
            int preset_index = _msg.preset_index;
            if (preset_index >= 0 && preset_index < 4)
            {
                cb.GetSkillStateInfo().RemoveAllSkill();
                for (int i = 0; i < skill_assit.Count; i++)
                {
                    if (skill_assit[i].SkillId > 0)
                    {
                        SkillInfo info = new SkillInfo(skill_assit[i].SkillId);
                        info.SkillLevel = skill_assit[i].SkillLevel;
                        info.Postions.SetCurSkillSlotPos(preset_index, (SlotPosition)(i + 1));
                        SkillCategory cur_skill_pos = SkillCategory.kNone;
                        if ((i + 1) == (int)SlotPosition.SP_A)
                        {
                            cur_skill_pos = SkillCategory.kSkillA;
                        }
                        else if ((i + 1) == (int)SlotPosition.SP_B)
                        {
                            cur_skill_pos = SkillCategory.kSkillB;
                        }
                        else if ((i + 1) == (int)SlotPosition.SP_C)
                        {
                            cur_skill_pos = SkillCategory.kSkillC;
                        }
                        else if ((i + 1) == (int)SlotPosition.SP_D)
                        {
                            cur_skill_pos = SkillCategory.kSkillD;
                        }
                        info.Category = cur_skill_pos;
                        cb.GetSkillStateInfo().AddSkill(info);
                        ///
                        if (null != user)
                        {
                            WorldSystem.Instance.AddSubSkill(user, info.SkillId, cur_skill_pos, info.SkillLevel);
                        }
                    }
                }
                Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(cb.GetLinkId());
                if (null != playerData && null != playerData.m_FixedSkillList
                  && playerData.m_FixedSkillList.Count > 0)
                {
                    for (int i = 0; i < playerData.m_FixedSkillList.Count; i++)
                    {
                        int skill_id = playerData.m_FixedSkillList[i];
                        SkillInfo info = new SkillInfo(skill_id, 1);
                        cb.GetSkillStateInfo().AddSkill(info);
                    }
                    /*
                    foreach (int skill_id in playerData.m_FixedSkillList) {
                      SkillInfo info = new SkillInfo(skill_id, 1);
                      cb.GetSkillStateInfo().AddSkill(info);
                    }*/
                }
                cb.ResetSkill();
            }
            if (cb.GetId() == WorldSystem.Instance.PlayerSelfId)
            {
                GfxSystem.EventChannelForLogic.Publish("ge_request_equiped_skills", "ui");
            }
        }
        ///
        if (null != _msg.equip_info && _msg.equip_info.Count > 0)
        {
            for (int i = 0; i < _msg.equip_info.Count; i++)
            {
                cb.GetEquipmentStateInfo().ResetEquipmentData(i);
                ItemDataInfo info = new ItemDataInfo();
                info.Level = _msg.equip_info[i].equip_level;
                info.ItemNum = 1;
                info.RandomProperty = _msg.equip_info[i].equip_random_property;
                info.ItemConfig = ItemConfigProvider.Instance.GetDataById(_msg.equip_info[i].equip_id);
                if (null != info.ItemConfig)
                {
                    cb.GetEquipmentStateInfo().SetEquipmentData(i, info);
                }
            }
        }
        ///
        if (null != _msg.legacy_info && _msg.legacy_info.Count > 0)
        {
            for (int i = 0; i < _msg.legacy_info.Count; i++)
            {
                if (null != _msg.legacy_info[i] && _msg.legacy_info[i].legacy_IsUnlock)
                {
                    cb.GetLegacyStateInfo().ResetLegacyData(i);
                    int item_id = _msg.legacy_info[i].legacy_id;
                    if (item_id > 0)
                    {
                        ItemDataInfo info = new ItemDataInfo();
                        info.Level = _msg.legacy_info[i].legacy_level;
                        info.ItemNum = 1;
                        info.RandomProperty = _msg.legacy_info[i].legacy_random_property;
                        info.IsUnlock = _msg.legacy_info[i].legacy_IsUnlock;
                        info.ItemConfig = ItemConfigProvider.Instance.GetDataById(item_id);
                        if (null != info.ItemConfig)
                        {
                            cb.GetLegacyStateInfo().SetLegacyData(i, info);
                        }
                    }
                }
            }
            cb.GetLegacyStateInfo().UpdateLegacyComplexAttr();
        }
        cb.GetXSoulInfo().GetAllXSoulPartData().Clear();
        for (int i = 0; i < _msg.XSouls.Count; i++)
        {
            ItemDataInfo item = new ItemDataInfo();
            item.ItemId = _msg.XSouls[i].ItemId;
            item.Level = _msg.XSouls[i].Level;
            item.Experience = _msg.XSouls[i].Experience;
            ItemConfig config = ItemConfigProvider.Instance.GetDataById(item.ItemId);
            if (config != null)
            {
                item.ItemConfig = config;
                XSoulPartInfo part = new XSoulPartInfo((XSoulPart)config.m_WearParts, item);
                part.ShowModelLevel = _msg.XSouls[i].ModelLevel;
                cb.GetXSoulInfo().SetXSoulPartData((XSoulPart)config.m_WearParts, part);
            }
        }
        UserView view = EntityManager.Instance.GetUserViewById(cb.GetId());
        if (view != null)
        {
            view.UpdateXSoulEquip();
        }
        if (null != user)
        {
            for (int i = 0; i < _msg.Partners.Count; ++i)
            {
                PartnerConfig partnerConfig = PartnerConfigProvider.Instance.GetDataById(_msg.Partners[i].PartnerId);
                if (null != partnerConfig)
                {
                    PartnerInfo partner = new PartnerInfo(partnerConfig);
                    partner.CurAdditionLevel = _msg.Partners[i].PartnerLevel;
                    partner.CurSkillStage = _msg.Partners[i].PartnerStage;
                    user.SetPartnerInfo(partner);
                }
            }
        }
        view.UpdateEquipment();
        //LogSystem.Debug("---in updateuser battle info!");
        cb.SetHp(Operate_Type.OT_Absolute, cb.GetActualProperty().HpMax);
        cb.SetEnergy(Operate_Type.OT_Absolute, cb.GetActualProperty().EnergyMax);
    }
}

internal class Msg_RC_MissionCompleted_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_MissionCompleted _msg = msg as Msg_RC_MissionCompleted;
        if (null == _msg)
            return;

        NetworkSystem.Instance.QuitBattlePassive();
        GfxSystem.EventChannelForLogic.Publish("ge_stage_clear", "lobby", _msg.main_scene_id);
    }
}

internal class Msg_RC_ChangeScene_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_ChangeScene _msg = msg as Msg_RC_ChangeScene;
        if (null == _msg)
            return;
        WorldSystem.Instance.QuitBattle();
        int mainSceneId = _msg.main_scene_id;
        if (mainSceneId <= 0)
        {
            mainSceneId = LobbyClient.Instance.CurrentRole.CitySceneId;
        }
        WorldSystem.Instance.ChangeScene(mainSceneId);
    }
}

internal class Msg_RC_CampChanged_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_CampChanged _msg = msg as Msg_RC_CampChanged;
        if (null == _msg)
            return;
        CharacterInfo obj = WorldSystem.Instance.GetCharacterById(_msg.obj_id);
        if (null != obj)
        {
            obj.SetCampId(_msg.camp_id);

            CharacterView view = EntityManager.Instance.GetCharacterViewById(_msg.obj_id);
            if (null != view)
            {
                view.ObjectInfo.CampId = _msg.camp_id;
            }
        }
    }
}

internal class Msg_RC_EnableInput_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_EnableInput _msg = msg as Msg_RC_EnableInput;
        if (null == _msg)
            return;
        PlayerControl.Instance.EnableMoveInput = _msg.is_enable;
        PlayerControl.Instance.EnableRotateInput = _msg.is_enable;
    }
}

internal class Msg_RC_ShowUi_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_ShowUi _msg = msg as Msg_RC_ShowUi;
        if (null == _msg)
            return;
        GfxSystem.SendMessage("GfxGameRoot", "ShowUi", _msg.is_show);
    }
}

internal class Msg_RC_ShowWall_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_ShowWall _msg = msg as Msg_RC_ShowWall;
        if (null == _msg)
            return;
        if (_msg.is_show)
            GfxSystem.SendMessage(_msg.wall_name, "OpenDoor", null);
        else
            GfxSystem.SendMessage(_msg.wall_name, "CloseDoor", null);
    }
}

internal class Msg_RC_ShowDlg_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_ShowDlg _msg = msg as Msg_RC_ShowDlg;
        if (null == _msg)
            return;
        GfxSystem.SendMessage("GfxGameRoot", "TriggerStory", _msg.dialog_id);
    }
}

internal class Msg_RC_CameraLookat_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_CameraLookat _msg = msg as Msg_RC_CameraLookat;
        if (null == _msg)
            return;
        if (_msg.is_immediately)
        {
            GfxSystem.SendMessage("GfxGameRoot", "CameraLookatImmediately", new float[] { _msg.x, _msg.y, _msg.z });
        }
        else
        {
            GfxSystem.SendMessage("GfxGameRoot", "CameraLookat", new float[] { _msg.x, _msg.y, _msg.z });
        }
    }
}

internal class Msg_RC_CameraFollow_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_CameraFollow _msg = msg as Msg_RC_CameraFollow;
        if (null == _msg)
            return;
        CharacterView view = EntityManager.Instance.GetCharacterViewById(_msg.obj_id);
        if (null != view)
        {
            if (_msg.is_immediately)
            {
                GfxSystem.SendMessage("GfxGameRoot", "CameraFollowImmediately", view.Actor);
            }
            else
            {
                GfxSystem.SendMessage("GfxGameRoot", "CameraFollow", view.Actor);
            }
        }
    }
}

internal class Msg_CRC_GfxControlMoveStart_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_GfxControlMoveStart _msg = msg as Msg_CRC_GfxControlMoveStart;
        if (null == _msg)
            return;
        CharacterInfo target = WorldSystem.Instance.GetCharacterById(_msg.obj_id);
        if (null != target)
        {
            LogSystem.Debug("Msg_CRC_GfxControlMoveStart {0}, skill_or_impact {1}, is_skill {2}", _msg.obj_id, _msg.skill_or_impact_id, _msg.is_skill);

            CharacterView view = EntityManager.Instance.GetCharacterViewById(_msg.obj_id);
            if (null != view)
            {
            }
        }
    }
}

internal class Msg_CRC_GfxControlMoveStop_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_CRC_GfxControlMoveStop _msg = msg as Msg_CRC_GfxControlMoveStop;
        if (null == _msg)
            return;
        CharacterInfo target = WorldSystem.Instance.GetCharacterById(_msg.obj_id);
        if (null != target)
        {
            LogSystem.Debug("Msg_CRC_GfxControlMoveStop {0}, skill_or_impact {1}, is_skill {2}, pos ({3},{4}), face {5}", _msg.obj_id, _msg.skill_or_impact_id, _msg.is_skill, _msg.target_pos.x, _msg.target_pos.z, _msg.face_dir);

            MovementStateInfo msi = target.GetMovementStateInfo();
            msi.SetPosition2D(_msg.target_pos.x, _msg.target_pos.z);
            msi.SetFaceDir(_msg.face_dir);

            CharacterView view = EntityManager.Instance.GetCharacterViewById(_msg.obj_id);
            if (null != view)
            {
                GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, msi.PositionX, msi.PositionZ);
                GfxSystem.UpdateGameObjectLocalRotateY(view.Actor, _msg.face_dir);
            }
        }
    }
}

internal class Msg_RC_UpdateCoefficient_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_UpdateCoefficient _msg = msg as Msg_RC_UpdateCoefficient;
        if (null == _msg)
            return;
        CharacterInfo target = WorldSystem.Instance.GetCharacterById(_msg.obj_id);
        if (null != target)
        {
            target.HpMaxCoefficient = _msg.hpmax_coefficient;
            target.EnergyMaxCoefficient = _msg.hpmax_coefficient;

            UserInfo user = target as UserInfo;
            if (null != user)
            {
                UserAttrCalculator.Calc(user);
            }
            else
            {
                NpcInfo npc = target as NpcInfo;
                if (null != npc)
                {
                    NpcAttrCalculator.Calc(npc);
                }
            }
        }
    }
}

internal class Msg_RC_AdjustPosition_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_AdjustPosition _msg = msg as Msg_RC_AdjustPosition;
        if (null == _msg)
            return;
        CharacterInfo target = WorldSystem.Instance.GetCharacterById(_msg.role_id);
        if (null != target)
        {
            UserInfo user = target as UserInfo;
            if (null != user)
            {
                user.GetMovementStateInfo().SetPosition2D(_msg.x, _msg.z);

                UserView view = EntityManager.Instance.GetUserViewById(_msg.role_id);
                if (null != view)
                {
                    GfxSystem.UpdateGameObjectLocalPosition2D(view.Actor, _msg.x, _msg.z);
                }
            }
        }
    }
}

internal class Msg_RC_LockFrame_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_LockFrame _msg = msg as Msg_RC_LockFrame;
        if (null == _msg)
            return;
        GfxSystem.SetTimeScale(_msg.scale);
    }
}

internal class Msg_RC_PlayAnimation_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_PlayAnimation _msg = msg as Msg_RC_PlayAnimation;
        if (null == _msg)
            return;
        CharacterView view = EntityManager.Instance.GetCharacterViewById(_msg.obj_id);
        if (null != view)
        {
            view.PlayStoryAnimation((Animation_Type)_msg.anim_type, _msg.anim_time, _msg.is_queued);
        }
    }
}

internal class Msg_RC_CameraYaw_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_CameraYaw _msg = msg as Msg_RC_CameraYaw;
        if (null == _msg)
            return;
        GfxSystem.SendMessage("GfxGameRoot", "CameraYaw", new float[] { _msg.yaw, _msg.smooth_lag });
    }
}

internal class Msg_RC_CameraHeight_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_CameraHeight _msg = msg as Msg_RC_CameraHeight;
        if (null == _msg)
            return;
        GfxSystem.SendMessage("GfxGameRoot", "CameraHeight", new float[] { _msg.height, _msg.smooth_lag });
    }
}

internal class Msg_RC_CameraDistance_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_CameraDistance _msg = msg as Msg_RC_CameraDistance;
        if (null == _msg)
            return;
        GfxSystem.SendMessage("GfxGameRoot", "CameraDistance", new float[] { _msg.distance, _msg.smooth_lag });
    }
}

internal class Msg_RC_SetBlockedShader_Handler
{
    internal static void Execute(object msg, NetConnection conn)
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
        WorldSystem.Instance.SetBlockedShader(rimColor1, rimPower1, rimCutValue1, rimColor2, rimPower2, rimCutValue2);
    }
}

internal class Msg_RC_StartCountDown_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_StartCountDown _msg = msg as Msg_RC_StartCountDown;
        if (null == _msg)
            return;
        GfxSystem.SendMessage("GfxGameRoot", "StartCountDown", _msg.count_down_time);
    }
}

internal class Msg_RC_PublishEvent_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_PublishEvent _msg = msg as Msg_RC_PublishEvent;
        if (null == _msg)
            return;
        try
        {
            bool isLogic = _msg.is_logic_event;
            string name = _msg.ev_name;
            string group = _msg.group;
            ArrayList args = new ArrayList();
            for (int i = 0; i < _msg.args.Count; i++)
            {
                switch (_msg.args[i].val_type)
                {
                    case 0://null
                        args.Add(null);
                        break;
                    case 1://int
                        args.Add(int.Parse(_msg.args[i].str_val));
                        break;
                    case 2://float
                        args.Add(float.Parse(_msg.args[i].str_val));
                        break;
                    default://string
                        args.Add(_msg.args[i].str_val);
                        break;
                }
            }
            /*
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
            }*/
            object[] objArgs = args.ToArray();
            if (isLogic)
                GfxSystem.EventChannelForLogic.Publish(name, group, objArgs);
            else
                GfxSystem.PublishGfxEvent(name, group, objArgs);
        }
        catch (Exception ex)
        {
            LogSystem.Error("Msg_RC_PublishEvent_Handler throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
}

internal class Msg_RC_CameraEnable_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_CameraEnable _msg = msg as Msg_RC_CameraEnable;
        if (null == _msg)
            return;
        string camera = _msg.camera_name;
        bool isEnable = _msg.is_enable;
        GfxSystem.SendMessage("GfxGameRoot", "CameraEnable", new object[] { camera, isEnable });
    }
}

internal class Msg_RC_SendGfxMessage_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_SendGfxMessage _msg = msg as Msg_RC_SendGfxMessage;
        if (null == _msg)
            return;
        try
        {
            bool isWithTag = _msg.is_with_tag;
            string name = _msg.name;
            string message = _msg.msg;
            ArrayList args = new ArrayList();
            for (int i = 0; i < _msg.args.Count; i++)
            {
                switch (_msg.args[i].val_type)
                {
                    case 0://null
                        args.Add(null);
                        break;
                    case 1://int
                        args.Add(int.Parse(_msg.args[i].str_val));
                        break;
                    case 2://float
                        args.Add(float.Parse(_msg.args[i].str_val));
                        break;
                    default://string
                        args.Add(_msg.args[i].str_val);
                        break;
                }
            }
            /*
            foreach (Msg_RC_SendGfxMessage.EventArg arg in _msg.args) {
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
            }*/
            object[] objArgs = args.ToArray();
            if (isWithTag)
            {
                if (objArgs.Length == 1)
                    GfxSystem.SendMessageWithTag(name, message, objArgs[0]);
                else
                    GfxSystem.SendMessageWithTag(name, message, objArgs);
            }
            else
            {
                if (objArgs.Length == 1)
                    GfxSystem.SendMessage(name, message, objArgs[0]);
                else
                    GfxSystem.SendMessage(name, message, objArgs);
            }
        }
        catch (Exception ex)
        {
            LogSystem.Error("Msg_RC_PublishEvent_Handler throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
}

internal class Msg_RC_SendGfxMessageById_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_SendGfxMessageById _msg = msg as Msg_RC_SendGfxMessageById;
        if (null == _msg)
            return;
        try
        {
            int objid = _msg.obj_id;
            CharacterView view = EntityManager.Instance.GetCharacterViewById(objid);
            if (null != view)
            {
                string message = _msg.msg;
                ArrayList args = new ArrayList();
                for (int i = 0; i < _msg.args.Count; i++)
                {
                    switch (_msg.args[i].val_type)
                    {
                        case 0://null
                            args.Add(null);
                            break;
                        case 1://int
                            args.Add(int.Parse(_msg.args[i].str_val));
                            break;
                        case 2://float
                            args.Add(float.Parse(_msg.args[i].str_val));
                            break;
                        default://string
                            args.Add(_msg.args[i].str_val);
                            break;
                    }
                }
                /*
                foreach (Msg_RC_SendGfxMessageById.EventArg arg in _msg.args) {
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
                }*/
                object[] objArgs = args.ToArray();
                if (objArgs.Length == 1)
                    GfxSystem.SendMessage(view.Actor, message, objArgs[0]);
                else
                    GfxSystem.SendMessage(view.Actor, message, objArgs);
            }
        }
        catch (Exception ex)
        {
            LogSystem.Error("Msg_RC_PublishEvent_Handler throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
}

internal class Msg_RC_AddSkill_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_AddSkill _msg = msg as Msg_RC_AddSkill;
        if (null == _msg)
            return;
        CharacterInfo obj = WorldSystem.Instance.GetCharacterById(_msg.obj_id);
        if (null != obj)
        {
            if (obj.GetSkillStateInfo().GetSkillInfoById(_msg.skill_id) == null)
            {
                obj.GetSkillStateInfo().AddSkill(new SkillInfo(_msg.skill_id));
            }
        }
    }
}

internal class Msg_RC_RemoveSkill_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_RemoveSkill _msg = msg as Msg_RC_RemoveSkill;
        if (null == _msg)
            return;
        CharacterInfo obj = WorldSystem.Instance.GetCharacterById(_msg.obj_id);
        if (null != obj)
        {
            obj.GetSkillStateInfo().RemoveSkill(_msg.skill_id);
        }
    }
}

internal class Msg_RC_StopImpact_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_StopImpact _msg = msg as Msg_RC_StopImpact;
        if (null == _msg)
            return;
        CharacterInfo obj = WorldSystem.Instance.GetCharacterById(_msg.obj_id);
        if (null != obj)
        {
            ImpactSystem.Instance.StopImpactById(obj, _msg.impact_id);
        }
    }
}

internal class Msg_CRC_SummonNpc_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        //LogSystem.Debug("----got summon npc msg!!");
        Msg_CRC_SummonNpc target_msg = msg as Msg_CRC_SummonNpc;
        if (null == target_msg)
        {
            return;
        }
        NpcInfo npc = GameLogicNotification.SummonNpc(target_msg.npc_id, target_msg.summon_owner_id,
                                                      target_msg.owner_skillid,
                                                      target_msg.link_id,
                                                      target_msg.model_prefab,
                                                      target_msg.skill_id,
                                                      target_msg.ai_id,
                                                      target_msg.follow_dead,
                                                      target_msg.pos_x, target_msg.pos_y, target_msg.pos_z,
                                                      target_msg.ai_params,
                                                      0,//?
                                                      false,
                                                      true);
        npc.OwnerId = target_msg.owner_id;
    }
}

internal class Msg_RC_SyncNpcOwnerId_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_SyncNpcOwnerId target_msg = msg as Msg_RC_SyncNpcOwnerId;
        if (null == target_msg)
        {
            return;
        }
        CharacterInfo obj = WorldSystem.Instance.GetCharacterById(target_msg.npc_id);
        if (null != obj)
        {
            NpcInfo npc = obj as NpcInfo;
            if (null != npc)
                npc.OwnerId = target_msg.owner_id;
        }
    }
}

internal class Msg_RC_DropNpc_Handler
{
    internal static void Execute(object msg, NetConnection conn)
    {
        Msg_RC_DropNpc targetmsg = msg as Msg_RC_DropNpc;
        if (null == targetmsg)
        {
            return;
        }
        CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
        if (cb != null)
        {
            LogSystem.Debug("DropNpc obj already exist {0}", targetmsg.npc_id);
            return;
        }
        CharacterInfo from_npc = WorldSystem.Instance.GetCharacterById(targetmsg.from_npc_id);
        if (null == from_npc)
        {
            LogSystem.Debug("DropNpc obj from npc not exist {0}", targetmsg.from_npc_id);
            return;
        }
        //LogSystem.Debug("DropNpcCreate: npc id = {0} drop type = {1}, drop num = {2}.", targetmsg.npc_id, targetmsg.drop_type, targetmsg.drop_num);
        NpcInfo npc = WorldSystem.Instance.CreateNpcByLinkId(targetmsg.npc_id, targetmsg.link_id);
        if (null != npc)
        {
            npc.SetAIEnable(true);
            npc.IsBorning = true;
            Vector3 pos = from_npc.GetMovementStateInfo().GetPosition3D();
            npc.GetMovementStateInfo().SetPosition(pos + new Vector3(0, 0.5f, 0));
            npc.GetMovementStateInfo().IsMoving = false;
            if (targetmsg.camp_id > 0)
            {
                npc.SetCampId(targetmsg.camp_id);
            }
            if (targetmsg.owner_id > 0)
            {
                npc.OwnerId = targetmsg.owner_id;
            }
            npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
            npc.GetAiStateInfo().AiLogic = (int)AiStateLogicId.DropOut_AutoPick;
            DropOutInfo dropInfo = new DropOutInfo();
            dropInfo.DropType = (DropOutType)targetmsg.drop_type;
            dropInfo.Model = targetmsg.model;
            dropInfo.Particle = targetmsg.effect;
            dropInfo.Value = targetmsg.drop_num;
            npc.GetAiStateInfo().AiDatas.AddData<DropOutInfo>(dropInfo);
            npc.SetModel(targetmsg.model);
            EntityManager.Instance.CreateNpcView(targetmsg.npc_id);
            /*
            CharacterView view = EntityManager.Instance.GetCharacterViewById(targetmsg.npc_id);
            if(null != view){
              GfxSystem.SetRigidbodyVelocity(view.Actor, 0, 10, 0);
            }
             */
        }
    }
}