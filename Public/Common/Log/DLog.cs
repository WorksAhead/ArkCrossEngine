using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    namespace Debug
    {
        public static class DLog
        {
            static DLog()
            {
                log_fs_ = new MyDictionary<string, FileStream>();
            }

            public static void _(string key, string format, params object[] args)
            {
                if (!enable_) return;

                FileStream fs = LogFS(key);
                string log = string.Format(Now + " -> " + format + "\n", args);
                byte[] bytes = Encoding.UTF8.GetBytes(log);
                fs.Write(bytes, 0, Encoding.UTF8.GetByteCount(log));
                fs.Flush();
            }

            public static void _(string key, IFormatProvider provider, string format, params object[] args)
            {
                if (!enable_) return;

                FileStream fs = LogFS(key);
                string log = string.Format(Now + " -> " + format + "\n", args);
                byte[] bytes = Encoding.UTF8.GetBytes(log);
                fs.Write(bytes, 0, Encoding.UTF8.GetByteCount(log));
                fs.Flush();
            }

            private static long Now
            {
                get
                {
                    return (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                }
            }

            private static FileStream LogFS(string key)
            {
                FileStream fs;
                if (!log_fs_.TryGetValue(key, out fs))
                {
                    string t = DateTime.UtcNow.ToString("HH.mm.ss.fffffff");
                    string path = HomePath.GetAbsolutePath(string.Format("{0}_{1}.log", key, t));
                    fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                    log_fs_.Add(key, fs);
                }
                return fs;
            }

            private static MyDictionary<string, FileStream> log_fs_;
            private static bool enable_ = false;
        }

    }
}