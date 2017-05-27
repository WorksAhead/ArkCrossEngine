using System;

namespace ArkCrossEngine
{
    [Serializable]
    public class ItemCompoundConfig : IData
    {
        public int m_ItemId = 0;
        public string m_Describe = "";
        public int m_PartId = 0;
        public int m_PartNum = 0;
        public int m_MaterialId = 0;
        public int m_MaterialNum = 0;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_ItemId = DBCUtil.ExtractNumeric<int>(node, "ItemId", 0, true);
            m_Describe = DBCUtil.ExtractString(node, "Describe", "", false);
            m_PartId = DBCUtil.ExtractNumeric<int>(node, "PartId", 0, false);
            m_PartNum = DBCUtil.ExtractNumeric<int>(node, "PartNum", 0, false);
            m_MaterialId = DBCUtil.ExtractNumeric<int>(node, "MaterialId", 0, false);
            m_MaterialNum = DBCUtil.ExtractNumeric<int>(node, "MaterialNum", 0, false);
            return true;
        }
        public int GetId()
        {
            return m_ItemId;
        }
    }
    public class ItemCompoundConfigProvider
    {
        public DataDictionaryMgr<ItemCompoundConfig> ItemCompoundConfigMgr
        {
            get { return m_ItemCompoundConfigMgr; }
        }
        public ItemCompoundConfig GetDataById(int id)
        {
            return m_ItemCompoundConfigMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_ItemCompoundConfigMgr.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_ItemCompoundConfigMgr.CollectDataFromDBC(file, root);
        }

        private DataDictionaryMgr<ItemCompoundConfig> m_ItemCompoundConfigMgr = new DataDictionaryMgr<ItemCompoundConfig>();
        public static ItemCompoundConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static ItemCompoundConfigProvider s_Instance = new ItemCompoundConfigProvider();
    }
}
