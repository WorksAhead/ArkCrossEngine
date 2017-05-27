namespace ArkCrossEngine
{
    /// <summary>
    /// common heal impact
    /// param0 : type; 0 = percent recover, 1 = absolute recover
    /// param1 : recover value
    /// </summary>
    class ImpactLogic_Heal : AbstractImpactLogic
    {
        private enum RecoverTypeEnum
        {
            PERCENT = 0,
            ABSOLUTE = 1,
        }
        public override void StartImpact(CharacterInfo obj, int impactId)
        {
            if (null != obj)
            {
                ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (null != impactInfo && impactInfo.ConfigData.ParamNum >= 2)
                {
                    int recoverType = int.Parse(impactInfo.ConfigData.ExtraParams[0]);
                    float recoverValue = float.Parse(impactInfo.ConfigData.ExtraParams[1]);
                    switch (recoverType)
                    {
                        case (int)RecoverTypeEnum.ABSOLUTE:
                            DoRecover(obj, impactInfo.m_ImpactSenderId, recoverValue);
                            break;
                        case (int)RecoverTypeEnum.PERCENT:
                            DoRecover(obj, impactInfo.m_ImpactSenderId, recoverValue * obj.GetActualProperty().HpMax);
                            break;
                    }
                }
            }
        }
        private void DoRecover(CharacterInfo obj, int senderId, float recoverValue)
        {
            if (obj.Hp + recoverValue > obj.GetActualProperty().HpMax)
            {
                recoverValue = obj.GetActualProperty().HpMax - obj.Hp;
            }
            obj.SetHp(Operate_Type.OT_Relative, (int)recoverValue);
            if (null != EventImpactLogicDamage)
            {
                EventImpactLogicDamage(obj, senderId, (int)recoverValue, false, false, true);
            }
        }
    }
}
