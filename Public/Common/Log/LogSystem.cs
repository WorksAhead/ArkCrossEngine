
/**
 * @file LogSystem.cs
 * @brief 日志系统
 *          需要重构，暂时使用不同的接口分离多线程访问
 */

namespace ArkCrossEngine
{
    /**
     * @brief 日志类型
     */
    public enum Log_Type
    {
        LT_Debug,
        LT_Info,
        LT_Warn,
        LT_Error,
        LT_Assert,
    }
    public delegate void LogSystemOutputDelegation(Log_Type type, string msg);
    /**
     * @brief 日志系统
     */
    public class LogSystem
    {
        public static LogSystemOutputDelegation OnOutput;
        public static LogSystemOutputDelegation OnOutput2;

        public static void Debug(string format, params object[] args)
        {
            string str = string.Format("[Debug]:" + format, args);
            Output(Log_Type.LT_Debug, str);
        }
        public static void Info(string format, params object[] args)
        {
            string str = string.Format("[Info]:" + format, args);
            Output(Log_Type.LT_Info, str);
        }
        public static void Warn(string format, params object[] args)
        {
            string str = string.Format("[Warn]:" + format, args);
            Output(Log_Type.LT_Warn, str);
        }
        public static void Error(string format, params object[] args)
        {
            string str = string.Format("[Error]:" + format, args);
            Output(Log_Type.LT_Error, str);
        }
        public static void Assert(bool check, string format, params object[] args)
        {
            if (!check)
            {
                string str = string.Format("[Assert]:" + format, args);
                Output(Log_Type.LT_Assert, str);
            }
        }

        public static void GfxLog(Log_Type type, string format, params object[] args)
        {
            string str;
            switch (type)
            {
                case Log_Type.LT_Info:
                    str = string.Format("[Info]:" + format, args);
                    break;
                case Log_Type.LT_Debug:
                    str = string.Format("[Debug]:" + format, args);
                    break;
                case Log_Type.LT_Warn:
                    str = string.Format("[Warn]:" + format, args);
                    break;
                case Log_Type.LT_Assert:
                    str = string.Format("[Assert]:" + format, args);
                    break;
                case Log_Type.LT_Error:
                    str = string.Format("[Error]:" + format, args);
                    break;
                default:
                    str = string.Format("[Unknown]:" + format, args);
                    break;
            }
            OutputGfx(type, str);
        }

        private static void Output(Log_Type type, string msg)
        {
            if (null != OnOutput)
            {
                OnOutput(type, msg);
            }
            if (null != OnOutput2)
            {
                OnOutput2(type, msg);
            }
        }

        private static void OutputGfx(Log_Type type, string msg)
        {
            if (null != OnOutput2)
            {
                OnOutput2(type, msg);
            }
        }
    }
}
