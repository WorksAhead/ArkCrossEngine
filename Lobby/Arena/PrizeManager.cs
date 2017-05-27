using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;
namespace Lobby
{
  internal class PrizeManager : IModuleMailHandler
  {
    public bool HaveMail(ModuleMailInfo moduleMailInfo, UserInfo user)
    {
      DateTime day = moduleMailInfo.m_SendTime;
      ArenaInfo arena_info = m_Rank.GetRankEntityById(user.Guid);
      if (arena_info == null) {
        return false;
      }
      int day_rank = arena_info.GetDaysRank(day);
      ArenaPrizeConfig prize = GetFitPrize(day_rank);
      if (prize == null) {
        return false;
      }
      return true;
    }

    public MailInfo GetMail(ModuleMailInfo moduleMailInfo, UserInfo user, bool onlyAttachment)
    {
      DateTime day = moduleMailInfo.m_SendTime;
      ArenaPrizeConfig prize = GetDayPrize(user.Guid, day);
      if (prize == null) {
        //LogSys.Log(LOG_TYPE.DEBUG, "not find player {0} date {1} prize!", user.Guid, day);
        return null;
      }
      MailInfo mail = moduleMailInfo.NewDerivedMailInfo();
      if (!onlyAttachment) {
        mail.m_Title = "$1111$";
        mail.m_Text = "$1112$";
        mail.m_Sender = "$1113$";
      }
      mail.m_Gold = prize.Gold;
      mail.m_Money = prize.Money;
      foreach (PrizeItemConfig item_config in prize.Items) {
        MailItem mail_item = new MailItem();
        mail_item.m_ItemId = item_config.ItemId;
        mail_item.m_ItemNum = item_config.ItemNum;
        mail.m_Items.Add(mail_item);
      }
      //LogSys.Log(LOG_TYPE.DEBUG, "player {0} got arena day {1} prize {2} isOnlyAttachment {3}", user.Guid, day, prize.Gold, onlyAttachment);
      return mail;
    }

    public ArkCrossEngine.ArenaPrizeConfig GetDayPrize(ulong guid, DateTime date)
    {
      ArenaPrizeConfig result = null;
      if (date > m_NextPrizeDate) {
        return null;
      }
      ArenaInfo arena_info = m_Rank.GetRankEntityById(guid);
      if (arena_info == null) {
        return null;
      }
      int day_rank = arena_info.GetDaysRank(date);
      result = GetFitPrize(day_rank);
      return result;
    }

    internal PrizeManager(Rank<ArenaInfo> rank, List<ArenaPrizeConfig> prize_rules, SimpleTime prizetime, MailSystem mailsystem)
    {
      m_Rank = rank;
      m_PrizeRules = prize_rules;
      m_PrizePresentTime = prizetime;
      m_MailSystem = mailsystem;
      m_NextPrizeDate = ArenaSystem.GetNextExcuteDate(m_PrizePresentTime);
      if (m_MailSystem != null) {
        m_MailSystem.RegisterModuleMailHandler(ModuleMailTypeEnum.ArenaModule, this);
      }
    }

    internal void Tick()
    {
      if (DateTime.Now > m_NextPrizeDate) {
        m_NextPrizeDate = ArenaSystem.GetNextExcuteDate(m_PrizePresentTime);
        ModuleMailInfo module_mail = new ModuleMailInfo();
        module_mail.m_Module = ModuleMailTypeEnum.ArenaModule;
        if (m_MailSystem != null) {
          m_MailSystem.SendModuleMail(module_mail, ArenaSystem.PRIZE_RETAIN_DAYS);
        }
      }
    }

    internal ArkCrossEngine.ArenaPrizeConfig GetFitPrize(int rank)
    {
      foreach (ArenaPrizeConfig rule in m_PrizeRules) {
        if (rank >= rule.FitBegin && rank < rule.FitEnd) {
          return rule;
        }
      }
      return null;
    }

    private Rank<ArenaInfo> m_Rank;
    private List<ArenaPrizeConfig> m_PrizeRules;
    private SimpleTime m_PrizePresentTime;
    private MailSystem m_MailSystem;
    private DateTime m_NextPrizeDate;
  }
}
