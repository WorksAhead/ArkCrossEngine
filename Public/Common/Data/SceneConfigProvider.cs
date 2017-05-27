using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum MatchSceneEnum : int
    {
        Gow = 3001,
        Attempt = 4011,
        Gold = 4021,
    }
    public enum SceneTypeEnum : int
    {
        TYPE_UNKNOWN = -1,
        TYPE_PURE_CLIENT_SCENE = 0,
        TYPE_PVE,
        TYPE_MULTI_PVE,
        TYPE_PVP,
        TYPE_SERVER_SELECT,
        TYPE_NUM,
    }
    public enum SceneSubTypeEnum : int
    {
        TYPE_UNKNOWN = -1,
        TYPE_STORY = 0,
        TYPE_ELITE,
        TYPE_EXPEDITION,
        TYPE_ATTEMPT,
        TYPE_GOLD,
        TYPE_PVAP,
        TYPE_PLATFORM_DEFENSE,
    }
    public enum GowMatchResult : int
    {
        TYPE_ALREADYATTEMPT = 0,
        TYPE_ALREADYGOW,
        TYPE_TIMEWRONG,
        TYPE_LEVELWRONG,
    }
    public class Data_SceneConfig : IData
    {
        public int m_Id;
        public int m_Type;
        public int m_SubType;
        public int m_Chapter;
        public int m_Order;
        public string m_ChapterName;
        public string m_SceneName;
        public string m_SceneDescription;
        public string m_UnitFile;
        public string m_SceneLogicFile;
        public List<string> m_StoryDslFile;
        public string m_ClientSceneFile;
        public string m_ObstacleFile;
        public string m_BlockInfoFile;
        public float m_TiledDataScale;
        public int m_DropId;
        public string m_BkMusic;
        public float m_BkMusicVolume;
        public string m_StoryMusic;
        public float m_StoryMusicVolume;
        public List<int> m_CompletedRewards;
        public int m_CompletedTime;
        public int m_CompletedHitCount;
        public int m_CostStamina;
        public int m_RecommendFighting;
        public Vector3[] m_ReachableSet;
        public string m_AtlasPath;
        public string m_SceneIconName;
        public float m_RecoverHpCoefficient;
        public float m_RecoverMpCoefficient;
        public int m_BossLinkId;
        public int m_RelatedMainCity;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            m_SceneName = DBCUtil.ExtractString(node, "Name", "", false);
            m_ChapterName = DBCUtil.ExtractString(node, "ChapterName", "", false);
            m_SceneDescription = DBCUtil.ExtractString(node, "Description", "", false);
            m_Type = DBCUtil.ExtractNumeric<int>(node, "Type", 0, true);
            m_SubType = DBCUtil.ExtractNumeric<int>(node, "SubType", 0, true);
            m_Chapter = DBCUtil.ExtractNumeric<int>(node, "Chapter", 0, true);
            m_Order = DBCUtil.ExtractNumeric<int>(node, "Order", 0, true);
            m_UnitFile = DBCUtil.ExtractString(node, "UnitFile", "", false);
            m_SceneLogicFile = DBCUtil.ExtractString(node, "SceneLogicFile", "", false);
            m_StoryDslFile = DBCUtil.ExtractStringList(node, "StoryDslFile", "", false);
            m_ClientSceneFile = DBCUtil.ExtractString(node, "ClientSceneFile", "", true);
            m_ObstacleFile = DBCUtil.ExtractString(node, "ObstacleFile", "", false);
            m_BlockInfoFile = DBCUtil.ExtractString(node, "BlockInfoFile", "", false);
            m_TiledDataScale = DBCUtil.ExtractNumeric<float>(node, "TiledDataScale", 0, false);
            m_DropId = DBCUtil.ExtractNumeric<int>(node, "DropId", 0, false);
            m_BkMusic = DBCUtil.ExtractString(node, "BkMusic", "", false);
            m_BkMusicVolume = DBCUtil.ExtractNumeric<float>(node, "BkMusicVolume", 1f, false);
            m_StoryMusic = DBCUtil.ExtractString(node, "StoryMusic", "", false);
            m_StoryMusicVolume = DBCUtil.ExtractNumeric<float>(node, "StoryMusicVolume", 1f, false);
            m_CompletedRewards = DBCUtil.ExtractNumericList<int>(node, "CompletedReward", 0, false);
            m_CompletedTime = DBCUtil.ExtractNumeric<int>(node, "CompletedTime", 0, false);
            m_CompletedHitCount = DBCUtil.ExtractNumeric<int>(node, "CompletedHitCount", 0, false);
            m_CostStamina = DBCUtil.ExtractNumeric<int>(node, "CostStamina", 0, false);
            m_RecommendFighting = DBCUtil.ExtractNumeric<int>(node, "RecommendFighting", 0, false);
            m_AtlasPath = DBCUtil.ExtractString(node, "AtlasPath", "", false);
            m_SceneIconName = DBCUtil.ExtractString(node, "SceneIconName", "", false);
            m_RecoverHpCoefficient = DBCUtil.ExtractNumeric<float>(node, "RecoverHpCoefficient", 1, false);
            m_RecoverMpCoefficient = DBCUtil.ExtractNumeric<float>(node, "RecoverMpCoefficient", 1, false);
            m_BossLinkId = DBCUtil.ExtractNumeric<int>(node, "BossLinkId", -1, false);
            List<float> coords = DBCUtil.ExtractNumericList<float>(node, "ReachableSet", 0, false);
            m_RelatedMainCity = DBCUtil.ExtractNumeric<int>(node, "RelatedMainCity", 0, false);
            if (coords.Count > 0)
            {
                m_ReachableSet = new Vector3[coords.Count / 2];
                for (int i = 0; i < coords.Count - 1; i += 2)
                {
                    m_ReachableSet[i / 2] = new Vector3(coords[i], 0, coords[i + 1]);
                }
            }
            else
            {
                m_ReachableSet = null;
            }
            return true;
        }

        public int GetCompletedRewardId(int starNum)
        {
            if (starNum > 0 && m_CompletedRewards.Count >= starNum)
            {
                return m_CompletedRewards[starNum - 1];
            }
            return -1;
        }
        public int GetId()
        {
            return m_Id;
        }
    }

    public class Data_SceneDropOut : IData
    {

        private enum HeroIdEnum
        {
            WARRIOR = 1,
            ROGUE = 2,
            MAGE = 3,
        }
        public int m_Id;
        public int m_GoldSum;
        public int m_GoldMin;
        public int m_GoldMax;
        public int m_Exp;
        public int m_Diamond;
        public int m_PartnerId;
        public int m_HpCount;
        public int m_HpPercent;
        public int m_MpCount;
        public int m_MpPercent;
        public string m_GoldModel;
        public string m_GoldParticle;
        public string m_HpModel;
        public string m_HpParticle;
        public string m_MpModel;
        public string m_MpParticle;
        public int m_ItemCount;
        public List<int> m_WarriorItemIdList;
        public List<int> m_RogueItemIdList;
        public List<int> m_MageItemIdList;
        public List<int> m_ItemWeightList;
        public List<int> m_ItemCountList;
        public bool CollectDataFromDBC(DBC_Row node)
        {
            m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
            m_GoldSum = DBCUtil.ExtractNumeric<int>(node, "GoldSum", 0, false);
            m_GoldMin = DBCUtil.ExtractNumeric<int>(node, "GoldMin", 0, false);
            m_GoldMax = DBCUtil.ExtractNumeric<int>(node, "GoldMax", 0, false);
            m_Exp = DBCUtil.ExtractNumeric<int>(node, "Exp", 0, false);
            m_Diamond = DBCUtil.ExtractNumeric<int>(node, "Diamond", 0, false);
            m_PartnerId = DBCUtil.ExtractNumeric<int>(node, "PartnerId", 0, false);
            m_ItemCount = DBCUtil.ExtractNumeric<int>(node, "ItemCount", 0, false);
            m_HpCount = DBCUtil.ExtractNumeric<int>(node, "HpCount", 0, false);
            m_HpPercent = DBCUtil.ExtractNumeric<int>(node, "HpPercent", 0, false);
            m_MpCount = DBCUtil.ExtractNumeric<int>(node, "MpCount", 0, false);
            m_MpPercent = DBCUtil.ExtractNumeric<int>(node, "MpPercent", 0, false);
            m_GoldModel = DBCUtil.ExtractString(node, "GoldModel", "", false);
            m_GoldParticle = DBCUtil.ExtractString(node, "GoldParticle", "", false);
            m_HpModel = DBCUtil.ExtractString(node, "HpModel", "", false);
            m_HpParticle = DBCUtil.ExtractString(node, "HpParticle", "", false);
            m_MpModel = DBCUtil.ExtractString(node, "MpModel", "", false);
            m_MpParticle = DBCUtil.ExtractString(node, "MpParticle", "", false);
            m_WarriorItemIdList = new List<int>();
            m_RogueItemIdList = new List<int>();
            m_MageItemIdList = new List<int>();
            m_ItemWeightList = new List<int>();
            m_ItemCountList = new List<int>();
            for (int i = 0; i < m_ItemCount; i++)
            {
                m_WarriorItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "WarriorItem" + i, 0, false));
                m_RogueItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "RogueItem" + i, 0, false));
                m_MageItemIdList.Add(DBCUtil.ExtractNumeric<int>(node, "MageItem" + i, 0, false));
                m_ItemWeightList.Add(DBCUtil.ExtractNumeric<int>(node, "Weight" + i, 0, false));
                m_ItemCountList.Add(DBCUtil.ExtractNumeric<int>(node, "Count" + i, 0, false));
            }
            return true;
        }

        public List<int> GetRewardItemByHeroId(int heroId)
        {
            List<int> itemList = null;
            switch (heroId)
            {
                case (int)HeroIdEnum.WARRIOR:
                    itemList = m_WarriorItemIdList;
                    break;
                case (int)HeroIdEnum.ROGUE:
                    itemList = m_RogueItemIdList;
                    break;
                case (int)HeroIdEnum.MAGE:
                    itemList = m_MageItemIdList;
                    break;
            }
            return itemList;
        }
        public void GetSceneReward(int heroId, out int itemId, out int itemCount)
        {
            itemId = -1;
            itemCount = 0;
            int maxWeight = 0;
            List<int> itemList = GetRewardItemByHeroId(heroId);
            if (itemList == null) return;
            foreach (int i in m_ItemWeightList)
            {
                maxWeight += i;
            }
            int randomWeight = CrossEngineHelper.Random.Next(maxWeight);
            for (int i = 0; i < m_ItemCount; ++i)
            {
                randomWeight -= m_ItemWeightList[i];
                if (randomWeight < 0)
                {
                    itemId = itemList[i];
                    itemCount = m_ItemCountList[i];
                    return;
                }
            }
        }

        public int GetId()
        {
            return m_Id;
        }
    }

    public class SceneConfigProvider
    {
        public DataDictionaryMgr<Data_SceneConfig> SceneConfigMgr
        {
            get { return m_SceneConfigMgr; }
        }

        public DataDictionaryMgr<Data_SceneDropOut> SceneDropOutMgr
        {
            get { return m_SceneDropOutMgr; }
        }

        public MyDictionary<int, MapDataProvider> MapDataProviders
        {
            get { return m_MapDataProviders; }
        }
        public Data_SceneConfig GetSceneConfigById(int id)
        {
            return m_SceneConfigMgr.GetDataById(id);
        }
        public MyDictionary<int, object> GetAllSceneConfig()
        {
            return m_SceneConfigMgr.GetData();
        }

        public Data_SceneDropOut GetSceneDropOutById(int id)
        {
            return m_SceneDropOutMgr.GetDataById(id);
        }
        public MapDataProvider GetMapDataBySceneResId(int resId)
        {
            MapDataProvider data = null;
            m_MapDataProviders.TryGetValue(resId, out data);
            return data;
        }
        public void Load(string file, string root)
        {
            m_SceneConfigMgr.CollectDataFromDBC(file, root);
        }
        public void LoadDropOutConfig(string file, string root)
        {
            m_SceneDropOutMgr.CollectDataFromDBC(file, root);
        }
        public MapDataProvider LoadSceneConfig(int id, string rootPath)
        {
            if (m_MapDataProviders.ContainsKey(id))
            {
                return m_MapDataProviders[id];
            }
            else
            {
                MapDataProvider provider = null;
                Data_SceneConfig sceneCfg = m_SceneConfigMgr.GetDataById(id);
                if (null != sceneCfg)
                {
                    provider = new MapDataProvider();
                    provider.CollectData(DataMap_Type.DT_Unit, rootPath + sceneCfg.m_UnitFile, "UnitInfo");
                    provider.CollectData(DataMap_Type.DT_SceneLogic, rootPath + sceneCfg.m_SceneLogicFile, "SceneLogic");
                    m_MapDataProviders.Add(id, provider);
                }
                return provider;
            }
        }
        public void LoadAllSceneConfig(string rootPath)
        {
            m_MapDataProviders.Clear();
            foreach (int id in m_SceneConfigMgr.GetData().Keys)
            {
                LoadSceneConfig(id, rootPath);
            }
        }
        public void InitChapterData()
        {
            m_StoryChapterData.Clear();
            foreach (Data_SceneConfig sc in m_SceneConfigMgr.GetData().Values)
            {
                if (sc.m_Type == (int)SceneTypeEnum.TYPE_PVE && sc.m_Chapter >= 0 && sc.m_Order >= 0)
                {
                    int index = sc.m_Chapter * m_MaxStageInChapter + sc.m_Order;
                    if ((int)SceneSubTypeEnum.TYPE_STORY == sc.m_SubType)
                    {
                        if (!m_StoryChapterData.ContainsKey(index))
                        {
                            m_StoryChapterData.Add(index, sc.m_Id);
                        }
                    }
                    else if ((int)SceneSubTypeEnum.TYPE_ELITE == sc.m_SubType)
                    {
                        if (!m_EliteChapterData.ContainsKey(index))
                        {
                            m_EliteChapterData.Add(index, sc.m_Id);
                        }
                    }
                }
            }
        }
        public int GetPreSceneId(int sceneId)
        {
            Data_SceneConfig sc = GetSceneConfigById(sceneId);
            if (null != sc)
            {
                return GetPreSceneId(sc.m_Chapter, sc.m_Order, sc.m_SubType);
            }
            return -1;
        }
        public int GetPreSceneId(int chapter, int order, int subType)
        {
            int preSceneIndex = chapter * m_MaxStageInChapter + order - 1;
            if ((int)SceneSubTypeEnum.TYPE_STORY == subType)
            {
                int id;
                if (m_StoryChapterData.TryGetValue(preSceneIndex, out id))
                {
                    return id;
                }
            }
            else if ((int)SceneSubTypeEnum.TYPE_ELITE == subType)
            {
                int id;
                if (m_EliteChapterData.TryGetValue(preSceneIndex, out id))
                {
                    return id;
                }
            }
            return -1;
        }
        public void Clear()
        {
            m_SceneConfigMgr.Clear();
            m_SceneDropOutMgr.Clear();
            m_MapDataProviders.Clear();
        }
        private DataDictionaryMgr<Data_SceneConfig> m_SceneConfigMgr = new DataDictionaryMgr<Data_SceneConfig>();
        private DataDictionaryMgr<Data_SceneDropOut> m_SceneDropOutMgr = new DataDictionaryMgr<Data_SceneDropOut>();
        private MyDictionary<int, MapDataProvider> m_MapDataProviders = new MyDictionary<int, MapDataProvider>();
        private MyDictionary<int, int> m_StoryChapterData = new MyDictionary<int, int>();
        private MyDictionary<int, int> m_EliteChapterData = new MyDictionary<int, int>();

        private const int m_MaxStageInChapter = 9;   //一章最多9关

        public static SceneConfigProvider Instance
        {
            get { return s_Instance; }
        }
        private static SceneConfigProvider s_Instance = new SceneConfigProvider();
    }
}
