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
    public class Random
    {
        public static float Range(float min, float max)
        {
            return CrossEngineImpl.Random.Range(min, max);
        }
        public static int Range(int min, int max)
        {
            return CrossEngineImpl.Random.Range(min, max);
        }

        public CrossEngineImpl.Random RandomImpl = new CrossEngineImpl.Random();
    }
}
