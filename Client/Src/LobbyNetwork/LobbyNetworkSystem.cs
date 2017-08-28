using System;
using LitJson;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace ArkCrossEngine.Network
{
    internal sealed partial class LobbyNetworkSystem
    {
        internal void Init(IActionQueue asyncQueue)
        {
            //WebSocket的事件不是在当前线程触发的，我们需要自己进行线程调整
            m_AsyncActionQueue = asyncQueue;

            JsonMessageDispatcher.Init();
            LobbyMessageInit();
            FriendMessageInit();
            GroupMessageInit();
            PartnerMessageInit();
            ActivityMessageInit();
            m_IsWaitStart = true;
            m_HasLoggedOn = false;
            m_IsLogining = false;
            m_IsQueueing = false;
            m_LastReceiveHeartbeatTime = 0;
            m_LastQueueingTime = 0;
            m_LastShowQueueingNum = 0;
        }
        internal void ConnectIfNotOpen()
        {
            if (!IsConnected)
            {
                m_LastReceiveHeartbeatTime = 0;
                m_LastQueueingTime = 0;
                m_LastShowQueueingNum = 0;
                m_LastConnectTime = TimeUtility.GetLocalMilliseconds();
                LogSystem.Debug("ConnectIfNotOpen at time:{0} ServerAddress:{1}", m_LastConnectTime, m_Url);

                WorldSystem.Instance.WaitMatchSceneId = -1;
                m_IsLogining = true;
                m_IsQueueing = false;
#if !PLATFORM_WEBGL
                m_WebSocket = new WebSocket4Net.WebSocket(m_Url);
#else
                m_WebSocket = WebSocketWrapper.Instance;
                m_WebSocket.SetUrl(m_Url);
#endif
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
        internal void Tick()
        {
            if (!m_IsWaitStart)
            {
                long curTime = TimeUtility.GetLocalMilliseconds();

                if (!IsConnected)
                {
                    if (m_LastConnectTime + 10000 < curTime)
                    {
                        if (WorldSystem.Instance.IsServerSelectScene())
                        {
                            GfxSystem.PublishGfxEvent("ge_ui_serverselect_network_failed", "ui");
                            m_IsWaitStart = true;
                            m_HasLoggedOn = false;
                        }
                        else
                        {
                            ConnectIfNotOpen();
                            GfxSystem.PublishGfxEvent("ge_ui_connect_hint", "ui", false, true);
                        }
                    }
                }
                else
                {
                    if (m_IsQueueing)
                    {
                        if (m_LastQueueingTime + 5000 < curTime)
                        {
                            m_LastQueueingTime = curTime;

                            SendGetQueueingCount();
                        }
                        if (m_LastShowQueueingNum + 1000 < curTime && m_QueueingNum > 0)
                        {
                            m_LastShowQueueingNum = curTime;

                            WorldSystem.Instance.HighlightPrompt(30, m_QueueingNum);
                        }
                    }
                    if (m_HasLoggedOn && !m_IsLogining && m_LastConnectTime + 5000 < curTime)
                    {
                        if (m_LastHeartbeatTime + 5000 < curTime)
                        {
                            m_LastHeartbeatTime = curTime;
                            if (m_LastReceiveHeartbeatTime == 0)
                            {
                                m_LastReceiveHeartbeatTime = curTime;
                            }
                            SendHeartbeat();
                        }
                        if (m_LastReceiveHeartbeatTime > 0 && m_LastReceiveHeartbeatTime + 30000 < curTime)
                        {
                            //断开连接
                            if (IsConnected)
                            {
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
            if (IsConnected)
            {
                m_WebSocket.Close();
            }
        }
        internal void QuitRoom(bool isQuitTeam)
        {
            if (m_Guid != 0)
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.QuitRoom);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_QuitRoom protoMsg = new ArkCrossEngineMessage.Msg_CL_QuitRoom();
                protoMsg.m_IsQuitRoom = isQuitTeam;
                msg.m_ProtoData = protoMsg;
                SendMessage(msg);
            }
        }
        internal void QuitPve()
        {
            if (m_Guid != 0)
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.QuitPve);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                SendMessage(msg);
            }
        }
        internal void QuitClient()
        {
            if (m_Guid != 0)
            {
                JsonData msg = new JsonData();
                msg.SetJsonType(JsonType.Object);
                msg.Set("m_Guid", m_Guid);
                SendMessage(JsonMessageID.Logout, msg);
            }
            if (LobbyClient.Instance.AccountInfo.Account != string.Empty)
            {
                JsonData msg = new JsonData();
                msg.SetJsonType(JsonType.Object);
                msg.Set("m_Account", m_Account);
                SendMessage(JsonMessageID.AccountLogout, msg);
            }
            m_IsWaitStart = true;
            m_HasLoggedOn = false;
            if (IsConnected)
            {
                m_WebSocket.Close();
            }
        }
        internal void ChangeCityScene(int citySceneId)
        {
            LobbyClient.Instance.CurrentRole.CitySceneId = citySceneId;

            JsonMessage msg = new JsonMessage(JsonMessageID.ChangeCityScene);
            msg.m_JsonData.Set("m_Guid", m_Guid);
            ArkCrossEngineMessage.Msg_CL_ChangeCityScene protoMsg = new ArkCrossEngineMessage.Msg_CL_ChangeCityScene();
            protoMsg.m_SceneId = citySceneId;

            msg.m_ProtoData = protoMsg;
            SendMessage(msg);
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
        internal int QueueingNum
        {
            get { return m_QueueingNum; }
            set { m_QueueingNum = value; }
        }
        public bool IsQueueing
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
        internal ulong Guid
        {
            get { return m_Guid; }
        }

        internal bool SendMessage(string msgStr)
        {
            bool ret = false;
            try
            {
                if (IsConnected)
                {
                    m_WebSocket.Send(msgStr);
#if DEBUG
                    LogSystem.Debug("SendToLobby {0}", msgStr);
#endif
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("SendMessage throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
            return ret;
        }

        private void SendGetQueueingCount()
        {
            JsonData msg = new JsonData();
            msg.SetJsonType(JsonType.Object);
            msg.Set("m_Account", m_Account);
            SendMessage(JsonMessageID.GetQueueingCount, msg);
        }

        private void SendHeartbeat()
        {
            JsonData msg = new JsonData();
            msg.SetJsonType(JsonType.Object);
            msg.Set("m_Guid", m_Guid);
            SendMessage(JsonMessageID.UserHeartbeat, msg);
        }

        private void HandleQueueingCountResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_QueueingCountResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_QueueingCountResult;
            if (null != protoData)
            {
                LobbyNetworkSystem.Instance.QueueingNum = protoData.m_QueueingCount;
            }
        }

        private void HandleUserHeartbeat(JsonMessage lobbyMsg)
        {
            m_LastReceiveHeartbeatTime = TimeUtility.GetLocalMilliseconds();
        }

        private void RegisterMsgHandler(JsonMessageID id, JsonMessageHandlerDelegate handler)
        {
            JsonMessageDispatcher.RegisterMessageHandler((int)id, null, handler);
        }
        private void SendMessage(JsonMessageID id, JsonData msg)
        {
            try
            {
                JsonMessageDispatcher.SendMessage((int)id, msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("LobbyNetworkSystem.SendMessage throw Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RegisterMsgHandler(JsonMessageID id, Type t, JsonMessageHandlerDelegate handler)
        {
            JsonMessageDispatcher.RegisterMessageHandler((int)id, t, handler);
        }
        private void SendMessage(JsonMessage msg)
        {
            try
            {
                JsonMessageDispatcher.SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("LobbyNetworkSystem.SendMessage throw Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void OnOpened()
        {
            //首先校验版本
            JsonData versionMsg = new JsonData();
            versionMsg.Set("m_Version", (uint)CodeVersion.Value);
            SendMessage(JsonMessageID.VersionVerify, versionMsg);
        }
        private void OnError(Exception ex)
        {
            if (null != ex)
            {
                LogSystem.Error("LobbyNetworkSystem.OnError Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
            else
            {
                LogSystem.Error("LobbyNetworkSystem.OnError (Unknown)");
            }
        }
        private void OnClosed()
        {
            LogSystem.Error("LobbyNetworkSystem.OnClosed");
        }
        private void OnMessageReceived(string msg)
        {
            if (null != msg)
            {
                JsonMessageDispatcher.HandleNodeMessage(msg);
            }
        }
        private void OnDataReceived(byte[] data)
        {
            LogSystem.Debug("Receive Data Message:\n{0}", CrossEngineHelper.BinToHex(data));
        }

        private void OnWsOpened(object sender, EventArgs e)
        {
            if (null != m_AsyncActionQueue)
            {
                m_AsyncActionQueue.QueueActionWithDelegation((MyAction)this.OnOpened);
            }
        }
        private void OnWsError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (null != m_AsyncActionQueue)
            {
                m_AsyncActionQueue.QueueActionWithDelegation((MyAction<Exception>)this.OnError, e.Exception);
            }
        }
        private void OnWsClosed(object sender, EventArgs e)
        {
            if (null != m_AsyncActionQueue)
            {
                m_AsyncActionQueue.QueueActionWithDelegation((MyAction)this.OnClosed);
            }
        }
        private void OnWsMessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            if (null != m_AsyncActionQueue)
            {
                m_AsyncActionQueue.QueueActionWithDelegation((MyAction<string>)this.OnMessageReceived, e.Message);
            }
        }
        private void OnWsDataReceived(object sender, WebSocket4Net.DataReceivedEventArgs e)
        {
            if (null != m_AsyncActionQueue)
            {
                m_AsyncActionQueue.QueueActionWithDelegation((MyAction<byte[]>)this.OnDataReceived, e.Data);
            }
        }

        private string GetIp()
        {
            string ip = "127.0.0.1";
            try
            {
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters)
                {
                    if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        UnicastIPAddressInformationCollection uniCast = adapter.GetIPProperties().UnicastAddresses;
                        if (uniCast.Count > 0)
                        {
                            foreach (UnicastIPAddressInformation uni in uniCast)
                            {
                                //得到IPv4的地址。 AddressFamily.InterNetwork指的是IPv4
                                if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    ip = uni.Address.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogSystem.Error("LobbyNetworkSystem.OnError (Unknown)");
            }
            return ip;
        }

        private bool m_IsWaitStart = true;
        private bool m_IsQueueing = false;
        private bool m_IsLogining = false;
        private bool m_HasLoggedOn = false;
        private long m_LastConnectTime = 0;
        private long m_LastQueueingTime = 0;
        private long m_LastHeartbeatTime = 0;
        private long m_LastReceiveHeartbeatTime = 0;

        private long m_LastShowQueueingNum = 0;
        private int m_QueueingNum = 0;
        private LoginMode m_LoginMode = LoginMode.DirectLogin;
        private string m_Url;
        private int m_WorldId = -1;       //客户端连接的WorldId
        private int m_LogicServerId = 1;
        private string m_Account;
        private string m_Ip = "";
        private string m_GameChannelId = "2010071003";
        private ulong m_Guid;
        private string m_UniqueIdentifier;
        private string m_ClientGameVersion = "";
        private string m_System = "all";

        private int m_OpCode;
        private int m_ChannelId;
        private string m_Data;
#if !PLATFORM_WEBGL
        private WebSocket4Net.WebSocket m_WebSocket;
#else
        private WebSocketWrapper m_WebSocket;
#endif
        private IActionQueue m_AsyncActionQueue;

        internal static LobbyNetworkSystem Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static LobbyNetworkSystem s_Instance = new LobbyNetworkSystem();
    }
}
