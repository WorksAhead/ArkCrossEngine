using System;
using System.Text;
using System.Diagnostics;
using CSharpCenterClient;

internal class Config
{
    public static uint DSRequestTimeout
    {
        get { return s_Instance.m_DSRequestTimeout; }
    }
    public static void Init ()
    {
        StringBuilder sb = new StringBuilder(256);
        if ( CenterClientApi.GetConfig("DSRequestTimeout", sb, 256) )
        {
            s_Instance.m_DSRequestTimeout = uint.Parse(sb.ToString());
        }
    }
    private uint m_DSRequestTimeout = 45000;
    private static Config s_Instance = new Config();
}

namespace DashFire.DataStore
{
    public static class Debug
    {
        public delegate void LoggerCB ( string log );
        public static event LoggerCB Logger;

        [Conditional("DEBUG")]
        internal static void Log ( string fmt, params object[] objs )
        {
            if ( null != Logger )
                Logger(string.Format("DSC.Debug# " + fmt, objs));
        }
    }
}