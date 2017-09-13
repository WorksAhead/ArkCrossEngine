namespace ArkCrossEngine
{
    public class CameraConfig : IData
  {
    public int Id;
    public bool IsActive;
    public int EnterBattleTime;
    public int LeaveBattleTime;
    public float HeightInMainCity;
    public float DisInMainCity;
    public float HeightInIdle;
    public float DisInIdle;
    public float HeightInCombat;
    public float DisInCombat;

    public bool CollectDataFromDBC(DBC_Row node)
    {
      Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
      IsActive = DBCUtil.ExtractBool(node, "IsActive", false, true);
      EnterBattleTime = DBCUtil.ExtractNumeric<int>(node, "EnterBattleTime", 500, true);
      LeaveBattleTime = DBCUtil.ExtractNumeric<int>(node, "LeaveBattleTime", 500, true);
      HeightInMainCity = DBCUtil.ExtractNumeric<float>(node, "HeightInMainCity", 0.0f, true);
      DisInMainCity = DBCUtil.ExtractNumeric<float>(node, "DisInMainCity", 0.0f, true);
      HeightInIdle = DBCUtil.ExtractNumeric<float>(node, "HeightInIdle", 0.0f, true);
      DisInIdle = DBCUtil.ExtractNumeric<float>(node, "DisInIdle", 0.0f, true);
      HeightInCombat = DBCUtil.ExtractNumeric<float>(node, "HeightInCombat", 0.0f, true);
      DisInCombat = DBCUtil.ExtractNumeric<float>(node, "DisInCombat", 0.0f, true);
      return true;
    }

    public int GetId()
    {
      return Id;
    }
  }
  public class CameraConfigProvider
  {
    public CameraConfig GetCameraConfig()
    {
      return GetDataById(1);
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_CameraConfigMgr.CollectDataFromDBC(file, root, bytes);
        }
        public static CameraConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private CameraConfig GetDataById(int id)
    {
      return m_CameraConfigMgr.GetDataById(id);
    }
    private DataDictionaryMgr<CameraConfig> m_CameraConfigMgr = new DataDictionaryMgr<CameraConfig>();
    private static CameraConfigProvider s_Instance = new CameraConfigProvider();
  }
}
