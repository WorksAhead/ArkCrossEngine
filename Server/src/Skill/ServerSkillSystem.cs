using System;
using System.Collections.Generic;
using SkillSystem;
using ArkCrossEngine;

namespace DashFire
{
  /// <summary>
  /// Server技能系统不具体模拟技能dsl的逻辑，仅对dsl进行分析获取校验所需数据，然后在技能最大时间结束技能（正常情形客户端会在此时间前发消息结束技能）。
  /// </summary>
  internal sealed class ServerSkillSystem
  {
    private class SkillInstanceInfo
    {
      internal int m_SkillId;
      internal SkillInstance m_SkillInstance;
      internal bool m_IsUsed;
    }
    private class SkillLogicInfo
    {
      internal int SenderId
      {
        get
        {
          return m_SenderId;
        }
      }
      internal int SkillId
      {
        get
        {
          return m_SkillInfo.m_SkillId;
        }
      }
      internal SkillInstance SkillInst
      {
        get
        {
          return m_SkillInfo.m_SkillInstance;
        }
      }
      internal SkillInstanceInfo Info
      {
        get
        {
          return m_SkillInfo;
        }
      }

      internal SkillLogicInfo(int objId, SkillInstanceInfo info)
      {
        m_SenderId = objId;
        m_SkillInfo = info;
      }

      private int m_SenderId;
      private SkillInstanceInfo m_SkillInfo;
    }

    internal Scene CurScene
    {
      get { return m_CurScene; }
    }
    internal void Init(Scene scene)
    {
      StaticInit();
      m_CurScene = scene;
    }
    internal void Reset()
    {
      m_LastTickTime = 0;
      int count = m_SkillLogicInfos.Count;
      for (int index = count - 1; index >= 0; --index) {
        SkillLogicInfo info = m_SkillLogicInfos[index];
        RecycleSkillInstance(info.Info);
        m_SkillLogicInfos.RemoveAt(index);
      }
      m_SkillLogicInfos.Clear();
    }
    internal void PreloadSkillInstance(int skillId)
    {
      SkillInstanceInfo info = NewSkillInstance(skillId);
      if (null != info) {
        RecycleSkillInstance(info);
      }
    }
    internal void ClearSkillInstancePool()
    {
      m_SkillInstancePool.Clear();
    }

