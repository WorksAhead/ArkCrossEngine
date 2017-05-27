using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class AiActionInfo
    {
        public AiActionInfo(AiActionConfig config)
        {
            m_Config = config;
            m_LastStartTime = 0;
            m_StartTime = 0;
        }
        public bool IsSatisfySelfHp(float selfHp, float maxHp)
        {
            // self hp
            return IsInRange(selfHp, m_Config.SelfHpMin * maxHp, m_Config.SelfHpMax * maxHp);
        }
        public bool IsSatisfyTargetHp(float targetHp, float maxHp)
        {
            return IsInRange(targetHp, m_Config.TargetHpMin * maxHp, m_Config.TargetHpMax * maxHp);
        }
        public bool IsSatifyDis(float dis)
        {
            return IsInRange(dis, m_Config.DisMin, m_Config.DisMax);
        }
        public bool IsSatifyCD(long curTime)
        {
            if (m_LastStartTime == 0 || curTime - m_LastStartTime > m_Config.Cooldown * 1000)
            {
                return true;
            }
            return false;
        }
        public bool IsLuckyEnough()
        {
            return m_Config.Weight >= CrossEngineHelper.Random.NextFloat();
        }
        public virtual bool IsSatifyUser(CharacterInfo entity)
        {
            return true;
        }
        public bool IsTimeOut(long curTime)
        {
            if (curTime - m_StartTime > m_Config.LastTime * 1000)
            {
                return true;
            }
            return false;
        }
        public AiActionConfig Config
        {
            get { return m_Config; }
        }
        public long StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }
        public long LastStartTime
        {
            get { return m_LastStartTime; }
            set { m_LastStartTime = value; }
        }
        public int CurActionData
        {
            get { return m_CurActionData; }
            set { m_CurActionData = value; }
        }

        private bool IsInRange(float value, float min, float max)
        {
            if (min > 0 && value < min)
            {
                return false;
            }
            if (max > 0 && value > max)
            {
                return false;
            }
            return true;
        }
        private AiActionConfig m_Config;
        private long m_LastStartTime = 0;
        private long m_StartTime = 0;
        private int m_CurActionData = 0;
    }
    public class AiSkillActionInfo : AiActionInfo
    {
        public AiSkillActionInfo(AiActionConfig config) : base(config)
        {
        }
        public int RandomGetSkill()
        {
            List<int> skillList = Converter.ConvertNumericList<int>(Config.ActionParam);
            if (skillList.Count > 0)
            {
                return skillList[CrossEngineHelper.Random.Next(skillList.Count)];
            }
            else
            {
                return 0;
            }
        }
        public override bool IsSatifyUser(CharacterInfo entity)
        {
            List<int> skillList = Converter.ConvertNumericList<int>(Config.ActionParam);
            long curTime = TimeUtility.GetServerMilliseconds();
            for (int i = 0; i < skillList.Count; i++)
            {
                if (skillList[i] > 0)
                {
                    if (entity.GetSkillStateInfo().GetSkillInfoById(skillList[i]) == null)
                    {
                        return false;
                    }
                    SkillInfo skillInfo = entity.GetSkillStateInfo().GetSkillInfoById(skillList[i]);
                    if (null == skillInfo || skillInfo.IsInCd(curTime / 1000.0f))
                    {
                        return false;
                    }
                }
                else
                {
                    AiSkillComboList aiSkillComboConfig = AiSkillComboListProvider.Instance.GetDataById(skillList[i] * -1);
                    if (null != aiSkillComboConfig)
                    {
                        for (int j = 0; j < aiSkillComboConfig.SkillList.Count; j++)
                        {
                            if (entity.GetSkillStateInfo().GetSkillInfoById(aiSkillComboConfig.SkillList[i]) == null)
                            {
                                return false;
                            }
                            SkillInfo skillInfo = entity.GetSkillStateInfo().GetSkillInfoById(aiSkillComboConfig.SkillList[i]);
                            if (null == skillInfo || skillInfo.IsInCd(curTime / 1000.0f))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            /*
            foreach (int comboId in skillList) {
              if (comboId > 0) {
                if (entity.GetSkillStateInfo().GetSkillInfoById(comboId) == null) {
                  return false;
                }
              } else {
                AiSkillComboList aiSkillComboConfig = AiSkillComboListProvider.Instance.GetDataById(comboId * -1);
                if (null != aiSkillComboConfig) {
                  foreach (int skillId in aiSkillComboConfig.SkillList) {
                    if (entity.GetSkillStateInfo().GetSkillInfoById(skillId) == null) {
                      return false;
                    }
                  }
                } else {
                  return false;
                }
              }
            }*/
            return true;
        }
        public List<int> GetSkillCombo()
        {
            List<int> result = new List<int>();
            int id = RandomGetSkill();
            if (id < 0)
            {
                AiSkillComboList aiSkillComboConfig = AiSkillComboListProvider.Instance.GetDataById(id * -1);
                if (null != aiSkillComboConfig)
                {
                    result = aiSkillComboConfig.SkillList;
                }
            }
            else
            {
                result.Add(id);
            }
            return result;
        }
        public int CurSkillIndex
        {
            get { return m_CurSkillIndex; }
            set { m_CurSkillIndex = value; }
        }
        public List<int> CurSkillCombo
        {
            get { return m_CurSkillCombo; }
            set { m_CurSkillCombo = value; }
        }
        private List<int> m_CurSkillCombo = new List<int>();
        private int m_CurSkillIndex = 0;
    }
}

