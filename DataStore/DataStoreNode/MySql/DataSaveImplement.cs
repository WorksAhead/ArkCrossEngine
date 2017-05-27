using System;
using System.Text;
using System.Data;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Descriptors;
using MySql.Data.MySqlClient;

internal static class DataSaveImplement
{
  /// <summary>
  /// 批量存储
  /// </summary>
  /// <param name="datas">存储的数据集</param>
  /// <returns></returns>
  internal static int BatchSave(List<IMessage> datas)
  {    
    if (datas.Count <= 0)
      return 0;    
   
    // 提取出table名称, protobuf中消息数据的命名规则是"DS_<tablename>"
    Type tableType = datas[0].GetType();
    string tableName = GetTableName(tableType);
    MessageDescriptor md = (MessageDescriptor)tableType.InvokeMember(
      "Descriptor", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty,null, null, null);
    List<string> rowKeys = new List<string>();
    foreach (FieldDescriptor fd in md.Fields) {
      rowKeys.Add(fd.Name);
    }
    StringBuilder sb = new StringBuilder(string.Format("replace into {0} ", tableName), 4096);
    sb.AppendFormat("({0})", string.Join(",", rowKeys));
    sb.Append(" values ");
    Dictionary<string, object> byteArrayParams = new Dictionary<string, object>();
    for (int i = 0; i < datas.Count; ++i) {
      IMessage data = datas[i];
      sb.Append("(");
      foreach (FieldDescriptor fd in md.Fields) {
        object value = tableType.InvokeMember(fd.CSharpOptions.PropertyName,
          BindingFlags.DeclaredOnly | BindingFlags.Instance |
          BindingFlags.Public | BindingFlags.GetProperty,
          null, data, null);
        string valueStr = null;
        if (fd.FieldType == FieldType.Bytes) {
          //如果类型是bytes, ProtocolBuffers.dll使用的ByteString, 需要将byte[]取出来
          //blob类型只能通过参数的方式写入数据库，不能通过SQL语句
          value = ByteString.Unsafe.GetBuffer(((ByteString)value));
          valueStr = string.Format("@{0}{1}", fd.Name, i);
          byteArrayParams.Add(valueStr, value);
          sb.AppendFormat("{0},", valueStr);
          continue;
        } else if (fd.FieldType == FieldType.Bool) {
          //如果类型是bool, 转成数值 0或1
          if ((bool)value == true) {
            valueStr = "1";
          } else {
            valueStr = "0";
          }
        } else {
          valueStr = value.ToString();
        }
        sb.AppendFormat("'{0}',", valueStr);
      }
      sb.Remove(sb.Length - 1, 1);
      sb.Append("),");     
    }
    sb.Remove(sb.Length - 1, 1);
    string statement = sb.ToString();
    int count = 0;
    try {
      using (MySqlCommand cmd = new MySqlCommand()) {
        cmd.Connection = DBConn.MySqlConn;
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = statement;
        cmd.CommandTimeout = 90;
        foreach (var kv in byteArrayParams) {
          MySqlParameter param = new MySqlParameter(kv.Key, MySql.Data.MySqlClient.MySqlDbType.Blob);
          param.Value = kv.Value;
          cmd.Parameters.Add(param);
        }
        //LogSys.Log(LOG_TYPE.INFO, "Save SQL Statement : {0}", cmd.CommandText);
        count = cmd.ExecuteNonQuery();
        LogSys.Log(LOG_TYPE.MONITOR, "BatchSave {0}: {1} rows affected. Thread:{2}", tableName, count, Thread.CurrentThread.ManagedThreadId);
      }
    } catch (Exception ex) {
      if (datas.Count > 100) {
        statement = "the sql statement is too large to log.";
      }
      LogSys.Log(LOG_TYPE.ERROR, "Batch Save ERROR.Table:{0}, Count:{1}, Statement:{2} \nError:{3}\nStacktrace:{4}",
                                  tableName, datas.Count, statement, ex.Message, ex.StackTrace);
      throw ex;
    }
    return count;
  }

  private static string GetTableName(Type tableType)
  {
    string tableName = tableType.Name;
    if (tableName.StartsWith("DS_")) {
      tableName = tableName.Substring(3);     //去除“DS_”，得到数据表名
    } else {
      LogSys.Log(LOG_TYPE.ERROR, "Error data table type:{0}", tableType.Name);
      return string.Empty;
    }
    return tableName;
  }

  internal static int DirectSave(string statement)
  {
    int count = -1;
    try {
      using (MySqlCommand cmd = new MySqlCommand()) {
        cmd.Connection = DBConn.MySqlConn;
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = statement;
        cmd.CommandTimeout = 90;       
        //LogSys.Log(LOG_TYPE.INFO, "Save SQL Statement : {0}", cmd.CommandText);
        count = cmd.ExecuteNonQuery();
        LogSys.Log(LOG_TYPE.MONITOR, "Direct Save SUCCESS. Count:{0}, Statement:{1}", count, statement);
      }
    } catch (Exception ex) {
      LogSys.Log(LOG_TYPE.ERROR, "Direct Save ERROR. Statement:{0} \nError:{1}\nStacktrace:{2}", statement, ex.Message, ex.StackTrace);
      throw ex;
    }
    return count;
  }
}