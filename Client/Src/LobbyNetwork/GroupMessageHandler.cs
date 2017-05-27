using System;
using LitJson;

namespace ArkCrossEngine.Network
{
    public enum TeamOperateResult : int
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
    internal sealed partial class LobbyNetworkSystem
    {
        private void GroupMessageInit()
        {
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("ge_pinvite_team_by_guid", "lobby", PinviteTeamByGuid);
            GfxSystem.EventChannelForLogic.Subscribe<string, string>("ge_pinvite_team", "lobby", PinviteTeam);
            GfxSystem.EventChannelForLogic.Subscribe<string, ulong>("ge_request_join_group", "lobby", RequestJoinGroup);
            GfxSystem.EventChannelForLogic.Subscribe<string, ulong>("ge_confirm_join_group", "lobby", ConfirmJoinGroup);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_quit_group", "lobby", QuitGroup);
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("ge_refuse_request", "lobby", RefuseGroupRequest);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_group_info", "lobby", RequestGroupInfo);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_request_match_mpve", "lobby", RequestMatchMpve);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_start_mpve", "lobby", StartMpve);
            GroupMessageHandler();
        }
        private void GroupMessageHandler()
        {
            RegisterMsgHandler(JsonMessageID.SyncPinviteTeam, typeof(ArkCrossEngineMessage.Msg_LC_SyncPinviteTeam), HandleSyncPinviteTeam);
            RegisterMsgHandler(JsonMessageID.RequestJoinGroupResult, typeof(ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult), HandleRequestJoinGroupResult);
            RegisterMsgHandler(JsonMessageID.ConfirmJoinGroupResult, typeof(ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult), HandleConfirmJoinGroupResult);
            RegisterMsgHandler(JsonMessageID.SyncLeaveGroup, typeof(ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup), HandleSyncLeaveGroup);
            RegisterMsgHandler(JsonMessageID.SyncGroupUsers, typeof(ArkCrossEngineMessage.Msg_LC_SyncGroupUsers), HandleSyncGroupUsers);
            RegisterMsgHandler(JsonMessageID.ChangeCaptain, typeof(ArkCrossEngineMessage.Msg_LC_ChangeCaptain), HandleChangeCaptain);
            RegisterMsgHandler(JsonMessageID.MpveResult, typeof(ArkCrossEngineMessage.Msg_LC_MpveGeneralResult), HandleMpveResult);
        }
        private void PinviteTeamByGuid(ulong guid2)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.PinviteTeam);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_PinviteTeam protoData = new ArkCrossEngineMessage.Msg_CL_PinviteTeam();
                protoData.m_FirstGuid = m_Guid;
                protoData.m_SecondGuid = guid2;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void PinviteTeam(string nick1, string nick2)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.PinviteTeam);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_PinviteTeam protoData = new ArkCrossEngineMessage.Msg_CL_PinviteTeam();
                protoData.m_FirstNick = nick1;
                protoData.m_SecondNick = nick2;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RequestJoinGroup(string group_nick, ulong invitee_guid)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestJoinGroup);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_RequestJoinGroup protoData = new ArkCrossEngineMessage.Msg_CL_RequestJoinGroup();
                protoData.m_InviteeGuid = invitee_guid;
                protoData.m_GroupNick = group_nick;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void ConfirmJoinGroup(string group_name, ulong invitee_guid)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.ConfirmJoinGroup);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_ConfirmJoinGroup protoData = new ArkCrossEngineMessage.Msg_CL_ConfirmJoinGroup();
                protoData.m_InviteeGuid = invitee_guid;
                protoData.m_GroupNick = group_name;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void QuitGroup(string nick)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.QuitGroup);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_QuitGroup protoData = new ArkCrossEngineMessage.Msg_CL_QuitGroup();
                protoData.m_DropoutNick = nick;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RefuseGroupRequest(ulong requester_guid)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.RefuseGroupRequest);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_RefuseGroupRequest protoData = new ArkCrossEngineMessage.Msg_CL_RefuseGroupRequest();
                protoData.m_RequesterGuid = requester_guid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void RequestMatchMpve(int scene_id)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestMatch);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_RequestMatch protoData = new ArkCrossEngineMessage.Msg_CL_RequestMatch();
                protoData.m_SceneType = scene_id;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
                WorldSystem.Instance.WaitMatchSceneId = scene_id;
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void StartMpve(int scene_id)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.StartMpve);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_StartMpve protoData = new ArkCrossEngineMessage.Msg_CL_StartMpve();
                protoData.m_SceneType = scene_id;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void RequestGroupInfo()
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestGroupInfo);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleRequestJoinGroupResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RequestJoinGroupResult;
            if (null != protoData)
            {
                string nick = protoData.m_Nick;
                TeamOperateResult result = (TeamOperateResult)protoData.m_Result;
                GfxSystem.PublishGfxEvent("ge_request_join_group_result", "group", nick, result);
            }
        }
        private void HandleConfirmJoinGroupResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ConfirmJoinGroupResult;
            if (null != protoData)
            {
                string nick = protoData.m_Nick;
                TeamOperateResult result = (TeamOperateResult)protoData.m_Result;
                GfxSystem.PublishGfxEvent("ge_confirm_join_group_result", "group", nick, result);

                //Note:加入队伍，通知RC
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (result == TeamOperateResult.OR_Succeed && role_info != null)
                {
                    if (role_info.Guid == role_info.Group.CreatorGuid)
                    {
                        GfxSystem.PublishGfxEvent("ge_joinABandWithBandId", "gt", role_info.Group.CreatorGuid.ToString());
                    }
                    else
                    {
                        GfxSystem.PublishGfxEvent("ge_joinABandWithBandId", "gt", role_info.Group.CreatorGuid.ToString());
                    }
                }
            }
        }
        private void HandleSyncLeaveGroup(JsonMessage lobbyMsg)
        {
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (null == role_info)
                return;
            GroupInfo group = role_info.Group;
            if (null == group)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncLeaveGroup;
            if (null != protoData)
            {
                string group_nick = protoData.m_GroupNick;
                TeamOperateResult result = (TeamOperateResult)protoData.m_Result;
                if (TeamOperateResult.OR_Succeed == result)
                {
                    group.Reset();
                }
                GfxSystem.PublishGfxEvent("ge_leave_group_result", "group", group_nick, result);
                switch (result)
                {
                    case TeamOperateResult.OR_Succeed:
                        GfxSystem.PublishGfxEvent("ge_quitFromABandUserJoined", "gt");
                        break;
                    case TeamOperateResult.OR_Kickout:
                        GfxSystem.PublishGfxEvent("ge_quitFromABandUserJoined", "gt");
                        break;
                    case TeamOperateResult.OR_Dismiss:
                        GfxSystem.PublishGfxEvent("ge_breakBandUserJoined", "gt");
                        break;
                }
            }
        }
        private void HandleChangeCaptain(JsonMessage lobbyMsg)
        {
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (null == role_info)
                return;
            GroupInfo group = role_info.Group;
            if (null == group)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ChangeCaptain protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ChangeCaptain;
            if (null != protoData)
            {
                ulong group_id = protoData.m_CreatorGuid;
                if (role_info.Guid == group_id)
                {
                    GfxSystem.PublishGfxEvent("ge_change_captain", "group");
                }
            }
        }
        private void HandleSyncGroupUsers(JsonMessage lobbyMsg)
        {
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (null == role_info)
                return;
            GroupInfo group = role_info.Group;
            if (null == group)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncGroupUsers protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncGroupUsers;
            if (null != protoData)
            {
                group.Reset();
                group.CreatorGuid = protoData.m_Creator;
                group.Count = protoData.m_Count;
                if (null != protoData.m_Members && protoData.m_Members.Count > 0)
                {
                    int ct = protoData.m_Members.Count;
                    for (int i = 0; i < ct; i++)
                    {
                        GroupMemberInfo member_info = new GroupMemberInfo();
                        member_info.Guid = protoData.m_Members[i].m_Guid;
                        member_info.HeroId = protoData.m_Members[i].m_HeroId;
                        member_info.Nick = protoData.m_Members[i].m_Nick;
                        member_info.Level = protoData.m_Members[i].m_Level;
                        member_info.FightingScore = protoData.m_Members[i].m_FightingScore;
                        member_info.Status = (UserState)protoData.m_Members[i].m_Status;
                        group.Members.Add(member_info);
                        if (role_info.Guid == member_info.Guid)
                        {
                            //Note:更新RC队伍信息，只需更新玩家自己
                            GfxSystem.PublishGfxEvent("ge_joinABandWithBandId", "gt", group.CreatorGuid.ToString());
                        }
                    }
                }
                if (null != protoData.m_Confirms && protoData.m_Confirms.Count > 0)
                {
                    int ct = protoData.m_Confirms.Count;
                    for (int i = 0; i < ct; i++)
                    {
                        GroupMemberInfo member_info = new GroupMemberInfo();
                        member_info.Guid = protoData.m_Confirms[i].m_Guid;
                        member_info.HeroId = protoData.m_Confirms[i].m_HeroId;
                        member_info.Nick = protoData.m_Confirms[i].m_Nick;
                        member_info.Level = protoData.m_Confirms[i].m_Level;
                        member_info.FightingScore = protoData.m_Confirms[i].m_FightingScore;
                        member_info.Status = (UserState)protoData.m_Confirms[i].m_Status;
                        group.Confirms.Add(member_info);
                    }
                }
                GfxSystem.PublishGfxEvent("ge_sync_group_info", "group", group);
            }
        }
        private void HandleSyncPinviteTeam(JsonMessage lobbyMsg)
        {
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (null == role_info)
                return;
            GroupInfo group = role_info.Group;
            if (null == group)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncPinviteTeam protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncPinviteTeam;
            if (null != protoData)
            {
                GfxSystem.PublishGfxEvent("ge_pinvite_team_message", "group", protoData.m_Sponsor, protoData.m_LeaderNick);
            }
        }
        private void HandleMpveResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_MpveGeneralResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MpveGeneralResult;
            if (null != protoData)
            {
                if ((int)TeamOperateResult.OR_Succeed == protoData.m_Result && protoData.m_Type > 0)
                {
                    WorldSystem.Instance.WaitMatchSceneId = protoData.m_Type;
                }
                else if ((int)TeamOperateResult.OR_CancelMatch == protoData.m_Result
                || (int)TeamOperateResult.OR_Overflow == protoData.m_Result
                || (int)TeamOperateResult.OR_Busyness == protoData.m_Result
                || (int)TeamOperateResult.OR_LevelError == protoData.m_Result
                || (int)TeamOperateResult.OR_NotCaptain == protoData.m_Result
                || (int)TeamOperateResult.OR_TimeError == protoData.m_Result)
                {
                    WorldSystem.Instance.WaitMatchSceneId = -1;
                }
                GfxSystem.PublishGfxEvent("ge_mpve_match_result", "group", protoData.m_Nick, protoData.m_Result, protoData.m_Type);
            }
        }
    }
}