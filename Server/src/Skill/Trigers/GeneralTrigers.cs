using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;
using SkillSystem;

namespace DashFire.Trigers
{
    /// <summary>
    /// movecontrol(is_skill_control_move);
    /// </summary>
    internal class MoveControlTriger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            MoveControlTriger triger = new MoveControlTriger();
            triger.m_IsControlMove = m_IsControlMove;
            return triger;
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            if (m_IsControlMove)
            {
                instance.IsControlMove = true;
                ++instance.StartWithoutStopMoveCount;
            }
            else
            {
                if (instance.StartWithoutStopMoveCount > 0)
                    --instance.StartWithoutStopMoveCount;
                ++instance.StopMoveCount;
            }
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num > 0)
            {
                m_IsControlMove = (callData.GetParamId(0) == "true");
            }
        }

        private bool m_IsControlMove = false;
    }

    internal class MoveSectionInfo
    {
        internal float moveTime;
        internal Vector3 speedVect;
        internal Vector3 accelVect;
    }

    internal class CurveMovementTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            CurveMovementTrigger copy = new CurveMovementTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_SectionList.AddRange(m_SectionList);
            return copy;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() > 0)
            {
                m_StartTime = int.Parse(callData.GetParamId(0));
            }
            m_SectionList.Clear();
            int section_num = 0;
            while (callData.GetParamNum() >= 7 * (section_num + 1) + 2)
            {
                MoveSectionInfo section = new MoveSectionInfo();
                section.moveTime = (float)System.Convert.ToDouble(callData.GetParamId((section_num * 7) + 2));
                section.speedVect.X = (float)System.Convert.ToDouble(callData.GetParamId((section_num * 7) + 3));
                section.speedVect.Y = (float)System.Convert.ToDouble(callData.GetParamId((section_num * 7) + 4));
                section.speedVect.Z = (float)System.Convert.ToDouble(callData.GetParamId((section_num * 7) + 5));
                section.accelVect.X = (float)System.Convert.ToDouble(callData.GetParamId((section_num * 7) + 6));
                section.accelVect.Y = (float)System.Convert.ToDouble(callData.GetParamId((section_num * 7) + 7));
                section.accelVect.Z = (float)System.Convert.ToDouble(callData.GetParamId((section_num * 7) + 8));
                m_SectionList.Add(section);
                section_num++;
            }
            if (m_SectionList.Count == 0)
            {
                return;
            }
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            float lenth = 0;
            foreach (MoveSectionInfo info in m_SectionList)
            {
                float dx = 0;
                float dz = 0;
                float time_sqr_div_2 = info.moveTime * info.moveTime / 2;
                dx = info.speedVect.X * info.moveTime + info.accelVect.X * time_sqr_div_2;
                dz = info.speedVect.Z * info.moveTime + info.accelVect.Z * time_sqr_div_2;
                lenth += (float)Math.Sqrt(dx * dx + dz * dz);
            }
            if (lenth < instance.CurMoveDistanceForFindTarget)
            {
                if (lenth > Geometry.c_FloatPrecision)
                {
                    lenth = instance.CurMoveDistanceForFindTarget;
                }
            }
            instance.CurMoveDistanceForFindTarget = 0;
            instance.MaxMoveDelta += lenth;
        }

        private List<MoveSectionInfo> m_SectionList = new List<MoveSectionInfo>();
    }

    /// <summary>
    /// areadamage(start_time,center_x, center_y, center_z, range, is_clear_when_finish[,impact_id,...]) {
    ///   showtip(time, color_r, color_g, color_b);
    ///   stateimpact(statename, impact_id[,impact_id...]); 
    /// };
    /// </summary>
    internal class AreaDamageTriger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            AreaDamageTriger triger = new AreaDamageTriger();
            triger.m_Impacts = m_Impacts;
            return triger;
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            instance.EnableImpactsToOther.AddRange(m_Impacts);
        }

        protected override void Load(ScriptableData.FunctionData funcData)
        {
            ScriptableData.CallData callData = funcData.Call;
            if (null != callData)
            {
                foreach (ScriptableData.ISyntaxComponent statement in funcData.Statements)
                {
                    ScriptableData.CallData stCall = statement as ScriptableData.CallData;
                    if (null != stCall)
                    {
                        string id = stCall.GetId();
                        if (id == "stateimpact")
                        {
                            if (stCall.GetParamNum() > 1)
                            {
                                m_Impacts.Add(int.Parse(stCall.GetParamId(1)));
                            }
                        }
                    }
                }
            }
        }

        private List<int> m_Impacts = new List<int>();
    }

    /// <summary>
    /// colliderdamage(start_time, remain_time, is_clear_when_finish, is_always_enter_damage, damage_interval, max_damage_times)
    /// {
    ///   stateimpact("kDefault", 100101);
    ///   scenecollider("prefab",vector3(0,0,0));
    ///   bonecollider("prefab","bone", is_attach);
    /// };
    /// </summary>
    internal class ColliderDamageTriger : AbstractSkillTriger
    {
        public static int NORMAL_TICK_MS = 35;
        public override ISkillTriger Clone()
        {
            ColliderDamageTriger copy = new ColliderDamageTriger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            copy.m_IsClearWhenFinish = m_IsClearWhenFinish;
            copy.m_IsAlwaysEnterDamage = m_IsAlwaysEnterDamage;
            copy.m_DamageInterval = m_DamageInterval;
            copy.m_MaxDamageTimes = m_MaxDamageTimes;
            copy.m_Impacts = m_Impacts;
            return copy;
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            int count = 1;
            if (m_DamageInterval > 0)
            {
                count = (int)((m_RemainTime + NORMAL_TICK_MS) / m_DamageInterval) + 1;
            }
            else if (m_IsAlwaysEnterDamage)
            {
                count = m_MaxDamageTimes;
            }
            for (int i = 0; i < count; ++i)
            {
                instance.EnableImpactsToOther.AddRange(m_Impacts);
            }
        }

        protected override void Load(ScriptableData.FunctionData funcData)
        {
            ScriptableData.CallData callData = funcData.Call;
            if (null == callData)
            {
                return;
            }
            int num = callData.GetParamNum();
            if (num >= 6)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
                m_IsClearWhenFinish = bool.Parse(callData.GetParamId(2));
                m_IsAlwaysEnterDamage = bool.Parse(callData.GetParamId(3));
                m_DamageInterval = long.Parse(callData.GetParamId(4));
                m_MaxDamageTimes = int.Parse(callData.GetParamId(5));
            }
            foreach (ScriptableData.ISyntaxComponent statement in funcData.Statements)
            {
                ScriptableData.CallData stCall = statement as ScriptableData.CallData;
                if (null != stCall)
                {
                    string id = stCall.GetId();
                    if (id == "stateimpact")
                    {
                        if (stCall.GetParamNum() > 1)
                        {
                            m_Impacts.Add(int.Parse(stCall.GetParamId(1)));
                        }
                    }
                }
            }
        }

        private long m_RemainTime = 0;
        private bool m_IsClearWhenFinish = false;
        private bool m_IsAlwaysEnterDamage = false;
        private long m_DamageInterval = 0;
        private int m_MaxDamageTimes = 0;
        private List<int> m_Impacts = new List<int>();
    }

    internal class SummonObjectTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SummonObjectTrigger copy = new SummonObjectTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_NpcTypeId = m_NpcTypeId;
            copy.m_ModelPrefab = m_ModelPrefab;
            copy.m_SkillId = m_SkillId;
            copy.m_IsSimulate = m_IsSimulate;
            return copy;
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            instance.SummonNpcSkills.Add(m_SkillId);
            instance.SummonNpcs.Add(m_NpcTypeId);
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
        }

        protected override void Load(ScriptableData.FunctionData funcData)
        {
            ScriptableData.CallData callData = funcData.Call;
            if (null == callData)
            {
                return;
            }
            Load(callData);
        }

        private int m_NpcTypeId;
        private string m_ModelPrefab;
        private int m_SkillId;
        private bool m_IsSimulate = false;
    }

    internal class ChooseTargetTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            ChooseTargetTrigger copy = new ChooseTargetTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_Center = m_Center;
            copy.m_Radius = m_Radius;
            copy.m_Degree = m_Degree;
            copy.m_DistancePriority = m_DistancePriority;
            copy.m_DegreePriority = m_DegreePriority;
            copy.m_ToTargetDistanceRatio = m_ToTargetDistanceRatio;
            copy.m_ToTargetConstDistance = m_ToTargetConstDistance;
            return copy;
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            float max_distance = Geometry.Distance(m_Center, Vector3.Zero) + m_Radius;
            instance.CurMoveDistanceForFindTarget = max_distance * (1 + m_ToTargetDistanceRatio) + m_ToTargetConstDistance;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int num = callData.GetParamNum();
            if (num >= 8)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                ScriptableData.CallData vect_param1 = callData.GetParam(1) as ScriptableData.CallData;
                m_Center = ScriptableDataUtility.CalcVector3(vect_param1);
                m_Radius = float.Parse(callData.GetParamId(2));
                m_Degree = float.Parse(callData.GetParamId(3));
                m_DistancePriority = float.Parse(callData.GetParamId(4));
                m_DegreePriority = float.Parse(callData.GetParamId(5));
                m_ToTargetDistanceRatio = float.Parse(callData.GetParamId(6));
                m_ToTargetConstDistance = float.Parse(callData.GetParamId(7));
            }
        }

        private Vector3 m_Center;
        private float m_Radius;
        private float m_Degree;
        private float m_DistancePriority;
        private float m_DegreePriority;
        private float m_ToTargetDistanceRatio;
        private float m_ToTargetConstDistance;
    }

    internal class AddImpactToSelfTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            AddImpactToSelfTrigger copy = new AddImpactToSelfTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_ImpactId = m_ImpactId;
            copy.m_RemainTime = m_RemainTime;
            return copy;
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            instance.EnableImpactsToMyself.Add(m_ImpactId);
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 2)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_ImpactId = int.Parse(callData.GetParamId(1));
            }
            if (callData.GetParamNum() >= 3)
            {
                m_RemainTime = int.Parse(callData.GetParamId(2));
            }
        }

        private int m_ImpactId;
        private int m_RemainTime = -1;
    }

    internal class AddImpactToTargetTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            AddImpactToTargetTrigger copy = new AddImpactToTargetTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_ImpactId = m_ImpactId;
            copy.m_RemainTime = m_RemainTime;
            return copy;
        }

        public override void Reset()
        {
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            instance.EnableImpactsToOther.Add(m_ImpactId);
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 2)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_ImpactId = int.Parse(callData.GetParamId(1));
            }
            if (callData.GetParamNum() >= 3)
            {
                m_RemainTime = long.Parse(callData.GetParamId(2));
            }
        }

        private int m_ImpactId;
        private long m_RemainTime = -1;
    }

    internal class ExchangePositionTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            ExchangePositionTrigger copy = new ExchangePositionTrigger();
            copy.m_StartTime = m_StartTime;
            return copy;
        }

        public override void Reset()
        {
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            if (instance.MaxMoveDelta < 10)
                instance.MaxMoveDelta = 10;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
        }
    }

    internal class FruitNinjiaTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            FruitNinjiaTrigger copy = new FruitNinjiaTrigger();
            copy.m_StartTime = m_StartTime;
            foreach (int si in m_Impacts)
            {
                copy.m_Impacts.Add(si);
            }
            return copy;
        }

        public override void Reset()
        {
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            instance.EnableImpactsToOther.AddRange(m_Impacts);
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
        }

        protected override void Load(ScriptableData.FunctionData funcData)
        {
            ScriptableData.CallData callData = funcData.Call;
            if (null != callData)
            {
                Load(callData);

                foreach (ScriptableData.ISyntaxComponent statement in funcData.Statements)
                {
                    ScriptableData.CallData stCall = statement as ScriptableData.CallData;
                    if (null == stCall || stCall.GetParamNum() <= 0)
                    {
                        continue;
                    }
                    string id = stCall.GetId();
                    if (id == "stateimpact")
                    {
                        LoadStateImpactConfig(stCall);
                    }
                }
            }
        }

        private void LoadStateImpactConfig(ScriptableData.CallData stCall)
        {
            for (int i = 1; i < stCall.GetParamNum(); i = i + 2)
            {
                int impactId = int.Parse(stCall.GetParamId(i));
                m_Impacts.Add(impactId);
            }
        }

        private List<int> m_Impacts = new List<int>();
    }

    internal class OnCrossTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            OnCrossTrigger copy = new OnCrossTrigger();
            copy.m_StartTime = m_StartTime;
            foreach (int si in m_Impacts)
            {
                copy.m_Impacts.Add(si);
            }
            return copy;
        }

        public override void Reset()
        {
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            instance.EnableImpactsToOther.AddRange(m_Impacts);
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
        }

        protected override void Load(ScriptableData.FunctionData funcData)
        {
            ScriptableData.CallData callData = funcData.Call;
            if (null != callData)
            {
                Load(callData);

                foreach (ScriptableData.ISyntaxComponent statement in funcData.Statements)
                {
                    ScriptableData.CallData stCall = statement as ScriptableData.CallData;
                    if (null == stCall || stCall.GetParamNum() <= 0)
                    {
                        continue;
                    }
                    string id = stCall.GetId();
                    if (id == "stateimpact")
                    {
                        LoadStateImpactConfig(stCall);
                    }
                }
            }
        }

        private void LoadStateImpactConfig(ScriptableData.CallData stCall)
        {
            for (int i = 1; i < stCall.GetParamNum(); i = i + 2)
            {
                int impactId = int.Parse(stCall.GetParamId(i));
                m_Impacts.Add(impactId);
            }
        }

        private List<int> m_Impacts = new List<int>();
    }

    internal class Move2TargetPosTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            Move2TargetPosTrigger copy = new Move2TargetPosTrigger();
            copy.m_StartTime = m_StartTime;
            return copy;
        }

        public override void Reset()
        {
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            if (instance.MaxMoveDelta < 10)
                instance.MaxMoveDelta = 10;
        }
    }

    internal class RestorePosTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            RestorePosTrigger copy = new RestorePosTrigger();
            copy.m_StartTime = m_StartTime;
            return copy;
        }

        public override void Reset()
        {
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            if (instance.MaxMoveDelta < 10)
                instance.MaxMoveDelta = 10;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
        }
    }

    internal class CrossSummonMoveTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            CrossSummonMoveTrigger copy = new CrossSummonMoveTrigger();
            copy.m_StartTime = m_StartTime;
            return copy;
        }

        public override void Reset()
        {
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            if (instance.MaxMoveDelta < 10)
                instance.MaxMoveDelta = 10;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 5)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
        }
    }

    public class SetlifeTimeTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SetlifeTimeTrigger copy = new SetlifeTimeTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_RemainTime = m_RemainTime;
            return copy;
        }

        public override void Reset()
        {
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 2)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_RemainTime = long.Parse(callData.GetParamId(1));
            }
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            instance.MaxSkillLifeTime += m_RemainTime;
        }

        private long m_RemainTime;
    }

    public class SimulateMoveTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SimulateMoveTrigger copy = new SimulateMoveTrigger();
            copy.m_StartTime = m_StartTime;
            return copy;
        }

        public override void Reset()
        {
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            instance.IsSimulate = true;
        }
    }

    public class SetTransformTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            SetTransformTrigger copy = new SetTransformTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_BoneName = m_BoneName;
            copy.m_Postion = m_Postion;
            copy.m_RelativeType = m_RelativeType;
            copy.m_IsAttach = m_IsAttach;
            return copy;
        }

        public override void Reset()
        {
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 6)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_BoneName = callData.GetParamId(1);
                m_Postion = ScriptableDataUtility.CalcVector3(callData.GetParam(2) as ScriptableData.CallData);
                m_RelativeType = callData.GetParamId(4);
                m_IsAttach = bool.Parse(callData.GetParamId(5));
            }
        }

        public override void Analyze(object sender, SkillInstance instance)
        {
            if (m_RelativeType == "RelativeOwner" || m_RelativeType == "RelativeSelf")
            {
                instance.MaxMoveDelta += Geometry.Distance(m_Postion, Vector3.Zero);
            }
            else if (m_RelativeType == "RelativeTarget")
            {
                instance.MaxMoveDelta += instance.CurMoveDistanceForFindTarget + Geometry.Distance(m_Postion, Vector3.Zero);
            }
        }

        private string m_BoneName;
        private string m_RelativeType;
        private Vector3 m_Postion;
        private bool m_IsAttach;
    }
}
