using System.Collections.Generic;

namespace ArkCrossEngine
{
    public interface IImpactLogic
    {
        void StartImpact(CharacterInfo obj, int impactId);
        void Tick(CharacterInfo obj, int impactId);
        void StopImpact(CharacterInfo obj, int impactId);
        void OnInterrupted(CharacterInfo obj, int impactId);
        int RefixHpDamage(CharacterInfo obj, int impactId, int hpDamage, int senderId, ref bool isCritical, int impact_owner);
        void OnAddImpact(CharacterInfo obj, int impactId, int addImpactId);
        bool CanInterrupt(CharacterInfo obj, int impactId);
    }

    public abstract class AbstractImpactLogic : IImpactLogic
    {

        public delegate void ImpactLogicDamageDelegate(CharacterInfo entity, int attackerId, int damage, bool isKiller, bool isCritical, bool isOrdinary);
        public delegate void ImpactLogicSkillDelegate(CharacterInfo entity, int skillId);
        public delegate void ImpactLogicEffectDelegate(CharacterInfo entity, string effectPath, string bonePath, float recycleTime);
        public delegate void ImpactLogicScreenTipDelegate(CharacterInfo entity, string tip);
        public delegate void ImpactLogicRageDelegate(CharacterInfo entity, int rage);
        public delegate void ImpactLogicRefreshSkill(CharacterInfo entity);
        public static ImpactLogicDamageDelegate EventImpactLogicDamage;
        public static ImpactLogicSkillDelegate EventImpactLogicSkill;
        public static ImpactLogicEffectDelegate EventImpactLogicEffect;
        public static ImpactLogicScreenTipDelegate EventImpactLogicScreenTip;
        public static ImpactLogicRageDelegate EventImpactLogicRage;
        public static ImpactLogicRefreshSkill EventRefreshSkill;
        public virtual void StartImpact(CharacterInfo obj, int impactId)
        {
            if (null != obj)
            {
                ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (null != impactInfo)
                {
                    if (impactInfo.ConfigData.BreakSuperArmor)
                    {
                        obj.SuperArmor = false;
                    }
                }
                if (obj is NpcInfo)
                {
                    NpcInfo npcObj = obj as NpcInfo;
                    NpcAiStateInfo aiInfo = npcObj.GetAiStateInfo();
                    if (null != aiInfo && 0 == aiInfo.HateTarget)
                    {
                        aiInfo.HateTarget = impactInfo.m_ImpactSenderId;
                    }
                }
            }
        }
        public virtual void Tick(CharacterInfo obj, int impactId)
        {
            ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
            if (null != impactInfo && impactInfo.m_IsActivated)
            {
                long curTime = TimeUtility.GetServerMilliseconds();
                if (curTime > impactInfo.m_StartTime + impactInfo.m_ImpactDuration)
                {
                    impactInfo.m_IsActivated = false;
                }
            }
        }

        public virtual void StopImpact(CharacterInfo obj, int impactId)
        {
        }

        public virtual void OnInterrupted(CharacterInfo obj, int impactId)
        {
            StopImpact(obj, impactId);
        }

        public virtual int RefixHpDamage(CharacterInfo obj, int impactId, int hpDamage, int senderId, ref bool isCritical, int impactOwnerId)
        {
            return hpDamage;
        }

