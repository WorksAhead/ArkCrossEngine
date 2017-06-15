using System;
using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    class FaceToAttackerTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            FaceToAttackerTrigger copy = new FaceToAttackerTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            return copy;
        }

        public override void Reset()
        {
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 2)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
            }
        }

        public override bool Execute(object sender, SkillInstance instance, long delta, long curSectionTime)
        {
            if (curSectionTime < m_StartTime)
            {
                return true;
            }
            if (curSectionTime > (m_StartTime + m_RemainTime))
            {
                return false;
            }
            UnityEngine.GameObject obj = sender as UnityEngine.GameObject;
            if (obj == null)
            {
                return false;
            }
            GameObjectBox gob = instance.CustomDatas.GetData<GameObjectBox>();
            if (gob != null && LogicSystem.ExistGameObject(gob.MyGameObject))
            {
                UnityEngine.Vector3 dir = gob.MyGameObject.transform.position - obj.transform.position;
                GfxSkillSystem.ChangeAllDir(obj, (float)Math.Atan2(dir.x, dir.z));
            }
            return true;
        }

        private long m_RemainTime = 0;
    }
}