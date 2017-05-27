using System.Collections.Generic;
using ArkCrossEngine;

/// <summary>
/// skill(10001)
/// {
///   section(80)
///   {
///     disableinput(10);
///     animation("jump");
///     sound("haha.mp3");
///   };
///   section(10)
///   {
///     animation("kan");
///     addimpact(123,43);
///   };
///   section(1000)
///   {
///     checkinput(skillQ,skillE);
///     complex_triger(123)
///     {
///       configdata1(1231);
///       configdata2(3432)
///       {
///         subconfigdata(1232,321321);
///         subconfigdata(12d32,3fd21321);
///       };
///     };
///   };
///   onstop
///   {
///     triger();
///   };
///   oninterrupt
///   {
///     triger();
///   };
///   onmessage("npckilled",12)
///   {
///     triger();
///   };
/// };
/// </summary>
namespace SkillSystem
{
    public sealed class SkillSection
    {
        public long Duration
        {
            get { return m_Duration; }
        }
        public long CurTime
        {
            get { return m_CurTime; }
        }
        public bool IsFinished
        {
            get { return m_IsFinished; }
        }
        public SkillSection Clone()
        {
            SkillSection section = new SkillSection();
            for (int i = 0; i < m_LoadedTrigers.Count; i++)
            {
                section.m_LoadedTrigers.Add(m_LoadedTrigers[i].Clone());
            }
            /*
            foreach (ISkillTriger triger in m_LoadedTrigers) {
              section.m_LoadedTrigers.Add(triger.Clone());
            }*/
            section.m_Duration = m_Duration;
            return section;
        }
        public void Load(ScriptableData.FunctionData sectionData, int skillId)
        {
            ScriptableData.CallData callData = sectionData.Call;
            if (null != callData && callData.HaveParam())
            {
                if (callData.GetParamNum() > 0)
                {
                    m_Duration = long.Parse(callData.GetParamId(0));
                }
                else
                {
                    m_Duration = 1000;
                    LogSystem.Error("Skill {0} DSL, section must have a time argument !", skillId);
                }
            }
            RefreshTrigers(sectionData, skillId);
        }
        public void Reset()
        {
            for (int i = 0; i < m_Trigers.Count; i++)
            {
                m_Trigers[i].Reset();
            }
            /*
            foreach (ISkillTriger triger in m_Trigers) {
              triger.Reset();
            }*/
            m_Trigers.Clear();
            m_CurTime = 0;
            m_IsFinished = true;
        }
        public void Prepare()
        {
            for (int i = 0; i < m_Trigers.Count; i++)
            {
                m_Trigers[i].Reset();
            }
            /*
            foreach (ISkillTriger triger in m_Trigers) {
              triger.Reset();
            }*/
            m_Trigers.Clear();
            m_CurTime = 0;
            m_IsFinished = false;
            for (int i = 0; i < m_LoadedTrigers.Count; i++)
            {
                m_Trigers.Add(m_LoadedTrigers[i]);
            }
            /*
            foreach (ISkillTriger triger in m_LoadedTrigers) {
              m_Trigers.Add(triger);
            }*/
            m_Trigers.Sort((left, right) =>
            {
                if (left.GetStartTime() > right.GetStartTime())
                {
                    return -1;
                }
                else if (left.GetStartTime() == right.GetStartTime())
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            });
        }
        public void Tick(object sender, SkillInstance instance, long delta)
        {
            if (m_IsFinished)
            {
                return;
            }
            m_CurTime += delta;
            int ct = m_Trigers.Count;
            for (int i = ct - 1; i >= 0; --i)
            {
                ISkillTriger triger = m_Trigers[i];
                if (!triger.Execute(sender, instance, delta, m_CurTime / 1000))
                {
                    triger.Reset();
                    m_Trigers.RemoveAt(i);
                }
            }
            if (m_CurTime / 1000 > m_Duration)
            {
                m_IsFinished = true;
            }
        }
        public void Analyze(object sender, SkillInstance instance)
        {
            for (int i = 0; i < m_LoadedTrigers.Count; i++)
            {
                m_LoadedTrigers[i].Analyze(sender, instance);
            }
            /*
            foreach (ISkillTriger triger in m_LoadedTrigers) {
              triger.Analyze(sender, instance);
            }*/
        }

