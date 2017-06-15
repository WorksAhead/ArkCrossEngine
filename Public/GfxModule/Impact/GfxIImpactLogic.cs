using System;
using ArkCrossEngine;
using UnityEngine;

namespace GfxModule.Impact
{
    public enum ImpactMovementType
    {
        SenderDir = 0,
        SenderToTarget = 1,
        Inherit = 2,
    }
    public interface IGfxImpactLogic
    {
        void StartImpact(ImpactLogicInfo logicInfo);
        void Tick(ImpactLogicInfo logicInfo);
        void StopImpact(ImpactLogicInfo logicInfo);
        void OnInterrupted(ImpactLogicInfo logicInfo);
        bool OnOtherImpact(int logicId, ImpactLogicInfo logicInfo, bool isSameImpact);
    }


    public abstract class AbstarctGfxImpactLogic : IGfxImpactLogic
    {

        public virtual void StartImpact(ImpactLogicInfo logicInfo)
        {
        }

        protected void GeneralStartImpact(ImpactLogicInfo logicInfo)
        {
            InitLayer(logicInfo.Target, logicInfo);
            LogicSystem.NotifyGfxAnimationStart(logicInfo.Target, false);
            LogicSystem.NotifyGfxMoveControlStart(logicInfo.Target, logicInfo.ImpactId, false);
            InitMovement(logicInfo);
        }
        public virtual void Tick(ImpactLogicInfo logicInfo)
        {
        }

        public virtual bool OnOtherImpact(int logicId, ImpactLogicInfo logicInfo, bool isSameImpact)
        {
            return true;
        }
        public virtual void UpdateEffect(ImpactLogicInfo logicInfo)
        {
            if (null == logicInfo.Target) return;
            SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(logicInfo.Target);
            if (null != shareInfo && !shareInfo.AcceptStiffEffect) return;
            for (int i = 0; i < logicInfo.EffectList.Count; ++i)
            {
                EffectInfo effectInfo = logicInfo.EffectList[i];
                if (null != effectInfo)
                {
                    if (effectInfo.StartTime < 0 && Time.time > logicInfo.StartTime + effectInfo.DelayTime / 1000)
                    {
                        effectInfo.IsActive = true;
                        effectInfo.StartTime = Time.time;
                        GameObject obj = ResourceSystem.NewObject(effectInfo.Path, effectInfo.PlayTime / 1000) as GameObject;
                        if (null != obj)
                        {
                            if (effectInfo.DelWithImpact)
                            {
                                logicInfo.EffectsDelWithImpact.Add(obj);
                            }
                            if (String.IsNullOrEmpty(effectInfo.MountPoint))
                            {
                                obj.transform.position = logicInfo.Target.transform.position + effectInfo.RelativePoint;
                                UnityEngine.Quaternion q = UnityEngine.Quaternion.Euler(effectInfo.RelativeRotation.x, effectInfo.RelativeRotation.y, effectInfo.RelativeRotation.z);
                                if (effectInfo.RotateWithTarget && null != logicInfo.Sender)
                                {
                                    obj.transform.rotation = UnityEngine.Quaternion.LookRotation(logicInfo.Target.transform.position - logicInfo.Sender.transform.position, UnityEngine.Vector3.up);
                                    obj.transform.rotation = UnityEngine.Quaternion.Euler(obj.transform.rotation.eulerAngles + effectInfo.RelativeRotation);
                                }
                                else
                                {
                                    obj.transform.rotation = q;
                                }
                            }
                            else
                            {
                                Transform parent = LogicSystem.FindChildRecursive(logicInfo.Target.transform, effectInfo.MountPoint);
                                if (null != parent)
                                {
                                    obj.transform.parent = parent;
                                    obj.transform.localPosition = UnityEngine.Vector3.zero;
                                    UnityEngine.Quaternion q = UnityEngine.Quaternion.Euler(ImpactUtility.RadianToDegree(effectInfo.RelativeRotation.x), ImpactUtility.RadianToDegree(effectInfo.RelativeRotation.y), ImpactUtility.RadianToDegree(effectInfo.RelativeRotation.z));
                                    obj.transform.localRotation = q;
                                }
                            }
                        }
                    }
                }
            }
        }
        public virtual void StopImpact(ImpactLogicInfo logicInfo)
        {
        }

