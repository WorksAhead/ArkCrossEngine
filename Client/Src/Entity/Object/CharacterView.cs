using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    internal partial class CharacterView
    {
        internal int Actor
        {
            get { return m_Actor; }
        }
        internal int ObjId
        {
            get { return m_ObjId; }
        }
        internal SharedGameObjectInfo ObjectInfo
        {
            get { return m_ObjectInfo; }
        }

        internal bool Visible
        {
            get { return m_Visible; }
            set
            {
                m_Visible = UpdateVisible(value);
            }
        }

        internal bool CanAffectPlayerSelf
        {
            get { return m_CanAffectPlayerSelf || WorldSystem.Instance.IsObserver; }
            set { m_CanAffectPlayerSelf = value; }
        }

        internal void PlayParticle(string particlename, bool enable)
        {
            if (enable && !CanAffectPlayerSelf) return;
        }

        internal void PlayParticle(string particlename, float x, float y, float z, float duration)
        {
            if (!CanAffectPlayerSelf) return;
            TemporaryEffectArgs args = new TemporaryEffectArgs(particlename, duration, x, y, z);
            GfxSystem.SendMessage("GfxGameRoot", "AddTemporaryEffect", args);
        }

        internal void AddAttachedObject(int objectId, string path)
        {
            GfxSystem.AttachGameObject(objectId, m_Actor, path);
        }

        internal void RemoveAttachedObject(int objectId, bool isDestroy = false)
        {
            GfxSystem.DetachGameObject(objectId);
            if (isDestroy)
            {
                GfxSystem.DestroyGameObject(objectId);
            }
        }
        internal void PlaySound(string filename, Vector3 pos)
        {
            if (!CanAffectPlayerSelf) return;
        }

        internal void SetVisible(bool bVis, string model = null)
        {
            if (m_Actor == 0)
            {
                return;
            }
        }

        internal void SetMaterial(string material_name)
        {
            if (!CanAffectPlayerSelf) return;
        }

        internal void ResetMaterial()
        {
        }

        protected virtual void CreateActor(int objId, string model, Vector3 pos, float dir, float scale = 1.0f)
        {
            Init();

            m_ObjId = objId;
            m_Actor = GameObjectIdManager.Instance.GenNextId();
            m_ObjectInfo.m_ActorId = m_Actor;
            m_ObjectInfo.m_LogicObjectId = objId;
            m_ObjectInfo.X = pos.X;
            m_ObjectInfo.Y = pos.Y;
            m_ObjectInfo.Z = pos.Z;
            m_ObjectInfo.FaceDir = dir;
            m_ObjectInfo.Sx = scale;
            m_ObjectInfo.Sy = scale;
            m_ObjectInfo.Sz = scale;
            m_ObjectInfo.HitSounds = GetOwner().GetHitSounds();
            GfxSystem.CreateGameObject(m_Actor, model, m_ObjectInfo);
        }

        protected void CreateBornEffect(int parentActor, string effect, float scale, float time)
        {
            if (!String.IsNullOrEmpty(effect))
            {
                GfxSystem.CreateAndAttachParticle(effect, parentActor, "", scale, time);
            }
        }

        protected void DestroyActor()
        {
            GfxSystem.DestroyGameObject(m_Actor);
            Release();
        }

        protected virtual bool UpdateVisible(bool visible)
        {
            GfxSystem.SetGameObjectVisible(m_Actor, visible);
            return visible;
        }

        internal void OnCombat2IdleSkillOver()
        {
            m_IsCombatState = false;
            m_IsWeaponMoved = false;
            m_IdleState = IdleState.kNotIdle;
        }

        internal void UpdateLastLeaveCombatTime()
        {
            long now = TimeUtility.GetServerMilliseconds();
            m_LastLeaveCombatTime = now;
        }

        protected bool IsInCombatState()
        {
            SkillStateInfo state = GetOwner().GetSkillStateInfo();
            if (state == null)
            {
                return false;
            }
            if (state.IsSkillActivated() && state.GetCurSkillInfo().SkillId != GetOwner().Combat2IdleSkill)
            {
                return true;
            }
            if (state.IsImpactActive())
            {
                return true;
            }
            return false;
        }

        protected void UpdateState()
        {
            if (GetOwner().IsDead())
            {
                return;
            }
            long now = TimeUtility.GetServerMilliseconds();
            if (IsInCombatState())
            {
                m_LastLeaveCombatTime = now;
                m_IsCombat2IdleChanging = false;
                m_IsCombatState = true;
            }
            else if (m_IsCombatState)
            {
                if (GetOwner().GetMovementStateInfo().IsMoving)
                {
                    m_LastLeaveCombatTime = now;
                    m_IsCombat2IdleChanging = false;
                }
            }
            if (GetOwner().GetId() == WorldSystem.Instance.GetPlayerSelf().GetId())
            {
                if (m_LastLeaveCombatTime + GetOwner().Combat2IdleTime * GetOwner().Combat2IdleCoefficient * 1000 <= now && !m_IsCombat2IdleChanging)
                {
                    GetOwner().SkillController.PushSkill(SkillCategory.kCombat2Idle, Vector3.Zero);
                    m_IsCombat2IdleChanging = true;
                    GetOwner().ResetCombat2IdleCoefficient();
                }
            }
            if (m_IsCombatState && !m_IsWeaponMoved)
            {
                EnterCombatState();
            }
        }

        internal void EnterCombatState()
        {
            m_IsCombat2IdleChanging = false;
            m_IsCombatState = true;
            string[] weapon_moves = GetOwner().Idle2CombatWeaponMoves.Split('|');
            for (int i = 1; i < weapon_moves.Length; i += 2)
            {
                string child = weapon_moves[i - 1];
                string node = weapon_moves[i];
                GfxSystem.QueueGfxAction(GfxModule.Skill.Trigers.TriggerUtil.MoveChildToNode, Actor, child, node);
            }
            m_IsWeaponMoved = true;
        }

        protected void UpdateMovement()
        {
            CharacterInfo obj = GetOwner();
            if (null != obj && !obj.IsDead() && null != ObjectInfo)
            {
                if (obj.IsNpc && !obj.CastNpcInfo().CanMove) return;
                MovementStateInfo msi = obj.GetMovementStateInfo();
                ObjectInfo.FaceDir = msi.GetFaceDir();
                ObjectInfo.WantFaceDir = msi.GetWantFaceDir();
                if (msi.IsMoving)
                {
                    Vector3 pos = msi.GetPosition3D();
                    ObjectInfo.MoveCos = (float)msi.MoveDirCosAngle;
                    ObjectInfo.MoveSin = (float)msi.MoveDirSinAngle;
                    ObjectInfo.MoveSpeed = (float)obj.GetActualProperty().MoveSpeed * (float)obj.VelocityCoefficient;

                    if (obj is UserInfo)
                    {
                        if (msi.TargetPosition.LengthSquared() < Geometry.c_FloatPrecision)
                        {
                            ObjectInfo.MoveTargetDistanceSqr = 100.0f;
                        }
                        else
                        {
                            ObjectInfo.MoveTargetDistanceSqr = msi.CalcDistancSquareToTarget();
                        }
                    }
                    else
                    {
                        ObjectInfo.MoveTargetDistanceSqr = msi.CalcDistancSquareToTarget();
                    }

                    ObjectInfo.IsLogicMoving = true;
                }
                else
                {
                    ObjectInfo.IsLogicMoving = false;
                }
            }
            else
            {
                ObjectInfo.IsLogicMoving = false;
            }
        }

        protected void UpdateAffectPlayerSelf(Vector3 pos)
        {
            if (null != WorldSystem.Instance.GetPlayerSelf())
            {
                Vector3 myselfPos = WorldSystem.Instance.GetPlayerSelf().GetMovementStateInfo().GetPosition3D();
                if (Geometry.DistanceSquare(pos, myselfPos) < c_AffectPlayerSelfDistanceSquare)
                {
                    CanAffectPlayerSelf = true;
                }
                else
                {
                    CanAffectPlayerSelf = false;
                }
            }
            else
            {
                CanAffectPlayerSelf = false;
            }
        }

        private void Init()
        {
            m_NormalColor = new Vector4(1, 1, 1, 1);
            m_BurnColor = new Vector4(0.75f, 0.2f, 0.2f, 1);
            m_FrozonColor = new Vector4(0.2f, 0.2f, 0.75f, 1);
            m_ShineColor = new Vector4(0.2f, 0.75f, 0.2f, 1);
            m_Actor = 0;

            m_CurActionConfig = null;
        }

        private void Release()
        {
            List<string> keyList = new List<string>(effect_map_.Keys);
            if (keyList != null && keyList.Count > 0)
            {
                foreach (string model in keyList)
                {
                    //DetachActor(model);
                }
            }
            CurWeaponList.Clear();
        }

        internal List<string> CurWeaponList
        {
            get
            {
                return m_CurWeaponName;
            }
        }

        internal string Cylinder
        {
            get { return c_CylinderName; }
        }

        private int m_Actor = 0;
        private int m_ObjId = 0;
        private SharedGameObjectInfo m_ObjectInfo = new SharedGameObjectInfo();

        protected long m_LastLeaveCombatTime = 0;
        protected bool m_IsCombat2IdleChanging = true;
        protected bool m_IsWeaponMoved = false;

        private const string c_CylinderName = "1_Cylinder";
        private const float c_AffectPlayerSelfDistanceSquare = 900;
        private List<string> m_CurWeaponName = new List<string>();

        private bool m_Visible = true;
        private bool m_CanAffectPlayerSelf = true;

        private Vector4 m_NormalColor = new Vector4(1, 1, 1, 1);
        private Vector4 m_BurnColor = new Vector4(0.75f, 0.2f, 0.2f, 1);
        private Vector4 m_FrozonColor = new Vector4(0.2f, 0.2f, 0.75f, 1);
        private Vector4 m_ShineColor = new Vector4(0.2f, 0.75f, 0.2f, 1);
        private Dictionary<string, uint> effect_map_ = new Dictionary<string, uint>();
    }
}
