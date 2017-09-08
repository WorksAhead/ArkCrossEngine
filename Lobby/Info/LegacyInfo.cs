using System;
using System.Collections.Generic;
using DashFire;

namespace Lobby
{
    internal class LegacyInfo
    {
        internal object Lock
        {
            get { return m_Lock; }
        }
        internal ItemInfo[] SevenArcs
        {
            get { return m_SevenArcs; }
        }
        internal void SetLegacyData(int index, ItemInfo info)
        {
            if (null == m_SevenArcs)
                return;
            lock (m_Lock)
            {
                if (index >= 0 && index < m_SevenArcs.Length)
                {
                    m_SevenArcs[index] = info;
                }
            }
        }
        internal ItemInfo GetLegacyData(int index)
        {
            if (null == m_SevenArcs)
                return null;
            ItemInfo info = null;
            lock (m_Lock)
            {
                if (index >= 0 && index < m_SevenArcs.Length)
                {
                    info = m_SevenArcs[index];
                }
            }
            return info;
        }
        internal void ResetLegacyData(int index)
        {
            if (null == m_SevenArcs)
                return;
            lock (m_Lock)
            {
                if (index >= 0 && index < m_SevenArcs.Length)
                {
                    m_SevenArcs[index] = null;
                }
            }
        }
        internal void Reset()
        {
            if (null == m_SevenArcs)
                return;
            lock (m_Lock)
            {
                for (int index = 0; index < m_SevenArcs.Length; ++index)
                {
                    m_SevenArcs[index] = null;
                }
            }
        }
        internal const int c_AttrCarrier = 101080;
        internal const int c_MaxLegacyNum = 4;
        private object m_Lock = new object();
        private ItemInfo[] m_SevenArcs = new ItemInfo[c_MaxLegacyNum];
    }
}