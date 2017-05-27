namespace ArkCrossEngine
{
    class ImpactLogic_Undead : AbstractImpactLogic
    {
        public override void StartImpact(CharacterInfo obj, int impactId)
        {
            if (null != obj)
            {
                obj.CanDead = false;
            }
            base.StartImpact(obj, impactId);
        }

        public override void Tick(CharacterInfo obj, int impactId)
        {
            ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
            if (null != impactInfo)
            {
                if (impactInfo.m_IsActivated)
                {
                    if (obj.Hp <= 0)
                    {
                        obj.SetHp(Operate_Type.OT_Absolute, 1);
                    }
                }
            }
        }
        public override void OnInterrupted(CharacterInfo obj, int impactId)
        {
            if (null != obj)
            {
                obj.CanDead = true;
            }
            base.OnInterrupted(obj, impactId);
        }
        public override bool CanInterrupt(CharacterInfo obj, int impactId)
        {
            return false;
        }
    }
}
