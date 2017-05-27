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
    public class Shader : Object
    {
        public Shader()
        {

        }
        
        public static Shader Find(string name)
        {
            return ObjectFactory.Create<Shader>(CrossEngineImpl.Shader.Find(name));
        }
    }
}
