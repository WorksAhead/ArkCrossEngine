using System.Collections;
using System.Collections.Generic;
using StorySystem;

namespace ArkCrossEngine.Story.Commands
{
    /// <summary>
    /// createnpc(npc_unit_id)[objid("@objid")];
    /// createnpc(npc_unit_id,rnd)[objid("@objid")];
    /// createnpc(npc_unit_id,vector3(x,y,z),dir)[objid("@objid")];
    /// </summary>
    internal class CreateNpcCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CreateNpcCommand cmd = new CreateNpcCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_Rnd = m_Rnd.Clone();
            cmd.m_Pos = m_Pos.Clone();
            cmd.m_Dir = m_Dir.Clone();
            cmd.m_ParamNum = m_ParamNum;
            cmd.m_HaveObjId = m_HaveObjId;
            cmd.m_ObjIdVarName = m_ObjIdVarName.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            if (m_ParamNum == 2)
            {
                m_Rnd.Evaluate(iterator, args);
            }
            else if (m_ParamNum > 2)
            {
                m_Pos.Evaluate(iterator, args);
                m_Dir.Evaluate(iterator, args);
            }
            if (m_HaveObjId)
            {
                m_ObjIdVarName.Evaluate(iterator, args);
            }
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            if (m_ParamNum == 2)
            {
                m_Rnd.Evaluate(instance);
            }
            else if (m_ParamNum > 2)
            {
                m_Pos.Evaluate(instance);
                m_Dir.Evaluate(instance);
            }
            if (m_HaveObjId)
            {
                m_ObjIdVarName.Evaluate(instance);
            }
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int objId = 0;
            if (m_ParamNum == 1)
            {
                objId = WorldSystem.Instance.CreateNpcEntity(m_UnitId.Value);
            }
            else if (m_ParamNum == 2)
            {
                objId = WorldSystem.Instance.CreateNpcEntity(m_UnitId.Value, m_Rnd.Value);
            }
            else
            {
                Vector3 pos = m_Pos.Value;
                float dir = m_Dir.Value;
                objId = WorldSystem.Instance.CreateNpcEntityWithPos(m_UnitId.Value, pos.X, pos.Y, pos.Z, dir);
            }
            if (m_HaveObjId)
            {
                string varName = m_ObjIdVarName.Value;
                if (varName.StartsWith("@") && !varName.StartsWith("@@"))
                {
                    if (instance.LocalVariables.ContainsKey(varName))
                    {
                        instance.LocalVariables[varName] = objId;
                    }
                    else
                    {
                        instance.LocalVariables.Add(varName, objId);
                    }
                }
                else
                {
                    if (null != instance.GlobalVariables)
                    {
                        if (instance.GlobalVariables.ContainsKey(varName))
                        {
                            instance.GlobalVariables[varName] = objId;
                        }
                        else
                        {
                            instance.GlobalVariables.Add(varName, objId);
                        }
                    }
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            m_ParamNum = callData.GetParamNum();
            if (m_ParamNum > 0)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
            }
            if (m_ParamNum > 2)
            {
                m_Pos.InitFromDsl(callData.GetParam(1));
                m_Dir.InitFromDsl(callData.GetParam(2));
            }
            else if (m_ParamNum == 2)
            {
                m_Rnd.InitFromDsl(callData.GetParam(1));
            }
        }

        protected override void Load(ScriptableData.StatementData statementData)
        {
            if (statementData.Functions.Count == 2)
            {
                ScriptableData.FunctionData first = statementData.First;
                ScriptableData.FunctionData second = statementData.Second;
                if (null != first && null != first.Call && null != second && null != second.Call)
                {
                    Load(first.Call);
                    LoadVarName(second.Call);
                }
            }
        }

