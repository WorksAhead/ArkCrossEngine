using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using System.Threading;
using System.Reflection;

using DashFireMessage;
using System.Runtime.InteropServices;

namespace DashFire.Network
{
  internal class NetworkSystem
  {
    internal bool Init(LobbyRobot.Robot robot)
    {
      m_Robot = robot;

      Serialize.Init();
      InitMessageHandler();

      m_NetClientStarted = false;
      m_IsWaitStart = true;
      m_IsQuited = false;
      m_IsConnected = false;
      m_CanSendMessage = false;

      m_Config = new NetPeerConfiguration("RoomServer");
      m_Config.AutoFlushSendQueue = false;
      m_Config.ConnectionTimeout = 5.0f;
      m_Config.PingInterval = 1.0f;
      m_Config.DisableMessageType(NetIncomingMessageType.DebugMessage);
      m_Config.DisableMessageType(NetIncomingMessageType.VerboseDebugMessage);
      m_Config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
      m_Config.EnableMessageType(NetIncomingMessageType.WarningMessage);
      m_NetClient = new NetClient(m_Config);
      m_NetThread = new Thread(new ThreadStart(NetworkThread));
      m_NetThread.IsBackground = true;
      m_NetThread.Start ();
      return true;
    }
    internal LobbyRobot.Robot Robot
    {
      get { return m_Robot; }
    }

    internal bool IsWaitStart
    {
      get { return m_IsWaitStart; }
    }
    internal bool IsConnected
    {
      get { return m_IsConnected; }
    }

    internal void Start(uint key, string ip, int port)
    {
      StartNetClient();

      m_Key = key;
      m_Ip = ip;
      m_Port = port;

      m_IsWaitStart = false;
      m_IsConnected = false;
      m_WaitDisconnect = false;
      m_CanSendMessage = false;

      LogSystem.Info("{0} NetworkSystem.Start key {1} ip {2} port {3} {4}", Robot.LobbyNetworkSystem.User, key, ip, port, LobbyRobot.Robot.GetDateTime());
    }

    internal void WaitDisconnect()
    {
      if (!m_WaitDisconnect) {
        m_WaitDisconnect = true;
        m_WaitDisconnectTime = TimeUtility.GetLocalMilliseconds();
      }
    }

    internal void Tick()
    {
      try {
        if (m_NetClient == null)
          return;
        long curTime = TimeUtility.GetLocalMilliseconds();
        if (m_IsConnected && m_CanSendMessage) {
          if (curTime - m_LastPingTime >= m_PingInterval) {
            InternalPing();
          }
        }
        ProcessMsg();

        if (m_WaitDisconnect && m_WaitDisconnectTime + 5000 < curTime) {
          m_WaitDisconnect = false;

          LogSystem.Debug("{0} auth failed, restart match !!! {1}", Robot.LobbyNetworkSystem.User, LobbyRobot.Robot.GetDateTime());
          
          QuitBattle(true);
          Robot.LobbyNetworkSystem.QuitRoom();

          Robot.StorySystem.SendMessage("missionfailed");
        }
      } catch (Exception e) {
        LogSystem.Error("{0} Exception:{1}\n{2}", Robot.LobbyNetworkSystem.User, e.Message, e.StackTrace);
      }
    }

    internal void QuitBattle()
    {
      QuitBattle(false);
    }

    internal void QuitBattle(bool isForce)
    {
      m_WaitDisconnect = false;
      m_IsWaitStart = true;

      Msg_CR_Quit msg = new Msg_CR_Quit();
      msg.is_force = isForce;
      SendMessage(msg);

      m_Robot.DelayActionQueue.QueueAction(this.ShutdownNetClient);
    }

    internal void QuitBattlePassive()
    {
      m_WaitDisconnect = false;
      m_IsWaitStart = true;

      m_Robot.DelayActionQueue.QueueAction(this.ShutdownNetClient);
    }

    internal void QuitClient()
    {
      m_IsQuited = true;
    }

    internal void SendLoginMsg(object msg)
    {
      SendMessage(msg);
    }

