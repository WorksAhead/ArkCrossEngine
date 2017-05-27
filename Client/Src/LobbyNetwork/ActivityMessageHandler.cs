using System;
using LitJson;

namespace ArkCrossEngine.Network
{
    /// <summary>
    /// 活动相关的消息变更频繁，放在单独的文件里
    /// 现在有签到，礼包码。
    /// </summary>
    internal sealed partial class LobbyNetworkSystem
    {
        private void ActivityMessageInit()
        {
            GfxSystem.EventChannelForLogic.Subscribe("ge_signin_and_get_reward", "lobby", SignInAndGetReward);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_exchange_gift", "lobby", ExchangeGift);
            GfxSystem.EventChannelForLogic.Subscribe("ge_get_weekly_login_reward", "lobby", GetWeeklyLoginReward);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_get_online_duration_reward", "lobby", GetOnlineDurationReward);
            ActivityMessageHandler();
        }
        private void ActivityMessageHandler()
        {
            RegisterMsgHandler(JsonMessageID.SignInAndGetRewardResult, typeof(ArkCrossEngineMessage.Msg_LC_SignInAndGetRewardResult), HandleSignInAndGetRewardResult);
            RegisterMsgHandler(JsonMessageID.SyncSignInCount, typeof(ArkCrossEngineMessage.Msg_LC_SyncSignInCount), HandleSyncSignInCount);
            RegisterMsgHandler(JsonMessageID.ExchangeGiftResult, typeof(ArkCrossEngineMessage.Msg_LC_ExchangeGiftResult), HandleExchangeGiftResult);
            RegisterMsgHandler(JsonMessageID.WeeklyLoginRewardResult, typeof(ArkCrossEngineMessage.Msg_LC_WeeklyLoginRewardResult), HandleGetWeeklyRewardResult);
            RegisterMsgHandler(JsonMessageID.ResetWeeklyLoginRewardData, HandleResetWeeklyRewardData);
            RegisterMsgHandler(JsonMessageID.ResetOnlineTimeRewardData, HandleResetDailyOnlineDurationData);
            RegisterMsgHandler(JsonMessageID.GetOnlineTimeRewardResult, typeof(ArkCrossEngineMessage.Msg_LC_GetOnlineTimeRewardResult), HandleGetOnlineTimeRewardResult);
        }

