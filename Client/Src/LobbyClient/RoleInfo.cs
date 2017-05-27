using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum GuideActionEnum : int
    {
        Gow = 1,
        Arena = 2,
        Expedition = 3,
        Attempt = 4,
        Gold = 5,
    }
    public class Teammate
    {
        public string Nick { get; set; }
        public int ResId { get; set; }
        public int Money { get; set; }
    }
    public class RoleInfo
    {
        public UserInfo GetPlayerSelfInfo()
        {
            return WorldSystem.Instance.GetPlayerSelf();
        }
        public void FightingScoreChangeCB(float score, int id)
        {
            UserInfo user = GetPlayerSelfInfo();
            if (null != user && id == user.GetId()
              && !Geometry.IsSameFloat(FightingScore, score))
            {
                if (WorldSystem.Instance.IsPureClientScene())
                {
                    GfxSystem.PublishGfxEvent("ge_fightscore_change", "lobby", FightingScore, score);//通知ui做效果
                }
                FightingScore = score;
                Network.LobbyNetworkSystem.Instance.UpdateFightingScore(score);
            }
        }
        public float SkillAppendScore()
        {
            int score = 0;
            if (null != m_SkillInfos)
            {
                for (int i = 0; i < m_SkillInfos.Count; i++)
                {
                    if (null == m_SkillInfos[i])
                        continue;
                    if (m_SkillInfos[i].Postions.Presets[0] > 0)
                    {
                        SkillLogicData skill_data = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, m_SkillInfos[i].SkillId) as SkillLogicData;
                        if (null != skill_data)
                        {
                            score += skill_data.ShowScore;
                        }
                    }
                }
            }
            return (float)score;
        }
        public ItemDataInfo GetItemData(int item_id, int random_property)
        {
            if (null != m_Items && m_Items.Count > 0)
            {
                bool ret = false;
                ItemDataInfo exist_info = null;
                foreach (ItemDataInfo info in m_Items)
                {
                    if (info.ItemId == item_id && random_property == info.RandomProperty)
                    {
                        exist_info = info;
                        ret = true;
                        break;
                    }
                }
                if (ret)
                {
                    ItemDataInfo item_info = new ItemDataInfo();
                    item_info.ItemConfig = ItemConfigProvider.Instance.GetDataById(item_id);
                    item_info.Level = exist_info.Level;
                    item_info.ItemNum = exist_info.ItemNum;
                    item_info.RandomProperty = exist_info.RandomProperty;
                    if (null != item_info.ItemConfig)
                    {
                        return item_info;
                    }
                }
            }
            return null;
        }
        public void DelItemData(int item_id, int random_property)
        {
            if (null != m_Items && m_Items.Count > 0)
            {
                for (int i = m_Items.Count - 1; i >= 0; i--)
                {
                    if (m_Items[i].ItemId == item_id
                      && random_property == m_Items[i].RandomProperty)
                    {
                        m_Items.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        public void ReduceItemData(int item_id, int random_property)
        {
            if (null != m_Items && m_Items.Count > 0)
            {
                for (int i = m_Items.Count - 1; i >= 0; i--)
                {
                    if (m_Items[i].ItemId == item_id
                      && m_Items[i].RandomProperty == random_property)
                    {
                        if (m_Items[i].ItemNum > 1)
                        {
                            m_Items[i].ItemNum -= 1;
                        }
                        else
                        {
                            m_Items.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
        }
        public void ReduceItemData(int item_id, int random_property, int num)
        {
            if (null != m_Items && m_Items.Count > 0)
            {
                for (int i = m_Items.Count - 1; i >= 0; i--)
                {
                    if (m_Items[i].ItemId == item_id
                      && m_Items[i].RandomProperty == random_property)
                    {
                        int residue_num = m_Items[i].ItemNum - num;
                        if (residue_num >= 0)
                        {
                            if (residue_num >= 1)
                            {
                                m_Items[i].ItemNum -= num;
                            }
                            else
                            {
                                m_Items.RemoveAt(i);
                            }
                        }
                        break;
                    }
                }
            }
        }
        public void AddItemData(ItemDataInfo item)
        {
            if (null != item && null != m_Items)
            {
                bool isHave = false;
                for (int i = 0; i < m_Items.Count; i++)
                {
                    if (m_Items[i].ItemId == item.ItemId && m_Items[i].RandomProperty == item.RandomProperty
                      && null != item.ItemConfig && item.ItemConfig.m_MaxStack > 1)
                    {
                        m_Items[i].ItemNum += 1;
                        isHave = true;
                        break;
                    }
                }
                if (!isHave)
                {
                    m_Items.Add(item);
                }
                if (WorldSystem.Instance.IsPureClientScene())
                {
                    GfxSystem.PublishGfxEvent("ge_item_change", "item");//通知ui做效果
                }
            }
        }
        public void AddItemData(ItemDataInfo item, int num)
        {
            if (null != item && null != m_Items)
            {
                bool isHave = false;
                for (int i = 0; i < m_Items.Count; i++)
                {
                    if (m_Items[i].ItemId == item.ItemId && m_Items[i].RandomProperty == item.RandomProperty
                      && null != item.ItemConfig && item.ItemConfig.m_MaxStack > 1)
                    {
                        m_Items[i].ItemNum += num;
                        isHave = true;
                        break;
                    }
                }
                if (!isHave)
                {
                    m_Items.Add(item);
                }
                if (WorldSystem.Instance.IsPureClientScene())
                {
                    GfxSystem.PublishGfxEvent("ge_item_change", "item");//通知ui做效果
                }
            }
        }
        public void SetEquip(int pos, ItemDataInfo info)
        {
            if (null != m_Equips && m_Equips.Length > 0)
            {
                for (int i = 0; i < m_Equips.Length; i++)
                {
                    if (i == pos)
                    {
                        m_Equips[i] = info;
                        break;
                    }
                }
            }
        }
        public void DeleteEquip(int equip_id)
        {
            if (null != m_Equips && m_Equips.Length > 0)
            {
                for (int i = 0; i < m_Equips.Length; i++)
                {
                    if (null != m_Equips[i] && m_Equips[i].ItemId == equip_id)
                    {
                        m_Equips[i] = null;
                        break;
                    }
                }
            }
        }
        public void ClearEquip()
        {
            if (null != m_Equips && m_Equips.Length > 0)
            {
                for (int i = 0; i < m_Equips.Length; i++)
                {
                    m_Equips[i] = null;
                }
            }
        }
        public XSoulInfo<XSoulPartInfo> GetXSoulInfo()
        {
            return m_XSoulInfo;
        }
        public ArenaStateInfo ArenaStateInfo
        {
            get { return m_ArenaStateInfo; }
        }
        public TalentManager TalentManager
        {
            get { return m_TalentManager; }
        }
        public ItemDataInfo GetLegacyData(int index)
        {
            ItemDataInfo info = null;
            for (int i = 0; i < m_Legacys.Length; i++)
            {
                if (i == index)
                {
                    info = m_Legacys[i];
                    break;
                }
            }
            return info;
        }
        public void SetLegacy(int pos, ItemDataInfo info)
        {
            if (null != m_Legacys && m_Legacys.Length > 0)
            {
                for (int i = 0; i < m_Legacys.Length; i++)
                {
                    if (i == pos)
                    {
                        m_Legacys[i] = info;
                        break;
                    }
                }
            }
        }
        public void SetSceneInfo(int sceneId, int grade)
        {
            int grade_;
            if (m_SceneInfo.TryGetValue(sceneId, out grade_))
            {
                if (grade_ < grade)
                {
                    m_SceneInfo[sceneId] = grade;
                }
            }
            else
            {
                m_SceneInfo.Add(sceneId, grade);
            }
        }
        public Dictionary<int, int> SceneInfo
        {
            get { return m_SceneInfo; }
        }
        public List<int> NewbieGuides
        {
            get { return m_NewbieGuides; }
            set { m_NewbieGuides = value; }
        }
        public int GetSceneInfo(int sceneId)
        {
            int info;
            m_SceneInfo.TryGetValue(sceneId, out info);
            return info;
        }
        public void AddCompletedSceneCount(int sceneId, int count = 1)
        {
            int ct;
            if (m_ScenesCompletedCountData.TryGetValue(sceneId, out ct))
            {
                m_ScenesCompletedCountData[sceneId] = ct + count;
            }
            else
            {
                m_ScenesCompletedCountData.Add(sceneId, count);
            }
        }
        public int GetCompletedSceneCount(int sceneId)
        {
            int ct;
            m_ScenesCompletedCountData.TryGetValue(sceneId, out ct);
            return ct;
        }
        public void ResetNewEquipCache()
        {
            if (null != m_NewEquipCache)
            {
                m_NewEquipCache.Clear();
            }
        }
        public void AddToNewEquipCache(int item_id, int property_id)
        {
            if (null != m_NewEquipCache && item_id > 0)
            {
                NewEquipInfo info = new NewEquipInfo();
                info.ItemId = item_id;
                info.ItemRandomProperty = property_id;
                m_NewEquipCache.Add(info);
            }
        }
        public void RemoveMailByGuid(ulong mail_guid)
        {
            MailInfo mi = null;
            foreach (MailInfo info in m_MailInfos)
            {
                if (info.m_MailGuid == mail_guid)
                {
                    mi = info;
                    break;
                }
            }
            if (null != mi)
            {
                m_MailInfos.Remove(mi);
            }
        }
        public bool IsDoneGuide(int bit)
        {
            return (m_NewbieFlag >> bit & 1) == 1 ? true : false;
        }
        public bool IsDoneGuideAction(int bit)
        {
            return (m_NewbieActionFlag >> bit & 1) == 1 ? true : false;
        }
        public bool IsDoneAction(int bit)
        {
            return (m_GuideFlag >> bit & 1) == 1 ? true : false;
        }
        public void SetActionFlag(long value)
        {
            m_GuideFlag = value;
        }
        public int GetOnlineMinutes()
        {
            TimeSpan deltaTime = DateTime.Now - m_OnlineDurationStartTime;
            return m_DailyOnLineDuration + (int)deltaTime.TotalMinutes;
        }
        public Dictionary<int, int> ScenesCompletedCountData
        {
            get { return m_ScenesCompletedCountData; }
        }
        public ulong Guid
        {
            get { return m_Guid; }
            set { m_Guid = value; }
        }
        public string Nickname
        {
            get { return m_Nickname; }
            set { m_Nickname = value; }
        }
        public int NewBieGuideScene
        {
            get { return m_NewBieGuideScene; }
            set { m_NewBieGuideScene = value; }
        }
        public int HeroId
        {
            get { return m_HeroId; }
            set { m_HeroId = value; }
        }
        public int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        public int Money
        {
            get { return m_Money; }
            set { m_Money = value; }
        }
        public int Gold
        {
            get { return m_Gold; }
            set { m_Gold = value; }
        }
        public int ExchangeCurrency
        {
            get { return m_ExchangeCurrency; }
            set { m_ExchangeCurrency = value; }
        }
        public int CurrencyId
        {
            get { return m_CurrencyId; }
            set { m_CurrencyId = value; }
        }
        public List<ItemDataInfo> Items
        {
            get { return m_Items; }
        }
        public ItemDataInfo[] Equips
        {
            get { return m_Equips; }
        }
        public List<SkillInfo> SkillInfos
        {
            get { return m_SkillInfos; }
        }
        public GowInfo Gow
        {
            get { return m_GowInfo; }
        }
        public List<MailInfo> MailInfos
        {
            get { return m_MailInfos; }
        }
        public int CurPresetIndex
        {
            get { return m_CurPresetIndex; }
            set { m_CurPresetIndex = value; }
        }
        public int Exp
        {
            get { return m_Exp; }
            set { m_Exp = value; }
        }
        public int Vip
        {
            get { return m_Vip; }
            set { m_Vip = value; }
        }
        public int TotalPurchasedDiamond
        {
            get { return m_TotalPurchasedDiamond; }
        }
        public int CitySceneId
        {
            get { return m_CitySceneId; }
            set { m_CitySceneId = value; }
        }
        // 精力相关
        public int VigorMax = 2000;
        public int Vigor
        {
            get { return m_Vigor; }
            set { m_Vigor = value; }
        }
        // 体力相关
        public int StaminaMax = 120;
        public int CurStamina
        {
            get { return m_CurStamina; }
            set
            {
                m_CurStamina = value;
            }
        }
        public int BuyStaminaCount
        {
            get { return m_BuyStaminaCount; }
            set { m_BuyStaminaCount = value; }
        }
        // 兑换金币相关
        public int BuyMoneyCount
        {
            get { return m_BuyMoneyCount; }
            set { m_BuyMoneyCount = value; }
        }
        // 出售物品钻石收益
        public int SellItemGoldIncome
        {
            get { return m_SellItemGoldIncome; }
            set { m_SellItemGoldIncome = value; }
        }
        // 任务
        public MissionStateInfo GetMissionStateInfo()
        {
            return m_MissionStateInfo;
        }
        // 远征
        public ExpeditionPlayerInfo GetExpeditionInfo()
        {
            return m_Expeditioninfo;
        }
        public ItemDataInfo[] Legacys
        {
            get { return m_Legacys; }
        }
        public Dictionary<ulong, FriendInfo> Friends
        {
            get { return m_Friends; }
        }
        public PartnerStateInfo PartnerStateInfo
        {
            get { return m_PartnerStateInfo; }
        }
        public List<NewEquipInfo> NewEquipCache
        {
            get { return m_NewEquipCache; }
        }
        public GroupInfo Group
        {
            get { return m_Group; }
        }
        public int AttemptAward
        {
            get { return m_AttemptAward; }
            set { m_AttemptAward = value; }
        }
        public int AttemptAcceptedAward
        {
            get { return m_AttemptAcceptedAward; }
            set { m_AttemptAcceptedAward = value; }
        }
        public long NewbieFlag
        {
            get { return m_NewbieFlag; }
            set { m_NewbieFlag = value; }
        }
        public int MatchKey
        {
            get { return m_MatchKey; }
            set { m_MatchKey = value; }
        }
        public long NewbieActionFlag
        {
            get { return m_NewbieActionFlag; }
            set { m_NewbieActionFlag = value; }
        }
        public const int c_AttemptAcceptedHzMax = 1;
        public int AttemptCurAcceptedCount
        {
            get { return m_AttemptCurAcceptedCount; }
            set { m_AttemptCurAcceptedCount = value; }
        }
        public int SignInCountCurMonth
        {
            get { return m_SignInCountCurMonth; }
            set { m_SignInCountCurMonth = value; }
        }
        public int RestSignInCount
        {
            get { return m_RestSignInCount; }
            set { m_RestSignInCount = value; }
        }
        public bool IsGetLoginReward
        {
            get { return m_IsGetLoginReward; }
            set { m_IsGetLoginReward = value; }
        }
        public List<int> WeeklyLoginRewardRecord
        {
            get { return m_WeeklyLoginRewardRecord; }
        }
        public int DailyOnLineDuration
        {
            get { return m_DailyOnLineDuration; }
            set { m_DailyOnLineDuration = value; }
        }
        public List<int> OnLineDurationRewardedIndex
        {
            get { return m_OnLineDurationRewardedIndex; }
        }
        public System.DateTime OnlineDurationStartTime
        {
            get { return m_OnlineDurationStartTime; }
            set { m_OnlineDurationStartTime = value; }
        }
        public Dictionary<int, int> ExchangeGoodsDic
        {
            get { return m_ExchangeGoodsDic; }
        }
        public Dictionary<int, int> RefreshDic
        {
            get { return m_RefreshDic; }
        }
        public bool LevelUp
        {
            get { return m_LevelUp; }
            set
            {
                m_LevelUp = value;
                LevelLock levelLockCfg = LevelLockProvider.Instance.GetDataById(17);
                if (levelLockCfg != null)
                {
                    if (levelLockCfg.m_Level == m_Level)
                        m_HasPvpTip = true;
                }
                levelLockCfg = LevelLockProvider.Instance.GetDataById(18);
                if (levelLockCfg != null)
                {
                    if (levelLockCfg.m_Level == m_Level)
                        m_HasPvpTip = true;
                }
                levelLockCfg = LevelLockProvider.Instance.GetDataById(13);
                if (levelLockCfg != null)
                {
                    if (levelLockCfg.m_Level == m_Level)
                        m_HasActivityTip = true;
                }
                levelLockCfg = LevelLockProvider.Instance.GetDataById(14);
                if (levelLockCfg != null)
                {
                    if (levelLockCfg.m_Level == m_Level)
                        m_HasActivityTip = true;
                }
                levelLockCfg = LevelLockProvider.Instance.GetDataById(15);
                if (levelLockCfg != null)
                {
                    if (levelLockCfg.m_Level == m_Level)
                        m_HasActivityTip = true;
                }
            }
        }
        public int GoldCurAcceptedCount
        {
            get { return m_GoldCurAcceptedCount; }
            set { m_GoldCurAcceptedCount = value; }
        }
        public bool HasActivityTip
        {
            get { return m_HasActivityTip; }
            set { m_HasActivityTip = value; }
        }
        public bool HasPvpTip
        {
            get { return m_HasPvpTip; }
            set { m_HasPvpTip = value; }
        }
        // 角色GUID
        private ulong m_Guid = 0;
        // 角色昵称               
        private string m_Nickname;
        // 新手指导场景
        private int m_NewBieGuideScene;
        // 角色职业               
        private int m_HeroId = 0;
        // 角色等级      
        private int m_Level = 0;
        // 金钱数
        private int m_Money = 0;
        // 钻石数
        private int m_Gold = 0;
        //兑换币
        private int m_CurrencyId = 0;
        private int m_ExchangeCurrency = 0;
        // 精力
        private int m_Vigor = 0;
        // 体力
        private int m_CurStamina = 0;
        // 经验
        private int m_Exp = 0;
        // vip等级
        private int m_Vip = 0;
        // 购买的钻石数
        private int m_TotalPurchasedDiamond = 0;
        // 所在的主城场景ID
        private int m_CitySceneId = 0;
        // 购买体力计数
        private int m_BuyStaminaCount = 0;
        // 兑换金币计数
        private int m_BuyMoneyCount = 0;
        // 出售物品收益
        private int m_SellItemGoldIncome = 0;
        // 战斗分数
        public float FightingScore { get; set; }
        // 角色物品信息
        private List<ItemDataInfo> m_Items = new List<ItemDataInfo>();
        public const int c_MaxItemNum = 128;
        // 角色装备信息
        private ItemDataInfo[] m_Equips = new ItemDataInfo[EquipmentStateInfo.c_EquipmentCapacity];
        // 新获得装备信息
        private List<NewEquipInfo> m_NewEquipCache = new List<NewEquipInfo>();
        // 角色技能预设信息
        private List<SkillInfo> m_SkillInfos = new List<SkillInfo>();
        // 角色任务信息
        private MissionStateInfo m_MissionStateInfo = new MissionStateInfo();
        // 神器信息
        private ItemDataInfo[] m_Legacys = new ItemDataInfo[LegacyStateInfo.c_LegacyCapacity];
        private XSoulInfo<XSoulPartInfo> m_XSoulInfo = new XSoulInfo<XSoulPartInfo>();
        private ArenaStateInfo m_ArenaStateInfo = new ArenaStateInfo();
        private TalentManager m_TalentManager = new TalentManager();

        // 通关信息
        private Dictionary<int, int> m_SceneInfo = new Dictionary<int, int>();
        // 通关次数
        private Dictionary<int, int> m_ScenesCompletedCountData = new Dictionary<int, int>();
        // 教学信息
        private List<int> m_NewbieGuides = new List<int>();
        // 远征信息
        private ExpeditionPlayerInfo m_Expeditioninfo = new ExpeditionPlayerInfo();
        // 预设索引
        private int m_CurPresetIndex = 0;
        // 战神赛信息
        private GowInfo m_GowInfo = new GowInfo();
        // 邮件信息
        private List<MailInfo> m_MailInfos = new List<MailInfo>();
        // 好友信息
        private Dictionary<ulong, FriendInfo> m_Friends = new Dictionary<ulong, FriendInfo>();
        // 伙伴信息
        private PartnerStateInfo m_PartnerStateInfo = new PartnerStateInfo();
        // 队伍信息
        private GroupInfo m_Group = new GroupInfo();
        // Mpve
        public const int c_AttemptUnlockLevel = 30;
        public const int c_GoldUnlockLevel = 15;
        private int m_AttemptAward = 0;
        private int m_AttemptCurAcceptedCount = 0;
        private int m_AttemptAcceptedAward = 0;
        private int m_GoldCurAcceptedCount = 0;
        // 签到次数
        private int m_SignInCountCurMonth = 0;
        private int m_RestSignInCount = 0;
        private bool m_IsGetLoginReward = false;
        private List<int> m_WeeklyLoginRewardRecord = new List<int>();
        private bool m_LevelUp = false;
        //ExchangeGoods
        private Dictionary<int, int> m_ExchangeGoodsDic = new Dictionary<int, int>();
        private Dictionary<int, int> m_RefreshDic = new Dictionary<int, int>();
        // 新手引导相关
        private long m_NewbieFlag = 0;
        private long m_NewbieActionFlag = 0;
        private long m_GuideFlag = 0;
        // 在线时长领奖
        private int m_DailyOnLineDuration = 0;
        private List<int> m_OnLineDurationRewardedIndex = new List<int>();
        private DateTime m_OnlineDurationStartTime = new DateTime(1970, 1, 1, 0, 0, 0);
        // 结算用和服务器匹配的随机数
        private int m_MatchKey = 0;
        //（todo 临时）是否有活动提示（红点）
        private bool m_HasActivityTip = false;
        //（todo 临时）是否有pvp提示（红点）
        private bool m_HasPvpTip = false;
    }
}
