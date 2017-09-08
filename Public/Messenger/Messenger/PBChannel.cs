using System;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using CSharpCenterClient;

namespace Messenger
{
    /// <summary>
    /// 消息channel, 即表示了某一类dcore的消息类型的消息
    /// </summary>
    public sealed class PBChannel
    {
        public delegate uint MsgIdQuery(Type t);
        public delegate Type MsgTypeQuery(uint id);
        public delegate void DefaultMessageHandler(IMessage msg, PBChannel channel, int src, uint session);

        /// <summary>
        /// 内部初始化channel, 比较重要的是(id<->type)的相互查询函数
        /// </summary>
        /// <param name="msgid_query">id->type查询函数</param>
        /// <param name="msgtype_query">type->id查询函数</param>
        public PBChannel(MsgIdQuery msgid_query, MsgTypeQuery msgtype_query)
        {
            msgid_query_ = msgid_query;
            msgtype_query_ = msgtype_query;
        }

        public void OnUpdateNameHandle(bool addOrUpdate, string name, int handle)
        {
            if (name == default_service_name_)
            {
                if (addOrUpdate)
                    default_service_handle_ = handle;
                else
                    default_service_handle_ = 0;
            }
        }
        public string DefaultServiceName
        {
            get { return default_service_name_; }
            set { default_service_name_ = value; }
        }
        public int DefaultServiceHandle
        {
            get
            {
                if (default_service_handle_ == 0)
                {
                    default_service_handle_ = CenterClientApi.TargetHandle(default_service_name_);
                }
                return default_service_handle_;
            }
        }

        /// <summary>
        /// 注册对于类型是MsgType的消息的处理函数, 例如:
        /// 对于MsgA
        /// c.Register<MsgA>(Handler);
        /// Handler的定义是
        /// void Handler(MsgA msg, PBChannel channel, int src,uint session) { ... }
        /// </summary>
        /// <typeparam name="MsgType">protobuf的消息类型</typeparam>
        /// <param name="f">消息处理函数</param>
        public void Register<MsgType>(PBHandler<MsgType>.F f)
          where MsgType : IMessage
        {
            uint id = msgid_query_(typeof(MsgType));
            if (uint.MaxValue != id)
                handlers_[id] = new PBHandler<MsgType>(f);
        }

        /// <summary>
        /// 默认的消息处理函数, 凡是没有注册处理函数的消息, 都会调用到这个函数
        /// </summary>
        /// <param name="h">消息处理函数</param>
        public void RegisterDefaultHandler(DefaultMessageHandler h)
        {
            default_handler_ = h;
        }

        public void Dispatch(int from_handle, uint seq, byte[] data)
        {
            uint id;
            IMessage msg;
            if (PBCodec.Decode(data, msgtype_query_, out msg, out id))
            {
                IPBHandler h;
                if (handlers_.TryGetValue(id, out h))
                    h.Execute(msg, this, from_handle, seq);
                else if (null != default_handler_)
                    default_handler_(msg, this, from_handle, seq);
            }
        }

        public bool Send(IMessage msg)
        {
            bool ret = false;
            int handle = DefaultServiceHandle;
            if (handle > 0)
            {
                ret = Send(handle, msg);
            }
            return ret;
        }

        public bool Send(string dest_name, IMessage msg)
        {
            bool ret = false;
            int handle = CenterClientApi.TargetHandle(dest_name);
            if (handle != 0)
            {
                ret = Send(handle, msg);
            }
            return ret;
        }

        public bool Send(int dest_handle, IMessage msg)
        {
            byte[] data;
            bool ret = Build(msg, out data);
            ret = ret && CenterClientApi.SendByHandle(dest_handle, data, data.Length);
            return ret;
        }

        public byte[] Encode(IMessage msg)
        {
            uint id = msgid_query_(msg.GetType());
            return (uint.MaxValue != id) ?
              PBCodec.Encode(id, msg) :
              null;
        }

        public IMessage Decode(byte[] data)
        {
            uint id;
            IMessage msg;
            PBCodec.Decode(data, msgtype_query_, out msg, out id);
            return msg;
        }

        private bool Build(IMessage msg, out byte[] data)
        {
            uint id = msgid_query_(msg.GetType());
            if (uint.MaxValue != id)
            {
                data = PBCodec.Encode(id, msg);
                return null != data;
            }
            else
            {
                data = null;
                return false;
            }
        }

        private MsgIdQuery msgid_query_;
        private MsgTypeQuery msgtype_query_;
        private string default_service_name_;
        private int default_service_handle_;
        private DefaultMessageHandler default_handler_;
        private Dictionary<uint, IPBHandler> handlers_ = new Dictionary<uint, IPBHandler>();
    }
}