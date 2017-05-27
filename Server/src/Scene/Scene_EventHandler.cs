using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArkCrossEngineMessage;
using Google.ProtocolBuffers;
using ArkCrossEngineSpatial;
using Lobby_RoomServer;
using ArkCrossEngine;

namespace DashFire
{
  internal sealed partial class Scene
  {
    internal void DestroyEntities(int[] unit_ids)
    {
      Msg_RC_DestroyNpc destroyNpcBuilder = new Msg_RC_DestroyNpc();
      for (int i = 0; i < unit_ids.Length; i++) {
        NpcInfo npc = NpcManager.GetNpcInfoByUnitId(unit_ids[i]);
        if (npc != null) {
          destroyNpcBuilder.npc_id = npc.GetId();
          NotifyAllUser(destroyNpcBuilder);
          NpcManager.RemoveNpc(npc.GetId());
        }
      }
    }

    internal void ReloadObjects()
    {
      if (null != m_MapData) {
        //刷新场景逻辑
        foreach (SceneLogicConfig cfg in m_MapData.m_SceneLogicMgr.GetData().Values) {
          SceneLogicInfo sli = m_SceneLogicInfoMgr.GetSceneLogicInfoByConfigId(cfg.GetId());
          if (null == sli) {
            m_SceneLogicInfoMgr.AddSceneLogicInfo(cfg);
          }
        }
        //刷新npc
        foreach (Data_Unit unit in m_MapData.m_UnitMgr.GetData().Values) {
          if (unit.m_IsEnable) {
            NpcInfo npc = m_NpcMgr.GetNpcInfoByUnitId(unit.GetId());
            if (null == npc) {
              m_NpcMgr.AddNpc(unit);
            }
          }
        }
      }
    }

    internal void TryFireFinalBlow(CharacterInfo entity)
    {
      if (null != entity) {
        int objId = entity.GetId();
        if (IsPvpScene) {
          if (entity is UserInfo && GetLivingUserCount()<=2) {
            m_StorySystem.SendMessage("finalblow", objId);
          }
        } else {
          if (1 == GetBattleNpcCount()) {
            m_StorySystem.SendMessage("finalblow", objId);
          }
        }
      }
    }

    internal void  OnUserRevive(Msg_LR_UserReLive msg)
    {
      User user = m_Room.GetUserByGuid(msg.UserGuid);
      if (null != user) {
        UserInfo info = user.Info;
        info.SetHp(Operate_Type.OT_Absolute, info.GetActualProperty().HpMax);
        info.SetEnergy(Operate_Type.OT_Absolute, info.GetActualProperty().EnergyMax);
        info.DeadTime = 0;
        DataSyncUtility.SyncUserPropertyToCaredUsers(info);
        DataSyncUtility.SyncUserReliveToCaredUsers(info);
      }
    }
    internal void SummonPartner(Msg_CR_SummonPartner msg)
    {
      UserInfo userInfo = UserManager.GetUserInfo(msg.obj_id);
      if (null != userInfo) {
        // summonpartner
        PartnerInfo partnerInfo =userInfo.GetPartnerInfo();
        if(null != partnerInfo && (TimeUtility.GetServerMilliseconds() - userInfo.LastSummonPartnerTime > partnerInfo.CoolDown || userInfo.LastSummonPartnerTime == 0)){
          Data_Unit data = new Data_Unit();
          data.m_Id = -1;
          data.m_LinkId = partnerInfo.LinkId;
          data.m_CampId = userInfo.GetCampId();
          data.m_Pos = userInfo.GetMovementStateInfo().GetPosition3D();
          data.m_RotAngle = 0;
          data.m_AiLogic = partnerInfo.GetAiLogic();
          data.m_AiParam[0] = "";
          data.m_AiParam[1] = "";
          data.m_AiParam[2] = partnerInfo.GetAiParam().ToString();
          data.m_IsEnable = true;
          NpcInfo npc = NpcManager.AddNpc(data);
          if (null != npc){
            AppendAttributeConfig aac = AppendAttributeConfigProvider.Instance.GetDataById(partnerInfo.GetAppendAttrConfigId());
            float inheritAttackAttrPercent = partnerInfo.GetInheritAttackAttrPercent();
            float inheritDefenceAttrPercent = partnerInfo.GetInheritDefenceAttrPercent();
            if (null != aac) {
              // attack
              npc.GetBaseProperty().SetAttackBase(Operate_Type.OT_Absolute, (int)(userInfo.GetActualProperty().AttackBase * inheritAttackAttrPercent));
              npc.GetBaseProperty().SetFireDamage(Operate_Type.OT_Absolute, userInfo.GetActualProperty().FireDamage * inheritAttackAttrPercent);
              npc.GetBaseProperty().SetIceDamage(Operate_Type.OT_Absolute, userInfo.GetActualProperty().IceDamage * inheritAttackAttrPercent);
              npc.GetBaseProperty().SetPoisonDamage(Operate_Type.OT_Absolute, userInfo.GetActualProperty().PoisonDamage * inheritAttackAttrPercent);
              // defence
              npc.GetBaseProperty().SetHpMax(Operate_Type.OT_Absolute, (int)(userInfo.GetActualProperty().HpMax * inheritDefenceAttrPercent));
              npc.GetBaseProperty().SetEnergyMax(Operate_Type.OT_Absolute, (int)(userInfo.GetActualProperty().EnergyMax * inheritDefenceAttrPercent));
              npc.GetBaseProperty().SetADefenceBase(Operate_Type.OT_Absolute, (int)(userInfo.GetActualProperty().ADefenceBase * inheritDefenceAttrPercent));
              npc.GetBaseProperty().SetMDefenceBase(Operate_Type.OT_Absolute, (int)(userInfo.GetActualProperty().MDefenceBase * inheritDefenceAttrPercent));
              npc.GetBaseProperty().SetFireERD(Operate_Type.OT_Absolute, userInfo.GetActualProperty().FireERD * inheritDefenceAttrPercent);
              npc.GetBaseProperty().SetIceERD(Operate_Type.OT_Absolute, userInfo.GetActualProperty().IceERD * inheritDefenceAttrPercent);
              npc.GetBaseProperty().SetPoisonERD(Operate_Type.OT_Absolute, userInfo.GetActualProperty().PoisonERD * inheritDefenceAttrPercent);
              // reset hp & energy
              npc.SetHp(Operate_Type.OT_Absolute, npc.GetBaseProperty().HpMax);
              npc.SetEnergy(Operate_Type.OT_Absolute, npc.GetBaseProperty().EnergyMax);
            }
            npc.SetAIEnable(true);
            npc.GetSkillStateInfo().RemoveAllSkill();
            npc.BornTime = TimeUtility.GetServerMilliseconds();
            List<int> skillList = partnerInfo.GetSkillList();
            if (null != skillList) {
              for (int i = 0; i < skillList.Count; ++i) {
                SkillInfo skillInfo = new SkillInfo(skillList[i]);
                npc.GetSkillStateInfo().AddSkill(skillInfo);
              }
            }
            userInfo.LastSummonPartnerTime = TimeUtility.GetServerMilliseconds();
            npc.OwnerId = userInfo.GetId();
            userInfo.PartnerId = npc.GetId();
            if (partnerInfo.BornSkill > 0) {
              SkillInfo skillInfo = new SkillInfo(partnerInfo.BornSkill);
              npc.GetSkillStateInfo().AddSkill(skillInfo);
            }
            ArkCrossEngineMessage.Msg_RC_CreateNpc builder = DataSyncUtility.BuildCreateNpcMessage(npc);
            NotifyAllUser(builder);
          }
        }
      }
    }

