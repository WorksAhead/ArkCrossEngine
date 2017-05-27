using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public sealed class GowDataForMsg
    {
        public ulong m_Guid;
        public int m_GowElo;
        public string m_Nick;
        public int m_Heroid;
        public int m_Level;
        public int m_FightingScore;
        public int m_RankId;
        public int m_Point;
        public int m_CriticalTotalMatches;
        public int m_CriticalAmassWinMatches;
        public int m_CriticalAmassLossMatches;
        public List<ItemDataInfo> m_Equips = new List<ItemDataInfo>();
        public List<SkillInfo> m_Skills = new List<SkillInfo>();
    }
    public sealed class GowInfo
    {
        public int GowElo
        {
            get { return m_GowElo; }
            set { m_GowElo = value; }
        }
        public int GowMatches
        {
            get { return m_GowMatches; }
            set { m_GowMatches = value; }
        }
        public int GowWinMatches
        {
            get { return m_GowWinMatches; }
            set { m_GowWinMatches = value; }
        }
        public int LeftMatchCount
        {
            get { return m_LeftMatchCount; }
            set { m_LeftMatchCount = value; }
        }
        public DateTime LastBuyTime
        {
            get { return m_LastBuyTime; }
            set { m_LastBuyTime = value; }
        }
        public int LeftBuyCount
        {
            get { return m_LeftBuyCount; }
            set { m_LeftBuyCount = value; }
        }
        public List<GowDataForMsg> GowTop
        {
            get { return m_GowTop; }
        }
        public int RankId
        {
            get { return m_RankId; }
            set { m_RankId = value; }
        }
        public int Point
        {
            get { return m_Point; }
            set { m_Point = value; }
        }
        public int CriticalTotalMatches
        {
            get { return m_CriticalTotalMatches; }
            set { m_CriticalTotalMatches = value; }
        }
        public int AmassWinMatches
        {
            get { return m_CriticalAmassWinMatches; }
            set { m_CriticalAmassWinMatches = value; }
        }
        public int AmassLossMatches
        {
            get { return m_CriticalAmassLossMatches; }
            set { m_CriticalAmassLossMatches = value; }
        }
        public bool IsAcquirePrize
        {
            get { return m_IsAcquirePrize; }
            set { m_IsAcquirePrize = value; }
        }

        private int m_GowElo = 1000;
        private int m_GowMatches = 0;
        private int m_GowWinMatches = 0;
        private int m_LeftMatchCount = 0;
        private DateTime m_LastBuyTime;
        private int m_LeftBuyCount = 0;
        private List<GowDataForMsg> m_GowTop = new List<GowDataForMsg>();

        private int m_RankId = 0;
        private int m_Point = 0;
        private int m_CriticalTotalMatches = 0;
        private int m_CriticalAmassWinMatches = 0;
        private int m_CriticalAmassLossMatches = 0;

        private bool m_IsAcquirePrize = false;
    }
}