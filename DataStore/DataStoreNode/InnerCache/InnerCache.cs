using System;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Descriptors;
using System.Reflection;

namespace DashFire.DataStore
{  
  internal class InnerCache
  {
    /// <summary>
    /// 主键查找
    /// </summary>
    /// <param name="msgId">数据类型ID</param>
    /// <param name="key">主键</param>
    /// <returns>查找成功返回对应数据对象，不存在返回null</returns>
    internal DataValue Find(uint msgId, string key)
    {
      TableCache tableCache = null;
      m_TableCacheDict.TryGetValue(msgId, out tableCache);
      if (tableCache != null) {
        return tableCache.Find(key);       
      }
      return null;
    }
    /// <summary>
    /// 外键查找
    /// </summary>
    /// <param name="msgId">数据类型ID</param>
    /// <param name="key">外键</param>
    /// <returns></returns>
    internal List<DataValue> FindByForeignKey(uint msgId, string foreignKey)
    {
      TableCache tableCache = null;
      m_TableCacheDict.TryGetValue(msgId, out tableCache);
      if (tableCache != null) {
        return tableCache.FindByForeignKey(foreignKey);        
      }
      return new List<DataValue>();
    }
    /// <summary>
    /// 表查找
    /// </summary>
    /// <param name="msgId">数据表ID</param>
    /// <returns>该表对应所有的值</returns>
    internal List<DataValue> FindTable(uint msgId)
    {
      TableCache tableCache = null;
      m_TableCacheDict.TryGetValue(msgId, out tableCache);
      if (tableCache != null) {
        return tableCache.GetDataValues();
      }
      return new List<DataValue>();
    }
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="msgId">数据类型ID</param>
    /// <param name="key">Key</param>
    /// <returns>删除成功返回true，失败返回false</returns>
    internal bool Remove(uint msgId, string key)
    {      
      TableCache tableCache = null;
      m_TableCacheDict.TryGetValue(msgId, out tableCache);
      if (tableCache != null) {
        return tableCache.Remove(key);
      }
      return false;
    }
    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <param name="msgId">数据类型ID</param>
    /// <param name="key">Key</param>
    /// <param name="dataMessage">待添加的数据对象</param>
    internal void AddOrUpdate(uint msgId, string key, string foreignKey, IMessage dataMessage)
    {
      if (dataMessage == null) {
        return;
      }
      DataValue dataValue = new DataValue(dataMessage);
      TableCache tableCache = null;
      m_TableCacheDict.TryGetValue(msgId, out tableCache);
      if (tableCache != null) {
        tableCache.AddOrUpdate(key, foreignKey, dataMessage);
      } else {
        TableCache newTableCache = new TableCache();
        newTableCache.AddOrUpdate(key, foreignKey, dataMessage);
        m_TableCacheDict.Add(msgId, newTableCache);
      }
    }
    /// <summary>
    /// 从缓存中抽取出脏数据以写入数据库
    /// </summary>
    /// <returns></returns>
    internal Dictionary<uint, List<IMessage>> FetchDirtyData()
    {
      Dictionary<uint, List<IMessage>> dirtySet = new Dictionary<uint, List<IMessage>>();
      foreach (var table in m_TableCacheDict) {
        List<IMessage> dirtyList = new List<IMessage>();
        foreach (var dataValue in table.Value.GetDataValues()) {
          if (dataValue.Dirty) {
            dirtyList.Add(dataValue.DataMessage);
            dataValue.Dirty = false;
          }
        }
        dirtySet.Add(table.Key, dirtyList);
      }
      return dirtySet;      
    }

    internal void Tick()
    {      
      foreach (var tableCache in m_TableCacheDict.Values) {
        tableCache.Tick();       
      }
    }

    private Dictionary<uint, TableCache> m_TableCacheDict = new Dictionary<uint, TableCache>();
  }
}
