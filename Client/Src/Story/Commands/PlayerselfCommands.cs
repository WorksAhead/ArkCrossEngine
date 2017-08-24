using System.Collections.Generic;
using StorySystem;

namespace ArkCrossEngine.Story.Commands
{
    /// <summary>
    /// playerselfface(dir);
    /// </summary>
    internal class PlayerselfFaceCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            PlayerselfFaceCommand cmd = new PlayerselfFaceCommand();
            cmd.m_Dir = m_Dir.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Dir.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Dir.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            float dir = m_Dir.Value;
            UserInfo user = WorldSystem.Instance.GetPlayerSelf();
            if (null != user)
            {
                MovementStateInfo msi = user.GetMovementStateInfo();
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
            if (num > 0)
            {
                m_Dir.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<float> m_Dir = new StoryValue<float>();
    }
    /// <summary>
    /// playerselfmove(vector3(x,y,z));
    /// </summary>
    internal class PlayerselfMoveCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            PlayerselfMoveCommand cmd = new PlayerselfMoveCommand();
            cmd.m_Pos = m_Pos.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Pos.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Pos.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Vector3 pos = m_Pos.Value;
            UserInfo user = WorldSystem.Instance.GetPlayerSelf();
            if (null != user)
            {
                if (!user.UnityPathFinding)
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
                    user.PathFindingFinished = false;
                    user.GetAiStateInfo().ChangeToState((int)AiStateId.PathFinding);
                    user.GetAiStateInfo().PreviousState = (int)AiStateId.MoveCommand;
                    GfxSystem.ForMoveCommandPathToTarget(user, pos);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_Pos.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<Vector3> m_Pos = new StoryValue<Vector3>();
    }
    /// <summary>
    /// playerselfmovewithwaypoints(vector2list("1 2 3 4 5 6 7"));
    /// </summary>
    internal class PlayerselfMoveWithWaypointsCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            PlayerselfMoveWithWaypointsCommand cmd = new PlayerselfMoveWithWaypointsCommand();
            cmd.m_WayPoints = m_WayPoints.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_WayPoints.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_WayPoints.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            List<object> poses = m_WayPoints.Value;
            UserInfo user = WorldSystem.Instance.GetPlayerSelf();
            if (null != user)
            {
                List<Vector3> waypoints = new List<Vector3>();
                waypoints.Add(user.GetMovementStateInfo().GetPosition3D());
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
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_WayPoints.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<List<object>> m_WayPoints = new StoryValue<List<object>>();
    }
    /// <summary>
    /// playerselfpursuit(target_obj_id);
    /// </summary>
    internal class PlayerselfPursuitCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            PlayerselfPursuitCommand cmd = new PlayerselfPursuitCommand();
            cmd.m_TargetId = m_TargetId.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_TargetId.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_TargetId.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            int targetId = m_TargetId.Value;
            UserInfo user = WorldSystem.Instance.GetPlayerSelf();
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
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_TargetId.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<int> m_TargetId = new StoryValue<int>();
    }
    /// <summary>
    /// playerselfstop();
    /// </summary>
    internal class PlayerselfStopCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            PlayerselfStopCommand cmd = new PlayerselfStopCommand();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            UserInfo user = WorldSystem.Instance.GetPlayerSelf();
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
                    aiInfo.ChangeToState((int)AiStateId.Idle);
                }
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
            }
        }
    }
    /// <summary>
    /// playerselflisten(消息类别, true_or_false);
    /// </summary>
    internal class PlayerselfListenCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            PlayerselfListenCommand cmd = new PlayerselfListenCommand();
            cmd.m_Event = m_Event.Clone();
            cmd.m_Enable = m_Enable.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Event.Evaluate(iterator, args);
            m_Enable.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Event.Evaluate(instance);
            m_Enable.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            string eventName = m_Event.Value;
            string enable = m_Enable.Value;
            CharacterInfo obj = WorldSystem.Instance.GetPlayerSelf();
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
            if (num > 1)
            {
                m_Event.InitFromDsl(callData.GetParam(0));
                m_Enable.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<string> m_Event = new StoryValue<string>();
        private IStoryValue<string> m_Enable = new StoryValue<string>();
    }
}
