using System;
using System.Collections.Generic;

namespace Lobby
{
  internal sealed class GowInfo
  {
    internal int GowElo
    {
      get { return m_GowElo; }
      set { m_GowElo = value; }
    }
    internal int GowMatches
    {
      get { return m_GowMatches; }
      set { m_GowMatches = value; }
    }
    internal int GowWinMatches
    {
      get { return m_GowWinMatches; }
      set { m_GowWinMatches = value; }
    }
    internal int LeftMatchCount
    {
      get { return m_LeftMatchCount; }
      set { m_LeftMatchCount = value; }
    }
    internal DateTime LastBuyTime
    {
      get { return m_LastBuyTime; }
      set { m_LastBuyTime = value; }
    }
    internal int LeftBuyCount
    {
      get { return m_LeftBuyCount; }
      set { m_LeftBuyCount = value; }
    }
    internal SortedList<long, int> HistoryGowElos
    {
      get { return m_HistoryGowElos; }
    }
    internal int RankId
    {
      get { return m_RankId; }
      set {
        if (value < 0) {
          m_RankId = 0;
        } else {
          m_RankId = value;
        }
      }
    }
    internal int Point
    {
      get { return m_Point; }
      set { m_Point = value; }
    }
    internal int CriticalTotalMatches
    {
      get { return m_CriticalTotalMatches; }
    }
    internal int AmassWinMatches
    {
      get { return m_CriticalAmassWinMatches; }
    }
    internal int AmassLossMatches
    {
      get { return m_CriticalAmassLossMatches; }
    }
    internal bool IsAcquirePrize
    {
      get { return m_IsAcquirePrize; }
      set { m_IsAcquirePrize = value; }
    }
    internal void Reset()
    {
      m_GowElo = 1000;
      m_GowMatches = 0;
      m_GowWinMatches = 0;
      m_LeftMatchCount = 0;     
      m_HistoryGowElos.Clear();

      m_RankId = 0;
      m_Point = 0;
      m_CriticalTotalMatches = 0;
      m_CriticalAmassWinMatches = 0;
      m_CriticalAmassLossMatches = 0;

      m_IsAcquirePrize = false;
    }
    internal void InitRankInfo(int rank, int point, int total, int win, int loss)
    {
      m_RankId = rank;
      m_Point = point;
      m_CriticalTotalMatches = total;
      m_CriticalAmassWinMatches = win;
      m_CriticalAmassLossMatches = loss;
    }
    internal void IncreaseWinMatches()
    {
      m_CriticalTotalMatches += 1;
      m_CriticalAmassWinMatches += 1;
    }
    internal void IncreaseLossMatches()
    {
      m_CriticalTotalMatches += 1;
      m_CriticalAmassLossMatches += 1;
    }
    internal void ResetCriticalData()
    {
      m_CriticalTotalMatches = 0;
      m_CriticalAmassWinMatches = 0;
      m_CriticalAmassLossMatches = 0;
    }

    private int m_GowElo = 1000;
    private int m_GowMatches = 0;
    private int m_GowWinMatches = 0;
    private int m_LeftMatchCount = 0;
    private DateTime m_LastBuyTime;
    private int m_LeftBuyCount = 0;
    private SortedList<long, int> m_HistoryGowElos = new SortedList<long, int>();
    
    private int m_RankId = 0;
    private int m_Point = 0;
    private int m_CriticalTotalMatches = 0;
    private int m_CriticalAmassWinMatches = 0;
    private int m_CriticalAmassLossMatches = 0;

    private bool m_IsAcquirePrize = false;
  }
}
