using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArkCrossEngineMessage;
using Google.ProtocolBuffers;
using ArkCrossEngineSpatial;
using ArkCrossEngine;

namespace DashFire
{
  internal enum SceneState : int
  {
    Sleeping = 0,
    Preloading,
    Running,
  }
  internal sealed partial class Scene
  {
    internal sealed class GameTimeUtil
    {
      internal bool IsGameStart { get { return game_start_ms_ != 0; } }
      internal long StartTime { get { return game_start_ms_; } }
      internal long ElapseMilliseconds { get { return TimeUtility.GetServerMilliseconds() - game_start_ms_; }  }      

      internal void Start()
      {
        game_start_ms_ = TimeUtility.GetServerMilliseconds();
      }

      internal void Reset()
      {
        game_start_ms_ = 0;
      }

      private long game_start_ms_ = 0;
    }

    internal Scene()
    {
      m_SceneContext.OnHighlightPrompt += this.OnHightlightPrompt;

      m_SceneContext.SpatialSystem = m_SpatialSystem;
      m_SceneContext.SightManager = m_SightManager;
      m_SceneContext.SceneLogicInfoManager = m_SceneLogicInfoMgr;
      m_SceneContext.NpcManager = m_NpcMgr;
      m_SceneContext.BlackBoard = m_BlackBoard;
      m_SceneContext.CustomData = this;

      m_NpcMgr.SetSceneContext(m_SceneContext);
      m_SceneLogicInfoMgr.SetSceneContext(m_SceneContext);

      m_MovementSystem.SetNpcManager(m_NpcMgr);
      m_AiSystem.SetNpcManager(m_NpcMgr);
      m_SceneLogicSystem.SetSceneLogicInfoManager(m_SceneLogicInfoMgr);

      m_ControlSystemOperation.Init(this);
      m_SkillSystem.Init(this);
      m_StorySystem.Init(this);

      m_GmStorySystem.Init(this);
    }

    internal void Reset()
    {
      m_NpcMgr.Reset();
      m_SceneLogicInfoMgr.Reset();
      m_SceneLogicSystem.Reset();
      m_MovementSystem.Reset();
      m_AiSystem.Reset();
      m_SpatialSystem.Reset();
      m_GameTime.Reset();
      if (null != m_SightManager) {
        m_SightManager.Reset();
      }
      m_ServerDelayActionProcessor.Reset();
      m_BlackBoard.Reset();

      m_ControlSystemOperation.Reset();
      m_SkillSystem.Reset();
      m_StorySystem.Reset();

      m_LastPreloadingTickTime = 0;
      m_LastTickTimeForTickPerSecond = 0;

      m_SceneState = SceneState.Sleeping;
    }

    internal void SetRoom(Room room)
    {
      m_Room = room;
      m_MovementSystem.SetUserManager(room.UserManager);
      m_AiSystem.SetUserManager(room.UserManager);
      m_SceneContext.UserManager = room.UserManager;
    }

    internal Room GetRoom()
    {
      return m_Room;
    }

    internal long StartTime
    {
      get { return m_GameTime.StartTime; }
    }

    internal void EnterScene(UserInfo info)
    {
      info.SceneContext = m_SceneContext;
      if (null != m_SpatialSystem) {
        m_SpatialSystem.AddObj(info.SpaceObject);
      }
      if (m_SightManager != null) {
        SightManager.AddObject(info);
      }
      /*if (!IsPvpScene) {
        AddCareList(info);
      }*/
      //移动版本全场景可见
      AddCareList(info);
    }

    internal void LeaveScene(UserInfo info)
    {
      if (null != m_SpatialSystem) {
        m_SpatialSystem.RemoveObj(info.SpaceObject);
      }
      if (null != m_SightManager) {
        m_SightManager.RemoveObject(info);
      }
      /*if (!IsPvpScene) {
        RemoveCareList(info);
      }*/
      //移动版本全场景可见
      RemoveCareList(info);
      info.SceneContext = null;
    }

