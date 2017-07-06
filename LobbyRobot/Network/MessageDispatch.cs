using System;
using System.Collections.Generic;
using System.Reflection;
using Lidgren.Network;

namespace ArkCrossEngine.Network
{
  class MessageDispatch
  {
    internal delegate void MsgHandler(object msg, NetConnection conn, NetworkSystem networkSystem);
    MyDictionary<Type, MsgHandler> m_DicHandler = new MyDictionary<Type, MsgHandler>();
    internal void RegisterHandler(Type t, MsgHandler handler)
    {
      m_DicHandler[t] = handler;
    }
    internal bool Dispatch(object msg, NetConnection conn, NetworkSystem networkSystem)
    {
      MsgHandler msghandler;
      if (m_DicHandler.TryGetValue(msg.GetType(), out msghandler))
      {
        msghandler(msg, conn, networkSystem);
        return true;
      }
      return false;
    }
  }
}
