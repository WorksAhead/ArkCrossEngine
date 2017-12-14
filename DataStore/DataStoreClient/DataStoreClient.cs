using System;
using System.Collections.Generic;
using Messenger;
using Google.ProtocolBuffers;
using System.Security.Cryptography;

namespace DashFire.DataStore
{
    public sealed class DataStoreClient
    {
        public enum ConnectStatus
        {
            None = 0,
            Connecting,
            Connected,
            Disconnect,
        }

        public delegate void ConnectCallback ( bool ret, string error );
        public delegate void SaveCallback ( DSSaveResult ret, string error );
        public delegate void LoadCallback<T> ( DSLoadResult ret, string error, T data ) where T : IMessage;

        public DataStoreClient ( PBChannel channel, ArkCrossEngine.MyServerThread thread )
        {
            channel_ = channel;
            m_RunningThread = thread;

            channel_.Register<NLRep_Connect>(OnConnectReply);
            channel_.Register<NLRep_Save>(OnSaveReply);
            channel_.Register<NLRep_Load>(OnLoadReply);

            Config.Init();
            current_status = ConnectStatus.None;
            loadOpt_timeout_ = new ArkCrossEngine.Timeout<LoadCBBox>();
            loadOpt_timeout_.DefaultTimeoutMS = Config.DSRequestTimeout;
            saveOpt_timeout_ = new ArkCrossEngine.Timeout<SaveCallback>();
            saveOpt_timeout_.DefaultTimeoutMS = Config.DSRequestTimeout;
        }

        public ConnectStatus CurrentStatus
        {
            get { return current_status; }
            set { current_status = value; }
        }

        public void Connect ( string clientName, ConnectCallback cb )
        {
            connect_callback_ = cb;
            if ( current_status == ConnectStatus.Connecting || current_status == ConnectStatus.Connected )
            {
                return;
            }
            string error = null;
            LNReq_Connect connectData = null;
            try
            {
                LNReq_Connect.Builder connectBuilder = LNReq_Connect.CreateBuilder();
                connectBuilder.SetClientName(clientName);
                connectData = connectBuilder.Build();
            }
            catch ( Exception e )
            {
                error = e.Message;
            }
            if ( null != error )
            {
                cb(false, error);
                return;
            }
            if ( !channel_.Send(connectData) )
            {
                cb(false, "unknown");
            }
            else
            {
                current_status = ConnectStatus.Connecting;
                //connectOpt_timeout_.Add(session, cb, () => cb(false, "connect request timeout"));        
            }
        }

        private void OnConnectReply ( NLRep_Connect msg, PBChannel channel, int src, uint session )
        {
            ConnectCallback cb = connect_callback_;
            if ( null != cb )
            {
                bool connectRet = false;
                string errorStr = "Unknown Error";
                if ( msg.Result == true )
                {
                    connectRet = true;
                    errorStr = "Connect Success";
                }
                else
                {
                    connectRet = false;
                    errorStr = msg.Error;
                }
                cb(connectRet, errorStr);
            }
        }

        public void Save ( string key, IMessage data, SaveCallback cb )
        {
            Save(key, data, cb, true);
        }

        public void Save ( string key, IMessage data, SaveCallback cb, bool isForce )
        {
            Constraints.MaxSize(data);
            string error = null;
            LNReq_Save saveData = null;
            uint dsMsgId = MessageMapping.Query(data.GetType());
            if ( dsMsgId == uint.MaxValue )
            {
                error = string.Format("unknown data message: " + data.GetType().Name);
            }
            string timeoutKey = string.Format("{0}:{1}", dsMsgId, key);
            if ( !isForce && saveOpt_timeout_.Exists(timeoutKey) )
            {
                error = "Save operation are too frequent";
                cb(DSSaveResult.PrepError, error);
                return;
            }
            try
            {
                byte[] bytes = data.ToByteArray();
                LNReq_Save.Builder saveBuilder = LNReq_Save.CreateBuilder();
                saveBuilder.SetDsMsgId(dsMsgId);
                saveBuilder.SetKey(key);
                saveBuilder.SetDsBytes(ByteString.Unsafe.FromBytes(bytes));
                saveBuilder.SetChecksum(Crc32.Compute(bytes));
                saveData = saveBuilder.Build();
            }
            catch ( Exception e )
            {
                error = e.Message;
            }
            if ( null != error )
            {
                cb(DSSaveResult.PrepError, error);
                return;
            }
            if ( !channel_.Send(saveData) )
            {
                cb(DSSaveResult.PrepError, "unknown");
            }
            else
            {
                //添加到超时验证
                string timeoutTip = string.Format("DataStore save request timeout. MsgId:{0}, Key:{1}", dsMsgId, key);
                saveOpt_timeout_.Set(timeoutKey, cb, () => cb(DSSaveResult.TimeoutError, timeoutTip));
            }
        }

        private void OnSaveReply ( NLRep_Save msg, PBChannel channel, int src, uint session )
        {
            if ( null != m_RunningThread )
            {
                m_RunningThread.QueueAction(this.DoSaveReply, msg, channel, src, session);
            }
        }
        private void DoSaveReply ( NLRep_Save msg, PBChannel channel, int src, uint session )
        {
            string timeoutKey = string.Format("{0}:{1}", msg.DsMsgId, msg.Key);
            SaveCallback cb = saveOpt_timeout_.Get(timeoutKey);
            if ( null != cb )
            {
                DSSaveResult saveRet = DSSaveResult.UnknownError;
                string errorStr = "Save Unknown Error";
                if ( msg.Result == NLRep_Save.Types.SaveResult.Success )
                {
                    saveRet = DSSaveResult.Success;
                    errorStr = "Save Success";
                }
                else if ( msg.Result == NLRep_Save.Types.SaveResult.Error )
                {
                    saveRet = DSSaveResult.PostError;
                    if ( msg.HasError )
                    {
                        errorStr = msg.Error.ToString();
                    }
                    else
                    {
                        errorStr = "Save Post Error";
                    }
                }
                cb(saveRet, errorStr);
                saveOpt_timeout_.Remove(timeoutKey);
            }
        }

