using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    class ImpactLogic_StopImpact : AbstractImpactLogic
    {
        private enum RemoveType
        {
            IMPACT_ID,
            LOGIC_ID,
            GFX_ID,
        }
        public override void StartImpact(CharacterInfo obj, int impactId)
        {
            if (null != obj)
            {
                ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (impactInfo.ConfigData.ParamNum > 1)
                {
                    if (!String.IsNullOrEmpty(impactInfo.ConfigData.ExtraParams[0]) && !String.IsNullOrEmpty(impactInfo.ConfigData.ExtraParams[1]))
                    {
                        int targetLogicId = int.Parse(impactInfo.ConfigData.ExtraParams[0]);
                        int removeType = int.Parse(impactInfo.ConfigData.ExtraParams[1]);
                        RemoveImpact(obj, targetLogicId, (RemoveType)removeType);
                    }
                }
            }
        }

        private void RemoveImpact(CharacterInfo target, int id, RemoveType removeType)
        {
            List<ImpactInfo> impactInfos = target.GetSkillStateInfo().GetAllImpact();
            if (-1 == id)
            {
                ImpactSystem.Instance.StopAllImpact(target);
            }
            else
            {
                switch (removeType)
                {
                    case RemoveType.IMPACT_ID:
                        for (int i = 0; i < impactInfos.Count; i++)
                        {
                            if (impactInfos[i].m_ImpactId == id)
                            {
                                ImpactSystem.Instance.StopImpactById(target, impactInfos[i].m_ImpactId);
                            }
                        }
                        /*
                        foreach (ImpactInfo impact in target.GetSkillStateInfo().GetAllImpact()) {
                          if (-1 == id || impact.m_ImpactId == id) {
                            ImpactSystem.Instance.StopImpactById(target, impact.m_ImpactId);
                          }
                        }*/
                        break;
                    case RemoveType.LOGIC_ID:
                        for (int i = 0; i < impactInfos.Count; i++)
                        {
                            if (impactInfos[i].ConfigData.ImpactLogicId == id)
                            {
                                ImpactSystem.Instance.StopImpactById(target, impactInfos[i].m_ImpactId);
                            }
                        }
                        /*
                        foreach (ImpactInfo impact in target.GetSkillStateInfo().GetAllImpact()) {
                          if (-1 == id || impact.ConfigData.ImpactLogicId == id) {
                            ImpactSystem.Instance.StopImpactById(target, impact.m_ImpactId);
                          }
                        }*/
                        break;
                    case RemoveType.GFX_ID:
                        for (int i = 0; i < impactInfos.Count; i++)
                        {
                            if (impactInfos[i].ConfigData.ImpactGfxLogicId == id)
                            {
                                ImpactSystem.Instance.StopImpactById(target, impactInfos[i].m_ImpactId);
                            }
                        }
                        /*
                        foreach (ImpactInfo impact in target.GetSkillStateInfo().GetAllImpact()) {
                          if (-1 == id || impact.ConfigData.ImpactGfxLogicId == id) {
                            ImpactSystem.Instance.StopImpactById(target, impact.m_ImpactId);
                          }
                        }*/
                        break;
                }
            }
        }
    }
}

