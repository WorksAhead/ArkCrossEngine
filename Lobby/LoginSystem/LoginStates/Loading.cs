using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using DashFire.Billing;
using DashFire.DataStore;
using System.Text;

namespace Lobby.LoginSystem
{
  class Loading : LoginMachine 
  {
    public Loading(Player p)
    {
      billing_player_ = p; 
      LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Account {0} login state: {1}", Account, GetType().Name);
    }

    public override void Run()
    {
      var data_scdr = LobbyServer.Instance.DataProcessScheduler;
      ulong guid = data_scdr.GetGuidByAccount(Account);
      if (guid != 0)
      {
        LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Account {0} is already online, do BillingOnline", Account);
        AlreadyOnline(guid);
      }
      else
      {
        if (LobbyConfig.DataStoreAvailable)
        {
          LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Load account {0}", Account);
          var dsc = LobbyServer.Instance.DataStoreConnector;
          dsc.Load<DS_Account>(Account, LoadAccountCallback);
          Pending();
        }
        else
        {
          LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "DataStore not available, do CreateNickName for account {0}", Account);
          Next("CreateNickName", billing_player_);
        }
      }
    }

    public override void OnMessage(JsonMessage msg)
    {
      throw new System.NotImplementedException();
    }

    private void LoadAccountCallback(string error, DS_Account data)
    {
      if (null != error)
      {
        LogSys.Log(LOG_TYPE.INFO, "Load account : {0} failed: {1}", Account, error);
        Next("CreateNickName", billing_player_);
      }
      else
      { 
        LogSys.Log(LOG_TYPE.INFO, "Load aoount : {0} success: {1}", Account, data.Guid);
        LoadUserInfo(data.Guid); 
      }
    }

    private void LoadUserInfo(long guid)
    { 
      DataStoreClient dsc = LobbyServer.Instance.DataStoreConnector;
      dsc.Load<DS_UserInfo>(guid.ToString(), (e, d) => LoadUserInfoCallback(e, d, guid));
    }

    private void LoadUserInfoCallback(string error, DS_UserInfo data, long guid)
    { 
      var data_scdr = LobbyServer.Instance.DataProcessScheduler;
      if (null != error)
      {
        Next("End", GetType().Name, error);
      }
      else
      {
        LogSys.Log(LOG_TYPE.INFO, "Load {0} data success: {1}", guid, data.Account);
        UserInfo user = data_scdr.NewUserInfo();
        user.BillingPlayer = billing_player_;
        user.Guid = (ulong)data.Guid;
        user.Account = data.Account;
        user.Nickname = data.Nickname;
        user.Level = data.Level;
        user.ExpPoints = data.Exp;
        user.Gold = data.Gold;        
        user.NodeName = NodeName;
        user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
        data_scdr.DoUserLogin(user);            
        ///////////////////////
        //向客户端发送玩家数据
        JsonMessageUserInfo userInfoMsg = new JsonMessageUserInfo();
        userInfoMsg.m_Account = user.Account;
        userInfoMsg.m_Guid = user.Guid;
        userInfoMsg.m_Nick = user.Nickname;
        userInfoMsg.m_Level = user.Level;
        userInfoMsg.m_Sign = user.Sign;
        JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, userInfoMsg);
        LogSys.Log(LOG_TYPE.DEBUG, "DoAccountLogin,User New! guid:{0},nick:{1},acc:{2},node:{3}", user.Guid, user.Nickname, user.Account, user.NodeName);

        Next("BillingOnline", user);
      }
    }

    private WeaponInfo GetWeaponInfoByID(List<WeaponInfo> weaponInfos, int weaponID)
    {     
      foreach (var wp in weaponInfos)
      {
        if (wp.WeaponId == weaponID)
        {
          return wp;
        }
      }     
      return null;
    }

    private void AlreadyOnline(ulong guid)
    { 
      var data_scdr = LobbyServer.Instance.DataProcessScheduler;
      UserInfo user = data_scdr.GetUserInfo(guid);
      //处理账号重复登录的问题：根据玩家当前状态分别处理
      if (user.CurrentState == UserState.Quit)
      {
        //GameClient中途退出，玩家状态为Quit
        user.BillingPlayer = billing_player_;
        user.NodeName = NodeName;
        user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
        JsonMessageUserInfo userInfoMsg = new JsonMessageUserInfo();
        userInfoMsg.m_Account = Account;
        userInfoMsg.m_Guid = user.Guid;
        userInfoMsg.m_Nick = user.Nickname;
        userInfoMsg.m_Level = user.Level;
        userInfoMsg.m_Sign = user.Sign;
        JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, userInfoMsg);
        LogSys.Log(LOG_TYPE.DEBUG, "DoAccountLogin,guid:{0},nick:{1},acc:{2},node:{3}", user.Guid, user.Nickname, user.Account, user.NodeName);
        //向LobbyClient发送断线重连消息
        JsonMessageUserQuitRoom uqrMsg = new JsonMessageUserQuitRoom();
        uqrMsg.m_Guid = user.Guid;
        uqrMsg.m_RoomID = user.CurrentRoomID;
        uqrMsg.m_IsEndQuit = false;
        JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, user.NodeName, uqrMsg);
        Next("BillingOnline", user);
      }
      else if (user.CurrentState == UserState.Game && user.LeftLife <= 0)
      {
        //LobbyClient中途退出，GameClient仍在，玩家状态为Game
        //应验证LobbyClient是同一个IP
        user.BillingPlayer = billing_player_;
        user.NodeName = NodeName;
        user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
        JsonMessageUserInfo userInfoMsg = new JsonMessageUserInfo();
        userInfoMsg.m_Account = Account;
        userInfoMsg.m_Guid = user.Guid;
        userInfoMsg.m_Nick = user.Nickname;
        userInfoMsg.m_Level = user.Level;
        userInfoMsg.m_Sign = user.Sign;
        JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, userInfoMsg);
        LogSys.Log(LOG_TYPE.DEBUG, "DoAccountLogin,guid:{0},nick:{1},acc:{2},node:{3}", user.Guid, user.Nickname, user.Account, user.NodeName);
        Next("BillingOnline", user);
      }
      else
      {
        //向第二个登录的玩家返回重复登录的错误信息
        JsonMessageAccountRepeatLogin accRepeatLoginMsg = new JsonMessageAccountRepeatLogin();
        accRepeatLoginMsg.m_Account = Account;
        JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, accRepeatLoginMsg);

        string errorMsg = "Account already online : " + Account;
        LogSys.Log(LOG_TYPE.ERROR, errorMsg);
        Next("End");
      }     
    }      
    Player billing_player_;
  }
}