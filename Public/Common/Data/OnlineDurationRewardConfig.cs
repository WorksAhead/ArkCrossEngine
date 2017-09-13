using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    class OnlineDurationRewardConfig : IData
    {
        public int Id;
        public int RewardCount;
        public List<int> TimeList;
        public List<int> ItemIdList;
        public List<int> ItemNumList;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            RewardCount = DBCUtil.ExtractNumeric<int>(node, "RewardCount", 0, true);
            TimeList = new List<int>();
            ItemIdList = new List<int>();
            ItemNumList = new List<int>();
            for (int i = 0; i < RewardCount; ++i)
            {
                TimeList.Add(DBCUtil.ExtractNumeric<int>(node, "Time" + i, 0, true));
                ItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemId" + i, 0, true));
                ItemNumList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemNum" + i, 0, true));
            }
            return true;
        }

        public int GetId()
        {
            return Id;
        }
    }
    public class OnlineDurationRewardConfigProvider
    {
        public void GetOnlineRewardByCount(int count, out int duration, out int itemId, out int itemNum)
        {
            duration = 0;
            itemId = 0;
            itemNum = 0;
            OnlineDurationRewardConfig config = GetOnlineRewardConfigByDay((int)DateTime.Now.DayOfWeek);
            if (null != config)
            {
                duration = config.TimeList[count];
                itemId = config.ItemIdList[count];
                itemNum = config.ItemNumList[count];
            }
        }
        private OnlineDurationRewardConfig GetOnlineRewardConfigByDay(int day)
        {
            return m_OnlineDurationConfigMgr.GetDataById(day);
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_OnlineDurationConfigMgr.CollectDataFromDBC(file, root, bytes);
        }
        public static OnlineDurationRewardConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<OnlineDurationRewardConfig> m_OnlineDurationConfigMgr = new DataDictionaryMgr<OnlineDurationRewardConfig>();
        private static OnlineDurationRewardConfigProvider s_Instance = new OnlineDurationRewardConfigProvider();
    }
}
