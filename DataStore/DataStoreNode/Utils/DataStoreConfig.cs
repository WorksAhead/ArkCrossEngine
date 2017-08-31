using System;
using System.Text;
using CSharpCenterClient;

/// <summary>
/// mysql¡¨Ω”≈‰÷√
/// </summary>
internal class DataStoreConfig
{
    internal static string DataBase
    {
        get { return s_Instance.m_Database; }
    }
    internal static uint PersistentInterval
    {
        get { return s_Instance.m_PersistentInterval; }
    }

    internal static string MySqlConnectString
    {
        get
        {
            string mysqlConnString = string.Format("SERVER={0};UID={1};PWD={2};DATABASE={3};CHARSET=utf8",
                                        s_Instance.m_Server, s_Instance.m_User, s_Instance.m_Password, s_Instance.m_Database);
            return mysqlConnString;
        }
    }
    internal static int LoadThreadNum
    {
        get { return s_Instance.m_LoadThreadNum; }
    }
    internal static int SaveThreadNum
    {
        get { return s_Instance.m_SaveThreadNum; }
    }

    internal static void Init()
    {
        StringBuilder sb = new StringBuilder(256);
        if (CenterClientApi.GetConfig("Server", sb, 256))
        {
            s_Instance.m_Server = sb.ToString();
        }
        if (CenterClientApi.GetConfig("User", sb, 256))
        {
            s_Instance.m_User = sb.ToString();
        }
        if (CenterClientApi.GetConfig("Password", sb, 256))
        {
            s_Instance.m_Password = sb.ToString();
        }
        if (CenterClientApi.GetConfig("Database", sb, 256))
        {
            s_Instance.m_Database = sb.ToString();
        }
        if (CenterClientApi.GetConfig("PersistentInterval", sb, 256))
        {
            s_Instance.m_PersistentInterval = uint.Parse(sb.ToString());
        }
        if (CenterClientApi.GetConfig("LoadThreadNum", sb, 256))
        {
            s_Instance.m_LoadThreadNum = int.Parse(sb.ToString());
        }
        if (CenterClientApi.GetConfig("SaveThreadNum", sb, 256))
        {
            s_Instance.m_SaveThreadNum = int.Parse(sb.ToString());
        }
    }

    private string m_Server = "127.0.0.1";
    private string m_User = "dfds";
    private string m_Password = "dfds";
    private string m_Database = "dsnode";
    private uint m_PersistentInterval = 120000;
    private int m_LoadThreadNum = 40;
    private int m_SaveThreadNum = 40;

    private static DataStoreConfig s_Instance = new DataStoreConfig();
}