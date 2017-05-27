#region NameSpaceImplDecl
    #if UNITY_IMPL
        using CrossEngineImpl = UnityEngine;
    #elif UNREAL_IMPL
        using CrossEngineImpl = UnrealEngine;
    #else
        using CrossEngineImpl = ArkCrossEngine;
    #endif
#endregion

namespace ArkCrossEngine
{
    public class Profiler
    {
        public static void BeginSample(string name)
        {
            CrossEngineImpl.Profiling.Profiler.BeginSample(name);
        }

        public static void EndSample()
        {
            CrossEngineImpl.Profiling.Profiler.EndSample();
        }
    }
}
