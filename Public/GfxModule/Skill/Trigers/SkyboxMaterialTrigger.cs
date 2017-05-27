using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    class SkyboxMaterialTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SkyboxMaterialTrigger copy = new SkyboxMaterialTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            copy.m_SBMRemain = m_SBMRemain;
            copy.m_SBMStart = SkyBoxMeterial;
            return copy;
        }
        public override void Reset()
        {
            m_IsInited = false;
            SkyBoxMeterial = m_SBMStart;
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
                SkyBoxMeterial = m_SBMRemain;
            }
            return true;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 3)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
                string materialname = callData.GetParamId(2);
                Material material = ResourceSystem.GetSharedResource(materialname) as Material;
                if (material != null)
                {
                    m_SBMRemain = material;
                }
            }
        }
        private Material SkyBoxMeterial
        {
            get
            {
                return RenderSettings.skybox;
            }
            set
            {
                RenderSettings.skybox = value;
            }
        }
        private long m_RemainTime = 0;
        private Material m_SBMStart = null;
        private Material m_SBMRemain = null;
        private bool m_IsInited = false;
    }
}
