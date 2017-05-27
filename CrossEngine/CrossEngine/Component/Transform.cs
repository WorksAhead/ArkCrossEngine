using System.Collections;

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
    public class Transform : Component, IEnumerable
    {
        public Transform()
        {

        }

        public int childCount
        {
            get { return GetImpl<CrossEngineImpl.Transform>().childCount; }
        }

        public Transform parent
        {
            get { return ObjectFactory.Create<Transform>(GetImpl<CrossEngineImpl.Transform>().parent); }
            set
            {
                if(value != null)
                {
                    GetImpl<CrossEngineImpl.Transform>().parent = value._GetImpl() as CrossEngineImpl.Transform;
                }
                else
                {
                    GetImpl<CrossEngineImpl.Transform>().parent = null;
                }
            }
        }

        public ArkCrossEngine.Vector3 position
        {
            get { return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Transform>().position); }
            set { GetImpl<CrossEngineImpl.Transform>().position = Helper.Vec3ToUnity(value); }
        }
        public ArkCrossEngine.Quaternion localRotation
        {
            get { return Helper.QuatFromUnity(GetImpl<CrossEngineImpl.Transform>().localRotation); }
            set { GetImpl<CrossEngineImpl.Transform>().localRotation = Helper.QuatToUnity(value); }
        }
        public ArkCrossEngine.Vector3 localScale
        {
            get { return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Transform>().localScale); }
            set { GetImpl<CrossEngineImpl.Transform>().localScale = Helper.Vec3ToUnity(value); }
        }
        public ArkCrossEngine.Vector3 localPosition
        {
            get { return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Transform>().localPosition); }
            set { GetImpl<CrossEngineImpl.Transform>().localPosition = Helper.Vec3ToUnity(value); }
        }
        public ArkCrossEngine.Quaternion rotation
        {
            get { return Helper.QuatFromUnity(GetImpl<CrossEngineImpl.Transform>().rotation); }
            set { GetImpl<CrossEngineImpl.Transform>().rotation = Helper.QuatToUnity(value); }
        }
        public ArkCrossEngine.Vector3 forward
        {
            get { return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Transform>().forward); }
            set { GetImpl<CrossEngineImpl.Transform>().forward = Helper.Vec3ToUnity(value); }
        }
        public ArkCrossEngine.Vector3 eulerAngles
        {
            get { return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Transform>().eulerAngles); }
            set { GetImpl<CrossEngineImpl.Transform>().eulerAngles = Helper.Vec3ToUnity(value); }
        }

        public Transform GetChild(int i)
        {
            return ObjectFactory.Create<Transform>(GetImpl<CrossEngineImpl.Transform>().GetChild(i));
        }

        public Transform Find(string path)
        {
            return ObjectFactory.Create<Transform>(GetImpl<CrossEngineImpl.Transform>().Find(path));
        }

        public ArkCrossEngine.Vector3 TransformPoint(ArkCrossEngine.Vector3 position)
        {
            return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Transform>().TransformPoint(Helper.Vec3ToUnity(position)));
        }

        public ArkCrossEngine.Vector3 InverseTransformPoint(ArkCrossEngine.Vector3 position)
        {
            return Helper.Vec3FromUnity(GetImpl<CrossEngineImpl.Transform>().InverseTransformPoint(Helper.Vec3ToUnity(position)));
        }

        public void Rotate(ArkCrossEngine.Vector3 eulerAngles)
        {
            GetImpl<CrossEngineImpl.Transform>().Rotate(Helper.Vec3ToUnity(eulerAngles));
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        private sealed partial class Enumerator : IEnumerator
        {
            Transform outer;
            int currentIndex = -1;

            internal Enumerator(Transform outer)
            {
                this.outer = outer;
            }

            public object Current
            {
                get { return outer.GetChild(currentIndex); }
            }
            public bool MoveNext()
            {
                int childCount = outer.childCount;
                return ++currentIndex < childCount;
            }
            public void Reset()
            {
                currentIndex = -1;
            }
        }
    }
}