        public void Load<T> ( string key, LoadCallback<T> cb )
          where T : IMessage
        {
            uint dsMsgId = MessageMapping.Query(typeof(T));
            if ( dsMsgId == uint.MaxValue )
            {
                cb(DSLoadResult.PrepError, string.Format("unknown data message: {0}", typeof(T).Name), default(T));
                return;
            }
            string timeoutKey = string.Format("{0}:{1}", dsMsgId, key);
            if ( loadOpt_timeout_.Exists(timeoutKey) )
            {
                cb(DSLoadResult.PrepError, "Load operation are too frequent", default(T));
            }
            else
            {
                LNReq_Load.Builder loadBuilder = LNReq_Load.CreateBuilder();
                loadBuilder.SetDsMsgId(dsMsgId);
                loadBuilder.SetKey(key);
                LNReq_Load loadData = loadBuilder.Build();
                if ( !channel_.Send(loadData) )
                {
                    cb(DSLoadResult.PrepError, "Create protobuf data error.", default(T));
                }
                else
                {
                    LoadCBBox cbbox = new LoadCBBoxI<T>(cb);
                    string timeoutTip = string.Format("DataStore load request timeout. MsgId:{0}, Key:{1}", dsMsgId, key);
                    loadOpt_timeout_.Set(timeoutKey, cbbox, () => cbbox.Invoke(DSLoadResult.TimeoutError, timeoutTip, null));
                }
            }
        }

        private void OnLoadReply ( NLRep_Load msg, PBChannel channel, int src, uint session )
        {
            if ( null != m_RunningThread )
            {
                m_RunningThread.QueueAction(this.DoLoadReply, msg, channel, src, session);
            }
        }
        private void DoLoadReply ( NLRep_Load msg, PBChannel channel, int src, uint session )
        {
            string timeoutKey = string.Format("{0}:{1}", msg.DsMsgId, msg.Key);
            LoadCBBox cbbox = loadOpt_timeout_.Get(timeoutKey);
            if ( null != cbbox )
            {
                DSLoadResult loadRet = DSLoadResult.UnknownError;
                string errorStr = "Save Unknown Error";
                IMessage data = null;
                if ( msg.Result == NLRep_Load.Types.LoadResult.Success )
                {
                    loadRet = DSLoadResult.Success;
                    data = channel_.Decode(ByteString.Unsafe.GetBuffer(msg.Data));
                    errorStr = "Load Success";
                    loadOpt_timeout_.Remove(timeoutKey);
                }
                else if ( msg.Result == NLRep_Load.Types.LoadResult.Undone )
                {
                    loadRet = DSLoadResult.Undone;
                    data = channel_.Decode(ByteString.Unsafe.GetBuffer(msg.Data));
                    errorStr = "Load Undone";
                }
                else if ( msg.Result == NLRep_Load.Types.LoadResult.NotFound )
                {
                    loadRet = DSLoadResult.NotFound;
                    if ( msg.HasError )
                    {
                        errorStr = msg.Error.ToString();
                    }
                    else
                    {
                        errorStr = "Load Key NOT Found in DataStore";
                    }
                    loadOpt_timeout_.Remove(timeoutKey);
                }
                else
                {
                    loadRet = DSLoadResult.PostError;
                    if ( msg.HasError )
                    {
                        errorStr = msg.Error.ToString();
                    }
                    else
                    {
                        errorStr = "Load data error";
                    }
                    loadOpt_timeout_.Remove(timeoutKey);
                }
                cbbox.Invoke(loadRet, errorStr, data);
                //LogSys.Log(LOG_TYPE.DEBUG, "Load Reply ! result:({0}) session:({1})  error:({2})", loadRet, session, errorStr);
            }
        }

        private Tuple<string, LNReq_Save> BuildSaveRequestMsg ( IMessage data )
        {
            uint id = MessageMapping.Query(data.GetType());
            if ( id == uint.MaxValue )
            {
                return Tuple.Create("unknown data message: " + data.GetType().Name, default(LNReq_Save));
            }
            try
            {
                byte[] bytes = data.ToByteArray();
                return Tuple.Create(
                         default(string),
                         LNReq_Save.CreateBuilder()
                                   .SetDsMsgId(id)
                                   .SetDsBytes(ByteString.Unsafe.FromBytes(bytes))
                                   .SetChecksum(Crc32.Compute(bytes))
                                   .Build());
            }
            catch ( Exception e )
            {
                return Tuple.Create(e.Message, default(LNReq_Save));
            }
        }

        public void Tick ()
        {
            loadOpt_timeout_.Tick();
            saveOpt_timeout_.Tick();
        }

        private PBChannel channel_;
        private ArkCrossEngine.MyServerThread m_RunningThread;

        private ConnectStatus current_status = ConnectStatus.None;
        private ConnectCallback connect_callback_;
        private ArkCrossEngine.Timeout<LoadCBBox> loadOpt_timeout_;
        private ArkCrossEngine.Timeout<SaveCallback> saveOpt_timeout_;
    }
}
