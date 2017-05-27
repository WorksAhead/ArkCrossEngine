using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum TalentType
    {
        kUnknow,
        kTheOne,
        kAngryFire,
        kCombat,
        kSuperArmor,
        kBlock,
        kSkill,
        kMars,
    }

    public enum TalentPhase : int
    {
        kGreen,
        kBlue,
        kPurple,
        kOrange,
        kMax,
    }

    public class TalentCard
    {
        public ItemDataInfo Item
        {
            get { return m_Item; }
        }

        public TalentCard(TalentType talent_type)
        {
            m_TalentType = talent_type;
            m_TalentAttributes = new TalentAttribute[(int)TalentPhase.kMax];
        }

        public virtual TalentType GetTalentType()
        {
            return m_TalentType;
        }

        public virtual TalentAttribute GetAttribute(TalentPhase phase)
        {
            return m_TalentAttributes[(int)phase];
        }

        public virtual void Init(ItemDataInfo item)
        {
            m_Item = item;
            List<int> item_talent_attributes = item.ItemConfig.m_TalentAttributes;
            for (int i = 0; i < (int)TalentPhase.kMax; i++)
            {
                if (item_talent_attributes.Count > i)
                {
                    int attribute_id = item_talent_attributes[i];
                    ItemAttributeConfig config = ItemAttributeConfigProvider.Instance.GetDataById(attribute_id);
                    if (config != null)
                    {
                        m_TalentAttributes[i] = AttributeCreator.Instance.Create((AttributeId)config.AttributeType);
                        if (m_TalentAttributes[i] == null)
                        {
                            continue;
                        }
                        try
                        {
                            m_TalentAttributes[i].Init(config.ParamValues, config.LevelAddValues);
                        }
                        catch (Exception ex)
                        {
                            LogSystem.Error("----talent attribut {0} init error!\n {1}\n{2}", config.AttributeType, ex.Message, ex.StackTrace);
                        }
                        m_TalentAttributes[i].UpdateToLevel(item.Level);
                    }
                    foreach (int active_id in item.ItemConfig.m_ActiveAttributes)
                    {
                        if (attribute_id == active_id)
                        {
                            m_TalentAttributes[i].IsActive = true;
                            break;
                        }
                    }
                }
                else
                {
                    LogSystem.Warn("TalentCard have no {0} attribute", (AttributeId)i);
                }
            }
        }

        protected TalentType m_TalentType = TalentType.kUnknow;
        protected TalentAttribute[] m_TalentAttributes;
        protected ItemDataInfo m_Item;
    }
}
