using System;
using System.Collections.Generic;
using System.Threading;
using DashFire;
using LitJson;

namespace DashFire.Network
{
  public sealed partial class LobbyNetworkSystem
  {
    public void Init()
    {
      JsonMessageDispatcher.Init();
      NetWorkMessageInit();

      m_IsWaitStart = true;
      m_IsLogged = false;
      m_LastReceiveHeartbeatTime = 0;
    }
    public void Tick()
    {
      if (!m_IsWaitStart) {
        long curTime = TimeUtility.GetLocalMilliseconds();

        if (!IsConnected) {
          if (m_LastConnectTime + 10000 < curTime) {
            ConnectIfNotOpen();
          }
        } else {
          if (/*m_IsLogged &&*/ m_LastConnectTime + 5000 < curTime) {
            if (m_LastHeartbeatTime + 5000 < curTime) {
              m_LastHeartbeatTime = curTime;
              if (m_LastReceiveHeartbeatTime == 0) {
                m_LastReceiveHeartbeatTime = curTime;
              }
              SendHeartbeat();
            }
            if (m_LastReceiveHeartbeatTime > 0 && m_LastReceiveHeartbeatTime + 30000 < curTime) {
              //断开连接
              if (IsConnected) {
                m_WebSocket.Close();
                m_LastReceiveHeartbeatTime = 0;
              }
              //触发重连
              m_LastConnectTime = curTime - 10000;
            }
          }
        }
      }
    }
    public void Release()
    {
      if (IsConnected) {
        m_WebSocket.Close();
      }
    }
    public void QuitClient()
    {
      if (m_Guid != 0) {
        JsonMessage msg = new JsonMessage(JsonMessageID.Logout);
        JsonData jsonData = msg.m_JsonData;
        jsonData.SetJsonType(JsonType.Object);
        jsonData.Set("m_Guid", m_Guid);
        SendMessage(msg);
      }
      if (IsConnected) {
        m_WebSocket.Close();
      }
    }
    public bool IsConnected
    {
      get
      {
        bool ret = false;
        if (null != m_WebSocket)
          ret = (m_WebSocket.State == WebSocket4Net.WebSocketState.Open && m_WebSocket.IsSocketConnected);
        return ret;
      }
    }
    public ulong Guid
    {
      get { return m_Guid; }
    }

    internal bool SendMessage(string msgStr)
    {
      bool ret = false;
      if (IsConnected) {
        m_WebSocket.Send(msgStr);
        LogSystem.Info("SendToLobby {0}", msgStr);
        ret = true;
      }
      return ret;
    }

    private void ConnectIfNotOpen()
    {
      if (!IsConnected) {
        m_IsLogged = false;
        m_LastReceiveHeartbeatTime = 0;
        m_LastConnectTime = TimeUtility.GetLocalMilliseconds();
        LogSystem.Info("ConnectIfNotOpen at time {0}", m_LastConnectTime);

        m_WebSocket = new WebSocket4Net.WebSocket(m_Url);
        m_WebSocket.AllowUnstrustedCertificate = true;
        m_WebSocket.EnableAutoSendPing = true;
        m_WebSocket.AutoSendPingInterval = 10;
        m_WebSocket.Opened += OnOpened;
        m_WebSocket.MessageReceived += OnMessageReceived;
        m_WebSocket.DataReceived += OnDataReceived;
        m_WebSocket.Error += OnError;
        m_WebSocket.Closed += OnClosed;
        m_WebSocket.Open();
      }
    }
    private void SendHeartbeat()
    {
      JsonMessage msg = new JsonMessage(JsonMessageID.UserHeartbeat);
      JsonData jsonData = msg.m_JsonData;
      jsonData.SetJsonType(JsonType.Object);
      jsonData.Set("m_Guid", m_Guid);
      SendMessage(msg);
    }

    private void HandleUserHeartbeat(JsonMessage lobbyMsg)
    {
      m_LastReceiveHeartbeatTime = TimeUtility.GetLocalMilliseconds();
    }

