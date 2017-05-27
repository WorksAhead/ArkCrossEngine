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
    public class CharacterController : Collider
    {
        public CharacterController()
        {

        }
        
        public bool enabled
        {
            get { return GetImpl<CrossEngineImpl.CharacterController>().enabled; }
            set { GetImpl<CrossEngineImpl.CharacterController>().enabled = value; }
        }
        public ArkCrossEngine.CollisionFlags collisionFlags
        {
            get
            {
                return (ArkCrossEngine.CollisionFlags)(int)(CrossEngineImpl.CollisionFlags)GetImpl<CrossEngineImpl.CharacterController>().collisionFlags;
            }
        }
        public bool isGrounded
        {
            get { return GetImpl<CrossEngineImpl.CharacterController>().isGrounded; }
        }
        public float radius
        {
            get { return GetImpl<CrossEngineImpl.CharacterController>().radius; }
        }

        public ArkCrossEngine.CollisionFlags Move(ArkCrossEngine.Vector3 motion)
        {
            CrossEngineImpl.CollisionFlags flag = GetImpl<CrossEngineImpl.CharacterController>().Move(Helper.Vec3ToUnity(motion));
            return (ArkCrossEngine.CollisionFlags)(int)flag;
        }
    }
}
