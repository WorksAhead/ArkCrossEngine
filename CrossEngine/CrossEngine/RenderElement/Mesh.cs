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
    public class Mesh : Object
    {
        public Mesh()
        {

        }

        public static Mesh Create()
        {
            return ObjectFactory.Create<Mesh>(new CrossEngineImpl.Mesh());
        }

        public ArkCrossEngine.Vector3[] vertices
        {
            get
            {
                if (GetImpl<CrossEngineImpl.Mesh>().vertices.Length > 0)
                {
                    ArkCrossEngine.Vector3[] verts = new ArkCrossEngine.Vector3[GetImpl<CrossEngineImpl.Mesh>().vertices.Length];
                    for (int i = 0; i < verts.Length; ++i)
                    {
                        verts[i] = Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Mesh>().vertices[i]);
                    }
                    return verts;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.Length > 0 )
                {
                    CrossEngineImpl.Vector3[] verts = new CrossEngineImpl.Vector3[value.Length];
                    for (int i = 0; i < verts.Length; ++i)
                    {
                        verts[i] = Helper.Vec3ToUnity(value[i]);
                    }
                    GetImpl<CrossEngineImpl.Mesh>().vertices = verts;
                }
                else
                {
                    GetImpl<CrossEngineImpl.Mesh>().vertices = null;
                }
                
                
            }
        }
        public ArkCrossEngine.Vector2[] uv
        {
            get
            {
                if (GetImpl<CrossEngineImpl.Mesh>().uv.Length > 0)
                {
                    ArkCrossEngine.Vector2[] verts = new ArkCrossEngine.Vector2[GetImpl<CrossEngineImpl.Mesh>().uv.Length];
                    for (int i = 0; i < verts.Length; ++i)
                    {
                        verts[i] = Helper.Vec2FromUnity(GetImpl<CrossEngineImpl.Mesh>().uv[i]);
                    }
                    return verts;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.Length > 0)
                {
                    CrossEngineImpl.Vector2[] verts = new CrossEngineImpl.Vector2[value.Length];
                    for (int i = 0; i < verts.Length; ++i)
                    {
                        verts[i] = Helper.Vec2ToUnity(value[i]);
                    }
                    GetImpl<CrossEngineImpl.Mesh>().uv = verts;
                }
                else
                {
                    GetImpl<CrossEngineImpl.Mesh>().uv = null;
                }
            }
        }
        public int[] triangles
        {
            get
            {
                if (GetImpl<CrossEngineImpl.Mesh>().triangles.Length > 0)
                {
                    int[] verts = new int[GetImpl<CrossEngineImpl.Mesh>().triangles.Length];
                    for (int i = 0; i < verts.Length; ++i)
                    {
                        verts[i] = GetImpl<CrossEngineImpl.Mesh>().triangles[i];
                    }
                    return verts;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.Length > 0)
                {
                    int[] verts = new int[value.Length];
                    for (int i = 0; i < verts.Length; ++i)
                    {
                        verts[i] = value[i];
                    }
                    GetImpl<CrossEngineImpl.Mesh>().triangles = verts;
                }
                else
                {
                    GetImpl<CrossEngineImpl.Mesh>().triangles = null;
                }
            }
        }

        public void Optimize()
        {
            // no mesh.optimize equals in unity5
        }
    }
}
