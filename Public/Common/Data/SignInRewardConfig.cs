using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class SignInRewardConfig : IData
    {
        public int Month;
        public List<int> ItemIdList;
        public List<int> ItemNumList;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            Month = DBCUtil.ExtractNumeric<int>(node, "Month", 0, true);
            ItemIdList = new List<int>();
            ItemNumList = new List<int>();
            for (int i = 0; i < c_MaxItemNum; ++i)
            {
                ItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemId" + i, 0, false));
                ItemNumList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemNum" + i, 0, false));
            }
            return true;
        }
        public int GetId()
        {
            return Month;
        }
        public int c_MaxItemNum = 31;
        public int C_MaxMonth = 12;
    }

    public class SignInRewardConfigProvider
    {
        public SignInRewardConfig GetDataById(int id)
        {
            return m_SignInRewardConfigMgr.GetDataById(id);
        }
        public bool GetDataByDate(int month, int day, out int itemId, out int itemCount)
        {
            itemId = 0;
            itemCount = 0;
            SignInRewardConfig src = GetDataById(month);
            if (null != src)
            {
                if (day > 0 && day <= src.c_MaxItemNum)
                {
                    itemId = src.ItemIdList[day - 1];
                    itemCount = src.ItemNumList[day - 1];
                    return true;
                }
            }
            return false;
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_SignInRewardConfigMgr.CollectDataFromDBC(file, root, bytes);
        }
        public static SignInRewardConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<SignInRewardConfig> m_SignInRewardConfigMgr = new DataDictionaryMgr<SignInRewardConfig>();
        private static SignInRewardConfigProvider s_Instance = new SignInRewardConfigProvider();
    }
}
