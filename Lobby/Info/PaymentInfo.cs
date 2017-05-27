using System;
using System.Collections.Generic;

namespace Lobby
{
  public sealed class PaymentItem
  {
    public int m_OrderId;
    public int m_Diamond;
    public DateTime m_Time;
  }
  public sealed class PaymentInfo
  {
    public void AddPaymentItem(int orderId, int diamond, DateTime time)
    {
      lock (m_Lock) {
        PaymentItem item = new PaymentItem();
        item.m_OrderId = orderId;
        item.m_Diamond= diamond;
        item.m_Time = time;
        m_Payments.Add(item);
      }
    }
    public int GetTotalBuyDiamondsAfterDate(DateTime time)
    {
      int result = 0;
      lock (m_Lock) {
        for (int i = 0; i < m_Payments.Count; ++i) {
          if (m_Payments[i].m_Time > time) {
            result += m_Payments[i].m_Diamond;
          }
        }
      }
      return result;
    }
    public List<PaymentItem> Payments
    {
      get { return m_Payments; }
    }

    private object m_Lock = new object();
    private List<PaymentItem> m_Payments = new List<PaymentItem>();
  }
}
