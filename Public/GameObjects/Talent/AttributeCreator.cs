using System.Collections.Generic;

namespace ArkCrossEngine
{
    public interface IAttributeFactory
    {
        TalentAttribute Create();
    }

    public class AttributeFactory<T> : IAttributeFactory where T : TalentAttribute, new()
    {
        public TalentAttribute Create()
        {
            return new T();
        }
    }

    public class AttributeCreator
    {
        public static AttributeCreator Instance { get { return m_Instance; } }
        private static AttributeCreator m_Instance = new AttributeCreator();
        private AttributeCreator()
        {
            RegisterAttribute(AttributeId.kKillAddDamage, new AttributeFactory<KillAddDamage>());
            RegisterAttribute(AttributeId.kKillAddCritical, new AttributeFactory<KillAddCritical>());
            RegisterAttribute(AttributeId.kHitAddDamageRate, new AttributeFactory<HitAddDamageRate>());
            RegisterAttribute(AttributeId.kKillRefreshAllSkills, new AttributeFactory<KillRefreshSkill>());
        }

        public void RegisterAttribute(AttributeId id, IAttributeFactory factory)
        {
            m_AttributeFactory[id] = factory;
        }

        public TalentAttribute Create(AttributeId id)
        {
            IAttributeFactory factory;
            if (m_AttributeFactory.TryGetValue(id, out factory))
            {
                return factory.Create();
            }
            else
            {
                LogSystem.Error("-----create talent failed: not find talent type {0}", id);
            }
            return null;
        }
        private Dictionary<AttributeId, IAttributeFactory> m_AttributeFactory = new Dictionary<AttributeId, IAttributeFactory>();
    }
}
