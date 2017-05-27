using SkillSystem;
using ArkCrossEngine;

namespace GfxModule.Skill.Trigers
{
    class PalmHitTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            PalmHitTrigger copy = new PalmHitTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            copy.m_LeftMessage = m_LeftMessage;
            copy.m_RightMessage = m_RightMessage;
            return copy;
        }
        public override void Reset()
        {
            m_IsInited = false;
            m_skillinstance = null;
            if (m_EventLeft != null)
            {
                LogicSystem.EventChannelForGfx.Unsubscribe(m_EventLeft);
                m_EventLeft = null;
            }
            if (m_EventRight != null)
            {
                LogicSystem.EventChannelForGfx.Unsubscribe(m_EventRight);
                m_EventRight = null;
            }
            LogicSystem.EventChannelForGfx.Publish("ge_show_palm", "ui", false);
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
                m_skillinstance = instance;
                LogicSystem.EventChannelForGfx.Publish("ge_show_palm", "ui", true);
                m_EventLeft = LogicSystem.EventChannelForGfx.Subscribe("ge_ui_leftpalm", "ui", Left);
                m_EventRight = LogicSystem.EventChannelForGfx.Subscribe("ge_ui_rightpalm", "ui", Right);
            }
            return true;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 2)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
            }
            if (callData.GetParamNum() >= 3)
            {
                m_LeftMessage = callData.GetParamId(2);
                if (m_LeftMessage == " ")
                {
                    m_LeftMessage = "";
                }
            }
            if (callData.GetParamNum() >= 4)
            {
                m_RightMessage = callData.GetParamId(3);
                if (m_RightMessage == " ")
                {
                    m_RightMessage = "";
                }
            }
        }
        private void Left()
        {
            if (m_skillinstance != null)
            {
                m_skillinstance.SendMessage(m_LeftMessage);
            }
        }
        private void Right()
        {
            if (m_skillinstance != null)
            {
                m_skillinstance.SendMessage(m_RightMessage);
            }
        }
        private long m_RemainTime = 0;
        private bool m_IsInited = false;

        private string m_LeftMessage = "left";
        private string m_RightMessage = "right";

        private object m_EventLeft = null;
        private object m_EventRight = null;
        private SkillInstance m_skillinstance = null;
    }
}