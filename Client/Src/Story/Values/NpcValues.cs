using System.Collections.Generic;
using StorySystem;

namespace ArkCrossEngine.Story.Values
{
    internal sealed class NpcIdListValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "npcidlist")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            NpcIdListValue val = new NpcIdListValue();
            val.m_HaveValue = m_HaveValue;
            val.m_Value = m_Value;
            return val;
        }
        public void Evaluate(object iterator, object[] args)
        {
            m_Iterator = iterator;
            m_Args = args;
        }
        public void Evaluate(StoryInstance instance)
        {
            TryUpdateValue(instance);
        }
        public void Analyze(StoryInstance instance)
        {
        }
        public bool HaveValue
        {
            get
            {
                return m_HaveValue;
            }
        }
        public object Value
        {
            get
            {
                return m_Value;
            }
        }

        private void TryUpdateValue(StoryInstance instance)
        {
            List<object> npcs = new List<object>();
            WorldSystem.Instance.NpcManager.Npcs.VisitValues((NpcInfo npcInfo) =>
            {
                npcs.Add(npcInfo.GetId());
            });
            m_HaveValue = true;
            m_Value = npcs;
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class CombatNpcCountValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "combatnpccount")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            CombatNpcCountValue val = new CombatNpcCountValue();
            val.m_HaveValue = m_HaveValue;
            val.m_Value = m_Value;
            return val;
        }
        public void Evaluate(object iterator, object[] args)
        {
            m_Iterator = iterator;
            m_Args = args;
        }
        public void Evaluate(StoryInstance instance)
        {
            TryUpdateValue(instance);
        }
        public void Analyze(StoryInstance instance)
        {
        }
        public bool HaveValue
        {
            get
            {
                return m_HaveValue;
            }
        }
        public object Value
        {
            get
            {
                return m_Value;
            }
        }

        private void TryUpdateValue(StoryInstance instance)
        {
            m_HaveValue = true;
            m_Value = WorldSystem.Instance.GetBattleNpcCount();
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class IsCombatNpcValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "iscombatnpc" && 1 == callData.GetParamNum())
            {
                m_ObjId.InitFromDsl(callData.GetParam(0));
            }
        }
        public IStoryValue<object> Clone()
        {
            IsCombatNpcValue val = new IsCombatNpcValue();
            val.m_ObjId = m_ObjId.Clone();
            val.m_HaveValue = m_HaveValue;
            val.m_Value = m_Value;
            return val;
        }
        public void Evaluate(object iterator, object[] args)
        {
            m_Iterator = iterator;
            m_Args = args;
            m_ObjId.Evaluate(iterator, args);
        }
        public void Evaluate(StoryInstance instance)
        {
            m_ObjId.Evaluate(instance);
            TryUpdateValue(instance);
        }
        public void Analyze(StoryInstance instance)
        {
            m_ObjId.Analyze(instance);
        }
        public bool HaveValue
        {
            get
            {
                return m_HaveValue;
            }
        }
        public object Value
        {
            get
            {
                return m_Value;
            }
        }

        private void TryUpdateValue(StoryInstance instance)
        {
            int objId = m_ObjId.Value;
            m_HaveValue = true;
            NpcInfo npc = WorldSystem.Instance.GetCharacterById(objId) as NpcInfo;
            if (null != npc)
            {
                m_Value = (npc.IsCombatNpc() ? 1 : 0);
            }
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private bool m_HaveValue;
        private object m_Value;
    }
}
