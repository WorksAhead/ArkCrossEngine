using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class PartnerInfo
    {
        public PartnerInfo(PartnerConfig config)
        {
            m_Id = config.Id;
            m_linkId = config.LinkId;
            m_Name = config.Name;
            m_CurAdditionLevel = 1;
            m_CurSkillStage = 1;
            m_Config = config;
        }
        public int Id
        {
            get { return m_Id; }
        }
        public int LinkId
        {
            get { return m_linkId; }
        }
        public int CurAdditionLevel
        {
            get { return m_CurAdditionLevel; }
            set { m_CurAdditionLevel = value; }
        }
        public int CurSkillStage
        {
            get { return m_CurSkillStage; }
            set { m_CurSkillStage = value; }
        }
        public int LevelUpItemId
        {
            get
            {
                if (null != m_Config)
                {
                    return m_Config.LevelUpItemId;
                }
                else
                {
                    return 0;
                }
            }
        }
        public int StageUpItemId
        {
            get
            {
                if (null != m_Config)
                {
                    return m_Config.StageUpItemId;
                }
                else
                {
                    return 0;
                }
            }
        }
        public int BornSkill
        {
            get
            {
                if (null != m_Config)
                {
                    return m_Config.BornSkill;
                }
                else
                {
                    return -1;
                }
            }
        }

        public long Duration
        {
            get
            {
                if (null != m_Config)
                {
                    return (long)(m_Config.Duration * 1000);
                }
                else
                {
                    return 0;
                }
            }
        }

        public long CoolDown
        {
            get
            {
                if (null != m_Config)
                {
                    return (long)(m_Config.CoolDown * 1000);
                }
                else
                {
                    return 0;
                }
            }
        }
        public int GetAppendAttrConfigId()
        {
            if (null != m_Config && m_CurAdditionLevel <= m_Config.AttrAppendList.Count)
            {
                return m_Config.AttrAppendList[m_CurAdditionLevel - 1];
            }
            return 0;
        }

        public float GetInheritAttackAttrPercent()
        {
            if (null != m_Config && m_Config.InheritAttackAttrPercentList.Count >= m_CurAdditionLevel)
            {
                return m_Config.InheritAttackAttrPercentList[m_CurAdditionLevel - 1];
            }
            return 0.0f;
        }
        public float GetInheritAttackAttrPercent(int additionLevel)
        {
            if (null != m_Config && m_Config.InheritAttackAttrPercentList.Count >= additionLevel)
            {
                return m_Config.InheritAttackAttrPercentList[additionLevel - 1];
            }
            return 0.0f;
        }
        public float GetInheritDefenceAttrPercent()
        {
            if (null != m_Config && m_Config.InheritDefenceAttrPercentList.Count >= m_CurAdditionLevel)
            {
                return m_Config.InheritDefenceAttrPercentList[m_CurAdditionLevel - 1];
            }
            return 0.0f;
        }
        public float GetInheritDefenceAttrPercent(int additionLevel)
        {
            if (null != m_Config && m_Config.InheritDefenceAttrPercentList.Count >= additionLevel)
            {
                return m_Config.InheritDefenceAttrPercentList[additionLevel - 1];
            }
            return 0.0f;
        }

        public int GetAiLogic()
        {
            if (null != m_Config)
            {
                return m_Config.Ailogic;
            }
            return 0;
        }

        public int GetAiParam()
        {
            if (null != m_Config)
            {
                return m_Config.AiParam;
            }
            return 0;
        }
        public List<int> GetSkillList()
        {
            if (null != m_Config)
            {
                int skillCount = m_Config.SkillList.Count;
                if (m_CurSkillStage <= skillCount)
                {
                    return m_Config.SkillList[CurSkillStage - 1];
                }
                else
                {
                    return m_Config.SkillList[skillCount - 1];
                }
            }
            return null;
        }
        public int GetMaxLevel()
        {
            return m_MaxLevel;
        }
        public int GetMaxStage()
        {
            return m_MaxStage;
        }

        public float GetHpCostPerTick(int hpMax)
        {
            if (Duration != 0)
            {
                return hpMax * (1.0f * m_TickInterval / Duration) * -1;
            }
            else
            {
                return hpMax * -1;
            }
        }
        public long LastTickTime
        {
            get { return m_LastTickTime; }
            set { m_LastTickTime = value; }
        }
        public long TickInterval
        {
            get { return m_TickInterval; }
        }
        private int m_Id;
        private int m_linkId;
        private string m_Name;
        private int m_CurAdditionLevel;  // 当前属性加成等级
        private int m_CurSkillStage;  // 当前技能等级
        private PartnerConfig m_Config;
        private const int m_MaxLevel = 9;
        private const int m_MaxStage = 4;
        private long m_LastTickTime = 0;
        private const long m_TickInterval = 1000;
    }
}
