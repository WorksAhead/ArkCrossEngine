using System;

namespace ArkCrossEngine
{
    /*
     * 攻击第N次回复X属性
     * param 0 : 第几次
     * param 1 : 属性类型HP/MP
     * param 2 : 回复最大值的%
     * param 3 : 回复固定值
     */
    public class ImpactLogic_HitRecover : AbstractImpactLogic
    {
        public delegate void ImpactLogicHitRecoverDelegate(CharacterInfo entity, string attribute, int value);
        public static ImpactLogicHitRecoverDelegate EventImpactHitRecover;
        class RecoverInfo
        {
            public long LastRecoverTime;
            public int LastRecoverHitCount;
        }

        public override int RefixHpDamage(CharacterInfo obj, int impactId, int hpDamage, int senderId, ref bool isCritical, int impactOnwerId)
        {
            //LogSystem.Debug("---in hit recover");
            if (obj == null)
            {
                return hpDamage;
            }
            if (senderId != impactOnwerId)
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
            int triggerCount = 0;
            try
            {
                triggerCount = int.Parse(impactinfo.ConfigData.ExtraParams[0]);
                if (combat_info.MultiHitCount != 0 && combat_info.MultiHitCount % triggerCount == 0)
                {
                    Recover(sender, impactinfo, combat_info.MultiHitCount, combat_info.LastHitTime);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("parse HitRcover count param error:\n" + ex.Message);
            }
            return hpDamage;
        }

        private void Recover(CharacterInfo sender, ImpactInfo impactInfo, int hit_count, long hit_count_time)
        {
            if (impactInfo.ConfigData.ExtraParams.Count < 4)
            {
                return;
            }
            string add_attribute = "HP";
            float add_max_ratio = 0;
            int add_value = 0;
            try
            {
                add_attribute = impactInfo.ConfigData.ExtraParams[1];
                add_max_ratio = float.Parse(impactInfo.ConfigData.ExtraParams[2]);
                add_value = int.Parse(impactInfo.ConfigData.ExtraParams[3]);
            }
            catch (Exception ex)
            {
                LogSystem.Error("parse recover param error:\n" + ex.Message);
            }
            if (!IsNewHitRecoverCount(sender, impactInfo, hit_count, hit_count_time))
            {
                return;
            }
            if (add_attribute.Equals("HP"))
            {
                int sould_add = (int)(sender.GetActualProperty().HpMax * add_max_ratio) + add_value;
                int final_add = sould_add;
                if (sender.Hp + final_add > sender.GetActualProperty().HpMax)
                {
                    final_add = sender.GetActualProperty().HpMax - sender.Hp;
                }
                sender.SetHp(Operate_Type.OT_Relative, final_add);
                UpdateRecoverInfo(impactInfo, hit_count, hit_count_time);
                if (EventImpactHitRecover != null)
                {
                    EventImpactHitRecover(sender, "HP", sould_add);
                }
                //LogSystem.Debug("----hit recover set hp {0}/{1}", sender.Hp, sender.GetActualProperty().HpMax);
            }
            else if (add_attribute.Equals("MP"))
            {
                int final_add = (int)(sender.GetActualProperty().EnergyMax * add_max_ratio) + add_value;
                sender.SetEnergy(Operate_Type.OT_Relative, final_add);
                UpdateRecoverInfo(impactInfo, hit_count, hit_count_time);
                if (EventImpactHitRecover != null)
                {
                    EventImpactHitRecover(sender, "MP", final_add);
                }
            }
        }

        private void UpdateRecoverInfo(ImpactInfo impactInfo, int hit_count, long hit_count_time)
        {
            RecoverInfo info = impactInfo.LogicDatas.GetData<RecoverInfo>();
            if (info == null)
            {
                info = new RecoverInfo();
                impactInfo.LogicDatas.AddData<RecoverInfo>(info);
            }
            info.LastRecoverTime = hit_count_time;
            info.LastRecoverHitCount = hit_count;
        }

        private bool IsNewHitRecoverCount(CharacterInfo sender, ImpactInfo impactInfo, int hit_count, long hit_count_time)
        {
            RecoverInfo info = impactInfo.LogicDatas.GetData<RecoverInfo>();
            if (info == null)
            {
                return true;
            }
            if (info.LastRecoverHitCount == hit_count && info.LastRecoverTime == hit_count_time)
            {
                return false;
            }
            return true;
        }
    }
}
