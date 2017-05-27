namespace ArkCrossEngine
{
    /*
     * 荆棘buff
     */
    class ImpactLogic_Thorns : AbstractImpactLogic
    {
        public override int RefixHpDamage(CharacterInfo obj, int impactId, int hpDamage, int senderId, ref bool isCritical, int impactOwnerId)
        {
            if (null != obj)
            {
                ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (null != impactInfo)
                {
                    CharacterInfo sender = obj.SceneContext.GetCharacterInfoById(senderId);
                    if (null != sender)
                    {
                        float percent = 1.0f;
                        if (impactInfo.ConfigData.ExtraParams.Count >= 1)
                        {
                            percent = float.Parse(impactInfo.ConfigData.ExtraParams[0]);
                        }
                        // apply damage from target to sender
                        ApplyDamage(sender, obj, (int)(hpDamage * percent));
                    }
                }
            }
            return hpDamage;
        }
    }
}

