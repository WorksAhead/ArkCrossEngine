using System;
using System.Threading;
using System.Collections.Generic;
using ArkCrossEngine;

namespace DashFire
{
  internal class SceneLoadThread : MyServerThread
  {
    protected override void OnStart()
    {      
      TickSleepTime = 10;
      LogSys.Log(LOG_TYPE.DEBUG, "scene load thread start.");
    }

    protected override void OnTick()
    {
      try {
        long curTime = TimeUtility.GetServerMilliseconds();
        if (m_LastLogTime + 60000 < curTime) {
          m_LastLogTime = curTime;

          DebugPoolCount((string msg) => {
            LogSys.Log(LOG_TYPE.INFO, "SceneLoadThread.ActionQueue {0}", msg);
          });
        }
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
      }      
    }

    protected override void OnQuit()
    {
      
    }

    private long m_LastLogTime = 0;

    internal static SceneLoadThread Instance
    {
      get
      {
        return s_Instance;
      }
    }
    private static SceneLoadThread s_Instance = new SceneLoadThread();
  }
}