        private void RefreshTrigers(ScriptableData.FunctionData sectionData, int skillId)
        {
            m_LoadedTrigers.Clear();
            for (int i = 0; i < sectionData.Statements.Count; i++)
            {
                ISkillTriger triger = SkillTrigerManager.Instance.CreateTriger(sectionData.Statements[i]);
                if (null != triger)
                {
                    m_LoadedTrigers.Add(triger);
                }
            }
            /*
            foreach (ScriptableData.ISyntaxComponent data in sectionData.Statements) {
              ISkillTriger triger = SkillTrigerManager.Instance.CreateTriger(data);
              if (null != triger) {
                m_LoadedTrigers.Add(triger);

                //LogSystem.Debug("AddTriger:{0}", triger.GetType().Name);
              }
            }*/
        }

        private long m_Duration = 0;
        private long m_CurTime = 0;
        private bool m_IsFinished = true;
        private List<ISkillTriger> m_Trigers = new List<ISkillTriger>();
        private List<ISkillTriger> m_LoadedTrigers = new List<ISkillTriger>();
    }
    public sealed class SkillMessageHandler
    {
        public string MsgId
        {
            get { return m_MsgId; }
        }
        public long CurTime
        {
            get { return m_CurTime; }
        }
        public bool IsTriggered
        {
            get { return m_IsTriggered; }
            set { m_IsTriggered = value; }
        }
        public SkillMessageHandler Clone()
        {
            SkillMessageHandler section = new SkillMessageHandler();
            for (int i = 0; i < m_LoadedTrigers.Count; i++)
            {
                section.m_LoadedTrigers.Add(m_LoadedTrigers[i].Clone());
            }
            /*
            foreach (ISkillTriger triger in m_LoadedTrigers) {
              section.m_LoadedTrigers.Add(triger.Clone());
            }*/
            section.m_MsgId = m_MsgId;
            return section;
        }
        public void Load(ScriptableData.FunctionData sectionData, int skillId)
        {
            ScriptableData.CallData callData = sectionData.Call;
            if (null != callData && callData.HaveParam())
            {
                string[] args = new string[callData.GetParamNum()];
                for (int i = 0; i < callData.GetParamNum(); ++i)
                {
                    args[i] = callData.GetParamId(i);
                }
                m_MsgId = string.Join(":", args);
            }
            RefreshTrigers(sectionData, skillId);
        }
        public void Reset()
        {
            m_IsTriggered = false;
            m_CurTime = 0;
            for (int i = 0; i < m_Trigers.Count; i++)
            {
                m_Trigers[i].Reset();
            }
            /*
            foreach (ISkillTriger triger in m_Trigers) {
              triger.Reset();
            }*/
            m_Trigers.Clear();
        }
        public void Prepare()
        {
            for (int i = 0; i < m_Trigers.Count; i++)
            {
                m_Trigers[i].Reset();
            }
            /*
            foreach (ISkillTriger triger in m_Trigers) {
              triger.Reset();
            }*/
            m_Trigers.Clear();
            m_CurTime = 0;
            for (int i = 0; i < m_LoadedTrigers.Count; i++)
            {
                m_Trigers.Add(m_LoadedTrigers[i]);
            }
            /*
            foreach (ISkillTriger triger in m_LoadedTrigers) {
              m_Trigers.Add(triger);
            }*/
            m_Trigers.Sort((left, right) =>
            {
                if (left.GetStartTime() > right.GetStartTime())
                {
                    return -11;
                }
                else if (left.GetStartTime() == right.GetStartTime())
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            });
        }
        public void Tick(object sender, SkillInstance instance, long delta)
        {
            m_CurTime += delta;
            int ct = m_Trigers.Count;
            for (int i = ct - 1; i >= 0; --i)
            {
                ISkillTriger triger = m_Trigers[i];
                if (!triger.Execute(sender, instance, delta, m_CurTime / 1000))
                {
                    triger.Reset();
                    m_Trigers.RemoveAt(i);
                    if (m_Trigers.Count == 0)
                    {
                        m_IsTriggered = false;
                    }
                }
            }
        }

