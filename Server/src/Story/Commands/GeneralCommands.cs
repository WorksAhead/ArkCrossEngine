using System;
using System.Collections;
using System.Collections.Generic;
using StorySystem;
using ArkCrossEngineMessage;
using DashFire;
using ArkCrossEngine;

namespace DashFire.Story.Commands
{
  /// <summary>
  /// startstory(story_id);
  /// </summary>
  internal class StartStoryCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      StartStoryCommand cmd = new StartStoryCommand();
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
      Scene scene = instance.Context as Scene;
      if(null!=scene){
        scene.StorySystem.StartStory(m_StoryId.Value);
      }
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
  /// stopstory(story_id);
  /// </summary>
  internal class StopStoryCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      StopStoryCommand cmd = new StopStoryCommand();
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
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        scene.StorySystem.StopStory(m_StoryId.Value);
      }
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
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        scene.StorySystem.SendMessage(msgId, args);
      }
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
  /// <summary>
  /// missioncomplete(main_scene_id);
  /// </summary>
  internal class MissionCompletedCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      MissionCompletedCommand cmd = new MissionCompletedCommand();
      cmd.m_MainSceneId = m_MainSceneId.Clone();
      return cmd;
    }

    protected override void ResetState()
    { }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_MainSceneId.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_MainSceneId.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        ArkCrossEngineMessage.Msg_RC_MissionCompleted msg = new ArkCrossEngineMessage.Msg_RC_MissionCompleted();
        msg.main_scene_id = m_MainSceneId.Value;
        scene.NotifyAllUser(msg);
        if (scene.IsPvpScene) {
          VictoryJudgment(scene);
        } else if (scene.SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_ATTEMPT) {
          AttemptVictoryJudgment(scene);
        } else if (scene.SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_GOLD) {
          GoldVictoryJudgment(scene);
        } else {
          //延迟一帧结束战斗以保证网络消息发出
          Room room = scene.GetRoom();
          scene.DelayActionProcessor.QueueAction(room.EndBattle, (int)CampIdEnum.Blue);
        }
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 0) {
        m_MainSceneId.InitFromDsl(callData.GetParam(0));
      }
    }

    private void VictoryJudgment(Scene scene)
    {
      Room room = scene.GetRoom();
      UserInfo one = null, two = null;
      LinkedListNode<UserInfo> node = scene.UserManager.Users.FirstValue;
      if (null != node) {
        one = node.Value;
        node = node.Next;
        if (null != node) {
          two = node.Value;
        }
      }
      int winCampId = 0;
      if (null != one) {
        if (null != two) {
          float maxHpOne = one.GetActualProperty().HpMax;
          float maxHpTwo = two.GetActualProperty().HpMax;
          if (one.Hp / maxHpOne >= two.Hp / maxHpTwo) {
            winCampId = one.GetCampId();
          } else {
            winCampId = two.GetCampId();
          }
        } else {
          winCampId = one.GetCampId();
        }
      } else if (null != two) {
        winCampId = two.GetCampId();
      } else {
        winCampId = one.GetCampId();
      }
      //延迟一帧结束战斗以保证网络消息发出
      scene.DelayActionProcessor.QueueAction(room.EndBattle, winCampId);
    }

    private void AttemptVictoryJudgment(Scene scene)
    {
      Room room = scene.GetRoom();
      int kill_npc_ct = 0;
      for (LinkedListNode<UserInfo> node = scene.UserManager.Users.FirstValue; null != node; ) {
        UserInfo user = node.Value;
        if (null != user) {
          kill_npc_ct += user.GetCombatStatisticInfo().KillNpcCount;
        }
        node = node.Next;
      }
      if (kill_npc_ct > 0)
        //延迟一帧结束战斗以保证网络消息发出
        scene.DelayActionProcessor.QueueAction(room.MpveEndBattle, kill_npc_ct > 5 ? 5 : kill_npc_ct);
    }

    private void GoldVictoryJudgment(Scene scene)
    {
      Room room = scene.GetRoom();
      int kill_npc_ct = 1;
      scene.DelayActionProcessor.QueueAction(room.MpveEndBattle, kill_npc_ct);
    }

    private IStoryValue<int> m_MainSceneId = new StoryValue<int>();
  }
  /// <summary>
  /// changescene(main_scene_id);
  /// </summary>
  internal class ChangeSceneCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      ChangeSceneCommand cmd = new ChangeSceneCommand();
      cmd.m_MainSceneId = m_MainSceneId.Clone();
      return cmd;
    }

    protected override void ResetState()
    { }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_MainSceneId.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_MainSceneId.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        ArkCrossEngineMessage.Msg_RC_ChangeScene msg = new ArkCrossEngineMessage.Msg_RC_ChangeScene();
        msg.main_scene_id = m_MainSceneId.Value;
        scene.NotifyAllUser(msg);
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 0) {
        m_MainSceneId.InitFromDsl(callData.GetParam(0));
      }
    }

    private IStoryValue<int> m_MainSceneId = new StoryValue<int>();
  }
  /// <summary>
  /// updatecoefficient([objid]);
  /// </summary>
  internal class UpdateCoefficientCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      UpdateCoefficientCommand cmd = new UpdateCoefficientCommand();
      cmd.m_HaveParam = m_HaveParam;
      cmd.m_ObjId = m_ObjId.Clone();
      return cmd;
    }

    protected override void ResetState()
    {
    }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_ObjId.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_ObjId.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        if (m_HaveParam) {
          int objId = m_ObjId.Value;
          UserInfo user = scene.UserManager.GetUserInfo(objId);
          if (null != user) {
            Msg_RC_UpdateCoefficient msg = new Msg_RC_UpdateCoefficient();
            msg.obj_id = user.GetId();
            msg.hpmax_coefficient = user.HpMaxCoefficient;
            scene.NotifyAllUser(msg);

            Msg_RC_SyncProperty propMsg = DataSyncUtility.BuildSyncPropertyMessage(user);
            scene.NotifyAllUser(propMsg);

            for (LinkedListNode<UserInfo> linkNode = scene.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
              UserInfo info = linkNode.Value;
              User reporter = user.CustomData as User;
              if (null != reporter && null != info && objId != info.GetId()) {
                Msg_RC_UpdateCoefficient factorMsg = new Msg_RC_UpdateCoefficient();
                factorMsg.obj_id = info.GetId();
                factorMsg.hpmax_coefficient = info.HpMaxCoefficient;
                reporter.SendMessage(factorMsg);

                Msg_RC_SyncProperty otherPropMsg = DataSyncUtility.BuildSyncPropertyMessage(info);
                reporter.SendMessage(otherPropMsg);
              }
            }
          }
        } else {
          Room room = scene.GetRoom();
          UserInfo one = null, two = null;
          LinkedListNode<UserInfo> node = scene.UserManager.Users.FirstValue;
          if (null != node) {
            one = node.Value;
            node = node.Next;
            if (null != node) {
              two = node.Value;
            }
          }
          if (null != one && null != two) {
            float lvl1 = one.GetLevel();
            float lvl2 = two.GetLevel();
            float lvl = (lvl1 + lvl2) / 2;
            double c = 6;// 4.09 * 1.2 * 1.3 * (1 + lvl * 0.04) * (1 + (0.15 * (1.62 - 1) / 50 * lvl) + (1.05 + 0.55 / 50 * lvl - 1) * 0.5 + (1.05 + 0.55 / 50 * lvl - 1) * 0.5);
            one.HpMaxCoefficient = (float)c;
            two.HpMaxCoefficient = (float)c;
            one.EnergyMaxCoefficient = (float)c;
            two.EnergyMaxCoefficient = (float)c;
            UserAttrCalculator.Calc(one);
            UserAttrCalculator.Calc(two);
            one.SetHp(Operate_Type.OT_Absolute, one.GetActualProperty().HpMax);
            one.SetEnergy(Operate_Type.OT_Absolute, one.GetActualProperty().EnergyMax);
            two.SetHp(Operate_Type.OT_Absolute, two.GetActualProperty().HpMax);
            two.SetEnergy(Operate_Type.OT_Absolute, two.GetActualProperty().EnergyMax);

            Msg_RC_UpdateCoefficient msg1 = new Msg_RC_UpdateCoefficient();
            msg1.obj_id = one.GetId();
            msg1.hpmax_coefficient = one.HpMaxCoefficient;
            scene.NotifyAllUser(msg1);

            Msg_RC_UpdateCoefficient msg2 = new Msg_RC_UpdateCoefficient();
            msg2.obj_id = two.GetId();
            msg2.hpmax_coefficient = two.HpMaxCoefficient;
            scene.NotifyAllUser(msg2);

            Msg_RC_SyncProperty propMsg1 = DataSyncUtility.BuildSyncPropertyMessage(one);
            scene.NotifyAllUser(propMsg1);

            Msg_RC_SyncProperty propMsg2 = DataSyncUtility.BuildSyncPropertyMessage(two);
            scene.NotifyAllUser(propMsg2);
          }
        }
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 0) {
        m_ObjId.InitFromDsl(callData.GetParam(0));
        m_HaveParam = true;
      }
    }

    private IStoryValue<int> m_ObjId = new StoryValue<int>();
    private bool m_HaveParam = false;
  }
  /// <summary>
  /// pausescenelogic(scene_logic_config_id,true_or_false);
  /// </summary>
  internal class PauseSceneLogicCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      PauseSceneLogicCommand cmd = new PauseSceneLogicCommand();
      cmd.m_SceneLogicConfigId = m_SceneLogicConfigId.Clone();
      cmd.m_Enabled = m_Enabled.Clone();
      return cmd;
    }

    protected override void ResetState()
    { }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_SceneLogicConfigId.Evaluate(iterator, args);
      m_Enabled.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_SceneLogicConfigId.Evaluate(instance);
      m_Enabled.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        int cfgId = m_SceneLogicConfigId.Value;
        string enabled = m_Enabled.Value;
        SceneLogicInfo info = scene.SceneLogicInfoMgr.GetSceneLogicInfoByConfigId(cfgId);
        if (null != info) {
          info.IsLogicPaused = (0 == string.Compare(enabled, "true"));
        } else {
          LogSystem.Error("pausescenelogic can't find scenelogic {0}", cfgId);
        }
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 1) {
        m_SceneLogicConfigId.InitFromDsl(callData.GetParam(0));
        m_Enabled.InitFromDsl(callData.GetParam(1));
      }
    }

    private IStoryValue<int> m_SceneLogicConfigId = new StoryValue<int>();
    private IStoryValue<string> m_Enabled = new StoryValue<string>();
  }
  /// <summary>
  /// restartareamonitor(scene_logic_config_id);
  /// </summary>
  internal class RestartAreaMonitorCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      RestartAreaMonitorCommand cmd = new RestartAreaMonitorCommand();
      cmd.m_SceneLogicConfigId = m_SceneLogicConfigId.Clone();
      return cmd;
    }

    protected override void ResetState()
    { }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_SceneLogicConfigId.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_SceneLogicConfigId.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        int cfgId = m_SceneLogicConfigId.Value;
        SceneLogicInfo info = scene.SceneLogicInfoMgr.GetSceneLogicInfoByConfigId(cfgId);
        if(null!=info){
          UserEnterAreaLogicInfo data = info.LogicDatas.GetData<UserEnterAreaLogicInfo>();
          if (null != data) {
            data.m_IsTriggered = false;
          } else {
            LogSystem.Warn("restartareamonitor scenelogic {0} dosen't start, add wait command !", cfgId);
          }
        } else {
          LogSystem.Error("restartareamonitor can't find scenelogic {0}", cfgId);
        }
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      int num = callData.GetParamNum();
      if (num > 0) {
        m_SceneLogicConfigId.InitFromDsl(callData.GetParam(0));
      }
    }

    private IStoryValue<int> m_SceneLogicConfigId = new StoryValue<int>();
  }
  /// <summary>
  /// restarttimeout(scene_logic_config_id[,timeout]);
  /// </summary>
  internal class RestartTimeoutCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      RestartTimeoutCommand cmd = new RestartTimeoutCommand();
      cmd.m_ParamNum = m_ParamNum;
      cmd.m_SceneLogicConfigId = m_SceneLogicConfigId.Clone();
      cmd.m_Timeout = m_Timeout.Clone();
      return cmd;
    }

    protected override void ResetState()
    {
    }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_SceneLogicConfigId.Evaluate(iterator, args);
      if (m_ParamNum > 1)
        m_Timeout.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_SceneLogicConfigId.Evaluate(instance);
      if (m_ParamNum > 1)
        m_Timeout.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        int cfgId = m_SceneLogicConfigId.Value;
        SceneLogicInfo info = scene.SceneLogicInfoMgr.GetSceneLogicInfoByConfigId(cfgId);
        if (null != info) {
          TimeoutLogicInfo data = info.LogicDatas.GetData<TimeoutLogicInfo>();
          if (null != data) {
            data.m_IsTriggered = false;
            data.m_CurTime = 0;
            if (m_ParamNum > 1) {
              data.m_Timeout = m_Timeout.Value;
            }
          } else {
            LogSystem.Warn("restarttimeout scenelogic {0} dosen't start, add wait command !", cfgId);
          }
        } else {
          LogSystem.Error("restarttimeout can't find scenelogic {0}", cfgId);
        }
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      m_ParamNum = callData.GetParamNum();
      if (m_ParamNum > 0) {
        m_SceneLogicConfigId.InitFromDsl(callData.GetParam(0));
      }
      if (m_ParamNum > 1) {
        m_Timeout.InitFromDsl(callData.GetParam(1));
      }
    }

    private int m_ParamNum = 0;
    private IStoryValue<int> m_SceneLogicConfigId = new StoryValue<int>();
    private IStoryValue<int> m_Timeout = new StoryValue<int>();
  }
  /// <summary>
  /// restartareadetect(scene_logic_config_id[,timeout]);
  /// </summary>
  internal class RestartAreaDetectCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      RestartAreaDetectCommand cmd = new RestartAreaDetectCommand();
      cmd.m_ParamNum = m_ParamNum;
      cmd.m_SceneLogicConfigId = m_SceneLogicConfigId.Clone();
      cmd.m_Timeout = m_Timeout.Clone();
      return cmd;
    }

    protected override void ResetState()
    {
    }

    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_SceneLogicConfigId.Evaluate(iterator, args);
      if (m_ParamNum > 1)
        m_Timeout.Evaluate(iterator, args);
    }

    protected override void UpdateVariables(StoryInstance instance)
    {
      m_SceneLogicConfigId.Evaluate(instance);
      if (m_ParamNum > 1)
        m_Timeout.Evaluate(instance);
    }

    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Scene scene = instance.Context as Scene;
      if (null != scene) {
        int cfgId = m_SceneLogicConfigId.Value;
        SceneLogicInfo info = scene.SceneLogicInfoMgr.GetSceneLogicInfoByConfigId(cfgId);
        if (null != info) {
          AreaDetectLogicInfo data = info.LogicDatas.GetData<AreaDetectLogicInfo>();
          if (null != data) {
            data.m_IsTriggered = false;
            data.m_CurTime = 0;
            if (m_ParamNum > 1) {
              data.m_Timeout = m_Timeout.Value;
            }
          } else {
            LogSystem.Warn("restartareadetect scenelogic {0} dosen't start, add wait command !", cfgId);
          }
        } else {
          LogSystem.Error("restartareadetect can't find scenelogic {0}", cfgId);
        }
      }
      return false;
    }

    protected override void Load(ScriptableData.CallData callData)
    {
      m_ParamNum = callData.GetParamNum();
      if (m_ParamNum > 0) {
        m_SceneLogicConfigId.InitFromDsl(callData.GetParam(0));
      }
      if (m_ParamNum > 1) {
        m_Timeout.InitFromDsl(callData.GetParam(1));
      }
    }

    private int m_ParamNum = 0;
    private IStoryValue<int> m_SceneLogicConfigId = new StoryValue<int>();
    private IStoryValue<int> m_Timeout = new StoryValue<int>();
  }
}
