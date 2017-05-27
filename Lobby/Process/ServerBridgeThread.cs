using System;
using System.Collections.Generic;
using System.Threading;
using Messenger;
using DashFire.Billing;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  internal sealed class ServerBridgeThread : MyServerThread
  {
    internal void Init(PBChannel channel)
    {
      m_BillingClient = new BillingClient(channel, this);
    }
    internal void DirectLogin(string accountKey, int loginServerId, string nodeName)
    {
      LogSys.Log(LOG_TYPE.INFO, "Account connected: {0}, Login type: DirectLogin", accountKey);
      //直接登录模式下默认accountId与设备标识accountKey相同
      //TO DO:不同设备的设备标识会不会有重复？会不会与Billing返回的accountId重复？
      string accountId = accountKey;
      if (m_KickedUsers.ContainsKey(accountId)) {
        JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
        replyMsg.m_Account = accountKey;
        replyMsg.m_AccountId = "";
        replyMsg.m_Result = (int)AccountLoginResult.Error;
        JsonMessageDispatcher.SendDcoreMessage(nodeName, replyMsg);
      } else {
        DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
        dataProcess.DispatchAction(dataProcess.DoAccountLogin, accountKey, accountId, loginServerId, "", "", "", "", "", nodeName);
      }
    }
    internal void AccountLogin(string accountKey, int opcode, int channelId, string data, int login_server_id, string client_game_version, string client_login_ip, string unique_identifier, string system, string game_channel_id, string nodeName)
    {
      ServerBridgeThread bridgeThread = LobbyServer.Instance.ServerBridgeThread;
      if (bridgeThread.CurActionNum < 30000)
        LogSys.Log(LOG_TYPE.INFO, "Account connected: {0}, Login type: AccountLogin", accountKey);
      /// Normlog
      LogSys.NormLog("serverevent", LobbyConfig.AppKeyStr, LobbyConfig.LogNormVersionStr, system, game_channel_id,
        unique_identifier, "null", (int)ServerEventCode.VerifyToken, client_game_version);
      //账号登录模式下，首先向Billing服务器验证账号，获取accountId
      //注意这里的回调执行线程是在ServerBridgeThread线程。
      VerifyAccount(accountKey, opcode, channelId, data, (BillingClient.VerifyAccountCB)((a, ret, accountId) => {
        if (ret == true) {
          if (m_KickedUsers.ContainsKey(accountId)) {
            LogSys.Log(LOG_TYPE.WARN, ConsoleColor.Green, "Account verify success but user is a kicked user. account:{0}, id:{1}", accountKey, accountId);

            JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
            replyMsg.m_Account = accountKey;
            replyMsg.m_AccountId = "";
            replyMsg.m_Result = (int)AccountLoginResult.Error;
            JsonMessageDispatcher.SendDcoreMessage(nodeName, replyMsg);
          } else {
            LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Account verify success. account:{0}, id:{1}", accountKey, accountId);

            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            dataProcess.DispatchAction(dataProcess.DoAccountLogin, accountKey, accountId, login_server_id, client_game_version, client_login_ip, unique_identifier, system, game_channel_id, nodeName);
          }
        } else {
          LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Yellow, "Account verify failed. account:{0}", accountKey);
          JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
          replyMsg.m_Account = accountKey;
          replyMsg.m_AccountId = "";
          replyMsg.m_Result = (int)AccountLoginResult.Error;
          JsonMessageDispatcher.SendDcoreMessage(nodeName, replyMsg);
        }
        /// norm log
        LogSys.NormLog("serverevent", LobbyConfig.AppKeyStr, LobbyConfig.LogNormVersionStr, system, game_channel_id,
        unique_identifier, accountId, (int)ServerEventCode.VerifyResult, client_game_version);
      }));
    }
    internal void AddKickUser(string accountId, long time)
    {
      long unlockTime = TimeUtility.GetLocalMilliseconds() + time;
      if (m_KickedUsers.ContainsKey(accountId)) {
        m_KickedUsers[accountId] = unlockTime;
      } else {
        m_KickedUsers.Add(accountId, unlockTime);
      }
    }

    private void VerifyAccount(string account, int opcode, int channelId, string data, BillingClient.VerifyAccountCB cb)
    {
      m_BillingClient.VerifyAccount(account, opcode, channelId, data, cb);
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
          LogSys.Log(LOG_TYPE.INFO, "ServerBridgeThread.ActionQueue {0}", msg);
        });
      }
      if (m_LastUnlockTime + c_UnlockCheckInterval < curTime) {
        m_LastUnlockTime = curTime;

        foreach (KeyValuePair<string, long> pair in m_KickedUsers) {
          if (pair.Value < curTime) {
            m_UnlockUsers.Add(pair.Key);
          }
        }
        foreach (string key in m_UnlockUsers) {
          m_KickedUsers.Remove(key);
        }
        m_UnlockUsers.Clear();
      }

      m_BillingClient.Tick();
    }

    private const long c_UnlockCheckInterval = 60000;
    private long m_LastUnlockTime = 0;
    private Dictionary<string, long> m_KickedUsers = new Dictionary<string, long>();
    private List<string> m_UnlockUsers = new List<string>();

    private BillingClient m_BillingClient = null;
    private long m_LastLogTime = 0;    
  }
}
