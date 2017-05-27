using System;

using Options;
using DashFire.Billing;

namespace Lobby.LoginSystem
{
  class Authentication : LoginMachine
  {
    public Authentication(string passwd, string ip, string mac_addr)
    {
      passwd_ = passwd;
      ip_ = ip;
      mac_addr_ = mac_addr;
      LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Account {0} login state: {1}", Account, GetType().Name);
    }

    public override void Run()
    {
      if (OptionCollection.Instance.Get<LobbyConfig>().BillingDisable)
      {
        LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Authenticate account {0} skipped, we should start loading directly", Account);
        LoginSystem.SendLoginResult(Account, NodeName, Session, LoginResult.LOGIN_SUCCESS);
        Next("Loading", default(Player));
      }
      else
      {
        if (!LoginSystem.Instance.BillingConnected)
        {
          LoginSystem.SendLoginResult(Account, NodeName, Session, LoginResult.LOGIN_FAIL);
          Next("End", GetType().Name, SysECode.ConnectBillingTimeout, 0);
        }
        else
        {
          BillingClient.Instance.PlayerAuthentication(
              Account, passwd_, ip_, 0, null, mac_addr_,
              (se, e, p) => 
              {
                if (se != SysECode.Good || (e != 0x01 && e != 0xf1))
                {
                  LoginResult lr = LoginResult.LOGIN_FAIL;
                  if (e == 0x02)
                  {
                    lr = LoginResult.LOGIN_USER_ERROR;
                  }
                  else if (e == 0x03)
                  {
                    lr = LoginResult.LOGIN_PWD_ERROR;
                  }
                  LoginSystem.SendLoginResult(Account, NodeName, Session, lr);
                  Next("End", GetType().Name, se, e);
                }
                else if (!p.ProtocolConfirm)
                {
                  Next("CYPConfirm", p);
                }
                else
                {
                  LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "LoginAccount {0} authenticated, Internal Account is {1}", p.LoginAccount, p.Account);
                  LoginSystem.SendLoginResult(Account, NodeName, Session, LoginResult.LOGIN_SUCCESS);
                  Next("Loading", p);
                }
              });
          Pending();
        }
      }
    }

    public override void OnMessage(JsonMessage msg)
    {
      throw new System.NotImplementedException();
    }

    private string passwd_;
    private string ip_;
    private string mac_addr_;
  }
}