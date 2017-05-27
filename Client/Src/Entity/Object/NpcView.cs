using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    internal class NpcView : CharacterView
    {
        internal NpcInfo Npc
        {
            get { return m_Npc; }
        }

        protected override CharacterInfo GetOwner()
        {
            return m_Npc;
        }

        internal void Create(NpcInfo npc)
        {
            Init();
            if (null != npc)
            {
                m_Npc = npc;
                ObjectInfo.UnitId = m_Npc.GetUnitId();
                ObjectInfo.CampId = m_Npc.GetCampId();
                ObjectInfo.LinkId = m_Npc.GetLinkId();
                MovementStateInfo msi = m_Npc.GetMovementStateInfo();
                Vector3 pos = msi.GetPosition3D();
                float dir = msi.GetFaceDir();
                CreateActor(m_Npc.GetId(), m_Npc.GetModel(), pos, dir, m_Npc.Scale);
                ObjectInfo.IsNpc = true;
                CreateBornEffect(Actor, npc.GetBornEffect(), npc.ParticleScale, npc.GetBornEffectTime());
                InitAnimationSets();
            }
        }
        protected override void CreateActor(int objId, string model, Vector3 pos, float dir, float scale = 1.0f)
        {
            base.CreateActor(objId, model, pos, dir, scale);
            ObjectInfo.IsNpc = true;
            if (Npc.NpcType == (int)NpcTypeEnum.AutoPickItem)
            {
                ObjectInfo.IsFloat = true;
            }
        }
        internal void Destroy()
        {
            Release();
            DestroyActor();
        }

        internal void Update()
        {
            UpdateAttr();
            UpdateSpatial();
            UpdateAnimation();
            UpdateSuperArmor();
        }

        protected override bool UpdateVisible(bool visible)
        {
            bool ret = visible;
            if (null != m_Npc)
            {
                if (visible)
                {
                    if (m_Npc.IsDead())
                    {
                        SetVisible(false);
                    }
                    else
                    {
                        SetVisible(true);
                    }
                }
                else
                {
                    SetVisible(false);
                }
            }
            else
            {
                SetVisible(false);
                ret = false;
            }
            return ret;
        }

        private void UpdateAttr()
        {
            if (null != m_Npc)
            {
                ObjectInfo.Blood = m_Npc.Hp;
                ObjectInfo.MaxBlood = m_Npc.GetActualProperty().HpMax;
                ObjectInfo.Energy = m_Npc.Energy;
                ObjectInfo.MaxEnergy = m_Npc.GetActualProperty().EnergyMax;
                ObjectInfo.FightingScore = m_Npc.FightingScore;
                ObjectInfo.CanHitMove = m_Npc.CanHitMove;
                ObjectInfo.AcceptStiffEffect = m_Npc.AcceptStiffEffect;
                ObjectInfo.IsSuperArmor = (m_Npc.SuperArmor || m_Npc.UltraArmor);
                m_Npc.GfxStateFlag = ObjectInfo.GfxStateFlag;
            }
        }

        private void UpdateSpatial()
        {
            if (null != m_Npc)
            {
                MovementStateInfo msi = m_Npc.GetMovementStateInfo();
                if (ObjectInfo.IsGfxMoveControl)
                {
                    if (ObjectInfo.DataChangedByGfx)
                    {
                        msi.PositionX = ObjectInfo.X;
                        msi.PositionY = ObjectInfo.Y;
                        msi.PositionZ = ObjectInfo.Z;
                        msi.SetFaceDir(ObjectInfo.FaceDir);
                        ObjectInfo.DataChangedByGfx = false;
                    }
                    if (ObjectInfo.WantDirChangedByGfx)
                    {
                        msi.SetWantFaceDir(ObjectInfo.WantFaceDir);
                        ObjectInfo.WantDirChangedByGfx = false;
                    }
                }
                else
                {
                    if (ObjectInfo.DataChangedByGfx)
                    {
                        msi.PositionX = ObjectInfo.X;
                        msi.PositionY = ObjectInfo.Y;
                        msi.PositionZ = ObjectInfo.Z;
                        //msi.SetFaceDir(ObjectInfo.FaceDir);

                        ObjectInfo.DataChangedByGfx = false;
                    }
                    SimulateMove();
                    UpdateMovement();
                }
                ObjectInfo.WantFaceDir = msi.GetWantFaceDir();
            }
        }

        protected void SimulateMove()
        {
            if (!m_Npc.IsSimulateMove)
            {
                return;
            }
            if (m_Npc.SummonOwnerId < 0)
            {
                return;
            }
            CharacterInfo owner = WorldSystem.Instance.GetCharacterById(m_Npc.SummonOwnerId);
            if (owner == null)
            {
                return;
            }
            CharacterView owner_view = EntityManager.Instance.GetCharacterViewById(m_Npc.SummonOwnerId);
            if (owner_view == null)
            {
                return;
            }
            MovementStateInfo msi = m_Npc.GetMovementStateInfo();
            MovementStateInfo sim_msi = owner.GetMovementStateInfo();
            m_Npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, owner.GetActualProperty().MoveSpeed);
            m_Npc.VelocityCoefficient = owner.VelocityCoefficient;
            if (owner_view.ObjectInfo.IsGfxMoveControl)
            {
                msi.IsMoving = false;
            }
            else
            {
                msi.IsMoving = sim_msi.IsMoving;
            }
            msi.SetFaceDir(sim_msi.GetFaceDir());
            msi.SetWantFaceDir(sim_msi.GetWantFaceDir());
            msi.SetMoveDir(sim_msi.GetMoveDir());
        }

        protected void SimulateState()
        {
            CharacterInfo owner = WorldSystem.Instance.GetCharacterById(m_Npc.SummonOwnerId);
            if (owner == null)
            {
                return;
            }
            CharacterView owner_view = EntityManager.Instance.GetCharacterViewById(m_Npc.SummonOwnerId);
            if (owner_view == null)
            {
                return;
            }
            if (m_IsCombatState != owner_view.IsCombatState)
            {
                if (!m_IsCombatState)
                {
                    EnterCombatState();
                }
            }
        }

        private void UpdateSuperArmor()
        {
            if (null != m_Npc)
            {
                if (m_Npc.IsCombatNpc())
                {
                    if (m_Npc.SuperArmor || m_Npc.UltraArmor)
                    {
                        if (ObjectInfo.m_ShaderPath != "DFM/Basic Outline")
                        {
                            GfxSystem.SetShader(Actor, "DFM/Basic Outline");
                            ObjectInfo.m_ShaderPath = "DFM/Basic Outline";
                        }
                    }
                    else
                    {
                        if (ObjectInfo.m_ShaderPath != "DFM/NormalMonster")
                        {
                            GfxSystem.SetShader(Actor, "DFM/NormalMonster");
                            ObjectInfo.m_ShaderPath = "DFM/NormalMonster";
                        }
                    }
                    m_Npc.IsArmorChanged = false;
                }
            }
        }

        private void UpdateAnimation()
        {
            if (!CanAffectPlayerSelf) return;
            UpdateStoryAnim();
            if (IsPlayingStoryAnim())
            {
                return;
            }
            if (null != m_Npc)
            {
                if (m_Npc.IsSimulateMove)
                {
                    SimulateState();
                }
                if (ObjectInfo.IsGfxAnimation)
                {
                    m_CharacterAnimationInfo.Reset();
                    m_IdleState = IdleState.kNotIdle;
                    return;
                }

                if (m_Npc.CanControl && m_Npc.ControllerObject == null)
                {
                    PlayAnimation(Animation_Type.AT_SLEEP);
                    return;
                }

                if (m_Npc.NpcType == (int)NpcTypeEnum.InteractiveNpc && m_Npc.IsHaveStateFlag(CharacterState_Type.CST_Opened))
                {
                    PlayAnimation(Animation_Type.AT_SkillSection2);
                    return;
                }

                if (m_Npc.IsBorning)
                {
                    UpdateBornAnimation();
                }

                if (m_Npc.CanMove)
                {
                    UpdateMoveAnimation();
                }
                UpdateTaunt();
                UpdateDead();
                UpdateIdle();
            }
        }

        protected override void UpdateIdle()
        {
            if ((int)AiStateId.Idle == m_Npc.GetAiStateInfo().CurState && !m_Npc.IsSimulateMove)
            {
                if (!GetOwner().IsDead() && m_CharacterAnimationInfo.IsIdle())
                {
                    if (m_IdleState == IdleState.kNotIdle)
                    {
                        PlayAnimation(Animation_Type.AT_Stand);
                        m_IdleState = IdleState.kBegin;
                        m_BeginIdleTime = TimeUtility.GetServerMilliseconds();
                        m_IdleInterval = CrossEngineHelper.Random.Next(3000, 7000);
                    }
                    else if (m_IdleState == IdleState.kBegin)
                    {
                        if (TimeUtility.GetServerMilliseconds() - m_BeginIdleTime > m_IdleInterval)
                        {
                            PlayAnimation(GetNextIdleAnim());
                            PlayQueuedAnimation(Animation_Type.AT_IdelStand);
                            m_BeginIdleTime = TimeUtility.GetServerMilliseconds();
                            m_IdleInterval = CrossEngineHelper.Random.Next(3000, 7000);
                        }
                    }
                    else
                    {
                        m_IdleState = IdleState.kNotIdle;
                    }
                }
            }
            else
            {
                base.UpdateIdle();
            }
        }

        private Animation_Type GetNextIdleAnim()
        {
            if (m_IdleAnimDict.Count > 0)
            {
                return m_IdleAnimDict[CrossEngineHelper.Random.Next(0, m_IdleAnimDict.Count)];
            }
            else
            {
                return Animation_Type.AT_None;
            }
        }

        internal void SetIdleAnim(List<int> anims)
        {
            foreach (int anim in anims)
            {
                if (!m_IdleAnimDict.Contains((Animation_Type)anim))
                {
                    m_IdleAnimDict.Add((Animation_Type)anim);
                }
            }
        }

        private void UpdateTaunt()
        {
            if (!m_Npc.IsDead())
            {
                if (m_Npc.IsTaunt)
                {
                    if (!m_CharacterAnimationInfo.IsPlayTaunt)
                    {
                        m_CharacterAnimationInfo.IsPlayTaunt = true;
                        PlayAnimation(Animation_Type.AT_Taunt);
                        int tauntSoundCount = m_Npc.TauntSound.Count;
                        if (tauntSoundCount > 0)
                        {
                            string sound = m_Npc.TauntSound[CrossEngineHelper.Random.Next(tauntSoundCount)];
                            if (!String.IsNullOrEmpty(sound))
                            {
                                GfxSystem.PlaySound(Actor, sound, m_Npc.GetMovementStateInfo().GetFaceDir());
                            }
                        }
                    }
                }
                else
                {
                    if (m_CharacterAnimationInfo.IsPlayTaunt)
                    {
                        m_CharacterAnimationInfo.IsPlayTaunt = false;
                        StopAnimation(Animation_Type.AT_Taunt);
                    }
                }
            }
        }
        private void UpdateBornAnimation()
        {
            if (m_CurActionConfig == null || Actor == 0)
            {
                return;
            }
            if (m_Npc.BornAnimTimeMs == 0)
            {
                return;
            }
            PlayAnimation(Animation_Type.AT_Born);
        }


        private void Init()
        {
            m_CurActionConfig = null;
            m_IdleAnimDict.Clear();
            m_Npc = null;
        }

        private void Release()
        {
            if (ObjectInfo.SummonOwnerActorId > 0)
            {
                CharacterInfo owner = WorldSystem.Instance.GetCharacterById(m_Npc.SummonOwnerId);
                if (owner != null)
                {
                    owner.GetSkillStateInfo().RecyleSummonObject(m_Npc);
                }
                CharacterView owner_view = EntityManager.Instance.GetCharacterViewById(m_Npc.SummonOwnerId);
                if (owner_view != null)
                {
                    owner_view.ObjectInfo.Summons.Remove(Actor);
                }
            }
        }

        private NpcInfo m_Npc = null;
        private List<Animation_Type> m_IdleAnimDict = new List<Animation_Type>();
    }
}