    internal void LoginLobby(string url, string user, string pass)
    {
      if (IsConnected) {
        m_WebSocket.Close();
      }

      m_IsWaitStart = false;
      m_IsLogged = false;

      m_Url = url;
      m_User = user;
      m_Pass = pass;

      ConnectIfNotOpen();

      LogSystem.Info("LoginLobby {0} {1} {2}", url, user, pass);
    }
    internal void SelectScene(int id)
    {
      JsonMessage singlePveMsg = new JsonMessage(JsonMessageID.SinglePVE);
      JsonData jsonData = singlePveMsg.m_JsonData;
      jsonData.SetJsonType(JsonType.Object);
      jsonData.Set("m_Guid", m_Guid);
      jsonData.Set("m_SceneType", id);
      SendMessage(singlePveMsg);
    }
    internal void StartGame()
    {
      JsonMessage startGameMsg = new JsonMessage(JsonMessageID.StartGame);
      JsonData jsonData = startGameMsg.m_JsonData;
      jsonData.Set("m_Guid", m_Guid);
      SendMessage(startGameMsg);
    }
    private void HandleAccountLoginResult(JsonMessage msg)
    {
      JsonData jsonData = msg.m_JsonData;
      int ret = jsonData.GetInt("m_Result");
      if (ret == (int)AccountLoginResult.Success) {
        LogSystem.Info("Login success.");
        //登录成功，向服务器请求玩家角色
        JsonMessage sendMsg = new JsonMessage(JsonMessageID.RoleList);
        sendMsg.m_JsonData["m_Account"] = m_User;
        SendMessage(sendMsg);

        LogSystem.Info("HandleAccountLoginResult, success.");
      } else if (ret == (int)AccountLoginResult.FirstLogin) {
        //账号首次登录，需要验证激活码        

        LogSystem.Info("HandleAccountLoginResult, need activate code.");
      } else {
        //账号登录失败
        Thread.Sleep(1000);

        JsonMessage loginMsg = new JsonMessage(JsonMessageID.DirectLogin);
        loginMsg.m_JsonData["m_Account"] = m_User;
        SendMessage(loginMsg);

        LogSystem.Info("HandleAccountLoginResult, failed, Relogin...");
      }
    }
    private void HandleRoleListResult(JsonMessage msg)
    {
      JsonData jsonData = msg.m_JsonData;
      DashFireMessage.Msg_LC_RoleListResult protoData = msg.m_ProtoData as DashFireMessage.Msg_LC_RoleListResult;
      if (null != protoData) {
        int ret = protoData.m_Result;
        if (ret == (int)RoleListResult.Success) {
          //获取玩家角色数据列表 
          int userinfoCount = protoData.m_UserInfoCount;
          List<DashFireMessage.Msg_LC_RoleListResult.UserInfoForMessage> userInfos = protoData.m_UserInfos;
          if (userInfos.Count > 0) {
            for (int i = 0; i < userInfos.Count; ++i) {
              DashFireMessage.Msg_LC_RoleListResult.UserInfoForMessage ui = userInfos[i];
              ulong guid = ui.m_UserGuid;

              JsonMessage sendMsg = new JsonMessage(JsonMessageID.RoleEnter);
              sendMsg.m_JsonData["m_Account"] = m_User;
              DashFireMessage.Msg_CL_RoleEnter protoMsg = new DashFireMessage.Msg_CL_RoleEnter();
              protoMsg.m_Guid = guid;
              sendMsg.m_ProtoData = protoMsg;
              SendMessage(sendMsg);
            }
          } else {
            JsonMessage sendMsg = new JsonMessage(JsonMessageID.CreateRole);
            sendMsg.m_JsonData["m_Account"] = m_User;
            sendMsg.m_JsonData["m_HeroId"] = 1;
            sendMsg.m_JsonData["m_Nickname"] = "Nick_" + m_User;
            SendMessage(sendMsg);

            LogSystem.Info("Create Role {0}", m_User);
          }
        }
      }
      LogSystem.Info("HandleRoleListResult");
    }
    private void HandleCreateRoleResult(JsonMessage msg)
    {
      JsonData jsonData = msg.m_JsonData;

      int ret = jsonData.GetInt("m_Result");
      if (ret == (int)CreateRoleResult.Success) {
        JsonData userInfo = jsonData["m_UserInfo"];
        ulong userGuid = userInfo.GetUlong("m_UserGuid");

        JsonMessage sendMsg = new JsonMessage(JsonMessageID.RoleEnter);
        sendMsg.m_JsonData["m_Account"] = m_User;
        DashFireMessage.Msg_CL_RoleEnter protoData = new DashFireMessage.Msg_CL_RoleEnter();
        protoData.m_Guid = userGuid;
        sendMsg.m_ProtoData = protoData;
        SendMessage(sendMsg);

        LogSystem.Info("Role Enter {0} {1}", m_User, userGuid);
      } else {
        LogSystem.Info("Role {0} HandleCreateRoleResult failed !", m_User);
      }
    }
    private void HandleRoleEnterResult(JsonMessage msg)
    {
      JsonData jsonData = msg.m_JsonData;
      DashFireMessage.Msg_LC_RoleEnterResult protoData = msg.m_ProtoData as DashFireMessage.Msg_LC_RoleEnterResult;
      if (null != protoData) {
        int ret = protoData.m_Result;
        if (ret == (int)RoleEnterResult.Success) {
          ulong userGuid = jsonData.GetUlong("m_Guid");
          m_Guid = userGuid;
        }
      }
      m_IsLogged = true;
    }
    private void HandleMatchResult(JsonMessage msg)
    {
      LogSystem.Info("HandleFindTeamResult");
      StartGame();
    }
    private void HandleStartGameResult(JsonMessage msg)
    {
      JsonData jsonData = msg.m_JsonData;
      DashFireMessage.Msg_LC_StartGameResult protoData = msg.m_ProtoData as DashFireMessage.Msg_LC_StartGameResult;
      if (null != protoData) {
        uint key = protoData.key;
        string ip = protoData.server_ip;
        uint port = protoData.server_port;
        int heroId = protoData.hero_id;
        int campId = protoData.camp_id;
        int sceneId = protoData.scene_type;
      }
      LogSystem.Info("HandleStartGameResult");
    }
    private void NetWorkMessageInit()
    {
      RegisterMsgHandler(JsonMessageID.UserHeartbeat, HandleUserHeartbeat);
      RegisterMsgHandler(JsonMessageID.AccountLoginResult, HandleAccountLoginResult);
      RegisterMsgHandler(JsonMessageID.RoleListResult, typeof(DashFireMessage.Msg_LC_RoleListResult), HandleRoleListResult);
      RegisterMsgHandler(JsonMessageID.CreateRoleResult, HandleCreateRoleResult);
      RegisterMsgHandler(JsonMessageID.RoleEnterResult, typeof(DashFireMessage.Msg_LC_RoleEnterResult), HandleRoleEnterResult);
      RegisterMsgHandler(JsonMessageID.MatchResult, HandleMatchResult);
      RegisterMsgHandler(JsonMessageID.StartGameResult, typeof(DashFireMessage.Msg_LC_StartGameResult), HandleStartGameResult);
    }
    private void RegisterMsgHandler(JsonMessageID id, JsonMessageHandlerDelegate handler)
    {
      JsonMessageDispatcher.RegisterMessageHandler((int)id, null, handler);
    }
    private void RegisterMsgHandler(JsonMessageID id, Type protoType, JsonMessageHandlerDelegate handler)
    {
      JsonMessageDispatcher.RegisterMessageHandler((int)id, protoType, handler);
    }
    private void SendMessage(JsonMessage msg)
    {
      try {
        JsonMessageDispatcher.SendMessage(msg);
      } catch (Exception ex) {
        LogSystem.Error("LobbyNetworkSystem.SendMessage throw Exception:{0}\n{1}", ex.Message, ex.StackTrace);
      }
    }

