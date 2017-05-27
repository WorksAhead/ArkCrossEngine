using System;
using System.Collections.Generic;
using ArkCrossEngine.Network;

namespace ArkCrossEngine
{
    public partial class WorldSystem
    {
        private void TickArenaLogic()
        {
            if (!IsPvapScene())
            {
                return;
            }
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            if (roleself == null)
            {
                return;
            }
            ArenaStateInfo arena_state = roleself.ArenaStateInfo;
            if (arena_state.IsBattleStarted && !arena_state.IsEntityCreated)
            {
                roleself.ArenaStateInfo.IsEntityCreated = true;
                CreatePlayerSelfParters();
                arena_state.TargetInfo = CreateArenaTarget(arena_state.ChallengeTarget);
            }
            if (!arena_state.IsChallengeOver && arena_state.IsBeginFight)
            {
                m_AiSystem.Tick();
            }
            ClientStorySystem.Instance.Tick();
            CameraChangeCheck(arena_state, roleself);
            OverCheck(arena_state);
            TimeOutCheck(arena_state);
            if (arena_state.IsChallengeOver && arena_state.IsBeginFight)
            {
                arena_state.CheckChallengeResult();
            }
        }

        private void TimeOutCheck(ArenaStateInfo state)
        {
            if (state.IsChallengeOver || !state.IsBeginFight)
            {
                return;
            }
            if (DateTime.Now >= state.EndFightLocalTime)
            {
                state.IsChallengeOver = true;
                state.IsChallengeSuccess = false;
                ClientStorySystem.Instance.SendMessage("timeout");
            }
        }

        private void CameraChangeCheck(ArenaStateInfo state, RoleInfo self)
        {
            if (state.IsChallengeOver)
            {
                return;
            }
            if (self.GetPlayerSelfInfo().IsDead() && state.CurCameraTargetPartner == -1)
            {
                ChangeCameraToPartner(state);
            }
            if (state.CurCameraTargetPartner >= 0)
            {
                CharacterInfo npc = WorldSystem.Instance.GetCharacterById(state.CurCameraTargetPartner);
                if (npc != null && npc.IsDead())
                {
                    ChangeCameraToPartner(state);
                }
            }
        }

        private void ChangeCameraToPartner(ArenaStateInfo state)
        {
            NpcInfo not_dead_partner = FindNotdeadPartner(state.CreatedPartners);
            if (not_dead_partner != null)
            {
                NpcView view = EntityManager.Instance.GetNpcViewById(not_dead_partner.GetId());
                if (view != null)
                {
                    GfxSystem.SendMessage("GfxGameRoot", "CameraFollow", view.Actor);
                    state.CurCameraTargetPartner = not_dead_partner.GetId();
                }
            }
        }

        private NpcInfo FindNotdeadPartner(Dictionary<int, NpcInfo> partners)
        {
            foreach (NpcInfo npc in partners.Values)
            {
                if (!npc.IsDead())
                {
                    return npc;
                }
            }
            return null;
        }

        internal float CalcPvpCoefficient(float lvl1, float lvl2)
        {
            float lvl = (lvl1 + lvl2) / 2;
            double c = 4.09 * 1.2 * 1.3 * (1 + lvl * 0.04) * (1 + (0.15 * (1.62 - 1) / 50 * lvl) +
                       (1.05 + 0.55 / 50 * lvl - 1) * 0.5 + (1.05 + 0.55 / 50 * lvl - 1) * 0.5);
            return (float)c;
        }

        private void SetArenaCharacterCoefficient(CharacterInfo character)
        {
            ArenaStateInfo state = LobbyClient.Instance.CurrentRole.ArenaStateInfo;
            if (GetPlayerSelf() == null || state == null || state.ChallengeTarget == null)
            {
                return;
            }
            int level_self = GetPlayerSelf().GetLevel();
            int level_target = state.ChallengeTarget.Level;
            character.HpMaxCoefficient = CalcPvpCoefficient(level_self, level_target);
            character.EnergyMaxCoefficient = character.HpMaxCoefficient;
        }

