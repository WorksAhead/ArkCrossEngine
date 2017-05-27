using System;
using System.Collections.Generic;
using DashFire;
using DashFire.DataStore;
using ArkCrossEngine;

namespace Lobby
{
    /// <summary>
    /// 全局数据（非玩家拥有的数据）处理线程，在这里处理邮件、远征、帮会等数据。
    /// </summary>
    /// <remarks>
    /// 其它线程不应直接调用此类方法，应通过QueueAction发起调用。
    /// </remarks>
    internal sealed class GlobalDataProcessThread : MyServerThread
    {
        //=========================================================================================
        //全局数据初始化方法，由其它线程同步调用。
        //=========================================================================================
        internal bool LastSaveFinished
        {
            get { return m_LastSaveFinished; }
        }
        internal ulong GenerateUserGuid()
        {
            return m_GuidSystem.GenerateUserGuid();
        }
        internal ulong GenerateMailGuid()
        {
            return m_GuidSystem.GenerateMailGuid();
        }
        internal void InitGuidData(List<GuidInfo> guidList)
        {
            m_GuidSystem.InitGuidData(guidList);
        }
        internal void InitMailData(List<MailInfo> mailList)
        {
            m_MailSystem.InitMailData(mailList);
        }
        internal void InitGowStarData(List<GowStarInfo> gowstarList)
        {
            m_GowSystem.InitGowStarData(gowstarList);
        }
        internal void InitGiftCodeData(List<GiftCodeInfo> giftcodeList)
        {
            m_GiftCodeSystem.InitGiftCodeData(giftcodeList);
        }
        internal void InitArenaData(List<ArenaInfo> arenaList)
        {
            m_ArenaSystem.InitArenaData(arenaList);
        }
        internal void InitChallengeData(List<Tuple<ulong, ChallengeInfo>> challengeInfoList)
        {
            m_ArenaSystem.InitChallengeData(challengeInfoList);
        }
        //=========================================================================================
        //同步调用方法部分，其它线程可直接调用(需要考虑多线程安全)。
        //=========================================================================================
        internal int GetLogicServerUserCount(int server_id)
        {
            return m_LogicServerMgr.GetLogicServerUserCount(server_id);
        }
        //=========================================================================================
        //异步调用方法部分，需要通过QueueAction调用。
        //=========================================================================================
        internal void GetMailList(ulong user)
        {
            m_MailSystem.GetMailList(user);
        }
        internal void SendUserMail(MailInfo userMail, int validityPeriod)
        {
            m_MailSystem.SendUserMail(userMail, validityPeriod);
        }
        internal void SendWholeMail(MailInfo wholeMail, int validityPeriod)
        {
            m_MailSystem.SendWholeMail(wholeMail, validityPeriod);
        }
        internal void SendModuleMail(ModuleMailInfo moduleMail, int validityPeriod)
        {
            m_MailSystem.SendModuleMail(moduleMail, validityPeriod);
        }
        internal void ReadMail(ulong userGuid, ulong mailGuid)
        {
            m_MailSystem.ReadMail(userGuid, mailGuid);
        }
        internal void ReceiveMail(ulong userGuid, ulong mailGuid)
        {
            m_MailSystem.ReceiveMail(userGuid, mailGuid);
        }

