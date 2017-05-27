using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class KillAddDamage : TalentAttribute
    {
        public int KillCount;
        public int DamageAdd;
        public int ImpactId;
        public int LevelAddDamage;

        public bool IsTriggered = false;
        public long KillHitCountId = -1;
        private bool IsAddDamageHitRecord = false;
        private long AddDamageHitCountId = -1;

        public override AttributeId GetId() { return AttributeId.kKillAddDamage; }

        public override void Init(List<string> attri_list, List<string> level_add)
        {
            if (attri_list.Count >= 3)
            {
                KillCount = int.Parse(attri_list[0]);
                DamageAdd = int.Parse(attri_list[1]);
                ImpactId = int.Parse(attri_list[2]);
            }
            if (level_add.Count >= 2)
            {
                LevelAddDamage = int.Parse(level_add[1]);
            }
            //LogSystem.Error("----talent attribute init: {0} kill count {1} damange add {2}", GetId(), KillCount, DamageAdd);
        }

        public override void UpdateToLevel(int level)
        {
            //LogSystem.Error("----talent attribute {0} update to level {1}", GetId(), level);
            DamageAdd = DamageAdd - (m_Level - 1) * LevelAddDamage;
            m_Level = level;
            DamageAdd = DamageAdd + (m_Level - 1) * LevelAddDamage;
        }

        public bool IsTriggerHit(long hit_count_id)
        {
            return hit_count_id == KillHitCountId;
        }

        public bool IsDamageHit(long hit_count_id)
        {
            if (!IsTriggered)
            {
                return false;
            }
            if (IsTriggerHit(hit_count_id))
            {
                return false;
            }
            if (!IsAddDamageHitRecord)
            {
                AddDamageHitCountId = hit_count_id;
                IsAddDamageHitRecord = true;
                return true;
            }
            else
            {
                if (hit_count_id == AddDamageHitCountId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Refresh()
        {
            IsTriggered = false;
            KillHitCountId = -1;
            IsAddDamageHitRecord = false;
            AddDamageHitCountId = -1;
        }
    }
}
