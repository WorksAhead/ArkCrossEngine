using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Data;

internal static class DataProcedureImplement
{
    internal static string GetDSNodeVersion ()
    {
        string version = string.Empty;
        try
        {
            using ( MySqlCommand cmd = new MySqlCommand() )
            {
                cmd.Connection = DBConn.MySqlConn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetDSNodeVersion";
                cmd.Parameters.Add("@dsversion", MySqlDbType.String).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                version = (string)cmd.Parameters["@dsversion"].Value;
            }
        }
        catch ( Exception ex )
        {
            LogSys.Log(LOG_TYPE.ERROR, "GetDSNodeVersion procedure ERROR:{0}\n Stacktrace:{1}", ex.Message, ex.StackTrace);
        }
        return version;
    }
}

