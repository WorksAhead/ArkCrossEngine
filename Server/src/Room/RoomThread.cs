using System;
using System.Threading;
using System.Collections.Generic;
using Messenger;
using Lobby_RoomServer;
using ArkCrossEngine;

namespace DashFire
{
    /// <remarks>
    /// 注意这个类的internal方法，都应考虑跨线程调用是否安全！！！
    /// </remarks>
    internal class RoomThread : MyServerThread
    {
        // constructors------------------------------------------------------------
        internal RoomThread(RoomManager roomMgr)
        {
            room_mgr_ = roomMgr;
            cur_thread_id_ = thread_id_creator_++;
            tick_interval_ = 100;
            max_room_count_ = 0;
            active_room_ = new List<Room>();
            unused_room_ = new List<Room>();
            room_pool_ = new RoomPool();
            scene_pool_ = new ScenePool();
        }

        internal bool Init(uint tick_interval, uint room_count,
            UserPool userpool, Connector conn)
        {
            tick_interval_ = tick_interval;
            max_room_count_ = room_count;
            room_pool_.Init(max_room_count_);
            user_pool_ = userpool;
            connector_ = conn;
            preactive_room_count_ = 0;
            LogSys.Log(LOG_TYPE.DEBUG, "thread {0} init ok.", cur_thread_id_);
            return true;
        }

        internal int IdleRoomCount()
        {
            return (int)(max_room_count_ - active_room_.Count - preactive_room_count_);
        }

        internal void PreActiveRoom()
        {
            Interlocked.Increment(ref preactive_room_count_);
        }

        internal void AddNewUsr(int roomid, User[] users)
        {
            Room rm = GetRoomByID(roomid);
            if (rm != null)
            {
                foreach (User us in users)
                {
                    LogSys.Log(LOG_TYPE.INFO, "[3] active room {0} scene {1} thread {2} for user {3}({4})", roomid, scenetype, cur_thread_id_, us.Guid, us.GetKey());
                    rm.AddNewUser(us);
                }
            }
        }

        internal void ActiveRoom(int roomid, int scenetype, User[] users)
        {
            LogSys.Log(LOG_TYPE.INFO, "[0] active room {0} scene {1} thread {2} for {3} users", roomid, scenetype, cur_thread_id_, users.Length);
            Room rm = room_pool_.NewRoom();
            if (null == rm)
            {
                //由于并发原因，有可能lobby或主线程会多发起一些房间激活，这种失败情形需要回收user。
                //我们通过预留一定数量的房间来降低这种情形发生的概率。
                foreach (User u in users)
                {
                    LogSys.Log(LOG_TYPE.INFO, "FreeUser {0} for {1} {2}, [RoomThread.ActiveRoom]", u.LocalID, u.Guid, u.GetKey());
                    user_pool_.FreeUser(u.LocalID);
                }
                Interlocked.Decrement(ref preactive_room_count_);

                LogSys.Log(LOG_TYPE.ERROR, "Failed active room {0} in thread {1}, preactive room count {2}",
                    roomid, cur_thread_id_, preactive_room_count_);
                return;
            }
            LogSys.Log(LOG_TYPE.INFO, "[1] active room {0} scene {1} thread {2} for {3} users", roomid, scenetype, cur_thread_id_, users.Length);
            rm.ScenePool = scene_pool_;
            rm.Init(roomid, scenetype, user_pool_, connector_);
            LogSys.Log(LOG_TYPE.INFO, "[2] active room {0} scene {1} thread {2} for {3} users", roomid, scenetype, cur_thread_id_, users.Length);
            if (null != users)
            {
                int maxScore = 0;
                int maxLevel = 0;
                foreach (User us in users)
                {
                    LogSys.Log(LOG_TYPE.INFO, "[3] active room {0} scene {1} thread {2} for user {3}({4})", roomid, scenetype, cur_thread_id_, us.Guid, us.GetKey());
                    rm.AddNewUser(us);
                    if (maxLevel < us.Level)
                        maxLevel = us.Level;
                    if (maxScore < us.ArgFightingScore)
                        maxScore = us.ArgFightingScore;
                }
                ///
                Scene scene = rm.GetActiveScene();
                if (null != scene && !scene.IsPvpScene)
                {
                    if (!scene.StorySystem.GlobalVariables.ContainsKey("@@MaxScore"))
                    {
                        scene.StorySystem.GlobalVariables.Add("@@MaxScore", maxScore);
                    }
                    if (!scene.StorySystem.GlobalVariables.ContainsKey("@@MaxLevel"))
                    {
                        scene.StorySystem.GlobalVariables.Add("@@MaxLevel", maxLevel);
                    }
                }
            }
            LogSys.Log(LOG_TYPE.INFO, "[4] active room {0} scene {1} thread {2} for {3} users", roomid, scenetype, cur_thread_id_, users.Length);
            /*
            //临时添加测试观察者
            for (int obIx = 0; obIx < 5; ++obIx) {
              uint key = 0xf0000000 + (uint)((roomid << 4) + obIx);
              string observerName = "Observer_" + key;
              if (rm.AddObserver(key, observerName, key)) {
                LogSys.Log(LOG_TYPE.DEBUG, "Add room observer successed, guid:{0} name:{1} key:{2}", key, observerName, key);
              } else {
                LogSys.Log(LOG_TYPE.DEBUG, "Add room observer failed, guid:{0} name:{1} key:{2}", key, observerName, key);
              }
            }
            */
            //工作全部完成后再加到激活房间列表，开始tick
            active_room_.Add(rm);
            Interlocked.Decrement(ref preactive_room_count_);

            LogSys.Log(LOG_TYPE.DEBUG, "active room {0} in thread {1}, preactive room count {2}",
                roomid, cur_thread_id_, preactive_room_count_);
        }

