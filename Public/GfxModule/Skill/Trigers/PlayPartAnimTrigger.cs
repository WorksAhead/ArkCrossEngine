using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    public class PlayPartAnimTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            PlayPartAnimTrigger copy = new PlayPartAnimTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_PartName = m_PartName;
            copy.m_AnimName = m_AnimName;
            copy.m_WrapMode = m_WrapMode;
            copy.m_AnimSpeed = m_AnimSpeed;
            copy.m_FadeLength = m_FadeLength;
            return copy;
        }

        public override void Reset()
        {
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 3)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_PartName = callData.GetParamId(1);
                m_AnimName = callData.GetParamId(2);
            }
            if (callData.GetParamNum() >= 4)
            {
                m_WrapMode = (UnityEngine.WrapMode)int.Parse(callData.GetParamId(3));
            }
            if (callData.GetParamNum() >= 5)
            {
                m_AnimSpeed = float.Parse(callData.GetParamId(4));
            }
            if (callData.GetParamNum() >= 6)
            {
                m_FadeLength = float.Parse(callData.GetParamId(5));
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
            UnityEngine.Transform part_transform = TriggerUtil.GetChildNodeByName(obj, m_PartName);
            if (part_transform == null || part_transform.gameObject == null)
            {
                LogSystem.Debug("----play part anim: not find part {0}", m_PartName);
                return false;
            }
            UnityEngine.GameObject part = part_transform.gameObject;
            UnityEngine.AnimationState anim_state = part.GetComponent<UnityEngine.Animation>()[m_AnimName];
            if (anim_state == null)
            {
                LogSystem.Debug("----play part anim: not find anim {0}", m_AnimName);
                return false;
            }
            anim_state.speed = m_AnimSpeed;
            anim_state.wrapMode = m_WrapMode;
            if (m_FadeLength <= 0)
            {
                part.GetComponent<UnityEngine.Animation>().Play(m_AnimName);
            }
            else
            {
                part.GetComponent<UnityEngine.Animation>().CrossFade(m_AnimName, m_FadeLength);
            }
            return false;
        }

        private string m_PartName = "";
        private string m_AnimName = "";
        private UnityEngine.WrapMode m_WrapMode = UnityEngine.WrapMode.Once;
        private float m_AnimSpeed = 1;
        private float m_FadeLength = 0;
    }
}
