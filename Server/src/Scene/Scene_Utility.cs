using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArkCrossEngineMessage;
using Google.ProtocolBuffers;
using ArkCrossEngineSpatial;
using ArkCrossEngine;

namespace DashFire
{
    internal sealed partial class Scene
    {
        private void LoadObjects()
        {
            m_SceneState = SceneState.Running;
            LogSys.Log(LOG_TYPE.DEBUG, "Scene {0} start Running.", m_SceneResId);
            // TODO: m_GameStartTime should be set when game of this scene 
            // is really started
            m_GameTime.Start();
            m_SceneContext.StartTime = m_GameTime.StartTime;
            Room room = GetRoom();
            if (null != room && null != m_MapData)
            {
                MyDictionary<int, object> units = m_MapData.m_UnitMgr.GetData();
                foreach (Data_Unit unit in units.Values)
                {
                    if (null != unit && unit.m_IsEnable)
                    {
                        NpcInfo npc = m_NpcMgr.AddNpc(unit);
                        if (null != npc)
                        {
                            if (room.RoomUsers.Count() > 0)
                            {
                                npc.OwnerId = room.RoomUsers[0].RoleId;
                                LogSystem.Debug("User {0} is responsible for npc {1}", npc.OwnerId, npc.GetId());
                            }
                            else
                            {
                                LogSystem.Warn("No User is responsible for npc");
                            }
                        }
                    }
                }

                MyDictionary<int, object> slogics = m_MapData.m_SceneLogicMgr.GetData();
                foreach (SceneLogicConfig sc in slogics.Values)
                {
                    if (null != sc && sc.m_IsServer)
                    {
                        m_SceneLogicInfoMgr.AddSceneLogicInfo(sc.GetId(), sc);
                    }
                }

                foreach (User us in room.RoomUsers)
                {
                    UserInfo info = us.Info;
                    Data_Unit unit = m_MapData.ExtractData(DataMap_Type.DT_Unit, info.GetUnitId()) as Data_Unit;
                    if (null != unit)
                    {
                        info.GetMovementStateInfo().SetPosition(unit.m_Pos);
                        info.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
                        info.RevivePoint = unit.m_Pos;
                    }
                }
            }
        }

