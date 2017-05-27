using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class GowPrizeItem
    {
        public int ItemId;
        public int ItemNum;
    }
    public sealed class GowPrizeConfig : IData
    {
        public int FitBegin;
        public int FitEnd;
        public int Money;
        public int Gold;
        public int ItemCount;
        public List<GowPrizeItem> Items;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            FitBegin = DBCUtil.ExtractNumeric<int>(node, "FitBegin", 0, true);
            FitEnd = DBCUtil.ExtractNumeric<int>(node, "FitEnd", 0, true);
            Money = DBCUtil.ExtractNumeric<int>(node, "Money", 0, false);
            Gold = DBCUtil.ExtractNumeric<int>(node, "Gold", 0, false);
            ItemCount = DBCUtil.ExtractNumeric<int>(node, "ItemCount", 0, true);
            Items = new List<GowPrizeItem>();
            for (int i = 1; i <= ItemCount; i++)
            {
                string item_str = DBCUtil.ExtractString(node, "Item_" + i, "", false);
                string[] item_str_array = item_str.Split('|');
                if (item_str_array.Length >= 2)
                {
                    GowPrizeItem item = new GowPrizeItem();
                    item.ItemId = int.Parse(item_str_array[0]);
                    item.ItemNum = int.Parse(item_str_array[1]);
                    Items.Add(item);
                }
            }
            return true;
        }
        public int GetId()
        {
            return FitBegin;
        }
    }

    public sealed class GowTimeConfig : IData
    {
        public enum TimeTypeEnum : int
        {
            PrizeTime = 1,
            MatchTime,
        }

        public int m_Id;
        public int m_Type;
        public int m_StartHour;
        public int m_StartMinute;
        public int m_StartSecond;
        public int m_EndHour;
        public int m_EndMinute;
        public int m_EndSecond;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "ID", 0, true);
            m_Type = DBCUtil.ExtractNumeric<int>(node, "Type", 1, true);
            m_StartHour = DBCUtil.ExtractNumeric<int>(node, "StartHour", 0, true);
            m_StartMinute = DBCUtil.ExtractNumeric<int>(node, "StartMinute", 0, true);
            m_StartSecond = DBCUtil.ExtractNumeric<int>(node, "StartSecond", 0, true);
            m_EndHour = DBCUtil.ExtractNumeric<int>(node, "EndHour", 0, true);
            m_EndMinute = DBCUtil.ExtractNumeric<int>(node, "EndMinute", 0, true);
            m_EndSecond = DBCUtil.ExtractNumeric<int>(node, "EndSecond", 0, true);

            return true;
        }

        public int GetId()
        {
            return m_Id;
        }
    }

    public sealed class GowConstConfig : IData
    {
        public int m_TotalPoint;
        public float m_HighRate;
        public float m_LowRate;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_TotalPoint = DBCUtil.ExtractNumeric<int>(node, "TotalPoint", 0, true);
            m_HighRate = DBCUtil.ExtractNumeric<float>(node, "HighRate", 0.0f, true);
            m_LowRate = DBCUtil.ExtractNumeric<float>(node, "LowRate", 0.0f, true);
            return true;
        }

        public int GetId()
        {
            return m_TotalPoint;
        }
    }

    public sealed class GowFormulaConfig : IData
    {
        public enum FormulaNameEnum : int
        {
            Upper = 1,
            Lower,
            K2_1,
            K2_2,
            K1,
            K3,
            TC,
        }

        public int m_Index;
        public string m_Name;
        public int m_Value;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Index = DBCUtil.ExtractNumeric<int>(node, "Index", 0, true);
            m_Name = DBCUtil.ExtractNumeric<string>(node, "Name", "", true);
            m_Value = DBCUtil.ExtractNumeric<int>(node, "Value", 0, true);
            return true;
        }

        public int GetId()
        {
            return m_Index;
        }
    }

    public sealed class GowRankConfig : IData
    {
        public int m_RankId;
        public string m_RankName;
        public int m_Point;
        public bool m_IsTriggerAdvance;
        public int m_TotalMatches;
        public int m_WinMatches;
        public bool m_IsTriggerReduced;
        public int m_LossesMatches;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_RankId = DBCUtil.ExtractNumeric<int>(node, "RankId", 0, true);
            m_RankName = DBCUtil.ExtractString(node, "RankName", "", true);
            m_Point = DBCUtil.ExtractNumeric<int>(node, "Point", 100, false);
            m_IsTriggerAdvance = DBCUtil.ExtractBool(node, "IsTriggerAdvance", true, true);
            m_TotalMatches = DBCUtil.ExtractNumeric<int>(node, "TotalMatches", 3, false);
            m_WinMatches = DBCUtil.ExtractNumeric<int>(node, "WinMatches", 2, false);
            m_IsTriggerReduced = DBCUtil.ExtractBool(node, "IsTriggerReduced", false, true);
            m_LossesMatches = DBCUtil.ExtractNumeric<int>(node, "LossesMatches", 6, false);
            return true;
        }
        public int GetId()
        {
            return m_RankId;
        }
    }

    public sealed class GowConfigProvider
    {
        public DataListMgr<GowPrizeConfig> GowPrizeConfigMgr
        {
            get { return m_GowPrizeConfigMgr; }
        }
        public DataListMgr<GowTimeConfig> GowTimeConfigMgr
        {
            get { return m_GowTimeConfigMgr; }
        }
        public DataListMgr<GowConstConfig> GowConstConfigMgr
        {
            get { return m_GowConstConfigMgr; }
        }
        public DataListMgr<GowRankConfig> GowRankConfigMgr
        {
            get { return m_GowRankConfigMgr; }
        }
        public DataDictionaryMgr<GowFormulaConfig> GowFormulaConfigMgr
        {
            get { return m_GowFormulaConfigMgr; }
        }

        public GowPrizeConfig FindGowPrizeConfig(int rank)
        {
            foreach (GowPrizeConfig rule in GowPrizeConfigMgr.GetData())
            {
                if (rank >= rule.FitBegin && rank < rule.FitEnd)
                {
                    return rule;
                }
            }
            return null;
        }
        public GowConstConfig FindGowConstConfig(int point)
        {
            int ct = m_GowConstConfigMgr.GetDataCount();
            List<GowConstConfig> consts = m_GowConstConfigMgr.GetData();
            int st = 0;
            int ed = ct - 1;
            for (int findCt = 0; findCt < ct && st < ed; ++findCt)
            {
                int mid = (st + ed) / 2;
                int ranking = consts[mid].m_TotalPoint;
                if (ranking > point)
                {
                    if (mid > 0 && consts[mid - 1].m_TotalPoint <= point)
                    {
                        return consts[mid];
                    }
                    else
                    {
                        ed = mid;
                    }
                }
                else if (ranking == point)
                {
                    return consts[mid];
                }
                else
                {
                    if (mid < ed && consts[mid + 1].m_TotalPoint > point)
                    {
                        return consts[mid];
                    }
                    else
                    {
                        st = mid;
                    }
                }
            }
            return null;
        }
        public GowFormulaConfig GetGowFormulaConfig(int id)
        {
            return m_GowFormulaConfigMgr.GetDataById(id);
        }

        public void LoadForClient()
        {
            m_GowPrizeConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_GowPrizeConfig, "GowPrize");
            m_GowTimeConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_GowTimeConfig, "GowTime");
            m_GowRankConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_GowRankConfig, "GowRank");
        }
        public void LoadForServer()
        {
            m_GowPrizeConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_GowPrizeConfig, "GowPrize");
            m_GowTimeConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_GowTimeConfig, "GowTime");
            m_GowConstConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_GowConstConfig, "GowConst");
            m_GowRankConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_GowRankConfig, "GowRank");
            m_GowFormulaConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_GowFormulaConfig, "GowFormula");
        }

        private DataListMgr<GowPrizeConfig> m_GowPrizeConfigMgr = new DataListMgr<GowPrizeConfig>();
        private DataListMgr<GowTimeConfig> m_GowTimeConfigMgr = new DataListMgr<GowTimeConfig>();
        private DataListMgr<GowConstConfig> m_GowConstConfigMgr = new DataListMgr<GowConstConfig>();
        private DataListMgr<GowRankConfig> m_GowRankConfigMgr = new DataListMgr<GowRankConfig>();
        private DataDictionaryMgr<GowFormulaConfig> m_GowFormulaConfigMgr = new DataDictionaryMgr<GowFormulaConfig>();

        public static GowConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static GowConfigProvider s_Instance = new GowConfigProvider();
    }
}
