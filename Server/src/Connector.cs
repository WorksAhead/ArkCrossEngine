using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CSharpCenterClient;
using Messenger;
using Google.ProtocolBuffers;

namespace DashFire
{
    internal class Connector
    {
        internal Connector(PBChannel channel)
        {
            channel_ = channel;
        }

        internal void SendMsgToLobby(IMessage msg)
        {
            channel_.Send(msg);
        }

        private PBChannel channel_;
    }
}  // namespace dashfire
