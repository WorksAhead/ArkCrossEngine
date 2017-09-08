using System.Collections.Generic;
using UnityEngine;

namespace ArkCrossEngine
{
    /// <summary>
    /// 资源管理器，提供资源缓存重用机制。
    /// 
    /// todo:分包策略确定后需要修改为从分包里加载资源
    /// </summary>
    internal class ResourceManager
    {
        internal void PreloadResource(string res, int count)
        {
            Object prefab = GetSharedResource(res);
            PreloadResource(prefab, count);
        }
        internal void PreloadResource(Object prefab, int count)
        {
            if (null != prefab)
            {
                if (!m_PreloadResources.Contains(prefab.GetInstanceID()))
                    m_PreloadResources.Add(prefab.GetInstanceID());
                for (int i = 0; i < count; ++i)
                {
                    Object obj = GameObject.Instantiate(prefab);
                    AddToUnusedResources(prefab.GetInstanceID(), obj);
                }
            }
        }
        internal void PreloadSharedResource(string res)
        {
            Object prefab = GetSharedResource(res);
            if (null != prefab)
            {
                if (!m_PreloadResources.Contains(prefab.GetInstanceID()))
                    m_PreloadResources.Add(prefab.GetInstanceID());
            }
        }

        //优化setActive函数
        internal void SetActiveOptim(GameObject obj, bool active)
        {
            obj.SetActive(active);
            //SetTransformActiveRecursively(obj.transform, active);
            //if(active)
            //{
            //    obj.transform.position = Vector3.zero;
            //}
            //else
            //{
            //    obj.transform.position = new Vector3(-1000.0f, -1000.0f, -1000.0f);
            //}
        }


        void SetTransformActiveRecursively(Transform transform, bool active)
        {
            SetActiveOptimFinal(transform, active);
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                    SetTransformActiveRecursively(transform.GetChild(i), active);
            }
        }

        //优化setActive函数
        internal void SetActiveOptimFinal(Transform transform, bool active)
        {
            GameObject obj = transform.gameObject;
            //obj.SetActive(active);
            // Flags used to find the 'enabled' property on Unity components that don't expose it.
            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.SetField |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Instance;
            Component[] animations = obj.GetComponents<Animation>();
            if (animations.Length > 0)
            {
                Animation anim;
                for (int i = 0; i < animations.Length; i++)
                {
                    anim = animations[i] as Animation;
                    if (active)
                        anim.Sample();
                    else
                        anim.Stop();
                }

                Component[] components = obj.GetComponents<Component>();
                Component component;
                System.Type type;
                System.Reflection.PropertyInfo property;
                for (int j = 0; j < components.Length; j++)
                {
                    component = components[j];
                    type = component.GetType();
                    if (!(type == typeof(Animation)))
                    {
                        property = type.GetProperty("enabled", flags);
                        if (property != null)
                            property.SetValue(component, active, null);
                    }
                }
            }
            else
            {
                obj.SetActive(active);
            }
        }



