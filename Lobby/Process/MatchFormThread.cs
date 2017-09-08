using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal enum MatchSceneEnum : int
    {
        Gow = 3001,
        Arena = 3002,
        Dare = 3003,
        Expedition = 4001,
        Attempt = 4011,
        Gold = 4021,
        PlatformDefense = 4031,
    }
    /// <summary>
    /// 匹配逻辑线程。处理玩家匹配对战的逻辑。
    /// </summary>
    /// <remarks>
    /// 其它线程不应直接调用此类方法，应通过QueueAction发起调用。
    /// </remarks>
    internal sealed class MatchFormThread : MyServerThread
    {
        internal MatchFormThread()
        {
        }

        internal int MatchUserCount
        {
            get { return m_MatchUserCount; }
        }

        private void PublishMpveMsg2Client(ulong guid, TeamOperateResult result, string nick, int type)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user == null) return;
            JsonMessageWithGuid mrMsg = new JsonMessageWithGuid(JsonMessageID.MpveResult);
            mrMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_MpveGeneralResult protoData = new ArkCrossEngineMessage.Msg_LC_MpveGeneralResult();
            protoData.m_Result = (int)result;
            if (nick.Length > 0)
                protoData.m_Nick = nick;
            else
                protoData.m_Nick = user.Nickname;
            if (type > 0)
                protoData.m_Type = type;
            mrMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, mrMsg);
        }

        internal void StartMpve(ulong guid, int type)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            UserInfo user = scheduler.GetUserInfo(guid);
            if (user == null) return;
            MpveMatchHelper helper = null;
            if (m_MpveMatch.TryGetValue(type, out helper))
            {
                List<string> nick = null;
                List<TeamOperateResult> ret = null;
                bool isMeetTime = helper.IsMeetTime(type);
                if (isMeetTime)
                {
                    bool iscross = helper.CanMatchMpve(guid, out nick, out ret);
                    if (iscross)
                    {
                        List<ulong> guidList = new List<ulong>();
                        if (null != user.Group)
                        {
                            foreach (GroupMemberInfo m in user.Group.Members)
                            {
                                UserInfo mbr = scheduler.GetUserInfo(m.Guid);
                                if (null != mbr)
                                {
                                    mbr.CurrentBattleInfo.init(type, user.HeroId);
                                    guidList.Add(mbr.Guid);
                                }
                            }
                        }
                        else
                        {
                            user.CurrentBattleInfo.init(type, user.HeroId);
                            guidList.Add(guid);
                        }
                        ///
                        RoomProcessThread roomProcess = LobbyServer.Instance.RoomProcessThread;
                        roomProcess.QueueAction(roomProcess.AllocLobbyRoom, guidList.ToArray(), type);
                    }
                    else
                    {
                        if (null != nick && null != ret)
                        {
                            for (int i = 0; i < ret.Count; i++)
                            {
                                string t_nick = nick.Count > i ? nick[i] : "";
                                if (TeamOperateResult.OR_NotCaptain == ret[i])
                                {
                                    PublishMpveMsg2Client(guid, ret[i], t_nick, type);
                                }
                                else
                                {
                                    if (null != user.Group)
                                    {
                                        foreach (GroupMemberInfo m in user.Group.Members)
                                        {
                                            PublishMpveMsg2Client(m.Guid, ret[i], t_nick, type);
                                        }
                                    }
                                    else
                                    {
                                        PublishMpveMsg2Client(guid, ret[i], t_nick, type);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    PublishMpveMsg2Client(guid, TeamOperateResult.OR_TimeError, "", type);
                }
            }
        }

        internal void RequestMatch(ulong guid, int type)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                if (user.CurrentState == UserState.Online)
                {
                    if (type == (int)MatchSceneEnum.Gow)
                    {
                        TeamOperateResult ret = TeamOperateResult.OR_Succeed;
                        if (user.Level >= GowSystem.m_UnlockLevel)
                        {
                            if (GowSystem.CanMatch() || GlobalVariables.Instance.IsDebug)
                            {
                                AddGowUser(guid, user);
                                user.CurrentState = UserState.Teaming;

                                LogSys.Log(LOG_TYPE.DEBUG, "RequestMatch gow, guid:{0},type:{1}", guid, type);
                            }
                            else
                            {
                                ret = TeamOperateResult.OR_TimeError;
                            }
                        }
                        else
                        {
                            ret = TeamOperateResult.OR_LevelError;
                        }
                        if (TeamOperateResult.OR_Succeed != ret)
                        {
                            JsonMessageWithGuid mrMsg = new JsonMessageWithGuid(JsonMessageID.MatchResult);
                            mrMsg.m_Guid = guid;
                            ArkCrossEngineMessage.Msg_LC_MatchResult protoData = new ArkCrossEngineMessage.Msg_LC_MatchResult();
                            protoData.m_Result = (int)ret;
                            mrMsg.m_ProtoData = protoData;
                            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, mrMsg);
                        }
                    }
                    else
                    {
                        MpveMatchHelper helper = null;
                        if (m_MpveMatch.TryGetValue(type, out helper))
                        {
                            List<string> nick = null;
                            List<TeamOperateResult> ret = null;
                            bool isMeetTime = helper.IsMeetTime(type);
                            if (isMeetTime)
                            {
                                bool iscross = helper.CanMatchMpve(guid, out nick, out ret);
                                if (iscross)
                                {
                                    AddMpveUser(type, guid, user);
                                    user.CurrentState = UserState.Teaming;

                                    ret.Add(TeamOperateResult.OR_Succeed);
                                    LogSys.Log(LOG_TYPE.DEBUG, "RequestMatch Mpve, guid:{0},type:{1}", guid, type);
                                }
                            }
                            else
                            {
                                if (null == ret || null == nick)
                                {
                                    ret = new List<TeamOperateResult>();
                                    nick = new List<string>();
                                    ret.Add(TeamOperateResult.OR_TimeError);
                                }
                            }
                            if (null != nick && null != ret)
                            {
                                for (int i = 0; i < ret.Count; i++)
                                {
                                    string t_nick = nick.Count > i ? nick[i] : "";
                                    if (TeamOperateResult.OR_NotCaptain == ret[i])
                                    {
                                        PublishMpveMsg2Client(guid, ret[i], t_nick, type);
                                    }
                                    else
                                    {
                                        if (null != user.Group)
                                        {
                                            foreach (GroupMemberInfo m in user.Group.Members)
                                            {
                                                PublishMpveMsg2Client(m.Guid, ret[i], t_nick, type);
                                            }
                                        }
                                        else
                                        {
                                            PublishMpveMsg2Client(guid, ret[i], t_nick, type);
                                        }
                                    }
                                }
                            }
                        }
                        LogSys.Log(LOG_TYPE.DEBUG, "RequestMatch mpve, guid:{0},type:{1}", guid, type);
                    }
                }
                else
                {
                    LogSys.Log(LOG_TYPE.DEBUG, "RequestMatch failed, guid:{0},type:{1},UserState:{1}", guid, type, user.CurrentState);
                }
            }
        }


        internal void CancelRequestMatch(ulong guid, int scene_type)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            if ((int)MatchSceneEnum.Attempt == scene_type || (int)MatchSceneEnum.Gold == scene_type)
            {
                UserInfo user = scheduler.GetUserInfo(guid);
                if (null != user)
                {
                    if (null != user.Group)
                    {
                        foreach (GroupMemberInfo m in user.Group.Members)
                        {
                            RemoveMpveUser(scene_type, m.Guid, true, user.Nickname);
                            UserInfo m_info = scheduler.GetUserInfo(m.Guid);
                            if (m_info != null && m_info.CurrentState == UserState.Teaming)
                            {
                                m_info.CurrentState = UserState.Online;
                            }
                        }
                    }
                    else
                    {
                        RemoveMpveUser(scene_type, guid, true);
                    }
                }
            }
            else
            {
                RemoveGowUser(guid);
            }

            UserInfo info = scheduler.GetUserInfo(guid);
            if (info != null && info.CurrentState == UserState.Teaming)
            {
                info.CurrentState = UserState.Online;
            }
            LogSys.Log(LOG_TYPE.DEBUG, "CancelRequestTeam,guid:{0}", guid);
        }

        internal void OnUserQuit(ulong guid)
        {
            RemoveGowUser(guid);
            for (int ix = 1; ix < LobbyServer.Instance.SceneList.Count; ++ix)
            {
                SceneInfo sceneInfo = LobbyServer.Instance.SceneList[ix];
                if (null != sceneInfo && (sceneInfo.Type == SceneTypeEnum.TYPE_MULTI_PVE))
                {
                    RemoveMpveUser(sceneInfo.SceneID, guid, true);
                }
            }
            UserInfo info = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != info)
            {
                QuitGroup(guid, info.Nickname);
            }
            if (info != null && info.CurrentState == UserState.Teaming)
            {
                info.CurrentState = UserState.Online;
            }
        }

        protected override void OnStart()
        {
            ActionNumPerTick = 1024;
            TickSleepTime = 10;

            for (int ix = 1; ix < LobbyServer.Instance.SceneList.Count; ++ix)
            {
                SceneInfo sceneInfo = LobbyServer.Instance.SceneList[ix];
                if (null != sceneInfo && (sceneInfo.Type == SceneTypeEnum.TYPE_MULTI_PVE))
                {
                    if (SceneSubTypeEnum.TYPE_ATTEMPT == sceneInfo.SubType)
                    {
                        MpveAttempt assit_ = new MpveAttempt();
                        m_MpveMatch.Add(sceneInfo.SceneID, assit_);
                    }
                    else if (SceneSubTypeEnum.TYPE_GOLD == sceneInfo.SubType)
                    {
                        MpveGold assit_ = new MpveGold();
                        m_MpveMatch.Add(sceneInfo.SceneID, assit_);
                    }
                    else if (SceneSubTypeEnum.TYPE_PLATFORM_DEFENSE == sceneInfo.SubType)
                    {
                        MpvePlatformDefense assit_ = new MpvePlatformDefense();
                        m_MpveMatch.Add(sceneInfo.SceneID, assit_);
                    }
                }
            }
        }
        protected override void OnTick()
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (m_LastLogTime + 60000 < curTime)
            {
                m_LastLogTime = curTime;

                DebugPoolCount((string msg) =>
                {
                    LogSys.Log(LOG_TYPE.INFO, "MatchFormThread.ActionQueue {0}", msg);
                });
            }
            if (m_LastCountTime + c_CountInterval < curTime)
            {
                m_LastCountTime = curTime;

                m_MatchUserCount = m_GowMatchUsers.Count;
                foreach (MpveMatchHelper helper in m_MpveMatch.Values)
                {
                    m_MatchUserCount += helper.MpveMatchUsers.Count;
                }
            }

            const int c_GowCount = 2;
            if (m_GowMatchUsers.Count >= c_GowCount)
            {
                int ct = 0;
                for (LinkedListNode<GowMatchInfo> node = m_GowMatchUsers.FirstValue; null != node && ct < 256;)
                {
                    GowMatchInfo matchInfo = node.Value;
                    float x = c_BaseRateX;
                    if (curTime - matchInfo.m_StartTime > GowSystem.m_TC)
                    {
                        x *= (curTime - matchInfo.m_StartTime) / (float)GowSystem.m_TC;
                    }

                    //LogSys.Log(LOG_TYPE.DEBUG, "try find match for gow user {0}(elo:{1} time:{2} x:{3})", matchInfo.m_Guid, matchInfo.m_Elo, curTime - matchInfo.m_StartTime, x);

                    List<ulong> guidList = FindMatchedGowUsers(matchInfo.m_Guid, matchInfo.m_Elo, x, c_GowCount - 1);
                    guidList.Add(matchInfo.m_Guid);
                    if (guidList.Count == c_GowCount)
                    {
                        node = node.Next;
                        for (int ix = 0; ix < c_GowCount; ++ix)
                        {
                            ulong guid = guidList[ix];
                            if (m_GowMatchUsers.Contains(guid))
                            {
                                if (null != node && node.Value.m_Guid == guid)
                                    node = node.Next;
                                GowMatchInfo findMatchInfo = m_GowMatchUsers[guid];
                                DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
                                UserInfo mbr = scheduler.GetUserInfo(findMatchInfo.m_Guid);
                                if (null != mbr)
                                {
                                    mbr.CurrentBattleInfo.init((int)MatchSceneEnum.Gow, mbr.HeroId);
                                }
                                LogSys.Log(LOG_TYPE.DEBUG, "found matched user {0}(elo:{1}) for gow user {2}(elo:{3})", findMatchInfo.m_Guid, findMatchInfo.m_Elo, matchInfo.m_Guid, matchInfo.m_Elo);
                                ///
                                RemoveGowUser(guid);
                            }
                        }
                        ///
                        RoomProcessThread roomProcess = LobbyServer.Instance.RoomProcessThread;
                        roomProcess.QueueAction(roomProcess.AllocLobbyRoom, guidList.ToArray(), (int)MatchSceneEnum.Gow);
                        ++ct;
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }
            ///
            TickMpve(curTime);
            ///
            m_GroupsInfo.Tick();
        }

        private List<ulong> FindMatchedGowUsers(ulong guid, int elo, float x, int count)
        {
            List<ulong> guids = new List<ulong>();
            IList<int> keys = m_GowMatchUserElos.Keys;
            int ct = keys.Count;
            int ix = keys.IndexOf(elo);
            if (ix >= 0)
            {
                SortedSet<ulong> users = m_GowMatchUserElos[elo];
                if (users.Count > 1)
                {
                    foreach (ulong user in users)
                    {
                        if (user != guid)
                        {
                            guids.Add(user);
                            if (guids.Count >= count)
                                break;
                        }
                    }
                }
                else
                {
                    for (int i = 1; ; ++i)
                    {
                        bool valid = false;
                        int left = ix - i;
                        if (left >= 0)
                        {
                            valid = true;
                            int leftElo = keys[left];
                            if (leftElo >= elo * (1 - x))
                            {
                                SortedSet<ulong> leftUsers = m_GowMatchUserElos[leftElo];
                                if (leftUsers.Count > 0)
                                {
                                    foreach (ulong user in leftUsers)
                                    {
                                        if (user != guid)
                                        {
                                            guids.Add(user);
                                            if (guids.Count >= count)
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        int right = ix + i;
                        if (right < ct)
                        {
                            valid = true;
                            int rightElo = keys[right];
                            if (rightElo <= elo * (1 + x))
                            {
                                SortedSet<ulong> rightUsers = m_GowMatchUserElos[rightElo];
                                if (rightUsers.Count > 0)
                                {
                                    foreach (ulong user in rightUsers)
                                    {
                                        if (user != guid)
                                        {
                                            guids.Add(user);
                                            if (guids.Count >= count)
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        if (!valid || guids.Count >= count)
                            break;
                    }
                }
            }
            return guids;
        }

        private void AddGowUser(ulong guid, UserInfo user)
        {
            if (!m_GowMatchUsers.Contains(guid))
            {
                int elo = user.GowInfo.GowElo;
                GowMatchInfo matchInfo = new GowMatchInfo();
                matchInfo.m_Guid = guid;
                matchInfo.m_Elo = elo;
                matchInfo.m_StartTime = TimeUtility.GetServerMilliseconds();
                m_GowMatchUsers.AddFirst(guid, matchInfo);
                SortedSet<ulong> users;
                if (m_GowMatchUserElos.TryGetValue(elo, out users))
                {
                    if (!users.Contains(guid))
                    {
                        users.Add(guid);
                    }
                }
                else
                {
                    users = new SortedSet<ulong>();
                    m_GowMatchUserElos.Add(user.GowInfo.GowElo, users);
                    users.Add(guid);
                }
            }
        }

        private void RemoveGowUser(ulong guid)
        {
            if (m_GowMatchUsers.Contains(guid))
            {
                GowMatchInfo matchInfo = m_GowMatchUsers[guid];
                if (null != matchInfo)
                {
                    int elo = matchInfo.m_Elo;
                    SortedSet<ulong> users;
                    if (m_GowMatchUserElos.TryGetValue(elo, out users))
                    {
                        users.Remove(guid);
                        if (users.Count == 0)
                        {
                            m_GowMatchUserElos.Remove(elo);
                        }
                    }
                }
                m_GowMatchUsers.Remove(guid);
            }
        }

        private List<ulong> FindMatchedMpveUsers(int type, ulong guid, int score, float x, int count)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            List<ulong> guids = new List<ulong>();
            MpveMatchHelper helper = null;
            if (m_MpveMatch.TryGetValue(type, out helper))
            {
                SortedList<int, SortedSet<ulong>> score_dic = helper.MpveMatchUserScore;
                IList<int> keys = score_dic.Keys;
                int ct = keys.Count;
                int ix = keys.IndexOf(score);
                if (ix >= 0)
                {
                    SortedSet<ulong> users = score_dic[score];
                    if (users.Count > 1)
                    {
                        foreach (ulong user in users)
                        {
                            if (user != guid)
                            {
                                UserInfo matched_user = scheduler.GetUserInfo(user);
                                if (null != matched_user)
                                {
                                    if (null == matched_user.Group)
                                    {
                                        guids.Add(user);
                                    }
                                    else
                                    {
                                        int g_ct_ = matched_user.Group.Members.Count;
                                        if (guids.Count + g_ct_ <= count)
                                        {
                                            foreach (GroupMemberInfo m in matched_user.Group.Members)
                                            {
                                                ulong m_guid_ = m.Guid;
                                                UserInfo member = scheduler.GetUserInfo(m_guid_);
                                                if (null != member)
                                                    guids.Add(m_guid_);
                                            }
                                        }
                                    }
                                }
                                if (guids.Count >= count)
                                    break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 1; ; ++i)
                        {
                            bool valid = false;
                            int left = ix - i;
                            if (left >= 0)
                            {
                                valid = true;
                                int leftScore = keys[left];
                                if (leftScore >= score * (1 - x))
                                {
                                    SortedSet<ulong> leftUsers = score_dic[leftScore];
                                    if (leftUsers.Count > 0)
                                    {
                                        foreach (ulong user in leftUsers)
                                        {
                                            if (user != guid)
                                            {
                                                UserInfo player = scheduler.GetUserInfo(user);
                                                if (null != player.Group)
                                                {
                                                    int g_ct_ = player.Group.Members.Count;
                                                    if (guids.Count + g_ct_ <= count)
                                                    {
                                                        foreach (GroupMemberInfo m in player.Group.Members)
                                                        {
                                                            ulong m_guid_ = m.Guid;
                                                            UserInfo member = scheduler.GetUserInfo(m_guid_);
                                                            if (null != member)
                                                                guids.Add(m_guid_);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    guids.Add(user);
                                                }
                                                if (guids.Count >= count)
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (guids.Count >= count)
                                break;
                            int right = ix + i;
                            if (right < ct)
                            {
                                valid = true;
                                int rightScore = keys[right];
                                if (rightScore <= score * (1 + x))
                                {
                                    SortedSet<ulong> rightUsers = score_dic[rightScore];
                                    if (rightUsers.Count > 0)
                                    {
                                        foreach (ulong user in rightUsers)
                                        {
                                            if (user != guid)
                                            {
                                                UserInfo player = scheduler.GetUserInfo(user);
                                                if (null != player.Group)
                                                {
                                                    int g_ct_ = player.Group.Members.Count;
                                                    if (guids.Count + g_ct_ <= count)
                                                    {
                                                        foreach (GroupMemberInfo m in player.Group.Members)
                                                        {
                                                            ulong m_guid_ = m.Guid;
                                                            UserInfo member = scheduler.GetUserInfo(m_guid_);
                                                            if (null != member)
                                                                guids.Add(m_guid_);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    guids.Add(user);
                                                }
                                                if (guids.Count >= count)
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (!valid || guids.Count >= count)
                                break;
                        }
                    }
                }
            }
            return guids;
        }

        private void TickMpve(long curTime)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            for (int ix = 1; ix < LobbyServer.Instance.SceneList.Count; ++ix)
            {
                SceneInfo sceneInfo = LobbyServer.Instance.SceneList[ix];
                if (null != sceneInfo && (sceneInfo.Type == SceneTypeEnum.TYPE_MULTI_PVE))
                {
                    int scene_type = sceneInfo.SceneID;
                    MpveMatchHelper helper = null;
                    if (m_MpveMatch.TryGetValue(scene_type, out helper))
                    {
                        LinkedListDictionary<ulong, MpveMatchInfo> users_dic = helper.MpveMatchUsers;
                        if (users_dic.Count >= MpveMatchHelper.c_MpveCount - 1)
                        {
                            int ct = 0;
                            for (LinkedListNode<MpveMatchInfo> node = users_dic.FirstValue; null != node && ct < 256;)
                            {
                                MpveMatchInfo matchInfo = node.Value;
                                float x = MpveMatchHelper.c_BaseRateX;
                                if (curTime - matchInfo.m_StartTime > MpveMatchHelper.c_TC)
                                {
                                    x *= (curTime - matchInfo.m_StartTime) / (float)MpveMatchHelper.c_TC;
                                    x = x > MpveMatchHelper.c_MaxRateX ? MpveMatchHelper.c_MaxRateX : x;
                                }
                                //LogSys.Log(LOG_TYPE.DEBUG, "try find match for Mpve user {0}(score:{1} time:{2} x:{3})", matchInfo.m_Guid, matchInfo.m_Score, curTime - matchInfo.m_StartTime, x);
                                List<ulong> guidList = null;
                                UserInfo cur_player = scheduler.GetUserInfo(matchInfo.m_Guid);
                                if (null != cur_player)
                                {
                                    if (null != cur_player.Group)
                                    {
                                        int match_score_ = matchInfo.m_Score;
                                        ulong cur_player_m_ = 0;
                                        foreach (GroupMemberInfo m in cur_player.Group.Members)
                                        {
                                            if (matchInfo.m_Guid != m.Guid)
                                            {
                                                cur_player_m_ = m.Guid;
                                                break;
                                            }
                                        }
                                        if (cur_player_m_ > 0)
                                        {
                                            UserInfo cur_member_ = scheduler.GetUserInfo(cur_player_m_);
                                            match_score_ = null == cur_member_ ? match_score_ : (int)((match_score_ + cur_member_.FightingScore) / 2.0f);
                                        }
                                        guidList = FindMatchedMpveUsers(scene_type, matchInfo.m_Guid, match_score_, x, MpveMatchHelper.c_MpveCount - 2);
                                        if (cur_player_m_ > 0)
                                            guidList.Add(cur_player_m_);
                                    }
                                    else
                                    {
                                        guidList = FindMatchedMpveUsers(scene_type, matchInfo.m_Guid, matchInfo.m_Score, x, MpveMatchHelper.c_MpveCount - 1);
                                    }
                                }
                                guidList.Add(matchInfo.m_Guid);
                                if (guidList.Count == MpveMatchHelper.c_MpveCount)
                                {
                                    ///
                                    AutoFormTeam(guidList);
                                    ///
                                    node = node.Next;
                                    for (int idx = 0; idx < MpveMatchHelper.c_MpveCount; ++idx)
                                    {
                                        ulong guid = guidList[idx];
                                        if (users_dic.Contains(guid))
                                        {
                                            if (null != node && node.Value.m_Guid == guid)
                                                node = node.Next;
                                            ///
                                            MpveMatchInfo findMatchInfo = users_dic[guid];
                                            LogSys.Log(LOG_TYPE.DEBUG, "found matched user {0}(score:{1}) for mpve user {2}(score:{3})", findMatchInfo.m_Guid, findMatchInfo.m_Score, matchInfo.m_Guid, matchInfo.m_Score);
                                            ///
                                            UserInfo mbr = scheduler.GetUserInfo(findMatchInfo.m_Guid);
                                            if (null != mbr)
                                            {
                                                mbr.CurrentBattleInfo.init(scene_type, mbr.HeroId);
                                            }
                                            ///
                                            RemoveMpveUser(scene_type, guid, false);
                                        }
                                    }
                                    ///
                                    RoomProcessThread roomProcess = LobbyServer.Instance.RoomProcessThread;
                                    roomProcess.QueueAction(roomProcess.AllocLobbyRoom, guidList.ToArray(), scene_type);
                                    ++ct;
                                }
                                else
                                {
                                    node = node.Next;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddMpveUser(int type, ulong guid, UserInfo user)
        {
            MpveMatchHelper helper = null;
            if (m_MpveMatch.TryGetValue(type, out helper))
            {
                LinkedListDictionary<ulong, MpveMatchInfo> users_dic = helper.MpveMatchUsers;
                if (!users_dic.Contains(guid))
                {
                    int score = user.FightingScore - user.Skill.GetSkillAppendScore();
                    MpveMatchInfo matchInfo = new MpveMatchInfo();
                    matchInfo.m_Guid = guid;
                    matchInfo.m_Score = score;
                    matchInfo.m_StartTime = TimeUtility.GetServerMilliseconds();
                    users_dic.AddFirst(guid, matchInfo);
                    SortedList<int, SortedSet<ulong>> score_dic = helper.MpveMatchUserScore;
                    SortedSet<ulong> users;
                    if (score_dic.TryGetValue(score, out users))
                    {
                        if (!users.Contains(guid))
                        {
                            users.Add(guid);
                        }
                    }
                    else
                    {
                        users = new SortedSet<ulong>();
                        score_dic.Add(user.FightingScore, users);
                        users.Add(guid);
                    }
                }
            }
        }
        private void RemoveMpveUser(int type, ulong guid, bool is_notice_client, string cancel_match_player = "")
        {
            MpveMatchHelper helper = null;
            if (m_MpveMatch.TryGetValue(type, out helper))
            {
                LinkedListDictionary<ulong, MpveMatchInfo> users_dic = helper.MpveMatchUsers;
                if (users_dic.Contains(guid))
                {
                    MpveMatchInfo matchInfo = users_dic[guid];
                    if (null != matchInfo)
                    {
                        int score = matchInfo.m_Score;
                        SortedList<int, SortedSet<ulong>> score_dic = helper.MpveMatchUserScore;
                        SortedSet<ulong> users;
                        if (score_dic.TryGetValue(score, out users))
                        {
                            users.Remove(guid);
                            if (0 == users.Count)
                            {
                                score_dic.Remove(score);
                            }
                        }
                    }
                    users_dic.Remove(guid);
                    ///
                    if (is_notice_client)
                        PublishMpveMsg2Client(guid, TeamOperateResult.OR_CancelMatch, "", type);
                }
                else
                {
                    if (is_notice_client && cancel_match_player.Length > 0)
                        PublishMpveMsg2Client(guid, TeamOperateResult.OR_CancelMatch, cancel_match_player, type);
                }
            }
        }

        /// the following are group interface
        internal void CheckGroupForReLogin(UserInfo user)
        {
            if (null == user)
                return;
            m_GroupsInfo.ReLoginHandle(user);
            if (null != user.Group)
                SyncGroupUsers(user.Group);
        }
        internal void RequestJoinGroup(ulong guid, string player_nick)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            UserInfo player1 = dataProcess.GetUserInfo(guid);
            ulong player2_id = dataProcess.GetGuidByNickname(player_nick);
            UserInfo player2 = dataProcess.GetUserInfo(player2_id);
            if (null != player1 && null != player2 && null == player1.Group
              && UserState.Online == player1.CurrentState && UserState.Online == player2.CurrentState)
            {
                ulong group_id = player2_id;
                if (null != player2.Group)
                {
                    UserInfo leader = dataProcess.GetUserInfo(player2.Group.CreatorGuid);
                    if (null != leader)
                    {
                        group_id = leader.Guid;
                        TeamOperateResult join_result = m_GroupsInfo.RequestJoinGroup(guid, group_id);
                        if (join_result == TeamOperateResult.OR_Succeed)
                        {
                            join_result = group_id == player2_id ? TeamOperateResult.OR_Succeed : TeamOperateResult.OR_IndirectSucceed;
                            SyncGroupUsers(player2.Group);
                        }
                        ///
                        JsonMessageWithGuid rjgMsg = new JsonMessageWithGuid(JsonMessageID.RequestJoinGroupResult);
                        rjgMsg.m_Guid = guid;
                        ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult protoData = new ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult();
                        protoData.m_Result = (int)join_result;
                        protoData.m_Nick = leader.Nickname;
                        rjgMsg.m_ProtoData = protoData;
                        JsonMessageDispatcher.SendDcoreMessage(player1.NodeName, rjgMsg);
                        ///
                        if (join_result != TeamOperateResult.OR_Exists)
                        {
                            JsonMessageWithGuid rjgMsg2 = new JsonMessageWithGuid(JsonMessageID.RequestJoinGroupResult);
                            rjgMsg2.m_Guid = leader.Guid;
                            ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult protoData2 = new ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult();
                            protoData2.m_Result = (int)join_result;
                            protoData2.m_Nick = player1.Nickname;
                            rjgMsg2.m_ProtoData = protoData2;
                            JsonMessageDispatcher.SendDcoreMessage(leader.NodeName, rjgMsg2);
                        }
                    }
                }
            }
        }
        internal void ConfirmJoinGroup(ulong guid, string group_nick)
        {
            TeamOperateResult result = TeamOperateResult.OR_OutDate;
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            UserInfo player = dataProcess.GetUserInfo(guid);
            ulong leader_id = dataProcess.GetGuidByNickname(group_nick);
            UserInfo leader = dataProcess.GetUserInfo(leader_id);
            if (null != player && null != leader)
            {
                if (null == player.Group)
                {
                    if (UserState.Online == player.CurrentState && UserState.Online == leader.CurrentState)
                    {
                        if (null != leader.Group && leader.Group.CreatorGuid != leader_id)
                        {
                            result = TeamOperateResult.OR_OutDate;
                        }
                        else
                        {
                            result = m_GroupsInfo.JoinGroup(guid, leader_id, GroupInfo.c_MemberNumMax);
                        }
                    }
                    else
                    {
                        result = TeamOperateResult.OR_Busyness;
                    }
                }
                else
                {
                    result = TeamOperateResult.OR_HasTeam;
                }
            }
            ///
            if (result != TeamOperateResult.OR_HasTeam)
            {
                JsonMessageWithGuid rjgMsg = new JsonMessageWithGuid(JsonMessageID.ConfirmJoinGroupResult);
                rjgMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult protoData = new ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult();
                protoData.m_Result = (int)result;
                protoData.m_Nick = group_nick;
                rjgMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(player.NodeName, rjgMsg);
            }
            ///
            if (result != TeamOperateResult.OR_OutDate)
            {
                JsonMessageWithGuid rjgMsg2 = new JsonMessageWithGuid(JsonMessageID.ConfirmJoinGroupResult);
                rjgMsg2.m_Guid = leader_id;
                ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult protoData2 = new ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult();
                protoData2.m_Result = (int)result;
                protoData2.m_Nick = player.Nickname;
                rjgMsg2.m_ProtoData = protoData2;
                JsonMessageDispatcher.SendDcoreMessage(leader.NodeName, rjgMsg2);
            }
            ///
            GroupInfo group = m_GroupsInfo.GetGroupById(leader_id);
            if (null != group && TeamOperateResult.OR_Succeed == result)
            {
                ///
                if (null != group && null != group.Confirms)
                {
                    GroupMemberInfo rm_record = null;
                    foreach (GroupMemberInfo value in group.Confirms)
                    {
                        if (value.Guid == player.Guid)
                        {
                            rm_record = value;
                            break;
                        }
                    }
                    if (null != rm_record)
                    {
                        group.Confirms.Remove(rm_record);
                    }
                }
                ///
                foreach (GroupMemberInfo info in group.Members)
                {
                    if (info.Guid != guid && info.Guid != leader_id)
                    {
                        UserInfo m = dataProcess.GetUserInfo(info.Guid);
                        if (null != m)
                        {
                            JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.ConfirmJoinGroupResult);
                            msg.m_Guid = m.Guid;
                            ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult protoData3 = new ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult();
                            protoData3.m_Result = (int)TeamOperateResult.OR_Notice;
                            protoData3.m_Nick = player.Nickname;
                            msg.m_ProtoData = protoData3;
                            JsonMessageDispatcher.SendDcoreMessage(m.NodeName, msg);
                        }
                    }
                }
            }
            if (null != group)
            {
                SyncGroupUsers(group);
            }
        }
        internal void PinviteTeamByGuid(ulong first_guid, ulong second_guid)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            UserInfo player1 = dataProcess.GetUserInfo(first_guid);
            UserInfo player2 = dataProcess.GetUserInfo(second_guid);
            if (null != player1 && null != player2)
            {
                PinviteTeam(player1.Nickname, player2.Nickname);
            }
        }
        internal void PinviteTeam(string first_nick, string second_nick)
        {
            DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
            ulong player1_guid = dataProcess.GetGuidByNickname(first_nick);
            ulong player2_guid = dataProcess.GetGuidByNickname(second_nick);
            UserInfo player1 = dataProcess.GetUserInfo(player1_guid);
            UserInfo player2 = dataProcess.GetUserInfo(player2_guid);
            if (null != player1 && null != player2)
            {
                if (null != player1.Group && null != player2.Group)
                {
                    JsonMessageWithGuid retMsg = new JsonMessageWithGuid(JsonMessageID.RequestJoinGroupResult);
                    retMsg.m_Guid = player1_guid;
                    ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult retProtoData = new ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult();
                    retProtoData.m_Result = (int)TeamOperateResult.OR_HasTeam;
                    retProtoData.m_Nick = player2.Nickname;
                    retMsg.m_ProtoData = retProtoData;
                    JsonMessageDispatcher.SendDcoreMessage(player1.NodeName, retMsg);
                    return;
                }
                if (player2.CurrentState == UserState.Online)
                {
                    if (null != player1.Group && null == player2.Group
                    || null == player1.Group && null == player2.Group)
                    {
                        string group_nick = player1.Nickname;
                        if (null != player1.Group)
                        {
                            UserInfo leader = dataProcess.GetUserInfo(player1.Group.CreatorGuid);
                            if (null != leader)
                                group_nick = leader.Nickname;
                        }
                        JsonMessageWithGuid sptMsg = new JsonMessageWithGuid(JsonMessageID.SyncPinviteTeam);
                        sptMsg.m_Guid = player2_guid;
                        ArkCrossEngineMessage.Msg_LC_SyncPinviteTeam protoData = new ArkCrossEngineMessage.Msg_LC_SyncPinviteTeam();
                        protoData.m_LeaderNick = group_nick;
                        protoData.m_Sponsor = player1.Nickname;
                        sptMsg.m_ProtoData = protoData;
                        JsonMessageDispatcher.SendDcoreMessage(player2.NodeName, sptMsg);
                    }
                    else
                    {
                        RequestJoinGroup(player1_guid, player2.Nickname);
                    }
                }
                else
                {
                    JsonMessageWithGuid rjgMsg = new JsonMessageWithGuid(JsonMessageID.RequestJoinGroupResult);
                    rjgMsg.m_Guid = player1_guid;
                    ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult protoData = new ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult();
                    protoData.m_Result = (int)TeamOperateResult.OR_Busyness;
                    protoData.m_Nick = player2.Nickname;
                    rjgMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(player1.NodeName, rjgMsg);
                }
            }
        }
        internal void SyncGroupUsers(GroupInfo group)
        {
            if (null != group)
            {
                DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
                ArkCrossEngineMessage.Msg_LC_SyncGroupUsers protoData = new ArkCrossEngineMessage.Msg_LC_SyncGroupUsers();
                protoData.m_Creator = group.CreatorGuid;
                protoData.m_Count = group.Members.Count;
                foreach (GroupMemberInfo member in group.Members)
                {
                    UserInfo umember = dataProcess.GetUserInfo(member.Guid);
                    if (null != umember/* && UserState.Online == umember.CurrentState*/)
                    {
                        ArkCrossEngineMessage.Msg_LC_SyncGroupUsers.UserInfoForGroup assit_data = new ArkCrossEngineMessage.Msg_LC_SyncGroupUsers.UserInfoForGroup();
                        assit_data.m_Guid = umember.Guid;
                        assit_data.m_HeroId = umember.HeroId;
                        assit_data.m_Nick = umember.Nickname;
                        assit_data.m_Level = umember.Level;
                        assit_data.m_FightingScore = umember.FightingScore;
                        assit_data.m_Status = (int)umember.CurrentState;
                        protoData.m_Members.Add(assit_data);
                    }
                }
                JsonMessageWithGuid sguMsg = new JsonMessageWithGuid(JsonMessageID.SyncGroupUsers);
                foreach (GroupMemberInfo member in group.Members)
                {
                    sguMsg.m_Guid = member.Guid;
                    if (member.Guid == group.CreatorGuid)
                    {
                        foreach (GroupMemberInfo c in group.Confirms)
                        {
                            UserInfo cmember = dataProcess.GetUserInfo(c.Guid);
                            if (null != cmember)
                            {
                                ArkCrossEngineMessage.Msg_LC_SyncGroupUsers.UserInfoForGroup assit_data = new ArkCrossEngineMessage.Msg_LC_SyncGroupUsers.UserInfoForGroup();
                                assit_data.m_Guid = cmember.Guid;
                                assit_data.m_HeroId = cmember.HeroId;
                                assit_data.m_Nick = cmember.Nickname;
                                assit_data.m_Level = cmember.Level;
                                assit_data.m_FightingScore = cmember.FightingScore;
                                assit_data.m_Status = (int)cmember.CurrentState;
                                protoData.m_Confirms.Add(assit_data);
                            }
                        }
                    }
                    sguMsg.m_ProtoData = protoData;
                    UserInfo userInfo = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(member.Guid);
                    if (null != userInfo)
                        JsonMessageDispatcher.SendDcoreMessage(userInfo.NodeName, sguMsg);
                }
            }
        }
        internal void QuitGroup(ulong guid, string dropout_nick)
        {
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            ulong player_guid = scheduler.GetGuidByNickname(dropout_nick);
            UserInfo player = scheduler.GetUserInfo(player_guid);
            if (player != null && player.Group != null)
            {
                GroupInfo group = player.Group;
                if (null == group)
                    return;
                if (guid != group.CreatorGuid && player_guid != guid)
                {
                    return;
                }
                GroupMemberInfo rm_member = new GroupMemberInfo();
                foreach (GroupMemberInfo value in group.Members)
                {
                    if (value.Guid == player.Guid)
                    {
                        rm_member = value;
                        break;
                    }
                }
                ///
                TeamOperateResult result = TeamOperateResult.OR_Unknown;
                if (guid == group.CreatorGuid && player_guid != guid)
                {
                    result = TeamOperateResult.OR_Kickout;
                }
                else
                {
                    result = TeamOperateResult.OR_Succeed;
                }
                if (group.Members.Count > 1)
                {
                    if (m_GroupsInfo.LeaveGroup(group, rm_member))
                    {
                        player.Group = null;
                        UserInfo leader = scheduler.GetUserInfo(group.CreatorGuid);
                        JsonMessageWithGuid slgMsg = new JsonMessageWithGuid(JsonMessageID.SyncLeaveGroup);
                        slgMsg.m_Guid = player_guid;
                        ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup protoData = new ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup();
                        protoData.m_Result = (int)result;
                        protoData.m_GroupNick = leader.Nickname;
                        slgMsg.m_ProtoData = protoData;
                        JsonMessageDispatcher.SendDcoreMessage(player.NodeName, slgMsg);
                        ///
                        foreach (GroupMemberInfo info in group.Members)
                        {
                            UserInfo m = scheduler.GetUserInfo(info.Guid);
                            if (null != m)
                            {
                                JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.SyncLeaveGroup);
                                msg.m_Guid = m.Guid;
                                ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup protoData2 = new ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup();
                                protoData2.m_Result = (int)TeamOperateResult.OR_Notice;
                                protoData2.m_GroupNick = player.Nickname;
                                msg.m_ProtoData = protoData2;
                                JsonMessageDispatcher.SendDcoreMessage(m.NodeName, msg);
                            }
                        }
                    }
                }
                else
                {
                    m_GroupsInfo.DismissGroup(group);
                }
                if (null != group && group.Members.Count > 0)
                {
                    SyncGroupUsers(group);
                }
            }
        }
        internal void HandleRequestGroupInfo(ulong guid)
        {
            UserInfo player = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (player != null && player.Group != null)
            {
                GroupInfo group = player.Group;
                if (group.Members.Count > 0)
                {
                    SyncGroupUsers(group);
                }
            }
        }
        internal void HandleRefuseGroupRequest(ulong guid, ulong requester_guid)
        {
            UserInfo player = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (player != null && player.Group != null && player.Group.CreatorGuid == guid)
            {
                if (null != player.Group.Confirms && player.Group.Confirms.Count > 0)
                {
                    GroupMemberInfo out_value = null;
                    foreach (GroupMemberInfo value in player.Group.Confirms)
                    {
                        if (value.Guid == requester_guid)
                        {
                            out_value = value;
                            break;
                        }
                    }
                    if (null != out_value)
                    {
                        player.Group.Confirms.Remove(out_value);
                    }
                }
            }
        }
        internal void AutoFormTeam(List<ulong> guids)
        {
            if (null == guids)
                return;
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            foreach (ulong e in guids)
            {
                UserInfo info = scheduler.GetUserInfo(e);
                if (null != info && null != info.Group)
                    m_GroupsInfo.DestroyGroup(info.Group);
            }
            if (GroupInfo.c_MemberNumMax == guids.Count)
            {
                ulong leader_guid = guids[0];
                m_GroupsInfo.AutoJoinGroup(guids[1], leader_guid);
                m_GroupsInfo.AutoJoinGroup(guids[2], leader_guid);
                GroupInfo new_group = m_GroupsInfo.GetGroupByUserGuid(leader_guid);
                if (null != new_group)
                    SyncGroupUsers(new_group);
            }
        }
        private sealed class GowMatchInfo
        {
            internal ulong m_Guid;
            internal int m_Elo;
            internal long m_StartTime;
        }

        private const float c_BaseRateX = 0.1f;
        private LinkedListDictionary<ulong, GowMatchInfo> m_GowMatchUsers = new LinkedListDictionary<ulong, GowMatchInfo>();
        private SortedList<int, SortedSet<ulong>> m_GowMatchUserElos = new SortedList<int, SortedSet<ulong>>();

        private Dictionary<int, MpveMatchHelper> m_MpveMatch = new Dictionary<int, MpveMatchHelper>();

        private GroupsInfo m_GroupsInfo = new GroupsInfo();

        private long m_LastLogTime = 0;
        private long m_LastCountTime = 0;
        private const long c_CountInterval = 10000;
        private int m_MatchUserCount = 0;
    }
}

