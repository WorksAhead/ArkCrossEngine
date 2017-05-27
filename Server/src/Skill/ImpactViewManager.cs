using System;
using System.Collections;
using System.Collections.Generic;
using ArkCrossEngineMessage;
using ArkCrossEngine;

namespace DashFire {
  internal sealed class ImpactViewManager {
    internal void Init(){
      //ImpactSystem.EventSendImpact += OnSendImpact;
      AbstractImpactLogic.EventImpactLogicDamage += OnImpactDamage;
      AbstractImpactLogic.EventImpactLogicSkill += OnImpactSkill;
      AbstractImpactLogic.EventImpactLogicRage += OnImpactRage;
      ImpactLogic_HitRecover.EventImpactHitRecover += OnHitRecover;
      ImpactLogic_AppendDamage.EventImpactAppendImpact += OnAppendImpact;
    }

    private void OnSendImpact(CharacterInfo sender, int targetId, int impactId) {
      /*Scene scene = sender.SceneContext.CustomData as Scene;
      if (null != scene) {
        Msg_CRC_SendImpactToEntity bd = new Msg_CRC_SendImpactToEntity();
        bd.sender_id = sender.GetId();
        bd.target_id = targetId;
        bd.impact_id = impactId;
        bd.sender_pos = new ArkCrossEngineMessage.Position3D();
        bd.sender_pos.x = sender.GetMovementStateInfo().PositionX;
        bd.sender_pos.y = sender.GetMovementStateInfo().PositionY;
        bd.sender_pos.z = sender.GetMovementStateInfo().PositionZ;
        bd.sender_dir = sender.GetMovementStateInfo().GetFaceDir();
        if (null != bd)
          scene.NotifyAreaUser(sender, bd);
      }*/
    }

    private void OnImpactSkill(CharacterInfo sender, int skillId)
    {
      Scene scene = sender.SceneContext.CustomData as Scene;
      if (null != scene) {
        SkillInfo skillInfo = sender.GetSkillStateInfo().GetCurSkillInfo();
        if (null == skillInfo || !skillInfo.IsSkillActivated) {
          scene.SkillSystem.StartSkill(sender.GetId(), skillId);

          Msg_RC_NpcSkill skillBuilder = new Msg_RC_NpcSkill();
          skillBuilder.npc_id = sender.GetId();
          skillBuilder.skill_id = skillId;
          ArkCrossEngineMessage.Position posBuilder1 = new ArkCrossEngineMessage.Position();
          posBuilder1.x = sender.GetMovementStateInfo().GetPosition3D().X;
          posBuilder1.z = sender.GetMovementStateInfo().GetPosition3D().Z;
          skillBuilder.stand_pos = posBuilder1;
          skillBuilder.face_direction = (float)sender.GetMovementStateInfo().GetFaceDir();

          LogSystem.Debug("Send Msg_RC_NpcSkill, EntityId={0}, SkillId={1}",
            sender.GetId(), skillId);
          scene.NotifyAreaUser(sender, skillBuilder);
        }
      }
    }
    private void OnImpactDamage(CharacterInfo entity, int attackerId, int damage, bool isKiller, bool isCritical, bool isOrdinary)
    {
      if (null != entity) {
        Msg_RC_ImpactDamage bd = new Msg_RC_ImpactDamage();
        bd.role_id = entity.GetId();
        bd.attacker_id = attackerId;
        bd.is_killer = isKiller;
        bd.is_critical = isCritical;
        bd.is_ordinary = isOrdinary;
        bd.hp = damage;

        Scene scene = entity.SceneContext.CustomData as Scene;
        if (null != scene) {
          if (entity.IsHaveStoryFlag(StoryListenFlagEnum.Damage)) {
            scene.StorySystem.SendMessage("objdamage", entity.GetId(), attackerId, damage, 0, isCritical ? 1 : 0);
            NpcInfo npc = entity as NpcInfo;
            if (null != npc) {
              scene.StorySystem.SendMessage("npcdamage:" + npc.GetUnitId(), entity.GetId(), attackerId, damage, 0, isCritical ? 1 : 0);
            }
          }

          int estimateDamage = damage;
          if (isCritical)
            estimateDamage /= 2;
          if (entity.Hp + estimateDamage <= 0) {
            scene.TryFireFinalBlow(entity);
          }
        }

        for (LinkedListNode<UserInfo> linkNode = entity.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
          UserInfo info = linkNode.Value;
          if (null != info && null != info.CustomData) {
            User u = info.CustomData as User;
            if (null != u) {
              u.SendMessage(bd);
            }
          }
        }
        entity.SetAttackerInfo(attackerId, 0, isKiller, isOrdinary, isCritical, damage, 0);
      }
    }

    private void OnImpactRage(CharacterInfo entity, int rage)
    {
      if (null != entity) {
        Msg_RC_ImpactRage bd = new Msg_RC_ImpactRage();
        bd.role_id = entity.GetId();
        bd.rage = entity.Rage;
        for (LinkedListNode<UserInfo> linkNode = entity.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
          UserInfo info = linkNode.Value;
          if (null != info && null != info.CustomData) {
            User u = info.CustomData as User;
            if (null != u) {
              u.SendMessage(bd);
            }
          }
        }
      }
    }

    private void OnHitRecover(CharacterInfo entity, string attribute, int value)
    {
      LogSys.Log(LOG_TYPE.DEBUG, "---hi recover " + attribute + ":" + value);
      Msg_RC_ImpactDamage bd = new Msg_RC_ImpactDamage();
      bd.role_id = entity.GetId();
      bd.attacker_id = entity.GetId();
      bd.is_killer = false;
      bd.is_critical = false;
      bd.is_ordinary = false;
      bd.hp = 0;
      bd.energy = 0;
      if (attribute == "HP") {
        bd.hp = value;
        entity.SetAttackerInfo(entity.GetId(), 0, false, false, false, value, 0);
      } else {
        bd.energy = value;
        entity.SetAttackerInfo(entity.GetId(), 0, false, false, false, 0, value);
      }
      for (LinkedListNode<UserInfo> linkNode = entity.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
        UserInfo info = linkNode.Value;
        if (null != info && null != info.CustomData) {
          User u = info.CustomData as User;
          if (null != u) {
            u.SendMessage(bd);
          }
        }
      }
    }

    private void OnAppendImpact(CharacterInfo target, int impactid, int senderid, Vector3 pos, float dir)
    {
      Msg_CRC_SendImpactToEntity msg = new Msg_CRC_SendImpactToEntity();
      msg.target_id = target.GetId();
      msg.sender_id = senderid;
      msg.sender_pos = new Position3D();
      msg.sender_pos.x = pos.X;
      msg.sender_pos.y = pos.Y;
      msg.sender_pos.z = pos.Z;
      msg.sender_dir = dir;
      msg.impact_id = impactid;
      msg.hit_count = 0;
      msg.skill_id = -1;
      LogSys.Log(LOG_TYPE.DEBUG, "---on append impact send Msg_CRC_SendImpactToEntity");
      for (LinkedListNode<UserInfo> linkNode = target.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
        UserInfo info = linkNode.Value;
        if (null != info && null != info.CustomData) {
          User u = info.CustomData as User;
          if (null != u) {
            LogSys.Log(LOG_TYPE.DEBUG, "---on append impact send Msg_CRC_SendImpactToEntity to " + u.GetKey());
            u.SendMessage(msg);
          }
        }
      }
    }

    internal static ImpactViewManager Instance
    {
      get
      {
        return s_Instance;
      }
    }
    private static ImpactViewManager s_Instance = new ImpactViewManager();
  }
}