        private void SyncUserToUserHelper(User infoUser, User user, bool isSelf)
        {
            Room room = GetRoom();
            if (null != infoUser && null != user && null != room && null != room.GetActiveScene())
            {
                UserInfo userInfo = infoUser.Info;
                if (null != userInfo)
                {
                    Vector3 pos = userInfo.GetMovementStateInfo().GetPosition3D();
                    ArkCrossEngineMessage.Position pos_bd0 = new ArkCrossEngineMessage.Position();
                    pos_bd0.x = pos.X;
                    pos_bd0.z = pos.Z;
                    Msg_CRC_Create bd0 = new Msg_CRC_Create();
                    bd0.role_id = infoUser.RoleId;
                    bd0.hero_id = infoUser.HeroId;
                    bd0.camp_id = infoUser.CampId;
                    bd0.role_level = infoUser.Level;
                    bd0.is_player_self = isSelf;
                    bd0.position = pos_bd0;
                    bd0.face_dirction = (float)userInfo.GetMovementStateInfo().GetFaceDir();
                    for (int index = 0; index < userInfo.GetSkillStateInfo().GetAllSkill().Count; index++)
                    {
                        bd0.skill_levels.Add(userInfo.GetSkillStateInfo().GetSkillInfoByIndex(index).SkillLevel);
                    }
                    bd0.scene_start_time = StartTime;
                    bd0.nickname = infoUser.Name;
                    user.SendMessage(bd0);
                    ///
                    if (infoUser.PresetIndex >= 0)
                    {
                        Msg_RC_UpdateUserBattleInfo uusMsg = new Msg_RC_UpdateUserBattleInfo();
                        uusMsg.role_id = infoUser.Info.GetId();
                        uusMsg.preset_index = infoUser.PresetIndex;
                        for (int i = 0; i < infoUser.Skill.Count; i++)
                        {
                            Msg_RC_UpdateUserBattleInfo.PresetInfo preset_info = new Msg_RC_UpdateUserBattleInfo.PresetInfo();
                            preset_info.skill_id = infoUser.Skill[i].SkillId;
                            preset_info.skill_level = infoUser.Skill[i].SkillLevel;
                            uusMsg.skill_info.Add(preset_info);
                        }
                        for (int i = 0; i < infoUser.Equip.Count; i++)
                        {
                            Msg_RC_UpdateUserBattleInfo.EquipInfo equip_info = new Msg_RC_UpdateUserBattleInfo.EquipInfo();
                            equip_info.equip_id = infoUser.Equip[i].ItemId;
                            equip_info.equip_level = infoUser.Equip[i].ItemLevel;
                            equip_info.equip_random_property = infoUser.Equip[i].ItemRandomProperty;
                            uusMsg.equip_info.Add(equip_info);
                        }
                        for (int i = 0; i < infoUser.Legacy.Count; i++)
                        {
                            Msg_RC_UpdateUserBattleInfo.LegacyInfo legacy_info = new Msg_RC_UpdateUserBattleInfo.LegacyInfo();
                            legacy_info.legacy_id = infoUser.Legacy[i].ItemId;
                            legacy_info.legacy_level = infoUser.Legacy[i].ItemLevel;
                            legacy_info.legacy_random_property = infoUser.Legacy[i].ItemRandomProperty;
                            legacy_info.legacy_IsUnlock = infoUser.Legacy[i].IsUnlock;
                            uusMsg.legacy_info.Add(legacy_info);
                        }
                        foreach (XSoulPartInfo part in infoUser.XSouls.GetAllXSoulPartData().Values)
                        {
                            Msg_RC_UpdateUserBattleInfo.XSoulDataInfo xsoul_info = new Msg_RC_UpdateUserBattleInfo.XSoulDataInfo();
                            xsoul_info.ItemId = part.XSoulPartItem.ItemId;
                            xsoul_info.Level = part.XSoulPartItem.Level;
                            xsoul_info.Experience = part.XSoulPartItem.Experience;
                            xsoul_info.ModelLevel = part.ShowModelLevel;
                            uusMsg.XSouls.Add(xsoul_info);
                        }
                        if (null != infoUser.Partner)
                        {
                            Msg_RC_UpdateUserBattleInfo.PartnerDataInfo partner_info = new Msg_RC_UpdateUserBattleInfo.PartnerDataInfo();
                            partner_info.PartnerId = infoUser.Partner.Id;
                            partner_info.PartnerLevel = infoUser.Partner.CurAdditionLevel;
                            partner_info.PartnerStage = infoUser.Partner.CurSkillStage;
                            uusMsg.Partners.Add(partner_info);
                        }
                        user.SendMessage(uusMsg);
                    }
                    ///
                    DataSyncUtility.SyncBuffListToUser(userInfo, user);

                    Msg_RC_SyncProperty propBuilder = DataSyncUtility.BuildSyncPropertyMessage(userInfo);
                    user.SendMessage(propBuilder);

                    Msg_RC_SyncCombatStatisticInfo combatBuilder = DataSyncUtility.BuildSyncCombatStatisticInfo(userInfo);
                    user.SendMessage(combatBuilder);

                    LogSys.Log(LOG_TYPE.DEBUG, "send user {0} msg to user {1}", infoUser.RoleId, user.RoleId);
                }
            }
        }

