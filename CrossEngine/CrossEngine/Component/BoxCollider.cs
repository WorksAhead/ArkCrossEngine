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
    public class BoxCollider : Collider
    {
        public BoxCollider()
        {

        }

        public bool isTrigger
        {
            get { return GetImpl<CrossEngineImpl.BoxCollider>().isTrigger; }
            set { GetImpl<CrossEngineImpl.BoxCollider>().isTrigger = value; }
        }
    }
}