        internal Object NewObject(string res)
        {
            return NewObject(res, 0);
        }
        internal Object NewObject(string res, float timeToRecycle)
        {
            Object prefab = GetSharedResource(res);
            return NewObject(prefab, timeToRecycle);
        }
        internal Object NewObject(Object prefab)
        {
            return NewObject(prefab, 0);
        }
        internal Object NewObject(Object prefab, float timeToRecycle)
        {
            Object obj = null;
            if (null != prefab)
            {
                float curTime = Time.time;
                float time = timeToRecycle;
                if (timeToRecycle > 0)
                    time += curTime;
                int resId = prefab.GetInstanceID();
                obj = NewFromUnusedResources(resId);
                if (null == obj)
                {
                    obj = GameObject.Instantiate(prefab);
                }
                if (null != obj)
                {
                    AddToUsedResources(obj, resId, time);

                    InitializeObject(obj);
                }
            }
            return obj;
        }
        internal bool RecycleObject(Object obj)
        {
            bool ret = false;
            if (null != obj)
            {
                //GameObject gameObject = obj as GameObject;
                //if (null != gameObject) {
                //LogicSystem.LogFromGfx("RecycleObject {0} {1}", gameObject.name, gameObject.tag);
                //}

                int objId = obj.GetInstanceID();
                if (m_UsedResources.Contains(objId))
                {
                    UsedResourceInfo resInfo = m_UsedResources[objId];
                    if (null != resInfo)
                    {
                        FinalizeObject(resInfo.m_Object);
                        RemoveFromUsedResources(objId);
                        AddToUnusedResources(resInfo.m_ResId, obj);
                        resInfo.Recycle();
                        ret = true;
                    }
                }
            }
            return ret;
        }
        internal void Tick()
        {
            float curTime = Time.time;
            /*
            if (m_LastTickTime <= 0) {
              m_LastTickTime = curTime;
              return;
            }
            float delta = curTime - m_LastTickTime;
            if (delta < 0.1f) {
              return;
            }
            m_LastTickTime = curTime;
            */

            for (LinkedListNode<UsedResourceInfo> node = m_UsedResources.FirstValue; null != node;)
            {
                UsedResourceInfo resInfo = node.Value;
                if (resInfo.m_RecycleTime > 0 && resInfo.m_RecycleTime < curTime)
                {
                    node = node.Next;

                    //GameObject gameObject = resInfo.m_Object as GameObject;
                    //if (null != gameObject) {
                    //LogicSystem.LogFromGfx("RecycleObject {0} {1} by Tick", gameObject.name, gameObject.tag);
                    //}

                    FinalizeObject(resInfo.m_Object);
                    AddToUnusedResources(resInfo.m_ResId, resInfo.m_Object);
                    RemoveFromUsedResources(resInfo.m_ObjId);
                    resInfo.Recycle();
                }
                else
                {
                    node = node.Next;
                }
            }
        }
        internal Object GetSharedResource(string res, bool isUseAssetbundle = true)
        {
            Object obj = null;
            if (string.IsNullOrEmpty(res))
            {
                return obj;
            }
            if (!m_LoadedPrefabs.TryGetValue(res, out obj))
            {
                if (GlobalVariables.Instance.IsPublish && isUseAssetbundle)
                {
                    obj = ResUpdateHandler.LoadAssetFromABWithoutExtention(res);
                }
                if (obj == null)
                {
                    obj = Resources.Load(res);
                }
                if (obj != null)
                {
                    m_LoadedPrefabs.Add(res, obj);
                }
                else
                {
                    LogSystem.Error("LoadAsset failed:" + res);
                }
            }
            return obj;
        }
        internal void CleanupResourcePool()
        {
            for (LinkedListNode<UsedResourceInfo> node = m_UsedResources.FirstValue; null != node;)
            {
                UsedResourceInfo resInfo = node.Value;
                if (!m_PreloadResources.Contains(resInfo.m_ResId))
                {
                    node = node.Next;
                    RemoveFromUsedResources(resInfo.m_ObjId);
                    resInfo.Recycle();
                }
                else
                {
                    node = node.Next;
                }
            }

            foreach (KeyValuePair<int, Queue<Object>> pair in m_UnusedResources)
            {
                int key = pair.Key;
                if (m_PreloadResources.Contains(key))
                    continue;
                Queue<Object> queue = pair.Value;
                queue.Clear();
            }

            foreach (KeyValuePair<string, Object> pair in m_LoadedPrefabs)
            {
                string key = pair.Key;
                Object obj = pair.Value;
                if (null != obj)
                {
                    try
                    {
                        int instId = obj.GetInstanceID();
                        if (!m_PreloadResources.Contains(instId))
                        {
                            m_WaitDeleteLoadedPrefabEntrys.Add(key);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        m_WaitDeleteLoadedPrefabEntrys.Add(key);
                        LogicSystem.LogErrorFromLogic("Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
                    }
                }
                else
                {
                    m_WaitDeleteLoadedPrefabEntrys.Add(key);
                }
            }
            for (int i = 0; i < m_WaitDeleteLoadedPrefabEntrys.Count; i++)
            {
                m_LoadedPrefabs.Remove(m_WaitDeleteLoadedPrefabEntrys[i]);
            }
            /*
            foreach (string key in m_WaitDeleteLoadedPrefabEntrys) {
              m_LoadedPrefabs.Remove(key);
            }*/
            m_WaitDeleteLoadedPrefabEntrys.Clear();

            Resources.UnloadUnusedAssets();
        }

        private Object NewFromUnusedResources(int res)
        {
            Object obj = null;
            Queue<Object> queue;
            if (m_UnusedResources.TryGetValue(res, out queue))
            {
                if (queue.Count > 0)
                    obj = queue.Dequeue();
            }
            return obj;
        }
        private void AddToUnusedResources(int res, Object obj)
        {
            Queue<Object> queue;
            if (m_UnusedResources.TryGetValue(res, out queue))
            {
                queue.Enqueue(obj);
            }
            else
            {
                queue = new Queue<Object>();
                queue.Enqueue(obj);
                m_UnusedResources.Add(res, queue);
            }
        }
        private void AddToUsedResources(Object obj, int resId, float recycleTime)
        {
            int objId = obj.GetInstanceID();
            if (!m_UsedResources.Contains(objId))
            {
                UsedResourceInfo info = m_UsedResourceInfoPool.Alloc();
                info.m_ObjId = objId;
                info.m_Object = obj;
                info.m_ResId = resId;
                info.m_RecycleTime = recycleTime;

                m_UsedResources.AddLast(objId, info);
            }
        }
        private void RemoveFromUsedResources(int objId)
        {
            m_UsedResources.Remove(objId);
        }

        private void InitializeObject(Object obj)
        {
            GameObject gameObj = obj as GameObject;
            if (null != gameObj)
            {
                if (!gameObj.activeSelf)
                    //gameObj.SetActive(true);
                    SetActiveOptim(gameObj, true);
                /*ParticleSystem[] pss = gameObj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in pss) {
                  if (null != ps && ps.playOnAwake) {
                    ps.Play();
                  }
                }*/
                ParticleSystem ps = gameObj.GetComponent<ParticleSystem>();
                if (null != ps && ps.main.playOnAwake)
                {
                    ps.Play();
                }
            }
        }
        private void FinalizeObject(Object obj)
        {
            GameObject gameObj = obj as GameObject;
            if (null != gameObj)
            {
                ParticleSystem ps0 = gameObj.GetComponent<ParticleSystem>();
                if (null != ps0 && ps0.main.playOnAwake)
                {
                    ps0.Stop();
                }
                Component[] pss = gameObj.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < pss.Length; i++)
                {
                    if (null != pss[i])
                    {
                        ((ParticleSystem)pss[i]).Clear();
                    }
                }
                /*
                foreach (ParticleSystem ps in pss) {
                  if (null != ps) {
                    ps.Clear();
                  }
                }*/
                if (null != gameObj.transform.parent)
                    gameObj.transform.parent = null;
                if (gameObj.activeSelf)
                    SetActiveOptim(gameObj, false);
                //gameObj.SetActive(false);
            }
        }

        private ResourceManager()
        {
            m_UsedResourceInfoPool.Init(256);
        }

        private class UsedResourceInfo : IPoolAllocatedObject<UsedResourceInfo>
        {
            internal int m_ObjId;
            internal Object m_Object;
            internal int m_ResId;
            internal float m_RecycleTime;

            internal void Recycle()
            {
                m_Object = null;
                m_Pool.Recycle(this);
            }
            public void InitPool(ObjectPool<UsedResourceInfo> pool)
            {
                m_Pool = pool;
            }
            public UsedResourceInfo Downcast()
            {
                return this;
            }
            private ObjectPool<UsedResourceInfo> m_Pool = null;
        }

        private ObjectPool<UsedResourceInfo> m_UsedResourceInfoPool = new ObjectPool<UsedResourceInfo>();

        private HashSet<int> m_PreloadResources = new HashSet<int>();
        private Dictionary<string, Object> m_LoadedPrefabs = new Dictionary<string, Object>();
        private List<string> m_WaitDeleteLoadedPrefabEntrys = new List<string>();

        private LinkedListDictionary<int, UsedResourceInfo> m_UsedResources = new LinkedListDictionary<int, UsedResourceInfo>();
        private Dictionary<int, Queue<Object>> m_UnusedResources = new Dictionary<int, Queue<Object>>();
        //private float m_LastTickTime = 0;

        public static ResourceManager Instance
        {
            get { return s_Instance; }
        }
        private static ResourceManager s_Instance = new ResourceManager();
    }
}
