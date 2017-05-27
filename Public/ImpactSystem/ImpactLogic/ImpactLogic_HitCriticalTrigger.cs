using System;

namespace ArkCrossEngine
{
    /*
     * 攻击第N次必然爆击效果
     * param 0 : 第N次触发
     */
    class ImpactLogic_HitCriticalTrigger : AbstractImpactLogic
    {
        public override int RefixHpDamage(CharacterInfo obj, int impactId, int hpDamage, int senderId, ref bool isCritical, int impactOwnerId)
        {
            if (obj == null)
            {
                return hpDamage;
            }
            if (senderId != impactOwnerId)
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
            int criticalCount = 0;
            try
            {
                criticalCount = int.Parse(impactinfo.ConfigData.ExtraParams[0]);
                if (combat_info.MultiHitCount != 0 && combat_info.MultiHitCount % criticalCount == 0)
                {
                    if (!isCritical)
                    {
                        hpDamage = (int)(hpDamage * sender.GetActualProperty().CriticalPow);
                        isCritical = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("parse HitCriticalTrigger param error:\n" + ex.Message);
            }
            return hpDamage;
        }
    }
}
