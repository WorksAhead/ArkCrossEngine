using System;
using System.Collections.Generic;
using ArkCrossEngine;

namespace Lobby
{
    internal class ArenaSystem
    {
        internal static int UNLOCK_LEVEL = 1;
        internal static int UNKNOWN_RANK = -1;
        internal const int PRIZE_RETAIN_DAYS = 7;
        internal GeneralOperationResult ErrorCode = GeneralOperationResult.LC_Succeed;
        internal static int CHALLENGE_CD_DEVIATION_MS = 5000;
        internal static int FIGHT_TIME_DEVIATION_MS = 10000;
        internal static int DEFAULT_READY_FIGHT_MS = 10000;
        internal ArkCrossEngine.ArenaBaseConfig BaseConfig
        {
            get { return m_BaseConfig; }
        }

        internal bool Init(MailSystem mailsystem)
        {
            m_BaseConfig = ArenaConfigProvider.Instance.BaseConfig.GetDataById(1);
            if (m_BaseConfig == null)
            {
                return false;
            }
            m_MaxParterLimit = m_BaseConfig.MaxParterLimit;
            m_MailSystem = mailsystem;
            ArenaInfo.PrizeTime = m_BaseConfig.PrizeSettlementTime;
            return true;
        }
        internal void InitArenaData(List<ArenaInfo> arenaList)
        {
            m_ArenaRank = new Rank<ArenaInfo>(m_BaseConfig.MaxRank);
            if (arenaList.Count > 0)
            {
                foreach (var arenaInfo in arenaList)
                {
                    arenaInfo.BattleCD = m_BaseConfig.BattleCd;
                    arenaInfo.MaxFightCount = m_BaseConfig.MaxBattleCount;
                    m_ArenaRank.SetRankEntity(arenaInfo.GetRank(), arenaInfo);
                }
            }
            else
            {
                //若数据库中的竞技场排行榜数据为空，初始化机器人数据
                InitRank();
            }
            InitRuleManager();
            InitPrizeManager();
        }
        internal void InitChallengeData(List<Tuple<ulong, ArkCrossEngine.ChallengeInfo>> challengerInfoList)
        {
            m_ChallengeManager = new ChallengeManager(m_ArenaRank, m_BaseConfig.MaxFightTime, m_BaseConfig.MaxHistoryCount);
            foreach (var record in challengerInfoList)
            {
                m_ChallengeManager.AddChallengeHistory(record.Item1, record.Item2);
            }
        }

        internal List<ArenaInfo> ArenaRankList
        {
            get { return m_ArenaRank.EntityList; }
        }
        internal Dictionary<ulong, List<ArkCrossEngine.ChallengeInfo>> ArenaChallengeHistory
        {
            get { return m_ChallengeManager.ChallengeHistory; }
        }

        internal void AddArenaInfo(ArenaInfo arenaInfo)
        {
            if (arenaInfo != null)
            {
                arenaInfo.BattleCD = m_BaseConfig.BattleCd;
                arenaInfo.MaxFightCount = m_BaseConfig.MaxBattleCount;
                m_ArenaRank.SetRankEntity(arenaInfo.GetRank(), arenaInfo);
            }
        }
        internal void AddUserArenaChallenge(ulong userGuid, List<ChallengeInfo> challengerInfoList)
        {
            foreach (var record in challengerInfoList)
            {
                m_ChallengeManager.AddChallengeHistory(userGuid, record);
            }
        }

        internal void RemoveUnRankEntity(ulong guid)
        {
            m_ArenaRank.RemoveUnRankEntity(guid);
            m_ChallengeManager.RemoveChallengeHistory(guid);
        }

        internal void Test()
        {
            int i = -1;
            List<MatchGroup> match_groups = QueryMatchArena(i, 3);
            LogSys.Log(LOG_TYPE.DEBUG, "query rank {0} match result:", i);
            foreach (MatchGroup group in match_groups)
            {
                LogSys.Log(LOG_TYPE.DEBUG, "one: {0} two: {1} three: {2}", group.One.GetId(), group.Two.GetId(), group.Three.GetId());
            }
            ArenaRobotConfig robot = ArenaConfigProvider.Instance.RobotConfig.GetDataById(11);
            ArenaInfo arena_info = new ArenaInfo(LobbyServer.Instance.GlobalDataProcessThread.GenerateUserGuid(), robot, GetMaxPartnerCount(robot.Level));
            ArenaInfo arena_info2 = new ArenaInfo(LobbyServer.Instance.GlobalDataProcessThread.GenerateUserGuid(), robot, GetMaxPartnerCount(robot.Level));
            m_ArenaRank.AddUnRankEntity(arena_info);
            m_ArenaRank.AddUnRankEntity(arena_info2);
            PrintAllRank();
            PrintUnRankedEntities();
        }

