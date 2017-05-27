using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{

  internal sealed class PaymentRebateActivity
  {
    internal void Init()
    {
      m_MailSystem = new MailSystem();
    }
    internal void TryGivePaymentRebate(UserInfo user)
    {
      List<PaymentRebateConfig> activeConfigs = PaymentRebateConfigProvider.Instacne.GetUnderProgressData();
      for (int i = 0; i < activeConfigs.Count; ++i) {
        if (null != user && !user.ActivityFinish.FinishedActivities.Contains(activeConfigs[i].Id)) {
          int totalDiamonds = user.PaymentStateInfo.GetTotalBuyDiamondsAfterDate(activeConfigs[i].StartTime);
          if (totalDiamonds >= activeConfigs[i].TotalDiamond) {
            // finish activity
            GiveReward(user, activeConfigs[i]);
            user.ActivityFinish.AddToFinishedActivitiesList(activeConfigs[i].Id);
          }
        }
      }
    }
    private void GiveReward(UserInfo user, ArkCrossEngine.PaymentRebateConfig config)
    {
      MailInfo mail_info = new MailInfo();
      mail_info.m_Title = "充值返利";
      mail_info.m_Text = "充值返利";
      mail_info.m_LevelDemand = 0;
      mail_info.m_Money = config.Gold;
      mail_info.m_Gold = config.Diamond;
      mail_info.m_Stamina = 0;
      mail_info.m_Items = new List<MailItem>();
      for (int i = 0; i < config.ItemCount; ++i) {
        MailItem mail_item = new MailItem();
        mail_item.m_ItemId = config.ItemIdList[i];
        mail_item.m_ItemNum = config.ItemNumList[i];
        mail_info.m_Items.Add(mail_item);
      }
      mail_info.m_Receiver = user.Guid;
      m_MailSystem.SendUserMail(mail_info, 10);
    }

    MailSystem m_MailSystem = null;
  }
}
