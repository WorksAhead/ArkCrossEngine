using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  class WeeklyLogInReward
  {
    internal void Init()
    {
      WeeklyLoginConfig config = WeeklyLoginConfigProvider.Instance.GetDataByType(ActivityTypeEnum.WEEKLY_LOGIN_REWARD);
      if (null != config) {
        m_StartTime = config.StartTime;
        m_EndTime = config.EndTime;
      }
    }

    internal void Tick()
    {
      long curTime = TimeUtility.GetServerMilliseconds();
      if (curTime - m_LastTickTime > m_TickInterval) {
        m_LastTickTime = curTime;
      }
    }

    internal bool IsInProgress()
    {
      return (DateTime.Now > m_StartTime && DateTime.Now < m_EndTime);
    }
    private DateTime m_StartTime;
    private DateTime m_EndTime;
    private long m_LastTickTime = 0;
    private long m_TickInterval = 60000;
  }
}
