using System.Collections.Generic;
using StorySystem;

namespace ArkCrossEngine
{
    internal sealed class ClientScriptSystem
    {
        private class ScriptInstanceInfo
        {
            internal int m_StoryId;
            internal StoryInstance m_StoryInstance;
            internal bool m_IsUsed;
        }
        internal void Init()
        {
            
        }
        internal void Reset()
        {
            int count = m_StoryLogicInfos.Count;
            for (int index = count - 1; index >= 0; --index)
            {
                ScriptInstanceInfo info = m_StoryLogicInfos[index];
                if (null != info)
                {
                    RecycleStorylInstance(info);
                    m_StoryLogicInfos.RemoveAt(index);
                }
            }
            m_StoryLogicInfos.Clear();
        }
        internal void PreloadStoryInstance(int storyId)
        {
            ScriptInstanceInfo info = NewStoryInstance(storyId, false);
            if (null != info)
            {
                RecycleStorylInstance(info);
            }
        }
        internal void ClearStoryInstancePool()
        {
            m_StoryInstancePool.Clear();
        }

        internal int ActiveStoryCount
        {
            get
            {
                return m_StoryLogicInfos.Count;
            }
        }
        internal void StartStory(int storyId)
        {
            ScriptInstanceInfo inst = NewStoryInstance(storyId);
            if (null != inst)
            {
                m_StoryLogicInfos.Add(inst);
                inst.m_StoryInstance.Context = WorldSystem.Instance;
                inst.m_StoryInstance.Start();

                LogSystem.Info("StartStory {0}", storyId);
            }
        }
        internal void StopStory(int storyId)
        {
            int count = m_StoryLogicInfos.Count;
            for (int index = count - 1; index >= 0; --index)
            {
                ScriptInstanceInfo info = m_StoryLogicInfos[index];
                if (info.m_StoryId == storyId)
                {
                    RecycleStorylInstance(info);
                    m_StoryLogicInfos.RemoveAt(index);
                }
            }
        }
        internal void Tick()
        {
            long time = TimeUtility.GetLocalMilliseconds();
            int ct = m_StoryLogicInfos.Count;
            for (int ix = ct - 1; ix >= 0; --ix)
            {
                ScriptInstanceInfo info = m_StoryLogicInfos[ix];
                info.m_StoryInstance.Tick(time);
                if (info.m_StoryInstance.IsTerminated)
                {
                    RecycleStorylInstance(info);
                    m_StoryLogicInfos.RemoveAt(ix);
                }
            }
        }
        internal void SendMessage(string msgId, params object[] args)
        {
            int ct = m_StoryLogicInfos.Count;
            for (int ix = ct - 1; ix >= 0; --ix)
            {
                ScriptInstanceInfo info = m_StoryLogicInfos[ix];
                info.m_StoryInstance.SendMessage(msgId, args);
            }
        }

        private ScriptInstanceInfo NewStoryInstance(int storyId)
        {
            return NewStoryInstance(storyId, true);
        }

        private ScriptInstanceInfo NewStoryInstance(int storyId, bool logIfNotFound)
        {
            ScriptInstanceInfo instInfo = GetUnusedStoryInstanceInfoFromPool(storyId);
            if (null == instInfo)
            {
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(WorldSystem.Instance.GetCurSceneId());
                if (null != cfg)
                {
                    int ct = cfg.m_StoryDslFile.Count;
                    string[] filePath = new string[ct];
                    for (int i = 0; i < ct; i++)
                    {
                        filePath[i] = HomePath.GetAbsolutePath(FilePathDefine_Client.C_RootPath + cfg.m_StoryDslFile[i]);
                        filePath[i] = filePath[i].Replace('\\', '/');
                    }
                    StoryConfigManager.Instance.LoadStoryIfNotExist(storyId, 0, filePath);
                    StoryInstance inst = StoryConfigManager.Instance.NewStoryInstance(storyId, 0);

                    if (inst == null)
                    {
                        if (logIfNotFound)
                            LogSystem.Error("Can't load story config, story:{0} scene:{1} !", storyId, WorldSystem.Instance.GetCurSceneId());
                        return null;
                    }
                    ScriptInstanceInfo res = new ScriptInstanceInfo();
                    res.m_StoryId = storyId;
                    res.m_StoryInstance = inst;
                    res.m_IsUsed = true;

                    AddStoryInstanceInfoToPool(storyId, res);
                    return res;
                }
                else
                {
                    if (logIfNotFound)
                        LogSystem.Error("Can't find story config, story:{0} scene:{1} !", storyId, WorldSystem.Instance.GetCurSceneId());
                    return null;
                }
            }
            else
            {
                instInfo.m_IsUsed = true;
                return instInfo;
            }
        }
        private void RecycleStorylInstance(ScriptInstanceInfo info)
        {
            info.m_IsUsed = false;
        }
        private void AddStoryInstanceInfoToPool(int storyId, ScriptInstanceInfo info)
        {
            List<ScriptInstanceInfo> infos;
            if (m_StoryInstancePool.TryGetValue(storyId, out infos))
            {
                infos.Add(info);
            }
            else
            {
                infos = new List<ScriptInstanceInfo>();
                infos.Add(info);
                m_StoryInstancePool.Add(storyId, infos);
            }
        }
        private ScriptInstanceInfo GetUnusedStoryInstanceInfoFromPool(int storyId)
        {
            ScriptInstanceInfo info = null;
            List<ScriptInstanceInfo> infos;
            if (m_StoryInstancePool.TryGetValue(storyId, out infos))
            {
                int ct = infos.Count;
                for (int ix = 0; ix < ct; ++ix)
                {
                    if (!infos[ix].m_IsUsed)
                    {
                        info = infos[ix];
                        break;
                    }
                }
            }
            return info;
        }

