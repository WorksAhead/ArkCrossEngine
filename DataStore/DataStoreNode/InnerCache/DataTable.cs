using System;
using System.Collections.Generic;
using Google.ProtocolBuffers;

namespace DashFire.DataStore
{
  /// <summary>
  /// 数据存储缓存数据表类型
  /// 对应数据库一张数据表中的数据
  /// </summary>
  internal class TableCache
  {
    /// <summary>
    /// 主键查询
    /// </summary>
    /// <param name="key">主键</param>
    /// <returns>若查找到返回唯一对应数据值，否则返回null</returns>
    internal DataValue Find(string key)
    {
      DataValue dataValue = null;
      m_PrimaryDict.TryGetValue(key, out dataValue);
      return dataValue;
    }
    /// <summary>
    /// 外键查询
    /// </summary>
    /// <param name="foreignKey">外键</param>
    /// <returns>外键所对应的数据值列表</returns>
    internal List<DataValue> FindByForeignKey(string foreignKey)
    {
      List<DataValue> dataValueList = new List<DataValue>();
      HashSet<string> associateKeys = null;
      m_AssociateDict.TryGetValue(foreignKey, out associateKeys);
      if (associateKeys != null) {
        List<string> deleteKeys = new List<string>();
        foreach (var key in associateKeys) {
          DataValue dataMessage = this.Find(key);
          if (dataMessage != null) {
            dataValueList.Add(dataMessage);
          } else {
            deleteKeys.Add(key);
          }
        }
        foreach (var key in deleteKeys) {
          associateKeys.Remove(key);
        }
      }
      return dataValueList;
    }
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="key">主键</param>
    /// <returns>删除成功返回true，失败为false</returns>
    internal bool Remove(string key)
    {
      return m_PrimaryDict.Remove(key);
    }
    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <param name="key">主键,不能为空</param>
    /// <param name="foreignKey">外键，可以为空</param>
    /// <param name="dataMessage">数据值</param>
    internal void AddOrUpdate(string key, string foreignKey, IMessage dataMessage)
    {
      if (dataMessage == null) {
        return;
      }
      DataValue dataValue = null;
      m_PrimaryDict.TryGetValue(key, out dataValue);
      if (dataValue == null) {
        dataValue = new DataValue(dataMessage);
        m_PrimaryDict.Add(key, dataValue);
      } else {
        dataValue.Dirty = true;
        dataValue.Valid = true;
        dataValue.DataMessage = dataMessage;
      }
      if (foreignKey != null) {
        HashSet<string> associateKeys = null;
        m_AssociateDict.TryGetValue(foreignKey, out associateKeys);
        if (associateKeys != null) {
          associateKeys.Add(key);
        } else {
          associateKeys = new HashSet<string>();
          associateKeys.Add(key);
          m_AssociateDict.Add(foreignKey, associateKeys);
        }
      }
    }
    /// <summary>
    /// 返回数据表DataValue的集合
    /// </summary>
    /// <returns></returns>
    internal List<DataValue> GetDataValues()
    {
      List<DataValue> dvList = new List<DataValue>(m_PrimaryDict.Values);
      return dvList;
    }

    internal void Tick()
    {
      //删除无效或过期的数据
      List<string> deleteKeys = new List<string>();
      foreach (var data in m_PrimaryDict) {
        data.Value.DecreaseLifeCount();
        if ((data.Value.Dirty == false && data.Value.Valid == false) || (data.Value.LifeCount < 0)) {
          deleteKeys.Add(data.Key);
        }             
      }
      foreach (var key in deleteKeys) {
        m_PrimaryDict.Remove(key);
      }
      if (deleteKeys.Count > 0) {
        LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Delete invalid or out-of-date items. Msg:{0} Count:{1}", 0, deleteKeys.Count);
      }
    }

    Dictionary<string, HashSet<string>> m_AssociateDict = new Dictionary<string, HashSet<string>>();    //foreignKey-primaryKeys
    Dictionary<string, DataValue> m_PrimaryDict = new Dictionary<string, DataValue>();                  //primaryKey-DataValue
  }
}
