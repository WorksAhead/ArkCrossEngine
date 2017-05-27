using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ArkCrossEngine
{
  /// <summary>
  /// 用于限制操作的频率，如果同一操作不能同时发起多个，则仅用Timeout类限制即可，此类用于允许同时发起多次同一操作的情形。
  /// 使用方式如下：
  /// 1、根据操作生成操作key。
  /// 2、判断对应操作key的OperationWatch是否已经存在，如果不存在则继续操作；否则拒绝。
  /// 3、添加对应操作key的OperationWatch。
  /// 4、在操作成功时移除对应操作key的OperationWatch。
  /// 5、在操作允许间隔时间到时移除对应操作key的OperationWatch。（此步聚由Tick驱动）
  /// </summary>
  public class OperationWatch
  {
    public OperationWatch()
    {
      OperationIntervalMs = 5000;
    }

    public uint OperationIntervalMs { get; set; }
    
    public bool Exists(string operationKey)
    {
      return m_OperationTimeDict.ContainsKey(operationKey);
    }

    public void Add(string operationKey)
    {
      long startTime = TimeUtility.GetLocalMilliseconds();
      m_OperationTimeDict.AddOrUpdate(operationKey, startTime, startTime);
    }

    public bool Remove(string operationKey)
    {
      bool ret = false;
      long startTime;
      ret = m_OperationTimeDict.TryRemove(operationKey, out startTime);
      return ret;
    }

    public void Tick()
    {
      long curTime = TimeUtility.GetServerMilliseconds();
      List<string> deleteKeys = new List<string>();
      foreach (KeyValuePair<string, long> kv in m_OperationTimeDict) {
        if (curTime - kv.Value > OperationIntervalMs) {
          deleteKeys.Add(kv.Key);
        }
      }
      long startTime;
      foreach (string key in deleteKeys) {
        m_OperationTimeDict.TryRemove(key, out startTime);
      }
    }

    private ServerConcurrentDictionary<string, long> m_OperationTimeDict = new ServerConcurrentDictionary<string, long>();
  }
}
