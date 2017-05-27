using System;
using System.Collections.Generic;
using ArkCrossEngine;
using ArkCrossEngineMessage;

namespace DashFire
{
  internal class AiView_NpcGeneral
  {
    internal AiView_NpcGeneral()
    {
      AbstractNpcStateLogic.OnNpcMove += this.OnNpcMove;
      AbstractNpcStateLogic.OnNpcFace += this.OnNpcFace;
      AbstractNpcStateLogic.OnNpcTargetChange += this.OnNpcTargetChange;
      AbstractNpcStateLogic.OnNpcSkill += this.OnNpcSkill;
      AbstractNpcStateLogic.OnNpcStopSkill += this.OnNpcStopSkill;
      AbstractNpcStateLogic.OnNpcAddImpact += this.OnNpcImpact;
      AbstractNpcStateLogic.OnNpcSendStoryMessage += this.OnNpcSendStoryMessage;
    }
    private void OnNpcMove(NpcInfo npc)
    {
      Scene scene = npc.SceneContext.CustomData as Scene;
      if (null != scene && !npc.GetMovementStateInfo().IsSkillMoving) {
        Msg_RC_NpcMove npcMoveBuilder = DataSyncUtility.BuildNpcMoveMessage(npc);
        if (null != npcMoveBuilder)
          scene.NotifyAreaUser(npc, npcMoveBuilder);
      }
    }
    private void OnNpcFace(NpcInfo npc)
    {
      Scene scene = npc.SceneContext.CustomData as Scene;
      if (null != scene) {
        Msg_RC_NpcFace npcFaceBuilder = DataSyncUtility.BuildNpcFaceMessage(npc);
        if (null != npcFaceBuilder)
          scene.NotifyAreaUser(npc, npcFaceBuilder);
      }
    }
    private void OnNpcTargetChange(NpcInfo npc)
    {
      Scene scene = npc.SceneContext.CustomData as Scene;
      if (null != scene) {
        Msg_RC_NpcTarget npcTargetBuilder = DataSyncUtility.BuildNpcTargetMessage(npc);
        if (null != npcTargetBuilder)
          scene.NotifyAreaUser(npc, npcTargetBuilder);
      }
    }
    private void OnNpcSkill(NpcInfo npc, int skillId)
    {
      Scene scene = npc.SceneContext.CustomData as Scene;
      if (null != scene) {
        SkillInfo skillInfo = npc.GetSkillStateInfo().GetCurSkillInfo();
        if (null == skillInfo || !skillInfo.IsSkillActivated) {
          SkillInfo curSkillInfo = npc.GetSkillStateInfo().GetSkillInfoById(skillId);
          if (null != curSkillInfo) {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (!curSkillInfo.IsInCd(curTime / 1000.0f)) {
              curSkillInfo.StartTime = curTime / 1000.0f;
              curSkillInfo.BeginCD();
              scene.SkillSystem.StartSkill(npc.GetId(), skillId);

              Msg_RC_NpcSkill skillBuilder = new Msg_RC_NpcSkill();
              skillBuilder.npc_id = npc.GetId();
              skillBuilder.skill_id = skillId;
              ArkCrossEngineMessage.Position posBuilder1 = new ArkCrossEngineMessage.Position();
              posBuilder1.x = npc.GetMovementStateInfo().GetPosition3D().X;
              posBuilder1.z = npc.GetMovementStateInfo().GetPosition3D().Z;
              skillBuilder.stand_pos = posBuilder1;
              skillBuilder.face_direction = (float)npc.GetMovementStateInfo().GetFaceDir();

              LogSystem.Debug("Send Msg_RC_NpcSkill, EntityId={0}, SkillId={1}",
                npc.GetId(), skillId);
              scene.NotifyAreaUser(npc, skillBuilder);
            }
          }
        }
      }
    }
    private void OnNpcImpact(NpcInfo npc, int impactId)
    {
      Scene scene = npc.SceneContext.CustomData as Scene;
      if (null != scene) {
        Msg_CRC_SendImpactToEntity sendImpactBuilder = new Msg_CRC_SendImpactToEntity();
        sendImpactBuilder.duration = -1;
        sendImpactBuilder.impact_id = impactId;
        sendImpactBuilder.sender_dir = npc.GetMovementStateInfo().GetFaceDir();
        sendImpactBuilder.sender_id = npc.GetId();
        Vector3 pos = npc.GetMovementStateInfo().GetPosition3D();
        sendImpactBuilder.sender_pos = new Position3D();
        sendImpactBuilder.sender_pos.x = pos.X;
        sendImpactBuilder.sender_pos.y = pos.Y;
        sendImpactBuilder.sender_pos.z = pos.Z;
        sendImpactBuilder.skill_id = -1;
        sendImpactBuilder.target_id = npc.GetId();
        scene.NotifyAreaUser(npc, sendImpactBuilder);
      }
    }
    private void OnNpcStopSkill(NpcInfo npc)
    {
      Scene scene = npc.SceneContext.CustomData as Scene;
      if (null != scene) {
        SkillInfo skillInfo = npc.GetSkillStateInfo().GetCurSkillInfo();
        if (null == skillInfo || skillInfo.IsSkillActivated) {
          scene.SkillSystem.StopSkill(npc.GetId());
        }

        Msg_CRC_NpcStopSkill skillBuilder = new Msg_CRC_NpcStopSkill();
        skillBuilder.npc_id = npc.GetId();

        LogSystem.Debug("Send Msg_RC_NpcStopSkill, EntityId={0}",
          npc.GetId());
        scene.NotifyAreaUser(npc, skillBuilder);
      }
    }
    private void OnNpcSendStoryMessage(NpcInfo npc, string msgId, object[] args)
    {
      Scene scene = npc.SceneContext.CustomData as Scene;
      if (null != scene) {
        scene.StorySystem.SendMessage(msgId, args);
      }
    }
  }
}
