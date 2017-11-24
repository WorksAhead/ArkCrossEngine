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
        class DnsConnectState
        {
            public IPAddress[] Addresses { get; set; }

            public int NextAddressIndex { get; set; }

            public int Port { get; set; }

            public Socket Socket4 { get; set; }

            public Socket Socket6 { get; set; }

            public object State { get; set; }

            public MyConnectedCallback Callback { get; set; }
        }

        private static IPAddress GetNextAddress(DnsConnectState state, out Socket attempSocket)
        {
            IPAddress address = null;
            attempSocket = null;

            var currentIndex = state.NextAddressIndex;

            while (attempSocket == null)
            {
                if (currentIndex >= state.Addresses.Length)
                    return null;

                address = state.Addresses[currentIndex++];

                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    attempSocket = state.Socket6;
                }
                else if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    attempSocket = state.Socket4;
                }
            }

            state.NextAddressIndex = currentIndex;
            return address;
        }

        static partial void CreateAttempSocket(DnsConnectState connectState);

        //异步创建网络连接
        private static void MyConnectAsyncInternal(this EndPoint remoteEndPoint, MyConnectedCallback callback, object state)
        {
            if (remoteEndPoint is DnsEndPoint)
            {
                var dnsEndPoint = (DnsEndPoint)remoteEndPoint;

                var asyncResult = Dns.BeginGetHostAddresses(dnsEndPoint.Host, MyOnGetHostAddresses,
                    new DnsConnectState
                    {
                        Port = dnsEndPoint.Port,
                        Callback = callback,
                        State = state
                    });

                if (asyncResult.CompletedSynchronously)
                    MyOnGetHostAddresses(asyncResult);
            }
            else
            {
                var e = MyCreateSocketAsyncArgs(remoteEndPoint, callback, state);
                var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                e.AcceptSocket = socket;
                socket.BeginConnect(remoteEndPoint, new AsyncCallback(MyConnectionCallback), e);
            }
        }
        //创建自定义连接参数
        static MySocketAsyncArgs MyCreateSocketAsyncArgs(EndPoint remoteEndPoint, MyConnectedCallback callback, object state)
        {
            var e = new MySocketAsyncArgs();
            e.State = state;
            e.Callback = callback;
            e.RemoteEndPoint = remoteEndPoint;
            return e;
        }
        //异步创建连接的回调方法
        private static void MyConnectionCallback(IAsyncResult ar)
        {
            MySocketAsyncArgs args = (MySocketAsyncArgs)ar.AsyncState;
            try
            {
                args.AcceptSocket.EndConnect(ar);
                //args.AcceptSocket.Blocking = true;
                args.AcceptSocket.SendTimeout = 10;
                args.AcceptSocket.ReceiveTimeout = 10;
                args.SocketError = SocketError.Success;
                //连接成功，主动调用原事件            
                args.Callback(args.AcceptSocket, args.State, args);
            }
            catch (System.Exception ex)
            {
                // 错误处理
                if (ex.GetType() == typeof(SocketException))
                {
                    if (((SocketException)ex).SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        //连接被服务器拒绝
                        throw new Exception("socket error because of connection refused.");
                    }
                    else
                    {
                        //连接丢失
                        throw new Exception("socket error: " + ex.Message);
                    }
                }
                if (args.AcceptSocket.Connected)
                {
                    args.AcceptSocket.Shutdown(SocketShutdown.Receive);
                    args.AcceptSocket.Close(0);
                }
                else
                {
                    args.AcceptSocket.Close();
                }
            }
        }
        private static void MyOnGetHostAddresses(IAsyncResult result)
        {
            var connectState = result.AsyncState as DnsConnectState;

            IPAddress[] addresses;

            try
            {
                addresses = Dns.EndGetHostAddresses(result);
            }
            catch
            {
                connectState.Callback(null, connectState.State, null);
                return;
            }

            if (addresses == null || addresses.Length <= 0)
            {
                connectState.Callback(null, connectState.State, null);
                return;
            }

            connectState.Addresses = addresses;

            CreateAttempSocket(connectState);

            Socket attempSocket;

            var address = GetNextAddress(connectState, out attempSocket);

            if (address == null)
            {
                connectState.Callback(null, connectState.State, null);
                return;
            }

            var ipEndPoint = new IPEndPoint(address, connectState.Port);
            var e = MyCreateSocketAsyncArgs(ipEndPoint, connectState.Callback, null);
            var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            e.AcceptSocket = socket;
            socket.BeginConnect(ipEndPoint, new AsyncCallback(MyConnectionCallback), e);
        }
    }
}
