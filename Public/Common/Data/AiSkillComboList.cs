using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class AiSkillComboList : IData
    {
        public int Id;
        public string Describe;
        public List<int> SkillList;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            Describe = DBCUtil.ExtractString(node, "Describe", "", false);
            SkillList = DBCUtil.ExtractNumericList<int>(node, "SkillCombo", 0, false);
            return true;
        }
        public int GetId()
        {
            return Id;
        }
    }

    public class AiSkillComboListProvider
    {
        public AiSkillComboList GetDataById(int id)
        {
            return m_AiSkillComboListConfigMgr.GetDataById(id);
        }

        public int GetDataCount()
        {
            return m_AiSkillComboListConfigMgr.GetDataCount();
        }

        public void Load(string file, string root, byte[] bytes)
        {
            m_AiSkillComboListConfigMgr.CollectDataFromDBC(file, root, bytes);
        }

        public static AiSkillComboListProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<AiSkillComboList> m_AiSkillComboListConfigMgr = new DataDictionaryMgr<AiSkillComboList>();
        private static AiSkillComboListProvider s_Instance = new AiSkillComboListProvider();
    }
}
