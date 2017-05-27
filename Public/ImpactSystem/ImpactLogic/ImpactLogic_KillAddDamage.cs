namespace ArkCrossEngine
{
    /*
     * 杀死N个单位附加伤害
     */
    public class ImpactLogic_KillAddDamage : AbstractImpactLogic
    {
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
            CharacterInfo sender = obj.SceneContext.GetCharacterInfoById(senderId);
            if (sender == null)
            {
                return hpDamage;
            }
            CombatStatisticInfo combat_info = sender.GetCombatStatisticInfo();
            if (combat_info == null)
            {
                return hpDamage;
            }
            if (sender.TalentManager == null)
            {
                return hpDamage;
            }
            KillAddDamage kill_attr = sender.TalentManager.GetTalentAttribute(AttributeId.kKillAddDamage) as KillAddDamage;
            if (kill_attr == null || !kill_attr.IsActive)
            {
                return hpDamage;
            }
            //LogSystem.Error("-----KillAddDamange: trigger hit {0} add damage hit {1}", kill_attr.KillHitCountId, combat_info.LastHitCountId);
            if (kill_attr.IsDamageHit(combat_info.LastHitCountId))
            {
                hpDamage += kill_attr.DamageAdd;
                //LogSystem.Error("-----KillAddDamange: add damage {0}!", kill_attr.DamageAdd);
            }
            else if (!kill_attr.IsTriggerHit(combat_info.LastHitCountId))
            {
                //LogSystem.Error("-----KillAddDamange: stopped! 1");
                kill_attr.Refresh();
                ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (impactInfo != null)
                {
                    impactInfo.m_IsActivated = false;
                }
            }
            return hpDamage;
        }

        public override void StopImpact(CharacterInfo obj, int impactId)
        {
            ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
            if (impactInfo == null)
            {
                return;
            }
            CharacterInfo sender = obj.SceneContext.GetCharacterInfoById(impactInfo.m_ImpactSenderId);
            if (sender != null)
            {
                KillAddDamage kill_attr = sender.TalentManager.GetTalentAttribute(AttributeId.kKillAddDamage) as KillAddDamage;
                //LogSystem.Error("-----KillAddDamange: stopped! 1");
                if (kill_attr != null)
                {
                    kill_attr.Refresh();
                }
            }
            impactInfo.m_IsActivated = false;
        }
    }
}
