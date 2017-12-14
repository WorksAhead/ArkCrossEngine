using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ArkCrossEngine;
using Google.ProtocolBuffers;
using ArkCrossEngineMessage;
using Lobby_RoomServer;

namespace DashFire
{
    internal enum RoomState
    {
        Unuse,
        Active,
        Finish,
        Deactive,
    }

    internal class Room
    {
        internal RoomState CurrentState
        { get; set; }
        internal Room()
        {
            dispatcher_ = new Dispatcher();
            seat_player_dict_ = new MyDictionary<string, uint>();
            disconnected_users_ = new List<User>();
            request_delete_users_ = new List<User>();
            can_close_time_ = 0;
            room_users_ = new List<User>();

            for (int i = 0; i < room_observers_.Length; ++i)
            {
                room_observers_[i] = new Observer();
                room_observers_[i].OwnRoom = this;
            }
        }
        internal List<User> RoomUsers
        {
            get { return room_users_; }
        }
        internal Observer[] RoomObservers
        {
            get { return room_observers_; }
        }
        internal int RoomUserCount
        {
            get
            {
                return room_users_.Count;
            }
        }
        internal int GetActiveRoomUserCount()
        {
            int count = 0;
            foreach (User us in room_users_)
            {
                if ((int)UserControlState.User == us.UserControlState)
                {
                    ++count;
                }
            }
            return count;
        }
        internal long GetMinimizeElapsedDroppedTime()
        {
            long time = long.MaxValue;
            foreach (User us in room_users_)
            {
                if (null != us && (int)UserControlState.UserDropped == us.UserControlState)
                {
                    long _time = us.GetElapsedDroppedTime();
                    if (time > _time)
                        time = _time;
                }
            }
            return time;
        }
        internal User GetUserByGuid(ulong guid)
        {
            foreach (User us in room_users_)
            {
                if (us != null && us.Guid == guid)
                {
                    return us;
                }
            }
            return null;
        }
        internal User GetUserByRoleID(int roleId)
        {
            foreach (User us in room_users_)
            {
                if (us != null && us.RoleId == roleId)
                {
                    return us;
                }
            }
            return null;
        }
        internal int RoomID
        {
            get
            {
                return cur_room_id_;
            }
        }
        internal uint LocalID
        { set; get; }
        internal bool IsIdle
        { set; get; }
        internal bool CanClose
        {
            get
            {
                return can_close_time_ > 0 && can_close_time_ + c_close_wait_time_ < TimeUtility.GetServerMilliseconds();
            }
        }
        internal UserManager UserManager
        {
            get { return m_UserMgr; }
        }
        internal List<Scene> Scenes
        {
            get { return m_Scenes; }
        }
        internal int ActiveScene
        {
            get { return m_ActiveScene; }
            set { m_ActiveScene = value; }
        }
        internal Scene GetActiveScene()
        {
            Scene scene = null;
            if (m_ActiveScene >= 0 && m_ActiveScene < m_Scenes.Count)
                scene = m_Scenes[m_ActiveScene];
            return scene;
        }
        internal DashFire.ScenePool ScenePool
        {
            get { return m_ScenePool; }
            set { m_ScenePool = value; }
        }

        internal bool Init(int room_id, int scene_type, UserPool userpool, Connector conn)
        {
            LogSys.Log(LOG_TYPE.INFO, "[0] Room.Init {0} scene {1}", room_id, scene_type);
            cur_room_id_ = room_id;
            user_pool_ = userpool;
            connector_ = conn;
            can_close_time_ = 0;
            //todo：以后需要根据房间类型从配置文件读取场景列表，这里先构造一个用了
            m_ActiveScene = 0;
            Scene scene = m_ScenePool.NewScene();
            LogSys.Log(LOG_TYPE.INFO, "[1] Room.Init {0} scene {1}", room_id, scene_type);
            scene.SetRoom(this);
            //场景数据加载由加载线程执行（注：场景没有加载完成，场景状态仍然是sleep，Scene.Tick不会有实际的动作）
            SceneLoadThread.Instance.QueueAction(scene.LoadData, scene_type);
            m_Scenes.Add(scene);
            this.CurrentState = RoomState.Active;
            LogSys.Log(LOG_TYPE.DEBUG, "Room Initialize: {0}  Scene: {1}", room_id, scene_type);
            return true;
        }