    internal void StartSkill(int objId, int skillId)
    {
      CharacterInfo obj = CurScene.SceneContext.GetCharacterInfoById(objId);
      if (null != obj) {
        SkillInstanceInfo inst = NewSkillInstance(skillId);
        if (null != inst) {
          obj.GetSkillStateInfo().SetCurSkillInfo(skillId);
          SkillInfo skillInfo = obj.GetSkillStateInfo().GetCurSkillInfo();
          if (null != skillInfo) {
            if (MakeSkillCast(obj, skillInfo)) {
              ArkCrossEngineMessage.Msg_RC_SyncProperty propBuilder = DataSyncUtility.BuildSyncPropertyMessage(obj);
              Scene scene = obj.SceneContext.CustomData as Scene;
              if (null != scene) {
                scene.NotifyAllUser(propBuilder);
              }
            } else {
              skillInfo.m_EnableImpactsToMyself = null;
              skillInfo.m_EnableImpactsToOther = null;
              skillInfo.m_LeftEnableMoveCount = 0;
              skillInfo.m_LeftEnableImpactsToMyself.Clear();
              skillInfo.m_LeftEnableImpactsToOther.Clear();
              return;
            }
            skillInfo.IsSkillActivated = true;

            m_SkillLogicInfos.Add(new SkillLogicInfo(objId, inst));
            SkillLogicInfo logicInfo = m_SkillLogicInfos.Find(info => info.SenderId == objId && info.SkillId == skillId);
            if (null != logicInfo) {
              //目前没有与技能释放者相关的分析属性，对每个技能暂时只需要分析一次
              if (null == skillInfo.m_EnableImpactsToMyself || null == skillInfo.m_EnableImpactsToOther) {
                if (!logicInfo.SkillInst.AlreadyAnalyzed) {
                  logicInfo.SkillInst.Analyze(obj);
                  foreach (int skill in logicInfo.SkillInst.SummonNpcSkills) {
                    List<int> impacts = AnalyzeNpcSkills(skill, logicInfo.SkillInst);
                    logicInfo.SkillInst.EnableImpactsToOther.AddRange(impacts);
                  }
                }
                skillInfo.m_EnableMoveCount = logicInfo.SkillInst.EnableMoveCount;
                skillInfo.m_MaxMoveDistance = logicInfo.SkillInst.MaxMoveDelta;
                skillInfo.m_EnableImpactsToOther = logicInfo.SkillInst.EnableImpactsToOther;
                skillInfo.m_EnableImpactsToMyself = logicInfo.SkillInst.EnableImpactsToMyself;
                skillInfo.m_EnableSummonNpcs = logicInfo.SkillInst.SummonNpcs;
                /*
                LogSys.Log(LOG_TYPE.WARN, "Skill {0} EnableMoveCount {1} MaxMoveDistanceSqr {2}\n\tEnableImpactsToOther {3}\n\tEnableImpactsToMyself {4}\n\tSummonNpcSkills {5}", skillId, skillInfo.m_EnableMoveCount, skillInfo.m_MaxMoveDistanceSqr,
                  string.Join<int>(",", skillInfo.m_EnableImpactsToOther),
                  string.Join<int>(",", skillInfo.m_EnableImpactsToMyself),
                  string.Join<int>(",", logicInfo.SkillInst.SummonNpcSkills));
                */
              }
              skillInfo.m_LeftEnableMoveCount = skillInfo.m_EnableMoveCount;
              skillInfo.m_LeftEnableImpactsToMyself.AddRange(skillInfo.m_EnableImpactsToMyself);
              skillInfo.m_LeftEnableImpactsToOther.Clear();
              if (logicInfo.SkillInst.IsSimulate) {
                obj.GetSkillStateInfo().SimulateEndTime = TimeUtility.GetServerMilliseconds() + logicInfo.SkillInst.MaxSkillLifeTime;
              }

              logicInfo.SkillInst.Start(obj);
              /*
              DashFire.LogSystem.Warn("StartSkill {0} {1} EnableMoveCount {2} MaxMoveDistance {3}\n\tEnableImpactsToOther {4}\n\tEnableImpactsToMyself {5}\n\tSummonNpcSkills {6}", objId, skillId, skillInfo.m_LeftEnableMoveCount, skillInfo.m_MaxMoveDistanceSqr,
                string.Join<int>(",", skillInfo.m_EnableImpactsToOther),
                string.Join<int>(",", skillInfo.m_EnableImpactsToMyself),
                string.Join<int>(",", logicInfo.SkillInst.SummonNpcSkills));
              */
            }
          } else {
            LogSystem.Error("{0} StartSkill can't find skill {1}", objId, skillId);
          }
        }
      } else {
        LogSystem.Debug("not find game obj by id " + objId);
      }
    }
    internal void StopSkill(int objId)
    {
      CharacterInfo obj = CurScene.SceneContext.GetCharacterInfoById(objId);
      if (null == obj) {
        return;
      }
      int count = m_SkillLogicInfos.Count;
      for (int index = count - 1; index >= 0; --index) {
        SkillLogicInfo info = m_SkillLogicInfos[index];
        if (info.SenderId == objId) {
          //DashFire.LogSystem.Warn("Skill {0} finished (stop skill).", info.SkillId);

          if (info.SkillInst.IsControlMove) {
            obj.GetMovementStateInfo().IsSkillMoving = false;
            info.SkillInst.IsControlMove = false;
          }
          SkillInfo skillInfo = obj.GetSkillStateInfo().GetSkillInfoById(info.SkillId);
          if (null != skillInfo) {
            skillInfo.IsSkillActivated = false;
          }
          RecycleSkillInstance(info.Info);
          m_SkillLogicInfos.RemoveAt(index);
          info.SkillInst.IsInterrupted = true;
        }
      }
    }
    internal void SendMessage(int objId, int skillId, string msgId)
    {
      CharacterInfo obj = CurScene.SceneContext.GetCharacterInfoById(objId);
      if (null != obj) {
        SkillLogicInfo logicInfo = m_SkillLogicInfos.Find(info => info.SenderId == objId && info.SkillId == skillId);
        if (null != logicInfo && null != logicInfo.SkillInst) {
          logicInfo.SkillInst.SendMessage(msgId);
        }
      }
    }
    internal void Tick()
    {
      long time = TimeUtility.GetServerMilliseconds();
      long delta = (time - m_LastTickTime) * 1000;
      m_LastTickTime = time;
      int ct = m_SkillLogicInfos.Count;
      for (int ix = ct - 1; ix >= 0; --ix) {
        SkillLogicInfo info = m_SkillLogicInfos[ix];
        CharacterInfo obj = CurScene.SceneContext.GetCharacterInfoById(info.SenderId);
        if (null!=obj) {
          info.SkillInst.Tick(obj, delta);

          //DashFire.LogSystem.Debug("Skill {0} tick {1}.", info.SkillId, time);
        }
        if (null==obj || info.SkillInst.IsFinished) {
          //DashFire.LogSystem.Warn("Skill {0} finished (time finish).", info.SkillId);

          if (info.SkillInst.IsControlMove) {
            if (null != obj)
              obj.GetMovementStateInfo().IsSkillMoving = false;
            info.SkillInst.IsControlMove = false;
          }
          if (null != obj) {
            SkillInfo skillInfo = obj.GetSkillStateInfo().GetSkillInfoById(info.SkillId);
            if (null != skillInfo) {
              skillInfo.IsSkillActivated = false;
            }
          }
          RecycleSkillInstance(info.Info);
          m_SkillLogicInfos.RemoveAt(ix);
        }
      }
    }

