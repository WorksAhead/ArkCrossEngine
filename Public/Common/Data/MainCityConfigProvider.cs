using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class MainCityConfig : IData
    {
        public int m_SceneId = 0;
        public string m_Name = "";
        public string m_Discribe = "";
        public string m_ChapterName1 = "";
        public string m_ChapterName2 = "";
        public int m_Level = -1;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_SceneId = DBCUtil.ExtractNumeric<int>(node, "SceneId", 0, true);
            m_Name = DBCUtil.ExtractString(node, "Name", "", true);
            m_Discribe = DBCUtil.ExtractString(node, "Dis", "", true);
            m_ChapterName1 = DBCUtil.ExtractString(node, "ChapterName_1", "", false);
            m_ChapterName2 = DBCUtil.ExtractString(node, "ChapterName_2", "", false);
            m_Level = DBCUtil.ExtractNumeric<int>(node, "Level", -1, true);
            return true;
        }
        public int GetId()
        {
            return m_SceneId;
        }
    }
    public class MainCityConfigProvider
    {
        public DataDictionaryMgr<MainCityConfig> MainCityConfigMgr
        {
            get
            {
                return m_MainCityConfigMgr;
            }
        }
        public MainCityConfig GetDataById(int id)
        {
            return m_MainCityConfigMgr.GetDataById(id);
        }
        public MyDictionary<int, object> GetData()
        {
            return m_MainCityConfigMgr.GetData();
        }
        public void Load(string file, string root)
        {
            m_MainCityConfigMgr.CollectDataFromDBC(file, root);
        }
        public int GetDataCount()
        {
            return m_MainCityConfigMgr.GetDataCount();
        }
        public void Clear()
        {
            m_MainCityConfigMgr.Clear();
        }

        private DataDictionaryMgr<MainCityConfig> m_MainCityConfigMgr = new DataDictionaryMgr<MainCityConfig>();

        public static MainCityConfigProvider Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static MainCityConfigProvider s_Instance = new MainCityConfigProvider();
    }

}
