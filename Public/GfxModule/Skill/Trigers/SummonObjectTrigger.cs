using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    public class SummonObjectTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SummonObjectTrigger copy = new SummonObjectTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_NpcTypeId = m_NpcTypeId;
            copy.m_ModelPrefab = m_ModelPrefab;
            copy.m_SkillId = m_SkillId;
            copy.m_AiLogicId = m_AiLogicId;
            copy.m_followsummonerdead = m_followsummonerdead;
            copy.m_LocalPostion = m_LocalPostion;
            copy.m_LocalRotate = m_LocalRotate;
            copy.m_AiParamStr = m_AiParamStr;
            copy.m_SignForSkill = m_SignForSkill;
            copy.m_IsSimulate = m_IsSimulate;
            return copy;
        }

        public override void Reset()
        {

        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 4)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_NpcTypeId = int.Parse(callData.GetParamId(1));
                m_ModelPrefab = callData.GetParamId(2);
                if (m_ModelPrefab == " ")
                {
                    m_ModelPrefab = "";
                }
                m_SkillId = int.Parse(callData.GetParamId(3));
            }
            if (callData.GetParamNum() >= 5)
            {
                m_LocalPostion = ScriptableDataUtility.CalcVector3(callData.GetParam(4) as ScriptableData.CallData);
            }
            if (callData.GetParamNum() >= 6)
            {
                m_LocalRotate = ScriptableDataUtility.CalcVector3(callData.GetParam(5) as ScriptableData.CallData);
            }
            if (callData.GetParamNum() >= 7)
            {
                m_AiLogicId = int.Parse(callData.GetParamId(6));
            }
            if (callData.GetParamNum() >= 8)
            {
                m_followsummonerdead = bool.Parse(callData.GetParamId(7));
            }
            if (callData.GetParamNum() >= 9)
            {
                m_AiParamStr = callData.GetParamId(8);
                if (m_AiParamStr == " ")
                {
                    m_AiParamStr = "";
                }
            }
            if (callData.GetParamNum() >= 10)
            {
                m_SignForSkill = int.Parse(callData.GetParamId(9));
            }
        }
        protected override void Load(ScriptableData.FunctionData funcData)
        {
            ScriptableData.CallData callData = funcData.Call;
            if (null == callData)
            {
                return;
            }
            Load(callData);
            for (int i = 0; i < funcData.Statements.Count; i++)
            {
                ScriptableData.CallData stCall = funcData.Statements[i] as ScriptableData.CallData;
                if (null == stCall)
                {
                    continue;
                }
                if (stCall.GetId() == "signforskill")
                {
                    if (stCall.GetParamNum() >= 1)
                    {
                        m_SignForSkill = int.Parse(stCall.GetParamId(0));
                    }
                }
                if (stCall.GetId() == "issimulate")
                {
                    if (stCall.GetParamNum() >= 1)
                    {
                        m_IsSimulate = bool.Parse(stCall.GetParamId(0));
                        //Debug.Log("---part simulate=" + m_IsSimulate);
                    }
                }
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
            UnityEngine.Vector3 position = obj.transform.TransformPoint(m_LocalPostion);
            //Debug.Log("---summon npc: isSimulate=" + m_IsSimulate);
            LogicSystem.NotifyGfxSummonNpc(obj, instance.SkillId, m_NpcTypeId, m_ModelPrefab, m_SkillId, m_AiLogicId, m_followsummonerdead,
                                                    position.x, position.y, position.z, m_AiParamStr, m_SignForSkill, m_IsSimulate);
            return false;
        }

        private int m_NpcTypeId;
        private string m_ModelPrefab;
        private int m_SkillId;
        private int m_AiLogicId;
        private bool m_followsummonerdead;
        private UnityEngine.Vector3 m_LocalPostion;
        private UnityEngine.Vector3 m_LocalRotate;
        private string m_AiParamStr;
        private int m_SignForSkill = 0;
        private bool m_IsSimulate = false;
    }
}
