
#region NameSpaceImplDecl
#if UNITY_IMPL
using System;
using CrossEngineImpl = UnityEngine;
#elif UNREAL_IMPL
        using CrossEngineImpl = UnrealEngine;
#else
        using CrossEngineImpl = ArkCrossEngine;
#endif
#endregion

namespace ArkCrossEngine
{
    public class GameObject : Object
    {
        public static GameObject Create()
        {
            CrossEngineImpl.GameObject gameObject = new CrossEngineImpl.GameObject();
            return ObjectFactory.Create<GameObject>(gameObject);
        }

        public static GameObject Find(string name)
        {
            CrossEngineImpl.GameObject gameObject = CrossEngineImpl.GameObject.Find(name);
            return ObjectFactory.Create<GameObject>(gameObject);
        }

        public static GameObject[] FindGameObjectsWithTag(string name)
        {
            CrossEngineImpl.GameObject[] objects = CrossEngineImpl.GameObject.FindGameObjectsWithTag(name);
            return ObjectFactory.Create<GameObject>(objects);
        }

        public bool activeSelf
        {
            get { return GetImpl<CrossEngineImpl.GameObject>().activeSelf; }
        }
        public Transform transform
        {
            get { return ObjectFactory.Create<Transform>(GetImpl<CrossEngineImpl.GameObject>().transform); }
        }
        public AudioSource audio
        {
            get { return ObjectFactory.Create<AudioSource>(GetImpl<CrossEngineImpl.GameObject>().GetComponent<CrossEngineImpl.AudioSource>()); }
        }
        public Rigidbody rigidbody
        {
            get { return ObjectFactory.Create<Rigidbody>(GetImpl<CrossEngineImpl.GameObject>().GetComponent<CrossEngineImpl.Rigidbody>()); }
        }
        public GameObject gameObject
        {
            get { return this; }
        }
        public int layer
        {
            get { return GetImpl<CrossEngineImpl.GameObject>().layer; }
            set { GetImpl<CrossEngineImpl.GameObject>().layer = value; }
        }
        public Animation animation
        {
            get { return ObjectFactory.Create<Animation>(GetImpl<CrossEngineImpl.GameObject>().GetComponent<CrossEngineImpl.Animation>()); }
        }
        public Collider collider
        {
            get { return ObjectFactory.Create<Collider>(GetImpl<CrossEngineImpl.GameObject>().GetComponent<CrossEngineImpl.Collider>()); }
        }

        public static GameObject CreatePrimitive(ArkCrossEngine.PrimitiveType type)
        {
            CrossEngineImpl.GameObject gameObject = CrossEngineImpl.GameObject.CreatePrimitive((CrossEngineImpl.PrimitiveType)(int)(ArkCrossEngine.PrimitiveType)(type));
            return ObjectFactory.Create<GameObject>(gameObject);
        }

        public static Object Instantiate(Object obj)
        {
            try
            {
                CrossEngineImpl.Object o = CrossEngineImpl.GameObject.Instantiate(obj.GetImpl<CrossEngineImpl.Object>());
                return ObjectFactory.Create(o);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("CrossEngine Failure: nativeObject={0}", obj.name));
            }
        }

        public static void Destroy(Object obj)
        {
            if (obj != null)
            {
                CrossEngineImpl.GameObject.Destroy(obj._GetImpl());
            }
        }
        public static void Destroy(Object obj, float t)
        {
            if (obj != null)
            {
                CrossEngineImpl.GameObject.Destroy(obj._GetImpl(), t);
            }
        }

        public void SetActive(bool bActive)
        {
            GetImpl<CrossEngineImpl.GameObject>().SetActive(bActive);
        }

        private CrossEngineImpl.Component GetComponentImpl(System.Type type, bool bGetComponentInChildren)
        {
            if (bGetComponentInChildren)
            {
                return GetImpl<CrossEngineImpl.GameObject>().GetComponent(type);
            }
            else
            {
                return GetImpl<CrossEngineImpl.GameObject>().GetComponentInChildren(type);
            }
        }

        private CrossEngineImpl.Component[] GetComponentsImpl(System.Type type, bool bGetComponentInChildren)
        {
            CrossEngineImpl.Component[] components, componentsNative;
            if (bGetComponentInChildren)
            {
                componentsNative = GetImpl<CrossEngineImpl.GameObject>().GetComponents(type);
                components = (CrossEngineImpl.Component[])ObjectFactory.Create(type, componentsNative.Length);
                
            }
            else
            {
                componentsNative = GetImpl<CrossEngineImpl.GameObject>().GetComponentsInChildren(type);
                components = (CrossEngineImpl.Component[])ObjectFactory.Create(type, componentsNative.Length);
            }
            
            System.Array.Copy(componentsNative, components, componentsNative.Length);

            return components;
        }

        public Component GetTypedComponent(ObjectType type, bool bGetComponentInChildren = false)
        {
            return ObjectFactory.Create<Component>(GetComponentImpl(ObjectFactory.TypeToNative(type), bGetComponentInChildren));
        }

        public Component[] GetTypedComponents(ObjectType type, bool bGetComponentInChildren = false)
        {
            return ObjectFactory.Create<Component>(GetComponentsImpl(ObjectFactory.TypeToNative(type), bGetComponentInChildren));
        }

        private CrossEngineImpl.Component AddTypedComponentImpl(System.Type type)
        {
            return GetImpl<CrossEngineImpl.GameObject>().AddComponent(type);
        }

        public Component AddTypedComponent(ObjectType type)
        {
            return ObjectFactory.Create<Component>(AddTypedComponentImpl(ObjectFactory.TypeToNative(type)));
        }
        
        public object AddComponentAny(System.Type component)
        {
            return AddTypedComponentImpl(component);
        }

        public void SendMessage(string methodName)
        {
            GetImpl<CrossEngineImpl.GameObject>().SendMessage(methodName);
        }
        public void SendMessage(string methodName, string v)
        {
            GetImpl<CrossEngineImpl.GameObject>().SendMessage(methodName, v);
        }
        public void SendMessage(string methodName, float v)
        {
            GetImpl<CrossEngineImpl.GameObject>().SendMessage(methodName, v);
        }
        public void SendMessage(string methodName, bool bEnable)
        {
            GetImpl<CrossEngineImpl.GameObject>().SendMessage(methodName, bEnable);
        }
        public void SendMessage(string methodName, float[] bEnable)
        {
            GetImpl<CrossEngineImpl.GameObject>().SendMessage(methodName, bEnable);
        }
        public void SendMessage(string methodName, object value, ArkCrossEngine.SendMessageOptions options)
        {
            GetImpl<CrossEngineImpl.GameObject>().SendMessage(methodName, value, (CrossEngineImpl.SendMessageOptions)(int)(ArkCrossEngine.SendMessageOptions)(options));
        }
        public void SendMessage(string methodName, ArkCrossEngine.SendMessageOptions options)
        {
            GetImpl<CrossEngineImpl.GameObject>().SendMessage(methodName, (CrossEngineImpl.SendMessageOptions)(int)(ArkCrossEngine.SendMessageOptions)(options));
        }
    }
}
