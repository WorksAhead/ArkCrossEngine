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
    public class LayerMask
    {
        public static int NameToLayer(string layerName)
        {
            return CrossEngineImpl.LayerMask.NameToLayer(layerName);
        }
    }
}
