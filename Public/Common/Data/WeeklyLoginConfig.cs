using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum ActivityTypeEnum
    {
        WEEKLY_LOGIN_REWARD = 1,
    }
    public class WeeklyLoginConfig : IData
    {
        public int Id;
        public int ActivityType;
        public DateTime StartTime;
        public DateTime EndTime;
        public int SumDay;
        public List<int> RewardItemIdList;
        public List<int> RewardItemNumList;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            ActivityType = DBCUtil.ExtractNumeric<int>(node, "ActivityType", 0, false);
            SumDay = DBCUtil.ExtractNumeric<int>(node, "SumDay", 0, false);
            StartTime = DateTime.ParseExact(DBCUtil.ExtractString(node, "StartTime", "", true), "yyyyMMdd", null);
            EndTime = DateTime.ParseExact(DBCUtil.ExtractString(node, "EndTime", "", true), "yyyyMMdd", null);
            RewardItemIdList = new List<int>();
            RewardItemNumList = new List<int>();
            for (int i = 0; i < SumDay; ++i)
            {
                RewardItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "DayRewardItemId" + i, 0, false));
                RewardItemNumList.Add(DBCUtil.ExtractNumeric<int>(node, "DayRewardItemCount" + i, 0, false));
            }
            return true;
        }

        public int GetId()
        {
            return Id;
        }
    }

    public class WeeklyLoginConfigProvider
    {
        public WeeklyLoginConfig GetDataByType(ActivityTypeEnum type)
        {
            return m_ActivityConfigMgr.GetDataById((int)type);
        }

        public void GetRewardByDay(out int itemId, out int itemNum)
        {
            itemId = 0;
            itemNum = 0;
            WeeklyLoginConfig config = GetDataByType(ActivityTypeEnum.WEEKLY_LOGIN_REWARD);
            if (null != config)
            {
                TimeSpan deltaTime = DateTime.Now - config.StartTime;
                int day = deltaTime.Days;
                if (config.RewardItemIdList.Count > day && config.RewardItemNumList.Count > day)
                {
                    itemId = config.RewardItemIdList[day];
                    itemNum = config.RewardItemNumList[day];
                }
            }
        }
        public bool IsUnderProgress()
        {
            WeeklyLoginConfig config = GetDataByType(ActivityTypeEnum.WEEKLY_LOGIN_REWARD);
            if (null != config && config.StartTime <= DateTime.Now && config.EndTime > DateTime.Now)
            {
                return true;
            }
            return false;
        }
        public int GetTodayIndex()
        {
            if (IsUnderProgress())
            {
                WeeklyLoginConfig config = GetDataByType(ActivityTypeEnum.WEEKLY_LOGIN_REWARD);
                if (null != config)
                {
                    TimeSpan deltaTime = DateTime.Now - config.StartTime;
                    return deltaTime.Days;
                }
            }
            return -1;
        }
        public void Load(string file, string root)
        {
            m_ActivityConfigMgr.CollectDataFromDBC(file, root);
        }
        public static WeeklyLoginConfigProvider Instance
        {
            get { return s_Instance; }
        }

        private DataDictionaryMgr<WeeklyLoginConfig> m_ActivityConfigMgr = new DataDictionaryMgr<WeeklyLoginConfig>();
        private static WeeklyLoginConfigProvider s_Instance = new WeeklyLoginConfigProvider();
    }
}
