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
    public class Terrain : Component
    {
        public Terrain()
        {

        }

        public static Terrain activeTerrain
        {
            get { return ObjectFactory.Create<Terrain>(CrossEngineImpl.Terrain.activeTerrain); }
        }
        public float SampleHeight(ArkCrossEngine.Vector3 worldPosition)
        {
            return GetImpl<CrossEngineImpl.Terrain>().SampleHeight(Helper.Vec3ToUnity(worldPosition));
        }
    }
}
