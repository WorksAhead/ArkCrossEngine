using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum AttributeId
    {
        kUnknown = 0,
        kKillAddDamage = 1,
        kKillAddCritical,
        kHitAddDamageRate,
        kKillRefreshAllSkills,
        kAddBlockRate,
        kAddCounterAttackTime,
        kAddCounterAttackDamage,
        kCounterAttackBreakSupperArmor,
    }

    public class TalentAttribute
    {
        public bool IsActive
        {
            get { return m_IsActive; }
            set
            {
                m_IsActive = value;
                //LogSystem.Error("{0} set active {1}", GetId(), value);
            }
        }
        public int Level { get { return m_Level; } }
        public virtual AttributeId GetId() { return AttributeId.kUnknown; }
        public virtual void Init(List<string> attri_list, List<string> level_add) { }
        public virtual void UpdateToLevel(int level) { }
        public virtual void Equip(CharacterInfo owner)
        {
        }
        public virtual void Unequip(CharacterInfo owner)
        {
        }

        protected AttributeId m_Id = AttributeId.kUnknown;
        protected int m_Level = 1;
        protected bool m_IsActive = false;
        protected CharacterInfo m_EquipedOwner = null;
    }

    public class AddBlockRate : TalentAttribute
    {
        public override AttributeId GetId() { return AttributeId.kAddBlockRate; }
        public float RateAdd;
        public float LevelAddRate;
    }
}
