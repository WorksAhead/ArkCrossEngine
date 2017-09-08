using System;
using System.Collections.Generic;
using StorySystem;
using DashFire;
using ArkCrossEngine;

namespace DashFire.Story.Values
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
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                List<object> npcs = new List<object>();
                scene.NpcManager.Npcs.VisitValues((NpcInfo npcInfo) =>
                {
                    npcs.Add(npcInfo.GetId());
                });
                m_HaveValue = true;
                m_Value = npcs;
            }
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
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                m_HaveValue = true;
                m_Value = scene.GetBattleNpcCount();
            }
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
            Scene scene = instance.Context as Scene;
            if (null != scene)
            {
                int objId = m_ObjId.Value;
                m_HaveValue = true;
                NpcInfo npc = scene.SceneContext.GetCharacterInfoById(objId) as NpcInfo;
                if (null != npc)
                {
                    m_Value = (npc.IsCombatNpc() ? 1 : 0);
                }
            }
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private IStoryValue<int> m_ObjId = new StoryValue<int>();
        private bool m_HaveValue;
        private object m_Value;
    }
}
