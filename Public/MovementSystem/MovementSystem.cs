using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using ArkCrossEngine;
using ArkCrossEngineSpatial;

namespace ArkCrossEngine
{
  public class MovementSystem
  {
    public const float c_ForecastDistance = 0.75f;
    public MovementSystem()
    {
    }
    public void Reset()
    {
      m_LastTickTime = 0;
    }
    public void SetNpcManager(NpcManager npcMgr)
    {
      m_NpcMgr = npcMgr;
    }
    public void SetUserManager(UserManager userMgr)
    {
      m_UserMgr = userMgr;
    }
    public void Tick()
    {
      if (0 == m_LastTickTime) {
        m_LastTickTime = TimeUtility.GetServerMilliseconds();
        LogSystem.Debug("MovementSystem LastTickTime:{0}", m_LastTickTime);
      } else {
        long delta = TimeUtility.GetServerMilliseconds() - m_LastTickTime;
        m_LastTickTime = TimeUtility.GetServerMilliseconds();
        if (delta > 1000)
          delta = 1000;
        if (null != m_NpcMgr) {
          for (LinkedListNode<NpcInfo> node = m_NpcMgr.Npcs.FirstValue; null != node; node = node.Next) {
            NpcInfo npc = node.Value;
            if (null != npc) {
              MoveNpc(npc, delta);
              /*
              npc.TransformShape();
              if (null != npc.SceneContext.SightManager) {
                npc.SceneContext.SightManager.UpdateObject(npc);
              }
              */
            }
          }
        }
        if (null != m_UserMgr) {
          for (LinkedListNode<UserInfo> node = m_UserMgr.Users.FirstValue; null != node; node = node.Next) {
            UserInfo user = node.Value;
            if (null != user) {
              MoveUser(user, delta);
              /*
              user.TransformShape();
              if (null != user.SceneContext.SightManager) {
                user.SceneContext.SightManager.UpdateObject(user);
              }
              */
            }
          }
        }
      }
    }

    private void MoveNpc(NpcInfo obj, long deltaTime)
    {
      if (obj.IsHaveStateFlag(CharacterState_Type.CST_Sleep)
        || obj.IsHaveStateFlag(CharacterState_Type.CST_FixedPosition)
        || null!=obj.ControllerObject) {
        return;
      }
      MovementStateInfo msi = obj.GetMovementStateInfo();
      //npc执行移动时忽略阻挡与避让，这些行为由ai模块在规划其路径时执行。
      if (!obj.IsDead() &&obj.CanMove && msi.IsMoving && !msi.IsSkillMoving) {
        Vector3 pos = msi.GetPosition3D();
        float cos_angle = (float)msi.MoveDirCosAngle;
        float sin_angle = (float)msi.MoveDirSinAngle;
        float speed = (float)obj.GetActualProperty().MoveSpeed * (float)obj.VelocityCoefficient;
        float distance = (speed * (float)(int)deltaTime) / 1000.0f;
        
        //LogSystem.Debug("MovementSystem npc:{0} speed:{1} deltaTime:{2} distance:{3}", obj.GetId(), speed, deltaTime, distance);

        float x = 0, y = 0;
        if (msi.CalcDistancSquareToTarget() < distance * distance) {
          x = msi.TargetPosition.X;
          y = msi.TargetPosition.Z;
          AdjustPosition(obj.SpatialSystem, ref x, ref y);
          Vector2 newPos = new Vector2(x, y);
          msi.SetPosition2D(newPos);
        } else {
          float len = pos.Length();
          y = pos.Z + distance * cos_angle;
          x = pos.X + distance * sin_angle;
          AdjustPosition(obj.SpatialSystem, ref x, ref y);
          Vector2 newPos = new Vector2(x, y);
          msi.SetPosition2D(newPos);
        }
      }
    }

    private void MoveUser(UserInfo obj, long deltaTime)
    {
      MovementStateInfo msi = obj.GetMovementStateInfo();
      if (null != obj.ControlledObject) {
        MovementStateInfo ctrlMsi = obj.ControlledObject.GetMovementStateInfo();
        ctrlMsi.IsMoving = msi.IsMoving;
        ctrlMsi.SetFaceDir(msi.GetFaceDir());
        ctrlMsi.SetMoveDir(msi.GetMoveDir());
      }
      if (obj.IsHaveStateFlag(CharacterState_Type.CST_Sleep) || obj.IsHaveStateFlag(CharacterState_Type.CST_FixedPosition)) {
        return;
      }
      //玩家移动中忽略阻挡，由客户端与AI来规划路径。
      if (!obj.IsDead() && msi.IsMoving && !msi.IsSkillMoving && !msi.IsMoveMeetObstacle) {
        Vector3 pos = msi.GetPosition3D();
        float speed = (float)obj.GetRealControlledObject().GetActualProperty().MoveSpeed * (float)obj.VelocityCoefficient;
        float distance = (speed * (float)(int)deltaTime) / 1000.0f;

        //LogSystem.Debug("MovementSystem user:{0} speed:{1} deltaTime:{2} distance:{3}", obj.GetId(), speed, deltaTime, distance);

        float x = 0, y = 0;
        Vector2 newPos = new Vector2();
        if (obj.GetAiStateInfo().CurState != (int)AiStateId.Invalid && msi.CalcDistancSquareToTarget() < distance * distance) {
          x = msi.TargetPosition.X;
          y = msi.TargetPosition.Z;
          AdjustPosition(obj.SpatialSystem, ref x, ref y);
          newPos = new Vector2(x, y);
          msi.SetPosition2D(newPos);
        } else {
          float cosV = (float)msi.MoveDirCosAngle;
          float sinV = (float)msi.MoveDirSinAngle;
          y = pos.Z + distance * cosV;
          x = pos.X + distance * sinV;
          AdjustPosition(obj.SpatialSystem, ref x, ref y);
          newPos = new Vector2(x, y);
          msi.SetPosition2D(newPos);
        }
        if (null != obj.ControlledObject) {
          obj.ControlledObject.GetMovementStateInfo().SetPosition2D(newPos);
        }
      }
    }

    private void AdjustPosition(ArkCrossEngineSpatial.ISpatialSystem spatialSystem, ref float x, ref float y)
    {
      if (x < 0)
        x = 0;
      if (y < 0)
        y = 0;
      if (x > spatialSystem.Width)
        x = spatialSystem.Width;
      if (y > spatialSystem.Height)
        y = spatialSystem.Height;
    }

    private long m_LastTickTime = 0;
    private NpcManager m_NpcMgr = null;
    private UserManager m_UserMgr = null;
  }
}
