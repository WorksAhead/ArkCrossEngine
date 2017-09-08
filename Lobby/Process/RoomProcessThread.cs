using System;
using System.Threading;
using System.Threading.Tasks;

using CSharpCenterClient;
using Lobby_RoomServer;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    /// <summary>
    /// 房间逻辑线程。处理玩家在大厅组队后的各种逻辑。
    /// </summary>
    /// <remarks>
    /// 其它线程不应直接调用此类方法，应通过QueueAction发起调用。
    /// </remarks>
    internal sealed class RoomProcessThread : ArkCrossEngine.MyServerThread
    {
        internal RoomProcessThread()
        {
        }

        internal void RegisterRoomServer(RoomServerInfo info)
        {
            if (!m_LobbyInfo.RoomServerInfos.ContainsKey(info.RoomServerName))
            {
                m_LobbyInfo.RoomServerInfos.Add(info.RoomServerName, info);
            }
            else
            {
                RoomServerInfo info_ = m_LobbyInfo.RoomServerInfos[info.RoomServerName];
                info_.RoomServerName = info.RoomServerName;
                info_.ServerIp = info.ServerIp;
                info_.ServerPort = info.ServerPort;
                info_.MaxRoomNum = info.MaxRoomNum;
                //To do more 关闭原RoomServer上的所有房间
                foreach (var room in m_LobbyInfo.Rooms.Values)
                {
                    if (room.RoomServerName == info.RoomServerName)
                    {
                        room.EndBattle();
                    }
                }
            }
            Msg_LR_ReplyRegisterRoomServer.Builder resultBuilder = Msg_LR_ReplyRegisterRoomServer.CreateBuilder();
            resultBuilder.SetIsOk(true);
            LobbyServer.Instance.RoomSvrChannel.Send(info.RoomServerName, resultBuilder.Build());
            LogSys.Log(LOG_TYPE.DEBUG, "RegisterRoomServer,name:{0},ip:{1},port:{2},max room num:{3}", info.RoomServerName, info.ServerIp, info.ServerPort, info.MaxRoomNum);
        }

        internal void RegisterNodeJs(NodeInfo info)
        {
            NodeInfo info_;
            if (!m_LobbyInfo.NodeInfos.TryGetValue(info.NodeName, out info_))
                m_LobbyInfo.NodeInfos.Add(info.NodeName, info);
            else
            {
                info_.NodeName = info.NodeName;
            }
            JsonMessageNodeJsRegisterResult resultMsg = new JsonMessageNodeJsRegisterResult();
            resultMsg.m_IsOk = true;
            JsonMessageDispatcher.SendDcoreMessage(info.NodeName, resultMsg);

            LogSys.Log(LOG_TYPE.DEBUG, "RegisterNodeJs,name:{0}", info.NodeName);
        }

        internal void UpdateRoomServerInfo(RoomServerInfo info)
        {
            RoomServerInfo info_;
            if (!m_LobbyInfo.RoomServerInfos.TryGetValue(info.RoomServerName, out info_))
                m_LobbyInfo.RoomServerInfos.Add(info.RoomServerName, info);
            else
            {
                info_.IdleRoomNum = info.IdleRoomNum;
                info_.UserNum = info.UserNum;
                info_.AllocedRoomNum = 0;
            }

            LogSys.Log(LOG_TYPE.DEBUG, "UpdateRoomServerInfo,name:{0},idle room num:{1},cur user num:{2}", info.RoomServerName, info.IdleRoomNum, info.UserNum);
        }

        internal void RequestSinglePVE(ulong guid, int sceneId)
        {
            Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
            if (null == cfg || cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE)
            {
                //单人pve不建立房间，直接开始游戏（todo:奖励记到人身上[多人的奖励也是这样]）
                DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
                UserInfo user = dataProcess.GetUserInfo(guid);
                if (user != null)
                {
                    user.CurrentBattleInfo.init(sceneId, user.HeroId);
                    SyncCombatData(user);
                    JsonMessageWithGuid startGameResultMsg = new JsonMessageWithGuid(JsonMessageID.StartGameResult);
                    startGameResultMsg.m_Guid = user.Guid;
                    ArkCrossEngineMessage.Msg_LC_StartGameResult protoData = new ArkCrossEngineMessage.Msg_LC_StartGameResult();
                    GeneralOperationResult result = GeneralOperationResult.LC_Succeed;
                    if (user.CurStamina >= cfg.m_CostStamina)
                    {
                        if (!GlobalVariables.Instance.IsDebug)
                        {
                            int preSceneId = SceneConfigProvider.Instance.GetPreSceneId(sceneId);
                            if (-1 != preSceneId)
                            {
                                if (!user.SceneData.ContainsKey(preSceneId))
                                {
                                    LogSystem.Error("player {0} try to enter an Illegal scene {1}", user.Guid, sceneId);
                                    return;
                                }
                            }
                            if (cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_ELITE && user.GetCompletedSceneCount(sceneId) >= user.MaxEliteSceneCompletedCount)
                            {
                                LogSystem.Error("player {0} enter an Elite scene {1} too many times {2}", user.Guid, sceneId, user.GetCompletedSceneCount(sceneId));
                                return;
                            }
                        }
                        protoData.server_ip = "127.0.0.1";
                        protoData.server_port = 9001;
                        protoData.key = user.Key;
                        protoData.hero_id = user.HeroId;
                        protoData.camp_id = (int)CampIdEnum.Blue;
                        protoData.scene_type = sceneId;
                        protoData.match_key = user.CurrentBattleInfo.MatchKey;
                        result = GeneralOperationResult.LC_Succeed;
                        user.CurrentState = UserState.Pve;
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_CostError;
                    }
                    protoData.result = (int)result;
                    startGameResultMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, startGameResultMsg);
                    LogSys.Log(LOG_TYPE.INFO, "Single Player Room will run on Lobby without room and roomserver, user {0} scene {1}", guid, sceneId);
                    /// norm log
                    AccountInfo accountInfo = dataProcess.FindAccountInfoById(user.AccountId);
                    if (null != accountInfo)
                    {
                        /// pvefight
                        LogSys.NormLog("PVEfight", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.pvefight, LobbyConfig.LogNormVersionStr,
                        "B4110", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level, sceneId, (int)FightingType.General, "null",
                        (int)PvefightResult.Failure, "null", "null");
                    }
                }
            }
        }
        private void SyncCombatData(UserInfo ui)
        {
            JsonMessageWithGuid syncCombatDataMsg = new JsonMessageWithGuid(JsonMessageID.SyncCombatData);
            syncCombatDataMsg.m_Guid = ui.Guid;
            ArkCrossEngineMessage.Msg_LC_SyncCombatData protoData = new ArkCrossEngineMessage.Msg_LC_SyncCombatData();
            int legacyCount = ui.Legacy.SevenArcs.Length;
            if (legacyCount > 0)
            {
                for (int i = 0; i < legacyCount; ++i)
                {
                    ArkCrossEngineMessage.LegacyDataMsg legacy_data = new ArkCrossEngineMessage.LegacyDataMsg();
                    legacy_data.ItemId = ui.Legacy.SevenArcs[i].ItemId;
                    legacy_data.Level = ui.Legacy.SevenArcs[i].Level;
                    legacy_data.AppendProperty = ui.Legacy.SevenArcs[i].AppendProperty;
                    legacy_data.IsUnlock = ui.Legacy.SevenArcs[i].IsUnlock;
                    protoData.m_Legacys.Add(legacy_data);
                }
            }
            foreach (ItemInfo item in ui.XSoul.GetAllXSoulPartData().Values)
            {
                ArkCrossEngineMessage.XSoulDataMsg item_msg = new ArkCrossEngineMessage.XSoulDataMsg();
                item_msg.ItemId = item.ItemId;
                item_msg.Level = item.Level;
                item_msg.Experience = item.Experience;
                item_msg.ModelLevel = item.ShowModelLevel;
                protoData.m_XSouls.Add(item_msg);
            }
            for (int i = 0; i < ui.Skill.Skills.Count; ++i)
            {
                if (ui.Skill.Skills[i].Postions.Presets[0] != SlotPosition.SP_None)
                {
                    ArkCrossEngineMessage.SkillDataInfo skill_data = new ArkCrossEngineMessage.SkillDataInfo();
                    skill_data.ID = ui.Skill.Skills[i].ID;
                    skill_data.Level = ui.Skill.Skills[i].Level;
                    skill_data.Postions = (int)ui.Skill.Skills[i].Postions.Presets[0];
                    protoData.m_Skills.Add(skill_data);
                }
            }
            for (int i = 0; i < ui.Equip.Armor.Length; ++i)
            {
                if (null != ui.Equip.Armor[i])
                {
                    ArkCrossEngineMessage.ItemDataMsg equip = new ArkCrossEngineMessage.ItemDataMsg();
                    equip.ItemId = ui.Equip.Armor[i].ItemId;
                    equip.Level = ui.Equip.Armor[i].Level;
                    equip.AppendProperty = ui.Equip.Armor[i].AppendProperty;
                    protoData.m_Equipments.Add(equip);
                }
            }
            ArkCrossEngineMessage.SelectedPartnerDataMsg partnerData = new ArkCrossEngineMessage.SelectedPartnerDataMsg();
            PartnerInfo partnerInfo = ui.PartnerStateInfo.GetActivePartner();
            if (null != partnerInfo)
            {
                partnerData.m_Id = partnerInfo.Id;
                partnerData.m_SkillStage = partnerInfo.CurSkillStage;
                partnerData.m_AdditionLevel = partnerInfo.CurAdditionLevel;
            }
            else
            {
                partnerData.m_Id = 0;
                partnerData.m_SkillStage = 0;
                partnerData.m_AdditionLevel = 0;
            }
            protoData.m_PartnerData = partnerData;
            syncCombatDataMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(ui.NodeName, syncCombatDataMsg);
        }

        internal void AllocLobbyRoom(ulong[] users, int type)
        {
            int roomId = m_LobbyInfo.CreateAutoRoom(users, type);
            long time = TimeUtility.GetServerMilliseconds();

            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            JsonMessageWithGuid matchResultMsg = new JsonMessageWithGuid(JsonMessageID.MatchResult);

            foreach (ulong user in users)
            {
                UserInfo info = dataProcess.GetUserInfo(user);
                if (info != null)
                {
                    info.LastNotifyMatchTime = time;
                    matchResultMsg.m_Guid = user;
                    ArkCrossEngineMessage.Msg_LC_MatchResult protoData = new ArkCrossEngineMessage.Msg_LC_MatchResult();
                    protoData.m_Result = (int)TeamOperateResult.OR_Succeed;
                    matchResultMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(info.NodeName, matchResultMsg);
                    ///
                    dataProcess.RecordCampaignAction(user, type);
                }
            }

            LogSys.Log(LOG_TYPE.DEBUG, "Alloc lobby room for {0} users, roomid {1} scene {2}", users.Length, roomId, type);
        }

        internal void RequestStartGame(ulong guid)
        {
            UserInfo info = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (info != null && info.Room != null)
            {
                info.Room.RequestStartGame(info);
            }
        }

        internal void QuitRoom(ulong guid, bool is_quit_room)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                if (null != user.Room)
                {
                    user.Room.DelUsers(guid);
                    user.ResetRoomInfo();
                }
                if (user.CurrentState == UserState.Room)
                {
                    if (null != user.Group)
                    {
                        if (is_quit_room)
                        {
                            LobbyServer.Instance.MatchFormThread.QueueAction(LobbyServer.Instance.MatchFormThread.QuitGroup, user.Guid, user.Nickname);
                        }
                    }
                    user.CurrentState = UserState.Online;
                }
                else if (user.CurrentState == UserState.Pve)
                {
                    user.CurrentState = UserState.Online;
                }
                LogSys.Log(LOG_TYPE.INFO, "QuitRoom, guid:{0} state:{1}", guid, user.CurrentState);
            }
        }

        internal void QuitPve(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
                if (user.CurrentState == UserState.Pve)
                    user.CurrentState = UserState.Online;
        }

        internal void HandleBuyLife(ulong guid)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            if (null == scheduler)
                return;
            // 响应玩家要求复活
            UserInfo user = scheduler.GetUserInfo(guid);
            bool result = false;
            if (null != user)
            {
                int reliveStoneId = ItemConfigProvider.Instance.GetReliveStoneId();
                if (user.ItemBag.GetItemCount(reliveStoneId, 0) >= 1)
                {
                    scheduler.ConsumeItem(guid, user.ItemBag.GetItemData(reliveStoneId, 0), 1, GainItemType.Props, ConsumeItemWay.BuyLife, false, "BuyLife");
                    result = true;
                }
                else if (user.Gold >= 50)
                {
                    int consume = 50;
                    scheduler.ConsumeAsset(guid, consume, ConsumeAssetType.BuyLife, AssetType.Glod, "BuyLife");
                    result = true;
                }
                if (result)
                {
                    user.CurrentBattleInfo.DeadCount += 1;
                    if (null != user.Room)
                    {
                        Msg_LR_UserReLive.Builder resultBuilder = Msg_LR_UserReLive.CreateBuilder();
                        RoomInfo room = m_LobbyInfo.GetRoomByID(user.CurrentRoomID);
                        resultBuilder.SetUserGuid(guid);
                        resultBuilder.SetRoomID(user.CurrentRoomID);
                        LobbyServer.Instance.RoomSvrChannel.Send(room.RoomServerName, resultBuilder.Build());
                    }
                    JsonMessageWithGuid blrMsg = new JsonMessageWithGuid(JsonMessageID.BuyLifeResult);
                    blrMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_BuyLifeResult protoData = new ArkCrossEngineMessage.Msg_LC_BuyLifeResult();
                    protoData.m_Succeed = result;
                    protoData.m_CurDiamond = (int)user.Gold;
                    blrMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, blrMsg);
                }
                else
                {
                    JsonMessageWithGuid blrMsg = new JsonMessageWithGuid(JsonMessageID.BuyLifeResult);
                    blrMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_BuyLifeResult protoData = new ArkCrossEngineMessage.Msg_LC_BuyLifeResult();
                    protoData.m_Succeed = result;
                    protoData.m_CurDiamond = (int)user.Gold;
                    blrMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, blrMsg);
                }
            }
            //JsonMessageDispatcher.SendDcoreMessage(user.NodeName, );
        }

        //玩家重新登录的消息
        internal void OnUserRelogin(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user == null) { return; }
            if (user.CurrentState == UserState.Room)
            {
                RoomInfo room = m_LobbyInfo.GetRoomByID(user.CurrentRoomID);
                if (room != null)
                {
                    //向RoomServer发送消息，重新进入房间
                    Msg_LR_ReconnectUser.Builder urBuilder = Msg_LR_ReconnectUser.CreateBuilder();
                    urBuilder.SetUserGuid(guid);
                    urBuilder.SetRoomID(user.CurrentRoomID);
                    LobbyServer.Instance.RoomSvrChannel.Send(room.RoomServerName, urBuilder.Build());

                    LogSys.Log(LOG_TYPE.INFO, "User Restart GameClient , guid:{0}", guid);
                }
                else
                {
                    //房间已经关闭
                    user.CurrentState = UserState.Online;
                }
            }
        }
        internal void UpdateAttemptAward(ulong guid, int type, int killNpcCount)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user)
                return;
            Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(type);
            if (null == cfg)
                return;
            if ((int)SceneTypeEnum.TYPE_MULTI_PVE == cfg.m_Type
              || (int)SceneSubTypeEnum.TYPE_ATTEMPT == cfg.m_SubType)
            {
                if (killNpcCount > user.AttemptAward)
                {
                    user.AttemptAward = killNpcCount;
                }
            }
        }
        //===================================================================
        //以下为与RoomServer之间的消息通信 
        //响应RoomServer发来的房间创建反馈消息
        internal void OnReplyCreateBattleRoom(int roomId, bool isSuccess)
        {
            if (isSuccess)
            {
                m_LobbyInfo.StartBattleRoom(roomId);

                RoomInfo room = m_LobbyInfo.GetRoomByID(roomId);
                if (room == null) return;

                foreach (WeakReference info in room.Users.Values)
                {
                    UserInfo user = info.Target as UserInfo;
                    if (user != null)
                    {
                        user.CurrentState = UserState.Room;
                        user.CurrentRoomID = roomId;
                    }
                }
            }
            else
            {
                //玩家重新进入匹配队列
                RoomInfo room = m_LobbyInfo.GetRoomByID(roomId);
                if (room == null) return;

                LogSys.Log(LOG_TYPE.DEBUG, "StartGameResult, failed from RoomServer:{0},room id:{1}", room.RoomServerName, room.RoomId);

                foreach (WeakReference info in room.Users.Values)
                {
                    UserInfo user = info.Target as UserInfo;
                    if (user != null)
                    {
                        user.CurrentState = UserState.Online;
                        user.IsPrepared = false;
                        LobbyServer.Instance.MatchFormThread.QueueAction(LobbyServer.Instance.MatchFormThread.RequestMatch, user.Guid, room.SceneType);
                    }
                }
                m_LobbyInfo.StopBattleRoom(roomId);
            }
        }
        //响应RoomServer玩家重新连接进入房间的反馈消息
        internal void OnReplyReconnectUser(ulong userGuid, int roomID, bool isSuccess)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(userGuid);
            if (user == null) { return; }
            if (isSuccess)
            {
                user.CurrentState = UserState.Room;
                RoomInfo room = m_LobbyInfo.GetRoomByID(roomID);
                if (null != room)
                {
                    RoomServerInfo svrInfo;
                    if (m_LobbyInfo.RoomServerInfos.TryGetValue(room.RoomServerName, out svrInfo))
                    {
                        if (null != svrInfo)
                        {
                            JsonMessageWithGuid startGameResultMsg = new JsonMessageWithGuid(JsonMessageID.StartGameResult);
                            startGameResultMsg.m_Guid = user.Guid;
                            ArkCrossEngineMessage.Msg_LC_StartGameResult protoData = new ArkCrossEngineMessage.Msg_LC_StartGameResult();
                            protoData.server_ip = svrInfo.ServerIp;
                            protoData.server_port = svrInfo.ServerPort;
                            protoData.key = user.Key;
                            protoData.hero_id = user.HeroId;
                            protoData.camp_id = user.CampId;
                            protoData.scene_type = room.SceneType;
                            protoData.match_key = user.CurrentBattleInfo.MatchKey;
                            protoData.result = (int)GeneralOperationResult.LC_Succeed;

                            startGameResultMsg.m_ProtoData = protoData;
                            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, startGameResultMsg);
                            //重新进入房间成功
                            LogSys.Log(LOG_TYPE.INFO, "User Reconnected RoomServer Success ! , guid:{0}", userGuid);
                        }
                    }
                }
            }
            else
            {
                user.CurrentState = UserState.Online;

                LogSys.Log(LOG_TYPE.INFO, "User Reconnected RoomServer Failed ! , guid:{0}", userGuid);
            }
        }
        //响应RoomServer发来的房间内玩家掉线消息
        internal void OnRoomUserQuit(int roomid, ulong guid, bool isBattleEnd)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                if (isBattleEnd)
                {
                    user.CurrentRoomID = 0;
                    user.CurrentState = UserState.Room == user.CurrentState ? UserState.Online : user.CurrentState;
                }
                else
                {
                    user.CurrentRoomID = roomid;
                }
                LogSys.Log(LOG_TYPE.INFO, "RoomServer User Quit , guid:{0} state:{1} isEnd:{2}", guid, user.CurrentState, isBattleEnd);
            }
        }
        //响应RoomServer发来的战斗结束消息
        internal void OnRoomBattleEnd(Msg_RL_BattleEnd msg)
        {
            RoomInfo room = m_LobbyInfo.GetRoomByID(msg.RoomID);
            if (room != null)
            {
                //接收战斗数据
                if (msg.WinnerCamp == Msg_RL_BattleEnd.Types.WinnerCampEnum.Red)
                {
                    room.WinnerCamp = CampIdEnum.Red;
                }
                else if (msg.WinnerCamp == Msg_RL_BattleEnd.Types.WinnerCampEnum.Blue)
                {
                    room.WinnerCamp = CampIdEnum.Blue;
                }
                else
                {
                    room.WinnerCamp = CampIdEnum.Unkown;
                }
                int money_ct = 0;
                List<Teammate> teammate = new List<Teammate>();
                foreach (var ubrMsg in msg.UserBattleInfosList)
                {
                    UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(ubrMsg.UserGuid);
                    if (user != null)
                    {
                        Teammate e = new Teammate();
                        e.Nick = user.Nickname;
                        e.ResId = user.HeroId;
                        e.Money = ubrMsg.Money;
                        money_ct += ubrMsg.Money;
                        teammate.Add(e);
                    }
                }
                List<UserInfo> users = new List<UserInfo>();
                foreach (var ubrMsg in msg.UserBattleInfosList)
                {
                    UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(ubrMsg.UserGuid);
                    if (user != null)
                    {
                        if (ubrMsg.BattleResult == Msg_RL_UserBattleInfo.Types.BattleResultEnum.Win)
                        {
                            user.CurrentBattleInfo.BattleResult = BattleResultEnum.Win;
                        }
                        else if (ubrMsg.BattleResult == Msg_RL_UserBattleInfo.Types.BattleResultEnum.Lost)
                        {
                            user.CurrentBattleInfo.BattleResult = BattleResultEnum.Lost;
                        }
                        else
                        {
                            user.CurrentBattleInfo.BattleResult = BattleResultEnum.Unfinish;
                        }
                        user.CurrentBattleInfo.TeamMate.AddRange(teammate);
                        if (ubrMsg.HasKillNpcCount)
                        {
                            user.CurrentBattleInfo.KillNpcCount = ubrMsg.KillNpcCount;
                            UpdateAttemptAward(ubrMsg.UserGuid, room.SceneType, ubrMsg.KillNpcCount);
                        }
                        user.CurrentBattleInfo.AddGold = ubrMsg.Money;
                        user.CurrentBattleInfo.TotalGold = money_ct;
                        user.CurrentBattleInfo.HitCount = ubrMsg.HitCount;
                        user.CurrentBattleInfo.MaxMultiHitCount = ubrMsg.MaxMultiHitCount;
                        user.CurrentBattleInfo.TotalDamageToMyself = ubrMsg.TotalDamageToMyself;
                        user.CurrentBattleInfo.TotalDamageFromMyself = ubrMsg.TotalDamageFromMyself;
                        users.Add(user);
                    }
                }
                //处理战斗结果
                room.ProcessBattleResult(users);
                //结束战斗        
                room.EndBattle();
            }
        }

        protected override void OnStart()
        {
            TickSleepTime = 10;
        }
        protected override void OnTick()
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (m_LastLogTime + 60000 < curTime)
            {
                m_LastLogTime = curTime;

                DebugPoolCount((string msg) =>
                {
                    LogSys.Log(LOG_TYPE.INFO, "RoomProcessThread.ActionQueue {0}", msg);
                });
                int gameRoomCount = 0;
                int gameUserCount = 0;
                foreach (var room in m_LobbyInfo.Rooms.Values)
                {
                    if (room.CurrentState == RoomState.Game)
                    {
                        gameRoomCount++;
                        gameUserCount += room.UserCount;
                    }
                }
                LogSys.Log(LOG_TYPE.WARN, "Lobby Game Room Count:{0}, GameUserCount:{1}, MatchUserCount:{2}", gameRoomCount, gameUserCount, LobbyServer.Instance.MatchFormThread.MatchUserCount);
            }

            m_LobbyInfo.Tick();
        }

        private const int c_MaxMemberCount = 5;
        private LobbyInfo m_LobbyInfo = new LobbyInfo();

        private long m_LastLogTime = 0;
    }
}

