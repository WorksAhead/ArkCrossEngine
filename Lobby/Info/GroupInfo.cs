using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  internal enum TeamOperateResult : int
  {
    OR_Succeed = 0,
    OR_IndirectSucceed = 1,
    OR_Overflow = 2,
    OR_Exists = 3,
    OR_Busyness = 4,
    OR_Kickout = 5,
    OR_Dismiss = 6,
    OR_Notice = 7,
    OR_HasTeam = 8,
    OR_OutDate = 9,
    OR_LevelError = 10,
    OR_NotCaptain = 11,
    OR_CancelMatch = 12,
    OR_TimeError = 13,
    OR_Unknown = 14,
  }
  internal class GroupMemberInfo
  {
    internal ulong Guid
    {
      get { return m_Guid; }
      set { m_Guid = value; }
    }
    internal int HeroId
    {
      get { return m_HeroId; }
      set { m_HeroId = value; }
    }
    internal string Nick
    {
      get { return m_Nick; }
      set { m_Nick = value; }
    }
    internal int Level
    {
      get { return m_Level; }
      set { m_Level = value; }
    }
    internal int FightingScore
    {
      get { return m_FightingScore; }
      set { m_FightingScore = value; }
    }
    internal UserState Status
    {
      get { return m_Status; }
      set { m_Status = value; }
    }
    internal double LeftLife
    {
      get { return m_LeftLife; }
      set { m_LeftLife = value; }
    }
    private ulong m_Guid;
    private int m_HeroId;
    private string m_Nick;
    private int m_Level;
    private int m_FightingScore;
    private UserState m_Status;
    private double m_LeftLife = -1;
  }

  internal class GroupInfo
  {
    internal ulong CreatorGuid
    {
      get { return m_CreatorGuid; }
      set { m_CreatorGuid = value; }
    }
    internal int Count
    {
      get { return m_Count; }
      set { m_Count = value; }
    }
    internal List<GroupMemberInfo> Confirms
    {
      get { return m_ConfirmList; }
      set { m_ConfirmList = value; }
    }
    internal List<GroupMemberInfo> Members
    {
      get { return m_Members; }
      set { m_Members = value; }
    }
    internal void Reset()
    {
      m_Count = 0;
      m_CreatorGuid = 0;
      m_Members.Clear();
      m_ConfirmList.Clear();
    }
    internal const int c_MemberNumMax = 3;
    internal const int c_ConfirmNumMax = 50;
    private int m_Count = 0;
    private ulong m_CreatorGuid = 0;
    private List<GroupMemberInfo> m_Members = new List<GroupMemberInfo>();
    private List<GroupMemberInfo> m_ConfirmList = new List<GroupMemberInfo>();
  }

  internal class GroupsInfo
  {
    internal GroupInfo CreateGroup(ulong guid, int count)
    {
      GroupInfo group;
      if (!m_Groups.TryGetValue(guid, out group)) {
        DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
        UserInfo creator = dataProcess.GetUserInfo(guid);
        if (null != creator && UserState.Online == creator.CurrentState) {
          group = NewGroupInfo();
          group.CreatorGuid = guid;
          group.Count = count;
          GroupMemberInfo member = new GroupMemberInfo();
          member.Guid = guid;
          member.HeroId = creator.HeroId;
          member.Nick = creator.Nickname;
          member.Level = creator.Level;
          member.FightingScore = creator.FightingScore;
          member.Status = creator.CurrentState;
          List<GroupMemberInfo> members = new List<GroupMemberInfo>();
          members.Add(member);
          group.Members = members;
          m_Groups.Add(guid, group);
          UserInfo info = dataProcess.GetUserInfo(guid);
          if (info != null) {
            info.Group = group;
          }
        }
      }
      return group;
    }
    internal bool LeaveGroup(GroupInfo group,GroupMemberInfo member)
    {
      if (null == group || null == member)
        return false;
      if (member.Guid == group.CreatorGuid && group.Members.Count > 1) {
        m_Groups.Remove(group.CreatorGuid);
        group.CreatorGuid = group.Members[1].Guid;
        if (!m_Groups.ContainsKey(group.CreatorGuid))
          m_Groups.Add(group.CreatorGuid, group);
        ///
        UserInfo leader = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(group.CreatorGuid);
        if (null != leader) {
          JsonMessageWithGuid slgMsg = new JsonMessageWithGuid(JsonMessageID.ChangeCaptain);
          slgMsg.m_Guid = group.CreatorGuid;
          ArkCrossEngineMessage.Msg_LC_ChangeCaptain protoData = new ArkCrossEngineMessage.Msg_LC_ChangeCaptain();
          protoData.m_CreatorGuid = group.CreatorGuid;
          slgMsg.m_ProtoData = protoData;
          JsonMessageDispatcher.SendDcoreMessage(leader.NodeName, slgMsg);
        }
      }
      bool result = group.Members.Remove(member);
      if (group.Members.Count == 0) {
        recycles.Enqueue(group);
      }
      return result;
    }
    internal void DismissGroup(GroupInfo group)
    {
      if (null == group)
        return;
      UserInfo leader = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(group.CreatorGuid);
      TeamOperateResult result = TeamOperateResult.OR_Dismiss;
      foreach (GroupMemberInfo m in group.Members) {
        UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(m.Guid);
        if (null != user) {
          JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.SyncLeaveGroup);
          msg.m_Guid = m.Guid;
          ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup protoData = new ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup();
          if (null != leader)
            protoData.m_GroupNick = leader.Nickname;
          protoData.m_Result = (int)result;
          msg.m_ProtoData = protoData;
          JsonMessageDispatcher.SendDcoreMessage(leader.NodeName, msg);
          user.Group = null;
        }
      }
      m_Groups.Remove(group.CreatorGuid);
      if (null != group.Members) {
        group.Members.Clear();
      }
      recycles.Enqueue(group);
    }
    internal void DestroyGroup(GroupInfo group)
    {
      if (null == group)
        return;
      UserInfo leader = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(group.CreatorGuid);
      foreach (GroupMemberInfo m in group.Members) {
        UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(m.Guid);
        if (null != user) {
          user.Group = null;
        }
      }
      m_Groups.Remove(group.CreatorGuid);
      RecycleGroupInfo(group);
    }
    internal TeamOperateResult RequestJoinGroup(ulong guid, ulong groupid)
    {
      TeamOperateResult result = TeamOperateResult.OR_Busyness;
      if (guid == groupid)
        return result;
      DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
      UserInfo intrant = dataProcess.GetUserInfo(guid);
      if (null != intrant && UserState.Online == intrant.CurrentState) {
        GroupInfo ginfo;
        if (m_Groups.TryGetValue(groupid, out ginfo)) {
          if (null != ginfo.Confirms
            && ginfo.Confirms.Count < GroupInfo.c_ConfirmNumMax) {
            bool ishave = false;
            foreach (GroupMemberInfo info in ginfo.Confirms) {
              if (info.Guid == guid) {
                ishave = true;
                result = TeamOperateResult.OR_Exists;
                break;
              }
            }
            if (!ishave) {
              GroupMemberInfo member = new GroupMemberInfo();
              member.Guid = guid;
              member.HeroId = intrant.HeroId;
              member.Nick = intrant.Nickname;
              member.Level = intrant.Level;
              member.FightingScore = intrant.FightingScore;
              member.Status = intrant.CurrentState;
              ginfo.Confirms.Add(member);
              result = TeamOperateResult.OR_Succeed;
            }
          } else {
            result = TeamOperateResult.OR_Overflow;
          }
        }
      }
      return result;
    }
    internal TeamOperateResult JoinGroup(ulong guid, ulong groupid, int count)
    {
      if (guid == groupid)
        return TeamOperateResult.OR_Unknown;
      DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
      TeamOperateResult result = TeamOperateResult.OR_OutDate;
      GroupInfo info;
      if (m_Groups.TryGetValue(groupid, out info)) {
        if (info.Count > info.Members.Count) {
          UserInfo player = dataProcess.GetUserInfo(guid);
          if (null != player && player.CurrentState == UserState.Online) {
            GroupMemberInfo member = new GroupMemberInfo();
            member.Guid = guid;
            member.HeroId = player.HeroId;
            member.Nick = player.Nickname;
            member.Level = player.Level;
            member.FightingScore = player.FightingScore;
            member.Status = player.CurrentState;
            bool isExist = false;
            foreach (GroupMemberInfo atom in info.Members) {
              if (atom.Guid == guid) {
                isExist = true;
                result = TeamOperateResult.OR_Exists;
                break;
              }
            }
            if (!isExist) {
              info.Members.Add(member);
              player.Group = info;
              result = TeamOperateResult.OR_Succeed;
            }
          }
        } else {
          result = TeamOperateResult.OR_Overflow;
        }
      } else {
        UserInfo creator = dataProcess.GetUserInfo(groupid);
        UserInfo intrant = dataProcess.GetUserInfo(guid);
        if (null != creator && creator.CurrentState == UserState.Online
          && null != intrant && intrant.CurrentState == UserState.Online) {
          GroupInfo group = NewGroupInfo();
          group.CreatorGuid = groupid;
          group.Count = count;
          List<GroupMemberInfo> members = new List<GroupMemberInfo>();
          GroupMemberInfo leader = new GroupMemberInfo();
          leader.Guid = groupid;
          leader.HeroId = creator.HeroId;
          leader.Nick = creator.Nickname;
          leader.Level = creator.Level;
          leader.FightingScore = creator.FightingScore;
          leader.Status = creator.CurrentState;
          members.Add(leader);
          GroupMemberInfo member = new GroupMemberInfo();
          member.Guid = guid;
          member.HeroId = intrant.HeroId;
          member.Nick = intrant.Nickname;
          member.Level = intrant.Level;
          member.FightingScore = intrant.FightingScore;
          member.Status = intrant.CurrentState;
          members.Add(member);
          group.Members = members;
          m_Groups.Add(groupid, group);
          ///
          creator.Group = group;
          intrant.Group = group;
          result = TeamOperateResult.OR_Succeed;
        } else {
          result = TeamOperateResult.OR_OutDate;
        }
      }
      return result;
    }
    internal void AutoJoinGroup(ulong guid, ulong groupid)
    {
      if (guid == groupid)
        return;
      DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
      GroupInfo info;
      if (m_Groups.TryGetValue(groupid, out info)) {
        if (info.Count > info.Members.Count) {
          UserInfo player = dataProcess.GetUserInfo(guid);
          if (null != player) {
            GroupMemberInfo member = new GroupMemberInfo();
            member.Guid = guid;
            member.HeroId = player.HeroId;
            member.Nick = player.Nickname;
            member.Level = player.Level;
            member.FightingScore = player.FightingScore;
            member.Status = player.CurrentState;
            bool isExist = false;
            foreach (GroupMemberInfo atom in info.Members) {
              if (atom.Guid == guid) {
                isExist = true;
                break;
              }
            }
            if (!isExist) {
              info.Members.Add(member);
              player.Group = info;
            }
          }
        }
      } else {
        UserInfo creator = dataProcess.GetUserInfo(groupid);
        UserInfo intrant = dataProcess.GetUserInfo(guid);
        if (null != creator && null != intrant) {
          GroupInfo group = NewGroupInfo();
          group.CreatorGuid = groupid;
          group.Count = GroupInfo.c_MemberNumMax;
          List<GroupMemberInfo> members = new List<GroupMemberInfo>();
          GroupMemberInfo leader = new GroupMemberInfo();
          leader.Guid = groupid;
          leader.HeroId = creator.HeroId;
          leader.Nick = creator.Nickname;
          leader.Level = creator.Level;
          leader.FightingScore = creator.FightingScore;
          leader.Status = creator.CurrentState;
          members.Add(leader);
          GroupMemberInfo member = new GroupMemberInfo();
          member.Guid = guid;
          member.HeroId = intrant.HeroId;
          member.Nick = intrant.Nickname;
          member.Level = intrant.Level;
          member.FightingScore = intrant.FightingScore;
          member.Status = intrant.CurrentState;
          members.Add(member);
          group.Members = members;
          m_Groups.Add(groupid, group);
          ///
          creator.Group = group;
          intrant.Group = group;
        }
      }
    }
    internal GroupInfo GetGroupById(ulong groupid)
    {
      GroupInfo info;
      m_Groups.TryGetValue(groupid, out info);
      return info;
    }
    internal GroupInfo GetGroupByUserGuid(ulong guid)
    {
      GroupInfo group = null;
      foreach (GroupInfo info in m_Groups.Values) {
        foreach (GroupMemberInfo m in info.Members) {
          if (m.Guid == guid) {
            group = info;
            break;
          }
        }
      }
      return group;
    }
    private GroupInfo NewGroupInfo()
    {
      GroupInfo group = null;
      if (m_UnusedGroupInfos.Count > 0) {
        group = m_UnusedGroupInfos.Dequeue();
      } else {
        group = new GroupInfo();
      }
      return group;
    }
    internal void ReLoginHandle(UserInfo user)
    {
      if (null == user)
        return;
      foreach (GroupInfo info in m_Groups.Values) {
        bool ishave = false;
        foreach (GroupMemberInfo m in info.Members) {
          if (m.Guid == user.Guid && m.Status == UserState.DropOrOffline) {
            ishave = true;
            m.Status = UserState.Online;
            break;
          }
        }
        if (ishave)
          break;
      }
    }
    internal void Tick()
    {
      while (recycles.Count > 0) {
        GroupInfo group = recycles.Dequeue();
        m_Groups.Remove(group.CreatorGuid);
        RecycleGroupInfo(group);
      }
    }
    private void RecycleGroupInfo(GroupInfo group)
    {
      group.Reset();
      m_UnusedGroupInfos.Enqueue(group);
    }
    private SortedDictionary<ulong, GroupInfo> m_Groups = new SortedDictionary<ulong, GroupInfo>();
    private Queue<GroupInfo> m_UnusedGroupInfos = new Queue<GroupInfo>();
    private Queue<GroupInfo> recycles = new Queue<GroupInfo>();
  }
}
