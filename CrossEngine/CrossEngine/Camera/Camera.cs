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
    public class Camera : Component
    {
        public Camera()
        {

        }

        public static Camera main
        {
            get
            {
                return ObjectFactory.Create<Camera>(CrossEngineImpl.Camera.main);
            }
        }

        public int cullingMask
        {
            get { return GetImpl<CrossEngineImpl.Camera>().cullingMask; }
            set { GetImpl<CrossEngineImpl.Camera>().cullingMask = value; }
        }
        public ArkCrossEngine.CameraClearFlags clearFlags
        {
            get
            {
                return (ArkCrossEngine.CameraClearFlags)(int)(CrossEngineImpl.CameraClearFlags)(GetImpl<CrossEngineImpl.Camera>().clearFlags);
            }
            set
            {
                GetImpl<CrossEngineImpl.Camera>().clearFlags = (CrossEngineImpl.CameraClearFlags)(int)(ArkCrossEngine.CameraClearFlags)(value);
            }
        }
        public float depth
        {
            get { return GetImpl<CrossEngineImpl.Camera>().depth; }
            set { GetImpl<CrossEngineImpl.Camera>().depth = value; }
        }
        public Transform transform
        {
            get { return ObjectFactory.Create<Transform>(GetImpl<CrossEngineImpl.Camera>().transform); }
        }
        public float fieldOfView
        {
            get { return GetImpl<CrossEngineImpl.Camera>().fieldOfView; }
            set { GetImpl<CrossEngineImpl.Camera>().fieldOfView = value; }
        }

        public void CopyFrom(Camera other)
        {
            GetImpl<CrossEngineImpl.Camera>().CopyFrom(other.GetImpl<CrossEngineImpl.Camera>());
        }

        public ArkCrossEngine.Ray ScreenPointToRay(ArkCrossEngine.Vector3 p)
        {
            return Helper.RayFromUnity(GetImpl<CrossEngineImpl.Camera>().ScreenPointToRay(Helper.Vec3ToUnity(p)));
        }

        public static Camera StaticMainCamera;
    }
}
