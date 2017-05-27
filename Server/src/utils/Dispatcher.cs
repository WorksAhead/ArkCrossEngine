using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Google.ProtocolBuffers;
using DashFire;

namespace DashFire
{
  delegate void ClientMsgHandler(object msg, User user);
  class Dispatcher
  {
    internal Dispatcher()
    {
      client_msg_handlers_ = new MyDictionary<Type, ClientMsgHandler>();
    }

    internal bool RegClientMsgHandler(Type msg, ClientMsgHandler handler)
    {
      if (client_msg_handlers_.ContainsKey(msg)) {
        return false;
      }
      client_msg_handlers_.Add(msg, handler);
      return true;
    }

    internal void HandleClientMsg(object msg, User user)
    {
      if (msg == null) {
        LogSys.Log(LOG_TYPE.ERROR, "{0}", "can't handle null msg");
        return;
      }
      Type msg_type = msg.GetType();
      ClientMsgHandler handler;
      if (!client_msg_handlers_.TryGetValue(msg_type, out handler))
      {
        if (client_default_handler_ != null) {
          client_default_handler_(msg, user);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, "{0}", "message no deal&default handler!");
        }
        return;
      }
      if (handler != null)
      {
        handler(msg, user);
      }
    }

    internal void SetClientDefaultHandler(ClientMsgHandler handler)
    {
      client_default_handler_ = handler;
    }

    private MyDictionary<Type, ClientMsgHandler> client_msg_handlers_;
    private ClientMsgHandler client_default_handler_;
  }
}
