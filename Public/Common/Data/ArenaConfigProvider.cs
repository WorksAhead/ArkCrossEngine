using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class SimpleTime
    {
        public int Hour;
        public int Minutes;
    }
    public sealed class ArenaBaseConfig : IData
    {
        public int Id;
        public int MaxRank;
        public int[] MaxParterLimit;
        public int MaxBattleCount;
        public long BattleCd;
        public long MaxFightTime;
        public int MaxHistoryCount;
        public int AIId;
        public SimpleTime BattleCountResetTime;
        public SimpleTime PrizeSettlementTime;
        public SimpleTime PrizePresentTime;
        public int QueryTopRankCount;
        public int QueryFrontRankCount;
        public int QueryBehindRankCount;
        public long InfoRefreshTime;
        public long TopRankRefreshTime;
        public long MatchGroupRefreshTime;
        public int PrizeRetainDays;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric(node, "Id", 0, true);
            MaxRank = DBCUtil.ExtractNumeric(node, "MaxRank", 0, true);
            List<int> num_list = new List<int>();
            num_list = DBCUtil.ExtractNumericList(node, "MaxParterLimit", 0, true);
            if (num_list != null)
            {
                MaxParterLimit = num_list.ToArray();
            }
            MaxBattleCount = DBCUtil.ExtractNumeric(node, "MaxBattleCount", 0, true);
            BattleCd = DBCUtil.ExtractNumeric(node, "BattleCd", 0, true);
            MaxFightTime = DBCUtil.ExtractNumeric(node, "MaxFightTime", 30000, true);
            MaxHistoryCount = DBCUtil.ExtractNumeric(node, "MaxHistoryCount", 10, true);
            AIId = DBCUtil.ExtractNumeric(node, "AIId", 0, true);
            BattleCountResetTime = ConvertToSimpleTime(DBCUtil.ExtractString(node, "BattleCountResetTime", "00:00", true));
            PrizeSettlementTime = ConvertToSimpleTime(DBCUtil.ExtractString(node, "PrizeSettlementTime", "00:00", true));
            PrizePresentTime = ConvertToSimpleTime(DBCUtil.ExtractString(node, "PrizePresentTime", "00:00", true));
            QueryTopRankCount = DBCUtil.ExtractNumeric(node, "QueryTopRank", 10, true);
            QueryFrontRankCount = DBCUtil.ExtractNumeric(node, "QueryFrontRank", 5, true);
            QueryBehindRankCount = DBCUtil.ExtractNumeric(node, "QueryFehindRank", 5, true);
            InfoRefreshTime = DBCUtil.ExtractNumeric<long>(node, "InfoRefreshTime", 600000, true);
            TopRankRefreshTime = DBCUtil.ExtractNumeric<long>(node, "RankRefreshTime", 600000, true);
            MatchGroupRefreshTime = DBCUtil.ExtractNumeric<long>(node, "MatchGroupRefreshTime", 600000, true);
            PrizeRetainDays = DBCUtil.ExtractNumeric(node, "PrizeRetainDays", 7, true);
            return true;
        }

        public SimpleTime ConvertToSimpleTime(string time_str)
        {
            SimpleTime result = new SimpleTime();
            string[] hour_minute_array = time_str.Split(':');
            int hour = 0;
            int minutes = 0;
            try
            {
                if (hour_minute_array.Length >= 1)
                {
                    hour = int.Parse(hour_minute_array[0]);
                }
                if (hour_minute_array.Length >= 2)
                {
                    minutes = int.Parse(hour_minute_array[1]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ConvertToSimpleTime: parse error: " + ex.Message);
            }
            result.Hour = hour;
            result.Minutes = minutes;
            return result;
        }

        public int GetId()
        {
            return Id;
        }
    }

    public sealed class ArenaMatchRuleConfig : IData
    {
        public int FitBegin = 0;
        public int FitEnd = 0;

        public int OneBegin = 0;
        public int TwoBegin = 0;
        public int ThreeBegin = 0;
        public int ThreeEnd = 0;

        public int GetId()
        {
            return FitBegin;
        }

        public bool CollectDataFromDBC(DBC_Row node)
        {
            int fitBegin = DBCUtil.ExtractNumeric(node, "FitBegin", 0, true);
            int fitEnd = DBCUtil.ExtractNumeric(node, "FitEnd", 0, true);
            FitBegin = Math.Min(fitBegin, fitEnd);
            FitEnd = Math.Max(fitBegin, fitEnd);
            List<int> phases = new List<int>();
            int point = DBCUtil.ExtractNumeric(node, "OneBegin", 0, true);
            phases.Add(point);
            point = DBCUtil.ExtractNumeric(node, "TwoBegin", 0, true);
            phases.Add(point);
            point = DBCUtil.ExtractNumeric(node, "ThreeBegin", 0, true);
            phases.Add(point);
            point = DBCUtil.ExtractNumeric(node, "ThreeEnd", 0, true);
            phases.Add(point);
            phases.Sort();
            if (phases.Count >= 4)
            {
                OneBegin = phases[3];
                TwoBegin = phases[2];
                ThreeBegin = phases[1];
                ThreeEnd = phases[0];
            }
            return true;
        }
    }

    public sealed class ArenaRobotConfig : IData
    {
        public int Rank;
        public string NickName;
        public int FightScore;
        public int HeroId;
        public int Level;
        public List<ArenaItemInfo> EquipInfo = new List<ArenaItemInfo>();
        public List<ArenaXSoulInfo> XSoulInfo = new List<ArenaXSoulInfo>();
        public List<ArenaPartnerInfo> PartnerInfo = new List<ArenaPartnerInfo>();
        public List<ArenaSkillInfo> SkillInfo = new List<ArenaSkillInfo>();

        public int GetId()
        {
            return Rank;
        }

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Rank = DBCUtil.ExtractNumeric(node, "Rank", 0, true);
            NickName = DBCUtil.ExtractString(node, "NickName", "", true);
            HeroId = DBCUtil.ExtractNumeric(node, "HeroId", 0, true);
            Level = DBCUtil.ExtractNumeric(node, "Level", 0, true);
            FightScore = DBCUtil.ExtractNumeric(node, "FightScore", 0, true);
            List<string> equips_list = DBCUtil.ExtractStringList(node, "EquipInfo", "", false);
            foreach (string e in equips_list)
            {
                List<int> array = ConvertToNumberList(e, '*');
                ArenaItemInfo item = CreateArenaItem(array);
                if (item != null)
                {
                    EquipInfo.Add(item);
                }
            }
            List<string> xsoul_list = DBCUtil.ExtractStringList(node, "XSoulInfo", "", false);
            foreach (string e in xsoul_list)
            {
                List<int> array = ConvertToNumberList(e, '*');
                ArenaXSoulInfo item = CreateArenaXSoulInfo(array);
                if (item != null)
                {
                    XSoulInfo.Add(item);
                }
            }
            List<string> partner_list = DBCUtil.ExtractStringList(node, "PartnerInfo", "", false);
            foreach (string e in partner_list)
            {
                List<int> array = ConvertToNumberList(e, '*');
                ArenaPartnerInfo partner = CreateArenaPartnerInfo(array);
                if (partner != null)
                {
                    PartnerInfo.Add(partner);
                }
            }
            List<string> skill_list = DBCUtil.ExtractStringList(node, "SkillInfo", "", false);
            foreach (string e in skill_list)
            {
                List<int> array = ConvertToNumberList(e, '*');
                ArenaSkillInfo skill = CreateArenaSkillInfo(array);
                if (skill != null)
                {
                    SkillInfo.Add(skill);
                }
            }
            return true;
        }

        private ArenaSkillInfo CreateArenaSkillInfo(List<int> array)
        {
            ArenaSkillInfo item = null;
            if (array.Count > 0)
            {
                item = new ArenaSkillInfo();
                item.Id = array[0];
                if (array.Count >= 2)
                {
                    item.Level = array[1];
                }
                if (array.Count >= 3)
                {
                    item.EquipPos = array[2];
                }
            }
            return item;
        }

        private ArenaPartnerInfo CreateArenaPartnerInfo(List<int> array)
        {
            ArenaPartnerInfo item = null;
            if (array.Count > 0)
            {
                item = new ArenaPartnerInfo();
                item.id = array[0];
                if (array.Count >= 2)
                {
                    item.AdditionLevel = array[1];
                }
                if (array.Count >= 3)
                {
                    item.SkillStage = array[2];
                }
            }
            return item;
        }

        public ArenaXSoulInfo CreateArenaXSoulInfo(List<int> array)
        {
            ArenaXSoulInfo item = null;
            if (array.Count > 0)
            {
                item = new ArenaXSoulInfo();
                item.ItemId = array[0];
                if (array.Count >= 2)
                {
                    item.Level = array[1];
                }
                if (array.Count >= 3)
                {
                    item.Experience = array[2];
                }
                if (array.Count >= 4)
                {
                    item.ModelLevel = array[3];
                }
            }
            return item;
        }

        public ArenaItemInfo CreateArenaItem(List<int> array)
        {
            ArenaItemInfo item = null;
            if (array.Count > 0)
            {
                item = new ArenaItemInfo();
                item.ItemId = array[0];
                if (array.Count >= 2)
                {
                    item.Level = array[1];
                }
                if (array.Count >= 3)
                {
                    item.AppendProperty = array[2];
                }
            }
            return item;
        }

        public List<int> ConvertToNumberList(string str, char split_ch)
        {
            List<int> result = new List<int>();
            string[] str_array = str.Split(split_ch);
            foreach (string s in str_array)
            {
                result.Add(int.Parse(s));
            }
            return result;
        }
    }

    public class PrizeItemConfig
    {
        public int ItemId;
        public int Level;
        public int ItemNum;
    }

    public sealed class ArenaPrizeConfig : IData
    {
        public int FitBegin;
        public int FitEnd;
        public int Money;
        public int Gold;
        public int ItemCount;
        public List<PrizeItemConfig> Items;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            FitBegin = DBCUtil.ExtractNumeric<int>(node, "FitBegin", 0, true);
            FitEnd = DBCUtil.ExtractNumeric<int>(node, "FitEnd", 0, true);
            Money = DBCUtil.ExtractNumeric<int>(node, "Money", 0, false);
            Gold = DBCUtil.ExtractNumeric<int>(node, "Gold", 0, false);
            ItemCount = DBCUtil.ExtractNumeric<int>(node, "ItemCount", 0, true);
            Items = new List<PrizeItemConfig>();
            for (int i = 1; i <= ItemCount; i++)
            {
                string item_str = DBCUtil.ExtractString(node, "Item_" + i, "", false);
                string[] item_str_array = item_str.Split('|');
                if (item_str_array.Length >= 3)
                {
                    PrizeItemConfig item = new PrizeItemConfig();
                    item.ItemId = int.Parse(item_str_array[0]);
                    item.Level = int.Parse(item_str_array[1]);
                    item.ItemNum = int.Parse(item_str_array[2]);
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

    public sealed class ArenaBuyFightCountConfig : IData
    {
        public int BuyTime;
        public int RequireVipLevel;
        public int Cost;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            BuyTime = DBCUtil.ExtractNumeric<int>(node, "BuyTime", 0, true);
            RequireVipLevel = DBCUtil.ExtractNumeric<int>(node, "RequireVipLevel", 0, true);
            Cost = DBCUtil.ExtractNumeric<int>(node, "Cost", 50, true);
            return true;
        }

        public int GetId()
        {
            return BuyTime;
        }
    }


    public sealed class ArenaConfigProvider
    {
        public DataDictionaryMgr<ArenaBaseConfig> BaseConfig
        {
            get { return m_BaseConfigMgr; }
        }
        public ArenaBaseConfig GetBaseConfigById(int id)
        {
            return m_BaseConfigMgr.GetDataById(id);
        }
        public DataDictionaryMgr<ArenaPrizeConfig> PrizeConfig
        {
            get { return m_PrizeConfigMgr; }
        }

        public DataDictionaryMgr<ArenaRobotConfig> RobotConfig
        {
            get { return m_RobotConfigMgr; }
        }

        public DataDictionaryMgr<ArenaMatchRuleConfig> MatchRuleConfig
        {
            get { return m_MatchRuleConfigMgr; }
        }

        public DataDictionaryMgr<ArenaBuyFightCountConfig> BuyFightCountConfig
        {
            get { return m_BuyFightCountConfigMgr; }
        }

        public void LoadServerConfig()
        {
            m_BaseConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_ArenaBaseConfig, "ArenaBase");
            m_PrizeConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_ArenaPrizeConfig, "ArenaPrize");
            m_RobotConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_ArenaRobotConfig, "ArenaRobot");
            m_MatchRuleConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_ArenaMatchRuleConfig, "ArenaMatchRule");
            m_BuyFightCountConfigMgr.CollectDataFromDBC(FilePathDefine_Server.C_ArenaBuyFightCountConfig, "ArenaBuyFightCount");
        }

        public void LoadClientConfig()
        {
            m_BaseConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_ArenaBaseConfig, "ArenaBase");
            m_PrizeConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_ArenaPrizeConfig, "ArenaPrize");
            m_BuyFightCountConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_ArenaBuyFightCountConfig, "ArenaBuyFightCount");
        }
        public void LoadClientConfigBase(byte[] bytes)
        {
            m_BaseConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_ArenaBaseConfig, "ArenaBase", bytes);
        }
        public void LoadClientConfigPrize(byte[] bytes)
        {
            m_PrizeConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_ArenaPrizeConfig, "ArenaPrize", bytes);
        }
        public void LoadClientConfigFight(byte[] bytes)
        {
            m_BuyFightCountConfigMgr.CollectDataFromDBC(FilePathDefine_Client.C_ArenaBuyFightCountConfig, "ArenaBuyFightCount", bytes);
        }
        DataDictionaryMgr<ArenaBaseConfig> m_BaseConfigMgr = new DataDictionaryMgr<ArenaBaseConfig>();
        DataDictionaryMgr<ArenaPrizeConfig> m_PrizeConfigMgr = new DataDictionaryMgr<ArenaPrizeConfig>();
        DataDictionaryMgr<ArenaRobotConfig> m_RobotConfigMgr = new DataDictionaryMgr<ArenaRobotConfig>();
        DataDictionaryMgr<ArenaMatchRuleConfig> m_MatchRuleConfigMgr = new DataDictionaryMgr<ArenaMatchRuleConfig>();
        DataDictionaryMgr<ArenaBuyFightCountConfig> m_BuyFightCountConfigMgr = new DataDictionaryMgr<ArenaBuyFightCountConfig>();

        public static ArenaConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static ArenaConfigProvider s_Instance = new ArenaConfigProvider();
    }
}
