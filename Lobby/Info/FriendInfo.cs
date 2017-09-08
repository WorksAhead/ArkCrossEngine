using System;

namespace Lobby
{
    internal class FriendInfo
    {
        internal FriendInfo()
        {
        }
        internal ulong Guid
        {
            get { return m_Guid; }
            set { m_Guid = value; }
        }
        internal string Nickname
        {
            get { return m_Nickname; }
            set { m_Nickname = value; }
        }
        internal int HeroId
        {
            get { return m_HeroId; }
            set { m_HeroId = value; }
        }
        internal int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        internal int FightingScore
        {
            get { return m_FightingScore; }
            set { m_FightingScore = value; }
        }
        internal bool IsOnline
        {
            get { return m_IsOnline; }
            set { m_IsOnline = value; }
        }
        internal bool IsBlack
        {
            get { return m_IsBlack; }
            set { m_IsBlack = value; }
        }
        internal EquipInfo Equips
        {
            get { return m_EquipInfo; }
            set { m_EquipInfo = value; }
        }
        internal SkillInfo Skills
        {
            get { return m_SkillInfo; }
            set { m_SkillInfo = value; }
        }
        internal const int c_Friend_Max = 40;
        private ulong m_Guid = 0;
        private string m_Nickname = null;
        private int m_HeroId = 0;
        private int m_Level = 1;
        private int m_FightingScore = 0;
        private bool m_IsOnline = false;
        private bool m_IsBlack = false;
        private EquipInfo m_EquipInfo = new EquipInfo();
        private SkillInfo m_SkillInfo = new SkillInfo();
    }
}

