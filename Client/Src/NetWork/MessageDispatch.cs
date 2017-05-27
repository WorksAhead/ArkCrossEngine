using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace ArkCrossEngine.Network
{
    class MessageDispatch
    {
        internal delegate void MsgHandler(object msg, NetConnection user);
        MyDictionary<Type, MsgHandler> m_DicHandler = new MyDictionary<Type, MsgHandler>();
        internal void RegisterHandler(Type t, MsgHandler handler)
        {
            m_DicHandler[t] = handler;
        }
        internal bool Dispatch(object msg, NetConnection conn)
        {
            MsgHandler msghandler;
            if (m_DicHandler.TryGetValue(msg.GetType(), out msghandler))
            {
                //Type[] param = new Type[] { msg.GetType() };
                //object[] param = new object[] { msg, conn };
                msghandler.Invoke(msg, conn);
                return true;
            }
            return false;
        }
    }
}
