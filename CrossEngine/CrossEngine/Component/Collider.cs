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
    public class Collider : Component
    {
        public Collider()
        {

        }
        
        public Transform transform
        {
            get { return ObjectFactory.Create<Transform>(GetImpl<CrossEngineImpl.Collider>().transform); }
        }

        public ArkCrossEngine.Vector3 ClosestPointOnBounds(ArkCrossEngine.Vector3 position)
        {
            return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Collider>().ClosestPointOnBounds(Helper.Vec3ToUnity(position)));
        }
    }
}
