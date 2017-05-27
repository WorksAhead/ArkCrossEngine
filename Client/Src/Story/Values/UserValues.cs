﻿using System.Collections.Generic;
using StorySystem;

namespace ArkCrossEngine.Story.Values
{
    internal sealed class UserIdListValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "useridlist")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            UserIdListValue val = new UserIdListValue();
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
            List<object> users = new List<object>();
            WorldSystem.Instance.UserManager.Users.VisitValues((UserInfo userInfo) =>
            {
                users.Add(userInfo.GetId());
            });
            m_HaveValue = true;
            m_Value = users;
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class PlayerselfIdValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "playerselfid")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            PlayerselfIdValue val = new PlayerselfIdValue();
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
            m_Value = WorldSystem.Instance.PlayerSelfId;
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class WinUserIdValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "winuserid")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            WinUserIdValue val = new WinUserIdValue();
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
            m_Value = WorldSystem.Instance.PlayerSelfId;
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class LostUserIdValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "lostuserid")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            LostUserIdValue val = new LostUserIdValue();
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
            m_Value = 0;
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class LivingUserCountValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "livingusercount")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            LivingUserCountValue val = new LivingUserCountValue();
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
            m_Value = WorldSystem.Instance.GetLivingUserCount();
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
    internal sealed class IsPlayerselfBusyValue : IStoryValue<object>
    {
        public void InitFromDsl(ScriptableData.ISyntaxComponent param)
        {
            ScriptableData.CallData callData = param as ScriptableData.CallData;
            if (null != callData && callData.GetId() == "isplayerselfbusy")
            {
            }
        }
        public IStoryValue<object> Clone()
        {
            IsPlayerselfBusyValue val = new IsPlayerselfBusyValue();
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
            UserInfo userInfo = WorldSystem.Instance.GetPlayerSelf();
            if (null != userInfo)
            {
                m_Value = userInfo.GetSkillStateInfo().IsSkillActivated() || userInfo.GetSkillStateInfo().IsImpactActive();
            }
            else
            {
                m_Value = WorldSystem.Instance.PlayerSelfId;
            }
        }

        private object m_Iterator = null;
        private object[] m_Args = null;

        private bool m_HaveValue;
        private object m_Value;
    }
}
