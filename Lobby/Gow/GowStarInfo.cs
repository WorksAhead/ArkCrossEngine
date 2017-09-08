using System;
using System.Collections.Generic;

namespace Lobby
{
    internal sealed class GowStarInfo
    {
        internal ulong m_Guid;
        internal int m_GowElo;
        internal string m_Nick;
        internal int m_HeroId;
        internal int m_Level;
        internal int m_FightingScore;
        internal int m_RankId;
        internal int m_Point;
        internal int m_CriticalTotalMatches;
        internal int m_CriticalAmassWinMatches;
        internal int m_CriticalAmassLossMatches;
        internal List<ItemInfo> m_Equips = new List<ItemInfo>();
        internal List<SkillDataInfo> m_Skills = new List<SkillDataInfo>();
    }
}