        private void OverCheck(ArenaStateInfo state)
        {
            if (!state.IsBattleStarted || !state.IsEntityCreated)
            {
                return;
            }
            if (state.IsChallengeOver)
            {
                return;
            }
            bool isScuccess = false;
            if (IsPlayerAndPartnerAllDead(GetPlayerSelf(), state.CreatedPartners))
            {
                isScuccess = false;
                state.IsChallengeOver = true;
            }
            else if (IsPlayerAndPartnerAllDead(state.TargetInfo, state.ChallengeTarget.CreatedPartners))
            {
                isScuccess = true;
                state.IsChallengeOver = true;
            }
            if (state.IsChallengeOver)
            {
                state.IsChallengeSuccess = isScuccess;
                ClientStorySystem.Instance.SendMessage("onenemykilled");
                //LobbyNetworkSystem.Instance.OnChallengeOver(isScuccess);
            }
        }

        private bool IsPlayerAndPartnerAllDead(UserInfo player, Dictionary<int, NpcInfo> partners)
        {
            if (player != null && player.Hp > 0)
            {
                return false;
            }
            /*foreach (NpcInfo partner in partners.Values) {
              if (partner != null && !partner.IsDead()) {
                return false;
              }
            }*/
            return true;
        }

        private UserInfo CreateArenaTarget(ArenaTargetInfo target)
        {
            if (target == null)
            {
                return null;
            }
            int image_res_id = target.HeroId;
            UserInfo image_player = m_UserMgr.AddUser(image_res_id);
            if (null != image_player)
            {
                image_player.SceneContext = m_SceneContext;
                int campid = NetworkSystem.Instance.CampId == (int)CampIdEnum.Blue ? (int)CampIdEnum.Red : (int)CampIdEnum.Blue;
                image_player.SetCampId(campid);
                Data_Unit unit = m_CurScene.StaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(campid)) as Data_Unit;
                if (null != unit)
                {
                    image_player.GetAiStateInfo().AiLogic = unit.m_AiLogic;
                    image_player.SetAIEnable(true);
                    image_player.GetMovementStateInfo().SetPosition(unit.m_Pos);
                    image_player.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
                }
                image_player.SetLevel(target.Level);
                image_player.SetNickName(target.Nickname);
                image_player.SkillController = new SwordManSkillController(image_player, GfxModule.Skill.GfxSkillSystem.Instance);
                if (null != m_SpatialSystem)
                {
                    m_SpatialSystem.AddObj(image_player.SpaceObject);
                }
                /// skills
                if (null != target.Skills)
                {
                    image_player.GetSkillStateInfo().RemoveAllSkill();
                    int skill_ct = target.Skills.Count;
                    for (int i = 0; i < skill_ct; i++)
                    {
                        SkillInfo info = target.Skills[i];
                        if (null != info)
                        {
                            SkillCategory cur_skill_pos = SkillCategory.kNone;
                            if (info.Postions.Presets[0] == SlotPosition.SP_A)
                            {
                                cur_skill_pos = SkillCategory.kSkillA;
                            }
                            else if (info.Postions.Presets[0] == SlotPosition.SP_B)
                            {
                                cur_skill_pos = SkillCategory.kSkillB;
                            }
                            else if (info.Postions.Presets[0] == SlotPosition.SP_C)
                            {
                                cur_skill_pos = SkillCategory.kSkillC;
                            }
                            else if (info.Postions.Presets[0] == SlotPosition.SP_D)
                            {
                                cur_skill_pos = SkillCategory.kSkillD;
                            }
                            info.Category = cur_skill_pos;
                            image_player.GetSkillStateInfo().AddSkill(info);
                            WorldSystem.Instance.AddSubSkill(image_player, info.SkillId, cur_skill_pos, info.SkillLevel);
                        }
                    }
                }
                Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(image_player.GetLinkId());
                if (null != playerData && null != playerData.m_FixedSkillList
                  && playerData.m_FixedSkillList.Count > 0)
                {
                    foreach (int skill_id in playerData.m_FixedSkillList)
                    {
                        SkillInfo info = new SkillInfo(skill_id, 1);
                        image_player.GetSkillStateInfo().AddSkill(info);
                    }
                }
                image_player.ResetSkill();
                /// equips
                if (null != target.Equips)
                {
                    int equip_ct = target.Equips.Length;
                    for (int i = 0; i < equip_ct; i++)
                    {
                        ItemDataInfo image_equip = target.Equips[i];
                        if (null != image_equip)
                        {
                            image_player.GetEquipmentStateInfo().ResetEquipmentData(i);
                            image_equip.ItemConfig = ItemConfigProvider.Instance.GetDataById(image_equip.ItemId);
                            if (null != image_equip.ItemConfig)
                            {
                                image_player.GetEquipmentStateInfo().SetEquipmentData(i, image_equip);
                            }
                        }
                    }
                }
                /// 
                if (null != target.Legacys)
                {
                    int legacy_ct = target.Legacys.Length;
                    for (int i = 0; i < legacy_ct; i++)
                    {
                        ItemDataInfo image_legacy = target.Legacys[i];
                        if (null != image_legacy && image_legacy.IsUnlock)
                        {
                            image_player.GetLegacyStateInfo().ResetLegacyData(i);
                            image_legacy.ItemConfig = ItemConfigProvider.Instance.GetDataById(image_legacy.ItemId);
                            if (null != image_legacy.ItemConfig)
                            {
                                image_player.GetLegacyStateInfo().SetLegacyData(i, image_legacy);
                            }
                        }
                    }
                    image_player.GetLegacyStateInfo().UpdateLegacyComplexAttr();
                }
                foreach (ArenaXSoulInfo x in target.XSoulInfo)
                {
                    ItemDataInfo item = new ItemDataInfo();
                    item.ItemId = x.ItemId;
                    item.Level = x.Level;
                    item.Experience = x.Experience;
                    item.UpdateLevelByExperience();
                    ItemConfig config = ItemConfigProvider.Instance.GetDataById(item.ItemId);
                    if (config == null)
                    {
                        continue;
                    }
                    item.ItemConfig = config;
                    XSoulPartInfo part = new XSoulPartInfo((XSoulPart)config.m_WearParts, item);
                    part.ShowModelLevel = x.ModelLevel;
                    image_player.GetXSoulInfo().SetXSoulPartData((XSoulPart)config.m_WearParts, part);
                }
                image_player.SetPartnerInfo(target.ActivePartner);
                SetArenaCharacterCoefficient(image_player);
                UserAttrCalculator.Calc(image_player);
                image_player.SetHp(Operate_Type.OT_Absolute, image_player.GetActualProperty().HpMax);
                image_player.SetRage(Operate_Type.OT_Absolute, 0);
                image_player.SetEnergy(Operate_Type.OT_Absolute, image_player.GetActualProperty().EnergyMax);

                target.CreatedPartners.Clear();
                int total_hp = image_player.GetActualProperty().HpMax;
                //LogSystem.Error("---challenge target hp=" + image_player.GetActualProperty().HpMax);
                foreach (PartnerInfo partner in target.FightPartners)
                {
                    NpcInfo partner_npc = CreateParterner(image_player, partner);
                    if (partner_npc != null)
                    {
                        target.CreatedPartners.Add(partner.Id, partner_npc);
                        //LogSystem.Error("---parnter {0} hp={1}", partner.Id, partner_npc.GetBaseProperty().HpMax);
                        total_hp += partner_npc.GetBaseProperty().HpMax;
                    }
                }
                //LogSystem.Error("-----challenge target total hp=" + total_hp);
                EntityManager.Instance.CreateUserView(image_player.GetId());
                UserView image_view = EntityManager.Instance.GetUserViewById(image_player.GetId());
                image_view.UpdateEquipment();
                image_view.UpdateXSoulEquip();
            }
            SyncGfxUserInfo(image_player.GetId());
            return image_player;
        }