        private void SyncUserToObservers(User infoUser)
        {
            Room room = GetRoom();
            if (null != infoUser && null != room && null != room.GetActiveScene())
            {
                UserInfo userInfo = infoUser.Info;
                if (null != userInfo)
                {
                    Vector3 pos = userInfo.GetMovementStateInfo().GetPosition3D();

                    ArkCrossEngineMessage.Position pos_bd0 = new ArkCrossEngineMessage.Position();
                    pos_bd0.x = pos.X;
                    pos_bd0.z = pos.Z;
                    Msg_CRC_Create bd0 = new Msg_CRC_Create();
                    bd0.role_id = infoUser.RoleId;
                    bd0.hero_id = infoUser.HeroId;
                    bd0.camp_id = infoUser.CampId;
                    bd0.role_level = infoUser.Level;
                    bd0.is_player_self = false;
                    bd0.position = pos_bd0;
                    bd0.face_dirction = (float)userInfo.GetMovementStateInfo().GetFaceDir();
                    for (int index = 0; index < userInfo.GetSkillStateInfo().GetAllSkill().Count; index++)
                    {
                        bd0.skill_levels.Add(userInfo.GetSkillStateInfo().GetSkillInfoByIndex(index).SkillLevel);
                    }
                    bd0.scene_start_time = StartTime;
                    bd0.nickname = infoUser.Name;
                    NotifyAllObserver(bd0);

                    DataSyncUtility.SyncBuffListToObservers(userInfo, this);

                    Msg_RC_SyncProperty propBuilder = DataSyncUtility.BuildSyncPropertyMessage(userInfo);
                    NotifyAllObserver(propBuilder);

                    Msg_RC_SyncCombatStatisticInfo combatBuilder = DataSyncUtility.BuildSyncCombatStatisticInfo(userInfo);
                    NotifyAllObserver(combatBuilder);

                    LogSys.Log(LOG_TYPE.DEBUG, "send user {0} msg to observers", infoUser.RoleId);
                }
            }
        }

        private void SyncUserToSelf(User user)
        {
            if (null != user)
            {
                SyncUserToUserHelper(user, user, true);
            }
        }

        private void SyncUserToOthers(User user)
        {
            Room room = GetRoom();
            if (null != user && null != room)
            {
                foreach (User other in room.RoomUsers)
                {
                    if (other != user)
                    {
                        if (!other.IsEntered)
                        {
                            continue;
                        }
                        SyncUserToUserHelper(user, other, false);
                    }
                }
            }
        }

        private void SyncOthersToUser(User user)
        {
            Room room = GetRoom();
            if (null != user && null != room)
            {
                foreach (User other in room.RoomUsers)
                {
                    if (other != user)
                    {
                        if (!other.IsEntered)
                        {
                            continue;
                        }
                        SyncUserToUserHelper(other, user, false);
                    }
                }
            }
        }

        private void SyncSceneObjectsToUser(User user)
        {
            if (null != user)
            {
                UserInfo userInfo = user.Info;
                Room room = GetRoom();
                if (null != userInfo && null != room && null != room.GetActiveScene())
                {
                    for (LinkedListNode<NpcInfo> linkNode = NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        NpcInfo npc = linkNode.Value;
                        if (null != npc)
                        {
                            Msg_RC_CreateNpc bder = DataSyncUtility.BuildCreateNpcMessage(npc);
                            if (npc.AppendAttrId > 0)
                            {
                                bder.add_attr_id = npc.AppendAttrId;
                            }
                            user.SendMessage(bder);
                        }
                    }

                    int totalKillCountForBlue = 0;
                    int totalKillCountForRed = 0;
                    for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo user_info = linkNode.Value;
                        if (user_info.GetCampId() == (int)CampIdEnum.Blue)
                        {
                            totalKillCountForBlue += user_info.GetCombatStatisticInfo().KillHeroCount;
                        }
                        else
                        {
                            totalKillCountForRed += user_info.GetCombatStatisticInfo().KillHeroCount;
                        }
                    }

                    Msg_RC_PvpCombatInfo combat_bd = new Msg_RC_PvpCombatInfo();
                    combat_bd.kill_hero_count_for_blue = totalKillCountForBlue;
                    combat_bd.kill_hero_count_for_red = totalKillCountForRed;
                    combat_bd.link_id_for_killer = -1;
                    combat_bd.link_id_for_killed = -1;
                    combat_bd.killed_nickname = "";
                    combat_bd.killer_nickname = "";
                    user.SendMessage(combat_bd);
                }
            }
        }

        private void RefreshItemSkills(UserInfo user)
        {
            user.RefreshItemSkills((int id) =>
            {
                return user.GetSkillStateInfo().GetImpactInfoById(id);
            }, (int id) =>
            {
            });
            Msg_RC_RefreshItemSkills builder = new Msg_RC_RefreshItemSkills();
            builder.role_id = user.GetId();
            NotifyAllUser(builder);
        }

