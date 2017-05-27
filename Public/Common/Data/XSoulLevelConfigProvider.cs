using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    [Serializable]
    public class XSoulLevelConfig : IData
    {
        public int m_ItemId = 0;
        public int[] m_ExperienceProvideItems;
        public int m_MaxLevel = 1;
        public Dictionary<int, int> m_LevelExperience;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_ItemId = DBCUtil.ExtractNumeric<int>(node, "ItemId", 0, true);
            List<int> list = DBCUtil.ExtractNumericList<int>(node, "ExperienceProvideItems", 0, false);
            if (list.Count > 0)
            {
                m_ExperienceProvideItems = list.ToArray();
            }
            m_MaxLevel = DBCUtil.ExtractNumeric<int>(node, "MaxLevel", 1, false);
            m_LevelExperience = ExtractLevelExperience(node);
            return true;
        }

        private Dictionary<int, int> ExtractLevelExperience(DBC_Row node)
        {
            Dictionary<int, int> level_experice_dict = new Dictionary<int, int>();
            for (int level = 2; level <= m_MaxLevel; ++level)
            {
                int experience = DBCUtil.ExtractNumeric<int>(node, "Level_" + level, 1, false);
                level_experice_dict.Add(level, experience);
            }
            return level_experice_dict;
        }

        public int GetId()
        {
            return m_ItemId;
        }
    }

    public class XSoulLevelConfigProvider
    {
        public DataDictionaryMgr<XSoulLevelConfig> ItemConfigMgr
        {
            get { return m_XSoulLevelConfigMgr; }
        }
        public XSoulLevelConfig GetDataById(int id)
        {
            return m_XSoulLevelConfigMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_XSoulLevelConfigMgr.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_XSoulLevelConfigMgr.CollectDataFromDBC(file, root);
        }
        public void Clear()
        {
            m_XSoulLevelConfigMgr.Clear();
        }

        private DataDictionaryMgr<XSoulLevelConfig> m_XSoulLevelConfigMgr = new DataDictionaryMgr<XSoulLevelConfig>();

        public static XSoulLevelConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static XSoulLevelConfigProvider s_Instance = new XSoulLevelConfigProvider();
    }
}
