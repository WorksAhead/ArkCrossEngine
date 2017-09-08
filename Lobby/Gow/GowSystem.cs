using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal sealed class GowSystem : IModuleMailHandler
    {
        internal const int c_PrizeValidityPeriod = 7;
        internal static Dictionary<int, GowRankConfig> RankData
        {
            get { return m_RankData; }
        }
        internal List<GowStarInfo> GowStarList
        {
            get { return m_GowStars; }
        }
        internal static double LastResetGowPrizeTimeStamp
        {
            get { return m_LastResetGowPrizeTimeStamp; }
            set { m_LastResetGowPrizeTimeStamp = value; }
        }
        internal void Init(MailSystem mailSystem)
        {
            m_MailSystem = mailSystem;
            mailSystem.RegisterModuleMailHandler(ModuleMailTypeEnum.GowModule, this);

            GowFormulaConfig cfg1 = GowConfigProvider.Instance.GetGowFormulaConfig((int)GowFormulaConfig.FormulaNameEnum.Upper);
            GowFormulaConfig cfg2 = GowConfigProvider.Instance.GetGowFormulaConfig((int)GowFormulaConfig.FormulaNameEnum.Lower);
            GowFormulaConfig cfg3 = GowConfigProvider.Instance.GetGowFormulaConfig((int)GowFormulaConfig.FormulaNameEnum.K2_1);
            GowFormulaConfig cfg4 = GowConfigProvider.Instance.GetGowFormulaConfig((int)GowFormulaConfig.FormulaNameEnum.K2_2);
            GowFormulaConfig cfg5 = GowConfigProvider.Instance.GetGowFormulaConfig((int)GowFormulaConfig.FormulaNameEnum.K1);
            GowFormulaConfig cfg6 = GowConfigProvider.Instance.GetGowFormulaConfig((int)GowFormulaConfig.FormulaNameEnum.K3);
            GowFormulaConfig cfg7 = GowConfigProvider.Instance.GetGowFormulaConfig((int)GowFormulaConfig.FormulaNameEnum.TC);
            if (null != cfg1 && cfg1.m_Value > 0)
            {
                m_Upper = cfg1.m_Value;
            }
            if (null != cfg2 && cfg2.m_Value > 0)
            {
                m_Lower = cfg2.m_Value;
            }
            if (null != cfg3 && cfg3.m_Value > 0)
            {
                m_K2_1 = cfg3.m_Value;
            }
            if (null != cfg4 && cfg4.m_Value > 0)
            {
                m_K2_2 = cfg4.m_Value;
            }
            if (null != cfg5 && cfg5.m_Value > 0)
            {
                m_K1 = cfg5.m_Value;
            }
            if (null != cfg6 && cfg6.m_Value > 0)
            {
                m_K3 = cfg6.m_Value;
            }
            if (null != cfg7 && cfg7.m_Value > 0)
            {
                m_TC = cfg7.m_Value;
            }
            List<GowTimeConfig> times = GowConfigProvider.Instance.GowTimeConfigMgr.GetData();
            foreach (GowTimeConfig cfg in times)
            {
                if ((int)GowTimeConfig.TimeTypeEnum.PrizeTime == cfg.m_Type)
                {
                    m_PrizeTime = new Time(cfg.m_StartHour, cfg.m_StartMinute, cfg.m_StartSecond);
                }
                else
                {
                    GowTime time = new GowTime();
                    time.m_StartTime = new Time(cfg.m_StartHour, cfg.m_StartMinute, cfg.m_StartSecond);
                    time.m_EndTime = new Time(cfg.m_EndHour, cfg.m_EndMinute, cfg.m_EndSecond);
                    m_GowTimes.Add(time);
                }
            }
            InitGowRankData();
            InitGowPrizeData();
        }
        internal void UpdateElo(UserInfo user)
        {
            GowStarInfo starInfo = new GowStarInfo();
            starInfo.m_Guid = user.Guid;
            starInfo.m_GowElo = user.GowInfo.GowElo;
            starInfo.m_Nick = user.Nickname;
            starInfo.m_HeroId = user.HeroId;
            starInfo.m_Level = user.Level;
            starInfo.m_FightingScore = user.FightingScore;
            starInfo.m_RankId = user.GowInfo.RankId;
            starInfo.m_Point = user.GowInfo.Point;
            starInfo.m_CriticalTotalMatches = user.GowInfo.CriticalTotalMatches;
            starInfo.m_CriticalAmassWinMatches = user.GowInfo.AmassWinMatches;
            starInfo.m_CriticalAmassLossMatches = user.GowInfo.AmassLossMatches;

            InsertGowStar(starInfo);
        }
        internal void GetStarList(ulong guid, int start, int count)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = scheduler.GetUserInfo(guid);
            if (null != user)
            {
                if (start < 0)
                    start = 0;
                int ct = m_GowStars.Count;
                int end = start + count;
                if (end > ct)
                    end = ct;
                JsonMessageWithGuid aaMsg = new JsonMessageWithGuid(JsonMessageID.SyncGowStarList);
                aaMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_SyncGowStarList protoData = new ArkCrossEngineMessage.Msg_LC_SyncGowStarList();
                for (int i = start; i < end; ++i)
                {
                    UserInfo member = scheduler.GetUserInfo(m_GowStars[i].m_Guid);
                    if (null != member)
                    {
                        m_GowStars[i].m_Level = member.Level;
                        m_GowStars[i].m_FightingScore = member.FightingScore;
                    }
                    ArkCrossEngineMessage.Msg_LC_SyncGowStarList.GowStarInfoForMessage info = new ArkCrossEngineMessage.Msg_LC_SyncGowStarList.GowStarInfoForMessage();
                    info.m_Guid = m_GowStars[i].m_Guid;
                    info.m_GowElo = m_GowStars[i].m_GowElo;
                    info.m_Nick = m_GowStars[i].m_Nick;
                    info.m_HeroId = m_GowStars[i].m_HeroId;
                    info.m_Level = m_GowStars[i].m_Level;
                    info.m_FightingScore = m_GowStars[i].m_FightingScore;
                    info.m_RankId = m_GowStars[i].m_RankId;
                    info.m_Point = m_GowStars[i].m_Point;
                    info.m_CriticalTotalMatches = m_GowStars[i].m_CriticalTotalMatches;
                    info.m_CriticalAmassWinMatches = m_GowStars[i].m_CriticalAmassWinMatches;
                    info.m_CriticalAmassLossMatches = m_GowStars[i].m_CriticalAmassLossMatches;
                    protoData.m_Stars.Add(info);
                }
                if (protoData.m_Stars.Count > 0)
                {
                    aaMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, aaMsg);
                }
            }
        }
        internal void InitGowStarData(List<GowStarInfo> gowstarList)
        {
            for (int i = 0; i < gowstarList.Count; ++i)
            {
                InsertGowStar(gowstarList[i]);
                if (i > c_MaxGowStarNum)
                {
                    break;
                }
            }
            //上一次发奖的时间
            List<ModuleMailInfo> moduleMails = m_MailSystem.GetModuleMailList(ModuleMailTypeEnum.GowModule);
            DateTime last = new DateTime(1984, 1, 1);
            if (moduleMails.Count > 0)
            {
                last = moduleMails[0].m_SendTime;
                for (int i = 1; i < moduleMails.Count; ++i)
                {
                    if (last < moduleMails[i].m_SendTime)
                    {
                        last = moduleMails[i].m_SendTime;
                    }
                }
            }
            m_LastPrizeDate = last.Date;
            m_IsDataLoaded = true;
        }
        internal void HandleRequestGowPrize(ulong guid)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = scheduler.GetUserInfo(guid);
            if (null == user || null == user.GowInfo) return;
            GowPrizeConfig cfg = GowConfigProvider.Instance.FindGowPrizeConfig(user.GowInfo.RankId);
            if (null != cfg)
            {
                ProvideGowPrize(guid, cfg);
            }
        }
        internal void Tick()
        {
            var ds_thread = LobbyServer.Instance.DataStoreThread;
            if (ds_thread.DataStoreAvailable)
            {
                if (!m_IsDataLoaded)
                {
                    return;
                }
            }
            /*
            /// 发奖     
            DateTime time = DateTime.Now;
            if (m_LastPrizeDate != time.Date) {
              int seconds = Time.CalcSeconds(time.Hour, time.Minute, time.Second);
              if (seconds > m_PrizeTime.CalcSeconds()) {
                if (null != m_MailSystem && 0 != m_LastPrizeDate.CompareTo(new DateTime(1984, 1, 1))) {
                  ModuleMailInfo mail = new ModuleMailInfo();
                  mail.m_Module = ModuleMailTypeEnum.GowModule;
                  m_MailSystem.SendModuleMail(mail, c_PrizeValidityPeriod);
                }
                m_LastPrizeDate = time.Date;
              }
            }
            */
        }
        public bool HaveMail(ModuleMailInfo moduleMailInfo, UserInfo user)
        {
            if (user.GowInfo.GowElo > 100)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public MailInfo GetMail(ModuleMailInfo moduleMailInfo, UserInfo user, bool onlyAttachment)
        {
            MailInfo mailInfo = null;
            mailInfo = moduleMailInfo.NewDerivedMailInfo();
            if (!onlyAttachment)
            {
                mailInfo.m_Title = "$1$";
                mailInfo.m_Text = "$2$";
                mailInfo.m_Sender = "$3$";
            }
            int gowElo = 0;
            DateTime sendDate = mailInfo.m_SendTime.Date;
            DateTime sendTime = new DateTime(sendDate.Year, sendDate.Month, sendDate.Day, m_PrizeTime.m_Hour, m_PrizeTime.m_Minute, m_PrizeTime.m_Second);
            long key = sendTime.ToBinary();
            SortedList<long, int> history = user.GowInfo.HistoryGowElos;
            foreach (long key0 in history.Keys)
            {
                if (key >= key0)
                {
                    gowElo = history[key0];
                    break;
                }
            }
            if (gowElo > 0)
            {
                GowPrizeConfig cfg = GowConfigProvider.Instance.FindGowPrizeConfig(user.GowInfo.RankId);
                if (null != cfg)
                {
                    mailInfo.m_Money = cfg.Money;
                    mailInfo.m_Gold = cfg.Gold;
                    foreach (GowPrizeItem item_config in cfg.Items)
                    {
                        MailItem mail_item = new MailItem();
                        mail_item.m_ItemId = item_config.ItemId;
                        mail_item.m_ItemNum = item_config.ItemNum;
                        mailInfo.m_Items.Add(mail_item);
                    }
                    return mailInfo;
                }
            }
            return null;
        }
        internal struct Time
        {
            internal int m_Hour;
            internal int m_Minute;
            internal int m_Second;

            internal int CalcSeconds()
            {
                return CalcSeconds(m_Hour, m_Minute, m_Second);
            }
            internal static int CalcSeconds(int hour, int minute, int second)
            {
                return hour * 3600 + minute * 60 + second;
            }

            internal Time(int hour, int minute, int second)
            {
                m_Hour = hour;
                m_Minute = minute;
                m_Second = second;
            }
        }
        internal static bool CanMatch()
        {
            bool ret = false;
            DateTime time = DateTime.Now;
            int seconds = Time.CalcSeconds(time.Hour, time.Minute, time.Second);
            int ct = m_GowTimes.Count;
            for (int ix = 0; ix < ct; ++ix)
            {
                GowTime gowTime = m_GowTimes[ix];
                int start = gowTime.m_StartTime.CalcSeconds();
                int end = gowTime.m_EndTime.CalcSeconds();
                if (seconds >= start && seconds <= end)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        private void ProvideGowPrize(ulong guid, GowPrizeConfig cfg)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = scheduler.GetUserInfo(guid);
            if (null == user) return;
            JsonMessageWithGuid pgpMsg = new JsonMessageWithGuid(JsonMessageID.RequestGowPrize);
            pgpMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_RequestGowPrizeResult protoData = new ArkCrossEngineMessage.Msg_LC_RequestGowPrizeResult();
            if (user.GowInfo.IsAcquirePrize)
            {
                protoData.m_Result = (int)GeneralOperationResult.LC_Failure_Unknown;
            }
            else
            {
                int money_incr = cfg.Money;
                int gold_incr = cfg.Gold;
                protoData.m_Money = money_incr;
                protoData.m_Gold = gold_incr;
                scheduler.DispatchAction(scheduler.DoAddAssets, guid, money_incr, gold_incr, 0, 0, GainConsumePos.Gow.ToString());
                if (null != cfg.Items && cfg.Items.Count > 0)
                {
                    for (int i = 0; i < cfg.Items.Count; ++i)
                    {
                        GowPrizeItem item = cfg.Items[i];
                        if (null != item)
                        {
                            scheduler.DispatchAction(scheduler.DoAddItem, guid, item.ItemId, item.ItemNum, GainConsumePos.Gow.ToString());
                            ArkCrossEngineMessage.Msg_LC_RequestGowPrizeResult.AwardItemInfo itemDataMsg =
                              new ArkCrossEngineMessage.Msg_LC_RequestGowPrizeResult.AwardItemInfo();
                            itemDataMsg.m_Id = item.ItemId;
                            itemDataMsg.m_Num = item.ItemNum;
                            protoData.m_Items.Add(itemDataMsg);
                        }
                    }
                }
                protoData.m_Result = (int)GeneralOperationResult.LC_Succeed;
                user.GowInfo.IsAcquirePrize = true;
            }
            pgpMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, pgpMsg);
        }
        private void InsertGowStar(GowStarInfo gsi)
        {
            if (gsi == null || gsi.m_RankId <= 0)
            {
                return;
            }
            GowStarInfo starInfo = m_GowStars.Find(info => info.m_Guid == gsi.m_Guid);
            if (null != starInfo)
            {
                m_GowStars.Remove(starInfo);
            }
            starInfo = gsi;
            int ct = m_GowStars.Count;
            if (ct <= 0)
            {
                m_GowStars.Add(starInfo);
            }
            else
            {
                int min = 0;
                int max = ct - 1;
                while (min <= max)
                {
                    int i = (min + max) / 2;
                    GowStarInfo info = m_GowStars[i];
                    if (info.m_RankId <= starInfo.m_RankId
                      && info.m_Level < starInfo.m_Level)
                    {
                        max = i - 1;
                    }
                    else
                    {
                        min = i + 1;
                    }
                }
                m_GowStars.Insert(min, starInfo);
            }
            while (m_GowStars.Count > c_MaxGowStarNum)
            {
                m_GowStars.RemoveAt(c_MaxGowStarNum);
            }
        }
        private bool InitGowRankData()
        {
            m_RankData.Clear();
            List<GowRankConfig> ranks = GowConfigProvider.Instance.GowRankConfigMgr.GetData();
            foreach (GowRankConfig cfg in ranks)
            {
                m_RankData.Add(cfg.m_RankId, cfg);
            }
            return true;
        }
        private bool InitGowPrizeData()
        {
            List<GowPrizeConfig> rules = new List<GowPrizeConfig>();
            foreach (GowPrizeConfig rule in GowConfigProvider.Instance.GowPrizeConfigMgr.GetData())
            {
                m_PrizeData.Add(rule);
            }
            return true;
        }

        internal struct GowTime
        {
            internal Time m_StartTime;
            internal Time m_EndTime;
        }

        internal const int c_PlacementRankMatches = 5;
        internal const float c_EloToPointRate = 2.5f;
        internal static int m_UnlockLevel = 20;
        internal static int m_Upper = 2400;
        internal static int m_Lower = 2000;
        internal static int m_K2_1 = 130;
        internal static int m_K2_2 = 20;
        internal static int m_K1 = 10;
        internal static int m_K3 = 30;
        internal static int m_TC = 60000;
        internal static Time m_PrizeTime;
        internal static List<GowTime> m_GowTimes = new List<GowTime>();

        private bool m_IsDataLoaded = false;

        private const int c_MaxGowStarNum = 100;
        private List<GowStarInfo> m_GowStars = new List<GowStarInfo>();
        private DateTime m_LastPrizeDate = new DateTime(1984, 1, 1);
        private MailSystem m_MailSystem = null;

        private static double m_LastResetGowPrizeTimeStamp = 0;
        private List<GowPrizeConfig> m_PrizeData = new List<GowPrizeConfig>();
        private static Dictionary<int, GowRankConfig> m_RankData = new Dictionary<int, GowRankConfig>();
    }
}
