using System;
using System.Reflection;
using System.Collections.Generic;

namespace Lobby.LoginSystem
{
  abstract class LoginMachine
  {
    public enum StateCode
    { 
      Initiate,
      Pending,
      Next,
      Finish,
    }

    public LoginMachine()
    {
      State = StateCode.Initiate;
    }

    public abstract void Run();
    public abstract void OnMessage(JsonMessage msg);

    // state of current machine
    public StateCode State { get; private set; }
    // name of next machine
    public string NextMachine { get; private set; }
    // parameters for next machine
    public object[] NextParams { get; private set; }
    // account name, set by initiate stage and pass on to every machine come next
    public string Account { get; set; }
    // node name, set like above
    public string NodeName { get; set; }
    // session, set like above
    // this session is came from the session of the login message
    public uint Session { get; set; }
    // expired at current phase
    public bool Expired { get { return expired_future_ != null && DateTime.UtcNow >= expired_future_; } }
    
    protected void Next(string machine_name, params object[] p)
    {
      if (null == machine_name)
      {
        State = StateCode.Finish;
      }
      else
      {
        NextMachine = machine_name;
        NextParams = p;
        State = StateCode.Next;
      }
    }

    protected void Pending(double? expire_after_s = null)
    {
      if (expire_after_s != null)
        expired_future_ = DateTime.UtcNow.AddSeconds((double)expire_after_s);
      State = StateCode.Pending;       
    }

    DateTime? expired_future_ = null;
  }

  static class LoginMachines
  {
    public static LoginMachine Create(string name, object[] parameters)
    {
      Type t;
      if (!type_cache_.TryGetValue(name, out t))
      {
        t = Assembly.GetExecutingAssembly().GetType("Lobby.LoginSystem." + name);
        if (null == t)
        {
          // TODO: exception
        }
        type_cache_.Add(name, t);
      }

      return (LoginMachine)Activator.CreateInstance(t, parameters);
    }

    private static Dictionary<string, Type> type_cache_ = new Dictionary<string, Type>();
  }
}