    internal void SendMessage(object msg)
    {
      if (!m_IsConnected) {
        return;
      }
      NetOutgoingMessage om = m_NetClient.CreateMessage();
      byte[] bt = Serialize.Encode(msg);
      om.Write(bt);
      NetSendResult result = m_NetClient.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
      if (result == NetSendResult.FailedNotConnected) {
        m_IsConnected = false;
        m_WaitDisconnect = false;
        m_CanSendMessage = false;
        LogSystem.Debug("{0} SendMessage FailedNotConnected {1}", Robot.LobbyNetworkSystem.User, LobbyRobot.Robot.GetDateTime());
      } else if (result == NetSendResult.Dropped) {
        LogSystem.Error("{0} SendMessage {1} Dropped {2}", Robot.LobbyNetworkSystem.User, msg.ToString(), LobbyRobot.Robot.GetDateTime());
      }
      m_NetClient.FlushSendQueue();
    }

    internal void Release()
    {
      ShutdownNetClient();
    }

    internal void OnPong(long time, long sendPingTime, long sendPongTime)
    {
      if (time < sendPingTime) return;
      ++m_PingPongNumber;

      long rtt = time - sendPingTime;
      if (m_Robot.AverageRoundtripTime == 0)
        m_Robot.AverageRoundtripTime = rtt;
      else
        m_Robot.AverageRoundtripTime = (long)(TimeUtility.AverageRoundtripTime * 0.7f + rtt * 0.3f);

      //LogSystem.Debug("RoundtripTime:{0} AverageRoundtripTime:{1}", rtt, TimeUtility.AverageRoundtripTime);

      long diff = sendPongTime + rtt/2 - time;
      m_Robot.RemoteTimeOffset = (TimeUtility.RemoteTimeOffset * (m_PingPongNumber - 1) + diff) / m_PingPongNumber;
    }

    internal void SyncFaceDirection(float face_direction)
    {
      Msg_CRC_Face bd = new Msg_CRC_Face();
      bd.face_direction = face_direction;
      SendMessage(bd);
    }

    internal void SyncPlayerMoveStart(float x, float z, float dir)
    {
        DashFireMessage.Msg_CRC_MoveStart builder = new DashFireMessage.Msg_CRC_MoveStart();
        builder.send_time = TimeUtility.GetServerMilliseconds();
        builder.dir = dir;
        Position pos = new Position();
        pos.x = x;
        pos.z = z;
        builder.position = pos;
        builder.is_skill_moving = false;
        SendMessage(builder);
    }

    internal void SyncPlayerMoveStop(float x, float z)
    {
      DashFireMessage.Msg_CRC_MoveStop builder = new DashFireMessage.Msg_CRC_MoveStop();
      builder.send_time = TimeUtility.GetServerMilliseconds();
      Position pos = new Position();
      pos.x = x;
      pos.z = z;
      builder.position = pos;
      SendMessage(builder);
    }

    internal void SyncPlayerSkill(int skillId, float x, float z, float dir)
    {
      Msg_CRC_Skill bd = new Msg_CRC_Skill();
      bd.skill_id = skillId;
      bd.stand_pos = new DashFireMessage.Position();
      bd.stand_pos.x = x;
      bd.stand_pos.z = z;
      bd.face_direction = dir;
      bd.send_time = TimeUtility.GetServerMilliseconds();
      SendMessage(bd);
    }

    internal void SyncPlayerStopSkill(int skillId)
    {
      DashFireMessage.Msg_CRC_StopSkill msg = new DashFireMessage.Msg_CRC_StopSkill();
      msg.skill_id = skillId;
      SendMessage(msg);
    }

    internal void SyncNpcStopSkill(int npcId, int skillId)
    {
      DashFireMessage.Msg_CRC_NpcStopSkill msg = new DashFireMessage.Msg_CRC_NpcStopSkill();
      msg.npc_id = npcId;
      msg.skill_id = skillId;
      SendMessage(msg);
    }

