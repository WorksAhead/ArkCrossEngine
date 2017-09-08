using System;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using Lidgren.Network;
using Messenger;
using Lobby_RoomServer;
using RoomServer;
using ArkCrossEngine;

namespace DashFire
{
    /// <remarks>
    /// 注意这个类的internal方法，都应考虑跨线程调用是否安全！！！
    /// </remarks>
    internal class RoomManager
    {
        internal RoomManager(uint thread_amount, uint room_amount)
        {
            thread_amount_ = thread_amount;
            room_amount_ = room_amount;
            roomthread_list_ = new RoomThread[thread_amount];
            user_pool_size_ = thread_amount * room_amount * 3;
            user_pool_ = new UserPool();
            thread_tick_interval_ = 50;
        }

        internal RoomManager(uint thread_amount, uint room_amount, uint tick_interval, Connector conn)
        {
            thread_amount_ = thread_amount;
            room_amount_ = room_amount;
            roomthread_list_ = new RoomThread[thread_amount];
            user_pool_size_ = thread_amount * room_amount * 3;
            user_pool_ = new UserPool();
            dispatcher_ = new Dispatcher();
            thread_tick_interval_ = tick_interval;
            connector_ = conn;
        }

        internal bool Init()
        {
            lock_obj_ = new object();
            active_rooms_ = new Dictionary<int, int>();
            user_pool_.Init(user_pool_size_);

            // 初始化房间线程
            for (int i = 0; i < thread_amount_; ++i)
            {
                roomthread_list_[i] = new RoomThread(this);
                roomthread_list_[i].Init(thread_tick_interval_, room_amount_, user_pool_, connector_);
            }
            return true;
        }

        internal void StartRoomThread()
        {
            SceneLoadThread.Instance.Start();
            for (int i = 0; i < thread_amount_; ++i)
            {
                roomthread_list_[i].Start();
            }
        }

        internal void StopRoomThread()
        {
            for (int i = 0; i < thread_amount_; ++i)
            {
                roomthread_list_[i].Stop();
            }
            SceneLoadThread.Instance.Stop();
        }

        internal int GetIdleRoomCount()
        {
            int count = 0;
            for (int i = 0; i < thread_amount_; ++i)
            {
                count += roomthread_list_[i].IdleRoomCount();
            }
            return count;
        }

        internal int GetUserCount()
        {
            return user_pool_.GetUsedCount();
        }

        internal int GetActiveRoomThreadIndex(int roomid)
        {
            int ix = -1;
            lock (lock_obj_)
            {
                active_rooms_.TryGetValue(roomid, out ix);
            }
            return ix;
        }

        internal void AddActiveRoom(int roomid, int roomthreadindex)
        {
            lock (lock_obj_)
            {
                if (active_rooms_.ContainsKey(roomid))
                {
                    active_rooms_[roomid] = roomthreadindex;
                }
                else
                {
                    active_rooms_.Add(roomid, roomthreadindex);
                }
            }
        }

        internal void RemoveActiveRoom(int roomid)
        {
            lock (lock_obj_)
            {
                active_rooms_.Remove(roomid);
            }
        }

        internal bool ActiveRoom(int roomid, int scenetype, User[] users)
        {
            int thread_id = GetIdleThread();
            if (thread_id < 0)
            {
                LogSys.Log(LOG_TYPE.ERROR, "all room are using, active room failed!");
                foreach (User u in users)
                {
                    LogSys.Log(LOG_TYPE.INFO, "FreeUser {0} for {1} {2}, [RoomManager.ActiveRoom]", u.LocalID, u.Guid, u.GetKey());
                    user_pool_.FreeUser(u.LocalID);
                }
                return false;
            }
            RoomThread roomThread = roomthread_list_[thread_id];
            AddActiveRoom(roomid, thread_id);
            roomThread.PreActiveRoom();
            LogSys.Log(LOG_TYPE.INFO, "queue active room {0} scene {1} thread {2} for {3} users", roomid, scenetype, thread_id, users.Length);
            roomThread.QueueAction(roomThread.ActiveRoom, roomid, scenetype, users);
            return true;
        }

