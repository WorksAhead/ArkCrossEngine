using System;
using System.Collections.Generic;
using System.Threading;
using LitJson;

namespace ArkCrossEngine.Network
{
    public enum GeneralOperationResult : int
    {
        LC_Succeed = 0,
        LC_Failure_CostError = 1,
        LC_Failure_Position = 2,
        LC_Failure_NotUnLock = 3,
        LC_Failure_LevelError = 4,
        LC_Failure_Overflow = 5,
        LC_Failure_Time = 6,
        LC_Failure_VigorError = 7,
        LC_Failure_Unknown = 8,
        LC_Failure_Arena_NotFindTarget = 9,
        LC_Failure_Code_Used = 10,          //激活码/礼品码已经被使用
        LC_Failure_Code_Error = 11,         //激活码/礼品码有误
        LC_Failure_NotFinduser = 12,
        LC_Failure_PartNumError = 13,
        LC_Failure_MaterialNumError = 14,
        LC_Failure_InCd,
        LC_Failure_NoFightCount,
        LC_Failure_InMatching,
        LC_Failure_Full,
        LC_Failuer_NotMatch,
        LC_Failuer_Offline,
        LC_Failuer_Busy,
    }
    public enum MpveAwardResult
    {
        Succeed = 0,
        Gained = 1,
        Nothing = 2,
        Failure = 3,
    }
    public enum LoginMode : int
    {
        AccountLogin = 0,
        DirectLogin = 1,
    }
    ///
    internal sealed partial class LobbyNetworkSystem
    {
        private void LobbyMessageInit()
        {
            GfxSystem.EventChannelForLogic.ProxySubscribe(FilterLogicEventByStory);
            GfxSystem.EventChannelForLogic.Subscribe<string, string>("ge_set_ip_and_channel", "client", SetIpAndChannel);
            GfxSystem.EventChannelForLogic.Subscribe<string, string, string>("ge_device_info", "lobby", InitDeviceInfo);
            GfxSystem.EventChannelForLogic.Subscribe<string, int>("ge_select_server", "lobby", SelectServer);
            GfxSystem.EventChannelForLogic.Subscribe("ge_direct_login", "lobby", DirectLoginLobby);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_account_login", "lobby", AccountLoginLobby);
            GfxSystem.EventChannelForLogic.Subscribe("ge_stop_login", "lobby", StopLoginLobby);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_activate_account", "lobby", ActivateAccount);
            GfxSystem.EventChannelForLogic.Subscribe("ge_create_nickname", "lobby", CreateNickname);
            GfxSystem.EventChannelForLogic.Subscribe<int, string>("ge_create_role", "lobby", CreateRole);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_role_enter", "lobby", RoleEnter);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_select_scene", "lobby", SelectScene);
            GfxSystem.EventChannelForLogic.Subscribe<MatchSceneEnum>("ge_cancel_match", "lobby", CancelMatch);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_stage_clear", "lobby", StageClear);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_sweep_stage", "lobby", SweepStage);
            GfxSystem.EventChannelForLogic.Subscribe<int[], int[]>("ge_discard_item", "lobby", DiscardItem);
            GfxSystem.EventChannelForLogic.Subscribe<int, int, int>("ge_mount_equipment", "lobby", MountEquipment);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_unmount_equipment", "lobby", UnmountEquipment);
            GfxSystem.EventChannelForLogic.Subscribe<int, int, SlotPosition>("ge_mount_skill", "lobby", MountSkill);
            GfxSystem.EventChannelForLogic.Subscribe<int, SlotPosition>("ge_unmount_skill", "lobby", UnmountSkill);
            GfxSystem.EventChannelForLogic.Subscribe<int, int, bool>("ge_upgrade_skill", "lobby", UpgradeSkill);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_unlock_skill", "lobby", UnlockSkill);
            GfxSystem.EventChannelForLogic.Subscribe<int, int, SlotPosition, SlotPosition>("ge_swap_skill", "lobby", SwapSkill);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_items", "ui", RequestItemInfo);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_skills", "ui", RequestSkillInfo);
            GfxSystem.EventChannelForLogic.Subscribe<int, int, bool>("ge_upgrade_item", "lobby", UpgradeItem);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_set_preset", "lobby", SaveSkillPreset);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_equiped_skills", "ui", RequestEquipedSkill);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_lift_skill", "lobby", LiftSkill);
            GfxSystem.EventChannelForLogic.Subscribe("ge_buy_stamina", "lobby", BuyStamina);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_read_finish", "lobby", ReadFinishMission);
            GfxSystem.EventChannelForLogic.Subscribe("ge_reload_missions", "lobby", ReloadMissions);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_role_property", "ui", RequestRoleProperty);
            GfxSystem.EventChannelForLogic.Subscribe<bool>("ge_request_relive", "lobby", RequestRelive);
            GfxSystem.EventChannelForLogic.Subscribe("ge_return_maincity", "lobby", ReturnMainCity);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_unlock_legacy", "lobby", UnlockLegacy);
            GfxSystem.EventChannelForLogic.Subscribe<int, int, bool>("ge_upgrade_legacy", "lobby", UpgradeLegacy);
            GfxSystem.EventChannelForLogic.Subscribe("ge_get_mail_list", "lobby", RequestMailList);
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("ge_read_mail", "lobby", ReadMail);
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("ge_receive_mail", "lobby", ReceiveMail);
            GfxSystem.EventChannelForLogic.Subscribe<int, int, int, int, bool, bool>("ge_expedition_reset", "lobby", ExpeditionReset);
            GfxSystem.EventChannelForLogic.Subscribe("ge_newbie_end", "lobby", OnNewbieEnd);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_request_expedition", "lobby", RequestExpedition);
            GfxSystem.EventChannelForLogic.Subscribe<int, int, int, int, int>("ge_finish_expedition", "lobby", FinishExpedition);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_expedition_award", "lobby", ExpeditionAward);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_get_gowstar_list", "lobby", RequestGowStarList);
            GfxSystem.EventChannelForLogic.Subscribe("ge_query_expedition_info", "lobby", QueryExpeditionInfo);
            GfxSystem.EventChannelForLogic.Subscribe("ge_expedition_failure", "lobby", ExpeditionFailure);
            GfxSystem.EventChannelForLogic.Subscribe("ge_midas_touch", "lobby", MidasTouch);
            GfxSystem.EventChannelForLogic.Subscribe<int, bool>("ge_exchange_goods", "lobby", ExchangeGoods);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_sync_exchanges", "lobby", SyncExchangeGoods);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_refresh_exchanges", "lobby", RefreshExchangeGoods);
            GfxSystem.EventChannelForLogic.Subscribe("ge_get_scenestart_time", "lobby", SceneStartTime);
            GfxSystem.EventChannelForLogic.Subscribe<bool>("ge_quit_battle", "lobby", QuitBattle);
            GfxSystem.EventChannelForLogic.Subscribe<string, int>("ge_publish_notice", "lobby", PublishNotice);
            GfxSystem.EventChannelForLogic.Subscribe<XSoulPart, int, int>("ge_addxsoul_experience", "lobby", AddXSoulExperience);
            GfxSystem.EventChannelForLogic.Subscribe<XSoulPart, int>("ge_xsoul_changemodel", "lobby", XSoulChangeShowModel);
            GfxSystem.EventChannelForLogic.Subscribe("ge_mpve_attempt_award", "lobby", RequestAttemptAward);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_request_player_info", "lobby", RequestPlayerInfo);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_vigor", "lobby", RequestVigor);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_set_newbie_flag", "lobby", SetNewbieFlag);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_set_newbie_action_flag", "lobby", SetNewbieActionFlag);
            GfxSystem.EventChannelForLogic.Subscribe("query_arena_info", "arena", QueryArenaInfo);
            GfxSystem.EventChannelForLogic.Subscribe("query_match_group", "arena", QueryArenaMatchGroup);
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("start_challenge", "arena", OnStartChallenge);
            GfxSystem.EventChannelForLogic.Subscribe<bool>("challenge_over", "arena", OnChallengeOver);
            GfxSystem.EventChannelForLogic.Subscribe("query_rank_list", "arena", OnQueryRankList);
            GfxSystem.EventChannelForLogic.Subscribe<int[]>("change_partners", "arena", OnChangePartners);
            GfxSystem.EventChannelForLogic.Subscribe("query_history", "arena", OnQueryHistory);
            GfxSystem.EventChannelForLogic.Subscribe<int>("compound_equip", "lobby", CompoundEquip);
            GfxSystem.EventChannelForLogic.Subscribe("buy_fight_count", "arena", OnBuyArenaFightCount);
            GfxSystem.EventChannelForLogic.Subscribe("pvp_begin_fight", "pvp", OnPvpBeginFight);
            GfxSystem.EventChannelForLogic.Subscribe("check_pvap_result", "pvap", OnCheckPvapResult);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_record_newbie_flag", "lobby", RecordNewbieFlag);
            GfxSystem.EventChannelForLogic.Subscribe<string, string>("ge_upload_fps", "lobby", UploadFPS);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_request_dare_by_nickname", "lobby", OnRequestDareByNickname);
            GfxSystem.EventChannelForLogic.Subscribe<ulong>("ge_request_dare_by_guid", "lobby", OnRequestDareByGuid);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_accepted_dare", "lobby", OnAcceptedDare);
            GfxSystem.EventChannelForLogic.Subscribe("ge_request_gow_prize", "lobby", OnRequestGowPrize);
            LobbyMessageHandler();
        }
        private void LobbyMessageHandler()
        {
            RegisterMsgHandler(JsonMessageID.QueueingCountResult, typeof(ArkCrossEngineMessage.Msg_LC_QueueingCountResult), HandleQueueingCountResult);
            RegisterMsgHandler(JsonMessageID.UserHeartbeat, HandleUserHeartbeat);
            RegisterMsgHandler(JsonMessageID.TooManyOperations, HandleTooManyOperations);
            RegisterMsgHandler(JsonMessageID.VersionVerifyResult, HandleVersionVerifyResult);
            RegisterMsgHandler(JsonMessageID.AccountLoginResult, HandleAccountLoginResult);
            RegisterMsgHandler(JsonMessageID.ActivateAccountResult, HandleActivateAccountResult);
            RegisterMsgHandler(JsonMessageID.RoleListResult, typeof(ArkCrossEngineMessage.Msg_LC_RoleListResult), HandleRoleListResult);
            RegisterMsgHandler(JsonMessageID.CreateNicknameResult, HandleCreateNicknameResult);
            RegisterMsgHandler(JsonMessageID.CreateRoleResult, HandleCreateRoleResult);
            RegisterMsgHandler(JsonMessageID.RoleEnterResult, typeof(ArkCrossEngineMessage.Msg_LC_RoleEnterResult), HandleRoleEnterResult);
            RegisterMsgHandler(JsonMessageID.MatchResult, typeof(ArkCrossEngineMessage.Msg_LC_MatchResult), HandleMatchResult);
            RegisterMsgHandler(JsonMessageID.StartGameResult, typeof(ArkCrossEngineMessage.Msg_LC_StartGameResult), HandleStartGameResult);
            RegisterMsgHandler(JsonMessageID.DiscardItemResult, typeof(ArkCrossEngineMessage.Msg_LC_DiscardItemResult), HandleDiscardItemResult);
            RegisterMsgHandler(JsonMessageID.MountEquipmentResult, typeof(ArkCrossEngineMessage.Msg_LC_MountEquipmentResult), HandleMountEquipmentResult);
            RegisterMsgHandler(JsonMessageID.UnmountEquipmentResult, typeof(ArkCrossEngineMessage.Msg_LC_UnmountEquipmentResult), HandleUnmountEquipmentResult);
            RegisterMsgHandler(JsonMessageID.MountSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_MountSkillResult), HandleMountSkillResult);
            RegisterMsgHandler(JsonMessageID.UnmountSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_UnmountSkillResult), HandleUnmountSkillResult);
            RegisterMsgHandler(JsonMessageID.UpgradeSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_UpgradeSkillResult), HandleUpgradeSkillResult);
            RegisterMsgHandler(JsonMessageID.UnlockSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_UnlockSkillResult), HandleUnlockSkillResult);
            RegisterMsgHandler(JsonMessageID.SwapSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_SwapSkillResult), HandleSwapSkillResult);
            RegisterMsgHandler(JsonMessageID.UpgradeItemResult, typeof(ArkCrossEngineMessage.Msg_LC_UpgradeItemResult), HandleUpgradeItemResult);
            RegisterMsgHandler(JsonMessageID.UserLevelup, typeof(ArkCrossEngineMessage.Msg_LC_UserLevelup), HandleUserLevelup);
            RegisterMsgHandler(JsonMessageID.SyncStamina, typeof(ArkCrossEngineMessage.Msg_LC_SyncStamina), HandleSyncStamina);
            RegisterMsgHandler(JsonMessageID.StageClearResult, typeof(ArkCrossEngineMessage.Msg_LC_StageClearResult), HandleStageClearResult);
            RegisterMsgHandler(JsonMessageID.SweepStageResult, typeof(ArkCrossEngineMessage.Msg_LC_SweepStageResult), HandleSweepStageResult);
            RegisterMsgHandler(JsonMessageID.AddAssetsResult, typeof(ArkCrossEngineMessage.Msg_LC_AddAssetsResult), HandleAddAssetsResult);
            RegisterMsgHandler(JsonMessageID.AddItemResult, typeof(ArkCrossEngineMessage.Msg_LC_AddItemResult), HandleAddItemResult);
            RegisterMsgHandler(JsonMessageID.LiftSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_LiftSkillResult), HandleLiftSkillResult);
            RegisterMsgHandler(JsonMessageID.BuyStaminaResult, typeof(ArkCrossEngineMessage.Msg_LC_BuyStaminaResult), HandleBuyStaminaResult);
            RegisterMsgHandler(JsonMessageID.FinishMissionResult, typeof(ArkCrossEngineMessage.Msg_LC_FinishMissionResult), HandleFinishMissionResult);
            RegisterMsgHandler(JsonMessageID.BuyLifeResult, typeof(ArkCrossEngineMessage.Msg_LC_BuyLifeResult), HandleBuyLifeResult);
            RegisterMsgHandler(JsonMessageID.UnlockLegacyResult, typeof(ArkCrossEngineMessage.Msg_LC_UnlockLegacyResult), HandleUnlockLegacyResult);
            RegisterMsgHandler(JsonMessageID.UpgradeLegacyResult, typeof(ArkCrossEngineMessage.Msg_LC_UpgradeLegacyResult), HandleUpgradeLegacyResult);
            RegisterMsgHandler(JsonMessageID.AddXSoulExperienceResult, typeof(ArkCrossEngineMessage.Msg_LC_AddXSoulExperienceResult), HandleAddXSoulExperienceResult);
            RegisterMsgHandler(JsonMessageID.XSoulChangeShowModelResult, typeof(ArkCrossEngineMessage.Msg_LC_XSoulChangeShowModelResult), HandleXSoulChangeShowModelResult);
            RegisterMsgHandler(JsonMessageID.NotifyNewMail, HandleNotifyNewMail);
            RegisterMsgHandler(JsonMessageID.SyncMailList, HandleSyncMailList);
            RegisterMsgHandler(JsonMessageID.ExpeditionResetResult, typeof(ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult), HandleExpeditionResetResult);
            RegisterMsgHandler(JsonMessageID.RequestExpeditionResult, typeof(ArkCrossEngineMessage.Msg_LC_RequestExpeditionResult), HandleRequestExpeditionResult);
            RegisterMsgHandler(JsonMessageID.FinishExpeditionResult, typeof(ArkCrossEngineMessage.Msg_LC_FinishExpeditionResult), HandleFinishExpeditionResult);
            RegisterMsgHandler(JsonMessageID.ExpeditionAwardResult, typeof(ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult), HandleExpeditionAwardResult);
            RegisterMsgHandler(JsonMessageID.SyncGowStarList, typeof(ArkCrossEngineMessage.Msg_LC_SyncGowStarList), HandleSyncGowStarList);
            RegisterMsgHandler(JsonMessageID.SyncMpveBattleResult, typeof(ArkCrossEngineMessage.Msg_LC_SyncMpveBattleResult), HandleSyncMpveBattleResult);
            RegisterMsgHandler(JsonMessageID.SyncGowBattleResult, typeof(ArkCrossEngineMessage.Msg_LC_SyncGowBattleResult), HandleSyncGowBattleResult);
            RegisterMsgHandler(JsonMessageID.MidasTouchResult, typeof(ArkCrossEngineMessage.Msg_LC_MidasTouchResult), HandleMidasTouchResult);
            RegisterMsgHandler(JsonMessageID.ExchangeGoodsResult, typeof(ArkCrossEngineMessage.Msg_LC_ExchangeGoodsResult), HandleExchangeGoodsResult);
            RegisterMsgHandler(JsonMessageID.RefreshExchangeResult, typeof(ArkCrossEngineMessage.Msg_LC_RefreshExchangeResult), HandleRefreshExchangeResult);
            RegisterMsgHandler(JsonMessageID.ResetDailyMissions, HandleResetDailyMissions);
            RegisterMsgHandler(JsonMessageID.MissionCompleted, typeof(ArkCrossEngineMessage.Msg_LC_MissionCompleted), HandleMissionCompleted);
            RegisterMsgHandler(JsonMessageID.SyncNoticeContent, typeof(ArkCrossEngineMessage.Msg_LC_SyncNoticeContent), HandleSyncNoticeContent);
            RegisterMsgHandler(JsonMessageID.RequestMpveAwardResult, typeof(ArkCrossEngineMessage.Msg_LC_MpveAwardResult), HandleMpveAwardResult);
            RegisterMsgHandler(JsonMessageID.RequestUsersResult, typeof(ArkCrossEngineMessage.Msg_LC_RequestUsersResult), HandleRequestUsersResult);
            RegisterMsgHandler(JsonMessageID.RequestUserPositionResult, typeof(ArkCrossEngineMessage.Msg_LC_RequestUserPositionResult), HandleRequestUserPositionResult);
            RegisterMsgHandler(JsonMessageID.SyncPlayerInfo, typeof(ArkCrossEngineMessage.Msg_LC_SyncPlayerInfo), HandleSyncPlayerInfo);
            RegisterMsgHandler(JsonMessageID.SyncVigor, typeof(ArkCrossEngineMessage.Msg_LC_SyncVigor), HandleSyncVigor);
            RegisterMsgHandler(JsonMessageID.SyncAttemptInfo, typeof(ArkCrossEngineMessage.Msg_LC_SyncAttemptInfo), HandleSyncAttemptInfo);
            RegisterMsgHandler(JsonMessageID.SyncNewbieFlag, typeof(ArkCrossEngineMessage.Msg_LC_SyncNewbieFlag), HandleSyncNewbieFlag);
            RegisterMsgHandler(JsonMessageID.SyncNewbieActionFlag, typeof(ArkCrossEngineMessage.Msg_LC_SyncNewbieActionFlag), HandleSyncNewbieActionFlag);
            RegisterMsgHandler(JsonMessageID.ArenaInfoResult, typeof(ArkCrossEngineMessage.Msg_LC_ArenaInfoResult), HandleArenaInfoResult);
            RegisterMsgHandler(JsonMessageID.ArenaMatchGroupResult, typeof(ArkCrossEngineMessage.Msg_LC_ArenaMatchGroupResult), HandleArenaMatchGroupResult);
            RegisterMsgHandler(JsonMessageID.SyncGoldTollgateInfo, typeof(ArkCrossEngineMessage.Msg_LC_SyncGoldTollgateInfo), HandleSyncGoldTollgateInfo);
            RegisterMsgHandler(JsonMessageID.ArenaStartChallengeResult, typeof(ArkCrossEngineMessage.Msg_LC_ArenaStartCallengeResult), HandleArenaStartChallengeResult);
            RegisterMsgHandler(JsonMessageID.ArenaChallengeResult, typeof(ArkCrossEngineMessage.Msg_LC_ArenaChallengeResult), HandleArenaChallengeResult);
            RegisterMsgHandler(JsonMessageID.ArenaQueryRankResult, typeof(ArkCrossEngineMessage.Msg_LC_ArenaQueryRankResult), HandleArenaQueryRankResult);
            RegisterMsgHandler(JsonMessageID.ArenaChangePartnersResult, typeof(ArkCrossEngineMessage.Msg_LC_ArenaChangePartnerResult), HandleArenaChangePartnerResult);
            RegisterMsgHandler(JsonMessageID.ArenaQueryHistoryResult, typeof(ArkCrossEngineMessage.Msg_LC_ArenaQueryHistoryResult), HandleArenaQueryHistoryResult);
            RegisterMsgHandler(JsonMessageID.CompoundEquipResult, typeof(ArkCrossEngineMessage.Msg_LC_CompoundEquipResult), HandleCompoundEquipResult);
            RegisterMsgHandler(JsonMessageID.ArenaBuyFightCountResult, typeof(ArkCrossEngineMessage.Msg_LC_ArenaBuyFightCountResult), HandleArenaBuyFightCountResult);
            RegisterMsgHandler(JsonMessageID.SyncCombatData, typeof(ArkCrossEngineMessage.Msg_LC_SyncCombatData), HandleSyncCombatData);
            RegisterMsgHandler(JsonMessageID.ServerShutdown, HandleServerShutdown);
            RegisterMsgHandler(JsonMessageID.SyncGuideFlag, typeof(ArkCrossEngineMessage.Msg_LC_SyncGuideFlag), HandleSyncGuideFlag);
            RegisterMsgHandler(JsonMessageID.RequestDare, typeof(ArkCrossEngineMessage.Msg_LC_RequestDare), HandleRequestDare);
            RegisterMsgHandler(JsonMessageID.RequestDareResult, typeof(ArkCrossEngineMessage.Msg_LC_RequestDareResult), HandleRequestDareResult);
            RegisterMsgHandler(JsonMessageID.SyncResetGowPrize, HandleSyncResetGowPrize);
            RegisterMsgHandler(JsonMessageID.RequestGowPrize, typeof(ArkCrossEngineMessage.Msg_LC_RequestGowPrizeResult), HandleRequestGowPrizeResult);
        }
        private void FilterLogicEventByStory(ProxyPublishData data)
        {
            ClientStorySystem.Instance.SendMessage("logicevent", data.m_EventName, data.m_Group, data.m_Args);
        }
        private void SetIpAndChannel(string ip, string channel)
        {
            LogSystem.Debug("SetIpAndChannel DeviceInfo ip : {0}, channel : {1}", ip, channel);
            m_Ip = ip;
            if (channel.Length > 0)
            {
                m_GameChannelId = channel;
            }
        }
        private void InitDeviceInfo(string uniqueIdentifier, string version, string system)
        {
            try
            {
                m_UniqueIdentifier = uniqueIdentifier;
                m_ClientGameVersion = version;
                m_System = system;
                if (m_ClientGameVersion.Length <= 0)
                {
                    m_ClientGameVersion = VersionConfigProvider.Instance.GetVersionNum();
                }
            }
            catch (Exception ex)
            {
                LogicSystem.LogicLog("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void SelectServer(string serverAddress, int logicServerId)
        {
            try
            {
                m_Url = serverAddress;
                m_LogicServerId = logicServerId;
            }
            catch (Exception ex)
            {
                LogicSystem.LogicLog("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void DirectLoginLobby()
        {
            try
            {
                m_LoginMode = LoginMode.DirectLogin;
                m_Account = m_UniqueIdentifier;
                if (!IsConnected)
                {
                    ConnectIfNotOpen();
                    m_IsWaitStart = false;
                    m_HasLoggedOn = false;
                }
                else
                {
                    JsonData loginMsg = new JsonData();
                    loginMsg["m_Account"] = m_Account;
                    loginMsg["m_LoginServerId"] = m_LogicServerId;
                    SendMessage(JsonMessageID.DirectLogin, loginMsg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void AccountLoginLobby(string cymgMsg)
        {
            try
            {
                LogSystem.Debug("AccountLoginLobby DeviceInfo ip : {0}, channel : {1}", m_Ip, m_GameChannelId);
                JsonData cymgObj = JsonMapper.ToObject(cymgMsg);
                m_OpCode = int.Parse(cymgObj.GetString("opcode"));
                m_ChannelId = int.Parse(cymgObj.GetString("channelid"));
                m_Data = cymgObj.GetString("data");
                m_LoginMode = LoginMode.AccountLogin;
                m_Account = m_UniqueIdentifier;
                if (!IsConnected)
                {
                    ConnectIfNotOpen();
                    m_IsWaitStart = false;
                    m_HasLoggedOn = false;
                }
                else
                {
                    JsonData loginMsg = new JsonData();
                    loginMsg["m_Account"] = m_Account;
                    loginMsg["m_OpCode"] = m_OpCode;
                    loginMsg["m_ChannelId"] = m_ChannelId;
                    loginMsg["m_Data"] = m_Data;
                    loginMsg["m_LoginServerId"] = m_LogicServerId;
                    loginMsg["m_ClientGameVersion"] = m_ClientGameVersion;
                    loginMsg["m_System"] = m_System;
                    loginMsg["m_ClientLoginIp"] = m_Ip.Length > 0 ? m_Ip : GetIp();
                    loginMsg["m_GameChannelId"] = m_GameChannelId;
                    loginMsg["m_UniqueIdentifier"] = m_UniqueIdentifier;
                    SendMessage(JsonMessageID.AccountLogin, loginMsg);
                }
            }
            catch (LitJson.JsonException jsonEx)
            {
                //TODO:处理异常情况：CYMGSDK登录成功，但是传给游戏的Token有误
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void StopLoginLobby()
        {
            try
            {
                //账号登录失败，取消自动登录，断开网络连接
                m_IsWaitStart = true;
                m_HasLoggedOn = false;
                if (IsConnected)
                {
                    m_WebSocket.Close();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void CreateNickname()
        {
            try
            {
                JsonData sendMsg = new JsonData();
                sendMsg["m_Account"] = LobbyClient.Instance.AccountInfo.Account;
                SendMessage(JsonMessageID.CreateNickname, sendMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void CreateRole(int heroId, string nickname)
        {
            if (heroId <= 0)
            {
                return;
            }
            try
            {
                JsonData sendMsg = new JsonData();
                sendMsg["m_Account"] = LobbyClient.Instance.AccountInfo.Account;
                sendMsg["m_HeroId"] = heroId;
                sendMsg["m_Nickname"] = nickname;
                SendMessage(JsonMessageID.CreateRole, sendMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RoleEnter(int index)
        {
            if (index < 0 || index >= LobbyClient.Instance.AccountInfo.Players.Count)
            {
                return;
            }
            try
            {
                RoleInfo pi = LobbyClient.Instance.AccountInfo.Players[index];
                if (pi != null)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.RoleEnter);
                    msg.m_JsonData.Set("m_Account", LobbyClient.Instance.AccountInfo.Account);
                    ArkCrossEngineMessage.Msg_CL_RoleEnter protoData = new ArkCrossEngineMessage.Msg_CL_RoleEnter();
                    protoData.m_Guid = pi.Guid;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void ActivateAccount(string activationCode)
        {
            try
            {
                JsonData sendMsg = new JsonData();
                sendMsg["m_Account"] = m_Account;
                sendMsg["m_ActivationCode"] = activationCode;
                SendMessage(JsonMessageID.ActivateAccount, sendMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RequestExpedition(int scene_id, int tollgate_num)
        {
            try
            {
                if (WorldSystem.Instance.WaitMatchSceneId > 0)
                {
                    GfxSystem.PublishGfxEvent("ge_request_expedition_failure", "expedition", GeneralOperationResult.LC_Failure_InMatching);
                    return;
                }
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(scene_id);
                if (null != cfg && cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_EXPEDITION)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.RequestExpedition);
                    msg.m_JsonData.Set("m_Guid", m_Guid);
                    ArkCrossEngineMessage.Msg_CL_RequestExpedition protoData = new ArkCrossEngineMessage.Msg_CL_RequestExpedition();
                    protoData.m_SceneId = scene_id;
                    protoData.m_TollgateNum = tollgate_num;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void SelectScene(int id)
        {
            try
            {
                if (WorldSystem.Instance.WaitMatchSceneId > 0)
                {
                    if (id == (int)MatchSceneEnum.Gow)
                    {
                        if (WorldSystem.Instance.WaitMatchSceneId == (int)MatchSceneEnum.Attempt)
                        {
                            GfxSystem.PublishGfxEvent("ge_pvpmatch_result", "gow", GowMatchResult.TYPE_ALREADYATTEMPT);
                        }
                        if (WorldSystem.Instance.WaitMatchSceneId == (int)MatchSceneEnum.Gow)
                        {
                            GfxSystem.PublishGfxEvent("ge_pvpmatch_result", "gow", GowMatchResult.TYPE_ALREADYGOW);
                        }
                    }
                    return;
                }
                if (!WorldSystem.Instance.IsPureClientScene())
                {
                    return;
                }
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(id);
                if (null == cfg || cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE)
                {
                    JsonData singlePveMsg = new JsonData();
                    singlePveMsg.SetJsonType(JsonType.Object);
                    singlePveMsg.Set("m_Guid", m_Guid);
                    singlePveMsg.Set("m_SceneType", id);
                    SendMessage(JsonMessageID.SinglePVE, singlePveMsg);
                }
                else
                {
                    //todo：发多人组队请求
                    JsonMessage msg = new JsonMessage(JsonMessageID.RequestMatch);
                    msg.m_JsonData.Set("m_Guid", m_Guid);
                    ArkCrossEngineMessage.Msg_CL_RequestMatch protoData = new ArkCrossEngineMessage.Msg_CL_RequestMatch();
                    protoData.m_SceneType = id;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                    WorldSystem.Instance.WaitMatchSceneId = id;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void CancelMatch(MatchSceneEnum type)
        {
            try
            {
                if (WorldSystem.Instance.WaitMatchSceneId == (int)type)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.CancelMatch);
                    msg.m_JsonData.Set("m_Guid", m_Guid);
                    ArkCrossEngineMessage.Msg_CL_CancelMatch protoData = new ArkCrossEngineMessage.Msg_CL_CancelMatch();
                    protoData.m_SceneType = (int)type;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                    WorldSystem.Instance.WaitMatchSceneId = -1;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void StageClear(int mainCityId)
        {
            try
            {
                if (WorldSystem.Instance.IsPvapScene())
                {
                    PvapOver();
                    return;
                }
                if (mainCityId <= 0)
                {
                    mainCityId = LobbyClient.Instance.CurrentRole.CitySceneId;
                }
                else
                {
                    Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(mainCityId);
                    if (null != cfg && cfg.m_Type == (int)SceneTypeEnum.TYPE_PURE_CLIENT_SCENE)
                    {
                        ChangeCityScene(mainCityId);
                    }
                }
                if (WorldSystem.Instance.IsPveScene() || WorldSystem.Instance.IsPvpScene())
                {
                    UserInfo player = WorldSystem.Instance.GetPlayerSelf();
                    if (null != player)
                    {
                        CombatStatisticInfo combatInfo = player.GetCombatStatisticInfo();
                        JsonMessage msg = new JsonMessage(JsonMessageID.StageClear);
                        msg.m_JsonData.Set("m_Guid", m_Guid);
                        ArkCrossEngineMessage.Msg_CL_StageClear protoData = new ArkCrossEngineMessage.Msg_CL_StageClear();
                        protoData.m_HitCount = combatInfo.HitCount;
                        protoData.m_MaxMultHitCount = combatInfo.MaxMultiHitCount;

                        if (WorldSystem.Instance.IsExpeditionScene())
                        {
                            float hpmax = (float)player.GetActualProperty().HpMax;
                            float mpmax = (float)player.GetActualProperty().EnergyMax;
                            if (hpmax > 0f && mpmax > 0f)
                            {
                                protoData.m_Hp = (int)((float)player.Hp / hpmax * 1000);
                                protoData.m_Mp = (int)((float)player.Energy / mpmax * 1000);
                            }
                        }
                        else
                        {
                            protoData.m_Hp = player.Hp;
                            protoData.m_Mp = player.Energy;
                        }
                        protoData.m_Gold = player.Money;
                        protoData.m_MatchKey = LobbyClient.Instance.CurrentRole.MatchKey;
                        msg.m_ProtoData = protoData;
                        SendMessage(msg);
                        LogSystem.Debug("SendMessage StageClear to lobby");
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void PvapOver()
        {
            RoleInfo role = LobbyClient.Instance.CurrentRole;
            if (role != null)
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.ArenaChallengeOver);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_ArenaChallengeOver over_msg = new ArkCrossEngineMessage.Msg_CL_ArenaChallengeOver();
                over_msg.IsSuccess = role.ArenaStateInfo.IsChallengeSuccess;
                over_msg.ChallengerDamage = role.GetPlayerSelfInfo().MakeDamage;
                over_msg.TargetDamage = role.ArenaStateInfo.TargetInfo.MakeDamage;
                over_msg.Sign = role.ArenaStateInfo.Sign;
                foreach (var pair in role.ArenaStateInfo.CreatedPartners)
                {
                    ArkCrossEngineMessage.DamageInfoData damage_msg = new ArkCrossEngineMessage.DamageInfoData();
                    damage_msg.OwnerId = pair.Key;
                    damage_msg.Damage = pair.Value.MakeDamage;
                    over_msg.ChallengerPartnerDamage.Add(damage_msg);
                }
                foreach (var pair in role.ArenaStateInfo.ChallengeTarget.CreatedPartners)
                {
                    ArkCrossEngineMessage.DamageInfoData damage_msg = new ArkCrossEngineMessage.DamageInfoData();
                    damage_msg.OwnerId = pair.Key;
                    damage_msg.Damage = pair.Value.MakeDamage;
                    over_msg.TargetPartnerDamage.Add(damage_msg);
                }
                msg.m_ProtoData = over_msg;
                SendMessage(msg);
                GfxSystem.PublishGfxEvent("ge_stop_countdown", "ui");
            }
        }

        private void SweepStage(int sceneId, int count)
        {
            // 这里不判断扫荡的合法性， 由发起的UI负责判断
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.SweepStage);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_SweepStage protoData = new ArkCrossEngineMessage.Msg_CL_SweepStage();
                protoData.m_SceneId = sceneId;
                protoData.m_SweepTime = count;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception : {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void StartGame()
        {
            try
            {
                JsonData startGameMsg = new JsonData();
                startGameMsg.Set("m_Guid", m_Guid);
                SendMessage(JsonMessageID.StartGame, startGameMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void DiscardItem(int[] itemId, int[] propertyId)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.DiscardItem);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_DiscardItem protoData = new ArkCrossEngineMessage.Msg_CL_DiscardItem();
                int ct = itemId.Length;
                for (int i = 0; i < ct; i++)
                {
                    protoData.m_ItemId.Add(itemId[i]);
                }
                ct = propertyId.Length;
                for (int i = 0; i < ct; i++)
                {
                    protoData.m_PropertyId.Add(propertyId[i]);
                }
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void MountEquipment(int item_id, int property_id, int equipment_pos)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.MountEquipment);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_MountEquipment protoData = new ArkCrossEngineMessage.Msg_CL_MountEquipment();
                protoData.m_ItemID = item_id;
                protoData.m_PropertyID = property_id;
                protoData.m_EquipPos = equipment_pos;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void UnmountEquipment(int equipment_pos)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.UnmountEquipment);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_UnmountEquipment protoData = new ArkCrossEngineMessage.Msg_CL_UnmountEquipment();
                protoData.m_EquipPos = equipment_pos;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void MountSkill(int preset_index, int skill_id, SlotPosition slot_pos)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.MountSkill);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_MountSkill protoData = new ArkCrossEngineMessage.Msg_CL_MountSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_SlotPos = (int)slot_pos;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void UnmountSkill(int preset_index, SlotPosition slot_pos)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.UnmountSkill);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_UnmountSkill protoData = new ArkCrossEngineMessage.Msg_CL_UnmountSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SlotPos = (int)slot_pos;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void UpgradeSkill(int preset_index, int skill_id, bool allow_cost_gold)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.UpgradeSkill);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_UpgradeSkill protoData = new ArkCrossEngineMessage.Msg_CL_UpgradeSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_AllowCostGold = allow_cost_gold;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void UnlockSkill(int preset_index, int skill_id)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.UnlockSkill);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_UnlockSkill protoData = new ArkCrossEngineMessage.Msg_CL_UnlockSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_UserLevel = playerself.GetLevel();
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void SwapSkill(int preset_index, int skill_id, SlotPosition source_pos, SlotPosition target_pos)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.SwapSkill);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_SwapSkill protoData = new ArkCrossEngineMessage.Msg_CL_SwapSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_SourcePos = (int)source_pos;
                protoData.m_TargetPos = (int)target_pos;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RequestItemInfo()
        {
            try
            {
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    /// items
                    if (null != role_info.Items && role_info.Items.Count > 0)
                    {
                        int[] items = new int[role_info.Items.Count];
                        int[] items_num = new int[role_info.Items.Count];
                        int[] item_random_propertys = new int[role_info.Items.Count];
                        for (int i = 0; i < role_info.Items.Count; i++)
                        {
                            items[i] = role_info.Items[i].ItemId;
                            items_num[i] = role_info.Items[i].ItemNum;
                            item_random_propertys[i] = role_info.Items[i].RandomProperty;
                        }
                        GfxSystem.PublishGfxEvent("ge_add_item", "bag", items, items_num, item_random_propertys);
                    }
                    /// equips
                    if (null != role_info.Equips && role_info.Equips.Length > 0)
                    {
                        for (int i = 0; i < role_info.Equips.Length; i++)
                        {
                            if (null != role_info.Equips[i])
                            {
                                GfxSystem.PublishGfxEvent("ge_fiton_equipment", "equipment", role_info.Equips[i].ItemId, i, role_info.Equips[i].Level, role_info.Equips[i].RandomProperty, GeneralOperationResult.LC_Succeed);
                            }
                        }
                    }
                    /// new equip list
                    if (LobbyClient.Instance.CurrentRole.CitySceneId == WorldSystem.Instance.GetCurSceneId()
                      && role_info.NewEquipCache.Count > 0)
                    {
                        List<NewEquipInfo> info = new List<NewEquipInfo>();
                        info.AddRange(role_info.NewEquipCache);
                        GfxSystem.PublishGfxEvent("ge_new_equip", "equipment", info);
                        role_info.ResetNewEquipCache();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RequestEquipedSkill()
        {
            try
            {
                if (null != LobbyClient.Instance.CurrentRole && null != LobbyClient.Instance.CurrentRole.SkillInfos
                  && LobbyClient.Instance.CurrentRole.SkillInfos.Count > 0)
                {
                    GfxSystem.PublishGfxEvent("ge_equiped_skills", "ui", LobbyClient.Instance.CurrentRole.SkillInfos);
#if DEBUG
                    foreach (SkillInfo info in LobbyClient.Instance.CurrentRole.SkillInfos)
                    {
                        LogSystem.Debug("RequestEquipedSkill skill_id : {0}, skill_level : {1}, skill_slotpos : {2}", info.SkillId, info.SkillLevel, (SlotPosition)info.Postions.Presets[0]);
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RequestSkillInfo()
        {
            try
            {
                if (null != LobbyClient.Instance.CurrentRole && null != LobbyClient.Instance.CurrentRole.SkillInfos
                  && LobbyClient.Instance.CurrentRole.SkillInfos.Count > 0)
                {
                    GfxSystem.PublishGfxEvent("ge_init_skills", "skill", LobbyClient.Instance.CurrentRole.SkillInfos);
#if DEBUG
                    foreach (SkillInfo info in LobbyClient.Instance.CurrentRole.SkillInfos)
                    {
                        LogSystem.Debug("RequestSkillInfo skill_id : {0}, skill_level : {1}, skill_slotpos : {2}", info.SkillId, info.SkillLevel, (SlotPosition)info.Postions.Presets[0]);
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void UpgradeItem(int equipment_pos, int item_id, bool allow_cost_gold)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.UpgradeItem);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_UpgradeItem protoData = new ArkCrossEngineMessage.Msg_CL_UpgradeItem();
                protoData.m_Position = equipment_pos;
                protoData.m_ItemId = item_id;
                protoData.m_AllowCostGold = allow_cost_gold;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void SaveSkillPreset(int preset_index)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    LobbyClient.Instance.CurrentRole.CurPresetIndex = preset_index;
                    WorldSystem.Instance.RefixSkills(playerself);
                }
                JsonData saveSkillPresetMsg = new JsonData();
                saveSkillPresetMsg.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                saveSkillPresetMsg.Set("m_SelectedPresetIndex", preset_index);
                SendMessage(JsonMessageID.SaveSkillPreset, saveSkillPresetMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void AddAssets(int money = 0, int gold = 0, int exp = 0, int stamina = 0)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                if (0 == money && 0 == gold && 0 == exp && 0 == stamina)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.AddAssets);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_AddAssets protoData = new ArkCrossEngineMessage.Msg_CL_AddAssets();
                protoData.m_Money = money;
                protoData.m_Gold = gold;
                protoData.m_Exp = exp;
                protoData.m_Stamina = stamina;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void SendMail(string receiver, string title, string text, int expiry_date, int levelDemand, int item_id, int item_num, int money, int gold, int stamina)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonData sendMailMsg = new JsonData();
                sendMailMsg.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                sendMailMsg.Set("m_Receiver", receiver);
                sendMailMsg.Set("m_Title", title);
                sendMailMsg.Set("m_Text", text);
                sendMailMsg.Set("m_ExpiryDate", expiry_date);
                sendMailMsg.Set("m_LevelDemand", levelDemand);
                sendMailMsg.Set("m_ItemId", item_id);
                sendMailMsg.Set("m_ItemNum", item_num);
                sendMailMsg.Set("m_Money", money);
                sendMailMsg.Set("m_Gold", gold);
                sendMailMsg.Set("m_Stamina", stamina);
                SendMessage(JsonMessageID.SendMail, sendMailMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void AddItem(int item_id)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                if (item_id > 0)
                {
                    JsonData addItemMsg = new JsonData();
                    addItemMsg.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    addItemMsg.Set("m_ItemId", item_id);
                    addItemMsg.Set("m_ItemNum", 1);
                    SendMessage(JsonMessageID.AddItem, addItemMsg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void UpdateMaxUserCount(int maxUserCount, int maxUserCountPerLogicServer, int maxQueueingCount)
        {
            try
            {
                JsonMessage updateMaxUserCountMsg = new JsonMessage(JsonMessageID.GmUpdateMaxUserCount);
                updateMaxUserCountMsg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_GmUpdateMaxUserCount protoData = new ArkCrossEngineMessage.Msg_CL_GmUpdateMaxUserCount();
                protoData.m_MaxUserCount = maxUserCount;
                protoData.m_MaxUserCountPerLogicServer = maxUserCountPerLogicServer;
                protoData.m_MaxQueueingCount = maxQueueingCount;

                updateMaxUserCountMsg.m_ProtoData = protoData;
                SendMessage(updateMaxUserCountMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void LiftSkill(int skill_id)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                if (skill_id > 0)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.LiftSkill);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_LiftSkill protoData = new ArkCrossEngineMessage.Msg_CL_LiftSkill();
                    protoData.m_SkillId = skill_id;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void BuyStamina()
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    VipConfig config_data = VipConfigProvider.Instance.GetDataById(role_info.Vip);
                    if (role_info.BuyStaminaCount < (null == config_data ? role_info.Vip + 1 : config_data.m_Stamina)
                      /*&& role_info.CurStamina < role_info.StaminaMax*/)
                    {
                        JsonMessage msg = new JsonMessage(JsonMessageID.BuyStamina);
                        msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                        SendMessage(msg);
                    }
                    else
                    {
                        GeneralOperationResult result = GeneralOperationResult.LC_Failure_Overflow;
                        GfxSystem.PublishGfxEvent("ge_buy_stamina", "stamina", result);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void MidasTouch()
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.MidasTouch);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ExchangeGoods(int exchangeid, bool refresh)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.ExchangeGoods);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_ExchangeGoods protoData = new ArkCrossEngineMessage.Msg_CL_ExchangeGoods();
                protoData.m_ExchangeId = exchangeid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void SyncExchangeGoods(int currency)
        {
            try
            {
                if (currency == 0)
                {
                    UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                    if (null == playerself)
                        return;
                    JsonMessage msg = new JsonMessage(JsonMessageID.RequestRefreshExchange);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_RequestRefreshExchange protoData = new ArkCrossEngineMessage.Msg_CL_RequestRefreshExchange();
                    protoData.m_RequestRefresh = true;
                    protoData.m_CurrencyId = 0;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
                else
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    if (null != role_info)
                    {
                        List<int> exchangeidarry = new List<int>();
                        List<int> exchangenumarry = new List<int>();
                        StoreConfig sc = null;
                        foreach (KeyValuePair<int, int> pair in role_info.ExchangeGoodsDic)
                        {
                            int exid = pair.Key;
                            sc = StoreConfigProvider.Instance.GetDataById(exid);
                            if (sc != null && sc.m_Currency == currency)
                            {
                                exchangeidarry.Add(exid);
                                exchangenumarry.Add(pair.Value);
                            }
                        }
                        GfxSystem.PublishGfxEvent("ge_sync_exchanges", "store", currency, exchangeidarry, exchangenumarry);
                        if (role_info.RefreshDic.ContainsKey(currency))
                        {
                            GfxSystem.PublishGfxEvent("ge_refresh_exchange_num", "store", currency, role_info.RefreshDic[currency]);
                        }
                        else
                        {
                            GfxSystem.PublishGfxEvent("ge_refresh_exchange_num", "store", currency, 0);
                        }
                        role_info.CurrencyId = currency;
                        ItemDataInfo idi = role_info.GetItemData(currency, 0);
                        if (idi != null)
                        {
                            role_info.ExchangeCurrency = idi.ItemNum;
                        }
                        else
                        {
                            role_info.ExchangeCurrency = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void RefreshExchangeGoods(int currency)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestRefreshExchange);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_RequestRefreshExchange protoData = new ArkCrossEngineMessage.Msg_CL_RequestRefreshExchange();
                protoData.m_RequestRefresh = true;
                protoData.m_CurrencyId = currency;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void SceneStartTime()
        {
            GfxSystem.PublishGfxEvent("ge_send_scenestart_time", "ui", WorldSystem.Instance.SceneStartTime);
        }
        internal void QuitBattle(bool isQuitTeam)
        {
            if (WorldSystem.Instance.IsPvpScene())
            {
                LobbyNetworkSystem.Instance.QuitRoom(isQuitTeam);
                NetworkSystem.Instance.QuitBattle(false);
                WorldSystem.Instance.QueueAction((MyFunc<int, bool>)WorldSystem.Instance.ChangeScene, LobbyClient.Instance.CurrentRole.CitySceneId);
            }
            else if (WorldSystem.Instance.IsMultiPveScene())
            {
                LobbyNetworkSystem.Instance.QuitRoom(isQuitTeam);
                NetworkSystem.Instance.QuitBattle(true);
                WorldSystem.Instance.QueueAction((MyFunc<int, bool>)WorldSystem.Instance.ChangeScene, LobbyClient.Instance.CurrentRole.CitySceneId);
            }
            else
            {
                WorldSystem.Instance.ChangeScene(LobbyClient.Instance.CurrentRole.CitySceneId);
                LobbyNetworkSystem.Instance.QuitPve();
            }
        }
        internal void PublishNotice(string content, int roll_num)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.PublishNotice);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_PublishNotice protoData = new ArkCrossEngineMessage.Msg_CL_PublishNotice();
                protoData.m_Content = content;
                protoData.m_RollNum = roll_num;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void FinishExpedition(int scene_id, int tollgate_num, int hp, int mp, int rage)
        {
            try
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    ExpeditionPlayerInfo e_info_ = role_info.GetExpeditionInfo();
                    if (null != e_info_)
                    {
                        e_info_.Hp = hp;
                        e_info_.Mp = mp;
                        e_info_.Rage = rage;
                        LogSystem.Debug("finish expedition num:{0},hp:{1},mp{2},rage{3}", tollgate_num, e_info_.Hp, e_info_.Mp, e_info_.Rage);
                    }
                    JsonMessage msg = new JsonMessage(JsonMessageID.FinishExpedition);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_FinishExpedition protoData = new ArkCrossEngineMessage.Msg_CL_FinishExpedition();
                    protoData.m_SceneId = scene_id;
                    protoData.m_TollgateNum = tollgate_num;
                    protoData.m_Hp = hp;
                    protoData.m_Mp = mp;
                    protoData.m_Rage = rage;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ExpeditionAward(int tollgate_num)
        {
            try
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    JsonMessage expeditionAwardMsg = new JsonMessage(JsonMessageID.ExpeditionAward);
                    expeditionAwardMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_ExpeditionAward protoData = new ArkCrossEngineMessage.Msg_CL_ExpeditionAward();
                    protoData.m_TollgateNum = tollgate_num;
                    expeditionAwardMsg.m_ProtoData = protoData;
                    SendMessage(expeditionAwardMsg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void RequestGowStarList(int start, int count)
        {
            try
            {
                JsonMessage getGowStarListMsg = new JsonMessage(JsonMessageID.GetGowStarList);
                getGowStarListMsg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_GetGowStarList protoData = new ArkCrossEngineMessage.Msg_CL_GetGowStarList();
                protoData.m_Start = start;
                protoData.m_Count = count;
                getGowStarListMsg.m_ProtoData = protoData;
                SendMessage(getGowStarListMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void QueryExpeditionInfo()
        {
            try
            {
                JsonData queryExpeditionInfoMsg = new JsonData();
                queryExpeditionInfoMsg.Set("m_Guid", m_Guid);
                SendMessage(JsonMessageID.QueryExpeditionInfo, queryExpeditionInfoMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ReadFinishMission(int id)
        {
            try
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.FinishMission);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_FinishMission protoData = new ArkCrossEngineMessage.Msg_CL_FinishMission();
                    protoData.m_MissionId = id;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ExpeditionFailure()
        {
            try
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.ExpeditionFailure);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ReloadMissions()
        {
            try
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    MissionStateInfo missionState = role_info.GetMissionStateInfo();
                    if (null != missionState)
                    {
                        foreach (MissionInfo mi in missionState.CompletedMissions.Values)
                        {
                            //LogSystem.Debug("Reload completed mission {0}", mi.MissionId);
                            GfxSystem.PublishGfxEvent("ge_about_task", "task", mi.MissionId, MissionOperationType.FINISH, mi.Progress);
                        }
                        foreach (MissionInfo mi in missionState.UnCompletedMissions.Values)
                        {
                            //LogSystem.Debug("Reload uncompleted mission {0}", mi.MissionId);
                            GfxSystem.PublishGfxEvent("ge_about_task", "task", mi.MissionId, MissionOperationType.ADD, mi.Progress);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void RequestRoleProperty()
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                CharacterProperty info = playerself.GetActualProperty();
                if (null != info)
                {
                    result = GeneralOperationResult.LC_Succeed;
                }
                GfxSystem.PublishGfxEvent("ge_request_role_property", "property", info, result);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RequestRelive(bool isRelive)
        {
            try
            {
                if (isRelive)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.BuyLife);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    SendMessage(msg);
                }
                else
                {
                    // 放弃副本
                    if (WorldSystem.Instance.IsPveScene())
                    {
                        ClientStorySystem.Instance.SendMessage("missionfailed");
                        LobbyNetworkSystem.Instance.QuitPve();
                    }
                    else
                    {
                        if (WorldSystem.Instance.IsMultiPveScene())
                        {
                            if (WorldSystem.Instance.IsAttemptScene())
                            {
                                QuitBattle(true);
                            }
                        }
                        else
                        {
                            //通知roomserver 放弃副本
                            NetworkSystem.Instance.SyncGiveUpCombat();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void ReturnMainCity()
        {
            try
            {
                WorldSystem.Instance.ChangeScene(LobbyClient.Instance.CurrentRole.CitySceneId);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void UnlockLegacy(int index, int item_id)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                if (index >= 0 && index < LegacyStateInfo.c_LegacyCapacity)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.UnlockLegacy);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_UnlockLegacy protoData = new ArkCrossEngineMessage.Msg_CL_UnlockLegacy();
                    protoData.m_Index = index;
                    protoData.m_ItemID = item_id;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void UpgradeLegacy(int index, int item_id, bool allow_cost_gold)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                if (index >= 0 && index < LegacyStateInfo.c_LegacyCapacity)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.UpgradeLegacy);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_UpgradeLegacy protoData = new ArkCrossEngineMessage.Msg_CL_UpgradeLegacy();
                    protoData.m_Index = index;
                    protoData.m_ItemID = item_id;
                    protoData.m_AllowCostGold = allow_cost_gold;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void AddXSoulExperience(XSoulPart part, int use_item_id, int use_num)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                {
                    return;
                }
                JsonMessage msg = new JsonMessage(JsonMessageID.AddXSoulExperience);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_AddXSoulExperience protoData = new ArkCrossEngineMessage.Msg_CL_AddXSoulExperience();
                protoData.m_XSoulPart = (int)part;
                protoData.m_UseItemId = use_item_id;
                protoData.m_ItemNum = use_num;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1|", ex.Message, ex.StackTrace);
            }
        }
        private void XSoulChangeShowModel(XSoulPart part, int model_level)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                {
                    return;
                }
                XSoulPartInfo part_info = playerself.GetXSoulInfo().GetXSoulPartData((XSoulPart)part);
                if (part_info == null || part_info.XSoulPartItem.Level < model_level)
                {
                    LogSystem.Debug("----change xsoul model error: model_level {0} can't larger than cur level {1}!", model_level, part_info.XSoulPartItem.Level);
                    GfxSystem.PublishGfxEvent("ge_xsoul_changemodel_result", "XSoul", (int)part, model_level, (int)(GeneralOperationResult.LC_Failure_LevelError));
                    return;
                }
                //LogSystem.Debug("----change xsoul model send message");
                JsonMessage msg = new JsonMessage(JsonMessageID.XSoulChangeShowModel);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_XSoulChangeShowModel protoData = new ArkCrossEngineMessage.Msg_CL_XSoulChangeShowModel();
                protoData.m_XSoulPart = (int)part;
                protoData.m_ModelLevel = model_level;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1|", ex.Message, ex.StackTrace);
            }
        }
        private void RequestAttemptAward()
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                {
                    return;
                }
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestMpveAward);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1|", ex.Message, ex.StackTrace);
            }
        }
        private void RequestPlayerInfo(string nick)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null == role_info)
                    return;
                ///
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestPlayerInfo);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_RequestPlayerInfo protoData = new ArkCrossEngineMessage.Msg_CL_RequestPlayerInfo();
                protoData.m_Nick = nick;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1|", ex.Message, ex.StackTrace);
            }
        }
        private void RequestMailList()
        {
            try
            {
                JsonData getMailListMsg = new JsonData();
                getMailListMsg.Set("m_Guid", m_Guid);
                SendMessage(JsonMessageID.GetMailList, getMailListMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void ReadMail(ulong mailGuid)
        {
            try
            {
                JsonMessage readMailMsg = new JsonMessage(JsonMessageID.ReadMail);
                readMailMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_ReadMail protoData = new ArkCrossEngineMessage.Msg_CL_ReadMail();
                protoData.m_MailGuid = mailGuid;
                readMailMsg.m_ProtoData = protoData;
                SendMessage(readMailMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void ReceiveMail(ulong mailGuid)
        {
            try
            {
                JsonMessage receiveMailMsg = new JsonMessage(JsonMessageID.ReceiveMail);
                receiveMailMsg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_ReceiveMail protoData = new ArkCrossEngineMessage.Msg_CL_ReceiveMail();
                protoData.m_MailGuid = mailGuid;
                receiveMailMsg.m_ProtoData = protoData;
                SendMessage(receiveMailMsg);
                ///
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    role_info.RemoveMailByGuid(mailGuid);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void UpdateFightingScore(float score)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself || score <= 0)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.UpdateFightingScore);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_UpdateFightingScore protoData = new ArkCrossEngineMessage.Msg_CL_UpdateFightingScore();
                protoData.score = (int)score;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ExpeditionReset(int hp, int mp, int rage, int request_num, bool is_reset, bool allow_cost_gold)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself)
                    return;
                JsonMessage msg = new JsonMessage(JsonMessageID.ExpeditionReset);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_ExpeditionReset protoData = new ArkCrossEngineMessage.Msg_CL_ExpeditionReset();
                protoData.m_Hp = hp;
                protoData.m_Mp = mp;
                protoData.m_Rage = rage;
                protoData.m_RequestNum = request_num;
                protoData.m_IsReset = is_reset;
                protoData.m_AllowCostGold = allow_cost_gold;
                protoData.m_Timestamp = Convert.ToInt64(TimeUtility.CurTimestamp);
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ResetDailyMissions()
        {
            JsonData msg = new JsonData();
            msg.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
            SendMessage(JsonMessageID.GMResetDailyMissions, msg);
        }
        private void HandleTooManyOperations(JsonMessage lobbyMsg)
        {
            //通知玩家操作过于频繁
            WorldSystem.Instance.HighlightPrompt(20);
        }

        internal void QueryArenaInfo()
        {
            JsonMessage msg = new JsonMessage(JsonMessageID.QueryArenaInfo);
            msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
            ArkCrossEngineMessage.Msg_CL_QueryArenaInfo protoData = new ArkCrossEngineMessage.Msg_CL_QueryArenaInfo();
            msg.m_ProtoData = protoData;
            SendMessage(msg);
        }

        internal void QueryArenaMatchGroup()
        {
            JsonMessage msg = new JsonMessage(JsonMessageID.QueryArenaMatchGroup);
            msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
            ArkCrossEngineMessage.Msg_CL_QueryArenaMatchGroup protoData = new ArkCrossEngineMessage.Msg_CL_QueryArenaMatchGroup();
            msg.m_ProtoData = protoData;
            SendMessage(msg);
        }

        internal void OnStartChallenge(ulong guid)
        {
            if (WorldSystem.Instance.WaitMatchSceneId > 0)
            {
                int error_code = -1;
                GfxSystem.PublishGfxEvent("start_challenge_result", "arena", error_code);
                return;
            }
            JsonMessage msg = new JsonMessage(JsonMessageID.ArenaStartChallenge);
            msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
            ArkCrossEngineMessage.Msg_CL_ArenaStartChallenge protoData = new ArkCrossEngineMessage.Msg_CL_ArenaStartChallenge();
            protoData.m_TargetGuid = guid;
            msg.m_ProtoData = protoData;
            SendMessage(msg);
        }

        internal void OnChallengeOver(bool is_success)
        {
            PvapOver();
            //LogSystem.Debug("----send challenge over msg!");
        }

        private void OnQueryRankList()
        {
            JsonMessage msg = new JsonMessage(JsonMessageID.ArenaQueryRank);
            msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
            ArkCrossEngineMessage.Msg_CL_ArenaQueryRank protoData = new ArkCrossEngineMessage.Msg_CL_ArenaQueryRank();
            msg.m_ProtoData = protoData;
            SendMessage(msg);
        }

        private void OnChangePartners(int[] partners)
        {
            JsonMessage msg = new JsonMessage(JsonMessageID.ArenaChangePartners);
            msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
            ArkCrossEngineMessage.Msg_CL_ArenaChangePartner protoData = new ArkCrossEngineMessage.Msg_CL_ArenaChangePartner();
            protoData.Partners.AddRange(partners);
            msg.m_ProtoData = protoData;
            SendMessage(msg);
        }

        private void OnQueryHistory()
        {
            JsonMessage msg = new JsonMessage(JsonMessageID.ArenaQueryHistory);
            msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
            ArkCrossEngineMessage.Msg_CL_ArenaQueryHistory protoData = new ArkCrossEngineMessage.Msg_CL_ArenaQueryHistory();
            msg.m_ProtoData = protoData;
            SendMessage(msg);
        }

        internal void CompoundEquip(int partid)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.CompoundEquip);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_CompoundEquip protoData = new ArkCrossEngineMessage.Msg_CL_CompoundEquip();
                protoData.m_PartId = partid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void OnBuyArenaFightCount()
        {
            JsonMessage msg = new JsonMessage(JsonMessageID.ArenaBuyFightCount);
            msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
            ArkCrossEngineMessage.Msg_CL_ArenaBuyFightCount protoData = new ArkCrossEngineMessage.Msg_CL_ArenaBuyFightCount();
            msg.m_ProtoData = protoData;
            SendMessage(msg);
        }

        private void OnPvpBeginFight()
        {
            if (WorldSystem.Instance.IsPvapScene())
            {
                ClientStorySystem.Instance.SendMessage("onbeginfight");
                RoleInfo self = LobbyClient.Instance.CurrentRole;
                if (self != null)
                {
                    self.ArenaStateInfo.BeginFight();
                    JsonMessage msg = new JsonMessage(JsonMessageID.ArenaBeginFight);
                    msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_ArenaBeginFight protoData = new ArkCrossEngineMessage.Msg_CL_ArenaBeginFight();
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
        }

        private void OnCheckPvapResult()
        {
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            if (roleself == null)
            {
                return;
            }
            if (WorldSystem.Instance.IsPvapScene())
            {
                roleself.ArenaStateInfo.IsCheckingResult = true;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////
        private void HandleVersionVerifyResult(JsonMessage lobbyMsg)
        {
            LogSystem.Debug("HandleVersionVerifyResult DeviceInfo ip : {0}, channel : {1}", m_Ip, m_GameChannelId);
            JsonData jsonData = lobbyMsg.m_JsonData;
            int ret = jsonData.GetInt("m_Result");
            if (0 == ret)
            {
                //版本校验失败，提示用户需要更新版本。
                m_IsWaitStart = true;
                m_HasLoggedOn = false;
                if (m_WebSocket != null)
                {
                    m_WebSocket.Close();
                }
                GfxSystem.PublishGfxEvent("ge_show_dialog", "ui", Dict.Get(5), Dict.Get(4), null, null, null, true);
            }
            else
            {
                //向服务器发送登录消息
                if (m_LoginMode == LoginMode.DirectLogin)
                {
                    JsonData loginMsg = new JsonData();
                    loginMsg["m_Account"] = m_Account;
                    loginMsg["m_LoginServerId"] = m_LogicServerId;
                    SendMessage(JsonMessageID.DirectLogin, loginMsg);
                }
                else if (m_LoginMode == LoginMode.AccountLogin)
                {
                    JsonData loginMsg = new JsonData();
                    loginMsg["m_Account"] = m_Account;
                    loginMsg["m_OpCode"] = m_OpCode;
                    loginMsg["m_ChannelId"] = m_ChannelId;
                    loginMsg["m_Data"] = m_Data;
                    loginMsg["m_LoginServerId"] = m_LogicServerId;
                    loginMsg["m_ClientGameVersion"] = m_ClientGameVersion;
                    loginMsg["m_System"] = m_System;
                    loginMsg["m_ClientLoginIp"] = m_Ip.Length > 0 ? m_Ip : GetIp();
                    loginMsg["m_GameChannelId"] = m_GameChannelId;
                    loginMsg["m_UniqueIdentifier"] = m_UniqueIdentifier;
                    SendMessage(JsonMessageID.AccountLogin, loginMsg);
                }
            }
        }
        private void HandleAccountLoginResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            int ret = jsonData.GetInt("m_Result");
            string accountId = jsonData.GetString("m_AccountId");
            LobbyNetworkSystem.Instance.IsQueueing = false;
            if (m_HasLoggedOn)
            {//重连处理
                JsonMessage msg = new JsonMessage(JsonMessageID.RoleEnter);
                msg.m_JsonData.Set("m_Account", LobbyClient.Instance.AccountInfo.Account);
                ArkCrossEngineMessage.Msg_CL_RoleEnter protoData = new ArkCrossEngineMessage.Msg_CL_RoleEnter();
                protoData.m_Guid = m_Guid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            else
            {//首次登录处理
                if (ret == (int)AccountLoginResult.Success)
                {
                    LobbyClient.Instance.AccountInfo.Account = m_Account;
                    //登录成功，向服务器请求玩家角色
                    JsonMessage sendMsg = new JsonMessage(JsonMessageID.RoleList);
                    sendMsg.m_JsonData.Set("m_Account", m_Account);
                    SendMessage(sendMsg);
                }
                else if (ret == (int)AccountLoginResult.FirstLogin)
                {
                    //账号首次登录，需要验证激活码        
                }
                else if (ret == (int)AccountLoginResult.Wait)
                {
                    //同时登录人太多，需要等待一段时间后再登录
                    WorldSystem.Instance.HighlightPrompt(20);
                    return;
                }
                else if (ret == (int)AccountLoginResult.Banned)
                {
                    //账号已被封停，禁止登录
                }
                else if (ret == (int)AccountLoginResult.Queueing)
                {
                    WorldSystem.Instance.HighlightPrompt(29);
                    LobbyNetworkSystem.Instance.IsQueueing = true;
                    return;
                }
                else
                {
                    //账号登录失败
                }
                GfxSystem.PublishGfxEvent("ge_login_result", "lobby", ret, accountId);
            }
        }
        private void HandleActivateAccountResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            int ret = jsonData.GetInt("m_Result");
            if (ret == (int)ActivateAccountResult.Success)
            {
                //激活成功，账号成功登陆
                LobbyClient.Instance.AccountInfo.Account = m_Account;
            }
            else
            {
                //激活失败
            }
            GfxSystem.PublishGfxEvent("ge_activate_result", "lobby", ret);
        }
        private void HandleRoleListResult(JsonMessage lobbyMsg)
        {
            bool isSuccess = false;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RoleListResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RoleListResult;
            if (null != protoData)
            {
                int ret = protoData.m_Result;
                if (ret == (int)RoleListResult.Success)
                {
                    //清空客户端已有的玩家角色列表
                    LobbyClient.Instance.AccountInfo.Players.Clear();
                    //获取玩家角色数据列表 
                    int userinfoCount = protoData.m_UserInfoCount;
                    if (null != protoData.m_UserInfos && protoData.m_UserInfos.Count > 0)
                    {
                        int ct = protoData.m_UserInfos.Count;
                        for (int i = 0; i < ct; ++i)
                        {
                            RoleInfo player = new RoleInfo();
                            player.Guid = protoData.m_UserInfos[i].m_UserGuid;
                            player.Nickname = protoData.m_UserInfos[i].m_Nickname;
                            player.HeroId = protoData.m_UserInfos[i].m_HeroId;
                            player.Level = protoData.m_UserInfos[i].m_Level;
                            LobbyClient.Instance.AccountInfo.Players.Add(player);
                        }
                    }
                    isSuccess = true;
                }
                else
                {
                    isSuccess = false;
                }
                GfxSystem.PublishGfxEvent("ge_rolelist_result", "lobby", isSuccess);
            }
        }
        private void HandleCreateNicknameResult(JsonMessage lobbyMsg)
        {
            List<string> nicknameList = new List<string>();
            JsonData jsonData = lobbyMsg.m_JsonData;
            JsonData nicknames = jsonData["m_Nicknames"];
            if (nicknames.IsArray && nicknames.Count > 0)
            {
                for (int i = 0; i < nicknames.Count; ++i)
                {
                    nicknameList.Add(nicknames[i].AsString());
                }
            }
            GfxSystem.PublishGfxEvent("ge_nickname_result", "lobby", nicknameList);
        }
        private void HandleCreateRoleResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            int ret = jsonData.GetInt("m_Result");
            if (ret == (int)CreateRoleResult.Success)
            {
                JsonData userInfo = jsonData["m_UserInfo"];
                ulong userGuid = userInfo.GetUlong("m_UserGuid");
                RoleInfo player = new RoleInfo();
                player.Guid = userGuid;
                player.Nickname = userInfo.GetString("m_Nickname");
                player.HeroId = userInfo.GetInt("m_HeroId");
                player.Level = userInfo.GetInt("m_Level");
                LobbyClient.Instance.AccountInfo.Players.Add(player);
                //服务器自动开始游戏，客户端不需要再主动发RoleEnter消息
                //JsonMessage msg = new JsonMessage(JsonMessageID.RoleEnter);
                //msg.m_JsonData.Set("m_Account", m_Account);
                //ArkCrossEngineMessage.Msg_CL_RoleEnter protoData = new ArkCrossEngineMessage.Msg_CL_RoleEnter();
                //protoData.m_Guid = userGuid;
                //msg.m_ProtoData = protoData;
                //SendMessage(msg);
                GfxSystem.PublishGfxEvent("ge_createhero_result", "lobby", true);
            }
            else
            {
                //角色创建失败
                GfxSystem.PublishGfxEvent("ge_createhero_result", "lobby", false);
            }
        }
        private void HandleRoleEnterResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RoleEnterResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RoleEnterResult;
            if (null != protoData)
            {
                int ret = protoData.m_Result;
                ulong userGuid = jsonData.GetUlong("m_Guid");
                RoleInfo role = LobbyClient.Instance.AccountInfo.FindRole(userGuid);
                if (ret == (int)RoleEnterResult.Wait)
                {
                    m_Guid = userGuid;
                    Thread.Sleep(2000);
                    JsonMessage msg = new JsonMessage(JsonMessageID.RoleEnter);
                    msg.m_JsonData.Set("m_Account", LobbyClient.Instance.AccountInfo.Account);
                    ArkCrossEngineMessage.Msg_CL_RoleEnter data = new ArkCrossEngineMessage.Msg_CL_RoleEnter();
                    data.m_Guid = m_Guid;
                    msg.m_ProtoData = data;
                    SendMessage(msg);
                    LogSystem.Debug("Retry RoleEnter {0} {1}", LobbyClient.Instance.AccountInfo.Account, m_Guid);
                    return;
                }
                else if (ret == (int)RoleEnterResult.Success)
                {
                    if (role != null)
                    {
                        m_IsLogining = false;
                        if (m_HasLoggedOn)
                        {
                            GfxSystem.PublishGfxEvent("ge_ui_connect_hint", "ui", false, false);
                            return;
                        }
                        //客户端接收服务器传来的数据，创建玩家对象
                        m_WorldId = protoData.m_WorldId;
                        /// money gold
                        role.NewBieGuideScene = protoData.m_NewbieGuideScene;
                        role.Money = protoData.m_Money;
                        role.Gold = protoData.m_Gold;
                        role.CurStamina = protoData.m_Stamina;
                        role.Vigor = protoData.m_Vigor;
                        role.Exp = protoData.m_Exp;
                        role.Level = protoData.m_Level;
                        role.CitySceneId = protoData.m_CitySceneId;
                        role.BuyStaminaCount = protoData.m_BuyStaminaCount;
                        role.BuyMoneyCount = protoData.m_BuyMoneyCount;
                        role.SellItemGoldIncome = protoData.m_CurSellItemGoldIncome;
                        role.Vip = protoData.m_Vip;
                        role.RestSignInCount = protoData.m_RestSignInCountCurDay;
                        role.SignInCountCurMonth = protoData.m_SignInCountCurMonth;
                        role.IsGetLoginReward = protoData.m_IsGetWeeklyReward;
                        role.DailyOnLineDuration = protoData.m_OnlineDuration;
                        role.OnlineDurationStartTime = DateTime.Now;
                        role.SetActionFlag(protoData.m_GuideFlag);
                        for (int i = 0; i < protoData.m_WeeklyRewardRecord.Count; ++i)
                        {
                            role.WeeklyLoginRewardRecord.Add(protoData.m_WeeklyRewardRecord[i]);
                        }
                        for (int i = 0; i < protoData.m_OnlineTimeRewardedIndex.Count; ++i)
                        {
                            role.OnLineDurationRewardedIndex.Add(protoData.m_OnlineTimeRewardedIndex[i]);
                        }
                        // 通关信息
                        if (null != protoData.m_SceneData && protoData.m_SceneData.Count > 0)
                        {
                            int ct = protoData.m_SceneData.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                role.SetSceneInfo(protoData.m_SceneData[i].m_SceneId, protoData.m_SceneData[i].m_Grade);
                            }
                        }
                        //通关次数
                        if (null != protoData.m_SceneCompletedCountData && protoData.m_SceneCompletedCountData.Count > 0)
                        {
                            int ct = protoData.m_SceneCompletedCountData.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                role.AddCompletedSceneCount(protoData.m_SceneCompletedCountData[i].m_SceneId, protoData.m_SceneCompletedCountData[i].m_Count);
                            }
                        }
                        // 新手教学信息
                        if (null != protoData.m_NewbieGuides && protoData.m_NewbieGuides.Count > 0)
                        {
                            int ct = protoData.m_NewbieGuides.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                if (!role.NewbieGuides.Contains(protoData.m_NewbieGuides[i]))
                                {
                                    role.NewbieGuides.Add(protoData.m_NewbieGuides[i]);
                                }
                            }
                        }
                        /// items
                        if (null != protoData.m_BagItems && protoData.m_BagItems.Count > 0 && null != role.Items)
                        {
                            int ct = protoData.m_BagItems.Count;
                            for (int i = 0; i < ct; i++)
                            {
                                ItemDataInfo info = new ItemDataInfo();
                                info.ItemId = protoData.m_BagItems[i].ItemId;
                                info.Level = protoData.m_BagItems[i].Level;
                                info.ItemNum = protoData.m_BagItems[i].Num;
                                info.RandomProperty = protoData.m_BagItems[i].AppendProperty;
                                role.Items.Add(info);
                                if (info.ItemId == role.CurrencyId)
                                {
                                    role.ExchangeCurrency += info.ItemNum;
                                }
                            }
                        }
                        /// equipments
                        if (null != protoData.m_Equipments && protoData.m_Equipments.Count > 0 && null != role.Equips)
                        {
                            int ct = protoData.m_Equipments.Count;
                            for (int i = 0; i < ct; i++)
                            {
                                if (null != protoData.m_Equipments[i])
                                {
                                    ItemDataInfo info = new ItemDataInfo();
                                    info.ItemId = protoData.m_Equipments[i].ItemId;
                                    info.Level = protoData.m_Equipments[i].Level;
                                    info.ItemNum = protoData.m_Equipments[i].Num;
                                    info.RandomProperty = protoData.m_Equipments[i].AppendProperty;
                                    role.SetEquip(i, info);
                                }
                            }
                        }
                        /// skills
                        if (null != protoData.m_SkillInfo && protoData.m_SkillInfo.Count > 0 && null != role.SkillInfos)
                        {
                            Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(role.HeroId);
                            if (null != playerData && null != playerData.m_PreSkillList)
                            {
                                foreach (int skill_id_ in playerData.m_PreSkillList)
                                {
                                    role.SkillInfos.Add(new SkillInfo(skill_id_));
                                }
                            }
                            ///
                            int ct = protoData.m_SkillInfo.Count;
                            for (int i = 0; i < ct; i++)
                            {
                                int skill_id = protoData.m_SkillInfo[i].ID;
                                SkillInfo si = new SkillInfo(skill_id);
                                si.SkillLevel = protoData.m_SkillInfo[i].Level;
                                si.Postions.Presets[0] = (SlotPosition)protoData.m_SkillInfo[i].Postions;
                                for (int index = 0; index < role.SkillInfos.Count; index++)
                                {
                                    if (role.SkillInfos[index].SkillId == si.SkillId)
                                    {
                                        role.SkillInfos[index].SkillLevel = si.SkillLevel;
                                        role.SkillInfos[index].Postions = si.Postions;
                                        break;
                                    }
                                }
                            }
                        }
                        /// missions
                        MissionStateInfo mission_info = role.GetMissionStateInfo();
                        if (null != protoData.m_Missions && protoData.m_Missions.Count > 0 && null != mission_info)
                        {
                            int ct = protoData.m_Missions.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                if (protoData.m_Missions[i].m_IsCompleted)
                                {
                                    mission_info.AddMission(protoData.m_Missions[i].m_MissionId, MissionStateType.COMPLETED);
                                    mission_info.CompletedMissions[protoData.m_Missions[i].m_MissionId].Progress = protoData.m_Missions[i].m_Progress;
                                    //GfxSystem.PublishGfxEvent("ge_about_task", "task", missions[i].GetInt("m_MissionId"), MissionOperationType.FINISH, missions[i].GetString("m_Progress"));
                                }
                                else
                                {
                                    mission_info.AddMission(protoData.m_Missions[i].m_MissionId, MissionStateType.UNCOMPLETED);
                                    mission_info.UnCompletedMissions[protoData.m_Missions[i].m_MissionId].Progress = protoData.m_Missions[i].m_Progress;
                                    GfxSystem.PublishGfxEvent("ge_about_task", "task", protoData.m_Missions[i].m_MissionId, MissionOperationType.ADD, protoData.m_Missions[i].m_Progress);
                                }
                            }
                        }

                        /// legacys
                        if (null != protoData.m_Legacys && protoData.m_Legacys.Count > 0)
                        {
                            int ct = protoData.m_Legacys.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                if (null != protoData.m_Legacys[i])
                                {
                                    role.Legacys[i] = new ItemDataInfo();
                                    role.Legacys[i].ItemId = protoData.m_Legacys[i].ItemId;
                                    role.Legacys[i].Level = protoData.m_Legacys[i].Level;
                                    role.Legacys[i].RandomProperty = protoData.m_Legacys[i].AppendProperty;
                                    role.Legacys[i].IsUnlock = protoData.m_Legacys[i].IsUnlock;
                                }
                            }
                        }
                        // XSoulInfo
                        for (int i = 0; i < protoData.m_XSouls.Count; i++)
                        {
                            ArkCrossEngineMessage.XSoulDataMsg item_msg = protoData.m_XSouls[i];
                            if (item_msg == null)
                            {
                                continue;
                            }
                            ItemDataInfo item = new ItemDataInfo();
                            item.ItemId = item_msg.ItemId;
                            item.Level = item_msg.Level;
                            item.Experience = item_msg.Experience;
                            item.UpdateLevelByExperience();
                            ItemConfig item_config = ItemConfigProvider.Instance.GetDataById(item.ItemId);
                            item.ItemConfig = item_config;
                            if (item_config != null)
                            {
                                XSoulPart part = (XSoulPart)item_config.m_WearParts;
                                XSoulPartInfo part_info = new XSoulPartInfo(part, item);
                                part_info.ShowModelLevel = item_msg.ModelLevel;
                                role.GetXSoulInfo().SetXSoulPartData(part, part_info);
                            }
                        }

                        /// gow
                        if (null != protoData.m_Gow && null != role.Gow)
                        {
                            role.Gow.GowElo = protoData.m_Gow.GowElo;
                            role.Gow.GowMatches = protoData.m_Gow.GowMatches;
                            role.Gow.GowWinMatches = protoData.m_Gow.GowWinMatches;
                            role.Gow.LeftMatchCount = protoData.m_Gow.LeftMatchCount;
                            role.Gow.RankId = protoData.m_Gow.RankId;
                            role.Gow.Point = protoData.m_Gow.Point;
                            role.Gow.CriticalTotalMatches = protoData.m_Gow.CriticalTotalMatches;
                            role.Gow.AmassWinMatches = protoData.m_Gow.CriticalAmassWinMatches;
                            role.Gow.AmassLossMatches = protoData.m_Gow.CriticalAmassLossMatches;
                            role.Gow.IsAcquirePrize = protoData.m_Gow.IsAcquirePrize;
                        }

                        role.ArenaStateInfo.Init(role.Guid);

                        /// friends
                        if (null != protoData.m_Friends && null != role.Friends)
                        {
                            role.Friends.Clear();
                            int ct = protoData.m_Friends.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                ArkCrossEngineMessage.FriendInfoForMsg friend_msg = protoData.m_Friends[i];
                                if (null != friend_msg)
                                {
                                    FriendInfo friend_data = new FriendInfo();
                                    friend_data.Guid = friend_msg.Guid;
                                    friend_data.Nickname = friend_msg.Nickname;
                                    friend_data.Level = friend_msg.Level;
                                    friend_data.FightingScore = friend_msg.FightingScore;
                                    friend_data.IsBlack = friend_msg.IsBlack;
                                    if (!role.Friends.ContainsKey(friend_data.Guid))
                                    {
                                        role.Friends.Add(friend_data.Guid, friend_data);
                                    }
                                }
                            }
                        }
                        // partners
                        if (null != protoData.m_Partners && null != role.PartnerStateInfo)
                        {
                            role.PartnerStateInfo.Reset();
                            int ct = protoData.m_Partners.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                ArkCrossEngineMessage.Msg_LC_RoleEnterResult.PartnerDataMsg msg = protoData.m_Partners[i];
                                role.PartnerStateInfo.AddPartner(msg.m_Id, msg.m_SkillStage, msg.m_AdditionLevel);
                            }
                            role.PartnerStateInfo.SetActivePartner(protoData.m_ActivePartnerId);
                        }
                        //ExchangeGoods
                        if (null != protoData.m_Exchanges && null != role.ExchangeGoodsDic)
                        {
                            role.ExchangeGoodsDic.Clear();
                            int ct = protoData.m_Exchanges.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                ArkCrossEngineMessage.Msg_LC_RoleEnterResult.ExchangeGoodsMsg msg = protoData.m_Exchanges[i];
                                if (null != msg && !role.ExchangeGoodsDic.ContainsKey(msg.m_Id))
                                {
                                    role.ExchangeGoodsDic.Add(msg.m_Id, msg.m_Num);
                                }
                            }
                        }
                        if (null != protoData.m_RefreshExchangeNum && null != role.RefreshDic)
                        {
                            role.RefreshDic.Clear();
                            int ct = protoData.m_RefreshExchangeNum.Count;
                            for (int i = 0; i < ct; ++i)
                            {
                                ArkCrossEngineMessage.Msg_LC_RoleEnterResult.ExchangeRefreshMsg msg = protoData.m_RefreshExchangeNum[i];
                                if (null != msg && !role.ExchangeGoodsDic.ContainsKey(msg.m_CurrencyId))
                                {
                                    role.RefreshDic.Add(msg.m_CurrencyId, msg.m_Num);
                                }
                            }
                        }

                        // talents
                        ItemDataInfo card = new ItemDataInfo();
                        card.ItemId = 201;
                        card.Level = 10;
                        card.Experience = 0;
                        card.ItemConfig = ItemConfigProvider.Instance.GetDataById(card.ItemId);
                        role.TalentManager.EquipTalent(EquipSlot.kFirst, card);

                        // mpve
                        role.AttemptAward = protoData.m_AttemptAward;
                        role.AttemptCurAcceptedCount = protoData.m_AttemptCurAcceptedCount;
                        role.AttemptAcceptedAward = protoData.m_AttemptAcceptedAward;
                        role.GoldCurAcceptedCount = protoData.m_GoldCurAcceptedCount;

                        // 新手引导相关
                        role.NewbieFlag = protoData.m_NewbieFlag;
                        role.NewbieActionFlag = protoData.m_NewbieActionFlag;
                        //设置当前玩家角色
                        LobbyClient.Instance.CurrentRole = role;
                        m_Guid = userGuid;
                        if (null != LobbyClient.Instance.CurrentRole)
                        {
                            if (role.Level >= ExpeditionPlayerInfo.c_UnlockLevel)
                            {
                                QueryExpeditionInfo();
                            }
                        }
                        GfxSystem.PublishGfxEvent("ge_role_enter_log", "log", m_UniqueIdentifier, m_LogicServerId, role.Nickname, m_Guid, role.Level, m_Account);
                        //通知GTSDK
                        GfxSystem.PublishGfxEvent("ge_ActivateWithNickName", "gt",
                          role.Nickname,
                          m_Guid.ToString(),
                          m_WorldId.ToString(),
                          false,
                          3,
                          role.Level.ToString(),
                          "");

                        // Temp: disable new bie guide
                        /*
                        if (role.SceneInfo.Count == 0)
                        {
                            NetworkSystem.Instance.SceneId = LobbyClient.Instance.CurrentRole.NewBieGuideScene;
                        }
                        else
                        */
                        {
                            NetworkSystem.Instance.SceneId = LobbyClient.Instance.CurrentRole.CitySceneId;
                        }
                        
                        //进主城场景
                        NetworkSystem.Instance.HeroId = LobbyClient.Instance.CurrentRole.HeroId;
                        NetworkSystem.Instance.CampId = (int)CampIdEnum.Blue;
                        WorldSystem.Instance.ChangeScene(NetworkSystem.Instance.SceneId);
                        m_HasLoggedOn = true;
                    }
                }
                else
                {
                    //进入游戏失败
                }
                GfxSystem.PublishGfxEvent("ge_enter_result", "lobby", ret);
            }
        }
        private void HandleMatchResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_MatchResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MatchResult;
            if (null != protoData)
            {
                WorldSystem.Instance.WaitMatchSceneId = -1;
                TeamOperateResult result = (TeamOperateResult)protoData.m_Result;
                if (result == TeamOperateResult.OR_Succeed)
                {
                    StartGame();
                }
                else if (result == TeamOperateResult.OR_TimeError)
                {
                    GfxSystem.PublishGfxEvent("ge_pvpmatch_result", "gow", GowMatchResult.TYPE_TIMEWRONG);
                }
                else if (result == TeamOperateResult.OR_LevelError)
                {
                    GfxSystem.PublishGfxEvent("ge_pvpmatch_result", "gow", GowMatchResult.TYPE_LEVELWRONG);
                }
            }
        }
        private void HandleRequestExpeditionResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RequestExpeditionResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RequestExpeditionResult;
            if (null != protoData)
            {
                uint key = protoData.m_Key;
                string ip = protoData.m_ServerIp;
                int port = (int)protoData.m_ServerPort;
                int heroId = protoData.m_HeroId;
                int campId = protoData.m_CampId;
                int sceneId = protoData.m_SceneType;
                int activeTollgate = protoData.m_ActiveTollgate;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
                    if (null != cfg && cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_EXPEDITION)
                    {
                        NetworkSystem.Instance.HeroId = heroId;
                        NetworkSystem.Instance.CampId = campId;
                        NetworkSystem.Instance.SceneId = sceneId;
                        ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
                        if (null != expedition)
                        {
                            expedition.ActiveTollgate = activeTollgate;
                        }
                    }
                    GameControler.ChangeScene(sceneId);
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_request_expedition_failure", "expedition", result);
                }
            }
        }
        private void HandleFinishExpeditionResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_FinishExpeditionResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_FinishExpeditionResult;
            if (null != protoData)
            {
                int scene_id = protoData.m_SceneId;
                int tollgate_num = protoData.m_TollgateNum;
                int hp = protoData.m_Hp;
                int mp = protoData.m_Mp;
                int rage = protoData.m_Rage;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
                    if (null != expedition)
                    {
                        Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(scene_id);
                        if (null != cfg && cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_EXPEDITION)
                        {
                            if (expedition.Schedule == tollgate_num)
                            {
                                if (null != expedition.Tollgates[expedition.Schedule])
                                {
                                    expedition.Tollgates[expedition.Schedule].IsFinish = true;
                                    expedition.Tollgates[expedition.Schedule].IsAcceptedAward = false;
                                    expedition.Schedule += 1;
                                    if (hp > 0)
                                    {
                                        expedition.Hp = hp;
                                    }
                                    if (mp > 0)
                                    {
                                        expedition.Mp = mp;
                                    }
                                    expedition.Rage = rage;
                                    if (expedition.Schedule < expedition.Tollgates.Length)
                                    {
                                        ExpeditionReset(expedition.Hp, expedition.Mp, expedition.Rage, expedition.Schedule, false, false);
                                        LogSystem.Debug("request expedition info -> hp:{0},mp{1},rage{2}", expedition.Hp, expedition.Mp, expedition.Rage);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void HandleExpeditionAwardResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult;
            if (null != protoData)
            {
                int tollgate_num = protoData.m_TollgateNum;
                int add_money = protoData.m_AddMoney;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
                    if (null != expedition)
                    {
                        if (tollgate_num <= expedition.Schedule)
                        {
                            expedition.Tollgates[tollgate_num].IsAcceptedAward = true;
                        }
                    }
                    ///
                    List<int> item_id_list = new List<int>();
                    List<int> item_num_list = new List<int>();
                    if (null != protoData.m_Items && protoData.m_Items.Count > 0)
                    {
                        int ct = protoData.m_Items.Count;
                        for (int i = 0; i < ct; i++)
                        {
                            int cur_item_id = protoData.m_Items[i].m_Id;
                            int cur_item_num = protoData.m_Items[i].m_Num;
                            item_id_list.Add(cur_item_id);
                            item_num_list.Add(cur_item_num);
                        }
                    }
                    GfxSystem.PublishGfxEvent("ge_expedition_award", "expedition", add_money, item_id_list.ToArray(), item_num_list.ToArray(), result);
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_expedition_award", "expedition", add_money, null, null, result);
                }
            }
        }
        private void HandleSyncGowStarList(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncGowStarList protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncGowStarList;
            if (null != protoData)
            {
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    if (null != role_info.Gow && null != role_info.Gow.GowTop)
                    {
                        if (null != protoData.m_Stars && protoData.m_Stars.Count > 0)
                        {
                            role_info.Gow.GowTop.Clear();
                            int ct = protoData.m_Stars.Count;
                            for (int i = 0; i < ct; i++)
                            {
                                ArkCrossEngineMessage.Msg_LC_SyncGowStarList.GowStarInfoForMessage assit_data = protoData.m_Stars[i];
                                if (null != assit_data)
                                {
                                    GowDataForMsg info = new GowDataForMsg();
                                    info.m_Guid = assit_data.m_Guid;
                                    info.m_GowElo = assit_data.m_GowElo;
                                    info.m_Nick = assit_data.m_Nick;
                                    info.m_Heroid = assit_data.m_HeroId;
                                    info.m_Level = assit_data.m_Level;
                                    info.m_FightingScore = assit_data.m_FightingScore;
                                    info.m_RankId = assit_data.m_RankId;
                                    info.m_Point = assit_data.m_Point;
                                    info.m_CriticalTotalMatches = assit_data.m_CriticalTotalMatches;
                                    info.m_CriticalAmassWinMatches = assit_data.m_CriticalAmassWinMatches;
                                    info.m_CriticalAmassLossMatches = assit_data.m_CriticalAmassLossMatches;
                                    role_info.Gow.GowTop.Add(info);
                                }
                            }
                        }
                        int gs_ct = role_info.Gow.GowTop.Count;
                        if (gs_ct > 0 && WorldSystem.Instance.IsPureClientScene())
                        {
                            GfxSystem.PublishGfxEvent("ge_sync_gowstar_list", "gowstar", role_info.Gow.GowTop);
                        }
                    }
                }
            }
        }
        private void HandleStartGameResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_StartGameResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_StartGameResult;
            if (null != protoData)
            {
                GeneralOperationResult result = (GeneralOperationResult)protoData.result;
                if (GeneralOperationResult.LC_Succeed == result)
                {
                    uint key = protoData.key;
                    string ip = protoData.server_ip;
                    int port = (int)protoData.server_port;
                    int heroId = protoData.hero_id;
                    int campId = protoData.camp_id;
                    int sceneId = protoData.scene_type;
                    LobbyClient.Instance.CurrentRole.MatchKey = protoData.match_key;
                    //延迟处理，防止当前正在切场景过程中
                    WorldSystem.Instance.QueueAction(() =>
                    {
                        Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
                        if (null == cfg || cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE)
                        {
                            NetworkSystem.Instance.HeroId = heroId;
                            NetworkSystem.Instance.CampId = campId;
                            NetworkSystem.Instance.SceneId = sceneId;
                        }
                        else if (cfg.m_Type == (int)SceneTypeEnum.TYPE_PVP || cfg.m_Type == (int)SceneTypeEnum.TYPE_MULTI_PVE)
                        {
                            NetworkSystem.Instance.Start(key, ip, port, heroId, campId, sceneId);
                        }
                        else
                        {
                            LogSystem.Warn("try to enter unexpected scene {0} !", sceneId);
                        }
                        GameControler.ChangeScene(sceneId);
                    });
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_startgame_failure", "game", result);
                }
            }
        }
        private void HandleDiscardItemResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_DiscardItemResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_DiscardItemResult;
            if (null != protoData)
            {
                List<int> items = protoData.m_ItemId;
                List<int> items_property = protoData.m_PropertyId;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.SellItemGoldIncome = protoData.m_TotalIncome;
                    int gold_income = protoData.m_Gold - role_info.Gold;
                    int money_income = protoData.m_Money - role_info.Money;
                    if (null != items && items.Count > 0
                      && null != items_property && items_property.Count > 0
                      && items_property.Count == items.Count)
                    {
                        List<int> del_items = new List<int>();
                        List<int> del_items_property = new List<int>();
                        for (int i = 0; i < items.Count; i++)
                        {
                            ItemDataInfo item = role_info.GetItemData(items[i], items_property[i]);
                            if (null != item && null != item.ItemConfig
                              && item.RandomProperty == items_property[i])
                            {
                                role_info.DelItemData(item.ItemId, item.RandomProperty);
                                del_items.Add(item.ItemId);
                                del_items_property.Add(item.RandomProperty);
                            }
                        }
                        if (del_items.Count > 0 && del_items_property.Count > 0
                          && del_items_property.Count == del_items.Count)
                        {
                            int[] vanish_items = new int[del_items.Count];
                            int[] vanish_items_property = new int[del_items_property.Count];
                            int[] vanish_items_num = new int[del_items.Count];
                            for (int i = 0; i < del_items.Count; i++)
                            {
                                vanish_items[i] = del_items[i];
                                vanish_items_property[i] = del_items_property[i];
                                vanish_items_num[i] = 1;
                            }
                            role_info.Gold = protoData.m_Gold;
                            role_info.Money = protoData.m_Money;
                            GfxSystem.PublishGfxEvent("ge_sell_item_income", "bag", money_income, gold_income);
                            GfxSystem.PublishGfxEvent("ge_delete_item", "bag", vanish_items, vanish_items_property, vanish_items_num, result);
                        }
                    }
                }
                else
                {
                    int[] assit_items = new int[] { 0 };
                    int[] assit_items_property = new int[] { 0 };
                    int[] assit_items_num = new int[] { 0 };
                    GfxSystem.PublishGfxEvent("ge_delete_item", "bag", assit_items, assit_items_property, assit_items_num, result);
                }
            }
        }
        private void HandleMountEquipmentResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself || null == playerself.GetEquipmentStateInfo())
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_MountEquipmentResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MountEquipmentResult;
            if (null != protoData)
            {
                int item_id = protoData.m_ItemID;
                int item_property = protoData.m_PropertyID;
                int equipment_pos = protoData.m_EquipPos;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    ItemDataInfo mountItem = role_info.GetItemData(item_id, item_property);
                    if (null == mountItem)
                        return;
                    if (null == mountItem.ItemConfig)
                        return;
                    if (false == mountItem.ItemConfig.m_CanWear
                      || equipment_pos != mountItem.ItemConfig.m_WearParts)
                        return;
                    if (role_info.Level < mountItem.ItemConfig.m_WearLevel)
                        return;
                    ItemDataInfo exsitItem = playerself.GetEquipmentStateInfo().GetEquipmentData(equipment_pos);
                    int asst_level = mountItem.Level;
                    int asst_randomp_roperty = mountItem.RandomProperty;
                    if (null != exsitItem && exsitItem.Level > 0)
                    {
                        ItemLevelupConfig item_levelup_Info = ItemLevelupConfigProvider.Instance.GetDataById(exsitItem.Level);
                        if (null != item_levelup_Info)
                        {
                            if (item_levelup_Info.m_ChangeEquipCost <= role_info.Money)
                            {
                                role_info.Money -= item_levelup_Info.m_ChangeEquipCost;
                                mountItem.Level = exsitItem.Level;
                                exsitItem.Level = 1;
                                asst_level = mountItem.Level;
                                asst_randomp_roperty = mountItem.RandomProperty;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    role_info.ReduceItemData(mountItem.ItemId, mountItem.RandomProperty);
                    if (null != exsitItem)
                    {
                        ItemDataInfo info = new ItemDataInfo();
                        info.ItemId = exsitItem.ItemId;
                        info.Level = exsitItem.Level;
                        info.ItemNum = exsitItem.ItemNum;
                        info.RandomProperty = exsitItem.RandomProperty;
                        role_info.AddItemData(info);
                        role_info.DeleteEquip(info.ItemId);
                        int[] items = new int[] { exsitItem.ItemId };
                        int[] items_num = new int[] { exsitItem.ItemNum };
                        int[] item_random_propertys = new int[] { exsitItem.RandomProperty };
                        GfxSystem.PublishGfxEvent("ge_add_item", "bag", items, items_num, item_random_propertys);
                    }
                    playerself.GetEquipmentStateInfo().SetEquipmentData(equipment_pos, null);
                    playerself.GetEquipmentStateInfo().SetEquipmentData(equipment_pos, mountItem);
                    UserView view = EntityManager.Instance.GetUserViewById(playerself.GetId());
                    if (view != null)
                    {
                        view.UpdateEquipment(mountItem);
                    }
                    role_info.SetEquip((int)mountItem.ItemConfig.m_WearParts, mountItem);
                    GfxSystem.PublishGfxEvent("ge_delete_item", "bag", new int[] { item_id }, new int[] { item_property }, new int[] { 1 }, GeneralOperationResult.LC_Succeed);
                    GfxSystem.PublishGfxEvent("ge_fiton_equipment", "equipment", item_id, equipment_pos, asst_level, asst_randomp_roperty, result);
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_fiton_equipment", "equipment", item_id, equipment_pos, 1, 0, result);
                }
            }
        }
        private void HandleUnmountEquipmentResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself || null == playerself.GetEquipmentStateInfo())
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UnmountEquipmentResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UnmountEquipmentResult;
            if (null != protoData)
            {
                int equipment_pos = protoData.m_EquipPos;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    ItemDataInfo exsitItem = playerself.GetEquipmentStateInfo().GetEquipmentData(equipment_pos);
                    if (null == exsitItem)
                        return;
                    if (null == exsitItem.ItemConfig)
                        return;
                    playerself.GetEquipmentStateInfo().SetEquipmentData(equipment_pos, null);

                    ItemDataInfo info = new ItemDataInfo();
                    info.ItemId = exsitItem.ItemId;
                    info.Level = exsitItem.Level;
                    info.ItemNum = exsitItem.ItemNum;
                    info.RandomProperty = exsitItem.RandomProperty;
                    if (null != role_info.Items && role_info.Items.Count < RoleInfo.c_MaxItemNum)
                    {
                        role_info.AddItemData(info);
                    }
                    role_info.DeleteEquip(exsitItem.ItemId);
                }
                GfxSystem.PublishGfxEvent("ge_fitoff_equipment", "equipment", equipment_pos, result);
            }
        }
        private void HandleMountSkillResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_MountSkillResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MountSkillResult;
            if (null != protoData)
            {
                if (null != LobbyClient.Instance.CurrentRole && null != LobbyClient.Instance.CurrentRole.SkillInfos)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    int preset_index = protoData.m_PresetIndex;
                    int skill_id = protoData.m_SkillID;
                    SlotPosition slot_position = (SlotPosition)protoData.m_SlotPos;
                    GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                    if (GeneralOperationResult.LC_Succeed == result)
                    {
                        if (preset_index >= 0 && preset_index < PresetInfo.PresetNum)
                        {
                            bool ret = false;
                            for (int i = 0; i < role_info.SkillInfos.Count; i++)
                            {
                                if (slot_position == role_info.SkillInfos[i].Postions.Presets[preset_index])
                                {
                                    role_info.SkillInfos[i].Postions.Presets[preset_index] = SlotPosition.SP_None;
                                    break;
                                }
                            }
                            for (int i = 0; i < role_info.SkillInfos.Count; i++)
                            {
                                if (skill_id == role_info.SkillInfos[i].SkillId)
                                {
                                    role_info.SkillInfos[i].Postions.Presets[preset_index] = slot_position;
                                    ret = true;
                                    break;
                                }
                            }
                            if (ret)
                            {
                                GfxSystem.PublishGfxEvent("ge_mount_skill", "skill", preset_index, skill_id, (int)slot_position, result);
                            }
                        }
                    }
                    else
                    {
                        GfxSystem.PublishGfxEvent("ge_mount_skill", "skill", preset_index, skill_id, (int)slot_position, result);
                    }
                }
            }
        }
        private void HandleUnmountSkillResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UnmountSkillResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UnmountSkillResult;
            if (null != protoData)
            {
                if (null != LobbyClient.Instance.CurrentRole && null != LobbyClient.Instance.CurrentRole.SkillInfos)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    int preset_index = protoData.m_PresetIndex;
                    SlotPosition slot_position = (SlotPosition)protoData.m_SlotPos;
                    GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                    if (GeneralOperationResult.LC_Succeed == result)
                    {
                        if (preset_index >= 0 && preset_index < PresetInfo.PresetNum)
                        {
                            bool ret = false;
                            for (int i = 0; i < role_info.SkillInfos.Count; i++)
                            {
                                if (slot_position == role_info.SkillInfos[i].Postions.Presets[preset_index])
                                {
                                    role_info.SkillInfos[i].Postions.Presets[preset_index] = SlotPosition.SP_None;
                                    ret = true;
                                    break;
                                }
                            }
                            if (ret)
                            {
                                GfxSystem.PublishGfxEvent("ge_unmount_skill", "skill", preset_index, (int)slot_position, result);
                            }
                        }
                    }
                    else
                    {
                        GfxSystem.PublishGfxEvent("ge_unmount_skill", "skill", preset_index, (int)slot_position, result);
                    }
                }
            }
        }
        private void HandleUpgradeSkillResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            if (null == LobbyClient.Instance.CurrentRole)
                return;
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UpgradeSkillResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UpgradeSkillResult;
            if (null != protoData)
            {
                int preset_index = protoData.m_PresetIndex;
                int skill_id = protoData.m_SkillID;
                bool allow_cost_gold = protoData.m_AllowCostGold;
                int money = protoData.m_Money;
                int gold = protoData.m_Gold;
                int vigor = protoData.m_Vigor;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result)
                {
                    int skill_level = 1;
                    for (int i = 0; i < role_info.SkillInfos.Count; i++)
                    {
                        if (skill_id == role_info.SkillInfos[i].SkillId)
                        {
                            role_info.SkillInfos[i].SkillLevel += 1;
                            skill_level = role_info.SkillInfos[i].SkillLevel;
                            result = GeneralOperationResult.LC_Succeed;
                            break;
                        }
                    }
                    GfxSystem.PublishGfxEvent("ge_upgrade_skill", "skill", preset_index, skill_id, skill_level, result);
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_upgrade_skill", "skill", preset_index, 0, 0, result);
                }
                role_info.Money = money;
                role_info.Gold = gold;
                role_info.Vigor = vigor;
            }
        }
        private void HandleUnlockSkillResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UnlockSkillResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UnlockSkillResult;
            if (null != protoData)
            {
                if (null != LobbyClient.Instance.CurrentRole && null != LobbyClient.Instance.CurrentRole.SkillInfos)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    int preset_index = protoData.m_PresetIndex;
                    int skill_id = protoData.m_SkillID;
                    int user_level = protoData.m_UserLevel;
                    GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                    if (GeneralOperationResult.LC_Succeed == result)
                    {
                        bool ret = false;
                        for (int i = 0; i < role_info.SkillInfos.Count; i++)
                        {
                            if (skill_id == role_info.SkillInfos[i].SkillId)
                            {
                                role_info.SkillInfos[i].SkillLevel = 1;
                                ret = true;
                                break;
                            }
                        }
                        if (ret)
                        {
                            GfxSystem.PublishGfxEvent("ge_unlock_skill", "skill", preset_index, skill_id, user_level, result);
                        }
                    }
                    else
                    {
                        GfxSystem.PublishGfxEvent("ge_unlock_skill", "skill", preset_index, skill_id, user_level, result);
                    }
                }
            }
        }
        private void HandleSwapSkillResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SwapSkillResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SwapSkillResult;
            if (null != protoData)
            {
                if (null != LobbyClient.Instance.CurrentRole && null != LobbyClient.Instance.CurrentRole.SkillInfos)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    int preset_index = protoData.m_PresetIndex;
                    int skill_id = protoData.m_SkillID;
                    SlotPosition source_pos = (SlotPosition)protoData.m_SourcePos;
                    SlotPosition target_pos = (SlotPosition)protoData.m_TargetPos;
                    GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                    if (GeneralOperationResult.LC_Succeed == result)
                    {
                        if (preset_index >= 0 && preset_index < PresetInfo.PresetNum)
                        {
                            bool ret = false;
                            for (int i = 0; i < role_info.SkillInfos.Count; i++)
                            {
                                if (target_pos == role_info.SkillInfos[i].Postions.Presets[preset_index])
                                {
                                    role_info.SkillInfos[i].Postions.Presets[preset_index] = source_pos;
                                    break;
                                }
                            }
                            for (int i = 0; i < role_info.SkillInfos.Count; i++)
                            {
                                if (skill_id == role_info.SkillInfos[i].SkillId)
                                {
                                    role_info.SkillInfos[i].Postions.Presets[preset_index] = target_pos;
                                    ret = true;
                                    break;
                                }
                            }
                            if (ret)
                            {
                                GfxSystem.PublishGfxEvent("ge_swap_skill", "skill", preset_index, skill_id, source_pos, target_pos, result);
                            }
                        }
                    }
                    else
                    {
                        GfxSystem.PublishGfxEvent("ge_swap_skill", "skill", preset_index, skill_id, source_pos, target_pos, result);
                    }
                }
            }
        }
        private void HandleUpgradeItemResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UpgradeItemResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UpgradeItemResult;
            if (null != protoData)
            {
                if (null != LobbyClient.Instance.CurrentRole && null != LobbyClient.Instance.CurrentRole.Equips)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    int equip_position = protoData.m_Position;
                    int money = protoData.m_Money;
                    int gold = protoData.m_Gold;
                    ItemDataInfo op_item = playerself.GetEquipmentStateInfo().GetEquipmentData(equip_position);
                    if (null != op_item)
                    {
                        GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                        if (GeneralOperationResult.LC_Succeed == result)
                        {
                            op_item.Level += 1;
                            playerself.GetEquipmentStateInfo().SetEquipmentData(equip_position, null);
                            playerself.GetEquipmentStateInfo().SetEquipmentData(equip_position, op_item);
                            role_info.SetEquip(op_item.ItemConfig.m_WearParts, op_item);
                            GfxSystem.PublishGfxEvent("ge_upgrade_item", "equipment", equip_position, op_item.ItemId, op_item.Level, op_item.RandomProperty, result);
                        }
                        else
                        {
                            GfxSystem.PublishGfxEvent("ge_upgrade_item", "equipment", equip_position, op_item.ItemId, 0, 0, result);
                        }
                        role_info.Money = money;
                        role_info.Gold = gold;
                    }
                }
            }
        }
        private void HandleUserLevelup(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UserLevelup protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UserLevelup;
            if (null != protoData)
            {
                int user_id = protoData.m_UserId;
                int user_level = protoData.m_UserLevel;
                if (user_level > 0)
                {
                    playerself.SetLevel(user_level);
                    if (null != LobbyClient.Instance.CurrentRole)
                    {
                        LobbyClient.Instance.CurrentRole.Level = user_level;
                        LobbyClient.Instance.CurrentRole.LevelUp = true;
                        ReloadMissions();
                    }
                    //通知UI，玩家升级
                    GfxSystem.PublishGfxEvent("ge_user_levelup", "property", user_level);
                    //通知GTSDK，玩家升级
                    GfxSystem.PublishGfxEvent("ge_modifyUsrInfoWithModifyType", "gt",
                      1, user_level.ToString());
                }
            }
        }
        private void HandleSyncStamina(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncStamina protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncStamina;
            if (null != protoData)
            {
                int stamina = protoData.m_Stamina;
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.CurStamina = stamina;
                    GfxSystem.PublishGfxEvent("ge_restamina_time", "ui");
                }
            }
        }
        private void HandleSyncMpveBattleResult(JsonMessage lobbyMsg)
        {
            //m_Result: 0--win 1--lost 2--unfinish
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncMpveBattleResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncMpveBattleResult;
            if (null != protoData)
            {
                if (protoData.m_Result == 0)
                {//多人pve只处理成功情形，奖励在StagClearResult里处理。失败处理不走大厅流程。
                    UserInfo player = WorldSystem.Instance.GetPlayerSelf();
                    if (null != player)
                    {
                        CombatStatisticInfo combatInfo = player.GetCombatStatisticInfo();
                        JsonMessage msg = new JsonMessage(JsonMessageID.StageClear);
                        msg.m_JsonData.Set("m_Guid", m_Guid);
                        ArkCrossEngineMessage.Msg_CL_StageClear assit_data = new ArkCrossEngineMessage.Msg_CL_StageClear();
                        assit_data.m_HitCount = combatInfo.HitCount;
                        assit_data.m_KillNpcCount = protoData.m_KillNpcCount;
                        assit_data.m_MaxMultHitCount = combatInfo.MaxMultiHitCount;
                        assit_data.m_Hp = player.Hp;
                        assit_data.m_Mp = player.Energy;
                        msg.m_ProtoData = assit_data;
                        SendMessage(msg);
                        LogSystem.Debug("SendMessage StageClear to lobby");
                    }
                }
            }
        }
        private void HandleSyncGowBattleResult(JsonMessage lobbyMsg)
        {
            //m_Result: 0--win 1--lost 2--unfinish
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncGowBattleResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncGowBattleResult;
            if (null != protoData)
            {
                int result = protoData.m_Result;
                int oldelo = protoData.m_OldGowElo;
                int elo = protoData.m_GowElo;
                int maxhitcount = protoData.m_MaxMultiHitCount;
                int damage = protoData.m_TotalDamage;
                int enemyheroid = protoData.m_EnemyHeroId;
                int enemyoldelo = protoData.m_EnemyOldGowElo;
                int enemyelo = protoData.m_EnemyGowElo;
                int enemyhitcount = protoData.m_EnemyMaxMultiHitCount;
                int enemydamage = protoData.m_EnemyTotalDamage;
                string enemynick = protoData.m_EnemyNick;

                GfxSystem.PublishGfxEvent("ge_pvp_result", "ui", result, enemyheroid, oldelo, elo, enemyoldelo, enemyelo, damage, enemydamage, maxhitcount, enemyhitcount, enemynick);
            }
        }
        private void HandleStageClearResult(JsonMessage lobbyMsg)
        {
            LogSystem.Debug("Receive StageClear message from lobby");
            if (null == lobbyMsg) return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_StageClearResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_StageClearResult;
            if (null != protoData && protoData.m_ResultCode == (int)GeneralOperationResult.LC_Succeed)
            {
                ulong userGuid = jsonData.GetUlong("m_Guid");
                RoleInfo player = LobbyClient.Instance.AccountInfo.FindRole(userGuid);
                int hitCount = protoData.m_HitCount;
                int maxMultHitCount = protoData.m_MaxMultHitCount;
                long duration = protoData.m_Duration;
                int itemId = protoData.m_ItemId;
                int itemCount = protoData.m_ItemCount;
                int expPoint = protoData.m_ExpPoint;
                int hp = protoData.m_Hp;
                int mp = protoData.m_Mp;
                int gold = protoData.m_Gold;
                int deadCount = protoData.m_DeadCount;
                int completedRewardId = protoData.m_CompletedRewardId;
                int curSceneStar = protoData.m_SceneStarNum;
                int sceneId = WorldSystem.Instance.GetCurSceneId();
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
                /// mpve
                if (null != cfg && cfg.m_Type == (int)SceneTypeEnum.TYPE_MULTI_PVE)
                {
                    if (cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_ATTEMPT)
                    {
                        if (protoData.m_KillNpcCount > player.AttemptAward)
                        {
                            player.AttemptAward = protoData.m_KillNpcCount;
                        }
                        GfxSystem.PublishGfxEvent("ge_mpve_tollgate_succeed", "ui", maxMultHitCount, hitCount, deadCount, (int)(duration / 1000));
                    }
                    else if (cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_GOLD)
                    {
                        player.GoldCurAcceptedCount += 1;
                        player.Money += gold;
                        List<Teammate> teammate = new List<Teammate>();
                        foreach (ArkCrossEngineMessage.Msg_LC_StageClearResult.Teammate e in protoData.m_Teammate)
                        {
                            Teammate v = new Teammate();
                            v.Nick = e.m_Nick;
                            v.ResId = e.m_ResId;
                            v.Money = e.m_Money;
                            teammate.Add(v);
                        }
                        GfxSystem.PublishGfxEvent("ge_mpve_gold_tollgate_succeed", "ui", maxMultHitCount, hitCount, deadCount, (int)(duration / 1000), teammate);
                    }
                }
                else
                {
                    if (null != cfg && cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_EXPEDITION)
                    {
                        ExpeditionPlayerInfo expedition = player.GetExpeditionInfo();
                        if (null != expedition)
                        {
                            expedition.Hp = hp;
                            expedition.Mp = mp;
                        }
                    }
                    int sceneStars = player.GetSceneInfo(sceneId);
                    // 第一次通关副本不显示结算
                    if (player.SceneInfo.Count == 0)
                    {
                        ReturnMainCity();
                    }
                    else
                    {
                        if (WorldSystem.Instance.IsPveScene() && !WorldSystem.Instance.IsStageClearing)
                        {
                            // 胜利页面
                            GfxSystem.PublishGfxEvent("ge_victory_panel", "ui", sceneId, maxMultHitCount, hitCount, deadCount, (int)(duration / 1000), expPoint, gold, (curSceneStar > sceneStars));
                            // 翻牌页面
                            GfxSystem.PublishGfxEvent("ge_turnover_card", "ui", itemId, itemCount);
                            WorldSystem.Instance.IsStageClearing = true;
                        }
                    }
                    // 记录通关信息
                    player.SetSceneInfo(sceneId, curSceneStar);
                    player.AddCompletedSceneCount(sceneId);
                    // 客户端结算
                    player.Exp += expPoint;
                    player.Money += gold;
                    // 首次通关结算
                    if (completedRewardId > 0)
                    {
                        Data_SceneDropOut dropOutConfig = SceneConfigProvider.Instance.GetSceneDropOutById(completedRewardId);
                        if (null != dropOutConfig)
                        {
                            player.Exp += dropOutConfig.m_Exp;
                            player.Money += dropOutConfig.m_GoldSum;
                            player.Gold += dropOutConfig.m_Diamond;
                        }
                    }
                }
                if (null != player)
                {
                    MissionStateInfo mission_info = player.GetMissionStateInfo();
                    if (null != protoData.m_Missions && protoData.m_Missions.Count > 0 && null != mission_info)
                    {
                        int ct = protoData.m_Missions.Count;
                        for (int i = 0; i < ct; ++i)
                        {
                            ArkCrossEngineMessage.Msg_LC_StageClearResult.MissionInfoForSync assit_info = protoData.m_Missions[i];
                            if (assit_info.m_IsCompleted)
                            {
                                mission_info.CompletedMission(assit_info.m_MissionId);
                                //mission_info.CompletedMissions[assit_info.m_MissionId].Progress = assit_info.m_Progress;
                            }
                            else
                            {
                                if (protoData.m_Missions[i].m_MissionId >= 0)
                                {
                                    mission_info.AddMission(assit_info.m_MissionId, MissionStateType.UNCOMPLETED);
                                    //mission_info.UnCompletedMissions[assit_info.m_MissionId].Progress = assit_info.m_Progress;
                                }
                            }
                        }
                    }
                }
                ///
                GfxSystem.PublishGfxEvent("ge_stop_backgroud_music", "music");
            }
            else
            {
                WorldSystem.Instance.ReturnToLogin();
                WorldSystem.Instance.ChangeScene(0);
            }
        }
        private void HandleSweepStageResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SweepStageResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SweepStageResult;
            RoleInfo roleInfo = LobbyClient.Instance.CurrentRole;
            if (null != protoData && null != roleInfo)
            {
                if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Succeed)
                {
                    // succeed
                    roleInfo.AddCompletedSceneCount(protoData.m_SceneId, protoData.m_SweepItemCost);
                    roleInfo.ReduceItemData(ItemConfigProvider.Instance.GetSweepStageItemId(), 0, protoData.m_SweepItemCost);
                    int gold = protoData.m_Gold;
                    int exp = protoData.m_Exp;
                    int count = protoData.m_SweepItemCost;
                    roleInfo.Money += gold;
                    roleInfo.Exp += exp;
                    // 扫荡卷消耗, 体力不在这里同步
                    // 物品 UI自己补全数据
                    List<int> idList = new List<int>();
                    List<int> numList = new List<int>();

                    LogSystem.Debug("HandleSweepStageResult sceneId = {0}, gold = {1}, exp = {2}", protoData.m_SceneId, gold, exp);
                    if (null != protoData.m_ItemInfo && protoData.m_ItemInfo.Count > 0)
                    {
                        for (int i = 0; i < protoData.m_ItemInfo.Count; ++i)
                        {
                            int itemId = protoData.m_ItemInfo[i].ItemId;
                            int itemCount = protoData.m_ItemInfo[i].Num;
                            int itemAppendAttrId = protoData.m_ItemInfo[i].AppendProperty;
                            LogSystem.Debug("HandleSweepStageResult ItemId = {0}", itemId);

                            idList.Add(itemId);
                            numList.Add(itemCount);
                        }
                    }
                    GfxSystem.PublishGfxEvent("ge_sync_wipeout_backInfo", "wipeout", gold / count, exp / count, idList, numList);
                }
            }
        }
        private void HandleAddAssetsResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_AddAssetsResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_AddAssetsResult;
            if (null != protoData)
            {
                int money = protoData.m_Money;
                int gold = protoData.m_Gold;
                int exp = protoData.m_Exp;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.Money += money;
                    role_info.Gold += gold;
                    role_info.Exp += exp;
                }
            }
        }
        private void HandleAddItemResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_AddItemResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_AddItemResult;
            if (null != protoData)
            {
                int item_id = protoData.m_ItemId;
                int item_random_property = protoData.m_RandomProperty;
                int item_count = protoData.m_ItemCount;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    if (null != ItemConfigProvider.Instance.GetDataById(item_id))
                    {
                        if (ItemConfigProvider.Instance.GetGoldId() == item_id)
                        {
                            role_info.Money += item_count;
                        }
                        else if (ItemConfigProvider.Instance.GetDiamondId() == item_id)
                        {
                            role_info.Gold += item_count;
                        }
                        else if (ItemConfigProvider.Instance.GetMonthCardId() == item_id)
                        {
                            int day = MonthCardConfigProvider.Instacne.GetDuration() * item_count;
                            int diamond = MonthCardConfigProvider.Instacne.GetRewardDiamond() * item_count;
                            role_info.Gold += diamond;
                        }
                        else
                        {
                            DoAddItemData(role_info, item_id, item_count, item_random_property);
                            if (role_info.CurrencyId == item_id && item_id != ItemConfigProvider.Instance.GetGoldId() && item_id != ItemConfigProvider.Instance.GetDiamondId())
                            {
                                ItemDataInfo idi = role_info.GetItemData(item_id, 0);
                                if (idi != null)
                                {
                                    role_info.ExchangeCurrency = idi.ItemNum;
                                }
                                else
                                {
                                    role_info.ExchangeCurrency = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void DoAddItemData(RoleInfo role_info, int item_id, int item_count, int item_random_property)
        {
            ItemDataInfo new_info = new ItemDataInfo();
            new_info.ItemId = item_id;
            new_info.Level = 1;
            new_info.ItemNum = item_count;
            new_info.RandomProperty = item_random_property;
            new_info.ItemConfig = ItemConfigProvider.Instance.GetDataById(item_id);
            role_info.AddItemData(new_info, item_count);
            ///
            if (new_info.ItemConfig.m_CanWear)
            {
                role_info.AddToNewEquipCache(item_id, item_random_property);
                if (LobbyClient.Instance.CurrentRole.CitySceneId == WorldSystem.Instance.GetCurSceneId())
                {
                    List<NewEquipInfo> assit_new_info = new List<NewEquipInfo>();
                    assit_new_info.AddRange(role_info.NewEquipCache);
                    GfxSystem.PublishGfxEvent("ge_new_equip", "equipment", assit_new_info);
                    role_info.ResetNewEquipCache();
                }
            }
            ///
            if (WorldSystem.Instance.IsPureClientScene())
            {
                int[] items = new int[] { item_id };
                int[] items_num = new int[] { item_count };
                int[] random_propertys = new int[] { item_random_property };
                GfxSystem.PublishGfxEvent("ge_add_item", "bag", items, items_num, random_propertys);
            }
        }
        private void HandleLiftSkillResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;

            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_LiftSkillResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_LiftSkillResult;
            if (null != protoData)
            {
                int skill_id = protoData.m_SkillID;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    SkillLogicData skill_data = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skill_id) as SkillLogicData;
                    if (null != skill_data && skill_data.LiftSkillId > 0)
                    {
                        bool ret = false;
                        bool exist = true;
                        for (int i = 0; i < skill_data.LiftCostItemList.Count; i++)
                        {
                            ItemDataInfo item_info = role_info.GetItemData(skill_data.LiftCostItemList[i], 0);
                            if (null == item_info)
                            {
                                exist = false;
                                break;
                            }
                            else
                            {
                                if (item_info.ItemNum < skill_data.LiftCostItemNumList[i])
                                {
                                    exist = false;
                                    break;
                                }
                            }
                        }
                        if (exist)
                        {
                            for (int index = 0; index < skill_data.LiftCostItemList.Count; index++)
                            {
                                role_info.ReduceItemData(skill_data.LiftCostItemList[index], 0, skill_data.LiftCostItemNumList[index]);
                                ///
                                int[] vanish_items = new int[skill_data.LiftCostItemList.Count];
                                int[] vanish_items_property = new int[skill_data.LiftCostItemList.Count];
                                int[] vanish_items_num = new int[skill_data.LiftCostItemList.Count];
                                for (int i = 0; i < vanish_items.Length; i++)
                                {
                                    vanish_items[i] = skill_data.LiftCostItemList[index];
                                    vanish_items_property[i] = 0;
                                    vanish_items_num[i] = skill_data.LiftCostItemNumList[index];
                                }
                                GfxSystem.PublishGfxEvent("ge_delete_item", "bag", vanish_items, vanish_items_property, vanish_items_num, result);
                            }
                            ret = true;
                        }
                        if (ret)
                        {
                            for (int i = 0; i < role_info.SkillInfos.Count; i++)
                            {
                                if (skill_id == role_info.SkillInfos[i].SkillId)
                                {
                                    role_info.SkillInfos[i].SkillId = skill_data.LiftSkillId;
                                    result = GeneralOperationResult.LC_Succeed;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_CostError;
                        }
                        GfxSystem.PublishGfxEvent("ge_lift_skill", "skill", skill_id, skill_data.LiftSkillId, result);
                    }
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_lift_skill", "skill", 0, 0, result);
                }
            }
        }
        private void HandleBuyStaminaResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_BuyStaminaResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_BuyStaminaResult;
            if (null != protoData)
            {
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    BuyStaminaConfig data = BuyStaminaConfigProvider.Instance.GetDataById(role_info.BuyStaminaCount + 1);
                    if (null != data)
                    {
                        role_info.Gold -= data.m_CostGold;
                        role_info.BuyStaminaCount += 1;
                    }
                }
                GfxSystem.PublishGfxEvent("ge_buy_stamina", "stamina", result);
            }
        }
        private void HandleFinishMissionResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_FinishMissionResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_FinishMissionResult;
            if (null != protoData)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    role_info.Money += protoData.m_Gold;
                    role_info.Exp += protoData.m_Exp;
                    role_info.Gold += protoData.m_Diamond;
                    int finish_mission_id = protoData.m_FinishMissionId;
                    role_info.GetMissionStateInfo().CompletedMissions.Remove(finish_mission_id);
                    MissionConfig mc = MissionConfigProvider.Instance.GetDataById(finish_mission_id);
                    if (null != mc.TriggerGuides && mc.TriggerGuides.Count > 0)
                    {
                        foreach (int triggerId in mc.TriggerGuides)
                        {
                            if (!role_info.NewbieGuides.Contains(triggerId))
                            {
                                role_info.NewbieGuides.Add(triggerId);
                            }
                            SystemGuideConfig newbieGuide = SystemGuideConfigProvider.Instance.GetDataById(role_info.NewbieGuides[0]);
                            if (null != newbieGuide)
                            {
                                GfxSystem.PublishGfxEvent("ge_show_newbieguide", "ui", newbieGuide.Operations);
                            }
                        }
                    }
                    GfxSystem.PublishGfxEvent("ge_about_task", "task", finish_mission_id, MissionOperationType.DELETE, "");
                    LogSystem.Debug("Finish mission {0}", finish_mission_id);
                    MissionStateInfo mission_info = role_info.GetMissionStateInfo();
                    if (null != protoData.m_UnlockMissions && protoData.m_UnlockMissions.Count > 0)
                    {
                        for (int i = 0; i < protoData.m_UnlockMissions.Count; ++i)
                        {
                            int unlockMissionId = protoData.m_UnlockMissions[i].m_MissionId;
                            string unlockMissionProgress = protoData.m_UnlockMissions[i].m_Progress;
                            bool isCompleted = protoData.m_UnlockMissions[i].m_IsCompleted;
                            if (unlockMissionId > 0)
                            {
                                if (isCompleted)
                                {
                                    mission_info.AddMission(unlockMissionId, MissionStateType.COMPLETED);
                                    GfxSystem.PublishGfxEvent("ge_about_task", "task", unlockMissionId, MissionOperationType.FINISH, unlockMissionProgress);
                                    role_info.GetMissionStateInfo().CompletedMissions[unlockMissionId].Progress = unlockMissionProgress;
                                }
                                else
                                {
                                    mission_info.AddMission(unlockMissionId, MissionStateType.UNCOMPLETED);
                                    GfxSystem.PublishGfxEvent("ge_about_task", "task", unlockMissionId, MissionOperationType.ADD, unlockMissionProgress);
                                    role_info.GetMissionStateInfo().UnCompletedMissions[unlockMissionId].Progress = unlockMissionProgress;
                                }
                                LogSystem.Debug("Unlock mission {0}", unlockMissionId);
                            }
                        }
                    }
                }
            }
        }
        private void HandleBuyLifeResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_BuyLifeResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_BuyLifeResult;
            if (null != protoData)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    if (null != jsonData && protoData.m_Succeed)
                    {
                        int curDiamond = protoData.m_CurDiamond;
                        role_info.Gold = curDiamond;
                        ItemDataInfo itemInfo = role_info.GetItemData(ItemConfigProvider.Instance.GetReliveStoneId(), 0);
                        if (null != itemInfo && itemInfo.ItemNum >= 1)
                        {
                            role_info.ReduceItemData(itemInfo.ItemId, 0);
                        }
                        if (WorldSystem.Instance.IsPveScene())
                        {
                            WorldSystem.Instance.RelivePlayer();
                        }
                    }
                    else
                    {
                        if (WorldSystem.Instance.IsPveScene())
                        {
                            ClientStorySystem.Instance.SendMessage("missionfailed");
                        }
                        else
                        {
                            WorldSystem.Instance.QuitBattle();
                        }
                        ReturnMainCity();
                    }
                }
            }
        }
        private void HandleUnlockLegacyResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UnlockLegacyResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UnlockLegacyResult;
            if (null != protoData)
            {
                int index = protoData.m_Index;
                int item_id = protoData.m_ItemID;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    if (null != role_info.Legacys && index < role_info.Legacys.Length)
                    {
                        if (null != role_info.Legacys[index] && item_id == role_info.Legacys[index].ItemId
                          && !role_info.Legacys[index].IsUnlock)
                        {
                            /// todo : check task
                            role_info.Legacys[index].IsUnlock = true;
                            playerself.GetLegacyStateInfo().ResetLegacyData(index);
                            int legacy_id = role_info.Legacys[index].ItemId;
                            if (legacy_id > 0)
                            {
                                ItemDataInfo legacy_info = new ItemDataInfo();
                                legacy_info.Level = role_info.Legacys[index].Level;
                                legacy_info.ItemNum = role_info.Legacys[index].ItemNum;
                                legacy_info.RandomProperty = role_info.Legacys[index].RandomProperty;
                                legacy_info.ItemConfig = ItemConfigProvider.Instance.GetDataById(legacy_id);
                                if (null != legacy_info.ItemConfig)
                                {
                                    playerself.GetLegacyStateInfo().SetLegacyData(index, legacy_info);
                                }
                            }
                            GfxSystem.PublishGfxEvent("ge_unlock_legacy", "legacy", index, item_id, result);
                            /// 
                            playerself.GetLegacyStateInfo().UpdateLegacyComplexAttr();
                        }
                    }
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_unlock_legacy", "legacy", index, item_id, result);
                }
            }
        }

        private void HandleUpgradeLegacyResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UpgradeLegacyResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UpgradeLegacyResult;
            if (null != protoData)
            {
                int index = protoData.m_Index;
                int item_id = protoData.m_ItemID;
                bool allow_cost_gold = protoData.m_AllowCostGold;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    ItemDataInfo op_legacy = role_info.GetLegacyData(index);
                    if (null != op_legacy && item_id == op_legacy.ItemId && op_legacy.IsUnlock)
                    {
                        int legacy_max_level = LegacyLevelupConfigProvider.Instance.GetDataCount();
                        int legacy_upgrade_level = op_legacy.Level + 1;
                        if (legacy_upgrade_level <= legacy_max_level && legacy_upgrade_level <= role_info.Level)
                        {
                            LegacyLevelupConfig legacy_levelup_Info = LegacyLevelupConfigProvider.Instance.GetDataById(op_legacy.Level);
                            if (null != legacy_levelup_Info)
                            {
                                bool ret = false;
                                ItemDataInfo have_item_info = role_info.GetItemData(legacy_levelup_Info.m_CostItemList[index], 0);
                                if (null != have_item_info)
                                {
                                    if (legacy_levelup_Info.m_CostNum <= have_item_info.ItemNum)
                                    {
                                        role_info.ReduceItemData(have_item_info.ItemId, have_item_info.RandomProperty, legacy_levelup_Info.m_CostNum);
                                        ret = true;
                                    }
                                    else
                                    {
                                        if (allow_cost_gold)
                                        {
                                            int replenish_num = legacy_levelup_Info.m_CostNum - have_item_info.ItemNum;
                                            int cost_gold = (int)(replenish_num * legacy_levelup_Info.m_Rate);
                                            if (cost_gold <= role_info.Gold)
                                            {
                                                role_info.DelItemData(have_item_info.ItemId, have_item_info.RandomProperty);
                                                role_info.Gold -= cost_gold;
                                                ret = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (allow_cost_gold)
                                    {
                                        int replenish_money = legacy_levelup_Info.m_CostNum;
                                        int cost_gold = (int)(replenish_money * legacy_levelup_Info.m_Rate);
                                        if (cost_gold <= role_info.Gold && 0 != cost_gold)
                                        {
                                            role_info.Gold -= cost_gold;
                                            ret = true;
                                        }
                                    }
                                }
                                if (ret)
                                {
                                    op_legacy.Level = legacy_upgrade_level;
                                    op_legacy.ItemConfig = ItemConfigProvider.Instance.GetDataById(op_legacy.ItemId);
                                    role_info.SetLegacy(index, op_legacy);
                                    playerself.GetLegacyStateInfo().ResetLegacyData(index);
                                    playerself.GetLegacyStateInfo().SetLegacyData(index, op_legacy);
                                    GfxSystem.PublishGfxEvent("ge_upgrade_legacy", "legacy", index, item_id, result);
                                }
                            }
                        }
                    }
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_upgrade_legacy", "legacy", index, item_id, result);
                }
            }
        }
        private void HandleAddXSoulExperienceResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ArkCrossEngineMessage.Msg_LC_AddXSoulExperienceResult result = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_AddXSoulExperienceResult;
            if (result != null)
            {
                GeneralOperationResult ret = (GeneralOperationResult)result.m_Result;
                if (ret == GeneralOperationResult.LC_Succeed)
                {
                    XSoulPartInfo part = playerself.GetXSoulInfo().GetXSoulPartData((XSoulPart)result.m_XSoulPart);
                    if (part != null)
                    {
                        part.UpdateXSoulExperience(result.m_Experience);
                        if (part.IsLevelChanged)
                        {
                            UserAttrCalculator.Calc(playerself);
                        }
                        UserView userview = EntityManager.Instance.GetUserViewById(playerself.GetId());
                        if (userview != null)
                        {
                            userview.UpdateXSoulEquip();
                        }
                        RoleInfo cur_role = LobbyClient.Instance.CurrentRole;
                        foreach (ItemDataInfo item in cur_role.Items)
                        {
                            if (item.ItemId == result.m_UseItemId)
                            {
                                item.ItemNum -= result.m_ItemNum;
                                if (item.ItemNum < 0)
                                {
                                    item.ItemNum = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        LogSystem.Debug("----not find xsoul part: " + result.m_XSoulPart);
                    }
                }
                GfxSystem.PublishGfxEvent("ge_addsoul_experience_result", "XSoul", result.m_XSoulPart,
                                          result.m_UseItemId, result.m_ItemNum, result.m_Experience, result.m_Result);
            }
        }

        private void HandleXSoulChangeShowModelResult(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
            {
                LogSystem.Debug("----change xsoul model: player is null!");
                return;
            }
            ArkCrossEngineMessage.Msg_LC_XSoulChangeShowModelResult result = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_XSoulChangeShowModelResult;
            if (result != null)
            {
                GeneralOperationResult ret = (GeneralOperationResult)result.m_Result;
                if (ret == GeneralOperationResult.LC_Succeed)
                {
                    XSoulPartInfo part = playerself.GetXSoulInfo().GetXSoulPartData((XSoulPart)result.m_XSoulPart);
                    part.ShowModelLevel = result.m_ModelLevel;
                    if (part != null)
                    {
                        UserView userview = EntityManager.Instance.GetUserViewById(playerself.GetId());
                        if (userview != null)
                        {
                            userview.UpdateXSoulEquip();
                        }
                    }
                }
                GfxSystem.PublishGfxEvent("ge_xsoul_changemodel_result", "XSoul", result.m_XSoulPart, result.m_ModelLevel, result.m_Result);
            }
        }
        private void HandleNotifyNewMail(JsonMessage lobbyMsg)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            if (WorldSystem.Instance.IsPureClientScene())
            {
                GfxSystem.PublishGfxEvent("ge_notify_new_mail", "mail");
            }
        }
        private void HandleSyncMailList(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            JsonData mails = jsonData["m_Mails"];
            if (null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info.MailInfos)
                {
                    if (true == mails.IsArray && mails.Count > 0)
                    {
                        role_info.MailInfos.Clear();
                        int msg_mail_ct = mails.Count;
                        for (int i = 0; i < msg_mail_ct; i++)
                        {
                            if (null != mails[i])
                            {
                                MailInfo info = new MailInfo();
                                info.m_AlreadyRead = mails[i].GetBoolean("m_AlreadyRead");
                                info.m_MailGuid = mails[i].GetUlong("m_MailGuid");
                                info.m_Title = mails[i].GetString("m_Title");
                                info.m_Title = Dict.Parse(info.m_Title);
                                info.m_Module = (ModuleMailTypeEnum)mails[i].GetInt("m_Module");
                                info.m_SendTime = DateTime.Parse(mails[i].GetString("m_SendTime"));
                                info.m_Text = mails[i].GetString("m_Text");
                                info.m_Text = Dict.Parse(info.m_Text);
                                info.m_Money = mails[i].GetInt("m_Money");
                                info.m_Gold = mails[i].GetInt("m_Gold");
                                info.m_Stamina = mails[i].GetInt("m_Stamina");
                                info.m_Sender = mails[i].GetString("m_Sender");
                                info.m_Sender = Dict.Parse(info.m_Sender);
                                JsonData mail_items = mails[i]["m_Items"];
                                if (null != mail_items && true == mail_items.IsArray && mail_items.Count > 0)
                                {
                                    info.m_Items = new List<MailItem>();
                                    info.m_Items.Clear();
                                    for (int index = 0; index < mail_items.Count; index++)
                                    {
                                        if (null != mail_items[index])
                                        {
                                            MailItem assit_item = new MailItem();
                                            assit_item.m_ItemId = mail_items[index].GetInt("m_ItemId");
                                            assit_item.m_ItemNum = mail_items[index].GetInt("m_ItemNum");
                                            info.m_Items.Add(assit_item);
                                        }
                                    }
                                }
                                role_info.MailInfos.Add(info);
                            }
                        }
                    }
                    int role_mail_ct = role_info.MailInfos.Count;
                    if (role_mail_ct > 0 && WorldSystem.Instance.IsPureClientScene())
                    {
                        GfxSystem.PublishGfxEvent("ge_sync_mail_list", "mail", role_info.MailInfos);
                    }
                }
            }
        }
        private void HandleExpeditionResetResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult;
            if (null != protoData)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int hp = protoData.m_Hp;
                int mp = protoData.m_Mp;
                int rage = protoData.m_Rage;
                int schedule = protoData.m_Schedule;
                int last_reset_timestamp = protoData.m_LastResetTimestamp;
                bool can_reset = protoData.m_CanReset;
                bool allow_cost_gold = protoData.m_AllowCostGold;
                bool is_unlock = protoData.m_IsUnlock;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    ExpeditionPlayerInfo expedition_info = LobbyClient.Instance.CurrentRole.GetExpeditionInfo();
                    if (null != expedition_info)
                    {
                        expedition_info.Reset();
                        ///
                        expedition_info.Hp = hp;
                        expedition_info.Mp = mp;
                        expedition_info.IsUnlock = is_unlock;
                        expedition_info.Rage = rage;
                        expedition_info.Schedule = schedule;
                        expedition_info.LastResetTimestamp = (double)last_reset_timestamp;
                        expedition_info.CanReset = can_reset;
                        if (0 == schedule)
                        {
                            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                            if (null != playerself)
                            {
                                expedition_info.Hp = 1 * 1000;
                                expedition_info.Mp = 1 * 1000;
                                expedition_info.Rage = 1 * 1000;
                            }
                        }
                        ///
                        if (null != protoData.Tollgates && schedule < expedition_info.Tollgates.Length)
                        {
                            ExpeditionPlayerInfo.TollgateData active_tollgates = expedition_info.Tollgates[schedule];
                            if (null != active_tollgates)
                            {
                                /// award
                                List<bool> award_info = protoData.Tollgates.IsAcceptedAward;
                                if (null != award_info && award_info.Count > 0)
                                {
                                    int award_ct = award_info.Count;
                                    if (expedition_info.Tollgates.Length == award_ct)
                                    {
                                        for (int i = 0; i < award_ct; i++)
                                        {
                                            expedition_info.Tollgates[i].IsAcceptedAward = award_info[i];
                                        }
                                    }
                                }
                                /// 
                                EnemyType type = (EnemyType)protoData.Tollgates.Type;
                                active_tollgates.Type = type;
                                active_tollgates.IsFinish = protoData.Tollgates.IsFinish;
                                active_tollgates.IsPostResult = false;
                                active_tollgates.IsPlayAnim = false;
                                active_tollgates.IsDoClear = false;
                                if (EnemyType.ET_Boss == type || EnemyType.ET_Monster == type)
                                {
                                    List<int> enemy_array = protoData.Tollgates.EnemyArray;
                                    List<int> enemy_attr_array = protoData.Tollgates.EnemyAttrArray;
                                    if (null != enemy_array && enemy_array.Count > 0
                                      && null != enemy_attr_array && enemy_attr_array.Count > 0
                                      && enemy_array.Count == enemy_attr_array.Count)
                                    {
                                        active_tollgates.EnemyList.Clear();
                                        active_tollgates.EnemyAttrList.Clear();
                                        active_tollgates.UserImageList.Clear();
                                        for (int index = 0; index < enemy_array.Count; index++)
                                        {
                                            active_tollgates.EnemyList.Add(enemy_array[index]);
                                            active_tollgates.EnemyAttrList.Add(enemy_attr_array[index]);
                                        }
                                    }
                                }
                                else
                                {
                                    if (null != protoData.Tollgates.UserImageArray && protoData.Tollgates.UserImageArray.Count > 0)
                                    {
                                        active_tollgates.EnemyList.Clear();
                                        active_tollgates.UserImageList.Clear();
                                        active_tollgates.EnemyAttrList.Clear();
                                        int image_ct = protoData.Tollgates.UserImageArray.Count;
                                        for (int index = 0; index < image_ct; index++)
                                        {
                                            if (null != protoData.Tollgates.UserImageArray[index])
                                            {
                                                ExpeditionImageInfo image_info = new ExpeditionImageInfo();
                                                image_info.Guid = protoData.Tollgates.UserImageArray[index].Guid;
                                                image_info.HeroId = protoData.Tollgates.UserImageArray[index].HeroId;
                                                image_info.Nickname = protoData.Tollgates.UserImageArray[index].Nickname;
                                                image_info.Level = protoData.Tollgates.UserImageArray[index].Level;
                                                image_info.FightingScore = protoData.Tollgates.UserImageArray[index].FightingScore;
                                                /// equips
                                                List<ArkCrossEngineMessage.ItemDataMsg> equip_info = protoData.Tollgates.UserImageArray[index].EquipInfo;
                                                if (null != equip_info && equip_info.Count > 0)
                                                {
                                                    for (int assit_index = 0; assit_index < equip_info.Count; assit_index++)
                                                    {
                                                        image_info.Equips[assit_index] = new ItemDataInfo();
                                                        image_info.Equips[assit_index].ItemId = equip_info[assit_index].ItemId;
                                                        image_info.Equips[assit_index].Level = equip_info[assit_index].Level;
                                                        image_info.Equips[assit_index].ItemNum = equip_info[assit_index].Num;
                                                        image_info.Equips[assit_index].RandomProperty = equip_info[assit_index].AppendProperty;
                                                    }
                                                }
                                                /// skills
                                                List<ArkCrossEngineMessage.SkillDataInfo> skill_info = protoData.Tollgates.UserImageArray[index].SkillInfo;
                                                if (null != skill_info && skill_info.Count > 0)
                                                {
                                                    for (int assit_index = 0; assit_index < skill_info.Count; assit_index++)
                                                    {
                                                        int skill_id = skill_info[assit_index].ID;
                                                        int skill_level = skill_info[assit_index].Level;
                                                        int skill_pos = skill_info[assit_index].Postions;
                                                        if (skill_pos > 0)
                                                        {
                                                            SkillInfo skill_data = new SkillInfo(skill_id, skill_level);
                                                            skill_data.Postions.Presets[0] = (SlotPosition)skill_pos;
                                                            image_info.Skills.Add(skill_data);
                                                        }
                                                    }
                                                }
                                                /// legacys
                                                List<ArkCrossEngineMessage.LegacyDataMsg> legacy_info = protoData.Tollgates.UserImageArray[index].LegacyInfo;
                                                if (null != legacy_info && legacy_info.Count > 0)
                                                {
                                                    for (int assit_index = 0; assit_index < legacy_info.Count; assit_index++)
                                                    {
                                                        image_info.Legacys[assit_index] = new ItemDataInfo();
                                                        image_info.Legacys[assit_index].ItemId = legacy_info[assit_index].ItemId;
                                                        image_info.Legacys[assit_index].Level = legacy_info[assit_index].Level;
                                                        image_info.Legacys[assit_index].IsUnlock = legacy_info[assit_index].IsUnlock;
                                                        image_info.Legacys[assit_index].RandomProperty = legacy_info[assit_index].AppendProperty;
                                                    }
                                                }
                                                ///
                                                active_tollgates.UserImageList.Add(image_info);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            /// award
                            List<bool> award_info = protoData.Tollgates.IsAcceptedAward;
                            if (null != award_info && award_info.Count > 0)
                            {
                                int award_ct = award_info.Count;
                                if (expedition_info.Tollgates.Length == award_ct)
                                {
                                    for (int i = 0; i < award_ct; i++)
                                    {
                                        expedition_info.Tollgates[i].IsAcceptedAward = award_info[i];
                                    }
                                }
                            }
                        }
                        if (0 == schedule)
                        {
                            GfxSystem.PublishGfxEvent("ge_expedition_info", "expedition", result);
                        }
                    }
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_expedition_info", "expedition", result);
                }
            }
        }
        private void OnNewbieEnd()
        {
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (null != role_info)
            {
                if (role_info.NewbieGuides.Count > 0)
                {
                    role_info.NewbieGuides.RemoveAt(0);
                }
                if (role_info.NewbieGuides.Count > 0)
                {
                    SystemGuideConfig newbieGuide = SystemGuideConfigProvider.Instance.GetDataById(role_info.NewbieGuides[0]);
                    if (null != newbieGuide)
                    {
                        GfxSystem.PublishGfxEvent("ge_show_newbieguide", "ui", newbieGuide.Operations);
                    }
                }
            }
        }
        private void HandleMidasTouchResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_MidasTouchResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MidasTouchResult;
            if (null != protoData)
            {
                int count = protoData.m_Count;
                int cost_glod = protoData.m_CostGlod;
                int gain_money = protoData.m_GainMoney;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.BuyMoneyCount = count;
                    role_info.Gold -= cost_glod;
                    role_info.Money += gain_money;
                }
                GfxSystem.PublishGfxEvent("ge_midas_touch", "midastouch", result);
            }
        }
        private void HandleExchangeGoodsResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ExchangeGoodsResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ExchangeGoodsResult;
            if (null != protoData)
            {
                int exchangeid = protoData.m_ExchangeId;
                int num = protoData.m_ExchangeNum;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                StoreConfig sc = StoreConfigProvider.Instance.GetDataById(exchangeid);
                if (GeneralOperationResult.LC_Succeed == result && null != role_info && sc != null)
                {
                    if (null != role_info.ExchangeGoodsDic)
                    {
                        if (role_info.ExchangeGoodsDic.ContainsKey(exchangeid))
                        {
                            role_info.ExchangeGoodsDic.Remove(exchangeid);
                        }
                        role_info.ExchangeGoodsDic.Add(exchangeid, num);
                    }
                    if (sc != null)
                    {
                        int cost = 0;
                        if ((num - 1) >= 0)
                        {
                            if (!sc.m_HaveDayLimit)
                            {
                                cost = sc.m_Price[0];
                            }
                            else if (sc.m_Price.Count > (num - 1))
                            {
                                cost = sc.m_Price[num - 1];
                            }
                        }
                        if (sc.m_Currency == ItemConfigProvider.Instance.GetGoldId())
                        {
                            role_info.Money -= cost;
                        }
                        else if (sc.m_Currency == ItemConfigProvider.Instance.GetDiamondId())
                        {
                            role_info.Gold -= cost;
                        }
                        else
                        {
                            role_info.ReduceItemData(sc.m_Currency, 0, cost);
                            if (role_info.CurrencyId == sc.m_Currency)
                            {
                                ItemDataInfo idi = role_info.GetItemData(sc.m_Currency, 0);
                                if (idi != null)
                                {
                                    role_info.ExchangeCurrency = idi.ItemNum;
                                }
                                else
                                {
                                    role_info.ExchangeCurrency = 0;
                                }
                            }
                        }
                    }
                }
                if (WorldSystem.Instance.IsPureClientScene() && sc != null)
                {
                    GfxSystem.PublishGfxEvent("ge_exchange_goods", "store", sc.m_Currency, false, result, exchangeid, num);
                }
            }
        }
        private void HandleRefreshExchangeResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RefreshExchangeResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RefreshExchangeResult;
            if (null != protoData)
            {
                RoleInfo ri = LobbyClient.Instance.CurrentRole;
                if (ri != null && ri.RefreshDic != null)
                {
                    if (protoData.m_CurrencyId == 0)
                    {
                        ri.RefreshDic.Clear();
                        ri.ExchangeGoodsDic.Clear();
                    }
                    else
                    {
                        if (ri.RefreshDic.ContainsKey(protoData.m_CurrencyId))
                        {
                            ri.RefreshDic[protoData.m_CurrencyId] = protoData.m_RefreshNum;
                        }
                        else
                        {
                            ri.RefreshDic.Add(protoData.m_CurrencyId, protoData.m_RefreshNum);
                        }
                        StoreConfig sc;
                        List<int> removelist = new List<int>();
                        foreach (int key in ri.ExchangeGoodsDic.Keys)
                        {
                            sc = StoreConfigProvider.Instance.GetDataById(key);
                            if (sc != null && sc.m_Currency == protoData.m_CurrencyId)
                            {
                                removelist.Add(key);
                            }
                        }
                        foreach (int remove in removelist)
                        {
                            ri.ExchangeGoodsDic.Remove(remove);
                        }
                        removelist.Clear();
                        if (protoData.m_RequestRefreshResult == (int)GeneralOperationResult.LC_Succeed)
                        {
                            ri.Gold -= protoData.m_RefreshNum * 50;
                        }
                    }
                }
                if (WorldSystem.Instance.IsPureClientScene())
                {
                    GfxSystem.PublishGfxEvent("ge_exchange_goods", "store", protoData.m_CurrencyId, true, (GeneralOperationResult)protoData.m_RequestRefreshResult, 0, 0);
                    GfxSystem.PublishGfxEvent("ge_refresh_exchange_num", "store", protoData.m_CurrencyId, protoData.m_RefreshNum);
                }
            }
        }
        private void HandleMissionCompleted(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            ArkCrossEngineMessage.Msg_LC_MissionCompleted protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MissionCompleted;
            if (null != protoData)
            {
                int missionId = protoData.m_MissionId;
                string progress = protoData.m_Progress;
                role_info.GetMissionStateInfo().CompletedMission(missionId);
                GfxSystem.PublishGfxEvent("ge_about_task", "task", missionId, MissionOperationType.FINISH, progress);
            }
        }
        private void HandleResetDailyMissions(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (null != role_info)
            {
                MissionStateInfo missionStateInfo = role_info.GetMissionStateInfo();
                if (null != missionStateInfo)
                {
                    missionStateInfo.Clear();
                    JsonData missions = jsonData["m_Missions"];
                    if (true == missions.IsArray && missions.Count > 0)
                    {
                        for (int i = 0; i < missions.Count; ++i)
                        {
                            int id = missions[i].GetInt("m_MissionId");
                            if (missions[i].GetBoolean("m_IsCompleted"))
                            {
                                missionStateInfo.AddMission(id, MissionStateType.COMPLETED);
                                missionStateInfo.CompletedMissions[id].Progress = missions[i].GetString("m_Progress");
                                GfxSystem.PublishGfxEvent("ge_about_task", "task", id, MissionOperationType.FINISH, missions[i].GetString("m_Progress"));
                            }
                            else
                            {
                                missionStateInfo.AddMission(id, MissionStateType.UNCOMPLETED);
                                missionStateInfo.UnCompletedMissions[id].Progress = missions[i].GetString("m_Progress");
                                GfxSystem.PublishGfxEvent("ge_about_task", "task", id, MissionOperationType.ADD, missions[i].GetString("m_Progress"));
                            }
                        }
                    }
                }
            }
        }
        private void HandleSyncNoticeContent(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncNoticeContent protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncNoticeContent;
            if (null != protoData)
            {
                string content = protoData.m_Content;
                int roll_num = protoData.m_RollNum;
                if (WorldSystem.Instance.IsPureClientScene()
                  || WorldSystem.Instance.IsPveScene()
                  || WorldSystem.Instance.IsPvpScene())
                {
                    GfxSystem.PublishGfxEvent("ge_notice", "notice", content, roll_num);
                }
            }
        }
        private void HandleMpveAwardResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_MpveAwardResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MpveAwardResult;
            if (null != protoData)
            {
                int award_index = protoData.m_AwardIndex;
                int add_money = protoData.m_AddMoney;
                MpveAwardResult result = (MpveAwardResult)protoData.m_Result;
                if (MpveAwardResult.Succeed == result && null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.AttemptAcceptedAward = award_index;
                    role_info.AttemptCurAcceptedCount += 1;
                    List<int> item_id_list = new List<int>();
                    List<int> item_num_list = new List<int>();
                    if (null != protoData.m_Items && protoData.m_Items.Count > 0)
                    {
                        int ct = protoData.m_Items.Count;
                        for (int i = 0; i < ct; i++)
                        {
                            int cur_item_id = protoData.m_Items[i].m_Id;
                            int cur_item_num = protoData.m_Items[i].m_Num;
                            item_id_list.Add(cur_item_id);
                            item_num_list.Add(cur_item_num);
                        }
                    }
                    GfxSystem.PublishGfxEvent("ge_mpve_attempt_award", "mpve", award_index, add_money, item_id_list, item_num_list, result);
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_mpve_attempt_award", "mpve", award_index, add_money, null, null, result);
                }
            }
        }
        private void HandleRequestUsersResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RequestUsersResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RequestUsersResult;
            if (null != protoMsg)
            {
                List<ArkCrossEngineMessage.Msg_LC_RequestUsersResult.UserInfo> users = protoMsg.m_Users;
                foreach (ArkCrossEngineMessage.Msg_LC_RequestUsersResult.UserInfo user in users)
                {
                    WorldSystem.Instance.AddCityUser(user.m_Guid, user.m_HeroId, user.m_Nick, user.m_X, user.m_Z, user.m_FaceDir, user.m_XSoulItemId, user.m_XSoulLevel, user.m_XSoulExp, user.m_XSoulShowLevel, user.m_WingItemId, user.m_WingLevel);
                }
            }
        }
        private void HandleRequestUserPositionResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RequestUserPositionResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RequestUserPositionResult;
            if (null != protoMsg)
            {
                if (protoMsg.m_X < 0 || protoMsg.m_Z < 0)
                {
                    WorldSystem.Instance.RemoveCityUser(protoMsg.m_User);
                }
                else
                {
                    WorldSystem.Instance.UpdateCityUser(protoMsg.m_User, protoMsg.m_X, protoMsg.m_Z, protoMsg.m_FaceDir);
                }
            }
        }
        private void HandleSyncPlayerInfo(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncPlayerInfo protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncPlayerInfo;
            if (null != protoMsg)
            {
                string target_nick = protoMsg.m_Nick;
                int target_level = protoMsg.m_Level;
                int target_score = protoMsg.m_Score;
                GfxSystem.PublishGfxEvent("ge_sync_player_info", "info", target_nick, target_level, target_score);
            }
        }
        private void RequestVigor()
        {
            try
            {
                JsonMessage rvMsg = new JsonMessage(JsonMessageID.RequestVigor);
                rvMsg.m_JsonData.Set("m_Guid", m_Guid);
                SendMessage(rvMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void SetNewbieFlag(int bit, int num)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.SetNewbieFlag);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_SetNewbieFlag protoData = new ArkCrossEngineMessage.Msg_CL_SetNewbieFlag();
                protoData.m_Bit = bit;
                protoData.m_Num = num;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void RecordNewbieFlag(int bit)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.RecordNewbieFlag);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_RecordNewbieFlag protoData = new ArkCrossEngineMessage.Msg_CL_RecordNewbieFlag();
                protoData.m_Bit = bit;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void UploadFPS(string fps, string hardwarename)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.UploadFPS);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_UploadFPS protoData = new ArkCrossEngineMessage.Msg_CL_UploadFPS();
                protoData.m_Fps = fps;
                protoData.m_Nickname = hardwarename;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void SetNewbieActionFlag(int bit, int num)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.SetNewbieActionFlag);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_SetNewbieActionFlag protoData = new ArkCrossEngineMessage.Msg_CL_SetNewbieActionFlag();
                protoData.m_Bit = bit;
                protoData.m_Num = num;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleSyncVigor(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncVigor protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncVigor;
            if (null != protoMsg)
            {
                int vigor = protoMsg.m_Vigor;
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.Vigor = vigor;
                    GfxSystem.PublishGfxEvent("ge_sync_player_vigor", "info", vigor);
                }
            }
        }
        private void HandleSyncAttemptInfo(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncAttemptInfo protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncAttemptInfo;
            if (null != protoMsg)
            {
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.AttemptAward = protoMsg.m_AttemptAward;
                    role_info.AttemptAcceptedAward = protoMsg.m_AttemptAcceptedAward;
                    role_info.AttemptCurAcceptedCount = protoMsg.m_AttemptCurAcceptedCount;
                    GfxSystem.PublishGfxEvent("ge_sync_attempt_info", "info");
                }
            }
        }
        private void HandleSyncNewbieFlag(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncNewbieFlag protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncNewbieFlag;
            if (null != protoMsg)
            {
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.NewbieFlag = protoMsg.m_NewbieFlag;
                    // GfxSystem.PublishGfxEvent("ge_sync_newbie_flag", "newbie");
                }
            }
        }
        private void HandleSyncNewbieActionFlag(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncNewbieActionFlag protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncNewbieActionFlag;
            if (null != protoMsg)
            {
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.NewbieActionFlag = protoMsg.m_NewbieFlag;
                    // GfxSystem.PublishGfxEvent("ge_sync_newbie_action_flag", "newbie");
                }
            }
        }
        private void HandleSyncGuideFlag(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncGuideFlag protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncGuideFlag;
            if (null != protoMsg)
            {
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    role_info.SetActionFlag(protoMsg.m_Flag);
                    // GfxSystem.PublishGfxEvent("ge_sync_guide_flag", "newbie");
                }
            }
        }
        private void HandleArenaInfoResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ArenaInfoResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ArenaInfoResult;
            if (null == protoMsg)
            {
                return;
            }
            if (null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                role_info.ArenaStateInfo.LastInfoQueryTime = DateTime.Now;
                role_info.ArenaStateInfo.Rank = protoMsg.m_ArenaInfo.Rank;
                role_info.ArenaStateInfo.LastInfoQueryRank = protoMsg.m_ArenaInfo.Rank;
                role_info.ArenaStateInfo.FightPartners.Clear();
                foreach (var partner_msg in protoMsg.m_ArenaInfo.FightParters)
                {
                    role_info.ArenaStateInfo.FightPartners.Add(partner_msg.Id);
                }
                role_info.ArenaStateInfo.LeftFightCount = protoMsg.m_LeftBattleCount;
                role_info.ArenaStateInfo.CurFightCountBuyTime = protoMsg.m_CurFightCountByTime;
                long battle_cd_passed_time = role_info.ArenaStateInfo.BaseConfig.BattleCd - protoMsg.m_BattleLeftCDTime;
                role_info.ArenaStateInfo.LastBattleServerTime = DateTime.Now.AddMilliseconds(-battle_cd_passed_time);
            }
            GfxSystem.PublishGfxEvent("arena_info_result", "arena");
        }

        private void HandleArenaMatchGroupResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ArenaMatchGroupResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ArenaMatchGroupResult;
            if (null == protoMsg)
            {
                return;
            }
            if (null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                role_info.ArenaStateInfo.MatchGroups.Clear();
                foreach (var group_data in protoMsg.m_MatchGroups)
                {
                    MatchGroup group = new MatchGroup();
                    group.One = CreateArenaTargetInfo(group_data.One);
                    group.Two = CreateArenaTargetInfo(group_data.Two);
                    group.Three = CreateArenaTargetInfo(group_data.Three);
                    role_info.ArenaStateInfo.MatchGroups.Add(group);
                }
                role_info.ArenaStateInfo.LastMatchGroupQueryRank = role_info.ArenaStateInfo.Rank;
                role_info.ArenaStateInfo.LastMatchGroupQueryTime = DateTime.Now;
            }
            GfxSystem.PublishGfxEvent("match_group_result", "arena");
        }

        private void HandleArenaStartChallengeResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ArenaStartCallengeResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ArenaStartCallengeResult;
            if (null == protoMsg)
            {
                return;
            }
            if (protoMsg.m_ResultCode != (int)GeneralOperationResult.LC_Succeed)
            {
                GfxSystem.PublishGfxEvent("start_challenge_result", "arena", protoMsg.m_ResultCode);
                return;
            }
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            ArenaTargetInfo target = null;
            for (int i = 0; i < roleself.ArenaStateInfo.MatchGroups.Count; i++)
            {
                MatchGroup group = roleself.ArenaStateInfo.MatchGroups[i];
                if (group.One.Guid == protoMsg.m_TargetGuid)
                {
                    target = group.One;
                    break;
                }
                if (group.Two.Guid == protoMsg.m_TargetGuid)
                {
                    target = group.Two;
                    break;
                }
                if (group.Three.Guid == protoMsg.m_TargetGuid)
                {
                    target = group.Three;
                    break;
                }
            }
            if (target == null)
            {
                LogSystem.Debug("----can't find {0} from match group info!", protoMsg.m_TargetGuid);
                return;
            }
            roleself.ArenaStateInfo.StartChallenge(target, protoMsg.m_Sign);
            //LogSystem.Debug("-----arena start challenge:");
            GameControler.ChangeScene(3002);
            GfxSystem.PublishGfxEvent("start_challenge_result", "arena", protoMsg.m_ResultCode);
        }

        private void HandleArenaChallengeResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ArenaChallengeResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ArenaChallengeResult;
            if (null == protoMsg)
            {
                return;
            }
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            if (roleself == null)
            {
                return;
            }
            ArenaStateInfo self_arena = roleself.ArenaStateInfo;
            ChallengeInfo challenge_info = CreateChallengeInfo(protoMsg.m_ChallengeInfo);
            self_arena.ChallengeHistory.Insert(0, challenge_info);
            int max_history = 10;
            ArenaBaseConfig base_config = ArenaConfigProvider.Instance.GetBaseConfigById(1);
            if (base_config != null)
            {
                max_history = base_config.MaxHistoryCount;
            }
            int cur_count = self_arena.ChallengeHistory.Count;
            if (cur_count > max_history)
            {
                self_arena.ChallengeHistory.RemoveRange(max_history - 1, cur_count - max_history);
            }
            self_arena.DealChallengeResult(challenge_info);
        }

        private void HandleArenaQueryRankResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ArenaQueryRankResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ArenaQueryRankResult;
            if (null == protoMsg)
            {
                return;
            }
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            roleself.ArenaStateInfo.LastTopQueryTime = DateTime.Now;
            roleself.ArenaStateInfo.LastTopQueryRank = roleself.ArenaStateInfo.Rank;
            roleself.ArenaStateInfo.RankList.Clear();
            foreach (ArkCrossEngineMessage.ArenaInfoMsg msg in protoMsg.RankMsg)
            {
                roleself.ArenaStateInfo.RankList.Add(CreateArenaTargetInfo(msg));
            }
            GfxSystem.PublishGfxEvent("query_rank_result", "arena");
        }

        private void HandleArenaChangePartnerResult(JsonMessage lobbyMsg)
        {
            ArkCrossEngineMessage.Msg_LC_ArenaChangePartnerResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ArenaChangePartnerResult;
            if (null == protoMsg)
            {
                return;
            }
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            roleself.ArenaStateInfo.FightPartners.Clear();
            UserInfo player = WorldSystem.Instance.GetPlayerSelf();
            if (protoMsg.Result == (int)GeneralOperationResult.LC_Succeed)
            {
                for (int i = 0; i < protoMsg.Partners.Count; i++)
                {
                    roleself.ArenaStateInfo.FightPartners.Add(protoMsg.Partners[i]);
                }
            }
            GfxSystem.PublishGfxEvent("change_partners_result", "arena");
        }

        private void HandleArenaQueryHistoryResult(JsonMessage lobbyMsg)
        {
            ArkCrossEngineMessage.Msg_LC_ArenaQueryHistoryResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ArenaQueryHistoryResult;
            if (null == protoData)
            {
                return;
            }
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            List<ChallengeInfo> history = roleself.ArenaStateInfo.ChallengeHistory;
            history.Clear();
            for (int i = 0; i < protoData.ChallengeHistory.Count; i++)
            {
                ArkCrossEngineMessage.ChallengeInfoData data = protoData.ChallengeHistory[i];
                history.Add(CreateChallengeInfo(data));
            }
            GfxSystem.PublishGfxEvent("query_history_result", "arena");
        }
        private ChallengeInfo CreateChallengeInfo(ArkCrossEngineMessage.ChallengeInfoData data)
        {
            ChallengeInfo info = new ChallengeInfo();
            info.Challenger = CreateChallengeEntity(data.Challenger);
            info.Target = CreateChallengeEntity(data.Target);
            info.IsChallengerSuccess = data.IsChallengeSuccess;
            info.ChallengeEndTime = new DateTime(data.EndTime);
            return info;
        }
        private ChallengeEntityInfo CreateChallengeEntity(ArkCrossEngineMessage.ChallengeEntityData entity)
        {
            ChallengeEntityInfo target = new ChallengeEntityInfo();
            target.Guid = entity.Guid;
            target.Level = entity.Level;
            target.HeroId = entity.HeroId;
            target.NickName = entity.NickName;
            target.FightScore = entity.FightScore;
            target.Rank = entity.Rank;
            target.UserDamage = entity.UserDamage;
            target.PartnerDamage.Clear();
            for (int i = 0; i < entity.PartnerDamage.Count; i++)
            {
                DamageInfo damange_info = new DamageInfo();
                damange_info.OwnerId = entity.PartnerDamage[i].OwnerId;
                damange_info.Damage = entity.PartnerDamage[i].Damage;
                target.PartnerDamage.Add(damange_info);
            }
            return target;
        }
        private ArenaTargetInfo CreateArenaTargetInfo(ArkCrossEngineMessage.ArenaInfoMsg msg)
        {
            ArenaTargetInfo target_info = new ArenaTargetInfo();
            target_info.Guid = msg.Guid;
            target_info.HeroId = msg.HeroId;
            target_info.Level = msg.Level;
            target_info.Rank = msg.Rank;
            target_info.Nickname = msg.NickName;
            target_info.FightingScore = msg.FightScore;

            if (msg.ActivePartner != null)
            {
                PartnerConfig active_partner_config = PartnerConfigProvider.Instance.GetDataById(msg.ActivePartner.Id);
                if (null != active_partner_config)
                {
                    PartnerInfo partner = new PartnerInfo(active_partner_config);
                    partner.CurAdditionLevel = msg.ActivePartner.AdditionLevel;
                    partner.CurSkillStage = msg.ActivePartner.SkillStage;
                    target_info.ActivePartner = partner;
                }
            }

            int i = 0;
            foreach (var p in msg.FightParters)
            {
                PartnerConfig config = PartnerConfigProvider.Instance.GetDataById(p.Id);
                if (null == config)
                {
                    continue;
                }
                PartnerInfo partner = new PartnerInfo(config);
                partner.CurAdditionLevel = p.AdditionLevel;
                partner.CurSkillStage = p.SkillStage;
                target_info.FightPartners.Add(partner);
            }
            foreach (var e in msg.EquipInfo)
            {
                ItemDataInfo equip = new ItemDataInfo();
                equip.ItemId = e.ItemId;
                equip.Level = e.Level;
                equip.RandomProperty = e.AppendProperty;
                target_info.Equips[i++] = equip;
            }
            foreach (var s in msg.ActiveSkills)
            {
                SkillInfo skill = new SkillInfo(s.ID);
                skill.SkillLevel = s.Level;
                skill.Postions.Presets[0] = (SlotPosition)s.Postions;
                target_info.Skills.Add(skill);
            }
            i = 0;
            foreach (var l in msg.LegacyAttr)
            {
                ItemDataInfo item = new ItemDataInfo();
                item.ItemId = l.ItemId;
                item.Level = l.Level;
                item.IsUnlock = l.IsUnlock;
                item.RandomProperty = l.AppendProperty;
                target_info.Legacys[i++] = item;
            }
            foreach (var xsoul_msg in msg.XSouls)
            {
                ArenaXSoulInfo xsoulinfo = new ArenaXSoulInfo();
                xsoulinfo.ItemId = xsoul_msg.ItemId;
                xsoulinfo.Level = xsoul_msg.Level;
                xsoulinfo.ModelLevel = xsoul_msg.ModelLevel;
                xsoulinfo.Experience = xsoul_msg.Experience;
                target_info.XSoulInfo.Add(xsoulinfo);
            }
            return target_info;
        }
        private void HandleSyncGoldTollgateInfo(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncGoldTollgateInfo protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncGoldTollgateInfo;
            if (null == protoMsg)
            {
                return;
            }
            if (null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                role_info.GoldCurAcceptedCount = protoMsg.m_GoldCurAcceptedCount;
            }
            GfxSystem.PublishGfxEvent("ge_sync_gold_tollgate_info", "mpve");
        }
        private void HandleCompoundEquipResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_CompoundEquipResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_CompoundEquipResult;
            if (null == protoData)
                return;
            if (null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;
                if (GeneralOperationResult.LC_Succeed == result)
                {
                    ItemCompoundConfig cpd_data = ItemCompoundConfigProvider.Instance.GetDataById(protoData.m_ItemId);
                    if (null == cpd_data)
                        return;
                    ItemDataInfo part_info = role_info.GetItemData(protoData.m_PartId, 0);
                    if (null != part_info)
                    {
                        if (part_info.ItemNum == cpd_data.m_PartNum)
                        {
                            role_info.DelItemData(part_info.ItemId, 0);
                        }
                        else
                        {
                            role_info.ReduceItemData(part_info.ItemId, 0, cpd_data.m_PartNum);
                        }
                    }
                    ItemDataInfo material_info = role_info.GetItemData(cpd_data.m_MaterialId, 0);
                    if (null != material_info && cpd_data.m_MaterialNum > 0)
                    {
                        if (material_info.ItemNum == cpd_data.m_MaterialNum)
                        {
                            role_info.DelItemData(material_info.ItemId, 0);
                        }
                        else
                        {
                            role_info.ReduceItemData(material_info.ItemId, 0, cpd_data.m_MaterialNum);
                        }
                    }
                }
                GfxSystem.PublishGfxEvent("ge_compound_equip", "compound", result);
            }
        }

        private void HandleArenaBuyFightCountResult(JsonMessage lobbyMsg)
        {
            ArkCrossEngineMessage.Msg_LC_ArenaBuyFightCountResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ArenaBuyFightCountResult;
            if (null == protoData)
            {
                return;
            }
            if (protoData.Result == (int)GeneralOperationResult.LC_Succeed)
            {
                RoleInfo roleself = LobbyClient.Instance.CurrentRole;
                roleself.ArenaStateInfo.LeftFightCount = protoData.CurFightCount;
                roleself.ArenaStateInfo.CurFightCountBuyTime = protoData.CurBuyTime;
                ArenaBuyFightCountConfig config = ArenaConfigProvider.Instance.BuyFightCountConfig.GetDataById(protoData.CurBuyTime);
                if (config != null)
                {
                    roleself.Gold -= config.Cost;
                }
            }
            GfxSystem.PublishGfxEvent("buy_fight_count_result", "arena", protoData.Result, protoData.CurBuyTime, protoData.CurFightCount);
        }

        private void HandleSyncCombatData(JsonMessage lobbyMsg)
        {
            ArkCrossEngineMessage.Msg_LC_SyncCombatData protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncCombatData;
            if (null == protoData)
                return;

            RoleInfo role = LobbyClient.Instance.CurrentRole;
            /// skills
            if (null != protoData.m_Skills && protoData.m_Skills.Count > 0 && null != role.SkillInfos)
            {
                int ct = protoData.m_Skills.Count;
                for (int i = 0; i < ct; i++)
                {
                    int skill_id = protoData.m_Skills[i].ID;
                    SkillInfo si = new SkillInfo(skill_id);
                    si.SkillLevel = protoData.m_Skills[i].Level;
                    si.Postions.Presets[0] = (SlotPosition)protoData.m_Skills[i].Postions;
                    LogSystem.Debug("sync skillinfo skillid = {0}", skill_id);
                    for (int index = 0; index < role.SkillInfos.Count; index++)
                    {
                        if (role.SkillInfos[index].SkillId == si.SkillId)
                        {
                            role.SkillInfos[index].SkillLevel = si.SkillLevel;
                            role.SkillInfos[index].Postions = si.Postions;
                            break;
                        }
                    }
                }
            }
            /// legacys
            if (null != protoData.m_Legacys && protoData.m_Legacys.Count > 0)
            {
                int ct = protoData.m_Legacys.Count;
                for (int i = 0; i < ct; ++i)
                {
                    if (null != protoData.m_Legacys[i])
                    {
                        role.Legacys[i] = new ItemDataInfo();
                        role.Legacys[i].ItemId = protoData.m_Legacys[i].ItemId;
                        role.Legacys[i].Level = protoData.m_Legacys[i].Level;
                        role.Legacys[i].RandomProperty = protoData.m_Legacys[i].AppendProperty;
                        role.Legacys[i].IsUnlock = protoData.m_Legacys[i].IsUnlock;
                    }
                }
            }
            /// equipments
            if (null != protoData.m_Equipments && protoData.m_Equipments.Count > 0 && null != role.Equips)
            {
                int ct = protoData.m_Equipments.Count;
                for (int i = 0; i < ct; i++)
                {
                    if (null != protoData.m_Equipments[i])
                    {
                        ItemDataInfo info = new ItemDataInfo();
                        info.ItemId = protoData.m_Equipments[i].ItemId;
                        info.Level = protoData.m_Equipments[i].Level;
                        info.ItemNum = protoData.m_Equipments[i].Num;
                        info.RandomProperty = protoData.m_Equipments[i].AppendProperty;
                        role.SetEquip(i, info);
                        LogSystem.Debug("sync equipment id = {0}, level = {1}", info.ItemId, info.Level);
                    }
                }
            }
            // XSoulInfo
            for (int i = 0; i < protoData.m_XSouls.Count; i++)
            {
                ArkCrossEngineMessage.XSoulDataMsg item_msg = protoData.m_XSouls[i];
                if (item_msg == null)
                {
                    continue;
                }
                ItemDataInfo item = new ItemDataInfo();
                item.ItemId = item_msg.ItemId;
                item.Level = item_msg.Level;
                item.Experience = item_msg.Experience;
                item.UpdateLevelByExperience();
                ItemConfig item_config = ItemConfigProvider.Instance.GetDataById(item.ItemId);
                item.ItemConfig = item_config;
                if (item_config != null)
                {
                    XSoulPart part = (XSoulPart)item_config.m_WearParts;
                    XSoulPartInfo part_info = new XSoulPartInfo(part, item);
                    part_info.ShowModelLevel = item_msg.ModelLevel;
                    role.GetXSoulInfo().SetXSoulPartData(part, part_info);
                }
            }
            // Partner
            PartnerInfo partnerInfo = role.PartnerStateInfo.GetPartnerInfoById(protoData.m_PartnerData.m_Id);
            if (null != partnerInfo)
            {
                partnerInfo.CurAdditionLevel = protoData.m_PartnerData.m_AdditionLevel;
                partnerInfo.CurSkillStage = protoData.m_PartnerData.m_SkillStage;
                role.PartnerStateInfo.SetActivePartner(partnerInfo.Id);
            }
            else
            {
                role.PartnerStateInfo.SetActivePartner(0);
            }
        }
        private void HandleServerShutdown(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            GfxSystem.PublishGfxEvent("ge_server_shutdown", "lobby");
        }
        private void OnRequestDareByNickname(string targetNickname)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestDare);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_RequestDare protoData = new ArkCrossEngineMessage.Msg_CL_RequestDare();
                protoData.m_TargetNickname = targetNickname;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void OnRequestDareByGuid(ulong targetGuid)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestDareByGuid);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_RequestDareByGuid protoData = new ArkCrossEngineMessage.Msg_CL_RequestDareByGuid();
                protoData.m_TargetGuid = targetGuid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleRequestDare(JsonMessage lobbyMsg)
        {
            ArkCrossEngineMessage.Msg_LC_RequestDare protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RequestDare;
            if (null == protoMsg)
            {
                return;
            }
            string challenger = protoMsg.m_ChallengerNickname;
            GfxSystem.PublishGfxEvent("ge_request_dare", "dare", challenger);
        }
        private void OnAcceptedDare(string challenger)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.AcceptedDare);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_AcceptedDare protoData = new ArkCrossEngineMessage.Msg_CL_AcceptedDare();
                protoData.m_ChallengerNickname = challenger;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleRequestDareResult(JsonMessage lobbyMsg)
        {
            ArkCrossEngineMessage.Msg_LC_RequestDareResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RequestDareResult;
            if (null == protoMsg)
            {
                return;
            }
            GfxSystem.PublishGfxEvent("ge_request_dare_result", "dare", protoMsg.m_Nickname, protoMsg.m_Result);
        }
        private void HandleSyncResetGowPrize(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (guid == m_Guid && null != role_info)
            {
                role_info.Gow.IsAcquirePrize = false;
                GfxSystem.PublishGfxEvent("ge_reset_gow_prize", "gow");
            }
        }
        private void OnRequestGowPrize()
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.RequestGowPrize);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleRequestGowPrizeResult(JsonMessage lobbyMsg)
        {
            ArkCrossEngineMessage.Msg_LC_RequestGowPrizeResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RequestGowPrizeResult;
            if (null == protoMsg)
            {
                return;
            }
            if ((int)GeneralOperationResult.LC_Succeed == protoMsg.m_Result)
            {
                GfxSystem.PublishGfxEvent("ge_sync_gow_prize", "gow", protoMsg.m_Money, protoMsg.m_Gold, protoMsg.m_Items, GeneralOperationResult.LC_Succeed);
            }
            else
            {
                GfxSystem.PublishGfxEvent("ge_sync_gow_prize", "gow", 0, 0, null, GeneralOperationResult.LC_Failure_Unknown);
            }

            LogSystem.Debug("gow prize ||| result:{0}", protoMsg.m_Result.ToString());
        }
    }
}