        public virtual void OnAddImpact(CharacterInfo obj, int impactId, int addImpactId)
        {
        }
        public virtual bool CanInterrupt(CharacterInfo obj, int impactId)
        {
            return true;
        }
        protected bool IsImpactDamageOrdinary(CharacterInfo target, int impactId)
        {
            if (null != target)
            {
                ImpactInfo impactInfo = target.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (null != impactInfo)
                {
                    CharacterInfo sender = target.SceneContext.GetCharacterInfoById(impactInfo.m_ImpactSenderId);
                    if (null != sender)
                    {
                        SkillInfo skillInfo = sender.GetSkillStateInfo().GetSkillInfoById(impactInfo.m_SkillId);
                        if (null != skillInfo)
                        {
                            if (skillInfo.Category == SkillCategory.kAttack)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        protected void ApplyDamage(CharacterInfo obj, CharacterInfo sender, int damage)
        {
            if (null != obj && !obj.IsDead())
            {
                if (GlobalVariables.Instance.IsClient && obj.SceneContext.IsRunWithRoomServer)
                {
                    return;
                }
                bool isKiller = false;
                if (damage < 1)
                {
                    return;
                }
                obj.GetCombatStatisticInfo().AddTotalDamageToMyself(damage);
                sender.GetCombatStatisticInfo().AddTotalDamageFromMyself(damage);
                sender.MakeDamage += damage;
                damage = damage * -1;
                int realDamage = damage;
                if (obj.Hp + damage < 0)
                {
                    realDamage = 0 - obj.Hp;
                }
                obj.SetHp(Operate_Type.OT_Relative, realDamage);
                if (obj.IsDead())
                {
                    isKiller = true;
                }
                if (isKiller)
                {
                    UpdateKillTalent(sender);
                    UpdateKillAddDamage(sender);
                }
                if (null != EventImpactLogicDamage)
                {
                    EventImpactLogicDamage(obj, sender.GetId(), damage, isKiller, false, true);
                }
            }
        }
        protected void ApplyDamage(CharacterInfo obj, int impactId, out int damage)
        {
            damage = 0;
            if (null != obj && !obj.IsDead())
            {
                if (GlobalVariables.Instance.IsClient && obj.SceneContext.IsRunWithRoomServer)
                {
                    return;
                }
                ImpactInfo impactInfo = obj.GetSkillStateInfo().GetImpactInfoById(impactId);
                if (null != impactInfo)
                {
                    CharacterInfo sender = obj.SceneContext.GetCharacterInfoById(impactInfo.m_ImpactSenderId);
                    ApplyDamageImpl(sender, obj, impactInfo, out damage, impactInfo.ConfigData.IsIgnorePassiveCheck);
                }
            }
        }

        protected void ApplyDamageImpl(CharacterInfo sender, CharacterInfo obj, ImpactInfo impactInfo, out int damage, bool is_ignore_passive_check = false)
        {
            damage = 0;
            int skillLevel = 0;
            bool isCritical = false;
            bool isOrdinary = false;
            if (null != sender)
            {
                SkillInfo skillInfo = sender.GetSkillStateInfo().GetSkillInfoById(impactInfo.m_SkillId);
                if (null != skillInfo)
                {
                    skillLevel = skillInfo.SkillLevel;
                    if (skillInfo.Category == SkillCategory.kAttack)
                    {
                        isOrdinary = true;
                    }
                }
                float old_critical = 0;
                bool is_adjusted = AdjustCritical(sender, out old_critical);
                int curDamage = DamageCalculator.CalcImpactDamage(
                  sender,
                  obj,
                  (SkillDamageType)impactInfo.ConfigData.DamageType,
                  ElementDamageType.DC_None == (ElementDamageType)impactInfo.ConfigData.ElementType ? sender.GetEquipmentStateInfo().WeaponDamageType : (ElementDamageType)impactInfo.ConfigData.ElementType,
                  impactInfo.ConfigData.DamageRate + skillLevel * impactInfo.ConfigData.LevelRate,
                  impactInfo.ConfigData.DamageValue,
                  out isCritical);
                if (is_adjusted)
                {
                    //LogSystem.Error("----critical: reset critical to {0}", old_critical);
                    sender.GetActualProperty().SetCritical(Operate_Type.OT_Absolute, old_critical);
                }
                curDamage = TalentAdjustDamage(sender, obj, curDamage);
                List<ImpactInfo> targetImpactInfos = obj.GetSkillStateInfo().GetAllImpact();
                for (int i = 0; i < targetImpactInfos.Count; i++)
                {
                    if (is_ignore_passive_check && targetImpactInfos[i].m_ImpactType == (int)ImpactType.PASSIVE)
                    {
                        continue;
                    }
                    IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(obj.GetSkillStateInfo().GetAllImpact()[i].ConfigData.ImpactLogicId);
                    if (null != logic)
                    {
                        curDamage = logic.RefixHpDamage(obj, targetImpactInfos[i].m_ImpactId, curDamage, sender.GetId(), ref isCritical, obj.GetId());
                    }
                }
                /*
                foreach (ImpactInfo ii in obj.GetSkillStateInfo().GetAllImpact()) {
                  if (is_ignore_passive_check && ii.m_ImpactType == (int)ImpactType.PASSIVE) {
                    continue;
                  }
                  IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(ii.ConfigData.ImpactLogicId);
                  if (null != logic) {
                    curDamage = logic.RefixHpDamage(obj, ii.m_ImpactId, curDamage, sender.GetId(), ref isCritical, obj.GetId());
                  }
                }*/
                if (!is_ignore_passive_check)
                {
                    List<ImpactInfo> senderImpactInfos = sender.GetSkillStateInfo().GetAllImpact();
                    for (int i = 0; i < senderImpactInfos.Count; i++)
                    {
                        if (senderImpactInfos[i].m_ImpactType == (int)ImpactType.PASSIVE)
                        {
                            IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(senderImpactInfos[i].ConfigData.ImpactLogicId);
                            if (null != logic)
                            {
                                curDamage = logic.RefixHpDamage(obj, senderImpactInfos[i].m_ImpactId, curDamage, sender.GetId(), ref isCritical, sender.GetId());
                            }
                        }
                    }
                    /*
                    foreach (ImpactInfo passive_impact in sender.GetSkillStateInfo().GetAllImpact()) {
                      if (passive_impact.m_ImpactType == (int)ImpactType.PASSIVE) {
                        IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(passive_impact.ConfigData.ImpactLogicId);
                        if (null != logic) {
                          curDamage = logic.RefixHpDamage(obj, passive_impact.m_ImpactId, curDamage, sender.GetId(), ref isCritical, sender.GetId());
                        }
                      }
                    }*/
                }
                damage = curDamage;
                OnCharacterDamage(sender, obj, curDamage, isCritical, isOrdinary);
            }
        }

        public void OnCharacterDamage(CharacterInfo sender, CharacterInfo obj, int curDamage, bool isCritical, bool isOrdinary)
        {
            bool isKiller = false;
            // 计算出的伤害小于1时， 不处理
            if (curDamage < 1)
            {
                return;
            }
            UserInfo user = obj as UserInfo;
            if (null != user)
            {
                user.GetCombatStatisticInfo().AddTotalDamageToMyself(curDamage);
            }
            UserInfo senderUser = sender as UserInfo;
            if (null != senderUser)
            {
                senderUser.GetCombatStatisticInfo().AddTotalDamageFromMyself(curDamage);
            }
            sender.MakeDamage += curDamage;
            curDamage = curDamage * -1;
            int realDamage = curDamage;
            if (obj.Hp + curDamage < 0)
            {
                realDamage = 0 - obj.Hp;
            }
            obj.SetHp(Operate_Type.OT_Relative, realDamage);
            if (obj.IsDead())
            {
                isKiller = true;
            }
            if (isKiller)
            {
                UpdateKillTalent(sender);
                UpdateKillAddDamage(sender);
                UpdateKillAddCritical(sender);
            }
            if (null != EventImpactLogicDamage)
            {
                EventImpactLogicDamage(obj, sender.GetId(), curDamage, isKiller, isCritical, isOrdinary);
            }
        }

        private int TalentAdjustDamage(CharacterInfo sender, CharacterInfo target, int damage)
        {
            if (sender == null)
            {
                return damage;
            }
            HitAddDamageRate attr = sender.TalentManager.GetTalentAttribute(AttributeId.kHitAddDamageRate) as HitAddDamageRate;
            if (attr != null && attr.IsTriggered())
            {
                damage = (int)(damage * (1 + attr.RateAdd));
                //LogSystem.Error("----HitAddDamage: damage {0} rate {1} ", damage, attr.RateAdd);
            }
            return damage;
        }

        private void UpdateKillTalent(CharacterInfo sender)
        {
            KillRefreshSkill kill_attr = sender.TalentManager.GetTalentAttribute(AttributeId.kKillRefreshAllSkills) as KillRefreshSkill;
            if (kill_attr == null)
            {
                return;
            }
            if (!kill_attr.IsActive)
            {
                return;
            }
            kill_attr.AddKillCount(sender);
            if (kill_attr.RefreshSkill(sender))
            {
                if (EventRefreshSkill != null)
                {
                    EventRefreshSkill(sender);
                }
            }
        }

        private void UpdateKillAddDamage(CharacterInfo sender)
        {
            KillAddDamage kill_attr = sender.TalentManager.GetTalentAttribute(AttributeId.kKillAddDamage) as KillAddDamage;
            if (kill_attr == null || !kill_attr.IsActive)
            {
                //LogSystem.Error("----KillAddDamage: killAddDamage is not active!");
                return;
            }
            CombatStatisticInfo combat_info = sender.GetCombatStatisticInfo();
            if (combat_info == null)
            {
                //LogSystem.Error("----KillAddDamage: combat info is null!");
                return;
            }
            if (kill_attr.IsTriggerHit(combat_info.LastHitCountId) && kill_attr.IsTriggered)
            {
                //LogSystem.Error("----KillAddDamage: the trigger hit!");
                return;
            }
            if (kill_attr.IsTriggered && kill_attr.IsDamageHit(combat_info.LastHitCountId))
            {
                //LogSystem.Error("----KillAddDamage: the damage hit!");
                return;
            }
            ImpactSystem.Instance.SendImpactToCharacter(sender, kill_attr.ImpactId, sender.GetId(), -1,
                                                        -1, sender.GetMovementStateInfo().GetPosition3D(),
                                                        sender.GetMovementStateInfo().GetFaceDir());
            //LogSystem.Error("----KillAddDamage: begin add impact!");
            kill_attr.KillHitCountId = combat_info.LastHitCountId;
            kill_attr.IsTriggered = true;
        }

        private void UpdateKillAddCritical(CharacterInfo sender)
        {
            KillAddCritical attr = sender.TalentManager.GetTalentAttribute(AttributeId.kKillAddCritical) as KillAddCritical;
            CombatStatisticInfo combat_info = sender.GetCombatStatisticInfo();
            if (attr != null && combat_info != null)
            {
                attr.OnKill(combat_info.LastHitCountId);
            }
        }

        private bool AdjustCritical(CharacterInfo sender, out float critical)
        {
            critical = sender.GetActualProperty().Critical;
            KillAddCritical attr = sender.TalentManager.GetTalentAttribute(AttributeId.kKillAddCritical) as KillAddCritical;
            CombatStatisticInfo combat_info = sender.GetCombatStatisticInfo();
            if (attr != null && combat_info != null)
            {
                attr.OnHit(combat_info.LastHitCountId);
                if (attr.IsCriticalHit(combat_info.LastHitCountId))
                {
                    float new_critical = critical + attr.CriticalAdd;
                    new_critical = new_critical > 1 ? 1 : new_critical;
                    sender.GetActualProperty().SetCritical(Operate_Type.OT_Absolute, new_critical);
                    //LogSystem.Error("----critical: set critical = {0}", new_critical);
                }
                return true;
            }
            return false;
        }
    }
}
