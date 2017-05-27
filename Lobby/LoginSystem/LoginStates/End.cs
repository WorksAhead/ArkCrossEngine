using System;
using DashFire.Billing;

namespace Lobby.LoginSystem
{
  class End : LoginMachine
  {
    public End() { }

    public End(string error_stage, string error)
    {
      error_ = string.Format("Account {0} login error: Stage[{1}] {2}", Account, error_stage, error);
    }

    public End(string error_stage, SysECode se, int e)
    {
      error_ = string.Format("Account {0} login error: Stage[{1}] SysECode[{2}] ECode[{3}]", Account, error_stage, se.ToString(), e);
    }

    public override void Run()
    {
      LoginResult lr = error_ == null ? LoginResult.LOGIN_SUCCESS : LoginResult.LOGIN_FAIL;
      if (error_ != null)
      {
        LogSys.Log(LOG_TYPE.ERROR, error_);
        var data_scdr = LobbyServer.Instance.DataProcessScheduler;
        data_scdr.DoUserLogoff(data_scdr.GetGuidByAccount(Account));
      }
      else
      {
        LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Account {0} login finished", Account);
      }

      Next(null);
    }

    public override void OnMessage(JsonMessage msg)
    {
      throw new System.NotImplementedException();
    }

    private string error_;
  }
}