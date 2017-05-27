using System;
using System.Collections;
using System.Collections.Generic;
using StorySystem;
using ArkCrossEngineMessage;

namespace DashFire.Story.Commands
{
  /// <summary>
  /// publishlogicevent(ev_name,group,arg1,arg2,...);
  /// </summary>
  internal class PublishLogicEventCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      PublishLogicEventCommand cmd = new PublishLogicEventCommand();
      cmd.m_EventName = m_EventName.Clone();
      cmd.m_Group = m_Group.Clone();
      foreach (StoryValue val in m_Args) {
        cmd.m_Args.Add(val.Clone());
      }
      return cmd;
    }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_EventName.Evaluate(iterator, args);
      m_Group.Evaluate(iterator, args);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(iterator, args);
      }
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_EventName.Evaluate(instance);
      m_Group.Evaluate(instance);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(instance);
      }
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        ArkCrossEngineMessage.Msg_RC_PublishEvent msg = new ArkCrossEngineMessage.Msg_RC_PublishEvent();
        msg.is_logic_event = true;
        msg.ev_name = m_EventName.Value;
        msg.group = m_Group.Value;
        foreach (StoryValue val in m_Args) {
          ArkCrossEngineMessage.Msg_RC_PublishEvent.EventArg arg = new Msg_RC_PublishEvent.EventArg();
          object v = val.Value;
          if (null != v) {
            if (v is int) {
              arg.val_type = 1;
            } else if (v is float) {
              arg.val_type = 2;
            } else {
              arg.val_type = 3;
            }
            arg.str_val = v.ToString();
          } else {
            arg.val_type = 0;
            arg.str_val = "";
          }
          msg.args.Add(arg);
        }
        scene.NotifyAllUser(msg);
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 1) {
        m_EventName.InitFromDsl(callData.GetParam(0));
        m_Group.InitFromDsl(callData.GetParam(1));
      }
      for (int i = 2; i < callData.GetParamNum(); ++i) {
        StoryValue val = new StoryValue();
        val.InitFromDsl(callData.GetParam(i));
        m_Args.Add(val);
      }
    }

    private IStoryValue<string> m_EventName = new StoryValue<string>();
    private IStoryValue<string> m_Group = new StoryValue<string>();
    private List<IStoryValue<object>> m_Args = new List<IStoryValue<object>>();
  }
  /// <summary>
  /// publishgfxevent(ev_name,group,arg1,arg2,...);
  /// </summary>
  internal class PublishGfxEventCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      PublishGfxEventCommand cmd = new PublishGfxEventCommand();
      cmd.m_EventName = m_EventName.Clone();
      cmd.m_Group = m_Group.Clone();
      foreach (StoryValue val in m_Args) {
        cmd.m_Args.Add(val.Clone());
      }
      return cmd;
    }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_EventName.Evaluate(iterator, args);
      m_Group.Evaluate(iterator, args);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(iterator, args);
      }
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_EventName.Evaluate(instance);
      m_Group.Evaluate(instance);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(instance);
      }
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        ArkCrossEngineMessage.Msg_RC_PublishEvent msg = new ArkCrossEngineMessage.Msg_RC_PublishEvent();
        msg.is_logic_event = false;
        msg.ev_name = m_EventName.Value;
        msg.group = m_Group.Value;
        foreach (StoryValue val in m_Args) {
          ArkCrossEngineMessage.Msg_RC_PublishEvent.EventArg arg = new Msg_RC_PublishEvent.EventArg();
          object v = val.Value;
          if (null != v) {
            if (v is int) {
              arg.val_type = 1;
            } else if (v is float) {
              arg.val_type = 2;
            } else {
              arg.val_type = 3;
            }
            arg.str_val = v.ToString();
          } else {
            arg.val_type = 0;
            arg.str_val = "";
          }
          msg.args.Add(arg);
        }
        scene.NotifyAllUser(msg);
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 1) {
        m_EventName.InitFromDsl(callData.GetParam(0));
        m_Group.InitFromDsl(callData.GetParam(1));
      }
      for (int i = 2; i < callData.GetParamNum(); ++i) {
        StoryValue val = new StoryValue();
        val.InitFromDsl(callData.GetParam(i));
        m_Args.Add(val);
      }
    }

    private IStoryValue<string> m_EventName = new StoryValue<string>();
    private IStoryValue<string> m_Group = new StoryValue<string>();
    private List<IStoryValue<object>> m_Args = new List<IStoryValue<object>>();
  }
  /// <summary>
  /// sendgfxmessage(objname,msg,arg1,arg2,...);
  /// </summary>
  internal class SendGfxMessageCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      SendGfxMessageCommand cmd = new SendGfxMessageCommand();
      cmd.m_ObjName = m_ObjName.Clone();
      cmd.m_Msg = m_Msg.Clone();
      foreach (StoryValue val in m_Args) {
        cmd.m_Args.Add(val.Clone());
      }
      return cmd;
    }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_ObjName.Evaluate(iterator, args);
      m_Msg.Evaluate(iterator, args);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(iterator, args);
      }
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_ObjName.Evaluate(instance);
      m_Msg.Evaluate(instance);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(instance);
      }
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        ArkCrossEngineMessage.Msg_RC_SendGfxMessage msg = new ArkCrossEngineMessage.Msg_RC_SendGfxMessage();
        msg.is_with_tag= false;
        msg.name = m_ObjName.Value;
        msg.msg = m_Msg.Value;
        foreach (StoryValue val in m_Args) {
          ArkCrossEngineMessage.Msg_RC_SendGfxMessage.EventArg arg = new Msg_RC_SendGfxMessage.EventArg();
          object v = val.Value;
          if (null != v) {
            if (v is int) {
              arg.val_type = 1;
            } else if (v is float) {
              arg.val_type = 2;
            } else {
              arg.val_type = 3;
            }
            arg.str_val = v.ToString();
          } else {
            arg.val_type = 0;
            arg.str_val = "";
          }
          msg.args.Add(arg);
        }
        scene.NotifyAllUser(msg);
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 1) {
        m_ObjName.InitFromDsl(callData.GetParam(0));
        m_Msg.InitFromDsl(callData.GetParam(1));
      }
      for (int i = 2; i < callData.GetParamNum(); ++i) {
        StoryValue val = new StoryValue();
        val.InitFromDsl(callData.GetParam(i));
        m_Args.Add(val);
      }
    }

    private IStoryValue<string> m_ObjName = new StoryValue<string>();
    private IStoryValue<string> m_Msg = new StoryValue<string>();
    private List<IStoryValue<object>> m_Args = new List<IStoryValue<object>>();
  }
  /// <summary>
  /// sendgfxmessagewithtag(tagname,msg,arg1,arg2,...);
  /// </summary>
  internal class SendGfxMessageWithTagCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      SendGfxMessageWithTagCommand cmd = new SendGfxMessageWithTagCommand();
      cmd.m_ObjTag = m_ObjTag.Clone();
      cmd.m_Msg = m_Msg.Clone();
      foreach (StoryValue val in m_Args) {
        cmd.m_Args.Add(val.Clone());
      }
      return cmd;
    }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_ObjTag.Evaluate(iterator, args);
      m_Msg.Evaluate(iterator, args);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(iterator, args);
      }
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_ObjTag.Evaluate(instance);
      m_Msg.Evaluate(instance);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(instance);
      }
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        ArkCrossEngineMessage.Msg_RC_SendGfxMessage msg = new ArkCrossEngineMessage.Msg_RC_SendGfxMessage();
        msg.is_with_tag = true;
        msg.name = m_ObjTag.Value;
        msg.msg = m_Msg.Value;
        foreach (StoryValue val in m_Args) {
          ArkCrossEngineMessage.Msg_RC_SendGfxMessage.EventArg arg = new Msg_RC_SendGfxMessage.EventArg();
          object v = val.Value;
          if (null != v) {
            if (v is int) {
              arg.val_type = 1;
            } else if (v is float) {
              arg.val_type = 2;
            } else {
              arg.val_type = 3;
            }
            arg.str_val = v.ToString();
          } else {
            arg.val_type = 0;
            arg.str_val = "";
          }
          msg.args.Add(arg);
        }
        scene.NotifyAllUser(msg);
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 1) {
        m_ObjTag.InitFromDsl(callData.GetParam(0));
        m_Msg.InitFromDsl(callData.GetParam(1));
      }
      for (int i = 2; i < callData.GetParamNum(); ++i) {
        StoryValue val = new StoryValue();
        val.InitFromDsl(callData.GetParam(i));
        m_Args.Add(val);
      }
    }

    private IStoryValue<string> m_ObjTag = new StoryValue<string>();
    private IStoryValue<string> m_Msg = new StoryValue<string>();
    private List<IStoryValue<object>> m_Args = new List<IStoryValue<object>>();
  }
  /// <summary>
  /// sendgfxmessagebyid(objid,msg,arg1,arg2,...);
  /// </summary>
  internal class SendGfxMessageByIdCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      SendGfxMessageByIdCommand cmd = new SendGfxMessageByIdCommand();
      cmd.m_ObjId = m_ObjId.Clone();
      cmd.m_Msg = m_Msg.Clone();
      foreach (StoryValue val in m_Args) {
        cmd.m_Args.Add(val.Clone());
      }
      return cmd;
    }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_ObjId.Evaluate(iterator, args);
      m_Msg.Evaluate(iterator, args);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(iterator, args);
      }
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_ObjId.Evaluate(instance);
      m_Msg.Evaluate(instance);
      foreach (StoryValue val in m_Args) {
        val.Evaluate(instance);
      }
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        ArkCrossEngineMessage.Msg_RC_SendGfxMessageById msg = new ArkCrossEngineMessage.Msg_RC_SendGfxMessageById();
        msg.obj_id = m_ObjId.Value;
        msg.msg = m_Msg.Value;
        foreach (StoryValue val in m_Args) {
          ArkCrossEngineMessage.Msg_RC_SendGfxMessageById.EventArg arg = new Msg_RC_SendGfxMessageById.EventArg();
          object v = val.Value;
          if (null != v) {
            if (v is int) {
              arg.val_type = 1;
            } else if (v is float) {
              arg.val_type = 2;
            } else {
              arg.val_type = 3;
            }
            arg.str_val = v.ToString();
          } else {
            arg.val_type = 0;
            arg.str_val = "";
          }
          msg.args.Add(arg);
        }
        scene.NotifyAllUser(msg);
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 1) {
        m_ObjId.InitFromDsl(callData.GetParam(0));
        m_Msg.InitFromDsl(callData.GetParam(1));
      }
      for (int i = 2; i < callData.GetParamNum(); ++i) {
        StoryValue val = new StoryValue();
        val.InitFromDsl(callData.GetParam(i));
        m_Args.Add(val);
      }
    }

    private IStoryValue<int> m_ObjId = new StoryValue<int>();
    private IStoryValue<string> m_Msg = new StoryValue<string>();
    private List<IStoryValue<object>> m_Args = new List<IStoryValue<object>>();
  }
}