    internal void AddCareList(UserInfo info)
    {
      User enter_user = info.CustomData as User;
      if (enter_user == null) { return; }
      /*
      if (null != m_SightManager) {
        IList<UserInfo> users = m_SightManager.GetCampUsers(info.GetCampId());
        foreach (UserInfo user_info in users) {
          if (user_info.GetId() == info.GetId()) { continue; }
          User user = user_info.CustomData as User;
          if (user == null) { continue; }
          user.AddCareMeUser(enter_user);
          enter_user.AddCareMeUser(user);
        }
      }
      */
      //移动版本不计算视野
      foreach (User user in enter_user.OwnRoom.RoomUsers) {
        user.AddCareMeUser(enter_user);
        enter_user.AddCareMeUser(user);
      }
    }

    internal void RemoveCareList(UserInfo info)
    {
      User leave_user = info.CustomData as User;
      if (leave_user == null) { return; }
      /*
      if (null != m_SightManager) {
        IList<UserInfo> users = SightManager.GetCampUsers(info.GetCampId());
        foreach (UserInfo user_info in users) {
          if (user_info.GetId() == info.GetId()) { continue; }
          User user = user_info.CustomData as User;
          if (user == null) { continue; }
          user.RemoveCareMeUser(leave_user);
          leave_user.RemoveCareMeUser(user);
        }
      }
      */
      //移动版本不计算视野
      foreach (User user in leave_user.OwnRoom.RoomUsers) {
        user.RemoveCareMeUser(leave_user);
        leave_user.RemoveCareMeUser(user);
      }
    }

    internal void LoadData(int resId)
    {
      m_SceneResId = resId;
      m_SceneContext.SceneResId = resId;
      m_SceneContext.IsRunWithRoomServer = true;

      LogSys.Log(LOG_TYPE.DEBUG, "Scene {0} start Preloading.", resId);
      
      // 加载地图阻挡信息
      m_SceneConfig = SceneConfigProvider.Instance.GetSceneConfigById(m_SceneResId);
      // 加载掉落信息
      m_SceneDropOut = SceneConfigProvider.Instance.GetSceneDropOutById(m_SceneConfig.m_DropId);
      // 加载场景配置数据
      m_MapData = SceneConfigProvider.Instance.GetMapDataBySceneResId(m_SceneResId);
      if (null != m_MapData) {
        m_SpatialSystem.Init(FilePathDefine_Server.C_RootPath + m_SceneConfig.m_BlockInfoFile, m_SceneConfig.m_ReachableSet);
        m_SpatialSystem.LoadPatch(FilePathDefine_Server.C_RootPath + m_SceneConfig.m_BlockInfoFile + ".patch");
        m_SpatialSystem.LoadObstacle(FilePathDefine_Server.C_RootPath + m_SceneConfig.m_ObstacleFile, 1 / m_SceneConfig.m_TiledDataScale);

        m_IsPvpScene = (m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_PVP);

        m_IsAttemptScene = (m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_MULTI_PVE
          && m_SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_ATTEMPT);

        m_IsGoldScene = (m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_MULTI_PVE
          && m_SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_GOLD);

        if (null != m_SightManager) {
          m_SightManager.Init(FilePathDefine_Server.C_RootPath + m_SceneConfig.m_BlockInfoFile, m_NpcMgr, m_SpatialSystem);
        }
      }

      m_StorySystem.ClearStoryInstancePool();
      m_StorySystem.PreloadStoryInstance(1);

      m_SceneState = SceneState.Preloading;
    }

    internal void Tick()
    {
      switch (m_SceneState) {
        case SceneState.Preloading:
          TickPreloading();
          break;
        case SceneState.Running:
          TickRunning();
          break;
      }
    }

    internal void NotifyAllObserver(object msg)
    {
      foreach (Observer observer in m_Room.RoomObservers) {
        if (null != observer && !observer.IsIdle) {
          observer.SendMessage(msg);
        }
      }
    }

