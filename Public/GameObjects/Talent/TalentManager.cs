using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum EquipSlot
    {
        kFirst,
        kSecond,
        kThird,
        kFourth,
        kMax
    }

    public class TalentManager
    {
        public TalentManager()
        {
        }

        public TalentCard EquipTalent(EquipSlot slot, TalentCard card)
        {
            //TODO: check whether the same kind card already equiped;
            TalentCard old_card = GetEquipedTalent(slot);
            m_EquipTalents[slot] = card;
            return old_card;
        }

        public ItemDataInfo EquipTalent(EquipSlot slot, ItemDataInfo item)
        {
            TalentCard card = null;
            if (item != null && item.ItemConfig != null)
            {
                //LogSystem.Error("-----talent: equip {0} itemid={1}", slot, item.ItemId);
                card = new TalentCard((TalentType)item.ItemConfig.m_TalentType);
                card.Init(item);
            }
            TalentCard old_card = EquipTalent(slot, card);
            if (old_card != null)
            {
                return old_card.Item;
            }
            return null;
        }

        public TalentCard GetEquipedTalent(EquipSlot slot)
        {
            TalentCard card = null;
            m_EquipTalents.TryGetValue(slot, out card);
            return card;
        }

        public TalentCard GetEquipedTalent(TalentType talent_type)
        {
            TalentCard card = null;
            for (int i = 0; i < (int)EquipSlot.kMax; i++)
            {
                if (m_EquipTalents.TryGetValue((EquipSlot)i, out card))
                {
                    if (card.GetTalentType() == talent_type)
                    {
                        return card;
                    }
                }
            }
            return null;
        }

        public TalentAttribute GetTalentAttribute(AttributeId attr_id)
        {
            for (int i = 0; i < (int)EquipSlot.kMax; i++)
            {
                TalentCard card = null;
                if (m_EquipTalents.TryGetValue((EquipSlot)i, out card))
                {
                    for (int phase = 0; phase < (int)TalentPhase.kMax; phase++)
                    {
                        TalentAttribute attr = card.GetAttribute((TalentPhase)phase);
                        if (attr != null && attr.GetId() == attr_id)
                        {
                            return attr;
                        }
                    }
                }
            }
            return null;
        }

        private Dictionary<EquipSlot, TalentCard> m_EquipTalents = new Dictionary<EquipSlot, TalentCard>();
    }
}
