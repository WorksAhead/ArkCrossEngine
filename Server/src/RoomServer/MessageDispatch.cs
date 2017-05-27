using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.ProtocolBuffers;
using Google.ProtocolBuffers.DescriptorProtos;
using System.Reflection;
using Lidgren.Network;

using DashFire;
using ArkCrossEngineMessage;

namespace RoomServer
{
  class MessageDispatch
  {
    internal delegate void MsgHandler(object msg, RoomPeer user);
    internal delegate void LobbyMsgHandler(IMessage msg, NetConnection conn);

    private MyDictionary<Type, MsgHandler> m_DicHandler = new MyDictionary<Type, MsgHandler>();
    private MyDictionary<Type, LobbyMsgHandler> m_SpecialHandlers = new MyDictionary<Type, LobbyMsgHandler>();

    internal void RegisterSpecialMsgHandler(Type t, LobbyMsgHandler handler)
    {
      m_SpecialHandlers[t] = handler;
    }

    internal void RegisterHandler(Type t, MsgHandler handler)
    {
      m_DicHandler[t] = handler;
    }

    internal void Dispatch(object msg, NetConnection conn)
    {
      try {
        // 特殊处理机器人系统消息
        /*if (msg.GetType() == typeof(Lobby_RoomServer.Msg_LR_CreateBattleRoom)) {
          LobbyMsgHandler lobby_msghandler;
          if (m_SpecialHandlers.TryGetValue(msg.GetType(), out lobby_msghandler)) {
            lobby_msghandler.Invoke(msg, conn);
          }
          return;
        }*/

        // 特殊处理认证消息
        if (msg.GetType() == typeof(Msg_CR_ShakeHands)) {
          Msg_CR_ShakeHands shakehandsMsg = msg as Msg_CR_ShakeHands;
          if (shakehandsMsg == null) {
            return;
          }
          bool ret = RoomPeerMgr.Instance.OnPeerShakeHands(shakehandsMsg.auth_key,
              conn);
          Msg_RC_ShakeHands_Ret builder = new Msg_RC_ShakeHands_Ret();
          if (ret) {
            builder.auth_result = Msg_RC_ShakeHands_Ret.RetType.SUCCESS;
            IOManager.Instance.SendMessage(conn, builder);
          } else {
            builder.auth_result = Msg_RC_ShakeHands_Ret.RetType.ERROR;
            IOManager.Instance.SendUnconnectedMessage(conn, builder);
          }
          return;
        }

        RoomPeer peer = RoomPeerMgr.Instance.GetPeerByConnection(conn);
        // 没有认证连接的消息不进行处理
        if (peer == null) {
          Msg_RC_ShakeHands_Ret builder = new Msg_RC_ShakeHands_Ret();
          builder.auth_result = Msg_RC_ShakeHands_Ret.RetType.ERROR;
          IOManager.Instance.SendUnconnectedMessage(conn, builder);

          conn.Disconnect("unauthed");
          LogSys.Log(LOG_TYPE.DEBUG, "unauthed peer {0} got message {1}, can't deal it!", conn.RemoteEndPoint.ToString(), msg.ToString());
          return;
        }

        // 直接转发消息(或进行其它处理)
        MsgHandler msghandler;
        if (m_DicHandler.TryGetValue(msg.GetType(), out msghandler)) {
          //object[] param = new object[] { msg, peer };
          msghandler.Invoke(msg, peer);
        }
        if (msg is Msg_Ping)
          return;

        // 消息分发到peer
        RoomPeerMgr.Instance.DispatchPeerMsg(peer, msg);
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
      }
    }
  }
}
