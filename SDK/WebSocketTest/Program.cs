using System;
using System.Collections.Generic;
using System.Threading;
using DashFire;
using DashFire.Network;

namespace WebSocketTest
{
  class Program
  {
    static void Main(string[] args)
    {
      LogSystem.OnOutput += (Log_Type type, string msg)=>{
        Console.WriteLine(msg);
      };
      LobbyNetworkSystem.Instance.Init();
      LobbyNetworkSystem.Instance.LoginLobby("wss://127.0.0.1:9001", "test", "test");
      for (int ct = 0; ct < 10000; ++ct) {
        //if (ct == 61) {
        //  LobbyNetworkSystem.Instance.SelectScene(3);
        //}
        LobbyNetworkSystem.Instance.Tick();
        Console.WriteLine(ct);
        Thread.Sleep(1000);
      }
    }
  }
}