        private void SignInAndGetReward()
        {
            try
            {
                LogSystem.Debug("Try SignIn and GetReward");
                JsonMessage msg = new JsonMessage(JsonMessageID.SignInAndGetReward);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_SignInAndGetReward protoData = new ArkCrossEngineMessage.Msg_CL_SignInAndGetReward();
                protoData.m_Guid = m_Guid;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void GetWeeklyLoginReward()
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.WeeklyLoginReward);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void GetOnlineDurationReward(int index)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.GetOnlineTimeReward);
                msg.m_JsonData.Set("m_Guid", m_Guid);
                ArkCrossEngineMessage.Msg_CL_GetOnlineTimeReward protoData = new ArkCrossEngineMessage.Msg_CL_GetOnlineTimeReward();
                protoData.m_Index = index;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void ExchangeGift(string giftcode)
        {
            try
            {
                JsonMessage msg = new JsonMessage(JsonMessageID.ExchangeGift);
                msg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                ArkCrossEngineMessage.Msg_CL_ExchangeGift protoData = new ArkCrossEngineMessage.Msg_CL_ExchangeGift();
                protoData.m_GiftCode = giftcode;
                msg.m_ProtoData = protoData;
                SendMessage(msg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////
        private void HandleSignInAndGetRewardResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SignInAndGetRewardResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SignInAndGetRewardResult;
            bool rewardResult = false;
            if (null != protoData)
            {
                if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Succeed)
                {
                    // 成功
                    rewardResult = true;
                    LogSystem.Debug("SignIn succeed");
                    RoleInfo roleInfo = LobbyClient.Instance.CurrentRole;
                    if (null != roleInfo)
                    {
                        --roleInfo.RestSignInCount;
                        ++roleInfo.SignInCountCurMonth;
                    }
                }
                else
                {
                    if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Failure_CostError)
                    {
                        // 次数不足
                        LogSystem.Debug("Do not have enough sign in time.");
                    }
                    else if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Failure_Overflow)
                    {
                        LogSystem.Debug("Sign in count can not exceed datetime.");
                        // 签到次数不能超过日期数
                    }
                    else
                    {
                        LogSystem.Debug("Unknonw error.");
                        // 未知错误
                    }
                }
            }
            GfxSystem.PublishGfxEvent("ge_sign_award_result", "ui", rewardResult);
        }
        private void HandleSyncSignInCount(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncSignInCount protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncSignInCount;
            if (null != protoData)
            {
                RoleInfo roleInfo = LobbyClient.Instance.CurrentRole;
                if (null != roleInfo)
                {
                    roleInfo.RestSignInCount = protoData.m_RestSignInCountCurDay;
                    roleInfo.SignInCountCurMonth = protoData.m_SignInCountCurMonth;
                    GfxSystem.PublishGfxEvent("ge_sync_sign_count", "ui");
                }
                LogSystem.Debug("Sync SignIn Count, count = {0}, cur SignInCountCurMonth = {1}", protoData.m_RestSignInCountCurDay, protoData.m_SignInCountCurMonth);
            }
        }
        private void HandleGetWeeklyRewardResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_WeeklyLoginRewardResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_WeeklyLoginRewardResult;
            if (null != protoData)
            {
                if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Succeed)
                {
                    RoleInfo role = LobbyClient.Instance.CurrentRole;
                    if (null != role)
                    {
                        role.IsGetLoginReward = true;
                        int index = WeeklyLoginConfigProvider.Instance.GetTodayIndex();
                        if (!role.WeeklyLoginRewardRecord.Contains(index))
                        {
                            role.WeeklyLoginRewardRecord.Add(index);
                        }
                    }
                    // succeed

                }
                else if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Failure_Time)
                {
                    // 不在活动时间
                }
                else if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Failure_Overflow)
                {
                    // 已经领过
                }
                GfxSystem.PublishGfxEvent("ge_get_weekly_reward", "ui", protoData.m_ResultCode);
            }
            else
            {
                GfxSystem.PublishGfxEvent("ge_get_weekly_reward", "ui", (int)GeneralOperationResult.LC_Failure_Unknown);
            }
        }
        private void HandleResetWeeklyRewardData(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            if (null != jsonData)
            {
                RoleInfo role = LobbyClient.Instance.CurrentRole;
                if (null != role)
                {
                    role.IsGetLoginReward = false;
                }
            }
        }
        private void HandleGetOnlineTimeRewardResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_GetOnlineTimeRewardResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_GetOnlineTimeRewardResult;
            RoleInfo role = LobbyClient.Instance.CurrentRole;
            if (null != protoData)
            {
                if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Succeed)
                {
                    // 成功
                    if (!role.OnLineDurationRewardedIndex.Contains(protoData.m_Index))
                    {
                        role.OnLineDurationRewardedIndex.Add(protoData.m_Index);
                        LogSystem.Debug("GetOnlineTimeRewardResult succed");
                    }
                }
                else if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Failure_Code_Used)
                {
                    LogSystem.Debug("GetOnlineTimeRewardResult alread get");
                    // 已经领取
                }
                else if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Failure_Time)
                {
                    LogSystem.Debug("GetOnlineTimeRewardResult time not OK");
                    // 时间不够
                }
                else
                {
                    // 未知错误
                }
            }
        }
        private void HandleResetDailyOnlineDurationData(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            RoleInfo role = LobbyClient.Instance.CurrentRole;
            if (null != role)
            {
                role.DailyOnLineDuration = 0;
                role.OnLineDurationRewardedIndex.Clear();
                role.OnlineDurationStartTime = DateTime.Now;
            }
        }
        private void HandleExchangeGiftResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ExchangeGiftResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ExchangeGiftResult;
            if (null == protoMsg)
            {
                return;
            }
            int giftId = protoMsg.m_GiftId;
            GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;
            //礼品码兑换结果，通知UI显示提示信息
            //返回结果的含义：
            //GeneralOperationResult.LC_Succeed               //礼品兑换成功
            //GeneralOperationResult.LC_Failure_Unknown       //礼品码兑换失败
            //GeneralOperationResult.LC_Failure_Code_Used     //礼品码已经被使用,无效
            //GeneralOperationResult.LC_Failure_Code_Error    //礼品码错误
            //GeneralOperationResult.LC_Failure_Overflow      //礼品领取次数超过限制      
            GfxSystem.PublishGfxEvent("ge_exchange_gift_result", "ui", result, giftId);
        }
    }
}
