using System;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;
using ArkCrossEngine;

namespace DashFire
{
  internal class DbThread : MyServerThread
  {
    internal DbThread(ServerAsyncActionProcessor actionQueue)
      : base(actionQueue)
    {
    }

    protected override void OnTick()
    {
      long curTime = TimeUtility.GetLocalMilliseconds();
      if (m_LastTickTime + c_TickInterval < curTime) {
        m_LastTickTime = curTime;

        DBConn.KeepConnection();
        try {
          MySqlConnection conn = DBConn.MySqlConn;
          using (MySqlCommand cmd = new MySqlCommand("select * from GowStar where 1=2", conn)) {
            cmd.ExecuteNonQuery();
          }
        } catch (Exception ex) {
          LogSys.Log(LOG_TYPE.INFO, "DbThread.Tick keep connection exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
      }
    }
    protected override void OnQuit()
    {
      DBConn.Close();
    }

    private long m_LastTickTime = 0;
    private const long c_TickInterval = 60000;
  }
}
