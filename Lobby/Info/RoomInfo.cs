using System;
using System.Collections.Generic;
using CSharpCenterClient;
using Lobby_RoomServer;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal enum RoomState
    {
        Prepare,    //准备
        Start,      //启动
        Game,       //游戏中
        End,        //游戏结束
        Close,      //房间关闭
        Recycle,    //回收状态(未激活)
    }

    internal class RoomBattleInfo
    {
        internal RoomBattleInfo()
        {
        }
        internal ulong Guid
        { get; set; }
        internal int SceneId
        { get; set; }
        internal DateTime StartTime
        { get; set; }
        internal DateTime EndTime
        { get; set; }
    }

    internal class RoomInfo
    {
        internal RoomInfo()
        {
            this.CurrentState = RoomState.Recycle;
        }
        internal void Reset()
        {
            m_IsPrepared = false;
            this.CurrentState = RoomState.Recycle;
        }
        internal LobbyInfo LobbyInfo
        {
            get { return m_LobbyInfo; }
            set { m_LobbyInfo = value; }
        }
        internal bool IsEmpty
        {
            get { return UserCount == 0; }
        }
        internal int UserCount
        {
            get { return m_Users.Count; }
        }
        internal int UserCountOfBlue
        {
            get { return m_UserCountOfBlue; }
        }
        internal int UserCountOfRed
        {
            get { return m_UserCountOfRed; }
        }
        internal string RoomServerName
        {
            get { return m_RoomServerName; }
            set { m_RoomServerName = value; }
        }
        internal ulong Creator
        {
            get { return m_Creator; }
            set
            {
                m_Creator = value;
                UserInfo info = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(m_Creator);
                if (null != info)
                {
                    m_CreatorNick = info.Nickname;
                }
            }
        }
        internal string CreatorNick
        {
            get { return m_CreatorNick; }
        }
        internal int RoomId
        {
            get { return m_RoomId; }
            set { m_RoomId = value; }
        }
        internal int SceneType
        {
            get { return m_SceneType; }
            set { m_SceneType = value; }
        }
        internal int TotalCount
        {
            get { return m_TotalCount; }
            set { m_TotalCount = value; }
        }
        internal float LifeTime
        {
            get { return m_LifeTime; }
            set { m_LifeTime = value; }
        }
        internal bool IsPrepared
        {
            get { return m_IsPrepared; }
            set { m_IsPrepared = value; }
        }
        internal bool IsCustomRoom
        {
            get { return m_Creator > 0; }
        }
        internal bool IsPvp
        {
            get { return LobbyServer.Instance.TypeIsPvp(m_SceneType); }
        }
        internal Dictionary<ulong, WeakReference> Users
        {
            get { return m_Users; }
        }
        internal RoomState CurrentState
        { get; set; }
        internal CampIdEnum WinnerCamp
        { get; set; }
        internal DateTime StartTime
        { get; set; }
        internal DateTime EndTime
        { get; set; }

        internal int CalcUserCount(int campId)
        {
            int ct = 0;
            foreach (WeakReference info in m_Users.Values)
            {
                UserInfo userInfo = info.Target as UserInfo;
                if (userInfo != null)
                {
                    if (userInfo.CampId == campId)
                        ++ct;
                }
            }
            return ct;
        }

        internal UserInfo GetUserInfo(ulong guid)
        {
            UserInfo info = null;
            WeakReference weakRef;
            if (m_Users.TryGetValue(guid, out weakRef))
            {
                info = weakRef.Target as UserInfo;
            }
            return info;
        }

        internal void AddUsers(int camp, ulong[] users)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            foreach (ulong user in users)
            {
                UserInfo info = dataProcess.GetUserInfo(user);
                if (info != null)
                {
                    info.Room = this;
                    info.IsPrepared = false;
                    info.CampId = camp;

                    if (m_Users.ContainsKey(user))
                    {
                        m_Users[user] = new WeakReference(info);
                    }
                    else
                    {
                        m_Users.Add(user, new WeakReference(info));
                    }
                }
            }
            UpdateUserCount();
        }
        internal void AddMachine(int camp, UserInfo machine)
        {
            if (machine != null)
            {
                machine.Room = this;
                machine.CampId = camp;
                if (m_Users.ContainsKey(machine.Guid))
                {
                    m_Users[machine.Guid] = new WeakReference(machine);
                }
                else
                {
                    m_Users.Add(machine.Guid, new WeakReference(machine));
                }
            }
            UpdateUserCount();
        }

        internal void DelUsers(params ulong[] users)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            foreach (ulong user in users)
            {
                foreach (WeakReference info in m_Users.Values)
                {
                    UserInfo userInfo = info.Target as UserInfo;
                    if (userInfo != null)
                    {
                        JsonMessageSyncQuitRoom syncQuitRoomMsg = new JsonMessageSyncQuitRoom();
                        syncQuitRoomMsg.m_Guid = userInfo.Guid;
                        syncQuitRoomMsg.m_QuitGuid = user;
                        JsonMessageDispatcher.SendDcoreMessage(userInfo.NodeName, syncQuitRoomMsg);
                        LogSys.Log(LOG_TYPE.DEBUG, "SyncQuitRoom,receiver:{0},guid:{1}", userInfo.Guid, user);
                    }
                }
            }
            foreach (ulong user in users)
            {
                UserInfo info = dataProcess.GetUserInfo(user);
                if (null != info)
                {
                    info.Room = null;
                    info.IsPrepared = false;
                    LogSys.Log(LOG_TYPE.DEBUG, "LeaveRoom:{0}", user);
                }
                m_Users.Remove(user);
            }
            UpdateUserCount();
        }

        internal void RequestStartGame(UserInfo userInfo)
        {
            if (null != userInfo)
            {
                userInfo.IsPrepared = true;
                foreach (WeakReference info in m_Users.Values)
                {
                    UserInfo user = info.Target as UserInfo;
                    if (user != null)
                    {
                        JsonMessageSyncPrepared syncPreparedMsg = new JsonMessageSyncPrepared();
                        syncPreparedMsg.m_Guid = user.Guid;
                        syncPreparedMsg.m_PreparedGuid = userInfo.Guid;
                        JsonMessageDispatcher.SendDcoreMessage(user.NodeName, syncPreparedMsg);
                    }
                }
                LogSys.Log(LOG_TYPE.DEBUG, "RequestStartGame,guid:{0}", userInfo.Guid);
            }
        }

        internal void StartBattleRoom()
        {
            if (CurrentState != RoomState.Game)
            {
                CurrentState = RoomState.Game;  //房间进入游戏状态
            }
            RoomServerInfo svrInfo;
            if (m_LobbyInfo.RoomServerInfos.TryGetValue(m_RoomServerName, out svrInfo))
            {
                foreach (WeakReference userRef in m_Users.Values)
                {
                    UserInfo info = userRef.Target as UserInfo;
                    if (info != null && info.CurrentState != UserState.Room)
                    {
                        JsonMessageWithGuid startGameResultMsg = new JsonMessageWithGuid(JsonMessageID.StartGameResult);
                        startGameResultMsg.m_Guid = info.Guid;
                        ArkCrossEngineMessage.Msg_LC_StartGameResult protoData = new ArkCrossEngineMessage.Msg_LC_StartGameResult();
                        protoData.server_ip = svrInfo.ServerIp;
                        protoData.server_port = svrInfo.ServerPort;
                        protoData.key = info.Key;
                        protoData.hero_id = info.HeroId;
                        protoData.camp_id = info.CampId;
                        protoData.scene_type = SceneType;
                        protoData.result = (int)GeneralOperationResult.LC_Succeed;

                        startGameResultMsg.m_ProtoData = protoData;

                        JsonMessageDispatcher.SendDcoreMessage(info.NodeName, startGameResultMsg);
                        LogSys.Log(LOG_TYPE.DEBUG, "StartGameResult, guid:{0},key:{1},ip:{2},port:{3},hero:{4},camp:{5},scene:{6}", info.Guid, info.Key, svrInfo.ServerIp, svrInfo.ServerPort, info.HeroId, protoData.camp_id, SceneType);
                    }
                }
            }
        }

        internal void StopBattleRoom()
        {
            ulong[] guids = new ulong[m_Users.Count];
            m_Users.Keys.CopyTo(guids, 0);
            DelUsers(guids);
            this.CurrentState = RoomState.Close;  //房间进入关闭状态：RoomServer创建游戏房间失败
        }

        internal bool CheckJoinCondition(ulong guid)
        {
            return true;
        }

        internal List<Msg_LR_RoomUserInfo> BuildRoomUsersInfo(long curTime)
        {
            List<Msg_LR_RoomUserInfo> builder = new List<Msg_LR_RoomUserInfo>();
            foreach (KeyValuePair<ulong, WeakReference> pair in m_Users)
            {
                ulong guid = pair.Key;
                UserInfo info = pair.Value.Target as UserInfo;
                if (info != null && guid == info.Guid)
                {
                    // check state
                    if (!info.IsPrepared || (info.CurrentState != UserState.Teaming))
                    {
                        continue;
                    }

                    // check time stamp
                    if (info.LastAddUserTime + 1000 < curTime)
                    {
                        info.LastAddUserTime = curTime;
                    }
                    else
                    {
                        continue;
                    }

                    Msg_LR_RoomUserInfo.Builder ruiBuilder = Msg_LR_RoomUserInfo.CreateBuilder();
                    ruiBuilder.SetGuid(info.Guid);
                    ruiBuilder.SetNick(info.Nickname);
                    ruiBuilder.SetKey(info.Key);
                    ruiBuilder.SetHero(info.HeroId);
                    ruiBuilder.SetCamp(info.CampId);
                    ruiBuilder.SetIsMachine(info.IsMachine);
                    ruiBuilder.SetLevel(info.Level);
                    ruiBuilder.SetX((long)info.X);
                    ruiBuilder.SetZ((long)info.Z);
                    int arg_score = info.FightingScore;
                    if (null != info.Group && null != info.Group.Members)
                    {
                        foreach (GroupMemberInfo m in info.Group.Members)
                        {
                            UserInfo member = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(m.Guid);
                            if (null != member && member.Guid != guid)
                            {
                                if (member.FightingScore > arg_score)
                                    arg_score = member.FightingScore;
                            }
                        }
                    }
                    arg_score = (int)(arg_score * 0.8);
                    ruiBuilder.SetArgScore(arg_score);
                    ///
                    Msg_LR_RoomUserInfo.Types.SkillInfo.Builder[] skill_assit = new Msg_LR_RoomUserInfo.Types.SkillInfo.Builder[4];
                    for (int i = 0; i < skill_assit.Length; i++)
                    {
                        skill_assit[i] = Msg_LR_RoomUserInfo.Types.SkillInfo.CreateBuilder();
                        skill_assit[i].SetSkillId(0);
                        skill_assit[i].SetSkillLevel(0);
                    }
                    if (null != info.Skill && null != info.Skill.Skills)
                    {
                        int cur_preset_index = info.Skill.CurPresetIndex;
                        if (cur_preset_index >= 0)
                        {
                            for (int i = 0; i < skill_assit.Length; i++)
                            {
                                for (int j = 0; j < info.Skill.Skills.Count; j++)
                                {
                                    if (info.Skill.Skills[j].Postions.Presets[cur_preset_index] == (SlotPosition)(i + 1))
                                    {
                                        skill_assit[i].SetSkillId(info.Skill.Skills[j].ID);
                                        skill_assit[i].SetSkillLevel(info.Skill.Skills[j].Level);
                                        break;
                                    }
                                }
                            }
                            for (int i = 0; i < skill_assit.Length; i++)
                            {
                                ruiBuilder.SkillsList.Add(skill_assit[i].Build());
                            }
                            ruiBuilder.SetPresetIndex(cur_preset_index);
                        }
                    }
                    ///
                    if (null != info.Equip && null != info.Equip.Armor)
                    {
                        for (int i = 0; i < info.Equip.Armor.Length; i++)
                        {
                            Msg_LR_RoomUserInfo.Types.EquipInfo.Builder equip_assit = Msg_LR_RoomUserInfo.Types.EquipInfo.CreateBuilder();
                            equip_assit.SetEquipId(info.Equip.Armor[i].ItemId);
                            equip_assit.SetEquipLevel(info.Equip.Armor[i].Level);
                            equip_assit.SetEquipRandomProperty(info.Equip.Armor[i].AppendProperty);
                            ruiBuilder.EquipsList.Add(equip_assit.Build());
                        }
                    }
                    ///
                    if (null != info.Legacy && null != info.Legacy.SevenArcs)
                    {
                        for (int i = 0; i < info.Legacy.SevenArcs.Length; i++)
                        {
                            Msg_LR_RoomUserInfo.Types.LegacyInfo.Builder legacy_assit = Msg_LR_RoomUserInfo.Types.LegacyInfo.CreateBuilder();
                            legacy_assit.SetLegacyId(info.Legacy.SevenArcs[i].ItemId);
                            legacy_assit.SetLegacyLevel(info.Legacy.SevenArcs[i].Level);
                            legacy_assit.SetLegacyRandomProperty(info.Legacy.SevenArcs[i].AppendProperty);
                            legacy_assit.SetLegacyIsUnlock(info.Legacy.SevenArcs[i].IsUnlock);
                            ruiBuilder.LegacysList.Add(legacy_assit.Build());
                        }
                    }
                    ///
                    if (null != info.XSoul)
                    {
                        foreach (ItemInfo item in info.XSoul.GetAllXSoulPartData().Values)
                        {
                            Msg_LR_RoomUserInfo.Types.XSoulDataInfo.Builder xsoul_msg = Msg_LR_RoomUserInfo.Types.XSoulDataInfo.CreateBuilder();
                            xsoul_msg.ItemId = item.ItemId;
                            xsoul_msg.Level = item.Level;
                            xsoul_msg.ModelLevel = item.ShowModelLevel;
                            xsoul_msg.Experience = item.Experience;
                            ruiBuilder.XSoulsList.Add(xsoul_msg.Build());
                        }
                    }
                    // partner
                    PartnerInfo partnerInfo = info.PartnerStateInfo.GetActivePartner();
                    if (null != partnerInfo)
                    {
                        Msg_LR_RoomUserInfo.Types.PartnerInfo.Builder partner = Msg_LR_RoomUserInfo.Types.PartnerInfo.CreateBuilder();
                        partner.SetPartnerId(partnerInfo.Id);
                        partner.SetPartnerLevel(partnerInfo.CurAdditionLevel);
                        partner.SetPartnerStage(partnerInfo.CurSkillStage);
                        ruiBuilder.SetPartner(partner);
                    }
                    builder.Add(ruiBuilder.Build());
                }
            }

            return builder;
        }

        internal void Tick()
        {
            if (this.CurrentState == RoomState.End)
            {
                //游戏结束状态：检测数据存储是否完成
                //等待每个玩家游戏数据存储完成后，将房间状态改为Close关闭状态
                int count = 0;
                foreach (bool flag in m_UserDSFlags.Values)
                {
                    if (flag == true)
                    {
                        count++;
                    }
                }
                if (count == m_UserDSFlags.Count)
                {
                    m_UserDSFlags.Clear();
                    this.CloseRoom();
                }
            }
            //清除不在游戏中的玩家数据
            if (UserCount > 0)
            {
                m_RecycledGuids.Clear();
                foreach (KeyValuePair<ulong, WeakReference> pair in m_Users)
                {
                    ulong guid = pair.Key;
                    UserInfo info = pair.Value.Target as UserInfo;
                    if (info == null || info.IsRecycled || info.CurrentState == UserState.DropOrOffline || info.Guid != guid)
                    {
                        m_RecycledGuids.Add(guid);
                        LogSys.Log(LOG_TYPE.DEBUG, "Room {0} has a exception user {1} !!!", m_RoomId, guid);
                    }
                }
                if (m_RecycledGuids.Count > 0)
                {
                    foreach (ulong guid in m_RecycledGuids)
                    {
                        m_Users.Remove(guid);
                    }
                    UpdateUserCount();
                }
            }
            // 首次创建房间
            // if (!m_IsPrepared)
            {
                if (UserCount > 0)
                {
                    long curTime = TimeUtility.GetServerMilliseconds();

                    // log per second
                    /*
                    bool canLog = false;
                    if (m_LastLogTime + 1000 < curTime)
                    {
                        m_LastLogTime = curTime;
                        canLog = true;
                    }
                    if (canLog)
                    {
                        LogSys.Log(LOG_TYPE.DEBUG, "Room {0} will on {1}", m_RoomId, m_RoomServerName);
                    }
                    */

                    // notify match result
                    foreach (KeyValuePair<ulong, WeakReference> pair in m_Users)
                    {
                        ulong guid = pair.Key;
                        UserInfo info = pair.Value.Target as UserInfo;
                        if (info != null && !info.IsPrepared)
                        {
                            if (info.LastNotifyMatchTime + 5000 < curTime)
                            {
                                info.LastNotifyMatchTime = curTime;

                                JsonMessageWithGuid mrMsg = new JsonMessageWithGuid(JsonMessageID.MatchResult);
                                mrMsg.m_Guid = guid;
                                ArkCrossEngineMessage.Msg_LC_MatchResult protoData = new ArkCrossEngineMessage.Msg_LC_MatchResult();
                                protoData.m_Result = (int)TeamOperateResult.OR_Succeed;
                                mrMsg.m_ProtoData = protoData;
                                JsonMessageDispatcher.SendDcoreMessage(info.NodeName, mrMsg);
                            }
                        }
                    }

                    // 检查成员状态，如果都准备好则通知RoomServer创建副本
                    if (this.CurrentState == RoomState.Prepare)
                    {
                        bool isOk = true;
                        foreach (KeyValuePair<ulong, WeakReference> pair in m_Users)
                        {
                            ulong guid = pair.Key;
                            UserInfo info = pair.Value.Target as UserInfo;
                            if (info != null && !info.IsPrepared && guid == info.Guid)
                            {
                                isOk = false;
                                break;
                            }
                        }
                        if (isOk)
                        {
                            var userInfoBuilders = BuildRoomUsersInfo(curTime);
                            if (userInfoBuilders.Count > 0)
                            {
                                Msg_LR_CreateBattleRoom.Builder cbrBuilder = Msg_LR_CreateBattleRoom.CreateBuilder();
                                cbrBuilder.SetRoomId(RoomId);
                                cbrBuilder.SetSceneType(SceneType);
                                cbrBuilder.AddRangeUsers(userInfoBuilders);

                                //sirius TODO:在此处确定要连接的RoomServer
                                LobbyServer.Instance.RoomSvrChannel.Send(m_RoomServerName, cbrBuilder.Build());
                                this.CurrentState = RoomState.Start;  //房间启动状态
                                this.StartTime = DateTime.Now;
                                LogSys.Log(LOG_TYPE.INFO, "Multi Play Room will run on Roomserver {0} roomid {1} scene {2} for {3} users ...", m_RoomServerName, RoomId, SceneType, UserCount);
                            }
                        }
                    }
                    // 检查是否有新添加进来的用户
                    else if (CurrentState == RoomState.Game)
                    {
                        bool isOk = false;
                        foreach (KeyValuePair<ulong, WeakReference> pair in m_Users)
                        {
                            ulong guid = pair.Key;
                            UserInfo info = pair.Value.Target as UserInfo;
                            if (info != null && info.IsPrepared && info.CurrentState != UserState.Room && guid == info.Guid)
                            {
                                isOk = true;
                                break;
                            }
                        }

                        if (isOk)
                        {
                            var userInfoBuilders = BuildRoomUsersInfo(curTime);
                            if (userInfoBuilders.Count > 0)
                            {
                                Msg_LR_AddNewUsr.Builder anuBuilder = Msg_LR_AddNewUsr.CreateBuilder();
                                anuBuilder.SetRoomId(RoomId);
                                anuBuilder.AddRangeUsers(userInfoBuilders);

                                //sirius TODO:在此处确定要连接的RoomServer
                                LobbyServer.Instance.RoomSvrChannel.Send(m_RoomServerName, anuBuilder.Build());
                                LogSys.Log(LOG_TYPE.INFO, "Add {1} users on roomid {0} ...", RoomId, UserCount);
                            }
                        }
                    }
                    else
                    {
                        ;
                    }
                }
            }
        }

        internal void ProcessBattleResult(List<UserInfo> users)
        {
            //gow旧积分缓存
            int ct = users.Count;
            int[] oldElos = null;
            if (m_SceneType == (int)MatchSceneEnum.Gow || (int)MatchSceneEnum.Dare == m_SceneType)
            {
                oldElos = new int[ct];
                int ix = 0;
                foreach (UserInfo user in users)
                {
                    if (user != null)
                    {
                        if (ix < ct)
                        {
                            oldElos[ix] = user.GowInfo.GowElo;
                            ++ix;
                        }
                    }
                }
            }

            //计算战斗结果
            UserInfo win = null, lost = null;
            foreach (UserInfo user in users)
            {
                if (user != null)
                {
                    //当前战斗数据
                    user.CurrentBattleInfo.SceneID = this.SceneType;
                    user.CurrentBattleInfo.EndTime = TimeUtility.GetServerMilliseconds();
                    if (user.CurrentBattleInfo.BattleResult == BattleResultEnum.Win)
                    {
                        win = user;
                    }
                    else if (user.CurrentBattleInfo.BattleResult == BattleResultEnum.Lost)
                    {
                        lost = user;
                    }
                }
            }
            if (m_SceneType == (int)MatchSceneEnum.Gow && null != win && null != lost)
            {
                CalcGowElo(win, lost);
                ++win.GowInfo.GowMatches;
                ++lost.GowInfo.GowMatches;
                ++win.GowInfo.GowWinMatches;
                GlobalDataProcessThread globalDataThread = LobbyServer.Instance.GlobalDataProcessThread;
                globalDataThread.QueueAction(globalDataThread.UpdateGowElo, win, lost);
            }

            //发送战斗结果数据
            if ((int)MatchSceneEnum.Gow == m_SceneType || (int)MatchSceneEnum.Dare == m_SceneType)
            {
                //todo:进一步根据场景ID区分是何种类型的pvp
                JsonMessageWithGuid gowResultMsg = new JsonMessageWithGuid(JsonMessageID.SyncGowBattleResult);
                ArkCrossEngineMessage.Msg_LC_SyncGowBattleResult protoData = new ArkCrossEngineMessage.Msg_LC_SyncGowBattleResult();
                string[] nicks = new string[ct];
                int[] heros = new int[ct];
                int[] elos = new int[ct];
                int[] hitcounts = new int[ct];
                int[] damages = new int[ct];
                int ix = 0;
                foreach (UserInfo user in users)
                {
                    if (user != null)
                    {
                        if (ix < ct)
                        {
                            nicks[ix] = user.Nickname;
                            heros[ix] = user.HeroId;
                            elos[ix] = user.GowInfo.GowElo;
                            hitcounts[ix] = user.CurrentBattleInfo.MaxMultiHitCount;
                            damages[ix] = user.CurrentBattleInfo.TotalDamageFromMyself;
                            ++ix;
                        }
                    }
                }
                ix = 0;
                foreach (UserInfo user in users)
                {
                    if (user != null)
                    {
                        gowResultMsg.m_Guid = user.Guid;
                        protoData.m_Result = (int)user.CurrentBattleInfo.BattleResult;
                        if (ix < ct)
                        {
                            protoData.m_OldGowElo = oldElos[ix];
                            protoData.m_GowElo = elos[ix];
                            protoData.m_MaxMultiHitCount = hitcounts[ix];
                            protoData.m_TotalDamage = damages[ix];
                            protoData.m_EnemyNick = nicks[ct - ix - 1];
                            protoData.m_EnemyHeroId = heros[ct - ix - 1];
                            protoData.m_EnemyOldGowElo = oldElos[ct - ix - 1];
                            protoData.m_EnemyGowElo = elos[ct - ix - 1];
                            protoData.m_EnemyMaxMultiHitCount = hitcounts[ct - ix - 1];
                            protoData.m_EnemyTotalDamage = damages[ct - ix - 1];
                            ++ix;
                        }
                        gowResultMsg.m_ProtoData = protoData;
                        JsonMessageDispatcher.SendDcoreMessage(user.NodeName, gowResultMsg);
                        if ((int)MatchSceneEnum.Gow == m_SceneType)
                            LogSys.Log(LOG_TYPE.INFO, "Send Pvp Battle Result to User:{0}, User data Count: {1}", user.Guid, m_Users.Count);
                        else
                            LogSys.Log(LOG_TYPE.INFO, "Send Dare Battle Result to User:{0}, User data Count: {1}", user.Guid, m_Users.Count);
                    }
                }
                ///
                if ((int)MatchSceneEnum.Gow == m_SceneType)
                    RecordPvpAction(win, lost);
            }
            else
            {
                //发送多人pve结束消息
                JsonMessageWithGuid mpveResultMsg = new JsonMessageWithGuid(JsonMessageID.SyncMpveBattleResult);
                ArkCrossEngineMessage.Msg_LC_SyncMpveBattleResult protoData = new ArkCrossEngineMessage.Msg_LC_SyncMpveBattleResult();
                foreach (UserInfo user in users)
                {
                    if (user != null)
                    {
                        mpveResultMsg.m_Guid = user.Guid;
                        protoData.m_Result = (int)user.CurrentBattleInfo.BattleResult;
                        protoData.m_KillNpcCount = user.CurrentBattleInfo.KillNpcCount;
                        mpveResultMsg.m_ProtoData = protoData;
                        JsonMessageDispatcher.SendDcoreMessage(user.NodeName, mpveResultMsg);
                        LogSys.Log(LOG_TYPE.INFO, "Send Pve Battle Result to User:{0}, User data Count: {1}", user.Guid, m_Users.Count);
                    }
                }
            }
        }

        private void RecordPvpAction(UserInfo win, UserInfo lost)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            if (null != win && null != lost)
            {
                AccountInfo win_acc = dataProcess.FindAccountInfoById(win.AccountId);
                AccountInfo lost_acc = dataProcess.FindAccountInfoById(lost.AccountId);
                if (null != win_acc && null != lost_acc)
                {
                    /// norm log
                    LogSys.NormLog("pvp", LobbyConfig.AppKeyStr, win_acc.ClientGameVersion, Module.pvp, LobbyConfig.LogNormVersionStr,
                      "C0400", win_acc.LogicServerId, win.AccountId, win.Guid, win.Level, win.HeroId, win.FightingScore,
                      lost.AccountId, lost.Guid, lost.Level, lost.HeroId, lost.FightingScore, 1);
                }
            }
        }

        //战斗结束
        internal void EndBattle()
        {
            this.EndTime = DateTime.Now;
            this.CurrentState = RoomState.End;  //房间进入游戏结束状态
        }

        internal void CloseRoom()
        {
            //清空房间内玩家数据 
            foreach (WeakReference info in m_Users.Values)
            {
                UserInfo user = info.Target as UserInfo;
                if (user != null)
                {
                    if (user.CurrentState == UserState.Room)
                    {
                        user.CurrentState = UserState.Online;
                    }
                    user.ResetRoomInfo();
                    if (user.IsDisconnected)
                    {
                        DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
                        dataProcess.DispatchAction(dataProcess.DoUserLogoff, user.Guid);
                    }
                }
            }
            m_Users.Clear();
            this.CurrentState = RoomState.Close;  //房间进入关闭状态
            LogSys.Log(LOG_TYPE.INFO, "Lobby Room Close, roomID:{0}", RoomId);
        }

        private void CalcGowElo(UserInfo win, UserInfo lost)
        {
            const float c_W = 1.0f;
            const float c_Wwin = 1.0f;
            const float c_Wlost = 0f;

            float winRate = 0, lostRate = 0;
            int delta = win.GowInfo.GowElo - lost.GowInfo.GowElo;
            GowConstConfig cfg = GowConfigProvider.Instance.FindGowConstConfig(Math.Abs(delta));
            if (null != cfg)
            {
                winRate = cfg.m_HighRate;
                lostRate = cfg.m_LowRate;
            }
            double pWin = winRate;
            double pLost = lostRate;
            double eWin = c_W * pWin;
            double eLost = c_W * pWin;
            int kWin = CalcGowK(win.GowInfo.GowElo);
            int kLost = CalcGowK(lost.GowInfo.GowElo);
            int deltaWin = (int)(kWin * (c_Wwin - eWin));
            int deltaLost = (int)(kLost * (c_Wlost - eLost));
            int final_score = deltaWin;
            if (win.GowInfo.GowElo < lost.GowInfo.GowElo)
            {
                final_score = Math.Abs(deltaLost);
            }
            int eloWin = win.GowInfo.GowElo + final_score;
            int eloLost = lost.GowInfo.GowElo - final_score;
            win.GowInfo.GowElo = eloWin;
            lost.GowInfo.GowElo = eloLost;
            win.CurrentBattleInfo.Elo = eloWin;
            lost.CurrentBattleInfo.Elo = eloLost;

            Tuple<UserInfo, int, bool> first = new Tuple<UserInfo, int, bool>(win, final_score, true);
            UpdateGowInfo(first);
            Tuple<UserInfo, int, bool> second = new Tuple<UserInfo, int, bool>(lost, -final_score, false);
            UpdateGowInfo(second);

            UpdateHistoryGowElo(win);
            UpdateHistoryGowElo(lost);
        }
        private void UpdateGowInfo(Tuple<UserInfo, int, bool> entity)
        {
            GowRank.UpdateGowPoint(entity);
            UserInfo user = entity.Item1;
            GowRank.UpdateGowRank(user);
        }
        private void UpdateHistoryGowElo(UserInfo user)
        {
            DateTime nowDate = DateTime.Now.Date;
            DateTime now = new DateTime(nowDate.Year, nowDate.Month, nowDate.Day, GowSystem.m_PrizeTime.m_Hour, GowSystem.m_PrizeTime.m_Minute, GowSystem.m_PrizeTime.m_Second);

            long key = now.ToBinary();
            int elo = user.GowInfo.GowElo;
            SortedList<long, int> history = user.GowInfo.HistoryGowElos;
            if (history.ContainsKey(key))
            {
                history[key] = elo;
            }
            else
            {
                history.Add(key, elo);
            }

            if (history.Count > GowSystem.c_PrizeValidityPeriod)
            {
                List<long> deletes = new List<long>();
                foreach (long key0 in history.Keys)
                {
                    DateTime dt = DateTime.FromBinary(key0);
                    if ((now - dt).Days > GowSystem.c_PrizeValidityPeriod)
                    {
                        deletes.Add(key0);
                    }
                }
                foreach (long key0 in deletes)
                {
                    history.Remove(key0);
                }
            }
        }

        private int CalcGowK(int elo)
        {
            int k = 0;
            if (elo > GowSystem.m_Upper)
            {
                k = GowSystem.m_K1;
            }
            else if (elo < GowSystem.m_Lower)
            {
                k = GowSystem.m_K3;
            }
            else
            {
                k = GowSystem.m_K2_1 - elo / GowSystem.m_K2_2;
            }
            return k;
        }

        private string m_RoomServerName = "";
        private ulong m_Creator = 0;
        private string m_CreatorNick = "";
        private int m_RoomId = 0;
        private int m_SceneType = 0;
        private int m_TotalCount = 0;
        private long m_LastLogTime = 0;
        private float m_LifeTime = 0;
        private bool m_IsPrepared = false;
        private int m_UserCountOfBlue = 0;
        private int m_UserCountOfRed = 0;
        private HashSet<ulong> m_RecycledGuids = new HashSet<ulong>();
        private LobbyInfo m_LobbyInfo = null;
        private Dictionary<ulong, bool> m_UserDSFlags = new Dictionary<ulong, bool>();  //玩家战斗数据存储标识   

        private Dictionary<ulong, WeakReference> m_Users = new Dictionary<ulong, WeakReference>();
        private Dictionary<ulong, UserBattleInfo> m_UserBattleInfos = new Dictionary<ulong, UserBattleInfo>();

        private void UpdateUserCount()
        {
            m_UserCountOfBlue = CalcUserCount((int)CampIdEnum.Blue);
            m_UserCountOfRed = CalcUserCount((int)CampIdEnum.Red);
        }
    }
}

