using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using CSharpCenterClient;
using Messenger;
using DashFire;
using ArkCrossEngine;

class DataStore
{
    private void Init(string[] args)
    {
        m_NameHandleCallback = this.OnNameHandleChanged;
        m_MsgCallback = this.OnMessage;
        m_CmdCallback = this.OnCommand;
        m_MsgResultCallback = this.OnMessageResult;
        CenterClientApi.Init("store", args.Length, args, m_NameHandleCallback, m_MsgCallback, m_MsgResultCallback, m_CmdCallback);

        m_Channel = new PBChannel(DashFire.DataStore.MessageMapping.Query,
                      DashFire.DataStore.MessageMapping.Query);
        m_Channel.DefaultServiceName = "Lobby";
        LogSys.Init("./config/logconfig.xml");
        DataStoreConfig.Init();

        ArkCrossEngine.GlobalVariables.Instance.IsClient = false;

        FileReaderProxy.RegisterReadFileHandler((string filePath) =>
        {
            byte[] buffer = null;
            try
            {
                buffer = File.ReadAllBytes(filePath);
            }
            catch (Exception e)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception:{0}\n{1}", e.Message, e.StackTrace);
                return null;
            }
            return buffer;
        }, (string filepath) => { return File.Exists(filepath); });
        LogSystem.OnOutput += (Log_Type type, string msg) =>
        {
            switch (type)
            {
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

        DbThreadManager.Instance.Init(DataStoreConfig.LoadThreadNum, DataStoreConfig.SaveThreadNum);
        DataOpSystem.Instance.Init(m_Channel);
        DataCacheSystem.Instance.Init();
        LogSys.Log(LOG_TYPE.INFO, "DataStore initialized");
    }
    private void Loop()
    {
        try
        {
            while (CenterClientApi.IsRun())
            {
                CenterClientApi.Tick();
                Thread.Sleep(10);
                if (m_WaitQuit && PersistentSystem.Instance.LastSaveFinished)
                {
                    DataOpSystem.Instance.Enable = false;
                    LogSys.Log(LOG_TYPE.MONITOR, "DataStore quit.");
                    CenterClientApi.Quit();
                }
            }
        }
        catch (Exception ex)
        {
            LogSys.Log(LOG_TYPE.ERROR, "DataStore.Loop throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    private void Release()
    {
        LogSys.Log(LOG_TYPE.INFO, "DataStore release");
        DataCacheSystem.Instance.Stop();
        DbThreadManager.Instance.Stop();
        CenterClientApi.Release();
        Thread.Sleep(3000);
        //简单点，kill掉自己
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    private void OnNameHandleChanged(bool addOrUpdate, string name, int handle)
    {
        try
        {
            m_Channel.OnUpdateNameHandle(addOrUpdate, name, handle);
        }
        catch (Exception ex)
        {
            LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    private void OnMessageResult(uint seq, int src, int dest, int result)
    {

    }

    private void OnCommand(int src, int dest, string command)
    {
        try
        {
            if (0 == command.CompareTo("QuitDataStore"))
            {
                LogSys.Log(LOG_TYPE.MONITOR, "receive {0} command, save data and then quitting ...", command);
                if (!m_WaitQuit)
                {
                    DataCacheSystem.Instance.QueueAction(DataCacheSystem.Instance.DoLastSave);
                    m_WaitQuit = true;
                }
            }
        }
        catch (Exception ex)
        {
            LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    private void OnMessage(uint seq, int source_handle, int dest_handle,
        IntPtr data, int len)
    {
        try
        {
            byte[] bytes = new byte[len];
            Marshal.Copy(data, bytes, 0, len);
            m_Channel.Dispatch(source_handle, seq, bytes);
        }
        catch (Exception ex)
        {
            LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }

    private bool m_WaitQuit = false;

    private PBChannel m_Channel = null;
    private CenterClientApi.HandleNameHandleChangedCallback m_NameHandleCallback = null;
    private CenterClientApi.HandleMessageCallback m_MsgCallback = null;
    private CenterClientApi.HandleCommandCallback m_CmdCallback = null;
    private CenterClientApi.HandleMessageResultCallback m_MsgResultCallback = null;

    internal static void Main(string[] args)
    {
        DataStore svr = new DataStore();
        svr.Init(args);
        svr.Loop();
        svr.Release();
    }
}
