using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal enum MpveAwardResult
    {
        Succeed = 0,
        Gained = 1,
        Nothing = 2,
        Failure = 3,
    }
    internal class MpveAwardItem
    {
        internal MpveAwardItem()
        {
            this.m_ItemId = 0;
            this.m_ItemNum = 0;
        }
        internal int ItemId
        {
            get { return m_ItemId; }
            set { m_ItemId = value; }
        }
        internal int ItemNum
        {
            get { return m_ItemNum; }
            set { m_ItemNum = value; }
        }
        private int m_ItemId;
        private int m_ItemNum;
    }
    internal sealed class MpveMatchInfo
    {
        internal ulong m_Guid;
        internal int m_Score;
        internal long m_StartTime;
    }
    internal class MpveMatchHelper
    {
        internal virtual bool CanMatchMpve(ulong guid, out List<string> nick, out List<TeamOperateResult> ret)
        {
            nick = new List<string>();
            ret = new List<TeamOperateResult>();
            return true;
        }
        internal LinkedListDictionary<ulong, MpveMatchInfo> MpveMatchUsers
        {
            get { return m_MpveMatchUsers; }
        }
        internal SortedList<int, SortedSet<ulong>> MpveMatchUserScore
        {
            get { return m_MpveMatchUserScore; }
        }
        internal bool IsMeetTime(int type)
        {
            bool ret = false;
            DateTime time = DateTime.Now;
            int seconds = Time.CalcSeconds(time.Hour, time.Minute, time.Second);
            MpveTimeConfig time_data = MpveTimeConfigProvider.Instance.GetDataById(type);
            if (null != time_data)
            {
                Time start_time = new Time(time_data.m_StartHour, time_data.m_StartMinute, time_data.m_StartSecond);
                Time end_time = new Time(time_data.m_EndHour, time_data.m_EndMinute, time_data.m_EndSecond);
                int start = start_time.CalcSeconds();
                int end = end_time.CalcSeconds();
                if (seconds >= start && seconds <= end)
                {
                    ret = true;
                }
            }
            return ret;
        }
        private struct Time
        {
            internal int m_Hour;
            internal int m_Minute;
            internal int m_Second;

            internal int CalcSeconds()
            {
                return CalcSeconds(m_Hour, m_Minute, m_Second);
            }
            internal static int CalcSeconds(int hour, int minute, int second)
            {
                return hour * 3600 + minute * 60 + second;
            }
            internal Time(int hour, int minute, int second)
            {
                m_Hour = hour;
                m_Minute = minute;
                m_Second = second;
            }
        }
        private struct MpveTime
        {
            internal Time m_StartTime;
            internal Time m_EndTime;
        }
        internal const int c_MpveCount = 2;
        internal const float c_BaseRateX = 0.3f;
        internal const float c_MaxRateX = 0.9f;
        internal const int c_TC = 10000;
        internal static int AttemptUnlockLevel = 30;
        internal static int GoldUnlockLevel = 15;
        internal static int PlatformDefenseUnlockLevel = 1;
        private LinkedListDictionary<ulong, MpveMatchInfo> m_MpveMatchUsers = new LinkedListDictionary<ulong, MpveMatchInfo>();
        private SortedList<int, SortedSet<ulong>> m_MpveMatchUserScore = new SortedList<int, SortedSet<ulong>>();
    }
    /// attempt
    internal class MpveAttempt : MpveMatchHelper
    {
        internal override bool CanMatchMpve(ulong guid, out List<string> nick, out List<TeamOperateResult> ret)
        {
            nick = new List<string>();
            ret = new List<TeamOperateResult>();
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            UserInfo player = scheduler.GetUserInfo(guid);
            if (null != player)
            {
                bool isTeam = null == player.Group ? false : true;
                if (isTeam)
                {
                    if (player.Guid == player.Group.CreatorGuid)
                    {
                        foreach (GroupMemberInfo m in player.Group.Members)
                        {
                            UserInfo member = scheduler.GetUserInfo(m.Guid);
                            if (null != member)
                            {
                                if (member.Level < MpveMatchHelper.AttemptUnlockLevel)
                                {
                                    nick.Add(m.Nick);
                                    ret.Add(TeamOperateResult.OR_LevelError);
                                }
                                if (member.CurrentState != UserState.Online)
                                {
                                    nick.Add(m.Nick);
                                    ret.Add(TeamOperateResult.OR_Busyness);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        ret.Add(TeamOperateResult.OR_NotCaptain);
                    }
                }
                else
                {
                    if (player.Level < MpveMatchHelper.AttemptUnlockLevel)
                    {
                        ret.Add(TeamOperateResult.OR_LevelError);
                    }
                    if (player.CurrentState != UserState.Online)
                    {
                        ret.Add(TeamOperateResult.OR_Busyness);
                    }
                }
            }
            else
            {
                ret.Add(TeamOperateResult.OR_Unknown);
            }
            return ret.Count > 0 ? false : true;
        }
    }
    /// attempt
    internal class MpveGold : MpveMatchHelper
    {
        internal override bool CanMatchMpve(ulong guid, out List<string> nick, out List<TeamOperateResult> ret)
        {
            nick = new List<string>();
            ret = new List<TeamOperateResult>();
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            UserInfo player = scheduler.GetUserInfo(guid);
            if (null != player)
            {
                bool isTeam = null == player.Group ? false : true;
                if (isTeam)
                {
                    if (player.Guid == player.Group.CreatorGuid)
                    {
                        foreach (GroupMemberInfo m in player.Group.Members)
                        {
                            UserInfo member = scheduler.GetUserInfo(m.Guid);
                            if (null != member)
                            {
                                if (member.GoldCurAcceptedCount >= UserInfo.c_GoldAcceptedHzMax)
                                {
                                    nick.Add(m.Nick);
                                    ret.Add(TeamOperateResult.OR_Overflow);
                                }
                                if (member.Level < MpveMatchHelper.GoldUnlockLevel)
                                {
                                    nick.Add(m.Nick);
                                    ret.Add(TeamOperateResult.OR_LevelError);
                                }
                                if (member.CurrentState != UserState.Online)
                                {
                                    nick.Add(m.Nick);
                                    ret.Add(TeamOperateResult.OR_Busyness);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        ret.Add(TeamOperateResult.OR_NotCaptain);
                    }
                }
                else
                {
                    if (player.GoldCurAcceptedCount >= UserInfo.c_GoldAcceptedHzMax)
                    {
                        ret.Add(TeamOperateResult.OR_Overflow);
                    }
                    if (player.Level < MpveMatchHelper.GoldUnlockLevel)
                    {
                        ret.Add(TeamOperateResult.OR_LevelError);
                    }
                    if (player.CurrentState != UserState.Online)
                    {
                        ret.Add(TeamOperateResult.OR_Busyness);
                    }
                }
            }
            else
            {
                ret.Add(TeamOperateResult.OR_Unknown);
            }
            return ret.Count > 0 ? false : true;
        }
    }
    internal class MpvePlatformDefense : MpveMatchHelper
    {
        internal override bool CanMatchMpve(ulong guid, out List<string> nick, out List<TeamOperateResult> ret)
        {
            nick = new List<string>();
            ret = new List<TeamOperateResult>();
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            UserInfo player = scheduler.GetUserInfo(guid);
            if (null != player)
            {
                bool isTeam = null == player.Group ? false : true;
                if (isTeam)
                {
                    if (player.Guid == player.Group.CreatorGuid)
                    {
                        foreach (GroupMemberInfo m in player.Group.Members)
                        {
                            UserInfo member = scheduler.GetUserInfo(m.Guid);
                            if (null != member)
                            {
                                if (member.Level < MpveMatchHelper.PlatformDefenseUnlockLevel)
                                {
                                    nick.Add(m.Nick);
                                    ret.Add(TeamOperateResult.OR_LevelError);
                                }
                                if (member.CurrentState != UserState.Online)
                                {
                                    nick.Add(m.Nick);
                                    ret.Add(TeamOperateResult.OR_Busyness);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        ret.Add(TeamOperateResult.OR_NotCaptain);
                    }
                }
                else
                {
                    if (player.Level < MpveMatchHelper.PlatformDefenseUnlockLevel)
                    {
                        ret.Add(TeamOperateResult.OR_LevelError);
                    }
                    if (player.CurrentState != UserState.Online)
                    {
                        ret.Add(TeamOperateResult.OR_Busyness);
                    }
                }
            }
            else
            {
                ret.Add(TeamOperateResult.OR_Unknown);
            }
            return ret.Count > 0 ? false : true;
        }
    }
}
