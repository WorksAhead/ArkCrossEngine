using System.Collections.Generic;
using StorySystem;

namespace ArkCrossEngine.GmCommands
{
    internal sealed class ClientGmStorySystem
    {
        private class StoryInstanceInfo
        {
            internal int m_StoryId;
            internal StoryInstance m_StoryInstance;
            internal bool m_IsUsed;
        }
        internal void Init()
        {
            //注册Gm命令
            StoryCommandManager.Instance.RegisterCommandFactory("switchdebug", new StoryCommandFactoryHelper<SwitchDebugCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("lobbyaddassets", new StoryCommandFactoryHelper<LobbyAddAssetsCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("lobbyadditem", new StoryCommandFactoryHelper<LobbyAddItemCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("lobbyresetmission", new StoryCommandFactoryHelper<LobbyResetDailyMissionsCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("updatemaxusercount", new StoryCommandFactoryHelper<UpdateMaxUserCountCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("setnewbieflag", new StoryCommandFactoryHelper<SetNewbieFlagCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("setnewbieactionflag", new StoryCommandFactoryHelper<SetNewbieActionFlagCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("mountequipment", new StoryCommandFactoryHelper<MountEquipmentCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("mountskill", new StoryCommandFactoryHelper<MountSkillCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("upgradeskill", new StoryCommandFactoryHelper<UpgradeSkillCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("unlockskill", new StoryCommandFactoryHelper<UnlockSkillCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("swapskill", new StoryCommandFactoryHelper<SwapSkillCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("liftskill", new StoryCommandFactoryHelper<LiftSkillCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("upgradeitem", new StoryCommandFactoryHelper<UpgradeItemCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("getmaillist", new StoryCommandFactoryHelper<GetMailListCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("receivemail", new StoryCommandFactoryHelper<ReceiveMailCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("sendmail", new StoryCommandFactoryHelper<SendMailCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("cancelmatch", new StoryCommandFactoryHelper<CancelMatchCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("addfriend", new StoryCommandFactoryHelper<AddFriendCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("delfriend", new StoryCommandFactoryHelper<DelFriendCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("queryfriend", new StoryCommandFactoryHelper<QueryFriendCommand>());

            //下面的命令只在单人pve场景内有效（仅修改客户端数据）
            StoryCommandManager.Instance.RegisterCommandFactory("levelto", new StoryCommandFactoryHelper<LevelToCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("full", new StoryCommandFactoryHelper<FullCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("clearequipments", new StoryCommandFactoryHelper<ClearEquipmentsCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("addequipment", new StoryCommandFactoryHelper<AddEquipmentCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("clearskills", new StoryCommandFactoryHelper<ClearSkillsCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("addskill", new StoryCommandFactoryHelper<AddSkillCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("clearbuffs", new StoryCommandFactoryHelper<ClearBuffsCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("addbuff", new StoryCommandFactoryHelper<AddBuffCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("sethd", new StoryCommandFactoryHelper<SetHdCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("showhd", new StoryCommandFactoryHelper<ShowHdCommand>());

            //
            StoryCommandManager.Instance.RegisterCommandFactory("allocmemory", new StoryCommandFactoryHelper<AllocMemoryCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("freememory", new StoryCommandFactoryHelper<FreeMemoryCommand>());
            StoryCommandManager.Instance.RegisterCommandFactory("consumecpu", new StoryCommandFactoryHelper<ConsumeCpuCommand>());

            //注册值与函数处理

        }

        internal int ActiveStoryCount
        {
            get
            {
                return m_StoryLogicInfos.Count;
            }
        }
        internal Dictionary<string, object> GlobalVariables
        {
            get { return m_GlobalVariables; }
        }
        internal void Reset()
        {
            m_GlobalVariables.Clear();
            int count = m_StoryLogicInfos.Count;
            for (int index = count - 1; index >= 0; --index)
            {
                StoryInstanceInfo info = m_StoryLogicInfos[index];
                if (null != info)
                {
                    RecycleStorylInstance(info);
                    m_StoryLogicInfos.RemoveAt(index);
                }
            }
            m_StoryLogicInfos.Clear();
        }
        internal void LoadStory(string file)
        {
            m_StoryInstancePool.Clear();
            m_ConfigManager.Clear();
            m_ConfigManager.LoadStory(file, 0);
        }
        internal void LoadStoryText(string text)
        {
            m_StoryInstancePool.Clear();
            m_ConfigManager.Clear();
            m_ConfigManager.LoadStoryText(text, 0);
        }
        internal void StartStory(int storyId)
        {
            StoryInstanceInfo inst = NewStoryInstance(storyId);
            if (null != inst)
            {
                m_StoryLogicInfos.Add(inst);
                inst.m_StoryInstance.Context = WorldSystem.Instance;
                inst.m_StoryInstance.GlobalVariables = m_GlobalVariables;
                inst.m_StoryInstance.Start();

                LogSystem.Debug("StartStory {0}", storyId);
            }
        }
        internal void StopStory(int storyId)
        {
            int count = m_StoryLogicInfos.Count;
            for (int index = count - 1; index >= 0; --index)
            {
                StoryInstanceInfo info = m_StoryLogicInfos[index];
                if (info.m_StoryId == storyId)
                {
                    RecycleStorylInstance(info);
                    m_StoryLogicInfos.RemoveAt(index);
                }
            }
        }
        internal void Tick()
        {
            long time = TimeUtility.GetLocalMilliseconds();
            int ct = m_StoryLogicInfos.Count;
            for (int ix = ct - 1; ix >= 0; --ix)
            {
                StoryInstanceInfo info = m_StoryLogicInfos[ix];
                info.m_StoryInstance.Tick(time);
                if (info.m_StoryInstance.IsTerminated)
                {
                    RecycleStorylInstance(info);
                    m_StoryLogicInfos.RemoveAt(ix);
                }
            }
        }
        internal void SendMessage(string msgId, params object[] args)
        {
            int ct = m_StoryLogicInfos.Count;
            for (int ix = ct - 1; ix >= 0; --ix)
            {
                StoryInstanceInfo info = m_StoryLogicInfos[ix];
                info.m_StoryInstance.SendMessage(msgId, args);
            }
        }

        private StoryInstanceInfo NewStoryInstance(int storyId)
        {
            StoryInstanceInfo instInfo = GetUnusedStoryInstanceInfoFromPool(storyId);
            if (null == instInfo)
            {
                StoryInstance inst = m_ConfigManager.NewStoryInstance(storyId, 0);

                if (inst == null)
                {
                    LogSystem.Error("Can't load story config, story:{0} !", storyId);
                    return null;
                }
                StoryInstanceInfo res = new StoryInstanceInfo();
                res.m_StoryId = storyId;
                res.m_StoryInstance = inst;
                res.m_IsUsed = true;

                AddStoryInstanceInfoToPool(storyId, res);
                return res;
            }
            else
            {
                instInfo.m_IsUsed = true;
                return instInfo;
            }
        }
        private void RecycleStorylInstance(StoryInstanceInfo info)
        {
            info.m_StoryInstance.Reset();
            info.m_IsUsed = false;
        }
        private void AddStoryInstanceInfoToPool(int storyId, StoryInstanceInfo info)
        {
            List<StoryInstanceInfo> infos;
            if (m_StoryInstancePool.TryGetValue(storyId, out infos))
            {
                infos.Add(info);
            }
            else
            {
                infos = new List<StoryInstanceInfo>();
                infos.Add(info);
                m_StoryInstancePool.Add(storyId, infos);
            }
        }
        private StoryInstanceInfo GetUnusedStoryInstanceInfoFromPool(int storyId)
        {
            StoryInstanceInfo info = null;
            List<StoryInstanceInfo> infos;
            if (m_StoryInstancePool.TryGetValue(storyId, out infos))
            {
                int ct = infos.Count;
                for (int ix = 0; ix < ct; ++ix)
                {
                    if (!infos[ix].m_IsUsed)
                    {
                        info = infos[ix];
                        break;
                    }
                }
            }
            return info;
        }

        private ClientGmStorySystem() { }

        private Dictionary<string, object> m_GlobalVariables = new Dictionary<string, object>();

        private List<StoryInstanceInfo> m_StoryLogicInfos = new List<StoryInstanceInfo>();
        private Dictionary<int, List<StoryInstanceInfo>> m_StoryInstancePool = new Dictionary<int, List<StoryInstanceInfo>>();

        private StoryConfigManager m_ConfigManager = StoryConfigManager.NewInstance();

        internal static ClientGmStorySystem Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static ClientGmStorySystem s_Instance = new ClientGmStorySystem();
    }
}
