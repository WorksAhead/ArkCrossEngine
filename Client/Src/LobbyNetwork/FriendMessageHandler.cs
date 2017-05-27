using System;
using System.Collections.Generic;
using LitJson;

namespace ArkCrossEngine.Network
{
    public enum AddFriendResult : int
    {
        ADD_SUCCESS = 0,
        ADD_NONENTITY_ERROR,
        ADD_OWN_ERROR,
        ADD_PLAYERSELF_ERROR,
        ADD_NOTICE,
        ADD_OVERFLOW,
        ERROR
    }
    public enum DelFriendResult : int
    {
        DEL_SUCCESS = 0,
        DEL_NONENTITY_ERROR,
        ERROR
    }
    public enum GenderType : int
    {
        Other = 0,
        Mr = 1,
        Ms = 2,
    }
    public enum QueryType : int
    {
        Guid = 0,
        Name = 1,
        Level = 2,
        Score = 3,
        Fortune = 4,
        Gender = 5,
        Unknown = 6,
    }
    internal sealed partial class LobbyNetworkSystem
    {
        private void FriendMessageInit()
        {
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("ge_add_friend_by_guid", "lobby", AddFriendByGuid);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_add_friend", "lobby", AddFriend);
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("ge_confirm_friend", "lobby", ConfirmFriend);
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("ge_delete_friend", "lobby", DeleteFriend);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_friends", "lobby", RequestFriends);
            GfxSystem.EventChannelForLogic.Subscribe<QueryType, string, int, int, int, int>("ge_query_friend_info", "lobby", QueryFriendInfo);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_client_friends", "ui", RequestClientFriends);
            FriendMessageHandler();
        }
        private void FriendMessageHandler()
        {
            RegisterMsgHandler(JsonMessageID.AddFriendResult, typeof(ArkCrossEngineMessage.Msg_LC_AddFriendResult), HandleAddFriendResult);
            RegisterMsgHandler(JsonMessageID.DelFriendResult, typeof(ArkCrossEngineMessage.Msg_LC_DelFriendResult), HandleDelFriendResult);
            RegisterMsgHandler(JsonMessageID.SyncFriendList, typeof(ArkCrossEngineMessage.Msg_LC_SyncFriendList), HandleSyncFriendList);
            RegisterMsgHandler(JsonMessageID.QueryFriendInfoResult, typeof(ArkCrossEngineMessage.Msg_LC_QueryFriendInfoResult), HandleQueryFriendInfoResult);
            RegisterMsgHandler(JsonMessageID.FriendOnline, typeof(ArkCrossEngineMessage.Msg_LC_FriendOnline), HandleOnFriendOnline);
            RegisterMsgHandler(JsonMessageID.FriendOffline, typeof(ArkCrossEngineMessage.Msg_LC_FriendOffline), HandleOnFriendOffline);
        }
        private void AddFriendByGuid(ulong target_guid)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.AddFriend);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_AddFriend protoData = new ArkCrossEngineMessage.Msg_CL_AddFriend();
                protoData.m_TargetGuid = target_guid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void AddFriend(string target_name)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.AddFriend);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_AddFriend protoData = new ArkCrossEngineMessage.Msg_CL_AddFriend();
                protoData.m_TargetNick = target_name;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleAddFriendResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_AddFriendResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_AddFriendResult;
            if (null != protoData)
            {
                string target_nick = protoData.m_TargetNick;
                AddFriendResult result = (AddFriendResult)protoData.m_Result;
                if (AddFriendResult.ADD_SUCCESS == result && null != LobbyClient.Instance.CurrentRole)
                {
                    Dictionary<ulong, FriendInfo> friends = LobbyClient.Instance.CurrentRole.Friends;
                    if (null != friends && friends.Count < FriendInfo.c_Friend_Max)
                    {
                        ulong friend_guid = protoData.m_FriendInfo.Guid;
                        string friend_nick = protoData.m_FriendInfo.Nickname;
                        int friend_hero_id = protoData.m_FriendInfo.HeroId;
                        int friend_level = protoData.m_FriendInfo.Level;
                        int friend_score = protoData.m_FriendInfo.FightingScore;
                        bool friend_online = protoData.m_FriendInfo.IsOnline;
                        bool friend_black = protoData.m_FriendInfo.IsBlack;
                        FriendInfo new_friend = new FriendInfo();
                        new_friend.Guid = friend_guid;
                        new_friend.Nickname = friend_nick;
                        new_friend.HeroId = friend_hero_id;
                        new_friend.Level = friend_level;
                        new_friend.FightingScore = friend_score;
                        new_friend.IsOnline = friend_online;
                        new_friend.IsBlack = friend_black;
                        if (!friends.ContainsKey(friend_guid))
                        {
                            friends.Add(friend_guid, new_friend);
                        }
                        GfxSystem.PublishGfxEvent("ge_add_friend", "friend", friend_guid, friend_nick, result);
                    }
                }
                else
                {
                    ulong guid = 0;
                    GfxSystem.PublishGfxEvent("ge_add_friend", "friend", guid, target_nick, result);
                }
            }
        }
        private void ConfirmFriend(ulong target_guid)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.ConfirmFriend);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_ConfirmFriend protoData = new ArkCrossEngineMessage.Msg_CL_ConfirmFriend();
                protoData.m_TargetGuid = target_guid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void DeleteFriend(ulong target_guid)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.DelFriend);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_DeleteFriend protoData = new ArkCrossEngineMessage.Msg_CL_DeleteFriend();
                protoData.m_TargetGuid = target_guid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleDelFriendResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_DelFriendResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_DelFriendResult;
            if (null != protoData)
            {
                ulong target_guid = protoData.m_TargetGuid;
                DelFriendResult result = (DelFriendResult)protoData.m_Result;
                if (DelFriendResult.DEL_SUCCESS == result && null != LobbyClient.Instance.CurrentRole)
                {
                    Dictionary<ulong, FriendInfo> friends = LobbyClient.Instance.CurrentRole.Friends;
                    if (null != friends && friends.Count > 0)
                    {
                        if (friends.ContainsKey(target_guid))
                        {
                            friends.Remove(target_guid);
                        }
                    }
                }
                GfxSystem.PublishGfxEvent("ge_del_friend", "friend", target_guid, result);
            }
        }
        internal void RequestFriends()
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.FriendList);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleSyncFriendList(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncFriendList protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncFriendList;
            if (null != protoData)
            {
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    Dictionary<ulong, FriendInfo> friends = LobbyClient.Instance.CurrentRole.Friends;
                    if (null != friends)
                    {
                        if (null != protoData.m_FriendInfo && protoData.m_FriendInfo.Count > 0)
                        {
                            friends.Clear();
                            int ct = protoData.m_FriendInfo.Count;
                            for (int i = 0; i < ct; i++)
                            {
                                ArkCrossEngineMessage.FriendInfoForMsg assit_fi = protoData.m_FriendInfo[i];
                                FriendInfo fi = new FriendInfo();
                                ulong target_guid = assit_fi.Guid;
                                fi.Guid = target_guid;
                                fi.Nickname = assit_fi.Nickname;
                                fi.HeroId = assit_fi.HeroId;
                                fi.Level = assit_fi.Level;
                                fi.FightingScore = assit_fi.FightingScore;
                                fi.IsOnline = assit_fi.IsOnline;
                                fi.IsBlack = assit_fi.IsBlack;
                                if (!friends.ContainsKey(target_guid))
                                {
                                    friends.Add(target_guid, fi);
                                }

                                //Note:通知rc，同步好友列表
                                GfxSystem.PublishGfxEvent("ge_addFriendSuccessWithFriendUserId", "gt", target_guid.ToString());
                            }
                        }
                        GfxSystem.PublishGfxEvent("ge_sync_friend_list", "friend");
                    }
                }
            }
        }
        private void QueryFriendInfo(QueryType type, string t_name, int t_level, int t_score, int t_fortune, int t_gender)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.QueryFriendInfo);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_QueryFriendInfo protoData = new ArkCrossEngineMessage.Msg_CL_QueryFriendInfo();
                protoData.m_QueryType = (int)type;
                protoData.m_TargetName = t_name;
                protoData.m_TargetLevel = t_level;
                protoData.m_TargetScore = t_score;
                protoData.m_TargetFortune = t_fortune;
                protoData.m_TargetGender = t_gender;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleQueryFriendInfoResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_QueryFriendInfoResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_QueryFriendInfoResult;
            if (null != protoData)
            {
                List<FriendInfo> friend_list = new List<FriendInfo>();
                int ct = protoData.m_Friends.Count;
                for (int i = 0; i < ct; i++)
                {
                    FriendInfo assit_info = new FriendInfo();
                    assit_info.Guid = protoData.m_Friends[i].Guid;
                    assit_info.Nickname = protoData.m_Friends[i].Nickname;
                    assit_info.HeroId = protoData.m_Friends[i].HeroId;
                    assit_info.Level = protoData.m_Friends[i].Level;
                    assit_info.FightingScore = protoData.m_Friends[i].FightingScore;
                    assit_info.IsOnline = protoData.m_Friends[i].IsOnline;
                    assit_info.IsBlack = protoData.m_Friends[i].IsBlack;
                    friend_list.Add(assit_info);
                }
                GfxSystem.PublishGfxEvent("ge_query_friend_result", "friend", friend_list);
            }
        }
        private void RequestClientFriends()
        {
            try
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info && null != role_info.Friends)
                {
                    GfxSystem.PublishGfxEvent("ge_client_friends", "friend", role_info.Friends);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleOnFriendOnline(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_FriendOnline protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_FriendOnline;
            if (null != protoData)
            {
                ulong friend_guid = protoData.m_Guid;
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    Dictionary<ulong, FriendInfo> friends = LobbyClient.Instance.CurrentRole.Friends;
                    if (null != friends && friends.Count > 0)
                    {
                        FriendInfo out_info = null;
                        if (friends.TryGetValue(friend_guid, out out_info))
                        {
                            out_info.IsOnline = true;
                            GfxSystem.PublishGfxEvent("ge_friend_online", "friend", friend_guid);
                        }
                    }
                }
            }
        }
        private void HandleOnFriendOffline(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_FriendOffline protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_FriendOffline;
            if (null != protoData)
            {
                ulong friend_guid = protoData.m_Guid;
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    Dictionary<ulong, FriendInfo> friends = LobbyClient.Instance.CurrentRole.Friends;
                    if (null != friends && friends.Count > 0)
                    {
                        FriendInfo out_info = null;
                        if (friends.TryGetValue(friend_guid, out out_info))
                        {
                            out_info.IsOnline = false;
                            GfxSystem.PublishGfxEvent("ge_friend_offline", "friend", friend_guid);
                        }
                    }
                }
            }
        }
    }
}