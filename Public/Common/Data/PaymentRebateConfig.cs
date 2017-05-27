using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class PaymentRebateConfig : IData
    {
        public int Id;
        public int Group;
        public string Describe;
        public DateTime AnnounceTime;
        public DateTime StartTime;
        public DateTime EndTime;
        public int TotalDiamond;
        public int Diamond;
        public int Gold;
        public int Exp;
        public int ItemCount;
        public List<int> ItemIdList;
        public List<int> ItemNumList;
        public string Test;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            Group = DBCUtil.ExtractNumeric<int>(node, "Group", 0, true);
            Describe = DBCUtil.ExtractString(node, "Describe", "", false);
            Test = DBCUtil.ExtractString(node, "AnnounceTime", "", true);
            AnnounceTime = DateTime.ParseExact(DBCUtil.ExtractString(node, "AnnounceTime", "", true), "yyyy/M/d H:mm", null);
            StartTime = DateTime.ParseExact(DBCUtil.ExtractString(node, "StartTime", "", true), "yyyy/M/d H:mm", null);
            EndTime = DateTime.ParseExact(DBCUtil.ExtractString(node, "EndTime", "", true), "yyyy/M/d H:mm", null);
            TotalDiamond = DBCUtil.ExtractNumeric<int>(node, "TotalDiamond", 0, false);
            Gold = DBCUtil.ExtractNumeric<int>(node, "Gold", 0, false);
            Exp = DBCUtil.ExtractNumeric<int>(node, "Exp", 0, false);
            ItemCount = DBCUtil.ExtractNumeric<int>(node, "ItemCount", 0, false);
            ItemIdList = new List<int>();
            ItemNumList = new List<int>();
            for (int i = 0; i < ItemCount; ++i)
            {
                ItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemId_" + i, 0, false));
                ItemNumList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemNum_" + i, 0, false));
            }
            return true;
        }
        public int GetId()
        {
            return Id;
        }
        public void Dump()
        {
            LogSystem.Debug("Id = {0}, Group = {1}, Describe = {2}, AnnounceTime = {3}, StartTime = {4}, EndTime = {5}, TotalDiamond = {6}, Gold = {7}, Exp = {8}", Id, Group, Describe, AnnounceTime, StartTime, EndTime, TotalDiamond, Gold, Exp);
            for (int i = 0; i < ItemCount; ++i)
            {
                LogSystem.Debug("ItemId = {0}, ItemNum = {1}", ItemIdList[i], ItemNumList[i]);
            }
        }
    }

    public class PaymentRebateConfigProvider
    {
        public int GetDataCount()
        {
            return m_PaymentRebateConfigMgr.GetDataCount();
        }
        public List<PaymentRebateConfig> GetData()
        {
            return m_PaymentRebateConfigMgr.GetData();
        }
        public List<PaymentRebateConfig> GetUnderProgressData()
        {
            List<PaymentRebateConfig> allData = GetData();
            List<PaymentRebateConfig> result = new List<PaymentRebateConfig>();
            for (int i = 0; i < allData.Count; ++i)
            {
                if (allData[i].StartTime < DateTime.Now && allData[i].EndTime > DateTime.Now)
                {
                    result.Add(allData[i]);
                }
            }
            return result;
        }
        public List<PaymentRebateConfig> GetNeedAnnouncedData()
        {
            List<PaymentRebateConfig> allData = GetData();
            List<PaymentRebateConfig> result = new List<PaymentRebateConfig>();
            for (int i = 0; i < allData.Count; ++i)
            {
                if (allData[i].AnnounceTime < DateTime.Now && allData[i].EndTime > DateTime.Now)
                {
                    result.Add(allData[i]);
                }
            }
            return result;
        }
        public void Load(string file, string root)
        {
            m_PaymentRebateConfigMgr.CollectDataFromDBC(file, root);
        }
        public static PaymentRebateConfigProvider Instacne
        {
            get { return s_Instance; }
        }
        private DataListMgr<PaymentRebateConfig> m_PaymentRebateConfigMgr = new DataListMgr<PaymentRebateConfig>();
        private static PaymentRebateConfigProvider s_Instance = new PaymentRebateConfigProvider();
    }
}
