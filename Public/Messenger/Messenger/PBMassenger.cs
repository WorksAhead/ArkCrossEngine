using System;
using System.Runtime.InteropServices;

namespace SharpSvrAPI
{
  namespace Messenger
  {
    /// <summary>
    /// 有关protobuf消息处理和基于channel概念发送消息的类
    /// </summary>
    public sealed class PBMessenger
    {
      /// <summary>
      /// 创建PBMessenger
      /// </summary>
      /// <param name="max_channel">channel最大数量, 即CoreMessageTypeExtend枚举用到的最大数值+1</param>
      /// <param name="svr_api">ServiceAPI</param>
      public PBMessenger(byte max_channel, ServiceAPI svr_api)
      {
        channels_ = new PBChannel[max_channel];
        svr_api_ = svr_api;
      }

      /// <summary>
      /// 分派消息到处理函数
      /// </summary>
      /// <param name="source_handle"></param>
      /// <param name="dest_handle"></param>
      /// <param name="session"></param>
      /// <param name="msg_type"></param>
      /// <param name="data"></param>
      /// <param name="len"></param>
      public unsafe void Dispatch(uint source_handle, uint dest_handle,
                                  uint session, byte msg_type,
                                  byte* data, ushort len)
      {
        if (msg_type < channels_.Length)
        {
          var c = channels_[msg_type];
          if (null != c)
          {
            byte[] buffer = new byte[len];
            Marshal.Copy((IntPtr)data, buffer, 0, (int)len);
            c.Dispatch(source_handle, session, buffer);
          }
        }
      }

      /// <summary>
      /// 初始化一个channel
      /// </summary>
      /// <param name="channel">CoreMessageExtendType的枚举值</param>
      /// <param name="msgid_query">id->type的查询函数</param>
      /// <param name="msgtype_query">type->id的查询函数</param>
      public void AddChannel(byte channel, 
                             PBChannel.MsgIdQuery msgid_query,
                             PBChannel.MsgTypeQuery msgtype_query)
      {
        if (channel < channels_.Length)
        {
          if (null == channels_[channel])
            channels_[channel] = new PBChannel(channel, svr_api_, msgid_query, msgtype_query);
        }
      }

      /// <summary>
      /// 取得指向某个channel的PBChannel object
      /// </summary>
      /// <param name="channel">CoreMessageExtendType的枚举值</param>
      /// <returns></returns>
      public PBChannel To(byte channel)
      {
        return (channel < channels_.Length) ? channels_[channel] : null;
      }

      private PBChannel[] channels_;
      private ServiceAPI svr_api_;
    }
  }
}