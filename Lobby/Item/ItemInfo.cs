using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal class ItemInfo
    {
        internal ItemInfo()
        { }
        internal ItemInfo(int id, int append_property = 0, int level = 1, bool is_unlock = true)
        {
            m_ItemId = id;
            m_AppendProperty = append_property;
            m_Level = level;
            m_IsUnlock = is_unlock;
            if (append_property > 0)
            {
                m_AppendProperty = append_property;
            }
            else
            {
                if (0 == m_AppendProperty && null != ItemConfig
                  && null != ItemConfig.m_AttachedProperty
                  && ItemConfig.m_AttachedProperty.Count > 0)
                {
                    int total = ItemConfig.m_AttachedProperty.Count;
                    int index = 0;
                    if (total > 1)
                    {
                        index = CrossEngineHelper.Random.Next(0, total - 1);
                    }
                    if (ItemConfig.m_AttachedProperty[index] > 0)
                    {
                        m_AppendProperty = ItemConfig.m_AttachedProperty[index];
                    }
                }
            }
        }
        internal ulong ItemGuid
        {
            get { return m_ItemGuid; }
            set { m_ItemGuid = value; }
        }
        internal int ItemId
        {
            get { return m_ItemId; }
            set { m_ItemId = value; }
        }
        internal int ItemNum
        {
            get { return m_ItemNum; }
            set { m_ItemNum = value; }
        }
        internal int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        internal int Experience
        {
            get { return m_Experience; }
            set { m_Experience = value; }
        }
        internal int ShowModelLevel
        {
            get { return m_ShowModelLevel; }
            set { m_ShowModelLevel = value; }
        }
        internal DateTime CreateTime
        {
            get { return m_CreateTime; }
            set { m_CreateTime = value; }
        }
        internal bool IsUnlock
        {
            get { return m_IsUnlock; }
            set { m_IsUnlock = value; }
        }
        internal ArkCrossEngine.ItemConfig ItemConfig
        {
            get
            {
                return ArkCrossEngine.ItemConfigProvider.Instance.GetDataById(this.ItemId);
            }
        }
        internal int AppendProperty
        {
            get { return m_AppendProperty; }
            set { m_AppendProperty = value; }
        }

        internal int UpdateLevelByExperience()
        {
            XSoulLevelConfig level_config = XSoulLevelConfigProvider.Instance.GetDataById(ItemId);
            if (level_config == null)
            {
                return 0;
            }
            int old_level = m_Level;
            int m_CurLevelExperience = m_Experience;
            for (int i = 2; i <= level_config.m_MaxLevel; i++)
            {
                int cur_level_exp = 0;
                if (level_config.m_LevelExperience.TryGetValue(i, out cur_level_exp))
                {
                    if (m_CurLevelExperience >= cur_level_exp)
                    {
                        m_CurLevelExperience -= cur_level_exp;
                        m_Level = i;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (m_Level == level_config.m_MaxLevel)
            {
                m_CurLevelExperience = 0;
            }
            if (old_level != m_Level)
            {
                m_ShowModelLevel = -1;
            }
            return m_CurLevelExperience;
        }
        //唯一标示物品
        private ulong m_ItemGuid = 0;
        //物品ID
        private int m_ItemId = 0;
        //物品数量
        private int m_ItemNum = 1;
        //物品等级
        private int m_Level = 1;
        private int m_Experience = 0;
        private int m_ShowModelLevel = -1;
        //物品扩展属性ID
        private int m_AppendProperty = 0;
        private string m_ItemName;
        private DateTime m_CreateTime;
        private bool m_IsTimeLimit;
        private bool m_IsUnlock = true;
        //使用时限
        private long m_UseTime;
        private bool m_CanUse;
        //是否可叠加
        private bool m_CanRepeat;
        //叠加上限
        private int m_RepeatLimit;
        //是否可商城购买
        private bool m_CanBuy;
        private int m_PriceInGold;
        private int m_PriceInRMB;
    }
}