    private void OnOpened(object sender, EventArgs e)
    {
      JsonMessage loginMsg = new JsonMessage(JsonMessageID.DirectLogin);
      loginMsg.m_JsonData["m_Account"] = m_User;
      SendMessage(loginMsg);

      LogSystem.Info("LobbyConnect opened.");
    }
    private void OnError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
    {
      if (null != e.Exception) {
        LogSystem.Error("LobbyNetworkSystem.OnError Exception:{0}\n{1}", e.Exception.Message, e.Exception.StackTrace);
      } else {
        LogSystem.Error("LobbyNetworkSystem.OnError (Unknown)");
      }
    }
    private void OnClosed(object sender, EventArgs e)
    {
      LogSystem.Error("LobbyNetworkSystem.OnClosed");
    }
    private void OnHeartBeatTimer(object sender, EventArgs e)
    {
      LogSystem.Info("Send Socket.io heartbeat");
    }
    private void OnMessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
    {
      string msg = e.Message;
      if (null != msg) {
        LogSystem.Info("Receive Lobby Message:{0}", msg);
        JsonMessageDispatcher.HandleNodeMessage(msg);
      }
    }

    private void OnDataReceived(object sender, WebSocket4Net.DataReceivedEventArgs e)
    {
      LogSystem.Info("Receive Data Message:\n{0}", Helper.BinToHex(e.Data));
    }

    private bool m_IsWaitStart = true;
    private bool m_IsLogged = false;
    private long m_LastConnectTime = 0;
    private long m_LastHeartbeatTime = 0;
    private long m_LastReceiveHeartbeatTime = 0;

    private string m_Url;
    private string m_User = "WebsocketTest";
    private string m_Pass;
    private ulong m_Guid;

    private WebSocket4Net.WebSocket m_WebSocket;

    public static LobbyNetworkSystem Instance
    {
      get
      {
        return s_Instance;
      }
    }
    private static LobbyNetworkSystem s_Instance = new LobbyNetworkSystem();
  }
}
