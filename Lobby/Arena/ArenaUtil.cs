using System;
using System.Collections.Generic;
using ArkCrossEngine;

namespace Lobby
{
    internal class ArenaUtil
    {
        internal static ArkCrossEngineMessage.ChallengeInfoData CreateChallengeInfoData(ChallengeInfo info)
        {
            ArkCrossEngineMessage.ChallengeInfoData msg = new ArkCrossEngineMessage.ChallengeInfoData();
            msg.Challenger = CreateChallengeEntityData(info.Challenger);
            msg.Target = CreateChallengeEntityData(info.Target);
            msg.IsChallengeSuccess = info.IsChallengerSuccess;
            msg.EndTime = info.ChallengeEndTime.Ticks;
            return msg;
        }

        internal static ArkCrossEngineMessage.ChallengeEntityData CreateChallengeEntityData(ChallengeEntityInfo entity)
        {
            ArkCrossEngineMessage.ChallengeEntityData target = new ArkCrossEngineMessage.ChallengeEntityData();
            target.Guid = entity.Guid;
            target.Level = entity.Level;
            target.HeroId = entity.HeroId;
            target.NickName = entity.NickName;
            target.FightScore = entity.FightScore;
            target.Rank = entity.Rank;
            target.UserDamage = entity.UserDamage;
            target.PartnerDamage.Clear();
            for (int i = 0; i < entity.PartnerDamage.Count; i++)
            {
                ArkCrossEngineMessage.DamageInfoData damange_info = new ArkCrossEngineMessage.DamageInfoData();
                damange_info.OwnerId = entity.PartnerDamage[i].OwnerId;
                damange_info.Damage = entity.PartnerDamage[i].Damage;
                target.PartnerDamage.Add(damange_info);
            }
            return target;
        }

        internal static ArkCrossEngineMessage.ArenaInfoMsg CreateArenaInfoMsg(ArenaInfo entity, bool is_detail = true)
        {
            if (null == entity)
            {
                return null;
            }
            ArkCrossEngineMessage.ArenaInfoMsg info_msg = new ArkCrossEngineMessage.ArenaInfoMsg();
            info_msg.Guid = entity.GetId();
            info_msg.HeroId = entity.HeroId;
            info_msg.Level = entity.Level;
            info_msg.NickName = entity.NickName;
            info_msg.Rank = entity.GetRank();
            info_msg.FightScore = entity.FightScore;

            foreach (PartnerInfo partner in entity.FightPartners)
            {
                ArkCrossEngineMessage.PartnerDataMsg partner_msg = new ArkCrossEngineMessage.PartnerDataMsg();
                partner_msg.Id = partner.Id;
                partner_msg.AdditionLevel = partner.CurAdditionLevel;
                partner_msg.SkillStage = partner.CurSkillStage;
                info_msg.FightParters.Add(partner_msg);
            }
            if (!is_detail)
            {
                return info_msg;
            }
            if (entity.ActivePartner != null)
            {
                ArkCrossEngineMessage.PartnerDataMsg active_partner_msg = new ArkCrossEngineMessage.PartnerDataMsg();
                active_partner_msg.Id = entity.ActivePartner.Id;
                active_partner_msg.AdditionLevel = entity.ActivePartner.CurAdditionLevel;
                active_partner_msg.SkillStage = entity.ActivePartner.CurSkillStage;
                info_msg.ActivePartner = active_partner_msg;
            }

            foreach (ItemInfo item in entity.EquipInfo)
            {
                ArkCrossEngineMessage.ItemDataMsg equip = new ArkCrossEngineMessage.ItemDataMsg();
                equip.ItemId = item.ItemId;
                equip.Level = item.Level;
                equip.Num = item.ItemNum;
                info_msg.EquipInfo.Add(equip);
            }
            foreach (SkillDataInfo skill in entity.SkillDataInfo)
            {
                ArkCrossEngineMessage.SkillDataInfo skill_msg = new ArkCrossEngineMessage.SkillDataInfo();
                skill_msg.ID = skill.ID;
                skill_msg.Level = skill.Level;
                skill_msg.Postions = (int)skill.Postions.Presets[0];
                info_msg.ActiveSkills.Add(skill_msg);
            }
            foreach (ArenaItemInfo legacy in entity.LegacyInfo)
            {
                ArkCrossEngineMessage.LegacyDataMsg legacy_msg = new ArkCrossEngineMessage.LegacyDataMsg();
                legacy_msg.ItemId = legacy.ItemId;
                legacy_msg.Level = legacy.Level;
                legacy_msg.AppendProperty = legacy.AppendProperty;
                legacy_msg.IsUnlock = legacy.IsUnlocked;
                info_msg.LegacyAttr.Add(legacy_msg);
            }
            foreach (ArenaXSoulInfo xsoul in entity.XSoulInfo)
            {
                ArkCrossEngineMessage.XSoulDataMsg msg = new ArkCrossEngineMessage.XSoulDataMsg();
                msg.ItemId = xsoul.ItemId;
                msg.Level = xsoul.Level;
                msg.Experience = xsoul.Experience;
                msg.ModelLevel = xsoul.ModelLevel;
                info_msg.XSouls.Add(msg);

            }
            return info_msg;
        }

        internal static DamageInfo GetDamageInfo(List<DamageInfo> list, int targetid)
        {
            foreach (var d in list)
            {
                if (d.OwnerId == targetid)
                {
                    return d;
                }
            }
            return null;
        }
    }
}
