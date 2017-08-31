using System;
using System.Data;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using DashFire;
using DashFire.DataStore;
using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Descriptors;
using Google.ProtocolBuffers.Serialization;
using MySql.Data.MySqlClient;
using System.Data.Common;

internal static class DataLoadImplement
{
    private class ColumnInfo
    {
        internal ColumnInfo(string cn, string dt, string ct)
        {
            ColumnName = cn;
            DataType = dt;
            ColumnType = ct;
        }
        internal string ColumnName { get; private set; }
        internal string DataType { get; private set; }
        internal string ColumnType { get; private set; }
    }
    internal static void LoadSingleRowWithCallback(Type dsType, string primaryKey, ArkCrossEngine.MyAction<IMessage> resultCallback)
    {
        IMessage dataMsg = LoadSingleRow(dsType, primaryKey);
        if (null != resultCallback)
        {
            DataCacheSystem.Instance.QueueAction(resultCallback, dataMsg);
        }
    }
    internal static void LoadMultiRowsWithCallback(Type dsType, string foreignKey, ArkCrossEngine.MyAction<List<IMessage>> resultCallback)
    {
        List<IMessage> datas = LoadMultiRows(dsType, foreignKey);
        if (null != datas)
        {
            DataCacheSystem.Instance.QueueAction(resultCallback, datas);
        }
    }
    internal static void LoadTableWithCallback(Type dsType, ArkCrossEngine.MyAction<List<IMessage>> resultCallback)
    {
        List<IMessage> datas = LoadTable(dsType);
        if (null != resultCallback)
        {
            DataCacheSystem.Instance.QueueAction(resultCallback, datas);
        }
    }

    internal static IMessage LoadSingleRow(Type dsType, string primaryKey)
    {
        string tableName = GetTableName(dsType);
        MessageDescriptor md = (MessageDescriptor)dsType.InvokeMember(
          "Descriptor",
          BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty,
          null, null, null);
        // 取得主键的名称  
        string pri_key_name = md.Options.GetExtension<string>(Data.DsPrimaryKey);
        string statement = string.Format("select * from {0} where {1} = '{2}'", tableName, pri_key_name, primaryKey);
        //LogSys.Log(LOG_TYPE.INFO, "Load {0}: {1}", table, statement);
        List<IMessage> datas = ExecuteLoadSQL(dsType, tableName, statement, md);
        if (datas.Count == 1)
        {
            return datas[0];
        }
        else
        {
            return null;
        }
    }
    internal static List<IMessage> LoadMultiRows(Type dsType, string foreignKey)
    {
        string tableName = GetTableName(dsType);
        MessageDescriptor md = (MessageDescriptor)dsType.InvokeMember(
          "Descriptor",
          BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty,
          null, null, null);
        string foreignKeyName = md.Options.GetExtension<string>(Data.DsForeignKey);
        string statement = string.Format("select * from {0} where {1} = '{2}' and IsValid = 1",
                                          tableName, foreignKeyName, foreignKey);
        //LogSys.Log(LOG_TYPE.INFO, "Load {0}: {1}", table, statement);
        return ExecuteLoadSQL(dsType, tableName, statement, md);
    }
    internal static List<IMessage> LoadTable(Type dsType)
    {
        string tableName = GetTableName(dsType);
        MessageDescriptor md = (MessageDescriptor)dsType.InvokeMember(
          "Descriptor",
          BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty,
          null, null, null);
        string statement = string.Format("select * from {0} where IsValid = 1", tableName);
        //LogSys.Log(LOG_TYPE.INFO, "Load {0}: {1}", table, select_stat);
        return ExecuteLoadSQL(dsType, tableName, statement, md);
    }
    private static List<IMessage> ExecuteLoadSQL(Type tableType, string tableName, string statement, MessageDescriptor md)
    {
        List<IMessage> datas = new List<IMessage>();
        Dictionary<string, ColumnInfo> columnInfos = GetColumnInfo(tableName);
        IBuilder builder = (IBuilder)tableType.InvokeMember(
          "CreateBuilder",
          BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
          null, null, null);

        try
        {
            MySqlCommand cmd = new MySqlCommand(statement, DBConn.MySqlConn);
            cmd.CommandType = CommandType.Text;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    foreach (FieldDescriptor fd in md.Fields)
                    {
                        string col_name = fd.Name;
                        string func_name = string.Format("Set{0}", fd.CSharpOptions.PropertyName);
                        ColumnInfo ci = columnInfos[col_name];
                        object value = reader[col_name];
                        if (fd.FieldType == FieldType.Bytes)
                        {
                            value = ByteString.Unsafe.FromBytes((byte[])value);
                        }
                        builder.GetType().InvokeMember(func_name,
                          BindingFlags.Instance | BindingFlags.Public |
                          BindingFlags.InvokeMethod,
                          null,
                          builder,
                          new object[] { value });
                    }
                    datas.Add(builder.WeakBuild());
                }
                return datas;
            }
        }
        catch (Exception ex)
        {
            LogSys.Log(LOG_TYPE.ERROR, "Execute load SQL ERROR:{0}\n Stacktrace:{1} \n SQL statement:{2}\n", ex.Message, ex.StackTrace, statement);
            throw ex;
        }
    }
    private static string GetTableName(Type tableType)
    {
        string tableName = tableType.Name;
        if (tableName.StartsWith("DS_"))
        {
            tableName = tableName.Substring(3);     //去除“DS_”，得到数据表名
        }
        else
        {
            LogSys.Log(LOG_TYPE.ERROR, "Error data table type:{0}", tableType.Name);
            return string.Empty;
        }
        return tableName;
    }
    private static Dictionary<string, ColumnInfo> GetColumnInfo(string tableName)
    {
        Dictionary<string, ColumnInfo> columnInfos;
        if (!s_TableColumnDict.TryGetValue(tableName, out columnInfos))
        {
            lock (s_Guard)
            {
                // double check
                if (!s_TableColumnDict.TryGetValue(tableName, out columnInfos))
                {
                    // 访问information_schema取得table的列信息
                    string sql = string.Format("select column_name,data_type,column_type from information_schema.columns where table_name='{0}' and table_schema='{1}'",
                      tableName, DataStoreConfig.DataBase);
                    columnInfos = new Dictionary<string, ColumnInfo>();
                    MySqlCommand cmd = new MySqlCommand(sql, DBConn.MySqlConn);
                    cmd.CommandType = CommandType.Text;
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ci = new ColumnInfo(
                              reader.GetString(0),
                              reader.GetString(1),
                              reader.GetString(2)
                            //reader.GetString("column_name"),
                            //reader.GetString("data_type"),
                            //reader.GetString("column_type")
                            );
                            columnInfos.Add(ci.ColumnName, ci);
                        }
                    }
                    s_TableColumnDict.Add(tableName, columnInfos);
                }
            }
        }
        return columnInfos;
    }
    private static Dictionary<string, Dictionary<string, ColumnInfo>> s_TableColumnDict = new Dictionary<string, Dictionary<string, ColumnInfo>>();
    private static object s_Guard = new object();
}