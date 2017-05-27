/**
 * @file   TestServer.cs
 * @author carl <zhangnaisheng@webjet-5U364-34113>
 * @date   Mon Feb  4 15:56:38 2013
 * 
 * @brief  测试房间服务器功能
 * 
 */
using System.Linq;
using DashFire;
using RoomServer;
using System.Collections;
using System.Threading;
using Google.ProtocolBuffers;

namespace DashFire
{

  public class Program
  {
    public Program()
    {
    }

    // public static void Main()
    // {
    //     RoomManager room_mgr = new RoomManager(5, 3, 100);
    //     room_mgr.Init();
    //     IOManager io_mgr = IOManager.Instance;
    //     ArrayList peer_list = new ArrayList();
    //     peer_list.AddRange(room_mgr.GetAllUserPeer());
    //     io_mgr.Init(peer_list);
    //     room_mgr.StartRoomThread();

    //     User[] users = new User[2];
    //     users[0] = room_mgr.NewUser();
    //     users[0].Init();
    //     users[0].Name = "zhang";
    //     users[1] = room_mgr.NewUser();
    //     users[1].Init();
    //     users[1].Name = "wang";
    //     room_mgr.ActiveRoom("1", 1, users);

    //     Thread.Sleep(1000);
    //     Msg_CR_Ping bd = new Msg_CR_Ping();
    //     bd.SetPingTime(10);
    //     object msg = bd;
    //     users[0].GetPeer().InsertLogicMsg(ref msg);
    //     System.Console.WriteLine("test server over");
    // }

    public static void Main()
    {
    }
  }

}  // namespace dashfire
