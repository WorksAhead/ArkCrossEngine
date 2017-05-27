﻿using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    class ParryCheckOverTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            ParryCheckOverTrigger copy = new ParryCheckOverTrigger();
            copy.m_StartTime = m_StartTime;
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
            GameObject obj = sender as GameObject;
            if (obj == null)
            {
                return false;
            }
            SharedGameObjectInfo sgoi = LogicSystem.GetSharedGameObjectInfo(obj);
            if (sgoi != null)
            {
                sgoi.HandleEventCheckHitCanRelease = null;
            }
            return false;
        }
        public override void Analyze(object sender, SkillInstance instance)
        {

        }
        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
        }
    }
}