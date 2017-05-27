using System.Collections.Generic;

namespace ArkCrossEngine
{
    public sealed class ImpactLogicManager
    {
        public enum ImpactLogicId
        {
            ImpactLogic_General = 1,
            ImpactLogic_SuperArmor = 2,
            ImpactLogic_Invincible = 3,
            ImpactLogic_ChangeSkill = 4,
            ImpactLogic_StopImpact = 5,
            ImpactLogic_RefixDamage = 6,
            ImpactLogic_BlockAndBeat = 7,
            ImpactLogic_SuperArmorShield = 8,
            ImpactLogic_DamageImmunityShield = 9,
            ImpactLogic_Thorns = 10,
            ImpactLogic_IceArmor = 11,
            ImpactLogic_HitCritical = 12,
            ImpactLogic_AppendDamage = 13,
            ImpactLogic_HitRecover = 14,
            ImpactLogic_Heal = 15,
            ImpactLogic_Undead = 16,
            ImpactLogic_KillAddDamage = 17,
        }

        public IImpactLogic GetImpactLogic(int id)
        {
            IImpactLogic logic;
            m_ImpactLogics.TryGetValue(id, out logic);
            return logic;
        }

        private ImpactLogicManager()
        {
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_General, new ImpactLogic_General());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_SuperArmor, new ImpactLogic_SuperArmor());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_Invincible, new ImpactLogic_Invincible());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_ChangeSkill, new ImpactLogic_ChangeSkill());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_StopImpact, new ImpactLogic_StopImpact());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_RefixDamage, new ImpactLogic_RefixDamage());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_BlockAndBeat, new ImpactLogic_BlockAndBeat());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_SuperArmorShield, new ImpactLogic_SuperArmorShield());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_DamageImmunityShield, new ImpactLogic_DamageImmunityShield());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_Thorns, new ImpactLogic_Thorns());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_IceArmor, new ImpactLogic_IceArmor());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_HitCritical, new ImpactLogic_HitCriticalTrigger());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_AppendDamage, new ImpactLogic_AppendDamage());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_HitRecover, new ImpactLogic_HitRecover());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_Heal, new ImpactLogic_Heal());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_Undead, new ImpactLogic_Undead());
            m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_KillAddDamage, new ImpactLogic_KillAddDamage());
        }

        public static ImpactLogicManager Instance
        {
            get { return s_Instance; }
        }

        private Dictionary<int, IImpactLogic> m_ImpactLogics = new Dictionary<int, IImpactLogic>();
        private static ImpactLogicManager s_Instance = new ImpactLogicManager();
    }
}