    internal void SyncSendImpact(int senderId,
      int impactId,
      int targetId,
      int skillId,
      int duration,
      float senderPosX,
      float senderPosY,
      float senderPosZ,
      float senderDir) {
      Msg_CRC_SendImpactToEntity bd = new Msg_CRC_SendImpactToEntity();
      bd.sender_id = senderId;
      bd.impact_id = impactId;
      bd.target_id = targetId;
      bd.skill_id = skillId;
      bd.duration = duration;
      bd.sender_pos = new Position3D();
      bd.sender_pos.x = senderPosX;
      bd.sender_pos.y = senderPosY;
      bd.sender_pos.z = senderPosZ;
      bd.sender_dir = senderDir;
      SendMessage(bd);
    }

    internal void SyncStopGfxImpact(int targetId,
      int impactId) {
      Msg_CRC_StopGfxImpact bd = new Msg_CRC_StopGfxImpact();
      bd.impact_Id = impactId;
      bd.target_Id = targetId;
      SendMessage(bd);
    }

    internal void SyncGfxMoveControlStart(int objId, float x, float z, int id, bool isSkill)
    {
      Msg_CRC_GfxControlMoveStart msg = new Msg_CRC_GfxControlMoveStart();
      msg.obj_id = objId;
      msg.skill_or_impact_id = id;
      msg.is_skill = isSkill;
      msg.cur_pos = new DashFireMessage.Position();
      msg.cur_pos.x = x;
      msg.cur_pos.z = z;
      msg.send_time = TimeUtility.GetServerMilliseconds();

      SendMessage(msg);
    }

    internal void SyncGfxMoveControlStop(int objId, float x, float z, int id, bool isSkill)
    {
      Msg_CRC_GfxControlMoveStop msg = new Msg_CRC_GfxControlMoveStop();
      msg.obj_id = objId;
      msg.skill_or_impact_id = id;
      msg.is_skill = isSkill;
      msg.target_pos = new DashFireMessage.Position();
      msg.target_pos.x = x;
      msg.target_pos.z = z;
      msg.face_dir = 0;
      msg.send_time = TimeUtility.GetServerMilliseconds();

      SendMessage(msg);
    }
    
    internal void SyncPlayerMoveToPos(float tx, float tz, float x, float z)
    {
      Msg_CR_UserMoveToPos builder = new Msg_CR_UserMoveToPos();
      builder.target_pos_x = tx;
      builder.target_pos_z = tz;
      builder.cur_pos_x = x;
      builder.cur_pos_z = z;
      SendMessage(builder);
    }

    internal void SyncPlayerMoveToAttack(int targetId, float attackRange, float x, float z)
    {
      Msg_CR_UserMoveToAttack builder = new Msg_CR_UserMoveToAttack();
      builder.target_id = targetId;
      builder.attack_range = attackRange;
      builder.cur_pos_x = x;
      builder.cur_pos_z = z;
      SendMessage(builder);
    }

    internal void SyncGiveUpCombat()
    {
      Msg_CR_GiveUpBattle msg = new Msg_CR_GiveUpBattle();
      SendMessage(msg);
    }

    internal void SyncDeleteDeadNpc(int npcId)
    {
      Msg_CR_DeleteDeadNpc msg = new Msg_CR_DeleteDeadNpc();
      msg.npc_id = npcId;
      SendMessage(msg);
    }

