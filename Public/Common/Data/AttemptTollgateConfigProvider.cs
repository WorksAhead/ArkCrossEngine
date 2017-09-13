using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    [Serializable]
    public class AttemptTollgateConfig : IData
    {
        public int m_BossNum = 0;
        public string m_Describe = "";
        public int m_DropOutMoney = 0;
        public float m_MoneyProbality = 1f;
        public float m_MoneyFactor = 0f;
        public int m_DropOutItemNum = 0;
        public List<int> m_ItemId = new List<int>();
        public List<float> m_ItemProbality = new List<float>();
        public List<float> m_ItemFactor = new List<float>();
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_BossNum = DBCUtil.ExtractNumeric<int>(node, "BossNum", 0, true);
            m_Describe = DBCUtil.ExtractString(node, "Describe", "", false);
            m_DropOutMoney = DBCUtil.ExtractNumeric<int>(node, "DropOutMoney", 0, false);
            m_MoneyProbality = DBCUtil.ExtractNumeric<float>(node, "MoneyProbality", 0f, false);
            m_MoneyFactor = DBCUtil.ExtractNumeric<float>(node, "MoneyFactor", 0f, false);
            m_DropOutItemNum = DBCUtil.ExtractNumeric<int>(node, "DropOutItemNum", 0, false);
            for (int i = 0; i < m_DropOutItemNum; ++i)
            {
                string key = "DropOutItemId_" + (i + 1).ToString();
                m_ItemId.Add(DBCUtil.ExtractNumeric<int>(node, key, 0, false));
                key = "ItemProbality_" + (i + 1).ToString();
                m_ItemProbality.Add(DBCUtil.ExtractNumeric<float>(node, key, 0, false));
                key = "ItemFactor_" + (i + 1).ToString();
                m_ItemFactor.Add(DBCUtil.ExtractNumeric<float>(node, key, 0, false));
            }
            return true;
        }
        public int GetId()
        {
            return m_BossNum;
        }
    }
    public class AttemptTollgateConfigProvider
    {
        public DataDictionaryMgr<AttemptTollgateConfig> AttemptTollgateConfigMgr
        {
            get { return m_AttemptTollgateConfigMgr; }
        }
        public AttemptTollgateConfig GetDataById(int id)
        {
            return m_AttemptTollgateConfigMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_AttemptTollgateConfigMgr.GetDataCount();
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_AttemptTollgateConfigMgr.CollectDataFromDBC(file, root, bytes);
        }

        private DataDictionaryMgr<AttemptTollgateConfig> m_AttemptTollgateConfigMgr = new DataDictionaryMgr<AttemptTollgateConfig>();
        public static AttemptTollgateConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static AttemptTollgateConfigProvider s_Instance = new AttemptTollgateConfigProvider();
    }
}
