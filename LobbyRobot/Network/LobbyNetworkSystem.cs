using System;
using System.Collections.Generic;
using LitJson;
using DashFire;
using DashFire.GmCommands;

namespace DashFire.Network
{
  internal sealed class LobbyNetworkSystem
  {
    internal void Init(IActionQueue asyncQueue)
    {
      //WebSocket的事件不是在当前线程触发的，我们需要自己进行线程调整
      m_AsyncActionQueue = asyncQueue;

      MessageDispatcher.Init();

      m_IsWaitStart = true;
      m_HasLoggedOn = false;
      m_IsLogining = false;
      m_IsQueueing = false;
      m_LastReceiveHeartbeatTime = 0;
      m_LastQueueingTime = 0;

      RegisterMsgHandler(JsonMessageID.UserHeartbeat, null, HandleUserHeartbeat);
    }
    internal void Tick()
    {
      if (!m_IsWaitStart) {
        long curTime = TimeUtility.GetLocalMilliseconds();

        if (!IsConnected) {
          if (m_LastConnectTime + 10000 < curTime) {
            ConnectIfNotOpen();
          }
        } else {
          if (m_IsQueueing) {
            if (m_LastQueueingTime + 5000 < curTime) {
              m_LastQueueingTime = curTime;

              SendGetQueueingCount();
            }
          }
          if (m_HasLoggedOn && !m_IsLogining && m_LastConnectTime + 5000 < curTime) {
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
    internal void Disconnect()
    {
      if (IsConnected) {
        m_WebSocket.Close();
      }
    }
    internal void QuitRoom()
    {
      if (m_Guid != 0) {
        JsonMessage msg = new JsonMessage(JsonMessageID.QuitRoom);
        msg.m_JsonData.SetJsonType(JsonType.Object);
        msg.m_JsonData.Set("m_Guid", m_Guid);
        DashFireMessage.Msg_CL_QuitRoom protoMsg = new DashFireMessage.Msg_CL_QuitRoom();
        protoMsg.m_IsQuitRoom = false;
        msg.m_ProtoData = protoMsg;
        SendMessage(msg);
      }
    }
    internal void QuitClient()
    {
      if (m_Guid != 0) {
        JsonMessage msg = new JsonMessage(JsonMessageID.Logout);
        msg.m_JsonData.SetJsonType(JsonType.Object);
        msg.m_JsonData.Set("m_Guid", m_Guid);
        SendMessage(msg);
      }
      if (m_User != string.Empty) {
        JsonMessage msg = new JsonMessage(JsonMessageID.AccountLogout);
        msg.m_JsonData.SetJsonType(JsonType.Object);
        msg.m_JsonData.Set("m_Account", m_User);
        SendMessage(msg);
      }
      //m_IsWaitStart = true;
      m_IsWaitStart = false;
      m_HasLoggedOn = false;
      if (IsConnected) {
        m_WebSocket.Close();
      }
    }
    internal long LastConnectTime
    {
      get { return m_LastConnectTime; }
    }
    internal long LastHeartbeatTime
    {
      get { return m_LastHeartbeatTime; }
    }
    internal bool IsConnected
    {
      get
      {
        bool ret = false;
        if (null != m_WebSocket)
          ret = (m_WebSocket.State == WebSocket4Net.WebSocketState.Open && m_WebSocket.IsSocketConnected);
        return ret;
      }
    }
    internal bool IsQueueing
    {
      get { return m_IsQueueing; }
      set { m_IsQueueing = value; }
    }
    internal bool IsLogining
    {
      get { return m_IsLogining; }
      set { m_IsLogining = value; }
    }
    internal bool HasLoggedOn
    {
      get { return m_HasLoggedOn; }
      set { m_HasLoggedOn = value; }
    }
    internal string User
    {
      get { return m_User; }
    }
    internal ulong Guid
    {
      get { return m_Guid; }
      set { m_Guid = value; }
    }
    internal void RegisterMsgHandler(JsonMessageID id, Type protoType, JsonMessageHandlerDelegate handler)
    {
      MessageDispatcher.RegisterMessageHandler((int)id, protoType, handler);
    }
    internal void SendMessage(JsonMessage msg)
    {
      string msgStr = MessageDispatcher.BuildNodeMessage(msg);
      SendMessage(msgStr);
    }
    internal void LoginLobby(string url, string user, string pass)
    {
      if (IsConnected) {
        m_WebSocket.Close();
      }

      m_IsWaitStart = false;
      m_HasLoggedOn = false;

      m_Url = url;
      m_User = user;
      m_Pass = pass;

      ConnectIfNotOpen();

      LogSystem.Info("LoginLobby {0} {1} {2} {3}", url, user, pass, LobbyRobot.Robot.GetDateTime());
    }
    internal void ConnectIfNotOpen()
    {
      if (!IsConnected) {
        m_LastReceiveHeartbeatTime = 0;
        m_LastQueueingTime = 0;
        m_IsLogining = true;
        m_IsQueueing = false;
        m_LastConnectTime = TimeUtility.GetLocalMilliseconds();
        LogSystem.Info("ConnectIfNotOpen at time {0} {1} {2}", m_LastConnectTime, m_User, LobbyRobot.Robot.GetDateTime());

        m_WebSocket = new WebSocket4Net.WebSocket(m_Url);
        m_WebSocket.AllowUnstrustedCertificate = true;
        m_WebSocket.EnableAutoSendPing = true;
        m_WebSocket.AutoSendPingInterval = 10;
        m_WebSocket.Opened += OnWsOpened;
        m_WebSocket.MessageReceived += OnWsMessageReceived;
        m_WebSocket.DataReceived += OnWsDataReceived;
        m_WebSocket.Error += OnWsError;
        m_WebSocket.Closed += OnWsClosed;
        m_WebSocket.Open();
      }
    }

    private bool SendMessage(string msgStr)
    {
      bool ret = false;
      if (IsConnected) {
        m_WebSocket.Send(msgStr);
        //LogSystem.Info("SendToLobby {0}", msgStr);
        ret = true;
      }
      return ret;
    }

    private void SendGetQueueingCount()
    {
      JsonMessage msg = new JsonMessage(JsonMessageID.GetQueueingCount);
      msg.m_JsonData.SetJsonType(JsonType.Object);
      msg.m_JsonData.Set("m_Account", m_User);
      SendMessage(msg);
    }

    private void SendHeartbeat()
    {
      JsonMessage msg = new JsonMessage(JsonMessageID.UserHeartbeat);
      msg.m_JsonData.SetJsonType(JsonType.Object);
      msg.m_JsonData.Set("m_Guid", m_Guid);
      SendMessage(msg);
    }

    private void HandleUserHeartbeat(JsonMessage lobbyMsg)
    {
      m_LastReceiveHeartbeatTime = TimeUtility.GetLocalMilliseconds();
    }

    private void OnOpened()
    {
      JsonMessage loginMsg = new JsonMessage(JsonMessageID.DirectLogin);
      loginMsg.m_JsonData["m_Account"] = m_User;
      SendMessage(loginMsg);

      LogSystem.Info("LobbyConnect opened, DirectLogin {0}. {1}", m_User, LobbyRobot.Robot.GetDateTime());
    }
    private void OnError(Exception ex)
    {
      if (null != ex) {
        LogSystem.Error("{0} LobbyNetworkSystem.OnError Exception:{1} {2}\n{3}", m_User, ex.Message, LobbyRobot.Robot.GetDateTime(), ex.StackTrace);
      } else {
        LogSystem.Error("{0} LobbyNetworkSystem.OnError (Unknown) {1}", m_User, LobbyRobot.Robot.GetDateTime());
      }
    }
    private void OnClosed()
    {
    }
    private void OnMessageReceived(string msg)
    {
      if (null != msg) {
        //LogSystem.Info("Receive Lobby Message:{0}", msg);
        MessageDispatcher.HandleNodeMessage(msg);
      }
    }
    private void OnDataReceived(byte[] data)
    {
      LogSystem.Info("Receive Data Message {0}:\n{1}", LobbyRobot.Robot.GetDateTime(), Helper.BinToHex(data));
    }

    private void OnWsOpened(object sender, EventArgs e)
    {
      if (null != m_AsyncActionQueue) {
        m_AsyncActionQueue.QueueActionWithDelegation((MyAction)this.OnOpened);
      }
    }
    private void OnWsError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
    {
      if (null != m_AsyncActionQueue) {
        m_AsyncActionQueue.QueueActionWithDelegation((MyAction<Exception>)this.OnError, e.Exception);
      }
    }
    private void OnWsClosed(object sender, EventArgs e)
    {
      if (null != m_AsyncActionQueue) {
        m_AsyncActionQueue.QueueActionWithDelegation((MyAction)this.OnClosed);
      }
    }
    private void OnWsMessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
    {
      if (null != m_AsyncActionQueue) {
        m_AsyncActionQueue.QueueActionWithDelegation((MyAction<string>)this.OnMessageReceived, e.Message);
      }
    }
    private void OnWsDataReceived(object sender, WebSocket4Net.DataReceivedEventArgs e)
    {
      if (null != m_AsyncActionQueue) {
        m_AsyncActionQueue.QueueActionWithDelegation((MyAction<byte[]>)this.OnDataReceived, e.Data);
      }
    }
    private DashFire.Network.JsonMessageDispatcher MessageDispatcher
    {
      get { return m_MessageDispatcher; }
    }


    private bool m_IsWaitStart = true;
    private bool m_IsQueueing = false;
    private bool m_IsLogining = false;
    private bool m_HasLoggedOn = false;
    private long m_LastConnectTime = 0;
    private long m_LastQueueingTime = 0;
    private long m_LastHeartbeatTime = 0;
    private long m_LastReceiveHeartbeatTime = 0;

    private string m_Url;
    private string m_User;
    private string m_Pass;
    private ulong m_Guid;

    private WebSocket4Net.WebSocket m_WebSocket;
    private IActionQueue m_AsyncActionQueue;
    private JsonMessageDispatcher m_MessageDispatcher = new JsonMessageDispatcher();
  }
}
