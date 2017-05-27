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
    public class Renderer : Component
    {
        public Renderer()
        {

        }

        public bool enabled
        {
            get { return GetImpl<CrossEngineImpl.Renderer>().enabled; }
            set { GetImpl<CrossEngineImpl.Renderer>().enabled = value; }
        }
        public Material material
        {
            get { return ObjectFactory.Create<Material>(GetImpl<CrossEngineImpl.Renderer>().material); }
            set
            {
                if (value != null)
                {
                    GetImpl<CrossEngineImpl.Renderer>().material = value.GetImpl<CrossEngineImpl.Material>();
                }
                else
                {
                    GetImpl<CrossEngineImpl.Renderer>().material = null;
                }
            }
        }
        public Material[] materials
        {
            get
            {
                return ObjectFactory.Create<Material>(GetImpl<CrossEngineImpl.Renderer>().materials);
            }
            set
            {
                if (value != null && value.Length > 0)
                {
                    CrossEngineImpl.Material[] unityMaterial = new CrossEngineImpl.Material[value.Length];
                    for (int i = 0; i < unityMaterial.Length; ++i)
                    {
                        unityMaterial[i] = value[i].GetImpl<CrossEngineImpl.Material>();
                    }
                    GetImpl<CrossEngineImpl.Renderer>().materials = unityMaterial;
                }
                else
                {
                    GetImpl<CrossEngineImpl.Renderer>().materials = null;
                }
            }
        }
    }
}
