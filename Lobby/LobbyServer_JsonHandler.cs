using System;
using System.Text;
using CSharpCenterClient;
using Newtonsoft.Json;
using Lobby;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  internal partial class LobbyServer
  {    
    private void InstallJsonHandlers()
    {
      JsonMessageDispatcher.Init();
      JsonMessageDispatcher.SetMessageFilter(this.FilterMessage);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.Zero, typeof(JsonMessageZero), null, this.HandleZero);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UserHeartbeat, typeof(JsonMessageUserHeartbeat), null, this.HandleUserHeartbeat);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.Logout, typeof(JsonMessageLogout), null, this.HandleLogout);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestMatch, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestMatch), this.HandleFindTeam);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.CancelMatch, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_CancelMatch), this.HandleCancelFindTeam);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.StartGame, typeof(JsonMessageStartGame), null, this.HandleStartGame);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.QuitRoom, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_QuitRoom), this.HandleQuitRoom);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.NodeJsRegister, typeof(JsonMessageNodeJsRegister), null, this.HandleNodeJsRegister);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.AddFriend, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_AddFriend), this.HandleAddFriend);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ConfirmFriend, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ConfirmFriend), this.HandleConfirmFriend);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.DelFriend, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_DeleteFriend), this.HandleDelFriend);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.FriendList, typeof(JsonMessageWithGuid), null, this.HandleFriendList);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SinglePVE, typeof(JsonMessageSinglePVE), null, this.HandleSinglePVE);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.DiscardItem, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_DiscardItem), this.HandleDiscardItem);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.MountEquipment, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_MountEquipment), this.HandleMountEquipment);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UnmountEquipment, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UnmountEquipment), this.HandleUnmountEquipment);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.DirectLogin, typeof(JsonMessageDirectLogin), null, HandleDirectLogin);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.AccountLogin, typeof(JsonMessageAccountLogin), null, HandleAccountLogin);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.AccountLogout, typeof(JsonMessageAccountLogout), null, HandleAccountLogout);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RoleList, typeof(JsonMessageWithAccount), null, HandleRoleList);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.CreateNickname, typeof(JsonMessageCreateNickname), null, HandleCreateNickname);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.CreateRole, typeof(JsonMessageCreateRole), null, HandleCreateRole);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RoleEnter, typeof(JsonMessageWithAccount), typeof(ArkCrossEngineMessage.Msg_CL_RoleEnter), HandleRoleEnter);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.MountSkill, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_MountSkill), HandleMountSkill);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UnmountSkill, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UnmountSkill), HandleUnmountSkill);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UpgradeSkill, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UpgradeSkill), HandleUpgradeSkill);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UnlockSkill, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UnlockSkill), HandleUnlockSkill);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SwapSkill, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_SwapSkill), HandleSwapSkill);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UpgradeItem, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UpgradeItem), HandleUpgradeItem);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SaveSkillPreset, typeof(JsonMessageSaveSkillPreset), null, HandleSaveSkillPreset);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ActivateAccount, typeof(JsonMessageActivateAccount), null, HandleActivateAccount);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.StageClear, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_StageClear), HandleStageClear);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SweepStage, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_SweepStage), HandleSweepStage);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.LiftSkill, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_LiftSkill), HandleLiftSkill);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.BuyStamina, typeof(JsonMessageWithGuid), null, HandleBuyStamina);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.FinishMission, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_FinishMission), HandleFinishMission);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.BuyLife, typeof(JsonMessageWithGuid), null, HandleBuyLife);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UnlockLegacy, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UnlockLegacy), HandleUnlockLegacy);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UpgradeLegacy, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UpgradeLegacy), HandleUpgradeLegacy);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.AddXSoulExperience, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_AddXSoulExperience), HandleAddXSoulExperience);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.XSoulChangeShowModel, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_XSoulChangeShowModel), HandleXSoulChangeShowModel);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UpdateFightingScore, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UpdateFightingScore), HandleUpdateFightingScore);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GetMailList, typeof(JsonMessageGetMailList), null, HandleGetMailList);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ReceiveMail, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ReceiveMail), HandleReceiveMail);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ExpeditionReset, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ExpeditionReset), HandleExpeditionReset);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestExpedition, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestExpedition), HandleRequestExpedition);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.FinishExpedition, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_FinishExpedition), HandleFinishExpedition);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ExpeditionAward, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ExpeditionAward), HandleExpeditionAward);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GetGowStarList, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_GetGowStarList), HandleGetGowStarList);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.QueryExpeditionInfo, typeof(JsonMessageQueryExpeditionInfo), null, HandleQueryExpeditionInfo);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ReadMail, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ReadMail), HandleReadMail);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ExpeditionFailure, typeof(JsonMessageWithGuid), null, HandleExpeditionFailure);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.MidasTouch, typeof(JsonMessageWithGuid), null, HandleMidasTouch);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ExchangeGoods, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ExchangeGoods), HandleExchangeGoods);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestRefreshExchange, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestRefreshExchange), HandleRequestRefreshExchange);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.QueryFriendInfo, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_QueryFriendInfo), HandleQueryFriendInfo);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.PinviteTeam, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_PinviteTeam), HandlePinviteTeam);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestJoinGroup, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestJoinGroup), HandleRequestJoinGroup);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ConfirmJoinGroup, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ConfirmJoinGroup), HandleConfirmJoinGroup);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.QuitGroup, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_QuitGroup), HandleQuitGroup);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestGroupInfo, typeof(JsonMessageWithGuid), null, HandleRequestGroupInfo);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RefuseGroupRequest, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RefuseGroupRequest), HandleRefuseGroupRequest);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SelectPartner, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_SelectPartner), HandleSelectPartner);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UpgradePartnerLevel, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UpgradePartnerLevel), HandleUpgradePartnerLevel);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UpgradePartnerStage, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UpgradePartnerStage), HandleUpgradePartnerStage);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.CompoundPartner, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_CompoundPartner), HandleCompoundPartner);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.StartMpve, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_StartMpve), HandleStartMpve);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.QuitPve, typeof(JsonMessageWithGuid), null, HandleQuitPve);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestMpveAward, typeof(JsonMessageWithGuid), null, HandleMpveAward);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UpdatePosition, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UpdatePosition), HandleUpdatePosition);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestUsers, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestUsers), HandleRequestUsers);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestUserPosition, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestUserPosition), HandleRequestUserPosition);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ChangeCityScene, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ChangeCityScene), HandleChangeCityScene);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestPlayerInfo, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestPlayerInfo), HandleRequestPlayerInfo);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestVigor, typeof(JsonMessageWithGuid), null, HandleRequestVigor);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SetNewbieFlag, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_SetNewbieFlag), HandleSetNewbieFlag);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SetNewbieActionFlag, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_SetNewbieActionFlag), HandleSetNewbieActionFlag);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.QueryArenaInfo, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_QueryArenaInfo), HandleQueryArenaInfo);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.QueryArenaMatchGroup, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_QueryArenaMatchGroup), HandleQueryArenaMatchGroup);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SignInAndGetReward, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_SignInAndGetReward), HandleSignInAndGetReward);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ArenaStartChallenge, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ArenaStartChallenge), HandleArenaStartChallenge);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ArenaChallengeOver, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ArenaChallengeOver), HandleArenaChallengeOver);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ExchangeGift, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ExchangeGift), HandleExchangeGift);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ArenaQueryRank, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ArenaQueryRank), HandleArenaQueryRank);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ArenaChangePartners, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ArenaChangePartner), HandleArenaChangePartners);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ArenaQueryHistory, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ArenaQueryHistory), HandleArenaQueryHistory);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.CompoundEquip, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_CompoundEquip), HandleCompoundEquip);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ArenaBuyFightCount, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_ArenaBuyFightCount), HandleArenaBuyFightCount);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.WeeklyLoginReward, typeof(JsonMessageWithGuid), null, HandleWeeklyLoginReward);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GetQueueingCount, typeof(JsonMessageWithAccount), null, HandleGetQueueingCount);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.ArenaBeginFight, typeof(JsonMessageWithAccount), typeof(ArkCrossEngineMessage.Msg_CL_ArenaBeginFight), HandleArenaBeginFight);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RecordNewbieFlag, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RecordNewbieFlag), HandleRecordNewbieFlag);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.UploadFPS, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_UploadFPS), HandleUploadFPS);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestDare, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestDare), HandleRequestDare);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.AcceptedDare, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_AcceptedDare), HandleAcceptedDare);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestDareByGuid, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_RequestDareByGuid), HandleRequestDareByGuid);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.RequestGowPrize, typeof(JsonMessageWithGuid), null, HandleRequestGowPrize);

      //GM指令消息统一放一块，处理函数放到本文件最后，便于安全检查
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.AddAssets, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_AddAssets), HandleAddAssets);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.AddItem, typeof(JsonMessageAddItem), null, HandleAddItem);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.SendMail, typeof(JsonMessageSendMail), null, HandleSendMail);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GMResetDailyMissions, typeof(JsonMessageGMResetDailyMissions), null, HandleGMResetDailyMissions);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.PublishNotice, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_PublishNotice), HandlePublishNotice);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GmKickUser, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_GmKickUser), HandleGmKickUser);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GmLockUser, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_GmLockUser), HandleGmLockUser);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GmUnlockUser, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_GmUnlockUser), HandleGmUnlockUser);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GmAddExp, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_GmAddExp), HandleGmAddExp);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GmUpdateMaxUserCount, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_GmUpdateMaxUserCount), HandleGmUpdateMaxUserCount);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GmQueryInfoByGuidOrNickname, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_GmQueryInfoByGuidOrNickname), HandleGmQueryInfoByGuidOrNickname);
      JsonMessageDispatcher.RegisterMessageHandler((int)JsonMessageID.GmQueryInfosByDimNickname, typeof(JsonMessageWithGuid), typeof(ArkCrossEngineMessage.Msg_CL_GmQueryInfosByDimNickname), HandleGmQueryInfosByDimNickname);
      
    }
    //------------------------------------------------------------------------------------------------------
    private bool FilterMessage(JsonMessage msg, int handle, uint seq)
    {
      bool isContinue = true;
      JsonMessageWithGuid msgWithGuid = msg as JsonMessageWithGuid;
      if (null != msgWithGuid) {
        ulong guid = msgWithGuid.m_Guid;
        isContinue = OperationMeasure.Instance.CheckOperation(msgWithGuid.m_Guid);
        if (!isContinue) {
          JsonMessageWithGuid retMsg = new JsonMessageWithGuid(JsonMessageID.TooManyOperations);
          retMsg.m_Guid = guid;

          JsonMessageDispatcher.SendDcoreMessage(handle, retMsg);
        }
      }
      return isContinue;
    }
    //------------------------------------------------------------------------------------------------------
    private void HandleDirectLogin(JsonMessage msg, int handle, uint seq)
    {
      StringBuilder stringBuilder = new StringBuilder(1024);
      int size = stringBuilder.Capacity;
      CenterClientApi.TargetName(handle, stringBuilder, size);
      string node_name = stringBuilder.ToString();

      var loginMsg = msg as JsonMessageDirectLogin;
      if (loginMsg != null) {
        //这里需要防止客户端不停发消息
        if (m_ServerBridgeThread.CurActionNum > c_MaxWaitLoginUserNum) {
          JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
          replyMsg.m_Account = loginMsg.m_Account;
          replyMsg.m_AccountId = "";
          replyMsg.m_Result = (int)AccountLoginResult.Wait;
          JsonMessageDispatcher.SendDcoreMessage(handle, replyMsg);
        } else {
          m_ServerBridgeThread.QueueAction(m_ServerBridgeThread.DirectLogin, loginMsg.m_Account, loginMsg.m_LoginServerId, node_name);
        }
      }
    }
    private void HandleAccountLogin(JsonMessage msg, int handle, uint seq)
    {
      StringBuilder stringBuilder = new StringBuilder(1024);
      int size = stringBuilder.Capacity;
      CenterClientApi.TargetName(handle, stringBuilder, size);
      string node_name = stringBuilder.ToString();

      var loginMsg = msg as JsonMessageAccountLogin;
      if (loginMsg != null) {
        //这里需要防止客户端不停发消息
        if (m_ServerBridgeThread.CurActionNum > c_MaxWaitLoginUserNum) {
          JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
          replyMsg.m_Account = loginMsg.m_Account;
          replyMsg.m_AccountId = "";
          replyMsg.m_Result = (int)AccountLoginResult.Wait;
          JsonMessageDispatcher.SendDcoreMessage(handle, replyMsg);
        } else {
          m_ServerBridgeThread.QueueAction(m_ServerBridgeThread.AccountLogin,
            loginMsg.m_Account, loginMsg.m_OpCode, loginMsg.m_ChannelId, loginMsg.m_Data, loginMsg.m_LoginServerId, loginMsg.m_ClientGameVersion, loginMsg.m_ClientLoginIp, loginMsg.m_UniqueIdentifier, loginMsg.m_System, loginMsg.m_GameChannelId, node_name);
        }
      }
    }
    private void HandleActivateAccount(JsonMessage msg, int handle, uint seq)
    {
      var activateMsg = msg as JsonMessageActivateAccount;
      if (activateMsg != null) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.OnActivateAccount, activateMsg.m_Account, activateMsg.m_ActivationCode);       
      }
    }
    private void HandleAccountLogout(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageAccountLogout logoutMsg = msg as JsonMessageAccountLogout;
      if (logoutMsg != null) {        
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.OnAccountLogout, logoutMsg.m_Account);
      }
    }
    private void HandleRoleList(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithAccount recvMsg = msg as JsonMessageWithAccount;
      if (null != recvMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.OnRoleList, recvMsg.m_Account);
      }
    }
    private void HandleCreateNickname(JsonMessage msg, int handle, uint seq)
    {
      var recvMsg = msg as JsonMessageCreateNickname;
      if (recvMsg != null) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.OnCreateNickname, recvMsg.m_Account);
      }
    }
    private void HandleCreateRole(JsonMessage msg, int handle, uint seq)
    {
      var recvMsg = msg as JsonMessageCreateRole;
      if (recvMsg != null) {
        m_DataProcessScheduler.DispatchAction(
          m_DataProcessScheduler.OnCreateRole,
          recvMsg.m_Account,
          recvMsg.m_Nickname,
          recvMsg.m_HeroId);
      }
    }
    private void HandleRoleEnter(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithAccount recvMsg = msg as JsonMessageWithAccount;
      if (null != recvMsg) {
        ArkCrossEngineMessage.Msg_CL_RoleEnter protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RoleEnter;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.OnRoleEnter, recvMsg.m_Account, protoData.m_Guid);
        }
      }
    }
    //==========================================================================================================================
    private void HandleZero(JsonMessage msg, int handle, uint seq)
    {
      LogSys.Log(LOG_TYPE.DEBUG, "receive json message:{0}", msg);
      JsonMessageDispatcher.SendDcoreMessage(handle, msg);
    }
    private void HandleUserHeartbeat(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageUserHeartbeat heartbeatMsg = msg as JsonMessageUserHeartbeat;
      if (heartbeatMsg != null) {
        if (null == m_DataProcessScheduler.GetUserInfo(heartbeatMsg.m_Guid)) {
          JsonMessageWithGuid retMsg = new JsonMessageWithGuid(JsonMessageID.KickUser);
          retMsg.m_Guid = heartbeatMsg.m_Guid;
          JsonMessageDispatcher.SendDcoreMessage(handle, retMsg);

          LogSys.Log(LOG_TYPE.DEBUG, "HandleUserHeartbeat, guid:{0} can't found, kick.", heartbeatMsg.m_Guid);

        } else {
          //echo
          JsonMessageDispatcher.SendDcoreMessage(handle, heartbeatMsg);
          //逻辑处理
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.DoUserHeartbeat, heartbeatMsg.m_Guid);
        }
      }
    }
    private void HandleLogout(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageLogout logoutMsg = msg as JsonMessageLogout;
      if (logoutMsg != null) {
        LogSys.Log(LOG_TYPE.INFO, "User Logout, Guid: {0}", logoutMsg.m_Guid);
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.DoUserLogoff, logoutMsg.m_Guid);
      }
    }
    private void HandleFindTeam(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid findTeamMsg = msg as JsonMessageWithGuid;
      if (null != findTeamMsg) {
        ArkCrossEngineMessage.Msg_CL_RequestMatch protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestMatch;
        if (null != protoData) {
          m_MatchFormThread.QueueAction(m_MatchFormThread.RequestMatch, findTeamMsg.m_Guid, protoData.m_SceneType);
        }
      }
    }
    private void HandleCancelFindTeam(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid cancelFindTeamMsg = msg as JsonMessageWithGuid;
      if (null != cancelFindTeamMsg) {
        ArkCrossEngineMessage.Msg_CL_CancelMatch protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_CancelMatch;
        if (null != protoData) {
          m_MatchFormThread.QueueAction(m_MatchFormThread.CancelRequestMatch, cancelFindTeamMsg.m_Guid, protoData.m_SceneType);
        }
      }
    }
    private void HandleStartGame(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageStartGame startGameMsg = msg as JsonMessageStartGame;
      if (startGameMsg != null) {
        m_RoomProcessThread.QueueAction(m_RoomProcessThread.RequestStartGame, startGameMsg.m_Guid);
      }
    }
    private void HandleQuitRoom(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid quitClientMsg = msg as JsonMessageWithGuid;
      if (null != quitClientMsg) {
        ArkCrossEngineMessage.Msg_CL_QuitRoom protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_QuitRoom;
        if (null != protoData) {
          m_RoomProcessThread.QueueAction(m_RoomProcessThread.QuitRoom, quitClientMsg.m_Guid, protoData.m_IsQuitRoom);
        }
      }
    }
    private void HandleNodeJsRegister(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageNodeJsRegister nodeJsRegisterMsg = msg as JsonMessageNodeJsRegister;
      if (nodeJsRegisterMsg != null) {
        m_RoomProcessThread.QueueAction(m_RoomProcessThread.RegisterNodeJs, new NodeInfo { NodeName = nodeJsRegisterMsg.m_Name });
      }
    }
    private void HandleAddFriend(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid addFriendMsg = msg as JsonMessageWithGuid;
      if (null != addFriendMsg) {
        ArkCrossEngineMessage.Msg_CL_AddFriend protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_AddFriend;
        if (null != protoData) {
          if (protoData.m_TargetGuid > 0) {
            m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.AddFriendByGuid, addFriendMsg.m_Guid, protoData.m_TargetGuid);
          } else {
            m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.AddFriend, addFriendMsg.m_Guid, protoData.m_TargetNick);
          }
        }
      }
    }
    private void HandleConfirmFriend(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid conFriendMsg = msg as JsonMessageWithGuid;
      if (null != conFriendMsg) {
        ArkCrossEngineMessage.Msg_CL_ConfirmFriend protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ConfirmFriend;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.ConfirmFriend, conFriendMsg.m_Guid, protoData.m_TargetGuid);
        }
      }
    }
    private void HandleDelFriend(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid delFriendMsg = msg as JsonMessageWithGuid;
      if (null != delFriendMsg) {
        ArkCrossEngineMessage.Msg_CL_DeleteFriend protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_DeleteFriend;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.DelFriend, delFriendMsg.m_Guid, protoData.m_TargetGuid);
        }
      }
    }
    private void HandleFriendList(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid friendListMsg = msg as JsonMessageWithGuid;
      if (null != friendListMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.FriendList, friendListMsg.m_Guid);
      }
    }
    private void HandleQueryFriendInfo(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid qfiMsg = msg as JsonMessageWithGuid;
      if (null != qfiMsg) {
        ArkCrossEngineMessage.Msg_CL_QueryFriendInfo protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_QueryFriendInfo;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleQueryFriendInfo,
            qfiMsg.m_Guid, (QueryType)protoData.m_QueryType, protoData.m_TargetName,
            protoData.m_TargetLevel, protoData.m_TargetScore, protoData.m_TargetFortune, (GenderType)protoData.m_TargetGender);
        }
      }
    }
    private void HandlePinviteTeam(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid ptMsg = msg as JsonMessageWithGuid;
      if (null != ptMsg) {
        ArkCrossEngineMessage.Msg_CL_PinviteTeam protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_PinviteTeam;
        if (null != protoData) {
          if (protoData.m_FirstGuid > 0 && protoData.m_SecondGuid > 0) {
            m_MatchFormThread.QueueAction(m_MatchFormThread.PinviteTeamByGuid, protoData.m_FirstGuid, protoData.m_SecondGuid);
          } else {
            m_MatchFormThread.QueueAction(m_MatchFormThread.PinviteTeam, protoData.m_FirstNick, protoData.m_SecondNick);
          }
        }
      }
    }
    private void HandleRequestJoinGroup(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid rjgMsg = msg as JsonMessageWithGuid;
      if (null != rjgMsg) {
        ArkCrossEngineMessage.Msg_CL_RequestJoinGroup protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestJoinGroup;
        if (null != protoData) {
          m_MatchFormThread.QueueAction(m_MatchFormThread.RequestJoinGroup, protoData.m_InviteeGuid, protoData.m_GroupNick);
        }
      }
    }
    private void HandleConfirmJoinGroup(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid rjgMsg = msg as JsonMessageWithGuid;
      if (null != rjgMsg) {
        ArkCrossEngineMessage.Msg_CL_ConfirmJoinGroup protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ConfirmJoinGroup;
        if (null != protoData) {
          m_MatchFormThread.QueueAction(m_MatchFormThread.ConfirmJoinGroup, protoData.m_InviteeGuid, protoData.m_GroupNick);
        }
      }
    }
    private void HandleQuitGroup(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid qgMsg = msg as JsonMessageWithGuid;
      if (null != qgMsg) {
        ArkCrossEngineMessage.Msg_CL_QuitGroup protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_QuitGroup;
        if (null != protoData) {
          m_MatchFormThread.QueueAction(m_MatchFormThread.QuitGroup, qgMsg.m_Guid, protoData.m_DropoutNick);
        }
      }
    }
    private void HandleRequestGroupInfo(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid rgiMsg = msg as JsonMessageWithGuid;
      if (null != rgiMsg) {
        m_MatchFormThread.QueueAction(m_MatchFormThread.HandleRequestGroupInfo, rgiMsg.m_Guid);
      }
    }
    private void HandleRefuseGroupRequest(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid rgiMsg = msg as JsonMessageWithGuid;
      if (null != rgiMsg) {
        ArkCrossEngineMessage.Msg_CL_RefuseGroupRequest protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RefuseGroupRequest;
        if (null != protoData) {
          m_MatchFormThread.QueueAction(m_MatchFormThread.HandleRefuseGroupRequest, rgiMsg.m_Guid, protoData.m_RequesterGuid);
        }
      }
    }
    private void HandleSinglePVE(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageSinglePVE singleMsg = msg as JsonMessageSinglePVE;
      if (singleMsg != null) {
        m_RoomProcessThread.QueueAction(m_RoomProcessThread.RequestSinglePVE, singleMsg.m_Guid, singleMsg.m_SceneType);
      }
    }
    private void HandleDiscardItem(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid discardItemMsg = msg as JsonMessageWithGuid;
      if (null != discardItemMsg) {
        ArkCrossEngineMessage.Msg_CL_DiscardItem protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_DiscardItem;
        if (null != protoData) {
          if (protoData.m_ItemId.Count == protoData.m_PropertyId.Count
            && protoData.m_ItemId.Count > 0 && protoData.m_PropertyId.Count > 0) {
              m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleDiscardItem, discardItemMsg.m_Guid, protoData.m_ItemId, protoData.m_PropertyId);
          }
        }
      }
    }
    private void HandleMountEquipment(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid mountEquipmentMsg = msg as JsonMessageWithGuid;
      if (null != mountEquipmentMsg) {
        ArkCrossEngineMessage.Msg_CL_MountEquipment protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_MountEquipment;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleMountEquipment, mountEquipmentMsg.m_Guid, protoData.m_ItemID, protoData.m_PropertyID, protoData.m_EquipPos);
        }
      }
    }
    private void HandleUnmountEquipment(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid unmountEquipmentMsg = msg as JsonMessageWithGuid;
      if (null != unmountEquipmentMsg) {
        ArkCrossEngineMessage.Msg_CL_UnmountEquipment protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UnmountEquipment;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUnmountEquipment, unmountEquipmentMsg.m_Guid, protoData.m_EquipPos);
        }
      }
    }
    private void HandleMountSkill(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid mountSkillMsg = msg as JsonMessageWithGuid;
      if (null != mountSkillMsg) {
        ArkCrossEngineMessage.Msg_CL_MountSkill protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_MountSkill;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleMountSkill, mountSkillMsg.m_Guid, protoData.m_PresetIndex, protoData.m_SkillID, protoData.m_SlotPos);
        }
      }
    }
    private void HandleUnmountSkill(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid unmountSkillMsg = msg as JsonMessageWithGuid;
      if (null != unmountSkillMsg) {
        ArkCrossEngineMessage.Msg_CL_UnmountSkill protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UnmountSkill;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUnmountSkill, unmountSkillMsg.m_Guid, protoData.m_PresetIndex, protoData.m_SlotPos);
        }
      }
    }
    private void HandleUpgradeSkill(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid upgradeSkillMsg = msg as JsonMessageWithGuid;
      if (null != upgradeSkillMsg) {
        ArkCrossEngineMessage.Msg_CL_UpgradeSkill protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UpgradeSkill;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUpgradeSkill, upgradeSkillMsg.m_Guid, protoData.m_PresetIndex, protoData.m_SkillID, protoData.m_AllowCostGold);
        }
      }
    }
    private void HandleUnlockSkill(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid unlockSkillMsg = msg as JsonMessageWithGuid;
      if (null != unlockSkillMsg) {
        ArkCrossEngineMessage.Msg_CL_UnlockSkill protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UnlockSkill;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUnlockSkill, unlockSkillMsg.m_Guid, protoData.m_PresetIndex, protoData.m_SkillID, protoData.m_UserLevel);
        }
      }
    }
    private void HandleSwapSkill(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid swapSkillMsg = msg as JsonMessageWithGuid;
      if (null != swapSkillMsg) {
        ArkCrossEngineMessage.Msg_CL_SwapSkill protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_SwapSkill;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleSwapSkill, swapSkillMsg.m_Guid, protoData.m_PresetIndex, protoData.m_SkillID, protoData.m_SourcePos, protoData.m_TargetPos);
        }
      }
    }
    private void HandleUpgradeItem(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid upgradeItemMsg = msg as JsonMessageWithGuid;
      if (null != upgradeItemMsg) {
        ArkCrossEngineMessage.Msg_CL_UpgradeItem protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UpgradeItem;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUpgradeItem, upgradeItemMsg.m_Guid, protoData.m_Position, protoData.m_ItemId, protoData.m_AllowCostGold);
        }
      }
    }
    private void HandleSaveSkillPreset(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageSaveSkillPreset saveSkillPresetMsg = msg as JsonMessageSaveSkillPreset;
      if (saveSkillPresetMsg != null) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleSaveSkillPreset, saveSkillPresetMsg.m_Guid, saveSkillPresetMsg.m_SelectedPresetIndex);
      }
    }
    private void HandleStageClear(JsonMessage msg, int handle, uint seq) 
    {
      JsonMessageWithGuid stageClearMsg = msg as JsonMessageWithGuid;
      if (null != stageClearMsg) {
        ArkCrossEngineMessage.Msg_CL_StageClear protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_StageClear;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleStageClear, stageClearMsg.m_Guid, protoData.m_HitCount, protoData.m_MaxMultHitCount, protoData.m_Hp, protoData.m_Mp, protoData.m_Gold, protoData.m_MatchKey);
        }
      }
    }
    private void HandleSweepStage(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid sweepStageMsg = msg as JsonMessageWithGuid;
      if (null != sweepStageMsg) {
        ArkCrossEngineMessage.Msg_CL_SweepStage protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_SweepStage;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleSweepStage, sweepStageMsg.m_Guid, protoData.m_SceneId,
            protoData.m_SweepTime);
        }
      }
    }
    private void HandleLiftSkill(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid liftSkillMsg = msg as JsonMessageWithGuid;
      if (null != liftSkillMsg) {
        ArkCrossEngineMessage.Msg_CL_LiftSkill protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_LiftSkill;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleLiftSkill, liftSkillMsg.m_Guid, protoData.m_SkillId);
        }
      }
    }
    private void HandleBuyStamina(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid buyStaminaMsg = msg as JsonMessageWithGuid;
      if (null != buyStaminaMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleBuyStamina, buyStaminaMsg.m_Guid);
      }
    }
    private void HandleFinishMission(JsonMessage msg, int handle, uint seq) 
    {
      JsonMessageWithGuid finishMissionMsg = msg as JsonMessageWithGuid;
      if (null != finishMissionMsg) {
        ArkCrossEngineMessage.Msg_CL_FinishMission protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_FinishMission;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleFinishMission, finishMissionMsg.m_Guid, protoData.m_MissionId);
        }
      }
    }
    private void HandleBuyLife(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid buyLifeMsg = msg as JsonMessageWithGuid;
      if (null != buyLifeMsg) {
        m_RoomProcessThread.QueueAction(m_RoomProcessThread.HandleBuyLife, buyLifeMsg.m_Guid);
      }
    }
    private void HandleUnlockLegacy(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid unlockLegacyMsg = msg as JsonMessageWithGuid;
      if (null != unlockLegacyMsg) {
        ArkCrossEngineMessage.Msg_CL_UnlockLegacy protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UnlockLegacy;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUnlockLegacy, unlockLegacyMsg.m_Guid, protoData.m_Index, protoData.m_ItemID);
        }
      }
    }
    private void HandleUpgradeLegacy(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid upgradeLegacyMsg = msg as JsonMessageWithGuid;
      if (null != upgradeLegacyMsg) {
        ArkCrossEngineMessage.Msg_CL_UpgradeLegacy protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UpgradeLegacy;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUpgradeLegacy, upgradeLegacyMsg.m_Guid, protoData.m_Index, protoData.m_ItemID, protoData.m_AllowCostGold);
        }
      }
    }
    private void HandleAddXSoulExperience(JsonMessage msg, int handle, uint seq)
    {

      JsonMessageWithGuid addXSoulExperienceMsg = msg as JsonMessageWithGuid;
      if (null != addXSoulExperienceMsg) {
        ArkCrossEngineMessage.Msg_CL_AddXSoulExperience protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_AddXSoulExperience;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleAddXSoulExperience, 
                                                addXSoulExperienceMsg.m_Guid, protoData.m_XSoulPart, 
                                                protoData.m_UseItemId, protoData.m_ItemNum);
        }
      }
    }
    private void HandleXSoulChangeShowModel(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid xsoulChangeShowModelMsg = msg as JsonMessageWithGuid;
      if (null != xsoulChangeShowModelMsg) {
        ArkCrossEngineMessage.Msg_CL_XSoulChangeShowModel protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_XSoulChangeShowModel;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleXSoulChangeShowModel,
                                                xsoulChangeShowModelMsg.m_Guid, protoData.m_XSoulPart,
                                                protoData.m_ModelLevel);
        }
      }
    }
    private void HandleUpdateFightingScore(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid updateFightingScoreMsg = msg as JsonMessageWithGuid;
      if (null != updateFightingScoreMsg) {
        ArkCrossEngineMessage.Msg_CL_UpdateFightingScore protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UpdateFightingScore;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUpdateFightingScore, updateFightingScoreMsg.m_Guid, protoData.score);
        }
      }
    }
    private void HandleGetMailList(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageGetMailList getMailListMsg = msg as JsonMessageGetMailList;
      if (null != getMailListMsg) {
        m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.GetMailList, getMailListMsg.m_Guid);        
      }
    }
    private void HandleReadMail(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid readMailMsg = msg as JsonMessageWithGuid;
      if (null != readMailMsg) {
        ArkCrossEngineMessage.Msg_CL_ReadMail protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ReadMail;
        if (null != protoData) {
          m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.ReadMail, readMailMsg.m_Guid, protoData.m_MailGuid);
        }
      }
    }
    private void HandleReceiveMail(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid receiveMailMsg = msg as JsonMessageWithGuid;
      if (null != receiveMailMsg) {
        ArkCrossEngineMessage.Msg_CL_ReceiveMail protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ReceiveMail;
        if (null != protoData) {
          m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.ReceiveMail, receiveMailMsg.m_Guid, protoData.m_MailGuid);
        }
      }
    }
	  private void HandleExpeditionReset(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid expeditionResetMsg = msg as JsonMessageWithGuid;
      if (null != expeditionResetMsg) {
        ArkCrossEngineMessage.Msg_CL_ExpeditionReset protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ExpeditionReset;
        if (null != protoData) {
          m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleRequestExpeditionInfo, expeditionResetMsg.m_Guid, protoData.m_Hp, protoData.m_Mp, protoData.m_Rage, protoData.m_RequestNum, protoData.m_IsReset, protoData.m_AllowCostGold, protoData.m_Timestamp);
        }
      }
    }
    private void HandleRequestExpedition(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid requestExpeditionMsg = msg as JsonMessageWithGuid;
      if (null != requestExpeditionMsg) {
        ArkCrossEngineMessage.Msg_CL_RequestExpedition protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestExpedition;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleRequestExpedition, requestExpeditionMsg.m_Guid, protoData.m_SceneId, protoData.m_TollgateNum);
        }
      }
    }
    private void HandleFinishExpedition(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid finishExpeditionMsg = msg as JsonMessageWithGuid;
      if (null != finishExpeditionMsg) {
        ArkCrossEngineMessage.Msg_CL_FinishExpedition protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_FinishExpedition;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleFinishExpedition, finishExpeditionMsg.m_Guid, protoData.m_SceneId, protoData.m_TollgateNum, protoData.m_Hp, protoData.m_Mp, protoData.m_Rage);
        }
      }
    }
    private void HandleExpeditionAward(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid expeditionAwardMsg = msg as JsonMessageWithGuid;
      if (null != expeditionAwardMsg) {
        ArkCrossEngineMessage.Msg_CL_ExpeditionAward protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ExpeditionAward;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleExpeditionAward, expeditionAwardMsg.m_Guid, protoData.m_TollgateNum);
        }
      }
    }
    private void HandleGetGowStarList(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid getGowStarListMsg = msg as JsonMessageWithGuid;
      if (null != getGowStarListMsg) {
        ArkCrossEngineMessage.Msg_CL_GetGowStarList protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_GetGowStarList;
        if (null != protoData) {
          m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.GetGowStarList, getGowStarListMsg.m_Guid, protoData.m_Start, protoData.m_Count);
        }
      }
    }
    private void HandleQueryExpeditionInfo(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageQueryExpeditionInfo queryExpeditionInfoMsg = msg as JsonMessageQueryExpeditionInfo;
      if (null != queryExpeditionInfoMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleQueryExpeditionInfo, queryExpeditionInfoMsg.m_Guid);
      }
    }
    private void HandleExpeditionFailure(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid expeditionFailureMsg = msg as JsonMessageWithGuid;
      if (null != expeditionFailureMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleExpeditionFailure, expeditionFailureMsg.m_Guid);
      }
    }
    private void HandleMidasTouch(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid midasTouchMsg = msg as JsonMessageWithGuid;
      if (null != midasTouchMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleMidasTouch, midasTouchMsg.m_Guid);
      }
    }
    private void HandleExchangeGoods(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid exchangeGoodsMsg = msg as JsonMessageWithGuid;
      if (null != exchangeGoodsMsg) {
        ArkCrossEngineMessage.Msg_CL_ExchangeGoods protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ExchangeGoods;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleExchangeGoods, exchangeGoodsMsg.m_Guid, protoData.m_ExchangeId);
        }
      }
    }
    private void HandleRequestRefreshExchange(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid requestMsg = msg as JsonMessageWithGuid;
      if (null != requestMsg) {
        ArkCrossEngineMessage.Msg_CL_RequestRefreshExchange protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestRefreshExchange;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleRequestRefreshExchange, requestMsg.m_Guid, protoData.m_RequestRefresh, protoData.m_CurrencyId);
        }
      }
    }
    private void HandleSelectPartner(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid selectPartnerMsg = msg as JsonMessageWithGuid;
      if (null != selectPartnerMsg) {
        ArkCrossEngineMessage.Msg_CL_SelectPartner protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_SelectPartner;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleSelectpartner, selectPartnerMsg.m_Guid, protoData.m_PartnerId);
        }
      }
    }
    private void HandleUpgradePartnerLevel(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid upgradePartnerLevelMsg = msg as JsonMessageWithGuid;
      if (null != upgradePartnerLevelMsg) {
        ArkCrossEngineMessage.Msg_CL_UpgradePartnerLevel protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UpgradePartnerLevel;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUpgradePartnerLevel, upgradePartnerLevelMsg.m_Guid, protoData.m_PartnerId);
        }
      }
    }
    private void HandleUpgradePartnerStage(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid upgradePartnerStageMsg = msg as JsonMessageWithGuid;
      if (null != upgradePartnerStageMsg) {
        ArkCrossEngineMessage.Msg_CL_UpgradePartnerStage protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UpgradePartnerStage;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUpgradePartnerStage, upgradePartnerStageMsg.m_Guid, protoData.m_PartnerId);
        }
      }
    }
    private void HandleCompoundPartner(JsonMessage msg, int hanle, uint seq)
    {
      JsonMessageWithGuid compoundPartnerMsg = msg as JsonMessageWithGuid;
      if (null != compoundPartnerMsg) {
        ArkCrossEngineMessage.Msg_CL_CompoundPartner protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_CompoundPartner;
        if(null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleCompoundPartner, compoundPartnerMsg.m_Guid, protoData.m_PartnerId);
        }
      }
    }
    private void HandleStartMpve(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid startMpveMsg = msg as JsonMessageWithGuid;
      if (null != startMpveMsg) {
        ArkCrossEngineMessage.Msg_CL_StartMpve protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_StartMpve;
        if (null != protoData) {
          m_MatchFormThread.QueueAction(m_MatchFormThread.StartMpve, startMpveMsg.m_Guid, protoData.m_SceneType);
        }
      }
    }
    private void HandleQuitPve(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid quitPveMsg = msg as JsonMessageWithGuid;
      if (null != quitPveMsg) {
        m_RoomProcessThread.QueueAction(m_RoomProcessThread.QuitPve, quitPveMsg.m_Guid);
      }
    }
    private void HandleMpveAward(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid mpveAwardMsg = msg as JsonMessageWithGuid;
      if (null != mpveAwardMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleMpveAward, mpveAwardMsg.m_Guid);
      }
    }
    private void HandleUpdatePosition(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid updatePosMsg = msg as JsonMessageWithGuid;
      if (null != updatePosMsg) {
        ArkCrossEngineMessage.Msg_CL_UpdatePosition protoMsg = updatePosMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UpdatePosition;
        if (null != protoMsg) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleUpdatePosition, updatePosMsg.m_Guid, protoMsg.m_X, protoMsg.m_Z, protoMsg.m_FaceDir);
        }
      }
    }
    private void HandleRequestUsers(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid requestUsersMsg = msg as JsonMessageWithGuid;
      if (null != requestUsersMsg) {
        ArkCrossEngineMessage.Msg_CL_RequestUsers protoMsg = requestUsersMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestUsers;
        if (null != protoMsg) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleRequestUsers, requestUsersMsg.m_Guid, protoMsg.m_Count, protoMsg.m_AlreadyExists);
        }
      }
    }
    private void HandleRequestUserPosition(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid requestUserPositionMsg = msg as JsonMessageWithGuid;
      if (null != requestUserPositionMsg) {
        ArkCrossEngineMessage.Msg_CL_RequestUserPosition protoMsg = requestUserPositionMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestUserPosition;
        if (null != protoMsg) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleRequestUserPosition, requestUserPositionMsg.m_Guid, protoMsg.m_User);
        }
      }
    }
    private void HandleChangeCityScene(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid changeCitySceneMsg = msg as JsonMessageWithGuid;
      if (null != changeCitySceneMsg) {
        ArkCrossEngineMessage.Msg_CL_ChangeCityScene protoMsg = changeCitySceneMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ChangeCityScene;
        if (null != protoMsg) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleChangeCityScene, changeCitySceneMsg.m_Guid, protoMsg.m_SceneId);
        }
      }
    }
    private void HandleRequestPlayerInfo(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid rpiMsg = msg as JsonMessageWithGuid;
      if (null != rpiMsg) {
        ArkCrossEngineMessage.Msg_CL_RequestPlayerInfo protoMsg = rpiMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestPlayerInfo;
        if (null != protoMsg) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleRequestPlayerInfo, rpiMsg.m_Guid, protoMsg.m_Nick);
        }
      }
    }
    private void HandleRequestVigor(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid rvMsg = msg as JsonMessageWithGuid;
      if (null != rvMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleRequestVigor, rvMsg.m_Guid);
      }
    }
    private void HandleSetNewbieFlag(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid snfMsg = msg as JsonMessageWithGuid;
      if (null != snfMsg) {
        ArkCrossEngineMessage.Msg_CL_SetNewbieFlag protoMsg = snfMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_SetNewbieFlag;
        if (null != protoMsg) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleSetNewbieFlag, snfMsg.m_Guid, protoMsg.m_Bit, protoMsg.m_Num);
        }
      }
    }
    private void HandleSetNewbieActionFlag(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid snfMsg = msg as JsonMessageWithGuid;
      if (null != snfMsg) {
        ArkCrossEngineMessage.Msg_CL_SetNewbieActionFlag protoMsg = snfMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_SetNewbieActionFlag;
        if (null != protoMsg) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleSetNewbieActionFlag, snfMsg.m_Guid, protoMsg.m_Bit, protoMsg.m_Num);
        }
      }
    }
    private void HandleSignInAndGetReward(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid signInMsg = msg as JsonMessageWithGuid;
      if (null != signInMsg) {
        ArkCrossEngineMessage.Msg_CL_SignInAndGetReward protoData = signInMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_SignInAndGetReward;
        if (null != protoData) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleSignInAndGetReward, protoData.m_Guid);
        }
      }
    }
    private void HandleWeeklyLoginReward(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid loginMsg = msg as JsonMessageWithGuid;
      if (null != loginMsg) {
        m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleWeeklyLoginReward, loginMsg.m_Guid);
      }
    }
    private void HandleGetQueueingCount(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithAccount queueingMsg = msg as JsonMessageWithAccount;
      if (null != queueingMsg) {
        int num = m_QueueingThread.GetQueueingNum(queueingMsg.m_Account);
        JsonMessageWithAccount retMsg = new JsonMessageWithAccount(JsonMessageID.QueueingCountResult);
        retMsg.m_Account = queueingMsg.m_Account;

        ArkCrossEngineMessage.Msg_LC_QueueingCountResult protoMsg = new ArkCrossEngineMessage.Msg_LC_QueueingCountResult();
        protoMsg.m_QueueingCount = num;

        retMsg.m_ProtoData = protoMsg;
        JsonMessageDispatcher.SendDcoreMessage(handle, retMsg);
      }
    }
    private void HandleExchangeGift(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_ExchangeGift protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ExchangeGift;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleExchangeGift, guid, protoData.m_GiftCode);
          }
        }
      }
    }
    private void HandleRecordNewbieFlag(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid rnMsg = msg as JsonMessageWithGuid;
      if (null != rnMsg) {
        ulong guid = rnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_RecordNewbieFlag protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RecordNewbieFlag;
          if (null != protoData) {
            m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleRecordNewbieFlag, rnMsg.m_Guid, protoData.m_Bit);
          }
        }
      }
    }
    private void HandleUploadFPS(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid uMsg = msg as JsonMessageWithGuid;
      if (null != uMsg) {
        ulong guid = uMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_UploadFPS protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_UploadFPS;
          if (null != protoData) {
            LogSys.Log(LOG_TYPE.INFO, "FPS ||| guid:{0}, accountid:{1}, nickname:{2}, hardwarename:{3}, fps:{4}",
            guid, user.AccountId, user.Nickname, protoData.m_Nickname, protoData.m_Fps);
          }
        }
      }
    }
    private void HandleRequestDare(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid rdMsg = msg as JsonMessageWithGuid;
      if (null != rdMsg) {
        ulong guid = rdMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_RequestDare protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestDare;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleRequestDare, guid, protoData.m_TargetNickname);
          }
        }
      }
    }
    private void HandleAcceptedDare(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid adMsg = msg as JsonMessageWithGuid;
      if (null != adMsg) {
        ulong guid = adMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_AcceptedDare protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_AcceptedDare;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleAcceptedDare, guid, protoData.m_ChallengerNickname);
          }
        }
      }
    }
    private void HandleRequestDareByGuid(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid adMsg = msg as JsonMessageWithGuid;
      if (null != adMsg) {
        ulong guid = adMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_RequestDareByGuid protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_RequestDareByGuid;
          if (null != protoData) {
            UserInfo target = m_DataProcessScheduler.GetUserInfo(protoData.m_TargetGuid);
            if (null != target) {
              m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleAcceptedDare, guid, target.Nickname);
            }
          }
        }
      }
    }
    private void HandleRequestGowPrize(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid adMsg = msg as JsonMessageWithGuid;
      if (null != adMsg) {
        ulong guid = adMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleRequestGowPrize, guid);
        }
      }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //************************************************************需要GM权限的消息处理******************************************************************************************
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void HandleAddAssets(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid addAssetsMsg = msg as JsonMessageWithGuid;
      if (null != addAssetsMsg) {
        ArkCrossEngineMessage.Msg_CL_AddAssets protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_AddAssets;
        if (null != protoData) {          
          ulong guid = addAssetsMsg.m_Guid;
          UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
          if (null != user && user.CanUseGmCommand) {
            m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleAddAssets, addAssetsMsg.m_Guid, protoData.m_Money, protoData.m_Gold, protoData.m_Exp, protoData.m_Stamina);
            m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.UnlockCountLimit, addAssetsMsg.m_Guid);
          }
        }
      }
    }
    private void HandleAddItem(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageAddItem addItemMsg = msg as JsonMessageAddItem;
      if (null != addItemMsg) {
        ulong guid = addItemMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleAddItem, addItemMsg.m_Guid, addItemMsg.m_ItemId, addItemMsg.m_ItemNum);
        }
      }
    }
    private void HandleSendMail(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageSendMail sendMailMsg = msg as JsonMessageSendMail;
      if (null != sendMailMsg) {
        ulong guid = sendMailMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          MailInfo mail_info = new MailInfo();
          mail_info.m_Title = sendMailMsg.m_Title;
          mail_info.m_Text = sendMailMsg.m_Text;
          mail_info.m_LevelDemand = sendMailMsg.m_LevelDemand;
          mail_info.m_Money = sendMailMsg.m_Money;
          mail_info.m_Gold = sendMailMsg.m_Gold;
          mail_info.m_Stamina = sendMailMsg.m_Stamina;
          mail_info.m_Items = new System.Collections.Generic.List<MailItem>();
          if (sendMailMsg.m_ItemId > 0) {
            MailItem mail_item = new MailItem();
            mail_item.m_ItemId = sendMailMsg.m_ItemId;
            mail_item.m_ItemNum = sendMailMsg.m_ItemNum;
            mail_info.m_Items.Add(mail_item);
          }
          if (null != sendMailMsg.m_Receiver && sendMailMsg.m_Receiver.Length > 0) {
            ulong receiver = m_DataProcessScheduler.GetGuidByNickname(sendMailMsg.m_Receiver);
            if (receiver > 0) {
              mail_info.m_Receiver = receiver;
              m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.SendUserMail, mail_info, sendMailMsg.m_ExpiryDate);
            }
          } else {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.SendWholeMail, mail_info, sendMailMsg.m_ExpiryDate);
          }
        }
      }
    }
    private void HandleGMResetDailyMissions(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageGMResetDailyMissions gm_msg = msg as JsonMessageGMResetDailyMissions;
      if (null != gm_msg) {
        ulong guid = gm_msg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleResetDailyMissions, gm_msg.m_Guid);
        }
      }
    }
    private void HandlePublishNotice(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          ArkCrossEngineMessage.Msg_CL_PublishNotice protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_PublishNotice;
          if (null != protoData) {
            m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandlePublishNotice, protoData.m_Content, protoData.m_RollNum);
          }
        }
      }
    }

    private void HandleQueryArenaInfo(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user) {
          ArkCrossEngineMessage.Msg_CL_QueryArenaInfo protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_QueryArenaInfo;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleQueryArenaInfo, guid);
          }
        }
      }
    }

    private void HandleQueryArenaMatchGroup(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user) {
          ArkCrossEngineMessage.Msg_CL_QueryArenaMatchGroup protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_QueryArenaMatchGroup;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleQueryArenaMatchGroup, guid);
          }
        }
      }
    }

    private void HandleArenaStartChallenge(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_ArenaStartChallenge protoData= msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ArenaStartChallenge;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleArenaStartChallenge, guid, protoData.m_TargetGuid);
          }
        }
      }
    }

    private void HandleArenaChallengeOver(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_ArenaChallengeOver protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ArenaChallengeOver;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleArenaChallengeOver, guid, 
              protoData);
          }
        }
      }
    }

    private void HandleArenaQueryRank(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_ArenaQueryRank protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ArenaQueryRank;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleArenaQueryRank, guid, handle);
          }
        }
      }
    }

    private void HandleArenaChangePartners(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_ArenaChangePartner protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ArenaChangePartner;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleArenaChangePartners, guid, handle, protoData.Partners);
          }
        }
      }
    }

    private void HandleArenaQueryHistory(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_ArenaQueryHistory protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ArenaQueryHistory;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleArenaQueryHistory, guid, handle);
          }
        }
      }
    }

    private void HandleArenaBuyFightCount(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_ArenaBuyFightCount protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ArenaBuyFightCount;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleArenaBuyFightCount, guid, handle);
          }
        }
      }
    }

    private void HandleArenaBeginFight(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid pnMsg = msg as JsonMessageWithGuid;
      if (null != pnMsg) {
        ulong guid = pnMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_ArenaBeginFight protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_ArenaBeginFight;
          if (null != protoData) {
            m_GlobalDataProcessThread.QueueAction(m_GlobalDataProcessThread.HandleArenaBeginFight, guid, handle);
          }
        }
      }
    }

    private void HandleCompoundEquip(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid ceMsg = msg as JsonMessageWithGuid;
      if (null != ceMsg) {
        ulong guid = ceMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (user != null) {
          ArkCrossEngineMessage.Msg_CL_CompoundEquip protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_CompoundEquip;
          if (null != protoData) {
            m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.HandleCompoundEquip, guid, protoData.m_PartId);
          }
        }
      }
    }

    private void HandleGmKickUser(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid kickMsg = msg as JsonMessageWithGuid;
      if (null != kickMsg) {
        ulong guid = kickMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          ArkCrossEngineMessage.Msg_CL_GmKickUser protoMsg = kickMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_GmKickUser;
          if (null != protoMsg) {
            ulong kickGuid = m_DataProcessScheduler.GetGuidByNickname(protoMsg.m_Nick);
            UserInfo kickUser = m_DataProcessScheduler.GetUserInfo(kickGuid);
            if (null != kickUser) {
              JsonMessageWithGuid retMsg = new JsonMessageWithGuid(JsonMessageID.KickUser);
              retMsg.m_Guid = kickGuid;
              JsonMessageDispatcher.SendDcoreMessage(kickUser.NodeName, retMsg);

              JsonMessageWithGuid kickResultMsg = new JsonMessageWithGuid(JsonMessageID.GmKickUser);
              kickResultMsg.m_Guid = guid;
              ArkCrossEngineMessage.Msg_LC_GmKickUser protoData = new ArkCrossEngineMessage.Msg_LC_GmKickUser();
              protoData.m_KickedAccountId = kickUser.AccountId;
              JsonMessageDispatcher.SendDcoreMessage(handle, kickResultMsg);

              LogSys.Log(LOG_TYPE.DEBUG, "HandleGmKickUser, user {0} {1}({2}) kicked by {3}.", protoMsg.m_Nick, kickGuid, kickUser.AccountId, guid);

              m_ServerBridgeThread.QueueAction(m_ServerBridgeThread.AddKickUser, kickUser.AccountId, ((long)protoMsg.m_LockMinutes) * 60 * 1000);
            }
          }
        }
      }
    }

    private void HandleGmLockUser(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid lockMsg = msg as JsonMessageWithGuid;
      if (null != lockMsg) {
        ulong guid = lockMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          ArkCrossEngineMessage.Msg_CL_GmLockUser protoMsg = lockMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_GmLockUser;
          if (null != protoMsg) {
            m_DataStoreThread.DSDSaveBanAccount(protoMsg.m_AccountId, true);
          }
        }
      }
    }

    private void HandleGmUnlockUser(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid unlockMsg = msg as JsonMessageWithGuid;
      if (null != unlockMsg) {
        ulong guid = unlockMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          ArkCrossEngineMessage.Msg_CL_GmUnlockUser protoMsg = unlockMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_GmUnlockUser;
          if (null != protoMsg) {
            m_DataStoreThread.DSDSaveBanAccount(protoMsg.m_AccountId, false);
          }
        }
      }
    }

    private void HandleGmAddExp(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid addExpMsg = msg as JsonMessageWithGuid;
      if (null != addExpMsg) {
        ulong guid = addExpMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          int result = 0;
          ArkCrossEngineMessage.Msg_CL_GmAddExp protoMsg = addExpMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_GmAddExp;
          if (null != protoMsg) {
            ulong addExpGuid = m_DataProcessScheduler.GetGuidByNickname(protoMsg.m_Nick);

            m_DataProcessScheduler.DispatchAction(m_DataProcessScheduler.UnlockCountLimit, addExpGuid);

            UserInfo addExpUser = m_DataProcessScheduler.GetUserInfo(addExpGuid);
            if (null != addExpUser) {
              addExpUser.ExpPoints += protoMsg.m_Exp;
              result = 1;
            }
          }
          JsonMessageWithGuid addExpResultMsg = new JsonMessageWithGuid(JsonMessageID.GmAddExp);
          addExpResultMsg.m_Guid = guid;
          ArkCrossEngineMessage.Msg_LC_GmAddExp protoData = new ArkCrossEngineMessage.Msg_LC_GmAddExp();
          protoData.m_Result = result;

          addExpResultMsg.m_ProtoData = protoData;
          JsonMessageDispatcher.SendDcoreMessage(handle, addExpResultMsg);
        }
      }
    }

    private void HandleGmUpdateMaxUserCount(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid updateMaxUserCountMsg = msg as JsonMessageWithGuid;
      if (null != updateMaxUserCountMsg) {
        ulong guid = updateMaxUserCountMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          ArkCrossEngineMessage.Msg_CL_GmUpdateMaxUserCount protoMsg = updateMaxUserCountMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_GmUpdateMaxUserCount;
          if (null != protoMsg) {
            m_QueueingThread.QueueAction(m_QueueingThread.UpdateMaxUserCount, protoMsg.m_MaxUserCount, protoMsg.m_MaxUserCountPerLogicServer, protoMsg.m_MaxQueueingCount);
          }
        }
      }
    }
    private ArkCrossEngineMessage.UserBaseData UserInfoBuilder(UserInfo user)
    {
      if (null == user)
        return null;
      ArkCrossEngineMessage.UserBaseData result = new ArkCrossEngineMessage.UserBaseData();
      result.m_Guid = user.Guid;
      result.m_Account = user.AccountId;
      result.m_LogicServerId = user.LogicServerId;
      result.m_Nickname = user.Nickname;
      result.m_HeroId = user.HeroId;
      result.m_Level = user.Level;
      result.m_Vip = user.Vip;
      result.m_Money = user.Money;
      result.m_Gold = user.Gold;
      result.m_LastLogoutTime = user.LastLogoutTime.ToString();
      return result;
    }
    private void HandleGmQueryInfoByGuidOrNickname(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid queryInfoMsg = msg as JsonMessageWithGuid;
      if (null != queryInfoMsg) {
        ulong guid = queryInfoMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          ArkCrossEngineMessage.Msg_CL_GmQueryInfoByGuidOrNickname protoMsg = queryInfoMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_GmQueryInfoByGuidOrNickname;
          if (null != protoMsg) {
            string target_nick_name = protoMsg.m_Nickname;
            ulong target_id = target_nick_name.Length > 0 ? m_DataProcessScheduler.GetGuidByNickname(target_nick_name) : protoMsg.m_Guid;
            UserInfo target = m_DataProcessScheduler.GetUserInfo(target_id);
            if (null != target) {
              JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.GmQueryInfoByGuidOrNickname);
              resultMsg.m_Guid = guid;
              ArkCrossEngineMessage.Msg_LC_GmQueryInfoByGuidOrNickname protoData = new ArkCrossEngineMessage.Msg_LC_GmQueryInfoByGuidOrNickname();
              protoData.m_Result = 1;
              protoData.m_Info = UserInfoBuilder(target);

              resultMsg.m_ProtoData = protoData;
              JsonMessageDispatcher.SendDcoreMessage(user.NodeName, resultMsg);
            } else {
              if (target_id > 0) {
                m_GmServerThread.QueueAction(m_GmServerThread.GMPQueryUser, guid, target_id, handle);
              } else if (target_nick_name.Length > 0) {
                // todo:
              }
            }
          }
        }
      }
    }
    private void HandleGmQueryInfosByDimNickname(JsonMessage msg, int handle, uint seq)
    {
      JsonMessageWithGuid queryInfoMsg = msg as JsonMessageWithGuid;
      if (null != queryInfoMsg) {
        ulong guid = queryInfoMsg.m_Guid;
        UserInfo user = m_DataProcessScheduler.GetUserInfo(guid);
        if (null != user && user.CanUseGmCommand) {
          ArkCrossEngineMessage.Msg_CL_GmQueryInfosByDimNickname protoMsg = queryInfoMsg.m_ProtoData as ArkCrossEngineMessage.Msg_CL_GmQueryInfosByDimNickname;
          if (null != protoMsg) {
            string target_dim_nick_name = protoMsg.m_DimNickname;
            // m_DataStoreThread.DSQQueryInfoByDimNickname(guid, target_dim_nick_name);
          }
        }
      }
    }
  }
}
