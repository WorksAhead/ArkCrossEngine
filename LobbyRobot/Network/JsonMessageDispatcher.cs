using System;
using System.Text;
using LitJson;
using DashFire;

namespace DashFire.Network
{
  internal class JsonMessageHandlerInfo
  {
    internal Type m_ProtoType = null;
    internal JsonMessageHandlerDelegate m_Handler = null;
  }
  internal class JsonMessageDispatcher
  {
    internal void Init()
    {
      if (!m_Inited) {
        m_MessageHandlers = new JsonMessageHandlerInfo[(int)JsonMessageID.MaxNum];
        for (int i = (int)JsonMessageID.Zero; i < (int)JsonMessageID.MaxNum; ++i) {
          m_MessageHandlers[i] = new JsonMessageHandlerInfo();
        }
        m_Inited = true;
      }
    }

    internal bool Inited
    {
      get
      {
        return m_Inited;
      }
    }
    
    internal void RegisterMessageHandler(int id, Type protoType, JsonMessageHandlerDelegate handler)
    {
      if (m_Inited) {
        if (id >= (int)JsonMessageID.Zero && id < (int)JsonMessageID.MaxNum) {
          m_MessageHandlers[id].m_ProtoType = protoType;
          m_MessageHandlers[id].m_Handler = handler;
        }
      }
    }

    internal string BuildNodeMessage(JsonMessage msg)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(msg.m_ID);
      sb.Append('|');
      sb.Append(JsonMapper.ToJson(msg.m_JsonData));
      if (null != msg.m_ProtoData) {
        byte[] bytes = Encoding.Encode(msg.m_ProtoData);
        sb.Append('|');
        sb.Append(Convert.ToBase64String(bytes));
      }
      return sb.ToString();
    }

    internal unsafe void HandleNodeMessage(string msgStr)
    {
      if (m_Inited) {
        JsonMessage msg = DecodeJsonMessage(msgStr);
        if (null != msg) {
          HandleNodeMessage(msg);
        }
      }
    }

    private void HandleNodeMessage(JsonMessage msg)
    {
      if (m_Inited && msg != null) {
        JsonMessageHandlerDelegate handler = m_MessageHandlers[msg.m_ID].m_Handler;
        if (handler != null) {
          try {
            handler(msg);
          }
          catch (Exception ex) {
            LogSystem.Error("[Exception] HandleNodeMessage:{0} throw:{1}\n{2}", msg.m_ID, ex.Message, ex.StackTrace);
          }
        }
      }
    }

    private unsafe JsonMessage DecodeJsonMessage(string msgStr)
    {
      JsonMessage msg = null;
      if (m_Inited) {
        try {
          //LogSystem.Info("DecodeJsonMessage:{0}", msgStr);

          int ix = msgStr.IndexOf('|');
          if (ix > 0) {
            int id = int.Parse(msgStr.Substring(0, ix));
            int ix2 = msgStr.IndexOf('|', ix + 1);
            msg = new JsonMessage(id);
            if (ix2 > 0) {
              string jsonStr = msgStr.Substring(ix + 1, ix2 - ix - 1);
              string protoStr = msgStr.Substring(ix2 + 1);
              msg.m_JsonData = JsonMapper.ToObject(jsonStr);              
              Type t = m_MessageHandlers[id].m_ProtoType;
              if (null != t) {
                byte[] bytes = Convert.FromBase64String(protoStr);
                msg.m_ProtoData = Encoding.Decode(t, bytes);
              }
            } else {
              string jsonStr = msgStr.Substring(ix + 1);
              msg.m_JsonData = JsonMapper.ToObject(jsonStr);
            }
          }
        }
        catch (Exception ex) {
          LogSystem.Error("[Exception] DecodeJsonMessage:{0} throw:{1}\n{2}", msgStr, ex.Message, ex.StackTrace);
        }
      }
      return msg;
    }

    private ProtoNetEncoding Encoding
    {
      get
      {
        if (null == m_Encoding) {
          m_Encoding = new ProtoNetEncoding();
        }
        return m_Encoding;
      }
    }

    private bool m_Inited = false;
    private JsonMessageHandlerInfo[] m_MessageHandlers = null;
    private ProtoNetEncoding m_Encoding = null;
  }
}

