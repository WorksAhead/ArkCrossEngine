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
    public class Texture : Object
    {
        public Texture()
        {

        }
    }

    public class Texture2D : Texture
    {
        public Texture2D()
        {

        }
    }
}
