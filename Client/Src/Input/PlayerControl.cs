using System;
using System.Collections.Generic;
using ArkCrossEngine.Network;

namespace ArkCrossEngine
{
    public class PlayerControl
    {
        // static methods
        private static PlayerControl inst_ = new PlayerControl();
        public static PlayerControl Instance { get { return inst_; } }
        // properties
        public bool EnableMoveInput { get; set; }
        public bool EnableRotateInput { get; set; }
        public bool EnableSkillInput { get; set; }

        // methods
        internal PlayerControl()
        {
            pm_ = new PlayerMovement();
            EnableMoveInput = true;
            EnableRotateInput = true;
            EnableSkillInput = true;
        }

        internal void Reset()
        {
            EnableMoveInput = true;
            EnableRotateInput = true;
            EnableSkillInput = true;
        }
        internal void Init()
        {
            GfxSystem.ListenKeyPressState(
              Keyboard.Code.Z,
              Keyboard.Code.X,
              Keyboard.Code.Space,
              Keyboard.Code.Period,
              Keyboard.Code.W,
              Keyboard.Code.S,
              Keyboard.Code.A,
              Keyboard.Code.D,
              Keyboard.Code.P,
              Keyboard.Code.M,
              Keyboard.Code.F1,
              Keyboard.Code.B,
              Keyboard.Code.F2,
              Keyboard.Code.F3,
              Keyboard.Code.F4,
              Keyboard.Code.F5,
              Keyboard.Code.F6,
              Keyboard.Code.F7,
              Keyboard.Code.F9,
              Keyboard.Code.F10);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F10, this.JoinTwoPlayerStage);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F9, this.ToggleAllUiVisble);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F7, this.JoinPlatformDefense);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F6, this.MatchMpve);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F5, this.PublishNotice);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F4, this.FillHp);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F3, this.KillAll);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F2, this.ToolPool);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.F1, this.DebugLog);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.B, this.BuyStamina);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.P, this.SwitchHero);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.Z, this.SwitchDebug);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.X, this.SwitchObserver);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.Space, this.InteractObject);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.Period, this.PrintPosition);
            GfxSystem.ListenTouchEvent(TouchEvent.Cesture, this.TouchHandle);
            GfxSystem.ListenKeyboardEvent(Keyboard.Code.M, this.OnPlaySkill);
        }

        internal void Tick()
        {
            long now = TimeUtility.GetServerMilliseconds();
            m_LastTickIntervalMs = now - m_LastTickTime;

            m_LastTickTime = now;

            if (WorldSystem.Instance.IsObserver && !WorldSystem.Instance.IsFollowObserver)
            {
                bool keyPressed = false;
                float x = 0.5f, y = 0.5f;
                if (GfxSystem.IsKeyPressed(Keyboard.Code.A))
                {
                    x = 0.1f;
                    keyPressed = true;
                }
                else if (GfxSystem.IsKeyPressed(Keyboard.Code.D))
                {
                    x = 0.9f;
                    keyPressed = true;
                }
                if (GfxSystem.IsKeyPressed(Keyboard.Code.W))
                {
                    y = 0.1f;
                    keyPressed = true;
                }
                else if (GfxSystem.IsKeyPressed(Keyboard.Code.S))
                {
                    y = 0.9f;
                    keyPressed = true;
                }
                if (keyPressed)
                    WorldSystem.Instance.UpdateObserverCamera(x, y);
                return;
            }


            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            // if move input is disable
            // MotionStatus is MoveStop, and MotionChanged is reflect the change accordingly 

            if (EnableMoveInput)
            {
                if (!IsKeyboardControl())
                {
                    CheckJoystickControl();
                }
            }

            if (!m_IsJoystickControl)
            {
                pm_.Update(EnableMoveInput);
            }
            MovementStateInfo msi = playerself.GetMovementStateInfo();
            Vector3 pos = msi.GetPosition3D();

            //LogSystem.Debug("Pos : {0}, Dir : {1}", pos.ToString(), playerself.GetMovementStateInfo().GetFaceDir());
            bool reface = false;
            if (m_LastTickIsSkillMoving && !msi.IsSkillMoving)
            {
                reface = true;
            }

            //操作同步机制改为发给服务器同时本地就开始执行（服务器转发给其它客户端，校验失败则同时发回原客户端进行位置调整）
            Vector3 mouse_pos = new Vector3(GfxSystem.GetMouseX(), GfxSystem.GetMouseY(), GfxSystem.GetMouseZ());
            if (pm_.MotionStatus == PlayerMovement.Motion.Moving || pm_.JoyStickMotionStatus == PlayerMovement.Motion.Moving)
            {
                if (pm_.MotionChanged || pm_.JoyStickMotionChanged || !m_LastTickIsMoving)
                {
                    StopAiMove();

                    playerself.SkillController.AddBreakSkillTask();
                    float moveDir = RoundMoveDir(pm_.MoveDir);

                    //GfxSystem.GfxLog("PlayerControl.Tick MoveDir:{0} RoundMoveDir:{1}", pm_.MoveDir, moveDir);

                    if (!m_LastTickIsMoving || !Geometry.IsSameFloat(moveDir, m_lastMoveDir))
                    {
                        msi.SetMoveDir(moveDir);
                        //ControlSystemOperation.AdjustCharacterMoveDir(playerself.GetId(), moveDir);
                        msi.IsMoving = true;
                        msi.TargetPosition = Vector3.Zero;

                        if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
                        {
                            NetworkSystem.Instance.SyncPlayerMoveStart(moveDir);
                        }
                    }
                    if (EnableRotateInput)
                    {
                        if (reface || !m_LastTickIsMoving || !Geometry.IsSameFloat(pm_.MoveDir, m_lastDir))
                        {
                            msi.SetFaceDir(pm_.MoveDir);
                            //ControlSystemOperation.AdjustCharacterFaceDir(playerself.GetId(), pm_.MoveDir);
                            msi.SetWantFaceDir(pm_.MoveDir);

                            if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
                            {
                                NetworkSystem.Instance.SyncFaceDirection(pm_.MoveDir);
                            }
                        }
                    }
                    m_lastDir = pm_.MoveDir;
                    m_lastMoveDir = moveDir;
                }
                m_LastTickIsMoving = true;
            }
            else
            {
                if (m_LastTickIsMoving)
                {
                    playerself.SkillController.CancelBreakSkillTask();
                    playerself.GetMovementStateInfo().IsMoving = false;
                    if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
                    {
                        NetworkSystem.Instance.SyncPlayerMoveStop();
                    }
                    if (EnableRotateInput)
                    {
                        if (reface)
                        {
                            msi.SetFaceDir(m_lastDir);
                            //ControlSystemOperation.AdjustCharacterFaceDir(playerself.GetId(), m_lastDir);
                            msi.SetWantFaceDir(m_lastDir);

                            if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
                            {
                                NetworkSystem.Instance.SyncFaceDirection(m_lastDir);
                            }
                        }
                    }
                }
                m_LastTickIsMoving = false;
            }
            m_LastTickIsSkillMoving = msi.IsSkillMoving;

            old_mouse_pos_ = mouse_pos_;
            mouse_pos_.X = GfxSystem.GetMouseX();
            mouse_pos_.Y = GfxSystem.GetMouseY();

            UserAiStateInfo aiInfo = playerself.GetAiStateInfo();
            if (null != aiInfo && (int)AiStateId.Idle == aiInfo.CurState)
            {
                m_lastSelectObjId = -1;
            }
        }

        private void StopAiMove()
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null != playerself)
            {
                UserAiStateInfo aiInfo = playerself.GetAiStateInfo();
                if (aiInfo.CurState == (int)AiStateId.MoveCommand || aiInfo.CurState == (int)AiStateId.PursuitCommand || aiInfo.CurState == (int)AiStateId.PatrolCommand)
                {
                    aiInfo.Time = 0;
                    aiInfo.Target = 0;
                    aiInfo.AiDatas.RemoveData<AiData_ForMoveCommand>();
                    aiInfo.AiDatas.RemoveData<AiData_ForPursuitCommand>();
                    aiInfo.AiDatas.RemoveData<AiData_ForPatrolCommand>();
                    aiInfo.ChangeToState((int)AiStateId.Idle);
                    ClientStorySystem.Instance.SendMessage("aimovestopped");
                }
            }
        }

        private float RoundMoveDir(float dir)
        {
            const float c_PiDiv8 = (float)Math.PI / 8;
            const float c_PiDiv16 = (float)Math.PI / 16;
            int intDir = (int)(dir / c_PiDiv16);
            intDir = (intDir + 1) % 32;
            intDir /= 2;
            return intDir * c_PiDiv8;
        }

        private bool IsKeyboardControl()
        {
            bool ret = false;
            if (GfxSystem.IsKeyPressed(Keyboard.Code.W)
                || GfxSystem.IsKeyPressed(Keyboard.Code.A)
                || GfxSystem.IsKeyPressed(Keyboard.Code.S)
                || GfxSystem.IsKeyPressed(Keyboard.Code.D))
            {
                ret = true;
            }
            return ret;
        }

        private void CheckJoystickControl()
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            float dir = GfxSystem.GetJoystickDir();
            if (dir < 0)
            {
                dir += c_2PI;
            }
            Vector3 target_pos = new Vector3(GfxSystem.GetJoystickTargetPosX(), GfxSystem.GetJoystickTargetPosY(), GfxSystem.GetJoystickTargetPosZ());
            UpdateMoveState(playerself, target_pos, dir);
        }

        private void OnPlaySkill(int key_code, int what)
        {
            if (what == (int)Keyboard.Event.Down)
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                playerself.SkillController.ForceInterruptCurSkill();
            }
        }

        private void SwitchHero(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                if (WorldSystem.Instance.IsPureClientScene() || WorldSystem.Instance.IsPveScene())
                {
                    WorldSystem.Instance.ChangeHero();
                }
                else
                {
                    //多人情形切英雄还不知道需求
                }
            }
        }

        public void SetCameraYawOffset(float offset)
        {
            pm_.MoveDirOffset = offset;
        }

        private void KillAll(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                for (LinkedListNode<UserInfo> linkNode = WorldSystem.Instance.UserManager.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                {
                    UserInfo info = linkNode.Value;
                    if (info.GetId() != playerself.GetId())
                    {
                        info.SetHp(Operate_Type.OT_Absolute, 0);
                    }
                }
                for (LinkedListNode<NpcInfo> linkNode = WorldSystem.Instance.NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
                {
                    NpcInfo info = linkNode.Value;
                    info.SetHp(Operate_Type.OT_Absolute, 0);
                }
            }
        }

        public void MatchMpve(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                LobbyNetworkSystem.Instance.RequestMatchMpve(4031);
            }
        }

        private void FillHp(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                playerself.SetHp(Operate_Type.OT_Absolute, playerself.GetActualProperty().HpMax);
            }
        }

        private void JoinMpve(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                LobbyNetworkSystem.Instance.StartMpve(4011);
            }
        }
        public void JoinPlatformDefense(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            if ((int)Keyboard.Event.Up == what)
            {
                LobbyNetworkSystem.Instance.StartMpve(4031);
            }
        }
        public void JoinTwoPlayerStage(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            if ((int)Keyboard.Event.Up == what)
            {
                LobbyNetworkSystem.Instance.StartMpve(5011);
            }
        }
        //隐藏所有UI
        private void ToggleAllUiVisble(int key_code, int what)
        {
            if ((int)Keyboard.Event.Down == what)
                GfxSystem.PublishGfxEvent("ge_toggle_all_ui", "ui");
        }
        private void SummonPartner(int key_code, int what)
        {
            RoleInfo roleself = LobbyClient.Instance.CurrentRole;
            if (null == roleself) return;
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself) return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                // summonpartner
                PartnerInfo partnerInfo = roleself.PartnerStateInfo.GetActivePartner();
                if (null != partnerInfo)
                {
                    Data_Unit data = new Data_Unit();
                    data.m_Id = -1;
                    data.m_LinkId = partnerInfo.LinkId;
                    data.m_CampId = playerself.GetCampId();
                    data.m_Pos = playerself.GetMovementStateInfo().GetPosition3D();
                    data.m_RotAngle = 0;
                    data.m_AiLogic = partnerInfo.GetAiLogic();
                    data.m_AiParam[0] = "";
                    data.m_AiParam[1] = "";
                    data.m_AiParam[2] = partnerInfo.GetAiParam().ToString();
                    data.m_IsEnable = true;
                    NpcInfo npc = WorldSystem.Instance.NpcManager.AddNpc(data);
                    if (null != npc)
                    {
                        AppendAttributeConfig aac = AppendAttributeConfigProvider.Instance.GetDataById(partnerInfo.GetAppendAttrConfigId());
                        float inheritAttackAttrPercent = partnerInfo.GetInheritAttackAttrPercent();
                        float inheritDefenceAttrPercent = partnerInfo.GetInheritDefenceAttrPercent();
                        if (null != aac)
                        {
                            // attack
                            npc.GetBaseProperty().SetAttackBase(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().AttackBase * inheritAttackAttrPercent));
                            npc.GetBaseProperty().SetFireDamage(Operate_Type.OT_Absolute, playerself.GetActualProperty().FireDamage * inheritAttackAttrPercent);
                            npc.GetBaseProperty().SetIceDamage(Operate_Type.OT_Absolute, playerself.GetActualProperty().IceDamage * inheritAttackAttrPercent);
                            npc.GetBaseProperty().SetPoisonDamage(Operate_Type.OT_Absolute, playerself.GetActualProperty().PoisonDamage * inheritAttackAttrPercent);
                            // defence
                            npc.GetBaseProperty().SetHpMax(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().HpMax * inheritDefenceAttrPercent));
                            npc.GetBaseProperty().SetEnergyMax(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().EnergyMax * inheritDefenceAttrPercent));
                            npc.GetBaseProperty().SetADefenceBase(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().ADefenceBase * inheritDefenceAttrPercent));
                            npc.GetBaseProperty().SetMDefenceBase(Operate_Type.OT_Absolute, (int)(playerself.GetActualProperty().MDefenceBase * inheritDefenceAttrPercent));
                            npc.GetBaseProperty().SetFireERD(Operate_Type.OT_Absolute, playerself.GetActualProperty().FireERD * inheritDefenceAttrPercent);
                            npc.GetBaseProperty().SetIceERD(Operate_Type.OT_Absolute, playerself.GetActualProperty().IceERD * inheritDefenceAttrPercent);
                            npc.GetBaseProperty().SetPoisonERD(Operate_Type.OT_Absolute, playerself.GetActualProperty().PoisonERD * inheritDefenceAttrPercent);
                            // reset hp & energy
                            npc.SetHp(Operate_Type.OT_Absolute, npc.GetActualProperty().HpMax);
                            npc.SetEnergy(Operate_Type.OT_Absolute, npc.GetActualProperty().EnergyMax);
                        }
                        npc.SetAIEnable(true);
                        npc.GetSkillStateInfo().RemoveAllSkill();
                        npc.BornTime = TimeUtility.GetServerMilliseconds();
                        List<int> skillList = partnerInfo.GetSkillList();
                        if (null != skillList)
                        {
                            for (int i = 0; i < skillList.Count; ++i)
                            {
                                SkillInfo skillInfo = new SkillInfo(skillList[i]);
                                npc.GetSkillStateInfo().AddSkill(skillInfo);
                            }
                        }
                        npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                        npc.OwnerId = playerself.GetId();
                        playerself.PartnerId = npc.GetId();
                        EntityManager.Instance.CreateNpcView(npc.GetId());
                        if (partnerInfo.BornSkill > 0)
                        {
                            SkillInfo skillInfo = new SkillInfo(partnerInfo.BornSkill);
                            npc.GetSkillStateInfo().AddSkill(skillInfo);
                            npc.SkillController.ForceStartSkill(partnerInfo.BornSkill);
                        }
                        CharacterView view = EntityManager.Instance.GetCharacterViewById(npc.GetId());
                        if (null != view)
                        {
                            GfxSystem.SetLayer(view.Actor, "PhysicObj");
                        }
                    }
                }
            }
        }

        private void PublishNotice(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                LobbyNetworkSystem.Instance.PublishNotice("Pikachu", 3);
            }
        }

        public void ToolPool(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                LobbyNetworkSystem.Instance.AddAssets(1000, 1000, 20000, 1000);
                LobbyNetworkSystem.Instance.AddItem(55101);
                LobbyNetworkSystem.Instance.AddItem(55102);
                LobbyNetworkSystem.Instance.AddItem(55103);
                LobbyNetworkSystem.Instance.AddItem(55104);
                LobbyNetworkSystem.Instance.AddItem(55105);
                LobbyNetworkSystem.Instance.AddItem(55106);
                LobbyNetworkSystem.Instance.AddItem(55107);
                LobbyNetworkSystem.Instance.AddItem(55108);
            }
        }

        private void DebugLog(int key_code, int what)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (null == playerself)
                return;
            ///
            if ((int)Keyboard.Event.Up == what)
            {
                List<SkillInfo> skill_info = playerself.GetSkillStateInfo().GetAllSkill();
                if (null != skill_info)
                {
                    foreach (SkillInfo value in skill_info)
                    {
                        LogSystem.Debug("skill id : {0}, skill level : {1}, skill pos : {2}", value.SkillId, value.SkillLevel, (SlotPosition)value.Postions.Presets[0]);
                    }
                }
                CharacterProperty info = playerself.GetActualProperty();
                if (null != info)
                {
                    LogSystem.Debug("PlayerInfo ||| HpMax : {0}, MpMax : {1}, Ad : {2}, ADp : {3}, Mdp : {4}, CriRate : {5}, CriDamage : {6}, bHitDR : {7}, cHitDR : {8}, fireDam : {9}, iceDam : {10}, poisonDam : {11}, fireERD : {12}, iceERD : {13}, poisonERD : {14}, ||| Level : {15}, Money : {16}, Gold : {17}, Stamina : {18}, AttrScore : {19}, CurExp : {20}",
                      info.HpMax, info.EnergyMax, info.AttackBase, info.ADefenceBase, info.MDefenceBase,
                      info.Critical, info.CriticalPow, info.CriticalBackHitPow, info.CriticalCrackPow,
                      info.FireDamage, info.IceDamage, info.PoisonDamage, info.FireERD, info.IceERD, info.PoisonERD,
                      playerself.GetLevel(), LobbyClient.Instance.CurrentRole.Money, LobbyClient.Instance.CurrentRole.Gold, LobbyClient.Instance.CurrentRole.CurStamina, playerself.FightingScore, LobbyClient.Instance.CurrentRole.Exp);
                    LogSystem.Debug("PlayerInfo ||| MpMax : {0}, Mp : {1}", info.EnergyMax, playerself.Energy);
                }
                CharacterInfo partner = WorldSystem.Instance.GetCharacterById(playerself.PartnerId);
                if (null != partner)
                {
                    CharacterProperty partnerInfo = partner.GetActualProperty();
                    if (null != partnerInfo)
                    {
                        LogSystem.Debug("PartnerpartnerInfo ||| HpMax : {0}, MpMax : {1}, Ad : {2}, ADp : {3}, Mdp : {4}, CriRate : {5}, CriDamage : {6}, bHitDR : {7}, cHitDR : {8}, fireDam : {9}, iceDam : {10}, poisonDam : {11}, fireERD : {12}, iceERD : {13}, poisonERD : {14}",
                          partnerInfo.HpMax, partnerInfo.EnergyMax, partnerInfo.AttackBase, partnerInfo.ADefenceBase, partnerInfo.MDefenceBase,
                          partnerInfo.Critical, partnerInfo.CriticalPow, partnerInfo.CriticalBackHitPow, partnerInfo.CriticalCrackPow,
                          partnerInfo.FireDamage, partnerInfo.IceDamage, partnerInfo.PoisonDamage, partnerInfo.FireERD, partnerInfo.IceERD, partnerInfo.PoisonERD);
                    }
                }
            }
        }

        private void BuyStamina(int key_code, int what)
        {
            if ((int)Keyboard.Event.Up == what)
            {
                WorldSystem.Instance.SwitchDebug();
            }
        }

        private void SwitchDebug(int key_code, int what)
        {
            if ((int)Keyboard.Event.Up == what)
            {
                WorldSystem.Instance.SwitchDebug();
            }
        }

        private void SwitchObserver(int key_code, int what)
        {
            if ((int)Keyboard.Event.Up == what)
            {
                LobbyNetworkSystem.Instance.BuyStamina();
                //WorldSystem.Instance.SwitchObserver();
            }
        }

        private void InteractObject(int key_code, int what)
        {
            UserInfo myself = WorldSystem.Instance.GetPlayerSelf();
            if (myself != null && myself.IsDead())
            {
            }
            WorldSystem.Instance.InteractObject();
        }

        private void PrintPosition(int key_code, int what)
        {
            UserInfo myself = WorldSystem.Instance.GetPlayerSelf();
            if (null != myself && what == (int)Keyboard.Event.Down)
            {
                Vector3 pos = myself.GetMovementStateInfo().GetPosition3D();
                float dir = myself.GetMovementStateInfo().GetFaceDir();
                LogSystem.Debug("PrintPosition {0:F2} {1:F2} {2:F2} {3:F2}", pos.X, pos.Y, pos.Z, dir);
            }
        }

        private void StopFindPath(UserInfo playerself, UserAiStateInfo aiInfo)
        {
            if (null == playerself || null == aiInfo)
            {
                return;
            }
            AiData_UserSelf_General data = playerself.GetAiStateInfo().AiDatas.GetData<AiData_UserSelf_General>();
            if (null == data)
            {
                data = new AiData_UserSelf_General();
                playerself.GetAiStateInfo().AiDatas.AddData(data);
            }
            playerself.GetMovementStateInfo().IsMoving = false;
            aiInfo.Time = 0;
            data.Time = 0;
            data.FoundPath.Clear();
            aiInfo.ChangeToState((int)AiStateId.Idle);
        }

        private void UpdateMoveState(UserInfo playerself, Vector3 targetpos, float towards)
        {
            CharacterView view = EntityManager.Instance.GetUserViewById(playerself.GetId());
            if (view != null && view.ObjectInfo.IsGfxMoveControl && Geometry.IsSamePoint(Vector3.Zero, targetpos))
            {
                //LogSystem.Debug("UpdateMoveState IsGfxMoveControl : {0} , targetpos : {1}", view.ObjectInfo.IsGfxMoveControl, targetpos.ToString());
                return;
            }
            PlayerMovement.Motion m = Geometry.IsSamePoint(Vector3.Zero, targetpos) ? PlayerMovement.Motion.Stop : PlayerMovement.Motion.Moving;
            pm_.JoyStickMotionChanged = pm_.JoyStickMotionStatus != m || !Geometry.IsSameFloat(m_lastDir, towards);
            pm_.JoyStickMotionStatus = m;
            pm_.MoveDir = towards;
            if (Geometry.IsSamePoint(Vector3.Zero, targetpos))
            {
                pm_.JoyStickMotionStatus = PlayerMovement.Motion.Stop;
                m_IsJoystickControl = false;
            }
            else
            {
                m_IsJoystickControl = true;
            }
        }

        internal void MovePosition(Vector3 targetpos)
        {
            UserInfo user = WorldSystem.Instance.GetPlayerSelf();
            if (null != user)
            {
                if (!user.UnityPathFinding)
                {
                    List<Vector3> waypoints = user.SpatialSystem.FindPath(user.GetMovementStateInfo().GetPosition3D(), targetpos, 1);
                    waypoints.Add(targetpos);
                    UserAiStateInfo aiInfo = user.GetAiStateInfo();
                    AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                    if (null == data)
                    {
                        data = new AiData_ForMoveCommand(waypoints);
                        aiInfo.AiDatas.AddData(data);
                    }
                    data.WayPoints = waypoints;
                    data.Index = 0;
                    data.EstimateFinishTime = 0;
                    data.IsFinish = false;
                    aiInfo.ChangeToState((int)AiStateId.MoveCommand);
                }
                else
                {
                    UserAiStateInfo aiInfo = user.GetAiStateInfo();
                    AiData_ForMoveCommand data = aiInfo.AiDatas.GetData<AiData_ForMoveCommand>();
                    if (null == data)
                    {
                        data = new AiData_ForMoveCommand(null);
                        aiInfo.AiDatas.AddData(data);
                    }

                    user.PathFindingFinished = false;
                    user.GetAiStateInfo().ChangeToState((int)AiStateId.PathFinding);
                    user.GetAiStateInfo().PreviousState = (int)AiStateId.MoveCommand;
                    GfxSystem.ForMoveCommandPathToTarget(user, targetpos);
                }
            }
        }

        private void FindPath(Vector3 targetpos)
        {
            /*
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (playerself != null && !playerself.IsDead())
            {
                if (Vector3.Zero == targetpos)
                {
                    return;
                }
                UserAiStateInfo aiInfo = playerself.GetAiStateInfo();
                if (null != aiInfo)
                {
                    StopFindPath(playerself, aiInfo);
                }
                aiInfo.TargetPos = targetpos;
                aiInfo.ChangeToState((int)AiStateId.Move);
            }
            */
            MovePosition(targetpos);
        }

        private void UpdateTowards(UserInfo playerself, float towards)
        {
            if (null != playerself && float.NegativeInfinity != towards)
            {
                playerself.GetMovementStateInfo().SetFaceDir(towards);
                playerself.GetMovementStateInfo().SetWantFaceDir(towards);
                playerself.GetMovementStateInfo().SetMoveDir(towards);
            }
        }

        private void RevokeSkill(UserInfo playerself, UserAiStateInfo aiInfo)
        {
            if (null == playerself || null == aiInfo)
            {
                return;
            }
            StopFindPath(playerself, aiInfo);
            aiInfo.Target = 0;
            aiInfo.IsAttacked = false;
            aiInfo.Time = 0;
            aiInfo.TargetPos = Vector3.Zero;
            aiInfo.AttackRange = 0;
            aiInfo.ChangeToState((int)AiStateId.Idle);
        }

        private void PushSkill(UserInfo playerself, Vector3 targetpos, float attackrange)
        {
            if (null != playerself && Vector3.Zero != targetpos)
            {
                UserAiStateInfo info = playerself.GetAiStateInfo();
                RevokeSkill(playerself, info);
                info.Time = 0;
                info.TargetPos = targetpos;
                info.AttackRange = attackrange;
                info.IsAttacked = false;
                info.ChangeToState((int)AiStateId.Combat);
            }
        }

        private void Combat(UserInfo playerself, int targetId, float attackrange)
        {
            if (null != playerself && m_lastSelectObjId != targetId)
            {
                UserAiStateInfo info = playerself.GetAiStateInfo();
                if ((int)AiStateId.Move == info.CurState)
                {
                    StopFindPath(playerself, info);
                }
                info.Time = 0;
                info.Target = targetId;
                info.IsAttacked = false;
                info.AttackRange = attackrange;

                info.ChangeToState((int)AiStateId.Combat);
            }
        }

        private void TouchHandle(int what, GestureArgs e)
        {
            try
            {
                UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
                if (null == playerself || null == e)
                    return;
                CharacterView view = EntityManager.Instance.GetUserViewById(playerself.GetId());
                if (null != playerself.SkillController &&
                  playerself.SkillController.GetSkillControlMode() != SkillControlMode.kTouch)
                {
                    return;
                }
                if ((int)TouchEvent.Cesture == what)
                {
                    string ename = e.name;
                    if (GestureEvent.OnSingleTap.ToString() == ename)
                    {
                        if (EnableMoveInput)
                        {
                            if (WorldSystem.Instance.IsPureClientScene() || WorldSystem.Instance.IsPveScene())
                            {
                                if (e.selectedObjID < 0)
                                {
                                    playerself.SkillController.AddBreakSkillTask();
                                    if (InputType.Touch == e.inputType)
                                    {
                                        FindPath(new Vector3(e.airWelGamePosX, e.airWelGamePosY, e.airWelGamePosZ));
                                    }
                                    else if (InputType.Joystick == e.inputType)
                                    {
                                        UpdateMoveState(playerself, new Vector3(e.airWelGamePosX, e.airWelGamePosY, e.airWelGamePosZ), e.towards);
                                    }
                                    UpdateTowards(playerself, e.towards);
                                }
                                else
                                {
                                    Combat(playerself, e.selectedObjID, e.attackRange);
                                }
                                m_lastSelectObjId = e.selectedObjID;
                                ///
                                // GfxSystem.PublishGfxEvent("Op_InputEffect", "Input", GestureEvent.OnSingleTap, e.airWelGamePosX, e.airWelGamePosY, e.airWelGamePosZ, e.selectedObjID < 0 ? false : true, true);
                            }
                            else if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
                            {
                                if (e.selectedObjID < 0)
                                {
                                    if (InputType.Touch == e.inputType)
                                    {
                                        NetworkSystem.Instance.SyncPlayerMoveToPos(new Vector3(e.airWelGamePosX, e.airWelGamePosY, e.airWelGamePosZ));
                                    }
                                    else if (InputType.Joystick == e.inputType)
                                    {
                                        UpdateMoveState(playerself, new Vector3(e.airWelGamePosX, e.airWelGamePosY, e.airWelGamePosZ), e.towards);
                                        playerself.SkillController.AddBreakSkillTask();
                                    }
                                }
                                else
                                {
                                    NetworkSystem.Instance.SyncPlayerMoveToAttack(e.selectedObjID, e.attackRange);
                                }
                            }
                        }
                    }
                    else if (GestureEvent.OnFingerMove.ToString() == ename)
                    {
                        if (EnableMoveInput)
                        {
                            if (view.ObjectInfo.IsGfxMoveControl && !view.ObjectInfo.IsGfxRotateControl)
                            {
                                UpdateTowards(playerself, e.towards);
                            }
                        }
                    }
                    else if (GestureEvent.OnSkillStart.ToString() == ename)
                    {
                        if (null != playerself)
                        {
                            UserAiStateInfo info = playerself.GetAiStateInfo();
                            if ((int)AiStateId.Move == info.CurState)
                            {
                                StopFindPath(playerself, info);
                            }
                        }
                    }
                    else if (GestureEvent.OnEasyGesture.ToString() == ename)
                    {
                        Vector3 targetpos = new Vector3(e.startGamePosX, e.startGamePosY, e.startGamePosZ);
                        if (Vector3.Zero != targetpos)
                        {
                            PushSkill(playerself, targetpos, e.attackRange);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogicSystem.LogFromGfx("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        // members
        private bool m_IsJoystickControl = false;
        private float m_lastDir = -1f;
        private float m_lastMoveDir = -1f;
        private bool m_LastTickIsMoving = false;
        private bool m_LastTickIsSkillMoving = false;
        private int m_lastSelectObjId = -1;
        private long m_LastTickTime = 0;
        private long m_LastTickIntervalMs = 0;
        private PlayerMovement pm_;

        private Vector2 old_mouse_pos_;
        private Vector2 mouse_pos_;

        private float c_2PI = (float)Math.PI * 2;
    }

    class PlayerMovement
    {
        private enum KeyIndex
        {
            W = 0,
            A,
            S,
            D
        }
        internal enum KeyHit
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8,
            Other = 16,
        }

        internal enum Motion
        {
            Moving,
            Stop,
        }

        internal PlayerMovement()
        {
            MoveDir = 0;
            MotionStatus = Motion.Stop;
            MotionChanged = false;
            JoyStickMotionStatus = Motion.Stop;
            JoyStickMotionChanged = false;
            last_key_hit_ = KeyHit.None;
        }

        internal void Update(bool move_enable)
        {
            UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
            if (playerself == null || playerself.IsDead())
            {
                return;
            }
            KeyHit kh = KeyHit.None;
            if (move_enable)
            {
                if (ArkCrossEngineSpatial.SpatialObjType.kNPC == playerself.GetRealControlledObject().SpaceObject.GetObjType())
                {
                    NpcInfo npcInfo = playerself.GetRealControlledObject().CastNpcInfo();
                    if (null != npcInfo)
                    {
                        if (!npcInfo.CanMove)
                        {
                            return;
                        }
                    }
                }
                if (GfxSystem.IsKeyPressed(GetKeyCode(KeyIndex.W)))
                    kh |= KeyHit.Up;
                if (GfxSystem.IsKeyPressed(GetKeyCode(KeyIndex.A)))
                    kh |= KeyHit.Left;
                if (GfxSystem.IsKeyPressed(GetKeyCode(KeyIndex.S)))
                    kh |= KeyHit.Down;
                if (GfxSystem.IsKeyPressed(GetKeyCode(KeyIndex.D)))
                    kh |= KeyHit.Right;
            }

            Motion m = kh == KeyHit.None ? Motion.Stop : Motion.Moving;
            MotionChanged = MotionStatus != m || last_key_hit_ != kh;

            if (MotionChanged)
            {
                //GfxSystem.GfxLog("MotionChanged:{0}!={1} || {2}!={3}", MotionStatus, m, last_key_hit_, kh);
            }

            last_key_hit_ = kh;
            MotionStatus = m;
            MoveDir = CalcMoveDir(kh);
            if (MoveDir < 0)
            {
                MotionStatus = Motion.Stop;
            }
            if (MotionChanged)
            {
                //GfxSystem.GfxLog("InputMoveDir:{0} Pos:{1}", MoveDir, playerself.GetMovementStateInfo().GetPosition3D().ToString());
            }
        }

        internal float MoveDirOffset = 0.0f;
        internal float _moveDir;
        internal float MoveDir
        {
            get
            {
                return _moveDir + MoveDirOffset;
            }
            set
            {
                _moveDir = value;
            }
        }
        internal Motion MotionStatus { get; set; }
        internal bool MotionChanged { get; set; }
        internal Motion JoyStickMotionStatus { get; set; }
        internal bool JoyStickMotionChanged { get; set; }

        private Keyboard.Code GetKeyCode(KeyIndex index)
        {
            Keyboard.Code ret = Keyboard.Code.W;
            if (index >= KeyIndex.W && index <= KeyIndex.D)
            {
                Keyboard.Code[] list = s_Normal;
                /*if (WorldSystem.Instance.IsPvpScene()) {
                  int campId = WorldSystem.Instance.CampId;
                  if (campId == (int)CampIdEnum.Blue)
                    list = s_Blue;
                  else if (campId == (int)CampIdEnum.Red)
                    list = s_Red;
                }*/
                ret = list[(int)index];
            }
            return ret;
        }

        /**
          * @brief 计算移动方向
          *           0       
          *          /|\
          *           |
          *3π/2 -----*-----> π/2
          *           |
          *           |
          *           |
          *           π
          */
        private float CalcMoveDir(KeyHit kh)
        {
            return s_MoveDirs[(int)kh];
        }

        internal KeyHit last_key_hit_;

        private static readonly Keyboard.Code[] s_Normal = new Keyboard.Code[] { Keyboard.Code.W, Keyboard.Code.A, Keyboard.Code.S, Keyboard.Code.D };
        private static readonly Keyboard.Code[] s_Blue = new Keyboard.Code[] { Keyboard.Code.A, Keyboard.Code.S, Keyboard.Code.D, Keyboard.Code.W };
        private static readonly Keyboard.Code[] s_Red = new Keyboard.Code[] { Keyboard.Code.D, Keyboard.Code.W, Keyboard.Code.A, Keyboard.Code.S };
        //                                                          N   U  D        UD  L            UL           DL
        private static readonly float[] s_MoveDirs = new float[] { -1,  0, (float)Math.PI, -1, 3*(float)Math.PI/2, 7*(float)Math.PI/4, 5*(float)Math.PI/4, 
      //                    UDL          R          UR         DR           UDR        LR  ULR  LRD      UDLR
                            3*(float)Math.PI/2, (float)Math.PI/2, (float)Math.PI/4, 3*(float)Math.PI/4, (float)Math.PI/2, -1, 0,   (float)Math.PI, -1 };
    }
}