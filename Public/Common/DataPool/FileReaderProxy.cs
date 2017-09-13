using System;
using System.Collections.Generic;
using System.IO;

namespace ArkCrossEngine
{
    public delegate void callback_readFile(byte[] bytes);

    public delegate System.Collections.IEnumerator delegate_readFileCoroutine(string path, callback_readFile callback);
    public delegate byte[] delegate_readFile(string path);
    public delegate bool delegate_fileExists(string path);
    
    public static class FileReaderProxy
    {
        private static delegate_readFile handlerReadFile;
        private static delegate_readFileCoroutine handlerReadFileCoroutine;
        private static delegate_fileExists handlerFileExists;

        public static int preloadCoroutine(string file, callback_readFile callback)
        {
            CoroutineObject obj = new CoroutineObject();
            return CoroutineManager.Instance.StartSingle(handlerReadFileCoroutine(file, delegate (byte[] bytes) {
                callback(bytes);
                obj.RetObject = true;
            }), obj);
        }

        public static int preloadTable(string file)
        {
            CoroutineObject obj = new CoroutineObject();
            return CoroutineManager.Instance.StartSingle(handlerReadFileCoroutine(file, delegate (byte[] bytes)
            {
                file = Path.GetFullPath(file).ToLower();
                byte[] o;
                if (!PreloadedTables.TryGetValue(file, out o))
                {
                    PreloadedTables.Add(file, bytes);
                }
                obj.RetObject = true;
            }), obj);
        }

        public static MemoryStream ReadFileAsMemoryStream(string filePath, byte[] buffer = null)
        {
            try
            {
                if (buffer == null)
                {
                    buffer = ReadFileAsArray(filePath);
                }
                
                if (buffer == null)
                {
                    LogSystem.Debug("Err ReadFileAsMemoryStream failed:{0}\n", filePath);
                    return null;
                }
                return new MemoryStream(buffer);
            }
            catch (Exception e)
            {
                LogSystem.Debug("Exception:{0}\n", e.Message);
                CrossEngineHelper.LogCallStack();
                return null;
            }
        }

        public static byte[] ReadFileAsArray(string filePath)
        {
            byte[] buffer = null;
            try
            {
                // try preloaded tables first
                filePath = Path.GetFullPath(filePath).ToLower();
                byte[] bytes;
                if (PreloadedTables.TryGetValue(filePath, out bytes))
                {
                    return bytes;
                }
                else
                {
                    if (handlerReadFile != null)
                    {
                        buffer = handlerReadFile(filePath);
                    }
                    else
                    {
                        LogSystem.Debug("ReadFileByEngine handler have not register: {0}", filePath);
                    }
                }
            }
            catch (Exception e)
            {
                LogSystem.Debug("Exception:{0}\n", e.Message);
                CrossEngineHelper.LogCallStack();
                return null;
            }
            return buffer;
        }

        public static bool Exists(string filePath)
        {
            try
            {
                if (handlerFileExists != null)
                {
                    return handlerFileExists(filePath);
                }
                else
                {
                    LogSystem.Debug("ReadFileByEngine handler have not register: {0}", filePath);
                    return false;
                }
            }
            catch (Exception e)
            {
                LogSystem.Debug("Exception:{0}\n", e.Message);
                CrossEngineHelper.LogCallStack();
                return false;
            }
        }

        public static void RegisterReadFileHandler(delegate_readFile hReadFile, delegate_fileExists hExists)
        {
            handlerReadFile = hReadFile;
            handlerFileExists = hExists;
        }

        public static void RegisterReadFileCoroutineHandler(delegate_readFileCoroutine hReadFile, delegate_fileExists hExists)
        {
            handlerReadFileCoroutine = hReadFile;
            handlerFileExists = hExists;
        }

        public static bool IsAllHandlerRegistered()
        {
            return (handlerReadFile != null || handlerReadFileCoroutine != null) && (handlerFileExists != null);
        }

        private static Dictionary<string, byte[]> PreloadedTables = new Dictionary<string, byte[]>();
    }
}
