using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal class DareSystem
    {
        internal static int UNLOCK_LEVEL = 1;
        internal static int CHALLENGE_CD_MS = 5000;
        internal GeneralOperationResult ErrorCode = GeneralOperationResult.LC_Succeed;

        internal class DareInfo
        {
            internal ulong offense;
            internal ulong defence;
        }

        internal bool Init()
        {
            return true;
        }
        internal void NotifyRequestDareResult(ulong guid, string nickname, GeneralOperationResult result)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user)
                return;
            JsonMessageWithGuid rdMsg = new JsonMessageWithGuid(JsonMessageID.RequestDareResult);
            rdMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_RequestDareResult protoData = new ArkCrossEngineMessage.Msg_LC_RequestDareResult();
            protoData.m_Nickname = nickname;
            protoData.m_Result = (int)result;
            rdMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, rdMsg);
        }
        internal void NotifyRequestDare(ulong userGuid, string targetNickname)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(userGuid);
            if (null == user)
                return;
            ulong targetGuid = LobbyServer.Instance.DataProcessScheduler.GetGuidByNickname(targetNickname);
            UserInfo target = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(targetGuid);
            if (null == target)
                return;
            long cur_time = TimeUtility.GetServerMilliseconds();
            if (cur_time - user.LastRequestDareTime > CHALLENGE_CD_MS)
            {
                user.LastRequestDareTime = TimeUtility.GetServerMilliseconds();
                JsonMessageWithGuid rdMsg = new JsonMessageWithGuid(JsonMessageID.RequestDare);
                rdMsg.m_Guid = targetGuid;
                ArkCrossEngineMessage.Msg_LC_RequestDare protoData = new ArkCrossEngineMessage.Msg_LC_RequestDare();
                protoData.m_ChallengerNickname = user.Nickname;
                rdMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(target.NodeName, rdMsg);
            }
            else
            {
                NotifyRequestDareResult(userGuid, user.Nickname, GeneralOperationResult.LC_Failure_InCd);
            }
        }
        internal void HandleAcceptedDare(ulong userGuid, string challenger)
        {
            DareInfo info = new DareInfo();
            info.defence = userGuid;
            info.offense = LobbyServer.Instance.DataProcessScheduler.GetGuidByNickname(challenger);
            bool ret = CanStart(info);
            if (ret)
            {
                StartDare(info);
            }
        }
        private bool CanStart(DareInfo info)
        {
            UserInfo offense_user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(info.offense);
            if (null == offense_user)
                return false;
            UserInfo defence_user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(info.defence);
            if (null == defence_user)
                return false;
            GeneralOperationResult ret = GeneralOperationResult.LC_Failure_NotUnLock;
            if (offense_user.Level < UNLOCK_LEVEL)
            {
                NotifyRequestDareResult(offense_user.Guid, offense_user.Nickname, ret);
                NotifyRequestDareResult(defence_user.Guid, offense_user.Nickname, ret);
                return false;
            }
            if (defence_user.Level < UNLOCK_LEVEL)
            {
                NotifyRequestDareResult(offense_user.Guid, defence_user.Nickname, ret);
                NotifyRequestDareResult(defence_user.Guid, defence_user.Nickname, ret);
                return false;
            }
            ret = GeneralOperationResult.LC_Failuer_Busy;
            if (UserState.Online != offense_user.CurrentState)
            {
                NotifyRequestDareResult(defence_user.Guid, offense_user.Nickname, ret);
                return false;
            }
            if (UserState.Online != defence_user.CurrentState)
            {
                NotifyRequestDareResult(offense_user.Guid, defence_user.Nickname, ret);
                return false;
            }
            if (offense_user.Guid == defence_user.Guid)
            {
                return false;
            }
            return true;
        }
        internal bool StartDare(DareInfo info)
        {
            if (null == info)
                return false;
            List<ulong> guidList = new List<ulong>();
            guidList.Add(info.offense);
            guidList.Add(info.defence);
            RoomProcessThread roomProcess = LobbyServer.Instance.RoomProcessThread;
            roomProcess.QueueAction(roomProcess.AllocLobbyRoom, guidList.ToArray(), (int)MatchSceneEnum.Dare);
            return true;
        }
    }
}
