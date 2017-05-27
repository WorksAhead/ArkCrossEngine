using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  internal class MatchGroup
  {
    internal ArenaInfo One;
    internal ArenaInfo Two;
    internal ArenaInfo Three;
  }

  internal class MatchRuleManager
  {
    internal MatchRuleManager(Rank<ArenaInfo> rank, List<ArkCrossEngine.ArenaMatchRuleConfig> rules)
    {
      m_Rank = rank;
      m_MatchRules = rules;
    }

    internal List<MatchGroup> GetMatchGroup(int rank, int group_count)
    {
      List<MatchGroup> result = new List<MatchGroup>();
      ArenaMatchRuleConfig rule = GetFitRule(rank);
      if (rule == null) {
        return result;
      }
      int oneBegin, twoBegin, threeBegin, threeEnd;
      GetRankMatchRange(rank, out oneBegin, out twoBegin, out threeBegin, out threeEnd);
      List<int> ones = GetRandomRank(rank, oneBegin, twoBegin, group_count);
      List<int> twos = GetRandomRank(rank, twoBegin, threeBegin, group_count);
      List<int> threes = GetRandomRank(rank, threeBegin, threeEnd, group_count);

      for (int i = 0; i < group_count; ++i) {
        MatchGroup group = new MatchGroup();
        if (ones.Count > i) {
          group.One = m_Rank.GetRankEntity(ones[i]);
        }
        if (twos.Count > i) {
          group.Two = m_Rank.GetRankEntity(twos[i]);
        }
        if (threes.Count > i) {
          group.Three = m_Rank.GetRankEntity(threes[i]);
        }
        result.Add(group);
      }
      return result;
    }

    internal void GetRankMatchRange(int rank, out int oneBegin, out int twoBegin, out int threeBegin, out int threeEnd)
    {
      ArenaMatchRuleConfig rule = GetFitRule(rank);
      if (rule == null) {
        oneBegin = twoBegin = threeBegin = threeEnd = -1;
        return;
      }
      oneBegin = GetLegalRank(rank - rule.OneBegin, m_Rank.MaxRank + 1);
      twoBegin = GetLegalRank(rank - rule.TwoBegin, m_Rank.MaxRank + 1);
      threeBegin = GetLegalRank(rank - rule.ThreeBegin, m_Rank.MaxRank + 1);
      threeEnd = GetLegalRank(rank - rule.ThreeEnd, m_Rank.MaxRank + 1);
    }

    // inclusive rank_begin
    internal static List<int> GetRandomRank(int except, int rank_begin, int rank_end, int count)
    {
      Random random = new Random();
      List<int> result = new List<int>();
      List<int> except_list = new List<int>();
      int end = rank_end - rank_begin;
      int self_fact = 0;
      if (except >= rank_begin && except < rank_end) {
        self_fact = 1;
      }
      if (self_fact > 0) {
        except_list.Add(except);
      }
      for (int c = 0; c < count; c++) {
        int left_count = end - c - self_fact;
        int random_rank;
        if (left_count <= 0) {
          random_rank = random.Next(0, end - self_fact);
          if (self_fact > 0 && random_rank >= except - rank_begin) {
            random_rank++;
          }
        } else {
          random_rank = random.Next(0, left_count);
          for (int k = 0; k < except_list.Count; ++k) {
            int rank = except_list[k];
            if (random_rank >= (rank - rank_begin)) {
              random_rank++;
            }
          }
        }
        result.Add(rank_begin + random_rank);
        except_list.Add(rank_begin + random_rank);
        except_list.Sort();
      }
      except_list.Clear();
      return result;
    }

    private ArkCrossEngine.ArenaMatchRuleConfig GetFitRule(int rank)
    {
      foreach (ArenaMatchRuleConfig rule in m_MatchRules) {
        if (rank >= rule.FitBegin && rank < rule.FitEnd) {
          return rule;
        }
      }
      return null;
    }

    private int GetLegalRank(int rank, int max_rank)
    {
      if (rank < 1) {
        return 1;
      }
      if (rank > max_rank) {
        return max_rank;
      }
      return rank;
    }

    private Rank<ArenaInfo> m_Rank;
    private List<ArkCrossEngine.ArenaMatchRuleConfig> m_MatchRules = new List<ArenaMatchRuleConfig>();
  }
}
