using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    class SonReleaseSkillTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SonReleaseSkillTrigger copy = new SonReleaseSkillTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_signforskill = m_signforskill;
            copy.m_SkillId = m_SkillId;
            return copy;
        }
        public override void Reset()
        {

        }
        public override bool Execute(object sender, SkillInstance instance, long delta, long curSectionTime)
        {
            if (curSectionTime < m_StartTime)
            {
                return true;
            }
            UnityEngine.GameObject obj = sender as UnityEngine.GameObject;
            if (obj == null)
            {
                return false;
            }
            LogicSystem.NotifyGfxSonsReleaseSkill(obj, m_signforskill, m_SkillId);
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 3)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_signforskill = int.Parse(callData.GetParamId(1));
                m_SkillId = int.Parse(callData.GetParamId(2));
            }
        }
        private int m_signforskill;
        private int m_SkillId;
    }
}