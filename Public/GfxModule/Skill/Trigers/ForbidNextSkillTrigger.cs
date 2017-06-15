using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    public class ForbidNextSkillTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            ForbidNextSkillTrigger copy = new ForbidNextSkillTrigger();
            copy.m_StartTime = m_StartTime;
            return copy;
        }

        public override void Reset()
        {

        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
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
            SharedGameObjectInfo owner_info = LogicSystem.GetSharedGameObjectInfo(obj);
            if (owner_info == null)
            {
                return false;
            }
            LogicSystem.NotifyGfxForbidNextSkill(owner_info.m_LogicObjectId);
            return false;
        }
    }
}