        internal void Destroy()
        {
            LogSys.Log(LOG_TYPE.INFO, "room {0}({1}) destroy.", RoomID, LocalID);
            foreach (Scene scene in m_Scenes)
            {
                scene.Reset();
                m_ScenePool.RecycleScene(scene);
            }
            m_Scenes.Clear();
            this.CurrentState = RoomState.Unuse;

            int userCt = room_users_.Count;
            for (int i = userCt - 1; i >= 0; --i)
            {
                User user = room_users_[i];
                if (null != user)
                {
                    LogSys.Log(LOG_TYPE.INFO, "FreeUser {0} for {1} {2}, [Room.Destroy]", user.LocalID, user.Guid, user.GetKey());
                    user.Reset();
                    user_pool_.FreeUser(user.LocalID);
                    room_users_.RemoveAt(i);
                }
            }
            for (int i = 0; i < room_observers_.Length; ++i)
            {
                room_observers_[i].Reset();
            }
        }

        internal void Tick()
        {
            try
            {
                if (this.CurrentState != RoomState.Active && this.CurrentState != RoomState.Finish)
                    return;

                long curTime = TimeUtility.GetServerMilliseconds();
                if (m_LastLogTime + 60000 < curTime)
                {
                    m_LastLogTime = curTime;

                    LogSys.Log(LOG_TYPE.INFO, "Room.Tick {0}", RoomID);
                }

                if (this.CurrentState == RoomState.Active)
                {
                    Scene scene = GetActiveScene();
                    if (null != scene)
                    {
                        scene.Tick();
                        scene.SightTick();
                    }
                    disconnected_users_.Clear();
                    request_delete_users_.Clear();
                    foreach (User user in room_users_)
                    {
                        if (user != null)
                        {
                            user.Tick();
                            if (user.IsTimeout())
                            {
                                if (user.UserControlState == (int)UserControlState.User)
                                {
                                    disconnected_users_.Add(user);
                                }
                                else if (user.UserControlState == (int)UserControlState.Remove)
                                {
                                    request_delete_users_.Add(user);
                                }
                            }
                        }
                    }
                    foreach (User user in disconnected_users_)
                    {
                        DropUser(user);
                    }
                    foreach (User user in request_delete_users_)
                    {
                        RemoveUser(user);
                    }
                    //todo:观察者掉线处理
                    for (int i = 0; i < room_observers_.Length; ++i)
                    {
                        Observer observer = room_observers_[i];
                        if (!observer.IsIdle)
                        {
                            observer.Tick();
                        }
                    }
                    int userCount = GetActiveRoomUserCount();
                    if (userCount <= 0)
                    {
                        if (GetMinimizeElapsedDroppedTime() > c_finish_time_for_no_users_)
                        {
                            //若房间内玩家数目为0，结束战斗，关闭房间
                            EndBattle((int)CampIdEnum.Unkown);
                        }
                    }
                    //每个Tick结束，将空间属性同步给Peer，用于Peer转发消息
                    foreach (User user in room_users_)
                    {
                        if (null != user && null != user.Info && null != user.Info.GetMovementStateInfo())
                        {
                            RoomServer.RoomPeer peer = user.GetPeer();
                            if (null != peer)
                            {
                                MovementStateInfo info = user.Info.GetMovementStateInfo();
                                peer.Position = info.GetPosition3D();
                                peer.FaceDir = info.GetFaceDir();
                            }
                        }
                    }
                }
                else if (m_FinishTime + c_DeactiveWaitTime < curTime)
                {
                    Deactive();
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal bool AddNewUser(User newUser)
        {
            foreach (User us in room_users_)
            {
                if (us != null && us.Guid == newUser.Guid)
                {
                    //当前玩家已在游戏房间内
                    LogSys.Log(LOG_TYPE.DEBUG, "Add user success: User already in the room! RoomID:{0}, Guid:{1}, OldUser[{2}]({3}) NewUser[{4}]({5}) ",
                    cur_room_id_, us.Guid, us.LocalID, us.GetKey(), newUser.LocalID, newUser.GetKey());
                    LogSys.Log(LOG_TYPE.INFO, "FreeUser {0} for {1} {2}, [Room.AddNewUser]", newUser.LocalID, newUser.Guid, newUser.GetKey());
                    user_pool_.FreeUser(newUser.LocalID);
                    return true;
                }
            }
            can_close_time_ = 0;
            newUser.EnterRoomTime = TimeUtility.GetServerMilliseconds();
            newUser.OwnRoom = this;
            newUser.RegisterObservers(room_observers_);
            newUser.Info = m_UserMgr.AddUser(newUser.HeroId);
            newUser.Info.SetUnitId(GlobalVariables.GetUnitIdByCampId(newUser.CampId));
            newUser.Info.SetCampId(newUser.CampId);
            newUser.Info.SetLevel(newUser.Level);
            newUser.Info.GetMovementStateInfo().SetPosition2D(newUser.InitialPosX, newUser.InitialPosY);
            
            if ((int)UserControlState.Ai == newUser.UserControlState)
            {
                newUser.Info.GetAiStateInfo().AiLogic = (int)AiStateLogicId.PvpUser_General;
                newUser.IsEntered = true;
            }
            if (m_ActiveScene >= 0 && m_Scenes.Count > m_ActiveScene)
            {
                Scene scene = m_Scenes[m_ActiveScene];
                scene.EnterScene(newUser.Info);
                if ((int)UserControlState.Ai == newUser.UserControlState)
                {
                    Data_Unit unit = scene.MapData.ExtractData(DataMap_Type.DT_Unit, newUser.Info.GetUnitId()) as Data_Unit;
                    if (null != unit)
                    {
                        newUser.Info.GetMovementStateInfo().SetPosition(unit.m_Pos);
                        newUser.Info.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
                    }
                }
            }
            foreach (User otheruser in room_users_)
            {
                if (otheruser != null)
                {
                    otheruser.AddSameRoomUser(newUser);
                    newUser.AddSameRoomUser(otheruser);
                }
            }
            room_users_.Add(newUser);
            LogSys.Log(LOG_TYPE.DEBUG, "Add user success ! RoomID:{0} , UserGuid:{1}({2})",
              cur_room_id_, newUser.Guid, newUser.GetKey());

            if (null != newUser.Skill && 4 == newUser.Skill.Count)
            {
                newUser.Info.GetSkillStateInfo().RemoveAllSkill();
                newUser.Info.ResetSkill();
                for (int index = 0; index < newUser.Skill.Count; index++)
                {
                    if (newUser.Skill[index].SkillId > 0)
                    {
                        SkillInfo info = new SkillInfo(newUser.Skill[index].SkillId);
                        info.SkillLevel = newUser.Skill[index].SkillLevel;
                        info.Postions.SetCurSkillSlotPos(newUser.PresetIndex, (SlotPosition)(index + 1));
                        SkillCategory cur_skill_pos = SkillCategory.kNone;
                        if ((index + 1) == (int)SlotPosition.SP_A)
                        {
                            cur_skill_pos = SkillCategory.kSkillA;
                        }
                        else if ((index + 1) == (int)SlotPosition.SP_B)
                        {
                            cur_skill_pos = SkillCategory.kSkillB;
                        }
                        else if ((index + 1) == (int)SlotPosition.SP_C)
                        {
                            cur_skill_pos = SkillCategory.kSkillC;
                        }
                        else if ((index + 1) == (int)SlotPosition.SP_D)
                        {
                            cur_skill_pos = SkillCategory.kSkillD;
                        }
                        info.ConfigData.Category = cur_skill_pos;
                        newUser.Info.GetSkillStateInfo().AddSkill(info);
                        newUser.Info.ResetSkill();
                        ///
                        AddSubSkill(newUser, info.SkillId, cur_skill_pos, info.SkillLevel);
                    }
                }
                Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(newUser.HeroId);
                if (null != playerData && null != playerData.m_FixedSkillList
                  && playerData.m_FixedSkillList.Count > 0)
                {
                    foreach (int skill_id in playerData.m_FixedSkillList)
                    {
                        if (null == newUser.Info.GetSkillStateInfo().GetSkillInfoById(skill_id))
                        {
                            SkillInfo info = new SkillInfo(skill_id, 1);
                            newUser.Info.GetSkillStateInfo().AddSkill(info);
                            newUser.Info.ResetSkill();
                        }
                    }
                }
            }
            if (null != newUser.Equip && newUser.Equip.Count > 0)
            {
                newUser.Info.GetEquipmentStateInfo().Reset();
                for (int index = 0; index < newUser.Equip.Count; index++)
                {
                    if (newUser.Equip[index].ItemId > 0)
                    {
                        ItemDataInfo info = new ItemDataInfo(newUser.Equip[index].ItemRandomProperty);
                        info.ItemId = newUser.Equip[index].ItemId;
                        info.Level = newUser.Equip[index].ItemLevel;
                        info.RandomProperty = newUser.Equip[index].ItemRandomProperty;
                        info.ItemConfig = ItemConfigProvider.Instance.GetDataById(info.ItemId);
                        newUser.Info.GetEquipmentStateInfo().SetEquipmentData(index, info);
                    }
                }
            }
            if (null != newUser.Legacy && newUser.Legacy.Count > 0)
            {
                newUser.Info.GetLegacyStateInfo().Reset();
                for (int index = 0; index < newUser.Legacy.Count; index++)
                {
                    if (null != newUser.Legacy[index] && newUser.Legacy[index].ItemId > 0
                      && newUser.Legacy[index].IsUnlock)
                    {
                        ItemDataInfo info = new ItemDataInfo(newUser.Legacy[index].ItemRandomProperty);
                        info.ItemId = newUser.Legacy[index].ItemId;
                        info.Level = newUser.Legacy[index].ItemLevel;
                        info.RandomProperty = newUser.Legacy[index].ItemRandomProperty;
                        info.IsUnlock = newUser.Legacy[index].IsUnlock;
                        info.ItemConfig = ItemConfigProvider.Instance.GetDataById(info.ItemId);
                        newUser.Info.GetLegacyStateInfo().SetLegacyData(index, info);
                    }
                }
                newUser.Info.GetLegacyStateInfo().UpdateLegacyComplexAttr();
            }
            if (null != newUser.Partner)
            {
                newUser.Info.SetPartnerInfo(newUser.Partner);
            }
            return true;
        }

        private bool AddSubSkill(User user, int skill_id, SkillCategory pos, int level)
        {
            if (null == user)
                return false;
            SkillLogicData skill_data = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skill_id) as SkillLogicData;
            if (null != skill_data && skill_data.NextSkillId > 0)
            {
                SkillInfo info = new SkillInfo(skill_data.NextSkillId);
                info.SkillLevel = level;
                info.ConfigData.Category = pos;
                user.Info.GetSkillStateInfo().AddSkill(info);
                user.Info.ResetSkill();
                AddSubSkill(user, info.SkillId, pos, level);
            }
            return true;
        }

        internal void DeleteUser(User user)
        {
            if (null != user)
            {
                //向游戏客户端广播玩家掉线消息
                Msg_CRC_Exit bd = new Msg_CRC_Exit();
                bd.role_id = user.RoleId;
                user.BroadCastMsgToRoom(bd);
                user.UserControlState = (int)UserControlState.Remove;
                if (null != user.Info)
                {
                    //user.Info.Suicide();
                }
                LogSys.Log(LOG_TYPE.DEBUG, "Room {0} User {1}({2}) deleted.", RoomID, user.Guid, user.GetKey());
            }
        }

        internal void DropUser(User user)
        {
            //向游戏客户端广播玩家掉线消息
            Msg_CRC_Exit bd = new Msg_CRC_Exit();
            bd.role_id = user.RoleId;
            user.BroadCastMsgToRoom(bd);
            //向Lobby发送玩家掉线消息
            Msg_RL_UserQuit.Builder uaqBuilder = Msg_RL_UserQuit.CreateBuilder();
            uaqBuilder.SetRoomID(cur_room_id_);
            uaqBuilder.SetUserGuid(user.Guid);
            uaqBuilder.SetIsBattleEnd(false);
            uaqBuilder.SetX((long)user.Info.GetMovementStateInfo().PositionX);
            uaqBuilder.SetZ((long)user.Info.GetMovementStateInfo().PositionZ);
            connector_.SendMsgToLobby(uaqBuilder.Build());
            //控制状态改为掉线
            user.UserControlState = (int)UserControlState.UserDropped;
            if (null != user.Info)
            {
                // user.Info.Suicide();
            }
            LogSys.Log(LOG_TYPE.DEBUG, "Room {0} User {1}({2}) dropped.", RoomID, user.Guid, user.GetKey());
        }

        internal void EndBattle(int winnerCampID)
        {
            if (this.CurrentState == RoomState.Finish || this.CurrentState == RoomState.Deactive)
            {
                return;
            }
            foreach (User user in room_users_)
            {
                if (user != null)
                {
                    Msg_RL_UserQuit.Builder unqBuilder = Msg_RL_UserQuit.CreateBuilder();
                    unqBuilder.SetRoomID(cur_room_id_);
                    unqBuilder.SetUserGuid(user.Guid);
                    unqBuilder.SetIsBattleEnd(true);
                    unqBuilder.SetX((long)user.Info.GetMovementStateInfo().PositionX);
                    unqBuilder.SetZ((long)user.Info.GetMovementStateInfo().PositionZ);
                    connector_.SendMsgToLobby(unqBuilder.Build());
                }
            }
            //向Lobby发送战斗结束消息：RoomID，胜方阵营，开始时间，结束时间。。。
            Msg_RL_BattleEnd.Builder beBuilder = Msg_RL_BattleEnd.CreateBuilder();
            beBuilder.SetRoomID(RoomID);
            Msg_RL_BattleEnd.Types.WinnerCampEnum winCamp = Msg_RL_BattleEnd.Types.WinnerCampEnum.None;
            if (winnerCampID == (int)CampIdEnum.Blue)
            {
                winCamp = Msg_RL_BattleEnd.Types.WinnerCampEnum.Blue;
            }
            else
            {
                winCamp = Msg_RL_BattleEnd.Types.WinnerCampEnum.Red;
            }
            beBuilder.SetWinnerCamp(winCamp);
            //战斗数据
            foreach (User user in room_users_)
            {
                if (user != null)
                {
                    var battleRecord = user.Info.GetCombatStatisticInfo();
                    Msg_RL_UserBattleInfo.Builder ubrBuilder = Msg_RL_UserBattleInfo.CreateBuilder();
                    ubrBuilder.SetUserGuid(user.Guid);
                    Msg_RL_UserBattleInfo.Types.BattleResultEnum result = Msg_RL_UserBattleInfo.Types.BattleResultEnum.Unfinish;
                    if (winnerCampID != (int)CampIdEnum.Unkown)
                    {
                        if (user.CampId == winnerCampID)
                        {
                            result = Msg_RL_UserBattleInfo.Types.BattleResultEnum.Win;
                        }
                        else
                        {
                            result = Msg_RL_UserBattleInfo.Types.BattleResultEnum.Lost;
                        }
                    }
                    ubrBuilder.SetBattleResult(result);
                    ubrBuilder.SetMoney(user.Info.Money);
                    CombatStatisticInfo combatInfo = user.Info.GetCombatStatisticInfo();
                    ubrBuilder.SetHitCount(combatInfo.HitCount);
                    ubrBuilder.SetMaxMultiHitCount(combatInfo.MaxMultiHitCount);
                    ubrBuilder.SetTotalDamageToMyself(combatInfo.TotalDamageToMyself);
                    ubrBuilder.SetTotalDamageFromMyself(combatInfo.TotalDamageFromMyself);
                    beBuilder.AddUserBattleInfos(ubrBuilder.Build());
                }
            }
            connector_.SendMsgToLobby(beBuilder.Build());
            this.CurrentState = RoomState.Finish;
            m_FinishTime = TimeUtility.GetServerMilliseconds();
            LogSys.Log(LOG_TYPE.DEBUG, "Room {0}({1}) EndBattle.", RoomID, LocalID);
        }

        internal void MpveEndBattle(int kill_npc_ct)
        {
            if (this.CurrentState == RoomState.Finish || this.CurrentState == RoomState.Deactive)
            {
                return;
            }
            foreach (User user in room_users_)
            {
                if (user != null)
                {
                    Msg_RL_UserQuit.Builder unqBuilder = Msg_RL_UserQuit.CreateBuilder();
                    unqBuilder.SetRoomID(cur_room_id_);
                    unqBuilder.SetUserGuid(user.Guid);
                    unqBuilder.SetIsBattleEnd(true);
                    connector_.SendMsgToLobby(unqBuilder.Build());
                }
            }
            //向Lobby发送战斗结束消息：RoomID，胜方阵营，开始时间，结束时间。。。
            Msg_RL_BattleEnd.Builder beBuilder = Msg_RL_BattleEnd.CreateBuilder();
            beBuilder.SetRoomID(RoomID);
            Msg_RL_BattleEnd.Types.WinnerCampEnum winCamp = Msg_RL_BattleEnd.Types.WinnerCampEnum.None;
            beBuilder.SetWinnerCamp(winCamp);
            //战斗数据
            foreach (User user in room_users_)
            {
                if (user != null)
                {
                    var battleRecord = user.Info.GetCombatStatisticInfo();
                    Msg_RL_UserBattleInfo.Builder ubrBuilder = Msg_RL_UserBattleInfo.CreateBuilder();
                    ubrBuilder.SetUserGuid(user.Guid);
                    Msg_RL_UserBattleInfo.Types.BattleResultEnum result = Msg_RL_UserBattleInfo.Types.BattleResultEnum.Unfinish;
                    if (kill_npc_ct > 0)
                    {
                        result = Msg_RL_UserBattleInfo.Types.BattleResultEnum.Win;
                    }
                    else
                    {
                        result = Msg_RL_UserBattleInfo.Types.BattleResultEnum.Lost;
                    }
                    ubrBuilder.SetBattleResult(result);
                    ubrBuilder.SetMoney(user.Info.Money);
                    CombatStatisticInfo combatInfo = user.Info.GetCombatStatisticInfo();
                    ubrBuilder.SetHitCount(combatInfo.HitCount);
                    ubrBuilder.SetKillNpcCount(kill_npc_ct);
                    ubrBuilder.SetMaxMultiHitCount(combatInfo.MaxMultiHitCount);
                    ubrBuilder.SetTotalDamageToMyself(combatInfo.TotalDamageToMyself);
                    ubrBuilder.SetTotalDamageFromMyself(combatInfo.TotalDamageFromMyself);
                    beBuilder.AddUserBattleInfos(ubrBuilder.Build());
                }
            }
            connector_.SendMsgToLobby(beBuilder.Build());
            this.CurrentState = RoomState.Finish;
            m_FinishTime = TimeUtility.GetServerMilliseconds();
            LogSys.Log(LOG_TYPE.DEBUG, "Room {0}({1}) MpveEndBattle.", RoomID, LocalID);
        }

        internal bool AddObserver(ulong guid, string name, uint key)
        {
            bool ret = false;
            Observer observer = GetUnusedObserver();
            if (null != observer)
            {
                observer.IsIdle = false;
                observer.Guid = guid;
                observer.Name = name;
                observer.SetKey(key);
                ret = true;
            }
            return ret;
        }

        internal void DropObserver(Observer observer)
        {
            if (observer.IsConnected())
            {
                observer.Disconnect();
            }
            observer.IsEntered = false;
            //observer.IsIdle = true;
        }
        internal void NoticeRoomClosing()
        {
            foreach (User user in room_users_)
            {
                if (user != null && (int)UserControlState.UserDropped == user.UserControlState)
                {
                    Msg_RL_UserQuit.Builder unqBuilder = Msg_RL_UserQuit.CreateBuilder();
                    unqBuilder.SetRoomID(cur_room_id_);
                    unqBuilder.SetUserGuid(user.Guid);
                    unqBuilder.SetIsBattleEnd(true);
                    connector_.SendMsgToLobby(unqBuilder.Build());
                }
            }
        }

        private void Deactive()
        {
            //准备关闭房间
            for (int index = room_users_.Count - 1; index >= 0; --index)
            {
                User user = room_users_[index];
                RemoveUser(user);
            }
            this.CurrentState = RoomState.Deactive;
            can_close_time_ = TimeUtility.GetServerMilliseconds();
            LogSys.Log(LOG_TYPE.DEBUG, "Room {0}({1}) Deactive.", RoomID, LocalID);
        }

        private void RemoveUser(User user)
        {
            if (user == null)
            {
                return;
            }
            foreach (User otheruser in room_users_)
            {
                if (null != otheruser && otheruser != user)
                {
                    otheruser.RemoveSameRoomUser(user);
                }
            }
            user.ClearSameRoomUser();
            if (m_ActiveScene >= 0 && m_Scenes.Count > m_ActiveScene)
            {
                Scene scene = m_Scenes[m_ActiveScene];
                scene.LeaveScene(user.Info);
            }
            LogSys.Log(LOG_TYPE.INFO, "FreeUser {0} for {1} {2}, [Room.RemoveUser]", user.LocalID, user.Guid, user.GetKey());
            m_UserMgr.RemoveUser(user.Info.GetId());
            user_pool_.FreeUser(user.LocalID);
            room_users_.Remove(user);
        }
        private Observer GetUnusedObserver()
        {
            Observer ret = null;
            for (int i = 0; i < room_observers_.Length; ++i)
            {
                Observer observer = room_observers_[i];
                if (observer.IsIdle)
                {
                    ret = observer;
                    break;
                }
            }
            return ret;
        }

        private const long c_close_wait_time_ = 1000;
        private const long c_finish_time_for_no_users_ = 1000;
        private int cur_room_id_;
        private MyDictionary<string, uint> seat_player_dict_;
        private Dispatcher dispatcher_;

        private List<User> room_users_;

        private UserPool user_pool_;
        private List<User> disconnected_users_;
        private List<User> request_delete_users_;
        private Connector connector_;
        private long can_close_time_;

        //每个房间固定几个观察者，就不另外使用内存池了(如果需要多人观战功能，应该独立开发观战服务器，观战服务器作观察者)
        private const int c_max_observer_num_ = 5;
        private Observer[] room_observers_ = new Observer[c_max_observer_num_];

        private UserManager m_UserMgr = new UserManager(64);
        private List<Scene> m_Scenes = new List<Scene>();
        private int m_ActiveScene = 0;
        private ScenePool m_ScenePool = null;

        private const long c_DeactiveWaitTime = 3000;
        private long m_FinishTime = 0;
        private long m_LastLogTime = 0;
    }
}
