using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using DashFire;
using ArkCrossEngine.Network;
using CSharpCenterClient;
using Messenger;
using Lobby_RoomServer;
using ArkCrossEngine;

namespace RoomServer
{
  internal enum RoomSrvStatus
  {
    STATUS_INIT = 0,
    STATUS_RUNNING = 1,
    STATUS_STOP = 2,
  }

  /// <remarks>
  /// 注意这个类的消息处理的逻辑里调用的其它方法，都要检查跨线程调用是否安全！！！
  /// </remarks>
  internal class RoomServer
  {
    private void Init(string[] args)
    {
      m_NameHandleCallback = this.OnNameHandleChanged;
      m_MsgCallback = this.OnMessage;
      m_CmdCallback = this.OnCommand;
      CenterClientApi.Init("roomserver", args.Length, args, m_NameHandleCallback, m_MsgCallback, m_CmdCallback);

      Console.WriteLine("begin init roomserver...");
      HomePath.InitHomePath();

      bool ret = LogSys.Init("./config/logconfig.xml");
      System.Diagnostics.Debug.Assert(ret);

      last_tick_time_ = TimeUtility.GetServerMilliseconds();
      last_send_roominfo_time_ = last_tick_time_;
      is_continue_register_ = true;
      channel_ = new PBChannel(MessageMapping.Query, MessageMapping.Query);
      channel_.DefaultServiceName = "Lobby";
      lobby_connector_ = new Connector(channel_);

      server_ip_ = "127.0.0.1";
      server_port_ = 9528;

      StringBuilder sb = new StringBuilder(256);
      if (CenterClientApi.GetConfig("name", sb, 256)) {
        room_server_name_ = sb.ToString();
      }
      if (CenterClientApi.GetConfig("ServerIp", sb, 256)) {
        server_ip_ = sb.ToString();
      }
      if (CenterClientApi.GetConfig("ServerPort", sb, 256)) {
        server_port_ = uint.Parse(sb.ToString());
      }
      if (CenterClientApi.GetConfig("Debug", sb, 256)) {
        int debug = int.Parse(sb.ToString());
        if (debug != 0) {
          GlobalVariables.Instance.IsDebug = true;
        }
      }

      GlobalVariables.Instance.IsClient = false;

      FileReaderProxy.RegisterReadFileHandler((string filePath) => {
        byte[] buffer = null;
        try {
          buffer = File.ReadAllBytes(filePath);
        } catch (Exception e) {
          LogSys.Log(LOG_TYPE.ERROR, "Exception:{0}\n{1}", e.Message, e.StackTrace);
          return null;
        }
        return buffer;
      }, (string filepath) => { return File.Exists(filepath); });
      LogSystem.OnOutput += (Log_Type type, string msg) => {
        switch (type) {
          case Log_Type.LT_Debug:
            LogSys.Log(LOG_TYPE.DEBUG, msg);
            break;
          case Log_Type.LT_Info:
            LogSys.Log(LOG_TYPE.INFO, msg);
            break;
          case Log_Type.LT_Warn:
            LogSys.Log(LOG_TYPE.WARN, msg);
            break;
          case Log_Type.LT_Error:
          case Log_Type.LT_Assert:
            LogSys.Log(LOG_TYPE.ERROR, msg);
            break;
        }
      };

      LoadData();

      LogSys.Log(LOG_TYPE.DEBUG, "room server init ip: {0}  port: {1}", server_ip_, server_port_);

      ret = Serialize.Init();
      if (!ret) {
        LogSys.Log(LOG_TYPE.DEBUG, "Serialize init error !!!");
      } else {
        LogSys.Log(LOG_TYPE.DEBUG, "Serialize init OK.");
      }
            
      thread_count_ = 16;
      per_thread_room_count_ = 20;
      uint tick_interval = 50;
      room_mgr_ = new RoomManager(thread_count_, per_thread_room_count_,
          tick_interval, lobby_connector_);
      room_mgr_.Init();
      IOManager.Instance.Init((int)server_port_);
      room_mgr_.StartRoomThread();
      AiViewManager.Instance.Init();
      SceneLogicViewManager.Instance.Init();
      ImpactViewManager.Instance.Init();

      ServerSkillSystem.StaticInit();
      ServerStorySystem.StaticInit();
      DashFire.GmCommands.GmStorySystem.StaticInit();

      channel_.Register<Msg_LR_ReplyRegisterRoomServer>(HandleReplyRegisterRoomServer);
      room_mgr_.RegisterMsgHandler(channel_);

      LogSys.Log(LOG_TYPE.DEBUG, "room server init ok.");     
    }
    private void Loop()
    {
      try {
        while (CenterClientApi.IsRun()) {
          CenterClientApi.Tick();
          Tick();
          Thread.Sleep(10);
        }
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "RoomServer.Loop throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
      }
    }
    private void Release()
    {
      room_mgr_.StopRoomThread();
      CenterClientApi.Release();
      Thread.Sleep(3000);
      //简单点，直接kill掉自己
      System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    private void Tick()
    {
      long curTime = TimeUtility.GetServerMilliseconds();
      if (last_tick_time_ + c_tick_interval_ms < curTime) {
        last_tick_time_ = curTime;

        if (is_continue_register_) {
          SendRegisterRoomServer();
        }
      }
	  if(!is_continue_register_)
      	SendRoomServerUpdateInfo();
    }

    private void OnNameHandleChanged(bool addOrUpdate, string name, int handle)
    {
      try {
        channel_.OnUpdateNameHandle(addOrUpdate, name, handle);
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
      }
    }
    private void OnCommand(int src, int dest, string command)
    {
      try {
        if (0 == command.CompareTo("QuitRoomServer")) {
          LogSys.Log(LOG_TYPE.MONITOR, "receive {0} command, quit", command);
          CenterClientApi.Quit();
        }
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
      }
    }
    private void OnMessage(uint seq, int source_handle,
        int dest_handle,
        IntPtr data, int len)
    {
      try {
        byte[] bytes = new byte[len];
        Marshal.Copy(data, bytes, 0, len);
        channel_.Dispatch(source_handle, seq, bytes);
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
      }
    }
    
    private void LoadData()
    {
      SceneConfigProvider.Instance.Load(FilePathDefine_Server.C_SceneConfig, "ScenesConfigs");
      SceneConfigProvider.Instance.LoadDropOutConfig(FilePathDefine_Server.C_SceneDropOut, "SceneDropOut");
      SceneConfigProvider.Instance.LoadAllSceneConfig(FilePathDefine_Server.C_RootPath);
      AttributeScoreConfigProvider.Instance.Load(FilePathDefine_Server.C_AttributeScoreConfig, "AttributeScoreConfig");

      ActionConfigProvider.Instance.Load(FilePathDefine_Server.C_ActionConfig, "ActionConfig");
      NpcConfigProvider.Instance.LoadNpcConfig(FilePathDefine_Server.C_NpcConfig, "NpcConfig");
      NpcConfigProvider.Instance.LoadNpcLevelupConfig(FilePathDefine_Server.C_NpcLevelupConfig, "NpcLevelupConfig");
      PlayerConfigProvider.Instance.LoadPlayerConfig(FilePathDefine_Server.C_PlayerConfig, "PlayerConfig");
      PlayerConfigProvider.Instance.LoadPlayerLevelupConfig(FilePathDefine_Server.C_PlayerLevelupConfig, "PlayerLevelupConfig");
      PlayerConfigProvider.Instance.LoadPlayerLevelupExpConfig(FilePathDefine_Server.C_PlayerLevelupExpConfig, "PlayerLevelupExpConfig");
      CriticalConfigProvider.Instance.Load(FilePathDefine_Server.C_CriticalConfig, "CriticalConfig");
      BuffConfigProvider.Instance.Load(FilePathDefine_Server.C_BuffConfig, "BuffConfig");

      AiActionConfigProvider.Instance.Load(FilePathDefine_Server.C_AiActionConfig, "AiActionConfig");
      AiConfigProvider.Instance.Load(FilePathDefine_Server.C_AiConfig, "AiConfig");
      AiSkillComboListProvider.Instance.Load(FilePathDefine_Server.C_AiSkillComboListConfig, "AiSkillComboListConfig");
      SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_SOUND, FilePathDefine_Server.C_SkillSoundConfig, "SoundConfig"); 
      SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_SKILL, FilePathDefine_Server.C_SkillSystemConfig, "SkillConfig");
      SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_IMPACT, FilePathDefine_Server.C_ImpactSystemConfig, "ImpactConfig");
      SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_EFFECT, FilePathDefine_Server.C_EffectConfig, "EffectConfig");

      ItemConfigProvider.Instance.Load(FilePathDefine_Server.C_ItemConfig, "ItemConfig");
      EquipmentConfigProvider.Instance.LoadEquipmentConfig(FilePathDefine_Server.C_EquipmentConfig, "EquipmentConfig");
      AppendAttributeConfigProvider.Instance.Load(FilePathDefine_Server.C_AppendAttributeConfig, "AppendAttributeConfig");
      PartnerConfigProvider.Instance.Load(FilePathDefine_Server.C_PartnerConfig, "ParterConfig");

      ExpeditionMonsterAttrConfigProvider.Instance.Load(FilePathDefine_Server.C_ExpeditionMonsterAttrConfig, "ExpeditionMonsterAttrConfig");
      MpveMonsterConfigProvider.Instance.Load(FilePathDefine_Server.C_MpveMonsterConfig, "MpveMonsterConfig");
    }    

    private void SendRegisterRoomServer()
    {
      Msg_RL_RegisterRoomServer.Builder rrsBuilder = Msg_RL_RegisterRoomServer.CreateBuilder();
      rrsBuilder.SetServerName(room_server_name_);
      rrsBuilder.SetMaxRoomNum((int)(thread_count_ * per_thread_room_count_));
      rrsBuilder.SetServerIp(server_ip_);
      rrsBuilder.SetServerPort(server_port_);
      channel_.Send(rrsBuilder.Build());
      LogSys.Log(LOG_TYPE.DEBUG, "register room server to Lobby.");
    }

    private void SendRoomServerUpdateInfo()
    {
      long curTime = TimeUtility.GetServerMilliseconds();
      int ts = (int)(curTime - last_send_roominfo_time_);
      if (ts >= c_send_interval_ms) {
        last_send_roominfo_time_ = curTime;
        Msg_RL_RoomServerUpdateInfo.Builder msgBuilder = Msg_RL_RoomServerUpdateInfo.CreateBuilder();
        msgBuilder.SetServerName(room_server_name_);
        msgBuilder.SetIdleRoomNum(room_mgr_.GetIdleRoomCount());
        msgBuilder.SetUserNum(room_mgr_.GetUserCount());
        channel_.Send(msgBuilder.Build());
        LogSys.Log(LOG_TYPE.DEBUG, "send room info to Lobby, Name:{0} IdleRoomNum:{1} UserNum:{2}.", room_server_name_, room_mgr_.GetIdleRoomCount(), room_mgr_.GetUserCount());
      }
    }
    
    private void HandleReplyRegisterRoomServer(Msg_LR_ReplyRegisterRoomServer msg, PBChannel channel, int handle, uint seq)
    {
      if (msg.IsOk == true) {
        is_continue_register_ = false;
      }
    }

    private const int c_tick_interval_ms = 5000;           // tick间隔
    private const int c_send_interval_ms = 1000;           // 发送间隔

    private RoomManager room_mgr_;
    private long last_tick_time_;
    private long last_send_roominfo_time_;// 上一次发送房间信息的时间
    private uint thread_count_;
    private uint per_thread_room_count_;
    private bool is_continue_register_;
    private string server_ip_;
    private uint server_port_;
    private Connector lobby_connector_;
    private PBChannel channel_;

    private string room_server_name_;

    private CenterClientApi.HandleNameHandleChangedCallback m_NameHandleCallback = null;
    private CenterClientApi.HandleMessageCallback m_MsgCallback = null;
    private CenterClientApi.HandleCommandCallback m_CmdCallback = null;


    internal static void Main(string[] args)
    {
      RoomServer svr = new RoomServer();
      svr.Init(args);
      svr.Loop();
      svr.Release();
    }
  }
}