    private bool MakeSkillCast(CharacterInfo obj, SkillInfo skillInfo)
    {
      if (obj.Energy < skillInfo.ConfigData.CostEnergy) {
        return false;
      }
      if (obj.Rage < skillInfo.ConfigData.CostRage) {
        return false;
      }
      obj.SetEnergy(Operate_Type.OT_Relative, -skillInfo.ConfigData.CostEnergy);
      obj.SetRage(Operate_Type.OT_Relative, -skillInfo.ConfigData.CostRage);
      return true;
    }

    private List<int> AnalyzeNpcSkills(int skillId, SkillInstance owner)
    {
      List<int> impacts = new List<int>();
      SkillInstanceInfo instance = NewSkillInstance(skillId);
      if (null != instance) {
        instance.m_SkillInstance.Analyze(null);
        impacts.AddRange(instance.m_SkillInstance.EnableImpactsToOther);
        if (instance.m_SkillInstance.IsSimulate) {
          owner.IsSimulate = true;
          owner.MaxSkillLifeTime = instance.m_SkillInstance.MaxSkillLifeTime;
        }
        foreach (int npcSkillId in instance.m_SkillInstance.SummonNpcSkills) {
          List<int> npcImpacts = AnalyzeNpcSkills(npcSkillId, owner);
          impacts.AddRange(npcImpacts);
        }
        RecycleSkillInstance(instance);
      }
      return impacts;
    }

