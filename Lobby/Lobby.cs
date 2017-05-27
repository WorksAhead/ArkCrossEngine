using System;
using System.IO;
using System.Text;
using CSharpCenterClient;
using Messenger;
using Newtonsoft.Json;
using Lobby;
using DashFire;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DashFire.DataStore;
using DashFire.Billing;
using System.Threading;
using ArkCrossEngine;

namespace Lobby
{
    /// Log
    internal enum Module
    {
        login,
        rolebuild,
        rolelogin,
        logout,
        levelup,
        acquire,
        moneycost,
        getitem,
        removeitem,
        gettask,
        finishtask,
        pvefight,
        heart,
        customacquire,
        customcost,
        newstages,
        activity,
        partner,
        pvp,
        arena,
        serverevent,
    }
    internal partial class LobbyServer
    {
        #region Singleton
        private static LobbyServer s_Instance = new LobbyServer();
        internal static LobbyServer Instance
        {
            get
            {
                return s_Instance;
            }
        }
        #endregion

        internal Messenger.PBChannel RoomSvrChannel
        {
            get { return m_RoomSvrChannel; }
        }
        internal Messenger.PBChannel BridgeChannel
        {
            get { return m_BridgeChannel; }
        }
        internal Messenger.PBChannel StoreChannel
        {
            get { return m_StoreChannel; }
        }
        internal Messenger.PBChannel GmSvrChannel
        {
            get { return m_GmSvrChannel; }
        }
        internal QueueingThread QueueingThread
        {
            get { return m_QueueingThread; }
        }
        internal MatchFormThread MatchFormThread
        {
            get { return m_MatchFormThread; }
        }
        internal RoomProcessThread RoomProcessThread
        {
            get { return m_RoomProcessThread; }
        }
        internal DataStoreThread DataStoreThread
        {
            get { return m_DataStoreThread; }
        }
        internal GmServerThread GmServerThread
        {
            get { return m_GmServerThread; }
        }
        internal ServerBridgeThread ServerBridgeThread
        {
            get { return m_ServerBridgeThread; }
        }
        internal GlobalDataProcessThread GlobalDataProcessThread
        {
            get { return m_GlobalDataProcessThread; }
        }
        internal DataProcessScheduler DataProcessScheduler
        {
            get { return m_DataProcessScheduler; }
        }
        internal List<SceneInfo> SceneList
        {
            get { return m_SceneInfos; }
        }
        internal bool IsUnknownServer(int handle)
        {
            return !IsNode(handle) && !IsRoomServer(handle) && !IsBridge(handle) && !IsStore(handle);
        }
        internal bool IsNode(int handle)
        {
            return m_NodeHandles.Contains(handle);
        }
        internal bool IsRoomServer(int handle)
        {
            return m_RoomSvrHandles.Contains(handle);
        }
        internal bool IsBridge(int handle)
        {
            return m_BridgeChannel.DefaultServiceHandle == handle;
        }
        internal bool IsGmServer(int handle)
        {
            return m_GmSvrChannel.DefaultServiceHandle == handle;
        }
        internal bool IsStore(int handle)
        {
            return m_StoreChannel.DefaultServiceHandle == handle;
        }
        internal bool TypeIsPvp(int id)
        {
            SceneInfo tmp = null;
            foreach (SceneInfo si in m_SceneInfos)
            {
                if (si.SceneID == id)
                {
                    tmp = si;
                    break;
                }
            }
            if (null == tmp)
            {
                LogSys.Log(LOG_TYPE.ERROR, "TypeIsPvp({0}) can't find sceneinfo", id);
                return false;
            }
            if (tmp.Type == SceneTypeEnum.TYPE_PVP)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        internal bool TypeIsMpve(int id)
        {
            SceneInfo tmp = null;
            foreach (SceneInfo si in m_SceneInfos)
            {
                if (si.SceneID == id)
                {
                    tmp = si;
                    break;
                }
            }
            if (null == tmp)
            {
                LogSys.Log(LOG_TYPE.ERROR, "TypeIsPvp({0}) can't find sceneinfo", id);
                return false;
            }
            if (tmp.Type == SceneTypeEnum.TYPE_MULTI_PVE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Init(string[] args)
        {
            m_NameHandleCallback = this.OnNameHandleChanged;
            m_MsgCallback = this.OnMessage;
            m_CmdCallback = this.OnCommand;
            CenterClientApi.Init("lobby", args.Length, args, m_NameHandleCallback, m_MsgCallback, m_CmdCallback);

            LogSys.Init("./config/logconfig.xml");
            LobbyConfig.Init();

            if (LobbyConfig.IsDebug)
            {
                GlobalVariables.Instance.IsDebug = true;
            }

            GlobalVariables.Instance.IsClient = false;

            string key = "防君子不防小人";
            byte[] xor = Encoding.UTF8.GetBytes(key);

            FileReaderProxy.RegisterReadFileHandler((string filePath) =>
            {
                byte[] buffer = null;
                try
                {
                    buffer = File.ReadAllBytes(filePath);
#if !DEBUG
          if (filePath.EndsWith(".txt")) {
            Helper.Xor(buffer, xor);
          }
#endif
                }
                catch (Exception e)
                {
                    LogSys.Log(LOG_TYPE.ERROR, "Exception:{0}\n{1}", e.Message, e.StackTrace);
                    return null;
                }
                return buffer;
            });
            LogSystem.OnOutput += (Log_Type type, string msg) =>
            {
                switch (type)
                {
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

            LogSys.Log(LOG_TYPE.INFO, "Init Config ...");
            s_Instance = this;
            InstallMessageHandlers();
            LogSys.Log(LOG_TYPE.INFO, "Init Messenger ...");
            m_DataStoreThread.Init(m_StoreChannel);
            LogSys.Log(LOG_TYPE.INFO, "Init DataStore ...");
            m_ServerBridgeThread.Init(m_BridgeChannel);
            LogSys.Log(LOG_TYPE.INFO, "Init BillingClient ...");
            m_GmServerThread.Init(m_GmSvrChannel);
            LogSys.Log(LOG_TYPE.INFO, "Init GmServerThread ...");
            Start();
            LogSys.Log(LOG_TYPE.INFO, "Start Threads ...");
        }
        private void Loop()
        {
            try
            {
                while (CenterClientApi.IsRun())
                {
                    CenterClientApi.Tick();
                    Thread.Sleep(10);
                    if (m_WaitQuit && m_GlobalDataProcessThread.LastSaveFinished && m_DataProcessScheduler.LastSaveFinished)
                    {
                        LogSys.Log(LOG_TYPE.MONITOR, "Lobby quit.");
                        CenterClientApi.Quit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Lobby.Loop throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void Release()
        {
            Stop();
            CenterClientApi.Release();
            Thread.Sleep(3000);
            //简单点，kill掉自己
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void OnNameHandleChanged(bool addOrUpdate, string name, int handle)
        {
            try
            {
                m_GmSvrChannel.OnUpdateNameHandle(addOrUpdate, name, handle);
                m_BridgeChannel.OnUpdateNameHandle(addOrUpdate, name, handle);
                m_StoreChannel.OnUpdateNameHandle(addOrUpdate, name, handle);
                if (!addOrUpdate)
                {
                    m_NodeHandles.Remove(handle);
                    m_RoomSvrHandles.Remove(handle);
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void OnCommand(int src, int dest, string command)
        {
            try
            {
                if (0 == command.CompareTo("QuitLobby"))
                {
                    LogSys.Log(LOG_TYPE.MONITOR, "receive {0} command, save data and then quitting ...", command);
                    if (!m_WaitQuit)
                    {
                        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.DoLastSaveData);
                        m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.DoLastSaveGlobalData);
                        m_WaitQuit = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void OnMessage(uint seq, int source_handle, int dest_handle,
            IntPtr data, int len)
        {
            try
            {
                if (IsUnknownServer(source_handle))
                {
                    StringBuilder sb = new StringBuilder(256);
                    if (CenterClientApi.TargetName(source_handle, sb, 256))
                    {
                        string name = sb.ToString();
                        if (name.StartsWith("NodeJs"))
                        {
                            m_NodeHandles.Add(source_handle);
                        }
                        else if (name.StartsWith("RoomSvr"))
                        {
                            m_RoomSvrHandles.Add(source_handle);
                        }
                    }
                }
                byte[] bytes = new byte[len];
                Marshal.Copy(data, bytes, 0, len);
                if (IsNode(source_handle))
                {
                    if (!GlobalVariables.Instance.IsWaitQuit)
                    {
                        JsonMessageDispatcher.HandleDcoreMessage(seq, source_handle, dest_handle, bytes);
                    }
                }
                else if (IsRoomServer(source_handle))
                {
                    m_RoomSvrChannel.Dispatch(source_handle, seq, bytes);
                }
                else if (IsBridge(source_handle))
                {
                    m_BridgeChannel.Dispatch(source_handle, seq, bytes);
                }
                else if (IsStore(source_handle))
                {
                    m_StoreChannel.Dispatch(source_handle, seq, bytes);
                }
                else if (IsGmServer(source_handle))
                {
                    m_GmSvrChannel.Dispatch(source_handle, seq, bytes);
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void LoadData()
        {
            GmConfigProvider.Instance.Load(FilePathDefine_Server.C_GmListConfig, "GmListConfig");
            ItemConfigProvider.Instance.Load(FilePathDefine_Server.C_ItemConfig, "ItemConfig");
            ItemLevelupConfigProvider.Instance.Load(FilePathDefine_Server.C_ItemLevelupConfig, "ItemLevelupConfig");
            ItemCompoundConfigProvider.Instance.Load(FilePathDefine_Server.C_ItemCompoundConfig, "ItemCompoundConfig");
            XSoulLevelConfigProvider.Instance.Load(FilePathDefine_Server.C_XSoulLevelConfig, "XSoulLevelConfig");
            SceneConfigProvider.Instance.Load(FilePathDefine_Server.C_SceneConfig, "ScenesConfigs");
            SceneConfigProvider.Instance.LoadDropOutConfig(FilePathDefine_Server.C_SceneDropOut, "ScenesDropOut");
            SceneConfigProvider.Instance.InitChapterData();
            SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_SKILL, FilePathDefine_Server.C_SkillSystemConfig, "SkillConfig");
            SkillLevelupConfigProvider.Instance.Load(FilePathDefine_Server.C_SkillLevelupConfig, "SkillLevelupConfig");
            MissionConfigProvider.Instance.Load(FilePathDefine_Server.C_MissionConfig, "MissionConfig");
            PartnerConfigProvider.Instance.Load(FilePathDefine_Server.C_PartnerConfig, "PartnerConfig");
            PartnerLevelUpConfigProvider.Instance.Load(FilePathDefine_Server.C_PartnerLevelUpConfig, "PartnerLevelUpConfig");
            PartnerStageUpConfigProvider.Instance.Load(FilePathDefine_Server.C_PartnerStageUpConfig, "PartnerStageUpConfig");
            PlayerConfigProvider.Instance.LoadPlayerConfig(FilePathDefine_Server.C_PlayerConfig, "PlayerConfig");
            PlayerConfigProvider.Instance.LoadPlayerLevelupConfig(FilePathDefine_Server.C_PlayerLevelupConfig, "PlayerLevelupConfig");
            PlayerConfigProvider.Instance.LoadPlayerLevelupExpConfig(FilePathDefine_Server.C_PlayerLevelupExpConfig, "PlayerLevelupExpConfig");
            BuyStaminaConfigProvider.Instance.Load(FilePathDefine_Server.C_BuyStaminaConfig, "BuyStaminaConfig");
            BuyMoneyConfigProvider.Instance.Load(FilePathDefine_Server.C_BuyMoneyConfig, "BuyMoneyConfig");
            AppendAttributeConfigProvider.Instance.Load(FilePathDefine_Server.C_AppendAttributeConfig, "AppendAttributeConfig");
            LegacyLevelupConfigProvider.Instance.Load(FilePathDefine_Server.C_LegacyLevelupConfig, "LegacyLevelupConfig");
            LegacyComplexAttrConifgProvider.Instance.Load(FilePathDefine_Server.C_LegacyComplexAttrConifg, "LegacyComplexAttrConifg");
            ExpeditionTollgateConfigProvider.Instance.Load(FilePathDefine_Server.C_ExpeditionTollgateConfig, "ExpeditionTollgateConfig");
            ExpeditionMonsterConfigProvider.Instance.Load(FilePathDefine_Server.C_ExpeditionMonsterConfig, "ExpeditionMonsterConfig");
            GowConfigProvider.Instance.LoadForServer();
            MpveTimeConfigProvider.Instance.Load(FilePathDefine_Server.C_MpveTimeConfig, "MpveTimeConfig");
            ArenaConfigProvider.Instance.LoadServerConfig();
            LogicServerConfigProvider.Instance.Load(FilePathDefine_Server.C_LogicServerConfig, "LogicServerConfig");
            VipConfigProvider.Instance.Load(FilePathDefine_Server.C_VipConfig, "VipConfig");
            VersionConfigProvider.Instance.Load(FilePathDefine_Server.C_VersionConfig, "VersionConfig");
            AttemptTollgateConfigProvider.Instance.Load(FilePathDefine_Server.C_AttemptTollgateConfig, "AttemptTollgateConfig");
            StoreConfigProvider.Instance.Load(FilePathDefine_Server.C_ExchangeShopConfig, "ExchangeShop");
            WeeklyLoginConfigProvider.Instance.Load(FilePathDefine_Server.C_WeeklyLoginConfig, "WeeklyLogin");
            SignInRewardConfigProvider.Instance.Load(FilePathDefine_Server.C_SignInRewardConfig, "SignInReward");
            MonthCardConfigProvider.Instacne.Load(FilePathDefine_Server.C_MonthCardConfig, "MonthCard");
            GiftConfigProvider.Instance.Load(FilePathDefine_Server.C_GiftConfig, "Gift");
            OnlineDurationRewardConfigProvider.Instance.Load(FilePathDefine_Server.C_OnlineDurationRewardConfig, "OnlineDurationRewardConfig");
            PaymentRebateConfigProvider.Instacne.Load(FilePathDefine_Server.C_PaymentRebateConfig, "PaymentRebateConfig");
            LevelLockProvider.Instance.Load(FilePathDefine_Server.C_LevelLock, "LevelLock");
            foreach (Data_SceneConfig sceneConfigData in SceneConfigProvider.Instance.SceneConfigMgr.GetData().Values)
            {
                SceneInfo sceneInfo = new SceneInfo();
                sceneInfo.SceneID = sceneConfigData.m_Id;
                sceneInfo.Type = (SceneTypeEnum)sceneConfigData.m_Type;
                sceneInfo.SubType = (SceneSubTypeEnum)sceneConfigData.m_SubType;
                m_SceneInfos.Add(sceneInfo);
            }
            /// 
            SetModuleLevelLock();
        }
        private int GetUnlockLevelById(int id)
        {
            LevelLock data = LevelLockProvider.Instance.GetDataById(id);
            return null != data ? data.m_Level : 1;
        }
        private void SetModuleLevelLock()
        {
            const int expedition_unlock_index = 13;
            ExpeditionPlayerInfo.UnlockLevel = GetUnlockLevelById(expedition_unlock_index);
            const int attempt_unlock_index = 14;
            MpveMatchHelper.AttemptUnlockLevel = GetUnlockLevelById(attempt_unlock_index);
            const int gold_unlock_index = 15;
            MpveMatchHelper.GoldUnlockLevel = GetUnlockLevelById(gold_unlock_index);
            const int pvap_unlock_index = 17;
            ArenaSystem.UNLOCK_LEVEL = GetUnlockLevelById(pvap_unlock_index);
            const int pvp_unlock_index = 18;
            GowSystem.m_UnlockLevel = GetUnlockLevelById(pvp_unlock_index);
            const int dare_unlock_index = 19;
            DareSystem.UNLOCK_LEVEL = GetUnlockLevelById(dare_unlock_index);
        }
        private void Start()
        {
            m_ServerBridgeThread.Start();
            m_GlobalDataProcessThread.Start();
            m_DataProcessScheduler.Start();
            m_RoomProcessThread.Start();
            m_MatchFormThread.Start();
            m_QueueingThread.Start();
            m_DataStoreThread.Start();
            m_GmServerThread.Start();
        }
        private void Stop()
        {
            m_QueueingThread.Stop();
            m_MatchFormThread.Stop();
            m_RoomProcessThread.Stop();
            m_DataProcessScheduler.Stop();
            m_GlobalDataProcessThread.Stop();
            m_ServerBridgeThread.Stop();
            m_DataStoreThread.Stop();
            m_GmServerThread.Stop();
        }
        private void InstallMessageHandlers()
        {
            m_StoreChannel = new PBChannel(DashFire.DataStore.MessageMapping.Query,
                                DashFire.DataStore.MessageMapping.Query);
            m_StoreChannel.DefaultServiceName = "DataStoreNode";
            m_BridgeChannel = new PBChannel(DashFire.Billing.MessageMapping.Query,
                                DashFire.Billing.MessageMapping.Query);
            m_BridgeChannel.DefaultServiceName = "ServerBridge";
            m_GmSvrChannel = new PBChannel(DashFire.DataStore.MessageMapping.Query,
                                DashFire.DataStore.MessageMapping.Query);
            m_GmSvrChannel.DefaultServiceName = "GmServer";

            InstallJsonHandlers();
            InstallServerHandlers();
        }

        private const int c_MaxWaitLoginUserNum = 3000;
        private bool m_WaitQuit = false;

        private DataProcessScheduler m_DataProcessScheduler = new DataProcessScheduler();
        private GlobalDataProcessThread m_GlobalDataProcessThread = new GlobalDataProcessThread();
        private MatchFormThread m_MatchFormThread = new MatchFormThread();
        private RoomProcessThread m_RoomProcessThread = new RoomProcessThread();
        private DataStoreThread m_DataStoreThread = new DataStoreThread();
        private ServerBridgeThread m_ServerBridgeThread = new ServerBridgeThread();
        private QueueingThread m_QueueingThread = new QueueingThread();
        private GmServerThread m_GmServerThread = new GmServerThread();

        private List<SceneInfo> m_SceneInfos = new List<SceneInfo>();

        private HashSet<int> m_NodeHandles = new HashSet<int>();
        private HashSet<int> m_RoomSvrHandles = new HashSet<int>();
        private PBChannel m_RoomSvrChannel = null;
        private PBChannel m_BridgeChannel = null;
        private PBChannel m_StoreChannel = null;
        private PBChannel m_GmSvrChannel = null;

        private CenterClientApi.HandleNameHandleChangedCallback m_NameHandleCallback = null;
        private CenterClientApi.HandleMessageCallback m_MsgCallback = null;
        private CenterClientApi.HandleCommandCallback m_CmdCallback = null;

        internal static void Main(string[] args)
        {
            LobbyServer lobby = LobbyServer.Instance;
            lobby.Init(args);
            lobby.Loop();
            lobby.Release();
        }
    }
}
