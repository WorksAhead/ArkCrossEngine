namespace ArkCrossEngine
{
    public class AiActionConfig : IData
    {
        public int Id;
        public int AiActionType;
        public string ActionParam;
        public float DisMin;
        public float DisMax;
        public float TargetHpMin;
        public float TargetHpMax;
        public float SelfHpMin;
        public float SelfHpMax;
        public float Cooldown;
        public float Weight;
        public float LastTime;
        public bool CanInterrupt;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            AiActionType = DBCUtil.ExtractNumeric<int>(node, "ActionType", 1, true);
            ActionParam = DBCUtil.ExtractNumeric<string>(node, "ActionParam", "", false);
            DisMin = DBCUtil.ExtractNumeric<float>(node, "DisMin", -1.0f, false);
            DisMax = DBCUtil.ExtractNumeric<float>(node, "DisMax", -1.0f, false);
            TargetHpMin = DBCUtil.ExtractNumeric<float>(node, "TargetHpMin", -1.0f, false);
            TargetHpMax = DBCUtil.ExtractNumeric<float>(node, "TargetHpMax", -1.0f, false);
            SelfHpMin = DBCUtil.ExtractNumeric<float>(node, "SelfHpMin", -1.0f, false);
            SelfHpMax = DBCUtil.ExtractNumeric<float>(node, "SelfHpMax", -1.0f, false);
            Cooldown = DBCUtil.ExtractNumeric<float>(node, "Cooldown", -1.0f, false);
            Weight = DBCUtil.ExtractNumeric<float>(node, "Weight", 0.0f, false);
            LastTime = DBCUtil.ExtractNumeric<float>(node, "LastTime", 0.0f, true);
            CanInterrupt = DBCUtil.ExtractBool(node, "CanInterrupt", false, false);
            return true;
        }

        public int GetId()
        {
            return Id;
        }
    }

    public class AiActionConfigProvider
    {
        public AiActionConfig GetDataById(int id)
        {
            return m_AiActionConfigMrg.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_AiActionConfigMrg.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_AiActionConfigMrg.CollectDataFromDBC(file, root);
        }
        public static AiActionConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<AiActionConfig> m_AiActionConfigMrg = new DataDictionaryMgr<AiActionConfig>();
        private static AiActionConfigProvider s_Instance = new AiActionConfigProvider();
    }
}
