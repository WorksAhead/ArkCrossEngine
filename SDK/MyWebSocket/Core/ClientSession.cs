using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SuperSocket.ClientEngine
{
  public abstract class ClientSession : IClientSession, IBufferSetter
  {
    protected Socket Client { get; set; }

    protected EndPoint RemoteEndPoint { get; set; }

    public bool IsConnected { get; protected set; }

#if !__IOS__
    public bool NoDelay { get; set; }
#endif

    public ClientSession()
    {

    }

    public ClientSession(EndPoint remoteEndPoint)
    {
      if (remoteEndPoint == null)
        throw new ArgumentNullException("remoteEndPoint");

      RemoteEndPoint = remoteEndPoint;
    }

    public abstract void Connect();

    public abstract void Send(byte[] data, int offset, int length);

    public abstract void Send(IList<ArraySegment<byte>> segments);

    public abstract void Close();
    //==========================================================        
    public EventHandler Connected { get; set; }
    public EventHandler Closed { get; set; }
    public EventHandler<ErrorEventArgs> Error { get; set; }
    public EventHandler<DataEventArgs> DataReceived { get; set; }

    protected virtual void OnClosed()
    {
      IsConnected = false;

      var handler = Closed;

      if (handler != null)
        handler(this, EventArgs.Empty);
    }

    protected virtual void OnError(Exception e)
    {
      var handler = Error;
      if (handler == null)
        return;

      handler(this, new ErrorEventArgs(e));
    }

    protected virtual void OnConnected()
    {
      var client = Client;

#if !__IOS__
      if (client != null) {
        if (client.NoDelay != NoDelay)
          client.NoDelay = NoDelay;
      }
#endif
      IsConnected = true;

      var handler = Connected;
      if (handler == null)
        return;

      handler(this, EventArgs.Empty);
    }

    private DataEventArgs m_DataArgs = new DataEventArgs();

    protected virtual void OnDataReceived(byte[] data, int offset, int length)
    {
      var handler = DataReceived;
      if (handler == null)
        return;

      m_DataArgs.Data = data;
      m_DataArgs.Offset = offset;
      m_DataArgs.Length = length;

      handler(this, m_DataArgs);
    }

    public virtual int ReceiveBufferSize { get; set; }

    public IProxyConnector Proxy { get; set; }

    protected ArraySegment<byte> Buffer { get; set; }

    void IBufferSetter.SetBuffer(ArraySegment<byte> bufferSegment)
    {
      Buffer = bufferSegment;
    }
  }
}
