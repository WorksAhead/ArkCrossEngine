using System.Collections.Generic;
using ArkCrossEngine;

namespace StorySystem
{
    /// <summary>
    /// story(1)
    /// {
    ///   onmessage(start)
    ///   {
    ///     dialog(1);
    ///   };
    ///   onmessage(enterarea, 1)
    ///   {
    ///     dialog(2);
    ///   };
    ///   onmessage(enddialog, 2)
    ///   {
    ///     createnpc(10,11,12);
    ///     movenpc(10,vector2(10,20));
    ///     aienable(10,11,12);
    ///   };
    ///   onmessage(killnpc,12)
    ///   {
    ///     missioncomplete();
    ///   };
    /// };
    /// </summary>
    public sealed class StoryMessageHandler
    {
        public string MessageId
        {
            get { return m_MessageId; }
        }
        public bool IsTriggered
        {
            get { return m_IsTriggered; }
            set { m_IsTriggered = value; }
        }
        public StoryMessageHandler Clone()
        {
            StoryMessageHandler handler = new StoryMessageHandler();
            for (int i = 0; i < m_LoadedCommands.Count; i++)
            {
                handler.m_LoadedCommands.Add(m_LoadedCommands[i].Clone());
            }
            /*
            foreach (IStoryCommand cmd in m_LoadedCommands) {
              handler.m_LoadedCommands.Add(cmd.Clone());
            }*/
            handler.m_MessageId = m_MessageId;
            return handler;
        }
        public void Load(ScriptableData.FunctionData messageHandlerData)
        {
            ScriptableData.CallData callData = messageHandlerData.Call;
            if (null != callData && callData.HaveParam())
            {
                string[] args = new string[callData.GetParamNum()];
                for (int i = 0; i < callData.GetParamNum(); ++i)
                {
                    args[i] = callData.GetParamId(i);
                }
                m_MessageId = string.Join(":", args);
            }
            RefreshCommands(messageHandlerData);
        }
        public void Reset()
        {
            m_IsTriggered = false;
            foreach (IStoryCommand cmd in m_CommandQueue)
            {
                cmd.Reset();
            }
            m_CommandQueue.Clear();
        }
        public void Prepare()
        {
            Reset();
            for (int i = 0; i < m_LoadedCommands.Count; i++)
            {
                m_CommandQueue.Enqueue(m_LoadedCommands[i]);
            }
            /*
            foreach (IStoryCommand cmd in m_LoadedCommands) {
              m_CommandQueue.Enqueue(cmd);
            }*/
        }
        public void Tick(StoryInstance instance, long delta)
        {
            while (m_CommandQueue.Count > 0)
            {
                IStoryCommand cmd = m_CommandQueue.Peek();
                if (cmd.Execute(instance, delta))
                {
                    break;
                }
                else
                {
                    cmd.Reset();
                    m_CommandQueue.Dequeue();
                }
            }
            if (m_CommandQueue.Count == 0)
            {
                m_IsTriggered = false;
            }
        }
        public void Analyze(StoryInstance instance)
        {
            for (int i = 0; i < m_LoadedCommands.Count; i++)
            {
                m_LoadedCommands[i].Analyze(instance);
            }
            /*
            foreach (IStoryCommand cmd in m_LoadedCommands) {
              cmd.Analyze(instance);
            }*/
        }
        public void Trigger(StoryInstance instance, object[] args)
        {
            Prepare();
            m_IsTriggered = true;
            m_Arguments = args;
            foreach (IStoryCommand cmd in m_CommandQueue)
            {
                cmd.Prepare(instance, args.Length, args);
            }
        }

        private void RefreshCommands(ScriptableData.FunctionData handlerData)
        {
            m_LoadedCommands.Clear();
            for (int i = 0; i < handlerData.Statements.Count; i++)
            {
                IStoryCommand cmd = StoryCommandManager.Instance.CreateCommand(handlerData.Statements[i]);
                if (null != cmd)
                {
                    m_LoadedCommands.Add(cmd);
                }
            }
            /*
            foreach (ScriptableData.ISyntaxComponent data in handlerData.Statements) {
              IStoryCommand cmd = StoryCommandManager.Instance.CreateCommand(data);
              if (null != cmd) {
                m_LoadedCommands.Add(cmd);

                //LogSystem.Debug("AddCommand:{0}", cmd.GetType().Name);
              }
            }*/
        }

        private string m_MessageId = "";
        private bool m_IsTriggered = false;
        private Queue<IStoryCommand> m_CommandQueue = new Queue<IStoryCommand>();

        private object[] m_Arguments = null;

