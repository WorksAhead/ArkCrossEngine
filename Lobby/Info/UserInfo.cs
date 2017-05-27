using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal enum CampIdEnum : int
    {
        Unkown = 0,
        Friendly,
        Hostile,
        Blue,
        Red,
    }
    internal enum BattleResultEnum : int
    {
        Win,      //胜利
        Lost,     //失败
        Unfinish, //未完成
    }
    internal enum GuideActionEnum : int
    {
        Gow = 1,
        Arena = 2,
        Expedition = 3,
        Attempt = 4,
        Gold = 5,
    }

    #region use by norm log
    internal enum AssetType : int
    {
        Money = 0,
        Glod = 1,
        Exp = 2,
        Stamina = 3,
        Vigor = 4,
    }
    internal enum GainAssetType : int
    {
        SellItem = 0,
        StageClear = 1,
        AddAssets = 2,
        FinishMission = 3,
        MidasTouch = 4,
        BuyStamina = 5,
        AutoRecover = 6,
    }
    internal enum GainConsumePos : int
    {
        Bag = 0,
        Legacy = 1,
        Mail = 2,
        Expedition = 3,
        Gm = 4,
        Mission = 5,
        Equipment = 6,
        Skill = 7,
        StageClear = 8,
        Partner = 9,
        Mpve = 10,
        Gift = 11,
        Compound = 12,
        XSoul = 13,
        AutoRecover = 14,
        Gow = 15,
    }
    internal enum ConsumeAssetType : int
    {
        MountEquipment = 0,
        UpgradeSkill = 1,
        UpgradeItem = 2,
        StageClear = 3,
        BuyStamina = 4,
        UpgradeLegacy = 5,
        MidasTouch = 6,
        BuyLife = 7,
        UpgradePartner = 8,
        RefreshExchange = 9,
        ExchangeGoods = 10,
    }
    internal enum GainItemType : int
    {
        Equipment = 0,
        Props = 1,
        Currency = 2,
    }
    internal enum GainItemWay : int
    {
        GoldBuy = 0,
        DropOut = 1,
        AddItem = 2,
        FinishMission = 3,
        ExchangeGoods = 4,
        SignIn = 5,
        Compound = 6,
    }
    internal enum ConsumeItemWay : int
    {
        SellItem = 0,
        UpgradeLegacy = 1,
        LiftSkill = 2,
        UpgradePartner = 3,
        BuyLife = 4,
        SweepStage = 5,
        CompoundItem = 6,
        UpgradeXSoul = 7,
        ExchangeCurrency = 8,
    }
    internal enum GetTaskResult : int
    {
        Failure = 0,
        Succeed = 1,
    }
    internal enum FinishMissionResult : int
    {
        Failure = 0,
        Succeed = 1,
    }
    internal enum FightingType : int
    {
        General = 0,
        Auto = 1,
        MopUp = 2,
    }
    internal enum PvefightResult : int
    {
        Failure = 0,
        Succeed = 1,
    }
    internal enum PartnerCauseId : int
    {
        TollgateGain = 0,
        MissionGain = 1,
        CompoundGain = 2,
        UpgradeLevel = 3,
        UpgradeStage = 4,
        Active = 5,
    }
    internal enum PartnerOperateResult : int
    {
        Null = 0,
        Failure = 1,
        Succeed = 2,
    }
    #endregion

    internal class Teammate
    {
        internal string Nick { get; set; }
        internal int ResId { get; set; }
        internal int Money { get; set; }
    }
    internal class UserBattleInfo
    {
        internal UserBattleInfo()
        {
        }
        internal int SceneID
        {
            get { return m_SceneId; }
            set { m_SceneId = value; }
        }
        internal long StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }
        internal long EndTime
        {
            get { return m_EndTime; }
            set { m_EndTime = value; }
        }
        internal int SumGold
        {
            get { return m_SumGold; }
            set { m_SumGold = value; }
        }
        internal int AddGold
        {
            get { return m_AddGold; }
            set { m_AddGold = value; }
        }
        internal int TotalGold
        {
            get { return m_TotalGold; }
            set { m_TotalGold = value; }
        }
        internal BattleResultEnum BattleResult
        {
            get { return m_BattleResult; }
            set { m_BattleResult = value; }
        }
        internal int RewardItemCount
        {
            get { return m_RewardItemCount; }
        }
        internal int RewardItemId
        {
            get { return m_RewardItemId; }
        }
        internal int Exp
        {
            get { return m_Exp; }
        }
        internal int DeadCount
        {
            get { return m_DeadCount; }
            set { m_DeadCount = value; }
        }
        internal int Elo
        {
            get { return m_Elo; }
            set { m_Elo = value; }
        }
        internal int HitCount
        {
            get { return m_HitCount; }
            set { m_HitCount = value; }
        }
        internal int KillNpcCount
        {
            get { return m_KillNpcCount; }
            set { m_KillNpcCount = value; }
        }
        internal int MaxMultiHitCount
        {
            get { return m_MaxMultiHitCount; }
            set { m_MaxMultiHitCount = value; }
        }
        internal int TotalDamageToMyself
        {
            get { return m_TotalDamageToMyself; }
            set { m_TotalDamageToMyself = value; }
        }
        internal int TotalDamageFromMyself
        {
            get { return m_TotalDamageFromMyself; }
            set { m_TotalDamageFromMyself = value; }
        }
        internal List<Teammate> TeamMate
        {
            get { return m_Teammate; }
        }
        internal Lobby.JsonMessageWithGuid LastMsg
        {
            get { return m_LastMsg; }
            set { m_LastMsg = value; }
        }
        internal bool IsClearing
        {
            get { return m_IsClearing; }
            set { m_IsClearing = value; }
        }
        internal int MatchKey
        {
            get { return m_MatchKey; }
        }
        internal void Reset()
        {
            m_SceneId = -1;
            m_StartTime = 0;
            m_EndTime = 0;
            m_DeadCount = 0;
            m_SumGold = 0;
            m_AddGold = 0;
            m_TotalGold = 0;
            m_Exp = 0;
            m_RewardItemCount = 0;
            m_RewardItemId = 0;
            m_BattleResult = BattleResultEnum.Unfinish;
            m_LastMsg = null;
            m_IsClearing = false;
            m_MatchKey = 0;

            m_HitCount = 0;
            m_MaxMultiHitCount = 0;
            m_TotalDamageToMyself = 0;
            m_TotalDamageFromMyself = 0;
            m_Teammate.Clear();
        }
        internal bool init(int sceneId, int heroId)
        {
            Reset();
            m_SceneId = sceneId;
            m_StartTime = TimeUtility.GetServerMilliseconds();
            Data_SceneConfig sceneConfig = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
            if (null != sceneConfig)
            {
                if (sceneConfig.m_Type == (int)SceneTypeEnum.TYPE_PVE)
                {
                    m_MatchKey = CrossEngineHelper.Random.Next(int.MaxValue);
                }
                Data_SceneDropOut dropOutConfig = SceneConfigProvider.Instance.GetSceneDropOutById(sceneConfig.m_DropId);
                if (null != dropOutConfig)
                {
                    m_SumGold = dropOutConfig.m_GoldSum;
                    m_Exp = dropOutConfig.m_Exp;
                    dropOutConfig.GetSceneReward(heroId, out m_RewardItemId, out m_RewardItemCount);
                    LogSystem.Debug("Init UserCombatInfo: SceneId = {0}, StartTime = {1}, GoldSum = {2}, Exp = {3}, RewardItem = {4}, ItemCount = {5}", m_SceneId, m_StartTime, m_SumGold, m_Exp, m_RewardItemId, m_RewardItemCount);
                    return true;
                }
                else
                {
                    LogSystem.Warn("UserBattleInfo::init : Can not find dropOutConfig {0}", sceneConfig.m_DropId);
                }
            }
            else
            {
                LogSystem.Warn("UserBattleInfo::init : Can not find sceneConfig {0}", sceneId);
            }
            return false;
        }

        private int m_SceneId = -1;
        private long m_StartTime = 0;
        private long m_EndTime = 0;
        private int m_SumGold = 0;
        private int m_AddGold = 0;
        private int m_TotalGold = 0;
        private int m_Exp = 0;
        private int m_RewardItemId = 0;
        private int m_RewardItemCount = 0;
        private int m_DeadCount = 0;
        private BattleResultEnum m_BattleResult = BattleResultEnum.Unfinish;
        private int m_Elo = 0;

        private int m_HitCount = 0;
        private int m_KillNpcCount = 0;
        private int m_MaxMultiHitCount = 0;
        private int m_TotalDamageToMyself = 0;
        private int m_TotalDamageFromMyself = 0;
        private List<Teammate> m_Teammate = new List<Teammate>();
        private JsonMessageWithGuid m_LastMsg = null;
        private bool m_IsClearing = false;
        private int m_MatchKey = 0;
    }
    internal enum UserState : int
    {
        Online = 0,           //玩家登录，但并未加入游戏
        Pve = 1,              //玩家在单人PvE
        Teaming = 2,          //匹配中
        Room = 3,             //玩家在多人副本游戏中，PvP或多人PvE
        DropOrOffline = 4,    //掉线或离线状态（逻辑上区分掉线与离线意义不大）
    }
    internal class UserInfo
    {
        internal const int LifeTimeOfNoHeartbeat = 120000;    //玩家离线，如果心跳停止超过这个值认为玩家离线   
        internal UserInfo()
        { }
        internal UserState CurrentState
        {
            get { return m_CurrentState; }
            set { m_CurrentState = value; }
        }
        internal long NextUserSaveCount
        {
            get { return m_NextUserSaveCount; }
            set { m_NextUserSaveCount = value; }
        }
        internal long CurrentUserSaveCount
        {
            get { return m_CurrentUserSaveCount; }
            set
            {
                if (m_NextUserSaveCount > 0)
                {
                    if (m_CurrentUserSaveCount != -1 && m_CurrentUserSaveCount < value)
                    {
                        m_CurrentUserSaveCount = value;
                    }
                }
                else
                {
                    m_CurrentUserSaveCount = value;
                }
            }
        }
        //基本数据
        internal ulong Guid
        {
            get { return m_Guid; }
            set { m_Guid = value; }
        }
        internal string Account
        {
            get { return m_Account; }
            set { m_Account = value; }
        }
        internal string AccountId
        {
            get { return m_AccountId; }
            set { m_AccountId = value; }
        }
        internal int LogicServerId
        {
            get { return m_LogicServerId; }
            set { m_LogicServerId = value; }
        }
        internal string Nickname
        {
            get { return m_Nickname; }
            set { m_Nickname = value; }
        }
        internal int HeroId
        {
            get { return m_HeroId; }
            set { m_HeroId = value; }
        }
        internal string NodeName
        {
            get { return m_NodeName; }
            set { m_NodeName = value; }
        }
        internal uint Key
        {
            get { return m_Key; }
            set { m_Key = value; }
        }
        //游戏数据
        internal int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        internal int Vip
        {
            get { return m_Vip; }
            set { m_Vip = value; }
        }
        internal int Money
        {
            get { return m_Money; }
            set { m_Money = value; }
        }
        internal int Gold
        {
            get { return m_Gold; }
            set { m_Gold = value; }
        }
        internal int ExpPoints
        {
            get { return m_ExpPoints; }
            set { m_ExpPoints = value; }
        }
        internal int CitySceneId
        {
            get { return m_CitySceneId; }
            set { m_CitySceneId = value; }
        }
        internal float X
        {
            get { return m_X; }
            set { m_X = value; }
        }
        internal float Z
        {
            get { return m_Z; }
            set { m_Z = value; }
        }
        internal float FaceDir
        {
            get { return m_FaceDir; }
            set { m_FaceDir = value; }
        }
        //装备数据
        internal EquipInfo Equip
        {
            get { return m_EquipInfo; }
        }
        //背包物品数据
        internal ItemBag ItemBag       // 伙伴数据
        {
            get { return m_ItemBag; }
        }
        //技能数据
        internal SkillInfo Skill
        {
            get { return m_SkillInfo; }
        }
        // 任务信息
        internal MissionStateInfo Mission
        {
            get { return m_MissionInfo; }
        }
        // 神器
        internal LegacyInfo Legacy
        {
            get { return m_LegacyInfo; }
        }
        // X魂
        internal XSoulInfo<ItemInfo> XSoul
        {
            get { return m_XSoulInfo; }
        }
        // 远征
        internal ExpeditionPlayerInfo Expedition
        {
            get { return m_ExpeditionInfo; }
        }
        // 战神赛
        internal GowInfo GowInfo
        {
            get { return m_GowInfo; }
        }
        // 邮件列表等邮件状态
        internal MailStateInfo MailStateInfo
        {
            get { return m_MailStateInfo; }
        }
        //物品兑换
        internal ExchangeGoodsInfo ExchangeGoodsInfo
        {
            get { return m_ExchangeGoodsInfo; }
        }
        //玩家组队数据
        internal long LastNotifyMatchTime
        {
            get { return m_LastNotifyMatchTime; }
            set { m_LastNotifyMatchTime = value; }
        }
        internal LoginRewardInfo LoginRewardInfo
        {
            get { return m_LoginRewardInfo; }
            set { m_LoginRewardInfo = value; }
        }
        internal PartnerStateInfo PartnerStateInfo
        {
            get { return m_PartnerStateInfo; }
        }
        internal PaymentInfo PaymentStateInfo
        {
            get { return m_PaymentInfo; }
        }
        internal ActivityFinishInfo ActivityFinish
        {
            get { return m_ActivityFinishInfo; }
        }
        internal bool IsPrepared
        {
            get { return m_IsPrepared; }
            set { m_IsPrepared = value; }
        }
        //是否是机器
        internal bool IsMachine
        {
            get { return m_IsMachine; }
            set { m_IsMachine = value; }
        }
        internal bool IsDisconnected
        {
            get { return m_IsDisconnected; }
            set { m_IsDisconnected = value; }
        }
        internal GroupInfo Group
        {
            get
            {
                if (m_Group == null)
                    return null;
                else
                    return m_Group.Target as GroupInfo;
            }
            set
            {
                if (value == null)
                    m_Group = null;
                else
                    m_Group = new WeakReference(value);
            }
        }
        //玩家当前游戏数据
        internal RoomInfo Room
        {
            get
            {
                if (m_Room == null)
                    return null;
                else
                    return m_Room.Target as RoomInfo;
            }
            set
            {
                if (value == null)
                    m_Room = null;
                else
                    m_Room = new WeakReference(value);
            }
        }
        internal int CurrentRoomID
        {
            get { return m_curRoomID; }
            set { m_curRoomID = value; }
        }
        internal int CampId
        {
            get { return m_CampId; }
            set { m_CampId = value; }
        }
        internal UserBattleInfo CurrentBattleInfo
        {
            get { return m_curBattleInfo; }
            set { m_curBattleInfo = value; }
        }
        //其它
        internal long LastRequestDareTime
        {
            get { return m_LastRequestDareTime; }
            set { m_LastRequestDareTime = value; }
        }
        internal int LeftLife
        {
            get { return m_LeftLife; }
            set { m_LeftLife = value; }
        }
        internal long LastDSSaveTime
        {
            get { return m_LastDSSaveTime; }
            set { m_LastDSSaveTime = value; }
        }
        internal long LastCheckDataTime
        {
            get { return m_LastCheckDataTime; }
            set { m_LastCheckDataTime = value; }
        }
        internal bool IsRecycled
        {
            get { return m_IsRecycled; }
            set { m_IsRecycled = value; }
        }
        internal object Lock
        {
            get { return m_Lock; }
        }
        //玩家好友数据 
        internal ConcurrentDictionary<ulong, FriendInfo> FriendInfos
        {
            get { return m_Friends; }
        }
        internal void ResetRoomInfo()
        {
            this.Room = null;
            this.CurrentRoomID = 0;
        }
        internal int FightingScore
        {
            get { return m_FightingScore; }
            set
            {
                if (value > 0)
                {
                    m_FightingScore = value;
                }
            }
        }
        internal DateTime LastLogoutTime
        {
            get { return m_LastLogoutTime; }
            set { m_LastLogoutTime = value; }
        }
        ///
        internal int VigorMax = 2000;
        internal const double AddVigorIntervalTime = 6;
        internal int Vigor
        {
            get { return m_Vigor; }
            set { m_Vigor = value; }
        }
        internal double LastAddVigorTimestamp
        {
            get { return m_LastAddVigorTimestamp; }
            set { m_LastAddVigorTimestamp = value; }
        }
        internal void InitVigorForLogin(int lastVigor, double lastAddVigorTime)
        {
            if (lastVigor >= VigorMax)
            {
                this.Vigor = VigorMax;
            }
            else
            {
                this.LastAddVigorTimestamp = TimeUtility.CurTimestamp;
                double diff_value = TimeUtility.CurTimestamp - lastAddVigorTime;
                int vigor_count = (int)(diff_value / AddVigorIntervalTime);
                if (vigor_count > 0)
                {
                    int total_vigor = vigor_count + lastVigor;
                    if (total_vigor > VigorMax)
                    {
                        this.Vigor = VigorMax;
                    }
                    else
                    {
                        this.Vigor = total_vigor;
                    }
                }
                else
                {
                    this.Vigor = lastVigor;
                }
            }
        }
        internal bool IncreaseVigor()
        {
            bool result = false;
            if (this.Vigor < VigorMax)
            {
                if (TimeUtility.CurTimestamp - LastAddVigorTimestamp >= AddVigorIntervalTime)
                {
                    /// this.Vigor += 1;
                    this.LastAddVigorTimestamp = TimeUtility.CurTimestamp;
                    result = true;
                }
            }
            return result;
        }
        ///
        internal int StaminaMax = 120;
        internal const double AddStaminaIntervalTime = 300;
        internal int CurStamina
        {
            get { return m_CurStamina; }
            set
            {
                m_CurStamina = value;
                JsonMessageWithGuid ssMsg = new JsonMessageWithGuid(JsonMessageID.SyncStamina);
                ssMsg.m_Guid = m_Guid;
                ArkCrossEngineMessage.Msg_LC_SyncStamina protoData = new ArkCrossEngineMessage.Msg_LC_SyncStamina();
                protoData.m_Stamina = m_CurStamina;
                ssMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(NodeName, ssMsg);
            }
        }
        internal double LastAddStaminaTimestamp
        {
            get { return m_LastAddStaminaTimestamp; }
            set { m_LastAddStaminaTimestamp = value; }
        }
        internal void InitStaminaForLogin(int lastStamina, double lastAddStaminaTime, int buyStaminaCount)
        {
            if (IsDifferentDay(lastAddStaminaTime))
            {
                CurStamina = lastStamina;
                ResetStamina();
            }
            else
            {
                this.CurBuyStaminaCount = buyStaminaCount;
                if (lastStamina > StaminaMax)
                {
                    CurStamina = lastStamina;
                }
                else
                {
                    this.LastAddStaminaTimestamp = TimeUtility.CurTimestamp;
                    double diff_value = TimeUtility.CurTimestamp - lastAddStaminaTime;
                    int stamina_count = (int)(diff_value / AddStaminaIntervalTime);
                    if (stamina_count > 0)
                    {
                        int total_stamina = stamina_count + lastStamina;
                        if (total_stamina > StaminaMax)
                        {
                            CurStamina = StaminaMax;
                        }
                        else
                        {
                            CurStamina = total_stamina;
                        }
                    }
                    else
                    {
                        CurStamina = lastStamina;
                    }
                }
            }
        }
        internal void IncreaseStamina()
        {
            if (IsDifferentDay(m_LastResetStaminaTime))
            {
                ResetStamina();
            }
            if (CurStamina < StaminaMax)
            {
                if (TimeUtility.CurTimestamp - m_LastAddStaminaTimestamp >= AddStaminaIntervalTime)
                {
                    CurStamina += 1;
                    m_LastAddStaminaTimestamp = TimeUtility.CurTimestamp;
                }
            }
        }
        internal void ResetStamina()
        {
            if (CurStamina < StaminaMax)
            {
                CurStamina = StaminaMax;
            }
            m_UsedStamina = 0;
            m_CurBuyStaminaCount = 0;
            m_LastAddStaminaTimestamp = TimeUtility.CurTimestamp;
            DateTime now = DateTime.Now;
            m_LastResetStaminaTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        }
        internal void InitMidasTouchForLogin(double lastBuyMoneyTime, int buyMoneyCount)
        {
            m_CurBuyMoneyCount = buyMoneyCount;
            m_LastBuyMoneyTimestamp = TimeUtility.CurTimestamp;
            if (IsDifferentDay(lastBuyMoneyTime))
            {
                ResetMidasTouch();
            }
        }
        private void ResetMidasTouch()
        {
            m_CurBuyMoneyCount = 0;
            m_LastBuyMoneyTimestamp = TimeUtility.CurTimestamp;
            DateTime now = DateTime.Now;
            m_LastResetMidasTouchTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        }
        internal void UpdateMidasTouch()
        {
            if (IsDifferentDay(m_LastResetMidasTouchTime))
            {
                ResetMidasTouch();
            }
        }
        internal void InitExchangeGoodsForLogin(string goodListStr, string goodNumberStr, string refreshNumberStr)
        {
            List<int> goodList = new List<int>();
            string[] goodArray = goodListStr.Split(new char[] { '|' });
            foreach (string str in goodArray)
            {
                if (str.Trim() != string.Empty)
                {
                    int goodId = -1;
                    if (int.TryParse(str, out goodId))
                    {
                        goodList.Add(goodId);
                    }
                }
            }
            List<int> goodNumber = new List<int>();
            string[] goodNumberArray = goodNumberStr.Split(new char[] { '|' });
            foreach (string str in goodNumberArray)
            {
                if (str.Trim() != string.Empty)
                {
                    int number = -1;
                    if (int.TryParse(str, out number))
                    {
                        goodNumber.Add(number);
                    }
                }
            }
            if (goodList.Count == goodNumber.Count)
            {
                for (int i = 0; i < goodList.Count; ++i)
                {
                    this.ExchangeGoodsInfo.AddGoodData(goodList[i], goodNumber[i]);
                }
            }
            string[] currencyRefreshArray = refreshNumberStr.Split(new char[] { '&' });
            foreach (string str in currencyRefreshArray)
            {
                if (str.Trim() != string.Empty)
                {
                    string[] currencyRefresh = str.Split(new char[] { '|' });
                    if (currencyRefresh.Length == 2)
                    {
                        int currencyId = 0;
                        int refreshNumber = 0;
                        if (int.TryParse(currencyRefresh[0], out currencyId) && int.TryParse(currencyRefresh[1], out refreshNumber))
                        {
                            this.ExchangeGoodsInfo.CurrencyRefreshNum.TryAdd(currencyId, refreshNumber);
                        }
                    }
                }
            }
        }
        private void ResetExchangeGoods()
        {
            ExchangeGoodsInfo.Reset();
            DateTime now = DateTime.Now;
            m_LastResetExchangeGoodsTime = now;
            JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.RefreshExchangeResult);
            jsonMsg.m_Guid = Guid;
            ArkCrossEngineMessage.Msg_LC_RefreshExchangeResult protoData = new ArkCrossEngineMessage.Msg_LC_RefreshExchangeResult();
            protoData.m_RequestRefreshResult = (int)GeneralOperationResult.LC_Succeed;
            protoData.m_RefreshNum = 0;
            protoData.m_CurrencyId = 0;
            jsonMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(NodeName, jsonMsg);
        }
        internal void UpdateExchangeGoods()
        {
            DateTime now = DateTime.Now;
            DateTime lastday = DateTime.Now.AddDays(-1);
            if (m_LastResetExchangeGoodsTime < new DateTime(lastday.Year, lastday.Month, lastday.Day, 21, 0, 0))
            {
                ResetExchangeGoods();
            }
            else
            {
                if (now >= new DateTime(now.Year, now.Month, now.Day, 21, 0, 0) && m_LastResetExchangeGoodsTime < new DateTime(now.Year, now.Month, now.Day, 21, 0, 0))
                {
                    ResetExchangeGoods();
                }
            }
        }
        private void ResetAttempt()
        {
            if (m_AttemptCurAcceptedCount >= c_AttemptAcceptedHzMax)
            {
                m_AttemptAward = 0;
            }
            m_AttemptAcceptedAward = 0;
            m_AttemptCurAcceptedCount = 0;
            DateTime now = DateTime.Now;
            m_LastResetAttemptAwardCountTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            ///
            JsonMessageWithGuid ssMsg = new JsonMessageWithGuid(JsonMessageID.SyncAttemptInfo);
            ssMsg.m_Guid = m_Guid;
            ArkCrossEngineMessage.Msg_LC_SyncAttemptInfo protoData = new ArkCrossEngineMessage.Msg_LC_SyncAttemptInfo();
            protoData.m_AttemptAward = m_AttemptAward;
            protoData.m_AttemptAcceptedAward = m_AttemptAcceptedAward;
            protoData.m_AttemptCurAcceptedCount = m_AttemptCurAcceptedCount;
            ssMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(NodeName, ssMsg);
        }
        internal void UpdateAttempt()
        {
            if (IsDifferentDay(m_LastResetAttemptAwardCountTime))
            {
                ResetAttempt();
            }
        }
        private void ResetGoldTollgate()
        {
            m_GoldCurAcceptedCount = 0;
            DateTime now = DateTime.Now;
            m_LastResetGoldAwardCountTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            ///
            JsonMessageWithGuid ssMsg = new JsonMessageWithGuid(JsonMessageID.SyncGoldTollgateInfo);
            ssMsg.m_Guid = m_Guid;
            ArkCrossEngineMessage.Msg_LC_SyncGoldTollgateInfo protoData = new ArkCrossEngineMessage.Msg_LC_SyncGoldTollgateInfo();
            protoData.m_GoldCurAcceptedCount = m_GoldCurAcceptedCount;
            ssMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(NodeName, ssMsg);
        }
        internal void UpdateGoldTollgate()
        {
            if (IsDifferentDay(m_LastResetGoldAwardCountTime))
            {
                ResetGoldTollgate();
            }
        }
        internal void InitSellIncomeForLogin(double lastTimestamp, int incomeCount)
        {
            m_CurSellItemGoldIncome = incomeCount;
            m_LastSellItemGoldIncomeTimestamp = TimeUtility.CurTimestamp;
            if (IsDifferentDay(lastTimestamp))
            {
                ResetSellItemIncome();
            }
        }
        private void ResetSellItemIncome()
        {
            m_CurSellItemGoldIncome = 0;
            m_LastSellItemGoldIncomeTimestamp = TimeUtility.CurTimestamp;
            DateTime now = DateTime.Now;
            m_LastResetSellItemIncomeTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        }
        private double c_DropOrOfflineLifeTime = 60.0;
        internal List<ulong> CheckGroupMemberStatus()
        {
            List<ulong> quit_user_guid = new List<ulong>();
            if (null != Group && null != Group.Members && Group.Members.Count > 0)
            {
                int ct = Group.Members.Count;
                for (int i = 0; i < ct; i++)
                {
                    if (null != Group.Members[i] && Group.Members[i].Status == UserState.DropOrOffline
                      && Group.Members[i].LeftLife > 0)
                    {
                        double interval_time = TimeUtility.CurTimestamp - Group.Members[i].LeftLife;
                        if (interval_time > c_DropOrOfflineLifeTime)
                        {
                            Group.Members[i].LeftLife = -1;
                            quit_user_guid.Add(Group.Members[i].Guid);
                        }
                    }
                }
            }
            return quit_user_guid;
        }
        internal void UpdateSellItemIncome()
        {
            if (IsDifferentDay(m_LastResetSellItemIncomeTime))
            {
                ResetSellItemIncome();
            }
        }
        internal void InitCompleteSceneForLogin(string sceneListStr, string sceneNumberStr)
        {
            List<int> sceneList = new List<int>();
            string[] sceneArray = sceneListStr.Split(new char[] { '|' });
            foreach (string str in sceneArray)
            {
                if (str.Trim() != string.Empty)
                {
                    int sceneId = -1;
                    if (int.TryParse(str, out sceneId))
                    {
                        sceneList.Add(sceneId);
                    }
                }
            }
            List<int> sceneNumber = new List<int>();
            string[] sceneNumberArray = sceneNumberStr.Split(new char[] { '|' });
            foreach (string str in sceneNumberArray)
            {
                if (str.Trim() != string.Empty)
                {
                    int number = -1;
                    if (int.TryParse(str, out number))
                    {
                        sceneNumber.Add(number);
                    }
                }
            }
            if (sceneList.Count == sceneNumber.Count)
            {
                for (int i = 0; i < sceneList.Count; ++i)
                {
                    this.ScenesCompletedCountData.TryAdd(sceneList[i], sceneNumber[i]);
                }
            }
        }
        internal void UpdateSceneCompletedCountData()
        {
            if (IsDifferentDay(m_LastResetCompletedScenesCountTime))
            {
                ResetSceneCompletedCount();
            }
        }
        internal void ResetSceneCompletedCount()
        {
            m_ScenesCompletedCountData.Clear();
            m_LastResetCompletedScenesCountTime = DateTime.Now;
        }
        internal void InitWeeklyLoginRewardForLogin(string rewardListStr)
        {
            List<int> rewardList = new List<int>();
            string[] rewardArray = rewardListStr.Split(new char[] { '|' });
            foreach (string str in rewardArray)
            {
                if (str.Trim() != string.Empty)
                {
                    int reward = -1;
                    if (int.TryParse(str, out reward))
                    {
                        rewardList.Add(reward);
                    }
                }
            }
            foreach (int record in rewardList)
            {
                this.LoginRewardInfo.AddToWeeklyLoginRewardRecordList(record);
            }
        }
        internal bool UpdateWeeklyLoginData()
        {
            bool needSync = false;
            if (IsDifferentDay(m_LastResetWeeklyLoginRewardTime))
            {
                ResetWeeklyLoginData();
                needSync = true;
            }
            return needSync;
        }

        private void ResetWeeklyLoginData()
        {
            LoginRewardInfo.IsGetLoginReward = false;
            DateTime now = DateTime.Now;
            m_LastResetWeeklyLoginRewardTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        }
        internal void SetWeeklyLoginRecord(int index)
        {
            LoginRewardInfo.AddToWeeklyLoginRewardRecordListWithCheck(index);
        }
        internal void ClearWeeklyLoginRecord()
        {
            LoginRewardInfo.ClearWeeklyLoginRewardRecordList();
        }
        internal bool UpdateSignInData()
        {
            bool needSync = false;
            // check used stamina
            if (UsedStamina >= c_StaminaToAddSignInCount)
            {
                UsedStamina -= c_StaminaToAddSignInCount;
                IncreaseSignInDailyCount();
                needSync = true;
            }

            // reset every month
            if (IsDifferentMonth(LastResetSignInRewardMonthCountTime))
            {
                ResetSignInMonthCount();
                needSync = true;
            }
            // reset every day
            if (IsDifferentDay(LastResetSignInRewardDailyCountTime))
            {
                ResetSignInDailyCount();
                needSync = true;
            }
            return needSync;
        }
        internal void ResetSignInMonthCount()
        {
            m_SignInCountCurMonth = 0;
            DateTime now = DateTime.Now;
            m_LastResetSignInRewardMonthCountTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        }
        internal void IncreaseSignInDailyCount()
        {
            ++m_RestDailySignInCount;
        }
        internal bool SignIn()
        {
            if (m_SignInCountCurMonth < DateTime.Now.Day)
            {
                if (m_RestDailySignInCount > 0)
                {
                    --m_RestDailySignInCount;
                    ++m_SignInCountCurMonth;
                    return true;
                }
            }
            return false;
        }
        internal void ResetSignInDailyCount()
        {
            m_RestDailySignInCount = c_MaxDailySignInCount;
            m_UsedStamina = 0;
            DateTime now = DateTime.Now;
            m_LastResetSignInRewardDailyCountTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        }
        internal bool UpdateDailyOnlineDuration()
        {
            bool needSync = false;
            if (IsDifferentDay(m_LastResetDaliyOnlineDurationTime))
            {
                ResetDailyOnlineDuration();
                needSync = true;
            }
            return needSync;
        }
        internal void ResetDailyOnlineDuration()
        {
            OnlineDuration.Clear();
            DateTime now = DateTime.Now;
            m_LastResetDaliyOnlineDurationTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            OnlineDuration.OnlineDurationStartTime = m_LastResetDaliyOnlineDurationTime;
        }
        internal int GetOnlineMinutes()
        {
            TimeSpan deltaTime = DateTime.Now - OnlineDuration.OnlineDurationStartTime;
            return OnlineDuration.DailyOnLineDuration + (int)deltaTime.TotalMinutes;
        }
        internal void ResetDailyMissions()
        {
            DateTime now = DateTime.Now;
            m_LastResetDailyMissionTime = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0);
            MissionSystem.Instance.LastResetDaliyMissionsTimeStamp = (m_LastResetDailyMissionTime.AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            MissionSystem.Instance.ResetDailyMissions(this);
            MissionSystem.Instance.ResetMonthCardMissions(this);
        }
        internal void InitDailyMissionsForLogin(double lastTimestamp)
        {
            if (MissionSystem.Instance.LastResetDaliyMissionsTimeStamp < lastTimestamp)
            {
                ResetDailyMissions();
            }
        }
        internal void ResetGowPrize()
        {
            DateTime now = DateTime.Now;
            m_LastResetGowPrizeTime = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0);
            GowSystem.LastResetGowPrizeTimeStamp = (m_LastResetGowPrizeTime.AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            GowInfo.IsAcquirePrize = false;
        }
        internal void InitGowPrizeForLogin(double lastTimestamp)
        {
            if (GowSystem.LastResetGowPrizeTimeStamp < lastTimestamp)
            {
                ResetGowPrize();
            }
        }
        private bool IsDifferentDay(double last_time)
        {
            DateTime dt = (new DateTime(1970, 1, 1, 0, 0, 0)).AddHours(8).AddSeconds(last_time);
            if (dt.Day != DateTime.Now.Day)
            {
                return true;
            }
            return false;
        }
        private bool IsDifferentDay(DateTime last_time)
        {
            if (last_time.Day != DateTime.Now.Day)
            {
                return true;
            }
            return false;
        }
        private bool IsDifferentMonth(DateTime last_time)
        {
            if (last_time.Month != DateTime.Now.Month)
            {
                return true;
            }
            return false;
        }
        internal void SetSceneInfo(int sceneId, int grade)
        {
            if (m_SceneInfo.ContainsKey(sceneId))
            {
                if (m_SceneInfo[sceneId] < grade)
                {
                    m_SceneInfo[sceneId] = grade;
                }
            }
            else
            {
                m_SceneInfo.TryAdd(sceneId, grade);
            }
        }
        internal int GetSceneInfo(int sceneId)
        {
            int ret;
            m_SceneInfo.TryGetValue(sceneId, out ret);
            return ret;
        }
        internal void AddCompletedSceneCount(int sceneId)
        {
            if (m_ScenesCompletedCountData.ContainsKey(sceneId))
            {
                m_ScenesCompletedCountData[sceneId]++;
            }
            else
            {
                m_ScenesCompletedCountData.TryAdd(sceneId, 1);
            }
        }
        internal void AddCompletedSceneCount(int sceneId, int count)
        {
            if (m_ScenesCompletedCountData.ContainsKey(sceneId))
            {
                m_ScenesCompletedCountData[sceneId] += count;
            }
            else
            {
                m_ScenesCompletedCountData.TryAdd(sceneId, count);
            }
        }
        internal int GetCompletedSceneCount(int sceneId)
        {
            int ret;
            m_ScenesCompletedCountData.TryGetValue(sceneId, out ret);
            return ret;
        }
        internal bool IsHaveMonthCard()
        {
            return DateTime.Now < m_MonthCardExpiredTime;
        }
        internal void AddMonthCardTime(int day)
        {
            if (IsHaveMonthCard())
            {
                m_MonthCardExpiredTime = m_MonthCardExpiredTime.AddDays(day);
            }
            else
            {
                m_MonthCardExpiredTime = DateTime.Now.AddDays(day);
            }
        }
        internal void InitGowHistoryForLogin(string gowHistoryTimeStr, string gowHistoryEloStr)
        {
            List<long> gowTimeList = new List<long>();
            string[] gowTimeArray = gowHistoryTimeStr.Split(new char[] { '|' });
            foreach (string str in gowTimeArray)
            {
                if (str.Trim() != string.Empty)
                {
                    long time = 0;
                    if (long.TryParse(str, out time))
                    {
                        gowTimeList.Add(time);
                    }
                }
            }
            List<int> gowEloList = new List<int>();
            string[] gowEloArray = gowHistoryEloStr.Split(new char[] { '|' });
            foreach (string str in gowEloArray)
            {
                if (str.Trim() != string.Empty)
                {
                    int elo = 0;
                    if (int.TryParse(str, out elo))
                    {
                        gowEloList.Add(elo);
                    }
                }
            }
            if (gowTimeList.Count == gowEloList.Count)
            {
                for (int i = 0; i < gowTimeList.Count; ++i)
                {
                    this.GowInfo.HistoryGowElos.Add(gowTimeList[i], gowEloList[i]);
                }
            }
        }
        internal ConcurrentDictionary<int, int> SceneData
        {
            get { return m_SceneInfo; }
        }
        internal ConcurrentDictionary<int, int> ScenesCompletedCountData
        {
            get { return m_ScenesCompletedCountData; }
        }
        public int NewbieScene
        {
            get { return m_NewbieScene; }
        }
        internal NewBieGuideInfo NewBieGuideInfo
        {
            get { return m_NewBieGuideInfo; }
            set { m_NewBieGuideInfo = value; }
        }
        internal int UsedStamina
        {
            get { return m_UsedStamina; }
            set { m_UsedStamina = value; }
        }
        internal int CurBuyStaminaCount
        {
            get { return m_CurBuyStaminaCount; }
            set { m_CurBuyStaminaCount = value; }
        }
        internal int CurBuyMoneyCount
        {
            get { return m_CurBuyMoneyCount; }
            set { m_CurBuyMoneyCount = value; }
        }
        internal double LastBuyMoneyTimestamp
        {
            get { return m_LastBuyMoneyTimestamp; }
            set { m_LastBuyMoneyTimestamp = value; }
        }
        internal int c_SellItemGainGoldMax = 100;
        internal int CurSellItemGoldIncome
        {
            get { return m_CurSellItemGoldIncome; }
            set { m_CurSellItemGoldIncome = value; }
        }
        internal double LastSellItemGoldIncomeTimestamp
        {
            get { return m_LastSellItemGoldIncomeTimestamp; }
            set { m_LastSellItemGoldIncomeTimestamp = value; }
        }
        internal DateTime LastResetDailyMissionTime
        {
            get { return m_LastResetDailyMissionTime; }
            set { m_LastResetDailyMissionTime = value; }
        }
        internal DateTime LastResetGowPrizeTime
        {
            get { return m_LastResetGowPrizeTime; }
            set { m_LastResetGowPrizeTime = value; }
        }
        internal int MaxEliteSceneCompletedCount
        {
            get { return m_MaxEliteSceneCompletedCount; }
            set { m_MaxEliteSceneCompletedCount = value; }
        }
        internal DateTime LastResetStaminaTime
        {
            get { return m_LastResetStaminaTime; }
            set { m_LastResetStaminaTime = value; }
        }
        internal DateTime LastResetMidasTouchTime
        {
            get { return m_LastResetMidasTouchTime; }
            set { m_LastResetMidasTouchTime = value; }
        }
        internal DateTime LastResetExchangeGoodsTime
        {
            get { return m_LastResetExchangeGoodsTime; }
            set { m_LastResetExchangeGoodsTime = value; }
        }
        internal DateTime LastResetAttemptAwardCountTime
        {
            get { return m_LastResetAttemptAwardCountTime; }
            set { m_LastResetAttemptAwardCountTime = value; }
        }
        internal DateTime LastResetGoldAwardCountTime
        {
            get { return m_LastResetGoldAwardCountTime; }
            set { m_LastResetGoldAwardCountTime = value; }
        }
        internal DateTime LastResetSellItemIncomeTime
        {
            get { return m_LastResetSellItemIncomeTime; }
            set { m_LastResetSellItemIncomeTime = value; }
        }
        internal DateTime LastResetCompletedScenesCountTime
        {
            get { return m_LastResetCompletedScenesCountTime; }
            set { m_LastResetCompletedScenesCountTime = value; }
        }
        internal DateTime LastResetWeeklyLoginRewardTime
        {
            get { return m_LastResetWeeklyLoginRewardTime; }
            set { m_LastResetWeeklyLoginRewardTime = value; }
        }
        internal int SignInCountCurMonth
        {
            get { return m_SignInCountCurMonth; }
            set { m_SignInCountCurMonth = value; }
        }
        internal int RestDailySignInCount
        {
            get { return m_RestDailySignInCount; }
            set { m_RestDailySignInCount = value; }
        }

        internal OnlineDurationInfo OnlineDuration
        {
            get { return m_OnlineDurationInfo; }
            set { m_OnlineDurationInfo = value; }
        }

        internal int c_StaminaToAddSignInCount = 100;
        internal System.DateTime LastResetSignInRewardMonthCountTime
        {
            get { return m_LastResetSignInRewardMonthCountTime; }
            set { m_LastResetSignInRewardMonthCountTime = value; }
        }
        internal System.DateTime LastResetSignInRewardDailyCountTime
        {
            get { return m_LastResetSignInRewardDailyCountTime; }
            set { m_LastResetSignInRewardDailyCountTime = value; }
        }
        internal System.DateTime MonthCardExpiredTime
        {
            get { return m_MonthCardExpiredTime; }
            set { m_MonthCardExpiredTime = value; }
        }
        internal System.DateTime LastResetDaliyOnlineDurationTime
        {
            get { return m_LastResetDaliyOnlineDurationTime; }
            set { m_LastResetDaliyOnlineDurationTime = value; }
        }
        internal DateTime CreateTime
        {
            get { return m_CreateTime; }
            set { m_CreateTime = value; }
        }
        internal double LastLoginTime
        {
            get { return m_LastLoginTime; }
            set { m_LastLoginTime = value; }
        }
        internal string ClientGameVersion
        {
            get { return m_ClientGameVersion; }
            set { m_ClientGameVersion = value; }
        }
        internal string ClientDeviceidId
        {
            get { return m_ClientDeviceidId; }
            set { m_ClientDeviceidId = value; }
        }
        internal int AttemptAward
        {
            get { return m_AttemptAward; }
            set { m_AttemptAward = value; }
        }
        internal int AttemptAcceptedAward
        {
            get { return m_AttemptAcceptedAward; }
            set { m_AttemptAcceptedAward = value; }
        }
        internal const int c_AttemptAcceptedHzMax = 1;
        internal int AttemptCurAcceptedCount
        {
            get { return m_AttemptCurAcceptedCount; }
            set { m_AttemptCurAcceptedCount = value; }
        }
        internal const int c_GoldAcceptedHzMax = 2;
        internal int GoldCurAcceptedCount
        {
            get { return m_GoldCurAcceptedCount; }
            set { m_GoldCurAcceptedCount = value; }
        }

        private int CaclGuideBit(int type)
        {
            int bit = 0;
            if ((int)MatchSceneEnum.Attempt == type)
            {
                bit = (int)GuideActionEnum.Attempt;
            }
            else if ((int)MatchSceneEnum.Gold == type)
            {
                bit = (int)GuideActionEnum.Gold;
            }
            else if ((int)MatchSceneEnum.Gow == type)
            {
                bit = (int)GuideActionEnum.Gow;
            }
            else if ((int)MatchSceneEnum.Arena == type)
            {
                bit = (int)GuideActionEnum.Arena;
            }
            else if ((int)MatchSceneEnum.Expedition == type)
            {
                bit = (int)GuideActionEnum.Expedition;
            }
            return bit;
        }
        internal void UpdateGuideFlag(int type)
        {
            int bit = CaclGuideBit(type);
            if (bit > 0 && 0 == (NewBieGuideInfo.GuideFlag >> bit & 1))
            {
                int flag = (int)NewBieGuideInfo.GuideFlag;
                flag |= (1 << bit);
                NewBieGuideInfo.GuideFlag = (long)flag;
                JsonMessageWithGuid gfMsg = new JsonMessageWithGuid(JsonMessageID.SyncGuideFlag);
                gfMsg.m_Guid = Guid;
                ArkCrossEngineMessage.Msg_LC_SyncGuideFlag protoData = new ArkCrossEngineMessage.Msg_LC_SyncGuideFlag();
                protoData.m_Flag = NewBieGuideInfo.GuideFlag;
                gfMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(NodeName, gfMsg);
            }
            LogSys.Log(LOG_TYPE.DEBUG, "GuideFlag : {0}", NewBieGuideInfo.GuideFlag);
        }
        internal bool CheckLevelup()
        {
            PlayerLevelupExpConfig cfg = PlayerConfigProvider.Instance.GetPlayerLevelupExpConfigById(Level);
            if (null != cfg)
            {
                if (ExpPoints >= cfg.m_ConsumeExp)
                {
                    do
                    {
                        Level += 1;
                        cfg = PlayerConfigProvider.Instance.GetPlayerLevelupExpConfigById(Level);
                    } while (null != cfg && ExpPoints >= cfg.m_ConsumeExp);

                    JsonMessageWithGuid ulMsg = new JsonMessageWithGuid(JsonMessageID.UserLevelup);
                    ulMsg.m_Guid = Guid;
                    ArkCrossEngineMessage.Msg_LC_UserLevelup protoData = new ArkCrossEngineMessage.Msg_LC_UserLevelup();
                    protoData.m_UserId = HeroId;
                    protoData.m_UserLevel = Level;
                    ulMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(NodeName, ulMsg);
                    return true;
                }
            }
            return false;
        }
        internal bool CanUseGmCommand
        {
            get
            {
                return GlobalVariables.Instance.IsDebug || IsGmAccount;
            }
        }
        internal bool IsGmAccount
        {
            get
            {
                return GmConfigProvider.Instance.IsGmAccount(m_Account);
            }
        }
        internal void Reset()
        {
            this.Guid = 0;
            this.Nickname = null;
            this.Account = null;
            this.AccountId = null;
            this.LogicServerId = 1;
            this.NodeName = null;
            this.Key = 0;
            this.Level = 1;
            this.ExpPoints = 0;
            this.Money = 0;
            this.Gold = 0;
            this.Vip = 0;

            this.LastNotifyMatchTime = 0;
            this.IsPrepared = false;

            this.CampId = 0;
            this.Room = null;
            this.CurrentRoomID = 0;
            this.CurrentBattleInfo.Reset();
            this.IsDisconnected = false;
            this.CurrentState = UserState.DropOrOffline;
            this.NextUserSaveCount = 1;
            this.CurrentUserSaveCount = 0;
            this.LastDSSaveTime = 0;
            this.LeftLife = 0;

            this.m_LastLoginTime = 0;
            this.m_ClientGameVersion = "0";
            this.m_ClientDeviceidId = "0";

            this.m_AttemptAward = 0;
            this.m_AttemptCurAcceptedCount = 0;
            this.m_AttemptAcceptedAward = 0;
            this.m_GoldCurAcceptedCount = 0;

            this.LastRequestDareTime = 0;

            this.m_SignInCountCurMonth = 0;
            this.m_RestDailySignInCount = 0;
            this.m_UsedStamina = 0;
            this.m_CurBuyStaminaCount = 0;
            this.m_LastAddStaminaTimestamp = 0.0;
            this.m_CurBuyMoneyCount = 0;
            this.m_LastBuyMoneyTimestamp = 0.0;
            this.m_CurSellItemGoldIncome = 0;
            this.m_LastSellItemGoldIncomeTimestamp = 0.0;
            this.m_LastResetStaminaTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetMidasTouchTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetExchangeGoodsTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetSellItemIncomeTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetDailyMissionTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetCompletedScenesCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetAttemptAwardCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetGoldAwardCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetSignInRewardMonthCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetSignInRewardDailyCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_MonthCardExpiredTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetDaliyOnlineDurationTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.m_LastResetWeeklyLoginRewardTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            this.m_LastResetGowPrizeTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            this.FriendInfos.Clear();
            this.Equip.Reset();
            this.ItemBag.Reset();
            this.Skill.Reset();
            this.Mission.Reset();
            this.MailStateInfo.Reset();
            this.CurStamina = 120;
            this.FightingScore = 0;
            this.SceneData.Clear();
            this.ScenesCompletedCountData.Clear();
            this.Expedition.ResetData();
            this.PartnerStateInfo.Reset();
            this.ExchangeGoodsInfo.Reset();
            this.GowInfo.Reset();
            this.NewBieGuideInfo.Clear();
            this.LoginRewardInfo.Clear();
            this.ActivityFinish.Clear();
            this.OnlineDuration.Clear();
        }
        ///
        private double m_LastLoginTime = 0;
        private string m_ClientGameVersion = "0";
        private string m_ClientDeviceidId = "0";
        ///
        private ulong m_Guid = 0;
        private string m_Account;       //角色所对应的客户端连接标识，现在实际为设备标识符
        private int m_LogicServerId = 1;
        private uint m_Key = 0;
        private string m_NodeName;
        private string m_AccountId;     //角色所属的账号ID
        private string m_Nickname;
        private int m_HeroId = 0;
        private int m_Level = 1;
        private int m_Vip = 0;
        private int m_Money = 0;
        private int m_Gold = 0;
        private int m_ExpPoints = 0;
        private int m_CitySceneId = 0;        //角色所在的主城场景ID
        private float m_X = 1;
        private float m_Z = 1;
        private float m_FaceDir = 0;
        private int m_FightingScore = 0;      //战力 
        private DateTime m_LastLogoutTime = new DateTime(1970, 1, 1, 0, 0, 0);
        // 好友数据
        private ConcurrentDictionary<ulong, FriendInfo> m_Friends = new ConcurrentDictionary<ulong, FriendInfo>();
        private EquipInfo m_EquipInfo = new EquipInfo();                            // 装备数据
        private ItemBag m_ItemBag = new ItemBag();                                  // 物品数据
        private SkillInfo m_SkillInfo = new SkillInfo();                            // 技能数据
        private MissionStateInfo m_MissionInfo = new MissionStateInfo();            // 任务数据
        private LegacyInfo m_LegacyInfo = new LegacyInfo();                         // 神器数据
        private XSoulInfo<ItemInfo> m_XSoulInfo = new XSoulInfo<ItemInfo>();        // X魂数据
        private ExpeditionPlayerInfo m_ExpeditionInfo = new ExpeditionPlayerInfo(); // 远征数据
        private GowInfo m_GowInfo = new GowInfo();                                  // 战神赛数据
        private PartnerStateInfo m_PartnerStateInfo = new PartnerStateInfo();       // 伙伴数据
        private PaymentInfo m_PaymentInfo = new PaymentInfo();                      // 消费记录

        private ConcurrentDictionary<int, int> m_SceneInfo = new ConcurrentDictionary<int, int>();      // 副本数据（副本id，通关星数）,目前只记录了通关信息，解锁相关未设计。
        private ConcurrentDictionary<int, int> m_ScenesCompletedCountData = new ConcurrentDictionary<int, int>(); // 记录场景通关次数
        private MailStateInfo m_MailStateInfo = new MailStateInfo();                // 玩家邮件状态数据
        private ExchangeGoodsInfo m_ExchangeGoodsInfo = new ExchangeGoodsInfo();    // 兑换物品数据
        private NewBieGuideInfo m_NewBieGuideInfo = new NewBieGuideInfo();          // 新手引导数据----- Modified   
        private LoginRewardInfo m_LoginRewardInfo = new LoginRewardInfo();          // 登陆奖励数据----- Modified    
        private ActivityFinishInfo m_ActivityFinishInfo = new ActivityFinishInfo(); // 活动完成数据----- Modefied   
        private OnlineDurationInfo m_OnlineDurationInfo = new OnlineDurationInfo(); // 在线时长数据----- Modefied   
                                                                                    // 精力值相关
        private int m_Vigor = 0;
        private double m_LastAddVigorTimestamp = 0.0;
        // 体力值相关
        private int m_CurStamina = 120;
        private int m_CurBuyStaminaCount = 0;
        private double m_LastAddStaminaTimestamp = 0.0;
        // 兑换金币相关
        private int m_CurBuyMoneyCount = 0;
        private double m_LastBuyMoneyTimestamp = 0.0;
        // 出售装备钻石收益
        private int m_CurSellItemGoldIncome = 0;
        private double m_LastSellItemGoldIncomeTimestamp = 0.0;
        // 本月签到数
        private int m_SignInCountCurMonth = 0;
        // 每日签到数
        private int m_RestDailySignInCount = 0;
        private int m_UsedStamina = 0;
        private const int c_MaxDailySignInCount = 1;

        private long m_LastRequestDareTime = 0;
        private int m_LeftLife = 0;
        private long m_LastDSSaveTime = 0;     //上一次数据存储的时间点    
        private bool m_IsRecycled = false;
        private object m_Lock = new object();

        private long m_LastNotifyMatchTime = 0;
        private bool m_IsPrepared = false;

        private WeakReference m_Group = null;
        private WeakReference m_Room = null;
        private int m_curRoomID = 0;
        private int m_CampId = 0;
        private UserBattleInfo m_curBattleInfo = new UserBattleInfo();
        private bool m_IsMachine = false;

        private bool m_IsDisconnected = false;
        private UserState m_CurrentState = UserState.DropOrOffline;
        private long m_NextUserSaveCount = 1;
        private long m_CurrentUserSaveCount = 0;       //数据存储计数,1...表示tick中定期存储；0表示正常退出
        private int m_NewbieScene = 1001;
        //Mpve
        private int m_AttemptAward = 0;
        private int m_AttemptCurAcceptedCount = 0;
        private int m_AttemptAcceptedAward = 0;
        private int m_GoldCurAcceptedCount = 0;

        //重置相关
        private DateTime m_LastResetStaminaTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetMidasTouchTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetExchangeGoodsTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetSellItemIncomeTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetDailyMissionTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetCompletedScenesCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetAttemptAwardCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetGoldAwardCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetSignInRewardMonthCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetSignInRewardDailyCountTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetWeeklyLoginRewardTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_MonthCardExpiredTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private DateTime m_LastResetDaliyOnlineDurationTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private int m_MaxEliteSceneCompletedCount = 3;
        private DateTime m_CreateTime = new DateTime(1970, 1, 1, 0, 0, 0);    //角色创建时间
        private long m_LastCheckDataTime = 0;
        private DateTime m_LastResetGowPrizeTime = new DateTime(1970, 1, 1, 0, 0, 0);
    }
}

