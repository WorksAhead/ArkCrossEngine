using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobby
{
    class OnlineDurationInfo
    {
        internal OnlineDurationInfo()
        { }
        
        internal void Clear()
        {
            lock (m_Lock)
            {
                m_DailyOnLineDuration = 0;
                m_DailyOnLineRewardedIndex.Clear();
            }
        }

        internal object Lock
        {
            get { return m_Lock; }
        }
        internal List<int> DailyOnLineRewardedIndex
        {
            get { return m_DailyOnLineRewardedIndex; }
        }
        internal System.DateTime OnlineDurationStartTime
        {
            get { return m_OnlineDurationStartTime; }
            set { m_OnlineDurationStartTime = value; }
        }
        internal int DailyOnLineDuration
        {
            get { return m_DailyOnLineDuration; }
            set { m_DailyOnLineDuration = value; }
        }

        internal void AddToDailyOnlineRewardedList(int nIndex)
        {
            lock (m_Lock)
            {
                m_DailyOnLineRewardedIndex.Add(nIndex);
            }
        }
        internal void ClearDailyOnlineRewardedList()
        {
            lock (m_Lock)
            {
                m_DailyOnLineRewardedIndex.Clear();
            }
        }

        private object m_Lock = new object();

        // 在线时长
        private int m_DailyOnLineDuration = 0;
        private List<int> m_DailyOnLineRewardedIndex = new List<int>();
        private DateTime m_OnlineDurationStartTime = new DateTime(1970, 1, 1, 0, 0, 0);
    }
}
