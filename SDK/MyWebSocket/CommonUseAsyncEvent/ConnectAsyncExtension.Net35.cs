using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public static partial class ConnectAsyncExtension
    {
        public static void ConnectAsync(this EndPoint remoteEndPoint, ConnectedCallback callback, object state)
        {
            ConnectAsyncInternal(remoteEndPoint, callback, state);
        }

        static partial void CreateAttempSocket(DnsConnectState connectState)
        {
            connectState.Socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