        private void LoadVarName(ScriptableData.CallData callData)
        {
            if (callData.GetId() == "objid" && callData.GetParamNum() == 1)
            {
                m_ObjIdVarName.InitFromDsl(callData.GetParam(0));
                m_HaveObjId = true;
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private int m_ParamNum = 0;
        private IStoryValue<float> m_Rnd = new StoryValue<float>();
        private IStoryValue<Vector3> m_Pos = new StoryValue<Vector3>();
        private IStoryValue<float> m_Dir = new StoryValue<float>();
        private bool m_HaveObjId = false;
        private IStoryValue<string> m_ObjIdVarName = new StoryValue<string>();
    }
    /// <summary>
    /// dropnpc(owner_id, from_npc_id,drop_type,model,particle,num);
    /// </summary>
    internal class DropNpcCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            DropNpcCommand cmd = new DropNpcCommand();
            cmd.m_OwnerId = m_OwnerId.Clone();
            cmd.m_FromNpcId = m_FromNpcId.Clone();
            cmd.m_DropType = m_DropType.Clone();
            cmd.m_Model = m_Model.Clone();
            cmd.m_Particle = m_Particle.Clone();
            cmd.m_Num = m_Num.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_OwnerId.Evaluate(iterator, args);
            m_FromNpcId.Evaluate(iterator, args);
            m_DropType.Evaluate(iterator, args);
            m_Model.Evaluate(iterator, args);
            m_Particle.Evaluate(iterator, args);
            m_Num.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_OwnerId.Evaluate(instance);
            m_FromNpcId.Evaluate(instance);
            m_DropType.Evaluate(instance);
            m_Model.Evaluate(instance);
            m_Particle.Evaluate(instance);
            m_Num.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int ownerId = m_OwnerId.Value;
            int fromNpcId = m_FromNpcId.Value;
            int dropType = m_DropType.Value;
            string model = m_Model.Value;
            string particle = m_Particle.Value;
            int num = m_Num.Value;
            WorldSystem.Instance.DropNpc((DropOutType)dropType, fromNpcId, model, particle, num);
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int paramNum = callData.GetParamNum();
            if (paramNum > 5)
            {
                m_OwnerId.InitFromDsl(callData.GetParam(0));
                m_FromNpcId.InitFromDsl(callData.GetParam(1));
                m_DropType.InitFromDsl(callData.GetParam(2));
                m_Model.InitFromDsl(callData.GetParam(3));
                m_Particle.InitFromDsl(callData.GetParam(4));
                m_Num.InitFromDsl(callData.GetParam(5));
            }
        }

