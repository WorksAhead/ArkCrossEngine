using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class ArenaStateInfo
    {
        public ulong OwnerGuid;
        public int Rank;
        public bool IsBattleStarted = false;
        public bool IsEntityCreated = false;
        public long BattleStartTime;
        public ArenaTargetInfo ChallengeTarget = null;
        public UserInfo TargetInfo = null;
        public bool IsChallengeOver = false;
        public bool IsChallengeSuccess = false;
        public bool IsCheckingResult = false;
        public int LeftFightCount = 0;
        public int CurFightCountBuyTime = 0;
        public bool IsBeginFight = false;
        public DateTime EndFightLocalTime;
        public DateTime LastBattleServerTime;
        public List<int> FightPartners = new List<int>();
        public List<MatchGroup> MatchGroups = new List<MatchGroup>();
        public List<ArenaTargetInfo> RankList = new List<ArenaTargetInfo>();
        public List<ChallengeInfo> ChallengeHistory = new List<ChallengeInfo>();
        public ChallengeInfo LastChallengeResult = null;
        public Dictionary<int, NpcInfo> CreatedPartners = new Dictionary<int, NpcInfo>();
        public int CurCameraTargetPartner = -1;
        public int Sign;

        public DateTime LastInfoQueryTime;
        public DateTime LastMatchGroupQueryTime;
        public DateTime LastTopQueryTime;
        public int LastInfoQueryRank;
        public int LastTopQueryRank;
        public int LastMatchGroupQueryRank;
        public ArenaBaseConfig BaseConfig;

        public void Init(ulong id)
        {
            OwnerGuid = id;
            Rank = -1;
            LastInfoQueryTime = new DateTime();
            LastTopQueryTime = new DateTime();
            LastMatchGroupQueryTime = new DateTime();
            LastTopQueryRank = -1;
            LastInfoQueryRank = -1;
            LastMatchGroupQueryRank = -1;
            BaseConfig = ArenaConfigProvider.Instance.GetBaseConfigById(1);
        }

        public bool IsNeedQueryInfo()
        {
            TimeSpan delta = DateTime.Now - LastInfoQueryTime;
            if (delta.TotalMilliseconds >= BaseConfig.InfoRefreshTime)
            {
                return true;
            }
            if (Rank != LastInfoQueryRank)
            {
                return true;
            }
            return false;
        }

        public bool IsNeedQueryMatchGroup()
        {
            TimeSpan delta = DateTime.Now - LastMatchGroupQueryTime;
            if (delta.TotalMilliseconds >= BaseConfig.MatchGroupRefreshTime)
            {
                return true;
            }
            if (Rank != LastMatchGroupQueryRank)
            {
                return true;
            }
            return false;
        }

        public bool IsNeedQueryTopRank()
        {
            TimeSpan delta = DateTime.Now - LastTopQueryTime;
            if (delta.TotalMilliseconds >= BaseConfig.TopRankRefreshTime)
            {
                return true;
            }
            if (Rank != LastTopQueryRank)
            {
                return true;
            }
            return false;
        }

        public void ResetChallengeInfo()
        {
            IsBattleStarted = false;
            IsEntityCreated = false;
        }

        public void StartChallenge(ArenaTargetInfo target, int sign)
        {
            IsBattleStarted = true;
            IsBeginFight = false;
            ChallengeTarget = target;
            IsEntityCreated = false;
            TargetInfo = null;
            BattleStartTime = TimeUtility.GetServerMilliseconds();
            LastBattleServerTime = DateTime.Now.AddMilliseconds(BaseConfig.BattleCd);
            IsChallengeOver = false;
            LeftFightCount -= 1;
            IsChallengeSuccess = false;
            CurCameraTargetPartner = -1;
            LastChallengeResult = null;
            Sign = sign - (int)target.Guid;
        }

        public void BeginFight()
        {
            IsBeginFight = true;
            EndFightLocalTime = DateTime.Now.AddMilliseconds(BaseConfig.MaxFightTime);
        }

        public void CheckChallengeResult()
        {
            if (IsCheckingResult && LastChallengeResult != null)
            {
                IsCheckingResult = false;
                GfxSystem.PublishGfxEvent("ge_partnerpvp_result", "ui", LastChallengeResult);
            }
        }

        public void DealChallengeResult(ChallengeInfo info)
        {
            if (info.Challenger.Guid == OwnerGuid)
            {
                LastBattleServerTime = DateTime.Now;
                LastChallengeResult = info;
            }
            if (!info.IsChallengerSuccess)
            {
                return;
            }
            if (IsRankShouldChange(info.Challenger.Rank, info.Target.Rank))
            {
                if (info.Challenger.Guid == OwnerGuid)
                {
                    Rank = info.Target.Rank;
                }
                else
                {
                    Rank = info.Challenger.Rank;
                }
            }
        }

        public bool IsRankShouldChange(int challenger_rank, int target_rank)
        {
            if (IsRankLegal(target_rank))
            {
                if (IsRankLegal(challenger_rank))
                {
                    if (challenger_rank > target_rank)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool IsRankLegal(int rank)
        {
            if (BaseConfig == null)
            {
                return false;
            }
            int max_rank = BaseConfig.MaxRank;
            int rank_index = rank - 1;
            if (0 <= rank_index && rank_index < max_rank)
            {
                return true;
            }
            return false;
        }
    }

    public class MatchGroup
    {
        public ArenaTargetInfo One;
        public ArenaTargetInfo Two;
        public ArenaTargetInfo Three;
    }

    public class ArenaTargetInfo
    {
        public ArenaTargetInfo()
        {
        }
        public int Rank
        {
            get { return m_Rank; }
            set { m_Rank = value; }
        }
        public ulong Guid
        {
            get { return m_Guid; }
            set { m_Guid = value; }
        }
        public int HeroId
        {
            get { return m_HeroId; }
            set { m_HeroId = value; }
        }
        public string Nickname
        {
            get { return m_Nickname; }
            set { m_Nickname = value; }
        }
        public int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        public int FightingScore
        {
            get { return m_FightingScore; }
            set { m_FightingScore = value; }
        }
        public ItemDataInfo[] Equips
        {
            get { return m_EquipInfo; }
        }
        public List<SkillInfo> Skills
        {
            get { return m_SkillInfo; }
        }
        public ItemDataInfo[] Legacys
        {
            get { return m_LegacyInfo; }
        }
        public List<PartnerInfo> FightPartners
        {
            get { return m_FightPartners; }
        }

        public Dictionary<int, NpcInfo> CreatedPartners
        {
            set { m_CreatedPartners = value; }
            get { return m_CreatedPartners; }
        }

        public List<ArenaXSoulInfo> XSoulInfo
        {
            get { return m_XSoulInfo; }
        }

        public PartnerInfo ActivePartner
        {
            set { m_ActivePartner = value; }
            get { return m_ActivePartner; }
        }

        private int m_Rank = -1;
        private ulong m_Guid = 0;
        private int m_HeroId = 0;
        private string m_Nickname = "";
        private int m_Level = 1;
        private int m_FightingScore = 0;
        private ItemDataInfo[] m_EquipInfo = new ItemDataInfo[EquipmentStateInfo.c_EquipmentCapacity];
        private PartnerInfo m_ActivePartner;
        private List<PartnerInfo> m_FightPartners = new List<PartnerInfo>();
        public Dictionary<int, NpcInfo> m_CreatedPartners = new Dictionary<int, NpcInfo>();
        private List<SkillInfo> m_SkillInfo = new List<SkillInfo>();
        private ItemDataInfo[] m_LegacyInfo = new ItemDataInfo[LegacyStateInfo.c_LegacyCapacity];
        private List<ArenaXSoulInfo> m_XSoulInfo = new List<ArenaXSoulInfo>();
    }
}
