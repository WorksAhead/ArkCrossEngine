using System.Runtime.InteropServices;

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
    [StructLayout(LayoutKind.Sequential)]
    public class Object
    {
        public Object()
        {

        }

        public static implicit operator string(Object obj)
        {
            return obj.GetImpl<CrossEngineImpl.Object>().ToString();
        }

        public string name
        {
            get { return GetImpl<CrossEngineImpl.Object>().name; }
            set { GetImpl<CrossEngineImpl.Object>().name = value; }
        }

        public int GetInstanceID()
        {
            return GetImpl<CrossEngineImpl.Object>().GetInstanceID();
        }

        public GameObject TryCastToGameObject()
        {
            return ObjectFactory.Create<GameObject>(_GetImpl());
        }

        public T GetImpl<T>() where T : CrossEngineImpl.Object
        {
            return (T)ObjectImpl;
        }

        public CrossEngineImpl.Object _GetImpl()
        {
            return (CrossEngineImpl.Object)ObjectImpl;
        }

        public void _SetImpl(CrossEngineImpl.Object obj)
        {
            ObjectImpl = obj;
        }

        public T GetThis<T>() where T : Object
        {
            return (T)this;
        }

        protected CrossEngineImpl.Object ObjectImpl;
    }

    public class UnknownObject : Object
    {
        public UnknownObject(CrossEngineImpl.Object obj)
        {
            _SetImpl(obj);
        }
    }

}
