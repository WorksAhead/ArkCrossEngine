using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal interface IRankEntity
    {
        void SetRank(int rank);
        int GetRank();
        ulong GetId();
    }

    internal class ArenaInfo : IRankEntity
    {
        internal ArenaInfo()
        {
        }

        internal ArenaInfo(ulong guid, ArenaRobotConfig robot_info, int max_partner_count)
        {
            m_Guid = guid;
            m_Rank = ArenaSystem.UNKNOWN_RANK;
            m_HeroId = robot_info.HeroId;
            m_NickName = robot_info.NickName;
            m_Level = robot_info.Level;
            m_FightScore = robot_info.FightScore;
            foreach (ArenaItemInfo arena_item in robot_info.EquipInfo)
            {
                ItemInfo item = new ItemInfo(arena_item.ItemId, arena_item.Level);
                item.AppendProperty = arena_item.AppendProperty;
                m_EquipInfo.Add(item);
            }
            foreach (ArenaXSoulInfo xsoul in robot_info.XSoulInfo)
            {
                m_XSoulInfo.Add(xsoul);
            }
            foreach (ArenaPartnerInfo partner in robot_info.PartnerInfo)
            {
                PartnerConfig config = PartnerConfigProvider.Instance.GetDataById(partner.id);
                if (config == null)
                {
                    continue;
                }
                PartnerInfo target = new PartnerInfo(config);
                target.CurAdditionLevel = partner.AdditionLevel;
                target.CurSkillStage = partner.SkillStage;
                m_PartnerInfo.Add(target);
                if (m_PartnerInfo.Count == max_partner_count)
                {
                    m_ActivePartner = target;
                    break;
                }
            }
            foreach (ArenaSkillInfo e in robot_info.SkillInfo)
            {
                SkillDataInfo skilldata = new SkillDataInfo(e.Id, e.Level);
                skilldata.Postions.Presets[0] = (SlotPosition)e.EquipPos;
                m_SkillDataInfo.Add(skilldata);
            }
            m_IsRobot = true;
        }

        internal ArenaInfo(UserInfo userinfo, int max_partner_count, int max_battle_count, long battle_cd)
        {
            m_IsRobot = false;
            m_Guid = userinfo.Guid;
            m_Rank = ArenaSystem.UNKNOWN_RANK;
            m_HeroId = userinfo.HeroId;
            m_MaxFightCount = max_battle_count;
            m_LeftFightCount = max_battle_count;
            m_FightCountBuyTime = 0;
            m_BattleCd = battle_cd;
            UpdateArenaInfo(userinfo);
            m_LastBattleTime = new DateTime();
            InitPartnerInfo(userinfo, max_partner_count);
        }

        public void UpdateArenaInfo(UserInfo userinfo)
        {
            if (userinfo == null)
            {
                return;
            }
            if (m_Guid != userinfo.Guid)
            {
                return;
            }
            m_NickName = userinfo.Nickname;
            m_Level = userinfo.Level;
            m_FightScore = userinfo.FightingScore;
            m_ActivePartner = userinfo.PartnerStateInfo.GetActivePartner();
            EquipInfo.Clear();
            for (int i = 0; i < Lobby.EquipInfo.c_MaxEquipmentNum; i++)
            {
                ItemInfo equip = userinfo.Equip.GetEquipmentData(i);
                if (equip != null)
                {
                    EquipInfo.Add(equip);
                }
            }
            //skills
            m_SkillDataInfo.Clear();
            for (int i = 0; i < userinfo.Skill.Skills.Count; ++i)
            {
                SkillDataInfo skill_data = new SkillDataInfo();
                skill_data.ID = userinfo.Skill.Skills[i].ID;
                skill_data.Level = userinfo.Skill.Skills[i].Level;
                skill_data.Postions = userinfo.Skill.Skills[i].Postions;
                m_SkillDataInfo.Add(skill_data);
            }
            //xsoul
            m_XSoulInfo.Clear();
            foreach (ItemInfo item in userinfo.XSoul.GetAllXSoulPartData().Values)
            {
                ArenaXSoulInfo item_msg = new ArenaXSoulInfo();
                item_msg.ItemId = item.ItemId;
                item_msg.Level = item.Level;
                item_msg.Experience = item.Experience;
                item_msg.ModelLevel = item.ShowModelLevel;
                m_XSoulInfo.Add(item_msg);
            }
            //legacy
            m_LegacyInfo.Clear();
            for (int i = 0; i < userinfo.Legacy.SevenArcs.Length; i++)
            {
                ItemInfo item = userinfo.Legacy.SevenArcs[i];
                if (item != null)
                {
                    ArenaItemInfo legacy = new ArenaItemInfo();
                    legacy.ItemId = item.ItemId;
                    legacy.Level = item.Level;
                    legacy.AppendProperty = item.AppendProperty;
                    legacy.IsUnlocked = item.IsUnlock;
                    m_LegacyInfo.Add(legacy);
                }
            }
        }

        public bool IsNeedUpdate(UserInfo info)
        {
            if (m_Guid != info.Guid)
            {
                return false;
            }
            if (Level != info.Level)
            {
                return true;
            }
            if (Math.Abs(FightScore - info.FightingScore) >= 10)
            {
                return true;
            }
            return false;
        }


        private void InitPartnerInfo(UserInfo userinfo, int max_partner_count)
        {
            List<PartnerInfo> own_partners = userinfo.PartnerStateInfo.GetAllPartners();
            if (own_partners.Count < max_partner_count)
            {
                max_partner_count = own_partners.Count;
            }
            for (int i = 0; i < max_partner_count; i++)
            {
                m_PartnerInfo.Add(own_partners[i]);
            }
        }

        internal bool IsInBattleCd()
        {
            TimeSpan delta = DateTime.Now - m_LastBattleTime;
            if (delta.TotalMilliseconds + ArenaSystem.CHALLENGE_CD_DEVIATION_MS >= m_BattleCd)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetRank(int rank)
        {
            m_Rank = rank;
            AddRankHistory(m_Rank);
        }
        public int GetRank() { return m_Rank; }
        public ulong GetId() { return m_Guid; }
        internal void SetId(ulong guid) { m_Guid = guid; }

        internal DateTime LastBattleTime
        {
            set { m_LastBattleTime = value; }
            get { return m_LastBattleTime; }
        }

        internal int HeroId
        {
            set { m_HeroId = value; }
            get { return m_HeroId; }
        }

        internal string NickName
        {
            set { m_NickName = value; }
            get { return m_NickName; }
        }

        internal int Level
        {
            set { m_Level = value; }
            get { return m_Level; }
        }

        internal int FightScore
        {
            set { m_FightScore = value; }
            get { return m_FightScore; }
        }

        internal int LeftFightCount
        {
            set { m_LeftFightCount = value; }
            get { return m_LeftFightCount; }
        }
        internal int MaxFightCount
        {
            get { return m_MaxFightCount; }
            set { m_MaxFightCount = value; }
        }
        internal List<ItemInfo> EquipInfo
        {
            set { m_EquipInfo = value; }
            get { return m_EquipInfo; }
        }

        internal List<SkillDataInfo> SkillDataInfo
        {
            set { m_SkillDataInfo = value; }
            get { return m_SkillDataInfo; }
        }

        internal PartnerInfo ActivePartner
        {
            get { return m_ActivePartner; }
            set { m_ActivePartner = value; }
        }

        internal List<PartnerInfo> FightPartners
        {
            set { m_PartnerInfo = value; }
            get { return m_PartnerInfo; }
        }

        internal List<ArenaItemInfo> LegacyInfo
        {
            set { m_LegacyInfo = value; }
            get { return m_LegacyInfo; }
        }

        internal List<ArenaXSoulInfo> XSoulInfo
        {
            set { m_XSoulInfo = value; }
            get { return m_XSoulInfo; }
        }

        internal bool IsRobot
        {
            set { m_IsRobot = value; }
            get { return m_IsRobot; }
        }

        internal DateTime FightCountResetTime
        {
            set { m_FightCountResetTime = value; }
            get { return m_FightCountResetTime; }
        }

        internal int FightCountBuyTime
        {
            set { m_FightCountBuyTime = value; }
            get { return m_FightCountBuyTime; }
        }

        internal Dictionary<DateTime, int> RankHistory
        {
            get { return m_RankHistory; }
        }

        internal long BattleCD
        {
            get { return m_BattleCd; }
            set { m_BattleCd = value; }
        }

        internal void AddRankHistory(int rank)
        {
            DateTime today_prize_time = ArenaSystem.GetNextExcuteDate(PrizeTime);
            if (DateTime.Now > today_prize_time)
            {
                today_prize_time = today_prize_time.AddDays(1);
            }
            m_RankHistory[today_prize_time] = rank;
            RemoveExpireRankHistory();
        }

        internal void RemoveExpireRankHistory()
        {
            if (m_RankHistory.Count <= ArenaSystem.PRIZE_RETAIN_DAYS)
            {
                return;
            }
            List<DateTime> days = new List<DateTime>(m_RankHistory.Keys);
            days.Sort();
            for (int i = 0; i < days.Count - ArenaSystem.PRIZE_RETAIN_DAYS; i++)
            {
                m_RankHistory.Remove(days[i]);
            }
            days.Clear();
        }

        internal int GetDaysRank(DateTime day)
        {
            int result = ArenaSystem.UNKNOWN_RANK;
            List<DateTime> days = new List<DateTime>(m_RankHistory.Keys);
            days.Sort();
            for (int i = days.Count - 1; i >= 0; i--)
            {
                if (days[i] <= day)
                {
                    return m_RankHistory[days[i]];
                }
            }
            return result;
        }

        private int m_Rank = ArenaSystem.UNKNOWN_RANK;
        private bool m_IsRobot = false;
        private ulong m_Guid;
        private int m_HeroId;
        private string m_NickName;
        private int m_Level;
        private int m_FightScore;
        private List<ItemInfo> m_EquipInfo = new List<ItemInfo>();
        private List<SkillDataInfo> m_SkillDataInfo = new List<SkillDataInfo>();
        private PartnerInfo m_ActivePartner;
        private List<PartnerInfo> m_PartnerInfo = new List<PartnerInfo>();
        private List<ArenaItemInfo> m_LegacyInfo = new List<ArenaItemInfo>();
        private List<ArenaXSoulInfo> m_XSoulInfo = new List<ArenaXSoulInfo>();

        private int m_LeftFightCount;
        private int m_FightCountBuyTime;
        private DateTime m_FightCountResetTime;
        private DateTime m_LastBattleTime;
        private Dictionary<DateTime, int> m_RankHistory = new Dictionary<DateTime, int>();

        internal static SimpleTime PrizeTime;
        private long m_BattleCd;
        private int m_MaxFightCount;
    }
}
