using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobby
{
    class ActivityFinishInfo
    {
        internal ActivityFinishInfo()
        { }

        internal void Clear()
        {
            lock (m_Lock)
            {
                m_FinishedActivities.Clear();
            }
        }

        internal object Lock
        {
            get { return m_Lock; }
        }
        internal List<int>  FinishedActivities
        {
            get { return m_FinishedActivities; }
        }

        internal void AddToFinishedActivitiesList(int nActID)
        {
            lock (m_Lock)
            {
                m_FinishedActivities.Add(nActID);
            }
        }

        private object m_Lock = new object();
        private List<int> m_FinishedActivities = new List<int>();                   // 记录已经完成活动Id
    }
}