        public bool IsOver()
        {
            return m_Trigers.Count <= 0 ? true : false;
        }

        public void Analyze(object sender, SkillInstance instance)
        {
            for (int i = 0; i < m_LoadedTrigers.Count; i++)
            {
                m_LoadedTrigers[i].Analyze(sender, instance);
            }
            /*
            foreach (ISkillTriger triger in m_LoadedTrigers) {
              triger.Analyze(sender, instance);
            }*/
        }

        private void RefreshTrigers(ScriptableData.FunctionData sectionData, int skillId)
        {
            m_LoadedTrigers.Clear();
            for (int i = 0; i < sectionData.Statements.Count; i++)
            {
                ISkillTriger triger = SkillTrigerManager.Instance.CreateTriger(sectionData.Statements[i]);
                if (null != triger)
                {
                    m_LoadedTrigers.Add(triger);
                }
            }
            /*
            foreach (ScriptableData.ISyntaxComponent data in sectionData.Statements) {
              ISkillTriger triger = SkillTrigerManager.Instance.CreateTriger(data);
              if (null != triger) {
                m_LoadedTrigers.Add(triger);
              }
            }*/
        }

        private string m_MsgId = "";
        private long m_CurTime = 0;
        private bool m_IsTriggered = false;
        private List<ISkillTriger> m_Trigers = new List<ISkillTriger>();
        private List<ISkillTriger> m_LoadedTrigers = new List<ISkillTriger>();
    }
    public sealed class SkillInstance
    {
        public int SkillId
        {
            get { return m_SkillId; }
        }
        public object Context
        {
            get { return m_Context; }
            set { m_Context = value; }
        }
        public bool IsInterrupted
        {
            get { return m_IsInterrupted; }
            set { m_IsInterrupted = value; }
        }
        public bool IsFinished
        {
            get { return m_IsFinished; }
            set { m_IsFinished = value; }
        }
        public bool IsControlMove
        {
            get { return m_IsControlMove; }
            set { m_IsControlMove = value; }
        }
        public bool IsCurveMoveEnable
        {
            get { return m_IsCurveMoveEnable; }
            set { m_IsCurveMoveEnable = value; }
        }
        public bool IsRotateEnable
        {
            get { return m_IsRotateEnable; }
            set { m_IsRotateEnable = value; }
        }
        public bool IsDamageEnable
        {
            get { return m_IsDamageEnable; }
            set { m_IsDamageEnable = value; }
        }
        public long CurTime
        {
            get { return m_CurTime / 1000; }
        }
        public long OrigDelta
        {
            get { return m_OrigDelta; }
        }
        public float TimeScale
        {
            get { return m_TimeScale; }
            set { m_TimeScale = value; }
        }
        public float EffectScale
        {
            get { return m_EffectScale; }
            set { m_EffectScale = value; }
        }
        public float MoveScale
        {
            get { return m_MoveScale; }
            set { m_MoveScale = value; }
        }
        public int GoToSection
        {
            get { return m_GoToSectionId; }
            set { m_GoToSectionId = value; }
        }
        public TypedDataCollection CustomDatas
        {
            get { return m_CustomDatas; }
        }
        //----------------------------------------------
        public bool AlreadyAnalyzed
        {
            get { return m_AlreadyAnalyzed; }
            set { m_AlreadyAnalyzed = value; }
        }
        //下面几个property是Analyze获取的数据
        public float CurMoveDistanceForFindTarget
        {
            get { return m_CurMoveDistanceForFindTarget; }
            set { m_CurMoveDistanceForFindTarget = value; }
        }
        public int StartWithoutStopMoveCount
        {
            get { return m_StartWithoutMoveCount; }
            set { m_StartWithoutMoveCount = value; }
        }
        public int StopMoveCount
        {
            get { return m_StopMoveCount; }
            set { m_StopMoveCount = value; }
        }
        //
        public int EnableMoveCount
        {
            get { return m_EnableMoveCount; }
            set { m_EnableMoveCount = value; }
        }
        public float MaxMoveDelta
        {
            get { return m_MaxMoveDelta; }
            set { m_MaxMoveDelta = value; }
        }
        public List<int> EnableImpactsToOther
        {
            get { return m_EnableImpactsToOther; }
        }
        public List<int> EnableImpactsToMyself
        {
            get { return m_EnableImpactsToMyself; }
        }
        public List<int> SummonNpcSkills
        {
            get { return m_SummonNpcSkills; }
        }
        public List<int> SummonNpcs
        {
            get { return m_SummonNpcs; }
        }
        public bool IsSimulate
        {
            get { return m_IsSimulate; }
            set { m_IsSimulate = value; }
        }
        public long MaxSkillLifeTime
        {
            get { return m_MaxSkillLifeTime; }
            set { m_MaxSkillLifeTime = value; }
        }
        public List<string> Resources
        {
            get { return m_Resources; }
        }

