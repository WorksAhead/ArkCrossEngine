using System;
using System.Collections.Generic;

namespace Lobby
{  
  internal class EquipInfo
  {
    internal object Lock
    {
      get { return m_Lock; }
    }
    internal const int c_MaxEquipmentNum = 8;
    internal ItemInfo[] Armor
    {
      get { return m_BodyArmor; }
    }
    internal void SetEquipmentData(int index, ItemInfo info)
    {
        lock (m_Lock){                                      // Add lock zhaoli
            if (index >= 0 && index < c_MaxEquipmentNum){
                m_BodyArmor[index] = info;
            }
        }
    }
    internal ItemInfo GetEquipmentData(int index)
    {
      ItemInfo info = null;
      if (index >= 0 && index < c_MaxEquipmentNum) {
        info = m_BodyArmor[index];
      }
      return info;
    }
    internal void ResetEquipmentData(int index)
    {
        lock (m_Lock){                                      // Add lock   
            if (index >= 0 && index < c_MaxEquipmentNum){
                m_BodyArmor[index] = null;
            }
        }
    }
    internal void Reset()
    {
      for (int ix = 0; ix < c_MaxEquipmentNum; ++ix) {
        m_BodyArmor[ix] = null;
      }
    }

    private object m_Lock = new object();
    private ItemInfo[] m_BodyArmor = new ItemInfo[c_MaxEquipmentNum];
  }
}

