using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  internal class GowRank
  {
    internal static void UpdateGowPoint(Tuple<UserInfo, int, bool> entity)
    {
      if (null == entity) return;
      UserInfo user = entity.Item1;
      int inc_score = entity.Item2;
      bool is_winner = entity.Item3;
      if (user.GowInfo.RankId > 0) {
        if (is_winner) {
          UpdateWinnerPoint(user, inc_score);
        } else {
          UpdateLoserPoint(user, inc_score);
        }
      } else {
        PlacementRankMatches(user, is_winner);
      }
    }
    internal static void UpdateGowRank(UserInfo entity)
    {
      if (null == entity) return;
      GowRankConfig cfg = null;
      if (GowSystem.RankData.TryGetValue(entity.GowInfo.RankId, out cfg)) {
        if (cfg.m_IsTriggerAdvance && IsInUpMetches(entity, cfg.m_Point)) {
          if (CanAdvanceRank(entity, cfg)) {
            entity.GowInfo.RankId += 1;
            entity.GowInfo.Point = 0;
            entity.GowInfo.ResetCriticalData();
          }
        } else if (cfg.m_IsTriggerReduced && IsInDownMetches(entity)) {
          if (CanReduceRank(entity, cfg)) {
            entity.GowInfo.RankId -= 1;
            entity.GowInfo.Point = 0;
            entity.GowInfo.ResetCriticalData();
          }
        }
      }

      LogSys.Log(LOG_TYPE.INFO, "gowrank entitynickname:{0},rank:{1},point:{2},total:{3},win:{4},loss:{5}",
        entity.Nickname, entity.GowInfo.RankId, entity.GowInfo.Point, entity.GowInfo.CriticalTotalMatches, entity.GowInfo.AmassWinMatches, entity.GowInfo.AmassLossMatches);
    }

    private static void PlacementRankMatches(UserInfo user, bool is_winner)
    {
      if (is_winner) {
        user.GowInfo.IncreaseWinMatches();
      } else {
        user.GowInfo.IncreaseLossMatches();
      }
      if (CanPlacementRank(user)) {
        int win = user.GowInfo.AmassWinMatches;
        int real_rank = 1 + win;
        user.GowInfo.ResetCriticalData();
        user.GowInfo.RankId = real_rank;

        LogSys.Log(LOG_TYPE.INFO, "placement rank entitynickname:{0},rank:{1},point:{2},total:{3},win:{4},loss:{5}",
          user.Nickname, user.GowInfo.RankId, user.GowInfo.Point, user.GowInfo.CriticalTotalMatches, user.GowInfo.AmassWinMatches, user.GowInfo.AmassLossMatches);
      }
    }
    private static bool CanPlacementRank(UserInfo user)
    {
      int total = user.GowInfo.CriticalTotalMatches;
      if (total < GowSystem.c_PlacementRankMatches) {
        return false;
      }
      return true;
    }
    private static bool CanReduceRank(UserInfo user, GowRankConfig cfg)
    {
      int win = user.GowInfo.AmassWinMatches;
      int loss = user.GowInfo.AmassLossMatches;
      if (loss >= cfg.m_LossesMatches) {
        return true;
      }
      return false;
    }
    private static bool CanAdvanceRank(UserInfo user, GowRankConfig cfg)
    {
      int win = user.GowInfo.AmassWinMatches;
      int loss = user.GowInfo.AmassLossMatches;
      int allow_loss_count = cfg.m_TotalMatches - cfg.m_WinMatches;
      if (loss > allow_loss_count) {
        return false;
      }
      if (win < cfg.m_WinMatches) {
        return false;
      }
      return true;
    }
    private static void IncreasePoint(UserInfo user, int income, int max)
    {
      int real_inc = (int)(income * GowSystem.c_EloToPointRate);
      int cur_point = user.GowInfo.Point;
      if (cur_point + real_inc > max) {
        user.GowInfo.Point = max;
      } else {
        user.GowInfo.Point += real_inc;
      }
    }
    private static void ReducePoint(UserInfo user, int reduce)
    {
      int real_reduce = (int)(reduce * GowSystem.c_EloToPointRate);
      int cur_point = user.GowInfo.Point;
      if (cur_point + real_reduce < 0) {
        user.GowInfo.Point = 0;
      } else {
        user.GowInfo.Point += real_reduce;
      }
    }
    private static bool IsInUpMetches(UserInfo user, int max)
    {
      return user.GowInfo.Point >= max;
    }
    private static bool IsInDownMetches(UserInfo user)
    {
      return user.GowInfo.Point <= 0;
    }
    private static void UpdateWinnerPoint(UserInfo user, int income)
    {
      if (null == user) return;
      GowRankConfig cfg = null;
      if (GowSystem.RankData.TryGetValue(user.GowInfo.RankId, out cfg)) {
        if (cfg.m_IsTriggerAdvance && IsInUpMetches(user, cfg.m_Point)) {
          user.GowInfo.IncreaseWinMatches();
        } else if (cfg.m_IsTriggerReduced && IsInDownMetches(user)) {
          user.GowInfo.ResetCriticalData();
          IncreasePoint(user, income, cfg.m_Point);
        } else {
          IncreasePoint(user, income, cfg.m_Point);
        }
      }
    }
    private static void UpdateLoserPoint(UserInfo user, int reduce)
    {
      if (null == user) return;
      GowRankConfig cfg = null;
      if (GowSystem.RankData.TryGetValue(user.GowInfo.RankId, out cfg)) {
        if (cfg.m_IsTriggerReduced && IsInDownMetches(user)) {
          user.GowInfo.IncreaseLossMatches();
        } else if (cfg.m_IsTriggerAdvance && IsInUpMetches(user, cfg.m_Point)) {
          user.GowInfo.IncreaseLossMatches();
        } else {
          ReducePoint(user, reduce);
        }
      }
    }
  }
}
