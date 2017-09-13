using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class SystemGuideConfig : IData
    {
        public int Id;
        public List<int> Operations;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            Operations = DBCUtil.ExtractNumericList<int>(node, "Operation", 0, true);
            return true;
        }
        public int GetId()
        {
            return Id;
        }
    }

    public class SystemGuideConfigProvider
    {
        public SystemGuideConfig GetDataById(int id)
        {
            return m_SystemGuideConfigMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_SystemGuideConfigMgr.GetDataCount();
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_SystemGuideConfigMgr.CollectDataFromDBC(file, root, bytes);
        }
        public static SystemGuideConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<SystemGuideConfig> m_SystemGuideConfigMgr = new DataDictionaryMgr<SystemGuideConfig>();
        private static SystemGuideConfigProvider s_Instance = new SystemGuideConfigProvider();
    }
}
