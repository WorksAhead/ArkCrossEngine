using System;
using System.Collections.Generic;
using System.Threading;

namespace ArkCrossEngine
{
    public delegate void BeforeLoadSceneDelegation(string curName, string targetName, int targetSceneId);
    public delegate void AfterLoadSceneDelegation(string targetName, int targetSceneId);
    public sealed partial class GfxSystem
    {
        private class GameObjectInfo
        {
            public GameObject ObjectInstance;
            public SharedGameObjectInfo ObjectInfo;
            public float FaceDir;

            public GameObjectInfo(GameObject o, SharedGameObjectInfo i)
            {
                ObjectInstance = o;
                ObjectInfo = i;
                FaceDir = 0;
            }
        }
        // 初始化阶段调用的函数
        private void InitImpl()
        {
            m_EventChannelForLogic.RunInLogicThread = true;
            m_EventChannelForGfx.RunInLogicThread = false;
        }
        private void TickImpl()
        {
            long curTime = TimeUtility.GetLocalMilliseconds();
            if (m_LastLogTime + 10000 < curTime)
            {
                m_LastLogTime = curTime;

#if DEBUG
                if (m_GfxInvoker.CurActionNum > 10)
                {
                    CallGfxLog("GfxSystem.Tick actionNum:{0}", m_GfxInvoker.CurActionNum);
                }

                m_GfxInvoker.DebugPoolCount((string msg) =>
                {
                    CallGfxLog("GfxActionQueue {0}", msg);
                });
#endif
                m_GfxInvoker.ClearPool(1024);
            }

            try
            {
                Profiler.BeginSample("GfxSystem.HandleSync");
                HandleSync();
            }
            finally
            {
                Profiler.EndSample();
            }

            try
            {
                Profiler.BeginSample("GfxSystem.HandleInput");
                HandleInput();
            }
            finally
            {
                Profiler.EndSample();
            }

            try
            {
                Profiler.BeginSample("GfxSystem.HandleLoadingProgress");
                HandleLoadingProgress();
            }
            finally
            {
                Profiler.EndSample();
            }

            try
            {
                Profiler.BeginSample("ResourceManager.Tick");
                ResourceManager.Instance.Tick();
            }
            finally
            {
                Profiler.EndSample();
            }

            try
            {
                Profiler.BeginSample("GfxSystem.HandleActions");
                m_GfxInvoker.HandleActions(4096);
            }
            finally
            {
                Profiler.EndSample();
            }
        }
        private void ReleaseImpl()
        {

        }
        private void SetLogicInvokerImpl(IActionQueue processor)
        {
            m_LogicInvoker = processor;
        }
        private void SetLogicLogCallbackImpl(MyAction<bool, string, object[]> callback)
        {
            m_LogicLogCallback = callback;
        }
        private void SetGameLogicNotificationImpl(IGameLogicNotification notification)
        {
            m_GameLogicNotification = notification;
        }
        // Gfx线程执行的函数，供游戏逻辑线程异步调用
        private void LoadSceneImpl(string name, int chapter, int sceneId, HashSet<int> limitList, MyAction onFinish)
        {
            CallLogicLog("Begin LoadScene:{0}", name);
            m_TargetScene = name;
            m_TargetChapter = chapter;
            m_TargetSceneId = sceneId;
            m_TargetSceneLimitList = limitList;
            BeginLoading();
            if (null == m_LoadingBarAsyncOperation)
            {
                m_LoadingBarAsyncOperation = Application.LoadLevelAsync(m_LoadingBarScene);
                m_LevelLoadedCallback = onFinish;
            }
        }
        private void MarkPlayerSelfImpl(int id)
        {
            GameObjectInfo info = GetGameObjectInfo(id);
            if (null != info)
            {
                m_PlayerSelf = info;
                if (null != info.ObjectInstance)
                {
                    int layer = LayerMask.NameToLayer("Player");
                    if (layer >= 0)
                    {
                        info.ObjectInstance.layer = layer;
                    }
                    LogicSystem.EventChannelForGfx.Publish("player_self_created", "ui");
                }
            }
        }
        private void CreateGameObjectImpl(int id, string resource, SharedGameObjectInfo info)
        {
            if (null != info)
            {
                try
                {
                    Vector3 pos = new Vector3(info.X, info.Y, info.Z);
                    
                    pos.y = SampleTerrainHeight(pos.x, pos.z);
                    Quaternion q = Quaternion.Euler(0, RadianToDegree(info.FaceDir), 0);
                    GameObject obj = ResourceManager.Instance.NewObject(resource).TryCastToGameObject();
                    if (null != obj)
                    {
                        if (null != obj.transform)
                        {
                            obj.transform.position = pos;
                            obj.transform.localRotation = q;
                            if (info.Sx > 0 && info.Sy > 0 && info.Sz > 0)
                            {
                                obj.transform.localScale = new Vector3(info.Sx, info.Sy, info.Sz);
                            }
                        }
                        RememberGameObject(id, obj, info);
                        ResourceManager.Instance.SetActiveOptim(obj, true);
                        //obj.SetActive(true);
                    }
                    else
                    {
                        CallLogicErrorLog("CreateGameObject {0} can't load resource", resource);
                    }
                }
                catch (Exception ex)
                {
                    CallGfxErrorLog("CreateGameObject {0} throw exception:{1}\n{2}", resource, ex.Message, ex.StackTrace);
                }
            }
        }
        private void CreateGameObjectImpl(int id, string resource, float x, float y, float z, float rx, float ry, float rz, bool attachTerrain)
        {
            try
            {
                if (attachTerrain)
                    y = SampleTerrainHeight(x, z);
                Vector3 pos = new Vector3(x, y, z);
                Quaternion q = Quaternion.Euler(RadianToDegree(rx), RadianToDegree(ry), RadianToDegree(rz));
                GameObject obj = ResourceManager.Instance.NewObject(resource) as GameObject;
                if (null != obj)
                {
                    obj.transform.position = pos;
                    obj.transform.localRotation = q;
                    RememberGameObject(id, obj);
                    ResourceManager.Instance.SetActiveOptim(obj, true);
                    //obj.SetActive(true);
                }
                else
                {
                    CallLogicErrorLog("CreateGameObject {0} can't load resource", resource);
                }
            }
            catch (Exception ex)
            {
                CallGfxErrorLog("CreateGameObject {0} throw exception:{1}\n{2}", resource, ex.Message, ex.StackTrace);
            }
        }
        private void CreateGameObjectWithMeshDataImpl(int id, List<float> vertices, List<int> triangles, uint color, string mat, bool attachTerrain)
        {
            if (vertices.Count >= 3)
            {
                List<float> uvs = new List<float>();
                int count = vertices.Count / 3;
                for (int i = 0; i < count; ++i)
                {
                    int ix = i % 4;
                    switch (ix)
                    {
                        case 0:
                            uvs.Add(0);
                            uvs.Add(0);
                            break;
                        case 1:
                            uvs.Add(0);
                            uvs.Add(1);
                            break;
                        case 2:
                            uvs.Add(1);
                            uvs.Add(0);
                            break;
                        case 3:
                            uvs.Add(1);
                            uvs.Add(1);
                            break;
                    }
                }
                CreateGameObjectWithMeshDataImpl(id, vertices, uvs, triangles, color, mat, attachTerrain);
            }
        }
        private void CreateGameObjectWithMeshDataImpl(int id, List<float> vertices, List<float> uvs, List<int> triangles, uint color, string mat, bool attachTerrain)
        {
            byte a = (byte)((color & 0xff000000) >> 24);
            byte r = (byte)((color & 0x0ff0000) >> 16);
            byte g = (byte)((color & 0x0ff00) >> 8);
            byte b = (byte)(color & 0x0ff);
            Color32 c = new Color32(r, g, b, a);

            Material material = null;
            Shader shader = Shader.Find(mat);
            if (null != shader)
            {
                //material = new Material(shader);
                material = Material.Create(shader);
                material.color = c;
            }
            else
            {
                //material = new Material(mat);
                material = Material.Create(shader);
                material.color = c;
            }

            CreateGameObjectWithMeshDataHelper(id, vertices, uvs, triangles, material, attachTerrain);
        }
        private void CreateGameObjectWithMeshDataImpl(int id, List<float> vertices, List<int> triangles, string matRes, bool attachTerrain)
        {
            if (vertices.Count >= 3)
            {
                List<float> uvs = new List<float>();
                int count = vertices.Count / 3;
                for (int i = 0; i < count; ++i)
                {
                    int ix = i % 4;
                    switch (ix)
                    {
                        case 0:
                            uvs.Add(0);
                            uvs.Add(0);
                            break;
                        case 1:
                            uvs.Add(0);
                            uvs.Add(1);
                            break;
                        case 2:
                            uvs.Add(1);
                            uvs.Add(0);
                            break;
                        case 3:
                            uvs.Add(1);
                            uvs.Add(1);
                            break;
                    }
                }
                CreateGameObjectWithMeshDataImpl(id, vertices, uvs, triangles, matRes, attachTerrain);
            }
        }
        private void CreateGameObjectWithMeshDataImpl(int id, List<float> vertices, List<float> uvs, List<int> triangles, string matRes, bool attachTerrain)
        {
            Object matObj = ResourceManager.Instance.GetSharedResource(matRes);
            Material material = matObj as Material;
            if (null != material)
            {
                CreateGameObjectWithMeshDataHelper(id, vertices, uvs, triangles, material, attachTerrain);
            }
            else
            {
                CallLogicErrorLog("CreateGameObjectWithMeshData {0} can't load resource", matRes);
            }
        }
        private void CreateGameObjectForAttachImpl(int id, string resource)
        {
            try
            {
                GameObject obj = ResourceManager.Instance.NewObject(resource) as GameObject;
                if (null != obj)
                {
                    RememberGameObject(id, obj);
                    ResourceManager.Instance.SetActiveOptim(obj, true);
                    //obj.SetActive(true);
                }
                else
                {
                    CallLogicErrorLog("CreateGameObject {0} can't load resource", resource);
                }
            }
            catch (Exception ex)
            {
                CallGfxErrorLog("CreateGameObject {0} throw exception:{1}\n{2}", resource, ex.Message, ex.StackTrace);
            }
        }
        private void CreateAndAttachGameObjectImpl(string resource, int parentId, string path, float recycleTime)
        {
            try
            {
                GameObject obj = ResourceManager.Instance.NewObject(resource, recycleTime) as GameObject;
                GameObject parent = GetGameObject(parentId);
                if (null != obj)
                {
                    ResourceManager.Instance.SetActiveOptim(obj, true);
                    //obj.SetActive(true);
                    if (null != obj.transform && null != parent && null != parent.transform)
                    {
                        Transform t = parent.transform;
                        if (!String.IsNullOrEmpty(path))
                        {
                            t = FindChildRecursive(parent.transform, path);
                        }
                        if (null != t)
                        {
                            obj.transform.parent = t;
                            obj.transform.localPosition = new Vector3(0, 0, 0);
                        }
                        else
                        {
                            CallLogicErrorLog("Obj {0} CreateAndAttachGameObject {1} can't find bone {2}", resource, parentId, path);
                        }
                    }
                }
                else
                {
                    CallLogicErrorLog("CreateAndAttachGameObject {0} can't load resource", resource);
                }
            }
            catch (Exception ex)
            {
                CallGfxErrorLog("CreateAndAttachGameObject {0} throw exception:{1}\n{2}", resource, ex.Message, ex.StackTrace);
            }
        }
        private void CreateAndAttachParticleImpl(string resource, int parentId, string path, float scale, float recycleTime)
        {
            try
            {
                GameObject obj = ResourceManager.Instance.NewObject(resource, recycleTime) as GameObject;
                GameObject parent = GetGameObject(parentId);
                if (null != obj)
                {
                    obj.SetActive(true);
                    obj.SendMessage("SetParticleScaler", scale, SendMessageOptions.DontRequireReceiver);
                    if (null != obj.transform && null != parent && null != parent.transform)
                    {
                        Transform t = parent.transform;
                        if (!String.IsNullOrEmpty(path))
                        {
                            t = FindChildRecursive(parent.transform, path);
                        }
                        if (null != t)
                        {
                            obj.transform.parent = t;
                            obj.transform.localPosition = new Vector3(0, 0, 0);
                        }
                        else
                        {
                            CallLogicErrorLog("Obj {0} CreateAndAttachGameObject {1} can't find bone {2}", resource, parentId, path);
                        }
                    }
                }
                else
                {
                    CallLogicErrorLog("CreateAndAttachGameObject {0} can't load resource", resource);
                }
            }
            catch (Exception ex)
            {
                CallGfxErrorLog("CreateAndAttachGameObject {0} throw exception:{1}\n{2}", resource, ex.Message, ex.StackTrace);
            }
        }
        private void ChangeEquipImpl(int id, string wear_node_and_name, string new_weapon_prefabs)
        {
            try
            {
                GameObject obj = GetGameObject(id);
                if (obj == null)
                {
                    return;
                }
                string[] node_names = wear_node_and_name.Split('|');
                string[] new_weapons = new_weapon_prefabs.Split('@');
                int weapon_index = 0;
                for (int i = 0; i < node_names.Length; i++)
                {
                    string pair_str = node_names[i];
                    string[] pair = pair_str.Split('@');
                    if (pair.Length >= 2 && new_weapons.Length > weapon_index)
                    {
                        Transform equip_node = DestroyCurEquip(obj, pair[1]);
                        if (equip_node == null)
                        {
                            equip_node = GetChildNodeByName(obj, pair[0]);
                        }
                        AddNewEquip(equip_node, pair[1], new_weapons[weapon_index]);
                    }
                    weapon_index++;
                }
                /*
                foreach (string pair_str in node_names) {
                  string[] pair = pair_str.Split('@');
                  if (pair.Length >= 2 && new_weapons.Length > weapon_index) {
                    Transform equip_node = DestroyCurEquip(obj, pair[1]);
                    if (equip_node == null) {
                      equip_node = GetChildNodeByName(obj, pair[0]);
                    }
                    AddNewEquip(equip_node, pair[1], new_weapons[weapon_index]);
                    SetBlockedShaderImpl(id, 0x0000ff90, 0.5f, 0);
                  }
                  weapon_index++;
                }*/
            }
            catch (Exception ex)
            {
                CallGfxErrorLog(string.Format("ChangeWeaponImpl:{0} failed:{1}\n{2}", id, ex.Message, ex.StackTrace));
            }
        }

