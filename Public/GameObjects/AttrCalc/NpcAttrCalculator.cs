namespace ArkCrossEngine
{
    public sealed class NpcAttrCalculator
    {
        public static void Calc(NpcInfo npc)
        {
            AttrCalculateUtility.ResetBaseProperty(npc);
            AttrCalculateUtility.RefixAttrByEquipment(npc);
            AttrCalculateUtility.RefixAttrByImpact(npc);

            int hpMax = npc.GetActualProperty().HpMax;
            npc.GetActualProperty().SetHpMax(Operate_Type.OT_Absolute, (int)(npc.HpMaxCoefficient * hpMax));
        }
    }
}
