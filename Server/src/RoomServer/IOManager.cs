using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Reflection;
using Lidgren.Network;
using Google.ProtocolBuffers;
using CSharpCenterClient;

using DashFire;
using ArkCrossEngineMessage;
using ArkCrossEngine.Network;
using ArkCrossEngine;

namespace RoomServer
{
    class IOManager
    {
        #region
        private static IOManager s_Instance = new IOManager();
        internal static IOManager Instance
        {
            get { return s_Instance; }
        }
        #endregion

        private Thread m_IOThread;
        private NetServer m_NetServer;
        private NetPeerConfiguration m_Config;
        private RoomSrvStatus m_Status = RoomSrvStatus.STATUS_INIT;
        private MessageDispatch m_Dispatch = new MessageDispatch();

        internal void Init(int port)
        {
            InitMessageHandler();

            int receiveBufferSize = 64;
            int sendBufferSize = 64;
            StringBuilder sb = new StringBuilder(256);
            if (CenterClientApi.GetConfig("ReceiveBufferSize", sb, 256))
            {
                receiveBufferSize = int.Parse(sb.ToString());
            }
            if (CenterClientApi.GetConfig("SendBufferSize", sb, 256))
            {
                sendBufferSize = int.Parse(sb.ToString());
            }

            m_Config = new NetPeerConfiguration("RoomServer");
            m_Config.MaximumConnections = 1024;
            m_Config.ConnectionTimeout = 5.0f;
            m_Config.PingInterval = 1.0f;
            m_Config.ReceiveBufferSize = receiveBufferSize * 1024 * 1024;
            m_Config.SendBufferSize = sendBufferSize * 1024 * 1024;
            m_Config.Port = port;
            m_Config.DisableMessageType(NetIncomingMessageType.DebugMessage);
            m_Config.DisableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            //m_Config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            //m_Config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            m_Config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            m_Config.EnableMessageType(NetIncomingMessageType.WarningMessage);

            if (m_Config.IsMessageTypeEnabled(NetIncomingMessageType.DebugMessage))
                LogSys.Log(LOG_TYPE.DEBUG, "Enable NetIncomingMessageType.DebugMessage");
            if (m_Config.IsMessageTypeEnabled(NetIncomingMessageType.VerboseDebugMessage))
                LogSys.Log(LOG_TYPE.DEBUG, "Enable NetIncomingMessageType.VerboseDebugMessage");
            if (m_Config.IsMessageTypeEnabled(NetIncomingMessageType.ErrorMessage))
                LogSys.Log(LOG_TYPE.DEBUG, "Enable NetIncomingMessageType.ErrorMessage");
            if (m_Config.IsMessageTypeEnabled(NetIncomingMessageType.WarningMessage))
                LogSys.Log(LOG_TYPE.DEBUG, "Enable NetIncomingMessageType.WarningMessage");

            m_NetServer = new NetServer(m_Config);
            m_NetServer.Start();
            m_IOThread = new Thread(new ThreadStart(IOHandler));
            m_IOThread.Name = "IOHandler";
            m_IOThread.IsBackground = true;
            m_Status = RoomSrvStatus.STATUS_RUNNING;
            m_IOThread.Start();
            RoomPeerMgr.Instance.Init();
            Console.WriteLine("Init IOManager OK!");
        }

        private bool InitMessageHandler()
        {
            RegisterMsgHandler(typeof(Msg_Ping), new MessageDispatch.MsgHandler(MsgPingHandler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_MoveStart), new MessageDispatch.MsgHandler(Msg_CRC_Move_Handler.OnMoveStart));
            RegisterMsgHandler(typeof(Msg_CRC_MoveStop), new MessageDispatch.MsgHandler(Msg_CRC_Move_Handler.OnMoveStop));
            RegisterMsgHandler(typeof(Msg_CRC_Face), new MessageDispatch.MsgHandler(Msg_CRC_Face_Handler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_Skill), new MessageDispatch.MsgHandler(Msg_CRC_Skill_Handler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_StopSkill), new MessageDispatch.MsgHandler(Msg_CRC_StopSkill_Handler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_InteractObject), new MessageDispatch.MsgHandler(Msg_CRC_InteractObject_Handler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_MoveMeetObstacle), new MessageDispatch.MsgHandler(Msg_CRC_MoveMeetObstacle_Handler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_SendImpactToEntity), new MessageDispatch.MsgHandler(Msg_CRC_SendImpactToEntity_Handler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_StopGfxImpact), new MessageDispatch.MsgHandler(Msg_CRC_StopGfxImpact_Handler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_GfxControlMoveStart), new MessageDispatch.MsgHandler(Msg_CRC_GfxControlMoveStart_Handler.Execute));
            RegisterMsgHandler(typeof(Msg_CRC_GfxControlMoveStop), new MessageDispatch.MsgHandler(Msg_CRC_GfxControlMoveStop_Handler.Execute));
            return true;
        }

        internal void RegisterMsgHandler(Type msgtype, MessageDispatch.MsgHandler handler)
        {
            m_Dispatch.RegisterHandler(msgtype, handler);
        }

        internal void RegisterSpecialHandler(Type msgtype, MessageDispatch.LobbyMsgHandler handler)
        {
            m_Dispatch.RegisterSpecialMsgHandler(msgtype, handler);
        }

