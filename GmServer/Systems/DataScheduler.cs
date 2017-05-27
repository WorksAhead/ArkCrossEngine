using System;
using System.Collections.Concurrent;
using Google.ProtocolBuffers;
using Messenger;
using DashFire.DataStore;
using DashFire;
using ArkCrossEngine;

namespace GmServer
{
  internal class DataScheduler
  {
    private PBChannel channel_;
    internal static DataScheduler Instance
    {
      get { return s_Instance; }
    }
    private static DataScheduler s_Instance = new DataScheduler();

    internal void Init(PBChannel channel)
    {
      channel_ = channel;
      channel_.Register<LNReq_Connect>(ConnectHandler);
      channel_.Register<LNReq_Load>(LoadHandler);
      LogSys.Log(LOG_TYPE.INFO, "DataScheduler initialized");
    }
    internal void Release()
    {
      channel_ = null;
      LogSys.Log(LOG_TYPE.INFO, "DataScheduler disposed");
    }
    private void ConnectHandler(LNReq_Connect msg, PBChannel channel, int handle, uint seq)
    {
      try {
        bool ret = true;
        string errorMsg = string.Empty;
        LogSys.Log(LOG_TYPE.INFO, "GMServerClient connect :{0} ", msg.ClientName);
        var reply = NLRep_Connect.CreateBuilder();
        reply.SetResult(ret);
        reply.SetError(errorMsg);
        channel.Send(reply.Build());
      } catch (Exception e) {
        var reply = NLRep_Connect.CreateBuilder();
        reply.SetResult(false);
        reply.SetError(e.Message);
        channel.Send(reply.Build());
      }
    }
    private void LoadHandler(LNReq_Load msg, PBChannel channel, int handle, uint seq)
    {
      try
      {
        DataOperator.Instance.LoadActionQueue.QueueAction((MyAction<uint, string, MyAction<DSLoadResult, string, IMessage>>)DataOperator.Instance.Load,
          msg.DsMsgId, 
          msg.Key,
          (ret, error, data) =>
          {
            var reply = NLRep_Load.CreateBuilder();
            reply.SetDsMsgId(msg.DsMsgId);
            reply.SetKey(msg.Key);
            if (ret == DSLoadResult.Success) {
              reply.SetResult(NLRep_Load.Types.LoadResult.Success);
              reply.SetData(ByteString.Unsafe.FromBytes(channel_.Encode(data)));
            } else if (ret == DSLoadResult.Undone) {
              reply.SetResult(NLRep_Load.Types.LoadResult.Undone);
              reply.SetData(ByteString.Unsafe.FromBytes(channel_.Encode(data)));
            } else if (ret == DSLoadResult.NotFound) {
              reply.SetResult(NLRep_Load.Types.LoadResult.NotFound);            
              reply.SetError(error);
            } else {
              reply.SetResult(NLRep_Load.Types.LoadResult.Error);
              reply.SetError(error);
            }
            NLRep_Load replyData = reply.Build();          
            channel.Send(replyData);
            LogSys.Log(LOG_TYPE.INFO, "Load data finished. msgId:({0}) key:({1}) result:({2}) ", msg.DsMsgId, msg.Key, ret);
          });
      } catch (Exception e) {
        var errorReply = NLRep_Load.CreateBuilder();
        errorReply.SetResult(NLRep_Load.Types.LoadResult.Error);      
        errorReply.SetError(e.Message);
        channel.Send(errorReply.Build());
        LogSys.Log(LOG_TYPE.ERROR, "GMServer load data failed. msgId:({0}) key:({1}) seq:({2}) error:({3} detail:{4})", 
          msg.DsMsgId, msg.Key, seq, e.Message, e.StackTrace);
      }
    }
  }
}