        private void CalcKillIncome(UserInfo user)
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (m_IsPvpScene)
            {
                int lvl2 = user.GetLevel();
                int lvl1 = lvl2 - 1;
                if (lvl1 < 0) lvl1 = 0;
                PlayerLevelupExpConfig cfg1 = PlayerConfigProvider.Instance.GetPlayerLevelupExpConfigById(lvl1);
                PlayerLevelupExpConfig cfg2 = PlayerConfigProvider.Instance.GetPlayerLevelupExpConfigById(lvl2);
                if (null != cfg1 && null != cfg2)
                {
                    int exp = (cfg2.m_ConsumeExp - cfg1.m_ConsumeExp) * 70 / 100;
                    UserInfo killer = UserManager.GetUserInfo(user.KillerId);
                    if (null != killer)
                    {
                        //被英雄击杀连杀数清0
                        user.GetCombatStatisticInfo().ClearContinueKillCount();
                        //击杀英雄连死数清0
                        killer.GetCombatStatisticInfo().ClearContinueDeadCount();

                        user.GetCombatStatisticInfo().AddContinueDeadCount(1);
                        killer.GetCombatStatisticInfo().AddContinueKillCount(1);

                        killer.GetCombatStatisticInfo().AddKillHeroCount(1);
                        if (killer.GetCombatStatisticInfo().LastKillHeroTime + 10000 < curTime)
                        {
                            killer.GetCombatStatisticInfo().ClearMultiKillCount();
                        }
                        killer.GetCombatStatisticInfo().AddMultiKillCount(1);
                        killer.GetCombatStatisticInfo().LastKillHeroTime = curTime;

                        User us = killer.CustomData as User;
                        User killedUs = user.CustomData as User;
                        if (us != null && killedUs != null)
                            CalcPvpCombatInfo(killer.GetLinkId(), user.GetLinkId(), us.Name, killedUs.Name);
                        if (user.AttackerInfos.Count > 0)
                        {
                            foreach (int id in user.AttackerInfos.Keys)
                            {
                                UserInfo assit = UserManager.GetUserInfo(id);
                                CharacterInfo.AttackerInfo attackInfo = user.AttackerInfos[id];
                                if (null != assit && killer != assit && attackInfo.m_AttackTime + 5000 >= curTime)
                                {
                                    assit.GetCombatStatisticInfo().AddAssitKillCount(1);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void UpdateKillCount(NpcInfo npc)
        {
            if (null == npc)
                return;
            UserInfo killer = null;
            UserInfo user = UserManager.GetUserInfo(npc.KillerId);
            if (null != user)
            {
                killer = user;
            }
            else
            {
                NpcInfo parter = NpcManager.GetNpcInfo(npc.KillerId);
                if (null != parter && parter.OwnerId > 0)
                {
                    killer = UserManager.GetUserInfo(parter.OwnerId);
                }
            }
            if (null != killer)
            {
                if (m_IsAttemptScene)
                {
                    if ((int)NpcTypeEnum.BigBoss == npc.NpcType)
                    {
                        killer.GetCombatStatisticInfo().AddKillNpcCount(1);
                    }
                }
                else
                {
                    killer.GetCombatStatisticInfo().AddKillNpcCount(1);
                }
            }
        }
        private void CalcKillIncome(NpcInfo npc)
        {
            if (null == npc)
                return;
            UpdateKillCount(npc);
            //死亡掉落
            UserInfo userKiller = UserManager.GetUserInfo(npc.KillerId);
            if (null != userKiller && m_DropMoneyData.ContainsKey(npc.GetUnitId()) && m_DropMoneyData[npc.GetUnitId()] > 0)
            {
                DelayActionProcessor.QueueAction(DropNpc,
                                                 0,
                                                 npc.GetId(),
                                                 DropOutType.GOLD,
                                                 m_SceneDropOut.m_GoldModel,
                                                 m_SceneDropOut.m_GoldParticle,
                                                 m_DropMoneyData[npc.GetUnitId()]);
            }
        }

        private void DropNpc(Vector3 pos, int money, string model, string particle)
        {
            //给每个玩家掉落一个
            Data_Unit unit = new Data_Unit();
            unit.m_Id = -1;
            unit.m_LinkId = 100001;
            unit.m_AiLogic = (int)AiStateLogicId.DropOut_AutoPick;
            unit.m_RotAngle = 0;
            for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo userInfo = linkNode.Value;
                if (null != userInfo)
                {

                    float x = CrossEngineHelper.Random.NextFloat() * 4 - 2;
                    float z = CrossEngineHelper.Random.NextFloat() * 4 - 2;
                    pos.X += x;
                    pos.Z += z;

                    NpcInfo npcInfo = NpcManager.AddNpc(unit);
                    npcInfo.GetMovementStateInfo().SetPosition(pos);
                    npcInfo.GetMovementStateInfo().SetFaceDir(0);
                    npcInfo.GetMovementStateInfo().IsMoving = false;
                    npcInfo.SetAIEnable(true);
                    npcInfo.SetCampId((int)CampIdEnum.Friendly);
                    npcInfo.OwnerId = userInfo.GetId();
                    npcInfo.DropMoney = money;

                    DropOutInfo dropInfo = new DropOutInfo();
                    dropInfo.DropType = DropOutType.GOLD;
                    dropInfo.Value = money;
                    dropInfo.Model = model;
                    dropInfo.Particle = particle;
                    npcInfo.GetAiStateInfo().AiDatas.AddData<DropOutInfo>(dropInfo);
                    npcInfo.SetModel(dropInfo.Model);

                    User us = userInfo.CustomData as User;
                    if (null != us)
                    {
                        Msg_RC_CreateNpc builder = DataSyncUtility.BuildCreateNpcMessage(npcInfo);
                        us.SendMessage(builder);
                    }
                }
            }
        }

        internal void DropNpc(int ownerId, int fromNpcId, DropOutType dropType, string model, string particle, int num)
        {
            if (ownerId > 0)
            {
                UserInfo user = UserManager.GetUserInfo(ownerId);
                if (null == user)
                {
                    NpcInfo npc = NpcManager.GetNpcInfo(ownerId);
                    while (null != npc)
                    {
                        user = UserManager.GetUserInfo(npc.OwnerId);
                        if (null != user)
                        {
                            break;
                        }
                        else
                        {
                            npc = NpcManager.GetNpcInfo(npc.OwnerId);
                        }
                    }
                }
                if (null != user)
                {
                    Data_Unit unit = new Data_Unit();
                    unit.m_Id = -1;
                    switch (dropType)
                    {
                        case DropOutType.GOLD:
                            unit.m_LinkId = (int)DropNpcTypeEnum.GOLD;
                            break;
                        case DropOutType.HP:
                            unit.m_LinkId = (int)DropNpcTypeEnum.HP;
                            break;
                        case DropOutType.MP:
                            unit.m_LinkId = (int)DropNpcTypeEnum.MP;
                            break;
                        case DropOutType.MULT_GOLD:
                            unit.m_LinkId = (int)DropNpcTypeEnum.MUTI_GOLD;
                            break;
                    }
                    unit.m_RotAngle = 0;

                    NpcInfo npcInfo = NpcManager.AddNpc(unit);
                    npcInfo.GetMovementStateInfo().SetFaceDir(0);
                    npcInfo.GetMovementStateInfo().IsMoving = false;
                    npcInfo.SetAIEnable(true);
                    npcInfo.SetCampId(user.GetCampId());
                    npcInfo.OwnerId = user.GetId();

                    DropOutInfo dropInfo = new DropOutInfo();
                    dropInfo.DropType = dropType;
                    dropInfo.Value = num;
                    dropInfo.Model = model;
                    dropInfo.Particle = particle;

                    npcInfo.GetAiStateInfo().AiDatas.AddData<DropOutInfo>(dropInfo);
                    npcInfo.SetModel(dropInfo.Model);

                    User us = user.CustomData as User;
                    if (null != us)
                    {
                        Msg_RC_DropNpc builder = DataSyncUtility.BuildDropNpcMessage(npcInfo, fromNpcId, (int)dropType, num, model);
                        us.SendMessage(builder);
                    }
                }
            }
            else
            {
                int ct = UserManager.Users.Count;
                int rd = CrossEngineHelper.Random.Next(0, ct);
                UserInfo user = null;
                if (UserManager.Users.TryGetValue(rd, out user))
                {
                    Data_Unit unit = new Data_Unit();
                    unit.m_Id = -1;
                    switch (dropType)
                    {
                        case DropOutType.GOLD:
                            unit.m_LinkId = (int)DropNpcTypeEnum.GOLD;
                            break;
                        case DropOutType.HP:
                            unit.m_LinkId = (int)DropNpcTypeEnum.HP;
                            break;
                        case DropOutType.MP:
                            unit.m_LinkId = (int)DropNpcTypeEnum.MP;
                            break;
                        case DropOutType.MULT_GOLD:
                            unit.m_LinkId = (int)DropNpcTypeEnum.MUTI_GOLD;
                            break;
                    }
                    unit.m_RotAngle = 0;

                    NpcInfo npcInfo = NpcManager.AddNpc(unit);
                    npcInfo.GetMovementStateInfo().SetFaceDir(0);
                    npcInfo.GetMovementStateInfo().IsMoving = false;
                    npcInfo.SetAIEnable(true);
                    npcInfo.SetCampId(user.GetCampId());
                    npcInfo.OwnerId = user.GetId();

                    DropOutInfo dropInfo = new DropOutInfo();
                    dropInfo.DropType = dropType;
                    dropInfo.Value = num;
                    dropInfo.Model = model;
                    dropInfo.Particle = particle;

                    npcInfo.GetAiStateInfo().AiDatas.AddData<DropOutInfo>(dropInfo);
                    npcInfo.SetModel(dropInfo.Model);

                    User us = user.CustomData as User;
                    if (null != us)
                    {
                        Msg_RC_DropNpc builder = DataSyncUtility.BuildDropNpcMessage(npcInfo, fromNpcId, (int)dropType, num, model);
                        us.SendMessage(builder);
                    }
                }
            }
        }

        private void CalcPvpCombatInfo(int killerLinkId, int killedLinkId, string killerNickname, string killedNickname)
        {
            int totalKillCountForBlue = 0;
            int totalKillCountForRed = 0;
            for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo user = linkNode.Value;
                if (user.GetCampId() == (int)CampIdEnum.Blue)
                {
                    totalKillCountForBlue += user.GetCombatStatisticInfo().KillHeroCount;
                }
                else
                {
                    totalKillCountForRed += user.GetCombatStatisticInfo().KillHeroCount;
                }
            }

            Msg_RC_PvpCombatInfo builder = new Msg_RC_PvpCombatInfo();
            builder.kill_hero_count_for_blue = totalKillCountForBlue;
            builder.kill_hero_count_for_red = totalKillCountForRed;
            builder.link_id_for_killer = killerLinkId;
            builder.link_id_for_killed = killedLinkId;
            builder.killer_nickname = killerNickname;
            builder.killed_nickname = killedNickname;

            NotifyAllUser(builder);
        }

        private void ReleaseControl(UserInfo user)
        {
            if (null != user.ControlledObject)
            {
                int controller = user.GetId();
                int controlled = user.ControlledObject.GetId();
                CharacterInfo.ReleaseControlObject(user, user.ControlledObject);

                Msg_RC_ControlObject builder = DataSyncUtility.BuildControlObjectMessage(controller, controlled, false);
                NotifyAllUser(builder);
            }
        }

        private void ReleaseControl(NpcInfo npc)
        {
            if (null != npc.ControllerObject)
            {
                int controller = npc.ControllerObject.GetId();
                int controlled = npc.GetId();
                CharacterInfo.ReleaseControlObject(npc.ControllerObject, npc);

                Msg_RC_ControlObject builder = DataSyncUtility.BuildControlObjectMessage(controller, controlled, false);
                NotifyAllUser(builder);
            }
        }

        internal int GetBattleNpcCount()
        {
            int ct = 0;
            for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (null != info && info.DeadTime <= 0 && info.IsCombatNpc())
                {
                    ++ct;
                }
            }
            return ct;
        }

        internal int GetLivingUserCount()
        {
            int ct = 0;
            for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo info = linkNode.Value;
                if (null != info && info.Hp > 0)
                {
                    User us = info.CustomData as User;
                    if (null != us && us.UserControlState == (int)UserControlState.User)
                    {
                        ++ct;
                    }
                }
            }
            return ct;
        }
    }
}