        protected void GeneralStopImpact(ImpactLogicInfo logicInfo)
        {
            if (IsLogicDead(logicInfo.Target))
            {
                SetGfxStateFlag(logicInfo.Target, Operate_Type.OT_RemoveBit, GfxCharacterState_Type.Stiffness);
                SetGfxStateFlag(logicInfo.Target, Operate_Type.OT_RemoveBit, GfxCharacterState_Type.HitFly);
                SetGfxStateFlag(logicInfo.Target, Operate_Type.OT_RemoveBit, GfxCharacterState_Type.GetUp);
            }
            else
            {
                ClearGfxStateFlag(logicInfo.Target);
            }
            logicInfo.CustomDatas.Clear();
            //foreach=>for
            for (int i = 0; i < logicInfo.EffectsDelWithImpact.Count; ++i)
            {
                GameObject Obj = logicInfo.EffectsDelWithImpact[i];
                ResourceSystem.RecycleObject(Obj);
            }

            //foreach(GameObject obj in logicInfo.EffectsDelWithImpact){
            //  ResourceSystem.RecycleObject(obj);
            //}
            ResetLayer(logicInfo.Target, logicInfo);
            LogicSystem.NotifyGfxAnimationFinish(logicInfo.Target, false);
            LogicSystem.NotifyGfxMoveControlFinish(logicInfo.Target, logicInfo.ImpactId, false);
            LogicSystem.NotifyGfxStopImpact(logicInfo.Sender, logicInfo.ImpactId, logicInfo.Target);
        }
        public virtual void OnInterrupted(ImpactLogicInfo logicInfo)
        {
            if (IsLogicDead(logicInfo.Target))
            {
                SetGfxStateFlag(logicInfo.Target, Operate_Type.OT_RemoveBit, GfxCharacterState_Type.Stiffness);
                SetGfxStateFlag(logicInfo.Target, Operate_Type.OT_RemoveBit, GfxCharacterState_Type.HitFly);
            }
            else
            {
                ClearGfxStateFlag(logicInfo.Target);
            }
            ResetLayer(logicInfo.Target, logicInfo);
            LogicSystem.NotifyGfxAnimationFinish(logicInfo.Target, false);
            LogicSystem.NotifyGfxMoveControlFinish(logicInfo.Target, logicInfo.ImpactId, false);
        }

