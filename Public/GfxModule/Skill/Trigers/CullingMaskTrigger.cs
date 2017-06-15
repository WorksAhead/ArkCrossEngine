using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    class CullingMaskTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            CullingMaskTrigger copy = new CullingMaskTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            copy.m_CMRemain = m_CMRemain;
            copy.m_CMStart = CullingMask;
            return copy;
        }
        public override void Reset()
        {
            m_IsInited = false;
            CullingMask = m_CMStart;
        }
        public override bool Execute(object sender, SkillInstance instance, long delta, long curSectionTime)
        {
            if (curSectionTime < m_StartTime)
            {
                return true;
            }
            if (curSectionTime > m_StartTime + m_RemainTime)
            {
                return false;
            }
            if (!m_IsInited)
            {
                m_IsInited = true;
                CullingMask = m_CMRemain;
            }
            return true;
        }
        public override void Analyze(object sender, SkillInstance instance)
        {

        }
        protected override void Load(ScriptableData.CallData callData)
        {
            int count = callData.GetParamNum();
            if (count >= 2)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
            }
            string layer = "";
            int cullingmask = 0;
            for (int i = 2; i < count; ++i)
            {
                layer = callData.GetParamId(i);
                cullingmask |= 1 << UnityEngine.LayerMask.NameToLayer(layer);
            }
            m_CMRemain = cullingmask;
        }
        private int CullingMask
        {
            get
            {
                UnityEngine.Camera cm = UnityEngine.Camera.main;
                if (cm != null)
                {
                    UnityEngine.Camera cc = cm.GetComponent<UnityEngine.Camera>();
                    if (cc != null)
                    {
                        return cc.cullingMask;
                    }
                }
                //wrong Everything
                return -1;
            }
            set
            {
                UnityEngine.Camera cm = UnityEngine.Camera.main;
                if (cm != null)
                {
                    UnityEngine.Camera cc = cm.GetComponent<UnityEngine.Camera>();
                    if (cc != null)
                    {
                        cc.cullingMask = value;
                    }
                }
            }
        }
        private long m_RemainTime = 0;
        private int m_CMStart = 0;
        private int m_CMRemain = 0;
        private bool m_IsInited = false;
    }
}
