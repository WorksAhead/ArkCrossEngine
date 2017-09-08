using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    public class UserArenaAttr
    {
        public int HeroId;
        public int level;
        public int HpMax = 0;
        public int HpRecover = 0;

        public void Add(UserArenaAttr add)
        {
            if (add == null)
            {
                return;
            }
            HpMax += add.HpMax;
            HpRecover += add.HpRecover;
        }
    }

    public class ChallengeChecker
    {
        internal static int CalcTotalHp(ArenaInfo info, float coefficent)
        {
            int result = 0;
            if (info == null)
            {
                return result;
            }
            UserArenaAttr user_attr = CalcArenaTargetAttr(info);
            if (user_attr != null)
            {
                user_attr.HpMax = (int)(user_attr.HpMax * coefficent);
                result += user_attr.HpMax;
                //LogSys.Log(LOG_TYPE.INFO, "--user hp=" + user_attr.HpMax);
            }
            foreach (PartnerInfo partner in info.FightPartners)
            {
                UserArenaAttr partner_attr = CalcPartnerAttr(user_attr, partner);
                if (partner_attr != null)
                {
                    result += partner_attr.HpMax;
                    //LogSys.Log(LOG_TYPE.INFO, "--partner hp=" + partner_attr.HpMax);
                }
            }
            return result;
        }

        internal static int CalcPlayerHp(ArenaInfo info, float coefficent)
        {
            int result = 0;
            if (info == null)
            {
                return result;
            }
            UserArenaAttr user_attr = CalcArenaTargetAttr(info);
            if (user_attr != null)
            {
                user_attr.HpMax = (int)(user_attr.HpMax * coefficent);
                result += user_attr.HpMax;
                LogSys.Log(LOG_TYPE.INFO, "--user hp=" + user_attr.HpMax);
            }
            return result;
        }

        internal static UserArenaAttr CalcArenaTargetAttr(ArenaInfo info)
        {
            if (info == null)
            {
                return null;
            }
            UserArenaAttr attr = CalcBaseAttr(info.HeroId, info.Level);
            CalcItemsAddAttr(attr, info.EquipInfo, true);
            List<ItemInfo> legacy_seven = ConvertToItems(info.LegacyInfo);
            CalcItemsAddAttr(attr, legacy_seven, true);
            CalcItemsAddAttr(attr, CalcComplexAttr(legacy_seven), true);
            CalcItemsAddAttr(attr, ConvertToItems(info.XSoulInfo), true);
            UserArenaAttr partner_attr = CalcPartnerAddAttr(attr, info.ActivePartner);
            attr.Add(partner_attr);
            return attr;
        }

        internal static UserArenaAttr CalcPartnerAttr(UserArenaAttr owner, ArkCrossEngine.PartnerInfo partner)
        {
            UserArenaAttr result = new UserArenaAttr();
            if (partner == null || owner == null)
            {
                return result;
            }
            AppendAttributeConfig aac = AppendAttributeConfigProvider.Instance.GetDataById(partner.GetAppendAttrConfigId());
            if (aac == null)
            {
                return result;
            }
            float inheritDefenceAttrPercent = partner.GetInheritDefenceAttrPercent();
            result.HpMax = (int)(owner.HpMax * inheritDefenceAttrPercent);
            result.HpRecover = 0;
            return result;
        }

        internal static float CalcPvpCoefficient(float lvl1, float lvl2)
        {
            float lvl = (lvl1 + lvl2) / 2;
            double c = 4.09 * 1.2 * 1.3 * (1 + lvl * 0.04) * (1 + (0.15 * (1.62 - 1) / 50 * lvl) +
                       (1.05 + 0.55 / 50 * lvl - 1) * 0.5 + (1.05 + 0.55 / 50 * lvl - 1) * 0.5);
            return (float)c;
        }

        internal static List<ItemInfo> CalcComplexAttr(List<ItemInfo> seven)
        {
            List<ItemInfo> target_items = new List<ItemInfo>();
            foreach (ItemInfo se in seven)
            {
                if (se != null)
                {
                    LegacyComplexAttrConifg lcac_data = LegacyComplexAttrConifgProvider.Instance.GetDataById(se.ItemId);
                    if (null != lcac_data && lcac_data.Property > 0
                      && IsUnLock(seven, lcac_data.m_PeerA)
                      && IsUnLock(seven, lcac_data.m_PeerB))
                    {
                        ItemInfo attr_carrier = new ItemInfo();
                        attr_carrier.Level = 1;
                        attr_carrier.ItemNum = 1;
                        attr_carrier.AppendProperty = lcac_data.Property;
                        target_items.Add(attr_carrier);
                    }
                }
            }
            return target_items;
        }

        internal static bool IsUnLock(List<ItemInfo> seven, int id)
        {
            ItemInfo target = null;
            foreach (var item in seven)
            {
                if (item.ItemId == id)
                {
                    target = item;
                    break;
                }
            }
            if (target == null)
            {
                return false;
            }
            return target.IsUnlock;
        }

        internal static List<ItemInfo> ConvertToItems(List<ArkCrossEngine.ArenaItemInfo> arenaitems)
        {
            List<ItemInfo> items = new List<ItemInfo>();
            foreach (ArenaItemInfo arena_item in arenaitems)
            {
                ItemInfo item = new ItemInfo(arena_item.ItemId, arena_item.AppendProperty, arena_item.Level);
                items.Add(item);
            }
            return items;
        }

        internal static List<ItemInfo> ConvertToItems(List<ArkCrossEngine.ArenaXSoulInfo> arenaitems)
        {
            List<ItemInfo> items = new List<ItemInfo>();
            foreach (var arena_item in arenaitems)
            {
                ItemInfo item = new ItemInfo(arena_item.ItemId, 0, arena_item.Level);
                item.Experience = arena_item.Experience;
                item.UpdateLevelByExperience();
                item.Level -= 1;
                items.Add(item);
            }
            return items;
        }

        internal static UserArenaAttr CalcBaseAttr(int heroid, int level)
        {
            UserArenaAttr user = new UserArenaAttr();
            level = level > 0 ? level : 0;
            user.HeroId = heroid;
            user.level = level;
            Data_PlayerConfig config = PlayerConfigProvider.Instance.GetPlayerConfigById(heroid);
            if (config != null)
            {
                user.HpMax = (int)config.m_AttrData.GetAddHpMax(0, 0);
                user.HpRecover = (int)config.m_AttrData.GetAddHpRecover(0, 0);
            }

            LevelupConfig level_config = PlayerConfigProvider.Instance.GetPlayerLevelupConfigById(heroid);
            if (level_config != null)
            {
                user.HpMax += (int)(level_config.m_AttrData.GetAddHpMax(0, 0) * level);
                user.HpRecover += (int)(level_config.m_AttrData.GetAddHpRecover(0, 0) * level);
            }
            return user;
        }

        internal static UserArenaAttr CalcItemsAddAttr(UserArenaAttr base_attr, List<ItemInfo> items, bool is_add = false)
        {
            UserArenaAttr added = new UserArenaAttr();
            foreach (ItemInfo item in items)
            {
                if (is_add)
                {
                    base_attr.HpMax += (int)GetAddHpMax(item, base_attr.HpMax, base_attr.level);
                    base_attr.HpRecover += (int)GetAddHpMax(item, base_attr.HpRecover, base_attr.level);
                }
                added.HpMax += (int)GetAddHpMax(item, base_attr.HpMax, base_attr.level);
                added.HpRecover += (int)GetAddHpMax(item, base_attr.HpRecover, base_attr.level);
            }
            return added;
        }

        internal static UserArenaAttr CalcPartnerAddAttr(UserArenaAttr base_attr, ArkCrossEngine.PartnerInfo pi)
        {
            UserArenaAttr added = new UserArenaAttr();
            if (pi == null)
            {
                return added;
            }
            AppendAttributeConfig info = AppendAttributeConfigProvider.Instance.GetDataById(pi.GetAppendAttrConfigId());
            if (info != null)
            {
                added.HpMax = (int)info.GetAddHpMax(base_attr.HpMax, base_attr.level);
                added.HpRecover = (int)info.GetAddHpRecover(base_attr.HpRecover, base_attr.level);
            }
            return added;
        }

        internal static float GetAddHpMax(ItemInfo item, float refVal, int refLevel)
        {
            float ret = 0;
            if (null != item.ItemConfig)
            {
                ret += item.ItemConfig.m_AttrData.GetAddHpMax(refVal, refLevel, item.Level);
                if (item.AppendProperty > 0)
                {
                    AppendAttributeConfig data = AppendAttributeConfigProvider.Instance.GetDataById(item.AppendProperty);
                    if (null != data)
                    {
                        ret += data.GetAddHpMax(refVal, refLevel);
                    }
                }
            }
            return ret;
        }
    }
}