        private void TestChallenge(ulong src, ulong target, bool is_success)
        {
            StartChallenge(src, target, 0);
            ChallengeOver(src, is_success);
            LogSys.Log(LOG_TYPE.DEBUG, "after {0} challenge {1} is_success={2}!", src, target, is_success);
            PrintAllRank();
            PrintUnRankedEntities();
        }

        private void PrintAllRank()
        {
            LogSys.Log(LOG_TYPE.DEBUG, "---begin print ranked entities:");
            for (int i = 1; i <= m_ArenaRank.MaxRank; ++i)
            {
                PrintRank(i);
            }
        }

        private void PrintUnRankedEntities()
        {
            LogSys.Log(LOG_TYPE.DEBUG, "---begin print unranked entities:");
            foreach (ArenaInfo info in m_ArenaRank.GetUnRankedEntites().Values)
            {
                LogSys.Log(LOG_TYPE.DEBUG, "RankInfo: rank={0} guid={1} nickname={2}", info.GetRank(), info.GetId(), info.NickName);
            }
        }

        private void PrintRank(int rank)
        {
            ArenaInfo rank_info = m_ArenaRank.GetRankEntity(rank);
            if (rank_info != null)
            {
                LogSys.Log(LOG_TYPE.DEBUG, "RankInfo: rank={0} guid={1} nickname={2}",
                           rank_info.GetRank(), rank_info.GetId(), rank_info.NickName);
            }
            else
            {
                LogSys.Log(LOG_TYPE.DEBUG, "RankInfo: rank {0} is not set to anybody!", rank);
            }
        }

        private void PrintHistory(ulong guid)
        {
            List<ChallengeInfo> history = m_ChallengeManager.GetChallengeHistory(guid);
            if (history == null)
            {
                LogSys.Log(LOG_TYPE.DEBUG, "entity {0} history is empty!", guid);
                return;
            }
            foreach (var info in history)
            {
                LogSys.Log(LOG_TYPE.DEBUG, "\t {0}-rank({1}) challenge {2}-rank({3}) result:{4}", info.Challenger.Guid,
                                           info.Challenger.Rank, info.Target.Guid, info.Target.Rank, info.IsChallengerSuccess);
            }
        }

        internal void Check()
        {
            LogSys.Log(LOG_TYPE.DEBUG, "begin check match rule...");
            int oneBegin, twoBegin, threeBegin, threeEnd;
            for (int i = 1; i <= m_ArenaRank.MaxRank; ++i)
            {
                List<MatchGroup> match_groups = QueryMatchArena(i, 3);
                m_MatchRuleManager.GetRankMatchRange(i, out oneBegin, out twoBegin, out threeBegin, out threeEnd);
                if (match_groups.Count == 0)
                {
                    LogSys.Log(LOG_TYPE.DEBUG, "Rank {0} can't find any match! {1},{2},{3},{4}", i, oneBegin, twoBegin, threeBegin, threeEnd);
                    continue;
                }
                foreach (MatchGroup group in match_groups)
                {
                    bool is_need_break = false;
                    if (group.One == null)
                    {
                        LogSys.Log(LOG_TYPE.DEBUG, "Rank {0} one can't find any match! {1},{2}", i, oneBegin, twoBegin);
                        is_need_break = true;
                    }
                    if (group.Two == null)
                    {
                        LogSys.Log(LOG_TYPE.DEBUG, "Rank {0} two can't find any match! {1},{2}", i, twoBegin, threeBegin);
                        is_need_break = true;
                    }
                    if (group.Three == null)
                    {
                        LogSys.Log(LOG_TYPE.DEBUG, "Rank {0} three can't find any match! {1},{2}", i, threeBegin, threeEnd);
                        is_need_break = true;
                    }
                    if (is_need_break)
                    {
                        break;
                    }
                }
            }
            LogSys.Log(LOG_TYPE.DEBUG, "match rule check over!");
        }

        internal void Tick()
        {
            if (m_PrizeManager != null)
            {
                m_PrizeManager.Tick();
            }
        }

