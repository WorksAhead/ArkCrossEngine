using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ArkCrossEngine
{
  /// <summary>
  /// 管理操作超时，同时提供限制操作频率的机制（使用Exists检查特定操作是否已经存在，逻辑上可据此决定拒绝继续操作）。
  /// 对于允许同一操作发起多次的情形，应另使用OperationWatch限制操作频率。
  /// 使用方式如下：
  /// 1、根据操作生成操作key。
  /// 2、判断对应操作key的Timeout是否已经存在，如果不存在则继续操作；否则拒绝。
  /// 3、添加对应操作key的Timeout。
  /// 4、在操作成功时触发回调并移除对应操作key的Timeout。
  /// 5、在超时时间到时触发超时回调并移除对应操作key的Timeout。（此步聚由Tick驱动，超时回调里通常应该调用回调通知操作超时）
  /// </summary>
  public class Timeout<T>
  {
    public class TimeoutInfo
    {
      public TimeoutInfo(T callback, long startTime, Action onTimeout)
      {
        m_Callback = callback;
        m_StartTime = startTime;
        m_OnTimeout = onTimeout;
      }

      public T Callback
      {
        get { return m_Callback; }
        set { m_Callback = value; }
      }
      public long StartTime
      {
        get { return m_StartTime; }
        set { m_StartTime = value; }
      }
      public Action OnTimeout
      {
        get { return m_OnTimeout; }
        set { m_OnTimeout = value; }
      }
      private T m_Callback = default(T);
      private long m_StartTime = 0;
      private Action m_OnTimeout = null;
    }

    public Timeout()
    {
      DefaultTimeoutMS = 30000;
    }

    public uint DefaultTimeoutMS { get; set; }
    
    public bool Exists(string timeoutKey)
    {
      return m_TimeoutInfoDict.ContainsKey(timeoutKey);
    }

    public void Set(string timeoutKey, T callback, Action onTimeout)
    {
      long startTime = TimeUtility.GetServerMilliseconds();
      TimeoutInfo info = new TimeoutInfo(callback, startTime, onTimeout);
      m_TimeoutInfoDict.AddOrUpdate(timeoutKey, info, info);
    }

    public T Get(string timeoutKey)
    {
      TimeoutInfo info = null;
      m_TimeoutInfoDict.TryGetValue(timeoutKey, out info);
      if (info != null) {
        return info.Callback;
      } else {
        return default(T);
      }
    }

    public bool Remove(string timeoutKey)
    {
      bool ret = false;
      TimeoutInfo info = null;
      ret = m_TimeoutInfoDict.TryRemove(timeoutKey, out info);
      return ret;
    }

    public void Tick()
    {
      long curTime = TimeUtility.GetServerMilliseconds();
      List<string> deleteKeys = new List<string>();
      foreach (KeyValuePair<string, TimeoutInfo> kv in m_TimeoutInfoDict) {
        if (kv.Value != null && curTime - kv.Value.StartTime > DefaultTimeoutMS) {
          if (kv.Value.OnTimeout != null) {
            kv.Value.OnTimeout();
          }
          deleteKeys.Add(kv.Key);
        }
      }
      TimeoutInfo info = null;
      foreach (string key in deleteKeys) {
        m_TimeoutInfoDict.TryRemove(key, out info);
      }
    }

    private ServerConcurrentDictionary<string, TimeoutInfo> m_TimeoutInfoDict = new ServerConcurrentDictionary<string, TimeoutInfo>();
  }
}