    private void RegisterMsgHandler(Type msgtype, MessageDispatch.MsgHandler handler)
    {
      m_Dispatch.RegisterHandler(msgtype, handler);
    }
    private void InitMessageHandler()
    {
      RegisterMsgHandler(typeof(Msg_Pong), new MessageDispatch.MsgHandler(MsgPongHandler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ShakeHands_Ret), new MessageDispatch.MsgHandler(MsgShakeHandsRetHandler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_Create), new MessageDispatch.MsgHandler(Msg_CRC_Create_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Enter), new MessageDispatch.MsgHandler(Msg_RC_Enter_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Disappear), new MessageDispatch.MsgHandler(Msg_RC_Disappear_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Dead), new MessageDispatch.MsgHandler(Msg_RC_Dead_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Revive), new MessageDispatch.MsgHandler(Msg_RC_Revive_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_Exit), new MessageDispatch.MsgHandler(Msg_CRC_Exit_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_MoveStart), new MessageDispatch.MsgHandler(Msg_CRC_Move_Handler.OnMoveStart));
      RegisterMsgHandler(typeof(Msg_CRC_MoveStop), new MessageDispatch.MsgHandler(Msg_CRC_Move_Handler.OnMoveStop));
      RegisterMsgHandler(typeof(Msg_CRC_MoveMeetObstacle), new MessageDispatch.MsgHandler(Msg_CRC_MoveMeetObstacle_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_Face), new MessageDispatch.MsgHandler(Msg_CRC_Face_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_Skill), new MessageDispatch.MsgHandler(Msg_CRC_Skill_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_StopSkill), new MessageDispatch.MsgHandler(Msg_CRC_StopSkill_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_UserMove), new MessageDispatch.MsgHandler(Msg_RC_UserMove_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_UserFace), new MessageDispatch.MsgHandler(Msg_RC_UserFace_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CreateNpc), new MessageDispatch.MsgHandler(Msg_RC_CreateNpc_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_DestroyNpc), new MessageDispatch.MsgHandler(Msg_RC_DestroyNpc_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcEnter), new MessageDispatch.MsgHandler(Msg_RC_NpcEnter_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcMove), new MessageDispatch.MsgHandler(Msg_RC_NpcMove_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcFace), new MessageDispatch.MsgHandler(Msg_RC_NpcFace_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcTarget), new MessageDispatch.MsgHandler(Msg_RC_NpcTarget_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcSkill), new MessageDispatch.MsgHandler(Msg_RC_NpcSkill_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_NpcStopSkill), new MessageDispatch.MsgHandler(Msg_CRC_NpcStopSkill_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcDead), new MessageDispatch.MsgHandler(Msg_RC_NpcDead_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcDisappear), new MessageDispatch.MsgHandler(Msg_RC_NpcDisappear_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SyncProperty), new MessageDispatch.MsgHandler(Msg_RC_SyncProperty_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_DebugSpaceInfo), new MessageDispatch.MsgHandler(Msg_RC_DebugSpaceInfo_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SyncCombatStatisticInfo), new MessageDispatch.MsgHandler(Msg_RC_SyncCombatStatisticInfo_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_PvpCombatInfo), new MessageDispatch.MsgHandler(Msg_RC_PvpCombatInfo_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_SendImpactToEntity), new MessageDispatch.MsgHandler(Msg_CRC_SendImpactToEntity_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_StopGfxImpact), new MessageDispatch.MsgHandler(Msg_CRC_StopGfxImpact_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ImpactDamage), new MessageDispatch.MsgHandler(Msg_RC_ImpactDamage_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_InteractObject), new MessageDispatch.MsgHandler(Msg_CRC_InteractObject_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ControlObject), new MessageDispatch.MsgHandler(Msg_RC_ControlObject_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_RefreshItemSkills), new MessageDispatch.MsgHandler(Msg_RC_RefreshItemSkills_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_HighlightPrompt), new MessageDispatch.MsgHandler(Msg_RC_HighlightPrompt_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_UpdateUserBattleInfo), new MessageDispatch.MsgHandler(Msg_RC_UpdateUserBattleInfo_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_MissionCompleted), new MessageDispatch.MsgHandler(Msg_RC_MissionCompleted_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ChangeScene), new MessageDispatch.MsgHandler(Msg_RC_ChangeScene_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CampChanged), new MessageDispatch.MsgHandler(Msg_RC_CampChanged_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_EnableInput), new MessageDispatch.MsgHandler(Msg_RC_EnableInput_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ShowUi), new MessageDispatch.MsgHandler(Msg_RC_ShowUi_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ShowWall), new MessageDispatch.MsgHandler(Msg_RC_ShowWall_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ShowDlg), new MessageDispatch.MsgHandler(Msg_RC_ShowDlg_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CameraLookat), new MessageDispatch.MsgHandler(Msg_RC_CameraLookat_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CameraFollow), new MessageDispatch.MsgHandler(Msg_RC_CameraFollow_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_GfxControlMoveStart), new MessageDispatch.MsgHandler(Msg_CRC_GfxControlMoveStart_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_GfxControlMoveStop), new MessageDispatch.MsgHandler(Msg_CRC_GfxControlMoveStop_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_UpdateCoefficient), new MessageDispatch.MsgHandler(Msg_RC_UpdateCoefficient_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_AdjustPosition), new MessageDispatch.MsgHandler(Msg_RC_AdjustPosition_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_LockFrame), new MessageDispatch.MsgHandler(Msg_RC_LockFrame_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_PlayAnimation), new MessageDispatch.MsgHandler(Msg_RC_PlayAnimation_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CameraYaw), new MessageDispatch.MsgHandler(Msg_RC_CameraYaw_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CameraHeight), new MessageDispatch.MsgHandler(Msg_RC_CameraHeight_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CameraDistance), new MessageDispatch.MsgHandler(Msg_RC_CameraDistance_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SetBlockedShader), new MessageDispatch.MsgHandler(Msg_RC_SetBlockedShader_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_StartCountDown), new MessageDispatch.MsgHandler(Msg_RC_StartCountDown_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_PublishEvent), new MessageDispatch.MsgHandler(Msg_RC_PublishEvent_Handler.Execute));
    }
    private void NetworkThread()
    {
      while (!m_IsQuited) {
        if (m_IsWaitStart) {
          Thread.Sleep(1000);
        } else {
          while (!m_IsQuited && !m_IsConnected && !m_IsWaitStart) {
            LogSystem.Debug("{0} Connect ip:{1} port:{2} key:{3} {4}\nNetPeer Statistic:{5}", Robot.LobbyNetworkSystem.User, m_Ip, m_Port, m_Key, LobbyRobot.Robot.GetDateTime(), m_NetClient.Statistics.ToString());
            try {
              m_NetClient.Connect(m_Ip, m_Port);
            } catch {
            }
            for (int ct = 0; ct < 10 && !m_IsConnected; ++ct) {
              OnRecvMessage();
              LogSystem.Debug("{0} Wait NetConnectionStatus.Connected ... {1}", Robot.LobbyNetworkSystem.User, LobbyRobot.Robot.GetDateTime());
              if (!m_IsConnected) {
                Thread.Sleep(1000);
              }
            }
          }
          OnRecvMessage();
        }
      }
    }
    private void OnConnected(NetConnection conn)
    {
      m_Connection = conn;
      m_IsConnected = true;
      m_WaitDisconnect = false;
      Msg_CR_ShakeHands bd = new Msg_CR_ShakeHands();
      bd.auth_key = m_Key;
      SendMessage(bd);
    }
    private void OnRecvMessage()
    {
      m_NetClient.MessageReceivedEvent.WaitOne(1000);
      NetIncomingMessage im;
      while ((im = m_NetClient.ReadMessage()) != null) {
        switch (im.MessageType) {
          case NetIncomingMessageType.DebugMessage:
          case NetIncomingMessageType.VerboseDebugMessage:
            LogSystem.Debug("{0} Debug Message: {1} {2}", Robot.LobbyNetworkSystem.User, im.ReadString(), LobbyRobot.Robot.GetDateTime());
            break;
          case NetIncomingMessageType.ErrorMessage:
            LogSystem.Debug("{0} Error Message: {1} {2}", Robot.LobbyNetworkSystem.User, im.ReadString(), LobbyRobot.Robot.GetDateTime());
            break;
          case NetIncomingMessageType.WarningMessage:
            LogSystem.Debug("{0} Warning Message: {1} {2}", Robot.LobbyNetworkSystem.User, im.ReadString(), LobbyRobot.Robot.GetDateTime());
            break;
          case NetIncomingMessageType.StatusChanged:
            NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
            string reason = im.ReadString();
            if (null != im.SenderConnection) {
              LogSystem.Debug("{0} Network Status Changed:{1} Reason:{2} {3}", Robot.LobbyNetworkSystem.User, status.ToString(), reason, LobbyRobot.Robot.GetDateTime());
              if (NetConnectionStatus.Disconnected == status) {
                m_IsConnected = false;
                m_WaitDisconnect = false;
                m_CanSendMessage = false;
              } else if (NetConnectionStatus.Connected == status) {
                OnConnected(im.SenderConnection);
              }
            } else {
              LogSystem.Debug("{0} Network Status Changed:{1} reason:{2}", Robot.LobbyNetworkSystem.User, status, reason);
            }
            break;
          case NetIncomingMessageType.Data:
          case NetIncomingMessageType.UnconnectedData:
            if (!m_IsConnected && NetIncomingMessageType.Data == im.MessageType) {
              break;
            }
            try {
              byte[] data = im.ReadBytes(im.LengthBytes);
              object msg = Serialize.Decode(data);
              if (msg != null) {
                PushMsg(msg, im.SenderConnection);
              }
            } catch (Exception ex) {
              LogSystem.Error("{0} Decode Message exception:{1}\n{2}", Robot.LobbyNetworkSystem.User, ex.Message, ex.StackTrace);
            }
            break;
          default:
            break;
        }
        m_NetClient.Recycle(im);
      }
    }
    private bool PushMsg(object msg, NetConnection conn)
    {
      lock (m_Lock) {
        m_QueuePair.Enqueue(new KeyValuePair<NetConnection, object>(conn, msg));
        return true;
      }
    }
    private int ProcessMsg()
    {
      lock (m_Lock) {
        if (m_QueuePair.Count <= 0)
          return -1;
        foreach (KeyValuePair<NetConnection, object> kv in m_QueuePair) {
          object msg = kv.Value;
          try {
            m_Dispatch.Dispatch(msg, kv.Key, this);
          } catch (Exception ex) {
            LogSystem.Error("{0} ProcessMsg Exception:{1}\n{2}", Robot.LobbyNetworkSystem.User, ex.Message, ex.StackTrace);
          }
        }
        m_QueuePair.Clear();
        return 0;
      }
    }

