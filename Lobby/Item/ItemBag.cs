using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobby
{
  internal class ItemBag
  {
    internal object Lock
    {
      get { return m_Lock; }
    }
    //外部访问时先对Lock锁定！！！
    internal List<ItemInfo> ItemInfos
    {
      get { return m_ItemData; }
    }
    internal int ItemCount
    {
      get
      {
        return m_ItemData.Count;
      }
    }
    internal void AddItemData(ItemInfo info)
    {
      AddItemData(info, 1);
    }
    internal void AddItemData(ItemInfo info, int num)
    {
      lock (m_Lock) {
        if (null != info && null != m_ItemData) {
          bool isHave = false;
          int ct = m_ItemData.Count;
          for (int i = 0; i < ct; i++) {
            if (null != m_ItemData[i]) {
              if (m_ItemData[i].ItemId == info.ItemId && m_ItemData[i].AppendProperty == info.AppendProperty
              && null != info.ItemConfig && info.ItemConfig.m_MaxStack > 1) {
                m_ItemData[i].ItemNum += num;
                isHave = true;
                break;
              }
            }
          }
          if (!isHave && ct < c_MaxItemNum) {
            info.ItemNum = num;
            m_ItemData.Add(info);
          }
        }
      }
    }
    internal void DelItemData(ItemInfo info)
    {
      lock (m_Lock) {
        if (null != m_ItemData && null != info) {
          for (int i = m_ItemData.Count - 1; i >= 0; i--) {
            if (m_ItemData[i].ItemId == info.ItemId
              && m_ItemData[i].AppendProperty == info.AppendProperty) {
              m_ItemData.RemoveAt(i);
              break;
            }
          }
        }
      }
    }
    internal void ReduceItemData(ItemInfo info)
    {
      lock (m_Lock) {
        if (null != m_ItemData && null != info) {
          for (int i = m_ItemData.Count - 1; i >= 0; i--) {
            if (m_ItemData[i].ItemId == info.ItemId
              && m_ItemData[i].AppendProperty == info.AppendProperty) {
              if (info.ItemNum > 1) {
                info.ItemNum -= 1;
              } else {
                m_ItemData.RemoveAt(i);
              }
              break;
            }
          }
        }
      }
    }
    internal void ReduceItemData(ItemInfo info, int num)
    {
      lock (m_Lock) {
        if (null != m_ItemData && null != info) {
          for (int i = m_ItemData.Count - 1; i >= 0; i--) {
            if (m_ItemData[i].ItemId == info.ItemId
              && m_ItemData[i].AppendProperty == info.AppendProperty) {
              int residue_num = info.ItemNum - num;
              if (residue_num >= 0) {
                if (residue_num >= 1) {
                  info.ItemNum -= num;
                } else {
                  m_ItemData.RemoveAt(i);
                }
              }
              break;
            }
          }
        }
      }
    }
    internal ItemInfo GetItemData(int itemId, int propertyId)
    {
      ItemInfo itemInfo = null;
      lock (m_Lock) {
        itemInfo = m_ItemData.Find(delegate(ItemInfo p) { return (p.ItemId == itemId && p.AppendProperty == propertyId); });
      }
      return itemInfo;
    }
    internal int GetItemCount(int itemId, int propertyId)
    {
      ItemInfo itemInfo = GetItemData(itemId, propertyId);
      if (null != itemInfo) {
        return itemInfo.ItemNum;
      } else {
        return 0;
      }
    }
    internal void ResetItemData()
    {
      lock (m_Lock) {
        m_ItemData.Clear();
      }
    }
    internal void Reset()
    {
      ResetItemData();
    }

    internal const int c_MaxItemNum = 128;
    
    private object m_Lock = new object();
    private List<ItemInfo> m_ItemData = new List<ItemInfo>();
  }
}
