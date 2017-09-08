using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Collections.Generic;
using Google.ProtocolBuffers;

namespace Messenger
{
    internal static class PBCodec
    {
        /// <summary>
        /// 编码protobuf消息
        /// </summary>
        /// <param name="id">消息id</param>
        /// <param name="msg">消息object</param>
        /// <returns></returns>
        public static byte[] Encode(uint id, IMessage msg)
        {
            using (MemoryStream ms = new MemoryStream(sizeof(uint) + msg.SerializedSize))
            {
                id = (uint)IPAddress.HostToNetworkOrder((int)id);
                ms.Write(BitConverter.GetBytes(id), 0, 4);
                msg.WriteTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 解码protobuf消息
        /// </summary>
        /// <param name="data">字节数据</param>
        /// <param name="t_query">消息(id->type)查询函数</param>
        /// <param name="msg">返回的消息object</param>
        /// <param name="id">返回的消息id</param>
        /// <returns></returns>
        public static bool Decode(
            byte[] data, PBChannel.MsgTypeQuery t_query,
            out IMessage msg, out uint id)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                byte[] id_bytes = new byte[4];
                ms.Read(id_bytes, 0, 4);

                id = BitConverter.ToUInt32(id_bytes, 0);
                id = (uint)IPAddress.NetworkToHostOrder((int)id);

                Type t = t_query(id);
                msg = null;
                if (null != t)
                {
                    msg = t.InvokeMember(
                        "ParseFrom",
                        BindingFlags.Public |
                        BindingFlags.Static |
                        BindingFlags.InvokeMethod,
                        null,
                        null,
                        new object[] { ms }) as IMessage;
                    return null != msg;
                }
            }
            return false;
        }
    }
}