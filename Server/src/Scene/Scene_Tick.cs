using System;
using System.Collections.Generic;
using ArkCrossEngineMessage;
using Google.ProtocolBuffers;
using ArkCrossEngineSpatial;
using Lobby_RoomServer;
using ArkCrossEngine;

namespace DashFire
{
    internal sealed partial class Scene
    {
        private void TickPreloading()
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (m_LastPreloadingTickTime + c_PreloadingTickInterval < curTime)
            {
                m_LastPreloadingTickTime = curTime;

                bool canStart = true;
                foreach (User us in m_Room.RoomUsers)
                {
                    if (!us.IsEntered && !us.IsTimeout())
                    {
                        canStart = false;
                    }
                }

                if (canStart)
                {
                    LoadObjects();
                    CalculateDropOut();
                    m_StorySystem.StartStory(1);
                    for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo user = linkNode.Value;
                        UserAttrCalculator.Calc(user);
                        user.LevelChanged = false;
                        user.SetHp(Operate_Type.OT_Absolute, user.GetActualProperty().HpMax);
                        user.SetEnergy(Operate_Type.OT_Absolute, user.GetActualProperty().EnergyMax);
                    }
                    //先让各客户端创建自己与场景相关信息
                    foreach (User us in m_Room.RoomUsers)
                    {
                        if (us.IsEntered)
                        {
                            SyncUserToSelf(us);
                            SyncSceneObjectsToUser(us);
                        }
                    }
                    //再通知其他客户端看见自己
                    foreach (User us in m_Room.RoomUsers)
                    {
                        if (us.IsEntered)
                        {
                            SyncUserToOthers(us);
                        }
                    }
                    //给观察者发初始玩家与场景对象信息
                    foreach (Observer observer in m_Room.RoomObservers)
                    {
                        if (null != observer && !observer.IsIdle && observer.IsEntered)
                        {
                            SyncForNewObserver(observer);
                        }
                    }
                }
            }
        }

        private void TickRunning()
        {
            TimeSnapshot.DoCheckPoint();

            m_ServerDelayActionProcessor.HandleActions(100);
            m_SceneProfiler.DelayActionProcessorTime = TimeSnapshot.DoCheckPoint();

            m_ControlSystemOperation.Tick();
            m_MovementSystem.Tick();
            m_SceneProfiler.MovementSystemTime = TimeSnapshot.DoCheckPoint();

            m_SpatialSystem.Tick();
            m_SceneProfiler.SpatialSystemTime = TimeSnapshot.DoCheckPoint();

            m_AiSystem.Tick();
            m_SceneProfiler.AiSystemTime = TimeSnapshot.DoCheckPoint();

            m_SceneLogicSystem.Tick();
            m_SceneProfiler.SceneLogicSystemTime = TimeSnapshot.DoCheckPoint();

            m_StorySystem.Tick();
            m_GmStorySystem.Tick();
            m_SceneProfiler.StorySystemTime = TimeSnapshot.DoCheckPoint();

            //技能逻辑Tick
            TickSkill();
            m_SceneProfiler.TickSkillTime = TimeSnapshot.DoCheckPoint();

            //obj特殊状态处理（如死亡，重生等）
            TickUsers();
            m_SceneProfiler.TickUsersTime = TimeSnapshot.DoCheckPoint();

            TickNpcs();
            m_SceneProfiler.TickNpcsTime = TimeSnapshot.DoCheckPoint();

            TickLevelup();
            m_SceneProfiler.TickLevelupTime = TimeSnapshot.DoCheckPoint();

            //属性回复
            if (0 == m_LastTickTimeForTickPerSecond)
            {
                m_LastTickTimeForTickPerSecond = TimeUtility.GetServerMilliseconds();
                TickRecover();
                TickBlindage();
            }
            else
            {
                long curTime = TimeUtility.GetServerMilliseconds();
                if (curTime > m_LastTickTimeForTickPerSecond + c_IntervalPerSecond)
                {
                    m_LastTickTimeForTickPerSecond = curTime;
                    TickRecover();
                    TickBlindage();
                }
            }
            m_SceneProfiler.TickAttrRecoverTime = TimeSnapshot.DoCheckPoint();

            //空间信息调试
            TickDebugSpaceInfo();
            m_SceneProfiler.TickDebugSpaceInfoTime = TimeSnapshot.DoCheckPoint();
        }

        private void TickUsers()
        {
            for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo info = linkNode.Value;
                if (info.GetEquipmentStateInfo().EquipmentChanged)
                {
                    RefreshItemSkills(info);
                }
                if (info.LevelChanged || info.GetSkillStateInfo().BuffChanged || info.GetEquipmentStateInfo().EquipmentChanged || info.GetLegacyStateInfo().LegacyChanged)
                {
                    UserAttrCalculator.Calc(info);
                    info.LevelChanged = false;
                    info.GetSkillStateInfo().BuffChanged = false;
                    info.GetEquipmentStateInfo().EquipmentChanged = false;
                    info.GetLegacyStateInfo().LegacyChanged = false;
                }
                if (info.Hp <= 0)
                {
                    if (info.DeadTime <= 0)
                    {
                        //计算击杀收益
                        CalcKillIncome(info);
                        info.GetCombatStatisticInfo().AddDeadCount(1);  //死亡计数+1
                                                                        //解除控制
                        ReleaseControl(info);
                        //发送玩家死亡消息
                        Msg_RC_Dead build = new Msg_RC_Dead();
                        build.role_id = info.GetId();
                        NotifyAllUser(build);
                        PlayerLevelupExpConfig cfg = PlayerConfigProvider.Instance.GetPlayerLevelupExpConfigById(info.GetLevel());
                        info.SetStateFlag(Operate_Type.OT_AddBit, CharacterState_Type.CST_BODY);

                        m_StorySystem.SendMessage("userkilled", info.GetId(), GetLivingUserCount());

                        TryFireAllUserKilled();
                        NoticeAttempRoomClosing();

                        info.DeadTime = TimeUtility.GetServerMilliseconds();
                        if (null != cfg && m_IsPvpScene)
                        {
                            info.ReviveTime = TimeUtility.GetServerMilliseconds() + cfg.m_RebornTime * 1000;
                        }
                        else
                        {
                            info.ReviveTime = TimeUtility.GetServerMilliseconds() + info.ReleaseTime + 2000;
                        }
                        NpcInfo npc = NpcManager.GetNpcInfo(info.PartnerId);
                        if (null != npc && npc.NpcType == (int)NpcTypeEnum.Partner)
                        {
                            npc.NeedDelete = true;
                        }
                    }
                    else
                    {
                        /*
                        long delta = TimeUtility.GetServerMilliseconds() - info.DeadTime;
                        if (delta > info.ReleaseTime) {
                          info.DeadTime = info.ReviveTime;
                          Msg_RC_Disappear build = new Msg_RC_Disappear();
                          build.role_id = info.GetId();
                          NotifyAllUser(build);
                        }
                        */
                    }
                }
            }
        }

        private void TickNpcs()
        {
            List<NpcInfo> deletes = new List<NpcInfo>();
            List<NpcInfo> deletes2 = new List<NpcInfo>();
            Msg_RC_NpcDead npcDeadBuilder = new Msg_RC_NpcDead();
            for (LinkedListNode<NpcInfo> linkNode = NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (info.LevelChanged || info.GetSkillStateInfo().BuffChanged || info.GetEquipmentStateInfo().EquipmentChanged || info.GetLegacyStateInfo().LegacyChanged)
                {
                    NpcAttrCalculator.Calc(info);
                    info.LevelChanged = false;
                    info.GetSkillStateInfo().BuffChanged = false;
                    info.GetEquipmentStateInfo().EquipmentChanged = false;
                    info.GetLegacyStateInfo().LegacyChanged = false;
                }
                // 伙伴自动掉血
                if ((int)NpcTypeEnum.Partner == info.NpcType)
                {
                    UserInfo owner = UserManager.GetUserInfo(info.OwnerId);
                    if (null != owner)
                    {
                        PartnerInfo pi = owner.GetPartnerInfo();
                        if (null != pi && TimeUtility.GetServerMilliseconds() - pi.LastTickTime > pi.TickInterval)
                        {
                            info.SetHp(Operate_Type.OT_Relative, (int)pi.GetHpCostPerTick(info.GetActualProperty().HpMax));
                            pi.LastTickTime = TimeUtility.GetServerMilliseconds();
                        }
                    }
                }
                if (info.NeedDelete)
                {
                    deletes2.Add(info);
                }
                else if (info.IsDead())
                {
                    if (info.DeadTime <= 0)
                    {
                        info.DeadTime = TimeUtility.GetServerMilliseconds();
                        //击杀收益计算
                        CalcKillIncome(info);
                        //解除控制
                        ReleaseControl(info);
                        //发送npc死亡消息
                        npcDeadBuilder.npc_id = info.GetId();
                        NotifyAllUser(npcDeadBuilder);

                        if (info.IsCombatNpc())
                        {
                            m_StorySystem.SendMessage("objkilled", info.GetId(), GetBattleNpcCount());
                            m_StorySystem.SendMessage(string.Format("npckilled:{0}", info.GetUnitId()), info.GetId(), GetBattleNpcCount());
                            if (info.GetUnitId() > 0)
                            {
                                if (m_IsAttemptScene)
                                {
                                    if ((int)NpcTypeEnum.BigBoss == info.NpcType)
                                    {
                                        TryFireAllNpcKilled();
                                    }
                                }
                                else
                                {
                                    TryFireAllNpcKilled();
                                }
                            }
                        }

                    }
                    else if (TimeUtility.GetServerMilliseconds() - info.DeadTime > info.ReleaseTime && info.GfxDead)
                    {
                        deletes.Add(info);
                    }
                }
                if (info.IsBorning && IsNpcBornOver(info))
                {
                    info.IsBorning = false;
                    info.SetAIEnable(true);
                    info.SetStateFlag(Operate_Type.OT_RemoveBit, CharacterState_Type.CST_Invincible);
                }
                CheckNpcOwnerId(info);
            }
            if (deletes.Count > 0)
            {
                Msg_RC_DestroyNpc destroyNpcBuilder = new Msg_RC_DestroyNpc();
                foreach (NpcInfo ni in deletes)
                {
                    //发送npc消失消息
                    destroyNpcBuilder.npc_id = ni.GetId();
                    destroyNpcBuilder.need_play_effect = true;
                    NotifyAllUser(destroyNpcBuilder);
                    //删除npc
                    NpcManager.RemoveNpc(ni.GetId());
                    LogSystem.Debug("Npc {0}  name {1} is deleted.", ni.GetId(), ni.GetName());
                }
            }
            if (deletes2.Count > 0)
            {
                Msg_RC_DestroyNpc destroyNpcBuilder = new Msg_RC_DestroyNpc();
                foreach (NpcInfo ni in deletes2)
                {
                    //发送npc消失消息
                    destroyNpcBuilder.npc_id = ni.GetId();
                    destroyNpcBuilder.need_play_effect = false;
                    NotifyAllUser(destroyNpcBuilder);
                    //删除npc
                    NpcManager.RemoveNpc(ni.GetId());
                    LogSystem.Debug("Npc {0}  name {1} is deleted.", ni.GetId(), ni.GetName());
                }
            }
            NpcManager.ExecuteDelayAdd();
        }

        private void CheckNpcOwnerId(NpcInfo info)
        {
            if (null == info)
                return;
            if (info.OwnerId > 0)
            {
                Room room = GetRoom();
                if (null != room && null != room.GetActiveScene())
                {
                    int new_owner_id = 0;
                    bool is_online = false;
                    foreach (User v in room.RoomUsers)
                    {
                        if (null != v.Info && v.UserControlState != (int)UserControlState.UserDropped && v.UserControlState != (int)UserControlState.Remove)
                        {
                            new_owner_id = v.Info.GetId();
                            if (new_owner_id == info.OwnerId)
                            {
                                is_online = true;
                                break;
                            }
                        }
                    }
                    if (!is_online)
                    {
                        info.OwnerId = new_owner_id;
                        Msg_RC_SyncNpcOwnerId builder = DataSyncUtility.BuildSyncNpcOwnerIdMessage(info);
                        NotifyAreaUser(info, builder, false);
                    }
                }
            }
        }
        private void NoticeAttempRoomClosing()
        {
            if (!IsAttemptScene)
                return;
            Room room = GetRoom();
            if (null == room)
                return;
            Scene scene = room.GetActiveScene();
            if (null == scene)
                return;
            int ct = GetLivingUserCount();
            if (0 == ct)
            {
                scene.DelayActionProcessor.QueueAction(room.NoticeRoomClosing);
            }
        }
        private void TryFireAllNpcKilled()
        {
            int ct = GetBattleNpcCount();
            if (0 == ct)
            {
                m_StorySystem.SendMessage("allnpckilled");
            }
        }

        private void TryFireAllUserKilled()
        {
            int ct = GetLivingUserCount();
            if (0 == ct)
            {
                m_StorySystem.SendMessage("alluserkilled");
            }
        }
        private bool IsNpcBornOver(NpcInfo npc)
        {
            if (npc == null)
            {
                return false;
            }
            long cur_time = TimeUtility.GetServerMilliseconds();
            long born_anim_time = npc.BornAnimTimeMs;
            if ((npc.BornTime + born_anim_time) > cur_time)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void TickSkill()
        {
            for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo userInfo = linkNode.Value;
                if (userInfo != null)
                {
                    ImpactSystem.Instance.Tick(userInfo);
                }
            }
            for (LinkedListNode<NpcInfo> linkNode = NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (info != null)
                {
                    ImpactSystem.Instance.Tick(info);
                }
            }
            m_SkillSystem.Tick();
        }

        private void TickLevelup()
        {
            for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo info = linkNode.Value;
                if (!info.IsDead())
                {
                    if (info.GetCombatStatisticInfo().DataChanged)
                    {
                        Msg_RC_SyncCombatStatisticInfo combatBuilder = DataSyncUtility.BuildSyncCombatStatisticInfo(info);
                        NotifyAllUser(combatBuilder);
                        info.GetCombatStatisticInfo().DataChanged = false;
                    }
                }
            }
        }

        private void TickDebugSpaceInfo()
        {
            if (GlobalVariables.Instance.IsDebug)
            {
                bool needDebug = false;
                foreach (User user in m_Room.RoomUsers)
                {
                    if (user.IsDebug)
                    {
                        needDebug = true;
                        break;
                    }
                }
                if (needDebug)
                {
                    Msg_RC_DebugSpaceInfo builder = new Msg_RC_DebugSpaceInfo();
                    for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo info = linkNode.Value;
                        Msg_RC_DebugSpaceInfo.DebugSpaceInfo infoBuilder = new Msg_RC_DebugSpaceInfo.DebugSpaceInfo();
                        infoBuilder.obj_id = info.GetId();
                        infoBuilder.is_player = true;
                        infoBuilder.pos_x = (float)info.GetMovementStateInfo().GetPosition3D().X;
                        infoBuilder.pos_z = (float)info.GetMovementStateInfo().GetPosition3D().Z;
                        infoBuilder.face_dir = (float)info.GetMovementStateInfo().GetFaceDir();
                        builder.space_infos.Add(infoBuilder);
                    }
                    for (LinkedListNode<NpcInfo> linkNode = NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        NpcInfo info = linkNode.Value;
                        Msg_RC_DebugSpaceInfo.DebugSpaceInfo infoBuilder = new Msg_RC_DebugSpaceInfo.DebugSpaceInfo();
                        infoBuilder.obj_id = info.GetId();
                        infoBuilder.is_player = false;
                        infoBuilder.pos_x = (float)info.GetMovementStateInfo().GetPosition3D().X;
                        infoBuilder.pos_z = (float)info.GetMovementStateInfo().GetPosition3D().Z;
                        infoBuilder.face_dir = (float)info.GetMovementStateInfo().GetFaceDir();
                        builder.space_infos.Add(infoBuilder);
                    }
                    foreach (User user in m_Room.RoomUsers)
                    {
                        if (user.IsDebug)
                        {
                            user.SendMessage(builder);
                        }
                    }
                }
            }
        }

        private void TickRecover()
        {
            float hp_coefficient = 1.0f;
            float mp_coefficient = 1.0f;
            Data_SceneConfig scene_data = SceneConfigProvider.Instance.GetSceneConfigById(m_SceneResId);
            if (null != scene_data)
            {
                hp_coefficient = scene_data.m_RecoverHpCoefficient;
                mp_coefficient = scene_data.m_RecoverMpCoefficient;
            }
            for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo info = linkNode.Value;
                if (!info.IsDead())
                {
                    float hpRecover = info.GetActualProperty().HpRecover * hp_coefficient;
                    float epRecover = info.GetActualProperty().EnergyRecover * mp_coefficient;
                    if (hpRecover > 0.0001)
                    {
                        if (info.Hp + (int)hpRecover >= info.GetActualProperty().HpMax)
                            info.SetHp(Operate_Type.OT_Absolute, (int)info.GetActualProperty().HpMax);
                        else
                            info.SetHp(Operate_Type.OT_Relative, (int)hpRecover);
                    }
                    if (epRecover > 0.0001)
                    {
                        if (info.Energy + (int)epRecover >= info.GetActualProperty().EnergyMax)
                            info.SetEnergy(Operate_Type.OT_Absolute, (int)info.GetActualProperty().EnergyMax);
                        else
                            info.SetEnergy(Operate_Type.OT_Relative, (int)epRecover);
                    }
                    if (hpRecover > 0.0001 || epRecover > 0.0001)
                    {
                        Msg_RC_SyncProperty builder = DataSyncUtility.BuildSyncPropertyMessage(info);
                        NotifyAreaUser(info, builder, false);
                    }
                }
            }
            for (LinkedListNode<NpcInfo> linkNode = NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (!info.IsDead())
                {
                    float hpRecover = info.GetActualProperty().HpRecover;
                    float npRecover = info.GetActualProperty().EnergyRecover;
                    if (hpRecover > 0.0001)
                    {
                        if (info.Hp + (int)hpRecover >= info.GetActualProperty().HpMax)
                            info.SetHp(Operate_Type.OT_Absolute, (int)info.GetActualProperty().HpMax);
                        else
                            info.SetHp(Operate_Type.OT_Relative, (int)hpRecover);
                    }
                    if (npRecover > 0.0001)
                    {
                        if (info.Energy + (int)npRecover >= info.GetActualProperty().EnergyMax)
                            info.SetEnergy(Operate_Type.OT_Absolute, (int)info.GetActualProperty().EnergyMax);
                        else
                            info.SetEnergy(Operate_Type.OT_Relative, (int)npRecover);
                    }
                    if (hpRecover > 0.0001 || npRecover > 0.0001)
                    {
                        Msg_RC_SyncProperty builder = DataSyncUtility.BuildSyncPropertyMessage(info);
                        NotifyAreaUser(info, builder, false);
                    }
                }
            }
        }

        private void TickBlindage()
        {
            for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo info = linkNode.Value;
                if (null != info.Blindage)
                    TickBlindage(info);
            }
            for (LinkedListNode<NpcInfo> linkNode = NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (null != info.Blindage)
                    TickBlindage(info);
            }
        }

        private void TickBlindage(CharacterInfo obj)
        {
            if (obj.BlindageLeftTime > 1000)
                obj.BlindageLeftTime -= 1000;
            else
            {
                obj.BlindageLeftTime = 0;
                obj.Blindage = null;
                obj.BlindageId = 0;
                LogSystem.Debug("obj {0} leave blindage", obj.GetId());
            }
        }
    }
}
