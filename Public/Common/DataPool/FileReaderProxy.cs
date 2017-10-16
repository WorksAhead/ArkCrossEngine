using System;
using System.IO;

namespace ArkCrossEngine
{
    public delegate byte[] delegate_ReadFile(string path);
    public delegate bool delegate_FileExists(string path);

    public static class FileReaderProxy
    {
        private static delegate_ReadFile handlerReadFile;
        private static delegate_FileExists handlerFileExists;

        public static MemoryStream ReadFileAsMemoryStream(string filePath)
        {
            try
            {
                byte[] buffer = ReadFileAsArray(filePath);
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
                if (handlerReadFile != null)
                {
                    buffer = handlerReadFile(filePath);
                }
                else
                {
                    LogSystem.Debug("ReadFileByEngine handler have not register: {0}", filePath);
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

        public static void RegisterReadFileHandler(delegate_ReadFile hReadFile, delegate_FileExists hExists)
        {
            handlerReadFile = hReadFile;
            handlerFileExists = hExists;
        }

        public static void MakeSureAllHandlerRegistered()
        {
            if (handlerReadFile == null)
            {
                handlerReadFile = DefaultReadFileProxy;
            }
            if (handlerFileExists == null)
            {
                handlerFileExists = DefaultFileExistsProxy;
            }
        }

        private static byte[] DefaultReadFileProxy(string filePath)
        {
            try
            {
                byte[] buffer = null;
                buffer = File.ReadAllBytes(filePath);
                return buffer;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static bool DefaultFileExistsProxy(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