    internal void NotifyAllUser(object msg)
    {
      foreach (User us in m_Room.RoomUsers) {
        us.SendMessage(msg);
      }
      NotifyAllObserver(msg);
    }

    internal void NotifyAllUser(object msg, int exceptId)
    {
      foreach (User us in m_Room.RoomUsers) {
        if (us.RoleId!=exceptId) {
          us.SendMessage(msg);
        }
      }
      NotifyAllObserver(msg);
    }

    internal void NotifyAreaUser(CharacterInfo ch, object msg, bool exceptself = true)
    {
      //移动版本不考虑视野
      if (exceptself) {
        NotifyAllUser(msg, ch.GetId());
      } else {
        NotifyAllUser(msg);
      }
      /*
      if(IsPvpScene()) {
        NotifySightUsers(ch, msg, exceptself);
        NotifyAllObserver(msg);
      } else {
        if (exceptself) {
          NotifyAllUser(msg, ch.GetId());
        } else {
          NotifyAllUser(msg);
        }
      }
      */
    }
    
    internal void SyncForNewUser(User user)
    {
      if (null != user) {
        UserInfo userInfo = user.Info;
        Room room = GetRoom();
        if (null != userInfo && null != room && null != room.GetActiveScene()) {
          //同步玩家数据给自己
          SyncUserToSelf(user);
          //同步玩家数据给其他玩家
          SyncUserToOthers(user);
          //同步其他玩家数据给自己
          SyncOthersToUser(user);
          //同步场景数据给自己
          SyncSceneObjectsToUser(user);
          //同步玩家数据给观察者
          SyncUserToObservers(user);
          //剧情脚本处理
          m_StorySystem.SendMessage("userenterscene", userInfo.GetId());
        }
      }
    }

    internal void SyncForNewObserver(Observer observer)
    {
      if (null != observer) {
        Room room = GetRoom();
        if (null != room && null != room.GetActiveScene()) {
          //同步其他玩家数据与物品给自己
          foreach (User other in room.RoomUsers) {
            if (!other.IsEntered) {
              continue;
            }
            UserInfo otherInfo = other.Info;
            if (null != otherInfo) {
              Vector3 pos = otherInfo.GetMovementStateInfo().GetPosition3D();
              ArkCrossEngineMessage.Position pos_bd = new ArkCrossEngineMessage.Position();
              pos_bd.x = (float)pos.X;
              pos_bd.z = (float)pos.Z;
              Msg_CRC_Create bd = new Msg_CRC_Create();
              bd.role_id = other.RoleId;
              bd.hero_id = other.HeroId;
              bd.camp_id = other.CampId;
              bd.role_level = other.Level;
              bd.is_player_self = false;
              bd.position = pos_bd;
              bd.face_dirction = (float)otherInfo.GetMovementStateInfo().GetFaceDir();
              for (int index = 0; index < otherInfo.GetSkillStateInfo().GetAllSkill().Count; index++) {
                bd.skill_levels.Add(otherInfo.GetSkillStateInfo().GetSkillInfoByIndex(index).SkillLevel);
              }
              bd.scene_start_time = StartTime;
              bd.nickname = other.Name;
              observer.SendMessage(bd);

              DataSyncUtility.SyncBuffListToObserver(otherInfo, observer);

              Msg_RC_SyncProperty propBuilder = DataSyncUtility.BuildSyncPropertyMessage(otherInfo);
              observer.SendMessage(propBuilder);

              Msg_RC_SyncCombatStatisticInfo combatBuilder = DataSyncUtility.BuildSyncCombatStatisticInfo(otherInfo);
              observer.SendMessage(combatBuilder);

              LogSys.Log(LOG_TYPE.DEBUG, "send user {0} msg to observer {1}", other.RoleId, observer.Guid);
            }
          }
          //同步场景数据给观察者
          for (LinkedListNode<NpcInfo> linkNode = NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next) {
            NpcInfo npc = linkNode.Value;
            if (null != npc) {
              Msg_RC_CreateNpc bder = DataSyncUtility.BuildCreateNpcMessage(npc);
              observer.SendMessage(bder);
            }
          }

          int totalKillCountForBlue = 0;
          int totalKillCountForRed = 0;
          for (LinkedListNode<UserInfo> linkNode = UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
            UserInfo user_info = linkNode.Value;
            if (user_info.GetCampId() == (int)CampIdEnum.Blue) {
              totalKillCountForBlue += user_info.GetCombatStatisticInfo().KillHeroCount;
            } else {
              totalKillCountForRed += user_info.GetCombatStatisticInfo().KillHeroCount;
            }
          }

          Msg_RC_PvpCombatInfo combat_bd = new Msg_RC_PvpCombatInfo();
          combat_bd.kill_hero_count_for_blue = totalKillCountForBlue;
          combat_bd.kill_hero_count_for_red = totalKillCountForRed;
          combat_bd.link_id_for_killer = -1;
          combat_bd.link_id_for_killed = -1;
          combat_bd.killed_nickname = "";
          combat_bd.killer_nickname = "";
          observer.SendMessage(combat_bd);
        }
      }
    }

