using System;
using System.Text;
using Google.ProtocolBuffers;

namespace SharpSvrAPI
{
  namespace Messenger
  {
    /// <summary>
    /// protobuf消息发送器, 由PBChannnel创建, 向该channnel发送消息
    /// </summary>
    public class PBEmitter
    {
      public bool ConnectionAvailable
      {
        get
        {
          if (null != svr_name_)
          {
            uint handle = uint.MaxValue;
            ErrorCode ec = channel_.SvrAPI.QueryServiceHandleByName(svr_name_, ref handle);
            return ec == ErrorCode.good;
          }
          else if (uint.MaxValue != svr_handle_)
          {
            /*
              FIXME: p/invoke api of SvrAPI of QueryServiceNameByHandle is not safe
            StringBuilder sb = new StringBuilder();
            uint sz = 0;
            ErrorCode ec = channel_.SvrAPI.QueryServiceNameByHandle(svr_handle_, sb, ref sz);
            return ec == ErrorCode.good;
            */
          }
          return false;
        }
      }

      public string SvrName { get { return svr_name_; } }
      public uint SvrHandle { get { return svr_handle_; } }

      internal PBEmitter(PBChannel channel, string svr_name = null, uint svr_handle = uint.MaxValue)
      {
        channel_ = channel;
        svr_name_ = svr_name;
        svr_handle_ = svr_handle;
      }

      public Tuple<uint, ErrorCode> Emit(IMessage msg, uint session = uint.MaxValue)
      {
        if (null != svr_name_)
          return channel_.Send(svr_name_, msg, session);
        else if (uint.MaxValue != svr_handle_)
          return channel_.Send(svr_handle_, msg, session);
        else
          return Tuple.Create(uint.MaxValue, ErrorCode.invalid_parameters);
      }

      public override string ToString()
      {
        if (null == desc_)
        {
          string addr = null != svr_name_ ? svr_name_ : (uint.MaxValue != svr_handle_ ? string.Format("0x{0:x}", svr_handle_) : "invalid");
          desc_ = string.Format("[channel:{0} addr:{1}]", channel_.ChannelId, addr);
        }
        return desc_;
      }

      private string desc_;
      private string svr_name_;
      private uint svr_handle_;
      private PBChannel channel_;
    }
  }
}