        private Transform DestroyCurEquip(GameObject obj, string equip_name)
        {
            Transform old_equip = GetChildNodeByName(obj, equip_name);
            Transform equip_parent = null;
            if (old_equip != null)
            {
                equip_parent = old_equip.parent;
                old_equip.parent = null;
                if (!ResourceManager.Instance.RecycleObject(old_equip.gameObject))
                {
                    GameObject.Destroy(old_equip.gameObject);
                }
            }
            return equip_parent;
        }

        private void AddNewEquip(Transform equip_node, string weapon_name, string new_equip_prefab)
        {
            if (equip_node == null)
            {
                return;
            }
            GameObject new_equip = ResourceSystem.NewObject(new_equip_prefab) as GameObject;
            if (new_equip != null)
            {
                new_equip.name = weapon_name;
                new_equip.transform.parent = equip_node;
                new_equip.transform.localPosition = Vector3.zero;
                new_equip.transform.localRotation = Quaternion.identity;
            }
        }

        private Transform GetChildNodeByName(GameObject gameobj, string name)
        {
            if (gameobj == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            Component[] ts = gameobj.transform.GetTypedComponents(ObjectType.Transform, true);
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i].name == name)
                {
                    return (Transform)ts[i];
                }
            }
            return null;
        }
        private void DestroyGameObjectImpl(int id)
        {
            try
            {
                GameObject obj = GetGameObject(id);
                if (null != obj)
                {
                    ForgetGameObject(id, obj);
                    if (!ResourceManager.Instance.RecycleObject(obj))
                    {
                        GameObject.Destroy(obj);
                    }
                    else
                    {
                        ResourceManager.Instance.SetActiveOptim(obj, false);
                    }
                }
            }
            catch (Exception ex)
            {
                CallGfxErrorLog(string.Format("DestroyGameObject:{0} failed:{1}\n{2}", id, ex.Message, ex.StackTrace));
            }
        }
        private void UpdateGameObjectLocalPositionImpl(int id, float x, float y, float z)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                obj.transform.localPosition = new Vector3(x, y, z);
            }
        }
        private void UpdateGameObjectLocalPosition2DImpl(int id, float x, float z, bool attachTerrain)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                float y = 0;
                if (attachTerrain)
                    y = SampleTerrainHeight(x, z);
                else
                    y = obj.transform.localPosition.y;
                obj.transform.localPosition = new Vector3(x, y, z);
            }
        }
        private void UpdateGameObjectLocalRotateImpl(int id, float rx, float ry, float rz)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                obj.transform.localRotation = Quaternion.Euler(RadianToDegree(rx), RadianToDegree(ry), RadianToDegree(rz));
            }
        }
        private void UpdateGameObjectLocalRotateYImpl(int id, float ry)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                float rx = obj.transform.localRotation.eulerAngles.x;
                float rz = obj.transform.localRotation.eulerAngles.z;
                obj.transform.localRotation = Quaternion.Euler(rx, RadianToDegree(ry), rz);
            }
        }
        private void UpdateGameObjectLocalScaleImpl(int id, float sx, float sy, float sz)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                obj.transform.localScale = new Vector3(sx, sy, sz);
            }
        }
        private void AttachGameObjectImpl(int id, int parentId, float x, float y, float z, float rx, float ry, float rz)
        {
            GameObject obj = GetGameObject(id);
            GameObject parent = GetGameObject(parentId);
            if (null != obj && null != obj.transform && null != parent && null != parent.transform)
            {
                obj.transform.parent = parent.transform;
                obj.transform.localPosition = new Vector3(x, y, z);
                obj.transform.localRotation = Quaternion.Euler(RadianToDegree(rx), RadianToDegree(ry), RadianToDegree(rz));
            }
        }
        private void AttachGameObjectImpl(int id, int parentId, string path, float x, float y, float z, float rx, float ry, float rz)
        {
            GameObject obj = GetGameObject(id);
            GameObject parent = GetGameObject(parentId);
            if (null != obj && null != obj.transform && null != parent && null != parent.transform)
            {
                Transform t = FindChildRecursive(parent.transform, path);
                if (null != t)
                {
                    obj.transform.parent = t;
                    obj.transform.localPosition = new Vector3(x, y, z);
                    obj.transform.localRotation = Quaternion.Euler(RadianToDegree(rx), RadianToDegree(ry), RadianToDegree(rz));
                }
                else
                {
                    CallLogicLog("Obj {0} AttachGameObject {1} can't find bone {2}", id, parentId, path);
                }
            }
        }
        private void DetachGameObjectImpl(int id)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.transform)
            {
                obj.transform.parent = null;
            }
        }
        private void SetGameObjectVisibleImpl(int id, bool visible)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                Component[] renderers = obj.GetTypedComponents(ObjectType.Renderer, true);
                if (renderers != null)
                {
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        ((Renderer)renderers[i]).enabled = visible;
                    }
                }
            }
        }
        private void PlayAnimationImpl(int id, bool isStopAll)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    obj.animation.Play(isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
                }
                catch
                {
                }
            }
        }
        private void PlayAnimationImpl(int id, string animationName, bool isStopAll)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    if (null != obj.animation[animationName])
                    {
                        obj.animation.Play(animationName, isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
                        //CallLogicLog("Obj {0} PlayerAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} PlayerAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void StopAnimationImpl(int id, string animationName)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    if (null != obj.animation[animationName])
                    {
                        obj.animation.Stop(animationName);
                        //CallLogicLog("Obj {0} StopAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} StopAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void StopAnimationImpl(int id)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    obj.animation.Stop();
                }
                catch
                {
                }
            }
        }
        private void BlendAnimationImpl(int id, string animationName, float weight, float fadeLength)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    AnimationState state = obj.animation[animationName];
                    if (null != state)
                    {
                        obj.animation.Blend(animationName, weight, fadeLength);
                        //CallLogicLog("Obj {0} BlendAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} BlendAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void CrossFadeAnimationImpl(int id, string animationName, float fadeLength, bool isStopAll)
        {
            GameObject obj = GetGameObject(id);
            SharedGameObjectInfo obj_info = GetSharedGameObjectInfo(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    if (null != obj.animation[animationName] && obj_info != null && !obj_info.IsGfxAnimation)
                    {
                        obj.animation.CrossFade(animationName, fadeLength, isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
                        //CallLogicLog("Obj {0} CrossFadeAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                    else
                    {
                        if (null == obj.animation[animationName])
                        {
                            CallLogicErrorLog("Obj {0} CrossFadeAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                        }
                        if (null == obj_info)
                        {
                            CallLogicErrorLog("Obj {0} CrossFadeAnimation {1} obj_info is null, obj name {2}", id, animationName, obj.name);
                        }
                    }
                }
                catch
                {
                }
            }
        }
        private void PlayQueuedAnimationImpl(int id, string animationName, bool isPlayNow, bool isStopAll)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    if (null != obj.animation[animationName])
                    {
                        obj.animation.PlayQueued(animationName, isPlayNow ? QueueMode.PlayNow : QueueMode.CompleteOthers, isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
                        //CallLogicLog("Obj {0} PlayQueuedAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} PlayQueuedAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void CrossFadeQueuedAnimationImpl(int id, string animationName, float fadeLength, bool isPlayNow, bool isStopAll)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    if (null != obj.animation[animationName])
                    {
                        obj.animation.CrossFadeQueued(animationName, fadeLength, isPlayNow ? QueueMode.PlayNow : QueueMode.CompleteOthers, isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
                        //CallLogicLog("Obj {0} CrossFadeQueuedAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} CrossFadeQueuedAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void RewindAnimationImpl(int id, string animationName)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    if (null != obj.animation[animationName])
                    {
                        obj.animation.Rewind(animationName);
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} RewindAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void RewindAnimationImpl(int id)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    obj.animation.Rewind();
                }
                catch
                {
                }
            }
        }
        private void SetAnimationSpeedImpl(int id, string animationName, float speed)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    AnimationState state = obj.animation[animationName];
                    if (null != state)
                    {
                        state.speed = speed;
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} SetAnimationSpeed {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void SetAnimationSpeedByTimeImpl(int id, string animationName, float time)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    AnimationState state = obj.animation[animationName];
                    if (null != state)
                    {
                        state.speed = state.length / state.time;
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} SetAnimationSpeedByTime {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void SetAnimationWeightImpl(int id, string animationName, float weight)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    AnimationState state = obj.animation[animationName];
                    if (null != state)
                    {
                        state.weight = weight;
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} SetAnimationWeight {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void SetAnimationLayerImpl(int id, string animationName, int layer)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    AnimationState state = obj.animation[animationName];
                    if (null != state)
                    {
                        state.layer = layer;
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} SetAnimationLayer {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void SetAnimationBlendModeImpl(int id, string animationName, int blendMode)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation)
            {
                try
                {
                    AnimationState state = obj.animation[animationName];
                    if (null != state)
                    {
                        state.blendMode = (AnimationBlendMode)blendMode;
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} SetAnimationBlendMode {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void AddMixingTransformAnimationImpl(int id, string animationName, string path, bool recursive)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation && null != obj.transform)
            {
                try
                {
                    AnimationState state = obj.animation[animationName];
                    if (null != state)
                    {
                        Transform t = obj.transform.Find(path);
                        if (null != t)
                        {
                            state.AddMixingTransform(t, recursive);
                        }
                        else
                        {
                            CallLogicErrorLog("Obj {0} AddMixingTransformAnimation {1} Can't find bone {2}", id, animationName, path);
                        }
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} AddMixingTransformAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void RemoveMixingTransformAnimationImpl(int id, string animationName, string path)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.animation && null != obj.transform)
            {
                try
                {
                    AnimationState state = obj.animation[animationName];
                    if (null != state)
                    {
                        Transform t = obj.transform.Find(path);
                        if (null != t)
                        {
                            state.RemoveMixingTransform(t);
                        }
                        else
                        {
                            CallLogicErrorLog("Obj {0} RemoveMixingTransformAnimation {1} Can't find bone {2}", id, animationName, path);
                        }
                    }
                    else
                    {
                        CallLogicErrorLog("Obj {0} RemoveMixingTransformAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
                    }
                }
                catch
                {
                }
            }
        }
        private void SetCharacterControllerEnableImpl(int id, bool isEnable)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                CharacterController cc = obj.GetTypedComponent(ObjectType.CharacterController) as CharacterController;
                if (null != cc)
                {
                    cc.enabled = isEnable;
                }
            }
        }
        private AudioSource GetAudioSource(GameObject obj, string source_obj_name)
        {
            if (obj == null)
            {
                return null;
            }
            Component[] audiosources = obj.GetTypedComponents(ObjectType.AudioSource, true);
            for (int i = 0; i < audiosources.Length; i++)
            {
                if (audiosources[i].gameObject.name.Equals(source_obj_name))
                {
                    return (AudioSource)audiosources[i];
                }
            }
            return null;
        }
        private void PlaySoundImpl(int id, string audiosource, float pitch)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                AudioSource audio_source = obj.audio;
                if (audio_source == null)
                {
                    CallLogicErrorLog("id={0} obj name {1} can't find audiosource {2}! can't play sound!", id, obj.name, audiosource);
                    return;
                }
                AudioClip clip = ResourceSystem.GetSharedResource(audiosource) as AudioClip;
                if (null != clip)
                {
                    audio_source.clip = clip;
                    audio_source.dopplerLevel = 0;
                    audio_source.Play();
                }
                else
                {
                    CallLogicErrorLog("id={0} obj name {1} can't find audioclip {2}! can't play sound!", id, obj.name, audiosource);
                    return;
                }
            }
        }
        private void SetAudioSourcePitchImpl(int id, string audiosource, float pitch)
        {
            GameObject obj = GetGameObject(id);
            AudioSource target_audio_source = GetAudioSource(obj, audiosource);
            if (target_audio_source == null)
            {
                CallLogicErrorLog("id={0} obj can't find audiosource {1}! can't set sound pitch!", id, audiosource);
                return;
            }
            target_audio_source.pitch = pitch;
        }
        private void StopSoundImpl(int id, string audiosource)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                AudioSource audio_source = obj.audio;
                if (audio_source == null)
                {
                    CallLogicErrorLog("id={0} obj can't find audiosource {1}! can't play sound!", id, audiosource);
                    return;
                }
                if (audio_source.clip != null && audio_source.clip.name == audiosource)
                {
                    audio_source.clip = null;
                }
            }
        }
        private void SetShaderImpl(int id, string shaderPath)
        {
            GameObject obj = GetGameObject(id);
            if (null == obj)
            {
                return;
            }
            Shader shader = Shader.Find(shaderPath);
            if (null == shader)
            {
                CallLogicErrorLog("id={0} obj can't find shader {1}!", id, shaderPath);
                return;
            }
            Component[] renderers = obj.GetTypedComponents(ObjectType.SkinnedMeshRenderer, true);
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; ++i)
                {
                    if (((SkinnedMeshRenderer)renderers[i]).material.shader != shader)
                    {
                        ((SkinnedMeshRenderer)renderers[i]).material.shader = shader;
                    }
                }
            }
        }
        private void SetBlockedShaderImpl(int id, uint rimColor, float rimPower, float cutValue)
        {
            GameObjectInfo objInfo = GetGameObjectInfo(id);
            if (null == objInfo || null == objInfo.ObjectInstance || null == objInfo.ObjectInfo)
            {
                return;
            }
            bool needChange = true;
            Component[] skinnedRenderers = objInfo.ObjectInstance.GetTypedComponents(ObjectType.SkinnedMeshRenderer, true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = (SkinnedMeshRenderer)skinnedRenderers[i];
                for (int j = 0; j < renderer.materials.Length; j++)
                {
                    string name = renderer.materials[j].shader.name;
                    if (0 == name.CompareTo("DFM/Blocked"))
                    {
                        needChange = false;
                    }
                }
            }
            Component[] meshRenderers = objInfo.ObjectInstance.GetTypedComponents(ObjectType.MeshRenderer);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                MeshRenderer renderer = (MeshRenderer)meshRenderers[i];
                for (int j = 0; j < renderer.materials.Length; j++)
                {
                    string name = renderer.materials[j].shader.name;
                    if (0 == name.CompareTo("DFM/Blocked"))
                    {
                        needChange = false;
                    }
                }
            }
            if (needChange)
            {
                byte rb = (byte)((rimColor & 0xFF000000) >> 24);
                byte gb = (byte)((rimColor & 0x00FF0000) >> 16);
                byte bb = (byte)((rimColor & 0x0000FF00) >> 8);
                byte ab = (byte)(rimColor & 0x000000FF);
                float r = (float)rb / 255.0f;
                float g = (float)gb / 255.0f;
                float b = (float)bb / 255.0f;
                float a = (float)ab / 255.0f;
                Color c = new Color(r, g, b, a);

                Shader blocked = Shader.Find("DFM/Blocked");
                Shader notBlocked = Shader.Find("DFM/NotBlocked");
                if (null == blocked)
                {
                    CallLogicLog("id={0} obj can't find shader DFM/Blocked !", id);
                    return;
                }
                if (null == notBlocked)
                {
                    CallLogicLog("id={0} obj can't find shader DFM/NotBlocked !", id);
                    return;
                }
                for (int i = 0; i < skinnedRenderers.Length; i++)
                {
                    SkinnedMeshRenderer renderer = (SkinnedMeshRenderer)skinnedRenderers[i];
                    objInfo.ObjectInfo.m_SkinedMaterialChanged = true;
                    Texture texture = renderer.material.mainTexture;

                    //Material blockedMat = new Material(blocked);
                    Material blockedMat = Material.Create(blocked);
                    Material notBlockedMat = renderer.material;//new Material(notBlocked);
                    Material[] mats = new Material[]{
            notBlockedMat,
            blockedMat
          };
                    blockedMat.SetColor("_RimColor", c);
                    blockedMat.SetFloat("_RimPower", rimPower);
                    blockedMat.SetFloat("_CutValue", cutValue);
                    //notBlockedMat.SetTexture("_MainTex", texture);

                    renderer.materials = mats;
                }
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    MeshRenderer renderer = (MeshRenderer)meshRenderers[i];
                    objInfo.ObjectInfo.m_MeshMaterialChanged = true;
                    Texture texture = renderer.material.mainTexture;

                    Material blockedMat = Material.Create(blocked);
                    Material notBlockedMat = renderer.material;//new Material(notBlocked);
                    Material[] mats = new Material[]{
            notBlockedMat,
            blockedMat
          };
                    blockedMat.SetColor("_RimColor", c);
                    blockedMat.SetFloat("_RimPower", rimPower);
                    blockedMat.SetFloat("_CutValue", cutValue);
                    //notBlockedMat.SetTexture("_MainTex", texture);

                    renderer.materials = mats;
                }
            }
        }
        private void RestoreMaterialImpl(int id)
        {
            GameObjectInfo objInfo = GetGameObjectInfo(id);
            if (null == objInfo)
            {
                return;
            }
            GameObject obj = objInfo.ObjectInstance;
            SharedGameObjectInfo info = objInfo.ObjectInfo;
            if (null != obj && null != info)
            {
                if (info.m_SkinedMaterialChanged)
                {
                    Component[] renderers = obj.GetTypedComponents(ObjectType.SkinnedMeshRenderer, true);
                    int ix = 0;
                    int ct = info.m_SkinedOriginalMaterials.Count;
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        if (ix < ct)
                        {
                            ((SkinnedMeshRenderer)renderers[i]).materials = info.m_SkinedOriginalMaterials[ix] as Material[];
                            ++ix;
                        }
                    }
                    info.m_SkinedMaterialChanged = false;
                }
                if (info.m_MeshMaterialChanged)
                {
                    Component[] renderers = obj.GetTypedComponents(ObjectType.MeshRenderer);
                    int ix = 0;
                    int ct = info.m_MeshOriginalMaterials.Count;
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        if (ix < ct)
                        {
                            ((MeshRenderer)renderers[i]).materials = info.m_MeshOriginalMaterials[ix] as Material[];
                            ++ix;
                        }
                    }
                    info.m_MeshMaterialChanged = false;
                }
            }
        }

        private void AddForceImpl(int id, float x, float y, float z)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj && null != obj.rigidbody)
            {
                obj.rigidbody.AddForce(new Vector3(x, y, z));
            }
            else
            {
                CallLogicErrorLog("id={0} obj can't find rigidbody!", id);
            }
        }
        private void SetRigidbodyVelocityImpl(int id, float x, float y, float z)
        {
            GameObject obj = GetGameObject(id);
            if (null == obj) return;
            if (null != obj.rigidbody)
            {
                obj.rigidbody.velocity = new Vector3(x, y, z);
            }
            else
            {
                CallLogicErrorLog("id={0} obj can't find rigidbody!", id);
            }
        }
        private void SetTimeScaleImpl(float scale)
        {
            Time.timeScale = scale;
        }
        private void SetLayerImpl(int id, string layer)
        {
            GameObject obj = GetGameObject(id);
            if (null != obj)
            {
                obj.layer = LayerMask.NameToLayer(layer);
            }
            else
            {
                CallLogicErrorLog("id={0} obj can't find!", id);
            }
        }

        private void GfxLogImpl(string msg)
        {
            LogSystem.GfxLog(Log_Type.LT_Info, msg);
        }
        private void GfxErrorLogImpl(string error)
        {
            LogSystem.GfxLog(Log_Type.LT_Error, error);
        }
        private void PublishGfxEventImpl(string evt, string group, object[] args)
        {
            m_EventChannelForGfx.Publish(evt, group, args);
        }
        private void ProxyPublishGfxEventImpl(string evt, string group, object[] args)
        {
            m_EventChannelForGfx.ProxyPublish(evt, group, args);
        }
        private void SendMessageImpl(string objname, string msg, object arg, bool needReceiver)
        {
            GameObject obj = GameObject.Find(objname);
            if (null != obj)
            {
                try
                {
                    obj.SendMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
                }
                catch
                {

                }
            }
        }
        private void SendMessageByIdImpl(int objid, string msg, object arg, bool needReceiver)
        {
            GameObject obj = GetGameObject(objid);
            if (null != obj)
            {
                try
                {
                    obj.SendMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
                }
                catch
                {

                }
            }
        }
        private void SendMessageWithTagImpl(string objtag, string msg, object arg, bool needReceiver)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(objtag);
            if (null != objs)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    try
                    {
                        objs[i].SendMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
                    }
                    catch
                    {
                    }
                }
                /*
                foreach (GameObject obj in objs) {
                  try {
                    obj.SendMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
                  } catch {
                  }
                }*/
            }
        }
        //游戏逻辑层执行的函数，供Gfx线程异步调用
        private void PublishLogicEventImpl(string evt, string group, object[] args)
        {
            m_EventChannelForLogic.Publish(evt, group, args);
        }
        private void ProxyPublishLogicEventImpl(string evt, string group, object[] args)
        {
            m_EventChannelForLogic.ProxyPublish(evt, group, args);
        }

        //Gfx线程执行的函数，对游戏逻辑线程的异步调用由这里发起
        internal float SampleTerrainHeight(float x, float z)
        {
            float y = c_MinTerrainHeight;
            if (null != Terrain.activeTerrain)
            {
                y = Terrain.activeTerrain.SampleHeight(new Vector3(x, c_MinTerrainHeight, z));
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(x, c_MinTerrainHeight * 2, z), Vector3.down, out hit, c_MinTerrainHeight * 2, 1 << LayerMask.NameToLayer("Terrains")))
                {
                    y = hit.point.y;
                }
            }
            return y;
        }
        internal void SetLoadingBarScene(string name)
        {
            m_LoadingBarScene = name;
        }
        internal GameObject GetGameObject(int id)
        {
            GameObject ret = null;
            GameObjectInfo info;
            if (m_GameObjects.TryGetValue(id, out info))
                ret = info.ObjectInstance;
            return ret;
        }
        internal SharedGameObjectInfo GetSharedGameObjectInfo(int id)
        {
            SharedGameObjectInfo ret = null;
            GameObjectInfo info;
            if (m_GameObjects.TryGetValue(id, out info))
                ret = info.ObjectInfo;
            return ret;
        }
        internal SharedGameObjectInfo GetSharedGameObjectInfo(GameObject obj)
        {
            SharedGameObjectInfo ret = GetGameObjectInfoByObj(obj);
            return ret;
        }
        internal bool ExistGameObject(GameObject obj)
        {
            int id = GetGameObjectId(obj);
            return id > 0;
        }
        internal GameObject PlayerSelf
        {
            get
            {
                if (null != m_PlayerSelf)
                    return m_PlayerSelf.ObjectInstance;
                else
                    return null;
            }
        }
        internal SharedGameObjectInfo PlayerSelfInfo
        {
            get
            {
                if (null != m_PlayerSelf)
                    return m_PlayerSelf.ObjectInfo;
                else
                    return null;
            }
        }
        internal void CallLogicLog(string format, params object[] args)
        {
            QueueLogicActionWithDelegation(m_LogicLogCallback, false, format, args);
        }
        internal void CallLogicErrorLog(string format, params object[] args)
        {
            QueueLogicActionWithDelegation(m_LogicLogCallback, true, format, args);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        internal void CallGfxLog(string format, params object[] args)
        {
#if DEBUG
            string msg = string.Format(format, args);
            GfxLogImpl(msg);
#endif
        }
        [System.Diagnostics.Conditional("DEBUG")]
        internal void CallGfxErrorLog(string format, params object[] args)
        {
#if DEBUG
            string msg = string.Format(format, args);
            GfxErrorLogImpl(msg);
#endif
        }
        internal float RadianToDegree(float dir)
        {
            return (float)(dir * 180 / Math.PI);
        }
        internal bool SceneResourcePrepared
        {
            get { return m_SceneResourcePrepared; }
            set { m_SceneResourcePrepared = value; }
        }
        internal float SceneResourcePreparedProgress
        {
            get { return m_SceneResourcePreparedProgress; }
            set { m_SceneResourcePreparedProgress = value; }
        }
        internal void BeginLoading()
        {
            m_LoadingProgress = 0;
            EventChannelForGfx.Publish("ge_loading_start", "ui");
        }
        internal void EndLoading()
        {
            m_LoadingProgress = 1;
            //延迟处理，在逻辑层逻辑处理之后通知loading条结束，同时也让loading条能走完（视觉效果）。
            if (null != m_LogicInvoker)
            {
                m_LogicInvoker.QueueAction(NotifyGfxEndloading);
            }
        }
        internal void UpdateLoadingProgress(float progress)
        {
            m_LoadingProgress = progress;
        }
        internal void UpdateLoadingTip(string tip)
        {
            m_LoadingTip = tip;
        }
        internal void UpdateRandomLoadingTip()
        {
            EventChannelForGfx.Publish("ge_loading_tip_random", "ui");
        }
        internal void UpdateVersionInfo(string info)
        {
            m_VersionInfo = info;
        }
        internal float GetLoadingProgress()
        {
            return m_LoadingProgress;
        }
        internal string GetLoadingTip()
        {
            return m_LoadingTip;
        }
        internal string GetVersionInfo()
        {
            return m_VersionInfo;
        }
        internal Transform FindChildRecursive(Transform parent, string bonePath)
        {
            Transform t = parent.Find(bonePath);
            if (null != t)
            {
                return t;
            }
            else
            {
                int ct = parent.childCount;
                for (int i = 0; i < ct; ++i)
                {
                    t = FindChildRecursive(parent.GetChild(i), bonePath);
                    if (null != t)
                    {
                        return t;
                    }
                }
            }
            return null;
        }
        internal IActionQueue LogicInvoker
        {
            get { return m_LogicInvoker; }
        }
        internal void QueueLogicActionWithDelegation(Delegate action, params object[] args)
        {
            if (null != m_LogicInvoker)
            {
                m_LogicInvoker.QueueActionWithDelegation(action, args);
            }
        }
        internal void PublishLogicEvent(string evt, string group, object[] args)
        {
            if (null != m_LogicInvoker)
            {
                m_LogicInvoker.QueueActionWithDelegation((MyAction<string, string, object[]>)PublishLogicEventImpl, evt, group, args);
            }
        }
        internal void ProxyPublishLogicEvent(string evt, string group, object[] args)
        {
            if (null != m_LogicInvoker)
            {
                m_LogicInvoker.QueueActionWithDelegation((MyAction<string, string, object[]>)ProxyPublishLogicEventImpl, evt, group, args);
            }
        }
        internal PublishSubscribeSystem EventChannelForGfx
        {
            get { return m_EventChannelForGfx; }
        }
        internal IGameLogicNotification GameLogicNotification
        {
            get { return m_GameLogicNotification; }
        }
        internal BeforeLoadSceneDelegation OnBeforeLoadScene
        {
            get { return m_OnBeforeLoadScene; }
            set { m_OnBeforeLoadScene = value; }
        }
        internal AfterLoadSceneDelegation OnAfterLoadScene
        {
            get { return m_OnAfterLoadScene; }
            set { m_OnAfterLoadScene = value; }
        }
        internal void VisitGameObject(MyAction<GameObject, SharedGameObjectInfo> visitor)
        {
            if (Monitor.TryEnter(m_SyncLock))
            {
                try
                {
                    for (LinkedListNode<GameObjectInfo> node = m_GameObjects.FirstValue; null != node; node = node.Next)
                    {
                        GameObjectInfo info = node.Value;
                        if (null != info && null != info.ObjectInstance)
                        {
                            visitor(info.ObjectInstance, info.ObjectInfo);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(m_SyncLock);
                }
            }
        }

        private void HandleSync()
        {
            if (Monitor.TryEnter(m_SyncLock))
            {
                try
                {
                    for (LinkedListNode<GameObjectInfo> node = m_GameObjects.FirstValue; null != node; node = node.Next)
                    {
                        GameObjectInfo info = node.Value;
                        if (null != info && null != info.ObjectInstance && null != info.ObjectInfo)
                        {
                            if (info.ObjectInfo.DataChangedByLogic)
                            {
                                Vector3 pos = new Vector3(info.ObjectInfo.X, info.ObjectInfo.Y, info.ObjectInfo.Z);
                                //if (!info.ObjectInfo.IsFloat && pos.y <= c_MinTerrainHeight)
                                //  pos.y = SampleTerrainHeight(pos.x, pos.z);
                                GameObject obj = info.ObjectInstance;
                                Vector3 old = obj.transform.position;
                                CharacterController ctrl = obj.GetTypedComponent(ObjectType.CharacterController) as CharacterController;
                                if (null != ctrl)
                                {
                                    ctrl.Move(pos - old);
                                }
                                else
                                {
                                    info.ObjectInstance.transform.position = pos;
                                }
                                info.ObjectInstance.transform.rotation = Quaternion.Euler(0, RadianToDegree(info.ObjectInfo.FaceDir), 0);

                                info.ObjectInfo.DataChangedByLogic = false;
                            }
                            else
                            {
                                if (!info.ObjectInfo.IsGfxMoveControl)
                                {
                                    if (info.ObjectInfo.IsLogicMoving)
                                    {
                                        GameObject obj = info.ObjectInstance;
                                        Vector3 old = obj.transform.position;
                                        Vector3 pos;
                                        float distance = info.ObjectInfo.MoveSpeed * Time.deltaTime;
                                        if (distance * distance < info.ObjectInfo.MoveTargetDistanceSqr)
                                        {
                                            float dz = distance * info.ObjectInfo.MoveCos;
                                            float dx = distance * info.ObjectInfo.MoveSin;

                                            if (info.ObjectInfo.CurTime + Time.deltaTime < info.ObjectInfo.TotalTime)
                                            {
                                                info.ObjectInfo.CurTime += Time.deltaTime;
                                                float scale = Time.deltaTime / info.ObjectInfo.TotalTime;
                                                dx += info.ObjectInfo.AdjustDx * scale;
                                                dz += info.ObjectInfo.AdjustDz * scale;
                                            }
                                            else
                                            {
                                                info.ObjectInfo.TotalTime = 0;
                                            }

                                            CharacterController ctrl = obj.GetTypedComponent(ObjectType.CharacterController) as CharacterController;
                                            if (null != ctrl)
                                            {
                                                ctrl.Move(new Vector3(dx, 0, dz));
                                                pos = obj.transform.position;
                                                //if (!info.ObjectInfo.IsFloat && pos.y <= c_MinTerrainHeight) {
                                                //  pos.y = SampleTerrainHeight(pos.x, pos.z);
                                                //  obj.transform.position = pos;
                                                //}
                                                if (info == m_PlayerSelf && ctrl.collisionFlags == CollisionFlags.Sides)
                                                {
                                                    if (null != m_GameLogicNotification && null != m_LogicInvoker)
                                                    {
                                                        m_LogicInvoker.QueueActionWithDelegation((MyAction<int>)m_GameLogicNotification.OnGfxMoveMeetObstacle, info.ObjectInfo.m_LogicObjectId);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                pos = old + new Vector3(dx, 0, dz);
                                                info.ObjectInstance.transform.position = pos;
                                            }

                                            info.ObjectInfo.X = pos.x;
                                            info.ObjectInfo.Y = pos.y;
                                            info.ObjectInfo.Z = pos.z;
                                            info.ObjectInfo.DataChangedByGfx = true;
                                        }
                                    }
                                    Vector3 nowPos = info.ObjectInstance.transform.position;
                                    float terrainHeight = SampleTerrainHeight(nowPos.x, nowPos.z);
                                    if (!info.ObjectInfo.IsFloat && nowPos.y > terrainHeight)
                                    {
                                        float cur_height = nowPos.y + info.ObjectInfo.VerticlaSpeed * Time.deltaTime - 9.8f * Time.deltaTime * Time.deltaTime / 2;
                                        if (cur_height < terrainHeight)
                                        {
                                            cur_height = terrainHeight;
                                        }
                                        info.ObjectInfo.VerticlaSpeed += -9.8f * Time.deltaTime;
                                        CharacterController cc = info.ObjectInstance.GetTypedComponent(ObjectType.CharacterController) as CharacterController;
                                        if (null != cc)
                                        {
                                            cc.Move(new Vector3(nowPos.x, cur_height, nowPos.z) - nowPos);
                                        }
                                        else
                                        {
                                            info.ObjectInstance.transform.position = new Vector3(nowPos.x, cur_height, nowPos.z);
                                        }
                                        info.ObjectInfo.Y = cur_height;
                                        info.ObjectInfo.DataChangedByGfx = true;
                                    }
                                    else
                                    {
                                        info.ObjectInfo.VerticlaSpeed = 0;
                                    }

                                    if (info.FaceDir != info.ObjectInfo.FaceDir)
                                    {
                                        info.ObjectInstance.transform.rotation = Quaternion.Euler(RadianToDegree(0), RadianToDegree(info.ObjectInfo.FaceDir), RadianToDegree(0));
                                        info.FaceDir = info.ObjectInfo.FaceDir;
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(m_SyncLock);
                }
            }
        }
        private void HandleLoadingProgress()
        {
            if (GlobalVariables.Instance.IsPublish)
            {
                if (m_LoadScenePaused) return;
                //先等待loading bar加载完成,发起对目标场景的加载
                if (null != m_LoadingBarAsyncOperation)
                {
                    if (m_LoadingBarAsyncOperation.isDone)
                    {
                        m_LoadingBarAsyncOperation = null;
                        CallLogicLog("HandleLoadingProgress m_LoadingBarAsyncOperation.isDone");
                        if (null != m_OnBeforeLoadScene)
                        {
                            m_OnBeforeLoadScene(Application.loadedLevelName, m_TargetScene, m_TargetSceneId);
                        }
                        ResourceManager.Instance.CleanupResourcePool();
                        m_LoadCacheResInfo = ResUpdateHandler.CacheResByConfig(m_TargetSceneId);
                    }
                }
                else if (m_LoadCacheResInfo != null)
                {
                    if (m_LoadCacheResInfo.IsDone)
                    {
                        m_LoadCacheResInfo = null;
                        CallLogicLog("HandleLoadingProgress m_LoadCacheResInfo.IsDone");
                        if (null != m_LogicInvoker && null != m_LevelLoadedCallback)
                        {
                            QueueLogicActionWithDelegation(m_LevelLoadedCallback);
                            m_LevelLoadedCallback = null;
                        }
                        Resources.UnloadUnusedAssets();
                        EndLoading();
                        CallLogicLog("End LoadScene:{0}", m_TargetScene);
                        if (null != m_OnAfterLoadScene)
                        {
                            m_OnAfterLoadScene(m_TargetScene, m_TargetSceneId);
                        }
                    }
                    else if (m_LoadCacheResInfo.IsError)
                    {
                        CallLogicLog("HandleLoadingProgress m_LoadCacheResInfo.IsError");
                        ReStartLoad();
                    }
                    else
                    {
                        UpdateLoadingProgress(0.5f + m_LoadCacheResInfo.Progress * 0.5f);
                    }
                }
            }
            else
            {
                //先等待loading bar加载完成,发起对目标场景的加载
                if (null != m_LoadingBarAsyncOperation)
                {
                    if (m_LoadingBarAsyncOperation.isDone)
                    {
                        m_LoadingBarAsyncOperation = null;
                        CallLogicLog("HandleLoadingProgress m_LoadingBarAsyncOperation.isDone");
                        if (null != m_OnBeforeLoadScene)
                        {
                            m_OnBeforeLoadScene(Application.loadedLevelName, m_TargetScene, m_TargetSceneId);
                        }
                        ResourceManager.Instance.CleanupResourcePool();
                        m_LoadingLevelAsyncOperation = Application.LoadLevelAsync(m_TargetScene);
                    }
                }
                else if (null != m_LoadingLevelAsyncOperation)
                {//再等待目标场景加载
                    if (m_LoadingLevelAsyncOperation.isDone)
                    {
                        m_LoadingLevelAsyncOperation = null;
                        CallLogicLog("HandleLoadingProgress m_LoadingLevelAsyncOperation.IsDone");
                        if (null != m_LogicInvoker && null != m_LevelLoadedCallback)
                        {
                            QueueLogicActionWithDelegation(m_LevelLoadedCallback);
                            m_LevelLoadedCallback = null;
                        }
                        Resources.UnloadUnusedAssets();
                        GC.Collect();
                        EndLoading();
                        CallLogicLog("End LoadScene:{0}", m_TargetScene);
                        if (null != m_OnAfterLoadScene)
                        {
                            m_OnAfterLoadScene(m_TargetScene, m_TargetSceneId);
                        }
                    }
                    else
                    {
                        UpdateLoadingProgress(0.5f + m_LoadingLevelAsyncOperation.progress * 0.5f);
                    }
                }
            }
        }
        private void ReStartLoad()
        {
            ResUpdateHandler.IncReconnectNum();
            m_LoadScenePaused = true;
            string info = Dict.Format(27, (int)ResUpdateHandler.GetUpdateError());
            string dlgButton = Dict.Get(4);
            Action<bool> fun = new Action<bool>(delegate (bool selected)
            {
                if (selected)
                {
                    m_LoadScenePaused = false;
                    m_LoadCacheResInfo = null;
                    m_UpdateChapterInfo = null;
                    m_LoadingBarAsyncOperation = null;
                    m_LoadingLevelAsyncOperation = null;
                    ResUpdateHandler.ExitUpdate();
                    LoadSceneImpl(m_TargetScene, m_TargetChapter, m_TargetSceneId, m_TargetSceneLimitList, m_LevelLoadedCallback);
                }
            });
            LogicSystem.EventChannelForGfx.Publish("ge_show_yesornot", "ui", info, dlgButton, fun);
        }
        private GameObjectInfo GetGameObjectInfo(int id)
        {
            GameObjectInfo ret = null;
            m_GameObjects.TryGetValue(id, out ret);
            return ret;
        }
        private int GetGameObjectId(GameObject obj)
        {
            int ret = 0;
            m_GameObjectIds.TryGetValue(obj, out ret);
            return ret;
        }

        private SharedGameObjectInfo GetGameObjectInfoByObj(GameObject obj)
        {
            SharedGameObjectInfo ret = null;
            GameObjectInfo goi;
            if (m_GameObject2ObjInfo.TryGetValue(obj, out goi))
            {
                ret = goi.ObjectInfo;
            }
            return ret;
        }

        private void RememberGameObject(int id, GameObject obj)
        {
            RememberGameObject(id, obj, null);
        }
        private void RememberGameObject(int id, GameObject obj, SharedGameObjectInfo info)
        {
            GameObjectInfo objInfoRef = null;
            if (m_GameObjects.TryGetValue(id, out objInfoRef))
            {
                GameObject oldObj = m_GameObjects[id].ObjectInstance;
                ResourceManager.Instance.SetActiveOptim(oldObj, false);
                //oldObj.SetActive(false);
                m_GameObjectIds.Remove(oldObj);
                m_GameObject2ObjInfo.Remove(oldObj);
                GameObject.Destroy(oldObj);
                objInfoRef.ObjectInfo = info;
                objInfoRef.ObjectInstance = obj;

            }
            else
            {
                objInfoRef = new GameObjectInfo(obj, info);
                m_GameObjects.AddLast(id, objInfoRef);
            }
            if (null != info)
            {
                if (!info.m_SkinedMaterialChanged)
                {
                    Component[] renderers = obj.GetTypedComponents(ObjectType.SkinnedMeshRenderer, true);
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        info.m_SkinedOriginalMaterials.Add(((SkinnedMeshRenderer)renderers[i]).materials);
                    }
                    /*
                    foreach (SkinnedMeshRenderer renderer in renderers) {
                      info.m_SkinedOriginalMaterials.Add(renderer.materials);
                    }*/
                }
                if (!info.m_MeshMaterialChanged)
                {
                    Component[] renderers = obj.GetTypedComponents(ObjectType.MeshRenderer);
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        info.m_MeshOriginalMaterials.Add(((MeshRenderer)renderers[i]).materials);
                    }
                    /*
                    foreach (MeshRenderer renderer in renderers) {
                      info.m_MeshOriginalMaterials.Add(renderer.materials);
                    }*/
                }
            }
            m_GameObjectIds.Add(obj, id);
            m_GameObject2ObjInfo.Add(obj, objInfoRef);
        }
        private void ForgetGameObject(int id, GameObject obj)
        {
            SharedGameObjectInfo info = GetSharedGameObjectInfo(id);
            if (null != info)
            {
                RestoreMaterialImpl(id);
                info.m_SkinedOriginalMaterials.Clear();
                info.m_MeshOriginalMaterials.Clear();
            }
            m_GameObjects.Remove(id);
            m_GameObjectIds.Remove(obj);
            m_GameObject2ObjInfo.Remove(obj);
        }
        private void CreateGameObjectWithMeshDataHelper(int id, List<float> vertices, List<float> uvs, List<int> triangles, Material mat, bool attachTerrain)
        {
            GameObject obj = GameObject.Create();
            MeshFilter meshFilter = obj.AddTypedComponent(ObjectType.MeshFilter) as MeshFilter;
            MeshRenderer renderer = obj.AddTypedComponent(ObjectType.MeshRenderer) as MeshRenderer;
            Mesh mesh = Mesh.Create();

            Vector3[] _vertices = new Vector3[vertices.Count / 3];
            for (int i = 0; i < _vertices.Length; ++i)
            {
                float x = vertices[i * 3];
                float y = vertices[i * 3 + 1];
                float z = vertices[i * 3 + 2];
                if (attachTerrain)
                    y = SampleTerrainHeight(x, z) + 0.01f;
                _vertices[i] = new Vector3(x, y, z);
            }
            Vector2[] _uvs = new Vector2[uvs.Count / 2];
            for (int i = 0; i < _uvs.Length; ++i)
            {
                float u = uvs[i * 2];
                float v = uvs[i * 2 + 1];
                _uvs[i] = new Vector2(u, v);
            }

            mesh.vertices = _vertices;
            mesh.uv = _uvs;
            mesh.triangles = triangles.ToArray();
            mesh.Optimize();

            meshFilter.mesh = mesh;
            renderer.material = mat;

            RememberGameObject(id, obj);
            ResourceManager.Instance.SetActiveOptim(obj, true);
            //obj.SetActive(true);
        }

        private void NotifyGfxEndloading()
        {
            GfxSystem.PublishGfxEvent("ge_loading_finish", "ui");
        }

        private GfxSystem() { }

        private object m_SyncLock = new object();
        private LinkedListDictionary<int, GameObjectInfo> m_GameObjects = new LinkedListDictionary<int, GameObjectInfo>();
        private MyDictionary<GameObject, int> m_GameObjectIds = new MyDictionary<GameObject, int>();
        private MyDictionary<GameObject, GameObjectInfo> m_GameObject2ObjInfo = new MyDictionary<GameObject, GameObjectInfo>();

        private MyAction<bool, string, object[]> m_LogicLogCallback;

        private IActionQueue m_LogicInvoker;
        private ClientAsyncActionProcessor m_GfxInvoker = new ClientAsyncActionProcessor();

        private PublishSubscribeSystem m_EventChannelForLogic = new PublishSubscribeSystem();
        private PublishSubscribeSystem m_EventChannelForGfx = new PublishSubscribeSystem();

        private bool m_SceneResourcePrepared = false;
        //private bool m_SceneResourceStartPrepare = false;
        private float m_SceneResourcePreparedProgress = 0;
        private ResAsyncInfo m_LoadCacheResInfo = null;
        private ResAsyncInfo m_UpdateChapterInfo = null;
        private bool m_LoadScenePaused = false;
        private AsyncOperation m_LoadingBarAsyncOperation = null;
        private AsyncOperation m_LoadingLevelAsyncOperation = null;
        private MyAction m_LevelLoadedCallback = null;

        private IGameLogicNotification m_GameLogicNotification = null;
        private GameObjectInfo m_PlayerSelf = null;

        private BeforeLoadSceneDelegation m_OnBeforeLoadScene;
        private AfterLoadSceneDelegation m_OnAfterLoadScene;

        private string m_LoadingBarScene = "";
        private int m_TargetSceneId = 0;
        private HashSet<int> m_TargetSceneLimitList = null;
        private string m_TargetScene = "";
        private int m_TargetChapter = 0;
        private float m_LoadingProgress = 0;
        private string m_LoadingTip = "";
        private string m_VersionInfo = "";

        private long m_LastLogTime = 0;

        private float c_MinTerrainHeight = 120.0f;
    }
}
