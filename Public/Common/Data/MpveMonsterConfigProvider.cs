using System;

namespace ArkCrossEngine
{
    public enum MpveSceneType : int
    {
        MT_Attempt = 0,
        MT_Gold = 1,
        MT_Other,
    }
    [Serializable]
    public class MpveMonsterConfig : IData
    {
        public int m_Id = 0;
        public int m_FightingScore = 0;
        public MpveSceneType m_SceneType = MpveSceneType.MT_Attempt;
        public int m_AttributeId = 0;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            m_FightingScore = DBCUtil.ExtractNumeric<int>(node, "FightingScore", 0, true);
            m_SceneType = (MpveSceneType)DBCUtil.ExtractNumeric<int>(node, "SceneType", 0, true);
            m_AttributeId = DBCUtil.ExtractNumeric<int>(node, "AttributeId", 0, true);
            return true;
        }
        public int GetId()
        {
            return m_Id;
        }
    }
    public class MpveMonsterConfigProvider
    {
        public DataDictionaryMgr<MpveMonsterConfig> MpveMonsterConfigMgr
        {
            get { return m_MpveMonsterConfigMgr; }
        }
        public MpveMonsterConfig GetDataById(int id)
        {
            return m_MpveMonsterConfigMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_MpveMonsterConfigMgr.GetDataCount();
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_MpveMonsterConfigMgr.CollectDataFromDBC(file, root, bytes);
        }

        private DataDictionaryMgr<MpveMonsterConfig> m_MpveMonsterConfigMgr = new DataDictionaryMgr<MpveMonsterConfig>();
        public static MpveMonsterConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static MpveMonsterConfigProvider s_Instance = new MpveMonsterConfigProvider();
    }
}
