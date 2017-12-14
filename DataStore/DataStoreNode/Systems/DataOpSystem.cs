using System;
using System.Collections.Concurrent;
using Google.ProtocolBuffers;
using Messenger;
using DashFire;
using DashFire.DataStore;
using ArkCrossEngine;

internal class DataOpSystem
{
    internal void Init ( PBChannel channel )
    {
        channel_ = channel;
        channel_.Register<LNReq_Connect>(ConnectHandler);
        channel_.Register<LNReq_Load>(LoadHandler);
        channel_.Register<LNReq_Save>(SaveHandler);
        InitDSNodeVersion();
        LogSys.Log(LOG_TYPE.INFO, "DataOperator initialized");
    }

    internal void Release ()
    {
        channel_ = null;
        LogSys.Log(LOG_TYPE.INFO, "DataOperator disposed");
    }

    internal bool Enable
    {
        get { return enable_; }
        set { enable_ = value; }
    }

    internal void InitDSNodeVersion ()
    {
        m_DBVersion = DataProcedureImplement.GetDSNodeVersion().Trim();
        if ( m_DBVersion.Equals(DSNodeVersion.Version) )
        {
            Enable = true;
            LogSys.Log(LOG_TYPE.INFO, "Init DSNodeVersion Success:{0}", m_DBVersion);
        }
        else
        {
            string errorMsg = string.Format("DSNodeVersion:{0} ,DBVersion:{1} do not match! Fatel ERROR!!!", DSNodeVersion.Version, m_DBVersion);
            throw new Exception(errorMsg);
        }
    }

    private void ConnectHandler ( LNReq_Connect msg, PBChannel channel, int handle, uint seq )
    {
        if ( !Enable )
        {
            LogSys.Log(LOG_TYPE.ERROR, "Connect to DataStorNode while DataOperator is Disable");
            return;
        }
        try
        {
            bool ret = true;
            string errorMsg = string.Empty;
            LogSys.Log(LOG_TYPE.INFO, "DataStoreClient connect :{0} ", msg.ClientName);
            var reply = NLRep_Connect.CreateBuilder();
            reply.SetResult(ret);
            reply.SetError(errorMsg);
            channel.Send(reply.Build());
        }
        catch ( Exception e )
        {
            var reply = NLRep_Connect.CreateBuilder();
            reply.SetResult(false);
            reply.SetError(e.Message);
            channel.Send(reply.Build());
        }
    }

    private void LoadHandler ( LNReq_Load msg, PBChannel channel, int handle, uint seq )
    {
        if ( !Enable )
        {
            LogSys.Log(LOG_TYPE.ERROR, "Load a message while DataOperator is Disable");
            return;
        }
        try
        {
            DataCacheSystem.Instance.LoadActionQueue.QueueAction((MyAction<uint, string, MyAction<DSLoadResult, string, IMessage>>)DataCacheSystem.Instance.Load,
              msg.DsMsgId,
              msg.Key,
              ( ret, error, data ) =>
              {
                  //这段代码必须保证线程安全，会在不同线程调用！！！
                  var reply = NLRep_Load.CreateBuilder();
                  reply.SetDsMsgId(msg.DsMsgId);
                  reply.SetKey(msg.Key);
                  if ( ret == DSLoadResult.Success )
                  {
                      reply.SetResult(NLRep_Load.Types.LoadResult.Success);
                      reply.SetData(ByteString.Unsafe.FromBytes(channel_.Encode(data)));
                  }
                  else if ( ret == DSLoadResult.Undone )
                  {
                      reply.SetResult(NLRep_Load.Types.LoadResult.Undone);
                      reply.SetData(ByteString.Unsafe.FromBytes(channel_.Encode(data)));
                  }
                  else if ( ret == DSLoadResult.NotFound )
                  {
                      reply.SetResult(NLRep_Load.Types.LoadResult.NotFound);
                      reply.SetError(error);
                  }
                  else
                  {
                      reply.SetResult(NLRep_Load.Types.LoadResult.Error);
                      reply.SetError(error);
                  }
                  NLRep_Load replyData = reply.Build();
                  channel.Send(replyData);
                  LogSys.Log(LOG_TYPE.INFO, "Load data finished. msgId:({0}) key:({1}) result:({2}) ", msg.DsMsgId, msg.Key, ret);
              });
        }
        catch ( Exception e )
        {
            var errorReply = NLRep_Load.CreateBuilder();
            errorReply.SetResult(NLRep_Load.Types.LoadResult.Error);
            errorReply.SetError(e.Message);
            channel.Send(errorReply.Build());
            LogSys.Log(LOG_TYPE.ERROR, "DataStore load data failed. msgId:({0}) key:({1}) seq:({2}) error:({3} detail:{4})",
              msg.DsMsgId, msg.Key, seq, e.Message, e.StackTrace);
        }
    }

    private void SaveHandler ( LNReq_Save msg, PBChannel channel, int handle, uint seq )
    {
        if ( !Enable )
        {
            LogSys.Log(LOG_TYPE.ERROR, "Save a message while DataOperator is Disable");
            return;
        }
        var reply = NLRep_Save.CreateBuilder();
        reply.SetDsMsgId(msg.DsMsgId);
        reply.SetKey(msg.Key);
        reply.Result = NLRep_Save.Types.SaveResult.Success;
        try
        {
            byte[] data_bytes = ByteString.Unsafe.GetBuffer(msg.DsBytes);
            int calc_checksum = Crc32.Compute(data_bytes);
            if ( msg.Checksum != calc_checksum )
            {
                throw new DataChecksumError(msg.Checksum, calc_checksum);
            }
            string dataTypeName = string.Empty;
            if ( m_DSDMessages.TryGetValue(msg.DsMsgId, out dataTypeName) )
            {
                DataCacheSystem.Instance.QueueAction(DataCacheSystem.Instance.DirectSave, msg.DsMsgId, msg.Key, data_bytes);
            }
            else
            {
                dataTypeName = MessageMapping.Query(msg.DsMsgId).Name;
                if ( dataTypeName.StartsWith("DSD_") )
                {
                    //直接写入数据库
                    m_DSDMessages.AddOrUpdate(msg.DsMsgId, dataTypeName, ( key, oldValue ) => dataTypeName);
                    DataCacheSystem.Instance.QueueAction(DataCacheSystem.Instance.DirectSave, msg.DsMsgId, msg.Key, data_bytes);
                }
                else
                {
                    //写入数据缓存
                    DataCacheSystem.Instance.SaveActionQueue.QueueAction(DataCacheSystem.Instance.Save, msg.DsMsgId, msg.Key, data_bytes);
                }
            }
        }
        catch ( Exception e )
        {
            reply.Result = NLRep_Save.Types.SaveResult.Error;
            reply.SetError(e.Message);
            LogSys.Log(LOG_TYPE.ERROR, "Save data ERROR: msgId:({0}) seq:({1}) error:({2}) detail:{3}", msg.DsMsgId, seq, e.Message, e.StackTrace);
        }
        channel.Send(reply.Build());
    }

    private bool enable_ = false;
    private PBChannel channel_;
    private string m_DBVersion;
    private ConcurrentDictionary<uint, string> m_DSDMessages = new ConcurrentDictionary<uint, string>();

    internal static DataOpSystem Instance
    {
        get { return s_Instance; }
    }
    private static DataOpSystem s_Instance = new DataOpSystem();
}