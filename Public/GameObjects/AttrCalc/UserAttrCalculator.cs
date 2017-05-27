namespace ArkCrossEngine
{
    public sealed class UserAttrCalculator
    {
        public static void Calc(UserInfo user)
        {
            AttrCalculateUtility.ResetBaseProperty(user);
            AttrCalculateUtility.RefixAttrByEquipment(user);
            AttrCalculateUtility.RefixAttrByLegacy(user);
            AttrCalculateUtility.RefixAttrByXSoul(user);
            AttrCalculateUtility.RefixFightingScoreByPropertyWithOutPartner(user);
            AttrCalculateUtility.RefixAttrByPartner(user);
            AttrCalculateUtility.RefixFightingScoreByProperty(user);
            AttrCalculateUtility.RefixAttrByImpact(user);

            int hpMax = user.GetActualProperty().HpMax;
            user.GetActualProperty().SetHpMax(Operate_Type.OT_Absolute, (int)(user.HpMaxCoefficient * hpMax));
            int mpMax = user.GetActualProperty().EnergyMax;
            user.GetActualProperty().SetEnergyMax(Operate_Type.OT_Absolute, (int)(user.EnergyMaxCoefficient * mpMax));
        }
    }
}
