using System;
using System.Text;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using Options;
using SharpSvrAPI;
using DashFire.DataStore;
using SharpSvrAPI.Messenger;

// TODO:
//  1. status report 
//  2. pull record count constantly
//  3. cache count update by DataCacheSystem
//  4. s/l time cost update by PersistentSystem, store the time cost for a range of time
//  5. cahce miss, update by DataCacheSystem
//  6. save pull time cost, update by DataCacheSystem
//  7. cpu load
//  8. mem usage
[System(InitOrder = 4)]
public class MonitorSystem : ISystem
{
  public void OnInit(SvrDriver driver)
  {
    driver_ = driver;
    string dsm_name = OptionCollection.Instance.Get<Config>().DSMName;
    dsm_emitter_ = driver.Messenger.To((byte)CoreMessageTypeExtend.kDataStore).Emitter(dsm_name);

    //rec_count_cmd_ = new MySqlCommand(SqlTemplate.Proc_RecordCount, DBConn.MySqlConn);
    //rec_count_cmd_.CommandType = CommandType.StoredProcedure;
    //rec_count_cmd_.Parameters.Add("@c", MySqlDbType.Int32).Direction = ParameterDirection.Output;
    //rec_count_cmd_.Prepare();

    last_save_timecost_.Data = new Dictionary<string, double>();

    uint tick_interval = OptionCollection.Instance.Get<Config>().StatusUpdateInterval;
    driver.SvrAPI.AddTimer(0, tick_interval, Tick);

    LogSys.Log(LOG_TYPE.INFO, "MonitorSystem initialized");

    driver_.GetSystem<DataOpSystem>().Enable = true;    
    LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "We're ONLINE and ready to rock.");
  }

  public void Dispose()
  {
  }

  public void Start()
  {
    running_ = true;
    LogSys.Log(LOG_TYPE.INFO, ConsoleColor.DarkGreen, "MonitorSystem Start");
  }

  public void Stop()
  {
    running_ = false;
    LogSys.Log(LOG_TYPE.INFO, ConsoleColor.DarkGreen, "MonitorSystem Stop");
  }

  public void UpdateSaveTimecost(string table, double t_ms)
  {
    lock (last_save_timecost_guard_)
    {
      last_save_timecost_.Data[table] = t_ms;
      last_save_timecost_.Dirty = true;
    }
  }

  public void UpdateDataPullTimecost(double t_ms)
  {
    last_data_pull_timecost_.Update(t_ms);
  }

  private void Tick(ServiceAPI svr_api, uint session, object context)
  {
    if (!running_) return;    
  }

  private void UpdateCpuMemUsage()
  {
    double sys_cpu_usage, proc_cpu_usage;
    int sys_total_mem, sys_used_mem, proc_vm, proc_rss;

    driver_.SvrAPI.CpuUsage(out sys_cpu_usage, out proc_cpu_usage);
    driver_.SvrAPI.MemUsage(out sys_total_mem, out sys_used_mem, out proc_vm, out proc_rss);

    sys_cpu_usage_.Update(sys_cpu_usage);
    proc_cpu_usage_.Update(proc_cpu_usage);
    sys_mem_usage_.Update((double)sys_used_mem / sys_total_mem);
    proc_mem_usage_.Update((double)proc_rss / sys_total_mem);
  }

  private void Report()
  {
    StringBuilder sb = new StringBuilder(4096);
    sb.AppendLine()
      .Append('-', 80)
      .AppendFormat("\nStatus Report {0}\n", DateTime.UtcNow);
    /*
    var metadata_sys = driver_.GetSystem<MetaDataSystem>();
    var report = NMPush_NodeStatus.CreateBuilder()
                                  .SetNodeName(metadata_sys.NodeName);

    if (db_rec_count_.Dirty)
    {
      sb.AppendFormat("Record Count: {0}\n", db_rec_count_.Data);
      report.SetDbRecCount(db_rec_count_.Data);
      db_rec_count_.Dirty = false;
    }

    if (cache_count_.Dirty)
    {
      sb.AppendFormat("Cache Count: {0}\n", cache_count_.Data);
      report.SetCacheCount(cache_count_.Data);
      cache_count_.Dirty = false;
    }

    if (cache_miss_.Dirty)
    {
      sb.AppendFormat("Cache Miss: {0}\n", cache_miss_.Data);
      report.SetCacheMiss(cache_miss_.Data);
      cache_miss_.Dirty = false;
    }

    if (last_save_timecost_.Dirty)
    {
      sb.AppendLine("Last Save Timecost:");
      lock (last_save_timecost_guard_)
      {
        double time_cost_sum = 0;
        foreach (var kv in last_save_timecost_.Data)
        {
          sb.AppendFormat("  {0}: {1} ms\n", kv.Key, kv.Value);
          time_cost_sum += kv.Value;
        }
        report.SetLastSaveTimecost(time_cost_sum);
        last_save_timecost_.Dirty = false;
      }
    }

    if (last_data_pull_timecost_.Dirty)
    {
      sb.AppendFormat("Last Data Pull Timecost: {0} ms\n", last_data_pull_timecost_.Data);
      report.SetLastDataPullTimecost(last_data_pull_timecost_.Data);
      last_data_pull_timecost_.Dirty = false;
    }

    if (sys_cpu_usage_.Dirty)
    {
      sb.AppendFormat("Sys CPU Usage: {0:P}\n", sys_cpu_usage_.Data);
      report.SetSysCpuUsage(sys_cpu_usage_.Data);
      sys_cpu_usage_.Dirty = false;
    }

    if (proc_cpu_usage_.Dirty)
    {
      sb.AppendFormat("Proc CPU Usage: {0:P}\n", proc_cpu_usage_.Data);
      report.SetProcCpuUsage(proc_cpu_usage_.Data);
      proc_cpu_usage_.Dirty = false;
    }

    if (sys_mem_usage_.Dirty)
    {
      sb.AppendFormat("Sys Mem Usage: {0:P}\n", sys_mem_usage_.Data);
      report.SetSysMemUsage(sys_mem_usage_.Data);
      sys_mem_usage_.Dirty = false;
    }

    if (proc_mem_usage_.Dirty)
    {
      sb.AppendFormat("Proc Mem Usage: {0:P}\n", proc_mem_usage_.Data);
      report.SetProcMemUsage(proc_mem_usage_.Data);
      proc_mem_usage_.Dirty = false;
    }

    dsm_emitter_.Emit(report.Build());
    sb.Append('-', 80);
    LogSys.Log(LOG_TYPE.DEBUG, sb.ToString());
    */
  }

  private class FlagData<T>
  {
    public FlagData()
    {
      Dirty = true;
    }

    public void Update(T d)
    {
      if (!Data.Equals(d))
      {
        Data = d;
        Dirty = true;
      }
    }

    public T Data = default(T);
    public bool Dirty = false;
  }

  private bool running_ = false;
  private SvrDriver driver_;
  private PBEmitter dsm_emitter_;
  //private MySqlCommand rec_count_cmd_;
  private FlagData<int> db_rec_count_ = new FlagData<int>();
  private FlagData<uint> cache_count_ = new FlagData<uint>();
  private FlagData<int> cache_miss_ = new FlagData<int>();
  private FlagData<Dictionary<string, double>> last_save_timecost_ = new FlagData<Dictionary<string, double>>();
  private object last_save_timecost_guard_ = new object();
  private FlagData<double> last_data_pull_timecost_ = new FlagData<double>();
  private FlagData<double> sys_cpu_usage_ = new FlagData<double>();
  private FlagData<double> proc_cpu_usage_ = new FlagData<double>();
  private FlagData<double> sys_mem_usage_ = new FlagData<double>();
  private FlagData<double> proc_mem_usage_ = new FlagData<double>();
}