    internal void OnSummonNpc(Msg_CRC_SummonNpc msg)
    {
      CharacterInfo char_Info = SceneContext.GetCharacterInfoById(msg.summon_owner_id);
      if (null == char_Info) {
        return;
      }
      Data_Unit data = new Data_Unit();
      data.m_IsEnable = true;
      NpcInfo npc = SummonNpc(msg.npc_id, msg.summon_owner_id, msg.owner_skillid,
                              msg.link_id, msg.model_prefab, msg.skill_id, msg.ai_id, msg.follow_dead,
                              msg.pos_x, msg.pos_y, msg.pos_z, msg.ai_params);
      if (npc != null) {
        npc.OwnerId = char_Info.OwnerId;
        msg.npc_id = npc.GetId();
        msg.owner_id = char_Info.OwnerId;
        NotifyAllUser(msg);
      }
    }

    internal NpcInfo SummonNpc(int id, int owner_id, int owner_skillid, int npc_type_id, string modelPrefab, int skillid, int ailogicid, bool followsummonerdead,
                                    float x, float y, float z, string aiparamstr)
    {
      CharacterInfo charObj = SceneContext.GetCharacterInfoById(owner_id);
      if (charObj == null) {
        return null;
      }
      NpcInfo npc = null;
      SkillInfo ownerSkillInfo = charObj.GetSkillStateInfo().GetSkillInfoById(owner_skillid);
      if (null != ownerSkillInfo) {
        if (null != ownerSkillInfo.m_EnableSummonNpcs && ownerSkillInfo.m_EnableSummonNpcs.Contains(npc_type_id)) {
          //ownerSkillInfo.m_EnableSummonNpcs.Remove(npc_type_id);

          Data_Unit data = new Data_Unit();
          data.m_Id = -1;
          data.m_LinkId = npc_type_id;
          data.m_CampId = charObj.GetCampId();
          data.m_IsEnable = true;
          data.m_Pos = new Vector3(x, y, z);
          data.m_RotAngle = 0;
          data.m_AiLogic = ailogicid;
          if (!string.IsNullOrEmpty(aiparamstr)) {
            string[] strarry = aiparamstr.Split(new char[] { ',' }, 8);
            int i = 0;
            foreach (string str in strarry) {
              data.m_AiParam[i++] = str;
            }
          }
          npc = NpcManager.AddNpc(data);
          if (!string.IsNullOrEmpty(modelPrefab)) {
            npc.SetModel(modelPrefab);
          }
          npc.FollowSummonerDead = followsummonerdead;
          SkillInfo skillinfo = new SkillInfo(skillid);
          npc.GetSkillStateInfo().AddSkill(skillinfo);
          npc.GetMovementStateInfo().SetPosition(data.m_Pos);
          npc.SummonOwnerId = charObj.GetId();
          charObj.GetSkillStateInfo().AddSummonObject(npc);
          AbstractNpcStateLogic.OnNpcSkill(npc, skillid);
        }
      }
      return npc;
    }

    private void OnHightlightPrompt(int userId, int dict, object[] args)
    {
      Msg_RC_HighlightPrompt builder = new Msg_RC_HighlightPrompt();
      builder.dict_id = dict;
      foreach (object arg in args) {
        builder.argument.Add(arg.ToString());
      }
      if (userId > 0) {
        UserInfo info = UserManager.GetUserInfo(userId);
        if (null != info) {
          User user = info.CustomData as User;
          if (null != user) {
            user.SendMessage(builder);
          }
        }
      } else {
        NotifyAllUser(builder);
      }
    }
  }
}
