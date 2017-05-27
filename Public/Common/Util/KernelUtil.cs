using System;
using System.Reflection;

namespace ArkCrossEngine
{
    public class KernelUtil
    {
        public static T LoadClassByName<T>(string assemblyPath, string assemblyName, string className, object[] args)
        {
            Assembly assembly = GetAssembly(assemblyPath, assemblyName, className);
            if (assembly == null)
            {
                LogSystem.Assert(false, "KernelUtil.LoadClassByName Assembly load failed!");
                return default(T);
            }

            T obj = (T)(assembly.CreateInstance(className));
            return obj;
        }

        public static T InvokeMethodByName<T>(string assemblyPath, string assemblyName, string className,
            string methodName, object[] args)
        {
            Assembly assembly = GetAssembly(assemblyPath, assemblyName, className);
            if (assembly == null)
            {
                LogSystem.Assert(false, "KernelUtil.LoadClassByName Assembly load failed!");
                return default(T);
            }

            Type objType = assembly.GetType(className);
            MethodInfo method = objType.GetMethod(methodName);

            //method.Invoke(null, args);
            //return default(T);

            T retObj = (T)(method.Invoke(null, args));
            return retObj;
        }

        public static Assembly GetAssembly(string assemblyPath, string assemblyName, string className)
        {
            Assembly assembly = null;

            if (CrossEngineHelper.StringIsNullOrEmpty(assemblyPath))
            {
                if (CrossEngineHelper.StringIsNullOrEmpty(assemblyName))
                {
                    assembly = Assembly.GetExecutingAssembly();
                }
                else
                {
                    assembly = Assembly.Load(assemblyName);
                }
            }
            else
            {
                assembly = Assembly.LoadFile(string.Format("[0][1].dll", assemblyPath, assemblyName));
            }

            if (assembly == null)
            {
                LogSystem.Debug("Warn: KernelUtil.LoadClassByName Assembly load failed!");
            }

            return assembly;
        }

    }
}
