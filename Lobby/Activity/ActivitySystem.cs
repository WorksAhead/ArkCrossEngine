using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobby
{
  class ActivitySystem
  {
    internal void Init()
    {
      m_WeelyLogInReward.Init();
      m_PaymentRebateActivity.Init();
    }

    internal void Tick()
    {
      m_WeelyLogInReward.Tick();
    }

    internal void TryGivePaymentRebate(UserInfo user)
    {
      m_PaymentRebateActivity.TryGivePaymentRebate(user);
    }
    private WeeklyLogInReward m_WeelyLogInReward = new WeeklyLogInReward();
    private PaymentRebateActivity m_PaymentRebateActivity = new PaymentRebateActivity();
  }
}
