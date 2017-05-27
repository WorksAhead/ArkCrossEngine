using System.ComponentModel;

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
    public class Physics
    {
        public static bool Raycast(ArkCrossEngine.Ray ray, out ArkCrossEngine.RaycastHit hit)
        {
            CrossEngineImpl.RaycastHit uhit = new CrossEngineImpl.RaycastHit();
            bool castResult = CrossEngineImpl.Physics.Raycast(Helper.RayToUnity(ray), out uhit);
            hit = Helper.RayCastHitFromUnity(uhit);
            return castResult;
        }
        public static bool Raycast(ArkCrossEngine.Vector3 origin, ArkCrossEngine.Vector3 direction, out ArkCrossEngine.RaycastHit hitInfo, int layerMask)
        {
            CrossEngineImpl.RaycastHit uhit = new CrossEngineImpl.RaycastHit();
            bool castResult = CrossEngineImpl.Physics.Raycast(Helper.Vec3ToUnity(origin), Helper.Vec3ToUnity(direction), out uhit, layerMask);
            hitInfo = Helper.RayCastHitFromUnity(uhit);
            return castResult;
        }
        public static bool Raycast(ArkCrossEngine.Vector3 origin, ArkCrossEngine.Vector3 direction, out ArkCrossEngine.RaycastHit hitInfo, float distance, int layerMask)
        {
            CrossEngineImpl.RaycastHit uhit = new CrossEngineImpl.RaycastHit();
            bool castResult = CrossEngineImpl.Physics.Raycast(Helper.Vec3ToUnity(origin), Helper.Vec3ToUnity(direction), out uhit, distance, layerMask);
            hitInfo = Helper.RayCastHitFromUnity(uhit);
            return castResult;
        }
        public static ArkCrossEngine.RaycastHit[] RaycastAll(ArkCrossEngine.Ray ray, float distance, int layerMask)
        {
            CrossEngineImpl.RaycastHit[] hits = CrossEngineImpl.Physics.RaycastAll(Helper.RayToUnity(ray), distance, layerMask);
            ArkCrossEngine.RaycastHit[] retHits = new ArkCrossEngine.RaycastHit[hits.Length];
            for (int i = 0; i < hits.Length; ++i)
            {
                retHits[i] = Helper.RayCastHitFromUnity(hits[i]);
            }
            return retHits;
        }
        public static bool Linecast(ArkCrossEngine.Vector3 start, ArkCrossEngine.Vector3 end, out ArkCrossEngine.RaycastHit hitInfo, int layerMask)
        {
            CrossEngineImpl.RaycastHit uhit = new CrossEngineImpl.RaycastHit();
            bool castResult = CrossEngineImpl.Physics.Linecast(Helper.Vec3ToUnity(start), Helper.Vec3ToUnity(end), out uhit, layerMask);
            hitInfo = Helper.RayCastHitFromUnity(uhit);
            return castResult;
        }
        public static Collider[] OverlapSphere(ArkCrossEngine.Vector3 position, float radius, int layerMask)
        {
            CrossEngineImpl.Collider[] unityColliders = CrossEngineImpl.Physics.OverlapSphere(Helper.Vec3ToUnity(position), radius, layerMask);
            return ObjectFactory.Create<Collider>(unityColliders);
        }
    }
}
