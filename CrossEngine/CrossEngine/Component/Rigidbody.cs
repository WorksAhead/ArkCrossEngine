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
    public class Rigidbody : Component
    {
        public Rigidbody()
        {

        }

        public ArkCrossEngine.Vector3 velocity
        {
            get { return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Rigidbody>().velocity); }
            set { GetImpl<CrossEngineImpl.Rigidbody>().velocity = Helper.Vec3ToUnity(value); }
        }
        public bool useGravity
        {
            get { return GetImpl<CrossEngineImpl.Rigidbody>().useGravity; }
            set { GetImpl<CrossEngineImpl.Rigidbody>().useGravity = value; }
        }

        public void AddForce(ArkCrossEngine.Vector3 force)
        {
            GetImpl<CrossEngineImpl.Rigidbody>().AddForce(Helper.Vec3ToUnity(force));
        }
    }
}
