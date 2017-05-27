using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public enum ObjectType
    {
        /// Object
        Object,
        GameObject,

        /// Animation
        Animation,
        
        /// Audio
        AudioClip,
        AudioSource,

        /// Camera
        Camera,

        /// Character
        CharacterController,

        /// Component
        Component,
        Transform,
        Collider,
        BoxCollider,
        Renderer,
        LineRenderer,
        MeshRenderer,
        ParticleSystem,
        SkinnedMeshRenderer,
        ParticleSystemRenderer,
        Rigidbody,
        Terrain,

        /// RenderElement
        Material,
        Mesh,
        MeshFilter,
        Shader,
        Texture,
        Texture2D,

        /// Resource
        AsyncOperation,
        Resources,

        /// Custom
        Custom,

        /// ...

        /// Unknown
        Unknown
    }

    class ObjectImplPair
    {
        public ObjectImplPair(Type cross, Type native)
        {
            CrossImpl = cross;
            NativeImpl = native;
        }

        public Type CrossImpl;
        public Type NativeImpl;
    }

    public class ObjectFactory
    {
        public static object Create(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static object[] Create(Type type, int length)
        {
            return (object[])Array.CreateInstance(type, length);
        }

        public static Object Create(ObjectType type, CrossEngineImpl.Object nativeObject)
        {
            if (nativeObject == null)
            {
                return null;
            }

            Type objectType = TypeToCross(type);
            if (objectType != null)
            {
                // query from pool
                Object objectFromPool;
                int hashID = nativeObject.GetHashCode();
                if (Objects.TryGetValue(hashID, out objectFromPool))
                {
                    // is same object?
                    if (objectFromPool._GetImpl() != nativeObject)
                    {
                        objectFromPool._SetImpl(nativeObject);
                    }
                    return objectFromPool;
                }
                else
                {
                    Object obj = (Object)Create(objectType);
                    obj._SetImpl(nativeObject);
                    Objects.Add(hashID, obj);
                    return obj;
                }
            }
            else
            {
                return new UnknownObject(nativeObject);
            }
        }

        public static Object Create(CrossEngineImpl.Object nativeObject)
        {
            if (nativeObject == null)
            {
                return null;
            }

            ObjectType type = TypeFromNative(nativeObject);
            return Create(type, nativeObject);
        }

        public static T Create<T>(CrossEngineImpl.Object nativeObject) where T : Object
        {
            if (nativeObject == null)
            {
                return null;
            }

            return (T)Create(nativeObject);
        }

        public static T[] Create<T>(CrossEngineImpl.Object[] nativeObjects) where T : Object
        {
            Type arrTypeNative = nativeObjects.GetType();
            Type typeNative = arrTypeNative.GetElementType();

            ObjectType ot = TypeFromNative(typeNative);
            Type typeCross = TypeToCross(ot);

            T[] arr = (T[])Create(typeCross, nativeObjects.Length);
            for (int i = 0; i < nativeObjects.Length; ++i)
            {
                arr[i] = Create<T>(nativeObjects[i]);
            }
            return arr;
        }

        public static void GC()
        {
            // release dead object wrapper...
        }

        public static ObjectType TypeFromNative(CrossEngineImpl.Object obj)
        {
            ObjectType type;
            Type nativeType = obj.GetType();
            if (TypeKeyFastMap.TryGetValue(nativeType.FullName, out type))
            {
                return type;
            }
            return ObjectType.Unknown;
        }

        public static ObjectType TypeFromNative(Type nativeType)
        {
            ObjectType type;
            if (TypeKeyFastMap.TryGetValue(nativeType.FullName, out type))
            {
                return type;
            }
            return ObjectType.Unknown;
        }

        public static Type TypeToCross(ObjectType type)
        {
            ObjectImplPair ObjectType;
            if (ObjectKeyMap.TryGetValue(type, out ObjectType))
            {
                return ObjectType.CrossImpl;
            }
            return null;
        }

        public static Type TypeToNative(ObjectType type)
        {
            ObjectImplPair ObjectType;
            if (ObjectKeyMap.TryGetValue(type, out ObjectType))
            {
                return ObjectType.NativeImpl;
            }
            return null;
        }

        public static void ConstructObjectType()
        {
            try
            {
                ObjectKeyMap.Add(ObjectType.Animation, new ObjectImplPair(typeof(ArkCrossEngine.Animation), typeof(CrossEngineImpl.Animation)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Animation).FullName, ObjectType.Animation);

                ObjectKeyMap.Add(ObjectType.AudioClip, new ObjectImplPair(typeof(ArkCrossEngine.AudioClip), typeof(CrossEngineImpl.AudioClip)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.AudioClip).FullName, ObjectType.AudioClip);

                ObjectKeyMap.Add(ObjectType.AudioSource, new ObjectImplPair(typeof(ArkCrossEngine.AudioSource), typeof(CrossEngineImpl.AudioSource)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.AudioSource).FullName, ObjectType.AudioSource);

                ObjectKeyMap.Add(ObjectType.Camera, new ObjectImplPair(typeof(ArkCrossEngine.Camera), typeof(CrossEngineImpl.Camera)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Camera).FullName, ObjectType.Camera);

                ObjectKeyMap.Add(ObjectType.CharacterController, new ObjectImplPair(typeof(ArkCrossEngine.CharacterController), typeof(CrossEngineImpl.CharacterController)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.CharacterController).FullName, ObjectType.CharacterController);

                ObjectKeyMap.Add(ObjectType.BoxCollider, new ObjectImplPair(typeof(ArkCrossEngine.BoxCollider), typeof(CrossEngineImpl.BoxCollider)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.BoxCollider).FullName, ObjectType.BoxCollider);

                ObjectKeyMap.Add(ObjectType.Collider, new ObjectImplPair(typeof(ArkCrossEngine.Collider), typeof(CrossEngineImpl.Collider)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Collider).FullName, ObjectType.Collider);

                ObjectKeyMap.Add(ObjectType.Component, new ObjectImplPair(typeof(ArkCrossEngine.Component), typeof(CrossEngineImpl.Component)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Component).FullName, ObjectType.Component);

                ObjectKeyMap.Add(ObjectType.LineRenderer, new ObjectImplPair(typeof(ArkCrossEngine.LineRenderer), typeof(CrossEngineImpl.LineRenderer)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.LineRenderer).FullName, ObjectType.LineRenderer);

                ObjectKeyMap.Add(ObjectType.MeshRenderer, new ObjectImplPair(typeof(ArkCrossEngine.MeshRenderer), typeof(CrossEngineImpl.MeshRenderer)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.MeshRenderer).FullName, ObjectType.MeshRenderer);

                ObjectKeyMap.Add(ObjectType.ParticleSystem, new ObjectImplPair(typeof(ArkCrossEngine.ParticleSystem), typeof(CrossEngineImpl.ParticleSystem)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.ParticleSystem).FullName, ObjectType.ParticleSystem);

                ObjectKeyMap.Add(ObjectType.Renderer, new ObjectImplPair(typeof(ArkCrossEngine.Renderer), typeof(CrossEngineImpl.Renderer)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Renderer).FullName, ObjectType.Renderer);

                ObjectKeyMap.Add(ObjectType.Rigidbody, new ObjectImplPair(typeof(ArkCrossEngine.Rigidbody), typeof(CrossEngineImpl.Rigidbody)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Rigidbody).FullName, ObjectType.Rigidbody);

                ObjectKeyMap.Add(ObjectType.SkinnedMeshRenderer, new ObjectImplPair(typeof(ArkCrossEngine.SkinnedMeshRenderer), typeof(CrossEngineImpl.SkinnedMeshRenderer)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.SkinnedMeshRenderer).FullName, ObjectType.SkinnedMeshRenderer);

                ObjectKeyMap.Add(ObjectType.ParticleSystemRenderer, new ObjectImplPair(typeof(ArkCrossEngine.ParticleSystemRenderer), typeof(CrossEngineImpl.ParticleSystemRenderer)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.ParticleSystemRenderer).FullName, ObjectType.ParticleSystemRenderer);

                ObjectKeyMap.Add(ObjectType.Terrain, new ObjectImplPair(typeof(ArkCrossEngine.Terrain), typeof(CrossEngineImpl.Terrain)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Terrain).FullName, ObjectType.Terrain);

                ObjectKeyMap.Add(ObjectType.Transform, new ObjectImplPair(typeof(ArkCrossEngine.Transform), typeof(CrossEngineImpl.Transform)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Transform).FullName, ObjectType.Transform);

                ObjectKeyMap.Add(ObjectType.GameObject, new ObjectImplPair(typeof(ArkCrossEngine.GameObject), typeof(CrossEngineImpl.GameObject)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.GameObject).FullName, ObjectType.GameObject);

                ObjectKeyMap.Add(ObjectType.Object, new ObjectImplPair(typeof(ArkCrossEngine.Object), typeof(CrossEngineImpl.Object)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Object).FullName, ObjectType.Object);

                ObjectKeyMap.Add(ObjectType.Material, new ObjectImplPair(typeof(ArkCrossEngine.Material), typeof(CrossEngineImpl.Material)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Material).FullName, ObjectType.Material);

                ObjectKeyMap.Add(ObjectType.Mesh, new ObjectImplPair(typeof(ArkCrossEngine.Mesh), typeof(CrossEngineImpl.Mesh)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Mesh).FullName, ObjectType.Mesh);

                ObjectKeyMap.Add(ObjectType.MeshFilter, new ObjectImplPair(typeof(ArkCrossEngine.MeshFilter), typeof(CrossEngineImpl.MeshFilter)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.MeshFilter).FullName, ObjectType.MeshFilter);

                ObjectKeyMap.Add(ObjectType.Shader, new ObjectImplPair(typeof(ArkCrossEngine.Shader), typeof(CrossEngineImpl.Shader)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Shader).FullName, ObjectType.Shader);

                ObjectKeyMap.Add(ObjectType.Texture, new ObjectImplPair(typeof(ArkCrossEngine.Texture), typeof(CrossEngineImpl.Texture)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Texture).FullName, ObjectType.Texture);

                ObjectKeyMap.Add(ObjectType.Texture2D, new ObjectImplPair(typeof(ArkCrossEngine.Texture2D), typeof(CrossEngineImpl.Texture2D)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Texture2D).FullName, ObjectType.Texture2D);

                ObjectKeyMap.Add(ObjectType.AsyncOperation, new ObjectImplPair(typeof(ArkCrossEngine.AsyncOperation), typeof(CrossEngineImpl.AsyncOperation)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.AsyncOperation).FullName, ObjectType.AsyncOperation);

                ObjectKeyMap.Add(ObjectType.Resources, new ObjectImplPair(typeof(ArkCrossEngine.Resources), typeof(CrossEngineImpl.Resources)));
                TypeKeyFastMap.Add(typeof(CrossEngineImpl.Resources).FullName, ObjectType.Resources);
            }
            catch (System.Exception ex)
            {
                throw new Exception("Consutruct object fail : " + ex.Message);
            }
        }

        static Dictionary<ObjectType, ObjectImplPair> ObjectKeyMap = new Dictionary<ObjectType, ObjectImplPair>();
        static Dictionary<string, ObjectType> TypeKeyFastMap = new Dictionary<string, ObjectType>();

        static Dictionary<int, Object> Objects = new Dictionary<int, Object>();
    }
}