        private IStoryValue<int> m_OwnerId = new StoryValue<int>();
        private IStoryValue<int> m_FromNpcId = new StoryValue<int>();
        private IStoryValue<int> m_DropType = new StoryValue<int>();
        private IStoryValue<string> m_Model = new StoryValue<string>();
        private IStoryValue<string> m_Particle = new StoryValue<string>();
        private IStoryValue<int> m_Num = new StoryValue<int>();
    }
    /// <summary>
    /// destroynpc(npc_unit_id);
    /// </summary>
    internal class DestroyNpcCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            DestroyNpcCommand cmd = new DestroyNpcCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterByUnitId(unitId) as NpcInfo;
            if (null != npc)
            {
                npc.NeedDelete = true;
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
    }
    /// <summary>
    /// destroynpcwithobjid(npc_obj_id);
    /// </summary>
    internal class DestroyNpcWithObjIdCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            DestroyNpcWithObjIdCommand cmd = new DestroyNpcWithObjIdCommand();
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
            int objid = m_ObjId.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterById(objid) as NpcInfo;
            if (null != npc)
            {
                npc.NeedDelete = true;
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
    /// npcface(npc_unit_id,dir);
    /// </summary>
    internal class NpcFaceCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcFaceCommand cmd = new NpcFaceCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_Dir = m_Dir.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_Dir.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_Dir.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            float dir = m_Dir.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterByUnitId(unitId) as NpcInfo;
            if (null != npc)
            {
                MovementStateInfo msi = npc.GetMovementStateInfo();
                if (dir < 0)
                {
                    msi.SetFaceDir(msi.GetWantFaceDir());
                }
                else
                {
                    msi.SetFaceDir(dir);
                    msi.SetWantFaceDir(dir);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_Dir.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<float> m_Dir = new StoryValue<float>();
    }
    /// <summary>
    /// npcmove(npc_unit_id,vector3(x,y,z));
    /// </summary>
    internal class NpcMoveCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcMoveCommand cmd = new NpcMoveCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_Pos = m_Pos.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_Pos.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_Pos.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            Vector3 pos = m_Pos.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterByUnitId(unitId) as NpcInfo;
            if (null != npc)
            {
                if (!npc.UnityPathFinding)
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
                else
                {
                    npc.PathFindingFinished = false;
                    npc.GetAiStateInfo().ChangeToState((int)AiStateId.PathFinding);
                    npc.GetAiStateInfo().PreviousState = (int)AiStateId.MoveCommand;
                    GfxSystem.ForMoveCommandPathToTarget(npc, pos);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_Pos.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<Vector3> m_Pos = new StoryValue<Vector3>();
    }
    /// <summary>
    /// npcmovewithwaypoints(npc_unit_id,vector2list("1 2 3 4 5 6 7"));
    /// </summary>
    internal class NpcMoveWithWaypointsCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcMoveWithWaypointsCommand cmd = new NpcMoveWithWaypointsCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_WayPoints = m_WayPoints.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_WayPoints.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_WayPoints.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            List<object> poses = m_WayPoints.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterByUnitId(unitId) as NpcInfo;
            if (null != npc)
            {
                List<Vector3> waypoints = new List<Vector3>();
                waypoints.Add(npc.GetMovementStateInfo().GetPosition3D());
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
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_WayPoints.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<List<object>> m_WayPoints = new StoryValue<List<object>>();
    }
    /// <summary>
    /// npcpatrol(npc_unit_id,vector2list("1 2 3 4 5 6 7"),isloop);
    /// </summary>
    internal class NpcPatrolCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcPatrolCommand cmd = new NpcPatrolCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_WayPoints = m_WayPoints.Clone();
            cmd.m_IsLoop = m_IsLoop.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_WayPoints.Evaluate(iterator, args);
            m_IsLoop.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_WayPoints.Evaluate(instance);
            m_IsLoop.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            List<object> poses = m_WayPoints.Value;
            string isLoop = m_IsLoop.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterByUnitId(unitId) as NpcInfo;
            if (null != npc)
            {
                NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                AiData_ForPatrolCommand data = aiInfo.AiDatas.GetData<AiData_ForPatrolCommand>();
                if (null == data)
                {
                    data = new AiData_ForPatrolCommand();
                    aiInfo.AiDatas.AddData(data);
                }
                List<Vector3> wayPts = new List<Vector3>();
                foreach (Vector2 pt in poses)
                {
                    wayPts.Add(new Vector3(pt.X, 0, pt.Y));
                }
                data.PatrolPath.SetPathPoints(npc.GetMovementStateInfo().GetPosition3D(), wayPts);
                data.IsLoopPatrol = (isLoop == "isloop");
                aiInfo.ChangeToState((int)AiStateId.PatrolCommand);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 2)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_WayPoints.InitFromDsl(callData.GetParam(1));
                m_IsLoop.InitFromDsl(callData.GetParam(2));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<List<object>> m_WayPoints = new StoryValue<List<object>>();
        private IStoryValue<string> m_IsLoop = new StoryValue<string>();
    }
    /// <summary>
    /// npcpatrolwithobjid(npc_obj_id,vector2list("1 2 3 4 5 6 7"),isloop);
    /// </summary>
    internal class NpcPatrolWithObjIdCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcPatrolWithObjIdCommand cmd = new NpcPatrolWithObjIdCommand();
            cmd.m_ObjId = m_ObjId.Clone();
            cmd.m_WayPoints = m_WayPoints.Clone();
            cmd.m_IsLoop = m_IsLoop.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_ObjId.Evaluate(iterator, args);
            m_WayPoints.Evaluate(iterator, args);
            m_IsLoop.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            m_WayPoints.Evaluate(instance);
            m_IsLoop.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int objId = m_ObjId.Value;
            List<object> poses = m_WayPoints.Value;
            string isLoop = m_IsLoop.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterById(objId) as NpcInfo;
            if (null != npc)
            {
                NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                AiData_ForPatrolCommand data = aiInfo.AiDatas.GetData<AiData_ForPatrolCommand>();
                if (null == data)
                {
                    data = new AiData_ForPatrolCommand();
                    aiInfo.AiDatas.AddData(data);
                }
                List<Vector3> wayPts = new List<Vector3>();
                foreach (Vector2 pt in poses)
                {
                    wayPts.Add(new Vector3(pt.X, 0, pt.Y));
                }
                data.PatrolPath.SetPathPoints(npc.GetMovementStateInfo().GetPosition3D(), wayPts);
                data.IsLoopPatrol = (isLoop == "isloop");
                aiInfo.ChangeToState((int)AiStateId.PatrolCommand);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 2)
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
                m_WayPoints.InitFromDsl(callData.GetParam(1));
                m_IsLoop.InitFromDsl(callData.GetParam(2));
            }
        }

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private IStoryValue<List<object>> m_WayPoints = new StoryValue<List<object>>();
        private IStoryValue<string> m_IsLoop = new StoryValue<string>();
    }
    /// <summary>
    /// npcstop(npc_unit_id);
    /// </summary>
    internal class NpcStopCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcStopCommand cmd = new NpcStopCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterByUnitId(unitId) as NpcInfo;
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
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
    }
    /// <summary>
    /// npcpursuit(unit_id, target_obj_id);
    /// </summary>
    internal class NpcPursuitCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcPursuitCommand cmd = new NpcPursuitCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_TargetId = m_TargetId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_TargetId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_TargetId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            int targetId = m_TargetId.Value;
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
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
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_TargetId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_TargetId = new StoryValue<int>();
    }
    /// <summary>
    /// npcattack(npc_unit_id[,target_unit_id]);
    /// </summary>
    internal class NpcAttackCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcAttackCommand cmd = new NpcAttackCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_TargetUnitId = m_TargetUnitId.Clone();
            cmd.m_ParamNum = m_ParamNum;
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            if (m_ParamNum > 1)
                m_TargetUnitId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            if (m_ParamNum > 1)
                m_TargetUnitId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            NpcInfo npc = WorldSystem.Instance.GetCharacterByUnitId(unitId) as NpcInfo;
            CharacterInfo target = null;
            if (m_ParamNum > 1)
            {
                int targetUnitId = m_TargetUnitId.Value;
                target = WorldSystem.Instance.GetCharacterByUnitId(targetUnitId);
            }
            else
            {
                target = WorldSystem.Instance.GetPlayerSelf();
            }
            if (null != npc && null != target)
            {
                NpcAiStateInfo aiInfo = npc.GetAiStateInfo();
                aiInfo.Target = target.GetId();
                aiInfo.ChangeToState((int)AiStateId.Pursuit);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            m_ParamNum = callData.GetParamNum();
            if (m_ParamNum > 0)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
            }
            if (m_ParamNum > 1)
            {
                m_TargetUnitId.InitFromDsl(callData.GetParam(1));
            }
        }

        private int m_ParamNum = 0;
        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_TargetUnitId = new StoryValue<int>();
    }
    /// <summary>
    /// enableai(npc_unit_id,true_or_false);
    /// </summary>
    internal class EnableAiCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            EnableAiCommand cmd = new EnableAiCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_Enable = m_Enable.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_Enable.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_Enable.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(m_UnitId.Value);
            if (null != obj)
            {
                obj.SetAIEnable(m_Enable.Value != "false");
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_Enable.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<string> m_Enable = new StoryValue<string>();
    }
    /// <summary>
    /// setai(unitid,ai_logic_id,stringlist("param1 param2 param3 ..."));
    /// </summary>
    internal class SetAiCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            SetAiCommand cmd = new SetAiCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_AiLogic = m_AiLogic.Clone();
            cmd.m_AiParams = m_AiParams.Clone();
            return cmd;
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_AiLogic.Evaluate(iterator, args);
            m_AiParams.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_AiLogic.Evaluate(instance);
            m_AiParams.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            int aiLogic = m_AiLogic.Value;
            IEnumerable aiParams = m_AiParams.Value;
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
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
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 2)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_AiLogic.InitFromDsl(callData.GetParam(1));
                m_AiParams.InitFromDsl(callData.GetParam(2));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_AiLogic = new StoryValue<int>();
        private IStoryValue<IEnumerable> m_AiParams = new StoryValue<IEnumerable>();
    }
    /// <summary>
    /// npcaddimpact(unit_id, impactid);
    /// </summary>
    internal class NpcAddImpactCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcAddImpactCommand cmd = new NpcAddImpactCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_ImpactId = m_ImpactId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_ImpactId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_ImpactId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            int impactId = m_ImpactId.Value;
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
            if (null != obj)
            {
                ImpactSystem.Instance.SendImpactToCharacter(obj, impactId, obj.GetId(), -1, -1, obj.GetMovementStateInfo().GetPosition3D(), obj.GetMovementStateInfo().GetFaceDir());
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_ImpactId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_ImpactId = new StoryValue<int>();
    }
    /// <summary>
    /// npcremoveimpact(unit_id, impactid);
    /// </summary>
    internal class NpcRemoveImpactCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcRemoveImpactCommand cmd = new NpcRemoveImpactCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_ImpactId = m_ImpactId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_ImpactId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_ImpactId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            int impactId = m_ImpactId.Value;
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
            if (null != obj)
            {
                ImpactSystem.Instance.StopImpactById(obj, impactId);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_ImpactId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_ImpactId = new StoryValue<int>();
    }
    /// <summary>
    /// npccastskill(unit_id, skillid);
    /// </summary>
    internal class NpcCastSkillCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcCastSkillCommand cmd = new NpcCastSkillCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_SkillId = m_SkillId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_SkillId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_SkillId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            int skillId = m_SkillId.Value;
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
            if (null != obj && null != obj.SkillController)
            {
                obj.SkillController.ForceStartSkill(skillId);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_SkillId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_SkillId = new StoryValue<int>();
    }
    /// <summary>
    /// npcstopskill(unit_id);
    /// </summary>
    internal class NpcStopSkillCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcStopSkillCommand cmd = new NpcStopSkillCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
            if (null != obj && null != obj.SkillController)
            {
                obj.SkillController.ForceInterruptCurSkill();
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_SkillId = new StoryValue<int>();
    }
    /// <summary>
    /// npcaddskill(unit_id, skillid);
    /// </summary>
    internal class NpcAddSkillCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcAddSkillCommand cmd = new NpcAddSkillCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_SkillId = m_SkillId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_SkillId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_SkillId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            int skillId = m_SkillId.Value;
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
            if (null != obj)
            {
                if (obj.GetSkillStateInfo().GetSkillInfoById(skillId) == null)
                {
                    obj.GetSkillStateInfo().AddSkill(new SkillInfo(skillId));
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_SkillId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_SkillId = new StoryValue<int>();
    }
    /// <summary>
    /// npcremoveskill(unit_id, skillid);
    /// </summary>
    internal class NpcRemoveSkillCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcRemoveSkillCommand cmd = new NpcRemoveSkillCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_SkillId = m_SkillId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_SkillId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_SkillId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            int skillId = m_SkillId.Value;
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
            if (null != obj)
            {
                obj.GetSkillStateInfo().RemoveSkill(skillId);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_SkillId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_SkillId = new StoryValue<int>();
    }
    /// <summary>
    /// npclisten(unit_id, 消息类别, true_or_false);
    /// </summary>
    internal class NpcListenCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            NpcListenCommand cmd = new NpcListenCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_Event = m_Event.Clone();
            cmd.m_Enable = m_Enable.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_Event.Evaluate(iterator, args);
            m_Enable.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_Event.Evaluate(instance);
            m_Enable.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int unitId = m_UnitId.Value;
            string eventName = m_Event.Value;
            string enable = m_Enable.Value;
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(unitId);
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
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 2)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_Event.InitFromDsl(callData.GetParam(1));
                m_Enable.InitFromDsl(callData.GetParam(2));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<string> m_Event = new StoryValue<string>();
        private IStoryValue<string> m_Enable = new StoryValue<string>();
    }
    /// <summary>
    /// setcamp(npc_unit_id,camp_id);
    /// </summary>
    internal class SetCampCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            SetCampCommand cmd = new SetCampCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_CampId = m_CampId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_CampId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_CampId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            CharacterInfo obj = WorldSystem.Instance.GetCharacterByUnitId(m_UnitId.Value);
            if (null != obj)
            {
                int campId = m_CampId.Value;
                obj.SetCampId(campId);

                CharacterView view = EntityManager.Instance.GetCharacterViewById(obj.GetId());
                if (null != view)
                {
                    view.ObjectInfo.CampId = campId;
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
                m_CampId.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_CampId = new StoryValue<int>();
    }
    /// <summary>
    /// createnpcbyscore(npc_unit_id,score,scene_type,level)[objid("@objid")];
    /// createnpcbyscore(npc_unit_id,score,scene_type,pos,level)[objid("@objid")];
    /// createnpcbyscore(npc_unit_id,score,scene_type,pos,dir,level)[objid("@objid")];
    /// </summary>
    internal class CreateNpcByScoreCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CreateNpcByScoreCommand cmd = new CreateNpcByScoreCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_Score = m_Score.Clone();
            cmd.m_Type = m_Type.Clone();
            cmd.m_Level = m_Level.Clone();
            cmd.m_HavePos = m_HavePos;
            cmd.m_Pos = m_Pos.Clone();
            cmd.m_HaveDir = m_HaveDir;
            cmd.m_Dir = m_Dir.Clone();
            cmd.m_HaveObjId = m_HaveObjId;
            cmd.m_ObjIdVarName = m_ObjIdVarName.Clone();
            return cmd;
        }
        protected override void ResetState()
        {
        }
        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_UnitId.Evaluate(iterator, args);
            m_Score.Evaluate(iterator, args);
            m_Type.Evaluate(iterator, args);
            m_Level.Evaluate(iterator, args);
            if (m_HavePos)
            {
                m_Pos.Evaluate(iterator, args);
            }
            if (m_HaveDir)
            {
                m_Dir.Evaluate(iterator, args);
            }
            if (m_HaveObjId)
                m_ObjIdVarName.Evaluate(iterator, args);
        }
        protected override void UpdateVariables(StoryInstance instance)
        {
            m_UnitId.Evaluate(instance);
            m_Score.Evaluate(instance);
            m_Type.Evaluate(instance);
            m_Level.Evaluate(instance);
            if (m_HavePos)
            {
                m_Pos.Evaluate(instance);
            }
            if (m_HaveDir)
            {
                m_Dir.Evaluate(instance);
            }
            if (m_HaveObjId)
                m_ObjIdVarName.Evaluate(instance);
        }
        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int match_attr = 0;
            int min_monster_score = 100000;
            for (int index = 0; index < MpveMonsterConfigProvider.Instance.GetDataCount(); index++)
            {
                MpveMonsterConfig monster_data = MpveMonsterConfigProvider.Instance.GetDataById(index) as MpveMonsterConfig;
                if (null != monster_data && m_Type.Value == (int)monster_data.m_SceneType)
                {
                    int cur_score = monster_data.m_FightingScore;
                    if (cur_score < min_monster_score && cur_score > m_Score.Value)
                    {
                        min_monster_score = cur_score;
                        if (monster_data.m_AttributeId > 0)
                        {
                            match_attr = monster_data.m_AttributeId;
                        }
                    }
                }
            }
            int objId = WorldSystem.Instance.CreateNpcAndAddAttr(m_UnitId.Value, match_attr, m_Level.Value);
            if (m_HavePos)
            {
                Vector3 pos = m_Pos.Value;
                CharacterInfo entity = WorldSystem.Instance.GetCharacterById(objId);
                if (null != entity)
                {
                    entity.GetMovementStateInfo().SetPosition(pos);
                }
            }
            if (m_HaveDir)
            {
                float dir = m_Dir.Value;
                CharacterInfo entity = WorldSystem.Instance.GetCharacterById(objId);
                if (null != entity)
                {
                    entity.GetMovementStateInfo().SetFaceDir(dir);
                }
            }
            if (m_HaveObjId)
            {
                string varName = m_ObjIdVarName.Value;
                if (varName.StartsWith("@") && !varName.StartsWith("@@"))
                {
                    if (instance.LocalVariables.ContainsKey(varName))
                    {
                        instance.LocalVariables[varName] = objId;
                    }
                    else
                    {
                        instance.LocalVariables.Add(varName, objId);
                    }
                }
                else
                {
                    if (null != instance.GlobalVariables)
                    {
                        if (instance.GlobalVariables.ContainsKey(varName))
                        {
                            instance.GlobalVariables[varName] = objId;
                        }
                        else
                        {
                            instance.GlobalVariables.Add(varName, objId);
                        }
                    }
                }
            }
            return false;
        }
        protected override void Load(ScriptableData.CallData callData)
        {
            int m_ParamNum = callData.GetParamNum();
            m_UnitId.InitFromDsl(callData.GetParam(0));
            m_Score.InitFromDsl(callData.GetParam(1));
            m_Type.InitFromDsl(callData.GetParam(2));
            m_Level.InitFromDsl(callData.GetParam(3));
            if (m_HavePos)
            {
                m_Pos.InitFromDsl(callData.GetParam(4));
            }
            if (m_HaveDir)
            {
                m_Dir.InitFromDsl(callData.GetParam(5));
            }
        }
        protected override void Load(ScriptableData.StatementData statementData)
        {
            if (statementData.Functions.Count == 2)
            {
                ScriptableData.FunctionData first = statementData.First;
                ScriptableData.FunctionData second = statementData.Second;
                if (null != first && null != first.Call && null != second && null != second.Call)
                {
                    Load(first.Call);
                    LoadVarName(second.Call);
                }
            }
        }
        private void LoadVarName(ScriptableData.CallData callData)
        {
            if (callData.GetId() == "objid" && callData.GetParamNum() == 1)
            {
                m_ObjIdVarName.InitFromDsl(callData.GetParam(0));
                m_HaveObjId = true;
            }
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private IStoryValue<int> m_Score = new StoryValue<int>();
        private IStoryValue<int> m_Type = new StoryValue<int>();
        private IStoryValue<int> m_Level = new StoryValue<int>();
        private bool m_HavePos = false;
        private IStoryValue<Vector3> m_Pos = new StoryValue<Vector3>();
        private bool m_HaveDir = false;
        private IStoryValue<float> m_Dir = new StoryValue<float>();
        private bool m_HaveObjId = false;
        private IStoryValue<string> m_ObjIdVarName = new StoryValue<string>();
    }
}
