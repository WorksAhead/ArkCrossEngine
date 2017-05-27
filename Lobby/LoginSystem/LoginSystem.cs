using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using Options;
using SharpSvrAPI;
using DashFire.Billing;
using SharpSvrAPI.Messenger;

namespace Lobby.LoginSystem
{
  /// <summary>
  /// 登陆系统, 验证玩家身份
  /// 使用状态机的方式处理登陆流程
  /// </summary>
  class LoginSystem
  {
    private static LoginSystem s_inst_ = new LoginSystem();
    public static LoginSystem Instance { get { return s_inst_; } }

    public void Initiate(PBMessenger messenger, ServiceAPI svr_api)
    {
      SvrAPI = svr_api;
      BillingConnected = false;
      MessageInitiate(messenger);
      if (!OptionCollection.Instance.Get<LobbyConfig>().BillingDisable)
      {
        LogSys.Log(LOG_TYPE.INFO, "Initiate Billing connection");
        InitiateBillingConnection(messenger);
      }
      else
      { 
        LogSys.Log(LOG_TYPE.INFO, "Billing disable");
      }
      svr_api.AddTimer(0, 500, (api, s, c) => Tick());
    }

    public bool BillingConnected { get; private set; }
    public ServiceAPI SvrAPI { get; private set; }

    /// <summary>
    /// 执行状态机, 并删除过期的或已经完成的状态机
    /// </summary>
    public void Tick()
    {
      for (int i = 0; i < login_sessions_.Count; ++i)
        login_sessions_[i] = ProcessLoginSession(login_sessions_[i]);
      login_sessions_.RemoveAll(s => s.Expired || s.State == LoginMachine.StateCode.Finish);
    }

    public static void SendLoginResult(string account, string node_name, uint session, LoginResult lr)
    { 
      JsonMessageLoginResult reply = new JsonMessageLoginResult();
      reply.m_Account = account;
      reply.m_Result = (int)lr;
      JsonMessageDispatcher.SendDcoreMessage(LoginSystem.Instance.SvrAPI, node_name, reply, session);
    }

    private void MessageInitiate(PBMessenger messenger)
    { 
      messenger.AddChannel((byte)CoreMessageTypeExtend.kBilling,
                           DashFire.Billing.MessageMapping.Query,
                           DashFire.Billing.MessageMapping.Query);

      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.Login, typeof(JsonMessageLogin), OnLogin);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.CYPConfirmResult, typeof(JsonMessageCYPConfirmResult), ForwardToLoginSession);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.CreateNick, typeof(JsonMessageCreateNick), ForwardToLoginSession);
    }

    private LoginMachine ProcessLoginSession(LoginMachine s)
    {
      while (s.State != LoginMachine.StateCode.Finish)
      {
        if (s.State == LoginMachine.StateCode.Initiate)
        {
          s.Run();
        }
        else if (s.State == LoginMachine.StateCode.Next)
        {
          LoginMachine ns = LoginMachines.Create(s.NextMachine, s.NextParams);
          ns.Account = s.Account;
          ns.NodeName = s.NodeName;
          ns.Session = s.Session;
          s = ns;
        }
        else if (s.State == LoginMachine.StateCode.Pending)
        {
          if (s.Expired)
            LogSys.Log(LOG_TYPE.ERROR, "LoginSession for account {0} is expired at phase {1}", s.Account, s.GetType().Name);
          break;
        }
      }
      return s;
    }

    private void InitiateBillingConnection(PBMessenger messenger)
    { 
      BillingClient.Config config = new BillingClient.Config()
      {
        SvrAPI = SvrAPI,
        Channel = messenger.To((byte)CoreMessageTypeExtend.kBilling),
        OnClose = reason =>
          {
            BillingConnected = false;
            LogSys.Log(LOG_TYPE.WARN, "Billing connection close: " + (null != reason ? reason : ""));
          }
      };

      BillingClient.Instance.Initiate(config, sys_ecode =>
        {
          if (sys_ecode == SysECode.Good)
          {
            BillingConnected = true;
            LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Billing connected");
          }
          else
          { 
            LogSys.Log(LOG_TYPE.ERROR, "Billing connect failed: " + sys_ecode.ToString());
          }
        });
    
    }

    /// <summary>
    /// 用户登陆, 开启一个新的登陆流程
    /// </summary>
    /// <param name="msg">登陆消息</param>
    /// <param name="handle"></param>
    /// <param name="session"></param>
    private void OnLogin(JsonMessage msg, uint handle, uint session)
    {
      StringBuilder stringBuilder = new StringBuilder(1024);
      uint size = (uint)stringBuilder.Capacity;
      SvrAPI.QueryServiceNameByHandle(handle, stringBuilder, ref size);
      string node_name = stringBuilder.ToString();

      var login = msg as JsonMessageLogin;
      Authentication auth = new Authentication(login.m_Passwd, login.m_Ip, login.m_MacAddr) 
      { 
        Account = login.m_Account,
        NodeName = node_name,
        Session = session,
      };

      int index = login_sessions_.BinarySearch(auth, lmc_);
      if (index < 0)
      {
        LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "LoginAccount {0} is initiating a login", login.m_Account);

        LoginMachine s = ProcessLoginSession(auth);
        if (auth.State != LoginMachine.StateCode.Finish)
          login_sessions_.Insert(~index, s);
      }
      else
      { 
        LogSys.Log(LOG_TYPE.WARN, "Ignore multiple login request for login account {0}", login.m_Account);
        SendLoginResult(login.m_Account, node_name, session, LoginResult.LOGIN_FAIL);
      }
    }

    /// <summary>
    /// 转发消息到特定的登陆session中
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="handle"></param>
    /// <param name="session"></param>
    private void ForwardToLoginSession(JsonMessage msg, uint handle, uint session)
    {
      string account = null;
      try
      {
        account = (string)msg.GetType().InvokeMember(
            "m_Account",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField,
            null, msg, null);
      }
      catch (Exception e)
      {
        LogSys.Log(LOG_TYPE.ERROR, "JsonMessage which are send to login session are suppose to have a filed name m_Account\n" + e.ToString());
        return;
      }

      var login_session = login_sessions_.Find(s => s.Account == account);
      if (null == login_session)
      {
        // TODO: return a meaningful message to client
        LogSys.Log(LOG_TYPE.ERROR, "Login session for account {0} is not found", account);
      }
      else
      {
        login_session.OnMessage(msg);
      }
    }

    private class LMC : IComparer<LoginMachine>
    {
      public int Compare(LoginMachine left, LoginMachine right)
      {
        return left.Account.CompareTo(right.Account); 
      }
    }

    private List<LoginMachine> login_sessions_ = new List<LoginMachine>();
    private LMC lmc_ = new LMC();
  }
}