        //--------------------------------------
        internal void RegisterMsgHandler(PBChannel channel)
        {
            channel.Register<Msg_LR_CreateBattleRoom>(HandleCreateBattleRoom);
            channel.Register<Msg_LR_ReconnectUser>(HandleReconnectUser);
            channel.Register<Msg_LR_UserReLive>(HandleUserRelive);
        }
        //--------------------------------------
        private void HandleCreateBattleRoom(Msg_LR_CreateBattleRoom createRoomMsg, PBChannel channel, int handle, uint seq)
        {
            LogSys.Log(LOG_TYPE.DEBUG, "channel:{0}, seq:{1}", channel, seq);
            bool canContinue = true;
            //先检查是否玩家已经在room上。
            foreach (Msg_LR_RoomUserInfo rui in createRoomMsg.UsersList)
            {
                if (RoomPeerMgr.Instance.IsKeyExist(rui.Key))
                {
                    canContinue = false;
                    LogSys.Log(LOG_TYPE.WARN, "User is already in room. UserGuid:{0}, Key:{1}", rui.Guid, rui.Key);
                    break;
                }
            }
            if (!canContinue)
            {
                Msg_RL_ReplyCreateBattleRoom.Builder replyBuilder0 = Msg_RL_ReplyCreateBattleRoom.CreateBuilder();
                replyBuilder0.SetRoomId(createRoomMsg.RoomId);
                replyBuilder0.SetIsSuccess(false);
                channel.Send(replyBuilder0.Build());
                return;
            }
            List<User> users = new List<User>();
            foreach (Msg_LR_RoomUserInfo rui in createRoomMsg.UsersList)
            {
                User rsUser = user_pool_.NewUser();
                LogSys.Log(LOG_TYPE.INFO, "NewUser {0} for {1} {2}", rsUser.LocalID, rui.Guid, rui.Key);
                rsUser.Init();
                if (!rsUser.SetKey(rui.Key))
                {
                    LogSys.Log(LOG_TYPE.WARN, "user who's key is {0} already in room!", rui.Key);
                    LogSys.Log(LOG_TYPE.INFO, "FreeUser {0} for {1} {2}, [RoomManager.HandleCreateBattleRoom]", rsUser.LocalID, rui.Guid, rui.Key);
                    user_pool_.FreeUser(rsUser.LocalID);
                    continue;
                }
                rsUser.Guid = rui.Guid;
                rsUser.Name = rui.Nick;
                rsUser.HeroId = rui.Hero;
                rsUser.CampId = rui.Camp;
                rsUser.Level = rui.Level;
                rsUser.ArgFightingScore = rui.ArgScore;
                if (rui.IsMachine == true)
                    rsUser.UserControlState = (int)UserControlState.Ai;
                else
                    rsUser.UserControlState = (int)UserControlState.User;
                //装备数据
                for (int index = 0; index < rui.ShopEquipmentsIdCount; ++index)
                {
                    rsUser.ShopEquipmentsId.Add(rui.GetShopEquipmentsId(index));
                }
                if (null != rsUser.Skill)
                {
                    rsUser.Skill.Clear();
                    for (int i = 0; i < rui.SkillsCount; i++)
                    {
                        SkillTransmitArg skill_arg = new SkillTransmitArg();
                        skill_arg.SkillId = rui.SkillsList[i].SkillId;
                        skill_arg.SkillLevel = rui.SkillsList[i].SkillLevel;
                        rsUser.Skill.Add(skill_arg);
                    }
                    if (rui.HasPresetIndex)
                    {
                        rsUser.PresetIndex = rui.PresetIndex;
                    }
                }
                ///
                if (null != rsUser.Equip)
                {
                    rsUser.Equip.Clear();
                    for (int i = 0; i < rui.EquipsCount; i++)
                    {
                        ItemTransmitArg equip_arg = new ItemTransmitArg();
                        equip_arg.ItemId = rui.EquipsList[i].EquipId;
                        equip_arg.ItemLevel = rui.EquipsList[i].EquipLevel;
                        equip_arg.ItemRandomProperty = rui.EquipsList[i].EquipRandomProperty;
                        rsUser.Equip.Add(equip_arg);
                    }
                }
                ///
                if (null != rsUser.Legacy)
                {
                    rsUser.Legacy.Clear();
                    for (int i = 0; i < rui.LegacysCount; i++)
                    {
                        ItemTransmitArg legacy_arg = new ItemTransmitArg();
                        legacy_arg.ItemId = rui.LegacysList[i].LegacyId;
                        legacy_arg.ItemLevel = rui.LegacysList[i].LegacyLevel;
                        legacy_arg.ItemRandomProperty = rui.LegacysList[i].LegacyRandomProperty;
                        legacy_arg.IsUnlock = rui.LegacysList[i].LegacyIsUnlock;
                        rsUser.Legacy.Add(legacy_arg);
                    }
                }
                ///
                if (null != rsUser.XSouls)
                {
                    rsUser.XSouls.GetAllXSoulPartData().Clear();
                    for (int i = 0; i < rui.XSoulsCount; i++)
                    {
                        ItemDataInfo item = new ItemDataInfo();
                        item.ItemId = rui.XSoulsList[i].ItemId;
                        item.Level = rui.XSoulsList[i].Level;
                        item.Experience = rui.XSoulsList[i].Experience;
                        ItemConfig configer = ItemConfigProvider.Instance.GetDataById(item.ItemId);
                        item.ItemConfig = configer;
                        if (configer != null)
                        {
                            XSoulPartInfo part_info = new XSoulPartInfo((XSoulPart)configer.m_WearParts, item);
                            part_info.ShowModelLevel = rui.XSoulsList[i].ModelLevel;
                            rsUser.XSouls.SetXSoulPartData((XSoulPart)configer.m_WearParts, part_info);
                        }
                    }
                }
                // partner
                if (null != rui.Partner)
                {
                    PartnerConfig partnerConfig = PartnerConfigProvider.Instance.GetDataById(rui.Partner.PartnerId);
                    if (null != partnerConfig)
                    {
                        PartnerInfo partnerInfo = new PartnerInfo(partnerConfig);
                        partnerInfo.CurAdditionLevel = rui.Partner.PartnerLevel;
                        partnerInfo.CurSkillStage = rui.Partner.PartnerStage;
                        rsUser.Partner = partnerInfo;
                    }
                }
                users.Add(rsUser);
                LogSys.Log(LOG_TYPE.DEBUG, "enter room {0} scene {1} user info guid={2}, name={3}, key={4}, camp={5}", createRoomMsg.RoomId, createRoomMsg.SceneType, rui.Guid, rui.Nick, rui.Key, rui.Camp);
            }
            //临时测试人机机制
            /*
            Data_SceneConfig sceneCfg = SceneConfigProvider.Instance.GetSceneConfigById(createRoomMsg.SceneType);
            if (1 == createRoomMsg.UsersCount && null != sceneCfg && sceneCfg.m_Type == (int)SceneTypeEnum.TYPE_PVP) {
              for (int i = 0; i < 9; ++i) {
                User rsUser = room_mgr_.NewUser();
                rsUser.Init();
                rsUser.SetKey(0xffffffff);
                rsUser.Guid = 0xffffffff;
                rsUser.Name = "Computer" + i;
                rsUser.HeroId = CrossEngineHelper.Random.Next(1, 3);
                rsUser.CampId = (i < 4 ? (int)CampIdEnum.Blue : (int)CampIdEnum.Red);
                rsUser.UserControlState = (int)UserControlState.Ai;
                users.Add(rsUser);
                LogSys.Log(LOG_TYPE.DEBUG, "Computer enter room");
              }
            }
            */
            bool ret = false;
            if (users.Count == 0)
            {
                LogSys.Log(LOG_TYPE.WARN, "no user enter room");
                ret = false;
            }
            else
            {
                ret = ActiveRoom(createRoomMsg.RoomId, createRoomMsg.SceneType, users.ToArray());
            }
            if (ret)
            {
                LogSys.Log(LOG_TYPE.DEBUG, "user enter room success.");
            }
            else
            {
                LogSys.Log(LOG_TYPE.DEBUG, "user enter room failed!");
            }
            Msg_RL_ReplyCreateBattleRoom.Builder replyBuilder = Msg_RL_ReplyCreateBattleRoom.CreateBuilder();
            replyBuilder.SetRoomId(createRoomMsg.RoomId);
            replyBuilder.SetIsSuccess(ret);
            channel.Send(replyBuilder.Build());
        }

