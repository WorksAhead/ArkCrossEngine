using System;
using System.Collections.Generic;
using StorySystem;
using ArkCrossEngine;

namespace DashFire.Story.Commands
{
    /// <summary>
    /// cameralookat(x,y,z);
    /// 
    /// or
    /// 
    /// cameralookat(unit_id);
    /// </summary>
    internal class CameraLookatCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CameraLookatCommand cmd = new CameraLookatCommand();
            cmd.m_X = m_X.Clone();
            cmd.m_Y = m_Y.Clone();
            cmd.m_Z = m_Z.Clone();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_ParamNum = m_ParamNum;
            return cmd;
        }
        protected override void ResetState()
        {
        }
        protected override void UpdateArguments(object iterator, object[] args)
        {
            if (m_ParamNum >= 3)
            {
                m_X.Evaluate(iterator, args);
                m_Y.Evaluate(iterator, args);
                m_Z.Evaluate(iterator, args);
            }
            else if (m_ParamNum >= 1)
            {
                m_UnitId.Evaluate(iterator, args);
            }
        }
        protected override void UpdateVariables(StoryInstance instance)
        {
            if (m_ParamNum >= 3)
            {
                m_X.Evaluate(instance);
                m_Y.Evaluate(instance);
                m_Z.Evaluate(instance);
            }
            else if (m_ParamNum >= 1)
            {
                m_UnitId.Evaluate(instance);
            }
        }
        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                if (m_ParamNum >= 3)
                {
                    float x = m_X.Value;
                    float y = m_Y.Value;
                    float z = m_Z.Value;

                    LogSystem.Info("CameraLookat:{0} {1} {2}", x, y, z);

                    ArkCrossEngineMessage.Msg_RC_CameraLookat msg = new ArkCrossEngineMessage.Msg_RC_CameraLookat();
                    msg.x = x;
                    msg.y = y;
                    msg.z = z;
                    msg.is_immediately = false;
                    scene.NotifyAllUser(msg);
                }
                else if (m_ParamNum >= 1)
                {
                    int unitId = m_UnitId.Value;
                    NpcInfo npc = scene.SceneContext.GetCharacterInfoByUnitId(unitId) as NpcInfo;
                    if (null != npc)
                    {
                        MovementStateInfo msi = npc.GetMovementStateInfo();

                        LogSystem.Info("CameraLookat:{0}({1} {2} {3})", unitId, msi.PositionX, msi.PositionY, msi.PositionZ);

                        ArkCrossEngineMessage.Msg_RC_CameraLookat msg = new ArkCrossEngineMessage.Msg_RC_CameraLookat();
                        msg.x = msi.PositionX;
                        msg.y = msi.PositionY;
                        msg.z = msi.PositionZ;
                        msg.is_immediately = false;
                        scene.NotifyAllUser(msg);
                    }
                }
                else
                {
                    for (LinkedListNode<UserInfo> linkNode = scene.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo info = linkNode.Value;
                        User user = info.CustomData as User;

                        if (null != info && null != user)
                        {
                            MovementStateInfo msi = info.GetMovementStateInfo();

                            LogSystem.Info("CameraLookat:{0}({1} {2} {3})", info.GetId(), msi.PositionX, msi.PositionY, msi.PositionZ);

                            ArkCrossEngineMessage.Msg_RC_CameraLookat msg = new ArkCrossEngineMessage.Msg_RC_CameraLookat();
                            msg.x = msi.PositionX;
                            msg.y = msi.PositionY;
                            msg.z = msi.PositionZ;
                            msg.is_immediately = false;

                            user.SendMessage(msg);
                        }
                    }
                }
            }
            return false;
        }
        protected override void Load(ScriptableData.CallData callData)
        {
            m_ParamNum = callData.GetParamNum();
            if (m_ParamNum >= 3)
            {
                m_X.InitFromDsl(callData.GetParam(0));
                m_Y.InitFromDsl(callData.GetParam(1));
                m_Z.InitFromDsl(callData.GetParam(2));
            }
            else if (m_ParamNum >= 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<float> m_X = new StoryValue<float>();
        private IStoryValue<float> m_Y = new StoryValue<float>();
        private IStoryValue<float> m_Z = new StoryValue<float>();
        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private int m_ParamNum = 0;
    }
    /// <summary>
    /// camerafollow([unit_id]);
    /// </summary>
    internal class CameraFollowCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CameraFollowCommand cmd = new CameraFollowCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_ParamNum = m_ParamNum;
            return cmd;
        }
        protected override void ResetState()
        {
        }
        protected override void UpdateArguments(object iterator, object[] args)
        {
            if (m_ParamNum > 0)
            {
                m_UnitId.Evaluate(iterator, args);
            }
        }
        protected override void UpdateVariables(StoryInstance instance)
        {
            if (m_ParamNum > 0)
            {
                m_UnitId.Evaluate(instance);
            }
        }
        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                if (0 == m_ParamNum)
                {
                    for (LinkedListNode<UserInfo> linkNode = scene.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo info = linkNode.Value;
                        User user = info.CustomData as User;

                        if (null != info && null != user)
                        {
                            MovementStateInfo msi = info.GetMovementStateInfo();

                            LogSystem.Info("CameraFollow:{0}", info.GetId());

                            ArkCrossEngineMessage.Msg_RC_CameraFollow msg = new ArkCrossEngineMessage.Msg_RC_CameraFollow();
                            msg.obj_id = info.GetId();
                            msg.is_immediately = false;

                            user.SendMessage(msg);
                        }
                    }
                }
                else
                {
                    int unitId = m_UnitId.Value;
                    NpcInfo npc = scene.SceneContext.GetCharacterInfoByUnitId(unitId) as NpcInfo;
                    if (null != npc)
                    {
                        LogSystem.Info("CameraFollow:{0}", npc.GetId());

                        ArkCrossEngineMessage.Msg_RC_CameraFollow msg = new ArkCrossEngineMessage.Msg_RC_CameraFollow();
                        msg.obj_id = npc.GetId();
                        msg.is_immediately = false;
                        scene.NotifyAllUser(msg);
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
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private int m_ParamNum = 0;
    }
    /// <summary>
    /// cameralookatimmediately(x,y,z);
    /// 
    /// or
    /// 
    /// cameralookatimmediately(unit_id);
    /// </summary>
    internal class CameraLookatImmediatelyCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CameraLookatImmediatelyCommand cmd = new CameraLookatImmediatelyCommand();
            cmd.m_X = m_X.Clone();
            cmd.m_Y = m_Y.Clone();
            cmd.m_Z = m_Z.Clone();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_ParamNum = m_ParamNum;
            return cmd;
        }
        protected override void ResetState()
        {
        }
        protected override void UpdateArguments(object iterator, object[] args)
        {
            if (m_ParamNum >= 3)
            {
                m_X.Evaluate(iterator, args);
                m_Y.Evaluate(iterator, args);
                m_Z.Evaluate(iterator, args);
            }
            else if (m_ParamNum >= 1)
            {
                m_UnitId.Evaluate(iterator, args);
            }
        }
        protected override void UpdateVariables(StoryInstance instance)
        {
            if (m_ParamNum >= 3)
            {
                m_X.Evaluate(instance);
                m_Y.Evaluate(instance);
                m_Z.Evaluate(instance);
            }
            else if (m_ParamNum >= 1)
            {
                m_UnitId.Evaluate(instance);
            }
        }
        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                if (m_ParamNum >= 3)
                {
                    float x = m_X.Value;
                    float y = m_Y.Value;
                    float z = m_Z.Value;

                    LogSystem.Info("CameraLookatImmediately:{0} {1} {2}", x, y, z);

                    ArkCrossEngineMessage.Msg_RC_CameraLookat msg = new ArkCrossEngineMessage.Msg_RC_CameraLookat();
                    msg.x = x;
                    msg.y = y;
                    msg.z = z;
                    msg.is_immediately = true;
                    scene.NotifyAllUser(msg);
                }
                else if (m_ParamNum >= 1)
                {
                    int unitId = m_UnitId.Value;
                    NpcInfo npc = scene.SceneContext.GetCharacterInfoByUnitId(unitId) as NpcInfo;
                    if (null != npc)
                    {
                        MovementStateInfo msi = npc.GetMovementStateInfo();

                        LogSystem.Info("CameraLookatImmediately:{0}({1} {2} {3})", unitId, msi.PositionX, msi.PositionY, msi.PositionZ);

                        ArkCrossEngineMessage.Msg_RC_CameraLookat msg = new ArkCrossEngineMessage.Msg_RC_CameraLookat();
                        msg.x = msi.PositionX;
                        msg.y = msi.PositionY;
                        msg.z = msi.PositionZ;
                        msg.is_immediately = true;
                        scene.NotifyAllUser(msg);
                    }
                }
                else
                {
                    for (LinkedListNode<UserInfo> linkNode = scene.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo info = linkNode.Value;
                        User user = info.CustomData as User;

                        if (null != info && null != user)
                        {
                            MovementStateInfo msi = info.GetMovementStateInfo();

                            LogSystem.Info("CameraLookatImmediately:{0}({1} {2} {3})", info.GetId(), msi.PositionX, msi.PositionY, msi.PositionZ);

                            ArkCrossEngineMessage.Msg_RC_CameraLookat msg = new ArkCrossEngineMessage.Msg_RC_CameraLookat();
                            msg.x = msi.PositionX;
                            msg.y = msi.PositionY;
                            msg.z = msi.PositionZ;
                            msg.is_immediately = true;

                            user.SendMessage(msg);
                        }
                    }
                }
            }
            return false;
        }
        protected override void Load(ScriptableData.CallData callData)
        {
            m_ParamNum = callData.GetParamNum();
            if (m_ParamNum >= 3)
            {
                m_X.InitFromDsl(callData.GetParam(0));
                m_Y.InitFromDsl(callData.GetParam(1));
                m_Z.InitFromDsl(callData.GetParam(2));
            }
            else if (m_ParamNum >= 1)
            {
                m_UnitId.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<float> m_X = new StoryValue<float>();
        private IStoryValue<float> m_Y = new StoryValue<float>();
        private IStoryValue<float> m_Z = new StoryValue<float>();
        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private int m_ParamNum = 0;
    }
    /// <summary>
    /// camerafollowimmediately([unit_id]);
    /// </summary>
    internal class CameraFollowImmediatelyCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CameraFollowImmediatelyCommand cmd = new CameraFollowImmediatelyCommand();
            cmd.m_UnitId = m_UnitId.Clone();
            cmd.m_ParamNum = m_ParamNum;
            return cmd;
        }
        protected override void ResetState()
        {
        }
        protected override void UpdateArguments(object iterator, object[] args)
        {
            if (m_ParamNum > 0)
            {
                m_UnitId.Evaluate(iterator, args);
            }
        }
        protected override void UpdateVariables(StoryInstance instance)
        {
            if (m_ParamNum > 0)
            {
                m_UnitId.Evaluate(instance);
            }
        }
        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                if (0 == m_ParamNum)
                {
                    for (LinkedListNode<UserInfo> linkNode = scene.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                    {
                        UserInfo info = linkNode.Value;
                        User user = info.CustomData as User;

                        if (null != info && null != user)
                        {
                            MovementStateInfo msi = info.GetMovementStateInfo();

                            LogSystem.Info("CameraFollowImmediately:{0}", info.GetId());

                            ArkCrossEngineMessage.Msg_RC_CameraFollow msg = new ArkCrossEngineMessage.Msg_RC_CameraFollow();
                            msg.obj_id = info.GetId();
                            msg.is_immediately = true;

                            user.SendMessage(msg);
                        }
                    }
                }
                else
                {
                    int unitId = m_UnitId.Value;
                    NpcInfo npc = scene.SceneContext.GetCharacterInfoByUnitId(unitId) as NpcInfo;
                    if (null != npc)
                    {
                        LogSystem.Info("CameraFollowImmediately:{0}", npc.GetId());

                        ArkCrossEngineMessage.Msg_RC_CameraFollow msg = new ArkCrossEngineMessage.Msg_RC_CameraFollow();
                        msg.obj_id = npc.GetId();
                        msg.is_immediately = true;
                        scene.NotifyAllUser(msg);
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
        }

        private IStoryValue<int> m_UnitId = new StoryValue<int>();
        private int m_ParamNum = 0;
    }
    /// <summary>
    /// lockframe(scale);
    /// </summary>
    internal class LockFrameCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            LockFrameCommand cmd = new LockFrameCommand();
            cmd.m_Scale = m_Scale.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Scale.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Scale.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                float scale = m_Scale.Value;
                ArkCrossEngineMessage.Msg_RC_LockFrame msg = new ArkCrossEngineMessage.Msg_RC_LockFrame();
                msg.scale = scale;
                scene.NotifyAllUser(msg);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_Scale.InitFromDsl(callData.GetParam(0));
            }
        }

        private IStoryValue<float> m_Scale = new StoryValue<float>();
    }
    /// <summary>
    /// camerayaw(yaw,lag);
    /// </summary>
    internal class CameraYawCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CameraYawCommand cmd = new CameraYawCommand();
            cmd.m_Yaw = m_Yaw.Clone();
            cmd.m_SmoothLag = m_SmoothLag.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Yaw.Evaluate(iterator, args);
            m_SmoothLag.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Yaw.Evaluate(instance);
            m_SmoothLag.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                float yaw = m_Yaw.Value;
                int lag = m_SmoothLag.Value;

                ArkCrossEngineMessage.Msg_RC_CameraYaw msg = new ArkCrossEngineMessage.Msg_RC_CameraYaw();
                msg.yaw = yaw;
                msg.smooth_lag = lag;
                scene.NotifyAllUser(msg);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_Yaw.InitFromDsl(callData.GetParam(0));
                m_SmoothLag.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<float> m_Yaw = new StoryValue<float>();
        private IStoryValue<int> m_SmoothLag = new StoryValue<int>();
    }
    /// <summary>
    /// cameraheight(height,lag);
    /// </summary>
    internal class CameraHeightCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CameraHeightCommand cmd = new CameraHeightCommand();
            cmd.m_Height = m_Height.Clone();
            cmd.m_SmoothLag = m_SmoothLag.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Height.Evaluate(iterator, args);
            m_SmoothLag.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Height.Evaluate(instance);
            m_SmoothLag.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                float height = m_Height.Value;
                int lag = m_SmoothLag.Value;

                ArkCrossEngineMessage.Msg_RC_CameraHeight msg = new ArkCrossEngineMessage.Msg_RC_CameraHeight();
                msg.height = height;
                msg.smooth_lag = lag;
                scene.NotifyAllUser(msg);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_Height.InitFromDsl(callData.GetParam(0));
                m_SmoothLag.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<float> m_Height = new StoryValue<float>();
        private IStoryValue<int> m_SmoothLag = new StoryValue<int>();
    }
    /// <summary>
    /// cameradistance(dist,lag);
    /// </summary>
    internal class CameraDistanceCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CameraDistanceCommand cmd = new CameraDistanceCommand();
            cmd.m_Distance = m_Distance.Clone();
            cmd.m_SmoothLag = m_SmoothLag.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Distance.Evaluate(iterator, args);
            m_SmoothLag.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Distance.Evaluate(instance);
            m_SmoothLag.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                float dist = m_Distance.Value;
                int v = m_SmoothLag.Value;

                ArkCrossEngineMessage.Msg_RC_CameraDistance msg = new ArkCrossEngineMessage.Msg_RC_CameraDistance();
                msg.distance = dist;
                msg.smooth_lag = v;
                scene.NotifyAllUser(msg);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_Distance.InitFromDsl(callData.GetParam(0));
                m_SmoothLag.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<float> m_Distance = new StoryValue<float>();
        private IStoryValue<int> m_SmoothLag = new StoryValue<int>();
    }
    /// <summary>
    /// cameraenable(camera, enable);
    /// </summary>
    internal class CameraEnableCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            CameraEnableCommand cmd = new CameraEnableCommand();
            cmd.m_Camera = m_Camera.Clone();
            cmd.m_IsEnable = m_IsEnable.Clone();
            return cmd;
        }

        protected override void ResetState()
        {
        }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Camera.Evaluate(iterator, args);
            m_IsEnable.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Camera.Evaluate(instance);
            m_IsEnable.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                string camera = m_Camera.Value;
                string isEnable = m_IsEnable.Value;

                ArkCrossEngineMessage.Msg_RC_CameraEnable msg = new ArkCrossEngineMessage.Msg_RC_CameraEnable();
                msg.camera_name = camera;
                msg.is_enable = (isEnable == "true");
                scene.NotifyAllUser(msg);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 1)
            {
                m_Camera.InitFromDsl(callData.GetParam(0));
                m_IsEnable.InitFromDsl(callData.GetParam(1));
            }
        }

        private IStoryValue<string> m_Camera = new StoryValue<string>();
        private IStoryValue<string> m_IsEnable = new StoryValue<string>();
    }
}