        private void CreatePlayerSelfParters()
        {
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            if (null == roleself) return;
            roleself.PartnerStateInfo.ActivePartnerHpPercent = 1.0f;
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            roleself.ArenaStateInfo.CreatedPartners.Clear();
            foreach (int partnerid in roleself.ArenaStateInfo.FightPartners)
            {
                PartnerInfo partner = roleself.PartnerStateInfo.GetPartnerInfoById(partnerid);
                NpcInfo partner_npc = CreateParterner(playerself, partner);
                if (partner_npc != null)
                {
                    roleself.ArenaStateInfo.CreatedPartners.Add(partner.Id, partner_npc);
                }
            }
            SyncGfxUserInfo(playerself.GetId());
        }

        private NpcInfo CreateParterner(UserInfo owner, PartnerInfo partnerInfo)
        {
            if (null == owner)
            {
                return null;
            }
            if (null == partnerInfo)
            {
                return null;
            }
            Data_Unit data = new Data_Unit();
            data.m_Id = -1;
            data.m_LinkId = partnerInfo.LinkId;
            data.m_CampId = owner.GetCampId();
            data.m_Pos = owner.GetMovementStateInfo().GetPosition3D();
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
                    npc.GetBaseProperty().SetAttackBase(Operate_Type.OT_Absolute, (int)(owner.GetActualProperty().AttackBase * inheritAttackAttrPercent));
                    npc.GetBaseProperty().SetFireDamage(Operate_Type.OT_Absolute, owner.GetActualProperty().FireDamage * inheritAttackAttrPercent);
                    npc.GetBaseProperty().SetIceDamage(Operate_Type.OT_Absolute, owner.GetActualProperty().IceDamage * inheritAttackAttrPercent);
                    npc.GetBaseProperty().SetPoisonDamage(Operate_Type.OT_Absolute, owner.GetActualProperty().PoisonDamage * inheritAttackAttrPercent);
                    // defence
                    npc.GetBaseProperty().SetHpMax(Operate_Type.OT_Absolute, (int)(owner.GetActualProperty().HpMax * inheritDefenceAttrPercent));
                    npc.GetBaseProperty().SetEnergyMax(Operate_Type.OT_Absolute, (int)(owner.GetActualProperty().EnergyMax * inheritDefenceAttrPercent));
                    npc.GetBaseProperty().SetADefenceBase(Operate_Type.OT_Absolute, (int)(owner.GetActualProperty().ADefenceBase * inheritDefenceAttrPercent));
                    npc.GetBaseProperty().SetMDefenceBase(Operate_Type.OT_Absolute, (int)(owner.GetActualProperty().MDefenceBase * inheritDefenceAttrPercent));
                    npc.GetBaseProperty().SetFireERD(Operate_Type.OT_Absolute, owner.GetActualProperty().FireERD * inheritDefenceAttrPercent);
                    npc.GetBaseProperty().SetIceERD(Operate_Type.OT_Absolute, owner.GetActualProperty().IceERD * inheritDefenceAttrPercent);
                    npc.GetBaseProperty().SetPoisonERD(Operate_Type.OT_Absolute, owner.GetActualProperty().PoisonERD * inheritDefenceAttrPercent);
                    // reset hp & energy
                    npc.SetHp(Operate_Type.OT_Absolute, npc.GetBaseProperty().HpMax);
                    npc.SetEnergy(Operate_Type.OT_Absolute, npc.GetBaseProperty().EnergyMax);
                }
                npc.SetAIEnable(true);
                npc.IsArenaPartner = true;
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
                owner.LastSummonPartnerTime = TimeUtility.GetServerMilliseconds();
                npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                npc.OwnerId = owner.GetId();
                owner.PartnerId = npc.GetId();
                npc.SetLevel(owner.GetLevel());
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
            }
            return npc;
        }
    } // end class
}
