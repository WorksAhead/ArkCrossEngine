using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class ItemAttributeConfig : IData
    {
        public int AttributeId;
        public string Describe;
        public int AttributeType;
        public List<string> ParamValues = new List<string>();
        public List<string> LevelAddValues = new List<string>();

        public bool CollectDataFromDBC(DBC_Row node)
        {
            AttributeId = DBCUtil.ExtractNumeric<int>(node, "AttributeId", 0, true);
            Describe = DBCUtil.ExtractString(node, "Describe", "", false);
            AttributeType = DBCUtil.ExtractNumeric<int>(node, "AttributeType", 0, true);
            ParamValues = DBCUtil.ExtractNodeByPrefix(node, "ParamValue_");
            LevelAddValues = DBCUtil.ExtractNodeByPrefix(node, "LevelAdd_");
            return true;
        }

        public int GetId()
        {
            return AttributeId;
        }
    }

    public class ItemAttributeConfigProvider
    {
        public ItemAttributeConfig GetDataById(int id)
        {
            return m_ItemAttributeConfigMgr.GetDataById(id);
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_ItemAttributeConfigMgr.CollectDataFromDBC(file, root, bytes);
        }

        public static ItemAttributeConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static ItemAttributeConfigProvider s_Instance = new ItemAttributeConfigProvider();
        private DataDictionaryMgr<ItemAttributeConfig> m_ItemAttributeConfigMgr = new DataDictionaryMgr<ItemAttributeConfig>();
    }
}
