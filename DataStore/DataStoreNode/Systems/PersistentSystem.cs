using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using Google.ProtocolBuffers;
using DashFire.DataStore;
using DashFire;
using System.Collections.Concurrent;
using ArkCrossEngine;

internal class PersistentSystem
{
  internal void Init()
  {
    m_LastTickTime = TimeUtility.GetServerMilliseconds();
    m_SaveDBInterval = DataStoreConfig.PersistentInterval;
    //m_LargeSize = m_MaxAllowedPacket / 50;
    //m_MediumSize = m_MaxAllowedPacket / 600;
    //m_SmallSize = m_MaxAllowedPacket / 1000;
    LogSys.Log(LOG_TYPE.INFO, "PersistentSystem initialized");
  }
  internal void Tick()
  {
    try {
      long curTime = TimeUtility.GetServerMilliseconds();
      if (m_LastTickTime + m_SaveDBInterval < curTime) {
        m_LastTickTime = curTime;
        if (m_NextSaveCount > 0) {          
          SaveToDB(m_NextSaveCount);
          m_NextSaveCount++;
        }        
      }
      if (0 == m_NextSaveCount) {
        int count = 0;
        foreach (int saveCount in m_CurrentSaveCounts.Values) {
          if (0 == saveCount) {
            count++;
          }
        }
        if (count == m_CurrentSaveCounts.Count) {
          //TODO：最后一次存盘完成，可以关闭DataStoreNode进程
          m_LastSaveFinished = true;
        }
      }
    } catch (Exception ex) {
      LogSys.Log(LOG_TYPE.ERROR, "DataCacheSystem ERROR:{0} \n StackTrace:{1}", ex.Message, ex.StackTrace);      
    }
  }
  internal void LastSaveToDB()
  {
    m_LastSaveFinished = false;
    m_NextSaveCount = 0;
    SaveToDB(m_NextSaveCount);
  }
  internal bool LastSaveFinished
  {
    get { return m_LastSaveFinished; }
  }

  private void SaveToDB(long saveCount)
  {
    var cacheSystem = DataCacheSystem.Instance;
    Dictionary<uint, List<IMessage>> dataSet = cacheSystem.FetchDirtyData();
    int dirtyCount = 0;
    foreach (var dataList in dataSet.Values) {
      dirtyCount += dataList.Count;
    }
    LogSys.Log(LOG_TYPE.MONITOR, ConsoleColor.Yellow, "Save to MySQL Count: {0}, dirty DataValues: {1}", saveCount, dirtyCount);
    m_CurrentSaveCounts.Clear();
    foreach (var dataList in dataSet.Values) {
      if (dataList.Count > 0) {
        IMessage firstData = dataList[0];
        string tableTypeName = firstData.GetType().Name;
        int batchDataSize = m_SmallSize;
        if (firstData.SerializedSize < 50) {
          batchDataSize = m_LargeSize;
        } else if (firstData.SerializedSize < 1000) {
          batchDataSize = m_MediumSize;
        }
        int batchNumber = dataList.Count / batchDataSize + 1;
        LogSys.Log(LOG_TYPE.INFO, "SaveToDB SaveCount:{0}, Table:{1}, DataCount:{2}, BatchNumber:{3}, SingleDataSize:{4}",
                                            saveCount, tableTypeName, dataList.Count, batchNumber,firstData.SerializedSize); 
        for (int i = 0; i < batchNumber; ++i) {
          int beginIndex = i * batchDataSize;
          int endIndex = (i + 1) * batchDataSize;
          if (endIndex > dataList.Count) {
            endIndex = dataList.Count;
          }
          List<IMessage> batchList = dataList.GetRange(i * batchDataSize, endIndex - beginIndex);
          string saveCountKey = string.Format("{0}_{1}", tableTypeName, i);
          m_CurrentSaveCounts.AddOrUpdate(saveCountKey, -1, (g, u) => -1);
          DbThreadManager.Instance.SaveActionQueue.QueueAction(DoSaveWork, batchList, saveCountKey, saveCount);
        }        
      }
    }         
  }
  //在DBThread中执行
  private void DoSaveWork(List<IMessage> dataList,string saveCountKey, long saveCount)
  {
    try {
      DataSaveImplement.BatchSave(dataList);
      m_CurrentSaveCounts.AddOrUpdate(saveCountKey, saveCount, (g, u) => saveCount);
      LogSys.Log(LOG_TYPE.INFO, "DoSaveWork Finish. SaveCountKey:{0}, SaveCount:{1}, BatchDataCount:{2}", saveCountKey, saveCount, dataList.Count); 
    } catch (Exception e) {
      LogSys.Log(LOG_TYPE.ERROR, "Save to MySQL ERROR:{0}, \nStacktrace:{1}", e.Message, e.StackTrace);
    }   
  }

  private bool m_LastSaveFinished = false;
  private long m_LastTickTime = 0;
  private uint m_SaveDBInterval = 0;
  private long m_NextSaveCount = 1;     //存储计数，当值为0时表示最后一次存盘,-1表示存储未完成
  private ConcurrentDictionary<string, long> m_CurrentSaveCounts = new ConcurrentDictionary<string, long>();   //数据表对应的存盘计数
  
  private int m_MaxAllowedPacket = 30 * 1000 * 1000;
  private int m_SmallSize = 20000;
  private int m_MediumSize = 80000;
  private int m_LargeSize = 400000;
 
  internal static PersistentSystem Instance
  {
    get { return s_Instance; }
  }
  private static PersistentSystem s_Instance = new PersistentSystem();
}
