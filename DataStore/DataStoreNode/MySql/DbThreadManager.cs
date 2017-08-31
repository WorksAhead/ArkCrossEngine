using System;
using System.Collections.Generic;

namespace DashFire
{
    internal sealed class DbThreadManager
    {
        internal void Init(int loadThreadNum, int saveThreadNum)
        {
            for (int i = 0; i < loadThreadNum; ++i)
            {
                DbThread thread = new DbThread(m_LoadActionQueue);
                thread.Start();
                m_LoadThreads.Add(thread);
            }
            for (int i = 0; i < saveThreadNum; ++i)
            {
                DbThread thread = new DbThread(m_SaveActionQueue);
                thread.Start();
                m_SaveThreads.Add(thread);
            }
        }
        internal void Stop()
        {
            foreach (DbThread thread in m_LoadThreads)
            {
                thread.Stop();
            }
            foreach (DbThread thread in m_SaveThreads)
            {
                thread.Stop();
            }
        }
        internal ArkCrossEngine.IActionQueue LoadActionQueue
        {
            get { return m_LoadActionQueue; }
        }
        internal ArkCrossEngine.IActionQueue SaveActionQueue
        {
            get { return m_SaveActionQueue; }
        }

        private ArkCrossEngine.ServerAsyncActionProcessor m_LoadActionQueue = new ArkCrossEngine.ServerAsyncActionProcessor();
        private ArkCrossEngine.ServerAsyncActionProcessor m_SaveActionQueue = new ArkCrossEngine.ServerAsyncActionProcessor();
        private List<DbThread> m_LoadThreads = new List<DbThread>();
        private List<DbThread> m_SaveThreads = new List<DbThread>();

        internal static DbThreadManager Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static DbThreadManager s_Instance = new DbThreadManager();
    }
}
