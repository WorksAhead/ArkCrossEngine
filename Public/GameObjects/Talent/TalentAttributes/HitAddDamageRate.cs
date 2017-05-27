using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class HitAddDamageRate : TalentAttribute
    {
        public int HitCount;
        public float RateAdd;
        public long RemainTime;
        public long TriggerCD;
        public float LevelAddRate;
        public long LevelAddRemainTime;

        private long m_LastHitCountId = -1;
        private int m_HitNum = 0;
        private long m_LastTriggerTime = -1;

        public override AttributeId GetId() { return AttributeId.kHitAddDamageRate; }
        public override void Init(List<string> attri_list, List<string> level_add)
        {
            if (attri_list.Count >= 4)
            {
                HitCount = int.Parse(attri_list[0]);
                RateAdd = float.Parse(attri_list[1]);
                RemainTime = long.Parse(attri_list[2]);
                TriggerCD = long.Parse(attri_list[3]);
            }
            if (level_add.Count >= 3)
            {
                LevelAddRate = float.Parse(level_add[1]);
                LevelAddRemainTime = long.Parse(level_add[2]);
            }
            m_LastTriggerTime = -RemainTime;
            //LogSystem.Error("----HitAddDamageRate add {0} remain {1}", RateAdd, RemainTime);
        }
        public override void UpdateToLevel(int level)
        {
            RateAdd = RateAdd - (m_Level - 1) * LevelAddRate;
            RemainTime = RemainTime - (m_Level - 1) * LevelAddRemainTime;
            m_Level = level;
            RateAdd = RateAdd + (m_Level - 1) * LevelAddRate;
            RemainTime = RemainTime + (m_Level - 1) * LevelAddRemainTime;
        }

        public void OnHit(long hit_count_id)
        {
            if (TimeUtility.GetLocalMilliseconds() < m_LastTriggerTime + TriggerCD)
            {
                return;
            }
            if (hit_count_id != m_LastHitCountId)
            {
                m_HitNum = 0;
            }
            if (m_HitNum == 0)
            {
                m_LastHitCountId = hit_count_id;
                m_HitNum++;
            }
            else if (hit_count_id == m_LastHitCountId)
            {
                m_HitNum++;
            }
            if (m_HitNum >= HitCount)
            {
                m_LastTriggerTime = TimeUtility.GetLocalMilliseconds();
                //LogSystem.Error("---hit add damage rate triggered hit_count_id={0}", hit_count_id);
            }
        }

        public bool IsTriggered()
        {
            if (TimeUtility.GetLocalMilliseconds() < m_LastTriggerTime + RemainTime)
            {
                return true;
            }
            return false;
        }
    }
}
