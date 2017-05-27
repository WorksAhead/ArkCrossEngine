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
    public class Component : Object
    {
        public Component()
        {

        }
        
        public GameObject gameObject
        {
            get { return ObjectFactory.Create<GameObject>(GetImpl<CrossEngineImpl.Component>().gameObject); }
        }

        public Component GetTypedComponent(ObjectType type)
        {
            CrossEngineImpl.GameObject gobject = GetImpl<CrossEngineImpl.Component>().gameObject;
            return ObjectFactory.Create<Component>(gobject.GetComponent(ObjectFactory.TypeToNative(type)));
        }

        private CrossEngineImpl.Component[] GetComponentsImpl(System.Type type, bool bGetComponentInChildren)
        {
            CrossEngineImpl.GameObject gobject = GetImpl<CrossEngineImpl.Component>().gameObject;
            if (gobject == null)
            {
                return null;
            }

            if (bGetComponentInChildren)
            {
                return gobject.GetComponents(type);
            }
            else
            {
                return gobject.GetComponentsInChildren(type);
            }
        }

        public Component[] GetTypedComponents(ObjectType type, bool bGetComponentInChildren = false)
        {
            CrossEngineImpl.GameObject gobject = GetImpl<CrossEngineImpl.Component>().gameObject;
            return ObjectFactory.Create<Component>(GetComponentsImpl(ObjectFactory.TypeToNative(type), bGetComponentInChildren));
        }
    }
}
