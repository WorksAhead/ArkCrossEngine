using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  internal class ChallengeManager
  {
    internal Dictionary<ulong, List<ChallengeInfo>> ChallengeHistory
    {
      get { return m_ChallengeHistory; }
    }
    internal ChallengeManager(Rank<ArenaInfo> rank_manager, long max_fight_time, int max_history_count)
    {
      m_Rank = rank_manager;
      m_MaxFightTime = max_fight_time;
      m_MaxHistoryCount = max_history_count;
    }

    internal void BeginChallenge(ArenaInfo challenger, ArenaInfo target, int sign)
    {
      ChallengeInfo doing = GetDoingChallengeInfo(challenger.GetId());
      if (doing != null) {
        ChallengeResult(doing, false);
      }
      if (challenger.LeftFightCount <= 0) {
        return;
      }
      challenger.LeftFightCount -= 1;
      ChallengeInfo info = new ChallengeInfo();
      info.Sign = sign;
      info.Challenger = CreateChallengeEntityInfo(challenger);
      info.Target = CreateChallengeEntityInfo(target);
      info.IsChallengerSuccess = false;
      info.IsDone = false;
      info.ChallengeBeginTime = DateTime.Now;
      info.BeginFightTime = info.ChallengeBeginTime.AddMilliseconds(ArenaSystem.DEFAULT_READY_FIGHT_MS);
      info.ChallengeDeadLine = info.BeginFightTime.AddMilliseconds(m_MaxFightTime + ArenaSystem.FIGHT_TIME_DEVIATION_MS);
      challenger.LastBattleTime = info.BeginFightTime.AddMilliseconds(m_MaxFightTime);
      m_DoingChallenges.Add(challenger.GetId(), info);
    }

    internal void Tick()
    {
    }

    internal ChallengeInfo GetDoingChallengeInfo(ulong guid)
    {
      ChallengeInfo result = null;
      m_DoingChallenges.TryGetValue(guid, out result);
      return result;
    }

    internal bool IsChallengeOverTime(ChallengeInfo info)
    {
      if (info.ChallengeDeadLine >= DateTime.Now) {
        return false;
      } else {
        return true;
      }
    }

    internal void ChallengeResult(ChallengeInfo info, bool IsSuccess)
    {
      if (info.IsDone) {
        return;
      }
      ArenaInfo challenger = m_Rank.GetRankEntityById(info.Challenger.Guid);
      ArenaInfo target = m_Rank.GetRankEntityById(info.Target.Guid);
      info.Challenger.Rank = challenger.GetRank();
      info.Target.Rank = target.GetRank();
      if (IsSuccess && IsRankShouldChange(info.Challenger.Rank, info.Target.Rank)) {
        m_Rank.ExchangeRank(challenger, target);
      }
      info.IsDone = true;
      info.IsChallengerSuccess = IsSuccess;
      if (IsChallengeOverTime(info)) {
        info.ChallengeEndTime = info.ChallengeDeadLine;
        challenger.LastBattleTime = info.ChallengeDeadLine;
      } else {
        info.ChallengeEndTime = DateTime.Now;
        challenger.LastBattleTime = DateTime.Now;
      }
      m_DoingChallenges.Remove(challenger.GetId());
      AddChallengeHistory(info.Challenger.Guid, info);
      AddChallengeHistory(info.Target.Guid, info);
      UserInfo challenge_user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(info.Challenger.Guid);
      SendResultMsg(info, challenge_user);
      UserInfo target_user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(info.Target.Guid);
      SendResultMsg(info, target_user);
      LogSys.Log(LOG_TYPE.DEBUG, "-----send challenge result msg");
      ///
      RecordChallengeAction(challenger, target, IsSuccess);
    }

    private void RecordChallengeAction(ArenaInfo challenger, ArenaInfo target, bool IsSuccess)
    {
      DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
      if (null == challenger && null == target)
        return;
      UserInfo user1 = dataProcess.GetUserInfo(challenger.GetId());
      UserInfo user2 = dataProcess.GetUserInfo(target.GetId());
      if (null == user1 || null == user2)
        return;
      AccountInfo challenger_acc = dataProcess.FindAccountInfoById(user1.AccountId);
      if (null != challenger_acc) {
        int c_ct = challenger.FightPartners.Count;
        int c_partner_frt = 0 < c_ct ? challenger.FightPartners[0].Id : 0;
        int c_partner_scd = 1 < c_ct ? challenger.FightPartners[1].Id : 0;
        int c_partner_thd = 2 < c_ct ? challenger.FightPartners[2].Id : 0;
        int t_ct = target.FightPartners.Count;
        int t_partner_frt = 0 < t_ct ? target.FightPartners[0].Id : 0;
        int t_partner_scd = 1 < t_ct ? target.FightPartners[1].Id : 0;
        int t_partner_thd = 2 < t_ct ? target.FightPartners[2].Id : 0;
        /// norm log
        LogSys.NormLog("arena", LobbyConfig.AppKeyStr, challenger_acc.ClientGameVersion, Module.arena, 
          LobbyConfig.LogNormVersionStr, "C0500", challenger_acc.LogicServerId, 
          user1.AccountId, user1.Guid, user1.Level, user1.HeroId, user1.FightingScore, c_partner_frt, c_partner_scd, c_partner_thd,
          user2.AccountId, user2.Guid, user2.Level, user2.HeroId, user2.FightingScore, t_partner_frt, t_partner_scd, t_partner_thd,
          challenger.GetRank(), IsSuccess ? 1 : 0);
      }
    }

    private void SendResultMsg(ChallengeInfo info, UserInfo user)
    {
      if (user == null) {
        return;
      }
      JsonMessageWithGuid retMsg = new JsonMessageWithGuid(ArkCrossEngine.JsonMessageID.ArenaChallengeResult);
      retMsg.m_Guid = user.Guid;
      ArkCrossEngineMessage.Msg_LC_ArenaChallengeResult protoData = new ArkCrossEngineMessage.Msg_LC_ArenaChallengeResult();
      protoData.m_ChallengeInfo = ArenaUtil.CreateChallengeInfoData(info);
      retMsg.m_ProtoData = protoData;
      JsonMessageDispatcher.SendDcoreMessage(user.NodeName, retMsg);
    }

    internal List<ChallengeInfo> GetChallengeHistory(ulong guid)
    {
      List<ChallengeInfo> result = null;
      m_ChallengeHistory.TryGetValue(guid, out result);
      return result;
    }

    internal void RemoveChallengeHistory(ulong guid)
    {
      m_ChallengeHistory.Remove(guid);
    }

    internal void AddChallengeHistory(ulong guid, ChallengeInfo info)
    {
      List<ChallengeInfo> history = null;
      if (!m_ChallengeHistory.TryGetValue(guid, out history)) {
        history = new List<ChallengeInfo>();
        m_ChallengeHistory[guid] = history;
      }
      if (history.Count >= m_MaxHistoryCount) {
        history.RemoveAt(history.Count - 1);
      }
      history.Insert(0, info);
    }

    private bool IsRankShouldChange(int challenger_rank, int target_rank)
    {
      if (m_Rank.IsRankLegal(target_rank)) {
        if (m_Rank.IsRankLegal(challenger_rank)) {
          if (challenger_rank > target_rank) {
            return true;
          } else {
            return false;
          }
        } else {
          return true;
        }
      } else {
        return false;
      }
    }

    private ChallengeEntityInfo CreateChallengeEntityInfo(ArenaInfo arena)
    {
      ChallengeEntityInfo info = new ChallengeEntityInfo();
      info.Guid = arena.GetId();
      info.HeroId = arena.HeroId;
      info.Level = arena.Level;
      info.FightScore = arena.FightScore;
      info.NickName = arena.NickName;
      info.Rank = arena.GetRank();
      info.UserDamage = 0;
      info.PartnerDamage.Clear();
      for (int i = 0; i < arena.FightPartners.Count; i++) {
        PartnerInfo partner = arena.FightPartners[i];
        DamageInfo damange_info = new DamageInfo();
        damange_info.OwnerId = partner.Id;
        damange_info.Damage = 0;
        info.PartnerDamage.Add(damange_info);
      }
      return info;
    }

    private long m_MaxFightTime = 20000;
    private int m_MaxHistoryCount = 10;
    private Rank<ArenaInfo> m_Rank;
    private Dictionary<ulong, ChallengeInfo> m_DoingChallenges = new Dictionary<ulong, ChallengeInfo>();
    private Dictionary<ulong, List<ChallengeInfo>> m_ChallengeHistory = new Dictionary<ulong, List<ChallengeInfo>>();
  }
}
