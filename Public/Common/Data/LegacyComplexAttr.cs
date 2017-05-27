using System;

namespace ArkCrossEngine
{
    [Serializable]
    public class LegacyComplexAttrConifg : IData
    {
        public int m_Id = 0;
        public int m_PeerA = 0;
        public int m_PeerB = 0;
        public int Property = 0;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            m_PeerA = DBCUtil.ExtractNumeric<int>(node, "PeerA", 0, true);
            m_PeerB = DBCUtil.ExtractNumeric<int>(node, "PeerB", 0, true);
            Property = DBCUtil.ExtractNumeric<int>(node, "Property", 0, false);
            return true;
        }
        public int GetId()
        {
            return m_Id;
        }
    }
    public class LegacyComplexAttrConifgProvider
    {
        public DataDictionaryMgr<LegacyComplexAttrConifg> LegacyComplexAttrConifgMgr
        {
            get { return m_LegacyComplexAttrConifgMgr; }
        }
        public LegacyComplexAttrConifg GetDataById(int id)
        {
            return m_LegacyComplexAttrConifgMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_LegacyComplexAttrConifgMgr.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_LegacyComplexAttrConifgMgr.CollectDataFromDBC(file, root);
        }
        private DataDictionaryMgr<LegacyComplexAttrConifg> m_LegacyComplexAttrConifgMgr = new DataDictionaryMgr<LegacyComplexAttrConifg>();
        public static LegacyComplexAttrConifgProvider Instance
        {
            get { return s_Instance; }
        }
        private static LegacyComplexAttrConifgProvider s_Instance = new LegacyComplexAttrConifgProvider();
    }
}