        private void IOHandler()
        {
            while (m_Status == RoomSrvStatus.STATUS_RUNNING)
            {
                try
                {
                    m_NetServer.MessageReceivedEvent.WaitOne();
                    long startTime = TimeUtility.GetElapsedTimeUs();
                    NetIncomingMessage im;
                    for (int ct = 0; ct < 1024; ++ct)
                    {
                        try
                        {
                            if ((im = m_NetServer.ReadMessage()) != null)
                            {
                                switch (im.MessageType)
                                {
                                    case NetIncomingMessageType.DebugMessage:
                                    case NetIncomingMessageType.VerboseDebugMessage:
                                        LogSys.Log(LOG_TYPE.DEBUG, "Debug Message: {0}", im.ReadString());
                                        break;
                                    case NetIncomingMessageType.ErrorMessage:
                                        LogSys.Log(LOG_TYPE.DEBUG, "Error Message: {0}", im.ReadString());
                                        break;
                                    case NetIncomingMessageType.WarningMessage:
                                        LogSys.Log(LOG_TYPE.DEBUG, "Warning Message: {0}", im.ReadString());
                                        break;
                                    case NetIncomingMessageType.StatusChanged:
                                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                                        string reason = im.ReadString();
                                        if (null != im.SenderConnection)
                                        {
                                            RoomPeer peer = RoomPeerMgr.Instance.GetPeerByConnection(im.SenderConnection);
                                            if (null != peer)
                                            {
                                                LogSys.Log(LOG_TYPE.DEBUG, "Network Status Changed: {0} reason:{1} EndPoint:{2} Key:{3} User:{4}", status, reason, im.SenderEndPoint.ToString(), peer.GetKey(), peer.Guid);
                                            }
                                            else
                                            {
                                                LogSys.Log(LOG_TYPE.DEBUG, "Network Status Changed: {0} reason:{1} EndPoint:{2}", status, reason, im.SenderEndPoint.ToString());
                                            }
                                        }
                                        else
                                        {
                                            LogSys.Log(LOG_TYPE.DEBUG, "Network Status Changed:{0} reason:{1}", status, reason);
                                        }
                                        break;
                                    case NetIncomingMessageType.Data:
                                        object msg = null;
                                        byte[] data = null;
                                        try
                                        {
                                            data = im.ReadBytes(im.LengthBytes);
                                            msg = Serialize.Decode(data);
                                        }
                                        catch
                                        {
                                            if (null != im.SenderConnection)
                                            {
                                                RoomPeer peer = RoomPeerMgr.Instance.GetPeerByConnection(im.SenderConnection);
                                                if (null != peer)
                                                {
                                                    LogSys.Log(LOG_TYPE.WARN, "room server decode message error !!! from User:{0}({1})", peer.Guid, peer.GetKey());
                                                }
                                            }
                                        }
                                        if (msg != null)
                                        {
                                            m_Dispatch.Dispatch(msg, im.SenderConnection);
                                        }
                                        else
                                        {
                                            if (null != im.SenderConnection)
                                            {
                                                RoomPeer peer = RoomPeerMgr.Instance.GetPeerByConnection(im.SenderConnection);
                                                if (null != peer)
                                                {
                                                    LogSys.Log(LOG_TYPE.DEBUG, "got unknow message !!! from User:{0}({1})", peer.Guid, peer.GetKey());
                                                }
                                                else
                                                {
                                                    LogSys.Log(LOG_TYPE.DEBUG, "got unknow message !!!");
                                                }
                                            }
                                            else
                                            {
                                                LogSys.Log(LOG_TYPE.DEBUG, "got unknow message !!!");
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                m_NetServer.Recycle(im);
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
                        }
                    }
                    RoomPeerMgr.Instance.Tick();
                    long endTime = TimeUtility.GetElapsedTimeUs();
                    if (endTime - startTime >= 10000)
                    {
                        LogSys.Log(LOG_TYPE.DEBUG, "Warning, IOHandler() cost {0} us !\nNetPeer Statistic:{1}", endTime - startTime, m_NetServer.Statistics.ToString());
                    }
                }
                catch (Exception ex)
                {
                    LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
                }

                Thread.Sleep(10);
            }
        }

        internal void SendPeerMessage(RoomPeer peer, object msg)
        {
            try
            {
                NetOutgoingMessage om = m_NetServer.CreateMessage();
                om.Write(Serialize.Encode(msg));
                if (null != peer.GetConnection())
                {
                    NetSendResult res = m_NetServer.SendMessage(om, peer.GetConnection(), NetDeliveryMethod.ReliableOrdered, 0);
                    if (res == NetSendResult.Dropped)
                    {
                        LogSys.Log(LOG_TYPE.ERROR, "SendPeerMessage {0} failed:dropped, User:{1}({2})", msg.ToString(), peer.Guid, peer.GetKey());
                    }
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal void SendMessage(NetConnection conn, object msg)
        {
            try
            {
                NetOutgoingMessage om = m_NetServer.CreateMessage();
                om.Write(Serialize.Encode(msg));
                NetSendResult res = m_NetServer.SendMessage(om, conn, NetDeliveryMethod.ReliableOrdered, 0);
                if (res == NetSendResult.Dropped)
                {
                    RoomPeer peer = RoomPeerMgr.Instance.GetPeerByConnection(conn);
                    if (null != peer)
                    {
                        LogSys.Log(LOG_TYPE.ERROR, "SendMessage {0} failed:dropped, User:{1}({2})", msg.ToString(), peer.Guid, peer.GetKey());
                    }
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal void SendUnconnectedMessage(NetConnection conn, object msg)
        {
            try
            {
                NetOutgoingMessage om = m_NetServer.CreateMessage();
                om.Write(Serialize.Encode(msg));
                m_NetServer.SendUnconnectedMessage(om, conn.RemoteEndPoint.Address.ToString(), conn.RemoteEndPoint.Port);
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
