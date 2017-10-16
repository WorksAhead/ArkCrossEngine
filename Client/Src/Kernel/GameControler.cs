using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ArkCrossEngine.Network;
using UnityEngine.Profiling;

namespace ArkCrossEngine
{
    /**
     * @brief 游戏控制器
     */
    public sealed class GameControler
    {
        internal sealed class LogicLogger : IDisposable
        {
            internal void Log(string format, params object[] args)
            {
                if (m_LogStream == null)
                {
                    return;
                }
                string msg = string.Format(format, args);
#if USE_DISK_LOG
                if (m_LogStream != null)
                {
                    m_LogStream.WriteLine(msg);
                    m_LogStream.Flush();
                }
#else
                m_LogQueue.Enqueue(msg);
                if (!m_IsInRequestFlush && m_LogQueue.Count >= c_FlushCount)
                {
                    m_LastFlushTime = TimeUtility.GetLocalMilliseconds();

                    RequestFlush();
                }
#endif
            }
            internal void Init(string logPath)
            {
                try
                {
                    string logFile = string.Format("{0}/Game_{1}.log", logPath, DateTime.Now.ToString("yyyy-MM-dd"));
                    m_LogStream = new StreamWriter(logFile, true);
#if !USE_DISK_LOG
                    m_LogQueue = m_LogQueues[m_CurQueueIndex];
                    m_Thread.OnQuitEvent = OnThreadQuit;
                    m_Thread.Start();
#endif
                    Log("======GameLog Start ({0}, {1})======", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
                }
                catch(Exception ex)
                {
                    m_LogStream = null;
                }
            }
            public void Dispose()
            {
                Release();
            }
            internal void Tick()
            {
#if !USE_DISK_LOG
                long curTime = TimeUtility.GetLocalMilliseconds();
                if (m_LastFlushTime + 10000 < curTime)
                {
                    m_LastFlushTime = curTime;

                    RequestFlush();
                }
#endif
            }
            private void Release()
            {
#if !USE_DISK_LOG
                m_Thread.Stop();
#endif
                if (m_LogStream != null)
                {
                    m_LogStream.Close();
                    m_LogStream.Dispose();
                }  
            }
#if !USE_DISK_LOG
            private void RequestFlush()
            {
                m_IsInRequestFlush = true;
                lock (m_LogQueueLock)
                {
                    m_Thread.QueueActionWithDelegation((MyAction<Queue<string>>)FlushToFile, m_LogQueue);
                    m_CurQueueIndex = 1 - m_CurQueueIndex;
                    m_LogQueue = m_LogQueues[m_CurQueueIndex];
                }
                m_IsInRequestFlush = false;
            }
            private void OnThreadQuit()
            {
                FlushToFile(m_LogQueue);
            }
            private void FlushToFile(Queue<string> logQueue)
            {
                lock (m_LogQueueLock)
                {
                    while (logQueue.Count > 0)
                    {
                        string msg = logQueue.Dequeue();
                        if (m_LogStream != null)
                            m_LogStream.WriteLine(msg);
                    }
                    if (m_LogStream != null)
                        m_LogStream.Flush();
                }
            }

            private Queue<string>[] m_LogQueues = new Queue<string>[] { new Queue<string>(), new Queue<string>() };
            private MyClientThread m_Thread = new MyClientThread();
            private int m_CurQueueIndex = 0;
            private Queue<string> m_LogQueue;
            private object m_LogQueueLock = new object();

            private long m_LastFlushTime = 0;
            private const int c_FlushCount = 4096;
            private bool m_IsInRequestFlush = false;
#endif
            private StreamWriter m_LogStream;
        }
        //----------------------------------------------------------------------
        // 标准接口
        //----------------------------------------------------------------------
        public static bool IsInited
        {
            get { return s_IsInited; }
        }
        public static void Init(string logPath, string dataPath)
        {
            s_IsInited = true;
            s_LogicLogger.Init(logPath);
            HomePath.CurHomePath = dataPath;
            GlobalVariables.Instance.IsDebug = false;

            FileReaderProxy.MakeSureAllHandlerRegistered();

            LogSystem.OnOutput = (Log_Type type, string msg) =>
            {
                s_LogicLogger.Log("{0}", msg);
            };

            // GfxSystem
            GfxSystem.Init();
            GfxSystem.SetLogicInvoker(s_LogicThread);
            GfxSystem.SetLogicLogCallback((bool isError, string format, object[] args) =>
            {
                if (isError)
                    LogSystem.Error(format, args);
                else
                    LogSystem.Info(format, args);
            });

            LogicSystem.LogFromGfx("GameControler.Init");

            GfxSystem.SetGameLogicNotification(GameLogicNotification.Instance);
            GfxModule.Skill.GfxSkillSystem.Instance.Init();
        }
        public static void InitLogic()
        {
            LogicSystem.LogFromGfx("GameControler.InitLogic");

            EntityManager.Instance.Init();
            WorldSystem.Instance.Init();

            ArkProfiler.Start("WorldSystem.Instance.LoadData");
            WorldSystem.Instance.LoadData();
            ArkProfiler.Stop("WorldSystem.Instance.LoadData");
            
            GfxModule.Impact.GfxImpactSystem.Instance.Init();

            ClientStorySystem.Instance.Init();
            GmCommands.ClientGmStorySystem.Instance.Init();

            PlayerControl.Instance.Init();
            LobbyNetworkSystem.Instance.Init(s_LogicThread);
            NetworkSystem.Instance.Init();
            AiViewManager.Instance.Init();
            SceneLogicViewManager.Instance.Init();
            ImpactViewManager.Instance.Init();
        }
        public static void StartLogic()
        {
            LogicSystem.LogFromGfx("GameControler.StartLogic");
            s_LogicThread.Start();
        }
        public static void PauseLogic(bool isPause)
        {
            s_IsPaused = isPause;
        }
        public static void StopLogic()
        {
            LogicSystem.LogFromGfx("GameControler.StopLogic");
            s_LogicThread.Stop();
            LobbyNetworkSystem.Instance.QuitClient();
            NetworkSystem.Instance.QuitClient();
        }
        public static void Release()
        {
            LogicSystem.LogFromGfx("GameControler.Release");

            WorldSystem.Instance.Release();
            EntityManager.Instance.Release();
            NetworkSystem.Instance.Release();
            GfxSystem.Release();
            s_LogicLogger.Dispose();
        }
        public static void TickGame()
        {
            try
            {
#if PLATFORM_WEBGL
                DummyThread.Thread.TickAllThread();
#endif
                Profiler.BeginSample("GameController.TickGame");
                //这里是在渲染线程执行的tick，逻辑线程的tick在GameLogicThread.cs文件里执行。
                GfxSystem.Tick();
                GfxModule.Skill.GfxSkillSystem.Instance.Tick();
                GfxModule.Impact.GfxImpactSystem.Instance.Tick();
            }
            finally
            {
                Profiler.EndSample();
            }
        }
        internal static void ChangeScene(int sceneId)
        {
            LogSystem.Debug("GameControler.ChangeScene {0}", sceneId);
            WorldSystem.Instance.ChangeScene(sceneId);
        }
        internal static void NotifyRoomServerDisconnected()
        {
            s_LogicThread.QueueAction(WorldSystem.Instance.OnRoomServerDisconnected);
        }
        internal static void NotifyRoomServerConnected()
        {
            s_LogicThread.QueueAction(WorldSystem.Instance.OnRoomServerConnected);
        }

        internal static LogicLogger LogicLoggerInstance
        {
            get
            {
                return s_LogicLogger;
            }
        }

        internal static bool IsPaused
        {
            get
            {
                return s_IsPaused;
            }
        }

        private static LogicLogger s_LogicLogger = new LogicLogger();
        private static GameLogicThread s_LogicThread = new GameLogicThread();
        private static bool s_IsInited = false;
        private static bool s_IsPaused = false;
    }
}

