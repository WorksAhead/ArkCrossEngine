using System;
using System.Collections.Generic;
using System.Threading;
using Messenger;
using DashFire.DataStore;
using Lobby_GmServer;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal sealed class GmServerThread : MyServerThread
    {
        internal delegate void GMPLoadAccountCB(DSLoadResult ret, DSP_Account data);
        internal delegate void GMPLoadUserCB(DSLoadResult ret, GMP_User data);

        private bool m_GmSeverAvailable = false;
        private DataStoreClient m_DataStoreClient = null;
        private long m_LastLogTime = 0;

        private void OnConnectGMServer()
        {
        }
        private void ConnectGMServer()
        {
            if (DataStoreClient.ConnectStatus.Connecting == m_DataStoreClient.CurrentStatus
              || DataStoreClient.ConnectStatus.Connected == m_DataStoreClient.CurrentStatus)
            {
                return;
            }
            string clientName = "Lobby";
            m_DataStoreClient.Connect(clientName, (ret, error) =>
            {
                if (ret == true)
                {
                    m_DataStoreClient.CurrentStatus = DataStoreClient.ConnectStatus.Connected;
                    LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Connect to GMServer Success.");
                    OnConnectGMServer();
                }
                else
                {
                    m_DataStoreClient.CurrentStatus = DataStoreClient.ConnectStatus.Disconnect;
                    LogSys.Log(LOG_TYPE.ERROR, "Connect to GMServer Failed...Error:{0}", error);
                }
            });
        }
        internal void Init(PBChannel channel)
        {
            m_DataStoreClient = new DataStoreClient(channel, this);
        }
        protected override void OnStart()
        {
            TickSleepTime = 10;
            ActionNumPerTick = 1024;
            m_GmSeverAvailable = LobbyConfig.GMServerAvailable;
            if (true == LobbyConfig.GMServerAvailable)
            {
                LogSys.Log(LOG_TYPE.INFO, "Connect to GMServer ...");
                ConnectGMServer();
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
                    LogSys.Log(LOG_TYPE.INFO, "GmServerThread.ActionQueue {0}", msg);
                });
            }
        }
        ///=====================================================================================================
        /// 这里定义供其它线程通过QueueAction调用的函数，实际执行线程是GmServerThread。
        ///=====================================================================================================
        internal void GMPQueryUser(ulong userGuid, ulong targetGuid, int handle)
        {
            if (true == m_GmSeverAvailable)
            {
                GMPQueryUser(targetGuid, (GMPLoadUserCB)((ret, data) =>
                {
                    JsonMessageWithGuid resultMsg = new JsonMessageWithGuid(JsonMessageID.GmQueryInfoByGuidOrNickname);
                    resultMsg.m_Guid = userGuid;
                    ArkCrossEngineMessage.Msg_LC_GmQueryInfoByGuidOrNickname protoData = new ArkCrossEngineMessage.Msg_LC_GmQueryInfoByGuidOrNickname();
                    protoData.m_Result = 1;
                    if (ret == DSLoadResult.Success)
                    {
                        DS_UserInfo dataUser = data.UserBasic;
                        protoData.m_Info = UserInfoBuilder(dataUser);
                        protoData.m_Result = (int)ret;
                        resultMsg.m_ProtoData = protoData;
                    }
                    JsonMessageDispatcher.SendDcoreMessage(handle, resultMsg);
                }));
            }
        }
        ///=====================================================================================================
        /// tool
        ///=====================================================================================================
        private ArkCrossEngineMessage.UserBaseData UserInfoBuilder(DS_UserInfo user)
        {
            if (null == user)
                return null;
            ArkCrossEngineMessage.UserBaseData result = new ArkCrossEngineMessage.UserBaseData();
            result.m_Guid = (ulong)user.Guid;
            result.m_Account = user.AccountId;
            result.m_LogicServerId = 1;
            result.m_Nickname = user.Nickname;
            result.m_HeroId = user.HeroId;
            result.m_Level = user.Level;
            result.m_Vip = user.Vip;
            result.m_Money = user.Money;
            result.m_Gold = user.Gold;
            result.m_LastLogoutTime = user.LastLogoutTime.ToString();
            return result;
        }
        ///=====================================================================================================
        /// private
        ///=====================================================================================================
        private void GMPQueryUser(ulong userGuid, GMPLoadUserCB cb)
        {
            string key = userGuid.ToString();
            m_DataStoreClient.Load<GMP_User>(key, (ret, error, data) =>
            {
                if (ret == DSLoadResult.Success)
                {
                    LogSys.Log(LOG_TYPE.INFO, "GMServer Load Success: Msg:{0}, Key:{1}", "GMP_User", key);
                }
                else if (ret == DSLoadResult.NotFound)
                {
                    LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Yellow, "GMServer Load NotFound: Msg:{0}, Key:{1}", "GMP_User", key);
                }
                else
                {
                    LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "GMServer Load Failed: Msg:{0}, Key:{1}, ERROR:{2}", "GMP_User", key, error);
                }
                cb(ret, data);
            });
        }
    }
}