        private ClientScriptSystem() { }

        private List<ScriptInstanceInfo> m_StoryLogicInfos = new List<ScriptInstanceInfo>();
        private Dictionary<int, List<ScriptInstanceInfo>> m_StoryInstancePool = new Dictionary<int, List<ScriptInstanceInfo>>();

        internal static ClientScriptSystem Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static ClientScriptSystem s_Instance = new ClientScriptSystem();
    }

    public class CommandStack
    {
        #region Singleton
        private static CommandStack s_instance_ = new CommandStack();
        public static CommandStack Instance
        {
            get { return s_instance_; }
        }
        #endregion

        public static void ExecObjMoveCommand ( int objId, float x, float y, float z )
        {
            Vector3 pos = new Vector3(x, y, z);
            CharacterInfo obj = WorldSystem.Instance.GetCharacterById(objId);
            UserInfo user = obj as UserInfo;
            if ( null != user )
            {
                if ( !user.UnityPathFinding )
                {
                    List<Vector3> waypoints = user.SpatialSystem.FindPath(user.GetMovementStateInfo().GetPosition3D(), pos, 1);
                    waypoints.Add(pos);
                    UserAiStateInfo aiInfo = user.GetAiStateInfo();
                    AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                    if ( null == data )
                    {
                        data = new AiData_ForMoveCommand(waypoints);
                        aiInfo.AiDatas.AddData(data);
                    }
                    data.WayPoints = waypoints;
                    data.Index = 0;
                    data.EstimateFinishTime = 0;
                    data.IsFinish = false;
                    aiInfo.ChangeToState((int)AiStateId.MoveCommand);
                }
                else
                {
                    UserAiStateInfo aiInfo = user.GetAiStateInfo();
                    AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                    if ( null == data )
                    {
                        data = new AiData_ForMoveCommand(null);
                        aiInfo.AiDatas.AddData(data);
                    }

                    user.PathFindingFinished = false;
                    user.GetAiStateInfo().ChangeToState((int)AiStateId.PathFinding);
                    user.GetAiStateInfo().PreviousState = (int)AiStateId.MoveCommand;
                    GfxSystem.ForMoveCommandPathToTarget(user, pos);
                }
            }
            else
            {
                NpcInfo npc = obj as NpcInfo;
                if ( null != npc )
                {
                    if ( !npc.UnityPathFinding )
                    {
                        List<Vector3> waypoints = npc.SpatialSystem.FindPath(npc.GetMovementStateInfo().GetPosition3D(), pos, 1);
                        waypoints.Add(pos);
                        NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                        AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                        if ( null == data )
                        {
                            data = new AiData_ForMoveCommand(waypoints);
                            aiInfo.AiDatas.AddData(data);
                        }
                        data.WayPoints = waypoints;
                        data.Index = 0;
                        data.EstimateFinishTime = 0;
                        data.IsFinish = false;
                        aiInfo.ChangeToState((int)AiStateId.MoveCommand);
                    }
                    else
                    {
                        NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                        AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                        if ( null == data )
                        {
                            data = new AiData_ForMoveCommand(null);
                            aiInfo.AiDatas.AddData(data);
                        }

                        npc.PathFindingFinished = false;
                        npc.GetAiStateInfo().ChangeToState((int)AiStateId.PathFinding);
                        npc.GetAiStateInfo().PreviousState = (int)AiStateId.MoveCommand;
                        GfxSystem.ForMoveCommandPathToTarget(npc, pos);
                    }
                }
            }
        }

        public static void ExecObjFaceCommand ( int objId, float dir )
        {
            CharacterInfo obj = WorldSystem.Instance.GetCharacterById(objId);
            if ( null != obj )
            {
                MovementStateInfo msi = obj.GetMovementStateInfo();
                if ( dir < 0 )
                {
                    msi.SetFaceDir(msi.GetWantFaceDir());
                }
                else
                {
                    msi.SetFaceDir(dir);
                    msi.SetWantFaceDir(dir);
                }
            }
        }

        public static void ExecPlayerSelfMoveCommand ( float x, float y, float z )
        {
            Vector3 pos = new Vector3();
            pos.x = x;
            pos.y = y;
            pos.z = z;
            UserInfo user = WorldSystem.Instance.GetPlayerSelf();
            if ( null != user )
            {
                if ( !user.UnityPathFinding )
                {
                    List<Vector3> waypoints = user.SpatialSystem.FindPath(user.GetMovementStateInfo().GetPosition3D(), pos, 1);
                    waypoints.Add(pos);
                    UserAiStateInfo aiInfo = user.GetAiStateInfo();
                    AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                    if ( null == data )
                    {
                        data = new AiData_ForMoveCommand(waypoints);
                        aiInfo.AiDatas.AddData(data);
                    }

                    data.WayPoints = waypoints;
                    data.Index = 0;
                    data.EstimateFinishTime = 0;
                    data.IsFinish = false;
                    aiInfo.ChangeToState((int)AiStateId.MoveCommand);
                }
                else
                {
                    UserAiStateInfo aiInfo = user.GetAiStateInfo();
                    AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                    if ( null == data )
                    {
                        data = new AiData_ForMoveCommand(null);
                        aiInfo.AiDatas.AddData(data);
                    }

                    user.PathFindingFinished = false;
                    user.GetAiStateInfo().ChangeToState((int)AiStateId.PathFinding);
                    user.GetAiStateInfo().PreviousState = (int)AiStateId.MoveCommand;
                    GfxSystem.ForMoveCommandPathToTarget(user, pos);
                }
            }
        }
    }
}
