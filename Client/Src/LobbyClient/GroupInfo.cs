using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum UserState : int
    {
        Online = 0,
        Pve = 1,
        Teaming = 2,
        Room = 3,
        DropOrOffline = 4,
    }
    public class GroupMemberInfo
    {
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
        public string Nick
        {
            get { return m_Nick; }
            set { m_Nick = value; }
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
        public UserState Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }
        private ulong m_Guid;
        private int m_HeroId;
        private string m_Nick;
        private int m_Level;
        private int m_FightingScore;
        private UserState m_Status;
    }

    public class GroupInfo
    {
        public ulong CreatorGuid
        {
            get { return m_CreatorGuid; }
            set { m_CreatorGuid = value; }
        }
        public int Count
        {
            get { return m_Count; }
            set { m_Count = value; }
        }
        public List<GroupMemberInfo> Confirms
        {
            get { return m_ConfirmList; }
            set { m_ConfirmList = value; }
        }
        public List<GroupMemberInfo> Members
        {
            get { return m_Members; }
            set { m_Members = value; }
        }
        public void Reset()
        {
            m_Count = 0;
            m_CreatorGuid = 0;
            m_Members.Clear();
            m_ConfirmList.Clear();
        }
        public const int c_MemberNumMax = 3;
        public const int c_ConfirmNumMax = 50;
        private int m_Count = 0;
        private ulong m_CreatorGuid = 0;
        private List<GroupMemberInfo> m_Members = new List<GroupMemberInfo>();
        private List<GroupMemberInfo> m_ConfirmList = new List<GroupMemberInfo>();
    }
}