        //----------------------------------------------

        public SkillInstance Clone()
        {
            SkillInstance instance = new SkillInstance();
            for (int i = 0; i < m_Sections.Count; i++)
            {
                instance.m_Sections.Add(m_Sections[i].Clone());
            }
            /*
            foreach (SkillSection section in m_Sections) {
              instance.m_Sections.Add(section.Clone());
            }*/
            instance.m_IsInterrupted = false;
            instance.m_IsFinished = false;
            instance.m_IsCurveMoveEnable = true;
            instance.m_IsRotateEnable = true;
            instance.m_IsDamageEnable = true;
            instance.m_CurSection = 0;
            instance.m_CurSectionDuration = 0;
            instance.m_CurSectionTime = 0;
            instance.m_CurTime = 0;
            instance.m_GoToSectionId = -1;

            instance.m_SkillId = m_SkillId;
            if (m_StopSection != null)
            {
                instance.m_StopSection = m_StopSection.Clone();
            }
            if (m_InterruptSection != null)
            {
                instance.m_InterruptSection = m_InterruptSection.Clone();
            }
            for (int i = 0; i < m_MessageHandlers.Count; i++)
            {
                instance.m_MessageHandlers.Add(m_MessageHandlers[i].Clone());
            }
            /*
            foreach (SkillMessageHandler section in m_MessageHandlers) {
              instance.m_MessageHandlers.Add(section.Clone());
            }*/
            return instance;
        }

