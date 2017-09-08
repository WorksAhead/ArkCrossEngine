using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArkCrossEngine;

namespace DashFire
{
    internal sealed class ControlSystemOperation
    {
        internal void Init(Scene scene)
        {
            m_Scene = scene;
        }
        internal void Reset()
        {
            m_ControlSystem.Reset();
        }
        internal void AdjustCharacterFaceDir(int id, float faceDir)
        {
            const float c_PI = (float)Math.PI;
            const float c_2PI = (float)Math.PI * 2;
            CharacterInfo info = m_Scene.SceneContext.GetCharacterInfoById(id);
            if (null != info)
            {
                float curFaceDir = info.GetMovementStateInfo().GetFaceDir();
                float deltaDir = ((faceDir + c_2PI) - curFaceDir) % c_2PI;
                if (deltaDir > c_PI)
                {
                    deltaDir = c_2PI - deltaDir;
                }
                if (deltaDir > 0.1f)
                {
                    int ctrlId = ControllerIdCalculator.Calc(ControllerType.FaceDir, id);
                    FaceDirController ctrl = m_FaceControllerPool.Alloc();
                    if (null != ctrl)
                    {
                        ctrl.Init(m_Scene.SceneContext, ctrlId, id, faceDir);
                        m_ControlSystem.AddController(ctrl);
                    }
                }
                else
                {
                    info.GetMovementStateInfo().SetFaceDir(faceDir);
                }
            }
        }
        internal void AdjustCharacterMoveDir(int id, float moveDir)
        {
            const float c_PI = (float)Math.PI;
            const float c_2PI = (float)Math.PI * 2;
            CharacterInfo info = m_Scene.SceneContext.GetCharacterInfoById(id);
            if (null != info)
            {
                float curMoveDir = info.GetMovementStateInfo().GetMoveDir();
                float deltaDir = ((moveDir + c_2PI) - curMoveDir) % c_2PI;
                if (deltaDir > c_PI)
                {
                    deltaDir = c_2PI - deltaDir;
                }
                if (deltaDir > 0.1f && deltaDir < c_2PI / 8)
                {
                    int ctrlId = ControllerIdCalculator.Calc(ControllerType.MoveDir, id);
                    MoveDirController ctrl = m_MoveDirControllerPool.Alloc();
                    if (null != ctrl)
                    {
                        ctrl.Init(m_Scene.SceneContext, ctrlId, id, moveDir);
                        m_ControlSystem.AddController(ctrl);
                    }
                }
                else
                {
                    info.GetMovementStateInfo().SetMoveDir(moveDir);
                }
            }
        }
        internal void Tick()
        {
            m_ControlSystem.Tick();
        }

        internal ControlSystemOperation()
        {
            m_FaceControllerPool.Init(128);
            m_MoveDirControllerPool.Init(128);
        }

        private ObjectPool<FaceDirController> m_FaceControllerPool = new ObjectPool<FaceDirController>();
        private ObjectPool<MoveDirController> m_MoveDirControllerPool = new ObjectPool<MoveDirController>();
        private ControlSystem m_ControlSystem = new ControlSystem();
        private Scene m_Scene = null;
    }
}
