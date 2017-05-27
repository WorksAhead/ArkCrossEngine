namespace ArkCrossEngine
{
    class ImpactLogic_RefixDamage : AbstractImpactLogic
    {
        public override int RefixHpDamage(CharacterInfo obj, int impactId, int hpDamage, int senderId, ref bool isCritical, int impactOwnerId)
        {
            if (null != obj)
            {
                ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (null != impactInfo)
                {
                    if (impactInfo.ConfigData.ExtraParams.Count > 0)
                    {
                        float damageRate = float.Parse(impactInfo.ConfigData.ExtraParams[0]);
                        hpDamage = (int)(hpDamage * damageRate);
                    }
                }
            }
            return hpDamage;
        }
    }
}
