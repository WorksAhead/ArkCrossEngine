using System;
using DashFire.Billing;

namespace Lobby.LoginSystem
{
  // TODO: add timeout 
  class CYPConfirm : LoginMachine
  {
    public CYPConfirm(Player p) 
    {
      billing_player_ = p;
      LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Account {0} login state: {1}", Account, GetType().Name);
    }

    public override void Run()
    { 
      JsonMessageCYPConfirm request = new JsonMessageCYPConfirm();
      request.m_Account = Account;
      JsonMessageDispatcher.SendDcoreMessage(
          LoginSystem.Instance.SvrAPI,
          NodeName,
          request,
          Session);

      Pending(); 
    }

    public override void OnMessage(JsonMessage msg)
    {
      var reply = msg as JsonMessageCYPConfirmResult;
      if (!reply.m_Confirm)
      {
        LoginSystem.SendLoginResult(Account, NodeName, Session, LoginResult.LOGIN_FAIL);
        Next("End", GetType().Name, SysECode.Good, -1);
      }
      else
      { 
        LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Account {0} accept cy protocol, we should start loading", Account);
        LoginSystem.SendLoginResult(Account, NodeName, Session, LoginResult.LOGIN_SUCCESS);
        Next("Loading", billing_player_);
      }
    }

    private Player billing_player_;
  }
}