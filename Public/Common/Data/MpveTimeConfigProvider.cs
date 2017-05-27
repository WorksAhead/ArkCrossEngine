using System;

namespace ArkCrossEngine
{
    [Serializable]
    public class MpveTimeConfig : IData
    {
        public int m_SceneId;
        public int m_StartHour;
        public int m_StartMinute;
        public int m_StartSecond;
        public int m_EndHour;
        public int m_EndMinute;
        public int m_EndSecond;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_SceneId = DBCUtil.ExtractNumeric<int>(node, "SceneId", 0, true);
            m_StartHour = DBCUtil.ExtractNumeric<int>(node, "StartHour", 0, true);
            m_StartMinute = DBCUtil.ExtractNumeric<int>(node, "StartMinute", 0, true);
            m_StartSecond = DBCUtil.ExtractNumeric<int>(node, "StartSecond", 0, true);
            m_EndHour = DBCUtil.ExtractNumeric<int>(node, "EndHour", 0, true);
            m_EndMinute = DBCUtil.ExtractNumeric<int>(node, "EndMinute", 0, true);
            m_EndSecond = DBCUtil.ExtractNumeric<int>(node, "EndSecond", 0, true);

            return true;
        }
        public int GetId()
        {
            return m_SceneId;
        }
    }
    public class MpveTimeConfigProvider
    {
        public DataDictionaryMgr<MpveTimeConfig> MpveTimeConfigMgr
        {
            get { return m_MpveTimeConfigMgr; }
        }
        public MpveTimeConfig GetDataById(int id)
        {
            return m_MpveTimeConfigMgr.GetDataById(id);
        }
        public void Load(string file, string root)
        {
            m_MpveTimeConfigMgr.CollectDataFromDBC(file, root);
        }
        private DataDictionaryMgr<MpveTimeConfig> m_MpveTimeConfigMgr = new DataDictionaryMgr<MpveTimeConfig>();
        public static MpveTimeConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static MpveTimeConfigProvider s_Instance = new MpveTimeConfigProvider();
    }
}
