using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using DashFire;
using DashFire.DataStore;
using System.Threading;
using ArkCrossEngine;

internal class DataCacheSystem : ArkCrossEngine.MyServerThread
{
    internal delegate void LoadHandlerDelegate(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> lccb);
    internal delegate void SaveHandlerDelegate(uint msgId, string key, IMessage data);
    internal class DSMessageHandlerDispatcher
    {
        internal static DSMessageHandlerDispatcher Instance
        {
            get { return s_Instance; }
        }
        private static DSMessageHandlerDispatcher s_Instance = new DSMessageHandlerDispatcher();

        internal void RegisterLoadHandler(uint msgId, LoadHandlerDelegate handler)
        {
            if (m_LoadHandlers.ContainsKey(msgId))
            {
                m_LoadHandlers[msgId] = handler;
            }
            else
            {
                m_LoadHandlers.Add(msgId, handler);
            }
        }
        internal void DispatchLoadMessage(object state)
        {
            var tuple = (Tuple<uint, string, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage>>)state;
            uint msgId = tuple.Item1;
            string key = tuple.Item2;
            ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb = tuple.Item3;
            LoadHandlerDelegate handler = null;
            m_LoadHandlers.TryGetValue(msgId, out handler);
            if (handler != null)
            {
                handler(msgId, key, cb);
            }
        }

        internal void RegisterSaveHandler(uint msgId, SaveHandlerDelegate handler)
        {
            if (m_SaveHandlers.ContainsKey(msgId))
            {
                m_SaveHandlers[msgId] = handler;
            }
            else
            {
                m_SaveHandlers.Add(msgId, handler);
            }
        }
        internal void DispatchSaveMessage(uint msgId, string key, IMessage data)
        {
            SaveHandlerDelegate handler = null;
            m_SaveHandlers.TryGetValue(msgId, out handler);
            if (handler != null)
            {
                handler(msgId, key, data);
            }
        }

        private Dictionary<uint, LoadHandlerDelegate> m_LoadHandlers = new Dictionary<uint, LoadHandlerDelegate>();
        private Dictionary<uint, SaveHandlerDelegate> m_SaveHandlers = new Dictionary<uint, SaveHandlerDelegate>();
    }
    internal ArkCrossEngine.ServerAsyncActionProcessor LoadActionQueue
    {
        get { return m_LoadActionQueue; }
    }
    internal ArkCrossEngine.ServerAsyncActionProcessor SaveActionQueue
    {
        get { return m_SaveActionQueue; }
    }

