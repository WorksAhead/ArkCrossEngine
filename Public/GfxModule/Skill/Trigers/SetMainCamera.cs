using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    public class SetMainCamera : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SetMainCamera copy = new SetMainCamera();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            copy.m_Distance = m_Distance;
            copy.m_Height = m_Height;
            return copy;
        }

        public override void Reset()
        {
            ResetMainCameraAttr();
            m_IsSeted = false;
            m_MainCameraObj = null;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 4)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
                m_Distance = float.Parse(callData.GetParamId(2));
                m_Height = float.Parse(callData.GetParamId(3));
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
                ResetMainCameraAttr();
                return false;
            }
            GameObject obj = sender as GameObject;
            if (obj == null)
            {
                return false;
            }
            if (!m_IsSeted)
            {
                m_IsSeted = true;
                m_MainCameraObj = TriggerUtil.GetCameraObj();
                SetMainCameraAttr(m_Distance, m_Height);
            }
            return true;
        }

        private void SetMainCameraAttr(float distance, float height)
        {
            if (m_MainCameraObj != null)
            {
                m_MainCameraObj.SendMessage("SetDistanceAndHeight", new float[] { distance, height });
            }
        }

        private void ResetMainCameraAttr()
        {
            if (m_MainCameraObj != null)
            {
                m_MainCameraObj.SendMessage("ResetDistanceAndHeight");
            }
        }

        private long m_RemainTime;
        private float m_Distance;
        private float m_Height;

        private bool m_IsSeted = false;
        private GameObject m_MainCameraObj = null;
    }
}
