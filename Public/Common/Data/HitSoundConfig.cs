namespace ArkCrossEngine
{
    public class HitSoundConfig : IData
    {
        public int Id;
        public float PeriodTime;
        public int MaxPeriodCount;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            PeriodTime = DBCUtil.ExtractNumeric<float>(node, "PeriodTime", 0, false);
            MaxPeriodCount = DBCUtil.ExtractNumeric<int>(node, "MaxPeriodCount", 0, false);
            return true;
        }
        public int GetId()
        {
            return Id;
        }
    }

    public class HitSoundConfigProvider
    {
        public HitSoundConfig GetData()
        {
            return m_HitSoundConfigMgr.GetDataById(0);
        }
        public void Load(string file, string root)
        {
            m_HitSoundConfigMgr.CollectDataFromDBC(file, root);
        }
        public static HitSoundConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<HitSoundConfig> m_HitSoundConfigMgr = new DataDictionaryMgr<HitSoundConfig>();
        private static HitSoundConfigProvider s_Instance = new HitSoundConfigProvider();
    }
}
