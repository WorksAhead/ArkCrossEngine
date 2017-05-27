using System;
using System.Text;
using System.Diagnostics;
using CSharpCenterClient;

internal class LobbyConfig
{
  internal static string AppKeyStr
  {
    get { return s_Instance.m_AppKey; }
  }

  internal static string IOSGameChannelStr
  {
    get { return s_Instance.m_IOSGameChannel; }
  }

  internal static string AndroidGameChannelStr
  {
    get { return s_Instance.m_AndroidGameChannel; }
  }

  internal static string LogNormVersionStr
  {
    get { return s_Instance.m_LogNormVersion; }
  }

  internal static bool DataStoreAvailable
  {
    get { return s_Instance.m_DataStoreFlag; }
  }

  internal static bool GMServerAvailable
  {
    get { return s_Instance.m_GMServerFlag; }
  }

  internal static bool IsDebug
  {
    get { return s_Instance.m_Debug; }
  }

  internal static long UserDSSaveInterval
  {
    get { return s_Instance.m_UserSaveInterval; }
  }

  internal static uint ServerId
  {
    get { return s_Instance.m_ServerId; }
  }

  internal static bool ActivationCodeAvailable
  {
    get { return s_Instance.m_ActivateCodeAvailable; }
  }

  internal static int WorldId
  {
    get { return s_Instance.m_WorldId; }
  }

  internal static void Init()
  {
    StringBuilder sb = new StringBuilder(256);
    if (CenterClientApi.GetConfig("DataStoreFlag", sb, 256)) {
      string dsflag = sb.ToString();
      s_Instance.m_DataStoreFlag = (int.Parse(dsflag) != 0 ? true : false);
    }

    if (CenterClientApi.GetConfig("GMServerFlag", sb, 256)) {
      string gsflag = sb.ToString();
      s_Instance.m_GMServerFlag = (int.Parse(gsflag) != 0 ? true : false);
    }

    if (CenterClientApi.GetConfig("Debug", sb, 256)) {
      string debug = sb.ToString();
      s_Instance.m_Debug = (int.Parse(debug) != 0 ? true : false);
    }

    if (CenterClientApi.GetConfig("AppKey", sb, 256)) {
      string appkey = sb.ToString();
      s_Instance.m_AppKey = appkey;
    }

    if (CenterClientApi.GetConfig("IOSGameChannel", sb, 256)) {
      string iosgamechannel = sb.ToString();
      s_Instance.m_IOSGameChannel = iosgamechannel;
    }

    if (CenterClientApi.GetConfig("AndroidGameChannel", sb, 256)) {
      string androidgamechannel = sb.ToString();
      s_Instance.m_AndroidGameChannel = androidgamechannel;
    }

    if (CenterClientApi.GetConfig("LogNormVersion", sb, 256)) {
      string normver = sb.ToString();
      s_Instance.m_LogNormVersion = normver;
    }

    if (CenterClientApi.GetConfig("UserSaveInterval", sb, 256)) {
      string saveinterval = sb.ToString();
      s_Instance.m_UserSaveInterval = int.Parse(saveinterval);
    }

    if (CenterClientApi.GetConfig("ServerId", sb, 256)) {
      string serverid = sb.ToString();
      s_Instance.m_ServerId = uint.Parse(serverid);
    }
    if (CenterClientApi.GetConfig("ActivateCodeAvailable", sb, 256)) {
      string activatecode = sb.ToString();
      s_Instance.m_ActivateCodeAvailable = (int.Parse(activatecode) != 0 ? true : false);
    }
    if (CenterClientApi.GetConfig("worldid", sb, 256)) {
      string worldid = sb.ToString();
      s_Instance.m_WorldId = int.Parse(worldid);
    }
  }

  private bool m_DataStoreFlag = false;
  private bool m_GMServerFlag = false;
  private bool m_Debug = false;
  private string m_AppKey = "1407921103977";
  private string m_IOSGameChannel = "1010802002";
  private string m_AndroidGameChannel = "2010752003";
  private string m_LogNormVersion = "v1.8";
  private long m_UserSaveInterval = 180000;
  private uint m_ServerId = 1;
  private bool m_ActivateCodeAvailable = false;
  private int m_WorldId = -1;

  private static LobbyConfig s_Instance = new LobbyConfig();
}
