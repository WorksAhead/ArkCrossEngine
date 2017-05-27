using System;
using System.Collections.Generic;
using SkillSystem;
using DashFire;

namespace SkillSystem
{
  /// <summary>
  /// 技能分析不具体模拟技能dsl的逻辑，仅对dsl进行分析获取校验所需数据。
  /// </summary>
  internal sealed class SkillAnalysis
  {
    internal void Init()     
    {
      StaticInit();
    }
    internal void Reset()
    {
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

    internal SkillInstance Analyze(int skillId)
    {
      SkillInstanceInfo inst = NewSkillInstance(skillId);
      if (null != inst) {
        inst.m_SkillInstance.Analyze(null);
        List<int> impacts = new List<int>();
        List<string> resources = new List<string>();
        foreach (int skill in inst.m_SkillInstance.SummonNpcSkills) {
          AnalyzeNpcSkills(skill, ref impacts, ref resources);
        }
        inst.m_SkillInstance.EnableImpactsToOther.AddRange(impacts);
        inst.m_SkillInstance.Resources.AddRange(resources);
        return inst.m_SkillInstance;
      } else {
        return null;
      }
    }

    private void AnalyzeNpcSkills(int skillId, ref List<int> impacts, ref List<string> resources)
    {
      SkillInstanceInfo instance = NewSkillInstance(skillId);
      if (null != instance) {
        instance.m_SkillInstance.Analyze(null);
        impacts.AddRange(instance.m_SkillInstance.EnableImpactsToOther);
        resources.AddRange(instance.m_SkillInstance.Resources);
        foreach (int npcSkillId in instance.m_SkillInstance.SummonNpcSkills) {
          AnalyzeNpcSkills(npcSkillId, ref impacts, ref resources);
        }
        RecycleSkillInstance(instance);
      }
    }

    private SkillInstanceInfo NewSkillInstance(int skillId)
    {
      SkillInstanceInfo instInfo = GetUnusedSkillInstanceInfoFromPool(skillId);
      if (null == instInfo) {
        DashFire.SkillLogicData skillData = DashFire.SkillConfigProvider.Instance.ExtractData(DashFire.SkillConfigType.SCT_SKILL, skillId) as DashFire.SkillLogicData;
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
            DashFire.LogSystem.Error("Can't find skill dsl or skill dsl error, skill:{0} !", skillId);
            return null;
          }
        } else {
          DashFire.LogSystem.Error("Can't find skill config, skill:{0} !", skillId);
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
      if (m_SkillInstancePool.ContainsKey(skillId)) {
        List<SkillInstanceInfo> infos = m_SkillInstancePool[skillId];
        infos.Add(info);
      } else {
        List<SkillInstanceInfo> infos = new List<SkillInstanceInfo>();
        infos.Add(info);
        m_SkillInstancePool.Add(skillId, infos);
      }
    }
    private SkillInstanceInfo GetUnusedSkillInstanceInfoFromPool(int skillId)
    {
      SkillInstanceInfo info = null;
      if (m_SkillInstancePool.ContainsKey(skillId)) {
        List<SkillInstanceInfo> infos = m_SkillInstancePool[skillId];
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

    private class SkillInstanceInfo
    {
      internal int m_SkillId;
      internal SkillInstance m_SkillInstance;
      internal bool m_IsUsed;
    }
    private Dictionary<int, List<SkillInstanceInfo>> m_SkillInstancePool = new Dictionary<int, List<SkillInstanceInfo>>();

    private static void StaticInit()
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
      }
    }

    private static bool s_IsInited = false;
  }
}
