using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  internal sealed class QueueingThread : ArkCrossEngine.MyServerThread
  {
    internal sealed class LoginInfo
    {
      public string AccountKey;
      public string AccountId;
      public int LoginServerId;
      public string ClientGameVersion;
      public string ClientLoginIp;
      public string UniqueIdentifier;
      public string System;
      public string ChannelId;
      public string NodeName;
      public int QueueingNum;
    }
    //=========================================================================================
    //同步调用方法部分，其它线程可直接调用(需要考虑多线程安全)。
    //=========================================================================================
    internal int GetQueueingNum(string accountKey)
    {
      int num = 0;
      LoginInfo info;
      if (m_QueueingInfos.TryGetValue(accountKey, out info)) {
        num = info.QueueingNum - GetEnterCount(info.LoginServerId);
      }
      return num;
    }
    internal bool IsQueueingFull()
    {
      return m_QueueingInfos.Count >= m_MaxQueueingCount;
    }
    internal bool NeedQueueing(int serverId)
    {
      bool ret = false;
      if (IsLobbyFull() || GetQueueingCount(serverId) > 0) {
        ret = true;
      } else {
        ret = !CanEnter(serverId);
      }
      return ret;
    }
    //=========================================================================================
    //异步调用方法部分，需要通过QueueAction调用。
    //=========================================================================================
    internal void StartQueueing(string accountKey, string accountId, int login_server_id, string client_game_version, string client_login_ip, string unique_identifier, string system, string channelId, string nodeName)
    {
      if (IsQueueingFull())
        return;
      LoginInfo info;
      if (m_QueueingInfos.TryGetValue(accountKey, out info)) {
        if (info.LoginServerId != login_server_id) {
          info.AccountId = accountId;
          info.LoginServerId = login_server_id;
          info.ClientGameVersion = client_game_version;
          info.ClientLoginIp = client_login_ip;
          info.UniqueIdentifier = unique_identifier;
          info.System = system;
          info.ChannelId = channelId;
          info.NodeName = nodeName;
        } else {
          info.AccountId = accountId;
          info.LoginServerId = login_server_id;
          info.ClientGameVersion = client_game_version;
          info.ClientLoginIp = client_login_ip;
          info.UniqueIdentifier = unique_identifier;
          info.System = system;
          info.ChannelId = channelId;
          info.NodeName = nodeName;
          return;//没有切换服务器就不要重复进队列了。
        }
      } else {
        info = new LoginInfo();
        info.AccountKey = accountKey;
        info.AccountId = accountId;
        info.LoginServerId = login_server_id;
        info.ClientGameVersion = client_game_version;
        info.ClientLoginIp = client_login_ip;
        info.UniqueIdentifier = unique_identifier;
        info.System = system;
        info.ChannelId = channelId;
        info.NodeName = nodeName;
        m_QueueingInfos.AddOrUpdate(accountKey, info, (k, i) => info);
      }
      if (null != info) {
        Queue<string> queue;
        if (m_QueueingAccounts.TryGetValue(login_server_id, out queue)) {
          queue.Enqueue(accountKey);
        } else {
          queue = new Queue<string>();
          queue.Enqueue(accountKey);
          m_QueueingAccounts.Add(login_server_id, queue);
        }
        if (null != queue) {
          info.QueueingNum = queue.Count + GetEnterCount(login_server_id);
        } else {
          //out of memory
        }
      } else {
        //out of memory
      }
    }
    internal void UpdateMaxUserCount(int maxUserCount, int maxUserCountPerLogicServer, int maxQueueingCount)
    {
      m_MaxOnlineUserCount = maxUserCount;
      m_MaxOnlineUserCountPerLogicServer = maxUserCountPerLogicServer;
      m_MaxQueueingCount = maxQueueingCount;
    }
        
    protected override void OnStart()
    {
      TickSleepTime = 10;
      ActionNumPerTick = 1024;
    }
    protected override void OnTick()
    {
      long curTime = TimeUtility.GetServerMilliseconds();
      if (m_LastLogTime + 60000 < curTime) {
        m_LastLogTime = curTime;
        DebugPoolCount((string msg) => {
          LogSys.Log(LOG_TYPE.INFO, "GmServerThread.ActionQueue {0}", msg);
        });
      }

      const int c_MaxIterationPerTick = 100;
      if (IsLobbyFull() || GetTotalQueueingCount() <= 0) {
        //大厅已经满或者没有排队的玩家，多休息1秒
        System.Threading.Thread.Sleep(1000);
      } else {
        DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
        for (int i = 0; i < c_MaxIterationPerTick; ++i) {
          foreach (KeyValuePair<int, Queue<string>> pair in m_QueueingAccounts) {
            int serverId = pair.Key;
            Queue<string> queue = pair.Value;
            if (queue.Count>0 && CanEnter(serverId)) {
              string accountKey = queue.Dequeue();
              IncEnterCount(serverId);
              LoginInfo info;
              if (m_QueueingInfos.TryRemove(accountKey, out info) && info.LoginServerId == serverId) {
                dataProcess.DispatchAction(dataProcess.DoAccountLoginWithoutQueueing, accountKey, info.AccountId, info.LoginServerId, info.ClientGameVersion, info.ClientLoginIp, info.UniqueIdentifier, info.System, info.ChannelId, info.NodeName);
                ++i;
              }
            }
          }
        }
      }
    }

    private int GetTotalQueueingCount()
    {
      //注意这个函数不要跨线程调用
      int ct = 0;
      foreach(Queue<string> queue in m_QueueingAccounts.Values) {
        ct += queue.Count;
      }
      return ct;
    }
    private int GetEnterCount(int serverId)
    {
      int ct;
      if (!m_EnterCounts.TryGetValue(serverId, out ct)) {
        ct = 0;
      }
      return ct;
    }
    private void IncEnterCount(int serverId)
    {
      m_EnterCounts.AddOrUpdate(serverId, 1, (k, v) => v + 1);
    }
    private int GetQueueingCount(int serverId)
    {
      int ct = 0;
      //这里仅读数量，不加锁
      Queue<string> queue;
      if (m_QueueingAccounts.TryGetValue(serverId, out queue)) {
        ct = queue.Count;
      }
      return ct;
    }
    private bool IsLobbyFull()
    {
      int totalUserCount = LobbyServer.Instance.DataProcessScheduler.GetUserCount();
      return totalUserCount >= m_MaxOnlineUserCount;
    }
    private bool CanEnter(int serverId)
    {
      bool ret = true;
      int serverUserCount = LobbyServer.Instance.GlobalDataProcessThread.GetLogicServerUserCount(serverId);
      if (serverUserCount >= m_MaxOnlineUserCountPerLogicServer) {
        ret = false;
      }
      return ret;
    }

    private ConcurrentDictionary<string, LoginInfo> m_QueueingInfos = new ConcurrentDictionary<string, LoginInfo>();
    private ConcurrentDictionary<int, int> m_EnterCounts = new ConcurrentDictionary<int, int>();
    private Dictionary<int, Queue<string>> m_QueueingAccounts = new Dictionary<int, Queue<string>>();

    private int m_MaxOnlineUserCount = 12000;
    private int m_MaxOnlineUserCountPerLogicServer = 3000;
    private int m_MaxQueueingCount = 6000;

    private long m_LastLogTime = 0;    
  }
}
