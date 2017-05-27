using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.ClientEngine
{
    public interface IProxyConnector
    {
        void Connect(EndPoint remoteEndPoint);

      EventHandler<ProxyEventArgs> Completed { get; set; }
    }
}
