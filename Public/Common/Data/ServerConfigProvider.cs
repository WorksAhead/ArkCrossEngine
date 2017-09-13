using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class ServerConfig : IData
    {
        public int ServerId = -1;
        public string ServerName = "";
        public string NodeIp = "";
        public int NodePort = -1;
        public int LogicServerId = -1;
        public string LogicServerName = "";
        public int ServerState = -1;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            ServerId = DBCUtil.ExtractNumeric<int>(node, "ServerId", -1, true);
            ServerName = DBCUtil.ExtractString(node, "ServerName", "", true);
            NodeIp = DBCUtil.ExtractString(node, "NodeIp", "", true);
            NodePort = DBCUtil.ExtractNumeric<int>(node, "NodePort", -1, true);
            LogicServerId = DBCUtil.ExtractNumeric<int>(node, "LogicServerId", -1, true);
            LogicServerName = DBCUtil.ExtractNumeric(node, "LogicServerName", "", true);
            ServerState = DBCUtil.ExtractNumeric<int>(node, "ServerState", -1, true);
            return true;
        }
        public int GetId()
        {
            return ServerId;
        }
    }
    public class ServerConfigProvider
    {
        public ServerConfig GetDataById(int id)
        {
            return m_ServerConfigMgr.GetDataById(id);
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_ServerConfigMgr.CollectDataFromDBC(file, root, bytes);
        }
        public MyDictionary<int, object> GetData()
        {
            return m_ServerConfigMgr.GetData();
        }
        private DataDictionaryMgr<ServerConfig> m_ServerConfigMgr = new DataDictionaryMgr<ServerConfig>();
        public static ServerConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static ServerConfigProvider s_Instance = new ServerConfigProvider();
    }
}
