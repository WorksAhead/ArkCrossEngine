using System;
using System.Collections.Generic;
using System.Threading;
using DashFire.DataStore;
using DashFire;

namespace Lobby
{
  internal class GuidInfo
  {
    internal string GuidType
    { get; set; }
    internal long NextGuid
    { get; set; }
  }

  internal sealed class GuidSystem
  {   
    internal List<GuidInfo> GuidList
    {
      get
      {
        m_GuidList[0].NextGuid = m_NextUserGuid;
        m_GuidList[1].NextGuid = m_NextMailGuid;
        return m_GuidList;
      }    
    }
    internal void InitGuidData(List<GuidInfo> guidList)
    {      
      foreach (var dataGuid in guidList) {        
        if (dataGuid.GuidType.Equals(s_UserGuidType)) {
          m_NextUserGuid = dataGuid.NextGuid;          
        }
      }      
      foreach (var dataGuid in guidList) {
        if (dataGuid.GuidType.Equals(s_MailGuidType)) {
          m_NextMailGuid = dataGuid.NextGuid;         
        }
      }
      GuidInfo userGuidInfo = new GuidInfo();
      userGuidInfo.GuidType = s_UserGuidType;
      userGuidInfo.NextGuid = m_NextUserGuid;
      m_GuidList.Add(userGuidInfo);
      GuidInfo mailGuidInfo = new GuidInfo();
      mailGuidInfo.GuidType = s_MailGuidType;
      mailGuidInfo.NextGuid = m_NextMailGuid;
      m_GuidList.Add(mailGuidInfo);
    }
    internal ulong GenerateUserGuid()
    {
      return (ulong)Interlocked.Increment(ref m_NextUserGuid) - 1;
    }
    internal ulong GenerateMailGuid()
    {
      return (ulong)Interlocked.Increment(ref m_NextMailGuid) - 1;
    }  
    private static string s_UserGuidType = "UserGuid";
    private static string s_MailGuidType = "MailGuid";
    private long m_NextUserGuid = 1;
    private long m_NextMailGuid = 1;

    List<GuidInfo> m_GuidList = new List<GuidInfo>();    
  }
}
