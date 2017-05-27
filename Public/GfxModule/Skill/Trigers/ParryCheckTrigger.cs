using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    class ParryCheckTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            ParryCheckTrigger copy = new ParryCheckTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            copy.m_CheckDelay = m_CheckDelay;
            copy.m_MessageParryTrue = m_MessageParryTrue;
            copy.m_MessageParryFalse = m_MessageParryFalse;
            copy.m_IsForbidDamage = m_IsForbidDamage;
            return copy;
        }
        public override void Reset()
        {
            m_LastCheckTime = 0;
            m_IsFirstCheck = true;
            m_IsInited = false;
            m_SkillInstance = null;
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
            GameObject obj = sender as GameObject;
            if (obj == null)
            {
                return false;
            }
            if (!m_IsInited)
            {
                m_IsInited = true;
                m_SkillInstance = instance;
                SharedGameObjectInfo sgoi = LogicSystem.GetSharedGameObjectInfo(obj);
                if (sgoi != null)
                {
                    sgoi.HandleEventCheckHitCanRelease = CheckHitCanRelease;
                }
            }
            return true;
        }
        public bool CheckHitCanRelease(GameObject castgo, int impactId)
        {
            if (castgo != null)
            {
                if (m_SkillInstance != null)
                {
                    long now = TimeUtility.GetServerMilliseconds();
                    m_SkillInstance.CustomDatas.AddData<GameObjectBox>(new GameObjectBox(castgo));
                    if (m_IsFirstCheck || now - m_LastCheckTime >= m_CheckDelay)
                    {
                        ImpactLogicData ild = SkillConfigProvider.Instance.impactLogicDataMgr.GetDataById(impactId);
                        if (ild != null)
                        {
                            if (ild.BreakParry)
                            {
                                if (m_MessageParryFalse != "")
                                {
                                    m_SkillInstance.SendMessage(m_MessageParryFalse);
                                }
                            }
                            else
                            {
                                if (m_MessageParryTrue != "")
                                {
                                    m_SkillInstance.SendMessage(m_MessageParryTrue);
                                }
                            }
                        }
                        m_LastCheckTime = now;
                        m_IsFirstCheck = false;
                    }
                }
            }
            return !m_IsForbidDamage;
        }
        public override void Analyze(object sender, SkillInstance instance)
        {

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
                m_CheckDelay = long.Parse(callData.GetParamId(2));
            }
            if (callData.GetParamNum() >= 4)
            {
                m_MessageParryTrue = callData.GetParamId(3);
                if (m_MessageParryTrue == " ")
                {
                    m_MessageParryTrue = "";
                }
            }
            if (callData.GetParamNum() >= 5)
            {
                m_MessageParryFalse = callData.GetParamId(4);
                if (m_MessageParryFalse == " ")
                {
                    m_MessageParryFalse = "";
                }
            }
            if (callData.GetParamNum() >= 6)
            {
                m_IsForbidDamage = bool.Parse(callData.GetParamId(5));
            }
        }

        private long m_RemainTime = 0;
        private long m_CheckDelay = 0;
        private long m_LastCheckTime = 0;
        private string m_MessageParryTrue = "onparrytrue";
        private string m_MessageParryFalse = "onparryfalse";
        private bool m_IsFirstCheck = true;
        private bool m_IsInited = false;
        private bool m_IsForbidDamage = true;
        private SkillInstance m_SkillInstance = null;
    }
    public class GameObjectBox
    {
        public GameObjectBox(GameObject go)
        {
            myGameObject = go;
        }
        public GameObject MyGameObject
        {
            get { return myGameObject; }
        }
        private GameObject myGameObject = null;
    }
}