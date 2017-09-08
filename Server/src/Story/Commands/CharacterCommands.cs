using System;
using System.Collections;
using System.Collections.Generic;
using StorySystem;
using ArkCrossEngine;

namespace DashFire.Story.Commands
{
    /// <summary>
    /// objface(obj_id, dir);
    /// </summary>
    internal class ObjFaceCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjFaceCommand cmd = new ObjFaceCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_Dir = m_Dir.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_Dir.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_Dir.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                float dir = m_Dir.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != obj)
                {
                    obj.GetMovementStateInfo().SetFaceDir(dir);
                    obj.GetMovementStateInfo().SetWantFaceDir(dir);

                    UserInfo user = obj as UserInfo;
                    if (null != user)
                    {
                        ArkCrossEngineMessage.Msg_RC_UserFace msg = DataSyncUtility.BuildUserFaceMessage(user);
                        scene.NotifyAllUser(msg);
                    }
                    else
                    {
                        NpcInfo npc = obj as NpcInfo;
                        if (null != npc)
                        {
                            ArkCrossEngineMessage.Msg_RC_NpcFace msg = DataSyncUtility.BuildNpcFaceMessage(npc);
                            scene.NotifyAllUser(msg);
                        }
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_Dir.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<float> m_Dir = new StoryValue<float>();
    }
    /// <summary>
    /// objmove(obj_id, vector3(x,y,z));
    /// </summary>
    internal class ObjMoveCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjMoveCommand cmd = new ObjMoveCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_Pos = m_Pos.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_Pos.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_Pos.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                Vector3 pos = m_Pos.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                UserInfo user = obj as UserInfo;
                if (null != user)
                {
                    List<Vector3> waypoints = user.SpatialSystem.FindPath(user.GetMovementStateInfo().GetPosition3D(), pos, 1);
                    waypoints.Add(pos);
                    UserAiStateInfo aiInfo = user.GetAiStateInfo();
                    AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                    if (null == data)
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
                    NpcInfo npc = obj as NpcInfo;
                    if (null != npc)
                    {
                        List<Vector3> waypoints = npc.SpatialSystem.FindPath(npc.GetMovementStateInfo().GetPosition3D(), pos, 1);
                        waypoints.Add(pos);
                        NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                        AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                        if (null == data)
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
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_Pos.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<Vector3> m_Pos = new StoryValue<Vector3>();
    }
    /// <summary>
    /// objmovewithwaypoints(obj_id, vector2list("1 2 3 4 5 6 7"));
    /// </summary>
    internal class ObjMoveWithWaypointsCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjMoveWithWaypointsCommand cmd = new ObjMoveWithWaypointsCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_WayPoints = m_WayPoints.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_WayPoints.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_WayPoints.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                List<object> poses = m_WayPoints.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                UserInfo user = obj as UserInfo;
                if (null != user)
                {
                    List<Vector3> waypoints = new List<Vector3>();
                    waypoints.Add(obj.GetMovementStateInfo().GetPosition3D());
                    foreach (Vector2 pt in poses)
                    {
                        waypoints.Add(new Vector3(pt.X, 0, pt.Y));
                    }
                    UserAiStateInfo aiInfo = user.GetAiStateInfo();
                    AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                    if (null == data)
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
                    NpcInfo npc = obj as NpcInfo;
                    if (null != npc)
                    {
                        List<Vector3> waypoints = new List<Vector3>();
                        waypoints.Add(obj.GetMovementStateInfo().GetPosition3D());
                        foreach (Vector2 pt in poses)
                        {
                            waypoints.Add(new Vector3(pt.X, 0, pt.Y));
                        }
                        NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                        AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                        if (null == data)
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
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_WayPoints.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<List<object>> m_WayPoints = new StoryValue<List<object>>();
    }
    /// <summary>
    /// objstop(obj_id);
    /// </summary>
    internal class ObjStopCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjStopCommand cmd = new ObjStopCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                NpcInfo npc = obj as NpcInfo;
                if (null != npc)
                {
                    NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                    if (aiInfo.CurState == (int)AiStateId.MoveCommand || aiInfo.CurState == (int)AiStateId.PursuitCommand || aiInfo.CurState == (int)AiStateId.PatrolCommand)
                    {
                        aiInfo.Time = 0;
                        aiInfo.Target = 0;
                        aiInfo.AiDatas.RemoveData<AiData_ForMoveCommand>();
                        aiInfo.AiDatas.RemoveData<AiData_ForPursuitCommand>();
                        aiInfo.AiDatas.RemoveData<AiData_ForPatrolCommand>();
                    }
                    if (aiInfo.CurState > (int)AiStateId.Invalid && aiInfo.CurState < (int)AiStateId.MaxNum)
                        aiInfo.ChangeToState((int)AiStateId.Idle);
                }
                else
                {
                    UserInfo user = obj as UserInfo;
                    if (null != user)
                    {
                        UserAiStateInfo aiInfo = user.GetAiStateInfo();
                        if (aiInfo.CurState == (int)AiStateId.MoveCommand || aiInfo.CurState == (int)AiStateId.PursuitCommand || aiInfo.CurState == (int)AiStateId.PatrolCommand)
                        {
                            aiInfo.Time = 0;
                            aiInfo.Target = 0;
                            aiInfo.AiDatas.RemoveData<AiData_ForMoveCommand>();
                            aiInfo.AiDatas.RemoveData<AiData_ForPursuitCommand>();
                            aiInfo.AiDatas.RemoveData<AiData_ForPatrolCommand>();
                        }
                        if (aiInfo.CurState > (int)AiStateId.Invalid && aiInfo.CurState < (int)AiStateId.MaxNum)
                            aiInfo.ChangeToState((int)AiStateId.Idle);
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
    }
    /// <summary>
    /// objanimation(obj_id, anim_type[, anim_time=2000[, isqueued]]);
    /// </summary>
    internal class ObjAnimationCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjAnimationCommand cmd = new ObjAnimationCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_AnimType = m_AnimType.Clone();
            cmd.m_AnimTime = m_AnimTime.Clone();
            cmd.m_IsQueued = m_IsQueued.Clone();
            cmd.m_ParamNum = m_ParamNum;
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_AnimType.Evaluate(iterator, args);
            if (m_ParamNum > 2)
                m_AnimTime.Evaluate(iterator, args);
            if (m_ParamNum > 3)
                m_IsQueued.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_AnimType.Evaluate(instance);
            if (m_ParamNum > 2)
                m_AnimTime.Evaluate(instance);
            if (m_ParamNum > 3)
                m_IsQueued.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int animType = m_AnimType.Value;
                int animTime = 2000;
                bool isQueued = false;
                if (m_ParamNum > 2)
                {
                    animTime = m_AnimTime.Value;
                }
                if (m_ParamNum > 3)
                {
                    isQueued = (0 == m_IsQueued.Value.CompareTo("isqueued"));
                }
                ArkCrossEngineMessage.Msg_RC_PlayAnimation msg = new ArkCrossEngineMessage.Msg_RC_PlayAnimation();
                msg.obj_id = objId;
                msg.anim_type = animType;
                msg.anim_time = animTime;
                msg.is_queued = isQueued;
                scene.NotifyAllUser(msg);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_AnimType.InitFromDsl(callData.GetParam(1));
            }
            if (num > 2)
            {
                m_AnimTime.InitFromDsl(callData.GetParam(2));
            }
            if (num > 3)
            {
                m_IsQueued.InitFromDsl(callData.GetParam(3));
            }
            m_ParamNum = num;
        }

        private int m_ParamNum = 0;
        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_AnimType = new StoryValue<int>();
        private IStoryValue<int> m_AnimTime = new StoryValue<int>();
        private IStoryValue<string> m_IsQueued = new StoryValue<string>();
    }
    /// <summary>
    /// objpursuit(obj_id, target_obj_id);
    /// </summary>
    internal class ObjPursuitCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjPursuitCommand cmd = new ObjPursuitCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_TargetId = m_TargetId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_TargetId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_TargetId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int targetId = m_TargetId.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                NpcInfo npc = obj as NpcInfo;
                if (null != npc)
                {
                    NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                    AiData_ForPursuitCommand data = aiInfo.AiDatas.GetData<AiData_ForPursuitCommand>();
                    if (null == data)
                    {
                        data = new AiData_ForPursuitCommand();
                        aiInfo.AiDatas.AddData(data);
                    }
                    aiInfo.Target = targetId;
                    aiInfo.ChangeToState((int)AiStateId.PursuitCommand);
                }
                else
                {
                    UserInfo user = obj as UserInfo;
                    if (null != user)
                    {
                        UserAiStateInfo aiInfo = user.GetAiStateInfo();
                        AiData_ForPursuitCommand data = aiInfo.AiDatas.GetData<AiData_ForPursuitCommand>();
                        if (null == data)
                        {
                            data = new AiData_ForPursuitCommand();
                            aiInfo.AiDatas.AddData(data);
                        }
                        aiInfo.Target = targetId;
                        aiInfo.ChangeToState((int)AiStateId.PursuitCommand);
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_TargetId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_TargetId = new StoryValue<int>();
    }
    /// <summary>
    /// objenableai(obj_id, true_or_false);
    /// </summary>
    internal class ObjEnableAiCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjEnableAiCommand cmd = new ObjEnableAiCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_Enable = m_Enable.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_Enable.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_Enable.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                string enable = m_Enable.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != obj)
                {
                    obj.SetAIEnable(m_Enable.Value != "false");
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_Enable.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<string> m_Enable = new StoryValue<string>();
    }
    /// <summary>
    /// objsetai(objid,ai_logic_id,stringlist(param1,param2,param3,...));
    /// </summary>
    internal class ObjSetAiCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjSetAiCommand cmd = new ObjSetAiCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_AiLogic = m_AiLogic.Clone();
            cmd.m_AiParams = m_AiParams.Clone();
            return cmd;
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_AiLogic.Evaluate(iterator, args);
            m_AiParams.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_AiLogic.Evaluate(instance);
            m_AiParams.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int aiLogic = m_AiLogic.Value;
                IEnumerable aiParams = m_AiParams.Value;
                CharacterInfo charObj = scene.SceneContext.GetCharacterInfoById(objId);
                NpcInfo npc = charObj as NpcInfo;
                if (null != npc)
                {
                    npc.GetAiStateInfo().Reset();
                    npc.GetAiStateInfo().AiLogic = aiLogic;
                    int ix = 0;
                    foreach (string aiParam in aiParams)
                    {
                        if (ix < Data_Unit.c_MaxAiParamNum)
                        {
                            npc.GetAiStateInfo().AiParam[ix] = aiParam;
                            ++ix;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    UserInfo user = charObj as UserInfo;
                    user.GetAiStateInfo().Reset();
                    user.GetAiStateInfo().AiLogic = aiLogic;
                    int ix = 0;
                    foreach (string aiParam in aiParams)
                    {
                        if (ix < Data_Unit.c_MaxAiParamNum)
                        {
                            user.GetAiStateInfo().AiParam[ix] = aiParam;
                            ++ix;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 2)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_AiLogic.InitFromDsl(callData.GetParam(1));
                m_AiParams.InitFromDsl(callData.GetParam(2));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_AiLogic = new StoryValue<int>();
        private IStoryValue<IEnumerable> m_AiParams = new StoryValue<IEnumerable>();
    }
    /// <summary>
    /// objaddimpact(obj_id, impactid);
    /// </summary>
    internal class ObjAddImpactCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjAddImpactCommand cmd = new ObjAddImpactCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_ImpactId = m_ImpactId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_ImpactId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_ImpactId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int impactId = m_ImpactId.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != obj)
                {
                    ImpactSystem.Instance.SendImpactToCharacter(obj, impactId, obj.GetId(), -1, -1, obj.GetMovementStateInfo().GetPosition3D(), obj.GetMovementStateInfo().GetFaceDir());

                    ArkCrossEngineMessage.Msg_CRC_SendImpactToEntity bd = new ArkCrossEngineMessage.Msg_CRC_SendImpactToEntity();
                    bd.sender_id = objId;
                    bd.target_id = objId;
                    bd.impact_id = impactId;
                    bd.sender_pos = new ArkCrossEngineMessage.Position3D();
                    bd.sender_pos.x = obj.GetMovementStateInfo().PositionX;
                    bd.sender_pos.y = obj.GetMovementStateInfo().PositionY;
                    bd.sender_pos.z = obj.GetMovementStateInfo().PositionZ;
                    bd.sender_dir = obj.GetMovementStateInfo().GetFaceDir();
                    scene.NotifyAllUser(bd);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_ImpactId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_ImpactId = new StoryValue<int>();
    }
    /// <summary>
    /// objremoveimpact(obj_id, impactid);
    /// </summary>
    internal class ObjRemoveImpactCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjRemoveImpactCommand cmd = new ObjRemoveImpactCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_ImpactId = m_ImpactId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_ImpactId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_ImpactId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int impactId = m_ImpactId.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != obj)
                {
                    ImpactSystem.Instance.StopImpactById(obj, impactId);

                    ArkCrossEngineMessage.Msg_RC_StopImpact bd = new ArkCrossEngineMessage.Msg_RC_StopImpact();
                    bd.obj_id = objId;
                    bd.impact_id = impactId;
                    scene.NotifyAllUser(bd);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_ImpactId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_ImpactId = new StoryValue<int>();
    }
    /// <summary>
    /// objcastskill(obj_id, skillid);
    /// </summary>
    internal class ObjCastSkillCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjCastSkillCommand cmd = new ObjCastSkillCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_SkillId = m_SkillId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_SkillId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_SkillId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int skillId = m_SkillId.Value;

                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                NpcInfo npc = obj as NpcInfo;
                if (null != npc)
                {
                    SkillInfo skillInfo = npc.GetSkillStateInfo().GetCurSkillInfo();
                    if (null == skillInfo || !skillInfo.IsSkillActivated)
                    {
                        scene.SkillSystem.StartSkill(npc.GetId(), skillId);

                        ArkCrossEngineMessage.Msg_RC_NpcSkill skillBuilder = new ArkCrossEngineMessage.Msg_RC_NpcSkill();
                        skillBuilder.npc_id = npc.GetId();
                        skillBuilder.skill_id = skillId;
                        ArkCrossEngineMessage.Position posBuilder1 = new ArkCrossEngineMessage.Position();
                        posBuilder1.x = npc.GetMovementStateInfo().GetPosition3D().X;
                        posBuilder1.z = npc.GetMovementStateInfo().GetPosition3D().Z;
                        skillBuilder.stand_pos = posBuilder1;
                        skillBuilder.face_direction = (float)npc.GetMovementStateInfo().GetFaceDir();
                        scene.NotifyAllUser(skillBuilder);
                    }
                }
                else
                {
                    UserInfo user = obj as UserInfo;
                    if (null != user)
                    {
                        SkillInfo skillInfo = user.GetSkillStateInfo().GetCurSkillInfo();
                        if (null == skillInfo || !skillInfo.IsSkillActivated)
                        {
                            scene.SkillSystem.StartSkill(user.GetId(), skillId);

                            ArkCrossEngineMessage.Msg_CRC_Skill skillBuilder = new ArkCrossEngineMessage.Msg_CRC_Skill();
                            skillBuilder.role_id = user.GetId();
                            skillBuilder.skill_id = skillId;
                            ArkCrossEngineMessage.Position posBuilder1 = new ArkCrossEngineMessage.Position();
                            posBuilder1.x = user.GetMovementStateInfo().GetPosition3D().X;
                            posBuilder1.z = user.GetMovementStateInfo().GetPosition3D().Z;
                            skillBuilder.stand_pos = posBuilder1;
                            skillBuilder.face_direction = (float)user.GetMovementStateInfo().GetFaceDir();
                            skillBuilder.want_face_dir = (float)user.GetMovementStateInfo().GetFaceDir();
                            scene.NotifyAllUser(skillBuilder);
                        }
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_SkillId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_SkillId = new StoryValue<int>();
    }
    /// <summary>
    /// objstopskill(obj_id);
    /// </summary>
    internal class ObjStopSkillCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjStopSkillCommand cmd = new ObjStopSkillCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;

                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                NpcInfo npc = obj as NpcInfo;
                if (null != npc)
                {
                    SkillInfo skillInfo = npc.GetSkillStateInfo().GetCurSkillInfo();
                    if (null == skillInfo || skillInfo.IsSkillActivated)
                    {
                        scene.SkillSystem.StopSkill(npc.GetId());
                    }

                    ArkCrossEngineMessage.Msg_CRC_NpcStopSkill skillBuilder = new ArkCrossEngineMessage.Msg_CRC_NpcStopSkill();
                    skillBuilder.npc_id = npc.GetId();
                    scene.NotifyAllUser(skillBuilder);
                }
                else
                {
                    UserInfo user = obj as UserInfo;
                    if (null != user)
                    {
                        SkillInfo skillInfo = user.GetSkillStateInfo().GetCurSkillInfo();
                        if (null == skillInfo || skillInfo.IsSkillActivated)
                        {
                            scene.SkillSystem.StopSkill(user.GetId());
                        }

                        ArkCrossEngineMessage.Msg_CRC_StopSkill skillBuilder = new ArkCrossEngineMessage.Msg_CRC_StopSkill();
                        skillBuilder.role_id = user.GetId();
                        scene.NotifyAllUser(skillBuilder);
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_SkillId = new StoryValue<int>();
    }
    /// <summary>
    /// objaddskill(obj_id, skillid);
    /// </summary>
    internal class ObjAddSkillCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjAddSkillCommand cmd = new ObjAddSkillCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_SkillId = m_SkillId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_SkillId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_SkillId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int skillId = m_SkillId.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != obj)
                {
                    if (obj.GetSkillStateInfo().GetSkillInfoById(skillId) == null)
                    {
                        obj.GetSkillStateInfo().AddSkill(new SkillInfo(skillId));
                    }

                    ArkCrossEngineMessage.Msg_RC_AddSkill msg = new ArkCrossEngineMessage.Msg_RC_AddSkill();
                    msg.obj_id = objId;
                    msg.skill_id = skillId;
                    scene.NotifyAllUser(msg);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_SkillId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_SkillId = new StoryValue<int>();
    }
    /// <summary>
    /// objremoveskill(obj_id, skillid);
    /// </summary>
    internal class ObjRemoveSkillCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjRemoveSkillCommand cmd = new ObjRemoveSkillCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_SkillId = m_SkillId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_SkillId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_SkillId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int skillId = m_SkillId.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != obj)
                {
                    obj.GetSkillStateInfo().RemoveSkill(skillId);
                }

                ArkCrossEngineMessage.Msg_RC_RemoveSkill msg = new ArkCrossEngineMessage.Msg_RC_RemoveSkill();
                msg.obj_id = objId;
                msg.skill_id = skillId;
                scene.NotifyAllUser(msg);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_SkillId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_SkillId = new StoryValue<int>();
    }
    /// <summary>
    /// objlisten(unit_id, 消息类别, true_or_false);
    /// </summary>
    internal class ObjListenCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjListenCommand cmd = new ObjListenCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_Event = m_Event.Clone();
            cmd.m_Enable = m_Enable.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_Event.Evaluate(iterator, args);
            m_Enable.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_Event.Evaluate(instance);
            m_Enable.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                string eventName = m_Event.Value;
                string enable = m_Enable.Value;
                CharacterInfo obj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != obj)
                {
                    if (eventName == "damage")
                    {
                        if (0 == string.Compare(enable, "true"))
                            obj.AddStoryFlag(StoryListenFlagEnum.Damage);
                        else
                            obj.RemoveStoryFlag(StoryListenFlagEnum.Damage);
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 2)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_Event.InitFromDsl(callData.GetParam(1));
                m_Enable.InitFromDsl(callData.GetParam(2));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<string> m_Event = new StoryValue<string>();
        private IStoryValue<string> m_Enable = new StoryValue<string>();
    }
    /// <summary>
    /// setblockedshader(rimcolor1,rimpower1,rimcutvalue1,rimcolor2,rimpower2,rimcutvalue2);
    /// </summary>
    internal class SetBlockedShaderCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            SetBlockedShaderCommand cmd = new SetBlockedShaderCommand();
            cmd.m_RimColor1 = m_RimColor1.Clone();
            cmd.m_RimPower1 = m_RimPower1.Clone();
            cmd.m_RimCutValue1 = m_RimCutValue1.Clone();
            cmd.m_RimColor2 = m_RimColor2.Clone();
            cmd.m_RimPower2 = m_RimPower2.Clone();
            cmd.m_RimCutValue2 = m_RimCutValue2.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_RimColor1.Evaluate(iterator, args);
            m_RimPower1.Evaluate(iterator, args);
            m_RimCutValue1.Evaluate(iterator, args);
            m_RimColor2.Evaluate(iterator, args);
            m_RimPower2.Evaluate(iterator, args);
            m_RimCutValue2.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_RimColor1.Evaluate(instance);
            m_RimPower1.Evaluate(instance);
            m_RimCutValue1.Evaluate(instance);
            m_RimColor2.Evaluate(instance);
            m_RimPower2.Evaluate(instance);
            m_RimCutValue2.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                uint rimColor1 = m_RimColor1.Value;
                float rimPower1 = m_RimPower1.Value;
                float rimCutValue1 = m_RimCutValue1.Value;
                uint rimColor2 = m_RimColor2.Value;
                float rimPower2 = m_RimPower2.Value;
                float rimCutValue2 = m_RimCutValue2.Value;

                ArkCrossEngineMessage.Msg_RC_SetBlockedShader msg = new ArkCrossEngineMessage.Msg_RC_SetBlockedShader();
                msg.rim_color_1 = rimColor1;
                msg.rim_power_1 = rimPower1;
                msg.rim_cutvalue_1 = rimCutValue1;
                msg.rim_color_2 = rimColor2;
                msg.rim_power_2 = rimPower2;
                msg.rim_cutvalue_2 = rimCutValue2;
                scene.NotifyAllUser(msg);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 5)
            {
                m_RimColor1.InitFromDsl(callData.GetParam(0));
                m_RimPower1.InitFromDsl(callData.GetParam(1));
                m_RimCutValue1.InitFromDsl(callData.GetParam(2));
                m_RimColor2.InitFromDsl(callData.GetParam(3));
                m_RimPower2.InitFromDsl(callData.GetParam(4));
                m_RimCutValue2.InitFromDsl(callData.GetParam(5));
            }
        }

        private IStoryValue<uint> m_RimColor1 = new StoryValue<uint>();
        private IStoryValue<float> m_RimPower1 = new StoryValue<float>();
        private IStoryValue<float> m_RimCutValue1 = new StoryValue<float>();
        private IStoryValue<uint> m_RimColor2 = new StoryValue<uint>();
        private IStoryValue<float> m_RimPower2 = new StoryValue<float>();
        private IStoryValue<float> m_RimCutValue2 = new StoryValue<float>();
    }
    /// <summary>
    /// sethp(objid,value);
    /// </summary>
    internal class SetHpCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            SetHpCommand cmd = new SetHpCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_Value = m_Value.Clone();
            return cmd;
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_Value.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_Value.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int value = m_Value.Value;
                CharacterInfo charObj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != charObj)
                {
                    charObj.SetHp(Operate_Type.OT_Absolute, value);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_Value.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_Value = new StoryValue<int>();
    }
    /// <summary>
    /// setenergy(objid,value);
    /// </summary>
    internal class SetEnergyCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            SetEnergyCommand cmd = new SetEnergyCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_Value = m_Value.Clone();
            return cmd;
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_Value.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_Value.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int value = m_Value.Value;
                CharacterInfo charObj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != charObj)
                {
                    charObj.SetEnergy(Operate_Type.OT_Absolute, value);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_Value.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_Value = new StoryValue<int>();
    }
    /// <summary>
    /// setrage(objid,value);
    /// </summary>
    internal class SetRageCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            SetRageCommand cmd = new SetRageCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_Value = m_Value.Clone();
            return cmd;
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_Value.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_Value.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                int value = m_Value.Value;
                CharacterInfo charObj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != charObj)
                {
                    charObj.SetRage(Operate_Type.OT_Absolute, value);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_Value.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<int> m_Value = new StoryValue<int>();
    }
    /// <summary>
    /// objset(objid,localname,value);
    /// </summary>
    internal class ObjSetCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            ObjSetCommand cmd = new ObjSetCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_LocalName = m_LocalName.Clone();
            cmd.m_Value = m_Value.Clone();
            return cmd;
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_LocalName.Evaluate(iterator, args);
            m_Value.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_LocalName.Evaluate(instance);
            m_Value.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                string localName = m_LocalName.Value;
                object value = m_Value.Value;
                CharacterInfo charObj = scene.SceneContext.GetCharacterInfoById(objId);
                if (null != charObj)
                {
                    if (charObj.LocalVariables.ContainsKey(localName))
                    {
                        charObj.LocalVariables[localName] = value;
                    }
                    else
                    {
                        charObj.LocalVariables.Add(localName, value);
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 2)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_LocalName.InitFromDsl(callData.GetParam(1));
                m_Value.InitFromDsl(callData.GetParam(2));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<string> m_LocalName = new StoryValue<string>();
        private IStoryValue<object> m_Value = new StoryValue();
    }
}
