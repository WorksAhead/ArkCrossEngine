using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.ProtocolBuffers;
using Lidgren.Network;
using RoomServer;
using ArkCrossEngine;
using DashFire;
using ArkCrossEngineMessage;

internal class MsgTransmitHandler
{
  internal static void Execute(object msg, RoomPeer peer)
  {
    peer.BroadCastMsgToRoom(msg);
  }
}

internal class MsgPingHandler
{
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_Ping ping = msg as Msg_Ping;
    if (ping == null) {
      LogSys.Log(LOG_TYPE.DEBUG, "warning: convert to ping message failed!");
      return;
    }
    // LogSys.Log(LOG_TYPE.DEBUG, "got {0} ping msg send ping time = {1}",
    //                           peer.UserGuid, ping.SendPingTime);
    Msg_Pong pongBuilder = new Msg_Pong();
    long curtime = TimeUtility.GetServerMilliseconds();
    pongBuilder.send_ping_time = ping.send_ping_time;
    pongBuilder.send_pong_time = curtime;
    peer.SetLastPingTime(curtime);
    Msg_Pong msg_pong = pongBuilder;
    peer.SendMessage(msg_pong);
  }
}

internal class Msg_CRC_Move_Handler
{    
  internal static void OnMoveStart(object msg, RoomPeer peer)
  {
    Msg_CRC_MoveStart move_msg = msg as Msg_CRC_MoveStart;
    if (null == move_msg) {
      return;
    }
    move_msg.role_id = peer.RoleId;
    peer.BroadCastMsgToCareList(move_msg);
  }

  internal static void OnMoveStop(object msg, RoomPeer peer)
  { 
    Msg_CRC_MoveStop move_msg = msg as Msg_CRC_MoveStop;
    if (null == move_msg) {
      return;
    }
    Msg_CRC_MoveStop bd = move_msg;
    bd.role_id = peer.RoleId;
    peer.BroadCastMsgToCareList(bd);
  }
}

internal class Msg_CRC_Face_Handler
{    
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_Face face_msg = msg as Msg_CRC_Face;
    if (null == face_msg) {
      return;
    }
    Msg_CRC_Face bd = face_msg;
    bd.role_id = peer.RoleId;
    peer.BroadCastMsgToRoom(bd);
  }
}

internal class Msg_CRC_Skill_Handler
{    
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_Skill skill_msg = msg as Msg_CRC_Skill;
    if (null == skill_msg) {
      return;
    }
    skill_msg.role_id = peer.RoleId;
    peer.BroadCastMsgToRoom(skill_msg);
  }
}

internal class Msg_CRC_StopSkill_Handler
{
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_StopSkill stopskill_msg = msg as Msg_CRC_StopSkill;
    if (null == stopskill_msg) {
      return;
    }
    stopskill_msg.role_id = peer.RoleId;
    peer.BroadCastMsgToRoom(stopskill_msg);
  }
}

internal class Msg_CRC_InteractObject_Handler
{
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_InteractObject _msg = msg as Msg_CRC_InteractObject;
    if (null == _msg)
      return;
    Msg_CRC_InteractObject bd = _msg;
    bd.initiator_id = peer.RoleId;
    peer.BroadCastMsgToRoom(bd);
  }
}

internal class Msg_CRC_MoveMeetObstacle_Handler
{  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_MoveMeetObstacle obstacle_msg = msg as Msg_CRC_MoveMeetObstacle;
    if (null == obstacle_msg) {
      return;
    }
    obstacle_msg.role_id = peer.RoleId;
    peer.BroadCastMsgToRoom(obstacle_msg);
  }
}

internal class Msg_CRC_SendImpactToEntity_Handler
{
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_SendImpactToEntity _msg = msg as Msg_CRC_SendImpactToEntity;
    if (null == _msg) {
      return;
    }
    peer.BroadCastMsgToRoom(_msg);
  }
}

internal class Msg_CRC_StopGfxImpact_Handler
{
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_StopGfxImpact _msg = msg as Msg_CRC_StopGfxImpact;
    if (null == _msg) {
      return;
    }
    peer.BroadCastMsgToRoom(_msg);
  }
}

internal class Msg_CRC_GfxControlMoveStart_Handler
{
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_GfxControlMoveStart _msg = msg as Msg_CRC_GfxControlMoveStart;
    if (null == _msg) {
      return;
    }
    peer.BroadCastMsgToRoom(_msg);
  }
}

internal class Msg_CRC_GfxControlMoveStop_Handler
{
  internal static void Execute(object msg, RoomPeer peer)
  {
    Msg_CRC_GfxControlMoveStop _msg = msg as Msg_CRC_GfxControlMoveStop;
    if (null == _msg) {
      return;
    }
    peer.BroadCastMsgToRoom(_msg);
  }
}