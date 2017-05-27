using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobby
{
  internal class NodeInfo
  {
    internal string NodeName
    {
      get { return m_NodeName; }
      set { m_NodeName = value; }
    }

    private string m_NodeName = "";
  }
}
