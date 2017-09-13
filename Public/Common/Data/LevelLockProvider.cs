using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class LevelLock : IData
    {
        public int m_Id = 0;
        public string m_Name = "";
        public int m_Order = 0;
        public int m_Type = 0;
        public int m_Level = 0;
        public int m_Area = 0;
        public string m_Note = "";
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "ID", 0, true);
            m_Type = DBCUtil.ExtractNumeric<int>(node, "Type", 0, true);
            m_Level = DBCUtil.ExtractNumeric<int>(node, "Level", 0, true);
            m_Order = DBCUtil.ExtractNumeric<int>(node, "Order", 0, true);
            m_Area = DBCUtil.ExtractNumeric<int>(node, "Area", 0, true);
            m_Name = DBCUtil.ExtractString(node, "Name", "", true);
            m_Note = DBCUtil.ExtractString(node, "Note", "", true);
            return true;
        }
        public int GetId()
        {
            return m_Id;
        }
    }
    public class LevelLockProvider
    {
        public DataDictionaryMgr<LevelLock> LevelLockMgr
        {
            get
            {
                return m_LevelLockMgr;
            }
        }
        public LevelLock GetDataById(int id)
        {
            return m_LevelLockMgr.GetDataById(id);
        }
        public MyDictionary<int, object> GetData()
        {
            return m_LevelLockMgr.GetData();
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_LevelLockMgr.CollectDataFromDBC(file, root, bytes);
        }
        public int GetDataCount()
        {
            return m_LevelLockMgr.GetDataCount();
        }
        public void Clear()
        {
            m_LevelLockMgr.Clear();
        }
        private DataDictionaryMgr<LevelLock> m_LevelLockMgr = new DataDictionaryMgr<LevelLock>();

        public static LevelLockProvider Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static LevelLockProvider s_Instance = new LevelLockProvider();
    }
}