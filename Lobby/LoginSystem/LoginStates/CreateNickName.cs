using System;
using DashFire.Billing;
using DashFire.DataStore;

namespace Lobby.LoginSystem
{
  class CreateNickName : LoginMachine
  {
    private static ulong s_tempguid_ = 0;

    public CreateNickName(Player p)
    {
      billing_player_ = p; 
      LogSys.Log(LOG_TYPE.DEBUG, ConsoleColor.Cyan, "Account {0} login state: {1}", Account, GetType().Name);
    }

    public override void Run()
    {
      //测试版本取消自定义昵称步骤，改为直接指定账号为玩家昵称
      CreateNickNameAuto();      
      //正常流程（代码保留）
      /*
      JsonMessageCreateNick createNickMsg = new JsonMessageCreateNick();
      createNickMsg.m_Account = Account;
      JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, createNickMsg);
      Pending(300);
      */
    }

    #region 三十分钟版本    
    public void CreateNickNameAuto()
    {
      //直接指定玩家账号为玩家昵称
      string autoNickname = Account;
      if (LobbyConfig.DataStoreAvailable)
      {
        var dsc = LobbyServer.Instance.DataStoreConnector;
        dsc.Load<DS_Nickname>(autoNickname, (e,d) => LoadNicknameCallbackAuto(e, d, autoNickname));
        Pending();
      }
      else
      {
        var data_scdr = LobbyServer.Instance.DataProcessScheduler;
        var guid = data_scdr.GetGuidByNick(autoNickname);
        if (guid == 0)
        {
          //创建新的玩家角色
          ++s_tempguid_;
          var user = data_scdr.NewUserInfo();
          user.BillingPlayer = billing_player_;
          user.Guid = s_tempguid_;
          user.Account = Account;
          user.Nickname = autoNickname;
          user.NodeName = NodeName;
          user.Sign = "Dash Fire";
          user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
          data_scdr.DoUserLogin(user);

          JsonMessageUserInfo userInfoMsg = new JsonMessageUserInfo();
          userInfoMsg.m_Account = user.Account;
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
          //理论上不会出现重复昵称
          string errorMsg = "Duplicated nickname:" + autoNickname;
          LogSys.Log(LOG_TYPE.ERROR, errorMsg);
          Next("End", GetType().Name, errorMsg);
        }
      }
    }
    private void LoadNicknameCallbackAuto(string error, DS_Nickname data, string nickname)
    {
      if (null != error)
      {
        //nickname在数据库中不存在，可以作为新的昵称
        LogSys.Log(LOG_TYPE.ERROR, "Load {0} data failed: {1}", nickname, error);        
        DSCreateNewUser(nickname);
      }
      else
      {
        //若数据库中已有该昵称，则向客户端提示错误
        string errorMsg = "Duplicated nickname: " + nickname;
        LogSys.Log(LOG_TYPE.ERROR, errorMsg);
        Next("End", GetType().Name, errorMsg);
      }
    }
    #endregion

    public override void OnMessage(JsonMessage msg)
    {
      var reply = msg as JsonMessageCreateNick;
      if (LobbyConfig.DataStoreAvailable)
      {
        var dsc = LobbyServer.Instance.DataStoreConnector;
        dsc.Load<DS_Nickname>(reply.m_Nick, (e, d) => LoadNicknameCallback(e, d, reply.m_Nick));
      }
      else
      {
        var data_scdr = LobbyServer.Instance.DataProcessScheduler;
        var guid = data_scdr.GetGuidByNick(reply.m_Nick);
        if (guid == 0)
        {
          JsonMessageCreateNickResult cnResultMsg = new JsonMessageCreateNickResult();
          cnResultMsg.m_Account = Account;
          cnResultMsg.m_Result = (int)CreateNickResult.NICK_SUCCESS;
          cnResultMsg.m_Nick = reply.m_Nick;
          JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, cnResultMsg);

          //创建新的玩家角色
          ++s_tempguid_;
          var user = data_scdr.NewUserInfo();
          user.BillingPlayer = billing_player_;
          user.Guid = s_tempguid_;
          user.Account = Account;
          user.Nickname = reply.m_Nick;
          user.NodeName = NodeName;
          user.Sign = "Dash Fire";
          user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
          data_scdr.DoUserLogin(user);

          JsonMessageUserInfo userInfoMsg = new JsonMessageUserInfo();
          userInfoMsg.m_Account = user.Account;
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
          LogSys.Log(LOG_TYPE.ERROR, "Duplicated nickname: {0}", reply.m_Nick);
          JsonMessageCreateNickResult cnResultMsg = new JsonMessageCreateNickResult();
          cnResultMsg.m_Account = Account;
          cnResultMsg.m_Result = (int)CreateNickResult.NICK_REPEAT_ERROR;
          cnResultMsg.m_Nick = reply.m_Nick;
          JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, cnResultMsg);          
        }
      }
    }

    private void LoadNicknameCallback(string error, DS_Nickname data, string nickname)
    {
      if (null != error)
      {
        LogSys.Log(LOG_TYPE.ERROR, "Load {0} data failed: {1}", nickname, error);
        //若数据库中该昵称不存在，则可以创建新的
        JsonMessageCreateNickResult cnResultMsg = new JsonMessageCreateNickResult();
        cnResultMsg.m_Account = Account;
        cnResultMsg.m_Result = (int)CreateNickResult.NICK_SUCCESS;
        cnResultMsg.m_Nick = nickname;
        JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, cnResultMsg);
        DSCreateNewUser(nickname);
      }
      else
      {
        //若数据库中已有该昵称，则向客户端提示错误
        LogSys.Log(LOG_TYPE.ERROR, "Duplicated nickname: {0}", nickname);
        JsonMessageCreateNickResult cnResultMsg = new JsonMessageCreateNickResult();
        cnResultMsg.m_Account = Account;
        cnResultMsg.m_Result = (int)CreateNickResult.NICK_REPEAT_ERROR;
        cnResultMsg.m_Nick = nickname;
        JsonMessageDispatcher.SendDcoreMessage(LobbyServer.Instance.SvrAPI, NodeName, cnResultMsg);        
      }
    }

    private void DSCreateNewUser(string nickname)
    {     
      var dsc = LobbyServer.Instance.DataStoreConnector;
      dsc.NextGuid<DS_UserInfo>((e, g) => NextGuidCallback(e, g, nickname));
    }

    private void NextGuidCallback(string error, long guid, string nickname)
    {
      var data_scdr = LobbyServer.Instance.DataProcessScheduler;
      if (null == error)
      {
        UserInfo user = data_scdr.NewUserInfo();
        user.BillingPlayer = billing_player_;
        user.Account = Account;
        user.Guid = (ulong)guid;  //从数据库得到的guid起始值为0，+1使其从1开始
        user.Level = 1;
        user.Nickname = nickname;
        user.Sign = "Dash Fire DS";
        user.NodeName = NodeName;
        user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
        data_scdr.DoUserLogin(user);
        //一个新玩家信息创建完成，保存到数据库中
        var ds_thread = LobbyServer.Instance.DataStoreThread;
        ds_thread.QueueAction(ds_thread.DSSaveAccount, user);
        ds_thread.QueueAction(ds_thread.DSSaveUserInfo, user);        
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
      else
      {
        Next("End", GetType().Name, error);
      }
    }

    private Player billing_player_;
  }
}