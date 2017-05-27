namespace ArkCrossEngine
{
    public sealed class ScrollMessageConfig : IData
    {
        public int m_Id;
        public int m_Week;
        public int m_StartHour;
        public int m_StartMinute;
        public int m_EndHour;
        public int m_EndMinute;
        public int m_Interval;
        public string m_Message;
        public int m_ScrollCount;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "ID", 0, true);
            m_Week = DBCUtil.ExtractNumeric<int>(node, "Week", 0, true);
            m_StartHour = DBCUtil.ExtractNumeric<int>(node, "StartHour", 0, true);
            m_StartMinute = DBCUtil.ExtractNumeric<int>(node, "StartMinute", 0, true);
            m_EndHour = DBCUtil.ExtractNumeric<int>(node, "EndHour", 0, true);
            m_EndMinute = DBCUtil.ExtractNumeric<int>(node, "EndMinute", 0, true);
            m_Interval = DBCUtil.ExtractNumeric<int>(node, "Interval", 0, true);
            m_Message = DBCUtil.ExtractString(node, "Message", "", true);
            m_ScrollCount = DBCUtil.ExtractNumeric<int>(node, "ScrollCount", 0, true);

            return true;
        }

        public int GetId()
        {
            return m_Id;
        }
    }

    public sealed class ScrollMessageConfigProvider
    {
        public DataListMgr<ScrollMessageConfig> ScrollMessageConfigMgr
        {
            get { return m_ScrollMessageConfigMgr; }
        }

        public void LoadForClient()
        {
            m_ScrollMessageConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_ScrollMessageConfig, "ScrollMessage");
        }

        private DataListMgr<ScrollMessageConfig> m_ScrollMessageConfigMgr = new DataListMgr<ScrollMessageConfig>();

        public static ScrollMessageConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static ScrollMessageConfigProvider s_Instance = new ScrollMessageConfigProvider();
    }
}