        public bool Init(ScriptableData.ScriptableDataInfo config)
        {
            bool ret = false;
            ScriptableData.FunctionData skill = config.First;
            if (null != skill && skill.GetId() == "skill")
            {
                ret = true;
                ScriptableData.CallData callData = skill.Call;
                if (null != callData && callData.HaveParam())
                {
                    m_SkillId = int.Parse(callData.GetParamId(0));
                }

                for (int i = 0; i < skill.Statements.Count; i++)
                {
                    if (skill.Statements[i].GetId() == "section")
                    {
                        ScriptableData.FunctionData sectionData = skill.Statements[i] as ScriptableData.FunctionData;
                        if (null != sectionData)
                        {
                            SkillSection section = new SkillSection();
                            section.Load(sectionData, m_SkillId);
                            m_Sections.Add(section);
                        }
                        else
                        {
                            LogSystem.Error("Skill {0} DSL, section must be a function !", m_SkillId);
                        }
                    }
                    else if (skill.Statements[i].GetId() == "onmessage")
                    {
                        ScriptableData.FunctionData sectionData = skill.Statements[i] as ScriptableData.FunctionData;
                        if (null != sectionData)
                        {
                            SkillMessageHandler handler = new SkillMessageHandler();
                            handler.Load(sectionData, m_SkillId);
                            m_MessageHandlers.Add(handler);
                        }
                        else
                        {
                            LogSystem.Error("Skill {0} DSL, onmessage must be a function !", m_SkillId);
                        }
                    }
                    else if (skill.Statements[i].GetId() == "onstop")
                    {
                        ScriptableData.FunctionData sectionData = skill.Statements[i] as ScriptableData.FunctionData;
                        if (null != sectionData)
                        {
                            m_StopSection = new SkillMessageHandler();
                            m_StopSection.Load(sectionData, m_SkillId);
                        }
                        else
                        {
                            LogSystem.Error("Skill {0} DSL, onstop must be a function !", m_SkillId);
                        }
                    }
                    else if (skill.Statements[i].GetId() == "oninterrupt")
                    {
                        ScriptableData.FunctionData sectionData = skill.Statements[i] as ScriptableData.FunctionData;
                        if (null != sectionData)
                        {
                            m_InterruptSection = new SkillMessageHandler();
                            m_InterruptSection.Load(sectionData, m_SkillId);
                        }
                        else
                        {
                            LogSystem.Error("Skill {0} DSL, oninterrupt must be a function !", m_SkillId);
                        }
                    }
                    else
                    {
                        LogSystem.Error("SkillInstance::Init, unknown part {0}", skill.Statements[i].GetId());
                    }
                }
                /*
                foreach (ScriptableData.ISyntaxComponent info in skill.Statements) {
                  if (info.GetId() == "section") {
                    ScriptableData.FunctionData sectionData = info as ScriptableData.FunctionData;
                    if (null != sectionData) {
                      SkillSection section = new SkillSection();
                      section.Load(sectionData, m_SkillId);
                      m_Sections.Add(section);
                    } else {
                      LogSystem.Error("Skill {0} DSL, section must be a function !", m_SkillId);
                    }
                  } else if (info.GetId() == "onmessage") {
                    ScriptableData.FunctionData sectionData = info as ScriptableData.FunctionData;
                    if (null != sectionData) {
                      SkillMessageHandler handler = new SkillMessageHandler();
                      handler.Load(sectionData, m_SkillId);
                      m_MessageHandlers.Add(handler);
                    } else {
                      LogSystem.Error("Skill {0} DSL, onmessage must be a function !", m_SkillId);
                    }
                  } else if (info.GetId() == "onstop") {
                    ScriptableData.FunctionData sectionData = info as ScriptableData.FunctionData;
                    if (null != sectionData) {
                      m_StopSection = new SkillMessageHandler();
                      m_StopSection.Load(sectionData, m_SkillId);
                    } else {
                      LogSystem.Error("Skill {0} DSL, onstop must be a function !", m_SkillId);
                    }
                  } else if (info.GetId() == "oninterrupt") {
                    ScriptableData.FunctionData sectionData = info as ScriptableData.FunctionData;
                    if (null != sectionData) {
                      m_InterruptSection = new SkillMessageHandler();
                      m_InterruptSection.Load(sectionData, m_SkillId);
                    } else {
                      LogSystem.Error("Skill {0} DSL, oninterrupt must be a function !", m_SkillId);
                    }
                  } else {
                    LogSystem.Error("SkillInstance::Init, unknown part {0}", info.GetId());
                  }
                }*/
            }
            else
            {
                LogSystem.Error("SkillInstance::Init, isn't skill DSL");
            }
            //LogSystem.Debug("SkillInstance.Init section num:{0} {1}", m_Sections.Count, ret);
            return ret;
        }

        public void Reset()
        {
            m_IsInterrupted = false;
            m_IsFinished = false;
            m_IsCurveMoveEnable = true;
            m_IsRotateEnable = true;
            m_IsDamageEnable = true;
            m_IsStopCurSection = false;
            m_TimeScale = 1;
            m_CurSection = -1;
            m_GoToSectionId = -1;

            int ct = m_Sections.Count;
            for (int i = ct - 1; i >= 0; --i)
            {
                SkillSection section = m_Sections[i];
                section.Reset();
            }
            m_CustomDatas.Clear();
            m_MessageQueue.Clear();
        }

        public void Start(object sender)
        {
            m_CurTime = 0;
            m_CurSection = -1;
            ChangeToSection(0);
        }

        public void SendMessage(string msgId)
        {
            m_MessageQueue.Enqueue(msgId);
        }