        private void HandleReconnectUser(Msg_LR_ReconnectUser urMsg, PBChannel channel, int handle, uint seq)
        {
            int ix = GetActiveRoomThreadIndex(urMsg.RoomID);
            if (ix < 0)
            {
                Msg_RL_ReplyReconnectUser.Builder replyBuilder = Msg_RL_ReplyReconnectUser.CreateBuilder();
                replyBuilder.SetUserGuid(urMsg.UserGuid);
                replyBuilder.SetRoomID(urMsg.RoomID);
                replyBuilder.SetIsSuccess(false);
                channel.Send(replyBuilder.Build());
            }
            else
            {
                RoomThread roomThread = roomthread_list_[ix];
                roomThread.QueueAction(roomThread.HandleReconnectUser, urMsg, channel, handle, seq);
            }
        }

        private void HandleUserRelive(Msg_LR_UserReLive msg, PBChannel channel, int handle, uint seq)
        {
            int ix = GetActiveRoomThreadIndex(msg.RoomID);
            if (ix >= 0)
            {
                RoomThread roomThread = roomthread_list_[ix];
                roomThread.QueueAction(roomThread.HandleUserRelive, msg);
            }
        }

        // private functions--------------------
        private int GetIdleThread()
        {
            int most_idle_thread_id = 0;
            for (int i = 1; i < thread_amount_; ++i)
            {
                if (roomthread_list_[most_idle_thread_id].IdleRoomCount() <
                    roomthread_list_[i].IdleRoomCount())
                {
                    most_idle_thread_id = i;
                }
            }
            if (roomthread_list_[most_idle_thread_id].IdleRoomCount() < GlobalVariables.c_PreservedRoomCountPerThread)
            {
                return -1;
            }
            return most_idle_thread_id;
        }

        // private attributes-------------------
        private uint thread_amount_;                         // 房间服务器的线程数
        private uint room_amount_;                           // 房间服务器的房间数
        private uint thread_tick_interval_;                  // 线程心跳间隔
        private uint user_pool_size_;
        private Dispatcher dispatcher_;
        private RoomThread[] roomthread_list_;               // 线程列表
        private UserPool user_pool_;
        private Connector connector_;

        private object lock_obj_;
        private Dictionary<int, int> active_rooms_;
    }

} // namespace dashfire
