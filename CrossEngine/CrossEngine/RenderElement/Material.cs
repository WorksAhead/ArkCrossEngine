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
    public class Material : Object
    {
        public Material()
        {

        }

        public static Material Create(Shader shader)
        {
            return ObjectFactory.Create<Material>(shader.GetImpl<CrossEngineImpl.Shader>());
        }

        public ArkCrossEngine.Color32 color
        {
            get { return Helper.Color32FromUnity(GetImpl<CrossEngineImpl.Material>().color); }
            set { GetImpl<CrossEngineImpl.Material>().color = Helper.Color32ToUnity(value); }
        }
        public Shader shader
        {
            get { return ObjectFactory.Create<Shader>(GetImpl<CrossEngineImpl.Material>().shader); }
            set
            {
                if (value != null)
                {
                    GetImpl<CrossEngineImpl.Material>().shader = value.GetImpl<CrossEngineImpl.Shader>();
                }
                else
                {
                    GetImpl<CrossEngineImpl.Material>().shader = null;
                }
            }
        }
        public Texture mainTexture
        {
            get { return ObjectFactory.Create<Texture>(GetImpl<CrossEngineImpl.Material>().mainTexture); }
            set
            {
                if (value != null)
                {
                    GetImpl<CrossEngineImpl.Material>().mainTexture = value.GetImpl<CrossEngineImpl.Texture>();
                }
                else
                {
                    GetImpl<CrossEngineImpl.Material>().mainTexture = null;
                }
            }
        }

        public void SetColor(int nameID, ArkCrossEngine.Color color)
        {
            GetImpl<CrossEngineImpl.Material>().SetColor(nameID, Helper.ColorToUnity(color));
        }
        public void SetColor(string propertyName, ArkCrossEngine.Color color)
        {
            GetImpl<CrossEngineImpl.Material>().SetColor(propertyName, Helper.ColorToUnity(color));
        }
        public void SetFloat(string propertyName, float value)
        {
            GetImpl<CrossEngineImpl.Material>().SetFloat(propertyName, value);
        }
    }
}