        public void Tick(object sender, long deltaTime)
        {
            if (m_IsFinished)
            {
                return;
            }
            m_OrigDelta = deltaTime;
            long delta = (long)(deltaTime * m_TimeScale);
            m_CurSectionTime += delta;
            m_CurTime += delta;
            TickMessageHandlers(sender, delta);
            if (!IsSectionDone(m_CurSection))
            {
                m_Sections[m_CurSection].Tick(sender, this, delta);
            }
            if (m_IsStopCurSection)
            {
                m_IsStopCurSection = false;
                ResetCurSection();
                ChangeToSection(m_CurSection + 1);
            }
            // do change section task
            if (m_GoToSectionId >= 0)
            {
                ResetCurSection();
                ChangeToSection(m_GoToSectionId);
                m_GoToSectionId = -1;
            }
            if (IsSectionDone(m_CurSection) && m_CurSection < m_Sections.Count - 1)
            {
                ResetCurSection();
                ChangeToSection(m_CurSection + 1);
            }
            if (IsMessageDone() && IsAllSectionDone())
            {
                OnSkillStop(sender, delta);
            }
        }

        private bool IsSectionDone(int sectionnum)
        {
            if (sectionnum >= 0 && sectionnum < m_Sections.Count)
            {
                SkillSection section = m_Sections[sectionnum];
                if (section.IsFinished)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool IsAllSectionDone()
        {
            if (IsSectionDone(m_CurSection) && m_CurSection == m_Sections.Count - 1)
            {
                return true;
            }
            return false;
        }

        public void StopCurSection()
        {
            m_IsStopCurSection = true;
        }

        public void Analyze(object sender)
        {
            m_CurMoveDistanceForFindTarget = 0;
            m_StartWithoutMoveCount = 0;
            m_StopMoveCount = 0;

            m_EnableMoveCount = 0;
            m_MaxMoveDelta = 0;
            m_EnableImpactsToOther.Clear();
            m_EnableImpactsToMyself.Clear();
            m_SummonNpcSkills.Clear();
            m_SummonNpcs.Clear();
            m_IsSimulate = false;
            m_MaxSkillLifeTime = 0;

            for (int i = 0; i < m_Sections.Count; i++)
            {
                m_Sections[i].Analyze(sender, this);
                m_MaxSkillLifeTime += m_Sections[i].Duration;
            }
            for (int i = 0; i < m_MessageHandlers.Count; i++)
            {
                m_MessageHandlers[i].Analyze(sender, this);
                m_MaxSkillLifeTime += 1000;
            }
            /*
            foreach (SkillSection section in m_Sections) {
              section.Analyze(sender, this);
            }
            foreach (SkillMessageHandler handler in m_MessageHandlers) {
              handler.Analyze(sender, this);
            }*/
            if (null != m_InterruptSection)
            {
                m_InterruptSection.Analyze(sender, this);
            }
            if (null != m_StopSection)
            {
                m_StopSection.Analyze(sender, this);
            }

            m_EnableMoveCount = m_StopMoveCount + (m_StartWithoutMoveCount > 0 ? 1 : 0);

            m_AlreadyAnalyzed = true;
        }

        public void OnInterrupt(object sender, long delta)
        {
            if (m_InterruptSection != null)
            {
                m_InterruptSection.Prepare();
                m_InterruptSection.Tick(sender, this, delta);
            }
            ResetCurSection();
            StopMessageHandlers();
            m_IsFinished = true;
        }

        public void OnSkillStop(object sender, long delta)
        {
            if (m_StopSection != null)
            {
                m_StopSection.Prepare();
                m_StopSection.Tick(sender, this, delta);
            }
            ResetCurSection();
            StopMessageHandlers();
            m_IsFinished = true;
        }

        public void ChangeToSection(int index)
        {
            if (index >= 0 && index < m_Sections.Count)
            {
                SkillSection section = m_Sections[index];
                m_CurSection = index;
                m_CurSectionDuration = section.Duration * 1000;
                m_CurSectionTime = 0;
                section.Prepare();

                //LogSystem.Debug("ChangeToSection:{0} duration:{1}", index, m_CurDuration);
            }
        }

        private void ResetCurSection()
        {
            if (m_CurSection >= 0 && m_CurSection < m_Sections.Count)
            {
                SkillSection section = m_Sections[m_CurSection];
                section.Reset();
            }
        }

        private void TickMessageHandlers(object sender, long delta)
        {
            if (m_MessageQueue.Count > 0)
            {
                int cantTriggerCount = 0;
                int triggerCount = 0;
                string msgId = m_MessageQueue.Peek();
                for (int i = 0; i < m_MessageHandlers.Count; i++)
                {
                    if (m_MessageHandlers[i].MsgId == msgId)
                    {
                        if (m_MessageHandlers[i].IsTriggered)
                        {
                            ++cantTriggerCount;
                        }
                        else
                        {
                            m_MessageHandlers[i].Prepare();
                            m_MessageHandlers[i].IsTriggered = true;
                            ++triggerCount;
                        }
                    }
                }
                /*
                foreach (SkillMessageHandler handler in m_MessageHandlers) {
                  if (handler.MsgId == msgId) {
                    if (handler.IsTriggered) {
                      ++cantTriggerCount;
                    } else {
                      handler.Prepare();
                      handler.IsTriggered = true;
                      ++triggerCount;
                    }
                  }
                }*/
                if (cantTriggerCount == 0 || triggerCount > 0)
                {
                    m_MessageQueue.Dequeue();
                }
            }
            for (int i = 0; i < m_MessageHandlers.Count; i++)
            {
                if (m_MessageHandlers[i].IsTriggered)
                {
                    m_MessageHandlers[i].Tick(sender, this, delta);
                    if (m_MessageHandlers[i].IsOver())
                    {
                        m_MessageHandlers[i].Reset();
                    }
                }
            }
            /*
            foreach (SkillMessageHandler handler in m_MessageHandlers) {
              if (handler.IsTriggered) {
                handler.Tick(sender, this, delta);
                if (handler.IsOver()) {
                  handler.Reset();
                }
              }
            }*/
        }

        public bool IsMessageDone()
        {
            for (int i = 0; i < m_MessageHandlers.Count; i++)
            {
                if (m_MessageHandlers[i].IsTriggered)
                {
                    return false;
                }
            }
            /*
            foreach (SkillMessageHandler handler in m_MessageHandlers) {
              if (handler.IsTriggered) {
                return false;
              }
            }*/
            return true;
        }

        private void StopMessageHandlers()
        {
            for (int i = 0; i < m_MessageHandlers.Count; i++)
            {
                if (m_MessageHandlers[i].IsTriggered)
                {
                    m_MessageHandlers[i].Reset();
                }
            }
            /*
            foreach (SkillMessageHandler handler in m_MessageHandlers) {
              if (handler.IsTriggered) {
                handler.Reset();
              }
            }*/
        }

        private bool m_IsInterrupted = false;
        private bool m_IsFinished = false;

        private bool m_IsControlMove = false;
        private bool m_IsCurveMoveEnable = true;
        private bool m_IsRotateEnable = true;
        private bool m_IsDamageEnable = false;
        private bool m_IsStopCurSection = false;

        private int m_CurSection = -1;
        private int m_GoToSectionId = -1;
        private long m_CurSectionDuration = 0;
        private long m_CurSectionTime = 0;
        private long m_CurTime = 0;
        private long m_OrigDelta = 0;
        private float m_TimeScale = 1;
        private float m_EffectScale = 1;
        private float m_MoveScale = 1;

        private int m_SkillId = 0;
        private object m_Context = null;
        private List<SkillSection> m_Sections = new List<SkillSection>();
        private Queue<string> m_MessageQueue = new Queue<string>();
        private List<SkillMessageHandler> m_MessageHandlers = new List<SkillMessageHandler>();
        private SkillMessageHandler m_StopSection = null;
        private SkillMessageHandler m_InterruptSection = null;

        private bool m_AlreadyAnalyzed = false;

        private float m_CurMoveDistanceForFindTarget = 0;
        private int m_StartWithoutMoveCount = 0;
        private int m_StopMoveCount = 0;

        private int m_EnableMoveCount = 0;
        private float m_MaxMoveDelta = 0;
        private List<int> m_EnableImpactsToOther = new List<int>();
        private List<int> m_EnableImpactsToMyself = new List<int>();
        private List<int> m_SummonNpcSkills = new List<int>();
        private List<int> m_SummonNpcs = new List<int>();
        private List<string> m_Resources = new List<string>();
        private bool m_IsSimulate = false;
        private long m_MaxSkillLifeTime = 0;

        private TypedDataCollection m_CustomDatas = new TypedDataCollection();
    }
}
