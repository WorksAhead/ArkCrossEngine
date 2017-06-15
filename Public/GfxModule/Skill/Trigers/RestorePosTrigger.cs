using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    public class StorePosTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            StorePosTrigger copy = new StorePosTrigger();
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
            UnityEngine.Vector3 pos = obj.transform.position;
            instance.CustomDatas.AddData<UnityEngine.Vector3>(pos);
            return false;
        }
    }

    public class RestorePosTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            RestorePosTrigger copy = new RestorePosTrigger();
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
            UnityEngine.Vector3 old_pos = instance.CustomDatas.GetData<UnityEngine.Vector3>();
            if (old_pos != null)
            {
                obj.transform.position = old_pos;
                TriggerUtil.UpdateObjPosition(obj);
            }
            return false;
        }
    }
}
