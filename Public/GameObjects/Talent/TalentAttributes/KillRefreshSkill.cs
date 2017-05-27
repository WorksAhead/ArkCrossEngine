using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class KillRefreshSkill : TalentAttribute
    {
        public int KillCount;
        public long KillInterverl;

        private List<long> KillsInfo = new List<long>();

        public override AttributeId GetId() { return AttributeId.kKillRefreshAllSkills; }

        public override void Init(List<string> attri_list, List<string> level_add)
        {
            if (attri_list.Count >= 2)
            {
                KillCount = int.Parse(attri_list[0]);
                KillInterverl = long.Parse(attri_list[1]);
            }
            //LogSystem.Error("----talent attribute init: {0} kill count {1} interval {2}", GetId(), KillCount, KillInterverl);
        }

        public void AddKillCount(CharacterInfo character)
        {
            for (int i = KillsInfo.Count - 1; i >= 0; i--)
            {
                if (IsTimeOut(KillsInfo[i]))
                {
                    KillsInfo.RemoveAt(i);
                }
            }
            KillsInfo.Add(TimeUtility.GetLocalMilliseconds());
        }

        public bool RefreshSkill(CharacterInfo character)
        {
            if (KillsInfo.Count >= KillCount)
            {
                KillsInfo.Clear();
                RefreshAllSkills(character);
                return true;
            }
            return false;
        }

        public void RefreshAllSkills(CharacterInfo character)
        {
            //LogSystem.Error("---------refresh all skills!");
            List<SkillInfo> skills = character.GetSkillStateInfo().GetAllSkill();
            for (int i = 0; i < skills.Count; i++)
            {
                SkillInfo skill_info = skills[i];
                if (skill_info != null)
                {
                    skill_info.Refresh();
                }
            }
        }

        private bool IsTimeOut(long kill_time)
        {
            if (kill_time + KillInterverl < TimeUtility.GetLocalMilliseconds())
            {
                return true;
            }
            return false;
        }
    }
}
