using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class LogicServerConfig : IData
    {
        public int LogicId = -1;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            LogicId = DBCUtil.ExtractNumeric<int>(node, "LogicId", -1, true);
            return true;
        }
        public int GetId()
        {
            return LogicId;
        }
    }
    public class LogicServerConfigProvider
    {
        public LogicServerConfig GetDataById(int id)
        {
            return m_LogicServerConfigMgr.GetDataById(id);
        }
        public void Load(string file, string root)
        {
            m_LogicServerConfigMgr.CollectDataFromDBC(file, root);
        }
        public int GetServerCount()
        {
            return m_LogicServerConfigMgr.GetDataCount();
        }
        public MyDictionary<int, object> GetData()
        {
            return m_LogicServerConfigMgr.GetData();
        }
        private DataDictionaryMgr<LogicServerConfig> m_LogicServerConfigMgr = new DataDictionaryMgr<LogicServerConfig>();
        public static LogicServerConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static LogicServerConfigProvider s_Instance = new LogicServerConfigProvider();
    }
}
