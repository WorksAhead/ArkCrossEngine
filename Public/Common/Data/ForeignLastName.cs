﻿namespace ArkCrossEngine
{
    public class ForeignLastName : IData
    {
        public int m_Id = 0;
        public string m_LastName = "";
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            m_LastName = DBCUtil.ExtractString(node, "LastName", "", true);
            return true;
        }
        public int GetId()
        {
            return m_Id;
        }
    }

    public class ForeignLastNameProvider
    {
        public LastName GetDataById(int id)
        {
            return m_LastNameMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_LastNameMgr.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_LastNameMgr.CollectDataFromDBC(file, root);
        }

        private DataDictionaryMgr<LastName> m_LastNameMgr = new DataDictionaryMgr<LastName>();

        public static ForeignLastNameProvider Instance
        {
            get { return s_Instance; }
        }
        private static ForeignLastNameProvider s_Instance = new ForeignLastNameProvider();
    }
}
