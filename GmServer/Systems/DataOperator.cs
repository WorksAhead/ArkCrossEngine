using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using DashFire.DataStore;
using DashFire;
using System.Threading;
using ArkCrossEngine;

namespace GmServer
{
  internal class DataOperator : MyServerThread
  {
    internal delegate void LoadHandlerDelegate(uint msgId, string key, MyAction<DSLoadResult, string, IMessage> lccb);

    private static DataOperator s_Instance = new DataOperator();
    private ServerAsyncActionProcessor m_LoadActionQueue = new ServerAsyncActionProcessor();
    private long m_LastLogTime = 0;
    internal ArkCrossEngine.ServerAsyncActionProcessor LoadActionQueue
    {
      get { return m_LoadActionQueue; }
    }
    internal static DataOperator Instance
    {
      get { return s_Instance; }
    }

    internal class GMMessageHandlerDispatcher
    {
      private Dictionary<uint, LoadHandlerDelegate> m_LoadHandlers = new Dictionary<uint, LoadHandlerDelegate>();
      internal static GMMessageHandlerDispatcher Instance
      {
        get { return s_Instance; }
      }
      private static GMMessageHandlerDispatcher s_Instance = new GMMessageHandlerDispatcher();

      internal void RegisterLoadHandler(uint msgId, LoadHandlerDelegate handler)
      {
        if (m_LoadHandlers.ContainsKey(msgId)) {
          m_LoadHandlers[msgId] = handler;
        } else {
          m_LoadHandlers.Add(msgId, handler);
        }
      }
      internal void DispatchLoadMessage(object state)
      {
        var tuple = (Tuple<uint, string, MyAction<DSLoadResult, string, IMessage>>)state;
        uint msgId = tuple.Item1;
        string key = tuple.Item2;
        MyAction<DSLoadResult, string, IMessage> cb = tuple.Item3;
        LoadHandlerDelegate handler = null;
        m_LoadHandlers.TryGetValue(msgId, out handler);
        if (handler != null) {
          handler(msgId, key, cb);
        }
      }
    }

