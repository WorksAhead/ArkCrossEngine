using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal sealed class LogicServerManager
    {
        internal void InitLogicServer()
        {
            int server_count = LogicServerConfigProvider.Instance.GetServerCount();
            if (server_count <= 0)
                return;
            m_LogicServerData.Clear();
            for (int i = 1; i <= server_count; ++i)
            {
                LogicServerConfig config = LogicServerConfigProvider.Instance.GetDataById(i);
                if (null != config)
                {
                    HashSet<ulong> users = new HashSet<ulong>();
                    m_LogicServerData.Add(i, users);
                }
            }
        }
        internal void AddUserToLogicServer(int server_id, ulong user_guid)
        {
            HashSet<ulong> users = null;
            if (m_LogicServerData.TryGetValue(server_id, out users))
            {
                if (!users.Contains(user_guid))
                    users.Add(user_guid);
            }
        }
        internal void DelUserFromLogicServer(int server_id, ulong user_guid)
        {
            HashSet<ulong> users = null;
            if (m_LogicServerData.TryGetValue(server_id, out users))
            {
                users.Remove(user_guid);
            }
        }
        internal HashSet<ulong> GetUsersFromLogicServer(int server_id)
        {
            HashSet<ulong> users = null;
            m_LogicServerData.TryGetValue(server_id, out users);
            return users;
        }
        internal int GetLogicServerUserCount(int server_id)
        {
            int ct = 0;
            HashSet<ulong> users = null;
            if (m_LogicServerData.TryGetValue(server_id, out users))
            {
                ct = users.Count;
            }
            return ct;
        }
        ///
        private Dictionary<int, HashSet<ulong>> m_LogicServerData = new Dictionary<int, HashSet<ulong>>();
    }
}
