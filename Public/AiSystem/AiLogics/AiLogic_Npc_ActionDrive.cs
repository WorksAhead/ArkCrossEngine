using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    class AiLogic_Npc_ActionDrive : AbstractNpcStateLogic
    {
        private delegate void AiActionStartDelegate(NpcInfo npc, CharacterInfo target);
        private delegate void AiActionExcueteDelegate(NpcInfo npc, CharacterInfo target, long deltaTime);
        private delegate void AiActionForceStopDelegate(NpcInfo npc, CharacterInfo target);
        private enum AiActionType
        {
            STAND = 1,
            WALK,
            RUN,
            SKILL,
            TAUNT,
            SELFIMPACT,
            FLEE,
        }
        public override void OnEvent(NpcInfo npc, int eventId, CharacterInfo target, long deltaTime)
        {
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data)
            {
                NormalForceStopHandler(npc);
                switch (eventId)
                {
                    case (int)AiEventEnum.CONTROL_TIME_OVERFLOW:
                        if (data.MaxControlEventActions.Count > 0)
                        {
                            data.ActionStack.Clear();
                        }
                        int plusActionCount = data.MaxControlEventPlusActions.Count;
                        if (plusActionCount > 0)
                        {
                            data.ActionStack.Push(data.MaxControlEventPlusActions[CrossEngineHelper.Random.Next(plusActionCount)]);
                        }
                        for (int i = data.MaxControlEventActions.Count - 1; i >= 0; --i)
                        {
                            if (data.MaxControlEventActions[i].IsSatifyCD(TimeUtility.GetServerMilliseconds()) &&
                              data.MaxControlEventActions[i].IsLuckyEnough())
                            {
                                data.ActionStack.Push(data.MaxControlEventActions[i]);
                            }
                        }
                        break;
                    case (int)AiEventEnum.GET_UP:
                        if (data.GetUpEventActions.Count > 0)
                        {
                            data.ActionStack.Clear();
                        }
                        for (int i = data.GetUpEventActions.Count - 1; i >= 0; --i)
                        {
                            if (data.GetUpEventActions[i].IsSatifyCD(TimeUtility.GetServerMilliseconds()) &&
                              data.GetUpEventActions[i].IsLuckyEnough())
                            {
                                data.ActionStack.Push(data.GetUpEventActions[i]);
                            }
                        }
                        break;
                }
                if (data.ActionStack.Count > 0)
                {
                    data.ActiveAction = data.ActionStack.Pop();
                }
                ExecAiAction(npc, target, deltaTime);
            }
        }
        protected override void OnStateLogicInit(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            info.Time = 0;
            npc.GetMovementStateInfo().IsMoving = false;
            info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
            info.Target = 0;
            if (!string.IsNullOrEmpty(info.AiParam[0]))
            {
                if (int.Parse(info.AiParam[0]) == 1)
                {
                    AiLogicUtility.InitPatrolData(npc, this);
                }
            }
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && !string.IsNullOrEmpty(info.AiParam[2]))
            {
                int aiConfigId = int.Parse(info.AiParam[2]);
                AiConfig aiConfig = AiConfigProvider.Instance.GetDataById(aiConfigId);
                if (null != aiConfig)
                {
                    for (int i = 0; i < aiConfig.ActionList.Count; i++)
                    {
                        AiActionInfo aiActionInfo = CreateAiAction(aiConfig.ActionList[i]);
                        if (null != aiActionInfo)
                        {
                            data.Actions.Add(aiActionInfo);
                        }
                    }
                    for (int i = 0; i < aiConfig.MaxControlEvent.Count; i++)
                    {
                        AiActionInfo aiActionInfo = CreateAiAction(aiConfig.MaxControlEvent[i]);
                        if (null != aiActionInfo)
                        {
                            data.MaxControlEventActions.Add(aiActionInfo);
                        }
                    }
                    for (int i = 0; i < aiConfig.MaxControlEventPlus.Count; i++)
                    {
                        AiActionInfo aiActionInfo = CreateAiAction(aiConfig.MaxControlEventPlus[i]);
                        if (null != aiActionInfo)
                        {
                            data.MaxControlEventPlusActions.Add(aiActionInfo);
                        }
                    }
                    for (int i = 0; i < aiConfig.GetUpEvent.Count; ++i)
                    {
                        AiActionInfo aiActionInfo = CreateAiAction(aiConfig.GetUpEvent[i]);
                        if (null != aiActionInfo)
                        {
                            data.GetUpEventActions.Add(aiActionInfo);
                        }
                    }
                    /*
                    foreach (int id in aiConfig.ActionList) {
                      AiActionInfo aiActionInfo = CreateAiAction(id);
                      if (null != aiActionInfo) {
                        data.Actions.Add(aiActionInfo);
                      }
                    }
                    foreach (int id in aiConfig.MaxControlEvent) {
                      AiActionInfo aiActionInfo = CreateAiAction(id);
                      if (null != aiActionInfo) {
                        data.MaxControlEventActions.Add(aiActionInfo);
                      }
                    }*/
                    data.MaxControlTime = aiConfig.MaxControlTime;
                }
            }

        }
        protected override void OnInitStateHandlers()
        {
            SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
            SetStateHandler((int)AiStateId.Pursuit, this.PursuitHandler);
            SetStateHandler((int)AiStateId.ActionDrive, this.ActionDriveHandler);
            SetStateHandler((int)AiStateId.Patrol, this.PatrolHandler);
            SetStateHandler((int)AiStateId.MoveCommand, this.MoveCommandHandler);
            SetStateHandler((int)AiStateId.PursuitCommand, this.PursuitCommandHandler);
            SetStateHandler((int)AiStateId.PatrolCommand, this.PatrolCommandHandler);

            m_AiActionStartHandler.Add((int)AiActionType.STAND, NormalActionStartHandler);
            m_AiActionStartHandler.Add((int)AiActionType.WALK, NormalActionStartHandler);
            m_AiActionStartHandler.Add((int)AiActionType.RUN, NormalActionStartHandler);
            m_AiActionStartHandler.Add((int)AiActionType.SKILL, SkillActionStartHandler);
            m_AiActionStartHandler.Add((int)AiActionType.TAUNT, NormalActionStartHandler);
            m_AiActionStartHandler.Add((int)AiActionType.SELFIMPACT, NormalActionStartHandler);
            m_AiActionStartHandler.Add((int)AiActionType.FLEE, NormalActionStartHandler);

            m_AiActionExcueteHandler.Add((int)AiActionType.STAND, HandleStandAction);
            m_AiActionExcueteHandler.Add((int)AiActionType.WALK, HandleWalkAction);
            m_AiActionExcueteHandler.Add((int)AiActionType.RUN, HandleRunAction);
            m_AiActionExcueteHandler.Add((int)AiActionType.SKILL, HandleSkillAction);
            m_AiActionExcueteHandler.Add((int)AiActionType.TAUNT, HandleTauntAction);
            m_AiActionExcueteHandler.Add((int)AiActionType.SELFIMPACT, HandleSelfImpactAction);
            m_AiActionExcueteHandler.Add((int)AiActionType.FLEE, HandleFleeAction);
        }
        private void IdleHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            if (npc.GetSkillStateInfo().IsSkillActivated()) return;
            if (npc.IsDead()) return;
            if (npc.IsUnderControl())
            {
                // 被动进入战斗
            }
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            info.Time += deltaTime;
            if (info.Time > 100)
            {
                CharacterInfo target = AiLogicUtility.GetRandomTargetHelper(npc, CharacterRelation.RELATION_ENEMY, AiTargetType.ALL);
                if (null != target)
                {
                    info.Target = target.GetId();
                    NotifyNpcTargetChange(npc);
                    npc.GetMovementStateInfo().IsMoving = false;
                    NotifyNpcMove(npc);
                    info.Time = 0;
                    ChangeToState(npc, (int)AiStateId.Pursuit);
                }
                else
                {
                    if ((int)NpcTypeEnum.Partner == npc.NpcType)
                    {
                        CharacterInfo owner = npc.SceneContext.GetCharacterInfoById(npc.OwnerId);
                        if (null != owner)
                        {
                            Vector3 targetPos = owner.GetMovementStateInfo().GetPosition3D();
                            Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
                            if (Geometry.DistanceSquare(targetPos, srcPos) > 16)
                            {
                                AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, info.Time, true, this);
                            }
                            else
                            {
                                npc.GetMovementStateInfo().IsMoving = false;
                                NotifyNpcMove(npc);
                            }
                        }
                        else
                        {
                            UserInfo master = AiLogicUtility.GetRandomTargetHelper(npc, CharacterRelation.RELATION_FRIEND, AiTargetType.USER) as UserInfo;
                            if (null != master)
                            {
                                npc.OwnerId = master.GetId();
                            }
                        }
                    }
                    else
                    {
                        if (null != GetAiPatrolData(npc))
                        {
                            npc.GetMovementStateInfo().IsMoving = false;
                            ChangeToState(npc, (int)AiStateId.Patrol);
                        }
                    }
                }
                info.Time = 0;
            }
        }
        private void PursuitHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            if (!CanAiControl(npc))
            {
                npc.GetMovementStateInfo().IsMoving = false;
                return;
            }
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data)
            {
                CharacterInfo target = AiLogicUtility.GetLivingCharacterInfoHelper(npc, info.Target);
                if (null != target)
                {
                    float dist = (float)npc.GetActualProperty().AttackRange;
                    float distGoHome = (float)npc.GohomeRange;
                    Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                    Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
                    float powDist = Geometry.DistanceSquare(srcPos, targetPos);
                    float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
                    // 遇敌是播放特效， 逗留两秒。
                    if (data.WaitTime <= npc.MeetEnemyStayTime)
                    {
                        if (!data.HasMeetEnemy)
                        {
                            NotifyNpcMeetEnemy(npc, Animation_Type.AT_Attack);
                            data.HasMeetEnemy = true;
                        }
                        TrySeeTarget(npc, target);
                        data.WaitTime += deltaTime;
                        return;
                    }
                    // 走向目标1.5秒
                    if (data.MeetEnemyWalkTime < npc.MeetEnemyWalkTime)
                    {
                        data.MeetEnemyWalkTime += deltaTime;
                        NotifyNpcWalk(npc);
                        info.Time += deltaTime;
                        if (info.Time > m_IntervalTime)
                        {
                            info.Time = 0;
                            AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, m_IntervalTime, true, this);
                        }
                        return;
                    }
                    ChangeToState(npc, (int)AiStateId.ActionDrive);
                }
                else
                {
                    ChangeToState(npc, (int)AiStateId.Idle);
                }
            }
        }

        private void ActionDriveHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            if (npc.IsDead())
            {
                npc.GetMovementStateInfo().IsMoving = false;
                return;
            }
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data)
            {
                CharacterInfo target = AiLogicUtility.GetLivingCharacterInfoHelper(npc, info.Target);
                if (null != target)
                {
                    Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                    Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
                    float dist = Geometry.Distance(srcPos, targetPos);
                    if (data.LastGfxState != npc.GfxStateFlag)
                    {
                        if (data.LastGfxState == (int)GfxCharacterState_Type.KnockDown && npc.GfxStateFlag == (int)GfxCharacterState_Type.GetUp)
                        {
                            // 起身
                            OnEvent(npc, (int)AiEventEnum.GET_UP, target, deltaTime);
                        }
                        data.LastGfxState = npc.GfxStateFlag;
                    }
                    if (npc.IsUnderControl())
                    {
                        data.ControlTime += deltaTime;
                        if (data.ControlTime > data.MaxControlTime && npc.CanDisControl())
                        {
                            data.ControlTime = 0;
                            OnEvent(npc, (int)AiEventEnum.CONTROL_TIME_OVERFLOW, target, deltaTime);
                        }
                        return;
                    }
                    ExecAiAction(npc, target, deltaTime);
                }
                else
                {
                    if (null != data.ActiveAction)
                    {
                        AiActionInfo aiActionInfo = data.ActiveAction;
                        AiActionForceStopDelegate handler = GetAiActionForceStopHandler(aiActionInfo.Config.AiActionType);
                        if (null != handler)
                        {
                            handler(npc, target);
                        }
                    }
                    else
                    {
                        npc.GetMovementStateInfo().IsMoving = false;
                    }
                    ChangeToState(npc, (int)AiStateId.Pursuit);
                }
            }
        }
        private void PatrolHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            AiLogicUtility.CommonPatrolHandler(npc, aiCmdDispatcher, deltaTime, this);
        }
        private void MoveCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            AiLogicUtility.DoMoveCommandState(npc, aiCmdDispatcher, deltaTime, this);
        }
        private void PursuitCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            AiLogicUtility.DoPursuitCommandState(npc, aiCmdDispatcher, deltaTime, this);
        }
        private void PatrolCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            AiLogicUtility.DoPatrolCommandState(npc, aiCmdDispatcher, deltaTime, this);
        }
        private AiData_Npc_ActionDrive GetAiData(NpcInfo npc)
        {
            AiData_Npc_ActionDrive data = npc.GetAiStateInfo().AiDatas.GetData<AiData_Npc_ActionDrive>();
            if (null == data)
            {
                data = new AiData_Npc_ActionDrive();
                npc.GetAiStateInfo().AiDatas.AddData(data);
            }
            return data;
        }

        private AiActionInfo GetFitAiAction(NpcInfo npc)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data)
            {
                CharacterInfo target = AiLogicUtility.GetLivingCharacterInfoHelper(npc, info.Target);
                if (null != target)
                {
                    Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                    Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
                    float dist = Geometry.Distance(srcPos, targetPos);

                    for (int i = 0; i < data.Actions.Count; i++)
                    {
                        if (IsAiActionSatify(npc, target, data.Actions[i]))
                        {
                            return data.Actions[i];
                        }
                    }
                    /*
                    foreach(AiActionInfo aiActionInfo in data.Actions){
                      if(IsAiActionSatify(npc, target, aiActionInfo)){
                       return aiActionInfo;
                      }
                    }*/
                }
            }
            return null;
        }

        private void ExecAiAction(NpcInfo npc, CharacterInfo target, long deltaTime)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != info && null != data)
            {
                if (null != data.ActiveAction)
                {
                    AiActionInfo aiActionInfo = data.ActiveAction;
                    AiActionExcueteDelegate handler = GetAiActionExcuteHandler(aiActionInfo.Config.AiActionType);
                    if (null != handler)
                    {
                        handler(npc, target, deltaTime);
                    }
                }
                else
                {
                    if (data.ActionStack.Count > 0)
                    {
                        data.ActiveAction = data.ActionStack.Pop();
                    }
                    else
                    {
                        data.ActiveAction = GetFitAiAction(npc);
                    }
                    if (null != data.ActiveAction)
                    {
                        AiActionInfo aiActionInfo = data.ActiveAction;
                        AiActionStartDelegate handler = GetAiActionStartHandler(aiActionInfo.Config.AiActionType);
                        if (null != handler)
                        {
                            handler(npc, target);
                        }
                    }
                }
            }
        }
        private AiActionInfo CreateAiAction(int id)
        {
            AiActionInfo result = null;
            AiActionConfig config = AiActionConfigProvider.Instance.GetDataById(id);
            if (null != config)
            {
                if (config.AiActionType == (int)AiActionType.SKILL)
                {
                    result = new AiSkillActionInfo(config);
                }
                else
                {
                    result = new AiActionInfo(config);
                }
            }
            else
            {
                LogSystem.Warn("CreateAiAction:: can't find AiActionConfig {0}", id);
            }
            return result;
        }

        private bool IsAiActionSatify(NpcInfo npc, CharacterInfo target, AiActionInfo aiActionInfo)
        {
            Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
            Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float dist = Geometry.Distance(srcPos, targetPos);
            if (aiActionInfo.IsSatifyCD(TimeUtility.GetServerMilliseconds()) &&
               aiActionInfo.IsSatifyDis(dist) &&
               aiActionInfo.IsSatisfySelfHp(npc.Hp, npc.GetActualProperty().HpMax) &&
               aiActionInfo.IsSatisfyTargetHp(target.Hp, target.GetActualProperty().HpMax) &&
               aiActionInfo.IsSatifyUser(npc) &&
               aiActionInfo.IsLuckyEnough())
            {
                return true;
            }
            return false;
        }
        private bool IsAiActionSatifyWithOutProbability(NpcInfo npc, CharacterInfo target, AiActionInfo aiActionInfo)
        {
            Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
            Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float dist = Geometry.Distance(srcPos, targetPos);
            if (aiActionInfo.IsSatifyCD(TimeUtility.GetServerMilliseconds()) &&
               aiActionInfo.IsSatifyDis(dist) &&
               aiActionInfo.IsSatisfySelfHp(npc.Hp, npc.GetActualProperty().HpMax) &&
               aiActionInfo.IsSatisfyTargetHp(target.Hp, target.GetActualProperty().HpMax) &&
               aiActionInfo.IsSatifyUser(npc))
            {
                return true;
            }
            return false;
        }
        #region ActionStartHandlers
        private AiActionStartDelegate GetAiActionStartHandler(int actionType)
        {
            AiActionStartDelegate ret;
            m_AiActionStartHandler.TryGetValue(actionType, out ret);
            return ret;
        }
        private void NormalActionStartHandler(NpcInfo npc, CharacterInfo target)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo)
                {
                    //aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                    aiActionInfo.StartTime = TimeUtility.GetServerMilliseconds();
                }
            }
        }
        private void SkillActionStartHandler(NpcInfo npc, CharacterInfo target)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo)
                {
                    AiSkillActionInfo aiSkillActionInfo = aiActionInfo as AiSkillActionInfo;
                    if (null != aiSkillActionInfo)
                    {
                        aiSkillActionInfo.CurSkillIndex = 0;
                        aiSkillActionInfo.CurSkillCombo = aiSkillActionInfo.GetSkillCombo();
                        //aiSkillActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                        aiSkillActionInfo.StartTime = TimeUtility.GetServerMilliseconds();
                    }
                }
            }
        }
        #endregion

        #region ActionExcueteHandles

        private AiActionExcueteDelegate GetAiActionExcuteHandler(int actionType)
        {
            AiActionExcueteDelegate ret;
            m_AiActionExcueteHandler.TryGetValue(actionType, out ret);
            return ret;
        }
        private void HandleStandAction(NpcInfo npc, CharacterInfo target, long delatTime)
        {
            if (npc.GetSkillStateInfo().IsSkillActivated()) return;
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                npc.GetMovementStateInfo().IsMoving = false;
                NotifyNpcMove(npc);
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo)
                {
                    if ((!IsAiActionSatify(npc, target, aiActionInfo) && aiActionInfo.Config.CanInterrupt) || aiActionInfo.IsTimeOut(TimeUtility.GetServerMilliseconds()))
                    {
                        data.ActiveAction = null;
                        aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                    }
                }
            }
        }
        private void HandleWalkAction(NpcInfo npc, CharacterInfo target, long deltaTime)
        {
            if (npc.GetSkillStateInfo().IsSkillActivated()) return;
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo)
                {
                    info.Time += deltaTime;
                    if (info.Time > m_IntervalTime)
                    {
                        info.Time = 0;
                        NotifyNpcWalk(npc);
                        AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, m_IntervalTime, true, this);
                    }
                }
                if ((!IsAiActionSatify(npc, target, aiActionInfo) && aiActionInfo.Config.CanInterrupt) || aiActionInfo.IsTimeOut(TimeUtility.GetServerMilliseconds()))
                {
                    data.ActiveAction = null;
                    aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                    NotifyNpcRun(npc);
                }
            }
        }
        private void HandleRunAction(NpcInfo npc, CharacterInfo target, long deltaTime)
        {
            if (npc.GetSkillStateInfo().IsSkillActivated()) return;
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo)
                {
                    info.Time += deltaTime;
                    if (info.Time > m_IntervalTime)
                    {
                        info.Time = 0;
                        NotifyNpcRun(npc);
                        AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, m_IntervalTime, true, this);
                    }
                }
                if ((!IsAiActionSatify(npc, target, aiActionInfo) && aiActionInfo.Config.CanInterrupt) || aiActionInfo.IsTimeOut(TimeUtility.GetServerMilliseconds()))
                {
                    data.ActiveAction = null;
                    aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                }
            }
        }
        private void HandleTauntAction(NpcInfo npc, CharacterInfo target, long deltaTime)
        {
            if (npc.GetSkillStateInfo().IsSkillActivated()) return;
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo)
                {
                    npc.GetMovementStateInfo().IsMoving = false;
                    npc.IsTaunt = true;
                    TrySeeTarget(npc, target);
                }
                if ((!IsAiActionSatify(npc, target, aiActionInfo) && aiActionInfo.Config.CanInterrupt) || aiActionInfo.IsTimeOut(TimeUtility.GetServerMilliseconds()))
                {
                    npc.IsTaunt = false;
                    data.ActiveAction = null;
                    aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                }
            }
        }
        private void HandleSelfImpactAction(NpcInfo npc, CharacterInfo target, long deltaTime)
        {
            if (npc.GetSkillStateInfo().IsSkillActivated()) return;
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo && !String.IsNullOrEmpty(aiActionInfo.Config.ActionParam))
                {
                    NotifyNpcAddImpact(npc, int.Parse(aiActionInfo.Config.ActionParam));
                }
                data.ActiveAction = null;
                aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
            }
        }
        private void HandleSkillAction(NpcInfo npc, CharacterInfo target, long deltaTime)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo && !npc.GetSkillStateInfo().IsSkillActivated())
                {
                    AiSkillActionInfo skillActionInfo = aiActionInfo as AiSkillActionInfo;
                    if (null != skillActionInfo && skillActionInfo.CurSkillIndex < skillActionInfo.CurSkillCombo.Count)
                    {
                        int skillId = skillActionInfo.CurSkillCombo[skillActionInfo.CurSkillIndex];
                        if (AiLogicUtility.CanCastSkill(npc, skillId, target))
                        {
                            npc.GetMovementStateInfo().IsMoving = false;
                            NotifyNpcMove(npc);
                            if (TryCastSkill(npc, skillId, target))
                            {
                                skillActionInfo.CurSkillIndex++;
                            }
                        }
                        else
                        {
                            info.Time += deltaTime;
                            if (info.Time > m_IntervalTime)
                            {
                                info.Time = 0;
                                if (AiLogicUtility.IsTooCloseToCastSkill(npc, target))
                                {
                                    Vector3 escapePos = Vector3.Zero;
                                    if (AiLogicUtility.GetEscapeTargetPos(npc, target, 3.0f, ref escapePos))
                                    {
                                        NotifyNpcRun(npc);
                                        AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, escapePos, m_IntervalTime, true, this);
                                    }
                                }
                                else
                                {
                                    NotifyNpcRun(npc);
                                    AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, m_IntervalTime, true, this);
                                }
                            }
                        }
                    }
                    else
                    {
                        skillActionInfo.CurSkillIndex = 0;
                        data.ActiveAction = null;
                        aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                    }
                }
                if ((!IsAiActionSatifyWithOutProbability(npc, target, aiActionInfo) && aiActionInfo.Config.CanInterrupt) || (aiActionInfo.IsTimeOut(TimeUtility.GetServerMilliseconds()) && !npc.GetSkillStateInfo().IsSkillActivated()))
                {
                    data.ActiveAction = null;
                    aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                    NotifyNpcStopSkill(npc);
                }
            }
        }
        private void HandleFleeAction(NpcInfo npc, CharacterInfo target, long deltaTime)
        {
            if (npc.GetSkillStateInfo().IsSkillActivated()) return;
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data && null != target)
            {
                Vector3 targetPos = Vector3.Zero;
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo)
                {
                    float anglePlus = 0.0f;
                    float.TryParse(aiActionInfo.Config.ActionParam, out anglePlus);
                    if (GetEscapeTargetPos(npc, target, 3.0f, anglePlus, ref targetPos))
                    {
                        info.Time += deltaTime;
                        if (info.Time > m_IntervalTime)
                        {
                            info.Time = 0;
                            NotifyNpcRun(npc);
                            AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, m_IntervalTime, true, this);
                        }
                    }
                }
                if ((!IsAiActionSatify(npc, target, aiActionInfo) && aiActionInfo.Config.CanInterrupt) || aiActionInfo.IsTimeOut(TimeUtility.GetServerMilliseconds()))
                {
                    data.ActiveAction = null;
                    npc.GetMovementStateInfo().IsMoving = false;
                    NotifyNpcMove(npc);
                    aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                }
            }
        }
        private bool GetEscapeTargetPos(NpcInfo npc, CharacterInfo target, float dis, float anglePlus, ref Vector3 escapePos)
        {
            Vector3 targetPos = Vector3.Zero;
            Vector3 sourcePos = npc.GetMovementStateInfo().GetPosition3D();
            float angle = Geometry.GetYAngle(target.GetMovementStateInfo().GetPosition2D(), npc.GetMovementStateInfo().GetPosition2D());
            angle += CrossEngineHelper.DegreeToRadian(anglePlus);
            targetPos.X = sourcePos.X + dis * (float)Math.Sin(angle);
            targetPos.Y = sourcePos.Y;
            targetPos.Z = sourcePos.Z + dis * (float)Math.Cos(angle);
            bool isFind = AiLogicUtility.GetWalkablePosition(npc.SpatialSystem.GetCellMapView(npc.AvoidanceRadius), targetPos, sourcePos, ref escapePos);
            return isFind;
        }
        #endregion

        #region ActionForceStopHandlers
        private AiActionForceStopDelegate GetAiActionForceStopHandler(int actionType)
        {
            AiActionForceStopDelegate ret;
            m_AIActionFroceStopHandlers.TryGetValue(actionType, out ret);
            return ret;
        }
        private void NormalForceStopHandler(NpcInfo npc)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            AiData_Npc_ActionDrive data = GetAiData(npc);
            if (null != data)
            {
                npc.GetMovementStateInfo().IsMoving = false;
                npc.IsTaunt = false;
                NotifyNpcRun(npc);
                AiActionInfo aiActionInfo = data.ActiveAction;
                if (null != aiActionInfo)
                {
                    data.ActiveAction = null;
                    aiActionInfo.LastStartTime = TimeUtility.GetServerMilliseconds();
                }
            }
        }
        #endregion

        private const long m_IntervalTime = 100;
        private Dictionary<int, AiActionStartDelegate> m_AiActionStartHandler = new Dictionary<int, AiActionStartDelegate>();
        private Dictionary<int, AiActionExcueteDelegate> m_AiActionExcueteHandler = new Dictionary<int, AiActionExcueteDelegate>();
        private Dictionary<int, AiActionForceStopDelegate> m_AIActionFroceStopHandlers = new Dictionary<int, AiActionForceStopDelegate>();
    }
}
