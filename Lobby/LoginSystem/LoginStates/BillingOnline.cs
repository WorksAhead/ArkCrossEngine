using System;
using Options;
using DashFire.Billing;

namespace Lobby.LoginSystem
{
  class BillingOnline : LoginMachine 
  {
    public BillingOnline(UserInfo user)
    {
      user_ = user;
      LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Account {0} login state: {1}", Account, GetType().Name);
    }

    public override void Run()
    {
      if (OptionCollection.Instance.Get<LobbyConfig>().BillingDisable)
      {
        Next("End");
      }
      else
      {
        user_.BillingPlayer.Online((long)user_.Guid, user_.Nickname, OnlineCallback);
        Pending();
      }
    }

    public override void OnMessage(JsonMessage msg)
    {
      throw new System.NotImplementedException();
    }

    private void OnlineCallback(SysECode sys_ecode, OnlineECode ecode, Player p)
    {
      if (sys_ecode != SysECode.Good || ecode != OnlineECode.Succeed)
        Next("End", GetType().Name, sys_ecode, ecode);
      else
        Next("End");
    }

    private UserInfo user_;
  }
}