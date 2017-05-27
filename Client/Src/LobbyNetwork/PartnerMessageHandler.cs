using System;
using System.Collections.Generic;
using LitJson;

namespace ArkCrossEngine.Network
{
    internal sealed partial class LobbyNetworkSystem
    {
        private void PartnerMessageInit()
        {
            GfxSystem.EventChannelForLogic.Subscribe("ge_summon_partner", "partner", SummonPartner);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_select_partner", "partner", SelectPartner);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_upgrade_partner_level", "partner", UpgradePartnerLevel);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_upgrade_partner_stage", "partner", UpgradePartnerStage);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_compound_partner", "partner", CompoundPartner);
            PartnerMessageHandler();
        }

        private void PartnerMessageHandler()
        {
            RegisterMsgHandler(JsonMessageID.GetPartner, typeof(ArkCrossEngineMessage.Msg_LC_GetPartner), HandleGetPartner);
            RegisterMsgHandler(JsonMessageID.SelectPartnerResult, typeof(ArkCrossEngineMessage.Msg_LC_SelectPartnerResult), HandleSelectPartnerResult);
            RegisterMsgHandler(JsonMessageID.UpgradePartnerLevelResult, typeof(ArkCrossEngineMessage.Msg_LC_UpgradePartnerLevelResult), HandleUpgradePartnerLevelResult);
            RegisterMsgHandler(JsonMessageID.UpgradePartnerStageResult, typeof(ArkCrossEngineMessage.Msg_LC_UpgradeParnerStageResult), HandleUpgradePartnerStageResult);
            RegisterMsgHandler(JsonMessageID.CompoundPartnerResult, typeof(ArkCrossEngineMessage.Msg_LC_CompoundPartnerResult), HandleCompoundPartnerResult);
        }
        private void SummonPartner()
        {
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            if (null == roleself) return;
            roleself.PartnerStateInfo.ActivePartnerHpPercent = 1.0f;
            if (WorldSystem.Instance.IsPveScene())
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself) return;
                ///
                // summonpartner
                PartnerInfo partnerInfo = roleself.PartnerStateInfo.GetActivePartner();
                if (null != partnerInfo && (TimeUtility.GetServerMilliseconds() - playerself.LastSummonPartnerTime > partnerInfo.CoolDown || playerself.LastSummonPartnerTime == 0))
                {
                    Data_Unit data = new Data_Unit();
                    data.m_Id = -1;
                    data.m_LinkId = partnerInfo.LinkId;
                    data.m_CampId = playerself.GetCampId();
                    data.m_Pos = playerself.GetMovementStateInfo().GetPosition3D();
                    data.m_RotAngle = 0;
                    data.m_AiLogic = partnerInfo.GetAiLogic();
                    data.m_AiParam[0] = "";
                    data.m_AiParam[1] = "";
                    data.m_AiParam[2] = partnerInfo.GetAiParam().ToString();
                    data.m_IsEnable = true;
                    NpcInfo npc = WorldSystem.Instance.NpcManager.AddNpc(data);
                    if (null != npc)
                    {
                        AppendAttributeConfig aac = AppendAttributeConfigProvider.Instance.GetDataById(partnerInfo.GetAppendAttrConfigId());
                        float inheritAttackAttrPercent = partnerInfo.GetInheritAttackAttrPercent();
                        float inheritDefenceAttrPercent = partnerInfo.GetInheritDefenceAttrPercent();
                        if (null != aac)
                        {
                            // attack
                            npc.GetBaseProperty().SetAttackBase(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().AttackBase * inheritAttackAttrPercent));
                            npc.GetBaseProperty().SetFireDamage(Operate_Type.OT_Absolute, playerself.GetActualProperty().FireDamage * inheritAttackAttrPercent);
                            npc.GetBaseProperty().SetIceDamage(Operate_Type.OT_Absolute, playerself.GetActualProperty().IceDamage * inheritAttackAttrPercent);
                            npc.GetBaseProperty().SetPoisonDamage(Operate_Type.OT_Absolute, playerself.GetActualProperty().PoisonDamage * inheritAttackAttrPercent);
                            // defence
                            npc.GetBaseProperty().SetHpMax(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().HpMax * inheritDefenceAttrPercent));
                            npc.GetBaseProperty().SetEnergyMax(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().EnergyMax * inheritDefenceAttrPercent));
                            npc.GetBaseProperty().SetADefenceBase(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().ADefenceBase * inheritDefenceAttrPercent));
                            npc.GetBaseProperty().SetMDefenceBase(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().MDefenceBase * inheritDefenceAttrPercent));
                            npc.GetBaseProperty().SetFireERD(Operate_Type.OT_Absolute, playerself.GetActualProperty().FireERD * inheritDefenceAttrPercent);
                            npc.GetBaseProperty().SetIceERD(Operate_Type.OT_Absolute, playerself.GetActualProperty().IceERD * inheritDefenceAttrPercent);
                            npc.GetBaseProperty().SetPoisonERD(Operate_Type.OT_Absolute, playerself.GetActualProperty().PoisonERD * inheritDefenceAttrPercent);
                            // reset hp & energy
                            npc.SetHp(Operate_Type.OT_Absolute, npc.GetBaseProperty().HpMax);
                            npc.SetEnergy(Operate_Type.OT_Absolute, npc.GetBaseProperty().EnergyMax);
                        }
                        npc.SetAIEnable(true);
                        npc.GetSkillStateInfo().RemoveAllSkill();
                        npc.BornTime = TimeUtility.GetServerMilliseconds();
                        List<int> skillList = partnerInfo.GetSkillList();
                        if (null != skillList)
                        {
                            for (int i = 0; i < skillList.Count; ++i)
                            {
                                SkillInfo skillInfo = new SkillInfo(skillList[i]);
                                npc.GetSkillStateInfo().AddSkill(skillInfo);
                            }
                        }
                        playerself.LastSummonPartnerTime = TimeUtility.GetServerMilliseconds();
                        npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                        npc.OwnerId = playerself.GetId();
                        playerself.PartnerId = npc.GetId();
                        EntityManager.Instance.CreateNpcView(npc.GetId());
                        if (partnerInfo.BornSkill > 0)
                        {
                            SkillInfo skillInfo = new SkillInfo(partnerInfo.BornSkill);
                            npc.GetSkillStateInfo().AddSkill(skillInfo);
                            npc.SkillController.ForceStartSkill(partnerInfo.BornSkill);
                        }
                        CharacterView view = EntityManager.Instance.GetCharacterViewById(npc.GetId());
                        if (null != view)
                        {
                            GfxSystem.SetLayer(view.Actor, "PhysicObj");
                        }
                        GfxSystem.PublishGfxEvent("ge_partner_summon_result", "ui", true);
                    }
                    else
                    {
                        GfxSystem.PublishGfxEvent("ge_partner_summon_result", "ui", false);
                    }
                }
                else
                {
                    GfxSystem.PublishGfxEvent("ge_partner_summon_result", "ui", false);
                }
            }
            if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
            {
                NetworkSystem.Instance.SummonPartner();
            }
        }

        private void SelectPartner(int id)
        {
            try
            {
                RoleInfo role = LobbyClient.Instance.CurrentRole;
                if (null != role && role.PartnerStateInfo.GetActivePartnerId() != id)
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.SelectPartner);
                    msg.m_JsonData.Set("m_Guid", m_Guid);
                    ArkCrossEngineMessage.Msg_CL_SelectPartner protoData = new ArkCrossEngineMessage.Msg_CL_SelectPartner();
                    protoData.m_PartnerId = id;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleSelectPartnerResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SelectPartnerResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SelectPartnerResult;
            if (null != protoData)
            {
                if ((int)PartnerMsgResultEnum.SUCCESS == protoData.m_ResultCode)
                {
                    RoleInfo role = LobbyClient.Instance.CurrentRole;
                    if (null != role)
                    {
                        role.PartnerStateInfo.SetActivePartner(protoData.m_PartnerId);
                        UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                        if (null != user)
                        {
                            user.SetPartnerInfo(role.PartnerStateInfo.GetActivePartner());
                            user.ParterChanged = true;
                        }
                    }
                }
                GfxSystem.PublishGfxEvent("ge_partner_select_result", "ui", protoData.m_ResultCode);
            }
        }
        private void UpgradePartnerLevel(int id)
        {
            try
            {
                RoleInfo role = LobbyClient.Instance.CurrentRole;
                if (null != role && role.PartnerStateInfo.IsHavePartner(id))
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.UpgradePartnerLevel);
                    msg.m_JsonData.Set("m_Guid", m_Guid);
                    ArkCrossEngineMessage.Msg_CL_UpgradePartnerLevel protoData = new ArkCrossEngineMessage.Msg_CL_UpgradePartnerLevel();
                    protoData.m_PartnerId = id;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleUpgradePartnerLevelResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UpgradePartnerLevelResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UpgradePartnerLevelResult;
            if (null != protoData)
            {
                RoleInfo role = LobbyClient.Instance.CurrentRole;
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                PartnerInfo partner = role.PartnerStateInfo.GetPartnerInfoById(protoData.m_PartnerId);
                if (partner == null) return;
                if (null != user)
                {
                    user.ParterChanged = true;
                }
                if ((int)PartnerMsgResultEnum.SUCCESS == protoData.m_ResultCode ||
                    (int)PartnerMsgResultEnum.FAILED == protoData.m_ResultCode ||
                    (int)PartnerMsgResultEnum.DEMOTION == protoData.m_ResultCode)
                {
                    PartnerConfig partnerConfig = PartnerConfigProvider.Instance.GetDataById(protoData.m_PartnerId);
                    PartnerLevelUpConfig pluConfig = PartnerLevelUpConfigProvider.Instance.GetDataById(partner.CurAdditionLevel);
                    if (null != partnerConfig && null != pluConfig)
                    {
                        role.ReduceItemData(partnerConfig.LevelUpItemId, 0, pluConfig.ItemCost);
                        role.Money -= pluConfig.GoldCost;
                    }
                }
                // 成功失败都同步等级
                if (null != role)
                {
                    role.PartnerStateInfo.SetPartnerLevel(protoData.m_PartnerId, protoData.m_CurLevel);
                }
                GfxSystem.PublishGfxEvent("ge_partner_upgrade_level_result", "ui", protoData.m_ResultCode);
            }
        }
        private void UpgradePartnerStage(int id)
        {
            try
            {
                RoleInfo role = LobbyClient.Instance.CurrentRole;
                if (null != role && role.PartnerStateInfo.IsHavePartner(id))
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.UpgradePartnerStage);
                    msg.m_JsonData.Set("m_Guid", m_Guid);
                    ArkCrossEngineMessage.Msg_CL_UpgradePartnerStage protoData = new ArkCrossEngineMessage.Msg_CL_UpgradePartnerStage();
                    protoData.m_PartnerId = id;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleUpgradePartnerStageResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UpgradeParnerStageResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UpgradeParnerStageResult;
            if (null != protoData)
            {
                if ((int)PartnerMsgResultEnum.SUCCESS == protoData.m_ResultCode)
                {
                    RoleInfo role = LobbyClient.Instance.CurrentRole;
                    PartnerInfo partner = role.PartnerStateInfo.GetPartnerInfoById(protoData.m_PartnerId);
                    if (null == partner) return;
                    PartnerConfig partnerConfig = PartnerConfigProvider.Instance.GetDataById(protoData.m_PartnerId);
                    PartnerStageUpConfig psuConfig = PartnerStageUpConfigProvider.Instance.GetDataById(partner.CurSkillStage);
                    if (null != partnerConfig && null != psuConfig)
                    {
                        role.ReduceItemData(partnerConfig.StageUpItemId, 0, psuConfig.ItemCost);
                        role.Money -= psuConfig.GoldCost;
                    }
                    UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                    if (null != user)
                    {
                        user.ParterChanged = true;
                    }
                    if (null != role)
                    {
                        role.PartnerStateInfo.SetPartnerStage(protoData.m_PartnerId, protoData.m_CurStage);
                    }
                }
                GfxSystem.PublishGfxEvent("ge_partner_upgrade_skill_result", "ui", protoData.m_ResultCode);
            }
        }
        private void HandleGetPartner(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_GetPartner protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_GetPartner;
            if (null != protoData)
            {
                RoleInfo role = LobbyClient.Instance.CurrentRole;
                if (null != role)
                {
                    role.PartnerStateInfo.AddPartner(protoData.m_PartnerId, 1, 1);
                    GfxSystem.PublishGfxEvent("ge_add_partner", "ui", protoData.m_PartnerId);
                }
            }
        }

        private void CompoundPartner(int partnerId)
        {
            try
            {
                RoleInfo role = LobbyClient.Instance.CurrentRole;
                if (null != role && !role.PartnerStateInfo.IsHavePartner(partnerId))
                {
                    JsonMessage msg = new JsonMessage(JsonMessageID.CompoundPartner);
                    msg.m_JsonData.Set("m_Guid", m_Guid);
                    ArkCrossEngineMessage.Msg_CL_CompoundPartner protoData = new ArkCrossEngineMessage.Msg_CL_CompoundPartner();
                    protoData.m_PartnerId = partnerId;
                    msg.m_ProtoData = protoData;
                    SendMessage(msg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void HandleCompoundPartnerResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_CompoundPartnerResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_CompoundPartnerResult;
            if (null != protoData)
            {
                if ((int)PartnerMsgResultEnum.SUCCESS == protoData.m_ResultCode)
                {
                    // 增加伙伴 && 处理消耗
                    RoleInfo role = LobbyClient.Instance.CurrentRole;
                    if (null != role)
                    {
                        role.PartnerStateInfo.AddPartner(protoData.m_PartnerId, 1, 1);
                        PartnerConfig partnerConfig = PartnerConfigProvider.Instance.GetDataById(protoData.m_PartnerId);
                        if (null != partnerConfig)
                        {
                            role.ReduceItemData(partnerConfig.PartnerFragId, 0, partnerConfig.PartnerFragNum);
                        }
                    }
                    else
                    {
                        // 失败, 这里失败的话应该是客户端和服务器不同步导致的。
                    }
                }
                GfxSystem.PublishGfxEvent("ge_partner_compound_result", "ui", protoData.m_ResultCode);
            }
        }
    }
}