        private List<IStoryCommand> m_LoadedCommands = new List<IStoryCommand>();
    }
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
        public Dictionary<string, object> LocalVariables
        {
            get { return m_LocalVariables; }
        }
        public Dictionary<string, object> GlobalVariables
        {
            get { return m_GlobalVariables; }
            set { m_GlobalVariables = value; }
        }
        public TypedDataCollection CustomDatas
        {
            get { return m_CustomDatas; }
        }
        public StoryInstance Clone()
        {
            StoryInstance instance = new StoryInstance();
            foreach (KeyValuePair<string, object> pair in m_PreInitedLocalVariables)
            {
                instance.m_PreInitedLocalVariables.Add(pair.Key, pair.Value);
            }
            for (int i = 0; i < m_MessageHandlers.Count; i++)
            {
                instance.m_MessageHandlers.Add(m_MessageHandlers[i].Clone());
                string msgId = m_MessageHandlers[i].MessageId;
                if (!instance.m_MessageQueues.ContainsKey(msgId))
                {
                    instance.m_MessageQueues.Add(msgId, new Queue<MessageInfo>());
                }
            }
            /*
            foreach (StoryMessageHandler handler in m_MessageHandlers) {
              instance.m_MessageHandlers.Add(handler.Clone());
            }*/
            instance.m_StoryId = m_StoryId;
            return instance;
        }
        public bool Init(ScriptableData.ScriptableDataInfo config)
        {
            bool ret = false;
            ScriptableData.FunctionData story = config.First;
            if (null != story && (story.GetId() == "story" || story.GetId() == "script"))
            {
                ret = true;
                ScriptableData.CallData callData = story.Call;
                if (null != callData && callData.HaveParam())
                {
                    m_StoryId = int.Parse(callData.GetParamId(0));
                }
                for (int i = 0; i < story.Statements.Count; i++)
                {
                    if (story.Statements[i].GetId() == "local")
                    {
                        ScriptableData.FunctionData sectionData = story.Statements[i] as ScriptableData.FunctionData;
                        if (null != sectionData)
                        {
                            for (int j = 0; j < sectionData.Statements.Count; j++)
                            {
                                ScriptableData.CallData defData = sectionData.Statements[j] as ScriptableData.CallData;
                                if (null != defData && defData.HaveId() && defData.HaveParam())
                                {
                                    string id = defData.GetId();
                                    if (id.StartsWith("@") && !id.StartsWith("@@"))
                                    {
                                        StoryValue val = new StoryValue();
                                        val.InitFromDsl(defData.GetParam(0));
                                        if (!m_PreInitedLocalVariables.ContainsKey(id))
                                        {
                                            m_PreInitedLocalVariables.Add(id, val.Value);
                                        }
                                        else
                                        {
                                            m_PreInitedLocalVariables[id] = val.Value;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            LogSystem.Error("Story {0} DSL, local must be a function !", m_StoryId);
                        }
                    }
                    else if (story.Statements[i].GetId() == "onmessage")
                    {
                        ScriptableData.FunctionData sectionData = story.Statements[i] as ScriptableData.FunctionData;
                        if (null != sectionData)
                        {
                            StoryMessageHandler handler = new StoryMessageHandler();
                            handler.Load(sectionData);
                            string msgId = handler.MessageId;
                            if (!m_MessageQueues.ContainsKey(msgId))
                            {
                                m_MessageHandlers.Add(handler);
                                m_MessageQueues.Add(msgId, new Queue<MessageInfo>());
                            }
                            else
                            {
                                LogSystem.Error("Story {0} DSL, onmessage {1} duplicate, discard it !", m_StoryId, msgId);
                            }
                        }
                        else
                        {
                            LogSystem.Error("Story {0} DSL, onmessage must be a function !", m_StoryId);
                        }
                    }
                    else
                    {
                        LogSystem.Error("StoryInstance::Init, unknown part {0}", story.Statements[i].GetId());
                    }
                }
                /*
                foreach (ScriptableData.ISyntaxComponent info in story.Statements) {
                  if (info.GetId() == "local") {
                    ScriptableData.FunctionData sectionData = info as ScriptableData.FunctionData;
                    if (null != sectionData) {
                      foreach (ScriptableData.ISyntaxComponent def in sectionData.Statements) {
                        ScriptableData.CallData defData = def as ScriptableData.CallData;
                        if (null != defData && defData.HaveId() && defData.HaveParam()) {
                          string id = defData.GetId();
                          if (id.StartsWith("@") && !id.StartsWith("@@")) {
                            StoryValue val = new StoryValue();
                            val.InitFromDsl(defData.GetParam(0));
                            if (!m_PreInitedLocalVariables.ContainsKey(id)) {
                              m_PreInitedLocalVariables.Add(id, val.Value);
                            } else {
                              m_PreInitedLocalVariables[id] = val.Value;
                            }
                          }
                        }
                      }
                    } else {
                      LogSystem.Error("Story {0} DSL, local must be a function !", m_StoryId);
                    }
                  } else if (info.GetId() == "onmessage") {
                    ScriptableData.FunctionData sectionData = info as ScriptableData.FunctionData;
                    if (null != sectionData) {
                      StoryMessageHandler handler = new StoryMessageHandler();
                      handler.Load(sectionData);
                      m_MessageHandlers.Add(handler);
                    } else {
                      LogSystem.Error("Story {0} DSL, onmessage must be a function !", m_StoryId);
                    }
                  } else {
                    LogSystem.Error("StoryInstance::Init, unknown part {0}", info.GetId());
                  }
                }*/
            }
            else
            {
                LogSystem.Error("StoryInstance::Init, isn't story DSL");
            }
            //LogSystem.Debug("StoryInstance.Init message handler num:{0} {1}", m_MessageHandlers.Count, ret);
            return ret;
        }
        public void Reset()
        {
            m_IsTerminated = false;
            int ct = m_MessageHandlers.Count;
            for (int i = ct - 1; i >= 0; --i)
            {
                StoryMessageHandler handler = m_MessageHandlers[i];
                handler.Reset();
                Queue<MessageInfo> queue;
                if (m_MessageQueues.TryGetValue(handler.MessageId, out queue))
                {
                    queue.Clear();
                }
            }
            m_LocalVariables.Clear();
            m_CustomDatas.Clear();
        }
        public void Start()
        {
            m_LastTickTime = 0;
            m_CurTime = 0;
            foreach (KeyValuePair<string, object> pair in m_PreInitedLocalVariables)
            {
                m_LocalVariables.Add(pair.Key, pair.Value);
            }
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
        public void ClearMessage(params string[] msgIds)
        {
            int len = msgIds.Length;
            if (len <= 0)
            {
                foreach (Queue<MessageInfo> queue in m_MessageQueues.Values)
                {
                    queue.Clear();
                }
            }
            else
            {
                for (int i = 0; i < len; ++i)
                {
                    string msgId = msgIds[i];
                    Queue<MessageInfo> queue;
                    if (m_MessageQueues.TryGetValue(msgId, out queue))
                    {
                        queue.Clear();
                    }
                }
            }
        }
        public void Tick(long curTime)
        {
            const int c_MaxMsgCountPerTick = 256;
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
            int ct = m_MessageHandlers.Count;
            for (int ix = ct - 1; ix >= 0; --ix)
            {
                long dt = delta;
                StoryMessageHandler handler = m_MessageHandlers[ix];
                if (handler.IsTriggered)
                {
                    handler.Tick(this, dt);
                    dt = 0;
                }
                if (!handler.IsTriggered)
                {
                    string msgId = handler.MessageId;
                    Queue<MessageInfo> queue;
                    if (m_MessageQueues.TryGetValue(msgId, out queue))
                    {
                        for (int msgCt = 0; msgCt < c_MaxMsgCountPerTick && queue.Count > 0 && !handler.IsTriggered; ++msgCt)
                        {
                            MessageInfo info = queue.Dequeue();
                            handler.Trigger(this, info.m_Args);
                            handler.Tick(this, dt);
                            dt = 0;
                        }
                    }
                }
            }
        }
        public void Analyze()
        {
            for (int i = 0; i < m_MessageHandlers.Count; i++)
            {
                m_MessageHandlers[i].Analyze(this);
            }
            /*
            foreach (StoryMessageHandler handler in m_MessageHandlers) {
              handler.Analyze(this);
            }*/
        }

        private class MessageInfo
        {
            public string m_MsgId = null;
            public object[] m_Args = null;
        }

        private long m_CurTime = 0;
        private long m_LastTickTime = 0;

        private Dictionary<string, object> m_LocalVariables = new Dictionary<string, object>();
        private Dictionary<string, object> m_GlobalVariables = null;

        private int m_StoryId = 0;
        private bool m_IsTerminated = false;
        private object m_Context = null;
        private Dictionary<string, Queue<MessageInfo>> m_MessageQueues = new Dictionary<string, Queue<MessageInfo>>();
        private List<StoryMessageHandler> m_MessageHandlers = new List<StoryMessageHandler>();
        private Dictionary<string, object> m_PreInitedLocalVariables = new Dictionary<string, object>();

        private TypedDataCollection m_CustomDatas = new TypedDataCollection();
    }
}
