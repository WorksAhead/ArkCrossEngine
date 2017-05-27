using System;
using System.Collections.Generic;
using DashFire;

namespace Lobby
{
  internal class Rank<T> where T : IRankEntity
  {
    internal Rank(int max_rank)
    {
      m_MaxRank = max_rank;
      m_RankEntityInfos = new T[m_MaxRank];
      for (int i = 0; i < m_MaxRank; ++i) {
        m_RankEntityInfos[i] = default(T);
      }
    }

    internal int MaxRank
    {
      get { return m_MaxRank; }
    }

    internal List<T> EntityList
    {
      get
      {
        List<T> entityList = new List<T>(m_RankEntityInfos);
        entityList.AddRange(m_UnRankedEntities.Values);
        return entityList;
      }
    }

    internal void AddUnRankEntity(T info)
    {
      m_UnRankedEntities[info.GetId()] = info;
      info.SetRank(ArenaSystem.UNKNOWN_RANK);
    }

    internal void RemoveUnRankEntity(ulong id)
    {
      m_UnRankedEntities.Remove(id);
    }

    internal T SetRankEntity(int target_rank, T entity)
    {
      if (!IsRankLegal(target_rank)) {
        target_rank = ArenaSystem.UNKNOWN_RANK;
      }
      if (!IsRankLegal(entity.GetRank())) {
        entity.SetRank(ArenaSystem.UNKNOWN_RANK);
      }
      T old_entity = default(T);
      if (entity == null) {
        return entity;
      }
      RemoveEntity(entity);
      if (IsRankLegal(target_rank)) {
        old_entity = m_RankEntityInfos[target_rank - 1];
        RemoveEntity(old_entity);
        m_RankEntityInfos[target_rank - 1] = entity;
        entity.SetRank(target_rank);
        m_GuidRankDict[entity.GetId()] = target_rank;
      } else {
        m_UnRankedEntities.Add(entity.GetId(), entity);
        entity.SetRank(target_rank);
      }
      return old_entity;
    }

    internal T RemoveEntity(T entity)
    {
      if (entity == null) {
        return entity;
      }
      if (!IsRankLegal(entity.GetRank())) {
        entity.SetRank(ArenaSystem.UNKNOWN_RANK);
        m_UnRankedEntities.Remove(entity.GetId());
      } else {
        m_RankEntityInfos[entity.GetRank() - 1] = default(T);
        m_GuidRankDict.Remove(entity.GetId());
        entity.SetRank(ArenaSystem.UNKNOWN_RANK);
      }
      return entity;
    }

    internal void ExchangeRank(T src, T dest)
    {
      int src_rank = src.GetRank();
      int dest_rank = dest.GetRank();
      if (src_rank == dest_rank) {
        return;
      }
      SetRankEntity(src_rank, dest);
      SetRankEntity(dest_rank, src);
    }

    internal T GetRankEntity(int rank)
    {
      if (IsRankLegal(rank)) {
        return m_RankEntityInfos[rank - 1];
      }
      return default(T);
    }

    internal int GetRankById(ulong guid)
    {
      int result = ArenaSystem.UNKNOWN_RANK;
      m_GuidRankDict.TryGetValue(guid, out result);
      return result;
    }

    internal T GetRankEntityById(ulong guid)
    {
      int rank = -1;
      if (m_GuidRankDict.TryGetValue(guid, out rank)) {
        return GetRankEntity(rank);
      }
      T result = default(T);
      m_UnRankedEntities.TryGetValue(guid, out result);
      return result;
    }

    internal Dictionary<ulong, T> GetUnRankedEntites()
    {
      return m_UnRankedEntities;
    }

    internal bool IsRankLegal(int rank)
    {
      int rank_index = rank - 1;
      if (0 <= rank_index && rank_index < m_MaxRank) {
        return true;
      }
      return false;
    }

    private T[] m_RankEntityInfos;
    private Dictionary<ulong, int> m_GuidRankDict = new Dictionary<ulong, int>();
    private Dictionary<ulong, T> m_UnRankedEntities = new Dictionary<ulong, T>();
    private int m_MaxRank;
  }
}
