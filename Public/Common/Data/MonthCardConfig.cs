namespace ArkCrossEngine
{
    class MonthCardConfig : IData
    {
        public int Id;
        public int Duration;
        public float Price;
        public int Diamond;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            Duration = DBCUtil.ExtractNumeric<int>(node, "Duration", 30, true);
            Price = DBCUtil.ExtractNumeric<int>(node, "Price", 0, false);
            Diamond = DBCUtil.ExtractNumeric<int>(node, "Diamond", 0, false);
            return true;
        }

        public int GetId()
        {
            return Id;
        }
    }

    public class MonthCardConfigProvider
    {
        public void Load(string file, string root)
        {
            m_MonthCardConfigMgr.CollectDataFromDBC(file, root);
        }
        private MonthCardConfig GetDataById(int id)
        {
            return m_MonthCardConfigMgr.GetDataById(id);
        }
        public float GetPrice()
        {
            MonthCardConfig mcc = GetDataById(c_MonthCardId);
            if (null != mcc)
            {
                return mcc.Price;
            }
            return 0;
        }
        public int GetRewardDiamond()
        {
            MonthCardConfig mcc = GetDataById(c_MonthCardId);
            if (null != mcc)
            {
                return mcc.Diamond;
            }
            return 0;
        }
        public int GetDuration()
        {
            MonthCardConfig mcc = GetDataById(c_MonthCardId);
            if (null != mcc)
            {
                return mcc.Duration;
            }
            return 0;
        }
        public static MonthCardConfigProvider Instacne
        {
            get { return s_Instance; }
        }
        private const int c_MonthCardId = 1;
        private DataDictionaryMgr<MonthCardConfig> m_MonthCardConfigMgr = new DataDictionaryMgr<MonthCardConfig>();
        private static MonthCardConfigProvider s_Instance = new MonthCardConfigProvider();
    }
}
