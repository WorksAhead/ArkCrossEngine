using System;
using System.Text;
using Newtonsoft.Json;
using CSharpCenterClient;
using DashFire;
using ArkCrossEngine.Network;
using ArkCrossEngine;

namespace Lobby
{
    /// <summary>
    /// Json消息格式:
    /// ‘消息id’|json消息|base64编码的proto-buf数据串'\0'
    /// 
    /// 其中，proto-buf数据串为非必要部分
    /// 
    /// </summary>
    internal class JsonMessageHandlerInfo
    {
        internal Type m_Type = null;
        internal Type m_ProtoType = null;
        internal JsonMessageHandlerDelegate m_Handler = null;
    }
    internal class JsonMessageDispatcher
    {
        internal static void Init()
        {
            if (!s_Inited)
            {
                s_MessageHandlers = new JsonMessageHandlerInfo[(int)JsonMessageID.MaxNum];
                for (int i = (int)JsonMessageID.Zero; i < (int)JsonMessageID.MaxNum; ++i)
                {
                    s_MessageHandlers[i] = new JsonMessageHandlerInfo();
                }
                s_Inited = true;
            }
        }

        internal static bool Inited
        {
            get
            {
                return s_Inited;
            }
        }

        internal static void SetMessageFilter(JsonMessageFilterDelegate filter)
        {
            s_MessageFilter = filter;
        }

        internal static void RegisterMessageHandler(int id, Type type, Type protoType, JsonMessageHandlerDelegate handler)
        {
            if (s_Inited)
            {
                if (id >= (int)JsonMessageID.Zero && id < (int)JsonMessageID.MaxNum)
                {
                    s_MessageHandlers[id].m_Type = type;
                    s_MessageHandlers[id].m_ProtoType = protoType;
                    s_MessageHandlers[id].m_Handler = handler;
                }
            }
        }

        internal static unsafe void HandleDcoreMessage(
            uint seq,
            int source_handle,
            int dest_handle,
            byte[] data)
        {
            if (s_Inited)
            {
                JsonMessage msg = DecodeJsonMessage(data);
                if (null != msg)
                {
                    bool isContinue = true;
                    if (null != s_MessageFilter)
                    {
                        isContinue = s_MessageFilter(msg, source_handle, seq);
                    }
                    if (isContinue)
                    {
                        HandleDcoreMessage(msg, source_handle, seq);
                    }
                }
            }
        }

        internal static void HandleDcoreMessage(JsonMessage msg, int handle, uint session)
        {
            if (s_Inited && msg != null)
            {

                //LogSys.Log(LOG_TYPE.DEBUG, "Handle Json Message:{0}={1}", msg.m_ID, msg.GetType().Name);

                JsonMessageHandlerInfo info = s_MessageHandlers[(int)msg.m_ID];
                if (info != null && info.m_Handler != null)
                {
                    info.m_Handler(msg, handle, session);
                }
            }
        }

        internal static unsafe JsonMessage DecodeJsonMessage(byte[] data)
        {
            JsonMessage msg = null;
            if (s_Inited)
            {
                try
                {
                    string msgStr = System.Text.Encoding.UTF8.GetString(data);
                    int ix = msgStr.IndexOf('|');
                    if (ix > 0)
                    {
                        int id = int.Parse(msgStr.Substring(0, ix));
                        int ix2 = msgStr.IndexOf('|', ix + 1);
                        if (ix2 > 0)
                        {
                            string jsonStr = msgStr.Substring(ix + 1, ix2 - ix - 1);
                            string protoStr = msgStr.Substring(ix2 + 1);
                            int strLen = protoStr.Length;
                            Type type = GetMessageType(id);
                            msg = JsonConvert.DeserializeObject(jsonStr, type) as JsonMessage;
                            Type t = s_MessageHandlers[id].m_ProtoType;
                            if (null != t && strLen > 0)
                            {
                                byte[] bytes = null;
                                if (protoStr[strLen - 1] == '\0')
                                    bytes = Convert.FromBase64String(protoStr.Substring(0, strLen - 1));
                                else
                                    bytes = Convert.FromBase64String(protoStr);
                                msg.m_ProtoData = Encoding.Decode(t, bytes);
                            }
                        }
                        else
                        {
                            string jsonStr = msgStr.Substring(ix + 1);
                            int strLen = jsonStr.Length;
                            Type type = GetMessageType(id);
                            if (strLen > 0 && jsonStr[strLen - 1] == '\0')
                                msg = JsonConvert.DeserializeObject(jsonStr.Substring(0, strLen - 1), type) as JsonMessage;
                            else
                                msg = JsonConvert.DeserializeObject(jsonStr, type) as JsonMessage;
                        }
                        if (msg != null)
                            msg.m_ID = id;
                    }
                }
                catch (Exception ex)
                {
                    LogSys.Log(LOG_TYPE.ERROR, "[Exception] DecodeJsonMessage:{0}\n{1}", ex.Message, ex.StackTrace);
                }
            }
            return msg;
        }

        internal static Type GetMessageType(int id)
        {
            Type type = null;
            if (id >= (int)JsonMessageID.Zero && id < (int)JsonMessageID.MaxNum)
            {
                type = s_MessageHandlers[id].m_Type;
            }
            return type;
        }

        internal static Type GetMessageProtoType(int id)
        {
            Type type = null;
            if (id >= (int)JsonMessageID.Zero && id < (int)JsonMessageID.MaxNum)
            {
                type = s_MessageHandlers[id].m_ProtoType;
            }
            return type;
        }

        internal static void SendDcoreMessage(int handle, JsonMessage msg)
        {
            if (s_Inited)
            {
                byte[] data = BuildCoreMessage(msg);
                CenterClientApi.SendByHandle(handle, data, data.Length);
            }
        }

        internal static void SendDcoreMessage(string name, JsonMessage msg)
        {
            if (s_Inited)
            {
                byte[] data = BuildCoreMessage(msg);
                CenterClientApi.SendByName(name, data, data.Length);
            }
        }

        private static byte[] BuildCoreMessage(JsonMessage msg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(msg.m_ID);
            sb.Append('|');
            sb.Append(JsonConvert.SerializeObject(msg));
            if (null != msg.m_ProtoData)
            {
                byte[] bytes = Encoding.Encode(msg.m_ProtoData);
                sb.Append('|');
                sb.Append(Convert.ToBase64String(bytes));
            }
            sb.Append('\0');
            byte[] data = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return data;
        }

        private static ProtoNetEncoding Encoding
        {
            get
            {
                if (null == s_Encoding)
                {
                    s_Encoding = new ProtoNetEncoding();
                }
                return s_Encoding;
            }
        }

        private static bool s_Inited = false;
        private static JsonMessageFilterDelegate s_MessageFilter = null;
        private static JsonMessageHandlerInfo[] s_MessageHandlers = null;
        [ThreadStatic]
        private static ProtoNetEncoding s_Encoding = null;
    }
}

