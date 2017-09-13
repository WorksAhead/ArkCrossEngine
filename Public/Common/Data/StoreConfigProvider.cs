using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class StoreConfig : IData
    {
        public int Id = 0;
        public int m_ItemId = 0;
        public int m_DayLimit = 0;
        public bool m_HaveDayLimit = true;
        public List<int> m_Price = new List<int>();
        public int m_ItemNum = 0;
        public int m_Currency = 0;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            m_ItemId = DBCUtil.ExtractNumeric<int>(node, "ItemID", 0, false);
            m_DayLimit = DBCUtil.ExtractNumeric<int>(node, "DayLimit", 0, false);
            if (m_DayLimit == 0)
            {
                m_HaveDayLimit = false;
            }
            m_Price = DBCUtil.ExtractNumericList<int>(node, "Price", 0, false);
            m_ItemNum = DBCUtil.ExtractNumeric<int>(node, "ItemNum", 0, false);
            m_Currency = DBCUtil.ExtractNumeric<int>(node, "Currency", 0, false);
            return true;
        }
        public int GetId()
        {
            return Id;
        }
    }
    public class StoreConfigProvider
    {
        public DataDictionaryMgr<StoreConfig> StoreDictionaryMgr
        {
            get { return m_StoreConfigMgr; }
        }

        public StoreConfig GetDataById(int id)
        {
            return m_StoreConfigMgr.GetDataById(id);
        }

        public int GetDataCount()
        {
            return m_StoreConfigMgr.GetDataCount();
        }

        public void Load(string file, string root, byte[] bytes)
        {
            m_StoreConfigMgr.CollectDataFromDBC(file, root, bytes);
        }

        private DataDictionaryMgr<StoreConfig> m_StoreConfigMgr = new DataDictionaryMgr<StoreConfig>();

        public static StoreConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static StoreConfigProvider s_Instance = new StoreConfigProvider();
    }
}
