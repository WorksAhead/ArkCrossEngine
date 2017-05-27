/**
 * @file   Connector.cs
 * @author carl <zhangnaisheng@cyou-inc.com>
 * @date   2013-04-01
 * 
 * @brief  Connector class, provide interface to send 
 *  message to other service on dcore
 * 
 */

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
