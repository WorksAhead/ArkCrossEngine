using System;
using System.Collections.Generic;
using SkillSystem;
using StorySystem;
using LobbyRobot;

namespace ArkCrossEngine.GmCommands
{
  //---------------------------------------------------------------------------------------------------------------------------------
  internal class SelectSceneCommand : SimpleStoryCommandBase<SelectSceneCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int sceneId = _params.Param1Value;
        robot.SelectScene(sceneId);
      }
      return false;
    }
  }
  internal class StagClearCommand : SimpleStoryCommandBase<StagClearCommand, StoryValueParam>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.StageClear();
      }
      return false;
    }
  }
  internal class CancelMatchCommand : SimpleStoryCommandBase<CancelMatchCommand, StoryValueParam>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.CancelMatch();
      }
      return false;
    }
  }
  internal class RequestUsersCommand : SimpleStoryCommandBase<RequestUsersCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int ct = _params.Param1Value;
        robot.RequestUsers(ct);
      }
      return false;
    }
  }
  internal class UpdatePositionCommand : SimpleStoryCommandBase<UpdatePositionCommand, StoryValueParam<float,float,float>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<float, float, float> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.UpdatePosition(_params.Param1Value, _params.Param2Value, _params.Param3Value);
      }
      return false;
    }
  }
  internal class LobbyAddAssetsCommand : SimpleStoryCommandBase<LobbyAddAssetsCommand, StoryValueParam<int, int, int, int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, int, int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int money = _params.Param1Value;
        int gold = _params.Param2Value;
        int exp = _params.Param3Value;
        int stamina = _params.Param4Value;
        robot.AddAssets(money, gold, exp, stamina);
      }
      return false;
    }
  }
  internal class LobbyAddItemCommand : SimpleStoryCommandBase<LobbyAddItemCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int itemId = _params.Param1Value;
        robot.AddItem(itemId);
      }
      return false;
    }
  }
  internal class MountEquipmentCommand : SimpleStoryCommandBase<LobbyAddItemCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int itemId = _params.Param1Value;
        ItemConfig cfg = ItemConfigProvider.Instance.GetDataById(itemId);
        robot.UnmountEquipment(cfg.m_WearParts);
        robot.MountEquipment(itemId, 0, cfg.m_WearParts);
      }
      return false;
    }
  }
  internal class MountSkillCommand : SimpleStoryCommandBase<MountSkillCommand, StoryValueParam<int, int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int skillId = _params.Param1Value;
        int slot = _params.Param2Value;
        robot.UnmountSkill(0, (SlotPosition)slot);
        robot.MountSkill(0, skillId, (SlotPosition)slot);
      }
      return false;
    }
  }
  internal class UpgradeSkillCommand : SimpleStoryCommandBase<UpgradeSkillCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int skillId = _params.Param1Value;
        robot.UpgradeSkill(0, skillId, true);
      }
      return false;
    }
  }
  internal class UnlockSkillCommand : SimpleStoryCommandBase<UnlockSkillCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int skillId = _params.Param1Value;
        robot.UnlockSkill(0, skillId);
      }
      return false;
    }
  }
  internal class SwapSkillCommand : SimpleStoryCommandBase<SwapSkillCommand, StoryValueParam<int, int, int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int skillId = _params.Param1Value;
        int srcPos = _params.Param2Value;
        int destPos = _params.Param3Value;
        robot.SwapSkill(0, skillId, (SlotPosition)srcPos, (SlotPosition)destPos);
      }
      return false;
    }
  }
  internal class LiftSkillCommand : SimpleStoryCommandBase<LiftSkillCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int skillId = _params.Param1Value;
        robot.LiftSkill(skillId);
      }
      return false;
    }
  }
  internal class UpgradeItemCommand : SimpleStoryCommandBase<UpgradeItemCommand, StoryValueParam<int, int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int equipPos = _params.Param1Value;
        int itemId = _params.Param2Value;
        robot.UpgradeItem(equipPos, itemId, true);
      }
      return false;
    }
  }
  internal class ExpeditionResetCommand : SimpleStoryCommandBase<ExpeditionResetCommand, StoryValueParam<int, int, int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int hp = _params.Param1Value;
        int mp = _params.Param2Value;
        int requestNum = _params.Param3Value;
        robot.ExpeditionReset(hp, mp, requestNum, true);
      }
      return false;
    }
  }
  internal class RequestExpeditionCommand : SimpleStoryCommandBase<RequestExpeditionCommand, StoryValueParam<int, int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int sceneId = _params.Param1Value;
        int tollgateNum = _params.Param2Value;
        robot.RequestExpedition(sceneId, tollgateNum);
      }
      return false;
    }
  }
  internal class FinishExpeditionCommand : SimpleStoryCommandBase<FinishExpeditionCommand, StoryValueParam<int, int, int, int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, int, int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int sceneId = _params.Param1Value;
        int tollgateNum = _params.Param2Value;
        int hp = _params.Param3Value;
        int mp = _params.Param4Value;
        robot.FinishExpedition(sceneId, tollgateNum, hp, mp);
      }
      return false;
    }
  }
  internal class ExpeditionAwardCommand : SimpleStoryCommandBase<ExpeditionAwardCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int tollgateNum = _params.Param1Value;
        robot.ExpeditionAward(tollgateNum);
      }
      return false;
    }
  }
  internal class FaceCommand : SimpleStoryCommandBase<FaceCommand, StoryValueParam<float>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<float> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        float face = _params.Param1Value;
        robot.RoomNetworkSystem.SyncFaceDirection(face);
      }
      return false;
    }
  }
  internal class MoveStartCommand : SimpleStoryCommandBase<MoveStartCommand, StoryValueParam<float, float, float>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<float, float, float> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        float x = _params.Param1Value;
        float z = _params.Param2Value;
        float dir = _params.Param3Value;
        robot.RoomNetworkSystem.SyncPlayerMoveStart(x, z, dir);
      }
      return false;
    }
  }
  internal class MoveStopCommand : SimpleStoryCommandBase<MoveStopCommand, StoryValueParam<float, float>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<float, float> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        float x = _params.Param1Value;
        float z = _params.Param2Value;
        robot.RoomNetworkSystem.SyncPlayerMoveStop(x, z);
      }
      return false;
    }
  }
  internal class SkillCommand : SimpleStoryCommandBase<SkillCommand, StoryValueParam<int, float, float, float>>
  {
    protected override void ResetState()
    {
      m_CurPhase = 0;
      m_CurTime = 0;
    }
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, float, float, float> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        if (m_CurTime < 2000) {
          m_CurTime += delta;
          int skillId = _params.Param1Value;
          float x = _params.Param2Value;
          float z = _params.Param3Value;
          float dir = _params.Param4Value;
          switch (m_CurPhase) {
            case 0: {
                m_CurPhase = 1;

                if (IsExSkill(skillId) || skillId==120741 || skillId==161501) {
                  LogSystem.Warn("robot cast ex skill {0}", skillId);
                }
                robot.RoomNetworkSystem.SyncPlayerSkill(skillId, x, z, dir);
                SkillInstance skillInst = robot.SkillAnalysis.Analyze(skillId);
                if (null != skillInst) {
                  m_ImpactsToOther = skillInst.EnableImpactsToOther;
                  m_ImpactsToMyself = skillInst.EnableImpactsToMyself;
                }
                robot.RoomNetworkSystem.SyncGfxMoveControlStart(robot.MyselfId, x, z, skillId, true);
              }
              break;
            case 1:
              if (m_CurTime > 1000) {
                m_CurPhase = 2;

                if (null != m_ImpactsToMyself) {
                  foreach (int impactId in m_ImpactsToMyself) {
                    robot.RoomNetworkSystem.SyncSendImpact(robot.MyselfId, impactId, robot.MyselfId, skillId, 1000, 10, 10, 10, 0);
                  }
                }

                if (null != m_ImpactsToOther) {
                  foreach (int impactId in m_ImpactsToOther) {
                    foreach (int objId in robot.OtherIds) {
                      robot.RoomNetworkSystem.SyncSendImpact(robot.MyselfId, impactId, objId, skillId, 1000, 10, 10, 10, 0);
                    }
                  }
                }
              }
              break;
            case 2:
              if (m_CurTime > 1500) {
                m_CurPhase = 3;

                robot.RoomNetworkSystem.SyncGfxMoveControlStop(robot.MyselfId, x, z, skillId, true);
                robot.RoomNetworkSystem.SyncPlayerStopSkill(skillId);
              }
              break;
          }
          return true;
        }
      }
      return false;
    }

    private int m_CurPhase = 0;
    private long m_CurTime = 0;
    private List<int> m_ImpactsToOther = null;
    private List<int> m_ImpactsToMyself = null;
    private static bool IsExSkill(int skillId)
    {
      bool ret = true;
      SkillLogicData cfg = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skillId) as SkillLogicData;
      if (null != cfg) {
        ret = (cfg.Category == SkillCategory.kEx);
      }
      return ret;
    }
  }
  internal class NpcSkillCommand : SimpleStoryCommandBase<NpcSkillCommand, StoryValueParam<int, int, float, float, float>>
  {
    protected override void ResetState()
    {
      m_CurPhase = 0;
      m_CurTime = 0;
    }
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, float, float, float> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        if (m_CurTime < 2000) {
          m_CurTime += delta;
          int npcId = _params.Param1Value;
          int skillId = _params.Param2Value;
          float x = _params.Param3Value;
          float z = _params.Param4Value;
          float dir = _params.Param5Value;
          switch (m_CurPhase) {
            case 0: {
                m_CurPhase = 1;

                robot.RoomNetworkSystem.SyncPlayerSkill(skillId, x, z, dir);
                SkillInstance skillInst = robot.SkillAnalysis.Analyze(skillId);
                m_ImpactsToOther = skillInst.EnableImpactsToOther;
                m_ImpactsToMyself = skillInst.EnableImpactsToMyself;
              }
              break;
            case 1:
              if (m_CurTime > 1000) {
                m_CurPhase = 2;

                foreach (int impactId in m_ImpactsToMyself) {
                  robot.RoomNetworkSystem.SyncSendImpact(npcId, impactId, npcId, skillId, 1000, 10, 10, 10, 0);
                }

                foreach (int impactId in m_ImpactsToOther) {
                  foreach (int objId in robot.OtherIds) {
                    robot.RoomNetworkSystem.SyncSendImpact(npcId, impactId, objId, skillId, 1000, 10, 10, 10, 0);
                  }
                }
              }
              break;
            case 2:
              if (m_CurTime > 1500) {
                m_CurPhase = 3;

                robot.RoomNetworkSystem.SyncNpcStopSkill(npcId, skillId);
              }
              break;
          }
          return true;
        }
      }
      return false;
    }

    private int m_CurPhase = 0;
    private long m_CurTime = 0;
    private List<int> m_ImpactsToOther = null;
    private List<int> m_ImpactsToMyself = null;
  }
  internal class SendImpactCommand : AbstractStoryCommand
  {
    public override IStoryCommand Clone()
    {
      SendImpactCommand cmd = new SendImpactCommand();
      cmd.m_SenderId = m_SenderId.Clone();
      cmd.m_ImpactId = m_ImpactId.Clone();
      cmd.m_TargetId = m_TargetId.Clone();
      cmd.m_SkillId = m_SkillId.Clone();
      cmd.m_Duration = m_Duration.Clone();
      cmd.m_X = m_X.Clone();
      cmd.m_Y = m_Y.Clone();
      cmd.m_Z = m_Z.Clone();
      cmd.m_SenderDir = m_SenderDir.Clone();
      return cmd;
    }
    protected override bool ExecCommand(StoryInstance instance, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.RoomNetworkSystem.SyncSendImpact(m_SenderId.Value, m_ImpactId.Value, m_TargetId.Value, m_SkillId.Value, m_Duration.Value, m_X.Value, m_Y.Value, m_Z.Value, m_SenderDir.Value);
      }
      return false;
    }
    protected override void UpdateArguments(object iterator, object[] args)
    {
      m_SenderId.Evaluate(iterator, args);
      m_ImpactId.Evaluate(iterator, args);
      m_TargetId.Evaluate(iterator, args);
      m_SkillId.Evaluate(iterator, args);
      m_Duration.Evaluate(iterator, args);
      m_X.Evaluate(iterator, args);
      m_Y.Evaluate(iterator, args);
      m_Z.Evaluate(iterator, args);
      m_SenderDir.Evaluate(iterator, args);
    }
    protected override void UpdateVariables(StoryInstance instance)
    {
      m_SenderId.Evaluate(instance);
      m_ImpactId.Evaluate(instance);
      m_TargetId.Evaluate(instance);
      m_SkillId.Evaluate(instance);
      m_Duration.Evaluate(instance);
      m_X.Evaluate(instance);
      m_Y.Evaluate(instance);
      m_Z.Evaluate(instance);
      m_SenderDir.Evaluate(instance);
    }
    protected override void Load(ScriptableData.CallData callData)
    {
      int paramNum = callData.GetParamNum();
      if (paramNum > 8) {
        m_SenderId.InitFromDsl(callData.GetParam(0));
        m_ImpactId.InitFromDsl(callData.GetParam(1));
        m_TargetId.InitFromDsl(callData.GetParam(2));
        m_SkillId.InitFromDsl(callData.GetParam(3));
        m_Duration.InitFromDsl(callData.GetParam(4));
        m_X.InitFromDsl(callData.GetParam(5));
        m_Y.InitFromDsl(callData.GetParam(6));
        m_Z.InitFromDsl(callData.GetParam(7));
        m_SenderDir.InitFromDsl(callData.GetParam(8));
      }
    }

    private IStoryValue<int> m_SenderId = new StoryValue<int>();
    private IStoryValue<int> m_ImpactId = new StoryValue<int>();
    private IStoryValue<int> m_TargetId = new StoryValue<int>();
    private IStoryValue<int> m_SkillId = new StoryValue<int>();
    private IStoryValue<int> m_Duration = new StoryValue<int>();
    private IStoryValue<float> m_X = new StoryValue<float>();
    private IStoryValue<float> m_Y = new StoryValue<float>();
    private IStoryValue<float> m_Z = new StoryValue<float>();
    private IStoryValue<float> m_SenderDir = new StoryValue<float>();
  }
  internal class MoveToPosCommand : SimpleStoryCommandBase<MoveToPosCommand, StoryValueParam<float, float, float, float>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<float, float, float, float> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        float tx = _params.Param1Value;
        float tz = _params.Param2Value;
        float x = _params.Param3Value;
        float z = _params.Param4Value;
        robot.RoomNetworkSystem.SyncPlayerMoveToPos(tx, tz, x, z);
      }
      return false;
    }
  }
  internal class MoveToAttackCommand : SimpleStoryCommandBase<MoveToAttackCommand, StoryValueParam<int, float, float, float>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, float, float, float> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int targetId = _params.Param1Value;
        float attackRange = _params.Param2Value;
        float x = _params.Param3Value;
        float z = _params.Param4Value;
        robot.RoomNetworkSystem.SyncPlayerMoveToAttack(targetId, attackRange, x, z);
      }
      return false;
    }
  }
  internal class GiveUpCombatCommand : SimpleStoryCommandBase<GiveUpCombatCommand, StoryValueParam>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.RoomNetworkSystem.SyncGiveUpCombat();
      }
      return false;
    }
  }
  internal class DeleteDeadNpcCommand : SimpleStoryCommandBase<DeleteDeadNpcCommand, StoryValueParam<int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int npcId = _params.Param1Value;
        robot.RoomNetworkSystem.SyncDeleteDeadNpc(npcId);
      }
      return false;
    }
  }
  internal class QuitRoomCommand : SimpleStoryCommandBase<QuitRoomCommand, StoryValueParam>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.LobbyNetworkSystem.QuitRoom();
      }
      return false;
    }
  }
  internal class QuitBattleCommand : SimpleStoryCommandBase<QuitBattleCommand, StoryValueParam>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.RoomNetworkSystem.QuitBattle();
      }
      return false;
    }
  }
  internal class ReconnectLobbyCommand : SimpleStoryCommandBase<ReconnectLobbyCommand, StoryValueParam>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.LobbyNetworkSystem.ConnectIfNotOpen();
      }
      return false;
    }
  }
  internal class DisconnectLobbyCommand : SimpleStoryCommandBase<DisconnectLobbyCommand, StoryValueParam>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        robot.LobbyNetworkSystem.Disconnect();
      }
      return false;
    }
  }
  internal class UpdateMaxUserCountCommand : SimpleStoryCommandBase<UpdateMaxUserCountCommand, StoryValueParam<int, int, int>>
  {
    protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, int> _params, long delta)
    {
      Robot robot = instance.Context as Robot;
      if (null != robot) {
        int maxUserCount = _params.Param1Value;
        int maxUserCountPerLogicServer = _params.Param2Value;
        int maxQueueingCount = _params.Param3Value;
        robot.UpdateMaxUserCount(maxUserCount, maxUserCountPerLogicServer, maxQueueingCount);
      }
      return false;
    }
  }

  //---------------------------------------------------------------------------------------------------------------------------------
  internal class RobotNameValue : SimpleStoryValueBase<RobotNameValue, StoryValueParam>
  {
    protected override void UpdateValue(StoryInstance instance, StoryValueParam _params, StoryValueResult result)
    {
      if (null != instance) {
        Robot robot = instance.Context as Robot;
        if (null != robot) {
          result.Value = robot.LobbyNetworkSystem.User;
        }
      }
    }
  }
  internal class DateTimeValue : SimpleStoryValueBase<DateTimeValue, StoryValueParam>
  {
    protected override void UpdateValue(StoryInstance instance, StoryValueParam _params, StoryValueResult result)
    {
      if (null != instance) {
        result.Value = Robot.GetDateTime();
      }
    }
  }
  internal class RandSkillValue : SimpleStoryValueBase<RandSkillValue, StoryValueParam>
  {
    protected override void UpdateValue(StoryInstance instance, StoryValueParam _params, StoryValueResult result)
    {
      if (null != instance) {
        Robot robot = instance.Context as Robot;
        if (null != robot) {
          result.Value = robot.GetRandSkill();
        }
      }
    }
  }
  internal class IsLobbyConnectedValue : SimpleStoryValueBase<IsLobbyConnectedValue, StoryValueParam>
  {
    protected override void UpdateValue(StoryInstance instance, StoryValueParam _params, StoryValueResult result)
    {
      if (null != instance) {
        Robot robot = instance.Context as Robot;
        if (null != robot) {
          result.Value = robot.LobbyNetworkSystem.IsConnected;
        }
      }
    }
  }
  internal class IsLobbyLoginingValue : SimpleStoryValueBase<IsLobbyLoginingValue, StoryValueParam>
  {
    protected override void UpdateValue(StoryInstance instance, StoryValueParam _params, StoryValueResult result)
    {
      if (null != instance) {
        Robot robot = instance.Context as Robot;
        if (null != robot) {
          result.Value = robot.LobbyNetworkSystem.IsLogining;
        }
      }
    }
  }
  internal class HasLobbyLoggedOnValue : SimpleStoryValueBase<HasLobbyLoggedOnValue, StoryValueParam>
  {
    protected override void UpdateValue(StoryInstance instance, StoryValueParam _params, StoryValueResult result)
    {
      if (null != instance) {
        Robot robot = instance.Context as Robot;
        if (null != robot) {
          result.Value = robot.LobbyNetworkSystem.HasLoggedOn;
        }
      }
    }
  }
  internal class IsRoomStartedValue : SimpleStoryValueBase<IsRoomStartedValue, StoryValueParam>
  {
    protected override void UpdateValue(StoryInstance instance, StoryValueParam _params, StoryValueResult result)
    {
      if (null != instance) {
        Robot robot = instance.Context as Robot;
        if (null != robot) {
          result.Value = !robot.RoomNetworkSystem.IsWaitStart;
        }
      }
    }
  }
  internal class IsRoomConnectedValue : SimpleStoryValueBase<IsRoomConnectedValue, StoryValueParam>
  {
    protected override void UpdateValue(StoryInstance instance, StoryValueParam _params, StoryValueResult result)
    {
      if (null != instance) {
        Robot robot = instance.Context as Robot;
        if (null != robot) {
          result.Value = robot.RoomNetworkSystem.IsConnected;
        }
      }
    }
  }
  //---------------------------------------------------------------------------------------------------------------------------------
}