    private void StartNetClient()
    {
      if (m_NetClient != null) {
        if (!m_NetClientStarted) {
          m_NetClient.Start();
          m_NetClientStarted = true;
        }
      }
    }
    private void ShutdownNetClient()
    {
      if (m_NetClient != null) {
        if (m_NetClientStarted) {
          m_NetClient.Shutdown("bye");
          m_NetClientStarted = false;
          
          m_WaitDisconnect = false;
          m_IsConnected = false;
        }
      }
    }
    private void InternalPing()
    {
      if (m_CanSendMessage) {
        Msg_Ping builder = new Msg_Ping();
        m_LastPingTime = TimeUtility.GetLocalMilliseconds();
        builder.send_ping_time = (int)m_LastPingTime;
        SendMessage(builder);
      }
    }

    internal bool CanSendMessage
    {
      get { return m_CanSendMessage; }
      set { m_CanSendMessage = value; }
    }
    
    private long m_PingPongNumber = 0;
    private long m_LastPingTime = TimeUtility.GetLocalMilliseconds();        // ms
    private int m_PingInterval = 5000;                                       // ms

    private NetPeerConfiguration m_Config;
    private NetClient m_NetClient;
    private NetConnection m_Connection;
    private Thread m_NetThread;
    private bool m_NetClientStarted = false;

    private string m_Ip;
    private int m_Port;
    private bool m_IsConnected = false;

    private bool m_IsWaitStart = true;
    private bool m_IsQuited = false;
    private bool m_CanSendMessage = false;
    private MessageDispatch m_Dispatch = new MessageDispatch();
    private Queue<KeyValuePair<NetConnection, object>> m_QueuePair = new Queue<KeyValuePair<NetConnection, object>>();
    private object m_Lock = new object();
    private uint m_Key = 0;

    private bool m_WaitDisconnect = false;
    private long m_WaitDisconnectTime = 0;

    private LobbyRobot.Robot m_Robot = null;
  }

  internal struct PingRecord
  {
    internal long m_Ping;
    internal long m_TimeDifferental;
  }
}
