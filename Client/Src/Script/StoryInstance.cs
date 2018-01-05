using System;
using System.Collections.Generic;
using ArkCrossEngine;

namespace StorySystem
{
    public sealed class StoryInstance
    {
        public int StoryId
        {
            get { return m_StoryId; }
        }
        public bool IsTerminated
        {
            get { return m_IsTerminated; }
            set { m_IsTerminated = value; }
        }
        public object Context
        {
            get { return m_Context; }
            set { m_Context = value; }
        }
        public StoryInstance Clone()
        {
            StoryInstance instance = new StoryInstance();
            
            foreach( var v in m_ActionDatas )
            {
                instance.m_ActionDatas.Add(v.Key, v.Value);
            }
            foreach ( var v in m_MessageQueues )
            {
                if ( !instance.m_MessageQueues.ContainsKey(v.Key) )
                {
                    instance.m_MessageQueues.Add(v.Key, new Queue<MessageInfo>());
                }
            }
            instance.m_StoryId = m_StoryId;
            return instance;
        }
        private void AddMessageHandler(string msg, Action act)
        {
            if (act != null)
            {
                m_ActionDatas.Add(msg, act);
                if ( !m_MessageQueues.ContainsKey(msg) )
                {
                    m_MessageQueues.Add(msg, new Queue<MessageInfo>());
                }
            }
        }
        private void AddMessageHandler ( string msg, Action<object> act )
        {
            if ( act != null )
            {
                m_ActionDatas.Add(msg, act);
                if ( !m_MessageQueues.ContainsKey(msg) )
                {
                    m_MessageQueues.Add(msg, new Queue<MessageInfo>());
                }
            }
        }
        public bool Init( ScriptableData.ScriptableDataFile config )
        {
            AddMessageHandler("start", config.OnStart);
            AddMessageHandler("npcstore", config.OnNpcStore);
            AddMessageHandler("cityusermove", config.OnCityUserMove);
            AddMessageHandler("objarrived", config.OnObjArrived);
            AddMessageHandler("cityplayermove", config.OnCityPlayerMove);
            AddMessageHandler("playermovetopos", config.OnPlayerMoveToPos);
            AddMessageHandler("aimovestopped", config.OnAiMoveStop);
            
            return true;
        }
        public void Start()
        {
            m_LastTickTime = 0;
            m_CurTime = 0;
            SendMessage("start");
        }
        public void SendMessage(string msgId, params object[] args)
        {
            MessageInfo msgInfo = new MessageInfo();
            msgInfo.m_MsgId = msgId;
            msgInfo.m_Args = args;
            Queue<MessageInfo> queue;
            if (m_MessageQueues.TryGetValue(msgId, out queue))
            {
                queue.Enqueue(msgInfo);
            }
            else
            {
                //忽略没有处理的消息
            }
        }

        public void Tick(long curTime)
        {
            long delta = 0;
            if (m_LastTickTime == 0)
            {
                m_LastTickTime = curTime;
            }
            else
            {
                delta = curTime - m_LastTickTime;
                m_LastTickTime = curTime;
                m_CurTime += delta;
            }

            foreach( var msg in m_MessageQueues )
            {
                if (msg.Value.Count > 0)
                {
                    MessageInfo info = msg.Value.Dequeue();
                    object action;
                    if (m_ActionDatas.TryGetValue(info.m_MsgId, out action))
                    {
                        Action<object> act = action as Action<object>;
                        if (act != null)
                        {
                            act(info.m_Args);
                        }
                    }
                }
            }
        }

        private class MessageInfo
        {
            public string m_MsgId = null;
            public object[] m_Args = null;
        }

        private long m_CurTime = 0;
        private long m_LastTickTime = 0;

        private int m_StoryId = 0;
        private bool m_IsTerminated = false;
        private object m_Context = null;
        private Dictionary<string, Queue<MessageInfo>> m_MessageQueues = new Dictionary<string, Queue<MessageInfo>>();
        private Dictionary<string, object> m_ActionDatas = new Dictionary<string, object>();
    }
}
