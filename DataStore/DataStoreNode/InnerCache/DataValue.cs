using System;
using System.Collections.Generic;
using Google.ProtocolBuffers;

namespace DashFire.DataStore
{
  /// <summary>
  /// 数据存储缓存数据条目类型
  /// Protobuf对象对应数据库表中的一行数据
  /// </summary>
  internal class DataValue
  {
    internal DataValue(IMessage dataMessage)
    {
      m_DataMessage = dataMessage;
      m_Dirty = true;
      m_Valid = true;
      m_LifeCount = s_MaxLifeCount;
    }
    internal bool Dirty
    {
      get { return m_Dirty; }
      set 
      {
        m_Dirty = value;
        m_LifeCount = s_MaxLifeCount;
      }
    }
    internal bool Valid
    {
      get { return m_Valid; }
      set { m_Valid = value; }
    }
    internal int LifeCount
    {
      get { return m_LifeCount; }
    }
    internal IMessage DataMessage
    {
      get { return m_DataMessage; }
      set { m_DataMessage = value; }
    }
    internal void DecreaseLifeCount()
    {
      m_LifeCount--;
    }

    private bool m_Valid = true;      //数据是否有效标识
    private bool m_Dirty = true;      //脏数据标识
    private int m_LifeCount = s_MaxLifeCount;      //数据生命计数，数据被更新时（m_Dirty=true）计数设置为max，每个Tick中减1
    private static int s_MaxLifeCount = 10;   //数据的最大生命计数
    private IMessage m_DataMessage;   //数据protobuf对象
  }
}
