namespace ArkCrossEngine
{
    /*
     * 冰甲buff
     * param 0 : 减速buff id
     */
    class ImpactLogic_IceArmor : AbstractImpactLogic
    {
        public override void OnAddImpact(CharacterInfo obj, int impactId, int addImpactId)
        {
            ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
            if (null != impactInfo)
            {
                ImpactInfo addImpactInfo = obj.GetSkillStateInfo().GetImpactInfoById(addImpactId);
                if (null != addImpactInfo)
                {
                    if (addImpactInfo.m_ImpactSenderId == obj.GetId()) return;
                    if (impactInfo.ConfigData.ParamNum >= 1)
                    {
                        int slowImpactId = int.Parse(impactInfo.ConfigData.ExtraParams[0]);
                        ImpactSystem.Instance.SendImpactToCharacter(obj,
                                                                    slowImpactId,
                                                                    addImpactInfo.m_ImpactSenderId,
                                                                    -1,
                                                                    -1,
                                                                    obj.GetMovementStateInfo().GetPosition3D(),
                                                                    obj.GetMovementStateInfo().GetFaceDir());
                    }
                }
            }
        }
    }
}