        internal List<MatchGroup> QueryMatchArena(int rank, int group)
        {
            if (rank == UNKNOWN_RANK || rank <= 0 || rank > m_ArenaRank.MaxRank)
            {
                rank = m_ArenaRank.MaxRank + 1;
            }
            return m_MatchRuleManager.GetMatchGroup(rank, group);
        }

        internal List<ArenaInfo> QueryRankList(int rank)
        {
            if (!m_ArenaRank.IsRankLegal(rank))
            {
                rank = m_ArenaRank.MaxRank + 1;
            }
            List<ArenaInfo> result = new List<ArenaInfo>();
            int top_rank = m_BaseConfig.QueryTopRankCount;
            int front_rank = rank - m_BaseConfig.QueryFrontRankCount;
            int behind_rank = rank + m_BaseConfig.QueryBehindRankCount;
            if (top_rank > m_ArenaRank.MaxRank)
            {
                top_rank = m_ArenaRank.MaxRank;
            }
            if (front_rank < 1)
            {
                front_rank = 1;
            }
            if (front_rank < top_rank)
            {
                front_rank = top_rank + 1;
            }
            if (behind_rank < top_rank)
            {
                behind_rank = top_rank;
            }
            if (behind_rank > m_ArenaRank.MaxRank)
            {
                behind_rank = m_ArenaRank.MaxRank;
            }
            for (int i = 1; i <= top_rank; i++)
            {
                if (i == rank)
                {
                    continue;
                }
                ArenaInfo info = m_ArenaRank.GetRankEntity(i);
                if (info != null)
                {
                    result.Add(info);
                }
            }
            for (int i = front_rank; i <= behind_rank; i++)
            {
                if (i == rank)
                {
                    continue;
                }
                ArenaInfo info = m_ArenaRank.GetRankEntity(i);
                if (info != null)
                {
                    result.Add(info);
                }
            }
            return result;
        }

        internal List<ChallengeInfo> QueryHistory(ulong guid)
        {
            return m_ChallengeManager.GetChallengeHistory(guid);
        }

        internal int GetMaxPartnerCount(int level)
        {
            int max_count = 1;
            for (int i = 1; i < m_MaxParterLimit.Length; i += 2)
            {
                int phase_level = m_MaxParterLimit[i - 1];
                int phase_count = m_MaxParterLimit[i];
                if (level >= phase_level && phase_count > max_count)
                {
                    max_count = phase_count;
                }
            }
            return max_count;
        }

        internal ArenaInfo CreateArenaInfo(UserInfo info)
        {
            ArenaInfo result = GetArenaInfoById(info.Guid);
            if (result == null)
            {
                int max_partner_count = GetMaxPartnerCount(info.Level);
                result = new ArenaInfo(info, max_partner_count, m_BaseConfig.MaxBattleCount, m_BaseConfig.BattleCd);
                result.FightCountResetTime = GetNextExcuteDate(m_BaseConfig.BattleCountResetTime);
                m_ArenaRank.AddUnRankEntity(result);
            }
            return result;
        }

        internal void ResetArenaFightCount(ArenaInfo info)
        {
            if (DateTime.Now > info.FightCountResetTime)
            {
                info.FightCountResetTime = GetNextExcuteDate(m_BaseConfig.BattleCountResetTime);
                info.LeftFightCount = m_BaseConfig.MaxBattleCount;
                info.FightCountBuyTime = 0;
            }
        }

        internal ArenaInfo GetArenaInfoById(ulong guid)
        {
            return m_ArenaRank.GetRankEntityById(guid);
        }

        internal bool StartChallenge(ulong challenger_guid, ulong target_guid, int sign)
        {
            ArenaInfo challenger = GetArenaInfoById(challenger_guid);
            if (challenger == null)
            {
                ErrorCode = GeneralOperationResult.LC_Failure_Arena_NotFindTarget;
                return false;
            }
            if (challenger.IsInBattleCd())
            {
                ErrorCode = GeneralOperationResult.LC_Failure_InCd;
                return false;
            }
            if (challenger.LeftFightCount <= 0)
            {
                ErrorCode = GeneralOperationResult.LC_Failure_NoFightCount;
                return false;
            }
            ArenaInfo target_info = GetArenaInfoById(target_guid);
            if (target_info == null)
            {
                ErrorCode = GeneralOperationResult.LC_Failure_Arena_NotFindTarget;
                return false;
            }
            if (challenger.GetId() == target_info.GetId())
            {
                ErrorCode = GeneralOperationResult.LC_Failure_Arena_NotFindTarget;
                return false;
            }
            m_ChallengeManager.BeginChallenge(challenger, target_info, sign);
            return true;
            //m_ArenaRank.ExchangeRank(cur_rank, target_info.GetRank());
        }

