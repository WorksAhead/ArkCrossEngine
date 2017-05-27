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
    public class Resources
    {
        public Resources()
        {

        }

        public static Object Load(string resname)
        {
            CrossEngineImpl.Object obj = CrossEngineImpl.Resources.Load(resname);
            return ObjectFactory.Create(obj);
        }

        public static void UnloadUnusedAssets()
        {
            CrossEngineImpl.Resources.UnloadUnusedAssets();
        }
    }
}