    private void CalculateDropOut() {
      if (null != m_SceneDropOut) {
        m_DropMoneyData.Clear();
        List<int> npcList = new List<int>();
        foreach (Data_Unit npcUnit in m_MapData.m_UnitMgr.GetData().Values) {
          if (npcUnit.GetId() < 10000) {
            npcList.Add(npcUnit.GetId());
          }
        }
        int npcCount = npcList.Count;
        if (npcCount <= 0) return;
        if (m_SceneDropOut.m_GoldSum == 0 || m_SceneDropOut.m_GoldMin == 0) return;
        List<int> addIndex = new List<int>();
        int dropCount = m_SceneDropOut.m_GoldSum / m_SceneDropOut.m_GoldMin;
        int curMoney = m_SceneDropOut.m_GoldSum;
        dropCount = npcCount > dropCount ? dropCount : npcCount;
        while (dropCount > 0) {
          if (addIndex.Count == dropCount) break;
          int index = CrossEngineHelper.Random.Next(0, npcList.Count);
          if (npcList[index] == 20001 || npcList[index] == 20002 || npcList[index] == 20003) continue;
          if (addIndex.IndexOf(index) == -1) {
            int dropMoney = CrossEngineHelper.Random.Next(m_SceneDropOut.m_GoldMin, m_SceneDropOut.m_GoldMax);
            if (dropMoney > curMoney) dropMoney = curMoney;
            curMoney -= dropMoney;
            m_DropMoneyData.Add(npcList[index], dropMoney);
            addIndex.Add(index);
            if (curMoney <= 0) return;
          }
        }
      }
    }
    internal bool IsAllPlayerEntered()
    {
      foreach (User us in m_Room.RoomUsers) {
        if (!us.IsEntered && !us.IsTimeout()) {
          return false;
        }
      }
      return true;
    }

    internal MovementSystem MovementSystem
    {
      get { return m_MovementSystem; }
    }
    internal AiSystem AiSystem
    {
      get { return m_AiSystem; }
    }
    internal SceneLogicSystem SceneLogicSystem
    {
      get { return m_SceneLogicSystem; }
    }
    internal SceneContextInfo SceneContext
    {
      get { return m_SceneContext; }
    }

    internal SpatialSystem SpatialSystem
    {
      get { return m_SpatialSystem; }
    }