        internal void UpdateGowElo(UserInfo win, UserInfo lost)
        {
            m_GowSystem.UpdateElo(win);
            m_GowSystem.UpdateElo(lost);
        }
        internal void GetGowStarList(ulong guid, int start, int count)
        {
            m_GowSystem.GetStarList(guid, start, count);
        }
        internal void AddUserToLogicServer(int server_id, ulong user_guid)
        {
            m_LogicServerMgr.AddUserToLogicServer(server_id, user_guid);
        }
        internal void DelUserFromLogicServer(int server_id, ulong user_guid)
        {
            m_LogicServerMgr.DelUserFromLogicServer(server_id, user_guid);
        }
        internal void HandleExchangeGift(ulong userGuid, string giftcode)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(userGuid);
            if (user == null)
            {
                return;
            }
            int giftId = 0;
            GeneralOperationResult ret = m_GiftCodeSystem.ExchangeGift(userGuid, giftcode, out giftId);
            JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.ExchangeGiftResult);
            jsonMsg.m_Guid = userGuid;
            ArkCrossEngineMessage.Msg_LC_ExchangeGiftResult protoData = new ArkCrossEngineMessage.Msg_LC_ExchangeGiftResult();
            protoData.m_GiftId = giftId;
            protoData.m_Result = (int)ret;
            jsonMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
        }

        protected override void OnStart()
        {
            TickSleepTime = 10;
            m_LogicServerMgr.InitLogicServer();
            m_ActivitySystem.Init();
            m_GowSystem.Init(m_MailSystem);
            m_ArenaSystem.Init(m_MailSystem);
            //m_ArenaSystem.Check();
            //m_ArenaSystem.Test();
        }
        protected override void OnTick()
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (m_LastLogTime + 60000 < curTime)
            {
                m_LastLogTime = curTime;

                DebugPoolCount((string msg) =>
                {
                    LogSys.Log(LOG_TYPE.INFO, "GlobalDataProcessThread.ActionQueue {0}", msg);
                });
            }

            m_ActivitySystem.Tick();
            m_MailSystem.Tick();
            m_GowSystem.Tick();
            m_ArenaSystem.Tick();

            //全局数据存储
            var dsThread = LobbyServer.Instance.DataStoreThread;
            if (dsThread.DataStoreAvailable)
            {
                if (curTime - m_LastGuidSaveTime > s_GuidSaveInterval && m_NextGuidSaveCount != 0)
                {
                    dsThread.DSGSaveGuid(m_GuidSystem.GuidList, m_NextGuidSaveCount);
                    m_LastGuidSaveTime = curTime;
                    m_NextGuidSaveCount++;
                }
                if (curTime - m_LastMailSaveTime > s_MailSaveInterval && m_NextMailSaveCount != 0)
                {
                    dsThread.DSGSaveMail(m_MailSystem.TotalMailList, m_NextMailSaveCount);
                    m_LastMailSaveTime = curTime;
                    m_NextMailSaveCount++;
                }
                if (curTime - m_LastGowstarSaveTime > s_GowstarSaveInterval && m_NextGowstarSaveCount != 0)
                {
                    dsThread.DSGSaveGowStar(m_GowSystem.GowStarList, m_NextGowstarSaveCount);
                    m_LastGowstarSaveTime = curTime;
                    m_NextGowstarSaveCount++;
                }
                if (curTime - m_LastArenaRankSaveTime > s_ArenaRankSaveInterval && m_NextArenaRankSaveCount != 0)
                {
                    m_CurrentArenaRankSaveCountList.Clear();
                    int pieceCount = dsThread.DSGSaveArenaRank(m_ArenaSystem.ArenaRankList, m_NextArenaRankSaveCount);
                    m_CurrentArenaRankSaveCountList = new List<long>(pieceCount);
                    for (int i = 0; i < pieceCount; ++i)
                    {
                        m_CurrentArenaRankSaveCountList.Add(-1);
                    }
                    m_LastArenaRankSaveTime = curTime;
                    m_NextArenaRankSaveCount++;
                }
                if (curTime - m_LastArenaRecordSaveTime > s_ArenaRecordSaveInterval && m_NextArenaRecordSaveCount != 0)
                {
                    m_CurrentArenaRecordSaveCountList.Clear();
                    var challengeHistory = m_ArenaSystem.ArenaChallengeHistory;
                    Dictionary<ulong, int> userRankDict = new Dictionary<ulong, int>();
                    foreach (var userGuid in challengeHistory.Keys)
                    {
                        ArenaInfo ai = m_ArenaSystem.GetArenaInfoById(userGuid);
                        userRankDict.Add(userGuid, ai.GetRank());
                    }
                    int pieceCount = dsThread.DSGSaveArenaRecord(challengeHistory, userRankDict, m_NextArenaRecordSaveCount);
                    m_CurrentArenaRecordSaveCountList = new List<long>(pieceCount);
                    for (int i = 0; i < pieceCount; ++i)
                    {
                        m_CurrentArenaRecordSaveCountList.Add(-1);
                    }
                    m_LastArenaRecordSaveTime = curTime;
                    m_NextArenaRecordSaveCount++;
                }
                if (m_CurrentGuidSaveCount == 0 && m_CurrentMailSaveCount == 0 && m_CurrentGowstarSaveCount == 0
                  && CheckLastSaveArenaRankDone() && CheckLastSaveArenaRecordDone())
                {
                    if (m_LastSaveFinished == false)
                    {
                        //全局数据（Guid、战神赛排行榜，邮件，竞技场排行榜，竞技场战斗记录）存储完成 
                        LogSys.Log(LOG_TYPE.MONITOR, "GlobalDataProcessThread DoLastSaveGlobalData Done!");
                        m_LastSaveFinished = true;
                    }
                }
            }
        }
        protected override void OnQuit()
        {
        }
        internal void HandleRequestExpeditionInfo(ulong guid, int hp, int mp, int rage, int request_num, bool is_reset, bool allow_cost_gold, long timestamp)
        {
            long c_DeltaTime = 60 * 10;
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && null != user.Expedition)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                long dif_time = Convert.ToInt64(TimeUtility.CurTimestamp) - timestamp;
                bool is_valid_timestamp = Math.Abs(dif_time) <= c_DeltaTime ? true : false;
                if (is_reset && !is_valid_timestamp)
                {
                    result = GeneralOperationResult.LC_Failure_Time;
                }
                else
                {
                    result = user.Expedition.RequestExpeditionInfo(guid, request_num, is_reset, m_UsableImages);
                }
                if (GeneralOperationResult.LC_Succeed == result)
                {
                    user.Expedition.SyncExpeditionInfo(guid, hp, mp, rage, request_num, result);
                }
                else
                {
                    JsonMessageWithGuid resetMsg = new JsonMessageWithGuid(JsonMessageID.ExpeditionResetResult);
                    resetMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult protoData = new ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult();
                    protoData.m_Result = (int)result;
                    resetMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, resetMsg);
                }
            }
        }

        internal void HandleQueryArenaInfo(ulong guid)
        {
            LogSys.Log(LOG_TYPE.DEBUG, "---got query arena info msg! id=" + guid);
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user == null)
            {
                return;
            }
            JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.ArenaInfoResult);
            resultMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_ArenaInfoResult protoData = new ArkCrossEngineMessage.Msg_LC_ArenaInfoResult();
            ArenaInfo own_arena = m_ArenaSystem.GetArenaInfoById(guid);
            if (own_arena == null)
            {
                own_arena = m_ArenaSystem.CreateArenaInfo(user);
            }
            else if (own_arena.IsNeedUpdate(user))
            {
                own_arena.UpdateArenaInfo(user);
            }
            m_ArenaSystem.ResetArenaFightCount(own_arena);
            protoData.m_ArenaInfo = ArenaUtil.CreateArenaInfoMsg(own_arena, false);
            protoData.m_LeftBattleCount = own_arena.LeftFightCount;
            protoData.m_CurFightCountByTime = own_arena.FightCountBuyTime;
            long passed_time = (long)(DateTime.Now - own_arena.LastBattleTime).TotalMilliseconds;
            protoData.m_BattleLeftCDTime = m_ArenaSystem.BaseConfig.BattleCd - passed_time;
            resultMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, resultMsg);
            LogSys.Log(LOG_TYPE.DEBUG, "---send query arena info msg! id=" + guid);
        }

        internal void HandleQueryArenaMatchGroup(ulong guid)
        {
            LogSys.Log(LOG_TYPE.DEBUG, "---got query match group msg! id=" + guid);
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user == null)
            {
                return;
            }
            JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.ArenaMatchGroupResult);
            resultMsg.m_Guid = guid;
            int cur_user_rank = ArenaSystem.UNKNOWN_RANK;
            ArenaInfo arena_info = m_ArenaSystem.GetArenaInfoById(guid);
            if (arena_info != null)
            {
                cur_user_rank = arena_info.GetRank();
            };
            List<MatchGroup> match_groups = m_ArenaSystem.QueryMatchArena(cur_user_rank, 3);
            ArkCrossEngineMessage.Msg_LC_ArenaMatchGroupResult protoData = new ArkCrossEngineMessage.Msg_LC_ArenaMatchGroupResult();
            foreach (MatchGroup group in match_groups)
            {
                ArkCrossEngineMessage.Msg_LC_ArenaMatchGroupResult.MatchGroupData group_msg = new ArkCrossEngineMessage.Msg_LC_ArenaMatchGroupResult.MatchGroupData();
                group_msg.One = ArenaUtil.CreateArenaInfoMsg(group.One);
                group_msg.Two = ArenaUtil.CreateArenaInfoMsg(group.Two);
                group_msg.Three = ArenaUtil.CreateArenaInfoMsg(group.Three);
                if (group_msg.One == null || group_msg.Two == null || group_msg.Three == null)
                {
                    continue;
                }
                protoData.m_MatchGroups.Add(group_msg);
            }
            resultMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, resultMsg);
            LogSys.Log(LOG_TYPE.DEBUG, "---send match group msg! id=" + guid);
        }

        internal void HandleArenaStartChallenge(ulong guid, ulong target_guid)
        {
            LogSys.Log(LOG_TYPE.DEBUG, "---got start challenge result msg! id=" + guid);
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user == null)
            {
                return;
            }
            ArenaInfo target_arena_info = m_ArenaSystem.GetArenaInfoById(target_guid);

            JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.ArenaStartChallengeResult);
            resultMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_ArenaStartCallengeResult protoData = new ArkCrossEngineMessage.Msg_LC_ArenaStartCallengeResult();
            protoData.m_TargetGuid = target_guid;
            protoData.m_Sign = new Random().Next();
            if (target_arena_info != null && target_arena_info.IsNeedUpdate(user))
            {
                target_arena_info.UpdateArenaInfo(user);
            }
            bool start_ret = m_ArenaSystem.StartChallenge(guid, target_guid, protoData.m_Sign);
            if (target_arena_info == null)
            {
                protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Arena_NotFindTarget;
            }
            else if (!start_ret)
            {
                protoData.m_ResultCode = (int)m_ArenaSystem.ErrorCode;
            }
            else
            {
                user.UpdateGuideFlag((int)MatchSceneEnum.Arena);
                user.CurrentState = UserState.Pve;
                protoData.m_ResultCode = (int)GeneralOperationResult.LC_Succeed;
                ChallengeInfo challenge = m_ArenaSystem.GetDoingChallengeInfo(guid);
                if (challenge != null && challenge.Target != null)
                {
                    float coefficent = ChallengeChecker.CalcPvpCoefficient(user.Level, target_arena_info.Level);
                    int total_hp = ChallengeChecker.CalcPlayerHp(target_arena_info, coefficent);
                    challenge.Target.TotalHp = total_hp;
                    //LogSys.Log(LOG_TYPE.DEBUG, "challenge target totalhp={0}", total_hp);
                }
            }
            resultMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, resultMsg);
            LogSys.Log(LOG_TYPE.DEBUG, "---send start challenge result msg! id=" + guid);
        }

        internal void HandleArenaChallengeOver(ulong guid, ArkCrossEngineMessage.Msg_CL_ArenaChallengeOver protoMsg)
        {
            ChallengeInfo challeng_info = m_ArenaSystem.GetDoingChallengeInfo(guid);
            bool is_success = protoMsg.IsSuccess;
            if (challeng_info != null)
            {
                int challenger_total_damange = 0;
                challeng_info.Challenger.UserDamage = protoMsg.ChallengerDamage;
                challenger_total_damange = challeng_info.Challenger.UserDamage;
                challeng_info.Target.UserDamage = protoMsg.TargetDamage;
                foreach (var m in protoMsg.ChallengerPartnerDamage)
                {
                    DamageInfo d = ArenaUtil.GetDamageInfo(challeng_info.Challenger.PartnerDamage, m.OwnerId);
                    if (d != null)
                    {
                        d.Damage = m.Damage;
                        challenger_total_damange += d.Damage;
                    }
                    else
                    {
                        is_success = false;
                    }
                }
                foreach (var m in protoMsg.TargetPartnerDamage)
                {
                    DamageInfo d = ArenaUtil.GetDamageInfo(challeng_info.Target.PartnerDamage, m.OwnerId);
                    if (d != null)
                    {
                        d.Damage = m.Damage;
                    }
                    else
                    {
                        is_success = false;
                    }
                }
                if (is_success && challenger_total_damange < challeng_info.Target.TotalHp)
                {
                    LogSys.Log(LOG_TYPE.WARN, "ArenaChallenge result not correct, challenger {0} total damage {1} less than total hp {2}",
                               guid, challenger_total_damange, challeng_info.Target.TotalHp);
                    is_success = false;
                }
                int verify_sign = challeng_info.Sign - (int)challeng_info.Target.Guid;
                if (protoMsg.Sign != verify_sign)
                {
                    is_success = false;
                    LogSys.Log(LOG_TYPE.INFO, "ArenaChallenge Sign Verify Failed: guid{0}, rank{1}, target {2}-{3}",
                               guid, challeng_info.Challenger.Rank, challeng_info.Target.Guid, challeng_info.Target.Rank);
                }
            }
            m_ArenaSystem.ChallengeOver(guid, is_success);

            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                user.CurrentState = UserState.Online;
                StageClearInfo info = new StageClearInfo();
                info.SceneId = 3002;
                MissionSystem.Instance.CheckAndSyncMissions(user, info);
            }
        }

        internal void HandleArenaQueryRank(ulong guid, int handle)
        {
            JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.ArenaQueryRankResult);
            resultMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_ArenaQueryRankResult protoData = new ArkCrossEngineMessage.Msg_LC_ArenaQueryRankResult();
            ArenaInfo cur_info = m_ArenaSystem.GetArenaInfoById(guid);
            if (cur_info != null)
            {
                List<ArenaInfo> result = m_ArenaSystem.QueryRankList(cur_info.GetRank());
                for (int i = 0; i < result.Count; i++)
                {
                    protoData.RankMsg.Add(ArenaUtil.CreateArenaInfoMsg(result[i]));
                }
            }
            resultMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(handle, resultMsg);
            LogSys.Log(LOG_TYPE.DEBUG, "send query rank list!");
        }

        internal void HandleArenaChangePartners(ulong guid, int handle, List<int> partners)
        {
            //TODO: refresh user info to arenainfo
            JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.ArenaChangePartnersResult);
            resultMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_ArenaChangePartnerResult protoData = new ArkCrossEngineMessage.Msg_LC_ArenaChangePartnerResult();
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user == null)
            {
                protoData.Result = (int)GeneralOperationResult.LC_Failure_NotFinduser;
            }
            else
            {
                ArenaInfo cur_info = m_ArenaSystem.GetArenaInfoById(guid);
                int count = m_ArenaSystem.GetMaxPartnerCount(user.Level);
                if (partners.Count > count)
                {
                    protoData.Result = (int)GeneralOperationResult.LC_Failure_LevelError;
                }
                else
                {
                    cur_info.FightPartners.Clear();
                    for (int i = 0; i < partners.Count; i++)
                    {
                        int partnerid = partners[i];
                        PartnerInfo partner = user.PartnerStateInfo.GetPartnerInfoById(partnerid);
                        cur_info.FightPartners.Add(partner);
                        protoData.Partners.Add(partnerid);
                    }
                    protoData.Result = (int)GeneralOperationResult.LC_Succeed;
                }
            }
            resultMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(handle, resultMsg);
            LogSys.Log(LOG_TYPE.DEBUG, "send change partners result!");
        }

        internal void HandleArenaQueryHistory(ulong guid, int handle)
        {
            LogSys.Log(LOG_TYPE.DEBUG, "--handle arena query history!");
            JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.ArenaQueryHistoryResult);
            resultMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_ArenaQueryHistoryResult protoData = new ArkCrossEngineMessage.Msg_LC_ArenaQueryHistoryResult();
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            List<ChallengeInfo> history = m_ArenaSystem.QueryHistory(guid);
            if (history != null)
            {
                foreach (ChallengeInfo info in history)
                {
                    ArkCrossEngineMessage.ChallengeInfoData info_data = ArenaUtil.CreateChallengeInfoData(info);
                    protoData.ChallengeHistory.Add(info_data);
                }
            }
            resultMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(handle, resultMsg);
            LogSys.Log(LOG_TYPE.DEBUG, "--send arena query history result!");
        }

        internal void HandleArenaBuyFightCount(ulong guid, int handle)
        {
            LogSys.Log(LOG_TYPE.DEBUG, "--handle arena buy fight count!");
            JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.ArenaBuyFightCountResult);
            resultMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_ArenaBuyFightCountResult protoData = new ArkCrossEngineMessage.Msg_LC_ArenaBuyFightCountResult();
            ArenaInfo own_arena = m_ArenaSystem.GetArenaInfoById(guid);
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            protoData.CurBuyTime = 0;
            protoData.CurFightCount = 0;
            protoData.Result = (int)GeneralOperationResult.LC_Failure_Arena_NotFindTarget;
            if (own_arena != null && user != null && scheduler != null)
            {
                ArenaBuyFightCountConfig buy_config = ArenaConfigProvider.Instance.BuyFightCountConfig.GetDataById(own_arena.FightCountBuyTime + 1);
                if (user.Vip >= buy_config.RequireVipLevel && user.Gold >= buy_config.Cost)
                {
                    if (own_arena.LeftFightCount < own_arena.MaxFightCount)
                    {
                        own_arena.FightCountBuyTime += 1;
                        own_arena.LeftFightCount = own_arena.MaxFightCount;
                        protoData.Result = (int)GeneralOperationResult.LC_Succeed;
                    }
                    else
                    {
                        protoData.Result = (int)GeneralOperationResult.LC_Failure_Full;
                    }
                    protoData.CurBuyTime = own_arena.FightCountBuyTime;
                    protoData.CurFightCount = own_arena.LeftFightCount;
                }
            }
            resultMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(handle, resultMsg);
            LogSys.Log(LOG_TYPE.DEBUG, "--send arena buy fight count result!");
        }

        public void HandleArenaBeginFight(ulong guid, int handle)
        {
            m_ArenaSystem.BeginFight(guid);
        }

        internal bool AddUserToImages(int score, UserInfo user)
        {
            if (null != user)
            {
                while (m_UsableImages.ContainsKey(score))
                {
                    ++score;
                }
                ExpeditionImageInfo image_info = new ExpeditionImageInfo(user);
                image_info.FightingScore = score;
                m_UsableImages.Add(score, image_info);
            }
            return true;
        }
        internal void AddUserToImages(UserInfo user)
        {
            if (null != user && null != m_UsableImages)
            {
                int del_key = -1;
                foreach (ExpeditionImageInfo value in m_UsableImages.Values)
                {
                    if (value.Guid == user.Guid)
                    {
                        del_key = value.FightingScore;
                    }
                }
                if (del_key >= 0)
                {
                    m_UsableImages.Remove(del_key);
                }
                AddUserToImages(user.FightingScore * 10000, user);
            }
        }
        internal void LoadUserArena(ulong userGuid)
        {
            ArenaInfo arenaInfo = m_ArenaSystem.GetArenaInfoById(userGuid);
            if (arenaInfo == null)
            {
                //玩家未上榜，从数据库中读取数据
                var dsThread = LobbyServer.Instance.DataStoreThread;
                if (dsThread.DataStoreAvailable)
                {
                    dsThread.DSPLoadUserArena(userGuid, (DataStoreThread.DSPLoadUserArenaCB)((ret, ai, arenaRecordList) =>
                    {
                        if (ret == DSLoadResult.Success)
                        {
                            m_ArenaSystem.AddArenaInfo(ai);
                            m_ArenaSystem.AddUserArenaChallenge(userGuid, arenaRecordList);
                        }
                    }));
                }
            }
        }
        internal void SaveUserArena(ulong userGuid)
        {
            ArenaInfo arenaInfo = m_ArenaSystem.GetArenaInfoById(userGuid);
            if (arenaInfo != null && arenaInfo.GetRank() == ArenaSystem.UNKNOWN_RANK)
            {
                var dsThread = LobbyServer.Instance.DataStoreThread;
                if (dsThread.DataStoreAvailable)
                {
                    List<ChallengeInfo> arenaRecordList = m_ArenaSystem.QueryHistory(userGuid);
                    dsThread.DSPSaveUserArena(arenaInfo, arenaRecordList);
                    m_ArenaSystem.RemoveUnRankEntity(userGuid);
                }
            }
        }

        internal void SetCurrentGuidSaveCount(long saveCount)
        {
            if (m_NextGuidSaveCount > 0)
            {
                if (m_CurrentGuidSaveCount < saveCount && m_CurrentGuidSaveCount != 0)
                {
                    m_CurrentGuidSaveCount = saveCount;
                }
            }
            else
            {
                m_CurrentGuidSaveCount = saveCount;
            }
        }
        internal void SetCurrentMailSaveCount(long saveCount)
        {
            if (m_NextMailSaveCount > 0)
            {
                if (m_CurrentMailSaveCount < saveCount && m_CurrentMailSaveCount != 0)
                {
                    m_CurrentMailSaveCount = saveCount;
                }
            }
            else
            {
                m_CurrentMailSaveCount = saveCount;
            }
        }
        internal void SetCurrentGowstarSaveCount(long saveCount)
        {
            if (m_NextGowstarSaveCount > 0)
            {
                if (m_CurrentGowstarSaveCount < saveCount && m_CurrentGowstarSaveCount != 0)
                {
                    m_CurrentGowstarSaveCount = saveCount;
                }
            }
            else
            {
                m_CurrentGowstarSaveCount = saveCount;
            }
        }
        internal void SetCurrentArenaRankSaveCount(long saveCount, int pieceNumber)
        {
            if (pieceNumber >= 0 && pieceNumber < m_CurrentArenaRankSaveCountList.Count)
            {
                if (m_NextArenaRankSaveCount > 0)
                {
                    if (m_CurrentArenaRankSaveCountList[pieceNumber] < saveCount && m_CurrentArenaRankSaveCountList[pieceNumber] != 0)
                    {
                        m_CurrentArenaRankSaveCountList[pieceNumber] = saveCount;
                    }
                }
                else
                {
                    m_CurrentArenaRankSaveCountList[pieceNumber] = saveCount;
                }
            }
        }
        private bool CheckLastSaveArenaRankDone()
        {
            foreach (int saveCount in m_CurrentArenaRankSaveCountList)
            {
                if (saveCount != 0)
                {
                    return false;
                }
            }
            return true;
        }
        internal void SetCurrentArenaRecordSaveCount(long saveCount, int pieceNumber)
        {
            if (pieceNumber >= 0 && pieceNumber < m_CurrentArenaRecordSaveCountList.Count)
            {
                if (m_NextArenaRecordSaveCount > 0)
                {
                    if (m_CurrentArenaRecordSaveCountList[pieceNumber] < saveCount && m_CurrentArenaRecordSaveCountList[pieceNumber] != 0)
                    {
                        m_CurrentArenaRecordSaveCountList[pieceNumber] = saveCount;
                    }
                }
                else
                {
                    m_CurrentArenaRecordSaveCountList[pieceNumber] = saveCount;
                }
            }
        }
        private bool CheckLastSaveArenaRecordDone()
        {
            foreach (int saveCount in m_CurrentArenaRecordSaveCountList)
            {
                if (saveCount != 0)
                {
                    return false;
                }
            }
            return true;
        }
        internal void DoLastSaveGlobalData()
        {
            m_LastSaveFinished = false;
            m_NextGuidSaveCount = 0;
            m_NextMailSaveCount = 0;
            m_NextGowstarSaveCount = 0;
            m_NextArenaRankSaveCount = 0;
            m_NextArenaRecordSaveCount = 0;
            var dsThread = LobbyServer.Instance.DataStoreThread;
            if (dsThread.DataStoreAvailable)
            {
                dsThread.DSGSaveGuid(m_GuidSystem.GuidList, m_NextGuidSaveCount);
                dsThread.DSGSaveGowStar(m_GowSystem.GowStarList, m_NextGowstarSaveCount);
                dsThread.DSGSaveMail(m_MailSystem.TotalMailList, m_NextMailSaveCount);
                m_CurrentArenaRankSaveCountList.Clear();
                int rankPieceCount = dsThread.DSGSaveArenaRank(m_ArenaSystem.ArenaRankList, m_NextArenaRankSaveCount);
                m_CurrentArenaRankSaveCountList = new List<long>(rankPieceCount);
                for (int i = 0; i < rankPieceCount; ++i)
                {
                    m_CurrentArenaRankSaveCountList.Add(-1);
                }
                m_CurrentArenaRecordSaveCountList.Clear();
                var challengeHistory = m_ArenaSystem.ArenaChallengeHistory;
                Dictionary<ulong, int> userRankDict = new Dictionary<ulong, int>();
                foreach (var userGuid in challengeHistory.Keys)
                {
                    ArenaInfo ai = m_ArenaSystem.GetArenaInfoById(userGuid);
                    userRankDict.Add(userGuid, ai.GetRank());
                }
                int recordPieceCount = dsThread.DSGSaveArenaRecord(challengeHistory, userRankDict, m_NextArenaRecordSaveCount);
                m_CurrentArenaRecordSaveCountList = new List<long>(recordPieceCount);
                for (int i = 0; i < recordPieceCount; ++i)
                {
                    m_CurrentArenaRecordSaveCountList.Add(-1);
                }
            }
        }
        internal void HandleRequestDare(ulong userGuid, string targetNickname)
        {
            m_DareSystem.NotifyRequestDare(userGuid, targetNickname);
        }
        internal void HandleAcceptedDare(ulong guid, string challenger)
        {
            m_DareSystem.HandleAcceptedDare(guid, challenger);
        }
        internal void HandleRequestGowPrize(ulong guid)
        {
            m_GowSystem.HandleRequestGowPrize(guid);
        }

        private GuidSystem m_GuidSystem = new GuidSystem();
        private MailSystem m_MailSystem = new MailSystem();
        private GowSystem m_GowSystem = new GowSystem();
        private ActivitySystem m_ActivitySystem = new ActivitySystem();
        private GiftCodeSystem m_GiftCodeSystem = new GiftCodeSystem();
        private ArenaSystem m_ArenaSystem = new ArenaSystem();
        private DareSystem m_DareSystem = new DareSystem();
        private LogicServerManager m_LogicServerMgr = new LogicServerManager();
        private SortedDictionary<int, ExpeditionImageInfo> m_UsableImages = new SortedDictionary<int, ExpeditionImageInfo>();
        //Guid数据存储参数
        private long m_NextGuidSaveCount = 1;
        private long m_CurrentGuidSaveCount = -1;
        private static long s_GuidSaveInterval = 120000;      //存储时间间隔：2min      
        private long m_LastGuidSaveTime = 0;
        //邮件数据存储参数
        private long m_NextMailSaveCount = 1;
        private long m_CurrentMailSaveCount = -1;
        private static long s_MailSaveInterval = 150000;      //存储时间间隔：2.5min
        private long m_LastMailSaveTime = 0;
        //战神赛排行榜存储参数
        private long m_NextGowstarSaveCount = 1;
        private long m_CurrentGowstarSaveCount = -1;
        private static long s_GowstarSaveInterval = 150000;   //存储时间间隔：2.5min
        private long m_LastGowstarSaveTime = 0;
        //竞技场排行榜存储参数
        private long m_NextArenaRankSaveCount = 1;
        private List<long> m_CurrentArenaRankSaveCountList = new List<long>();
        private static long s_ArenaRankSaveInterval = 180000; //存储时间间隔：3min
        private long m_LastArenaRankSaveTime = 0;
        //竞技场战斗记录存储参数
        private long m_NextArenaRecordSaveCount = 1;
        private List<long> m_CurrentArenaRecordSaveCountList = new List<long>();
        private static long s_ArenaRecordSaveInterval = 180000;//存储时间间隔：3min
        private long m_LastArenaRecordSaveTime = 0;

        private long m_LastLogTime = 0;
        private bool m_LastSaveFinished = false;      //最后一次数据存储标识
    }
}
