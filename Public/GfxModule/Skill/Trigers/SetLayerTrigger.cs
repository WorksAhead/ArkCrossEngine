using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    public class SetLayerTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SetLayerTrigger copy = new SetLayerTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            copy.m_LayerName = m_LayerName;
            return copy;
        }

        public override void Reset()
        {
            m_IsInited = false;
            ResetLayer();
            m_Target = null;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
                m_LayerName = callData.GetParamId(2);
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
                ResetLayer();
                return false;
            }

            if (!m_IsInited)
            {
                GameObject obj = sender as GameObject;
                if (obj != null)
                {
                    Init(obj);
                }
            }
            return true;
        }

        private void Init(GameObject obj)
        {
            m_IsInited = true;
            m_Target = obj;
            if (m_Target != null)
            {
                m_OldLayer = m_Target.layer;
                m_Target.layer = LayerMask.NameToLayer(m_LayerName);
            }
        }

        private void ResetLayer()
        {
            if (m_Target != null)
            {
                m_Target.layer = m_OldLayer;
                m_Target = null;
            }
        }

        private float m_RemainTime;
        private string m_LayerName;

        private bool m_IsInited = false;
        private GameObject m_Target;
        private int m_OldLayer;
    }
}
