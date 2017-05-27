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
    public class RenderSettings : Object
    {
        public RenderSettings()
        {

        }

        public static Material skybox
        {
            get { return ObjectFactory.Create<Material>(CrossEngineImpl.RenderSettings.skybox); }
            set
            {
                if (value != null)
                {
                    CrossEngineImpl.RenderSettings.skybox = value.GetImpl<CrossEngineImpl.Material>();
                }
                else
                {
                    CrossEngineImpl.RenderSettings.skybox = null;
                }
            }
        }
    }
}
