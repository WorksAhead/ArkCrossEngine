using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Reflection;
using System.Net;

namespace SuperSocket.ClientEngine
{
    public delegate void ConnectedCallback(Socket socket, object state, SocketAsyncEventArgs e);
    public delegate void MyConnectedCallback(Socket socket, object state, MySocketAsyncArgs e);

    public static partial class ConnectAsyncExtension
    {
        class ConnectToken
        {
            public object State { get; set; }

            public ConnectedCallback Callback { get; set; }
        }       
    }
}
