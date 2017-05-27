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
    public class Input
    {
        public static ArkCrossEngine.Vector3 mousePosition
        {
            get { return Helper.Vec3FromUnity(CrossEngineImpl.Input.mousePosition); }
        }

        public static bool GetKeyDown(ArkCrossEngine.KeyCode kc)
        {
            return CrossEngineImpl.Input.GetKeyDown((CrossEngineImpl.KeyCode)(int)(ArkCrossEngine.KeyCode)(kc));
        }

        public static bool GetKeyUp(ArkCrossEngine.KeyCode kc)
        {
            return CrossEngineImpl.Input.GetKeyUp((CrossEngineImpl.KeyCode)(int)(ArkCrossEngine.KeyCode)(kc));
        }
    }
}
