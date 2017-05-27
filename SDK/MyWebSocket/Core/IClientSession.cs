using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public interface IClientSession
    {
        IProxyConnector Proxy { get; set; }

        int ReceiveBufferSize { get; set; }

        bool IsConnected { get; }

        void Connect();

        void Send(byte[] data, int offset, int length);

        void Send(IList<ArraySegment<byte>> segments);

        void Close();

        EventHandler Connected { get; set; }

        EventHandler Closed { get; set; }

        EventHandler<ErrorEventArgs> Error { get; set; }

        EventHandler<DataEventArgs> DataReceived { get; set; }
    }
}
