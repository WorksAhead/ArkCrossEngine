//#define USE_FOR_RELEASE

using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Text;
using ArkCrossEngine;

namespace LobbyRobot
{
    class GlobalInfo
    {
        public bool RequestLogin()
        {
            bool ret = false;
            if (Interlocked.Increment(ref m_LoginingCount) < 200)
            {
                ret = true;
            }
            else
            {
                Interlocked.Decrement(ref m_LoginingCount);
            }
            return ret;
        }
        public void FinishLogin()
        {
            Interlocked.Decrement(ref m_LoginingCount);
        }

        private int m_LoginingCount = 0;
        internal static GlobalInfo Instance
        {
            get { return s_Instance; }
        }
        private static GlobalInfo s_Instance = new GlobalInfo();
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("[Usage:]lobbyrobot robotGroup threadnum robotnumperthread gmscript.gm url");
                Console.WriteLine("Current args num: {0}", args.Length);
                return;
            }
            int robotGroup = int.Parse(args[0]);
            int threadNum = int.Parse(args[1]);
            int robotNum = int.Parse(args[2]);
            string wayPointScript = args[3];
            string url = args[4];
            string gmTxt = File.ReadAllText(wayPointScript);
            // file name as scene id
            string filename = Path.GetFileNameWithoutExtension(wayPointScript);
            int sceneId = int.Parse(filename);
            Console.WriteLine("====================================================");
            Console.WriteLine("robot group: {0}", robotGroup);
            Console.WriteLine("thread num: {0}", threadNum);
            Console.WriteLine("robot num per thread: {0}", robotNum);
            Console.WriteLine("gmScript {0}:", wayPointScript);
            Console.WriteLine("url: {0}", url);
            Console.WriteLine("====================================================");
            Console.WriteLine("{0}", gmTxt);
            Console.WriteLine("====================================================");
            Console.WriteLine("Load tables ...");
            LogSystem.OnOutput = (Log_Type type, string msg) =>
            {
                Console.WriteLine(msg);
            };

            FileReaderProxy.RegisterReadFileHandler((string filePath) =>
            {
                byte[] buffer = null;
                try
                {
                    buffer = File.ReadAllBytes(filePath);

                }
                catch (Exception e)
                {
                    LogSystem.Error("Exception:{0}\n{1}", e.Message, e.StackTrace);
                    return null;
                }
                return buffer;
            }, (string path) => { return File.Exists(path); });

            GlobalVariables.Instance.IsClient = false;
            string tmpPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            HomePath.CurHomePath = Path.Combine(tmpPath, "../DcoreEnv/bin");
            Console.WriteLine("home path:{0}", HomePath.CurHomePath);

            SceneConfigProvider.Instance.Load(FilePathDefine_Server.C_SceneConfig, "ScenesConfigs");
            ItemConfigProvider.Instance.Load(FilePathDefine_Server.C_ItemConfig, "ItemConfig");
            PlayerConfigProvider.Instance.LoadPlayerConfig(FilePathDefine_Server.C_PlayerConfig, "PlayerConfig");
            SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_SKILL, FilePathDefine_Server.C_SkillSystemConfig, "SkillConfig");
            SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_IMPACT, FilePathDefine_Server.C_ImpactSystemConfig, "ImpactConfig");
            Console.WriteLine("Startup robots ...");

            List<RobotThread> threads = new List<RobotThread>();
            for (int i = 0; i < threadNum; ++i)
            {
                RobotThread thread = new RobotThread();
                threads.Add(thread);

                thread.Start();
                for (int j = 0; j < robotNum; ++j)
                {
                    string user = string.Format("robot_{0}_{1}_{2}", robotGroup, i, j);
                    string pwd = "robot";
                    thread.QueueAction(thread.AddRobot, url, user, pwd, gmTxt, sceneId);
                }
            }
            Console.WriteLine("Enter infinite running ...");
            for (;;)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
