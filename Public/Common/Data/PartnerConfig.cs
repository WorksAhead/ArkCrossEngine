using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class PartnerConfig : IData
    {
        public int Id;
        public string Name;
        public string Story;                      // 传记
        public int LinkId;                        // 对应的npcid
        public int PartnerFragId;                 // 对应的碎片物品ID
        public int PartnerFragNum;                // 合成完成伙伴所需要的碎片数量
        public List<float> InheritAttackAttrPercentList = new List<float>();          // 继承属性百分比
        public List<float> InheritDefenceAttrPercentList = new List<float>();
        public int BornSkill;                     // 出生技能
        public int MaxImproveLevel;               // 最大强化等级
        public List<int> AttrAppendList = new List<int>();   // 对应强化等级提供的附加属性表
        public List<List<int>> SkillList = new List<List<int>>(); // 技能列表
        public float CoolDown;
        public float Duration;
        public int MaxStage;                      // 最高阶段
        public int LevelUpItemId;
        public int StageUpItemId;
        public int Ailogic;
        public int AiParam;
        public string AtlasPath;
        public string Icon0;
        public string Icon1;
        public string Icon2;
        public string Icon3;
        public string StageDescription0;
        public string StageDescription1;
        public string StageDescription2;
        public string StageDescription3;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            Name = DBCUtil.ExtractString(node, "Name", "", false);
            Story = DBCUtil.ExtractString(node, "Story", "", false);
            LinkId = DBCUtil.ExtractNumeric<int>(node, "LinkId", 0, false);
            PartnerFragId = DBCUtil.ExtractNumeric<int>(node, "PartnerFragId", 0, false);
            PartnerFragNum = DBCUtil.ExtractNumeric<int>(node, "PartnerFragNum", 0, false);
            InheritAttackAttrPercentList = DBCUtil.ExtractNumericList<float>(node, "InheritAttackAttrPercent", 0.0f, false);
            InheritDefenceAttrPercentList = DBCUtil.ExtractNumericList<float>(node, "InheritDefenseAttrPercent", 0.0f, false);
            BornSkill = DBCUtil.ExtractNumeric<int>(node, "BornSkill", -1, false);
            MaxImproveLevel = DBCUtil.ExtractNumeric<int>(node, "MaxImproveLevel", 0, false);
            AttrAppendList = DBCUtil.ExtractNumericList<int>(node, "AttrAppend", 0, false);
            CoolDown = DBCUtil.ExtractNumeric<float>(node, "CoolDown", 0, false);
            Duration = DBCUtil.ExtractNumeric<float>(node, "Duration", 0, false);
            for (int i = 0; i <= 3; ++i)
            {
                SkillList.Add(DBCUtil.ExtractNumericList<int>(node, "SkillList" + i, 0, false));
            }
            LevelUpItemId = DBCUtil.ExtractNumeric<int>(node, "LevelUpItemId", 0, false);
            StageUpItemId = DBCUtil.ExtractNumeric<int>(node, "StageUpItemId", 0, false);
            Ailogic = DBCUtil.ExtractNumeric<int>(node, "AiLogicId", 0, false);
            AiParam = DBCUtil.ExtractNumeric<int>(node, "AiParam", 0, false);
            AtlasPath = DBCUtil.ExtractString(node, "AtlasPath", "", false);
            Icon0 = DBCUtil.ExtractString(node, "Icon0", "", false);
            Icon1 = DBCUtil.ExtractString(node, "Icon1", "", false);
            Icon2 = DBCUtil.ExtractString(node, "Icon2", "", false);
            Icon3 = DBCUtil.ExtractString(node, "Icon3", "", false);
            StageDescription0 = DBCUtil.ExtractString(node, "Desc0", "", false);
            StageDescription1 = DBCUtil.ExtractString(node, "Desc1", "", false);
            StageDescription2 = DBCUtil.ExtractString(node, "Desc2", "", false);
            StageDescription3 = DBCUtil.ExtractString(node, "Desc3", "", false);
            return true;
        }
        public int GetId()
        {
            return Id;
        }
    }

    public class PartnerLevelUpConfig : IData
    {
        public int Id;
        public int Level;
        public int ItemCost;
        public int GoldCost;
        public float Rate;
        public bool IsFailedDemote;
        public string PartnerRank;
        public string PartnerRankColor;//伙伴颜色
        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            Level = DBCUtil.ExtractNumeric<int>(node, "Level", 0, false);
            ItemCost = DBCUtil.ExtractNumeric<int>(node, "ItemCost", 0, false);
            GoldCost = DBCUtil.ExtractNumeric<int>(node, "GoldCost", 0, false);
            Rate = DBCUtil.ExtractNumeric<float>(node, "Rate", 0.0f, false);
            IsFailedDemote = DBCUtil.ExtractBool(node, "FailedDemote", false, false);
            PartnerRank = DBCUtil.ExtractString(node, "PartnerRank", "", false);
            PartnerRankColor = DBCUtil.ExtractString(node, "PartnerRankColor", "", false);

            return true;
        }
        public int GetId()
        {
            return Id;
        }
    }
    public class PartnerStageUpConfig : IData
    {
        public int Id;
        public int Level;
        public int ItemCost;
        public int GoldCost;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            Level = DBCUtil.ExtractNumeric<int>(node, "Level", 0, true);
            ItemCost = DBCUtil.ExtractNumeric<int>(node, "ItemCost", 0, false);
            GoldCost = DBCUtil.ExtractNumeric<int>(node, "GoldCost", 0, false);
            return true;
        }
        public int GetId()
        {
            return Id;
        }
    }
    public class PartnerConfigProvider
    {
        public PartnerConfig GetDataById(int id)
        {
            return m_PartnerConfigMgr.GetDataById(id);
        }
        public List<PartnerConfig> GetAllData()
        {
            List<PartnerConfig> partnerList = new List<PartnerConfig>();
            MyDictionary<int, object> partnerDict = m_PartnerConfigMgr.GetData();
            foreach (PartnerConfig obj in partnerDict.Values)
            {
                if (null != obj)
                    partnerList.Add(obj);
            }
            return partnerList;
        }
        public int GetDataCount()
        {
            return m_PartnerConfigMgr.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_PartnerConfigMgr.CollectDataFromDBC(file, root);
        }
        public void Clear()
        {
            m_PartnerConfigMgr.Clear();
        }
        public static PartnerConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<PartnerConfig> m_PartnerConfigMgr = new DataDictionaryMgr<PartnerConfig>();
        private static PartnerConfigProvider s_Instance = new PartnerConfigProvider();
    }
    public class PartnerLevelUpConfigProvider
    {
        public PartnerLevelUpConfig GetDataById(int id)
        {
            return m_PartnerLevelUpConfigMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_PartnerLevelUpConfigMgr.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_PartnerLevelUpConfigMgr.CollectDataFromDBC(file, root);
        }
        public static PartnerLevelUpConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<PartnerLevelUpConfig> m_PartnerLevelUpConfigMgr = new DataDictionaryMgr<PartnerLevelUpConfig>();
        private static PartnerLevelUpConfigProvider s_Instance = new PartnerLevelUpConfigProvider();
    }
    public class PartnerStageUpConfigProvider
    {
        public PartnerStageUpConfig GetDataById(int id)
        {
            return m_PartnerStageUpConfigMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_PartnerStageUpConfigMgr.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_PartnerStageUpConfigMgr.CollectDataFromDBC(file, root);
        }
        public static PartnerStageUpConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<PartnerStageUpConfig> m_PartnerStageUpConfigMgr = new DataDictionaryMgr<PartnerStageUpConfig>();
        private static PartnerStageUpConfigProvider s_Instance = new PartnerStageUpConfigProvider();
    }
}
