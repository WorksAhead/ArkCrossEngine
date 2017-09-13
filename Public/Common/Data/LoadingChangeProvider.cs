using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class LoadingChange : IData
    {
        public int m_Id = 0;
        public string m_Name = "";
        public string m_Path = "";
        public string m_Note = "";
        public int m_strId = 0;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "ID", 0, true);
            m_Name = DBCUtil.ExtractString(node, "Name", "", true);
            m_Path = DBCUtil.ExtractString(node, "Path", "", true);
            m_Note = DBCUtil.ExtractString(node, "Note", "", true);
            m_strId = DBCUtil.ExtractNumeric<int>(node, "StrID", 0, true);
            return true;
        }
        public int GetId()
        {
            return m_Id;
        }
    }
    public class LoadingChangeProvider
    {
        public DataDictionaryMgr<LoadingChange> LoadingChangeMgr
        {
            get
            {
                return m_LoadingChangeMgr;
            }
        }
        public LoadingChange GetDataById(int id)
        {
            return m_LoadingChangeMgr.GetDataById(id);
        }
        public MyDictionary<int, object> GetData()
        {
            return m_LoadingChangeMgr.GetData();
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_LoadingChangeMgr.CollectDataFromDBC(file, root, bytes);
        }
        public int GetDataCount()
        {
            return m_LoadingChangeMgr.GetDataCount();
        }
        public void Clear()
        {
            m_LoadingChangeMgr.Clear();
        }
        private DataDictionaryMgr<LoadingChange> m_LoadingChangeMgr = new DataDictionaryMgr<LoadingChange>();

        public static LoadingChangeProvider Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static LoadingChangeProvider s_Instance = new LoadingChangeProvider();
    }
}