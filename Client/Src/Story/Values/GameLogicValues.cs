using StorySystem;

namespace ArkCrossEngine.Story.Values
{
    internal sealed class IsLobbyConnectedValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "islobbyconnected")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            IsLobbyConnectedValue val = new IsLobbyConnectedValue();
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
            m_Value = Network.LobbyNetworkSystem.Instance.IsConnected;
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class IsLobbyLoginingValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "islobbylogining")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            IsLobbyLoginingValue val = new IsLobbyLoginingValue();
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
            m_Value = Network.LobbyNetworkSystem.Instance.IsLogining;
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class HasLobbyLoggedOnValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "islobbylogged")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            HasLobbyLoggedOnValue val = new HasLobbyLoggedOnValue();
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
            m_Value = Network.LobbyNetworkSystem.Instance.HasLoggedOn;
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
}
