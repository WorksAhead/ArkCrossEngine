using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    [Serializable]
    public class VipConfig : IData
    {
        public int m_VipLevel;
        public int m_DiamondsRequired;
        public int m_Diamond;
        public int m_Gold;
        public int m_Exp;
        public int m_Shell;
        public int m_Honor;
        public int m_ItemCount;
        public List<int> m_ItemIdList;
        public List<int> m_ItemNumList;
        public int m_Stamina;
        public int m_BuyGold;
        public int m_TreasureTime;
        public int m_GoldCombatTime;
        public int m_PvpTime;
        public int m_BossCombatTime;
        public int m_EliteTime;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_VipLevel = DBCUtil.ExtractNumeric<int>(node, "VipLevel", 0, true);
            m_DiamondsRequired = DBCUtil.ExtractNumeric<int>(node, "DiamondsRequired", 0, true);
            m_Diamond = DBCUtil.ExtractNumeric<int>(node, "Diamond", 0, false);
            m_Gold = DBCUtil.ExtractNumeric<int>(node, "Gold", 0, false);
            m_Exp = DBCUtil.ExtractNumeric<int>(node, "Exp", 0, false);
            m_Shell = DBCUtil.ExtractNumeric<int>(node, "Shell", 0, false);
            m_Honor = DBCUtil.ExtractNumeric<int>(node, "Honor", 0, false);
            m_ItemCount = DBCUtil.ExtractNumeric<int>(node, "ItemCount", 0, false);
            m_ItemIdList = new List<int>();
            m_ItemNumList = new List<int>();
            for (int i = 0; i < m_ItemCount; ++i)
            {
                m_ItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemId_" + i, 0, false));
                m_ItemNumList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemNum_" + i, 0, false));
            }
            m_Stamina = DBCUtil.ExtractNumeric<int>(node, "BuyStaminaTime", 0, true);
            m_BuyGold = DBCUtil.ExtractNumeric<int>(node, "BuyGoldTime", 0, true);
            m_TreasureTime = DBCUtil.ExtractNumeric<int>(node, "TreasureTime", 0, true);
            m_GoldCombatTime = DBCUtil.ExtractNumeric<int>(node, "GoldCombatTime", 0, true);
            m_BossCombatTime = DBCUtil.ExtractNumeric<int>(node, "BossCombatTime", 0, true);
            m_PvpTime = DBCUtil.ExtractNumeric<int>(node, "PvPTime", 0, true);
            m_EliteTime = DBCUtil.ExtractNumeric<int>(node, "EliteTime", 0, true);

            return true;
        }
        public int GetId()
        {
            return m_VipLevel;
        }
    }
    public class VipConfigProvider
    {
        public DataDictionaryMgr<VipConfig> VipConfigMgr
        {
            get { return m_VipConfigMgr; }
        }
        public VipConfig GetDataById(int id)
        {
            return m_VipConfigMgr.GetDataById(id);
        }
        public int GetDataCount()
        {
            return m_VipConfigMgr.GetDataCount();
        }
        public void Load(string file, string root)
        {
            m_VipConfigMgr.CollectDataFromDBC(file, root);
        }
        private DataDictionaryMgr<VipConfig> m_VipConfigMgr = new DataDictionaryMgr<VipConfig>();
        public static VipConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static VipConfigProvider s_Instance = new VipConfigProvider();
    }
}
