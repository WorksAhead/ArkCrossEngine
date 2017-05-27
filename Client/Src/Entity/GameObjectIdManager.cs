using System;
using System.Collections.Generic;
using System.Text;

namespace ArkCrossEngine
{
    internal sealed class GameObjectIdManager
    {
        internal int GenNextId()
        {
            int ret = m_NextId;
            ++m_NextId;
            return ret;
        }

        private GameObjectIdManager() { }

        private int m_NextId = 1;

        internal static GameObjectIdManager Instance
        {
            get { return s_Instance; }
        }
        private static GameObjectIdManager s_Instance = new GameObjectIdManager();
    }
}
