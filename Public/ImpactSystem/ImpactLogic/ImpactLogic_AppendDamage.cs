using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    /*
     * 攻击第N次附加伤害
     * param 0 : 第N次触发
     * param 1 : 附加ImpactID
     * param 2 : 特效
     * param 3 : 挂点
     */
    public class ImpactLogic_AppendDamage : AbstractImpactLogic
    {
        public delegate void ImpactLogicAppendImpactDelegate(CharacterInfo sender, int impactid, int senderid, Vector3 pos, float dir);
        public static ImpactLogicAppendImpactDelegate EventImpactAppendImpact;
        public override int RefixHpDamage(CharacterInfo obj, int impactId, int hpDamage, int senderId, ref bool isCritical, int impactOwnerId)
        {
            if (obj == null)
            {
                return hpDamage;
            }
            if (impactOwnerId != senderId)
            {
                return hpDamage;
            }
            UserInfo sender = obj.SceneContext.GetCharacterInfoById(senderId) as UserInfo;
            if (sender == null)
            {
                return hpDamage;
            }
            CombatStatisticInfo combat_info = sender.GetCombatStatisticInfo();
            if (combat_info == null)
            {
                return hpDamage;
            }
            ImpactInfo impactinfo = sender.GetSkillStateInfo().GetImpactInfoById(impactId);
            if (impactinfo == null || impactinfo.ConfigData.ExtraParams.Count < 1)
            {
                return hpDamage;
            }
            int triggerCount = 0;
            string effect = "";
            string mountPoint = "";
            try
            {
                if (impactinfo.ConfigData.ExtraParams.Count > 4)
                {
                    triggerCount = int.Parse(impactinfo.ConfigData.ExtraParams[0]);
                    effect = impactinfo.ConfigData.ExtraParams[2];
                    mountPoint = impactinfo.ConfigData.ExtraParams[3];
                    if (combat_info.MultiHitCount != 0 && combat_info.MultiHitCount % triggerCount == 0)
                    {
                        AppendDamage(sender, obj, impactinfo, hpDamage);
                        AppendImpact(sender, obj, impactinfo);
                    }
                    if (!String.IsNullOrEmpty(effect) && !String.IsNullOrEmpty(mountPoint))
                    {
                        EventImpactLogicEffect(obj, effect, mountPoint, 2.0f);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("parse append damage param error:\n" + ex.Message + "\n" + ex.StackTrace);
            }
            return hpDamage;
        }

        private void AppendDamage(CharacterInfo sender, CharacterInfo receiver, ImpactInfo impactInfo, int parentDamage)
        {
            if (sender == null || receiver == null || impactInfo == null)
            {
                return;
            }
            int skillLevel = 0;
            bool isCritical = false;
            bool isOrdinary = false;
            SkillInfo skillInfo = sender.GetSkillStateInfo().GetSkillInfoById(impactInfo.m_SkillId);
            if (null != skillInfo)
            {
                skillLevel = skillInfo.SkillLevel;
                if (skillInfo.Category == SkillCategory.kAttack)
                {
                    isOrdinary = true;
                }
            }
            ElementDamageType element_type = (ElementDamageType)impactInfo.ConfigData.ElementType;
            if (ElementDamageType.DC_None == element_type)
            {
                element_type = sender.GetEquipmentStateInfo().WeaponDamageType;
            }
            int appendDamage = (int)(impactInfo.ConfigData.DamageRate * parentDamage + impactInfo.ConfigData.DamageValue);
            if (appendDamage <= 0)
            {
                return;
            }
            int curDamage = DamageCalculator.CalcImpactDamage(sender, receiver,
                                                               (SkillDamageType)impactInfo.ConfigData.DamageType,
                                                               element_type,
                                                               0,
                                                               appendDamage,
                                                               out isCritical);
            List<ImpactInfo> impactInfos = receiver.GetSkillStateInfo().GetAllImpact();
            for (int i = 0; i < impactInfos.Count; i++)
            {
                IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(impactInfos[i].ConfigData.ImpactLogicId);
                if (null != logic)
                {
                    curDamage = logic.RefixHpDamage(receiver, impactInfos[i].m_ImpactId, curDamage, sender.GetId(), ref isCritical, receiver.GetId());
                }
            }
            /*
            foreach (ImpactInfo ii in receiver.GetSkillStateInfo().GetAllImpact()) {
              IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(ii.ConfigData.ImpactLogicId);
              if (null != logic) {
                curDamage = logic.RefixHpDamage(receiver, ii.m_ImpactId, curDamage, sender.GetId(), ref isCritical, receiver.GetId());
              }
            }*/
            OnCharacterDamage(sender, receiver, curDamage, isCritical, isOrdinary);
        }

        private void AppendImpact(CharacterInfo sender, CharacterInfo target, ImpactInfo impactInfo)
        {
            if (impactInfo.ConfigData.ExtraParams.Count < 2)
            {
                return;
            }
            int impactId = -1;
            try
            {
                impactId = int.Parse(impactInfo.ConfigData.ExtraParams[1]);
            }
            catch (Exception ex)
            {
                LogSystem.Error("parse append impact param error:\n" + ex.Message);
            }
            if (impactId > 0)
            {
                ImpactLogicData impactLogicData = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_IMPACT, impactId) as ImpactLogicData;
                if (impactLogicData != null)
                {
                    impactLogicData.IsIgnorePassiveCheck = true;
                    ImpactSystem.Instance.SendImpactToCharacter(sender, impactId, target.GetId(), -1, -1,
                                                                sender.GetMovementStateInfo().GetPosition3D(),
                                                                sender.GetMovementStateInfo().GetFaceDir());
                    if (EventImpactAppendImpact != null)
                    {
                        EventImpactAppendImpact(target, impactId, sender.GetId(), sender.GetMovementStateInfo().GetPosition3D(),
                                                sender.GetMovementStateInfo().GetFaceDir());
                    }
                }
            }
        }
    }
}