    private SkillInstanceInfo NewSkillInstance(int skillId)
    {
      SkillInstanceInfo instInfo = GetUnusedSkillInstanceInfoFromPool(skillId);
      if (null == instInfo) {
        SkillLogicData skillData = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skillId) as SkillLogicData;
        if (null != skillData) {
          string filePath = HomePath.GetAbsolutePath(FilePathDefine_Server.C_SkillDslPath + skillData.SkillDataFile);
          SkillConfigManager.Instance.LoadSkillIfNotExist(skillId, filePath);
          SkillInstance inst = SkillConfigManager.Instance.NewSkillInstance(skillId);

          if (null != inst) {
            SkillInstanceInfo res = new SkillInstanceInfo();
            res.m_SkillId = skillId;
            res.m_SkillInstance = inst;
            res.m_IsUsed = true;

            AddSkillInstanceInfoToPool(skillId, res);
            return res;
          } else {
            LogSystem.Error("Can't find skill dsl or skill dsl error, skill:{0} !", skillId);
            return null;
          }
        } else {
          LogSystem.Error("Can't find skill config, skill:{0} !", skillId);
          return null;
        }
      } else {
        instInfo.m_IsUsed = true;
        return instInfo;
      }
    }
    private void RecycleSkillInstance(SkillInstanceInfo info)
    {
      info.m_SkillInstance.Reset();
      info.m_IsUsed = false;
    }
    private void AddSkillInstanceInfoToPool(int skillId, SkillInstanceInfo info)
    {
      List<SkillInstanceInfo> infos;
      if (m_SkillInstancePool.TryGetValue(skillId, out infos)) {
        infos.Add(info);
      } else {
        infos = new List<SkillInstanceInfo>();
        infos.Add(info);
        m_SkillInstancePool.Add(skillId, infos);
      }
    }
    private SkillInstanceInfo GetUnusedSkillInstanceInfoFromPool(int skillId)
    {
      SkillInstanceInfo info = null;
      List<SkillInstanceInfo> infos;
      if (m_SkillInstancePool.TryGetValue(skillId, out infos)) {
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

    private List<SkillLogicInfo> m_SkillLogicInfos = new List<SkillLogicInfo>();
    private Dictionary<int, List<SkillInstanceInfo>> m_SkillInstancePool = new Dictionary<int, List<SkillInstanceInfo>>();
    private Scene m_CurScene = null;
    private long m_LastTickTime = 0;

    internal static void StaticInit()
    {
      if (!s_IsInited) {
        s_IsInited = true;

        //注册技能触发器
        SkillTrigerManager.Instance.RegisterTrigerFactory("movecontrol", new SkillTrigerFactoryHelper<Trigers.MoveControlTriger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("startcurvemove", new SkillTrigerFactoryHelper<Trigers.CurveMovementTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("areadamage", new SkillTrigerFactoryHelper<Trigers.AreaDamageTriger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("colliderdamage", new SkillTrigerFactoryHelper<Trigers.ColliderDamageTriger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("summonnpc", new SkillTrigerFactoryHelper<Trigers.SummonObjectTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("findmovetarget", new SkillTrigerFactoryHelper<Trigers.ChooseTargetTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("addimpacttoself", new SkillTrigerFactoryHelper<Trigers.AddImpactToSelfTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("addimpacttotarget", new SkillTrigerFactoryHelper<Trigers.AddImpactToTargetTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("exchangeposition", new SkillTrigerFactoryHelper<Trigers.ExchangePositionTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("fruitninjia", new SkillTrigerFactoryHelper<Trigers.FruitNinjiaTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("oncross", new SkillTrigerFactoryHelper<Trigers.OnCrossTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("move2targetpos", new SkillTrigerFactoryHelper<Trigers.Move2TargetPosTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("crosssummonmove", new SkillTrigerFactoryHelper<Trigers.CrossSummonMoveTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("restorepos", new SkillTrigerFactoryHelper<Trigers.RestorePosTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("setlifetime", new SkillTrigerFactoryHelper<Trigers.SetlifeTimeTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("simulatemove", new SkillTrigerFactoryHelper<Trigers.SimulateMoveTrigger>());
        SkillTrigerManager.Instance.RegisterTrigerFactory("settransform", new SkillTrigerFactoryHelper<Trigers.SetTransformTrigger>());
      }
    }

    private static bool s_IsInited = false;
  }
}
