using System;
using System.Collections.Generic;

namespace Lobby
{  
  internal enum AccountState : int
  {
    Online,     //账号在线
    Dropped,    //账号掉线
    Offline,    //账号离线
  }
  /// <summary>
  /// 客户端显示玩家角色列表所需要的数据
  /// </summary>
  internal class RoleInfo
  {
    internal ulong Guid
    {
      get { return m_Guid; }
      set { m_Guid = value; }
    }   
    internal string Nickname
    {
      get { return m_Nickname; }
      set { m_Nickname = value; }
    }
    internal int HeroId
    {
      get { return m_HeroId; }
      set { m_HeroId = value; }
    }
    internal int Level
    {
      get { return m_Level; }
      set { m_Level = value; }
    }
    internal int Gold
    {
      get { return m_Gold; }
      set { m_Gold = value; }
    }
    private ulong m_Guid = 0;
    private string m_Nickname;
    private int m_HeroId = 0;
    private int m_Level = 1;
    private int m_Gold = 0;
  }
  internal class AccountInfo
  {   
    internal string AccountKey
    { 
      get { return m_AccountKey; }
      set { m_AccountKey = value; }
    }
    internal string AccountId
    {
      get { return m_AccountId; }
      set { m_AccountId = value; }
    }
    internal string NodeName
    { 
      get { return m_NodeName; }
      set { m_NodeName = value; }
    }
    internal AccountState CurrentState
    {
      get { return m_CurrentState; }
      set { m_CurrentState = value; }
    }
    internal List<RoleInfo> Users
    {
      get { return m_Users; }
    }
    internal string ClientGameVersion
    { 
      get { return m_ClientGameVersion; }
      set { m_ClientGameVersion = value; }
    }
    internal string ClientDeviceidId
    { 
      get { return m_ClientDeviceidId; }
      set { m_ClientDeviceidId = value; }
    }
    internal string System
    {
      get { return m_System; }
      set { 
        m_System = value;
        if ("IOS" == m_System) {
          ChannelId = LobbyConfig.IOSGameChannelStr;
        } else if ("android" == m_System) {
          ChannelId = LobbyConfig.AndroidGameChannelStr;
        }
      }
    }
    internal string ClientLoginIp
    { 
      get { return m_ClientLoginIp; }
      set { m_ClientLoginIp = value; }
    }
    internal string ChannelId
    {
      get { return m_ChannelId; }
      set { m_ChannelId = value; }
    }
    internal RoleInfo FindUser(ulong userGuid)
    {
      RoleInfo ret = null;
      for (int i = 0; i < m_Users.Count; ++i) {
        if (m_Users[i].Guid == userGuid) {
          ret = m_Users[i];
          break;
        }
      }
      return ret;
    }
    internal double LastLoginTime
    {
      get { return m_LastLoginTime; }
      set { m_LastLoginTime = value; }
    }
    internal int LogicServerId
    {
      get { return m_LogicServerId; }
      set { m_LogicServerId = value; }
    }
    ///
    private double m_LastLoginTime = 0;
    private string m_ClientGameVersion = "0";
    private string m_ClientDeviceidId = "0";
    private string m_System = "all";
    private string m_ClientLoginIp = "127.0.0.1";
    private int m_LogicServerId = 1;
    private string m_ChannelId = "";
    ///
    private string m_AccountKey;        //玩家账号标识，当前设为设备ID
    private string m_AccountId;         //由畅游平台返回的玩家账号ID
    private AccountState m_CurrentState = AccountState.Offline; 
    private string m_NodeName;
    private List<RoleInfo> m_Users = new List<RoleInfo>();
  }
}
