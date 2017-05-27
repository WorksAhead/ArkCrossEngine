using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StorySystem;
using DashFire;

namespace DashFire.GmCommands
{
  /// <summary>
  /// startscript(script_id);
  /// </summary>
  internal class StartScriptCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      StartScriptCommand cmd = new StartScriptCommand();
      cmd.m_StoryId = m_StoryId.Clone();
      return cmd;
    }

    protected override void ResetState()
    { }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_StoryId.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_StoryId.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      //ClientGmStorySystem.Instance.StartStory(m_StoryId.Value);
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 0) {
        m_StoryId.InitFromDsl(callData.GetParam(0));
      }
    }

    private IStoryValue<int> m_StoryId = new StoryValue<int>();
  }
  /// <summary>
  /// stopscript(script_id);
  /// </summary>
  internal class StopScriptCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      StopScriptCommand cmd = new StopScriptCommand();
      cmd.m_StoryId = m_StoryId.Clone();
      return cmd;
    }

    protected override void ResetState()
    { }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_StoryId.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_StoryId.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      //ClientGmStorySystem.Instance.StopStory(m_StoryId.Value);
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 0) {
        m_StoryId.InitFromDsl(callData.GetParam(0));
      }
    }

    private IStoryValue<int> m_StoryId = new StoryValue<int>();
  }
  /// <summary>
  /// firemessage(msgid,arg1,arg2,...);
  /// </summary>
  internal class FireMessageCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      FireMessageCommand cmd = new FireMessageCommand();
      cmd.m_MsgId = m_MsgId.Clone();
      foreach (IStoryValue<object> val in m_MsgArgs) {
        cmd.m_MsgArgs.Add(val.Clone());
      }
      return cmd;
    }

    protected override void ResetState()
    { }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_MsgId.Evaluate(iterator, args);
      foreach (StoryValue val in m_MsgArgs) {
        val.Evaluate(iterator, args);
      }
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_MsgId.Evaluate(instance);
      foreach (StoryValue val in m_MsgArgs) {
        val.Evaluate(instance);
      }
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      string msgId = m_MsgId.Value;
      ArrayList arglist = new ArrayList();
      foreach (StoryValue val in m_MsgArgs) {
        arglist.Add(val.Value);
      }
      object[] args = arglist.ToArray();
      //ClientGmStorySystem.Instance.SendMessage(msgId, args);
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 0) {
        m_MsgId.InitFromDsl(callData.GetParam(0));
      }
      for (int i = 1; i < callData.GetParamNum(); ++i) {
        StoryValue val = new StoryValue();
        val.InitFromDsl(callData.GetParam(i));
        m_MsgArgs.Add(val);
      }
    }

    private IStoryValue<string> m_MsgId = new StoryValue<string>();
    private List<IStoryValue<object>> m_MsgArgs = new List<IStoryValue<object>>();
  }
}