        protected bool IsLogicDead(GameObject obj)
        {
            if (null != obj)
            {
                SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(obj);
                {
                    if (null != shareInfo && shareInfo.Blood > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected bool IsGfxDead(GameObject obj)
        {
            if (null != obj)
            {
                SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(obj);
                {
                    if (null != shareInfo)
                    {
                        return shareInfo.IsDead;
                    }
                }
            }
            return true;
        }

        protected void SetGfxDead(GameObject obj, bool isDead)
        {
            if (null != obj)
            {
                SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(obj);
                {
                    if (null != shareInfo)
                    {
                        shareInfo.IsDead = isDead;
                    }
                }
            }
        }
        protected float GetAnimationSpeedByTime(ImpactLogicInfo info, float time)
        {
            return info.AnimationInfo.GetSpeedByTime(time) * info.LockFrameInfo.GetSpeedByTime(time);
        }

        protected float GetLockFrameRate(ImpactLogicInfo info, float time)
        {
            return info.LockFrameInfo.GetSpeedByTime(time);
        }

        protected void PlayAnimation(GameObject obj, Animation_Type anim, float speed = 1.0f, UnityEngine.AnimationBlendMode blendMode = UnityEngine.AnimationBlendMode.Blend)
        {
            Animation animation = obj.GetComponent<Animation>();
            if (null != obj && null != animation)
            {
                string animName = GetAnimationNameByType(obj, anim);
                if (!string.IsNullOrEmpty(animName) && null != animation[animName])
                {
                    if (!animation.IsPlaying(animName))
                    {
                        animation[animName].speed = speed;
                        animation.Play(animName);
                        animation[animName].blendMode = blendMode;
                    }
                }
            }
            else
            {
                if (null == obj)
                {
                    LogSystem.Error("null obj");
                }
                else
                {
                    LogSystem.Error("null anim");
                }
            }
        }

        protected void SetAnimationSpeed(GameObject obj, Animation_Type anim, float speed)
        {
            Animation animation = obj.GetComponent<Animation>();
            if (null != obj && null != animation)
            {
                string animName = GetAnimationNameByType(obj, anim);
                if (!string.IsNullOrEmpty(animName))
                {
                    if (null != animation[animName])
                    {
                        animation[animName].speed = speed;
                    }
                    else
                    {
                        LogSystem.Error("obj" + obj.name + " can't find animation clip" + animName);
                    }
                }
            }
            else
            {
                if (null == obj)
                {
                    LogSystem.Error("null obj");
                }
                else
                {
                    LogSystem.Error("obj " + obj.name + " does not have animation");
                }
            }
        }


        protected void CrossFadeAnimation(GameObject obj, Animation_Type anim, float time = 0.3f, float speed = 1.0f)
        {
            Animation animation = obj.GetComponent<Animation>();
            if (null != obj && null != animation)
            {
                string animName = GetAnimationNameByType(obj, anim);
                if (!string.IsNullOrEmpty(animName) && null != animation[animName])
                {
                    animation[animName].speed = speed;
                    animation.CrossFade(animName, time);
                }
            }
            else
            {
                if (null == obj)
                {
                    LogSystem.Error("null obj");
                }
                else
                {
                    LogSystem.Error("null anim");
                }
            }
        }

        protected void StopAnimation(GameObject obj, Animation_Type anim)
        {
            Animation animation = obj.GetComponent<Animation>();
            if (null != obj && null != animation)
            {
                string animName = GetAnimationNameByType(obj, anim);
                if (!string.IsNullOrEmpty(animName) && null != animation[animName])
                {
                    animation.Stop(animName);
                }
            }
            else
            {
                if (null == obj)
                {
                    LogSystem.Error("null obj");
                }
                else
                {
                    LogSystem.Error("null anim");
                }
            }
        }

        protected void InitMovement(ImpactLogicInfo info)
        {
            ImpactLogicData config = info.ConfigData;
            if (null != config)
            {
                switch ((ImpactMovementType)config.MoveMode)
                {
                    case ImpactMovementType.SenderDir:
                        if (null != info.Sender)
                        {
                            info.MoveDir = UnityEngine.Quaternion.Euler(0, ImpactUtility.RadianToDegree(info.ImpactSrcDir), 0);
                        }
                        break;
                    case ImpactMovementType.SenderToTarget:
                        if (null != info.Target)
                        {
                            UnityEngine.Vector3 direction = info.Target.transform.position - info.ImpactSrcPos;
                            direction.y = 0.0f;
                            direction.Normalize();
                            info.MoveDir = UnityEngine.Quaternion.LookRotation(direction);
                        }
                        break;
                    case ImpactMovementType.Inherit:
                        if (null != info.Sender)
                        {
                            info.MoveDir = UnityEngine.Quaternion.Euler(0, ImpactUtility.RadianToDegree(info.ImpactSrcDir), 0);
                        }
                        break;
                }
            }
        }
        protected void InitLayer(GameObject obj, ImpactLogicInfo info)
        {
            ImpactLogicData config = info.ConfigData;
            if (null != config && config.IgnoreBlock)
            {
                if (null != obj)
                {
                    SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(obj);
                    if (null != shareInfo)
                    {
                        if (shareInfo.IsPlayer)
                        {
                            ImpactUtility.SetLayer(obj, ImpactUtility.PlayerIgnoreBlockLayer);
                        }
                        else
                        {
                            ImpactUtility.SetLayer(obj, ImpactUtility.MonsterIgnoreBlockLayer);
                        }
                    }
                }
            }
        }
        protected void ResetLayer(GameObject obj, ImpactLogicInfo info)
        {
            ImpactLogicData config = info.ConfigData;
            if (null != config && config.IgnoreBlock)
            {
                if (null != obj)
                {
                    SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(obj);
                    if (null != shareInfo)
                    {
                        if (shareInfo.IsNpc)
                        {
                            ImpactUtility.SetLayer(obj, ImpactUtility.MonsterLayer);
                        }
                        else if (shareInfo.IsPlayer)
                        {
                            ImpactUtility.SetLayer(obj, ImpactUtility.PlayerLayer);
                        }
                    }
                }
            }
        }

        protected void Move(GameObject obj, UnityEngine.Vector3 motion)
        {
            if (null != obj)
            {
                SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(obj);
                if (null != info && info.CanHitMove)
                {
                    while (motion.magnitude > m_MaxMoveStep)
                    {
                        UnityEngine.Vector3 childMotion = UnityEngine.Vector3.ClampMagnitude(motion, m_MaxMoveStep);
                        motion = motion - childMotion;
                        ImpactUtility.MoveObject(obj, childMotion);
                    }
                    ImpactUtility.MoveObject(obj, motion);
                }
            }
        }

        protected void MoveTo(GameObject obj, UnityEngine.Vector3 pos)
        {
            if (null != obj)
            {
                UnityEngine.RaycastHit hit;
                if (Physics.Raycast(obj.transform.position, pos.normalized, out hit, pos.magnitude, 1 << LayerMask.NameToLayer("Terrains")))
                {
                    pos = hit.point;
                }
                UnityEngine.Vector3 motion = pos - obj.transform.position;
                Move(obj, motion);
            }
        }

        protected virtual void UpdateMovement(ImpactLogicInfo info, float deltaTime)
        {
        }
        protected string GetAnimationNameByType(GameObject obj, Animation_Type type)
        {
            if (null == obj) return null;
            SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(obj);
            if (null != info)
            {
                Data_ActionConfig curActionConfig = ActionConfigProvider.Instance.ActionConfigMgr.GetDataById(info.AnimConfigId);

                return curActionConfig.m_ActionAnimNameContainer[type];

                //Data_ActionConfig.Data_ActionInfo action = null;
                //if (null != curActionConfig && curActionConfig.m_ActionContainer.ContainsKey(type)) {
                //  if (curActionConfig.m_ActionContainer[type].Count > 0) {
                //    action = curActionConfig.m_ActionContainer[type][0];
                //  }
                //}
                //if (null != action) {
                //  return action.m_AnimName;
                //}
            }
            return null;
        }

        protected float GetAnimationLenthByType(GameObject obj, Animation_Type type)
        {
            string animName = GetAnimationNameByType(obj, type);
            Animation anim = obj.GetComponent<Animation>();
            if (!string.IsNullOrEmpty(animName))
            {
                if (null != obj && null != anim)
                {
                    try
                    {
                        AnimationState state = anim[animName];
                        if (null != state)
                        {
                            return state.length;
                        }
                        else
                        {
                            LogSystem.Error("Obj " + obj.name + " GetAnimationLenthBy Type " + type + " is null");
                        }
                    }
                    catch { }
                }
            }
            return 0.0f;
        }

        protected float GetTerrainHeight(UnityEngine.Vector3 pos)
        {
            if (Terrain.activeTerrain != null)
            {
                return Terrain.activeTerrain.SampleHeight(pos);
            }
            else
            {
                UnityEngine.RaycastHit hit;
                pos.y += 2;
                if (Physics.Raycast(pos, -UnityEngine.Vector3.up, out hit, 30 /*max_ray_cast_dis */, 1 << LayerMask.NameToLayer("Terrains")))
                {
                    return hit.point.y + 0.1f;
                }
                return 0;
            }
        }

        protected void PlayEffect(string effect, string bone, UnityEngine.Vector3 position, UnityEngine.Vector3 rotation, GameObject target, float playTime)
        {
            if (string.IsNullOrEmpty(bone))
            {
                bone = "Bone_Root";
            }
            GameObject obj = ResourceSystem.NewObject(effect, playTime) as GameObject;
            Transform parent = LogicSystem.FindChildRecursive(target.transform, bone);
            if (null != obj && null != parent)
            {
                obj.transform.parent = parent;
                obj.transform.localPosition = position;
                obj.transform.rotation = UnityEngine.Quaternion.Euler(rotation);
            }
            else if (null == parent)
            {
                LogSystem.Debug("GfxImpactLogic::PlayEffect can't find bone {0} on {1}", bone, target.name);
            }
            else
            {
                LogSystem.Debug("GfxImpactLogic::PlayEffect NewObject return null");
            }
        }

        protected void SetGfxStateFlag(GameObject obj, Operate_Type opType, GfxCharacterState_Type mask)
        {
            SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(obj);
            if (null != shareInfo)
            {
                if (opType == Operate_Type.OT_AddBit)
                {
                    shareInfo.GfxStateFlag |= (int)mask;
                }
                else if (opType == Operate_Type.OT_RemoveBit)
                {
                    shareInfo.GfxStateFlag &= ~((int)mask);
                }
            }
        }

        protected void ClearGfxStateFlag(GameObject obj)
        {
            SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(obj);
            if (null != shareInfo)
            {
                shareInfo.GfxStateFlag = 0;
            }
        }

        private const float m_MaxMoveStep = 2.0f;
    }
}
