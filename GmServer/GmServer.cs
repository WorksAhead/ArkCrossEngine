using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using CSharpCenterClient;
using Messenger;
using DashFire;
using Lobby_GmServer;
using ArkCrossEngine;

namespace GmServer
{
  internal sealed class GmServer
  {
    private PBChannel m_Channel = null;
    private CenterClientApi.HandleNameHandleChangedCallback m_NameHandleCallback = null;
    private CenterClientApi.HandleMessageCallback m_MsgCallback = null;
    private CenterClientApi.HandleCommandCallback m_CmdCallback = null;
    private ServerAsyncActionProcessor m_ActionQueue = new ServerAsyncActionProcessor();
    private static GmServer s_Instance = new GmServer();
    internal static GmServer Instance
    {
      get { return s_Instance; }
    }
    ///====================================================================================================
    /// 供db线程调度主线程函数用，用于需要在主线程里执行的函数的调用（通常不需要）
    ///====================================================================================================
    internal ServerAsyncActionProcessor ActionQueue
    {
      get { return m_ActionQueue; }
    }
    ///====================================================================================================
    /// 主模块基础功能
    ///====================================================================================================
    internal static void Main(string[] args)
    {
      s_Instance.Init(args);
      s_Instance.Loop();
      s_Instance.Release();
    }
    private void Init(string[] args)
    {
      m_NameHandleCallback = this.OnNameHandleChanged;
      m_MsgCallback = this.OnMessage;
      m_CmdCallback = this.OnCommand;
      CenterClientApi.Init("gmserver", args.Length, args, m_NameHandleCallback, m_MsgCallback, m_CmdCallback);
      m_Channel = new PBChannel(DashFire.DataStore.MessageMapping.Query, DashFire.DataStore.MessageMapping.Query);
      m_Channel.DefaultServiceName = "Lobby";
      LogSys.Init("./config/logconfig.xml");
      GmServerConfig.Init();

      GlobalVariables.Instance.IsClient = false;

      FileReaderProxy.RegisterReadFileHandler((string filePath) => {
        byte[] buffer = null;
        try {
          buffer = File.ReadAllBytes(filePath);
        } catch (Exception e) {
          LogSys.Log(LOG_TYPE.ERROR, "Exception:{0}\n{1}", e.Message, e.StackTrace);
          return null;
        }
        return buffer;
      }, (string filepath) => { return File.Exists(filepath); });
      LogSystem.OnOutput += (Log_Type type, string msg) => {
        switch (type) {
          case Log_Type.LT_Debug:
            LogSys.Log(LOG_TYPE.DEBUG, msg);
            break;
          case Log_Type.LT_Info:
            LogSys.Log(LOG_TYPE.INFO, msg);
            break;
          case Log_Type.LT_Warn:
            LogSys.Log(LOG_TYPE.WARN, msg);
            break;
          case Log_Type.LT_Error:
          case Log_Type.LT_Assert:
            LogSys.Log(LOG_TYPE.ERROR, msg);
            break;
        }
      };

      DbThreadManager.Instance.Init(GmServerConfig.LoadThreadNum, GmServerConfig.SaveThreadNum);
      DataScheduler.Instance.Init(m_Channel);
      DataOperator.Instance.Init();
      LogSys.Log(LOG_TYPE.INFO, "GmServer initialized");
    }
    private void Loop()
    {
      try {
        while (CenterClientApi.IsRun()) {
          CenterClientApi.Tick();
          Thread.Sleep(10);
        }
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "GmServer.Loop throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
      }
    }
    private void Release()
    {
      LogSys.Log(LOG_TYPE.INFO, "GmServer release");
      DbThreadManager.Instance.Stop();
      CenterClientApi.Release();
      Thread.Sleep(3000);
      System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    private void OnNameHandleChanged(bool addOrUpdate, string name, int handle)
    {
      try {
        m_Channel.OnUpdateNameHandle(addOrUpdate, name, handle);
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
      }
    }
    private void OnCommand(int src, int dest, string command)
    {
    }
    private void OnMessage(uint seq, int source_handle, int dest_handle,
        IntPtr data, int len)
    {
      try {
        byte[] bytes = new byte[len];
        Marshal.Copy(data, bytes, 0, len);
        m_Channel.Dispatch(source_handle, seq, bytes);
      } catch (Exception ex) {
        LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
      }
    }
  }
}
