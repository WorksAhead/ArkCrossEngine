using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ascii2Utf8
{
    class Program
    {
        static void Main(string[] args)
        {
            int defaultCodePage = Encoding.Default.CodePage;
            Console.WriteLine("defaultCodePage:{0}", defaultCodePage);
            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("[Usage:] ascii2utf8 source_dir target_dir");
                Console.WriteLine(" or");
                Console.WriteLine(" ascii2utf8 source_dir target_dir isserver");
            }
            else
            {
                string sourceDir = args[0];
                string targetDir = args[1];
                bool isServer = false;
                if (args.Length == 3)
                {
                    isServer = (0 == string.Compare(args[2], "isserver", true));
                }
                try
                {
                    CopyFolder(sourceDir, targetDir, isServer);
                    Console.WriteLine("files copy finished");
                    ConvertToUTF8(targetDir);
                    Console.WriteLine("files convert finished");
                    EncodeTxt(targetDir);
                    Console.WriteLine("txt encode finished");
                    EncodeDsl(targetDir);
                    Console.WriteLine("dsl encode finished");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static void CopyFolder(string from, string to, bool isServer)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);

            string clientPath = Path.Combine(from, "Client");
            string serverPath = Path.Combine(from, "Server");
            
            foreach (string sub in Directory.GetDirectories(from))
            {
                if (isServer)
                {
                    if (0 == string.Compare(sub, clientPath, true))
                        continue;
                }
                else
                {
                    if (0 == string.Compare(sub, serverPath, true))
                        continue;
                }
                CopyFolder(sub, Path.Combine(to, Path.GetFileName(sub)));
            }
            
            foreach (string file in Directory.GetFiles(from))
                File.Copy(file, Path.Combine(to, Path.GetFileName(file)), true);
        }

        private static void CopyFolder(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);
            
            foreach (string sub in Directory.GetDirectories(from))
            {
                CopyFolder(sub, Path.Combine(to, Path.GetFileName(sub)));
            }
            
            foreach (string file in Directory.GetFiles(from))
                File.Copy(file, Path.Combine(to, Path.GetFileName(file)), true);
        }

        private static void ConvertToUTF8(string dir)
        {
            Encoding ansi = Encoding.GetEncoding(936);
            List<string> destfiles = new List<string>();
            foreach (string filter in filters.Split(','))
            {
                foreach (var eachfileinfo in new DirectoryInfo(dir).GetFiles(filter, SearchOption.AllDirectories))
                {
                    destfiles.Add(eachfileinfo.FullName);
                }
            }
            foreach (string destfile in destfiles)
            {
                File.WriteAllText(destfile, File.ReadAllText(destfile, ansi), Encoding.UTF8);
            }
        }
        private static void EncodeTxt(string dir)
        {

        }

        private static void EncodeDsl(string dir)
        {

        }

        private const string filters = "*.txt,*.log";
    }
}
