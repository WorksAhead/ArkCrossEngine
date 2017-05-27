using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  internal sealed class AiViewManager
  {
    internal void Init()
    {
      m_AiViews.Add(new AiView_NpcGeneral());
      m_AiViews.Add(new AiView_UserGeneral());
      m_AiViews.Add(new AiView_LogicUtility());
    }
    private ArrayList m_AiViews = new ArrayList();
    internal static AiViewManager Instance
    {
      get { return s_Instance; }
    }
    private static AiViewManager s_Instance = new AiViewManager();
  }
}
