using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobby
{
    class LoginRewardInfo
    {
        internal LoginRewardInfo()
        { }

        internal void Clear()
        {
            lock (m_Lock)
            {
                m_IsGetLoginReward = false;
                m_WeeklyLoginRewardRecord.Clear();
            }
        }

        internal object Lock
        {
            get { return m_Lock; }
        }
        internal bool IsGetLoginReward
        {
            get { return m_IsGetLoginReward; }
            set { m_IsGetLoginReward = value; }
        }
        internal List<int> WeeklyLoginRewardRecord
        {
            get { return m_WeeklyLoginRewardRecord; }
        }

        internal void ClearWeeklyLoginRewardRecordList()
        {
            lock (m_Lock)
            {
                m_WeeklyLoginRewardRecord.Clear();
            }
        }
        internal void AddToWeeklyLoginRewardRecordList(int record)
        {
            lock (m_Lock)
            {                  // Add lock   
                WeeklyLoginRewardRecord.Add(record);
            }
        }
        internal void AddToWeeklyLoginRewardRecordListWithCheck(int record)
        {
            lock (m_Lock)
            {
                if (!WeeklyLoginRewardRecord.Contains(record))
                {
                    WeeklyLoginRewardRecord.Add(record);
                }
            }
        }

        private object m_Lock = new object();

        // 登陆奖励
        private bool m_IsGetLoginReward = false;
        private List<int> m_WeeklyLoginRewardRecord = new List<int>();
    }
}
