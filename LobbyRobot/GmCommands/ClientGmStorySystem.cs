using System;
using System.Collections.Generic;
using StorySystem;
using LobbyRobot;
using ArkCrossEngine;

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
    internal void Init(Robot robot)
    {
      m_Robot = robot;
      StaticInit();
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
    internal void LoadStory(string file)
    {
      m_StoryInstancePool.Clear();
      StoryConfigManager.Instance.LoadStory(file, 0);
    }
    internal void LoadStoryText(string text)
    {
      m_StoryInstancePool.Clear();
      StoryConfigManager.Instance.LoadStoryText(text, 0);
    }
    internal void StartStory(int storyId, object context)
    {
      StoryInstanceInfo inst = NewStoryInstance(storyId);
      if (null != inst) {
        m_StoryLogicInfos.Add(inst);
        inst.m_StoryInstance.Context = context;
        inst.m_StoryInstance.GlobalVariables = m_GlobalVariables;
        inst.m_StoryInstance.Start();

        LogSystem.Info("StartStory {0}", storyId);
      }
    }
    internal void StopStory(int storyId)
    {
      int count = m_StoryLogicInfos.Count;
      for (int index = count - 1; index >= 0; --index) {
        StoryInstanceInfo info = m_StoryLogicInfos[index];
        if (info.m_StoryId == storyId) {
          RecycleStorylInstance(info);
          m_StoryLogicInfos.RemoveAt(index);
        }
      }
    }
    internal void StopAllStories()
    {
      int count = m_StoryLogicInfos.Count;
      for (int index = count - 1; index >= 0; --index) {
        StoryInstanceInfo info = m_StoryLogicInfos[index];
        RecycleStorylInstance(info);
        m_StoryLogicInfos.RemoveAt(index);
      }
    }
    internal void Tick()
    {
      long time = TimeUtility.GetLocalMilliseconds();
      int ct = m_StoryLogicInfos.Count;
      for (int ix = ct - 1; ix >= 0; --ix) {
        StoryInstanceInfo info = m_StoryLogicInfos[ix];
        info.m_StoryInstance.Tick(time);
        if (info.m_StoryInstance.IsTerminated) {
          RecycleStorylInstance(info);
          m_StoryLogicInfos.RemoveAt(ix);
        }
      }
    }
    internal void SendMessage(string msgId, params object[] args)
    {
      int ct = m_StoryLogicInfos.Count;
      for (int ix = ct - 1; ix >= 0; --ix) {
        StoryInstanceInfo info = m_StoryLogicInfos[ix];
        info.m_StoryInstance.SendMessage(msgId, args);
      }
    }

    private StoryInstanceInfo NewStoryInstance(int storyId)
    {
      StoryInstanceInfo instInfo = GetUnusedStoryInstanceInfoFromPool(storyId);
      if (null == instInfo) {
        StoryInstance inst = StoryConfigManager.Instance.NewStoryInstance(storyId, 0);

        if (inst == null) {
          LogSystem.Error("Can't load story config, story:{0} !", storyId);
          return null;
        }
        StoryInstanceInfo res = new StoryInstanceInfo();
        res.m_StoryId = storyId;
        res.m_StoryInstance = inst;
        res.m_IsUsed = true;

        AddStoryInstanceInfoToPool(storyId, res);
        return res;
      } else {
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
      if (m_StoryInstancePool.ContainsKey(storyId)) {
        List<StoryInstanceInfo> infos = m_StoryInstancePool[storyId];
        infos.Add(info);
      } else {
        List<StoryInstanceInfo> infos = new List<StoryInstanceInfo>();
        infos.Add(info);
        m_StoryInstancePool.Add(storyId, infos);
      }
    }
    private StoryInstanceInfo GetUnusedStoryInstanceInfoFromPool(int storyId)
    {
      StoryInstanceInfo info = null;
      if (m_StoryInstancePool.ContainsKey(storyId)) {
        List<StoryInstanceInfo> infos = m_StoryInstancePool[storyId];
        int ct = infos.Count;
        for (int ix = 0; ix < ct; ++ix) {
          if (!infos[ix].m_IsUsed) {
            info = infos[ix];
            break;
          }
        }
      }
      return info;
    }

    private Dictionary<string, object> m_GlobalVariables = new Dictionary<string, object>();
    
    private List<StoryInstanceInfo> m_StoryLogicInfos = new List<StoryInstanceInfo>();
    private Dictionary<int, List<StoryInstanceInfo>> m_StoryInstancePool = new Dictionary<int, List<StoryInstanceInfo>>();

    private Robot m_Robot = null;

    private static void StaticInit()
    {
      if (!s_Inited) {
        s_Inited = true;

        //通用命令
        StoryCommandManager.Instance.RegisterCommandFactory("startscript", new StoryCommandFactoryHelper<GmCommands.StartScriptCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("stopscript", new StoryCommandFactoryHelper<GmCommands.StopScriptCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("firemessage", new StoryCommandFactoryHelper<GmCommands.FireMessageCommand>());

        //注册Gm命令
        StoryCommandManager.Instance.RegisterCommandFactory("selectscene", new StoryCommandFactoryHelper<SelectSceneCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("cancelmatch", new StoryCommandFactoryHelper<CancelMatchCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("requestusers", new StoryCommandFactoryHelper<RequestUsersCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("updateposition", new StoryCommandFactoryHelper<UpdatePositionCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("stagclear", new StoryCommandFactoryHelper<StagClearCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("lobbyaddassets", new StoryCommandFactoryHelper<LobbyAddAssetsCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("lobbyadditem", new StoryCommandFactoryHelper<LobbyAddItemCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("mountequipment", new StoryCommandFactoryHelper<MountEquipmentCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("mountskill", new StoryCommandFactoryHelper<MountSkillCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("upgradeskill", new StoryCommandFactoryHelper<UpgradeSkillCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("unlockskill", new StoryCommandFactoryHelper<UnlockSkillCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("swapskill", new StoryCommandFactoryHelper<SwapSkillCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("liftskill", new StoryCommandFactoryHelper<LiftSkillCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("upgradeitem", new StoryCommandFactoryHelper<UpgradeItemCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("expeditionreset", new StoryCommandFactoryHelper<ExpeditionResetCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("requestexpedition", new StoryCommandFactoryHelper<RequestExpeditionCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("finishexpedition", new StoryCommandFactoryHelper<FinishExpeditionCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("expeditionaward", new StoryCommandFactoryHelper<ExpeditionAwardCommand>());

        StoryCommandManager.Instance.RegisterCommandFactory("quitroom", new StoryCommandFactoryHelper<QuitRoomCommand>());

        StoryCommandManager.Instance.RegisterCommandFactory("face", new StoryCommandFactoryHelper<FaceCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("movestart", new StoryCommandFactoryHelper<MoveStartCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("movestop", new StoryCommandFactoryHelper<MoveStopCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("skill", new StoryCommandFactoryHelper<SkillCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("npcskill", new StoryCommandFactoryHelper<NpcSkillCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("sendimpact", new StoryCommandFactoryHelper<SendImpactCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("movetopos", new StoryCommandFactoryHelper<MoveToPosCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("movetoattack", new StoryCommandFactoryHelper<MoveToAttackCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("giveupcombat", new StoryCommandFactoryHelper<GiveUpCombatCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("deletedeadnpc", new StoryCommandFactoryHelper<DeleteDeadNpcCommand>());

        StoryCommandManager.Instance.RegisterCommandFactory("quitbattle", new StoryCommandFactoryHelper<QuitBattleCommand>());

        StoryCommandManager.Instance.RegisterCommandFactory("reconnectlobby", new StoryCommandFactoryHelper<ReconnectLobbyCommand>());
        StoryCommandManager.Instance.RegisterCommandFactory("disconnectlobby", new StoryCommandFactoryHelper<DisconnectLobbyCommand>());

        StoryCommandManager.Instance.RegisterCommandFactory("updatemaxusercount", new StoryCommandFactoryHelper<UpdateMaxUserCountCommand>());

        //注册值与函数处理
        StoryValueManager.Instance.RegisterValueHandler("robotname", new StoryValueFactoryHelper<RobotNameValue>());
        StoryValueManager.Instance.RegisterValueHandler("datetime", new StoryValueFactoryHelper<DateTimeValue>());
        StoryValueManager.Instance.RegisterValueHandler("randskill", new StoryValueFactoryHelper<RandSkillValue>());
        StoryValueManager.Instance.RegisterValueHandler("islobbyconnected", new StoryValueFactoryHelper<IsLobbyConnectedValue>());
        StoryValueManager.Instance.RegisterValueHandler("islobbylogining", new StoryValueFactoryHelper<IsLobbyLoginingValue>());
        StoryValueManager.Instance.RegisterValueHandler("haslobbyloggedon", new StoryValueFactoryHelper<HasLobbyLoggedOnValue>());
        StoryValueManager.Instance.RegisterValueHandler("isroomstarted", new StoryValueFactoryHelper<IsRoomStartedValue>());
        StoryValueManager.Instance.RegisterValueHandler("isroomconnected", new StoryValueFactoryHelper<IsRoomConnectedValue>());

      }
    }

    private static bool s_Inited = false;
  }
}
