using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Data.Odbc;

internal static class DBConn
{
  internal static MySqlConnection MySqlConn
  {
    get
    {
      KeepConnection();
      return m_MySqlConn;
    }
  }

  internal static void KeepConnection()
  {
    if (m_MySqlConn == null) {
      LogSys.Log(LOG_TYPE.INFO, "MySql Connection :{0}", GmServerConfig.MySqlConnectString);
      try {
        m_MySqlConn = new MySqlConnection(GmServerConfig.MySqlConnectString);
        m_MySqlConn.Open();
        LogSys.Log(LOG_TYPE.INFO, "MySql Connection Succeed !");
      } catch (System.Exception ex) {
        LogSys.Log(LOG_TYPE.INFO, "MySql Connection Error :{0}", ex);
      }
    } else {
      try {
        if (m_MySqlConn != null && m_MySqlConn.State == System.Data.ConnectionState.Closed) {
          m_MySqlConn.Open();
          LogSys.Log(LOG_TYPE.INFO, "MySql connection open again...", GmServerConfig.MySqlConnectString);
        }
      } catch (System.Exception ex) {
        LogSys.Log(LOG_TYPE.INFO, "MySql Connection Error :{0}", ex);
      }
    }
  }

  internal static void Close()
  {
    if (m_MySqlConn != null)
    {
      m_MySqlConn.Close();
      m_MySqlConn = null;
    }
  }

  [ThreadStatic]
  private static MySqlConnection m_MySqlConn = null;
}