        internal void HandleReconnectUser(Msg_LR_ReconnectUser urMsg, PBChannel channel, int handle, uint seq)
        {
            bool ret = false;
            if (IsContainDroppedUser(urMsg.UserGuid))
            {
                ret = true;
            }
            Msg_RL_ReplyReconnectUser.Builder replyBuilder = Msg_RL_ReplyReconnectUser.CreateBuilder();
            replyBuilder.SetUserGuid(urMsg.UserGuid);
            replyBuilder.SetRoomID(urMsg.RoomID);
            replyBuilder.SetIsSuccess(ret);
            channel.Send(replyBuilder.Build());
        }

        internal void HandleUserRelive(Msg_LR_UserReLive msg)
        {
            Room room = GetRoomByID(msg.RoomID);
            Scene curScene = room.GetActiveScene();
            if (null != curScene)
            {
                curScene.DelayActionProcessor.QueueAction(curScene.OnUserRevive, msg);
            }
        }

        protected override void OnStart()
        {
            ActionNumPerTick = 256;
            TickSleepTime = 0;
            LogSys.Log(LOG_TYPE.DEBUG, "thread {0} start.", cur_thread_id_);
        }

        protected override void OnTick()
        {
            try
            {
                long curTime = TimeUtility.GetServerMilliseconds();
                if (m_LastLogTime + 60000 < curTime)
                {
                    m_LastLogTime = curTime;

                    DebugPoolCount((string msg) =>
                    {
                        LogSys.Log(LOG_TYPE.INFO, "RoomThread.ActionQueue {0}, thread {1}", msg, cur_thread_id_);
                    });
                }

                long tick_interval_us = tick_interval_ * 1000;
                TimeSnapshot.Start();
                DoTick();
                long elapsedTime = TimeSnapshot.End();
                if (elapsedTime >= tick_interval_us)
                {
                    if (elapsedTime >= tick_interval_us * 2)
                    {
                        LogSys.Log(LOG_TYPE.DEBUG, "*** Warning, RoomThread tick interval is {0} us !", elapsedTime);
                        foreach (Room room in active_room_)
                        {
                            Scene scene = room.GetActiveScene();
                            if (null != scene)
                            {
                                if (scene.SceneState == SceneState.Running)
                                {
                                    SceneProfiler profiler = scene.SceneProfiler;
                                    LogSys.Log(LOG_TYPE.DEBUG, "{0}", profiler.GenerateLogString(scene.SceneResId, scene.GameTime.ElapseMilliseconds));
                                }
                            }
                        }
                    }
                    Thread.Sleep(0);
                }
                else
                {
                    Thread.Sleep((int)(tick_interval_ - elapsedTime / 1000));
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        protected override void OnQuit()
        {
        }

        private void DoTick()
        {
            foreach (Room rm in active_room_)
            {
                rm.Tick();
                if (rm.CanClose)
                {
                    unused_room_.Add(rm);
                }
            }
            foreach (Room rm in unused_room_)
            {
                room_mgr_.RemoveActiveRoom(rm.RoomID);
                active_room_.Remove(rm);
                rm.Destroy();
                room_pool_.FreeRoom(rm.LocalID);
            }
            unused_room_.Clear();
        }

        private bool IsContainUser(ulong guid)
        {
            foreach (Room rm in active_room_)
            {
                if (rm.GetUserByGuid(guid) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsContainDroppedUser(ulong guid)
        {
            foreach (Room rm in active_room_)
            {
                User us = rm.GetUserByGuid(guid);
                if (null != us && ((int)UserControlState.UserDropped == us.UserControlState || !us.IsConnected()))
                    return true;
            }
            return false;
        }

        private bool ReplaceDroppedUser(ulong guid, ulong replacer, uint key)
        {
            foreach (Room rm in active_room_)
            {
                User us = rm.GetUserByGuid(guid);
                if (null != us && (int)UserControlState.UserDropped == us.UserControlState)
                {
                    return us.ReplaceDroppedUser(replacer, key);
                }
            }
            return false;
        }

        private Room GetRoomByID(int roomid)
        {
            foreach (Room rm in active_room_)
            {
                if (rm.RoomID == roomid)
                {
                    return rm;
                }
            }
            return null;
        }

        // thread control attribute------------------------------------------------
        private static uint thread_id_creator_ = 1;
        private uint cur_thread_id_;
        private uint tick_interval_;          // tick的间隔, 毫秒

        // room relative attribtes-------------
        private List<Room> active_room_;               // 已激活的房间ID列表
        private List<Room> unused_room_;
        private RoomPool room_pool_;
        private uint max_room_count_;
        private UserPool user_pool_;
        private ScenePool scene_pool_;

        private RoomManager room_mgr_;
        private Connector connector_;

        private int preactive_room_count_ = 0;

        private long m_LastLogTime = 0;
    }

}
