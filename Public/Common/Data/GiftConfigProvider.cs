using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class GiftConfig : IData
    {
        public int GiftId;
        public string GiftName;
        public string Description;
        public int ItemCount;
        public List<int> ItemIdList;
        public List<int> ItemNumList;

        public bool CollectDataFromDBC(DBC_Row node)
        {
            GiftId = DBCUtil.ExtractNumeric<int>(node, "GiftId", 0, true);
            GiftName = DBCUtil.ExtractNumeric<string>(node, "GiftName", "", false);
            Description = DBCUtil.ExtractNumeric<string>(node, "Description", "", false);
            ItemCount = DBCUtil.ExtractNumeric<int>(node, "ItemCount", 0, false);
            ItemIdList = new List<int>();
            ItemNumList = new List<int>();
            for (int i = 0; i < ItemCount; ++i)
            {
                ItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemId" + i, 0, false));
                ItemNumList.Add(DBCUtil.ExtractNumeric<int>(node, "ItemNum" + i, 0, false));
            }
            return true;
        }
        public int GetId()
        {
            return GiftId;
        }
    }

    public class GiftConfigProvider
    {
        public GiftConfig GetDataById(int id)
        {
            return m_GiftConfigMgr.GetDataById(id);
        }
        public void Load(string file, string root, byte[] bytes)
        {
            m_GiftConfigMgr.CollectDataFromDBC(file, root, bytes);
        }
        public static GiftConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private DataDictionaryMgr<GiftConfig> m_GiftConfigMgr = new DataDictionaryMgr<GiftConfig>();
        private static GiftConfigProvider s_Instance = new GiftConfigProvider();
    }
}
