using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class Data_NotificationConfig : IData
    {
        const string C_NOTIFICATION_FORMAT = "HH/mm";

        public int m_Id;
        public string m_Title;
        public string m_Content;
        public DateTime m_Date;
        public string m_Interval;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            m_Title = DBCUtil.ExtractString(node, "Title", "", true);
            m_Content = DBCUtil.ExtractString(node, "Content", "", true);
            try
            {
                string strDate = DBCUtil.ExtractString(node, "Date", "", true);
                m_Date = DateTime.ParseExact(strDate, C_NOTIFICATION_FORMAT, System.Globalization.CultureInfo.CurrentCulture);
            }
            catch (System.Exception ex)
            {
                LogSystem.Error("NotificationConfig.txt date parse error Id:{0} Format:{1} ex:{2}",
                  m_Id, C_NOTIFICATION_FORMAT, ex.Message);
            }
            m_Interval = DBCUtil.ExtractString(node, "Interval", "", true);
            return true;
        }
        public int GetId()
        {
            return m_Id;
        }
    }

    public class NotificationConfigProvider
    {
        public DataDictionaryMgr<Data_NotificationConfig> NotificationConfigMgr
        {
            get { return m_NotificationConfigMgr; }
        }
        public Data_NotificationConfig GetNotificationConfigById(int id)
        {
            return m_NotificationConfigMgr.GetDataById(id);
        }
        public MyDictionary<int, object> GetAllNotificationConfig()
        {
            return m_NotificationConfigMgr.GetData();
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_NotificationConfigMgr.CollectDataFromDBC(file, root, bytes);
        }
        public void Clear()
        {
            m_NotificationConfigMgr.Clear();
        }
        private DataDictionaryMgr<Data_NotificationConfig> m_NotificationConfigMgr = new DataDictionaryMgr<Data_NotificationConfig>();
        public static NotificationConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static NotificationConfigProvider s_Instance = new NotificationConfigProvider();
    }
}
