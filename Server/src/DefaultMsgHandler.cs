using System.Collections.Generic;
using Google.ProtocolBuffers;
using ArkCrossEngineMessage;
using DashFire;
using ArkCrossEngineSpatial;
using ArkCrossEngine;

namespace DashFire
{
    class DefaultMsgHandler
    {
        internal static void Execute(object msg, User user)
        {
            LogSys.Log(LOG_TYPE.ERROR, "Unhandled msg {0} !!!", msg.GetType());
        }
    }

    class EnterHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_Create enter_msg = msg as Msg_CRC_Create;
            if (enter_msg == null)
            {
                return;
            }
            LogSys.Log(LOG_TYPE.DEBUG, "user {0}({1}) enter.", user.RoleId, user.GetKey());
            user.UserControlState = (int)UserControlState.User;
            user.IsEntered = true;

            Room room = user.OwnRoom;
            if (null != room)
            {
                Scene scene = room.GetActiveScene();
                if (null != scene)
                {
                    UserInfo userInfo = user.Info;
                    if (null != userInfo)
                    {
                        userInfo.GetXSoulInfo().GetAllXSoulPartData().Clear();
                        foreach (var pair in user.XSouls.GetAllXSoulPartData())
                        {
                            userInfo.GetXSoulInfo().SetXSoulPartData(pair.Key, pair.Value);
                        }
                        foreach (var pair in userInfo.GetXSoulInfo().GetAllXSoulPartData())
                        {
                            XSoulPartInfo part_info = pair.Value;
                            foreach (int impactid in part_info.GetActiveImpacts())
                            {
                                //LogSys.Log(LOG_TYPE.DEBUG, "---add xsoul impact to self: " + impactid);
                                ImpactSystem.Instance.SendImpactToCharacter(userInfo, impactid, userInfo.GetId(),
                                              -1, -1, userInfo.GetMovementStateInfo().GetPosition3D(),
                                              userInfo.GetMovementStateInfo().GetFaceDir());
                            }
                        }
                        if (scene.SceneState == SceneState.Running)
                        {
                            Data_Unit unit = scene.MapData.ExtractData(DataMap_Type.DT_Unit, userInfo.GetUnitId()) as Data_Unit;
                            if (null != unit)
                            {
                                userInfo.GetMovementStateInfo().SetPosition(unit.m_Pos);
                                userInfo.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
                                userInfo.RevivePoint = unit.m_Pos;
                            }
                            scene.SyncForNewUser(user);
                        }
                    }
                }
            }
        }
    }

    class MoveHandler
    {
        internal static void OnMoveStart(object msg, User user)
        {
            Msg_CRC_MoveStart move_msg = msg as Msg_CRC_MoveStart;
            if (null == move_msg)
                return;

            var msi = user.Info.GetMovementStateInfo();
            msi.SetMoveDir(move_msg.dir);
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                //scene.ControlSystemOperation.AdjustCharacterMoveDir(user.RoleId, move_msg.dir);
            }
            msi.StartMove();

            if (!msi.IsSkillMoving)
            {
                float x = move_msg.position.x;
                float z = move_msg.position.z;
                float velocity = (float)user.Info.GetActualProperty().MoveSpeed;
                if (!user.VerifyPosition(x, z, velocity, move_msg.send_time, 4.0f))
                {
                    //todo:记录违规次数
                }
                msi.SetPosition2D(x, z);

                user.SampleMoveData(x, z, velocity, msi.MoveDirCosAngle, msi.MoveDirSinAngle, move_msg.send_time);
            }

            user.LastIsMoving = true;
            //LogSys.Log(LOG_TYPE.DEBUG, "MoveStart User:{0} dir:{1} isskillmoving:{2} ismovemeetobstacle:{3} time:{4} client time:{5}", user.RoleId, move_msg.dir, msi.IsSkillMoving, msi.IsMoveMeetObstacle, TimeUtility.GetServerMilliseconds(), move_msg.send_time);
        }

        internal static void OnMoveStop(object msg, User user)
        {
            Msg_CRC_MoveStop move_msg = msg as Msg_CRC_MoveStop;
            if (null == move_msg)
                return;
            var msi = user.Info.GetMovementStateInfo();
            msi.StopMove();
            msi.IsMoving = false;

            if (!msi.IsSkillMoving)
            {
                float x = move_msg.position.x;
                float z = move_msg.position.z;
                float velocity = (float)user.Info.GetActualProperty().MoveSpeed;
                if (!user.VerifyMovingPosition(x, z, velocity, move_msg.send_time))
                {
                    //todo:记录违规次数
                    /*
                    Msg_RC_AdjustPosition syncMsg = new Msg_RC_AdjustPosition();
                    syncMsg.role_id = user.RoleId;
                    syncMsg.x = x;
                    syncMsg.z = z;
                    user.BroadCastMsgToRoom(syncMsg, false);
                    */
                }
                msi.SetPosition2D(x, z);

                user.SampleMoveData(x, z, velocity, msi.MoveDirCosAngle, msi.MoveDirSinAngle, move_msg.send_time);
            }
            user.LastIsMoving = false;
            //LogSys.Log(LOG_TYPE.DEBUG, "MoveStop User:{0} isskillmoving:{1} ismovemeetobstacle:{2} time:{3} client time:{4}", user.RoleId, msi.IsSkillMoving, msi.IsMoveMeetObstacle, TimeUtility.GetServerMilliseconds(), move_msg.send_time);
        }
    }

    class FaceDirHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_Face face_dir = msg as Msg_CRC_Face;
            if (null == face_dir)
                return;

            user.Info.GetMovementStateInfo().SetFaceDir(face_dir.face_direction);
        }
    }

    internal class MoveMeetObstacleHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_MoveMeetObstacle obstacle_msg = msg as Msg_CRC_MoveMeetObstacle;
            if (null == obstacle_msg)
            {
                return;
            }
            UserInfo userInfo = user.Info;
            if (userInfo != null)
            {
                MovementStateInfo msi = userInfo.GetMovementStateInfo();
                userInfo.GetMovementStateInfo().IsMoveMeetObstacle = true;

                if (!msi.IsSkillMoving)
                {
                    float x = obstacle_msg.cur_pos_x;
                    float z = obstacle_msg.cur_pos_z;
                    float velocity = (float)user.Info.GetActualProperty().MoveSpeed;
                    if (!user.VerifyPosition(x, z, velocity, obstacle_msg.send_time, 4.0f))
                    {
                        //todo:记录违规次数
                    }
                    msi.SetPosition2D(x, z);

                    user.SampleMoveData(obstacle_msg.cur_pos_x, obstacle_msg.cur_pos_z, velocity, msi.MoveDirCosAngle, msi.MoveDirSinAngle, obstacle_msg.send_time);
                }

                //LogSys.Log(LOG_TYPE.DEBUG, "MoveMeetObstacleHandler User:{0} isskillmoving:{1} ismovemeetobstacle:{2} time:{3} client time:{4}", user.RoleId, msi.IsSkillMoving, msi.IsMoveMeetObstacle, TimeUtility.GetServerMilliseconds(), obstacle_msg.send_time);
            }
        }
    }

    class UseSkillHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_Skill use_skill = msg as Msg_CRC_Skill;
            if (use_skill == null)
                return;
            CharacterInfo charactor = user.Info;
            if (charactor == null)
            {
                LogSys.Log(LOG_TYPE.ERROR, "UseSkillHandler, charactor {0} not exist", user.RoleId);
                return;
            }
            charactor = charactor.GetRealControlledObject();
            MovementStateInfo msi = charactor.GetMovementStateInfo();

            if (!msi.IsSkillMoving)
            {
                float x = use_skill.stand_pos.x;
                float z = use_skill.stand_pos.z;
                float velocity = (float)user.Info.GetActualProperty().MoveSpeed;
                if (!user.VerifyPosition(x, z, velocity, use_skill.send_time, 4.0f))
                {
                    //todo:记录违规次数
                }
                msi.SetPosition2D(x, z);

                user.SampleMoveData(use_skill.stand_pos.x, use_skill.stand_pos.z, velocity, msi.MoveDirCosAngle, msi.MoveDirSinAngle, use_skill.send_time);
            }

            msi.SetFaceDir(use_skill.face_direction);

            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                scene.SkillSystem.StartSkill(user.RoleId, use_skill.skill_id);
            }

            //LogSys.Log(LOG_TYPE.DEBUG, "UseSkillHandler User:{0} skill:{1} isskillmoving:{2} ismovemeetobstacle:{3} time:{4} client time:{5}", user.RoleId, use_skill.skill_id, msi.IsSkillMoving, msi.IsMoveMeetObstacle, TimeUtility.GetServerMilliseconds(), use_skill.send_time);
        }
    }

    class StopSkillHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_StopSkill end_skill = msg as Msg_CRC_StopSkill;
            if (end_skill == null)
                return;
            CharacterInfo charactor = user.Info;
            if (charactor == null)
            {
                LogSys.Log(LOG_TYPE.ERROR, "StopSkillHandler, charactor {0} not exist", user.RoleId);
                return;
            }
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                scene.SkillSystem.StopSkill(user.RoleId);
            }
        }
    }

    class NpcStopSkillHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_NpcStopSkill end_skill = msg as Msg_CRC_NpcStopSkill;
            if (end_skill == null)
                return;
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                CharacterInfo charactor = scene.SceneContext.GetCharacterInfoById(end_skill.npc_id);
                if (charactor == null)
                {
                    LogSys.Log(LOG_TYPE.ERROR, "NpcStopSkillHandler, charactor {0} not exist", user.RoleId);
                    return;
                }
                if (charactor.OwnerId != user.RoleId)
                {
                    LogSys.Log(LOG_TYPE.ERROR, "NpcStopSkillHandler, charactor {0} owner {1} not user {2}", charactor.GetId(), charactor.OwnerId, user.RoleId);
                    return;
                }
                scene.SkillSystem.StopSkill(end_skill.npc_id);
                user.BroadCastMsgToRoom(end_skill);
            }
        }
    }

    class SendImpactToEntityHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_SendImpactToEntity impact_msg = msg as Msg_CRC_SendImpactToEntity;
            if (null != impact_msg)
            {
                Scene scene = user.OwnRoom.GetActiveScene();
                if (null != scene)
                {
                    CharacterInfo character = scene.SceneContext.GetCharacterInfoById(impact_msg.sender_id);
                    if (null != character && (character.OwnerId == user.RoleId || impact_msg.sender_id == user.RoleId))
                    {
                        //校验
                        Vector3 senderPos = new Vector3(impact_msg.sender_pos.x, impact_msg.sender_pos.y, impact_msg.sender_pos.z);
                        SkillInfo skillInfo = character.GetSkillStateInfo().GetSkillInfoById(impact_msg.skill_id);
                        if (null != skillInfo && null != skillInfo.m_EnableImpactsToMyself && null != skillInfo.m_EnableImpactsToOther)
                        {
                            //LogSys.Log(LOG_TYPE.DEBUG, "----update hit count to " + impact_msg.hit_count);
                            UpdateCharacterHitCount(character, impact_msg.hit_count);
                            bool isSend = false;
                            if (impact_msg.sender_id == impact_msg.target_id)
                            {
                                if (skillInfo.m_LeftEnableImpactsToMyself.Contains(impact_msg.impact_id))
                                {//给自己发impact
                                    skillInfo.m_LeftEnableImpactsToOther.Remove(impact_msg.impact_id);
                                    ImpactSystem.Instance.SendImpactToCharacter(character, impact_msg.impact_id, impact_msg.target_id, impact_msg.skill_id, impact_msg.duration, senderPos, impact_msg.sender_dir);
                                    isSend = true;

                                    //LogSys.Log(LOG_TYPE.WARN, "SendImpactToEntityHandler, send impact {0} to charactor {1}, leftImpacts:{2} skill:{3}", impact_msg.impact_id, impact_msg.target_id, string.Join<int>(",", skillInfo.m_LeftEnableImpactsToMyself), impact_msg.skill_id);
                                }
                                else
                                {
                                    LogSys.Log(LOG_TYPE.ERROR, "SendImpactToEntityHandler, can't send impact {0} to charactor {1}, leftImpacts:{2} skill:{3}", impact_msg.impact_id, impact_msg.target_id, string.Join<int>(",", skillInfo.m_LeftEnableImpactsToMyself), impact_msg.skill_id);
                                }
                            }
                            else
                            {//给别人发impact
                                List<int> leftImpacts;
                                if (!skillInfo.m_LeftEnableImpactsToOther.TryGetValue(impact_msg.target_id, out leftImpacts))
                                {
                                    leftImpacts = new List<int>();
                                    leftImpacts.AddRange(skillInfo.m_EnableImpactsToOther);
                                    if (TimeUtility.GetServerMilliseconds() <= character.GetSkillStateInfo().SimulateEndTime)
                                    {
                                        leftImpacts.AddRange(skillInfo.m_EnableImpactsToOther);
                                    }
                                    skillInfo.m_LeftEnableImpactsToOther.Add(impact_msg.target_id, leftImpacts);
                                }
                                if (null != leftImpacts && leftImpacts.Contains(impact_msg.impact_id))
                                {
                                    leftImpacts.Remove(impact_msg.impact_id);
                                    ImpactSystem.Instance.SendImpactToCharacter(character, impact_msg.impact_id, impact_msg.target_id, impact_msg.skill_id, impact_msg.duration, senderPos, impact_msg.sender_dir);
                                    isSend = true;

                                    //LogSys.Log(LOG_TYPE.WARN, "SendImpactToEntityHandler, send impact {0} to charactor {1}, leftImpacts:{2} skill:{3}", impact_msg.impact_id, impact_msg.target_id, string.Join<int>(",", leftImpacts), impact_msg.skill_id);
                                }
                                else
                                {
                                    LogSys.Log(LOG_TYPE.ERROR, "SendImpactToEntityHandler, can't send impact {0} to charactor {1}, leftImpacts:{2} skill:{3}", impact_msg.impact_id, impact_msg.target_id, string.Join<int>(",", leftImpacts), impact_msg.skill_id);
                                }
                            }
                            if (!isSend)
                            {
                                //todo:记录违规次数
                            }
                        }
                        else
                        {
                            LogSys.Log(LOG_TYPE.WARN, "SendImpactToEntityHandler, can't add impact!!");
                        }
                    }
                    else
                    {
                        if (null == character)
                        {
                            LogSys.Log(LOG_TYPE.ERROR, "SendImpactToEntityHandler, charactor {0} not exist", impact_msg.target_id);
                        }
                        else
                        {
                            LogSys.Log(LOG_TYPE.ERROR, "SendImpactToEntityHandler, charactor {0} or owner {1} not user {2}", character.GetId(), character.OwnerId, user.RoleId);
                        }
                    }
                }
            }
        }
        private static void UpdateCharacterHitCount(CharacterInfo character, int hit_count)
        {
            UserInfo user = character as UserInfo;
            if (user == null)
            {
                return;
            }
            CombatStatisticInfo combat_info = user.GetCombatStatisticInfo();
            combat_info.MultiHitCount = hit_count;
            combat_info.LastHitTime = TimeUtility.GetServerMilliseconds();
        }
    }

    class StopGfxImpactHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_StopGfxImpact impact_msg = msg as Msg_CRC_StopGfxImpact;
            if (null != impact_msg)
            {
                Scene scene = user.OwnRoom.GetActiveScene();
                if (null != scene)
                {
                    CharacterInfo character = scene.SceneContext.GetCharacterInfoById(impact_msg.target_Id);
                    if (null != character && (character.GetId() == user.RoleId || character.OwnerId == user.RoleId))
                    {
                        ImpactSystem.Instance.OnGfxStopImpact(character, impact_msg.impact_Id);
                    }
                    else
                    {
                        if (null == character)
                        {
                            LogSys.Log(LOG_TYPE.ERROR, "StopGfxImpactHandler, charactor {0} not exist", impact_msg.target_Id);
                        }
                        else
                        {
                            LogSys.Log(LOG_TYPE.ERROR, "StopGfxImpactHandler, charactor {0} or owner {1} not user {2}", character.GetId(), character.OwnerId, user.RoleId);
                        }
                    }
                }
            }
        }
    }

    internal class Msg_CRC_GfxControlMoveStartHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_GfxControlMoveStart _msg = msg as Msg_CRC_GfxControlMoveStart;
            if (_msg == null)
                return;
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                CharacterInfo info = scene.SceneContext.GetCharacterInfoById(_msg.obj_id);
                if (null != info && (_msg.obj_id == user.RoleId || info.OwnerId == user.RoleId))
                {
                    bool enableControl = false;
                    if (_msg.is_skill)
                    {
                        SkillInfo skillInfo = info.GetSkillStateInfo().GetSkillInfoById(_msg.skill_or_impact_id);
                        if (null != skillInfo)
                        {
                            enableControl = true;
                        }
                    }
                    else
                    {
                        /*ImpactInfo impactInfo = info.GetSkillStateInfo().GetImpactInfoForCheck(_msg.skill_or_impact_id);
                        if (null != impactInfo) {*/
                        enableControl = true;
                        //}
                    }
                    if (enableControl)
                    {
                        MovementStateInfo msi = info.GetMovementStateInfo();
                        bool isSkillMoving = msi.IsSkillMoving;
                        msi.IsSkillMoving = true;

                        if (_msg.obj_id == user.RoleId)
                        {
                            if (!isSkillMoving)
                            {
                                float x = _msg.cur_pos.x;
                                float z = _msg.cur_pos.z;
                                float velocity = (float)user.Info.GetActualProperty().MoveSpeed;
                                if (!user.VerifyPosition(x, z, velocity, _msg.send_time, 4.0f))
                                {
                                    //todo:记录违规次数
                                }
                                msi.SetPosition2D(x, z);

                                user.SampleMoveData(x, z, velocity, msi.MoveDirCosAngle, msi.MoveDirSinAngle, _msg.send_time);
                            }
                        }

                        //LogSys.Log(LOG_TYPE.WARN, "Msg_CRC_GfxControlMoveStartHandler, charactor {0} skill_or_impact_id {1} isskill {2}", _msg.obj_id, _msg.skill_or_impact_id, _msg.is_skill);
                    }
                    else
                    {

                        LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStartHandler, charactor {0} skill_or_impact_id {1} isskill {2}, skill or impact not found", _msg.obj_id, _msg.skill_or_impact_id, _msg.is_skill);
                    }
                }
                else
                {
                    if (null == info)
                    {
                        LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStartHandler, charactor {0} not exist", _msg.obj_id);
                    }
                    else
                    {
                        LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStartHandler, charactor {0} or owner {1} not user {2}", info.GetId(), info.OwnerId, user.RoleId);
                    }
                }
            }
        }
    }

    internal class Msg_CRC_GfxControlMoveStopHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_GfxControlMoveStop _msg = msg as Msg_CRC_GfxControlMoveStop;
            if (_msg == null)
                return;
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                CharacterInfo info = scene.SceneContext.GetCharacterInfoById(_msg.obj_id);
                if (null != info && (_msg.obj_id == user.RoleId || info.OwnerId == user.RoleId))
                {
                    MovementStateInfo msi = info.GetMovementStateInfo();
                    Vector3 pos;
                    if (_msg.obj_id == user.RoleId)
                    {
                        pos = user.LastClientPosition;
                    }
                    else
                    {
                        pos = msi.GetPosition3D();
                    }
                    Vector3 newPos = new Vector3(_msg.target_pos.x, 0, _msg.target_pos.z);
                    msi.IsSkillMoving = false;

                    bool enableControl = false;
                    if (_msg.is_skill)
                    {
                        SkillInfo skillInfo = info.GetSkillStateInfo().GetSkillInfoById(_msg.skill_or_impact_id);
                        float distance = Geometry.Distance(pos, newPos);
                        if (null != skillInfo && (skillInfo.m_LeftEnableMoveCount > 0 || distance <= 0.3))
                        {
                            //校验
                            --skillInfo.m_LeftEnableMoveCount;

                            if (distance <= skillInfo.m_MaxMoveDistance + 1)
                            {
                                enableControl = true;

                                //LogSys.Log(LOG_TYPE.WARN, "Msg_CRC_GfxControlMoveStopHandler {0} ({1} <= {2}) LeftEnableMoveCount:{3} skill:{4} accept by server ({5}->{6})", _msg.obj_id, distSqr, skillInfo.m_MaxMoveDistanceSqr, skillInfo.m_LeftEnableMoveCount, _msg.skill_or_impact_id, pos.ToString(), newPos.ToString());
                            }
                            else
                            {
                                LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStopHandler {0} ({1} > {2}) LeftEnableMoveCount:{3} skill:{4} can't accept by server ({5}->{6})", _msg.obj_id, distance, skillInfo.m_MaxMoveDistance, skillInfo.m_LeftEnableMoveCount, _msg.skill_or_impact_id, pos.ToString(), newPos.ToString());
                            }
                        }
                        else
                        {
                            LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStopHandler {0} (LeftEnableMoveCount:{1} skill:{2}) can't accept by server ({3}->{4})", _msg.obj_id, skillInfo != null ? skillInfo.m_LeftEnableMoveCount : -1, _msg.skill_or_impact_id, pos.ToString(), newPos.ToString());
                        }
                    }
                    else
                    {
                        ImpactInfo impactInfo = info.GetSkillStateInfo().GetImpactInfoForCheck(_msg.skill_or_impact_id);
                        if (null != impactInfo && impactInfo.m_LeftEnableMoveCount > 0)
                        {
                            //校验
                            --impactInfo.m_LeftEnableMoveCount;

                            float distance = Geometry.Distance(pos, newPos);
                            if (distance <= impactInfo.m_MaxMoveDistance + 1)
                            {

                                enableControl = true;

                                //LogSys.Log(LOG_TYPE.WARN, "Msg_CRC_GfxControlMoveStopHandler {0} ({1} <= {2}) LeftEnableMoveCount:{3} skill:{4} impact:{5} accept by server ({6}->{7})", _msg.obj_id, distSqr, impactInfo.m_MaxMoveDistanceSqr, impactInfo.m_LeftEnableMoveCount, impactInfo.m_SkillId, _msg.skill_or_impact_id, pos.ToString(), newPos.ToString());
                            }
                            else
                            {
                                LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStopHandler {0} ({1} > {2}) LeftEnableMoveCount:{3} skill:{4} impact:{5} can't accept by server ({6}->{7})", _msg.obj_id, distance, impactInfo.m_MaxMoveDistance, impactInfo.m_LeftEnableMoveCount, impactInfo.m_SkillId, _msg.skill_or_impact_id, pos.ToString(), newPos.ToString());
                            }
                        }
                        else
                        {
                            LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStopHandler {0} (LeftEnableMoveCount:{1} skill:{2} impact:{3}) can't accept by server ({4}->{5})", _msg.obj_id, impactInfo != null ? impactInfo.m_LeftEnableMoveCount : -1, impactInfo != null ? impactInfo.m_SkillId : -1, _msg.skill_or_impact_id, pos.ToString(), newPos.ToString());
                        }
                    }
                    if (enableControl)
                    {
                        msi.SetFaceDir(_msg.face_dir);
                        msi.SetPosition(newPos);
                    }
                    else
                    {
                        //todo:记录违规次数
                    }
                    if (_msg.obj_id == user.RoleId)
                    {
                        float velocity = (float)user.Info.GetActualProperty().MoveSpeed;
                        user.SampleMoveData(msi.PositionX, msi.PositionZ, velocity, msi.MoveDirCosAngle, msi.MoveDirSinAngle, _msg.send_time);
                    }
                }
                else
                {
                    if (null == info)
                    {
                        LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStopHandler, charactor {0} not exist", _msg.obj_id);
                    }
                    else
                    {
                        LogSys.Log(LOG_TYPE.ERROR, "Msg_CRC_GfxControlMoveStopHandler, charactor {0} or owner {1} not user {2}", info.GetId(), info.OwnerId, user.RoleId);
                    }
                }
            }
        }
    }

    class SwitchDebugHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_SwitchDebug switchDebug = msg as Msg_CR_SwitchDebug;
            if (switchDebug == null)
                return;
            user.IsDebug = switchDebug.is_debug;
        }
    }

    internal class Msg_CRC_InteractObjectHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CRC_InteractObject ctrlObj = msg as Msg_CRC_InteractObject;
            if (ctrlObj == null)
                return;
            UserInfo userInfo = user.Info;
            if (userInfo == null)
            {
                LogSys.Log(LOG_TYPE.ERROR, "charactor {0} not exist", user.RoleId);
                return;
            }
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                NpcInfo receiver = scene.NpcManager.GetNpcInfo(ctrlObj.receiver_id);
                if (null != receiver)
                {
                }
            }
        }
    }

    internal class Msg_CR_QuitHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_Quit quitClient = msg as Msg_CR_Quit;
            if (quitClient == null)
                return;
            if (null != user.OwnRoom)
            {
                if (quitClient.is_force)
                {
                    user.OwnRoom.DeleteUser(user);
                }
                else
                {
                    user.OwnRoom.DropUser(user);
                }
            }
        }
    }

    internal class Msg_CR_UserMoveToPosHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_UserMoveToPos move_msg = msg as Msg_CR_UserMoveToPos;
            if (move_msg == null)
                return;
            UserInfo charactor = user.Info;
            if (charactor == null)
            {
                LogSys.Log(LOG_TYPE.ERROR, "charactor {0} not exist", user.RoleId);
                return;
            }
            ///
            if (charactor.GetAIEnable())
            {
                MovementStateInfo msi = charactor.GetMovementStateInfo();
                msi.PositionX = move_msg.cur_pos_x;
                msi.PositionZ = move_msg.cur_pos_z;

                UserAiStateInfo asi = charactor.GetAiStateInfo();
                Vector3 pos = new Vector3(move_msg.target_pos_x, 0, move_msg.target_pos_z);
                asi.TargetPos = pos;
                asi.IsTargetPosChanged = true;
                asi.ChangeToState((int)(AiStateId.Move));
            }
        }
    }

    internal class Msg_CR_UserMoveToAttackHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_UserMoveToAttack attack_msg = msg as Msg_CR_UserMoveToAttack;
            if (attack_msg == null)
                return;
            UserInfo charactor = user.Info;
            if (charactor == null)
            {
                LogSys.Log(LOG_TYPE.ERROR, "charactor {0} not exist", user.RoleId);
                return;
            }
            ///
            if (charactor.GetAIEnable())
            {
                MovementStateInfo msi = charactor.GetMovementStateInfo();
                msi.PositionX = attack_msg.cur_pos_x;
                msi.PositionZ = attack_msg.cur_pos_z;

                UserAiStateInfo aiInfo = charactor.GetAiStateInfo();

                AiData_UserSelf_General data = charactor.GetAiStateInfo().AiDatas.GetData<AiData_UserSelf_General>();
                if (null == data)
                {
                    data = new AiData_UserSelf_General();
                    charactor.GetAiStateInfo().AiDatas.AddData(data);
                }
                charactor.GetMovementStateInfo().IsMoving = false;
                data.FoundPath.Clear();
                aiInfo.Time = 0;
                aiInfo.Target = attack_msg.target_id;
                aiInfo.IsAttacked = false;
                aiInfo.AttackRange = attack_msg.attack_range;

                aiInfo.ChangeToState((int)AiStateId.Combat);
            }
        }
    }

    internal class Msg_CR_DlgClosedHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_DlgClosed dialog_msg = msg as Msg_CR_DlgClosed;
            if (dialog_msg == null)
                return;
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                scene.StorySystem.SendMessage("dialogover:" + dialog_msg.dialog_id);
            }
        }
    }

    internal class Msg_CR_GiveUpBattleHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_GiveUpBattle gub_msg = msg as Msg_CR_GiveUpBattle;
            if (null == gub_msg) return;
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                scene.StorySystem.SendMessage("missionfailed");
            }
        }
    }

    internal class Msg_CR_DeleteDeadNpcHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_DeleteDeadNpc ddn_msg = msg as Msg_CR_DeleteDeadNpc;
            if (null == ddn_msg) return;
            Scene scene = user.OwnRoom.GetActiveScene();
            if (null != scene)
            {
                NpcInfo npc = scene.NpcManager.GetNpcInfo(ddn_msg.npc_id);
                if (null != npc && npc.IsDead())
                {
                    npc.GfxDead = true;
                }
            }
        }
    }

    internal class Msg_CR_HitCountChangedHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_HitCountChanged hitCountMsg = msg as Msg_CR_HitCountChanged;
            if (null == hitCountMsg) return;
            UserInfo us = user.Info;
            if (null != us)
            {
                us.GetCombatStatisticInfo().MaxMultiHitCount = hitCountMsg.max_multi_hit_count;
                us.GetCombatStatisticInfo().HitCount = hitCountMsg.hit_count;
            }
        }
    }
    internal class Msg_CR_SyncCharacterGfxStateHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_SyncCharacterGfxState syncGfxStageMsg = msg as Msg_CR_SyncCharacterGfxState;
            if (null != syncGfxStageMsg)
            {
                Scene scene = user.OwnRoom.GetActiveScene();
                if (null != scene)
                {
                    UserInfo userInfo = scene.UserManager.GetUserInfo(syncGfxStageMsg.obj_id);
                    if (null != userInfo)
                    {
                        userInfo.GfxStateFlag = syncGfxStageMsg.gfx_state;
                    }
                    else
                    {
                        NpcInfo npc = scene.NpcManager.GetNpcInfo(syncGfxStageMsg.obj_id);
                        if (null != npc)
                        {
                            npc.GfxStateFlag = syncGfxStageMsg.gfx_state;
                        }
                    }
                }
            }
        }
    }
    internal class Msg_CR_SummonPartnerHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_SummonPartner summonParterMsg = msg as Msg_CR_SummonPartner;
            if (null != summonParterMsg)
            {
                UserInfo userInfo = user.Info;
                if (null != userInfo)
                {
                    Scene scene = user.OwnRoom.GetActiveScene();
                    if (null != scene)
                    {
                        scene.SummonPartner(summonParterMsg);
                    }
                }
            }
        }
    }

    internal class Msg_CR_PickUpNpcHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_PickUpNpc pickUpNpcMsg = msg as Msg_CR_PickUpNpc;
            if (null != pickUpNpcMsg)
            {
                UserInfo userInfo = user.Info;
                if (null != userInfo)
                {
                    Scene scene = user.OwnRoom.GetActiveScene();
                    if (null != scene)
                    {
                        NpcInfo npc = scene.SceneContext.GetCharacterInfoById(pickUpNpcMsg.npc_id) as NpcInfo;
                        if (null != npc && npc.OwnerId == userInfo.GetId())
                        {
                            npc.NeedDelete = true;
                            DropOutInfo info = npc.GetAiStateInfo().AiDatas.GetData<DropOutInfo>();
                            if (null != info)
                            {
                                userInfo.Money += info.Value;
                            }
                        }
                    }
                }
            }
        }
    }
    internal class Msg_CRC_SummonNpcHandler
    {
        internal static void Execute(object msg, User user)
        {
            LogSys.Log(LOG_TYPE.DEBUG, "----got summon npc msg!!");
            Msg_CRC_SummonNpc target_msg = msg as Msg_CRC_SummonNpc;
            if (target_msg == null)
            {
                return;
            }
            UserInfo userInfo = user.Info;
            if (userInfo != null)
            {
                Scene scene = user.OwnRoom.GetActiveScene();
                if (scene != null)
                {
                    scene.OnSummonNpc(target_msg);
                }
            }
        }
    }

    internal class Msg_CR_GmCommandHandler
    {
        internal static void Execute(object msg, User user)
        {
            Msg_CR_GmCommand cmdMsg = msg as Msg_CR_GmCommand;
            if (cmdMsg == null)
            {
                return;
            }
            if (!GlobalVariables.Instance.IsDebug)
                return;
            Scene scene = user.OwnRoom.GetActiveScene();
            if (scene != null)
            {
                scene.GmStorySystem.Reset();
                if (scene.GmStorySystem.GlobalVariables.ContainsKey("UserInfo"))
                {
                    scene.GmStorySystem.GlobalVariables["UserInfo"] = user.Info;
                }
                else
                {
                    scene.GmStorySystem.GlobalVariables.Add("UserInfo", user.Info);
                }
                switch (cmdMsg.type)
                {
                    case 0:
                        //resetdsl
                        StorySystem.StoryConfigManager.Instance.Clear();
                        scene.StorySystem.ClearStoryInstancePool();
                        for (int i = 1; i < 10; ++i)
                        {
                            scene.StorySystem.PreloadStoryInstance(i);
                        }
                        scene.StorySystem.StartStory(1);
                        break;
                    case 1:
                        //script
                        if (null != cmdMsg.content)
                        {
                            scene.GmStorySystem.LoadStory(cmdMsg.content);
                            scene.GmStorySystem.StartStory(1);
                        }
                        break;
                    case 2:
                        //command
#if DEBUG
                        if (null != cmdMsg.content)
                        {
                            scene.GmStorySystem.LoadStoryText("script(1){onmessage(\"start\"){" + cmdMsg.content + "}}");
                            scene.GmStorySystem.StartStory(1);
                        }
#else
            LogSys.Log(LOG_TYPE.ERROR, "GM command can't used in RELEASE !");
#endif
                        break;
                }
            }
        }
    }
}