    internal SightManager SightManager
    {
      get { return m_SightManager; }
    }
    internal SceneLogicInfoManager SceneLogicInfoMgr
    {
      get { return m_SceneLogicInfoMgr; }
    }
    internal NpcManager NpcManager
    {
      get { return m_NpcMgr; }
    }
    internal UserManager UserManager
    {
      get
      {
        UserManager mgr = null;
        if (null != m_SceneContext) {
          mgr = m_SceneContext.UserManager;
        }
        return mgr;
      }
    }
    internal BlackBoard BlackBoard
    {
      get
      {
        return m_BlackBoard;
      }
    }
    
    internal int SceneResId
    {
      get { return m_SceneResId; }
    }
    internal Data_SceneConfig SceneConfig
    {
      get { return m_SceneConfig; }
    }
    internal MapDataProvider MapData
    {
      get { return m_MapData; }
    }
    internal ServerDelayActionProcessor DelayActionProcessor
    {
      get { return m_ServerDelayActionProcessor; }
    }
    internal GameTimeUtil GameTime
    { 
      get
      {
        return m_GameTime;
      }
    }
    internal bool IsPvpScene
    {
      get { return m_IsPvpScene; }
    }
    internal bool IsAttemptScene
    {
      get { return m_IsAttemptScene; }
    }
    internal bool IsGoldScene
    {
      get { return m_IsGoldScene; }
    }
    internal int AOiZoneDepth
    {
      get { return m_AOiZoneDepth; }
      set { m_AOiZoneDepth = value; }
    }
    internal SceneState SceneState
    {
      get { return m_SceneState; }
    }
    internal SceneProfiler SceneProfiler
    {
      get { return m_SceneProfiler; }
    }
    internal ControlSystemOperation ControlSystemOperation
    {
      get { return m_ControlSystemOperation; }
    }
    internal ServerSkillSystem SkillSystem
    {
      get { return m_SkillSystem; }
    }
    internal ServerStorySystem StorySystem
    {
      get { return m_StorySystem; }
    }
    internal GmCommands.GmStorySystem GmStorySystem
    {
      get { return m_GmStorySystem; }
    }

    private bool m_IsPvpScene = false;
    private bool m_IsAttemptScene = false;
    private bool m_IsGoldScene = false;
    private int m_AOiZoneDepth = 2;

    private const long c_PreloadingTickInterval = 1000;
    private long m_LastPreloadingTickTime = 0;

    private const long c_IntervalPerSecond = 5000;
    private long m_LastTickTimeForTickPerSecond = 0;
    private int m_SceneResId = 0;
    private Data_SceneConfig m_SceneConfig = null;
    private Data_SceneDropOut m_SceneDropOut = null;
    private Dictionary<int, int> m_DropMoneyData = new Dictionary<int,int>();
    private MapDataProvider m_MapData = null;
    private ServerDelayActionProcessor m_ServerDelayActionProcessor = new ServerDelayActionProcessor();

    private NpcManager m_NpcMgr = new NpcManager(1024);
    private MovementSystem m_MovementSystem = new MovementSystem();
    private AiSystem m_AiSystem = new AiSystem();
    private SpatialSystem m_SpatialSystem = new SpatialSystem();
    private SightManager m_SightManager = null;//new SightManager();//移动版本不计算视野
    private SceneLogicInfoManager m_SceneLogicInfoMgr = new SceneLogicInfoManager(1024);
    private SceneLogicSystem m_SceneLogicSystem = new SceneLogicSystem();
    private BlackBoard m_BlackBoard = new BlackBoard();
    private SceneContextInfo m_SceneContext = new SceneContextInfo();

    private ControlSystemOperation m_ControlSystemOperation = new ControlSystemOperation();
    private ServerSkillSystem m_SkillSystem = new ServerSkillSystem();
    private ServerStorySystem m_StorySystem = new ServerStorySystem();

    private GmCommands.GmStorySystem m_GmStorySystem = new GmCommands.GmStorySystem();

    private Room m_Room = null;
    private GameTimeUtil m_GameTime = new GameTimeUtil();

    private SceneState m_SceneState = SceneState.Sleeping;
    private SceneProfiler m_SceneProfiler = new SceneProfiler();
  }
}
