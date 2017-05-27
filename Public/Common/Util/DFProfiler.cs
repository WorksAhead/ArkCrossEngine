using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public delegate void EventOutput(string format, params object[] args);
    public delegate void EventOutput2(string msg);
    public class ArkProfiler
    {
        class Profiler
        {
            internal string m_Name = string.Empty;
            internal double m_Elapsed = 0;
            private Stopwatch m_Stopwatch = null;
            internal Profiler(string name)
            {
                m_Name = name;
                m_Elapsed = 0;
                m_Stopwatch = new Stopwatch();
            }
            internal void Start()
            {
                m_Elapsed = 0;
                m_Stopwatch.Start();
            }
            internal void Stop()
            {
                m_Elapsed = m_Stopwatch.Elapsed.TotalMilliseconds;
                m_Stopwatch.Stop();
                m_Stopwatch.Reset();
            }
            internal bool IsRunning()
            {
                return m_Stopwatch.IsRunning;
            }
        }

        static EventOutput s_HandOutput;
        static EventOutput2 s_HandOutput2;
        static bool s_ProfilerEnable = true;

        static Dictionary<string, Profiler> m_ProfilerDict = new Dictionary<string, Profiler>();
        public static void RegisterOutput(EventOutput handler)
        {
            s_HandOutput = handler;
        }
        public static void RegisterOutput2(EventOutput2 handler)
        {
            s_HandOutput2 = handler;
        }
        public static void Start(string name)
        {
            try
            {
                if (!s_ProfilerEnable)
                {
                    return;
                }
                if (string.IsNullOrEmpty(name))
                {
                    Output("[Warn]:DFProfiler.Profiler name empty.");
                    return;
                }
                Profiler profiler = null;
                if (m_ProfilerDict.ContainsKey(name))
                {
                    profiler = m_ProfilerDict[name];
                    if (profiler.IsRunning())
                    {
                        Output("[Profiler][Warn]:DFProfiler.Profiler:{0} not Stop.", profiler.m_Name);
                        m_ProfilerDict.Remove(name);
                        return;
                    }
                }
                else
                {
                    profiler = new Profiler(name);
                    m_ProfilerDict.Add(name, profiler);
                }
                profiler.Start();
            }
            catch (Exception ex)
            {
                LogSystem.Error("DFProfiler.Start throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        public static void Stop(string name)
        {
            try
            {
                if (!s_ProfilerEnable)
                {
                    return;
                }
                if (string.IsNullOrEmpty(name))
                {
                    string info = string.Format("[Profiler][Warn]:DFProfiler.Profiler name empty.");
                    Output(info);
                    return;
                }
                Profiler profiler = null;
                if (m_ProfilerDict.ContainsKey(name))
                {
                    profiler = m_ProfilerDict[name];
                    if (profiler.IsRunning())
                    {
                        profiler.Stop();
                        OnProfilerStop(profiler);
                    }
                    else
                    {
                        Output("[Profiler][Warn]:DFProfiler.Profiler:{0} not Start.", profiler.m_Name);
                        m_ProfilerDict.Remove(name);
                        return;
                    }
                }
                else
                {
                    Output("[Profiler][Warn]:DFProfiler.Profiler:{0} not Start.", name);
                    return;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("DFProfiler.Stop throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private static void OnProfilerStop(Profiler profiler)
        {
            try
            {
                Output("[Profiler]:{0} Elapsed:{1}.", profiler.m_Name, profiler.m_Elapsed);
                Output2(string.Format("[Profiler]:{0} Elapsed:{1}.", profiler.m_Name, profiler.m_Elapsed));
                m_ProfilerDict.Remove(profiler.m_Name);
            }
            catch (Exception ex)
            {
                LogSystem.Error("DFProfiler.OnProfilerStop throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private static void Output(string format, params object[] args)
        {
            if (s_HandOutput != null)
            {
                s_HandOutput(format, args);
            }
        }
        private static void Output2(string msg)
        {
            if (s_HandOutput2 != null)
            {
                s_HandOutput2(msg);
            }
        }
    }
}