        internal void BeginFight(ulong guid)
        {
            ArenaInfo arena = GetArenaInfoById(guid);
            ChallengeInfo info = GetDoingChallengeInfo(guid);
            if (info != null && arena != null && info.Challenger.Guid == guid)
            {
                if (info.ChallengeBeginTime <= DateTime.Now && DateTime.Now < info.BeginFightTime)
                {
                    info.BeginFightTime = DateTime.Now;
                    info.ChallengeDeadLine = info.BeginFightTime.AddMilliseconds(m_BaseConfig.MaxFightTime + ArenaSystem.FIGHT_TIME_DEVIATION_MS);
                    arena.LastBattleTime = info.BeginFightTime.AddMilliseconds(m_BaseConfig.MaxFightTime);
                }
            }
        }

        internal ChallengeInfo GetDoingChallengeInfo(ulong guid)
        {
            return m_ChallengeManager.GetDoingChallengeInfo(guid);
        }

        internal bool ChallengeOver(ulong challenge_guid, bool result)
        {
            ChallengeInfo challengeInfo = m_ChallengeManager.GetDoingChallengeInfo(challenge_guid);
            if (challengeInfo != null)
            {
                if (m_ChallengeManager.IsChallengeOverTime(challengeInfo))
                {
                    result = false;
                }
                m_ChallengeManager.ChallengeResult(challengeInfo, result);
            }
            return true;
        }

        internal static DateTime GetNextExcuteDate(SimpleTime stime)
        {
            DateTime target_date = DateTime.Today;
            target_date = target_date.AddHours(stime.Hour);
            target_date = target_date.AddMinutes(stime.Minutes);
            if (target_date < DateTime.Now)
            {
                target_date = target_date.AddDays(1);
            }
            return target_date;
        }

        //private functions--------------------------------------
        private void InitRank()
        {
            for (int i = 1; i <= m_ArenaRank.MaxRank; i++)
            {
                ArenaRobotConfig robot = ArenaConfigProvider.Instance.RobotConfig.GetDataById(i);
                if (robot != null)
                {
                    int max_partner_count = GetMaxPartnerCount(robot.Level);
                    ArenaInfo arena_info = new ArenaInfo(LobbyServer.Instance.GlobalDataProcessThread.GenerateUserGuid(), robot, max_partner_count);
                    m_ArenaRank.SetRankEntity(i, arena_info);
                    //LogSys.Log(LOG_TYPE.DEBUG, "ArenaSetRobot: rank={0} Guid={1} NickName={2}", i, arena_info.GetId(), arena_info.NickName);
                }
            }
        }

        private void InitRuleManager()
        {
            List<ArenaMatchRuleConfig> rules = new List<ArenaMatchRuleConfig>();
            foreach (var pair in ArenaConfigProvider.Instance.MatchRuleConfig.GetData())
            {
                ArenaMatchRuleConfig rule = (ArenaMatchRuleConfig)pair.Value;
                rules.Add(rule);
            }
            m_MatchRuleManager = new MatchRuleManager(m_ArenaRank, rules);
        }

        private void InitPrizeManager()
        {
            List<ArenaPrizeConfig> rules = new List<ArenaPrizeConfig>();
            foreach (var pair in ArenaConfigProvider.Instance.PrizeConfig.GetData())
            {
                ArenaPrizeConfig rule = (ArenaPrizeConfig)pair.Value;
                rules.Add(rule);
            }
            m_PrizeManager = new PrizeManager(m_ArenaRank, rules, m_BaseConfig.PrizePresentTime, m_MailSystem);
        }

        //private attributes-------------------------------------
        private Rank<ArenaInfo> m_ArenaRank;
        private MatchRuleManager m_MatchRuleManager;
        private ChallengeManager m_ChallengeManager;
        private PrizeManager m_PrizeManager;

        private int[] m_MaxParterLimit;
        private MailSystem m_MailSystem;
        private ArenaBaseConfig m_BaseConfig;
    }
}
