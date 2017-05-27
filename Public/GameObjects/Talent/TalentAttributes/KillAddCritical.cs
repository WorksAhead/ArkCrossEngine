using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class KillAddCritical : TalentAttribute
    {
        public int KillCount;
        public float CriticalAdd;
        public long RemainTimeMS = 0;
        public float LevelAddCritical;

        public bool IsTriggered = false;
        public long TriggerHitCountId = -1;
        public long TriggerTime = -1;

        private bool IsAddCriticalHitRecord = false;
        private long AddCriticalHitCountId = -1;

        public override AttributeId GetId() { return AttributeId.kKillAddCritical; }

        public override void Init(List<string> attri_list, List<string> level_add)
        {
            if (attri_list.Count >= 3)
            {
                KillCount = int.Parse(attri_list[0]);
                CriticalAdd = float.Parse(attri_list[1]);
                RemainTimeMS = long.Parse(attri_list[2]);
            }
            if (level_add.Count >= 2)
            {
                LevelAddCritical = float.Parse(level_add[1]);
            }
            //LogSystem.Error("----talent attribute init: {0} kill count {1} critical add {2}", GetId(), KillCount, CriticalAdd);
        }

        public override void UpdateToLevel(int level)
        {
            //LogSystem.Error("----talent attribute {0} update to level {1}", GetId(), level);
            CriticalAdd = CriticalAdd - (m_Level - 1) * LevelAddCritical;
            m_Level = level;
            CriticalAdd = CriticalAdd + (m_Level - 1) * LevelAddCritical;
        }

        public bool OnKill(long hit_count_id)
        {
            if (CanTrigger(hit_count_id))
            {
                IsTriggered = true;
                TriggerHitCountId = hit_count_id;
                TriggerTime = TimeUtility.GetLocalMilliseconds();
                return true;
            }
            return false;
        }

        public void OnHit(long hit_count_id)
        {
            if (IsTriggered && TriggerTime + RemainTimeMS < TimeUtility.GetLocalMilliseconds())
            {
                Refresh();
                return;
            }
            if (IsTriggered && hit_count_id != TriggerHitCountId)
            {
                if (!IsAddCriticalHitRecord)
                {
                    AddCriticalHitCountId = hit_count_id;
                    IsAddCriticalHitRecord = true;
                }
                else if (hit_count_id != AddCriticalHitCountId)
                {
                    Refresh();
                }
            }
        }

        public bool IsCriticalHit(long hit_count_id)
        {
            if (IsTriggered && hit_count_id == AddCriticalHitCountId)
            {
                return true;
            }
            return false;
        }

        private bool CanTrigger(long hit_count_id)
        {
            if (!IsTriggered)
            {
                return true;
            }
            else if (TriggerTime + RemainTimeMS < TimeUtility.GetLocalMilliseconds())
            {
                return true;
            }
            return false;
        }

        private void Refresh()
        {
            IsTriggered = false;
            TriggerHitCountId = -1;
            IsAddCriticalHitRecord = false;
            AddCriticalHitCountId = -1;
        }
    }
}