    //==================================================================
    internal void Init()
    {
        m_InnerCache = new InnerCache();
        Assembly dsAssembly = Assembly.Load("DashFire.DataStore");
        List<Type> dsTypeList = new List<Type>();
        foreach (Type t in dsAssembly.GetTypes())
        {
            //命名规则："DS"开头为数据消息类型
            if (t.Name.StartsWith("DS"))
            {
                dsTypeList.Add(t);
            }
        }
        foreach (Type t in dsTypeList)
        {
            DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(t), HandleLoadDefault);
            DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(t), HandleSaveDefault);
        }
        //注册需要特殊处理的Load操作
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSP_Account)), HandleLoadDSPAccount);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSP_User)), HandleLoadDSPUser);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSP_UserArena)), HandleLoadDSPUserArena);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSG_Guid)), HandleLoadDSGGuid);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSG_ActivationCode)), HandleLoadDSGActivationCode);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSG_Nickname)), HandleLoadDSGNickname);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSG_GowStar)), HandleLoadDSGGowStar);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSG_Mail)), HandleLoadDSGMail);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSG_GiftCode)), HandleLoadDSGGiftCode);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSG_ArenaRank)), HandleLoadDSGArenaRank);
        DSMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(DSG_ArenaRecord)), HandleLoadDSGArenaRecord);
        //注册需要特殊处理的Save操作
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSP_CreateAccount)), HandleSaveDSPCreateAccount);
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSP_CreateUser)), HandleSaveDSPCreateUser);
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSP_User)), HandleSaveDSPUser);
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSP_UserArena)), HandleSaveDSPUserArena);
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSG_Guid)), HandleSaveDSGGuid);
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSG_GowStar)), HandleSaveDSGGowStar);
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSG_Mail)), HandleSaveDSGMail);
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSG_ArenaRank)), HandleSaveDSGArenaRank);
        DSMessageHandlerDispatcher.Instance.RegisterSaveHandler(MessageMapping.Query(typeof(DSG_ArenaRecord)), HandleSaveDSGArenaRecord);

        PersistentSystem.Instance.Init();
        Start();
        LogSys.Log(LOG_TYPE.INFO, "DataCacheSystem initialized");
    }
    //==========================通过QueueAction调用的方法===========================================
    //注意!回调函数目前在缓存线程与db线程都可能调用，回调函数的实现需要是线程安全的(目前一般都是发消息，满足此条件)。
    internal void Load(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DSMessageHandlerDispatcher.Instance.DispatchLoadMessage(Tuple.Create(msgId, key, cb));
    }
    internal void Save(uint msgId, string key, byte[] dataBytes)
    {
        Type t = MessageMapping.Query(msgId);
        if (null == t)
            throw new UnknownDataMessage(msgId);
        //解码
        IMessage data = t.InvokeMember(
                    "ParseFrom",
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.InvokeMethod,
                    null,
                    null,
                    new object[] { dataBytes }) as IMessage;
        DSMessageHandlerDispatcher.Instance.DispatchSaveMessage(msgId, key, data);
        LogSys.Log(LOG_TYPE.INFO, "Save data Success. msgType:({0}), key:({1})", data.GetType().Name, key);
    }
    //直接存入DB,不经过缓存
    internal void DirectSave(uint msgId, string key, byte[] dataBytes)
    {
        Type dataType = MessageMapping.Query(msgId);
        if (null == dataType)
        {
            throw new UnknownDataMessage(msgId);
        }
        //解码
        IMessage data = dataType.InvokeMember(
                    "ParseFrom",
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.InvokeMethod,
                    null,
                    null,
                    new object[] { dataBytes }) as IMessage;
        if (null == data)
        {
            throw new NullDataMessage(msgId);
        }
        string statement = string.Empty;
        if (dataType.Name.Equals("DSD_BanAccount"))
        {
            DSD_BanAccount dataBanAccount = data as DSD_BanAccount;
            int banValue = 0;
            if (dataBanAccount.IsBanned)
            {
                banValue = 1;
            }
            statement = string.Format("update Account set IsBanned = '{0}' where Account = '{1}';", banValue, dataBanAccount.Account);
        }
        int ret = DataSaveImplement.DirectSave(statement);
        //更新缓存
        if (ret > 0)
        {
            if (dataType.Name.Equals("DSD_BanAccount"))
            {
                DSD_BanAccount dataBanAccount = data as DSD_BanAccount;
                uint accountMsgId = MessageMapping.Query(typeof(DS_Account));
                DataValue accountDataValue = m_InnerCache.Find(accountMsgId, dataBanAccount.Account);
                if (accountDataValue != null)
                {
                    DS_Account oldData = accountDataValue.DataMessage as DS_Account;
                    DS_Account.Builder newDataBuilder = DS_Account.CreateBuilder();
                    newDataBuilder.SetAccount(oldData.Account);
                    newDataBuilder.SetIsValid(oldData.IsValid);
                    newDataBuilder.SetIsBanned(dataBanAccount.IsBanned);
                    newDataBuilder.SetUserGuid1(oldData.UserGuid1);
                    newDataBuilder.SetUserGuid2(oldData.UserGuid2);
                    newDataBuilder.SetUserGuid3(oldData.UserGuid3);
                    DS_Account newData = newDataBuilder.Build();
                    m_InnerCache.AddOrUpdate(accountMsgId, dataBanAccount.Account, null, newData);
                }
            }
        }

        LogSys.Log(LOG_TYPE.INFO, "Direct Save data Success. msgType:({0}), key:({1})", dataType.Name, key);
    }
    internal void DoLastSave()
    {
        PersistentSystem.Instance.LastSaveToDB();
    }
    //==========================只能在本线程调用的方法===========================================
    internal Dictionary<uint, List<IMessage>> FetchDirtyData()
    {
        return m_InnerCache.FetchDirtyData();
    }
    //=====================================================================
    protected override void OnStart()
    {
        TickSleepTime = 10;
        ActionNumPerTick = 1024;
    }
    protected override void OnTick()
    {
        try
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (m_LastLogTime + 60000 < curTime)
            {
                m_LastLogTime = curTime;
                DebugPoolCount((string msg) =>
                {
                    LogSys.Log(LOG_TYPE.INFO, "DataCacheSystem.ActionQueue {0}", msg);
                });
                m_LoadActionQueue.DebugPoolCount((string msg) =>
                {
                    LogSys.Log(LOG_TYPE.INFO, "DataCacheSystem.LoadActionQueue {0}", msg);
                });
                m_SaveActionQueue.DebugPoolCount((string msg) =>
                {
                    LogSys.Log(LOG_TYPE.INFO, "DataCacheSystem.SaveActionQueue {0}", msg);
                });
            }

            if (curTime - m_LastCacheTickTime > c_CacheTickInterval)
            {
                m_InnerCache.Tick();
                m_LastCacheTickTime = curTime;
            }

            m_LoadActionQueue.HandleActions(1024);
            m_SaveActionQueue.HandleActions(1024);
            PersistentSystem.Instance.Tick();
        }
        catch (Exception ex)
        {
            LogSys.Log(LOG_TYPE.ERROR, "DataCacheSystem ERROR:{0} \n StackTrace:{1}", ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
            {
                LogSys.Log(LOG_TYPE.ERROR, "DataCacheSystem INNER ERROR:{0} \n StackTrace:{1}", ex.InnerException.Message, ex.InnerException.StackTrace);
            }
        }
    }
    //==========================Load=========================================
    private void HandleLoadDefault(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        string error = null;
        IMessage data = null;
        Type dataType = MessageMapping.Query(msgId);
        try
        {
            DataValue dv = m_InnerCache.Find(msgId, key);
            if (null != dv)
            {
                data = dv.DataMessage;
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Cache: key:({0}), data({1})", key, data.GetType().Name);
                cb(DSLoadResult.Success, null, data);
            }
            else
            {
                DbThreadManager.Instance.LoadActionQueue.QueueAction((MyAction<Type, string, MyAction<IMessage>>)DataLoadImplement.LoadSingleRowWithCallback, dataType, key, (IMessage dataMsg) =>
                {
                    if (null == dataMsg)
                    {
                        error = string.Format("DataStore Load from Database MISS: key:({0}), data({1})", key, dataType.Name);
                        cb(DSLoadResult.NotFound, error, null);
                        LogSys.Log(LOG_TYPE.INFO, error);
                        return;
                    }
                    else
                    {
                        cb(DSLoadResult.Success, null, dataMsg);
                        LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
                    }
                });
            }
        }
        catch (Exception e)
        {
            error = e.Message;
            cb(DSLoadResult.PostError, error, data);
            LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
            return;
        }
    }
    private void HandleLoadDSPAccount(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        string error = null;
        IMessage data = null;
        Type dataType = MessageMapping.Query(msgId);
        try
        {
            uint accountMsgId = MessageMapping.Query(typeof(DS_Account));
            uint userMsgId = MessageMapping.Query(typeof(DS_UserInfo));
            DataValue accountDataValue = m_InnerCache.Find(accountMsgId, key);
            if (accountDataValue != null && accountDataValue.Valid)
            {
                //缓存命中
                DSP_Account.Builder dataAccountBuilder = DSP_Account.CreateBuilder();
                DS_Account dataAccountBasic = accountDataValue.DataMessage as DS_Account;
                dataAccountBuilder.SetAccount(dataAccountBasic.Account);
                dataAccountBuilder.SetAccountBasic(dataAccountBasic);
                List<long> userGuidList = new List<long>();
                if (dataAccountBasic.UserGuid1 > 0)
                {
                    long userGuid = dataAccountBasic.UserGuid1;
                    DataValue userDataValue = m_InnerCache.Find(userMsgId, userGuid.ToString());
                    if (userDataValue != null && userDataValue.Valid)
                    {
                        dataAccountBuilder.UserListList.Add(userDataValue.DataMessage as DS_UserInfo);
                    }
                    else
                    {
                        userGuidList.Add(userGuid);
                    }
                }
                if (dataAccountBasic.UserGuid2 > 0)
                {
                    long userGuid = dataAccountBasic.UserGuid2;
                    DataValue userDataValue = m_InnerCache.Find(userMsgId, userGuid.ToString());
                    if (userDataValue != null && userDataValue.Valid)
                    {
                        dataAccountBuilder.UserListList.Add(userDataValue.DataMessage as DS_UserInfo);
                    }
                    else
                    {
                        userGuidList.Add(userGuid);
                    }
                }
                if (dataAccountBasic.UserGuid3 > 0)
                {
                    long userGuid = dataAccountBasic.UserGuid3;
                    DataValue userDataValue = m_InnerCache.Find(userMsgId, userGuid.ToString());
                    if (userDataValue != null && userDataValue.Valid)
                    {
                        dataAccountBuilder.UserListList.Add(userDataValue.DataMessage as DS_UserInfo);
                    }
                    else
                    {
                        userGuidList.Add(userGuid);
                    }
                }
                DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
                {
                    foreach (long userGuid in userGuidList)
                    {
                        if (userGuid > 0)
                        {
                            DS_UserInfo dataUser = DataLoadImplement.LoadSingleRow(typeof(DS_UserInfo), userGuid.ToString()) as DS_UserInfo;
                            if (null != dataUser)
                            {
                                dataAccountBuilder.UserListList.Add(dataUser);
                            }
                        }
                    }
                    data = dataAccountBuilder.Build();
                    cb(DSLoadResult.Success, null, data);
                    LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Cache: key:({0}), data({1})", key, dataType.Name);
                });
            }
            else
            {
                //缓存未命中
                DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
                {
                    DSP_Account.Builder dataAccountBuilder = DSP_Account.CreateBuilder();
                    DS_Account dataAccountBasic = DataLoadImplement.LoadSingleRow(typeof(DS_Account), key) as DS_Account;
                    if (dataAccountBasic != null)
                    {
                        dataAccountBuilder.SetAccount(dataAccountBasic.Account);
                        dataAccountBuilder.SetAccountBasic(dataAccountBasic);
                        DataCacheSystem.Instance.QueueAction(() =>
                        {
                            m_InnerCache.AddOrUpdate(accountMsgId, key, null, dataAccountBasic);
                        });
                    }
                    else
                    {
                        error = string.Format("DataStore Load from Database MISS: key:({0}), data({1})", key, dataType.Name);
                        cb(DSLoadResult.NotFound, error, null);
                        LogSys.Log(LOG_TYPE.INFO, error);
                        return;
                    }
                    List<DS_UserInfo> dataUserList = new List<DS_UserInfo>();
                    if (dataAccountBasic.UserGuid1 > 0)
                    {
                        DS_UserInfo dataUser = DataLoadImplement.LoadSingleRow(typeof(DS_UserInfo), dataAccountBasic.UserGuid1.ToString()) as DS_UserInfo;
                        if (dataUser != null)
                        {
                            dataUserList.Add(dataUser);
                        }
                    }
                    if (dataAccountBasic.UserGuid2 > 0)
                    {
                        DS_UserInfo dataUser = DataLoadImplement.LoadSingleRow(typeof(DS_UserInfo), dataAccountBasic.UserGuid2.ToString()) as DS_UserInfo;
                        if (dataUser != null)
                        {
                            dataUserList.Add(dataUser);
                        }
                    }
                    if (dataAccountBasic.UserGuid3 > 0)
                    {
                        DS_UserInfo dataUser = DataLoadImplement.LoadSingleRow(typeof(DS_UserInfo), dataAccountBasic.UserGuid3.ToString()) as DS_UserInfo;
                        if (dataUser != null)
                        {
                            dataUserList.Add(dataUser);
                        }
                    }
                    foreach (var dataUser in dataUserList)
                    {
                        dataAccountBuilder.UserListList.Add(dataUser as DS_UserInfo);
                        //DS_UserInfo不加到缓存中
                    }
                    data = dataAccountBuilder.Build();
                    cb(DSLoadResult.Success, null, data);
                    LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
                });
            }
        }
        catch (Exception e)
        {
            error = e.Message;
            cb(DSLoadResult.PostError, error, data);
            LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
            return;
        }
    }
    private void HandleLoadDSPUser(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        string error = null;
        IMessage data = null;
        Type dataType = MessageMapping.Query(msgId);
        try
        {
            uint userTypeId = MessageMapping.Query(typeof(DS_UserInfo));
            uint userExtraTypeId = MessageMapping.Query(typeof(DS_UserInfoExtra));
            uint equipTypeId = MessageMapping.Query(typeof(DS_EquipInfo));
            uint itemTypeId = MessageMapping.Query(typeof(DS_ItemInfo));
            uint legacyTypeId = MessageMapping.Query(typeof(DS_LegacyInfo));
            uint xsoulTypeId = MessageMapping.Query(typeof(DS_XSoulInfo));
            uint skillTypeId = MessageMapping.Query(typeof(DS_SkillInfo));
            uint missionTypeId = MessageMapping.Query(typeof(DS_MissionInfo));
            uint levelTypeId = MessageMapping.Query(typeof(DS_LevelInfo));
            uint expeditionTypeId = MessageMapping.Query(typeof(DS_ExpeditionInfo));
            uint mailStateTypeId = MessageMapping.Query(typeof(DS_MailStateInfo));
            uint partnerTypeId = MessageMapping.Query(typeof(DS_PartnerInfo));
            uint friendTypeId = MessageMapping.Query(typeof(DS_FriendInfo));
            DSP_User.Builder dataUserBuilder = DSP_User.CreateBuilder();
            DataValue userDataValue = m_InnerCache.Find(userTypeId, key);
            if (userDataValue != null && userDataValue.Valid)
            {
                //缓存命中,从缓存中读取角色数据
                DS_UserInfo dataUserBasic = userDataValue.DataMessage as DS_UserInfo;
                if (dataUserBasic != null)
                {
                    dataUserBuilder.SetUserGuid(dataUserBasic.Guid);
                    dataUserBuilder.SetUserBasic(dataUserBasic);
                }
                else
                {
                    error = string.Format("DataStore Load from Cache Error: key:({0}), data({1})", key, dataType.Name);
                    cb(DSLoadResult.NotFound, error, null);
                    LogSys.Log(LOG_TYPE.INFO, error);
                    return;
                }
                DataValue userExtraDataValue = m_InnerCache.Find(userExtraTypeId, key);
                if (userExtraDataValue.Valid)
                {
                    dataUserBuilder.SetUserExtra(userExtraDataValue.DataMessage as DS_UserInfoExtra);
                }
                List<DataValue> equipDataValues = m_InnerCache.FindByForeignKey(equipTypeId, key);
                foreach (var dv in equipDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.EquipListList.Add(dv.DataMessage as DS_EquipInfo);
                    }
                }
                List<DataValue> itemDataValues = m_InnerCache.FindByForeignKey(itemTypeId, key);
                foreach (var dv in itemDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.ItemListList.Add(dv.DataMessage as DS_ItemInfo);
                    }
                }
                List<DataValue> legacyDataValues = m_InnerCache.FindByForeignKey(legacyTypeId, key);
                foreach (var dv in legacyDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.LegacyListList.Add(dv.DataMessage as DS_LegacyInfo);
                    }
                }
                List<DataValue> xsoulDataValues = m_InnerCache.FindByForeignKey(xsoulTypeId, key);
                foreach (var dv in xsoulDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.XSoulListList.Add(dv.DataMessage as DS_XSoulInfo);
                    }
                }
                List<DataValue> skillDataValues = m_InnerCache.FindByForeignKey(skillTypeId, key);
                foreach (var dv in skillDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.SkillListList.Add(dv.DataMessage as DS_SkillInfo);
                    }
                }
                List<DataValue> missionDataValues = m_InnerCache.FindByForeignKey(missionTypeId, key);
                foreach (var dv in missionDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.MissionListList.Add(dv.DataMessage as DS_MissionInfo);
                    }
                }
                List<DataValue> levelDataValues = m_InnerCache.FindByForeignKey(levelTypeId, key);
                foreach (var dv in levelDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.LevelListList.Add(dv.DataMessage as DS_LevelInfo);
                    }
                }
                List<DataValue> expeditonDataValues = m_InnerCache.FindByForeignKey(expeditionTypeId, key);
                foreach (var dv in expeditonDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.SetUserExpedition(dv.DataMessage as DS_ExpeditionInfo);
                    }
                }
                List<DataValue> mailStateDataValues = m_InnerCache.FindByForeignKey(mailStateTypeId, key);
                foreach (var dv in mailStateDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.MailStateListList.Add(dv.DataMessage as DS_MailStateInfo);
                    }
                }
                List<DataValue> partnerDataValues = m_InnerCache.FindByForeignKey(partnerTypeId, key);
                foreach (var dv in partnerDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.PartnerListList.Add(dv.DataMessage as DS_PartnerInfo);
                    }
                }
                List<DataValue> friendDataValues = m_InnerCache.FindByForeignKey(friendTypeId, key);
                foreach (var dv in friendDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserBuilder.FriendListList.Add(dv.DataMessage as DS_FriendInfo);
                    }
                }
                data = dataUserBuilder.Build();
                cb(DSLoadResult.Success, null, data);
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Cache: key:({0}), data({1})", key, dataType.Name);
            }
            else
            {
                //缓存未命中，从数据库读取,读取之后写入缓存
                DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
                {
                    DS_UserInfo dataUserBasic = DataLoadImplement.LoadSingleRow(typeof(DS_UserInfo), key) as DS_UserInfo;
                    if (dataUserBasic != null)
                    {
                        dataUserBuilder.SetUserGuid(dataUserBasic.Guid);
                        dataUserBuilder.SetUserBasic(dataUserBasic);
                        DataCacheSystem.Instance.QueueAction(() => { m_InnerCache.AddOrUpdate(userTypeId, key, dataUserBasic.AccountId, dataUserBasic); });
                    }
                    else
                    {
                        error = string.Format("DataStore Load from Database MISS: key:({0}), data({1})", key, dataType.Name);
                        cb(DSLoadResult.NotFound, error, null);
                        LogSys.Log(LOG_TYPE.INFO, error);
                        return;
                    }
                    DS_UserInfoExtra dataUserExtra = DataLoadImplement.LoadSingleRow(typeof(DS_UserInfoExtra), key) as DS_UserInfoExtra;
                    dataUserBuilder.SetUserExtra(dataUserExtra);
                    DataCacheSystem.Instance.QueueAction(() => { m_InnerCache.AddOrUpdate(userExtraTypeId, key, null, dataUserExtra); });
                    //
                    List<IMessage> dataEquips = DataLoadImplement.LoadMultiRows(typeof(DS_EquipInfo), key);
                    List<IMessage> dataItems = DataLoadImplement.LoadMultiRows(typeof(DS_ItemInfo), key);
                    List<IMessage> dataLegacies = DataLoadImplement.LoadMultiRows(typeof(DS_LegacyInfo), key);
                    List<IMessage> dataXSouls = DataLoadImplement.LoadMultiRows(typeof(DS_XSoulInfo), key);
                    List<IMessage> dataSkills = DataLoadImplement.LoadMultiRows(typeof(DS_SkillInfo), key);
                    List<IMessage> dataMissions = DataLoadImplement.LoadMultiRows(typeof(DS_MissionInfo), key);
                    List<IMessage> dataLevels = DataLoadImplement.LoadMultiRows(typeof(DS_LevelInfo), key);
                    List<IMessage> dataExpeditions = DataLoadImplement.LoadMultiRows(typeof(DS_ExpeditionInfo), key);
                    List<IMessage> dataMailStates = DataLoadImplement.LoadMultiRows(typeof(DS_MailStateInfo), key);
                    List<IMessage> dataPartners = DataLoadImplement.LoadMultiRows(typeof(DS_PartnerInfo), key);
                    List<IMessage> dataFriends = DataLoadImplement.LoadMultiRows(typeof(DS_FriendInfo), key);
                    foreach (var dataEquip in dataEquips)
                    {
                        dataUserBuilder.EquipListList.Add(dataEquip as DS_EquipInfo);
                    }
                    foreach (var dataItem in dataItems)
                    {
                        dataUserBuilder.ItemListList.Add(dataItem as DS_ItemInfo);
                    }
                    foreach (var dataLegacy in dataLegacies)
                    {
                        dataUserBuilder.LegacyListList.Add(dataLegacy as DS_LegacyInfo);
                    }
                    foreach (var dataXSoul in dataXSouls)
                    {
                        dataUserBuilder.XSoulListList.Add(dataXSoul as DS_XSoulInfo);
                    }
                    foreach (var dataSkill in dataSkills)
                    {
                        dataUserBuilder.SkillListList.Add(dataSkill as DS_SkillInfo);
                    }
                    foreach (var dataMission in dataMissions)
                    {
                        dataUserBuilder.MissionListList.Add(dataMission as DS_MissionInfo);
                    }
                    foreach (var dataLevel in dataLevels)
                    {
                        dataUserBuilder.LevelListList.Add(dataLevel as DS_LevelInfo);
                    }
                    foreach (var dataExpedition in dataExpeditions)
                    {
                        dataUserBuilder.SetUserExpedition(dataExpedition as DS_ExpeditionInfo);
                    }
                    foreach (var dataMailState in dataMailStates)
                    {
                        dataUserBuilder.MailStateListList.Add(dataMailState as DS_MailStateInfo);
                    }
                    foreach (var dataPartner in dataPartners)
                    {
                        dataUserBuilder.PartnerListList.Add(dataPartner as DS_PartnerInfo);
                    }
                    foreach (var dataFriend in dataFriends)
                    {
                        dataUserBuilder.FriendListList.Add(dataFriend as DS_FriendInfo);
                    }
                    data = dataUserBuilder.Build();
                    cb(DSLoadResult.Success, null, data);
                    LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
                    //
                    DataCacheSystem.Instance.QueueAction(() =>
                    {
                        foreach (var dataEquip in dataEquips)
                        {
                            DS_EquipInfo de = dataEquip as DS_EquipInfo;
                            m_InnerCache.AddOrUpdate(equipTypeId, de.Guid, key, de);
                        }
                        foreach (var dataItem in dataItems)
                        {
                            DS_ItemInfo di = dataItem as DS_ItemInfo;
                            m_InnerCache.AddOrUpdate(itemTypeId, di.Guid, key, di);
                        }
                        foreach (var dataLegacy in dataLegacies)
                        {
                            DS_LegacyInfo dl = dataLegacy as DS_LegacyInfo;
                            m_InnerCache.AddOrUpdate(legacyTypeId, dl.Guid, key, dl);
                        }
                        foreach (var dataXSoul in dataXSouls)
                        {
                            DS_XSoulInfo dx = dataXSoul as DS_XSoulInfo;
                            m_InnerCache.AddOrUpdate(xsoulTypeId, dx.Guid, key, dx);
                        }
                        foreach (var dataSkill in dataSkills)
                        {
                            DS_SkillInfo ds = dataSkill as DS_SkillInfo;
                            m_InnerCache.AddOrUpdate(skillTypeId, ds.Guid, key, ds);
                        }
                        foreach (var dataMission in dataMissions)
                        {
                            DS_MissionInfo mi = dataMission as DS_MissionInfo;
                            m_InnerCache.AddOrUpdate(missionTypeId, mi.Guid, key, mi);
                        }
                        foreach (var dataLevel in dataLevels)
                        {
                            DS_LevelInfo dl = dataLevel as DS_LevelInfo;
                            m_InnerCache.AddOrUpdate(levelTypeId, dl.Guid, key, dl);
                        }
                        foreach (var dataExpedition in dataExpeditions)
                        {
                            DS_ExpeditionInfo de = dataExpedition as DS_ExpeditionInfo;
                            m_InnerCache.AddOrUpdate(expeditionTypeId, de.Guid, key, de);
                        }
                        foreach (var dataMailState in dataMailStates)
                        {
                            DS_MailStateInfo dm = dataMailState as DS_MailStateInfo;
                            m_InnerCache.AddOrUpdate(mailStateTypeId, dm.Guid, key, dm);
                        }
                        foreach (var dataPartner in dataPartners)
                        {
                            DS_PartnerInfo dp = dataPartner as DS_PartnerInfo;
                            m_InnerCache.AddOrUpdate(partnerTypeId, dp.Guid, key, dp);
                        }
                        foreach (var dataFriend in dataFriends)
                        {
                            DS_FriendInfo df = dataFriend as DS_FriendInfo;
                            m_InnerCache.AddOrUpdate(friendTypeId, df.Guid, key, df);
                        }
                    });
                });
            }
        }
        catch (Exception e)
        {
            error = e.Message;
            cb(DSLoadResult.PostError, error, data);
            LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:{0}, data:{1}, error:{2},stacktrace:{3}",
                                          key, dataType.Name, error, e.StackTrace);
            return;
        }
    }
    private void HandleLoadDSPUserArena(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        string error = null;
        IMessage data = null;
        Type dataType = MessageMapping.Query(msgId);
        try
        {
            uint arenaInfoMsgId = MessageMapping.Query(typeof(DS_ArenaInfo));
            uint arenaRecordMsgId = MessageMapping.Query(typeof(DS_ArenaRecord));
            DataValue arenaInfoDataValue = m_InnerCache.Find(arenaInfoMsgId, key);
            if (arenaInfoDataValue != null && arenaInfoDataValue.Valid)
            {
                //缓存命中
                DSP_UserArena.Builder dataUserArenaBuilder = DSP_UserArena.CreateBuilder();
                DS_ArenaInfo dataArenaBasic = arenaInfoDataValue.DataMessage as DS_ArenaInfo;
                dataUserArenaBuilder.SetUserGuid(dataArenaBasic.UserGuid);
                dataUserArenaBuilder.SetArenaBasic(dataArenaBasic);
                List<DataValue> recordDataValues = m_InnerCache.FindByForeignKey(arenaRecordMsgId, key);
                foreach (var dv in recordDataValues)
                {
                    if (dv.Valid)
                    {
                        dataUserArenaBuilder.ArenaRecordListList.Add(dv.DataMessage as DS_ArenaRecord);
                    }
                }
                data = dataUserArenaBuilder.Build();
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Cache: key:({0}), data({1})", key, dataType.Name);
                cb(DSLoadResult.Success, null, data);
            }
            else
            {
                DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
                {
                    DSP_UserArena.Builder dataUserArenaBuilder = DSP_UserArena.CreateBuilder();
                    DS_ArenaInfo dataArenaBasic = DataLoadImplement.LoadSingleRow(typeof(DS_ArenaInfo), key) as DS_ArenaInfo;
                    if (dataArenaBasic != null)
                    {
                        dataUserArenaBuilder.SetUserGuid(dataArenaBasic.UserGuid);
                        dataUserArenaBuilder.SetArenaBasic(dataArenaBasic);
                        DataCacheSystem.Instance.QueueAction(() =>
                {
                    m_InnerCache.AddOrUpdate(arenaInfoMsgId, key, null, dataArenaBasic);
                });
                    }
                    else
                    {
                        error = string.Format("DataStore Load from Database MISS: key:({0}), data({1})", key, dataType.Name);
                        cb(DSLoadResult.NotFound, error, null);
                        LogSys.Log(LOG_TYPE.INFO, error);
                        return;
                    }
                    List<IMessage> dataRecords = DataLoadImplement.LoadMultiRows(typeof(DS_ArenaRecord), key);
                    foreach (var dataRecord in dataRecords)
                    {
                        dataUserArenaBuilder.ArenaRecordListList.Add(dataRecord as DS_ArenaRecord);
                    }
                    DataCacheSystem.Instance.QueueAction(() =>
            {
                foreach (var dataRecord in dataRecords)
                {
                    DS_ArenaRecord dr = dataRecord as DS_ArenaRecord;
                    m_InnerCache.AddOrUpdate(arenaRecordMsgId, dr.Guid, key, dr);
                }
            });
                    data = dataUserArenaBuilder.Build();
                    cb(DSLoadResult.Success, null, data);
                    LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
                });
            }
        }
        catch (Exception e)
        {
            error = e.Message;
            cb(DSLoadResult.PostError, error, data);
            LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
            return;
        }
    }
    private void HandleLoadDSGGuid(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
        {
            string error = null;
            IMessage data = null;
            Type dataType = MessageMapping.Query(msgId);
            try
            {
                DSG_Guid.Builder dsgGuidBuilder = DSG_Guid.CreateBuilder();
                dsgGuidBuilder.SetDataMsgId((int)msgId);
                List<IMessage> dataGuidList = DataLoadImplement.LoadTable(typeof(DS_Guid));
                foreach (var dataGuid in dataGuidList)
                {
                    dsgGuidBuilder.GuidListList.Add(dataGuid as DS_Guid);
                }
                data = dsgGuidBuilder.Build();
            }
            catch (Exception e)
            {
                error = e.Message;
                cb(DSLoadResult.PostError, error, data);
                LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
                return;
            }
            if (error == null)
            {
                cb(DSLoadResult.Success, error, data);
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
            }
        });
    }
    private void HandleLoadDSGActivationCode(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
        {
            int dataCapacity = 2000;
            string error = null;
            List<IMessage> dataList = new List<IMessage>();
            int dataCount = 1;
            Type dataType = MessageMapping.Query(msgId);
            try
            {
                List<IMessage> dataActivationCodes = DataLoadImplement.LoadTable(typeof(DS_ActivationCode));
                dataCount = (int)(dataActivationCodes.Count / dataCapacity) + 1;
                for (int i = 0; i < dataCount; ++i)
                {
                    DSG_ActivationCode.Builder dsgActivationCodeBuilder = DSG_ActivationCode.CreateBuilder();
                    dsgActivationCodeBuilder.SetDataMsgId((int)msgId);
                    for (int j = i * dataCapacity; j < (i + 1) * dataCapacity && j < dataActivationCodes.Count; ++j)
                    {
                        dsgActivationCodeBuilder.ActivationCodeListList.Add(dataActivationCodes[j] as DS_ActivationCode);
                    }
                    dataList.Add(dsgActivationCodeBuilder.Build());
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load ActivationCode: dataCount:({0})", dataCount);
            }
            catch (Exception e)
            {
                error = e.Message;
                cb(DSLoadResult.PostError, error, null);
                LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
                return;
            }
            if (error == null)
            {
                if (dataList.Count > 1)
                {
                    for (int i = 0; i < dataList.Count - 1; ++i)
                    {
                        cb(DSLoadResult.Undone, error, dataList[i]);
                        Thread.Sleep(200);  //略作延迟，保证消息的顺序性
                    }
                    cb(DSLoadResult.Success, error, dataList[dataList.Count - 1]);
                }
                else if (dataList.Count == 1)
                {
                    cb(DSLoadResult.Success, error, dataList[0]);
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
            }
        });
    }
    private void HandleLoadDSGNickname(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
        {
            int dataCapacity = 2000;
            string error = null;
            List<IMessage> dataList = new List<IMessage>();
            int dataCount = 1;
            Type dataType = MessageMapping.Query(msgId);
            try
            {
                List<IMessage> dataNicknames = DataLoadImplement.LoadTable(typeof(DS_Nickname));
                dataCount = (int)(dataNicknames.Count / dataCapacity) + 1;
                for (int i = 0; i < dataCount; ++i)
                {
                    DSG_Nickname.Builder dsgNicknameBuilder = DSG_Nickname.CreateBuilder();
                    dsgNicknameBuilder.SetDataMsgId((int)msgId);
                    for (int j = i * dataCapacity; j < (i + 1) * dataCapacity && j < dataNicknames.Count; ++j)
                    {
                        dsgNicknameBuilder.NicknameListList.Add(dataNicknames[j] as DS_Nickname);
                    }
                    dataList.Add(dsgNicknameBuilder.Build());
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load Nickname: dataCount:({0})", dataCount);
            }
            catch (Exception e)
            {
                error = e.Message;
                cb(DSLoadResult.PostError, error, null);
                LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
                return;
            }
            if (error == null)
            {
                if (dataList.Count > 1)
                {
                    for (int i = 0; i < dataList.Count - 1; ++i)
                    {
                        cb(DSLoadResult.Undone, error, dataList[i]);
                        Thread.Sleep(200);  //略作延迟，保证消息的顺序性
                    }
                    cb(DSLoadResult.Success, error, dataList[dataList.Count - 1]);
                }
                else if (dataList.Count == 1)
                {
                    cb(DSLoadResult.Success, error, dataList[0]);
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
            }
        });
    }
    private void HandleLoadDSGGowStar(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
        {
            string error = null;
            IMessage data = null;
            Type dataType = MessageMapping.Query(msgId);
            try
            {
                DSG_GowStar.Builder dsgGowstarBuilder = DSG_GowStar.CreateBuilder();
                dsgGowstarBuilder.SetDataMsgId((int)msgId);
                List<IMessage> datagowstars = DataLoadImplement.LoadTable(typeof(DS_GowStar));
                foreach (var dataGowstar in datagowstars)
                {
                    dsgGowstarBuilder.GowStarListList.Add(dataGowstar as DS_GowStar);
                }
                data = dsgGowstarBuilder.Build();
            }
            catch (Exception e)
            {
                error = e.Message;
                cb(DSLoadResult.PostError, error, data);
                LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
                return;
            }
            if (error == null)
            {
                cb(DSLoadResult.Success, error, data);
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
            }
        });
    }
    private void HandleLoadDSGMail(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
        {
            string error = null;
            IMessage data = null;
            Type dataType = MessageMapping.Query(msgId);
            try
            {
                DSG_Mail.Builder dsgWholeMailBuilder = DSG_Mail.CreateBuilder();
                dsgWholeMailBuilder.SetDataMsgId((int)msgId);
                List<IMessage> dataMails = DataLoadImplement.LoadTable(typeof(DS_MailInfo));
                foreach (var dataMail in dataMails)
                {
                    dsgWholeMailBuilder.MailListList.Add(dataMail as DS_MailInfo);
                }
                data = dsgWholeMailBuilder.Build();
            }
            catch (Exception e)
            {
                error = e.Message;
                cb(DSLoadResult.PostError, error, data);
                LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
                return;
            }
            if (error == null)
            {
                cb(DSLoadResult.Success, error, data);
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
            }
        });
    }
    private void HandleLoadDSGGiftCode(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
        {
            int dataCapacity = 2000;
            string error = null;
            List<IMessage> dataList = new List<IMessage>();
            int dataCount = 1;
            Type dataType = MessageMapping.Query(msgId);
            try
            {
                List<IMessage> dataGiftCodes = DataLoadImplement.LoadTable(typeof(DS_GiftCode));
                dataCount = (int)(dataGiftCodes.Count / dataCapacity) + 1;
                for (int i = 0; i < dataCount; ++i)
                {
                    DSG_GiftCode.Builder dsgGiftCodeBuilder = DSG_GiftCode.CreateBuilder();
                    dsgGiftCodeBuilder.SetDataMsgId((int)msgId);
                    for (int j = i * dataCapacity; j < (i + 1) * dataCapacity && j < dataGiftCodes.Count; ++j)
                    {
                        dsgGiftCodeBuilder.GiftCodeListList.Add(dataGiftCodes[j] as DS_GiftCode);
                    }
                    dataList.Add(dsgGiftCodeBuilder.Build());
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load GiftCode: dataCount:({0})", dataCount);
            }
            catch (Exception e)
            {
                error = e.Message;
                cb(DSLoadResult.PostError, error, null);
                LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
                return;
            }
            if (error == null)
            {
                if (dataList.Count > 1)
                {
                    for (int i = 0; i < dataList.Count - 1; ++i)
                    {
                        cb(DSLoadResult.Undone, error, dataList[i]);
                        Thread.Sleep(200);  //略作延迟，保证消息的顺序性
                    }
                    cb(DSLoadResult.Success, error, dataList[dataList.Count - 1]);
                }
                else if (dataList.Count == 1)
                {
                    cb(DSLoadResult.Success, error, dataList[0]);
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
            }
        });
    }
    private void HandleLoadDSGArenaRank(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
        {
            int msgCapacity = 200;
            string error = null;
            List<IMessage> msgList = new List<IMessage>();
            int msgCount = 1;
            Type dataType = MessageMapping.Query(msgId);
            try
            {
                List<IMessage> dataArenaList = DataLoadImplement.LoadTable(typeof(DS_ArenaInfo));
                List<DS_ArenaInfo> rankArenaList = new List<DS_ArenaInfo>();
                foreach (var dataArena in dataArenaList)
                {
                    DS_ArenaInfo dataArenaInfo = dataArena as DS_ArenaInfo;
                    if (dataArenaInfo.Rank > 0)
                    {
                        //只加载在排行榜的数据
                        rankArenaList.Add(dataArenaInfo);
                    }
                }
                msgCount = (int)(rankArenaList.Count / msgCapacity) + 1;
                for (int i = 0; i < msgCount; ++i)
                {
                    DSG_ArenaRank.Builder dsgArenaRankBuilder = DSG_ArenaRank.CreateBuilder();
                    dsgArenaRankBuilder.SetDataMsgId((int)msgId);
                    for (int j = i * msgCapacity; j < (i + 1) * msgCapacity && j < rankArenaList.Count; ++j)
                    {
                        dsgArenaRankBuilder.ArenaListList.Add(rankArenaList[j]);
                    }
                    msgList.Add(dsgArenaRankBuilder.Build());
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load ArenaRank: dataCount:({0})", msgCount);
            }
            catch (Exception e)
            {
                error = e.Message;
                cb(DSLoadResult.PostError, error, null);
                LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
                return;
            }
            if (error == null)
            {
                if (msgList.Count > 1)
                {
                    for (int i = 0; i < msgList.Count - 1; ++i)
                    {
                        cb(DSLoadResult.Undone, error, msgList[i]);
                        Thread.Sleep(200);  //略作延迟，保证消息的顺序性
                    }
                    cb(DSLoadResult.Success, error, msgList[msgList.Count - 1]);
                }
                else if (msgList.Count == 1)
                {
                    cb(DSLoadResult.Success, error, msgList[0]);
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
            }
        });
    }
    private void HandleLoadDSGArenaRecord(uint msgId, string key, ArkCrossEngine.MyAction<DSLoadResult, string, IMessage> cb)
    {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() =>
        {
            int msgCapacity = 350;
            string error = null;
            List<IMessage> msgList = new List<IMessage>();
            int msgCount = 1;
            Type dataType = MessageMapping.Query(msgId);
            try
            {
                List<IMessage> dataArenaRecordList = DataLoadImplement.LoadTable(typeof(DS_ArenaRecord));
                List<DS_ArenaRecord> rankRecordList = new List<DS_ArenaRecord>();
                foreach (var dataRecord in dataArenaRecordList)
                {
                    DS_ArenaRecord dataArenaRecord = dataRecord as DS_ArenaRecord;
                    if (dataArenaRecord.Rank > 0)
                    {
                        //只加载在排行榜的玩家的数据
                        rankRecordList.Add(dataArenaRecord);
                    }
                }
                msgCount = (int)(rankRecordList.Count / msgCapacity) + 1;
                for (int i = 0; i < msgCount; ++i)
                {
                    DSG_ArenaRecord.Builder dsgArenaRecordBuilder = DSG_ArenaRecord.CreateBuilder();
                    dsgArenaRecordBuilder.SetDataMsgId((int)msgId);
                    for (int j = i * msgCapacity; j < (i + 1) * msgCapacity && j < rankRecordList.Count; ++j)
                    {
                        dsgArenaRecordBuilder.RecordListList.Add(rankRecordList[j]);
                    }
                    msgList.Add(dsgArenaRecordBuilder.Build());
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load ArenaRecord: dataCount:({0})", msgCount);
            }
            catch (Exception e)
            {
                error = e.Message;
                cb(DSLoadResult.PostError, error, null);
                LogSys.Log(LOG_TYPE.ERROR, "DataStore Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
                return;
            }
            if (error == null)
            {
                if (msgList.Count > 1)
                {
                    for (int i = 0; i < msgList.Count - 1; ++i)
                    {
                        cb(DSLoadResult.Undone, error, msgList[i]);
                        Thread.Sleep(200);  //略作延迟，保证消息的顺序性
                    }
                    cb(DSLoadResult.Success, error, msgList[msgList.Count - 1]);
                }
                else if (msgList.Count == 1)
                {
                    cb(DSLoadResult.Success, error, msgList[0]);
                }
                LogSys.Log(LOG_TYPE.DEBUG, "DataStore Load from Database: key:({0}), data({1})", key, dataType.Name);
            }
        });
    }
    //==========================Save===========================================
    private void HandleSaveDefault(uint msgId, string key, IMessage data)
    {
        //直接更新到缓存
        m_InnerCache.AddOrUpdate(msgId, key, null, data);
    }
    internal void HandleSaveDSPCreateAccount(uint msgId, string key, IMessage data)
    {
        DSP_CreateAccount dsgNewAccount = data as DSP_CreateAccount;
        uint accountMsgId = MessageMapping.Query(typeof(DS_Account));
        m_InnerCache.AddOrUpdate(accountMsgId, dsgNewAccount.AccountBasic.Account, null, dsgNewAccount.AccountBasic);
        uint activationCodeMsgId = MessageMapping.Query(typeof(DS_ActivationCode));
        m_InnerCache.AddOrUpdate(activationCodeMsgId, dsgNewAccount.UsedActivationCode.ActivationCode, null, dsgNewAccount.UsedActivationCode);
    }
    internal void HandleSaveDSPCreateUser(uint msgId, string key, IMessage data)
    {
        DSP_CreateUser dsgNewUser = data as DSP_CreateUser;
        uint accountMsgId = MessageMapping.Query(typeof(DS_Account));
        m_InnerCache.AddOrUpdate(accountMsgId, dsgNewUser.AccountBasic.Account, null, dsgNewUser.AccountBasic);
        uint nicknameMsgId = MessageMapping.Query(typeof(DS_Nickname));
        m_InnerCache.AddOrUpdate(nicknameMsgId, dsgNewUser.UsedNickname.Nickname, null, dsgNewUser.UsedNickname);
    }
    private void HandleSaveDSPUser(uint msgId, string key, IMessage data)
    {
        DSP_User userData = data as DSP_User;
        //
        m_InnerCache.AddOrUpdate(MessageMapping.Query(typeof(DS_UserInfo)), key, null, userData.UserBasic);
        m_InnerCache.AddOrUpdate(MessageMapping.Query(typeof(DS_UserInfoExtra)), key, null, userData.UserExtra);
        string foreignKey = key;    //DS_ItemInfo等的外键是DSP_User的主键
                                    //装备
        uint equipMsgId = MessageMapping.Query(typeof(DS_EquipInfo));
        foreach (var dataEquip in userData.EquipListList)
        {
            m_InnerCache.AddOrUpdate(equipMsgId, dataEquip.Guid, foreignKey, dataEquip);
        }
        //物品
        uint itemMsgId = MessageMapping.Query(typeof(DS_ItemInfo));
        List<DataValue> oldItemValueList = m_InnerCache.FindByForeignKey(itemMsgId, foreignKey);
        List<DS_ItemInfo> newItemList = new List<DS_ItemInfo>(userData.ItemListList);
        for (int i = 0; i < oldItemValueList.Count; ++i)
        {
            DataValue oldItemValue = oldItemValueList[i];
            DS_ItemInfo oldItem = oldItemValue.DataMessage as DS_ItemInfo;
            bool flag = newItemList.Exists(delegate (DS_ItemInfo item)
            {
                return item.Guid.Equals(oldItem.Guid);
            });
            if (flag == false)
            {
                //不存在说明已经被删除
                DS_ItemInfo.Builder itemBuilder = DS_ItemInfo.CreateBuilder();
                itemBuilder.SetGuid(oldItem.Guid);
                itemBuilder.SetUserGuid(oldItem.UserGuid);
                itemBuilder.SetIsValid(false);
                itemBuilder.SetPosition(oldItem.Position);
                itemBuilder.SetItemId(oldItem.ItemId);
                itemBuilder.SetItemNum(oldItem.ItemNum);
                itemBuilder.SetLevel(oldItem.Level);
                itemBuilder.SetAppendProperty(oldItem.AppendProperty);
                oldItemValue.Dirty = true;
                oldItemValue.Valid = false;
                oldItemValue.DataMessage = itemBuilder.Build();
            }
        }
        foreach (var dataItem in newItemList)
        {
            m_InnerCache.AddOrUpdate(itemMsgId, dataItem.Guid, foreignKey, dataItem);
        }
        //神器
        uint legacyMsgId = MessageMapping.Query(typeof(DS_LegacyInfo));
        foreach (var dataLegacy in userData.LegacyListList)
        {
            m_InnerCache.AddOrUpdate(legacyMsgId, dataLegacy.Guid, foreignKey, dataLegacy);
        }
        //X魂
        uint xsoulMsgId = MessageMapping.Query(typeof(DS_XSoulInfo));
        foreach (var dataXSoul in userData.XSoulListList)
        {
            m_InnerCache.AddOrUpdate(xsoulMsgId, dataXSoul.Guid, foreignKey, dataXSoul);
        }
        //技能
        uint skillMsgId = MessageMapping.Query(typeof(DS_SkillInfo));
        foreach (var dataSkill in userData.SkillListList)
        {
            m_InnerCache.AddOrUpdate(skillMsgId, dataSkill.Guid, foreignKey, dataSkill);
        }
        //任务
        uint missionMsgId = MessageMapping.Query(typeof(DS_MissionInfo));
        List<DataValue> oldMissionValueList = m_InnerCache.FindByForeignKey(missionMsgId, foreignKey);
        List<DS_MissionInfo> newMissionList = new List<DS_MissionInfo>(userData.MissionListList);
        for (int i = 0; i < oldMissionValueList.Count; ++i)
        {
            DataValue oldMissionValue = oldMissionValueList[i];
            DS_MissionInfo oldMission = oldMissionValue.DataMessage as DS_MissionInfo;
            bool flag = newMissionList.Exists(delegate (DS_MissionInfo item)
            {
                return item.Guid.Equals(oldMission.Guid);
            });
            if (flag == false)
            {
                //不存在说明已经被删除
                DS_MissionInfo.Builder missionBuilder = DS_MissionInfo.CreateBuilder();
                missionBuilder.SetGuid(oldMission.Guid);
                missionBuilder.SetUserGuid(oldMission.UserGuid);
                missionBuilder.SetIsValid(false);
                missionBuilder.SetMissionId(oldMission.MissionId);
                missionBuilder.SetMissionValue(oldMission.MissionValue);
                missionBuilder.SetMissionState(oldMission.MissionState);
                oldMissionValue.Dirty = true;
                oldMissionValue.Valid = false;
                oldMissionValue.DataMessage = missionBuilder.Build();
            }
        }
        foreach (var dataMission in newMissionList)
        {
            m_InnerCache.AddOrUpdate(missionMsgId, dataMission.Guid, foreignKey, dataMission);
        }
        //关卡
        uint levelMsgId = MessageMapping.Query(typeof(DS_LevelInfo));
        foreach (var dataLevel in userData.LevelListList)
        {
            m_InnerCache.AddOrUpdate(levelMsgId, dataLevel.Guid, foreignKey, dataLevel);
        }
        //远征
        uint expeditionMsgId = MessageMapping.Query(typeof(DS_ExpeditionInfo));
        m_InnerCache.AddOrUpdate(expeditionMsgId, userData.UserExpedition.Guid, foreignKey, userData.UserExpedition);
        //邮件状态数据
        uint mailStateMsgId = MessageMapping.Query(typeof(DS_MailStateInfo));
        List<DataValue> oldMailStateValueList = m_InnerCache.FindByForeignKey(mailStateMsgId, foreignKey);
        List<DS_MailStateInfo> newMailStateList = new List<DS_MailStateInfo>(userData.MailStateListList);
        for (int i = 0; i < oldMailStateValueList.Count; ++i)
        {
            DataValue oldMailStateValue = oldMailStateValueList[i];
            DS_MailStateInfo oldMailState = oldMailStateValue.DataMessage as DS_MailStateInfo;
            bool flag = newMailStateList.Exists(delegate (DS_MailStateInfo item)
            {
                return item.Guid.Equals(oldMailState.Guid);
            });
            if (flag == false)
            {
                //不存在说明已经被删除
                DS_MailStateInfo.Builder mailStateBuilder = DS_MailStateInfo.CreateBuilder();
                mailStateBuilder.SetGuid(oldMailState.Guid);
                mailStateBuilder.SetUserGuid(oldMailState.UserGuid);
                mailStateBuilder.SetIsValid(false);
                mailStateBuilder.SetMailGuid(oldMailState.MailGuid);
                mailStateBuilder.SetIsRead(oldMailState.IsRead);
                mailStateBuilder.SetIsReceived(oldMailState.IsReceived);
                mailStateBuilder.SetExpiryDate(oldMailState.ExpiryDate);
                oldMailStateValue.Dirty = true;
                oldMailStateValue.Valid = false;
                oldMailStateValue.DataMessage = mailStateBuilder.Build();
            }
        }
        foreach (var dataMailState in newMailStateList)
        {
            m_InnerCache.AddOrUpdate(mailStateMsgId, dataMailState.Guid, foreignKey, dataMailState);
        }
        //伙伴
        uint partnerMsgId = MessageMapping.Query(typeof(DS_PartnerInfo));
        foreach (var dataPartner in userData.PartnerListList)
        {
            m_InnerCache.AddOrUpdate(partnerMsgId, dataPartner.Guid, foreignKey, dataPartner);
        }
        //好友
        uint friendMsgId = MessageMapping.Query(typeof(DS_FriendInfo));
        List<DataValue> oldFriendValueList = m_InnerCache.FindByForeignKey(friendMsgId, foreignKey);
        List<DS_FriendInfo> newFriendList = new List<DS_FriendInfo>(userData.FriendListList);
        for (int i = 0; i < oldFriendValueList.Count; ++i)
        {
            DataValue oldFriendValue = oldFriendValueList[i];
            DS_FriendInfo oldFriend = oldFriendValue.DataMessage as DS_FriendInfo;
            bool flag = newFriendList.Exists(delegate (DS_FriendInfo item)
            {
                return item.Guid.Equals(oldFriend.Guid);
            });
            if (flag == false)
            {
                //不存在说明已经被删除
                DS_FriendInfo.Builder friendBuilder = DS_FriendInfo.CreateBuilder();
                friendBuilder.SetGuid(oldFriend.Guid);
                friendBuilder.SetUserGuid(oldFriend.UserGuid);
                friendBuilder.SetIsValid(false);
                friendBuilder.SetFriendGuid(oldFriend.FriendGuid);
                friendBuilder.SetFriendNickname(oldFriend.FriendNickname);
                friendBuilder.SetHeroId(oldFriend.HeroId);
                friendBuilder.SetLevel(oldFriend.Level);
                friendBuilder.SetFightingScore(oldFriend.FightingScore);
                oldFriendValue.Dirty = true;
                oldFriendValue.Valid = false;
                oldFriendValue.DataMessage = friendBuilder.Build();
            }
        }
        foreach (var dataFriend in newFriendList)
        {
            m_InnerCache.AddOrUpdate(friendMsgId, dataFriend.Guid, foreignKey, dataFriend);
        }
    }
    private void HandleSaveDSPUserArena(uint msgId, string key, IMessage data)
    {
        DSP_UserArena dataUserArena = data as DSP_UserArena;
        //
        m_InnerCache.AddOrUpdate(MessageMapping.Query(typeof(DS_ArenaInfo)), key, null, dataUserArena.ArenaBasic);
        string foreignKey = key;    //DS_ArenaRecord等的外键是DSP_UserArena的主键
                                    //
        uint arenaRecordMsgId = MessageMapping.Query(typeof(DS_ArenaRecord));
        foreach (var dataRecord in dataUserArena.ArenaRecordListList)
        {
            m_InnerCache.AddOrUpdate(arenaRecordMsgId, dataRecord.Guid, foreignKey, dataRecord);
        }
    }
    internal void HandleSaveDSGGuid(uint msgId, string key, IMessage data)
    {
        uint guidMsgId = MessageMapping.Query(typeof(DS_Guid));
        DSG_Guid dsgGuidData = data as DSG_Guid;
        foreach (var dataGuid in dsgGuidData.GuidListList)
        {
            m_InnerCache.AddOrUpdate(guidMsgId, dataGuid.GuidType, null, dataGuid);
        }
    }
    internal void HandleSaveDSGGowStar(uint msgId, string key, IMessage data)
    {
        //排行榜只增不减，直到最大值
        uint gsMsgId = MessageMapping.Query(typeof(DS_GowStar));
        DSG_GowStar dsgGowstarData = data as DSG_GowStar;
        foreach (var gowstar in dsgGowstarData.GowStarListList)
        {
            m_InnerCache.AddOrUpdate(gsMsgId, gowstar.Rank.ToString(), null, gowstar);
        }
    }
    internal void HandleSaveDSGMail(uint msgId, string key, IMessage data)
    {
        uint mailMsgId = MessageMapping.Query(typeof(DS_MailInfo));
        DSG_Mail dsgMailData = data as DSG_Mail;
        List<DS_MailInfo> newMailList = new List<DS_MailInfo>();
        foreach (var mail in dsgMailData.MailListList)
        {
            newMailList.Add(mail);
        }
        List<DataValue> oldMailValueList = m_InnerCache.FindTable(mailMsgId);

        for (int i = 0; i < oldMailValueList.Count; ++i)
        {
            DataValue oldMailValue = oldMailValueList[i];
            DS_MailInfo oldMail = oldMailValue.DataMessage as DS_MailInfo;
            bool flag = newMailList.Exists(delegate (DS_MailInfo mail)
            {
                return mail.Guid.Equals(mail.Guid);
            });
            if (flag == false)
            {
                //不存在说明已经被删除
                DS_MailInfo.Builder mailBuilder = DS_MailInfo.CreateBuilder();
                mailBuilder.SetGuid(oldMail.Guid);
                mailBuilder.SetIsValid(false);
                mailBuilder.SetModuleTypeId(oldMail.ModuleTypeId);
                mailBuilder.SetSender(oldMail.Sender);
                mailBuilder.SetReceiver(oldMail.Receiver);
                mailBuilder.SetSendDate(oldMail.SendDate);
                mailBuilder.SetExpiryDate(oldMail.ExpiryDate);
                mailBuilder.SetTitle(oldMail.Title);
                mailBuilder.SetText(oldMail.Text);
                mailBuilder.SetMoney(oldMail.Money);
                mailBuilder.SetGold(oldMail.Gold);
                mailBuilder.SetStamina(oldMail.Stamina);
                mailBuilder.SetItemIds(oldMail.ItemIds);
                mailBuilder.SetItemNumbers(oldMail.ItemNumbers);
                mailBuilder.SetLevelDemand(oldMail.LevelDemand);
                mailBuilder.SetIsRead(oldMail.IsRead);
                oldMailValue.Dirty = true;
                oldMailValue.Valid = false;
                oldMailValue.DataMessage = mailBuilder.Build();
            }
        }
        foreach (var dataMail in newMailList)
        {
            m_InnerCache.AddOrUpdate(mailMsgId, dataMail.Guid.ToString(), null, dataMail);
        }
    }
    internal void HandleSaveDSGArenaRank(uint msgId, string key, IMessage data)
    {
        //排行榜只增不减，直到最大值
        uint gsMsgId = MessageMapping.Query(typeof(DS_ArenaInfo));
        DSG_ArenaRank dsgArenaRankData = data as DSG_ArenaRank;
        foreach (var arena in dsgArenaRankData.ArenaListList)
        {
            m_InnerCache.AddOrUpdate(gsMsgId, arena.UserGuid.ToString(), null, arena);
        }
    }
    internal void HandleSaveDSGArenaRecord(uint msgId, string key, IMessage data)
    {
        //竞技场战斗记录只增不减
        uint gsMsgId = MessageMapping.Query(typeof(DS_ArenaRecord));
        DSG_ArenaRecord dsgArenaRecordData = data as DSG_ArenaRecord;
        foreach (var record in dsgArenaRecordData.RecordListList)
        {
            m_InnerCache.AddOrUpdate(gsMsgId, record.Guid, null, record);
        }
    }

    private ArkCrossEngine.ServerAsyncActionProcessor m_LoadActionQueue = new ArkCrossEngine.ServerAsyncActionProcessor();
    private ArkCrossEngine.ServerAsyncActionProcessor m_SaveActionQueue = new ArkCrossEngine.ServerAsyncActionProcessor();
    private InnerCache m_InnerCache = null;
    private long m_LastCacheTickTime = 0;
    private const long c_CacheTickInterval = 600000;     //InnerCache的Tick周期:10min
    private long m_LastLogTime = 0;

    internal static DataCacheSystem Instance
    {
        get { return s_Instance; }
    }
    private static DataCacheSystem s_Instance = new DataCacheSystem();
}
