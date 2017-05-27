namespace ArkCrossEngine
{
    class ImpactLogic_General : AbstractImpactLogic
    {
        public override void StartImpact(CharacterInfo obj, int impactId)
        {
            if (null != obj)
            {
                ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (null != impactInfo)
                {
                    if (impactInfo.m_IsActivated)
                    {
                        int curdamage = 0;
                        float damageDelayTime = float.Parse(impactInfo.ConfigData.ExtraParams[0]);
                        if (damageDelayTime < 0.01f)
                        {
                            if (!obj.IsHaveStateFlag(CharacterState_Type.CST_Invincible))
                            {
                                ApplyDamage(obj, impactId, out curdamage);
                            }
                            ApplyRage(obj, impactInfo, curdamage);
                            impactInfo.m_HasEffectApplyed = true;
                        }
                    }
                }
            }
            base.StartImpact(obj, impactId);
        }
        public override void Tick(CharacterInfo character, int impactId)
        {
            ImpactInfo impactInfo = character.GetSkillStateInfo().GetImpactInfoById(impactId);
            if (null != impactInfo)
            {
                if (impactInfo.m_IsActivated)
                {
                    float damageDelayTime = float.Parse(impactInfo.ConfigData.ExtraParams[0]);
                    if (damageDelayTime > 0.01f && TimeUtility.GetServerMilliseconds() > impactInfo.m_StartTime + damageDelayTime * 1000 && !impactInfo.m_HasEffectApplyed)
                    {
                        int curdamage = 0;
                        int damage = int.Parse(impactInfo.ConfigData.ExtraParams[1]);
                        if (!character.IsHaveStateFlag(CharacterState_Type.CST_Invincible))
                        {
                            ApplyDamage(character, impactId, out curdamage);
                        }
                        impactInfo.m_HasEffectApplyed = true;
                        ApplyRage(character, impactInfo, curdamage);
                    }

                    if (TimeUtility.GetServerMilliseconds() > impactInfo.m_StartTime + impactInfo.m_ImpactDuration)
                    {
                        impactInfo.m_IsActivated = false;
                    }
                }
            }
        }
        private void ApplyRage(CharacterInfo target, ImpactInfo impactInfo, int damage)
        {
            if (impactInfo.ConfigData.ExtraParams.Count >= 3)
            {
                int rage = int.Parse(impactInfo.ConfigData.ExtraParams[2]);
                if (target.IsUser)
                {
                    target.SetRage(Operate_Type.OT_Relative, rage);
                    Data_PlayerConfig dpc = PlayerConfigProvider.Instance.GetPlayerConfigById(target.GetLinkId());
                    CharacterProperty cp = target.GetActualProperty();
                    if (dpc != null && cp != null)
                    {
                        target.SetRage(Operate_Type.OT_Relative, (int)(damage * dpc.m_DamageRagePercent * cp.RageMax / cp.HpMax));
                    }
                    if (null != EventImpactLogicRage)
                    {
                        EventImpactLogicRage(target, target.Rage);
                    }
                }
                else
                {
                    CharacterInfo user = target.SceneContext.GetCharacterInfoById(impactInfo.m_ImpactSenderId);
                    if (user != null && user.IsUser)
                    {
                        user.SetRage(Operate_Type.OT_Relative, rage);
                        if (null != EventImpactLogicRage)
                        {
                            EventImpactLogicRage(user, user.Rage);
                        }
                    }
                }
            }
        }
    }
}
