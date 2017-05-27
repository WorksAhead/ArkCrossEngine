using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using DashFire.DataStore;
using DashFire;
using System.Diagnostics;

namespace Lobby
{
  internal sealed class NicknameSystem
  {
    //创建昵称数据
    internal void CreateData()
    {
      string prefix = "Temp_";
      int index = 0;      
      while (true) {
        string name = string.Format("{0}{1}", prefix, index);
        ulong guid = 0;
        if (!m_UnusedNicknames.TryGetValue(name, out guid)) {
          m_UnusedNicknames.TryAdd(name, 0);
          index++;
        }        
        if (index >= c_NumberForCreate) {
          break;
        }
      }      
    }
    //初始化昵称数据，程序启动时从数据库中加载
    internal void InitNicknameData(List<DS_Nickname> nicknameList)
    {
      if (nicknameList.Count > 0) {
        foreach (var dataNickname in nicknameList) {
          if (dataNickname.UserGuid > 0) {
            m_UsedNicknames.TryAdd(dataNickname.Nickname, (ulong)dataNickname.UserGuid);
          } else {
            m_UnusedNicknames.TryAdd(dataNickname.Nickname, (ulong)dataNickname.UserGuid);
          }
        }        
      } else {
        CreateData();  
      }    
      m_IsDataLoaded = true;
    }
    //返回一定数量的可用昵称   
    internal List<string> RequestNicknames(string accountKey)
    {      
      List<string> nicknameList = new List<string>();
      lock (m_Lock) {
        RevertAccountNicknames(accountKey);
        if (m_UnusedNicknames.Count > c_NumberForOneRequest) {
          int randomStart = m_Random.Next(0, m_UnusedNicknames.Count - c_NumberForOneRequest);
          int index = 0;
          foreach (var key in m_UnusedNicknames.Keys) {
            if (index < randomStart) {
              index++;
            } else if (index < randomStart + c_NumberForOneRequest) {
              nicknameList.Add(key);
              index++;
            } else {
              break;
            }
          }
          ulong outValue = 0;
          for (int i = 0; i < nicknameList.Count; ++i) {
            m_UnusedNicknames.TryRemove(nicknameList[i], out outValue);            
          }
          m_AccountReqNicknames.AddOrUpdate(accountKey, nicknameList, (g, u) => nicknameList);
        }
      }
      return nicknameList;
    }
    //昵称是否可用，可用返回true，不可用返回false   
    internal bool CheckNickname(string accoountKey, string nickname)
    {
      lock (m_Lock) {
        RevertAccountNicknames(accoountKey);
        ulong guid = 0;
        if (!m_UsedNicknames.TryGetValue(nickname, out guid)) {
          m_UsedNicknames.TryAdd(nickname, 1);
          m_UnusedNicknames.TryRemove(nickname, out guid);
          return true;
        } else {
          return false;
        }
      }
    }
    //返还账号申请的昵称
    internal void RevertAccountNicknames(string accountKey)
    {
      List<string> abortNicknameList = null;
      if (m_AccountReqNicknames.TryGetValue(accountKey, out abortNicknameList)) {
        foreach (string nickname in abortNicknameList) {
          m_UnusedNicknames.AddOrUpdate(nickname, 0, (g, u) => 0);
        }
        m_AccountReqNicknames.TryRemove(accountKey, out abortNicknameList);
      }
    }    
    private ConcurrentDictionary<string, ulong> m_UnusedNicknames = new ConcurrentDictionary<string, ulong>();  //未使用的昵称
    private ConcurrentDictionary<string, ulong> m_UsedNicknames = new ConcurrentDictionary<string, ulong>();    //已使用的昵称   
    private ConcurrentDictionary<string, List<string>> m_AccountReqNicknames = new ConcurrentDictionary<string, List<string>>();    //客户端申请的昵称
    private const int c_NumberForOneRequest = 20;       //客户端一次申请获得的昵称数目  
    private const int c_NumberForCreate = 1000;         //初始创建的昵称数目
    private Random m_Random = new Random();
    private object m_Lock = new object();
    private bool m_IsDataLoaded = false;
  }
}
