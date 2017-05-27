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
    public class MeshFilter : Component
    {
        public MeshFilter()
        {

        }

        public Mesh mesh
        {
            get { return ObjectFactory.Create<Mesh>(GetImpl<CrossEngineImpl.MeshFilter>().mesh); }
            set
            {
                if (value != null)
                {
                    GetImpl<CrossEngineImpl.MeshFilter>().mesh = value.GetImpl<CrossEngineImpl.MeshFilter>().mesh;
                }
                else
                {
                    GetImpl<CrossEngineImpl.MeshFilter>().mesh = null;
                }
            }
        }
        public Mesh sharedMesh
        {
            get { return ObjectFactory.Create<Mesh>(GetImpl<CrossEngineImpl.MeshFilter>().sharedMesh); }
        }
    }
}
