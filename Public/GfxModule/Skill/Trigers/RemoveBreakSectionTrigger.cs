using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    public class RemoveBreakSectionTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            RemoveBreakSectionTrigger copy = new RemoveBreakSectionTrigger();
            copy.m_BreakType = this.m_BreakType;
            return copy;
        }

        public override void Reset()
        {
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() > 0)
            {
                m_BreakType = int.Parse(callData.GetParamId(0));
            }
        }

        public override bool Execute(object sender, SkillInstance instance, long delta, long curSectionTime)
        {
            UnityEngine.GameObject obj = sender as UnityEngine.GameObject;
            if (obj == null)
            {
                return false;
            }
            LogicSystem.NotifyGfxRemoveSkillBreakSection(obj, instance.SkillId, m_BreakType);
            return false;
        }

        private int m_BreakType = 0;
    }
}