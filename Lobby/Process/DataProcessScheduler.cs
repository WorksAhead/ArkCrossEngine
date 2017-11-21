using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Google.ProtocolBuffers;
using DashFire.DataStore;
using DashFire.Billing;
using DashFire;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ArkCrossEngine;

namespace Lobby
{
    /// <summary>
    /// 玩家数据处理调度器，玩家数据请求将被放到并行的若干个线程里进行处理。
    /// 有2类线程：
    /// 1、由DispatchAction调用发起的操作，此时执行线程无法指定。
    /// 2、调度器内部实例化一个线程，用以进行必须在一个线程里进行的操作。（未对外提供接口，目前假定用于为1中操作需要有序的服务。）
    /// </summary>
    /// <remarks>
    /// 这个类采用多线程操作数据，所有成员都不能假定其工作的线程。
    /// 请注意四条约束：
    /// 1、UserInfo一旦实例化，内存不会被释放（只回收到池子里供重用，RoomInfo也是这样）。
    /// 2、对于只操作小于等于机器字长的数据的函数，不加锁（操作本来就是原子的）。
    /// 3、对于操作的数据大于机器字长的并且必须保证事务性更新的，需要加锁，每个UserInfo带有一个Lock属性（mono的读写锁有死锁bug，这里直接用普通锁）。UserInfo上持有的具有复杂结构的属性，
    /// 如果该结构/类里涉及集合操作，应该对该结构/类的数据进行封装并通过内部加锁或lockfree机制保证多线程操作安全。
    /// 4、此类方法除Get开头的方法外通常通过DispatchAction调用发起，具体线程分配考虑如下：
    ///    a、玩家进入房间后基本上只有房间线程会修改玩家数据，故RoomProcessThread会直接修改玩家数据（通常都是简单数据或状态修改）。
    ///    b、玩家在大厅内但没有进入房间时的操作由Node发消息到Lobby，然后经DispatchAction调用各方法进行处理。
    ///    c、玩家在游戏中RoomServer会需要修改玩家数据，此时会发消息到Lobby，然后经DispatchAction调用各方法进行处理。
    /// </remarks>
    internal sealed class DataProcessScheduler : MyServerTaskDispatcher
    {
        internal DataProcessScheduler()
          : base(32)
        {
            m_Thread = new MyServerThread();
            m_Thread.TickSleepTime = 10;
            m_Thread.ActionNumPerTick = 1000;
            m_Thread.OnTickEvent += this.OnTick;
        }
        internal void Start()
        {
            m_Thread.Start();
        }
        internal void Stop()
        {
            StopTaskThreads();
            m_Thread.Stop();
        }
        //--------------------------------------------------------------------------------------------------------------------------
        //供外部直接调用的方法，需要保证多线程安全。
        //--------------------------------------------------------------------------------------------------------------------------
        internal void VisitUsers(MyAction<UserInfo> visitor)
        {
            foreach (UserInfo userInfo in m_UserInfos.Values)
            {
                visitor(userInfo);
            }
        }
        internal ulong GetGuidByNickname(string nickname)
        {
            ulong guid = 0;
            m_GuidByNickname.TryGetValue(nickname, out guid);
            return guid;
        }
        internal UserInfo GetUserInfo(ulong guid)
        {
            UserInfo info = null;
            m_UserInfos.TryGetValue(guid, out info);
            return info;
        }
        internal int GetUserCount()
        {
            return m_UserInfos.Count;
        }
        internal AccountInfo FindAccountInfoByKey(string accountKey)
        {
            AccountInfo accountInfo = null;
            m_AccountByKey.TryGetValue(accountKey, out accountInfo);
            return accountInfo;
        }
        internal AccountInfo FindAccountInfoById(string accountId)
        {
            AccountInfo accountInfo = null;
            m_AccountById.TryGetValue(accountId, out accountInfo);
            return accountInfo;
        }
        internal void DoLastSaveData()
        {
            m_LastSaveFinished = false;
            //服务器关闭前的最后一次存储操作
            var ds_thread = LobbyServer.Instance.DataStoreThread;
            if (ds_thread.DataStoreAvailable == true)
            {
                foreach (ulong guid in m_ActiveUserGuids)
                {
                    UserInfo user = GetUserInfo(guid);
                    if (user != null)
                    {
                        //通知客户端服务器关闭
                        JsonMessageWithGuid retMsg = new JsonMessageWithGuid(JsonMessageID.ServerShutdown);
                        retMsg.m_Guid = user.Guid;
                        JsonMessageDispatcher.SendDcoreMessage(user.NodeName, retMsg);
                        //保存玩家数据
                        user.NextUserSaveCount = 0;
                        ds_thread.DSPSaveUser(user, user.NextUserSaveCount);
                    }
                }
            }
            m_IsLastSave = true;
        }
        internal bool LastSaveFinished
        {
            get { return m_LastSaveFinished; }
        }
        //--------------------------------------------------------------------------------------------------------------------------
        //供GM消息调用的方法，实际执行线程是在线程池的某个线程里执行，实现时需要注意并发问题，需要加锁的加锁。
        //逻辑功能不要直接调用此类处理（会有gm权限判断，正式服会导致功能失效），应该调用逻辑上一个功能相同的方法（通常以Do开头命名）。
        //--------------------------------------------------------------------------------------------------------------------------
        internal void HandleAddAssets(ulong guid, int money, int gold, int exp, int stamina)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.CanUseGmCommand)
            {
                DoAddAssets(guid, money, gold, exp, stamina, GainConsumePos.Gm.ToString());
            }
        }
        internal void HandleAddItem(ulong guid, int item_id, int num)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.ItemBag != null && user.CanUseGmCommand)
            {
                DoAddItem(guid, item_id, num, GainItemWay.AddItem.ToString());
            }
        }
        internal void HandleResetDailyMissions(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.Mission && user.CanUseGmCommand)
            {
                DoResetDailyMissions(guid);
            }
        }
        internal void HandlePublishNotice(string content, int roll_num)
        {
            if (null != m_Thread)
            {
                m_Thread.QueueAction(this.PublishNotice, content, roll_num);
            }
        }
        internal void UnlockCountLimit(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                user.Vigor = user.VigorMax;
                user.CurBuyMoneyCount = 0;
                user.CurBuyStaminaCount = 0;
                user.GoldCurAcceptedCount = 0;
                user.AttemptCurAcceptedCount = 0;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------
        //供外部通过DispatchAction调用的方法，实际执行线程是在线程池的某个线程里执行，实现时需要注意并发问题，需要加锁的加锁。
        //--------------------------------------------------------------------------------------------------------------------------
        //登录流程相关方法
        //这个方法会在ServerBridgeThread里与DataProcessScheduler线程池里调用，修改一定要注意线程安全！！！
        internal void DoAccountLogin(string accountKey, string accountId, int login_server_id, string client_game_version, string client_login_ip, string unique_identifier, string system, string channelId, string nodeName)
        {
            QueueingThread queueingThread = LobbyServer.Instance.QueueingThread;
            if (!GmConfigProvider.Instance.IsGmAccount(accountKey) && queueingThread.NeedQueueing(login_server_id))
            {
                queueingThread.QueueAction(queueingThread.StartQueueing, accountKey, accountId, login_server_id, client_game_version, client_login_ip, unique_identifier, system, channelId, nodeName);
                JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
                replyMsg.m_Account = accountKey;
                replyMsg.m_AccountId = accountId;
                replyMsg.m_Result = (int)AccountLoginResult.Queueing;
                JsonMessageDispatcher.SendDcoreMessage(nodeName, replyMsg);
            }
            else
            {
                DoAccountLoginWithoutQueueing(accountKey, accountId, login_server_id, client_game_version, client_login_ip, unique_identifier, system, channelId, nodeName);
            }
        }
        internal void DoAccountLoginWithoutQueueing(string accountKey, string accountId, int login_server_id, string client_game_version, string client_login_ip, string unique_identifier, string system, string channelId, string nodeName)
        {
            AccountInfo accountInfo = FindAccountInfoById(accountId);
            if (accountInfo == null)
            {
                //当前accountId不在线
                accountInfo = new AccountInfo();
                accountInfo.AccountKey = accountKey;
                accountInfo.AccountId = accountId;
                if (client_game_version.Length <= 0)
                {
                    accountInfo.ClientGameVersion = VersionConfigProvider.Instance.GetVersionNum();
                }
                else
                {
                    accountInfo.ClientGameVersion = client_game_version;
                }
                accountInfo.ClientLoginIp = client_login_ip;
                accountInfo.ClientDeviceidId = unique_identifier;
                accountInfo.System = system;
                if (channelId.Length > 0)
                    accountInfo.ChannelId = channelId;
                accountInfo.NodeName = nodeName;
                accountInfo.LogicServerId = login_server_id;
                var ds_thread = LobbyServer.Instance.DataStoreThread;
                if (ds_thread.DataStoreAvailable == true)
                {
                    //注意这里的回调在DataStoreThread里执行
                    ds_thread.QueueAction(ds_thread.DSPLoadAccount, accountId, (DataStoreThread.DSPLoadAccountCB)((ret, data) =>
                    {
                        JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
                        replyMsg.m_Account = accountKey;
                        replyMsg.m_AccountId = accountId;
                        replyMsg.m_Result = (int)AccountLoginResult.Error;
                        if (ret == DSLoadResult.Success)
                        {
                            //读取DSP_Account数据成功
                            if (!data.AccountBasic.IsBanned)
                            {
                                SortedList<long, DS_UserInfo> sortedUserList = new SortedList<long, DS_UserInfo>();
                                foreach (var userData in data.UserListList)
                                {
                                    sortedUserList.Add(userData.Guid, userData);
                                }
                                foreach (var userData in sortedUserList.Values)
                                {
                                    RoleInfo ui = new RoleInfo();
                                    ui.Guid = (ulong)userData.Guid;
                                    ui.Nickname = userData.Nickname;
                                    ui.HeroId = userData.HeroId;
                                    ui.Level = userData.Level;
                                    ui.Gold = userData.Gold;
                                    accountInfo.Users.Add(ui);
                                }
                                accountInfo.CurrentState = AccountState.Online;
                                m_AccountById.AddOrUpdate(accountId, accountInfo, (g, u) => accountInfo);
                                m_AccountByKey.AddOrUpdate(accountKey, accountInfo, (g, u) => accountInfo);
                                replyMsg.m_Result = (int)AccountLoginResult.Success;
                                LogSys.Log(LOG_TYPE.INFO, "Account login success. AccountID:{0}", accountId);
                                /// norm log
                                accountInfo.LastLoginTime = TimeUtility.CurTimestamp;
                                long first_role_id = 0;
                                string first_role_name = "";
                                int first_role_level = 1;
                                int first_role_gold = 0;
                                foreach (var userData in data.UserListList)
                                {
                                    first_role_id = userData.Guid;
                                    first_role_name = userData.Nickname;
                                    first_role_level = userData.Level;
                                    first_role_gold = userData.Money;
                                }
                                LogSys.NormLog("login", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.login, LobbyConfig.LogNormVersionStr,
                                "2050", accountInfo.LogicServerId, accountInfo.ChannelId, accountId, accountId, first_role_id, first_role_name, first_role_level,
                                accountInfo.ClientDeviceidId, accountInfo.ClientLoginIp, first_role_gold);
                            }
                            else
                            {
                                //账号被封停
                                replyMsg.m_Result = (int)AccountLoginResult.Banned;
                            }
                        }
                        else if (ret == DSLoadResult.NotFound)
                        {
                            //读取DSP_Account数据为空,该账号首次进入游戏
                            m_AccountByKey.AddOrUpdate(accountKey, accountInfo, (g, u) => accountInfo);
                            if (LobbyConfig.ActivationCodeAvailable)
                            {
                                replyMsg.m_Result = (int)AccountLoginResult.FirstLogin;
                            }
                            else
                            {
                                accountInfo.CurrentState = AccountState.Online;
                                m_AccountById.AddOrUpdate(accountId, accountInfo, (g, u) => accountInfo);
                                m_AccountByKey.AddOrUpdate(accountKey, accountInfo, (g, u) => accountInfo);
                                replyMsg.m_Result = (int)AccountLoginResult.Success;
                            }
                            LogSys.Log(LOG_TYPE.INFO, "Account FIRST LOGIN, Activation is needed. AccountID:{0}", accountId);
                            /// norm log
                            accountInfo.LastLoginTime = TimeUtility.CurTimestamp;
                            int first_role_level = 1;
                            int first_role_gold = 0;
                            LogSys.NormLog("login", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.login, LobbyConfig.LogNormVersionStr,
                            "2050", accountInfo.LogicServerId, accountInfo.ChannelId, accountId, accountId, "null", "null", first_role_level,
                            accountInfo.ClientDeviceidId, accountInfo.ClientLoginIp, first_role_gold);
                        }
                        else
                        {
                            //读取DSP_Account数据失败
                            LogSys.Log(LOG_TYPE.INFO, "Account login failed, Error with DataStore. AccountID:{0}", accountId);
                        }
                        JsonMessageDispatcher.SendDcoreMessage(nodeName, replyMsg);
                    }));
                }
                else
                {
                    accountInfo = new AccountInfo();
                    accountInfo.AccountKey = accountKey;
                    accountInfo.AccountId = accountId;
                    accountInfo.NodeName = nodeName;
                    accountInfo.CurrentState = AccountState.Online;
                    m_AccountById.AddOrUpdate(accountId, accountInfo, (g, u) => accountInfo);
                    m_AccountByKey.AddOrUpdate(accountKey, accountInfo, (g, u) => accountInfo);
                    LogSys.Log(LOG_TYPE.INFO, "Account login success. Account:{0}", accountId);
                    JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
                    replyMsg.m_Account = accountKey;
                    replyMsg.m_AccountId = accountId;
                    replyMsg.m_Result = (int)AccountLoginResult.Success;
                    JsonMessageDispatcher.SendDcoreMessage(nodeName, replyMsg);
                }
            }
            else
            {
                //当前账号在线        
                string oldAccountKey = accountInfo.AccountKey;
                if (accountInfo.CurrentState == AccountState.Dropped || oldAccountKey.Equals(accountKey))
                {
                    //账号处在离线状态或同一设备重复登录，登录成功
                    LogSys.Log(LOG_TYPE.INFO, "Account relogin success. AccountId:{0}, AccountKey:{1}, OldAccountKey:{2}", accountId, accountKey, oldAccountKey);
                    accountInfo.AccountKey = accountKey;
                    accountInfo.AccountId = accountId;
                    accountInfo.NodeName = nodeName;
                    accountInfo.ClientGameVersion = VersionConfigProvider.Instance.GetVersionNum();
                    accountInfo.ClientLoginIp = client_login_ip;
                    accountInfo.System = system;
                    accountInfo.ChannelId = channelId;
                    accountInfo.CurrentState = AccountState.Online;
                    m_AccountById.AddOrUpdate(accountId, accountInfo, (g, u) => accountInfo);
                    m_AccountByKey.AddOrUpdate(accountKey, accountInfo, (g, u) => accountInfo);
                    JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
                    replyMsg.m_Account = accountKey;
                    replyMsg.m_AccountId = accountId;
                    replyMsg.m_Result = (int)AccountLoginResult.Success;
                    JsonMessageDispatcher.SendDcoreMessage(nodeName, replyMsg);
                }
                else
                {
                    //账号在别的设备上登录，登录失败
                    LogSys.Log(LOG_TYPE.INFO, "Account login failed. Account relogin with another device. AccountId:{0}, AccountKey:{1}", accountId, accountKey);
                    JsonMessageAccountLoginResult replyMsg = new JsonMessageAccountLoginResult();
                    replyMsg.m_Account = accountKey;
                    replyMsg.m_AccountId = accountId;
                    replyMsg.m_Result = (int)AccountLoginResult.AlreadyOnline;
                    JsonMessageDispatcher.SendDcoreMessage(nodeName, replyMsg);
                }
            }
        }
        internal void OnActivateAccount(string accountKey, string activationCode)
        {
            AccountInfo accountInfo = FindAccountInfoByKey(accountKey);
            if (accountInfo != null)
            {
                string accountId = accountInfo.AccountId;
                var ds_thread = LobbyServer.Instance.DataStoreThread;
                if (ds_thread.DataStoreAvailable == true)
                {
                    ActivateAccountResult ret = ActivateAccountResult.Error;
                    ret = m_ActivationCodeSystem.CheckActivationCode(activationCode);
                    if (ret == ActivateAccountResult.Success)
                    {
                        accountInfo.CurrentState = AccountState.Online;
                        m_AccountById.AddOrUpdate(accountId, accountInfo, (g, u) => accountInfo);
                        ds_thread.DSPSaveCreateAccount(accountInfo, activationCode);
                        LogSys.Log(LOG_TYPE.INFO, "Activate Account Success. AccountID:{0}, ActivationCode:{1}", accountId, activationCode);
                    }
                    else
                    {
                        LogSys.Log(LOG_TYPE.INFO, "Activate Account Failed. Account:{0}, ActivationCode:{1}, Result:{2}", accountId, activationCode, ret);
                    }
                    JsonMessageActivateAccountResult replyMsg = new JsonMessageActivateAccountResult();
                    replyMsg.m_Account = accountKey;
                    replyMsg.m_Result = (int)ret;
                    JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, replyMsg);
                }
            }
        }
        internal void OnAccountLogout(string accountKey)
        {
            if (LobbyConfig.DataStoreAvailable)
            {
                AccountInfo ai = FindAccountInfoByKey(accountKey);
                if (ai != null)
                {
                    //检查当前账号是否有角色在线          
                    bool isAccountKickable = true;
                    foreach (var role in ai.Users)
                    {
                        if (role != null)
                        {
                            UserInfo ui = GetUserInfo(role.Guid);
                            if (ui != null)
                            {
                                isAccountKickable = false;
                                break;
                            }
                        }
                    }
                    if (isAccountKickable)
                    {
                        //踢掉账号
                        ai.CurrentState = AccountState.Offline;
                        AccountInfo tmpAi = null;
                        m_AccountByKey.TryRemove(ai.AccountKey, out tmpAi);
                        m_AccountById.TryRemove(ai.AccountId, out tmpAi);
                        LogSys.Log(LOG_TYPE.INFO, "Account direct LOGOUT: AccountKey:{0}, AccountId:{1}", ai.AccountKey, ai.AccountId);
                    }
                    else
                    {
                        //AccountInfo设置为离线状态
                        ai.CurrentState = AccountState.Dropped;
                    }
                    m_NicknameSystem.RevertAccountNicknames(accountKey);
                }
            }
        }
        internal void OnRoleList(string accountKey)
        {
            AccountInfo accountInfo = FindAccountInfoByKey(accountKey);
            if (accountInfo != null)
            {
                //更新角色列表信息
                for (int i = 0; i < accountInfo.Users.Count; ++i)
                {
                    UserInfo user = GetUserInfo(accountInfo.Users[i].Guid);
                    if (user != null)
                    {
                        accountInfo.Users[i].Nickname = user.Nickname;
                        accountInfo.Users[i].HeroId = user.HeroId;
                        accountInfo.Users[i].Level = user.Level;
                    }
                }
                //返回结果
                JsonMessageWithAccount roleListResultMsg = new JsonMessageWithAccount(JsonMessageID.RoleListResult);
                roleListResultMsg.m_Account = accountKey;
                ArkCrossEngineMessage.Msg_LC_RoleListResult protoData = new ArkCrossEngineMessage.Msg_LC_RoleListResult();
                protoData.m_UserInfoCount = accountInfo.Users.Count;
                for (int i = 0; i < accountInfo.Users.Count; ++i)
                {
                    ArkCrossEngineMessage.Msg_LC_RoleListResult.UserInfoForMessage roleList = new ArkCrossEngineMessage.Msg_LC_RoleListResult.UserInfoForMessage();
                    roleList.m_UserGuid = accountInfo.Users[i].Guid;
                    roleList.m_Nickname = accountInfo.Users[i].Nickname;
                    roleList.m_HeroId = accountInfo.Users[i].HeroId;
                    roleList.m_Level = accountInfo.Users[i].Level;
                    protoData.m_UserInfos.Add(roleList);
                }
                protoData.m_Result = (int)RoleListResult.Success;
                roleListResultMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, roleListResultMsg);
            }
        }
        internal void OnCreateNickname(string accountKey)
        {
            AccountInfo accountInfo = FindAccountInfoByKey(accountKey);
            if (accountInfo != null)
            {
                JsonMessageCreateNicnameResult replyMsg = new JsonMessageCreateNicnameResult();
                replyMsg.m_Account = accountKey;
                List<string> nicknameList = m_NicknameSystem.RequestNicknames(accountKey);
                replyMsg.m_Nicknames = new string[nicknameList.Count];
                for (int i = 0; i < nicknameList.Count; ++i)
                {
                    replyMsg.m_Nicknames[i] = nicknameList[i];
                }
                JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, replyMsg);
            }
        }
        internal void OnCreateRole(string accountKey, string nickname, int heroId)
        {
            AccountInfo accountInfo = FindAccountInfoByKey(accountKey);
            var ds_thread = LobbyServer.Instance.DataStoreThread;
            if (accountInfo != null && accountInfo.Users.Count < 3)
            {
                //检查昵称是否可用
                bool ret = m_NicknameSystem.CheckNickname(accountKey, nickname);
                if (ret == true)
                {
                    UserInfo ui = NewUserInfo();
                    ui.Guid = LobbyServer.Instance.GlobalDataProcessThread.GenerateUserGuid();
                    ui.Account = accountInfo.AccountKey;
                    ui.AccountId = accountInfo.AccountId;
                    ui.LogicServerId = accountInfo.LogicServerId;
                    ui.Nickname = nickname;
                    ui.HeroId = heroId;
                    InitUserinfo(ui);
                    RoleInfo ri = new RoleInfo();
                    ri.Guid = ui.Guid;
                    ri.Nickname = nickname;
                    ri.HeroId = heroId;
                    ri.Level = 1;
                    accountInfo.Users.Add(ri);
                    LogSys.Log(LOG_TYPE.INFO, "Account create new user : Account:({0}), UserGuid:({1}) Nickname:({2}) HeroId:({3})", accountKey, ri.Guid, ri.Nickname, ri.HeroId);
                    if (ds_thread.DataStoreAvailable)
                    {
                        ds_thread.DSPSaveCreateUser(accountInfo, nickname, ui.Guid);
                        ds_thread.DSPSaveUser(ui, ui.NextUserSaveCount);
                    }
                    //向客户端回复消息
                    JsonMessageCreateRoleResult replyMsg = new JsonMessageCreateRoleResult();
                    replyMsg.m_Account = accountKey;
                    replyMsg.m_Result = (int)CreateRoleResult.Success;
                    replyMsg.m_UserInfo = new UserInfoForMessage();
                    replyMsg.m_UserInfo.m_UserGuid = ri.Guid;
                    replyMsg.m_UserInfo.m_Nickname = ri.Nickname;
                    replyMsg.m_UserInfo.m_HeroId = ri.HeroId;
                    replyMsg.m_UserInfo.m_Level = ri.Level;
                    JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, replyMsg);
                    //游戏角色创建成功，直接进入游戏
                    ui.NodeName = accountInfo.NodeName;
                    this.DoUserLogin(ui);
                    JsonMessageWithAccountAndGuid enterMsg = new JsonMessageWithAccountAndGuid(JsonMessageID.RoleEnterResult);
                    enterMsg.m_Account = ui.Account;
                    enterMsg.m_Guid = ui.Guid;
                    ArkCrossEngineMessage.Msg_LC_RoleEnterResult protoData = CreateRoleEnterResultMsg(ui);
                    protoData.m_Result = (int)RoleEnterResult.Success;
                    enterMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, enterMsg);
                    /// norm log
                    LogSys.NormLog("rolebuild", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.rolebuild, LobbyConfig.LogNormVersionStr,
                    "3025", accountInfo.LogicServerId, accountInfo.ChannelId, accountInfo.AccountId, ri.Guid, nickname, heroId);
                    /// norm log
                    LogSys.NormLog("serverevent", LobbyConfig.AppKeyStr, LobbyConfig.LogNormVersionStr, accountInfo.System, accountInfo.ChannelId,
                    accountInfo.ClientDeviceidId, accountInfo.AccountId, (int)ServerEventCode.CreateHero, accountInfo.ClientGameVersion);
                }
                else
                {
                    //昵称已存在，角色创建失败
                    JsonMessageCreateRoleResult replyMsg = new JsonMessageCreateRoleResult();
                    replyMsg.m_Account = accountKey;
                    replyMsg.m_Result = (int)CreateRoleResult.NicknameError;
                    JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, replyMsg);
                }
            }
        }
        internal void OnRoleEnter(string accountKey, ulong userGuid)
        {
            AccountInfo accountInfo = FindAccountInfoByKey(accountKey);
            if (accountInfo != null)
            {
                RoleInfo ri = accountInfo.FindUser(userGuid);
                if (ri != null)
                {
                    MatchFormThread match_form = LobbyServer.Instance.MatchFormThread;
                    UserInfo user = GetUserInfo(userGuid);
                    if (null != user)
                    {
                        if (user.CurrentState == UserState.DropOrOffline)
                        {
                            //用户处在下线过程中，需要等待lobby离线流程完成
                            JsonMessageWithAccountAndGuid roleEnterResultMsg = new JsonMessageWithAccountAndGuid(JsonMessageID.RoleEnterResult);
                            roleEnterResultMsg.m_Account = accountInfo.AccountKey;
                            roleEnterResultMsg.m_Guid = user.Guid;
                            ArkCrossEngineMessage.Msg_LC_RoleEnterResult protoData = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult();
                            protoData.m_Result = (int)RoleEnterResult.Wait;
                            roleEnterResultMsg.m_ProtoData = protoData;
                            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, roleEnterResultMsg);
                            LogSys.Log(LOG_TYPE.WARN, "RoleEnter AccountKey:{0} Guid:{1} Wait Offline", accountKey, userGuid);
                        }
                        else
                        {
                            user.Account = accountInfo.AccountKey;    //客户端设备可能改变
                            user.NodeName = accountInfo.NodeName;
                            user.LogicServerId = accountInfo.LogicServerId;
                            user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
                            DoUserRelogin(user);
                            //回复客户端
                            JsonMessageWithAccountAndGuid roleEnterResultMsg = new JsonMessageWithAccountAndGuid(JsonMessageID.RoleEnterResult);
                            roleEnterResultMsg.m_Account = user.Account;
                            roleEnterResultMsg.m_Guid = user.Guid;
                            ArkCrossEngineMessage.Msg_LC_RoleEnterResult protoData = CreateRoleEnterResultMsg(user);
                            protoData.m_Result = (int)RoleEnterResult.Success;
                            roleEnterResultMsg.m_ProtoData = protoData;
                            JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, roleEnterResultMsg);
                            if (user.CurrentState == UserState.Room)
                            {
                                //重新进入多人副本
                                RoomProcessThread roomProcess = LobbyServer.Instance.RoomProcessThread;
                                roomProcess.QueueAction(roomProcess.OnUserRelogin, userGuid);
                            }
                            else if (user.CurrentState == UserState.Pve)
                            {

                            }
                            LogSys.Log(LOG_TYPE.WARN, "RoleEnter AccountKey:{0} Guid:{1} Relogin", accountKey, userGuid);
                        }
                    }
                    else
                    {
                        var ds_thread = LobbyServer.Instance.DataStoreThread;
                        if (ds_thread.DataStoreAvailable == true)
                        {
                            //注意这里的回调在DataStoreThread里执行
                            ds_thread.QueueAction(ds_thread.DSPLoadUser, ri.Guid, (DataStoreThread.DSPLoadUserCB)((ret, data) =>
                            {
                                JsonMessageWithAccountAndGuid replyMsg = new JsonMessageWithAccountAndGuid(JsonMessageID.RoleEnterResult);
                                ArkCrossEngineMessage.Msg_LC_RoleEnterResult protoData = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult();
                                protoData.m_Result = (int)RoleEnterResult.UnknownError;
                                if (ret == DSLoadResult.Success)
                                {
                                    UserInfo ui = NewUserInfo();
                                    //角色基础数据
                                    DS_UserInfo dataUser = data.UserBasic;
                                    ui.Guid = (ulong)dataUser.Guid;
                                    ui.Account = accountKey;
                                    ui.LogicServerId = accountInfo.LogicServerId;
                                    ui.AccountId = dataUser.AccountId;
                                    ui.Nickname = dataUser.Nickname;
                                    ui.HeroId = dataUser.HeroId;
                                    ui.Level = dataUser.Level;
                                    ui.Money = dataUser.Money;
                                    ui.Gold = dataUser.Gold;
                                    ui.ExpPoints = dataUser.ExpPoints;
                                    ui.Vip = dataUser.Vip;
                                    ui.CitySceneId = dataUser.CitySceneId;
                                    ui.LastLogoutTime = DateTime.Parse(dataUser.LastLogoutTime);
                                    ui.CreateTime = DateTime.Parse(dataUser.CreateTime);
                                    ui.NewBieGuideInfo.NewbieFlag = dataUser.NewbieStep;
                                    ui.NewBieGuideInfo.NewbieActionFlag = dataUser.NewbieActionFlag;

                                    DS_UserInfoExtra dataUserExtra = data.UserExtra;
                                    ui.GowInfo.GowElo = dataUserExtra.GowElo;
                                    ui.GowInfo.GowMatches = dataUserExtra.GowMatches;
                                    ui.GowInfo.GowWinMatches = dataUserExtra.GowWinMatches;
                                    ui.InitGowHistoryForLogin(dataUserExtra.GowHistroyTimeList, dataUserExtra.GowHistroyEloList);
                                    ui.InitStaminaForLogin(dataUserExtra.Stamina, dataUserExtra.LastAddStaminaTimestamp, dataUserExtra.BuyStaminaCount);
                                    ui.InitMidasTouchForLogin(dataUserExtra.LastBuyMoneyTimestamp, dataUserExtra.BuyMoneyCount);
                                    ui.InitSellIncomeForLogin(dataUserExtra.LastSellTimestamp, dataUserExtra.SellIncome);
                                    ui.LastResetStaminaTime = DateTime.Parse(dataUserExtra.LastResetStaminaTime);
                                    ui.LastResetMidasTouchTime = DateTime.Parse(dataUserExtra.LastResetMidasTouchTime);
                                    ui.LastResetSellItemIncomeTime = DateTime.Parse(dataUserExtra.LastResetSellTime);
                                    ui.LastResetDailyMissionTime = DateTime.Parse(dataUserExtra.LastResetDailyMissionTime);
                                    ui.AttemptAward = dataUserExtra.AttemptAward;
                                    ui.AttemptCurAcceptedCount = dataUserExtra.AttemptCurAcceptedCount;
                                    ui.AttemptAcceptedAward = dataUserExtra.AttemptAcceptedAward;
                                    ui.LastResetAttemptAwardCountTime = DateTime.Parse(dataUserExtra.LastResetAttemptAwardCountTime);
                                    ui.GoldCurAcceptedCount = dataUserExtra.GoldTollgateCount;
                                    ui.LastResetGoldAwardCountTime = DateTime.Parse(dataUserExtra.LastResetGoldTollgateCountTime);
                                    ui.InitExchangeGoodsForLogin(dataUserExtra.ExchangeGoodList, dataUserExtra.ExchangeGoodNumber, dataUserExtra.ExchangeGoodRefreshCount);
                                    ui.LastResetExchangeGoodsTime = DateTime.Parse(dataUserExtra.LastResetExchangeGoodTime);
                                    ui.InitCompleteSceneForLogin(dataUserExtra.CompleteSceneList, dataUserExtra.CompleteSceneNumber);
                                    ui.LastResetCompletedScenesCountTime = DateTime.Parse(dataUserExtra.LastResetSceneCountTime);
                                    ui.InitVigorForLogin(dataUserExtra.Vigor, dataUserExtra.LastAddVigorTimestamp);
                                    ui.UsedStamina = dataUserExtra.UsedStamina;
                                    ui.RestDailySignInCount = dataUserExtra.DayRestSignCount;
                                    ui.LastResetSignInRewardDailyCountTime = DateTime.Parse(dataUserExtra.LastResetDaySignCountTime);
                                    ui.SignInCountCurMonth = dataUserExtra.MonthSignCount;
                                    ui.LastResetSignInRewardMonthCountTime = DateTime.Parse(dataUserExtra.LastResetMonthSignCountTime);
                                    ui.MonthCardExpiredTime = DateTime.Parse(dataUserExtra.MonthCardExpireTime);
                                    ui.LoginRewardInfo.IsGetLoginReward = dataUserExtra.IsWeeklyLoginRewarded;
                                    ui.InitWeeklyLoginRewardForLogin(dataUserExtra.WeeklyLoginRewardList);
                                    ui.LastResetWeeklyLoginRewardTime = DateTime.Parse(dataUserExtra.LastResetWeeklyLoginRewardTime);
                                    ui.UpdateSignInData();
                                    ui.UpdateAttempt();
                                    ui.UpdateGoldTollgate();
                                    ui.UpdateMidasTouch();
                                    ui.UpdateExchangeGoods();
                                    ui.UpdateSellItemIncome();
                                    ui.UpdateSceneCompletedCountData();
                                    ui.UpdateWeeklyLoginData();
                                    ui.UpdateDailyOnlineDuration();
                                    //装备数据                  
                                    for (int i = 0; i < EquipInfo.c_MaxEquipmentNum; ++i)
                                    {
                                        if (i < data.EquipListCount)
                                        {
                                            DS_EquipInfo dataEquip = data.EquipListList[i];
                                            if (dataEquip != null)
                                            {
                                                ItemInfo item = new ItemInfo();
                                                item.ItemId = dataEquip.ItemId;
                                                item.ItemNum = dataEquip.ItemNum;
                                                item.Level = dataEquip.Level;
                                                item.AppendProperty = dataEquip.AppendProperty;
                                                ui.Equip.SetEquipmentData(dataEquip.Position, item);
                                            }
                                        }
                                        else
                                        {
                                            ItemInfo item = new ItemInfo();
                                            item.ItemId = 0;
                                            item.ItemNum = 1;
                                            item.Level = 1;
                                            item.AppendProperty = 0;
                                            ui.Equip.SetEquipmentData(i, item);
                                        }
                                    }
                                    //物品数据
                                    for (int i = 0; i < data.ItemListCount; ++i)
                                    {
                                        DS_ItemInfo dataItem = data.ItemListList[i];
                                        if (dataItem != null)
                                        {
                                            ItemInfo item = new ItemInfo();
                                            item.ItemId = dataItem.ItemId;
                                            item.ItemNum = dataItem.ItemNum;
                                            item.Level = dataItem.Level;
                                            item.AppendProperty = dataItem.AppendProperty;
                                            ui.ItemBag.AddItemData(item, dataItem.ItemNum);
                                        }
                                    }
                                    //神器数据
                                    for (int i = 0; i < data.LegacyListList.Count; ++i)
                                    {
                                        DS_LegacyInfo dataLegacy = data.LegacyListList[i];
                                        if (dataLegacy != null)
                                        {
                                            ItemInfo legacy = new ItemInfo();
                                            legacy.ItemId = dataLegacy.LegacyId;
                                            legacy.Level = dataLegacy.LegacyLevel;
                                            legacy.AppendProperty = dataLegacy.AppendProperty;
                                            legacy.IsUnlock = dataLegacy.IsUnlock;
                                            ui.Legacy.SetLegacyData(dataLegacy.Position, legacy);
                                        }
                                    }
                                    //XSoul数据                  
                                    foreach (var dataXSoul in data.XSoulListList)
                                    {
                                        ItemInfo xsoulInfo = new ItemInfo(dataXSoul.XSoulId, 0, 1, true);
                                        xsoulInfo.Level = dataXSoul.XSoulLevel;
                                        xsoulInfo.Experience = dataXSoul.XSoulExp;
                                        xsoulInfo.ShowModelLevel = dataXSoul.XSoulModelLevel;
                                        ui.XSoul.SetXSoulPartData((XSoulPart)dataXSoul.XSoulType, xsoulInfo);
                                    }
                                    //技能数据
                                    foreach (var dataSkill in data.SkillListList)
                                    {
                                        SkillDataInfo skill = new SkillDataInfo();
                                        skill.ID = dataSkill.SkillId;
                                        skill.Level = dataSkill.Level;
                                        skill.Postions.Presets[0] = (SlotPosition)dataSkill.Preset;
                                        ui.Skill.AddSkillData(skill);
                                    }
                                    //任务数据
                                    foreach (var dataMission in data.MissionListList)
                                    {
                                        MissionStateType missionState = (MissionStateType)dataMission.MissionState;
                                        ui.Mission.AddMission(dataMission.MissionId, missionState, dataMission.MissionValue);
                                    }
                                    //通关数据
                                    foreach (var dataLevel in data.LevelListList)
                                    {
                                        ui.SetSceneInfo(dataLevel.LevelId, dataLevel.LevelRecord);
                                    }
                                    if (ui.SceneData.Count <= 0)
                                    {
                                        ui.CurrentBattleInfo.init(ui.NewbieScene, ui.HeroId);
                                    }
                                    //远征数据
                                    DS_ExpeditionInfo dataExpedition = data.UserExpedition;
                                    ui.Expedition.IsUnlock = ui.Level >= ExpeditionPlayerInfo.UnlockLevel ? true : false;
                                    ui.Expedition.ResetScore = dataExpedition.FightingScore;
                                    ui.Expedition.Hp = dataExpedition.HP;
                                    ui.Expedition.Mp = dataExpedition.MP;
                                    ui.Expedition.Rage = dataExpedition.Rage;
                                    ui.Expedition.Schedule = dataExpedition.Schedule;
                                    ui.Expedition.CurWeakMonsterCount = dataExpedition.MonsterCount;
                                    ui.Expedition.CurBossCount = dataExpedition.BossCount;
                                    ui.Expedition.CurOnePlayerCount = dataExpedition.OnePlayerCount;
                                    ui.Expedition.LastResetTimestamp = dataExpedition.StartTime;
                                    for (int i = 0; i < ui.Expedition.Schedule; ++i)
                                    {
                                        ui.Expedition.Tollgates[i].IsFinish = true;
                                    }
                                    List<int> unrewardedIndex = new List<int>();
                                    string[] unrewardedTollgates = dataExpedition.Unrewarded.Split(new char[] { '|' });
                                    foreach (string str in unrewardedTollgates)
                                    {
                                        if (str.Trim() != string.Empty)
                                        {
                                            int tollgateIndex = -1;
                                            if (int.TryParse(str, out tollgateIndex))
                                            {
                                                unrewardedIndex.Add(tollgateIndex);
                                            }
                                        }
                                    }
                                    foreach (int index in unrewardedIndex)
                                    {
                                        ui.Expedition.Tollgates[index].IsAcceptedAward = false;
                                    }
                                    int nextTollgateIndex = 0;
                                    if (ui.Expedition.Schedule < ExpeditionPlayerInfo.c_MaxExpeditionNum)
                                    {
                                        nextTollgateIndex = ui.Expedition.Schedule;
                                    }
                                    else
                                    {
                                        //已经通关
                                        nextTollgateIndex = ui.Expedition.Schedule - 1;
                                    }
                                    Lobby.ExpeditionPlayerInfo.TollgateData nextTollgate = ui.Expedition.Tollgates[nextTollgateIndex];
                                    if (dataExpedition.TollgateType == (int)EnemyType.ET_Monster)
                                    {
                                        nextTollgate.Type = EnemyType.ET_Monster;
                                    }
                                    else if (dataExpedition.TollgateType == (int)EnemyType.ET_Boss)
                                    {
                                        nextTollgate.Type = EnemyType.ET_Boss;
                                    }
                                    else if (dataExpedition.TollgateType == (int)EnemyType.ET_OnePlayer)
                                    {
                                        nextTollgate.Type = EnemyType.ET_OnePlayer;
                                    }
                                    else
                                    {
                                        nextTollgate.Type = EnemyType.ET_TwoPlayer;
                                    }
                                    if (nextTollgate.Type == EnemyType.ET_Monster || nextTollgate.Type == EnemyType.ET_Boss)
                                    {
                                        List<int> enemyList = new List<int>();
                                        string[] enemyStrArray = dataExpedition.EnemyList.Split(new char[] { '|' });
                                        foreach (string str in enemyStrArray)
                                        {
                                            if (str.Trim() != string.Empty)
                                            {
                                                int enemyId = -1;
                                                if (int.TryParse(str, out enemyId))
                                                {
                                                    enemyList.Add(enemyId);
                                                }
                                            }
                                        }
                                        foreach (int id in enemyList)
                                        {
                                            nextTollgate.EnemyList.Add(id);
                                        }
                                        List<int> enemyAttrList = new List<int>();
                                        string[] enemyAttrStrArray = dataExpedition.EnemyAttrList.Split(new char[] { '|' });
                                        foreach (string str in enemyAttrStrArray)
                                        {
                                            if (str.Trim() != string.Empty)
                                            {
                                                int enemyAttrId = -1;
                                                if (int.TryParse(str, out enemyAttrId))
                                                {
                                                    enemyAttrList.Add(enemyAttrId);
                                                }
                                            }
                                        }
                                        foreach (int attr in enemyAttrList)
                                        {
                                            nextTollgate.EnemyAttrList.Add(attr);
                                        }
                                    }
                                    else if (nextTollgate.Type == EnemyType.ET_OnePlayer)
                                    {
                                        ExpeditionImageInfo imageA = DeserializeImage(dataExpedition.ImageA.ToByteArray());
                                        nextTollgate.UserImageList.Add(imageA);
                                    }
                                    else
                                    {
                                        ExpeditionImageInfo imageA = DeserializeImage(dataExpedition.ImageA.ToByteArray());
                                        nextTollgate.UserImageList.Add(imageA);
                                        ExpeditionImageInfo imageB = DeserializeImage(dataExpedition.ImageB.ToByteArray());
                                        nextTollgate.UserImageList.Add(imageB);
                                    }
                                    //全局邮件状态数据
                                    foreach (var dataMailState in data.MailStateListList)
                                    {
                                        MailState ms = new MailState();
                                        ms.m_MailGuid = (ulong)dataMailState.MailGuid;
                                        ms.m_AlreadyRead = dataMailState.IsRead;
                                        ms.m_AlreadyReceived = dataMailState.IsReceived;
                                        ms.m_ExpiryDate = DateTime.Parse(dataMailState.ExpiryDate);
                                        ui.MailStateInfo.WholeMailStates.Add(ms.m_MailGuid, ms);
                                    }
                                    //伙伴数据
                                    foreach (var dataPartner in data.PartnerListList)
                                    {
                                        ui.PartnerStateInfo.AddPartner(dataPartner.PartnerId, dataPartner.SkillLevel, dataPartner.AdditionLevel);
                                    }
                                    ui.PartnerStateInfo.SetActivePartner(dataUserExtra.ActivePartnerId);
                                    //好友数据
                                    foreach (var dataFriend in data.FriendListList)
                                    {
                                        FriendInfo friend = new FriendInfo();
                                        friend.Guid = (ulong)dataFriend.FriendGuid;
                                        friend.Nickname = dataFriend.FriendNickname;
                                        friend.HeroId = dataFriend.HeroId;
                                        friend.Level = dataFriend.Level;
                                        friend.FightingScore = dataFriend.FightingScore;
                                        friend.IsOnline = false;
                                        UserInfo friend_info = GetUserInfo(friend.Guid);
                                        if (null != friend_info)
                                        {
                                            friend.FightingScore = friend_info.FightingScore;
                                            friend.IsOnline = true;
                                        }
                                        ui.FriendInfos.AddOrUpdate(friend.Guid, friend, (g, u) => friend);
                                    }
                                    //角色数据读取完毕
                                    ui.NodeName = accountInfo.NodeName;
                                    this.DoUserLogin(ui);
                                    replyMsg.m_Account = ui.Account;
                                    replyMsg.m_Guid = ui.Guid;
                                    protoData = CreateRoleEnterResultMsg(ui);
                                    protoData.m_Result = (int)RoleEnterResult.Success;
                                    replyMsg.m_ProtoData = protoData;
                                }
                                JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, replyMsg);
                            }));
                        }
                        else
                        {
                            //Lobby服务器加载  
                            UserInfo ui = NewUserInfo();
                            ui.Guid = ri.Guid;
                            ui.Account = accountInfo.AccountKey;
                            ui.LogicServerId = accountInfo.LogicServerId;
                            ui.AccountId = accountInfo.AccountId;
                            ui.Nickname = ri.Nickname;
                            ui.HeroId = ri.HeroId;
                            ui.Level = ri.Level;
                            InitUserinfo(ui);
                            ui.NodeName = accountInfo.NodeName;
                            this.DoUserLogin(ui);
                            //回复客户端
                            JsonMessageWithAccountAndGuid replyMsg = new JsonMessageWithAccountAndGuid(JsonMessageID.RoleEnterResult);
                            replyMsg.m_Account = ui.Account;
                            replyMsg.m_Guid = ui.Guid;
                            ArkCrossEngineMessage.Msg_LC_RoleEnterResult protoData = CreateRoleEnterResultMsg(ui);
                            protoData.m_Result = (int)RoleEnterResult.Success;
                            replyMsg.m_ProtoData = protoData;
                            JsonMessageDispatcher.SendDcoreMessage(accountInfo.NodeName, replyMsg);
                        }
                        LogSys.Log(LOG_TYPE.INFO, "RoleEnter AccountKey:{0} Guid:{1} Normal", accountKey, userGuid);
                    }
                }
            }
        }
        private void OnUserOnlineNoticeFriends(UserInfo user)
        {
            if (null != user && null != user.FriendInfos && user.FriendInfos.Count > 0)
            {
                lock (user.Lock)
                {
                    ulong guid = user.Guid;
                    foreach (FriendInfo info in user.FriendInfos.Values)
                    {
                        UserInfo assit_info = GetUserInfo(info.Guid);
                        if (assit_info != null && UserState.Online == assit_info.CurrentState)
                        {
                            FriendInfo out_info = null;
                            if (assit_info.FriendInfos.TryGetValue(guid, out out_info))
                            {
                                out_info.IsOnline = true;
                            }
                            ///
                            if (null != out_info)
                            {
                                JsonMessageWithGuid replyMsg = new JsonMessageWithGuid(JsonMessageID.FriendOnline);
                                replyMsg.m_Guid = assit_info.Guid;
                                ArkCrossEngineMessage.Msg_LC_FriendOnline protoData = new ArkCrossEngineMessage.Msg_LC_FriendOnline();
                                protoData.m_Guid = guid;
                                replyMsg.m_ProtoData = protoData;
                                JsonMessageDispatcher.SendDcoreMessage(assit_info.NodeName, replyMsg);
                            }
                        }
                    }
                }
            }
        }
        private void OnUserOfflineNoticeFriends(UserInfo user)
        {
            if (null != user && null != user.FriendInfos && user.FriendInfos.Count > 0)
            {
                lock (user.Lock)
                {
                    ulong guid = user.Guid;
                    foreach (FriendInfo info in user.FriendInfos.Values)
                    {
                        UserInfo assit_info = GetUserInfo(info.Guid);
                        if (assit_info != null)
                        {
                            FriendInfo out_info = null;
                            if (assit_info.FriendInfos.TryGetValue(guid, out out_info))
                            {
                                out_info.IsOnline = false;
                            }
                            ///
                            if (null != out_info)
                            {
                                JsonMessageWithGuid replyMsg = new JsonMessageWithGuid(JsonMessageID.FriendOffline);
                                replyMsg.m_Guid = assit_info.Guid;
                                ArkCrossEngineMessage.Msg_LC_FriendOffline protoData = new ArkCrossEngineMessage.Msg_LC_FriendOffline();
                                protoData.m_Guid = guid;
                                replyMsg.m_ProtoData = protoData;
                                JsonMessageDispatcher.SendDcoreMessage(assit_info.NodeName, replyMsg);
                            }
                        }
                    }
                }
            }
        }
        internal void DoUserLogin(UserInfo user)
        {
            user.IsDisconnected = false;
            user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
            user.LastDSSaveTime = TimeUtility.GetServerMilliseconds();
            //在这里生成游戏中要用的key
            if (user.Key == 0)
                user.Key = GenerateKey();
            user.CurrentState = UserState.Online;
            m_UserInfos.AddOrUpdate(user.Guid, user, (g, u) => user);
            m_GuidByNickname.AddOrUpdate(user.Nickname, user.Guid, (n, g) => user.Guid);
            GlobalDataProcessThread globalProcess = LobbyServer.Instance.GlobalDataProcessThread;
            if (null != globalProcess)
            {
                globalProcess.QueueAction(globalProcess.AddUserToImages, user);
                globalProcess.QueueAction(globalProcess.AddUserToLogicServer, user.LogicServerId, user.Guid);
                globalProcess.QueueAction(globalProcess.LoadUserArena, user.Guid);
            }
            m_Thread.QueueAction(this.ActivateUserGuid, user.Guid);
            OnUserOnlineNoticeFriends(user);
            LogSys.Log(LOG_TYPE.DEBUG, "DoUserLogin,guid:{0},nick:{1},key:{2},acc:{3}", user.Guid, user.Nickname, user.Key, user.AccountId);
            ///
            AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
            if (null != accountInfo)
            {
                user.ClientGameVersion = accountInfo.ClientGameVersion;
                user.ClientDeviceidId = accountInfo.ClientDeviceidId;
                user.LastLoginTime = accountInfo.LastLoginTime;
                user.OnlineDuration.OnlineDurationStartTime = DateTime.Now;
                LogSys.NormLog("rolelogin", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.rolelogin, LobbyConfig.LogNormVersionStr,
                  "3030", accountInfo.LogicServerId, accountInfo.ChannelId, accountInfo.AccountId, accountInfo.AccountId, user.Guid, user.Nickname,
                  user.Level, accountInfo.ClientDeviceidId, user.Money);
            }
        }
        internal void DoUserRelogin(UserInfo user)
        {
            user.IsDisconnected = false;
            if (user.CurrentState != UserState.Room)
            {
                user.CurrentState = UserState.Online;
            }
            m_Thread.QueueAction(this.ActivateUserGuid, user.Guid);
            MatchFormThread match_form = LobbyServer.Instance.MatchFormThread;
            match_form.QueueAction(match_form.CheckGroupForReLogin, user);
            OnUserOnlineNoticeFriends(user);
            LogSys.Log(LOG_TYPE.DEBUG, "DoUserRelogin,guid:{0},nick:{1},key:{2},acc:{3}", user.Guid, user.Nickname, user.Key, user.AccountId);
        }
        internal void DoUserLogoff(ulong guid)
        {
            DoUserLogoff(guid, false);
        }
        internal void DoUserLogoff(ulong guid, bool forceLogoff)
        {
            UserInfo user = GetUserInfo(guid);
            if (user != null)
            {
                user.IsDisconnected = true;
                //掉线即退出匹配队列
                if (user.CurrentState == UserState.Teaming)
                {
                    MatchFormThread teamForm = LobbyServer.Instance.MatchFormThread;
                    teamForm.QueueAction(teamForm.OnUserQuit, guid);
                }
                if (null != user.Group)
                {//组队情形不走离线流程
                    for (int i = 0; i < user.Group.Members.Count; i++)
                    {
                        if (user.Group.Members[i].Guid == guid)
                        {
                            user.Group.Members[i].Status = UserState.DropOrOffline;
                            user.Group.Members[i].LeftLife = TimeUtility.CurTimestamp;
                            MatchFormThread teamForm = LobbyServer.Instance.MatchFormThread;
                            teamForm.QueueAction(teamForm.SyncGroupUsers, user.Group);
                            break;
                        }
                    }
                }
                else if (user.CurrentState == UserState.DropOrOffline || forceLogoff)
                {//仅心跳超时(5分钟)或已经是离线状态才走离线流程
                    if (user.Room != null)
                    {
                        RoomProcessThread roomProcess = LobbyServer.Instance.RoomProcessThread;
                        roomProcess.QueueAction(roomProcess.QuitRoom, guid, true);
                    }
                    else
                    {
                        MatchFormThread teamForm = LobbyServer.Instance.MatchFormThread;
                        teamForm.QueueAction(teamForm.OnUserQuit, guid);
                    }
                    GlobalDataProcessThread global_process = LobbyServer.Instance.GlobalDataProcessThread;
                    if (null != global_process)
                    {
                        global_process.QueueAction(global_process.AddUserToImages, user);
                        global_process.QueueAction(global_process.DelUserFromLogicServer, user.LogicServerId, user.Guid);
                        global_process.QueueAction(global_process.SaveUserArena, user.Guid);
                    }
                    user.IsPrepared = false;
                    user.LastLogoutTime = DateTime.Now;
                    var ds_thread = LobbyServer.Instance.DataStoreThread;
                    if (ds_thread.DataStoreAvailable == true)
                    {
                        user.NextUserSaveCount = 0;
                        ds_thread.DSPSaveUser(user, user.NextUserSaveCount);
                    }
                    OnUserOfflineNoticeFriends(user);
                    m_Thread.QueueAction(this.AddWaitRecycleUser, guid);
                    LogSys.Log(LOG_TYPE.INFO, "User Logoff , guid:{0} , state : {1} , Account:{2}",
                      guid, user.CurrentState, user.AccountId);
                    user.CurrentState = UserState.DropOrOffline;
                }
            }
        }
        internal void DoUserHeartbeat(ulong guid)
        {
            UserInfo user = GetUserInfo(guid);
            if (user != null)
            {
                user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
                int roomId = 0;
                if (user.Room != null)
                    roomId = user.Room.RoomId;
                user.MailStateInfo.RemoveExpiredMails();

                //LogSys.Log(LOG_TYPE.DEBUG, "DoUserHeartbeat,guid:{0}", guid);
            }
            else
            {
                LogSys.Log(LOG_TYPE.DEBUG, "DoUserHeartbeat,guid:{0} can't found.", guid);
            }
        }
        private FriendInfo GenerateFriendInfo(ulong guid, UserInfo terget_user)
        {
            UserInfo user = GetUserInfo(guid);
            if (user != null && terget_user != null)
            {
                FriendInfo newFriend = new FriendInfo();
                newFriend.Guid = terget_user.Guid;
                newFriend.Nickname = terget_user.Nickname;
                newFriend.HeroId = terget_user.HeroId;
                newFriend.Level = terget_user.Level;
                newFriend.FightingScore = terget_user.FightingScore;
                newFriend.IsOnline = false;
                newFriend.IsBlack = false;
                if (UserState.Online == terget_user.CurrentState)
                {
                    newFriend.IsOnline = true;
                }
                /*
                /// equips
                newFriend.Equips = new EquipInfo();
                int equips_ct = terget_user.Equip != null ? terget_user.Equip.Armor.Length : 0;
                for (int i = 0; i < equips_ct; i++) {
                  newFriend.Equips.Armor[i] = new ItemInfo();
                  newFriend.Equips.Armor[i].ItemGuid = terget_user.Equip.Armor[i].ItemGuid;
                  newFriend.Equips.Armor[i].ItemId = terget_user.Equip.Armor[i].ItemId;
                  newFriend.Equips.Armor[i].ItemNum = terget_user.Equip.Armor[i].ItemNum;
                  newFriend.Equips.Armor[i].AppendProperty = terget_user.Equip.Armor[i].AppendProperty;
                }
                /// skills
                newFriend.Skills = new SkillInfo();
                int skills_ct = terget_user.Skill != null ? terget_user.Skill.Skills.Count : 0;
                for (int i = 0; i < skills_ct; i++) {
                  newFriend.Skills.Skills[i] = new SkillDataInfo();
                  newFriend.Skills.Skills[i].ID = terget_user.Skill.Skills[i].ID;
                  newFriend.Skills.Skills[i].Level = terget_user.Skill.Skills[i].Level;
                  newFriend.Skills.Skills[i].Postions.Presets[0] = terget_user.Skill.Skills[i].Postions.Presets[0];
                }*/
                return newFriend;
            }
            return null;
        }
        private ArkCrossEngineMessage.FriendInfoForMsg GenerateFriendMsg(UserInfo terget_user)
        {
            if (null != terget_user)
            {
                ArkCrossEngineMessage.FriendInfoForMsg friend_msg = new ArkCrossEngineMessage.FriendInfoForMsg();
                friend_msg.Guid = terget_user.Guid;
                friend_msg.Nickname = terget_user.Nickname;
                friend_msg.HeroId = terget_user.HeroId;
                friend_msg.Level = terget_user.Level;
                friend_msg.FightingScore = terget_user.FightingScore;
                friend_msg.IsOnline = false;
                friend_msg.IsBlack = false;
                if (UserState.Online == terget_user.CurrentState)
                {
                    friend_msg.IsOnline = true;
                }
                /*
                /// equips
                int equips_ct = terget_user.Equip != null ? terget_user.Equip.Armor.Length : 0;
                friend_msg.Equipments = new ItemDataMsg[equips_ct];
                for (int i = 0; i < equips_ct; i++) {
                  friend_msg.Equipments[i].ItemId = terget_user.Equip.Armor[i].ItemId;
                  friend_msg.Equipments[i].Level = terget_user.Equip.Armor[i].Level;
                  friend_msg.Equipments[i].Num = terget_user.Equip.Armor[i].ItemNum;
                  friend_msg.Equipments[i].AppendProperty = terget_user.Equip.Armor[i].AppendProperty;
                }
                /// skills
                int skills_ct = terget_user.Skill != null ? terget_user.Skill.Skills.Count : 0;
                friend_msg.SkillInfo = new SkillDataInfo[skills_ct];
                for (int i = 0; i < skills_ct; i++) {
                  friend_msg.SkillInfo[i].ID = terget_user.Skill.Skills[i].ID;
                  friend_msg.SkillInfo[i].Level = terget_user.Skill.Skills[i].Level;
                  friend_msg.SkillInfo[i].Postions.Presets[0] = terget_user.Skill.Skills[i].Postions.Presets[0];
                }*/
                return friend_msg;
            }
            return null;
        }
        private List<ArkCrossEngineMessage.FriendInfoForMsg> GenerateFriendMsg(List<FriendInfo> user_list)
        {
            List<ArkCrossEngineMessage.FriendInfoForMsg> userinfo_list_ = new List<ArkCrossEngineMessage.FriendInfoForMsg>();
            int ct = user_list.Count;
            for (int i = 0; i < ct; i++)
            {
                ArkCrossEngineMessage.FriendInfoForMsg assit_info = new ArkCrossEngineMessage.FriendInfoForMsg();
                assit_info.Guid = user_list[i].Guid;
                assit_info.Nickname = user_list[i].Nickname;
                assit_info.HeroId = user_list[i].HeroId;
                assit_info.Level = user_list[i].Level;
                assit_info.FightingScore = user_list[i].FightingScore;
                assit_info.IsOnline = user_list[i].IsOnline;
                assit_info.IsBlack = user_list[i].IsBlack;
                userinfo_list_.Add(assit_info);
            }
            return userinfo_list_;
        }
        internal void AddFriendByGuid(ulong guid, ulong target_guid)
        {
            UserInfo target = GetUserInfo(target_guid);
            if (null != target)
            {
                AddFriend(guid, target.Nickname);
            }
        }
        internal void AddFriend(ulong guid, string target_nick)
        {
            UserInfo user = GetUserInfo(guid);
            if (user != null)
            {
                bool ret = true;
                JsonMessageWithGuid afResultMsg = new JsonMessageWithGuid(JsonMessageID.AddFriendResult);
                afResultMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_AddFriendResult protoData = new ArkCrossEngineMessage.Msg_LC_AddFriendResult();
                protoData.m_Result = (int)AddFriendResult.ERROR;
                if (null != user.FriendInfos && user.FriendInfos.Count < FriendInfo.c_Friend_Max)
                {
                    if (target_nick.Equals(user.Nickname))
                    {
                        ret = false;
                        protoData.m_Result = (int)AddFriendResult.ADD_PLAYERSELF_ERROR;
                    }
                    lock (user.Lock)
                    {
                        foreach (FriendInfo info in user.FriendInfos.Values)
                        {
                            if (target_nick.Equals(info.Nickname))
                            {
                                ret = false;
                                protoData.m_Result = (int)AddFriendResult.ADD_OWN_ERROR;
                                break;
                            }
                        }
                    }
                    if (ret)
                    {
                        ulong t_guid_ = 0;
                        if (target_nick.Length > 0)
                        {
                            t_guid_ = GetGuidByNickname(target_nick);
                        }
                        if (t_guid_ > 0)
                        {
                            UserInfo target_user = GetUserInfo(t_guid_);
                            if (null != target_user && target_user.LogicServerId == user.LogicServerId)
                            {
                                FriendInfo newFriend = GenerateFriendInfo(guid, target_user);
                                lock (user.Lock)
                                {
                                    user.FriendInfos.AddOrUpdate(t_guid_, newFriend, (g, u) => newFriend);
                                }
                                protoData.m_FriendInfo = GenerateFriendMsg(target_user);
                                protoData.m_TargetNick = target_user.Nickname;
                                protoData.m_Result = (int)AddFriendResult.ADD_SUCCESS;
                                /// notice target
                                JsonMessageWithGuid target_afResultMsg = new JsonMessageWithGuid(JsonMessageID.AddFriendResult);
                                target_afResultMsg.m_Guid = t_guid_;
                                ArkCrossEngineMessage.Msg_LC_AddFriendResult assit_proto_data = new ArkCrossEngineMessage.Msg_LC_AddFriendResult();
                                assit_proto_data.m_TargetNick = user.Nickname;
                                assit_proto_data.m_Result = (int)AddFriendResult.ADD_NOTICE;
                                target_afResultMsg.m_ProtoData = assit_proto_data;
                                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, target_afResultMsg);
                            }
                            else
                            {
                                protoData.m_Result = (int)AddFriendResult.ADD_NONENTITY_ERROR;
                            }
                        }
                        else
                        {
                            protoData.m_Result = (int)AddFriendResult.ADD_NONENTITY_ERROR;
                        }
                    }
                }
                else
                {
                    protoData.m_Result = (int)AddFriendResult.ADD_OVERFLOW;
                }
                afResultMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, afResultMsg);
            }
        }
        internal void ConfirmFriend(ulong guid, ulong target_guid)
        {
            UserInfo user = GetUserInfo(guid);
            if (user != null)
            {
                if (guid.Equals(target_guid))
                {
                    return;
                }
                lock (user.Lock)
                {
                    foreach (FriendInfo info in user.FriendInfos.Values)
                    {
                        if (info.Guid.Equals(target_guid))
                        {
                            return;
                        }
                    }
                }
                UserInfo target_user = GetUserInfo(target_guid);
                if (null != target_user && target_user.LogicServerId == user.LogicServerId)
                {
                    FriendInfo newFriend = GenerateFriendInfo(guid, target_user);
                    lock (user.Lock)
                    {
                        user.FriendInfos.AddOrUpdate(target_guid, newFriend, (g, u) => newFriend);
                    }
                    JsonMessageWithGuid afResultMsg = new JsonMessageWithGuid(JsonMessageID.AddFriendResult);
                    afResultMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_AddFriendResult protoData = new ArkCrossEngineMessage.Msg_LC_AddFriendResult();
                    protoData.m_FriendInfo = GenerateFriendMsg(target_user);
                    protoData.m_TargetNick = target_user.Nickname;
                    protoData.m_Result = (int)AddFriendResult.ADD_SUCCESS;
                    afResultMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, afResultMsg);
                }
            }
        }
        internal void DelFriend(ulong guid, ulong target_guid)
        {
            UserInfo user = GetUserInfo(guid);
            if (user != null && target_guid > 0)
            {
                DelFriendResult dfr = DelFriendResult.ERROR;
                lock (user.Lock)
                {
                    FriendInfo fi = null;
                    if (user.FriendInfos.TryRemove(target_guid, out fi))
                    {
                        dfr = DelFriendResult.DEL_SUCCESS;
                    }
                }
                JsonMessageWithGuid dfResultMsg = new JsonMessageWithGuid(JsonMessageID.DelFriendResult);
                dfResultMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_DelFriendResult protoData = new ArkCrossEngineMessage.Msg_LC_DelFriendResult();
                protoData.m_TargetGuid = target_guid;
                protoData.m_Result = (int)dfr;
                dfResultMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, dfResultMsg);
            }
        }
        internal void FriendList(ulong guid)
        {
            UserInfo user = GetUserInfo(guid);
            if (user != null)
            {
                JsonMessageWithGuid flMsg = new JsonMessageWithGuid(JsonMessageID.SyncFriendList);
                flMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_SyncFriendList protoData = new ArkCrossEngineMessage.Msg_LC_SyncFriendList();
                lock (user.Lock)
                {
                    foreach (FriendInfo info in user.FriendInfos.Values)
                    {
                        ArkCrossEngineMessage.FriendInfoForMsg assit_info = new ArkCrossEngineMessage.FriendInfoForMsg();
                        UserInfo friend_info = GetUserInfo(info.Guid);
                        if (friend_info != null/*&& friend_info.CurrentState != UserState.DropOrOffline*/)
                        {
                            info.Level = friend_info.Level;
                            info.FightingScore = friend_info.FightingScore;
                            info.IsOnline = true;
                        }
                        else
                        {
                            info.IsOnline = false;
                        }
                        assit_info.Guid = info.Guid;
                        assit_info.Nickname = info.Nickname;
                        assit_info.HeroId = info.HeroId;
                        assit_info.Level = info.Level;
                        assit_info.FightingScore = info.FightingScore;
                        assit_info.IsOnline = info.IsOnline;
                        assit_info.IsBlack = info.IsBlack;
                        protoData.m_FriendInfo.Add(assit_info);
                    }
                }
                flMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, flMsg);
            }
        }
        private void SyncFriendInfo(ulong guid, List<FriendInfo> fi_list)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user)
            {
                JsonMessageWithGuid qfiMsg = new JsonMessageWithGuid(JsonMessageID.QueryFriendInfoResult);
                qfiMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_QueryFriendInfoResult protoData = new ArkCrossEngineMessage.Msg_LC_QueryFriendInfoResult();
                List<ArkCrossEngineMessage.FriendInfoForMsg> assit_user_list_ = GenerateFriendMsg(fi_list);
                if (assit_user_list_.Count > 0)
                {
                    protoData.m_Friends.AddRange(assit_user_list_);
                }
                qfiMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, qfiMsg);
            }
        }
        internal void HandleQueryFriendInfo(ulong guid,
                                          QueryType query_type,
                                          string target_name,
                                          int target_level,
                                          int target_score,
                                          int target_fortune,
                                          GenderType gender_type)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user)
                return;
            if (QueryType.Name == query_type)
            {
                List<FriendInfo> fi_list = new List<FriendInfo>();
                foreach (FriendInfo value in user.FriendInfos.Values)
                {
                    if (value.Nickname.Equals(target_name))
                    {
                        fi_list.Add(value);
                        break;
                    }
                }
                SyncFriendInfo(guid, fi_list);
            }
            else if (QueryType.Score == query_type)
            {
                List<FriendInfo> fi_list = new List<FriendInfo>();
                const int score_offset = 100;
                int score_min = target_score - score_offset;
                int score_max = target_score + score_offset;
                foreach (FriendInfo value in user.FriendInfos.Values)
                {
                    if (value.FightingScore >= score_min && value.FightingScore <= score_max)
                    {
                        fi_list.Add(value);
                    }
                }
                SyncFriendInfo(guid, fi_list);
            }
            else if (QueryType.Level == query_type)
            {
                List<FriendInfo> fi_list = new List<FriendInfo>();
                foreach (FriendInfo value in user.FriendInfos.Values)
                {
                    if (value.Level == target_level)
                    {
                        fi_list.Add(value);
                    }
                }
                SyncFriendInfo(guid, fi_list);
            }
            else if (QueryType.Fortune == query_type)
            {
            }
            else if (QueryType.Gender == query_type)
            {
            }
        }
        internal void HandleDiscardItem(ulong guid, List<int> itemId, List<int> propertyId)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                const int c_random_max = 101;
                bool ret = false;
                List<int> del_items = new List<int>();
                List<int> del_property = new List<int>();
                for (int i = 0; i < itemId.Count; i++)
                {
                    ItemInfo item = user.ItemBag.GetItemData(itemId[i], propertyId[i]);
                    if (null != item && null != item.ItemConfig
                      && item.AppendProperty == propertyId[i])
                    {
                        int gain_money = item.ItemConfig.m_SellingPrice;
                        int incr = (gain_money * item.ItemNum);
                        IncreaseAsset(guid, incr, GainAssetType.SellItem, AssetType.Money, GainConsumePos.Bag.ToString());
                        //gold income
                        int gain_gold = item.ItemConfig.m_SellGainGoldNum;
                        float gain_prob = item.ItemConfig.m_SellGainGoldProb;
                        int income_max = user.c_SellItemGainGoldMax - user.CurSellItemGoldIncome;
                        if (gain_gold > 0 && gain_prob > 0 && income_max > 0)
                        {
                            float random_num = (float)(CrossEngineHelper.Random.Next(0, c_random_max) / 100.0f);
                            if (random_num <= gain_prob)
                            {
                                int assit_income = (income_max - gain_gold) > 0 ? gain_gold : income_max;
                                if (assit_income > 0)
                                {
                                    user.CurSellItemGoldIncome += assit_income;
                                    int gold_incr = assit_income;
                                    IncreaseAsset(guid, gold_incr, GainAssetType.SellItem, AssetType.Glod, GainConsumePos.Bag.ToString());
                                }
                            }
                        }
                        ConsumeItem(guid, item, item.ItemNum, item.ItemConfig.m_CanWear ? GainItemType.Equipment : GainItemType.Props,
                        ConsumeItemWay.SellItem, true, GainConsumePos.Bag.ToString());
                        del_items.Add(itemId[i]);
                        del_property.Add(propertyId[i]);
                    }
                }
                if (del_items.Count > 0 && del_property.Count > 0
                  && del_items.Count == del_property.Count)
                {
                    ret = true;
                    JsonMessageWithGuid dirMsg = new JsonMessageWithGuid(JsonMessageID.DiscardItemResult);
                    dirMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_DiscardItemResult protoData = new ArkCrossEngineMessage.Msg_LC_DiscardItemResult();
                    for (int i = 0; i < del_items.Count; i++)
                    {
                        protoData.m_ItemId.Add(del_items[i]);
                    }
                    for (int i = 0; i < del_property.Count; i++)
                    {
                        protoData.m_PropertyId.Add(del_property[i]);
                    }
                    protoData.m_TotalIncome = user.CurSellItemGoldIncome;
                    protoData.m_Money = user.Money;
                    protoData.m_Gold = user.Gold;
                    if (ret)
                    {
                        protoData.m_Result = (int)GeneralOperationResult.LC_Succeed;
                    }
                    else
                    {
                        protoData.m_Result = (int)GeneralOperationResult.LC_Failure_Unknown;
                    }
                    dirMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, dirMsg);
                }
            }
        }
        internal void HandleMountEquipment(ulong guid, int item_id, int property_id, int equipment_pos)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && null != user.ItemBag && null != user.Equip)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                ItemInfo mountItem = user.ItemBag.GetItemData(item_id, property_id);
                if (null != mountItem && null != mountItem.ItemConfig)
                {
                    if (false != mountItem.ItemConfig.m_CanWear
                    && equipment_pos == mountItem.ItemConfig.m_WearParts)
                    {
                        ItemInfo exsitItem = user.Equip.GetEquipmentData(equipment_pos);
                        if (null != exsitItem && exsitItem.Level > 0 && user.Level >= mountItem.ItemConfig.m_WearLevel)
                        {
                            if (0 != exsitItem.ItemId)
                            {
                                ItemLevelupConfig item_levelup_Info = ItemLevelupConfigProvider.Instance.GetDataById(exsitItem.Level);
                                if (null != item_levelup_Info)
                                {
                                    if (item_levelup_Info.m_ChangeEquipCost <= user.Money)
                                    {
                                        int consume = item_levelup_Info.m_ChangeEquipCost;
                                        ConsumeAsset(guid, consume, ConsumeAssetType.MountEquipment, AssetType.Money, GainConsumePos.Equipment.ToString());
                                        mountItem.Level = exsitItem.Level;
                                        exsitItem.Level = 1;
                                        result = GeneralOperationResult.LC_Succeed;
                                    }
                                    else
                                    {
                                        result = GeneralOperationResult.LC_Failure_CostError;
                                    }
                                }
                                else
                                {
                                    result = GeneralOperationResult.LC_Failure_LevelError;
                                }
                            }
                            else
                            {
                                result = GeneralOperationResult.LC_Succeed;
                            }
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_LevelError;
                        }
                        if (GeneralOperationResult.LC_Succeed == result)
                        {
                            user.ItemBag.ReduceItemData(mountItem);
                            user.Equip.SetEquipmentData(equipment_pos, null);
                            user.Equip.SetEquipmentData(equipment_pos, mountItem);
                            if (null != exsitItem && 0 != exsitItem.ItemId)
                            {
                                user.ItemBag.AddItemData(exsitItem);
                            }
                        }
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_Position;
                    }
                }
                else
                {
                    result = GeneralOperationResult.LC_Failure_Unknown;
                }

                JsonMessageWithGuid merMsg = new JsonMessageWithGuid(JsonMessageID.MountEquipmentResult);
                merMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_MountEquipmentResult protoData = new ArkCrossEngineMessage.Msg_LC_MountEquipmentResult();
                protoData.m_ItemID = item_id;
                protoData.m_PropertyID = property_id;
                protoData.m_EquipPos = equipment_pos;
                protoData.m_Result = (int)result;
                merMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, merMsg);
            }
        }
        internal void HandleUnmountEquipment(ulong guid, int equipment_pos)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.Equip != null && user.ItemBag != null)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                ItemInfo exsitItem = user.Equip.GetEquipmentData(equipment_pos);
                if (null != exsitItem && null != exsitItem.ItemConfig)
                {
                    user.Equip.SetEquipmentData(equipment_pos, null);
                    if (user.ItemBag.ItemCount < ItemBag.c_MaxItemNum)
                    {
                        user.ItemBag.AddItemData(exsitItem);
                    }
                    result = GeneralOperationResult.LC_Succeed;
                }
                JsonMessageWithGuid uerMsg = new JsonMessageWithGuid(JsonMessageID.UnmountEquipmentResult);
                uerMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_UnmountEquipmentResult protoData = new ArkCrossEngineMessage.Msg_LC_UnmountEquipmentResult();
                protoData.m_EquipPos = equipment_pos;
                protoData.m_Result = (int)result;
                uerMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, uerMsg);
            }
        }
        internal void HandleMountSkill(ulong guid, int preset_index, int skill_id, int slot_position)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.Skill != null && (preset_index >= 0 && preset_index < PresetInfo.PresetNum))
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                if (null != user.Skill.GetSkillDataByID(skill_id) && user.Skill.GetSkillDataByID(skill_id).Level > 0)
                {
                    user.Skill.MountSkill(preset_index, skill_id, (SlotPosition)slot_position);
                    result = GeneralOperationResult.LC_Succeed;
                }
                else
                {
                    result = GeneralOperationResult.LC_Failure_NotUnLock;
                }
                JsonMessageWithGuid msrMsg = new JsonMessageWithGuid(JsonMessageID.MountSkillResult);
                msrMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_MountSkillResult protoData = new ArkCrossEngineMessage.Msg_LC_MountSkillResult();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_SlotPos = slot_position;
                protoData.m_Result = (int)result;
                msrMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, msrMsg);
            }
        }
        internal void HandleUnmountSkill(ulong guid, int preset_index, int slot_position)
        {
            GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.Skill != null && (preset_index >= 0 && preset_index < PresetInfo.PresetNum))
            {
                user.Skill.UnmountSkill(preset_index, (SlotPosition)slot_position);
                result = GeneralOperationResult.LC_Succeed;
            }
            JsonMessageWithGuid usrMsg = new JsonMessageWithGuid(JsonMessageID.UnmountSkillResult);
            usrMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_UnmountSkillResult protoData = new ArkCrossEngineMessage.Msg_LC_UnmountSkillResult();
            protoData.m_PresetIndex = preset_index;
            protoData.m_SlotPos = slot_position;
            protoData.m_Result = (int)result;
            usrMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, usrMsg);
        }
        internal void HandleUpgradeSkill(ulong guid, int preset_index, int skill_id, bool allow_cost_gold)
        {
            if (allow_cost_gold)
                return;
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.Skill != null && (preset_index >= 0 && preset_index < PresetInfo.PresetNum))
            {
                lock (user.Lock)
                {
                    GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                    SkillDataInfo info = user.Skill.GetSkillDataByID(skill_id);
                    if (null != info && info.Level > 0)
                    {
                        SkillLogicData skill_data = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skill_id) as SkillLogicData;
                        SkillLevelupConfig skill_levelup_data = SkillLevelupConfigProvider.Instance.GetDataById(info.Level) as SkillLevelupConfig;
                        if (null != skill_data && null != skill_levelup_data && info.Level < user.Level
                          && skill_data.LevelUpCostType > 0 && skill_data.LevelUpVigorType > 0
                          && skill_data.LevelUpCostType < SkillLevelupConfig.TypeNum + 1 && skill_data.LevelUpVigorType < SkillLevelupConfig.TypeNum + 1)
                        {
                            int cost_vigor = skill_levelup_data.m_VigorList[skill_data.LevelUpVigorType - 1];
                            if (cost_vigor > user.Vigor)
                            {
                                result = GeneralOperationResult.LC_Failure_VigorError;
                            }
                            if (GeneralOperationResult.LC_Failure_VigorError != result)
                            {
                                bool ret = false;
                                int cost_money = skill_levelup_data.m_TypeList[skill_data.LevelUpCostType - 1];
                                if (cost_money <= user.Money)
                                {
                                    int consume = cost_money;
                                    ConsumeAsset(guid, consume, ConsumeAssetType.UpgradeSkill, AssetType.Money, GainConsumePos.Skill.ToString());
                                    ConsumeAsset(guid, cost_vigor, ConsumeAssetType.UpgradeSkill, AssetType.Vigor, GainConsumePos.Skill.ToString());
                                    ret = true;
                                }
                                else
                                {
                                    if (allow_cost_gold)
                                    {
                                        int replenish_money = cost_money - (int)user.Money;
                                        int cost_gold = (int)(replenish_money * skill_levelup_data.m_Rate);
                                        if (cost_gold <= user.Gold && 0 != cost_gold)
                                        {
                                            int consume = user.Money;
                                            ConsumeAsset(guid, consume, ConsumeAssetType.UpgradeSkill, AssetType.Money, GainConsumePos.Skill.ToString());
                                            int gold_consume = cost_gold;
                                            ConsumeAsset(guid, gold_consume, ConsumeAssetType.UpgradeSkill, AssetType.Glod, GainConsumePos.Skill.ToString());
                                            ConsumeAsset(guid, cost_vigor, ConsumeAssetType.UpgradeSkill, AssetType.Vigor, GainConsumePos.Skill.ToString());
                                            ret = true;
                                        }
                                    }
                                }
                                if (ret)
                                {
                                    user.Skill.IntensifySkill(skill_id);
                                    result = GeneralOperationResult.LC_Succeed;
                                }
                                else
                                {
                                    result = GeneralOperationResult.LC_Failure_CostError;
                                }
                            }
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_LevelError;
                        }
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_NotUnLock;
                    }
                    JsonMessageWithGuid usrMsg = new JsonMessageWithGuid(JsonMessageID.UpgradeSkillResult);
                    usrMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_UpgradeSkillResult protoData = new ArkCrossEngineMessage.Msg_LC_UpgradeSkillResult();
                    protoData.m_PresetIndex = preset_index;
                    protoData.m_SkillID = skill_id;
                    protoData.m_AllowCostGold = allow_cost_gold;
                    protoData.m_Money = user.Money;
                    protoData.m_Gold = user.Gold;
                    protoData.m_Vigor = user.Vigor;
                    protoData.m_Result = (int)result;
                    usrMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, usrMsg);
                }
            }
        }
        internal void HandleUnlockSkill(ulong guid, int preset_index, int skill_id, int user_level)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.Skill != null && (preset_index >= 0 && preset_index < PresetInfo.PresetNum))
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                SkillDataInfo skill_info = user.Skill.GetSkillDataByID(skill_id);
                SkillLogicData skill_data = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skill_id) as SkillLogicData;
                if (null != skill_info && skill_info.Level <= 0)
                {
                    if (null != skill_data && user_level >= skill_data.ActivateLevel)
                    {
                        user.Skill.UnlockSkill(skill_id);
                        result = GeneralOperationResult.LC_Succeed;
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_LevelError;
                    }
                    JsonMessageWithGuid usrMsg = new JsonMessageWithGuid(JsonMessageID.UnlockSkillResult);
                    usrMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_UnlockSkillResult protoData = new ArkCrossEngineMessage.Msg_LC_UnlockSkillResult();
                    protoData.m_PresetIndex = preset_index;
                    protoData.m_SkillID = skill_id;
                    protoData.m_UserLevel = user_level;
                    protoData.m_Result = (int)result;
                    usrMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, usrMsg);
                }
            }
        }
        internal void HandleSwapSkill(ulong guid, int preset_index, int skill_id, int source_pos, int target_pos)
        {
            GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.Skill != null && (preset_index >= 0 && preset_index < PresetInfo.PresetNum))
            {
                user.Skill.SwapSkill(preset_index, skill_id, (SlotPosition)source_pos, (SlotPosition)target_pos);
                result = GeneralOperationResult.LC_Succeed;
            }

            JsonMessageWithGuid ssrMsg = new JsonMessageWithGuid(JsonMessageID.SwapSkillResult);
            ssrMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_SwapSkillResult protoData = new ArkCrossEngineMessage.Msg_LC_SwapSkillResult();
            protoData.m_PresetIndex = preset_index;
            protoData.m_SkillID = skill_id;
            protoData.m_SourcePos = source_pos;
            protoData.m_TargetPos = target_pos;
            protoData.m_Result = (int)result;
            ssrMsg.m_ProtoData = protoData;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, ssrMsg);
        }
        internal void HandleCompoundEquip(ulong guid, int partid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && null != user.ItemBag)
            {
                ItemConfig item_data = ItemConfigProvider.Instance.GetDataById(partid);
                if (null == item_data)
                    return;
                if (null == item_data.m_CompoundItemId)
                    return;
                int item_id = item_data.m_CompoundItemId.Count > 0 ? item_data.m_CompoundItemId[0] : 0;
                if (item_id > 0)
                {
                    GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                    int gain_item_id = 0;
                    bool ret = true;
                    ItemInfo part_info = user.ItemBag.GetItemData(partid, 0);
                    if (null != part_info)
                    {
                        ItemCompoundConfig cpd_data = ItemCompoundConfigProvider.Instance.GetDataById(item_id);
                        if (null != cpd_data && partid == cpd_data.m_PartId)
                        {
                            if (part_info.ItemNum < cpd_data.m_PartNum)
                            {
                                ret = false;
                                result = GeneralOperationResult.LC_Failure_PartNumError;
                            }
                            else
                            {
                                if (cpd_data.m_MaterialNum > 0)
                                {
                                    ItemInfo material_info = user.ItemBag.GetItemData(cpd_data.m_MaterialId, 0);
                                    if (null != material_info)
                                    {
                                        if (material_info.ItemNum < cpd_data.m_MaterialNum)
                                        {
                                            ret = false;
                                            result = GeneralOperationResult.LC_Failure_MaterialNumError;
                                        }
                                    }
                                    else
                                    {
                                        ret = false;
                                        result = GeneralOperationResult.LC_Failure_MaterialNumError;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ret = false;
                        result = GeneralOperationResult.LC_Failure_PartNumError;
                    }
                    if (ret)
                    {
                        ItemCompoundConfig cpd_data = ItemCompoundConfigProvider.Instance.GetDataById(item_id);
                        if (null != cpd_data)
                        {
                            lock (user.Lock)
                            {
                                ConsumeItem(guid, part_info, cpd_data.m_PartNum, part_info.ItemConfig.m_CanWear ? GainItemType.Equipment : GainItemType.Props,
                                ConsumeItemWay.CompoundItem, part_info.ItemNum == cpd_data.m_PartNum ? true : false, GainConsumePos.Compound.ToString());
                                ItemInfo material_info = user.ItemBag.GetItemData(cpd_data.m_MaterialId, 0);
                                if (null != material_info && cpd_data.m_MaterialNum > 0)
                                {
                                    ConsumeItem(guid, material_info, cpd_data.m_MaterialNum, material_info.ItemConfig.m_CanWear ? GainItemType.Equipment : GainItemType.Props,
                                    ConsumeItemWay.CompoundItem, material_info.ItemNum == cpd_data.m_MaterialNum ? true : false, GainConsumePos.Compound.ToString());
                                }
                                int rnd = CrossEngineHelper.Random.Next(0, item_data.m_CompoundItemId.Count);
                                DoAddItem(guid, item_data.m_CompoundItemId[rnd], 1, GainConsumePos.Compound.ToString());
                                gain_item_id = item_data.m_CompoundItemId[rnd];
                                result = GeneralOperationResult.LC_Succeed;
                            }
                        }
                    }
                    JsonMessageWithGuid uirMsg = new JsonMessageWithGuid(JsonMessageID.CompoundEquipResult);
                    uirMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_CompoundEquipResult protoData = new ArkCrossEngineMessage.Msg_LC_CompoundEquipResult();
                    protoData.m_PartId = partid;
                    protoData.m_ItemId = gain_item_id;
                    protoData.m_Result = (int)result;
                    uirMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, uirMsg);
                }
            }
        }
        internal void HandleUpgradeItem(ulong guid, int position, int item_id, bool allow_cost_gold)
        {
            if (allow_cost_gold)
                return;
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && null != user.Equip)
            {
                lock (user.Lock)
                {
                    GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                    ItemInfo op_item = user.Equip.GetEquipmentData(position);
                    if (null != op_item && null != op_item.ItemConfig && item_id == op_item.ItemId && op_item.ItemConfig.m_CanUpgrade)
                    {
                        bool ret = false;
                        int item_max_level = ItemLevelupConfigProvider.Instance.GetDataCount();
                        int item_upgrade_level = op_item.Level + 1;
                        if (item_upgrade_level <= item_max_level && item_upgrade_level <= user.Level)
                        {
                            ItemLevelupConfig item_levelup_Info = ItemLevelupConfigProvider.Instance.GetDataById(op_item.Level);
                            if (null != item_levelup_Info)
                            {
                                int cost_money = item_levelup_Info.m_PartsList[op_item.ItemConfig.m_WearParts];
                                if (cost_money <= user.Money)
                                {
                                    int consume = cost_money;
                                    ConsumeAsset(guid, consume, ConsumeAssetType.UpgradeItem, AssetType.Money, GainConsumePos.Equipment.ToString());
                                    ret = true;
                                }
                                else
                                {
                                    if (allow_cost_gold)
                                    {
                                        int replenish_money = cost_money - (int)user.Money;
                                        int cost_gold = (int)(replenish_money * item_levelup_Info.m_Rate);
                                        if (cost_gold <= user.Gold && 0 != cost_gold)
                                        {
                                            user.Money = 0;
                                            int consume = user.Money;
                                            ConsumeAsset(guid, consume, ConsumeAssetType.UpgradeItem, AssetType.Money, GainConsumePos.Equipment.ToString());
                                            int gold_consume = cost_gold;
                                            ConsumeAsset(guid, gold_consume, ConsumeAssetType.UpgradeItem, AssetType.Glod, GainConsumePos.Equipment.ToString());
                                            ret = true;
                                        }
                                    }
                                }
                                if (ret)
                                {
                                    op_item.Level = item_upgrade_level;
                                    user.Equip.SetEquipmentData(position, null);
                                    user.Equip.SetEquipmentData(position, op_item);
                                    result = GeneralOperationResult.LC_Succeed;
                                }
                                else
                                {
                                    result = GeneralOperationResult.LC_Failure_CostError;
                                }
                            }
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_LevelError;
                        }
                        JsonMessageWithGuid uirMsg = new JsonMessageWithGuid(JsonMessageID.UpgradeItemResult);
                        uirMsg.m_Guid = guid;
                        ArkCrossEngineMessage.Msg_LC_UpgradeItemResult protoData = new ArkCrossEngineMessage.Msg_LC_UpgradeItemResult();
                        protoData.m_Position = position;
                        protoData.m_Money = user.Money;
                        protoData.m_Gold = user.Gold;
                        protoData.m_Result = (int)result;
                        uirMsg.m_ProtoData = protoData;
                        JsonMessageDispatcher.SendDcoreMessage(user.NodeName, uirMsg);
                    }
                }
            }
        }
        internal void HandleSaveSkillPreset(ulong guid, int preset_index)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.Skill != null)
            {
                if (preset_index >= 0 && preset_index < 4)
                {
                    user.Skill.CurPresetIndex = preset_index;
                }
            }
        }
        internal void HandleStageClear(ulong guid, int hitCount, int maxMultHitCount, int hp, int mp, int gold, int matchKey)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user) return;
            int sceneId = user.CurrentBattleInfo.SceneID;
            if (user.CurrentBattleInfo.IsClearing)
            {
                // 已经结算时，直接回复结算消息
                if (user.CurrentBattleInfo.LastMsg != null)
                {
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, user.CurrentBattleInfo.LastMsg);
                }
                return;
            }
            Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
            JsonMessageWithGuid scrMsg = new JsonMessageWithGuid(JsonMessageID.StageClearResult);
            scrMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_StageClearResult protoData = new ArkCrossEngineMessage.Msg_LC_StageClearResult();
            protoData.m_ResultCode = (int)GeneralOperationResult.LC_Succeed;
            if (null != cfg)
            {
                // debug模式下关掉检查
                if (!GlobalVariables.Instance.IsDebug)
                {
                    if (cfg.m_CostStamina > user.CurStamina)
                    {
                        // 需要的体力值大于现有的体力值
                        protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_CostError;
                    }
                    if (TimeUtility.GetServerMilliseconds() - user.CurrentBattleInfo.StartTime < 15 * 1000)
                    {
                        // 检查通关时间，小于15秒无法完成
                        protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Time;
                    }
                    int preSceneId = SceneConfigProvider.Instance.GetPreSceneId(sceneId);
                    if (-1 != preSceneId && !user.SceneData.ContainsKey(preSceneId))
                    {
                        // 完成未解锁的关卡
                        protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_NotUnLock;
                    }
                    if (cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE && cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_ELITE)
                    {
                        // 检查精英关卡通关次数
                        if (user.GetCompletedSceneCount(sceneId) >= 3)
                        {
                            protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_NoFightCount;
                        }
                    }
                    if (cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE && user.CurrentBattleInfo.MatchKey != matchKey && user.SceneData.Count > 0)
                    {
                        protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failuer_NotMatch;
                    }
                }
                if (cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE)
                {
                    if (UserState.Room != user.CurrentState)
                        user.CurrentState = UserState.Online;
                }
                if (user.CurrentBattleInfo.StartTime <= 0 && user.SceneData.Count > 0)
                {
                    // 没有开始， 直接请求完成
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Unknown;
                }
                if (protoData.m_ResultCode == (int)GeneralOperationResult.LC_Succeed)
                {
                    int stamina_cons = cfg.m_CostStamina;
                    if (cfg.m_CostStamina > user.CurStamina)
                    {
                        stamina_cons = user.CurStamina;
                    }
                    ConsumeAsset(guid, stamina_cons, ConsumeAssetType.StageClear, AssetType.Stamina, sceneId.ToString());
                    if ((int)SceneSubTypeEnum.TYPE_EXPEDITION == cfg.m_SubType && null != user.Expedition)
                    {
                        user.Expedition.Hp = hp;
                        user.Expedition.Mp = mp;
                    }
                    if (!IsGoldScene(sceneId))
                    {
                        if (user.CurrentBattleInfo.SumGold >= gold)
                        {
                            int incr = gold;
                            IncreaseAsset(guid, incr, GainAssetType.StageClear, AssetType.Money, sceneId.ToString());
                        }
                        else
                        {
                            LogSystem.Warn("user {0} get more money {1} than sumgold {2}", user.AccountId, gold, user.CurrentBattleInfo.SumGold);
                            gold = user.CurrentBattleInfo.SumGold;
                            int incr = gold;
                            IncreaseAsset(guid, incr, GainAssetType.StageClear, AssetType.Money, sceneId.ToString());
                        }
                    }
                    // 结算副本奖励,记录战斗信息
                    StageClearInfo info = new StageClearInfo();
                    AddExpPoints(guid, user.CurrentBattleInfo.Exp, sceneId.ToString());
                    // 通关物品
                    if (user.CurrentBattleInfo.RewardItemId > 0)
                    {
                        DoAddItem(guid, user.CurrentBattleInfo.RewardItemId, user.CurrentBattleInfo.RewardItemCount, sceneId.ToString());
                    }
                    info.Level = user.Level;
                    info.SceneId = sceneId;
                    info.HitCount = hitCount;
                    info.MaxMultHitCount = maxMultHitCount;
                    info.Duration = TimeUtility.GetServerMilliseconds() - user.CurrentBattleInfo.StartTime;
                    info.DeadCount = user.CurrentBattleInfo.DeadCount;
                    // 记录通关信息
                    int curSceneStar = user.GetSceneInfo(sceneId);
                    int newSceneStar = -1;
                    int completedRewardId = -1;
                    if ((int)SceneSubTypeEnum.TYPE_STORY == cfg.m_SubType && 0 >= curSceneStar)
                    {
                        newSceneStar = 1;
                    }
                    if ((int)SceneSubTypeEnum.TYPE_ELITE == cfg.m_SubType)
                    {
                        if (0 >= curSceneStar)
                        {
                            newSceneStar = 1;
                        }
                        else if (1 == curSceneStar)
                        {
                            if (cfg.m_CompletedTime * 1000 >= TimeUtility.GetServerMilliseconds() - user.CurrentBattleInfo.StartTime)
                            {
                                newSceneStar = 2;
                            }
                        }
                        else if (2 == curSceneStar)
                        {
                            if (cfg.m_CompletedHitCount >= hitCount)
                            {
                                newSceneStar = 3;
                            }
                        }
                        else if (3 == curSceneStar)
                        {
                        }
                    }
                    if (newSceneStar > curSceneStar)
                    {
                        lock (user.Lock)
                        {
                            user.SetSceneInfo(sceneId, newSceneStar);
                        }
                        completedRewardId = cfg.GetCompletedRewardId(newSceneStar);
                    }
                    lock (user.Lock)
                    {
                        user.AddCompletedSceneCount(sceneId);
                    }
                    if (-1 != completedRewardId)
                    {
                        // 结算首次通关奖励
                        Data_SceneDropOut rewardConfig = SceneConfigProvider.Instance.GetSceneDropOutById(completedRewardId);
                        if (null != rewardConfig)
                        {
                            int incr = rewardConfig.m_GoldSum;
                            IncreaseAsset(guid, incr, GainAssetType.FinishMission, AssetType.Money, sceneId.ToString());
                            int gold_incr = rewardConfig.m_Diamond;
                            IncreaseAsset(guid, gold_incr, GainAssetType.FinishMission, AssetType.Glod, sceneId.ToString());
                            int exp_incr = rewardConfig.m_Exp;
                            IncreaseAsset(guid, exp_incr, GainAssetType.AddAssets, AssetType.Exp, sceneId.ToString());
                            // partner
                            if (rewardConfig.m_PartnerId > 0 && !user.PartnerStateInfo.IsHavePartner(rewardConfig.m_PartnerId))
                            {
                                user.PartnerStateInfo.AddPartner(rewardConfig.m_PartnerId, 1, 1);
                                JsonMessageWithGuid addPartnerMsg = new JsonMessageWithGuid(JsonMessageID.GetPartner);
                                addPartnerMsg.m_Guid = guid;
                                ArkCrossEngineMessage.Msg_LC_GetPartner addPartnerProto = new ArkCrossEngineMessage.Msg_LC_GetPartner();
                                addPartnerProto.m_PartnerId = rewardConfig.m_PartnerId;
                                addPartnerMsg.m_ProtoData = addPartnerProto;
                                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, addPartnerMsg);
                                ///
                                RecordPartnerAction(guid, rewardConfig.m_PartnerId, 1, PartnerCauseId.TollgateGain, PartnerOperateResult.Null);
                            }
                            List<int> rewardItemList = rewardConfig.GetRewardItemByHeroId(user.HeroId);
                            if (null != rewardItemList && rewardItemList.Count > 0)
                            {
                                for (int i = 0; i < rewardItemList.Count; ++i)
                                {
                                    if (rewardItemList[i] > 0)
                                    {
                                        DoAddItem(guid, rewardItemList[i], rewardConfig.m_ItemCountList[i], sceneId.ToString());
                                    }
                                }
                            }
                        }
                    }
                    List<int> completedMissions = new List<int>();
                    MissionSystem.Instance.OnStageClear(user, info, ref completedMissions);
                    protoData.m_Gold = gold;
                    if (IsMpveScene(sceneId))
                    {
                        if (IsAttempScene(sceneId))
                        {
                            if (user.CurrentBattleInfo.SumGold >= user.CurrentBattleInfo.AddGold)
                            {
                                int incr = user.CurrentBattleInfo.AddGold;
                                IncreaseAsset(guid, incr, GainAssetType.StageClear, AssetType.Money, sceneId.ToString());
                                protoData.m_Gold = incr;
                            }
                        }
                        else if (IsGoldScene(sceneId))
                        {
                            user.GoldCurAcceptedCount += 1;
                            int total_money = user.CurrentBattleInfo.TotalGold;
                            IncreaseAsset(guid, total_money, GainAssetType.StageClear, AssetType.Money, sceneId.ToString());
                            protoData.m_Gold = total_money;
                        }
                    }
                    else
                    {
                        int star = user.GetSceneInfo(sceneId);
                        /// norm log
                        AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                        if (null != accountInfo)
                        {
                            /// pvefight
                            LogSys.NormLog("PVEfight", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.pvefight, LobbyConfig.LogNormVersionStr,
                            "B4110", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level, sceneId, (int)FightingType.General, "null",
                            (int)PvefightResult.Succeed, "null", star);
                        }
                    }
                    /// teammate
                    foreach (Teammate t in user.CurrentBattleInfo.TeamMate)
                    {
                        ArkCrossEngineMessage.Msg_LC_StageClearResult.Teammate t_info = new ArkCrossEngineMessage.Msg_LC_StageClearResult.Teammate();
                        t_info.m_Nick = t.Nick;
                        t_info.m_ResId = t.ResId;
                        t_info.m_Money = t.Money;
                        protoData.m_Teammate.Add(t_info);
                    }
                    // answer to client
                    protoData.m_HitCount = hitCount;
                    protoData.m_MaxMultHitCount = maxMultHitCount;
                    protoData.m_Duration = TimeUtility.GetServerMilliseconds() - user.CurrentBattleInfo.StartTime;
                    protoData.m_ItemId = user.CurrentBattleInfo.RewardItemId;
                    protoData.m_ItemCount = user.CurrentBattleInfo.RewardItemCount;
                    protoData.m_ExpPoint = user.CurrentBattleInfo.Exp;
                    protoData.m_Hp = hp;
                    protoData.m_Mp = mp;
                    protoData.m_DeadCount = user.CurrentBattleInfo.DeadCount;
                    protoData.m_CompletedRewardId = completedRewardId;
                    protoData.m_SceneStarNum = newSceneStar;
                    int index = 0;
                    for (int i = 0; i < completedMissions.Count; ++i)
                    {
                        ArkCrossEngineMessage.Msg_LC_StageClearResult.MissionInfoForSync sync_info = new ArkCrossEngineMessage.Msg_LC_StageClearResult.MissionInfoForSync();
                        MissionInfo missionInfo = user.Mission.CompletedMissions[completedMissions[i]];
                        sync_info.m_MissionId = missionInfo.MissionId;
                        sync_info.m_IsCompleted = true;
                        sync_info.m_Progress = MissionSystem.Instance.GetMissionProgress(user, missionInfo, true);
                        protoData.m_Missions.Add(sync_info);
                        ++index;
                    }
                    protoData.m_KillNpcCount = user.CurrentBattleInfo.KillNpcCount;
                    user.CurrentBattleInfo.IsClearing = true;
                }
            }
            else
            {
                protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Unknown;
            }
            scrMsg.m_ProtoData = protoData;
            user.CurrentBattleInfo.LastMsg = scrMsg;
            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, scrMsg);
        }
        internal void HandleSweepStage(ulong guid, int sceneId, int sweepTime)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            int resultCode = (int)GeneralOperationResult.LC_Failure_Unknown;
            Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
            JsonMessageWithGuid scrMsg = new JsonMessageWithGuid(JsonMessageID.SweepStageResult);
            scrMsg.m_Guid = guid;
            ArkCrossEngineMessage.Msg_LC_SweepStageResult protoData = new ArkCrossEngineMessage.Msg_LC_SweepStageResult();
            protoData.m_SceneId = sceneId;
            if (null != user && null != cfg)
            {
                int sweepStageItemId = ItemConfigProvider.Instance.GetSweepStageItemId();
                // 检查条件 是否可以扫荡
                if (user.ItemBag.GetItemCount(sweepStageItemId, 0) >= sweepTime)
                {
                    if (null != cfg && user.CurStamina >= cfg.m_CostStamina * sweepTime)
                    {
                        if (cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE)
                        {
                            if (cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_STORY)
                            {
                                if (user.SceneData.ContainsKey(sceneId))
                                {
                                    resultCode = (int)GeneralOperationResult.LC_Succeed;
                                }
                            }
                            else if (cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_ELITE)
                            {
                                if (user.SceneData.ContainsKey(sceneId) && user.SceneData[sceneId] == 3 && user.GetCompletedSceneCount(sceneId) + sweepTime <= 3)
                                {
                                    resultCode = (int)GeneralOperationResult.LC_Succeed;
                                }
                            }
                        }
                    }
                }

                // 条件满足 可以扫荡
                if (resultCode == (int)GeneralOperationResult.LC_Succeed)
                {
                    // 扣除扫荡卷,体力,记录副本扫荡次数
                    ConsumeItem(guid, user.ItemBag.GetItemData(sweepStageItemId, 0), sweepTime, GainItemType.Props, ConsumeItemWay.SweepStage, false, GainConsumePos.StageClear.ToString());
                    ConsumeAsset(guid, cfg.m_CostStamina * sweepTime, ConsumeAssetType.StageClear, AssetType.Stamina, sceneId.ToString());
                    user.AddCompletedSceneCount(sceneId, sweepTime);
                    protoData.m_SweepItemCost = sweepTime;
                    // 计算任务奖励
                    StageClearInfo info = new StageClearInfo();
                    info.Level = user.Level;
                    info.SceneId = sceneId;
                    info.HitCount = 0;
                    info.MaxMultHitCount = 0;
                    info.Duration = 0;
                    info.DeadCount = user.CurrentBattleInfo.DeadCount;
                    info.SweepCount = sweepTime;
                    MissionSystem.Instance.CheckAndSyncMissions(user, info);
                    // 计算副本奖励
                    Data_SceneDropOut rewardConfig = SceneConfigProvider.Instance.GetSceneDropOutById(cfg.m_DropId);
                    if (null != rewardConfig)
                    {
                        protoData.m_Exp = sweepTime * rewardConfig.m_Exp;
                        protoData.m_Gold = sweepTime * rewardConfig.m_GoldSum;
                        IncreaseAsset(guid, protoData.m_Exp, GainAssetType.StageClear, AssetType.Exp, GainConsumePos.StageClear.ToString());
                        IncreaseAsset(guid, protoData.m_Gold, GainAssetType.StageClear, AssetType.Money, GainConsumePos.StageClear.ToString());
                        for (int i = 0; i < sweepTime; ++i)
                        {
                            int itemId = 0;
                            int itemCount = 1;
                            rewardConfig.GetSceneReward(user.HeroId, out itemId, out itemCount);
                            if (itemId > 0 && itemCount > 0)
                            {
                                ItemInfo addItemInfo = new ItemInfo(itemId);
                                addItemInfo.ItemNum = itemCount;
                                DoAddItem(guid, itemId, itemCount, GainConsumePos.StageClear.ToString());
                                ArkCrossEngineMessage.ItemDataMsg itemDataMsg = new ArkCrossEngineMessage.ItemDataMsg();
                                itemDataMsg.ItemId = itemId;
                                itemDataMsg.Level = addItemInfo.Level;
                                itemDataMsg.Num = addItemInfo.ItemNum;
                                itemDataMsg.AppendProperty = addItemInfo.AppendProperty;
                                protoData.m_ItemInfo.Add(itemDataMsg);
                            }
                        }
                    }
                }
                protoData.m_ResultCode = resultCode;
                scrMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, scrMsg);
            }
        }
        internal bool AddExpPoints(ulong guid, int addExpPoints, string gain_scene_id)
        {
            bool result = false;
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                PlayerLevelupExpConfig cfg = PlayerConfigProvider.Instance.GetPlayerLevelupExpConfigById(user.Level);
                if (null != cfg)
                {
                    if (user.ExpPoints + addExpPoints >= cfg.m_ConsumeExp)
                    {
                        result = true;
                    }
                }
                int incr = addExpPoints;
                IncreaseAsset(guid, incr, GainAssetType.StageClear, AssetType.Exp, gain_scene_id);
                return result;
            }
            return result;
        }
        internal void DoAddAssets(ulong guid, int money, int gold, int exp, int stamina, string channel)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                int incr = money;
                IncreaseAsset(guid, incr, GainAssetType.AddAssets, AssetType.Money, channel);
                int gold_consume = gold;
                IncreaseAsset(guid, gold_consume, GainAssetType.AddAssets, AssetType.Glod, channel);
                int exp_incr = exp;
                IncreaseAsset(guid, exp_incr, GainAssetType.AddAssets, AssetType.Exp, channel);
                int stamina_incr = stamina;
                IncreaseAsset(guid, stamina_incr, GainAssetType.AddAssets, AssetType.Stamina, channel);
                JsonMessageWithGuid aaMsg = new JsonMessageWithGuid(JsonMessageID.AddAssetsResult);
                aaMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_AddAssetsResult protoData = new ArkCrossEngineMessage.Msg_LC_AddAssetsResult();
                protoData.m_Money = money;
                protoData.m_Gold = gold;
                protoData.m_Exp = exp;
                protoData.m_Stamina = stamina;
                protoData.m_Result = (int)GeneralOperationResult.LC_Succeed;
                aaMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, aaMsg);
            }
        }
        internal void DoAddItem(ulong guid, int item_id, int num, string pos)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.ItemBag != null)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                if (item_id > 0)
                {
                    int random_property = 0;
                    if (ItemConfigProvider.Instance.GetGoldId() == item_id)
                    {
                        IncreaseAsset(guid, num, GainAssetType.StageClear, AssetType.Money, pos);
                    }
                    else if (ItemConfigProvider.Instance.GetDiamondId() == item_id)
                    {
                        IncreaseAsset(guid, num, GainAssetType.StageClear, AssetType.Glod, pos);
                    }
                    else if (ItemConfigProvider.Instance.GetMonthCardId() == item_id)
                    {
                        AddMonthCard(guid, num);
                    }
                    else
                    {
                        ItemInfo add_item_info = new ItemInfo(item_id);
                        add_item_info.ItemNum = num;
                        random_property = add_item_info.AppendProperty;
                        IncreaseItem(guid, add_item_info, num, add_item_info.ItemConfig.m_CanWear ? GainItemType.Equipment : GainItemType.Props, GainItemWay.AddItem, pos);
                    }
                    result = GeneralOperationResult.LC_Succeed;
                    JsonMessageWithGuid aiMsg = new JsonMessageWithGuid(JsonMessageID.AddItemResult);
                    aiMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_AddItemResult protoData = new ArkCrossEngineMessage.Msg_LC_AddItemResult();
                    protoData.m_ItemId = item_id;
                    protoData.m_RandomProperty = random_property;
                    protoData.m_Result = (int)result;
                    protoData.m_ItemCount = num;
                    aiMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, aiMsg);
                }
            }
        }
        internal void HandleLiftSkill(ulong guid, int skill_id)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && user.Skill != null)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                SkillDataInfo info = user.Skill.GetSkillDataByID(skill_id);
                if (null != info && info.Level > 0 && null != user.ItemBag)
                {
                    SkillLogicData skill_data = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skill_id) as SkillLogicData;
                    if (null != skill_data && skill_data.LiftSkillId > 0
                      && skill_data.LiftCostItemList.Count == skill_data.LiftCostItemNumList.Count)
                    {
                        bool ret = false;
                        bool exist = true;
                        for (int i = 0; i < skill_data.LiftCostItemList.Count; i++)
                        {
                            ItemInfo item_info = user.ItemBag.GetItemData(skill_data.LiftCostItemList[i], 0);
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
                                ItemInfo item_info = user.ItemBag.GetItemData(skill_data.LiftCostItemList[index], 0);
                                if (null != item_info)
                                {
                                    ConsumeItem(guid, item_info, skill_data.LiftCostItemNumList[index],
                                    item_info.ItemConfig.m_CanWear ? GainItemType.Equipment : GainItemType.Props,
                                    ConsumeItemWay.LiftSkill, false, GainConsumePos.Skill.ToString());
                                }
                            }
                            ret = true;
                        }
                        if (ret)
                        {
                            user.Skill.UpgradeSkill(skill_id, skill_data.LiftSkillId);
                            result = GeneralOperationResult.LC_Succeed;
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_CostError;
                        }
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_LevelError;
                    }
                }
                else
                {
                    result = GeneralOperationResult.LC_Failure_NotUnLock;
                }

                JsonMessageWithGuid lsrMsg = new JsonMessageWithGuid(JsonMessageID.LiftSkillResult);
                lsrMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_LiftSkillResult protoData = new ArkCrossEngineMessage.Msg_LC_LiftSkillResult();
                protoData.m_SkillID = skill_id;
                protoData.m_Result = (int)result;
                lsrMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, lsrMsg);
            }
        }
        internal void HandleBuyStamina(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                VipConfig config_data = VipConfigProvider.Instance.GetDataById(user.Vip);
                if (user.CurBuyStaminaCount < (null == config_data ? user.Vip + 1 : config_data.m_Stamina)
                  /*&& user.CurStamina < user.StaminaMax*/)
                {
                    int index = user.CurBuyStaminaCount + 1;
                    int total = BuyStaminaConfigProvider.Instance.GetDataCount();
                    if (index < total)
                    {
                        BuyStaminaConfig data = BuyStaminaConfigProvider.Instance.GetDataById(index);
                        if (null != data)
                        {
                            if (data.m_CostGold <= user.Gold)
                            {
                                int gold_consume = data.m_CostGold;
                                ConsumeAsset(guid, gold_consume, ConsumeAssetType.BuyStamina, AssetType.Glod, "null");
                                int stamina_incr = data.m_GainStamina;
                                IncreaseAsset(guid, stamina_incr, GainAssetType.BuyStamina, AssetType.Stamina, "null");
                                user.CurBuyStaminaCount += 1;
                                user.LastAddStaminaTimestamp = TimeUtility.CurTimestamp;
                                result = GeneralOperationResult.LC_Succeed;
                            }
                            else
                            {
                                result = GeneralOperationResult.LC_Failure_CostError;
                            }
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_Unknown;
                        }
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_Unknown;
                    }
                }
                else
                {
                    result = GeneralOperationResult.LC_Failure_Overflow;
                }
                JsonMessageWithGuid bsrMsg = new JsonMessageWithGuid(JsonMessageID.BuyStaminaResult);
                bsrMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_BuyStaminaResult protoData = new ArkCrossEngineMessage.Msg_LC_BuyStaminaResult();
                protoData.m_Result = (int)result;
                bsrMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, bsrMsg);
            }
        }
        internal void HandleFinishMission(ulong guid, int missionId)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                FinishMissionResult mission_result = FinishMissionResult.Failure;
                MissionInfo mi = user.Mission.GetMissionInfoById(missionId);
                if (null != mi && MissionStateType.COMPLETED == mi.State)
                {
                    //TODO 处理任务收益
                    ArkCrossEngineMessage.Msg_LC_FinishMissionResult protoData = new ArkCrossEngineMessage.Msg_LC_FinishMissionResult();
                    Data_SceneDropOut rewardConfig = SceneConfigProvider.Instance.GetSceneDropOutById(mi.RewardId);
                    if (null != rewardConfig)
                    {
                        int exp = user.Mission.GetMissionsExpReward(missionId, user.Level);
                        protoData.m_Gold = rewardConfig.m_GoldSum;
                        protoData.m_Exp = exp;
                        protoData.m_Diamond = rewardConfig.m_Diamond;
                        int incr = rewardConfig.m_GoldSum;
                        IncreaseAsset(guid, incr, GainAssetType.FinishMission, AssetType.Money, GainConsumePos.Mission.ToString());
                        int gold_incr = rewardConfig.m_Diamond;
                        IncreaseAsset(guid, gold_incr, GainAssetType.FinishMission, AssetType.Glod, GainConsumePos.Mission.ToString());
                        int exp_incr = exp;
                        IncreaseAsset(guid, exp_incr, GainAssetType.AddAssets, AssetType.Exp, GainConsumePos.Mission.ToString());
                        // partner
                        if (rewardConfig.m_PartnerId > 0 && !user.PartnerStateInfo.IsHavePartner(rewardConfig.m_PartnerId))
                        {
                            user.PartnerStateInfo.AddPartner(rewardConfig.m_PartnerId, 1, 1);
                            JsonMessageWithGuid addPartnerMsg = new JsonMessageWithGuid(JsonMessageID.GetPartner);
                            addPartnerMsg.m_Guid = guid;
                            ArkCrossEngineMessage.Msg_LC_GetPartner addPartnerProto = new ArkCrossEngineMessage.Msg_LC_GetPartner();
                            addPartnerProto.m_PartnerId = rewardConfig.m_PartnerId;
                            addPartnerMsg.m_ProtoData = addPartnerProto;
                            JsonMessageDispatcher.SendDcoreMessage(user.NodeName, addPartnerMsg);
                            ///
                            RecordPartnerAction(guid, rewardConfig.m_PartnerId, 1, PartnerCauseId.MissionGain, PartnerOperateResult.Null);
                        }
                        List<int> rewardItemList = rewardConfig.GetRewardItemByHeroId(user.HeroId);
                        if (null != rewardItemList && rewardItemList.Count > 0)
                        {
                            for (int i = 0; i < rewardItemList.Count; ++i)
                            {
                                if (rewardItemList[i] > 0)
                                {
                                    DoAddItem(guid, rewardItemList[i], rewardConfig.m_ItemCountList[i], GainConsumePos.Mission.ToString());
                                }
                            }
                        }
                    }
                    mission_result = FinishMissionResult.Succeed;
                    // 删除完成任务
                    user.Mission.RemoveMission(missionId);
                    // 解锁后续任务
                    if (null != mi.FollowMissions && mi.FollowMissions.Count > 0)
                    {
                        for (int i = 0; i < mi.FollowMissions.Count; ++i)
                        {
                            MissionInfo unlockMission = user.Mission.GetMissionInfoById(mi.FollowMissions[i]);
                            if (null != unlockMission)
                            {
                                ArkCrossEngineMessage.Msg_LC_FinishMissionResult.MissionInfoForSync syncMission = new ArkCrossEngineMessage.Msg_LC_FinishMissionResult.MissionInfoForSync();
                                syncMission.m_MissionId = mi.FollowMissions[i];
                                bool isCompleted = MissionSystem.Instance.IsMissionFinish(user, unlockMission.MissionId);
                                syncMission.m_Progress = MissionSystem.Instance.GetMissionProgress(user, unlockMission, isCompleted);
                                syncMission.m_IsCompleted = isCompleted;
                                protoData.m_UnlockMissions.Add(syncMission);
                                if (isCompleted)
                                {
                                    unlockMission.State = MissionStateType.COMPLETED;
                                }
                                else
                                {
                                    unlockMission.State = MissionStateType.UNCOMPLETED;
                                }
                                /// norm log
                                AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                                if (null != accountInfo)
                                {
                                    LogSys.NormLog("gettask", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.gettask, LobbyConfig.LogNormVersionStr,
                                    "B3110", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level, mi.FollowMissions[i], (int)GetTaskResult.Succeed);
                                }
                            }
                        }
                    }
                    if (null != mi.Config.TriggerGuides && mi.Config.TriggerGuides.Count > 0)
                    {
                        foreach (int id in mi.Config.TriggerGuides)
                        {
                            if (!user.NewBieGuideInfo.NewBieGuideList.Contains(id))
                            {
                                user.NewBieGuideInfo.AddToList(id);
                            }
                        }
                    }
                    protoData.m_FinishMissionId = missionId;
                    JsonMessageWithGuid fmrMsg = new JsonMessageWithGuid(JsonMessageID.FinishMissionResult);
                    fmrMsg.m_Guid = guid;
                    fmrMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, fmrMsg);
                    ///
                    int legacy_id = mi.Config.UnlockLegacyId;
                    if (legacy_id > 0 && null != user.Legacy
                      && null != user.Legacy.SevenArcs && user.Legacy.SevenArcs.Length > 0)
                    {
                        int ct = user.Legacy.SevenArcs.Length;
                        for (int index = 0; index < ct; index++)
                        {
                            ItemInfo assit_legacy = user.Legacy.SevenArcs[index];
                            if (null != assit_legacy && legacy_id == assit_legacy.ItemId && false == assit_legacy.IsUnlock)
                            {
                                assit_legacy.IsUnlock = true;
                                JsonMessageWithGuid ulrMsg = new JsonMessageWithGuid(JsonMessageID.UnlockLegacyResult);
                                ulrMsg.m_Guid = guid;
                                ArkCrossEngineMessage.Msg_LC_UnlockLegacyResult Data = new ArkCrossEngineMessage.Msg_LC_UnlockLegacyResult();
                                Data.m_Index = index;
                                Data.m_ItemID = legacy_id;
                                Data.m_Result = (int)GeneralOperationResult.LC_Succeed;
                                ulrMsg.m_ProtoData = Data;
                                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, ulrMsg);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    mission_result = FinishMissionResult.Failure;
                    LogSystem.Warn("User {0} wants to get reward from uncomplited mission {1}", guid, missionId);
                }
                /// norm log
                AccountInfo account = FindAccountInfoById(user.AccountId);
                if (null != account)
                {
                    LogSys.NormLog("finishtask", LobbyConfig.AppKeyStr, account.ClientGameVersion, Module.finishtask, LobbyConfig.LogNormVersionStr,
                    "B3120", account.LogicServerId, account.AccountId, user.Guid, user.Level, missionId, (int)mission_result);
                }
            }
        }
        internal void HandleUnlockLegacy(ulong guid, int index, int item_id)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                if (index >= 0 && index < LegacyInfo.c_MaxLegacyNum)
                {
                    if (null != user.Legacy && null != user.Legacy.SevenArcs && index < user.Legacy.SevenArcs.Length)
                    {
                        if (null != user.Legacy.SevenArcs[index] && user.Legacy.SevenArcs[index].ItemId == item_id
                          && !user.Legacy.SevenArcs[index].IsUnlock)
                        {
                            user.Legacy.SevenArcs[index].IsUnlock = true;
                            result = GeneralOperationResult.LC_Succeed;
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_Unknown;
                        }
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_Unknown;
                    }
                }
                else
                {
                    result = GeneralOperationResult.LC_Failure_Unknown;
                }
                JsonMessageWithGuid ulrMsg = new JsonMessageWithGuid(JsonMessageID.UnlockLegacyResult);
                ulrMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_UnlockLegacyResult protoData = new ArkCrossEngineMessage.Msg_LC_UnlockLegacyResult();
                protoData.m_Index = index;
                protoData.m_ItemID = item_id;
                protoData.m_Result = (int)result;
                ulrMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, ulrMsg);
            }
        }
        internal void HandleUpgradeLegacy(ulong guid, int index, int item_id, bool allow_cost_gold)
        {
            if (allow_cost_gold)
                return;
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                lock (user.Lock)
                {
                    GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                    ItemInfo op_legacy = user.Legacy.GetLegacyData(index);
                    if (null != op_legacy && null != op_legacy.ItemConfig
                      && item_id == op_legacy.ItemId && op_legacy.IsUnlock && op_legacy.ItemConfig.m_CanUpgrade)
                    {
                        int legacy_max_level = LegacyLevelupConfigProvider.Instance.GetDataCount();
                        int legacy_upgrade_level = op_legacy.Level + 1;
                        if (legacy_upgrade_level <= legacy_max_level && legacy_upgrade_level <= user.Level)
                        {
                            LegacyLevelupConfig legacy_levelup_Info = LegacyLevelupConfigProvider.Instance.GetDataById(op_legacy.Level);
                            if (null != legacy_levelup_Info)
                            {
                                bool ret = false;
                                ItemInfo have_item_info = user.ItemBag.GetItemData(legacy_levelup_Info.m_CostItemList[index], 0);
                                if (null != have_item_info)
                                {
                                    if (legacy_levelup_Info.m_CostNum <= have_item_info.ItemNum)
                                    {
                                        ConsumeItem(guid, have_item_info, legacy_levelup_Info.m_CostNum,
                                        have_item_info.ItemConfig.m_CanWear ? GainItemType.Equipment : GainItemType.Props,
                                        ConsumeItemWay.UpgradeLegacy, false, GainConsumePos.Legacy.ToString());
                                        ret = true;
                                    }
                                    else
                                    {
                                        if (allow_cost_gold)
                                        {
                                            int replenish_num = legacy_levelup_Info.m_CostNum - have_item_info.ItemNum;
                                            int cost_gold = (int)(replenish_num * legacy_levelup_Info.m_Rate);
                                            if (cost_gold <= user.Gold)
                                            {
                                                ConsumeItem(guid, have_item_info, have_item_info.ItemNum,
                                                have_item_info.ItemConfig.m_CanWear ? GainItemType.Equipment : GainItemType.Props,
                                                ConsumeItemWay.UpgradeLegacy, true, GainConsumePos.Legacy.ToString());
                                                user.Gold -= cost_gold;
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
                                        if (cost_gold <= user.Gold && 0 != cost_gold)
                                        {
                                            float cons = (float)cost_gold;
                                            ConsumeAsset(guid, cost_gold, ConsumeAssetType.UpgradeLegacy, AssetType.Glod, GainConsumePos.Legacy.ToString());
                                            ret = true;
                                        }
                                    }
                                }
                                if (ret)
                                {
                                    op_legacy.Level = legacy_upgrade_level;
                                    user.Legacy.SetLegacyData(index, null);
                                    user.Legacy.SetLegacyData(index, op_legacy);
                                    result = GeneralOperationResult.LC_Succeed;
                                }
                                else
                                {
                                    result = GeneralOperationResult.LC_Failure_CostError;
                                }
                            }
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_LevelError;
                        }
                    }
                    JsonMessageWithGuid ulrMsg = new JsonMessageWithGuid(JsonMessageID.UpgradeLegacyResult);
                    ulrMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_UpgradeLegacyResult protoData = new ArkCrossEngineMessage.Msg_LC_UpgradeLegacyResult();
                    protoData.m_Index = index;
                    protoData.m_ItemID = item_id;
                    protoData.m_AllowCostGold = allow_cost_gold;
                    protoData.m_Result = (int)result;
                    ulrMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, ulrMsg);
                }
            }
        }
        internal void HandleAddXSoulExperience(ulong guid, int part, int useItemId, int useItemNum)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                lock (user.Lock)
                {
                    ItemInfo xsoul_part = user.XSoul.GetXSoulPartData((XSoulPart)part);
                    ItemInfo use_item = user.ItemBag.GetItemData(useItemId, 0);
                    GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                    do
                    {
                        if (xsoul_part == null || use_item == null || use_item.ItemNum < useItemNum)
                        {
                            break;
                        }
                        if (IsMaxLevel(xsoul_part))
                        {
                            break;
                        }
                        if (!IsProviderLegal(xsoul_part.ItemId, useItemId))
                        {
                            break;
                        }
                        if (use_item.ItemNum < useItemNum)
                        {
                            break;
                        }
                        ItemConfig use_item_config = ItemConfigProvider.Instance.GetDataById(useItemId);
                        if (use_item_config == null)
                        {
                            break;
                        }
                        result = GeneralOperationResult.LC_Succeed;
                        xsoul_part.Experience += use_item_config.m_ExperienceProvide * useItemNum;
                        ConsumeItem(guid, use_item, useItemNum, use_item.ItemConfig.m_CanWear ? GainItemType.Equipment : GainItemType.Props,
                            ConsumeItemWay.UpgradeXSoul, false, GainConsumePos.XSoul.ToString());
                        xsoul_part.UpdateLevelByExperience();
                    } while (false);
                    JsonMessageWithGuid urlMsg = new JsonMessageWithGuid(JsonMessageID.AddXSoulExperienceResult);
                    urlMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_AddXSoulExperienceResult protoData = new ArkCrossEngineMessage.Msg_LC_AddXSoulExperienceResult();
                    protoData.m_Result = (int)result;
                    protoData.m_XSoulPart = part;
                    protoData.m_UseItemId = useItemId;
                    protoData.m_ItemNum = useItemNum;
                    if (xsoul_part != null)
                    {
                        protoData.m_Experience = xsoul_part.Experience;
                    }
                    else
                    {
                        protoData.m_Experience = 0;
                    }
                    urlMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, urlMsg);
                }
            }
        }
        private bool IsProviderLegal(int xsoulid, int privider_id)
        {
            XSoulLevelConfig part_config = XSoulLevelConfigProvider.Instance.GetDataById(xsoulid);
            if (part_config == null)
            {
                return false;
            }
            foreach (int id in part_config.m_ExperienceProvideItems)
            {
                if (id == privider_id)
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsMaxLevel(ItemInfo part)
        {
            XSoulLevelConfig level_config = XSoulLevelConfigProvider.Instance.GetDataById(part.ItemId);
            if (level_config == null)
            {
                return true;
            }
            part.UpdateLevelByExperience();
            if (part.Level >= level_config.m_MaxLevel)
            {
                return true;
            }
            return false;
        }
        internal void HandleXSoulChangeShowModel(ulong guid, int part, int model_level)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null)
            {
                lock (user.Lock)
                {
                    ItemInfo xsoul_part = user.XSoul.GetXSoulPartData((XSoulPart)part);
                    GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                    //LogSystem.Debug("----change xsoul model lobby got message model_level={0} cur_level={1}", model_level, xsoul_part.Level);
                    if (model_level <= xsoul_part.Level)
                    {
                        result = GeneralOperationResult.LC_Succeed;
                        xsoul_part.ShowModelLevel = model_level;
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_LevelError;
                    }

                    JsonMessageWithGuid urlMsg = new JsonMessageWithGuid(JsonMessageID.XSoulChangeShowModelResult);
                    urlMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_XSoulChangeShowModelResult protoData = new ArkCrossEngineMessage.Msg_LC_XSoulChangeShowModelResult();
                    protoData.m_Result = (int)result;
                    protoData.m_XSoulPart = part;
                    protoData.m_ModelLevel = model_level;
                    urlMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, urlMsg);
                }
            }
        }
        internal void HandleUpdateFightingScore(ulong guid, int score)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && score > 0)
            {
                user.FightingScore = score;
            }
        }
        internal void HandleRequestExpedition(ulong guid, int sceneId, int num)
        {
            Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
            if (null != cfg && cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_EXPEDITION)
            {
                DataProcessScheduler dataProcess = LobbyServer.Instance.DataProcessScheduler;
                UserInfo user = dataProcess.GetUserInfo(guid);
                if (user != null)
                {
                    JsonMessageWithGuid requestExpeditionResultMsg = new JsonMessageWithGuid(JsonMessageID.RequestExpeditionResult);
                    requestExpeditionResultMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_RequestExpeditionResult protoData = new ArkCrossEngineMessage.Msg_LC_RequestExpeditionResult();
                    if (null != protoData)
                    {
                        if (user.CurrentState == UserState.Teaming)
                        {
                            protoData.m_Result = (int)GeneralOperationResult.LC_Failure_InMatching;
                        }
                        else
                        {
                            if (null != user.Expedition && user.Expedition.Schedule == num && user.Expedition.Hp > 0)
                            {
                                user.UpdateGuideFlag(sceneId);
                                user.CurrentBattleInfo.init(user.HeroId, sceneId);
                                protoData.m_ServerIp = "127.0.0.1";
                                protoData.m_ServerPort = 9001;
                                protoData.m_Guid = user.Guid;
                                protoData.m_Key = user.Key;
                                protoData.m_HeroId = user.HeroId;
                                protoData.m_CampId = (int)CampIdEnum.Blue;
                                protoData.m_SceneType = sceneId;
                                protoData.m_ActiveTollgate = num;
                                protoData.m_Result = (int)GeneralOperationResult.LC_Succeed;
                                user.CurrentState = UserState.Pve;
                                LogSys.Log(LOG_TYPE.INFO, "Expedition Play Room will run on Lobby without room and roomserver...");
                                ///
                                dataProcess.RecordCampaignAction(guid, sceneId);
                            }
                            else
                            {
                                protoData.m_Result = (int)GeneralOperationResult.LC_Failure_Unknown;
                            }
                        }
                        requestExpeditionResultMsg.m_ProtoData = protoData;
                    }
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, requestExpeditionResultMsg);
                }
            }
        }
        internal void HandleFinishExpedition(ulong guid, int scene_id, int tollgate_num, int hp, int mp, int rage)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && null != user.Expedition)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(scene_id);
                if (null != cfg && cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_EXPEDITION)
                {
                    if (user.Expedition.Schedule == tollgate_num)
                    {
                        if (null != user.Expedition.Tollgates[user.Expedition.Schedule])
                        {
                            user.Expedition.Tollgates[user.Expedition.Schedule].IsFinish = true;
                            user.Expedition.Tollgates[user.Expedition.Schedule].IsAcceptedAward = false;
                            user.Expedition.Schedule += 1;
                            user.Expedition.Hp = hp;
                            user.Expedition.Mp = mp;
                            user.Expedition.Rage = rage;
                            result = GeneralOperationResult.LC_Succeed;
                        }
                    }
                }
                StageClearInfo scInfo = new StageClearInfo();
                scInfo.SceneId = scene_id;
                List<int> completedMissions = new List<int>();
                MissionSystem.Instance.OnStageClear(user, scInfo, ref completedMissions);
                foreach (int missionId in completedMissions)
                {
                    JsonMessageWithGuid missionCompletedMsg = new JsonMessageWithGuid(JsonMessageID.MissionCompleted);
                    missionCompletedMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_MissionCompleted missionProtoData = new ArkCrossEngineMessage.Msg_LC_MissionCompleted();
                    if (null != missionProtoData)
                    {
                        missionProtoData.m_MissionId = missionId;
                        missionProtoData.m_Progress = MissionSystem.Instance.GetMissionProgress(user, missionId, true);
                        missionCompletedMsg.m_ProtoData = missionProtoData;
                        JsonMessageDispatcher.SendDcoreMessage(user.NodeName, missionCompletedMsg);
                    }
                }
                if (UserState.Pve == user.CurrentState)
                {
                    user.CurrentState = UserState.Online;
                }
                JsonMessageWithGuid finishExpeditionResultMsg = new JsonMessageWithGuid(JsonMessageID.FinishExpeditionResult);
                finishExpeditionResultMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_FinishExpeditionResult protoData = new ArkCrossEngineMessage.Msg_LC_FinishExpeditionResult();
                if (null != protoData)
                {
                    protoData.m_SceneId = scene_id;
                    protoData.m_TollgateNum = tollgate_num;
                    protoData.m_Hp = hp;
                    protoData.m_Mp = mp;
                    protoData.m_Rage = rage;
                    protoData.m_Result = (int)result;
                    finishExpeditionResultMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, finishExpeditionResultMsg);
                }
            }
        }
        internal void HandleExpeditionAward(ulong guid, int tollgate_num)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (user != null && null != user.Expedition)
            {
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                JsonMessageWithGuid earMsg = new JsonMessageWithGuid(JsonMessageID.ExpeditionAwardResult);
                ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult protoData = new ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult();
                if (tollgate_num <= user.Expedition.Schedule)
                {
                    ExpeditionPlayerInfo.TollgateData tollgate_data = user.Expedition.Tollgates[tollgate_num];
                    if (null != tollgate_data && true == tollgate_data.IsFinish && false == tollgate_data.IsAcceptedAward)
                    {
                        List<ExpeditionPlayerInfo.AwardItemData> award_info = new List<ExpeditionPlayerInfo.AwardItemData>();
                        float money = 0;
                        ExpeditionTollgateConfig cur_tollgate_data = ExpeditionTollgateConfigProvider.Instance.GetDataById(tollgate_num + 1) as ExpeditionTollgateConfig;
                        if (null != cur_tollgate_data)
                        {
                            money = cur_tollgate_data.m_DropOutMoney + cur_tollgate_data.m_MoneyFactor * user.Expedition.ResetScore / 1000.0f;
                            if (cur_tollgate_data.m_ItemId.Count > 0)
                            {
                                for (int award_index = 0; award_index < cur_tollgate_data.m_ItemId.Count; award_index++)
                                {
                                    int award_item_id = cur_tollgate_data.m_ItemId[award_index];
                                    if (award_item_id > 0)
                                    {
                                        float award_item_probality = cur_tollgate_data.m_ItemProbality[award_index] + cur_tollgate_data.m_ItemFactor[award_index] * user.Expedition.ResetScore / 1000.0f;
                                        int accurate_num = (int)award_item_probality;
                                        int random_num = (CrossEngineHelper.Random.Next(0, 100) / 100.0f) <= (award_item_probality - (float)accurate_num) ? 1 : 0;
                                        int total_num = accurate_num + random_num;
                                        if (total_num > 0)
                                        {
                                            ExpeditionPlayerInfo.AwardItemData award_item = new ExpeditionPlayerInfo.AwardItemData();
                                            award_item.ItemId = award_item_id;
                                            award_item.ItemNum = total_num;
                                            award_info.Add(award_item);
                                        }
                                    }
                                }
                            }
                        }
                        tollgate_data.IsAcceptedAward = true;
                        ///
                        DoAddAssets(guid, (int)money, 0, 0, 0, GainConsumePos.Expedition.ToString());
                        protoData.m_AddMoney = (int)money;
                        int award_ct = award_info.Count;
                        if (award_ct > 0)
                        {
                            for (int i = 0; i < award_ct; i++)
                            {
                                int item_id = award_info[i].ItemId;
                                int item_num = award_info[i].ItemNum;
                                ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult.AwardItemInfo asset_info = new ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult.AwardItemInfo();
                                asset_info.m_Id = item_id;
                                asset_info.m_Num = item_num;
                                protoData.m_Items.Add(asset_info);
                                DoAddItem(guid, item_id, item_num, GainConsumePos.Expedition.ToString());
                            }
                        }
                        result = GeneralOperationResult.LC_Succeed;
                    }
                }
                protoData.m_TollgateNum = tollgate_num;
                protoData.m_Result = (int)result;
                earMsg.m_ProtoData = protoData;
                earMsg.m_Guid = guid;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, earMsg);
            }
        }
        internal void HandleQueryExpeditionInfo(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.Expedition)
            {
                user.Expedition.SyncExpeditionInfo(guid, user.Expedition.Hp, user.Expedition.Mp, user.Expedition.Rage, -1, GeneralOperationResult.LC_Succeed);
            }
        }
        internal void HandleExpeditionFailure(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                ExpeditionPlayerInfo expedition = user.Expedition;
                if (null != expedition)
                {
                    expedition.Hp = 0;
                    expedition.Mp = 0;
                }
            }
        }
        internal void HandleMidasTouch(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                int cost_gold = 0;
                int gain_money = 0;
                GeneralOperationResult result = GeneralOperationResult.LC_Failure_Unknown;
                VipConfig config_data = VipConfigProvider.Instance.GetDataById(user.Vip);
                if (user.CurBuyMoneyCount < (null == config_data ? (user.Vip > 0 ? user.Vip * 10 : 10) : config_data.m_BuyGold))
                {
                    int index = user.CurBuyMoneyCount + 1;
                    int total = BuyMoneyConfigProvider.Instance.GetDataCount();
                    if (index < total)
                    {
                        BuyMoneyConfig data = BuyMoneyConfigProvider.Instance.GetDataById(index);
                        if (null != data)
                        {
                            if (data.m_CostGold <= user.Gold)
                            {
                                cost_gold = data.m_CostGold;
                                gain_money = data.m_GainMoney;
                                int cons = cost_gold;
                                ConsumeAsset(guid, cons, ConsumeAssetType.MidasTouch, AssetType.Glod, "null");
                                int incr = gain_money;
                                IncreaseAsset(guid, incr, GainAssetType.MidasTouch, AssetType.Money, "null");
                                user.CurBuyMoneyCount += 1;
                                result = GeneralOperationResult.LC_Succeed;
                            }
                            else
                            {
                                result = GeneralOperationResult.LC_Failure_CostError;
                            }
                        }
                        else
                        {
                            result = GeneralOperationResult.LC_Failure_Unknown;
                        }
                    }
                    else
                    {
                        result = GeneralOperationResult.LC_Failure_Unknown;
                    }
                }
                else
                {
                    result = GeneralOperationResult.LC_Failure_Overflow;
                }
                JsonMessageWithGuid mtrMsg = new JsonMessageWithGuid(JsonMessageID.MidasTouchResult);
                mtrMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_MidasTouchResult protoData = new ArkCrossEngineMessage.Msg_LC_MidasTouchResult();
                protoData.m_Count = user.CurBuyMoneyCount;
                protoData.m_CostGlod = cost_gold;
                protoData.m_GainMoney = gain_money;
                protoData.m_Result = (int)result;
                mtrMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, mtrMsg);
            }
        }
        internal void HandleExchangeGoods(ulong guid, int id)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            StoreConfig sc = StoreConfigProvider.Instance.GetDataById(id);
            if (null != user && null != user.ExchangeGoodsInfo && null != sc)
            {
                JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.ExchangeGoodsResult);
                jsonMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_ExchangeGoodsResult protoData = new ArkCrossEngineMessage.Msg_LC_ExchangeGoodsResult();
                protoData.m_ExchangeId = id;
                int nowcurrency = 0;
                ItemInfo iteminfo = null;
                if (sc.m_Currency == ItemConfigProvider.Instance.GetGoldId())
                {
                    nowcurrency = user.Money;
                }
                else if (sc.m_Currency == ItemConfigProvider.Instance.GetDiamondId())
                {
                    nowcurrency = user.Gold;
                }
                else
                {
                    iteminfo = user.ItemBag.GetItemData(sc.m_Currency, 0);
                    if (iteminfo == null)
                    {
                        nowcurrency = 0;
                    }
                    else
                    {
                        nowcurrency = iteminfo.ItemNum;
                    }
                }
                int currency = 0;
                GeneralOperationResult gor = user.ExchangeGoodsInfo.CheckOKBuy(id, sc, nowcurrency, out currency);
                if (gor == GeneralOperationResult.LC_Succeed)
                {
                    if (sc.m_Currency == ItemConfigProvider.Instance.GetGoldId())
                    {
                        ConsumeAsset(guid, currency, ConsumeAssetType.ExchangeGoods, AssetType.Money, "ExchangeCurrency");
                    }
                    else if (sc.m_Currency == ItemConfigProvider.Instance.GetDiamondId())
                    {
                        ConsumeAsset(guid, currency, ConsumeAssetType.ExchangeGoods, AssetType.Glod, "ExchangeCurrency");
                    }
                    else
                    {
                        ConsumeItem(guid, iteminfo, currency, GainItemType.Currency, ConsumeItemWay.ExchangeCurrency, false, "ExchangeCurrency");
                    }
                    user.ExchangeGoodsInfo.BuyGoods(id);
                    DoAddItem(guid, sc.m_ItemId, sc.m_ItemNum, "ExchangeGoods");
                }
                protoData.m_ExchangeNum = user.ExchangeGoodsInfo.GetNum(id);
                protoData.m_Result = (int)gor;
                jsonMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
            }
        }
        internal void HandleRequestRefreshExchange(ulong guid, bool requestrefresh, int currencyid)
        {
            if (requestrefresh)
            {
                UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
                if (user != null)
                {
                    if (currencyid == 0)
                    {
                        user.UpdateExchangeGoods();
                    }
                    else if (user.ExchangeGoodsInfo != null)
                    {
                        JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.RefreshExchangeResult);
                        jsonMsg.m_Guid = guid;
                        ArkCrossEngineMessage.Msg_LC_RefreshExchangeResult protoData = new ArkCrossEngineMessage.Msg_LC_RefreshExchangeResult();
                        int cost = /*(user.ExchangeGoodsInfo.RefreshNum + 1) * 50*/0;
                        if (user.ExchangeGoodsInfo.CurrencyRefreshNum.ContainsKey(currencyid))
                        {
                            cost = (user.ExchangeGoodsInfo.CurrencyRefreshNum[currencyid] + 1) * 50;
                        }
                        else
                        {
                            cost = 50;
                        }
                        if (user.Gold >= cost)
                        {
                            ConsumeAsset(guid, cost, ConsumeAssetType.RefreshExchange, AssetType.Glod, "RefreshExchange");
                            int time;
                            if (user.ExchangeGoodsInfo.CurrencyRefreshNum.TryGetValue(currencyid, out time))
                            {
                                time += 1;
                                user.ExchangeGoodsInfo.CurrencyRefreshNum[currencyid] = time;
                            }
                            else
                            {
                                time = 1;
                                user.ExchangeGoodsInfo.CurrencyRefreshNum.TryAdd(currencyid, time);
                            }
                            user.ExchangeGoodsInfo.ResetByCurrency(currencyid);
                            protoData.m_RequestRefreshResult = (int)GeneralOperationResult.LC_Succeed;
                            protoData.m_RefreshNum = time;
                            protoData.m_CurrencyId = currencyid;
                        }
                        else
                        {
                            protoData.m_RequestRefreshResult = (int)GeneralOperationResult.LC_Failure_CostError;
                            int time = 0;
                            user.ExchangeGoodsInfo.CurrencyRefreshNum.TryGetValue(currencyid, out time);
                            protoData.m_RefreshNum = time;
                            protoData.m_CurrencyId = currencyid;
                        }
                        jsonMsg.m_ProtoData = protoData;
                        JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
                    }
                }
            }
        }
        internal void HandleSelectpartner(ulong guid, int partnerId)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.PartnerStateInfo)
            {
                lock (user.Lock)
                {
                    JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.SelectPartnerResult);
                    jsonMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_SelectPartnerResult protoData = new ArkCrossEngineMessage.Msg_LC_SelectPartnerResult();
                    protoData.m_PartnerId = partnerId;
                    if (user.PartnerStateInfo.IsHavePartner(partnerId))
                    {
                        protoData.m_ResultCode = (int)PartnerMsgResultEnum.SUCCESS;
                        user.PartnerStateInfo.SetActivePartner(partnerId);
                    }
                    else
                    {
                        protoData.m_ResultCode = (int)PartnerMsgResultEnum.ERROR;
                    }
                    jsonMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
                    ///
                    PartnerInfo cur_partner = user.PartnerStateInfo.GetActivePartner();
                    if (null != cur_partner)
                    {
                        RecordPartnerAction(guid, partnerId, cur_partner.CurAdditionLevel, PartnerCauseId.Active, PartnerOperateResult.Null);
                    }
                }
            }
        }
        internal void HandleUpgradePartnerLevel(ulong guid, int partnerId)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.PartnerStateInfo)
            {
                lock (user.Lock)
                {
                    JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.UpgradePartnerLevelResult);
                    jsonMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_UpgradePartnerLevelResult protoData = new ArkCrossEngineMessage.Msg_LC_UpgradePartnerLevelResult();
                    protoData.m_PartnerId = partnerId;
                    protoData.m_ResultCode = (int)PartnerMsgResultEnum.ERROR;
                    PartnerInfo pi = user.PartnerStateInfo.GetPartnerInfoById(partnerId);
                    if (null != pi)
                    {
                        PartnerLevelUpConfig pluConfig = PartnerLevelUpConfigProvider.Instance.GetDataById(pi.CurAdditionLevel);
                        if (null != pluConfig)
                        {
                            if (pi.CurAdditionLevel < pi.GetMaxLevel())
                            {
                                if (user.Money >= pluConfig.GoldCost && user.ItemBag.GetItemCount(pi.LevelUpItemId, 0) >= pluConfig.ItemCost)
                                {
                                    ConsumeAsset(guid, pluConfig.GoldCost, ConsumeAssetType.UpgradePartner, AssetType.Money, GainConsumePos.Partner.ToString());
                                    ConsumeItem(guid, user.ItemBag.GetItemData(pi.LevelUpItemId, 0), pluConfig.ItemCost, GainItemType.Props, ConsumeItemWay.UpgradePartner, false, GainConsumePos.Partner.ToString());
                                    if (CrossEngineHelper.Random.NextFloat() <= pluConfig.Rate)
                                    {
                                        protoData.m_ResultCode = (int)PartnerMsgResultEnum.SUCCESS;
                                        ++pi.CurAdditionLevel;
                                    }
                                    else
                                    {
                                        if (pluConfig.IsFailedDemote && pi.CurAdditionLevel > 1)
                                        {
                                            protoData.m_ResultCode = (int)PartnerMsgResultEnum.DEMOTION;
                                            --pi.CurAdditionLevel;
                                        }
                                        else
                                        {
                                            protoData.m_ResultCode = (int)PartnerMsgResultEnum.FAILED;
                                        }
                                    }
                                }
                                else
                                {
                                    if (user.Money < pluConfig.GoldCost)
                                    {
                                        protoData.m_ResultCode = (int)PartnerMsgResultEnum.NEED_MORE_GOLD;
                                    }
                                    else
                                    {
                                        protoData.m_ResultCode = (int)PartnerMsgResultEnum.NEED_MORE_ITEM;
                                    }
                                }
                            }
                            else
                            {
                                protoData.m_ResultCode = (int)PartnerMsgResultEnum.MAX_LEVEL;
                            }
                        }
                    }
                    else
                    {
                        protoData.m_ResultCode = (int)PartnerMsgResultEnum.ERROR;
                    }
                    protoData.m_CurLevel = pi.CurAdditionLevel;
                    jsonMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
                    ///
                    PartnerInfo op_partner = user.PartnerStateInfo.GetPartnerInfoById(partnerId);
                    if (null != op_partner)
                    {
                        PartnerOperateResult result =
                          protoData.m_ResultCode == (int)PartnerMsgResultEnum.SUCCESS ? PartnerOperateResult.Succeed : PartnerOperateResult.Failure;
                        RecordPartnerAction(guid, partnerId, op_partner.CurAdditionLevel, PartnerCauseId.UpgradeLevel, result);
                    }
                }
            }
        }
        internal void HandleUpgradePartnerStage(ulong guid, int partnerId)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.PartnerStateInfo)
            {
                JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.UpgradePartnerStageResult);
                jsonMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_UpgradeParnerStageResult protoData = new ArkCrossEngineMessage.Msg_LC_UpgradeParnerStageResult();
                protoData.m_PartnerId = partnerId;
                PartnerInfo pi = user.PartnerStateInfo.GetPartnerInfoById(partnerId);
                if (null != pi)
                {
                    if (pi.CurSkillStage < pi.GetMaxStage())
                    {
                        PartnerStageUpConfig psuConfig = PartnerStageUpConfigProvider.Instance.GetDataById(pi.CurSkillStage);
                        if (null != psuConfig)
                        {
                            if (user.Money >= psuConfig.GoldCost && user.ItemBag.GetItemCount(pi.StageUpItemId, 0) >= psuConfig.ItemCost)
                            {
                                protoData.m_ResultCode = (int)PartnerMsgResultEnum.SUCCESS;
                                ConsumeAsset(guid, psuConfig.GoldCost, ConsumeAssetType.UpgradePartner, AssetType.Money, GainConsumePos.Partner.ToString());
                                ConsumeItem(guid, user.ItemBag.GetItemData(pi.StageUpItemId, 0), psuConfig.ItemCost, GainItemType.Props, ConsumeItemWay.UpgradePartner, false, GainConsumePos.Partner.ToString());
                                ++pi.CurSkillStage;
                            }
                            else
                            {
                                if (user.Money < psuConfig.GoldCost)
                                {
                                    protoData.m_ResultCode = (int)PartnerMsgResultEnum.NEED_MORE_GOLD;
                                }
                                else
                                {
                                    protoData.m_ResultCode = (int)PartnerMsgResultEnum.NEED_MORE_ITEM;
                                }
                            }
                        }
                    }
                    else
                    {
                        protoData.m_ResultCode = (int)PartnerMsgResultEnum.MAX_LEVEL;
                    }
                }
                else
                {
                    protoData.m_ResultCode = (int)PartnerMsgResultEnum.ERROR;
                }
                protoData.m_CurStage = pi.CurSkillStage;
                jsonMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
                ///
                PartnerInfo op_partner = user.PartnerStateInfo.GetPartnerInfoById(partnerId);
                if (null != op_partner)
                {
                    PartnerOperateResult result =
                      protoData.m_ResultCode == (int)PartnerMsgResultEnum.SUCCESS ? PartnerOperateResult.Succeed : PartnerOperateResult.Failure;
                    RecordPartnerAction(guid, partnerId, op_partner.CurAdditionLevel, PartnerCauseId.UpgradeStage, result);
                }
            }
        }
        internal void HandleCompoundPartner(ulong guid, int partnerId)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.PartnerStateInfo)
            {
                JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.CompoundPartnerResult);
                jsonMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_CompoundPartnerResult protoData = new ArkCrossEngineMessage.Msg_LC_CompoundPartnerResult();
                protoData.m_ResultCode = (int)PartnerMsgResultEnum.FAILED;
                protoData.m_PartnerId = partnerId;
                if (user.PartnerStateInfo.IsHavePartner(partnerId))
                {
                    // 已经拥有伙伴，无需合成
                }
                else
                {
                    PartnerConfig partnerConfig = PartnerConfigProvider.Instance.GetDataById(partnerId);
                    if (null != partnerConfig)
                    {
                        int fragItemId = partnerConfig.PartnerFragId;
                        int fragItemNum = partnerConfig.PartnerFragNum;
                        if (user.ItemBag.GetItemCount(fragItemId, 0) >= fragItemNum)
                        {
                            protoData.m_ResultCode = (int)PartnerMsgResultEnum.SUCCESS;
                            ConsumeItem(guid, user.ItemBag.GetItemData(fragItemId, 0), fragItemNum, GainItemType.Props, ConsumeItemWay.UpgradePartner, false, GainConsumePos.Partner.ToString());
                            user.PartnerStateInfo.AddPartner(partnerId, 1, 1);
                        }
                    }
                }
                jsonMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
                ///
                PartnerInfo op_partner = user.PartnerStateInfo.GetPartnerInfoById(partnerId);
                if (null != op_partner)
                {
                    RecordPartnerAction(guid, partnerId, op_partner.CurAdditionLevel, PartnerCauseId.CompoundGain, PartnerOperateResult.Null);
                }
            }
        }
        internal void HandleUpdatePosition(ulong guid, float x, float z, float dir)
        {
            UserInfo user = GetUserInfo(guid);
            if (null != user)
            {
                user.X = x;
                user.Z = z;
                user.FaceDir = dir;
            }
        }
        internal void HandleRequestUsers(ulong guid, int count, List<ulong> exists)
        {
            HashSet<ulong> _exists = new HashSet<ulong>(exists);
            //取前面几个不在副本内的玩家
            UserInfo myself = GetUserInfo(guid);
            if (null != myself)
            {
                JsonMessageWithGuid retMsg = new JsonMessageWithGuid(JsonMessageID.RequestUsersResult);
                retMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_RequestUsersResult protoData = new ArkCrossEngineMessage.Msg_LC_RequestUsersResult();
                int ct = 0;
                foreach (UserInfo user in m_UserInfos.Values)
                {
                    if (UserState.Online == user.CurrentState && user.Guid != guid && !user.IsGmAccount && user.LogicServerId == myself.LogicServerId && user.CitySceneId == myself.CitySceneId && !_exists.Contains(user.Guid))
                    {
                        ArkCrossEngineMessage.Msg_LC_RequestUsersResult.UserInfo info = new ArkCrossEngineMessage.Msg_LC_RequestUsersResult.UserInfo();
                        info.m_Guid = user.Guid;
                        info.m_HeroId = user.HeroId;
                        info.m_Nick = user.Nickname;
                        info.m_X = user.X;
                        info.m_Z = user.Z;
                        info.m_FaceDir = user.FaceDir;

                        ItemInfo xsoulItemInfo = user.XSoul.GetXSoulPartData(XSoulPart.kWeapon);
                        if (null != xsoulItemInfo)
                        {
                            info.m_XSoulItemId = xsoulItemInfo.ItemId;
                            info.m_XSoulLevel = xsoulItemInfo.Level;
                            info.m_XSoulExp = xsoulItemInfo.Experience;
                            info.m_XSoulShowLevel = xsoulItemInfo.ShowModelLevel;
                        }
                        else
                        {
                            info.m_XSoulItemId = 0;
                        }

                        /*
                        ItemInfo wingItemInfo = user.Equip.GetEquipmentData((int)EquipmentType.E_Wing);
                        if (null != wingItemInfo)
                        {
                            info.m_WingItemId = wingItemInfo.ItemId;
                            info.m_WingLevel = wingItemInfo.Level;
                        }
                        else
                        */
                        {
                            info.m_WingItemId = 0;
                        }

                        protoData.m_Users.Add(info);
                        ++ct;
                    }
                    if (ct >= count)
                    {
                        break;
                    }
                }
                retMsg.m_ProtoData = protoData;
                if (ct > 0)
                {
                    JsonMessageDispatcher.SendDcoreMessage(myself.NodeName, retMsg);
                }
            }
        }
        internal void HandleRequestUserPosition(ulong guid, ulong userGuid)
        {
            UserInfo myself = GetUserInfo(guid);
            UserInfo user = GetUserInfo(userGuid);
            if (null != myself)
            {
                if (null != user && UserState.Online == user.CurrentState && user.CitySceneId == myself.CitySceneId)
                {
                    JsonMessageWithGuid retMsg = new JsonMessageWithGuid(JsonMessageID.RequestUserPositionResult);
                    retMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_RequestUserPositionResult protoData = new ArkCrossEngineMessage.Msg_LC_RequestUserPositionResult();
                    protoData.m_User = userGuid;
                    protoData.m_X = user.X;
                    protoData.m_Z = user.Z;
                    protoData.m_FaceDir = user.FaceDir;

                    retMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(myself.NodeName, retMsg);
                }
                else
                {
                    JsonMessageWithGuid retMsg = new JsonMessageWithGuid(JsonMessageID.RequestUserPositionResult);
                    retMsg.m_Guid = guid;
                    ArkCrossEngineMessage.Msg_LC_RequestUserPositionResult protoData = new ArkCrossEngineMessage.Msg_LC_RequestUserPositionResult();
                    protoData.m_User = userGuid;
                    protoData.m_X = -1;
                    protoData.m_Z = -1;
                    protoData.m_FaceDir = 0;

                    retMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(myself.NodeName, retMsg);
                }
            }
        }
        internal void HandleChangeCityScene(ulong guid, int cityScene)
        {
            UserInfo myself = GetUserInfo(guid);
            if (null != myself)
            {
                myself.CitySceneId = cityScene;
                if (myself.CurrentState != UserState.Room)
                    myself.CurrentState = UserState.Online;

                LogSys.Log(LOG_TYPE.DEBUG, "HandleChangeCityScene {0} {1}", guid, cityScene);
            }
        }
        internal void HandleRequestPlayerInfo(ulong guid, string nick)
        {
            UserInfo user = GetUserInfo(guid);
            if (null == user)
                return;
            ulong other_guid = GetGuidByNickname(nick);
            UserInfo other_info = GetUserInfo(other_guid);
            if (null != other_info && other_info.CurrentState != UserState.DropOrOffline)
            {
                JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.SyncPlayerInfo);
                jsonMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_SyncPlayerInfo protoData = new ArkCrossEngineMessage.Msg_LC_SyncPlayerInfo();
                protoData.m_Nick = other_info.Nickname;
                protoData.m_Level = other_info.Level;
                protoData.m_Score = other_info.FightingScore;
                jsonMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
            }
        }
        internal void HandleRequestVigor(ulong guid)
        {
            UserInfo user = GetUserInfo(guid);
            if (null != user)
            {
                int vigor = user.Vigor;
                JsonMessageWithGuid svMsg = new JsonMessageWithGuid(JsonMessageID.SyncVigor);
                svMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_SyncVigor protoData = new ArkCrossEngineMessage.Msg_LC_SyncVigor();
                protoData.m_Vigor = vigor;
                svMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, svMsg);
            }
        }
        internal void HandleRecordNewbieFlag(ulong guid, int bit)
        {
            UserInfo user = GetUserInfo(guid);
            if (null != user)
            {
                /// norm log
                AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                if (null != accountInfo)
                {
                    LogSys.NormLog("newstages", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.newstages, LobbyConfig.LogNormVersionStr,
                      (bit * 10 + 4000).ToString(), accountInfo.LogicServerId, accountInfo.ChannelId, accountInfo.AccountId, user.Guid, user.Level, bit);
                }
            }
        }
        internal void HandleSetNewbieFlag(ulong guid, int bit, int num)
        {
            UserInfo user = GetUserInfo(guid);
            if (null != user)
            {
                int flag = (int)user.NewBieGuideInfo.NewbieFlag;
                if (0 == num)
                    flag &= ~(1 << bit);
                else
                    flag |= (1 << bit);
                user.NewBieGuideInfo.NewbieFlag = (long)flag;
                JsonMessageWithGuid svMsg = new JsonMessageWithGuid(JsonMessageID.SyncNewbieFlag);
                svMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_SyncNewbieFlag protoData = new ArkCrossEngineMessage.Msg_LC_SyncNewbieFlag();
                protoData.m_NewbieFlag = user.NewBieGuideInfo.NewbieFlag;
                svMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, svMsg);
            }
        }
        internal void HandleSetNewbieActionFlag(ulong guid, int bit, int num)
        {
            UserInfo user = GetUserInfo(guid);
            if (null != user)
            {
                int flag = (int)user.NewBieGuideInfo.NewbieActionFlag;
                if (0 == num)
                    flag &= ~(1 << bit);
                else
                    flag |= (1 << bit);
                user.NewBieGuideInfo.NewbieActionFlag = (long)flag;
                JsonMessageWithGuid svMsg = new JsonMessageWithGuid(JsonMessageID.SyncNewbieActionFlag);
                svMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_SyncNewbieActionFlag protoData = new ArkCrossEngineMessage.Msg_LC_SyncNewbieActionFlag();
                protoData.m_NewbieFlag = user.NewBieGuideInfo.NewbieActionFlag;
                svMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, svMsg);
            }
        }
        internal void HandleSignInAndGetReward(ulong guid)
        {
            UserInfo user = GetUserInfo(guid);
            if (null != user)
            {
                JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.SignInAndGetRewardResult);
                msg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_SignInAndGetRewardResult protoData = new ArkCrossEngineMessage.Msg_LC_SignInAndGetRewardResult();
                if (user.SignIn())
                {
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Succeed;
                    int itemId = 0;
                    int itemCount = 0;
                    SignInRewardConfigProvider.Instance.GetDataByDate(DateTime.Now.Month, user.SignInCountCurMonth, out itemId, out itemCount);
                    if (itemId > 0 && itemCount > 0)
                    {
                        DoAddItem(guid, itemId, itemCount, "SignIn");
                    }
                }
                else
                {
                    if (user.SignInCountCurMonth >= DateTime.Now.Day)
                    {
                        protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Overflow;
                    }
                    else if (user.RestDailySignInCount <= 0)
                    {
                        protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_CostError;
                    }
                    else
                    {
                        protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Unknown;
                    }
                }
                msg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, msg);
            }
        }
        internal void HandleWeeklyLoginReward(ulong guid)
        {
            UserInfo user = GetUserInfo(guid);
            if (null != user)
            {
                JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.WeeklyLoginRewardResult);
                ArkCrossEngineMessage.Msg_LC_WeeklyLoginRewardResult protoData = new ArkCrossEngineMessage.Msg_LC_WeeklyLoginRewardResult();
                msg.m_Guid = guid;
                int index = WeeklyLoginConfigProvider.Instance.GetTodayIndex();
                if (user.LoginRewardInfo.IsGetLoginReward || user.LoginRewardInfo.WeeklyLoginRewardRecord.Contains(index))
                {
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Overflow;
                }
                else if (!WeeklyLoginConfigProvider.Instance.IsUnderProgress())
                {
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Time;
                }
                else
                {
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Succeed;
                    user.LoginRewardInfo.IsGetLoginReward = true;
                    user.SetWeeklyLoginRecord(index);
                    int itemId = 0;
                    int itemNum = 0;
                    WeeklyLoginConfigProvider.Instance.GetRewardByDay(out itemId, out itemNum);
                    DoAddItem(guid, itemId, itemNum, "WeeklyLoginReward");
                }
                msg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, msg);
            }
        }
        internal void HandleOnlineDurationReward(ulong guid, int index)
        {
            UserInfo user = GetUserInfo(guid);
            if (null != user)
            {
                JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.GetOnlineTimeRewardResult);
                ArkCrossEngineMessage.Msg_LC_GetOnlineTimeRewardResult protoData = new ArkCrossEngineMessage.Msg_LC_GetOnlineTimeRewardResult();
                msg.m_Guid = guid;
                int minutes = 0;
                int itemId = 0;
                int itemNum = 0;
                protoData.m_Index = index;
                protoData.m_OnlineTime = user.GetOnlineMinutes();
                OnlineDurationRewardConfigProvider.Instance.GetOnlineRewardByCount(index, out minutes, out itemId, out itemNum);
                if (minutes <= 0)
                {
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Unknown;
                }
                else if (user.OnlineDuration.DailyOnLineRewardedIndex.Contains(index))
                {
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Code_Used;
                }
                else if (user.GetOnlineMinutes() < minutes)
                {
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Failure_Time;
                }
                else
                {
                    protoData.m_ResultCode = (int)GeneralOperationResult.LC_Succeed;
                    if (itemId > 0 && itemNum > 0)
                    {
                        DoAddItem(guid, itemId, itemNum, "OnlineReward");
                    }
                    user.OnlineDuration.AddToDailyOnlineRewardedList(index);
                }
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, msg);
            }
        }
        private void DoResetGowPrize(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.GowInfo)
            {
                user.ResetGowPrize();
                JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.SyncResetGowPrize);
                msg.m_Guid = guid;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, msg);
            }
        }
        private void DoResetDailyMissions(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.Mission)
            {
                user.ResetDailyMissions();
                /// norm log
                AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                if (null != accountInfo)
                {
                    foreach (int missionId in MissionConfigProvider.Instance.GetDailyMissionId())
                    {
                        LogSys.NormLog("gettask", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.gettask, LobbyConfig.LogNormVersionStr,
                        "B3110", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level, missionId, (int)GetTaskResult.Succeed);
                    }
                }
                MissionSystem.Instance.SyncMissionList(user);
            }
        }
        internal void HandleMpveAward(ulong guid)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                MpveAwardResult result = MpveAwardResult.Failure;
                JsonMessageWithGuid jsonMsg = new JsonMessageWithGuid(JsonMessageID.RequestMpveAwardResult);
                jsonMsg.m_Guid = guid;
                ArkCrossEngineMessage.Msg_LC_MpveAwardResult protoData = new ArkCrossEngineMessage.Msg_LC_MpveAwardResult();
                if (user.AttemptCurAcceptedCount < UserInfo.c_AttemptAcceptedHzMax)
                {
                    if (user.AttemptAward > 0)
                    {
                        List<MpveAwardItem> assit_award = new List<MpveAwardItem>();
                        float money = 0;
                        AttemptTollgateConfig award_data = AttemptTollgateConfigProvider.Instance.GetDataById(user.AttemptAward);
                        if (null != award_data)
                        {
                            float money_random_num = CrossEngineHelper.Random.Next(0, 100) / 100.0f;
                            if (money_random_num <= award_data.m_MoneyProbality)
                            {
                                money = award_data.m_DropOutMoney + award_data.m_MoneyFactor * user.FightingScore / 1000.0f;
                            }
                            if (award_data.m_DropOutItemNum > 0)
                            {
                                for (int i = 0; i < award_data.m_DropOutItemNum; i++)
                                {
                                    int award_item_id = award_data.m_ItemId[i];
                                    if (award_item_id > 0)
                                    {
                                        float award_item_probality = award_data.m_ItemProbality[i] + award_data.m_ItemFactor[i] * user.Expedition.ResetScore / 1000.0f;
                                        int accurate_num = (int)award_item_probality;
                                        int random_num = (CrossEngineHelper.Random.Next(0, 100) / 100.0f) <= (award_item_probality - (float)accurate_num) ? 1 : 0;
                                        int total_num = accurate_num + random_num;
                                        if (total_num > 0)
                                        {
                                            MpveAwardItem award_item = new MpveAwardItem();
                                            award_item.ItemId = award_item_id;
                                            award_item.ItemNum = total_num;
                                            assit_award.Add(award_item);
                                        }
                                    }
                                }
                            }
                            user.AttemptAcceptedAward = user.AttemptAward;
                            user.AttemptCurAcceptedCount += 1;
                            ///
                            DoAddAssets(guid, (int)money, 0, 0, 0, GainConsumePos.Mpve.ToString());
                            protoData.m_AddMoney = (int)money;
                            int award_ct = assit_award.Count;
                            if (award_ct > 0)
                            {
                                for (int i = 0; i < award_ct; i++)
                                {
                                    int item_id = assit_award[i].ItemId;
                                    int item_num = assit_award[i].ItemNum;
                                    ArkCrossEngineMessage.Msg_LC_MpveAwardResult.AwardItemInfo asset_info = new ArkCrossEngineMessage.Msg_LC_MpveAwardResult.AwardItemInfo();
                                    asset_info.m_Id = item_id;
                                    asset_info.m_Num = item_num;
                                    protoData.m_Items.Add(asset_info);
                                    DoAddItem(guid, item_id, item_num, GainConsumePos.Mpve.ToString());
                                }
                            }
                            result = MpveAwardResult.Succeed;
                        }
                    }
                    else
                    {
                        result = MpveAwardResult.Nothing;
                    }
                }
                else
                {
                    result = MpveAwardResult.Gained;
                }
                protoData.m_AwardIndex = user.AttemptAward;
                protoData.m_Result = (int)result;
                jsonMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, jsonMsg);
            }
        }
        private void AddMonthCard(ulong guid, int count)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                bool needSyncMission = false;
                if (!user.IsHaveMonthCard())
                {
                    needSyncMission = true;
                }
                int day = MonthCardConfigProvider.Instacne.GetDuration() * count;
                int diamond = MonthCardConfigProvider.Instacne.GetRewardDiamond() * count;
                user.AddMonthCardTime(day);
                IncreaseAsset(guid, diamond, GainAssetType.StageClear, AssetType.Glod, "MonthCard");
                if (needSyncMission)
                {
                    MissionSystem.Instance.ResetMonthCardMissions(user);
                    MissionSystem.Instance.SyncMissionList(user);
                }
            }
        }
        private void IncreaseAsset(ulong guid, int incr, GainAssetType gain_type, AssetType asset_type, string gain_pos)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user)
                return;
            if (incr <= 0)
                return;
            AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
            if (AssetType.Money == asset_type)
            {
                int last_money = user.Money;
                user.Money += incr;
                int cur_money = user.Money;
                /// norm log
                if (null != accountInfo)
                {
                    LogSys.NormLog("acquire", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.acquire, LobbyConfig.LogNormVersionStr,
                    "B1010", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)gain_type, incr, cur_money, (int)asset_type, user.Vip, gain_pos);
                }
            }
            else if (AssetType.Glod == asset_type)
            {
                int last_gold = user.Gold;
                user.Gold += incr;
                int cur_gold = user.Gold;
                /// norm log
                if (null != accountInfo)
                {
                    LogSys.NormLog("acquire", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.acquire, LobbyConfig.LogNormVersionStr,
                    "B1010", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)gain_type, incr, cur_gold, (int)asset_type, user.Vip, gain_pos);
                }
            }
            else if (AssetType.Exp == asset_type)
            {
                int last_exp = user.ExpPoints;
                user.ExpPoints += incr;
                int cur_exp = user.ExpPoints;
                /// norm log
                if (null != accountInfo)
                {
                    LogSys.NormLog("customacquire", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.customacquire, LobbyConfig.LogNormVersionStr,
                    "C0100", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)gain_type, incr, cur_exp, (int)asset_type, user.Vip, gain_pos);
                }
            }
            else if (AssetType.Stamina == asset_type)
            {
                int last_stamina = user.CurStamina;
                user.CurStamina += incr;
                int cur_stamina = user.CurStamina;
                /// norm log
                if (null != accountInfo)
                {
                    LogSys.NormLog("customacquire", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.customacquire, LobbyConfig.LogNormVersionStr,
                    "C0100", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)gain_type, incr, cur_stamina, (int)asset_type, user.Vip, gain_pos);
                }
            }
            else if (AssetType.Vigor == asset_type)
            {
                int last_vigor = user.Vigor;
                user.Vigor += incr;
            }
        }
        internal void ConsumeAsset(ulong guid, int cons, ConsumeAssetType consume_type, AssetType asset_type, string consume_pos)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null == user)
                return;
            if (cons <= 0)
                return;
            AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
            if (AssetType.Money == asset_type)
            {
                int last_money = user.Money;
                user.Money -= cons;
                int cur_money = user.Money;
                /// norm log
                if (null != accountInfo)
                {
                    LogSys.NormLog("moneycost", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.moneycost, LobbyConfig.LogNormVersionStr,
                    "B1150", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)consume_type, cons, cur_money, (int)asset_type, user.Vip, consume_pos);
                }
            }
            else if (AssetType.Glod == asset_type)
            {
                int last_gold = user.Gold;
                user.Gold -= cons;
                int cur_gold = user.Gold;
                /// norm log
                if (null != accountInfo)
                {
                    LogSys.NormLog("moneycost", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.moneycost, LobbyConfig.LogNormVersionStr,
                    "B1150", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)consume_type, cons, cur_gold, (int)asset_type, user.Vip, consume_pos);
                }
            }
            else if (AssetType.Stamina == asset_type)
            {
                int last_stamina = user.CurStamina;
                user.UsedStamina += cons;
                user.CurStamina -= cons;
                int cur_stamina = user.CurStamina;
                /// norm log
                if (null != accountInfo)
                {
                    LogSys.NormLog("customcost", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.customcost, LobbyConfig.LogNormVersionStr,
                    "C0200", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)consume_type, cons, cur_stamina, (int)asset_type, user.Vip, consume_pos);
                }
            }
            else if (AssetType.Vigor == asset_type)
            {
                int last_vigor = user.Vigor;
                user.Vigor -= cons;
                int cur_vigor = user.Vigor;
                /// norm log
                if (null != accountInfo)
                {
                    LogSys.NormLog("customcost", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.customcost, LobbyConfig.LogNormVersionStr,
                    "C0200", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)consume_type, cons, cur_vigor, (int)asset_type, user.Vip, consume_pos);
                }
            }
        }
        private void IncreaseItem(ulong guid, ItemInfo item, int item_num, GainItemType type, GainItemWay way, string increase_pos)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.ItemBag && null != item
              && user.ItemBag.ItemCount < ItemBag.c_MaxItemNum)
            {
                user.ItemBag.AddItemData(item, item_num);
                /// norm log
                AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                if (null != accountInfo)
                {
                    LogSys.NormLog("getitem", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.getitem, LobbyConfig.LogNormVersionStr,
                    "B2110", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)type, item.ItemId, (int)way, item_num, user.Vip, increase_pos);
                }
            }
        }
        internal void ConsumeItem(ulong guid, ItemInfo item, int item_num, GainItemType type, ConsumeItemWay way, bool delete, string consume_pos)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user && null != user.ItemBag && null != item)
            {
                if (delete)
                {
                    user.ItemBag.DelItemData(item);
                }
                else
                {
                    user.ItemBag.ReduceItemData(item, item_num);
                }
                /// norm log
                AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                if (null != accountInfo)
                {
                    LogSys.NormLog("removeitem", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.removeitem, LobbyConfig.LogNormVersionStr,
                    "B2150", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level,
                    (int)type, item.ItemId, (int)way, item_num, user.Vip, consume_pos);
                }
            }
        }
        private void UpdateSignInData(UserInfo user)
        {
            if (user.UpdateSignInData())
            {
                JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.SyncSignInCount);
                msg.m_Guid = user.Guid;
                ArkCrossEngineMessage.Msg_LC_SyncSignInCount protoData = new ArkCrossEngineMessage.Msg_LC_SyncSignInCount();
                protoData.m_SignInCountCurMonth = user.SignInCountCurMonth;
                protoData.m_RestSignInCountCurDay = user.RestDailySignInCount;
                msg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, msg);
            }
        }
        private void UpdateDailyOnlineDurationData(UserInfo user)
        {
            if (user.UpdateDailyOnlineDuration())
            {
                JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.ResetOnlineTimeRewardData);
                msg.m_Guid = user.Guid;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, msg);
            }
        }
        private void UpdateWeeklyLoginData(UserInfo user)
        {
            if (user.UpdateWeeklyLoginData())
            {
                JsonMessageWithGuid msg = new JsonMessageWithGuid(JsonMessageID.ResetWeeklyLoginRewardData);
                msg.m_Guid = user.Guid;
                JsonMessageDispatcher.SendDcoreMessage(user.NodeName, msg);
            }
        }
        private void UpdateDailyMissions(UserInfo user)
        {
            TimeSpan deltaTime = DateTime.Now - user.LastResetDailyMissionTime;
            if (deltaTime.Days > 0)
            {
                DoResetDailyMissions(user.Guid);
            }
        }
        private void UpdateGowPrize(UserInfo user)
        {
            TimeSpan deltaTime = DateTime.Now - user.LastResetGowPrizeTime;
            if (deltaTime.Days > 0)
            {
                DoResetGowPrize(user.Guid);
            }
        }
        private void UpdateGroupMemberStatus(UserInfo user)
        {
            if (null == user)
                return;
            if (UserState.Room == user.CurrentState)
                return;
            if (null != user.Group)
            {
                List<ulong> offline_users = user.CheckGroupMemberStatus();
                if (null != offline_users && offline_users.Count > 0)
                {
                    for (int k = 0; k < offline_users.Count; k++)
                    {
                        if (offline_users[k] != null)
                        {
                            UserInfo member = this.GetUserInfo(offline_users[k]);
                            if (null != member)
                            {
                                MatchFormThread match_form = LobbyServer.Instance.MatchFormThread;
                                match_form.QueueAction(match_form.QuitGroup, member.Guid, member.Nickname);
                            }
                        }
                    }
                }
            }
        }
        private void UpdateExpedition(UserInfo user)
        {
            if (null == user)
                return;
            if (null != user.Expedition)
            {
                if (user.Expedition.IsUnlock)
                {
                    user.Expedition.ResetExpeditionTime();
                }
                if (user.Level >= ExpeditionPlayerInfo.UnlockLevel && !user.Expedition.IsUnlock)
                {
                    user.Expedition.IsUnlock = true;
                    user.Expedition.CanReset = true;
                    GlobalDataProcessThread global_process = LobbyServer.Instance.GlobalDataProcessThread;
                    if (null != global_process)
                    {
                        long cur_time = Convert.ToInt64(TimeUtility.CurTimestamp);
                        global_process.QueueAction(global_process.HandleRequestExpeditionInfo, user.Guid, 1000, 1000, 0, 0, true, true, cur_time);
                    }
                }
            }
        }
        internal void InitActivationCodeData(List<DS_ActivationCode> activationCodeList)
        {
            m_ActivationCodeSystem.InitActivationCodeData(activationCodeList);
        }
        internal void InitNicknameData(List<DS_Nickname> nicknameList)
        {
            m_NicknameSystem.InitNicknameData(nicknameList);
        }
        internal void RecordCampaignAction(ulong guid, int type)
        {
            const int activity_base = 4000;
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                user.UpdateGuideFlag(type);
                if (type > activity_base)
                {
                    AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                    if (null == accountInfo)
                        return;
                    /// norm log
                    LogSys.NormLog("activity", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.activity, LobbyConfig.LogNormVersionStr,
                      "B6110", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level, type);
                }
            }
        }
        private void RecordPartnerAction(ulong guid, int partnerid, int partnerlevel, PartnerCauseId type, PartnerOperateResult result)
        {
            UserInfo user = LobbyServer.Instance.DataProcessScheduler.GetUserInfo(guid);
            if (null != user)
            {
                AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                if (null == accountInfo)
                    return;
                /// norm log
                LogSys.NormLog("partner", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.partner, LobbyConfig.LogNormVersionStr,
                  "C0300", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Level, partnerid, (int)type,
                  result == PartnerOperateResult.Null ? "null" : ((int)result).ToString(), partnerlevel);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------
        //这些方法是一些工具方法，后面需要重新考虑并发相关的处理。
        //--------------------------------------------------------------------------------------------------------------------------
        internal UserInfo NewUserInfo()
        {
            UserInfo info = null;
            if (m_UnusedUserInfos.IsEmpty)
            {
                info = new UserInfo();
            }
            else
            {
                if (!m_UnusedUserInfos.TryDequeue(out info))
                {
                    info = new UserInfo();
                }
                else
                {
                    info.IsRecycled = false;
                }
            }
            return info;
        }
        internal void RecycleUserInfo(UserInfo info)
        {
            info.IsRecycled = true;
            info.Reset();
            m_UnusedUserInfos.Enqueue(info);
        }
        private string GenerateNickname()
        {
            int firstCount = FirstNameProvider.Instance.GetDataCount();
            int fi = m_Random.Next(1, firstCount);
            FirstName fn = FirstNameProvider.Instance.GetDataById(fi);
            int lastCount = LastNameProvider.Instance.GetDataCount();
            int li = m_Random.Next(1, lastCount);
            LastName ln = LastNameProvider.Instance.GetDataById(li);
            string fullName = string.Format("{0}-{1}", fn.m_FirstName, ln.m_LastName);
            return fullName;
        }
        private bool IsGoldScene(int sceneId)
        {
            bool ret = false;
            Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
            if (null != cfg
              && (int)SceneTypeEnum.TYPE_MULTI_PVE == cfg.m_Type
              && (int)SceneSubTypeEnum.TYPE_GOLD == cfg.m_SubType)
            {
                ret = true;
            }
            return ret;
        }
        private bool IsAttempScene(int sceneId)
        {
            bool ret = false;
            Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
            if (null != cfg
              && (int)SceneTypeEnum.TYPE_MULTI_PVE == cfg.m_Type
              && (int)SceneSubTypeEnum.TYPE_ATTEMPT == cfg.m_SubType)
            {
                ret = true;
            }
            return ret;
        }
        private bool IsMpveScene(int sceneId)
        {
            bool ret = false;
            Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
            if (null != cfg && (int)SceneTypeEnum.TYPE_MULTI_PVE == cfg.m_Type)
            {
                ret = true;
            }
            return ret;
        }
        private void InitUserinfo(UserInfo ui)
        {
            ui.Money = 0;
            ui.Gold = 0;
            ui.CurStamina = 120;
            ui.Vigor = 500;
            ui.ExpPoints = 0;
            ui.CitySceneId = 1020;
            ui.Level = 30;
            ui.Vip = 1;
            ui.CreateTime = DateTime.Now;
            ui.NextUserSaveCount = 1;
            ui.CurrentUserSaveCount = 0;
            ui.LastAddStaminaTimestamp = TimeUtility.CurTimestamp;
            ui.ResetSceneCompletedCount();
            ui.ResetSignInDailyCount();
            ui.ResetSignInMonthCount();
            ui.FightingScore = 200;
            ui.NewBieGuideInfo.NewbieFlag = 0;
            ui.NewBieGuideInfo.NewbieActionFlag = 0;
            ui.NewBieGuideInfo.GuideFlag = 0;
            ui.CurrentBattleInfo.init(ui.NewbieScene, ui.HeroId);
            // 开发版本关掉新手引导。如果需要打开新手引导，注释掉下面一行。
            //ui.SetSceneInfo(9001, 1);
            //ui.NewBieGuideList.Add(1);
            // equipments
            Data_PlayerConfig player_data = PlayerConfigProvider.Instance.GetPlayerConfigById(ui.HeroId) as Data_PlayerConfig;
            if (null != player_data && null != player_data.m_NoviceEquipList
              && player_data.m_NoviceEquipList.Count == EquipInfo.c_MaxEquipmentNum)
            {
                int[] equipments = player_data.m_NoviceEquipList.ToArray();
                for (int i = 0; i < equipments.Length; i++)
                {
                    if (ui.Equip.Armor[i] == null)
                    {
                        ItemInfo info = new ItemInfo(equipments[i]);
                        info.Level = 1;
                        info.ItemNum = 1;
                        ui.Equip.SetEquipmentData(i, info);
                    }
                }
            }
            //items
            if (ui.ItemBag.ItemCount == 0)
            {
                /*
                int[] bagItems = new int[] { 24101, 24102, 24103, 24104, 24105, 24106, 34101, 34102, 34103, 34104, 34105, 34106, 44101, 44102, 44103, 44104, 44105, 44106, 45101, 45102, 45103, 45104, 45105, 45106, 45201, 45202, 45203, 45204, 45205, 45206, 55101, 55102, 55103, 55104, 55105, 55106, 55201, 55202, 55203, 55204, 55205, 55206, 91002 };
                foreach (int item_id in bagItems) {
                  ItemInfo info = new ItemInfo(item_id);
                  ui.ItemBag.AddItemData(info);
                }*/
                /// Legacy Cost Item
                ItemInfo new_info = new ItemInfo(916011);
                new_info.ItemNum = 100;
                ui.ItemBag.AddItemData(new_info, new_info.ItemNum);
                new_info = new ItemInfo(916021);
                new_info.ItemNum = 100;
                ui.ItemBag.AddItemData(new_info, new_info.ItemNum);
                new_info = new ItemInfo(916031);
                new_info.ItemNum = 100;
                ui.ItemBag.AddItemData(new_info, new_info.ItemNum);
                new_info = new ItemInfo(916041);
                new_info.ItemNum = 100;
                ui.ItemBag.AddItemData(new_info, new_info.ItemNum);
            }
            //ui.ItemBag.AddItemData(new ItemInfo(303010), 100);
            //ui.ItemBag.AddItemData(new ItemInfo(303020), 100);
            //ui.ItemBag.AddItemData(new ItemInfo(303030), 100);
            //ui.ItemBag.AddItemData(new ItemInfo(104), 1);
            //ui.ItemBag.AddItemData(new ItemInfo(105), 100);
            //ui.ItemBag.AddItemData(new ItemInfo(846101), 100);
            //skills
            if (ui.Skill.Skills.Count == 0)
            {
                Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(ui.HeroId);
                if (null != playerData && null != playerData.m_PreSkillList)
                {
                    SkillDataInfo[] skillInfo = new SkillDataInfo[playerData.m_PreSkillList.Count];
                    for (int i = 0; i < skillInfo.Length; i++)
                    {
                        skillInfo[i] = new SkillDataInfo(playerData.m_PreSkillList[i]);
                    }
                    if (skillInfo.Length >= 2)
                    {
                        skillInfo[0].Level = 1;
                        skillInfo[0].Postions.Presets[0] = SlotPosition.SP_A;
                        skillInfo[1].Level = 1;
                        skillInfo[1].Postions.Presets[0] = SlotPosition.SP_B;
                    }
                    foreach (SkillDataInfo skill in skillInfo)
                    {
                        ui.Skill.AddSkillData(skill);
                    }
                    ui.Skill.CurPresetIndex = 0;
                }
            }
            // missions
            List<int> completedMissions = new List<int>();
            List<int> lockedMissions = new List<int>();
            List<int> uncompletedMissions = new List<int>();
            int count = 0;
            if (ui.Mission.UnCompletedMissions.Count == 0)
            {
                foreach (MissionConfig ms in MissionConfigProvider.Instance.GetData().Values)
                {
                    if (ms.IsBornAccept)
                    {
                        uncompletedMissions.Add(ms.Id);
                    }
                    else
                    {
                        lockedMissions.Add(ms.Id);
                    }
                }
                for (int i = 0; i < completedMissions.Count; ++i)
                {
                    ui.Mission.AddMission(completedMissions[i], MissionStateType.COMPLETED);
                }
                for (int i = 0; i < lockedMissions.Count; ++i)
                {
                    ui.Mission.AddMission(lockedMissions[i], MissionStateType.LOCKED);
                }
                for (int i = 0; i < uncompletedMissions.Count; ++i)
                {
                    ui.Mission.AddMission(uncompletedMissions[i], MissionStateType.UNCOMPLETED);
                }
            }
            ui.ResetDailyMissions();
            // Legacys
            ui.Legacy.SetLegacyData(0, new ItemInfo(101010, 0, 1, false));
            ui.Legacy.SetLegacyData(1, new ItemInfo(101020, 0, 1, false));
            ui.Legacy.SetLegacyData(2, new ItemInfo(101030, 0, 1, false));
            ui.Legacy.SetLegacyData(3, new ItemInfo(101040, 0, 1, false));

            // XSouls
            InitXSoulPart(ui);

            // gow
            if (null != ui.GowInfo)
            {
                ui.GowInfo.GowElo = 1000;
                ui.GowInfo.GowMatches = 0;
                ui.GowInfo.GowWinMatches = 0;
                ui.GowInfo.LeftMatchCount = 0;
                ui.GowInfo.InitRankInfo(1, 0, 0, 0, 0);
                ui.GowInfo.IsAcquirePrize = false;
            }
            // expedition
            ui.Expedition.IsUnlock = false;
            ui.Expedition.Schedule = 0;
            // partner
            /*
            ui.PartnerStateInfo.Reset();
            ui.PartnerStateInfo.AddPartner(10001, 3, 3);
            ui.PartnerStateInfo.AddPartner(10002, 3, 3);
            ui.PartnerStateInfo.AddPartner(10003, 3, 3);
            ui.PartnerStateInfo.AddPartner(10004, 3, 3);
            ui.PartnerStateInfo.AddPartner(10005, 3, 3);
            ui.PartnerStateInfo.AddPartner(10006, 3, 3);
            ui.PartnerStateInfo.AddPartner(10007, 3, 3);
            ui.PartnerStateInfo.AddPartner(10008, 3, 3);
            ui.PartnerStateInfo.SetActivePartner(10001);
             */
            // mpve
            ui.AttemptAward = 0;
            ui.AttemptCurAcceptedCount = 0;
            ui.AttemptAcceptedAward = 0;
            ui.GoldCurAcceptedCount = 0;
        }
        private void InitXSoulPart(UserInfo user)
        {
            Data_PlayerConfig config_data = PlayerConfigProvider.Instance.GetPlayerConfigById(user.HeroId);
            if (config_data != null)
            {
                foreach (int xsoul_item_id in config_data.m_InitXSoulPart)
                {
                    ItemConfig item_config = ItemConfigProvider.Instance.GetDataById(xsoul_item_id);
                    if (item_config != null)
                    {
                        ItemInfo xsoul_part = new ItemInfo(xsoul_item_id, 0, 1, true);
                        user.XSoul.SetXSoulPartData((XSoulPart)item_config.m_WearParts, xsoul_part);
                    }
                }
            }
        }

        private ArkCrossEngineMessage.Msg_LC_RoleEnterResult CreateRoleEnterResultMsg(UserInfo ui)
        {
            ArkCrossEngineMessage.Msg_LC_RoleEnterResult replyMsg = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult();
            replyMsg.m_NewbieGuideScene = ui.NewbieScene;
            // 场景信息
            foreach (KeyValuePair<int, int> pair in ui.SceneData)
            {
                int scene = pair.Key;
                ArkCrossEngineMessage.Msg_LC_RoleEnterResult.SceneDataMsg sceneData = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult.SceneDataMsg();
                sceneData.m_SceneId = scene;
                sceneData.m_Grade = pair.Value;
                replyMsg.m_SceneData.Add(sceneData);
            }
            foreach (int scene in ui.ScenesCompletedCountData.Keys)
            {
                ArkCrossEngineMessage.Msg_LC_RoleEnterResult.ScenesCompletedCountDataMsg sceneCompletedCountData = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult.ScenesCompletedCountDataMsg();
                sceneCompletedCountData.m_SceneId = scene;
                sceneCompletedCountData.m_Count = ui.GetCompletedSceneCount(scene);
                replyMsg.m_SceneCompletedCountData.Add(sceneCompletedCountData);
            }
            // 引导信息
            {
                // Add lock.
                lock (ui.NewBieGuideInfo.Lock)
                {
                    for (int i = 0; i < ui.NewBieGuideInfo.NewBieGuideList.Count; ++i)
                    {
                        replyMsg.m_NewbieGuides.Add(ui.NewBieGuideInfo.NewBieGuideList[i]);
                    }
                }
            }

            //money gold
            replyMsg.m_Money = ui.Money;
            replyMsg.m_Gold = ui.Gold;
            replyMsg.m_Stamina = ui.CurStamina;
            replyMsg.m_Vigor = ui.Vigor;
            replyMsg.m_Exp = ui.ExpPoints;
            replyMsg.m_Level = ui.Level;
            replyMsg.m_CitySceneId = ui.CitySceneId;
            replyMsg.m_BuyStaminaCount = ui.CurBuyStaminaCount;
            replyMsg.m_BuyMoneyCount = ui.CurBuyMoneyCount;
            replyMsg.m_CurSellItemGoldIncome = ui.CurSellItemGoldIncome;
            replyMsg.m_Vip = ui.Vip;
            replyMsg.m_SignInCountCurMonth = ui.SignInCountCurMonth;
            replyMsg.m_RestSignInCountCurDay = ui.RestDailySignInCount;
            replyMsg.m_NewbieFlag = ui.NewBieGuideInfo.NewbieFlag;
            replyMsg.m_NewbieActionFlag = ui.NewBieGuideInfo.NewbieActionFlag;
            replyMsg.m_GuideFlag = ui.NewBieGuideInfo.GuideFlag;
            replyMsg.m_IsGetWeeklyReward = ui.LoginRewardInfo.IsGetLoginReward;
            replyMsg.m_OnlineDuration = ui.OnlineDuration.DailyOnLineDuration;

            {     // Add Lock.
                lock (ui.LoginRewardInfo.Lock)
                {
                    for (int i = 0; i < ui.LoginRewardInfo.WeeklyLoginRewardRecord.Count; ++i)
                    {
                        replyMsg.m_WeeklyRewardRecord.Add(ui.LoginRewardInfo.WeeklyLoginRewardRecord[i]);
                    }
                }
            }

            {     // Add Lock.
                lock (ui.OnlineDuration.Lock)
                {
                    for (int i = 0; i < ui.OnlineDuration.DailyOnLineRewardedIndex.Count; ++i)
                    {
                        replyMsg.m_OnlineTimeRewardedIndex.Add(ui.OnlineDuration.DailyOnLineRewardedIndex[i]);
                    }
                }
            }

            //items
            for (int i = 0; i < ui.ItemBag.ItemInfos.Count; ++i)
            {
                ArkCrossEngineMessage.ItemDataMsg bag_item = new ArkCrossEngineMessage.ItemDataMsg();
                bag_item.ItemId = ui.ItemBag.ItemInfos[i].ItemId;
                bag_item.Level = ui.ItemBag.ItemInfos[i].Level;
                bag_item.Num = ui.ItemBag.ItemInfos[i].ItemNum;
                bag_item.AppendProperty = ui.ItemBag.ItemInfos[i].AppendProperty;
                replyMsg.m_BagItems.Add(bag_item);
            }
            //equipments
            for (int i = 0; i < ui.Equip.Armor.Length; ++i)
            {
                if (null != ui.Equip.Armor[i])
                {
                    ArkCrossEngineMessage.ItemDataMsg equip = new ArkCrossEngineMessage.ItemDataMsg();
                    equip.ItemId = ui.Equip.Armor[i].ItemId;
                    equip.Level = ui.Equip.Armor[i].Level;
                    equip.Num = ui.Equip.Armor[i].ItemNum;
                    equip.AppendProperty = ui.Equip.Armor[i].AppendProperty;
                    replyMsg.m_Equipments.Add(equip);
                }
            }
            //skills
            for (int i = 0; i < ui.Skill.Skills.Count; ++i)
            {
                ArkCrossEngineMessage.SkillDataInfo skill_data = new ArkCrossEngineMessage.SkillDataInfo();
                skill_data.ID = ui.Skill.Skills[i].ID;
                skill_data.Level = ui.Skill.Skills[i].Level;
                skill_data.Postions = (int)ui.Skill.Skills[i].Postions.Presets[0];
                replyMsg.m_SkillInfo.Add(skill_data);
            }
            //missions
            foreach (MissionInfo mi in ui.Mission.UnCompletedMissions.Values)
            {
                ArkCrossEngineMessage.Msg_LC_RoleEnterResult.MissionInfoForSync info = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult.MissionInfoForSync();
                info.m_MissionId = mi.MissionId;
                info.m_IsCompleted = false;
                info.m_Progress = MissionSystem.Instance.GetMissionProgress(ui, mi, false);
                replyMsg.m_Missions.Add(info);
            }
            foreach (MissionInfo mi in ui.Mission.CompletedMissions.Values)
            {
                ArkCrossEngineMessage.Msg_LC_RoleEnterResult.MissionInfoForSync info = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult.MissionInfoForSync();
                info.m_MissionId = mi.MissionId;
                info.m_IsCompleted = true;
                info.m_Progress = MissionSystem.Instance.GetMissionProgress(ui, mi, true);
                replyMsg.m_Missions.Add(info);
            }
            //Legacys
            int legacy_count = ui.Legacy.SevenArcs.Length;
            if (legacy_count > 0)
            {
                for (int i = 0; i < legacy_count; i++)
                {
                    if (ui.Legacy.SevenArcs[i] != null)
                    {
                        ArkCrossEngineMessage.LegacyDataMsg legacy_data = new ArkCrossEngineMessage.LegacyDataMsg();
                        legacy_data.ItemId = ui.Legacy.SevenArcs[i].ItemId;
                        legacy_data.Level = ui.Legacy.SevenArcs[i].Level;
                        legacy_data.AppendProperty = ui.Legacy.SevenArcs[i].AppendProperty;
                        legacy_data.IsUnlock = ui.Legacy.SevenArcs[i].IsUnlock;
                        replyMsg.m_Legacys.Add(legacy_data);
                    }
                }
            }
            // XSouls
            foreach (ItemInfo item in ui.XSoul.GetAllXSoulPartData().Values)
            {
                ArkCrossEngineMessage.XSoulDataMsg item_msg = new ArkCrossEngineMessage.XSoulDataMsg();
                item_msg.ItemId = item.ItemId;
                item_msg.Level = item.Level;
                item_msg.Experience = item.Experience;
                item_msg.ModelLevel = item.ShowModelLevel;
                replyMsg.m_XSouls.Add(item_msg);
            }
            // Gow
            GowInfo gow_info = ui.GowInfo;
            if (null != gow_info)
            {
                replyMsg.m_Gow = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult.GowDataMsg();
                replyMsg.m_Gow.GowElo = gow_info.GowElo;
                replyMsg.m_Gow.GowMatches = gow_info.GowMatches;
                replyMsg.m_Gow.GowWinMatches = gow_info.GowWinMatches;
                replyMsg.m_Gow.LeftMatchCount = gow_info.LeftMatchCount;
                replyMsg.m_Gow.RankId = gow_info.RankId;
                replyMsg.m_Gow.Point = gow_info.Point;
                replyMsg.m_Gow.CriticalTotalMatches = gow_info.CriticalTotalMatches;
                replyMsg.m_Gow.CriticalAmassWinMatches = gow_info.AmassWinMatches;
                replyMsg.m_Gow.CriticalAmassLossMatches = gow_info.AmassLossMatches;
                replyMsg.m_Gow.IsAcquirePrize = gow_info.IsAcquirePrize;
            }
            // Friends
            ConcurrentDictionary<ulong, FriendInfo> friends = ui.FriendInfos;
            if (null != friends && friends.Count > 0)
            {
                foreach (FriendInfo info in friends.Values)
                {
                    ArkCrossEngineMessage.FriendInfoForMsg msg = new ArkCrossEngineMessage.FriendInfoForMsg();
                    msg.Guid = info.Guid;
                    msg.Nickname = info.Nickname;
                    msg.Level = info.Level;
                    msg.FightingScore = info.FightingScore;
                    msg.IsBlack = info.IsBlack;
                    replyMsg.m_Friends.Add(msg);
                }
            }
            // Partners
            if (null != ui.PartnerStateInfo)
            {
                foreach (PartnerInfo parterInfo in ui.PartnerStateInfo.GetAllPartners())
                {
                    ArkCrossEngineMessage.Msg_LC_RoleEnterResult.PartnerDataMsg msg = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult.PartnerDataMsg();
                    msg.m_Id = parterInfo.Id;
                    msg.m_SkillStage = parterInfo.CurSkillStage;
                    msg.m_AdditionLevel = parterInfo.CurAdditionLevel;
                    replyMsg.m_Partners.Add(msg);
                }
                replyMsg.m_ActivePartnerId = ui.PartnerStateInfo.GetActivePartnerId();
            }
            //ExchangeGoods
            if (null != ui.ExchangeGoodsInfo)
            {
                ConcurrentDictionary<int, int> goodsdic = ui.ExchangeGoodsInfo.GetAllGoodsData();
                if (null != goodsdic)
                {
                    foreach (KeyValuePair<int, int> pair in goodsdic)
                    {
                        int id = pair.Key;
                        ArkCrossEngineMessage.Msg_LC_RoleEnterResult.ExchangeGoodsMsg msg = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult.ExchangeGoodsMsg();
                        msg.m_Id = id;
                        msg.m_Num = pair.Value;
                        replyMsg.m_Exchanges.Add(msg);
                    }
                }
                ConcurrentDictionary<int, int> refresh = ui.ExchangeGoodsInfo.CurrencyRefreshNum;
                if (null != refresh)
                {
                    foreach (KeyValuePair<int, int> pair in refresh)
                    {
                        int currency = pair.Key;
                        ArkCrossEngineMessage.Msg_LC_RoleEnterResult.ExchangeRefreshMsg msg = new ArkCrossEngineMessage.Msg_LC_RoleEnterResult.ExchangeRefreshMsg();
                        msg.m_CurrencyId = currency;
                        msg.m_Num = pair.Value;
                        replyMsg.m_RefreshExchangeNum.Add(msg);
                    }
                }
            }
            // Mpve
            replyMsg.m_AttemptAward = ui.AttemptAward;
            replyMsg.m_AttemptCurAcceptedCount = ui.AttemptCurAcceptedCount;
            replyMsg.m_AttemptAcceptedAward = ui.AttemptAcceptedAward;
            replyMsg.m_GoldCurAcceptedCount = ui.GoldCurAcceptedCount;
            replyMsg.m_WorldId = LobbyConfig.WorldId;
            return replyMsg;
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //后面的方法都是在内部线程执行的方法，不涉及多线程操作，不用加锁，串行执行。
        //--------------------------------------------------------------------------------------------------------------------------
        private void OnTick()
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (m_LastLogTime + 60000 < curTime)
            {
                m_LastLogTime = curTime;

                DebugPoolCount((string msg) =>
                {
                    LogSys.Log(LOG_TYPE.INFO, "DataProcessScheduler.DispatchActionQueue {0}", msg);
                });
                m_Thread.DebugPoolCount((string msg) =>
                {
                    LogSys.Log(LOG_TYPE.INFO, "DataProcessScheduler.ThreadActionQueue {0}", msg);
                });
                LogSys.Log(LOG_TYPE.WARN, "Lobby User Count:{0}", m_ActiveUserGuids.Count);
            }
            /// norm log
            if (m_LastNormLogTime + 600000 < curTime)
            {
                m_LastNormLogTime = curTime;
                int online_user_count = 0;
                if (null != m_ActiveUserGuids)
                {
                    online_user_count = m_ActiveUserGuids.Count;
                }
                string game_version = VersionConfigProvider.Instance.GetVersionNum();
                LogSys.NormLog("heart", LobbyConfig.AppKeyStr, game_version, Module.heart, LobbyConfig.LogNormVersionStr,
                  "B9990", LobbyConfig.ServerId, online_user_count);
            }

            var ds_thread = LobbyServer.Instance.DataStoreThread;
            m_DeactiveUserGuids.Clear();
            int userDSDoneCount = 0;          //完成最后一次存储操作的UserInfo计数
            foreach (ulong guid in m_ActiveUserGuids)
            {
                UserInfo user = GetUserInfo(guid);
                if (user == null)
                {
                    m_DeactiveUserGuids.Add(guid);
                    userDSDoneCount++;
                }
                else
                {
                    if (user.CurrentUserSaveCount == 0)
                    {
                        userDSDoneCount++;
                    }
                    user.LeftLife -= m_Thread.TickSleepTime;
                    if (user.LeftLife <= 0)
                    {
                        if (UserState.Room != user.CurrentState)
                        {
                            DispatchAction(this.DoUserLogoff, guid, true);
                            LogSys.Log(LOG_TYPE.INFO, "LobbyClient user dropped! , guid:{0} , state:{1} , LeftLife:{2}", guid, user.CurrentState, user.LeftLife);
                            user.LeftLife = 600000;
                        }
                        else
                        {
                            user.LeftLife = UserInfo.LifeTimeOfNoHeartbeat;
                        }
                    }
                    if (ds_thread.DataStoreAvailable)
                    {
                        if (user.CurrentState == UserState.Online && curTime - user.LastDSSaveTime > LobbyConfig.UserDSSaveInterval && user.NextUserSaveCount > 0)
                        {
                            ds_thread.DSPSaveUser(user, user.NextUserSaveCount);
                            user.NextUserSaveCount++;
                            user.LastDSSaveTime = curTime;
                        }
                    }
                    if (user.LastCheckDataTime + 1000 < curTime)
                    {
                        user.LastCheckDataTime = curTime;
                        /// norm log
                        int last_level = user.Level;
                        bool ret = user.CheckLevelup();
                        if (ret)
                        {
                            MissionSystem.Instance.CheckAndSyncMissions(user);
                            int cur_level = user.Level;
                            AccountInfo accountInfo = FindAccountInfoById(user.AccountId);
                            if (null != accountInfo)
                            {
                                LogSys.NormLog("levelup", LobbyConfig.AppKeyStr, accountInfo.ClientGameVersion, Module.levelup, LobbyConfig.LogNormVersionStr,
                                "6010", accountInfo.LogicServerId, accountInfo.AccountId, user.Guid, user.Nickname, cur_level, last_level);
                            }
                        }
                        if (user.IncreaseVigor())
                        {
                            IncreaseAsset(guid, 1, GainAssetType.AutoRecover, AssetType.Vigor, GainConsumePos.AutoRecover.ToString());
                        }
                        user.IncreaseStamina();
                        user.UpdateAttempt();
                        user.UpdateGoldTollgate();
                        user.UpdateMidasTouch();
                        //user.UpdateExchangeGoods();
                        user.UpdateSellItemIncome();
                        user.UpdateSceneCompletedCountData();
                        UpdateWeeklyLoginData(user);
                        UpdateDailyOnlineDurationData(user);
                        UpdateSignInData(user);
                        UpdateDailyMissions(user);
                        UpdateGroupMemberStatus(user);
                        UpdateExpedition(user);
                        UpdateGowPrize(user);
                    }
                }
            }
            if (m_IsLastSave && userDSDoneCount >= m_ActiveUserGuids.Count)
            {
                if (m_LastSaveFinished == false)
                {
                    LogSys.Log(LOG_TYPE.MONITOR, "DataProcessScheduler DoLastSaveData Done! UserCount:{0}", userDSDoneCount);
                    m_LastSaveFinished = true;
                }
            }
            foreach (ulong guid in m_DeactiveUserGuids)
            {
                m_ActiveUserGuids.Remove(guid);
            }

            m_DeactiveUserGuids.Clear();
            foreach (ulong guid in m_WaitRecycleUsers)
            {
                UserInfo user = GetUserInfo(guid);
                if (user == null)
                {
                    m_DeactiveUserGuids.Add(guid);
                }
                else
                {
                    if (user.Room == null && (!ds_thread.DataStoreAvailable || user.CurrentUserSaveCount == 0))
                    {
                        string accountId = user.AccountId;
                        /// norm log
                        AccountInfo account_info = FindAccountInfoById(accountId);
                        int onlinetimes = (int)(TimeUtility.CurTimestamp - user.LastLoginTime);
                        LogSys.NormLog("logout", LobbyConfig.AppKeyStr, user.ClientGameVersion, Module.logout, LobbyConfig.LogNormVersionStr,
                          "9999", user.LogicServerId, null != account_info ? account_info.ChannelId : LobbyConfig.AndroidGameChannelStr, user.AccountId,
                          user.AccountId, user.Guid, user.Nickname, user.Level, user.ClientDeviceidId, user.Money, onlinetimes, user.Vip, user.CurStamina);
                        ///
                        FreeKey(user.Key);
                        ulong g = 0;
                        m_GuidByNickname.TryRemove(user.Nickname, out g);
                        UserInfo tmp;
                        m_UserInfos.TryRemove(guid, out tmp);
                        RecycleUserInfo(user);
                        m_DeactiveUserGuids.Add(guid);
                        //userinfo所属的AccountInfo下线
                        if (LobbyConfig.DataStoreAvailable)
                        {
                            AccountInfo ai = FindAccountInfoById(accountId);
                            if (ai != null)
                            {
                                bool isAccountKickable = true;
                                foreach (var role in ai.Users)
                                {
                                    if (role != null && role.Guid != guid)
                                    {
                                        UserInfo ui = GetUserInfo(role.Guid);
                                        if (ui != null)
                                        {
                                            isAccountKickable = false;
                                            break;
                                        }
                                    }
                                }
                                if (isAccountKickable && ai.CurrentState == AccountState.Dropped)
                                {
                                    ai.CurrentState = AccountState.Offline;
                                    AccountInfo tmpAi = null;
                                    m_AccountByKey.TryRemove(ai.AccountKey, out tmpAi);
                                    m_AccountById.TryRemove(ai.AccountId, out tmpAi);
                                    LogSys.Log(LOG_TYPE.INFO, "Account LOGOUT with UserInfo. AccountKey:{0}, AccountId:{1}, UserGuid:{2}", ai.AccountKey, ai.AccountId, guid);
                                }
                            }
                        }
                    }
                }
            }
            foreach (ulong guid in m_DeactiveUserGuids)
            {
                m_WaitRecycleUsers.Remove(guid);
            }
        }
        private void ActivateUserGuid(ulong guid)
        {
            if (!m_ActiveUserGuids.Contains(guid))
            {
                m_ActiveUserGuids.Add(guid);
            }
        }
        private void AddWaitRecycleUser(ulong guid)
        {
            if (!m_WaitRecycleUsers.Contains(guid))
            {
                m_WaitRecycleUsers.Add(guid);
            }
        }
        private uint GenerateKey()
        {
            uint key = 0;
            for (;;)
            {
                key = (uint)(m_Random.NextDouble() * 0x0fffffff);
                if (!m_Keys.ContainsKey(key))
                {
                    m_Keys.AddOrUpdate(key, true, (k, v) => true);
                    break;
                }
            }
            return key;
        }
        private void FreeKey(uint key)
        {
            bool nouse = false;
            m_Keys.TryRemove(key, out nouse);
        }
        private void PublishNotice(string content, int roll_num)
        {
            foreach (ulong guid in m_ActiveUserGuids)
            {
                UserInfo user = GetUserInfo(guid);
                if (user != null)
                {
                    JsonMessageWithGuid pnMsg = new JsonMessageWithGuid(JsonMessageID.SyncNoticeContent);
                    pnMsg.m_Guid = user.Guid;
                    ArkCrossEngineMessage.Msg_LC_SyncNoticeContent protoData = new ArkCrossEngineMessage.Msg_LC_SyncNoticeContent();
                    protoData.m_Content = content;
                    protoData.m_RollNum = roll_num;
                    pnMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendDcoreMessage(user.NodeName, pnMsg);
                }
            }
        }
        private ExpeditionImageInfo DeserializeImage(byte[] byteImage)
        {
            ExpeditionImageInfo image = new ExpeditionImageInfo();
            DSA_ExpeditionImage dataImage = DSA_ExpeditionImage.ParseFrom(byteImage);
            image.Guid = dataImage.Guid;
            image.HeroId = dataImage.HeroId;
            image.Nickname = dataImage.Nickname;
            image.Level = dataImage.Level;
            image.FightingScore = dataImage.FightingScore;
            for (int i = 0; i < dataImage.EquipListCount && i < image.Equips.Armor.Length; ++i)
            {
                ItemInfo equip = new ItemInfo();
                equip.ItemId = dataImage.EquipListList[i].ItemId;
                equip.Level = dataImage.EquipListList[i].Level;
                equip.ItemNum = dataImage.EquipListList[i].Number;
                equip.AppendProperty = dataImage.EquipListList[i].AppendProperty;
                image.Equips.Armor[i] = equip;
            }
            for (int i = 0; i < dataImage.LegacyListCount && i < image.Legacys.SevenArcs.Length; ++i)
            {
                ItemInfo legacy = new ItemInfo();
                legacy.ItemId = dataImage.LegacyListList[i].ItemId;
                legacy.Level = dataImage.LegacyListList[i].Level;
                legacy.IsUnlock = dataImage.LegacyListList[i].IsUnlock;
                legacy.AppendProperty = dataImage.LegacyListList[i].AppendProperty;
                image.Legacys.SevenArcs[i] = legacy;
            }
            foreach (var dataSkill in dataImage.SkillListList)
            {
                SkillDataInfo skill = new SkillDataInfo();
                skill.ID = dataSkill.ID;
                skill.Level = dataSkill.Level;
                skill.Postions.Presets[0] = (SlotPosition)dataSkill.Postions;
                image.Skills.AddSkillData(skill);
            }
            return image;
        }

        private ConcurrentDictionary<string, AccountInfo> m_AccountByKey = new ConcurrentDictionary<string, AccountInfo>();
        private ConcurrentDictionary<string, AccountInfo> m_AccountById = new ConcurrentDictionary<string, AccountInfo>();
        private ConcurrentDictionary<ulong, UserInfo> m_UserInfos = new ConcurrentDictionary<ulong, UserInfo>();
        private ConcurrentDictionary<string, ulong> m_GuidByNickname = new ConcurrentDictionary<string, ulong>();
        private ConcurrentQueue<UserInfo> m_UnusedUserInfos = new ConcurrentQueue<UserInfo>();
        private ConcurrentDictionary<uint, bool> m_Keys = new ConcurrentDictionary<uint, bool>();

        private HashSet<ulong> m_ActiveUserGuids = new HashSet<ulong>();
        private List<ulong> m_DeactiveUserGuids = new List<ulong>();
        private List<ulong> m_WaitRecycleUsers = new List<ulong>();

        private Random m_Random = new Random();
        private MyServerThread m_Thread = null;
        private static ulong s_UserGuidGenerator = 10000;

        private long m_LastLogTime = 0;
        private long m_LastNormLogTime = 0;
        private bool m_IsLastSave = false;          //是否在执行最后一次存储操作
        private bool m_LastSaveFinished = false;

        //全局数据系统，因为与注册登录流程密切相关，故在DataProcessScheduler中实现
        private NicknameSystem m_NicknameSystem = new NicknameSystem();
        private ActivationCodeSystem m_ActivationCodeSystem = new ActivationCodeSystem();
    }
}

