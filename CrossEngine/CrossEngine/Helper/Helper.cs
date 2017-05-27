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
    public static class Helper
    {
        public static CrossEngineImpl.Vector2 Vec2ToUnity(ArkCrossEngine.Vector2 v)
        {
            return new CrossEngineImpl.Vector2(v.x, v.y);
        }
        public static ArkCrossEngine.Vector2 Vec2FromUnity(CrossEngineImpl.Vector2 v)
        {
            return new ArkCrossEngine.Vector2(v.x, v.y);
        }

        public static CrossEngineImpl.Vector3 Vec3ToUnity(ArkCrossEngine.Vector3 v)
        {
            return new CrossEngineImpl.Vector3(v.x, v.y, v.z);
        }
        public static ArkCrossEngine.Vector3 Vec3FromUnity(CrossEngineImpl.Vector3 v)
        {
            return new ArkCrossEngine.Vector3(v.x, v.y, v.z);
        }

        public static CrossEngineImpl.Color ColorToUnity(ArkCrossEngine.Color color)
        {
            return new CrossEngineImpl.Color(color.r, color.g, color.b);
        }
        public static ArkCrossEngine.Color ColorFromUnity(CrossEngineImpl.Color color)
        {
            return new ArkCrossEngine.Color(color.r, color.g, color.b);
        }

        public static CrossEngineImpl.Color32 Color32ToUnity(ArkCrossEngine.Color32 color)
        {
            return new CrossEngineImpl.Color32(color.r, color.g, color.b, color.a);
        }
        public static ArkCrossEngine.Color32 Color32FromUnity(CrossEngineImpl.Color32 color)
        {
            return new ArkCrossEngine.Color32(color.r, color.g, color.b, color.a);
        }

        public static CrossEngineImpl.Quaternion QuatToUnity(ArkCrossEngine.Quaternion q)
        {
            return new CrossEngineImpl.Quaternion(q.x, q.y, q.z, q.w);
        }
        public static ArkCrossEngine.Quaternion QuatFromUnity(CrossEngineImpl.Quaternion q)
        {
            return new ArkCrossEngine.Quaternion(q.x, q.y, q.z, q.w);
        }

        public static CrossEngineImpl.Ray RayToUnity(ArkCrossEngine.Ray ray)
        {
            return new CrossEngineImpl.Ray(Vec3ToUnity(ray.origin), Vec3ToUnity(ray.direction));
        }
        public static ArkCrossEngine.Ray RayFromUnity(CrossEngineImpl.Ray ray)
        {
            return new ArkCrossEngine.Ray(Vec3FromUnity(ray.origin), Vec3FromUnity(ray.direction));
        }

        public static ArkCrossEngine.RaycastHit RayCastHitFromUnity(CrossEngineImpl.RaycastHit hit)
        {
            ArkCrossEngine.RaycastHit retHit = new ArkCrossEngine.RaycastHit();
            retHit.normal = Vec3FromUnity(hit.normal);
            retHit.point = Vec3FromUnity(hit.point);
            return retHit;
        }
    }
}
