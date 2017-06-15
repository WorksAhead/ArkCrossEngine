using ArkCrossEngine;
using SkillSystem;
using UnityEngine;

namespace GfxModule.Skill.Trigers
{
    class ShaderPropertyTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            ShaderPropertyTrigger copy = new ShaderPropertyTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            copy.m_gopath = m_gopath;
            copy.m_shadername = m_shadername;
            copy.m_startcolor = m_startcolor;
            copy.m_changecolor = m_changecolor;
            return copy;
        }
        public override void Reset()
        {
            m_material = null;
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
            GameObject obj = sender as GameObject;
            if (obj == null)
            {
                return false;
            }
            if (m_material == null)
            {
                Transform tf = obj.transform.Find(m_gopath);
                if (tf != null)
                {
                    SkinnedMeshRenderer smr = tf.GetComponent<SkinnedMeshRenderer>();
                    if (smr != null)
                    {
                        int count = smr.materials.Length;
                        Material material = null;
                        for (int i = 0; i < count; ++i)
                        {
                            material = smr.materials[i];
                            if (material != null && material.shader != null)
                            {
                                if (material.shader.name.CompareTo(m_shadername) == 0)
                                {
                                    m_material = material;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (m_material != null)
            {
                m_material.color = m_startcolor + m_changecolor * ((curSectionTime - m_StartTime) / 1000f);
            }
            return true;
        }
        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 6)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
                m_gopath = callData.GetParamId(2);
                m_shadername = callData.GetParamId(3);
                m_startcolor = ScriptableDataUtility.CalcColor(callData.GetParam(4) as ScriptableData.CallData);
                m_changecolor = ScriptableDataUtility.CalcColor(callData.GetParam(5) as ScriptableData.CallData);
            }
        }
        private long m_RemainTime = 0;
        private Material m_material = null;
        private string m_gopath = "";
        private string m_shadername = "";
        private UnityEngine.Color m_startcolor = UnityEngine.Color.white;
        private UnityEngine.Color m_changecolor = UnityEngine.Color.white;
    }
}