    internal void Init()
    {
      GMMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(GMP_Account)), HandleLoadGMPAccount);
      GMMessageHandlerDispatcher.Instance.RegisterLoadHandler(MessageMapping.Query(typeof(GMP_User)), HandleLoadGMPUser);
      Start();
      LogSys.Log(LOG_TYPE.INFO, "DataOperator initialized");
    }
    ///==============================================================================================
    /// 通过QueueAction调用的方法。
    ///==============================================================================================
    internal void Load(uint msgId, string key, MyAction<DSLoadResult, string, IMessage> cb)
    {
      GMMessageHandlerDispatcher.Instance.DispatchLoadMessage(Tuple.Create(msgId, key, cb));
    }
    ///==============================================================================================
    /// 只能在本线程调用的方法。
    ///==============================================================================================
    private void HandleLoadGMPAccount(uint msgId, string key, MyAction<DSLoadResult, string, IMessage> cb)
    {
      string error = null;
      IMessage data = null;
      Type dataType = MessageMapping.Query(msgId);
      try {
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() => {
          GMP_Account.Builder dataAccountBuilder = GMP_Account.CreateBuilder();
          DS_Account dataAccountBasic = DataLoadImpl.LoadSingleRow(typeof(DS_Account), key) as DS_Account;
          if (dataAccountBasic != null) {
            dataAccountBuilder.SetAccount(dataAccountBasic.Account);
            dataAccountBuilder.SetAccountBasic(dataAccountBasic);
          } else {
            error = string.Format("GMServer Load from Database MISS: key:({0}), data({1})", key, dataType.Name);
            cb(DSLoadResult.NotFound, error, null);
            LogSys.Log(LOG_TYPE.INFO, error);
            return;
          }
          List<DS_UserInfo> dataUserList = new List<DS_UserInfo>();
          if (dataAccountBasic.UserGuid1 > 0) {
            DS_UserInfo dataUser = DataLoadImpl.LoadSingleRow(typeof(DS_UserInfo), dataAccountBasic.UserGuid1.ToString()) as DS_UserInfo;
            if (dataUser != null) {
              dataUserList.Add(dataUser);
            }
          }
          if (dataAccountBasic.UserGuid2 > 0) {
            DS_UserInfo dataUser = DataLoadImpl.LoadSingleRow(typeof(DS_UserInfo), dataAccountBasic.UserGuid2.ToString()) as DS_UserInfo;
            if (dataUser != null) {
              dataUserList.Add(dataUser);
            }
          }
          if (dataAccountBasic.UserGuid3 > 0) {
            DS_UserInfo dataUser = DataLoadImpl.LoadSingleRow(typeof(DS_UserInfo), dataAccountBasic.UserGuid3.ToString()) as DS_UserInfo;
            if (dataUser != null) {
              dataUserList.Add(dataUser);
            }
          }
          foreach (var dataUser in dataUserList) {
            dataAccountBuilder.UserListList.Add(dataUser as DS_UserInfo);
          }
          data = dataAccountBuilder.Build();
          cb(DSLoadResult.Success, null, data);
          LogSys.Log(LOG_TYPE.DEBUG, "GMServer Load from Database: key:({0}), data({1})", key, dataType.Name);
        });
      } catch (Exception e) {
        error = e.Message;
        cb(DSLoadResult.PostError, error, data);
        LogSys.Log(LOG_TYPE.ERROR, "GMServer Load from Database ERROR: key:({0}), data({1}), error({2})", key, dataType.Name, error);
        return;
      }
    }
    private void HandleLoadGMPUser(uint msgId, string key, MyAction<DSLoadResult, string, IMessage> cb)
    {
      string error = null;
      IMessage data = null;
      Type dataType = MessageMapping.Query(msgId);
      try {
        uint userTypeId = MessageMapping.Query(typeof(DS_UserInfo));
        GMP_User.Builder dataUserBuilder = GMP_User.CreateBuilder();
        DbThreadManager.Instance.LoadActionQueue.QueueAction(() => {
          DS_UserInfo dataUserBasic = DataLoadImpl.LoadSingleRow(typeof(DS_UserInfo), key) as DS_UserInfo;
          if (dataUserBasic != null) {
            dataUserBuilder.SetUserGuid(dataUserBasic.Guid);
            dataUserBuilder.SetUserBasic(dataUserBasic);
          } else {
            error = string.Format("GMServer Load from Database MISS: key:({0}), data({1})", key, dataType.Name);
            cb(DSLoadResult.NotFound, error, null);
            LogSys.Log(LOG_TYPE.INFO, error);
            return;
          }
          data = dataUserBuilder.Build();
          cb(DSLoadResult.Success, null, data);
          LogSys.Log(LOG_TYPE.DEBUG, "GMServer Load from Database: key:({0}), data({1})", key, dataType.Name);
        });
      } catch (Exception e) {
        error = e.Message;
        cb(DSLoadResult.PostError, error, data);
        LogSys.Log(LOG_TYPE.ERROR, "GMServer Load from Database ERROR: key:{0}, data:{1}, error:{2},stacktrace:{3}",
                                      key, dataType.Name, error, e.StackTrace);
        return;
      }
    }
    ///==============================================================================================
    /// override
    ///==============================================================================================
    protected override void OnStart()
    {
      TickSleepTime = 10;
      ActionNumPerTick = 1024;
    }
    protected override void OnTick()
    {
      try {
        long curTime = TimeUtility.GetServerMilliseconds();
        if (m_LastLogTime + 60000 < curTime) {
          m_LastLogTime = curTime;
          DebugPoolCount((string msg) => {
            LogSys.Log(LOG_TYPE.INFO, "DataOperator.ActionQueue {0}", msg);
          });
          m_LoadActionQueue.DebugPoolCount((string msg) => {
            LogSys.Log(LOG_TYPE.INFO, "DataOperator.LoadActionQueue {0}", msg);
          });
        }
        m_LoadActionQueue.HandleActions(1024);
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "DataOperator ERROR:{0} \n StackTrace:{1}", ex.Message, ex.StackTrace);
      }
    }
  }
}