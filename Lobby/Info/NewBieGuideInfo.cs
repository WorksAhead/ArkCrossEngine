using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobby
{
    class NewBieGuideInfo
    {
        internal NewBieGuideInfo()
        { }

        internal void Clear()
        {
            lock (m_Lock)
            {
                m_NewbieFlag = 0;
                m_NewbieActionFlag = 0;
                m_GuideFlag = 0;
                m_NewBieGuideList.Clear();
            }
        }

        internal object Lock
        {
            get { return m_Lock; }
        }
        internal List<int> NewBieGuideList
        {
            get { return m_NewBieGuideList; }
            set { m_NewBieGuideList = value; }
        }
        internal long NewbieFlag
        {
            get { return m_NewbieFlag; }
            set { m_NewbieFlag = value; }
        }
        internal long NewbieActionFlag
        {
            get { return m_NewbieActionFlag; }
            set { m_NewbieActionFlag = value; }
        }
        internal long GuideFlag
        {
            get { return m_GuideFlag; }
            set { m_GuideFlag = value; }
        }

        internal void AddToList(int id)
        {
            lock (m_Lock)
            {
                if (!NewBieGuideList.Contains(id))
                {
                    NewBieGuideList.Add(id);
                }
            }
        }

        private object m_Lock = new object();

        // 新手引导相关数据
        private long m_NewbieFlag = 0;
        private long m_NewbieActionFlag = 0;
        private long m_GuideFlag = 0;
        private List<int> m_NewBieGuideList = new List<int>(); // 需要进行的引导id.    
    }
}
