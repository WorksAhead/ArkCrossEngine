using System;
using System.Reflection;

using Lobby;
using CSharpCenterClient;
using Newtonsoft.Json;
using Lobby_RoomServer;
using Messenger;

namespace Lobby
{
    internal partial class LobbyServer
    {
        private void InstallServerHandlers()
        {
            m_RoomSvrChannel = new PBChannel(MessageMapping.Query, MessageMapping.Query);
            m_RoomSvrChannel.Register<Msg_RL_RegisterRoomServer>(HandleRegisterRoomServer);
            m_RoomSvrChannel.Register<Msg_RL_RoomServerUpdateInfo>(HandleRoomServerUpdateInfo);
            m_RoomSvrChannel.Register<Msg_RL_ReplyCreateBattleRoom>(HandelReplyCreateBattleRoom);
            m_RoomSvrChannel.Register<Msg_RL_ReplyAddNewUsr>(HandelReplyAddNewUser);
            m_RoomSvrChannel.Register<Msg_RL_UserQuit>(HandleUserQuit);
            m_RoomSvrChannel.Register<Msg_RL_ReplyReconnectUser>(HandelReplyReconnectUser);
            m_RoomSvrChannel.Register<Msg_RL_BattleEnd>(HandleBattleEnd);
        }
        private void HandleRegisterRoomServer(Msg_RL_RegisterRoomServer msg_, PBChannel channel, int src, uint session)
        {
            m_RoomProcessThread.QueueAction(m_RoomProcessThread.RegisterRoomServer, new RoomServerInfo
            {
                RoomServerName = msg_.ServerName,
                MaxRoomNum = msg_.MaxRoomNum,
                ServerIp = msg_.ServerIp,
                ServerPort = msg_.ServerPort
            });
        }
        private void HandleRoomServerUpdateInfo(Msg_RL_RoomServerUpdateInfo updateMsg, PBChannel channel, int src, uint session)
        {
            //更新RoomServer信息
            m_RoomProcessThread.QueueAction(m_RoomProcessThread.UpdateRoomServerInfo, new RoomServerInfo
            {
                RoomServerName = updateMsg.ServerName,
                IdleRoomNum = updateMsg.IdleRoomNum,
                UserNum = updateMsg.UserNum
            });
        }
        private void HandelReplyCreateBattleRoom(Msg_RL_ReplyCreateBattleRoom replySBMsg, PBChannel channel, int src, uint session)
        {
            //响应RoomServer消息，创建战斗房间结果消息
            m_RoomProcessThread.QueueAction(m_RoomProcessThread.OnReplyCreateBattleRoom, replySBMsg.RoomId, replySBMsg.IsSuccess);
        }
        private void HandelReplyAddNewUser(Msg_RL_ReplyAddNewUsr replySBMsg, PBChannel channel, int src, uint session)
        {
            //响应RoomServer消息，创建战斗房间结果消息
            m_RoomProcessThread.QueueAction(m_RoomProcessThread.OnReplyAddNewUser, replySBMsg.RoomId, replySBMsg.IsSuccess);
        }

        private void HandleUserQuit(Msg_RL_UserQuit msg, PBChannel channel, int src, uint session)
        {
            //响应RoomServer游戏客户端退出消息
            m_RoomProcessThread.QueueAction(m_RoomProcessThread.OnRoomUserQuit, msg.RoomID, msg.UserGuid, msg.IsBattleEnd);
        }

        private void HandelReplyReconnectUser(Msg_RL_ReplyReconnectUser replyMsg, PBChannel channel, int src, uint session)
        {
            //响应RoomServer消息
            m_RoomProcessThread.QueueAction(m_RoomProcessThread.OnReplyReconnectUser, replyMsg.UserGuid, replyMsg.RoomID, replyMsg.IsSuccess);
        }

        private void HandleBattleEnd(Msg_RL_BattleEnd msg, PBChannel channel, int src, uint session)
        {
            int roomID = msg.RoomID;
            LogSys.Log(LOG_TYPE.DEBUG, "Battle Info: RoomID = {0}", roomID);
            m_RoomProcessThread.QueueAction(m_RoomProcessThread.OnRoomBattleEnd, msg);
        }
    }
}

