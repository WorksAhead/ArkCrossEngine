using System;
using System.Collections.Generic;
using System.Threading;
using ArkCrossEngine;
using ArkCrossEngine.Network;
using ArkCrossEngine.GmCommands;
using SkillSystem;
using LitJson;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace LobbyRobot
{
    internal class MailItem
    {
        internal int m_ItemId;
        internal int m_ItemNum;
    }
    internal class MailInfo
    {
        internal bool m_AlreadyRead;
        internal ulong m_MailGuid;
        internal string m_Title;
        internal string m_Sender;
        internal ModuleMailTypeEnum m_Module = ModuleMailTypeEnum.None;
        internal DateTime m_SendTime;
        internal string m_Text;
        internal List<MailItem> m_Items;
        internal int m_Money;
        internal int m_Gold;
        internal int m_Stamina;
    }
    internal enum GeneralOperationResult : int
    {
        LC_Succeed = 0,
        LC_Failure_CostError = 1,
        LC_Failure_Position = 2,
        LC_Failure_NotUnLock = 3,
        LC_Failure_LevelError = 4,
        LC_Failure_Overflow = 5,
        LC_Failure_Time = 6,
        LC_Failure_Unknown = 7,
    }
    internal enum SlotPosition : int
    {
        SP_None = 0,
        SP_A,
        SP_B,
        SP_C,
        SP_D,
    }
    internal sealed class Robot
    {
        internal LobbyNetworkSystem LobbyNetworkSystem
        {
            get { return m_LobbyNetworkSystem; }
        }

        internal NetworkSystem RoomNetworkSystem
        {
            get { return m_RoomNetworkSystem; }
        }
        internal SkillAnalysis SkillAnalysis
        {
            get { return m_SkillAnalysis; }
        }
        internal ClientGmStorySystem StorySystem
        {
            get { return m_StorySystem; }
        }
        internal ClientDelayActionProcessor DelayActionQueue
        {
            get { return m_DelayActionQueue; }
        }

        internal int MyselfId
        {
            get { return m_MyselfId; }
            set { m_MyselfId = value; }
        }
        internal int ConfigId
        {
            get { return m_ConfigId; }
            set { m_ConfigId = value; }
        }
        internal int MyselfCampId
        {
            get { return m_MyselfCampId; }
            set { m_MyselfCampId = value; }
        }
        internal List<int> OtherIds
        {
            get { return m_OtherIds; }
        }
        internal List<int> OwnedNpcs
        {
            get { return m_OwnedNpcs; }
        }
        internal List<int> SkillIds
        {
            get { return m_SkillIds; }
        }

        internal long AverageRoundtripTime
        {
            get { return m_AverageRoundtripTime; }
            set { m_AverageRoundtripTime = value; }
        }
        internal long RemoteTimeOffset
        {
            get { return m_RemoteTimeOffset; }
            set { m_RemoteTimeOffset = value; }
        }
        internal long GetServerMilliseconds()
        {
            long val = TimeUtility.GetLocalMilliseconds();
            return val + m_RemoteTimeOffset;
        }

        internal void Init(IActionQueue asyncQueue)
        {
            m_LobbyNetworkSystem.Init(asyncQueue);
            m_RoomNetworkSystem.Init(this);
            m_SkillAnalysis.Init();
            m_StorySystem.Init(this);
            NetWorkMessageInit();
        }

        internal void Load(string gmTxt)
        {
            m_StorySystem.LoadStoryText(gmTxt);

            // find all waypoints data
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Robot");
            string[] wps = Directory.GetFiles(path, "*.wp", SearchOption.TopDirectoryOnly);
            if (wps == null || wps.Length == 0)
            {
                return;
            }

            // choose random file
            int num = m_Random.Next(0, wps.Length - 1);
            string p = wps[num];

            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] bytes = new byte[4];
                stream.Read(bytes, 0, 4);
                int len = BitConverter.ToInt32(bytes, 0);

                for (int i = 0; i < len; ++i)
                {
                    WayPoint w = new WayPoint();
                    stream.Read(bytes, 0, 4);
                    w.x = BitConverter.ToSingle(bytes, 0);
                    stream.Read(bytes, 0, 4);
                    w.y = BitConverter.ToSingle(bytes, 0);
                    WayPoints.Add(w);
                }
            }
        }
        internal void Start(string url, string user, string pwd)
        {
            m_WaitLogin = true;
            m_Url = url;
            m_User = user;
            m_Pass = pwd;

            m_WaitStart = false;

            m_DelayTime = TimeUtility.GetLocalMilliseconds();
        }

        internal void LogicTick()
        {
            if (WayPoints.Count == 0)
            {
                return;
            }

            long curTime = TimeUtility.GetLocalMilliseconds();

            if (!m_DelayMoveStart && (curTime - m_DelayTime < m_DelayTimeRamdom))
            {
                return;
            }
            else
            {
                m_DelayMoveStart = true;
            }

            
            if (curTime - m_LastTickLogicTime > 1000)
            {
                UpdatePosition(WayPoints[CurrentWayPointIndex].x, WayPoints[CurrentWayPointIndex].y, 0);

                CurrentWayPointIndex = (CurrentWayPointIndex + 1) % WayPoints.Count;
            }
        }

        internal void Tick()
        {
            if (m_WaitStart)
            {
                return;
            }
            if (m_WaitLogin)
            {
                if (GlobalInfo.Instance.RequestLogin())
                {
                    m_LobbyNetworkSystem.LoginLobby(m_Url, m_User, m_Pass);
                    m_WaitLogin = false;
                }
            }
            else
            {
                m_DelayActionQueue.HandleActions(100);
                m_LobbyNetworkSystem.Tick();
                m_RoomNetworkSystem.Tick();
                m_StorySystem.Tick();

                long curTime = TimeUtility.GetLocalMilliseconds();
                if (m_LastTickLogTime + 10000 < curTime)
                {
                    m_LastTickLogTime = curTime;

                    Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(m_CurScene);
                    if (null != cfg && (cfg.m_Type == (int)SceneTypeEnum.TYPE_PVP || cfg.m_Type == (int)SceneTypeEnum.TYPE_MULTI_PVE))
                    {
                        LogSystem.Info("AverageRoundtripTime:{0} robot {1} {2}", AverageRoundtripTime, LobbyNetworkSystem.User, Robot.GetDateTime());
                    }

                    if (m_LobbyNetworkSystem.IsConnected && !m_LobbyNetworkSystem.IsQueueing && m_LobbyNetworkSystem.LastConnectTime + 2000 < curTime && m_StorySystem.ActiveStoryCount <= 0)
                    {
                        LogSystem.Error("******************** robot {0} run failed, try again.{1}", LobbyNetworkSystem.User, Robot.GetDateTime());
                        m_LobbyNetworkSystem.Disconnect();
                    }
                }

                LogicTick();
            }
        }

        internal void SendLobbyMessage(JsonMessageID id, JsonData msgData)
        {
            JsonMessage msg = new JsonMessage((int)id);
            msg.m_JsonData = msgData;
            SendLobbyMessage(msg);
        }
        internal void SendLobbyMessage(JsonMessage msg)
        {
            m_LobbyNetworkSystem.SendMessage(msg);
        }
        internal void SendRoomMessage(object msg)
        {
            m_RoomNetworkSystem.SendMessage(msg);
        }

        internal void SelectScene(int id)
        {
            try
            {
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(id);
                if (null == cfg || cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE)
                {
                    JsonData singlePveMsg = new JsonData();
                    singlePveMsg.SetJsonType(JsonType.Object);
                    singlePveMsg.Set("m_Guid", LobbyNetworkSystem.Guid);
                    singlePveMsg.Set("m_SceneType", id);
                    SendLobbyMessage(JsonMessageID.SinglePVE, singlePveMsg);
                }
                else
                {
                    //todo：发多人组队请求
                    JsonMessage requestMatchMsg = new JsonMessage(JsonMessageID.RequestMatch);
                    requestMatchMsg.m_JsonData.SetJsonType(JsonType.Object);
                    requestMatchMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                    ArkCrossEngineMessage.Msg_CL_RequestMatch protoData = new ArkCrossEngineMessage.Msg_CL_RequestMatch();
                    protoData.m_SceneType = id;

                    requestMatchMsg.m_ProtoData = protoData;
                    SendLobbyMessage(requestMatchMsg);
                }
                m_CurScene = id;
                m_MyselfId = 0;
                m_MyselfCampId = (int)CampIdEnum.Blue;
                m_OtherIds.Clear();
                m_OwnedNpcs.Clear();
                LogSystem.Info("Robot {0} SelectScene {1} {2}", LobbyNetworkSystem.User, id, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void CancelMatch()
        {
            try
            {
                JsonMessage cancelMatchMsg = new JsonMessage(JsonMessageID.CancelMatch);
                cancelMatchMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_CancelMatch protoData = new ArkCrossEngineMessage.Msg_CL_CancelMatch();
                protoData.m_SceneType = m_CurScene;

                cancelMatchMsg.m_ProtoData = protoData;
                SendLobbyMessage(cancelMatchMsg);

                m_CurScene = 1010;
                LogSystem.Info("Robot {0} CancelMatch {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal void RequestUsers(int ct)
        {
            try
            {
                JsonMessage reqMsg = new JsonMessage(JsonMessageID.RequestUsers);
                reqMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_RequestUsers protoData = new ArkCrossEngineMessage.Msg_CL_RequestUsers();
                protoData.m_Count = ct;

                reqMsg.m_ProtoData = protoData;
                SendLobbyMessage(reqMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal void UpdatePosition(float x, float z, float dir)
        {
            try
            {
                JsonMessage updateMsg = new JsonMessage(JsonMessageID.UpdatePosition);
                updateMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_UpdatePosition protoData = new ArkCrossEngineMessage.Msg_CL_UpdatePosition();
                protoData.m_X = x;
                protoData.m_Z = z;
                protoData.m_FaceDir = dir;

                updateMsg.m_ProtoData = protoData;
                SendLobbyMessage(updateMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void StageClear()
        {
            try
            {
                JsonMessage stageClearMsg = new JsonMessage(JsonMessageID.StageClear);
                stageClearMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_StageClear protoData = new ArkCrossEngineMessage.Msg_CL_StageClear();
                protoData.m_MatchKey = m_MatchKey;
                protoData.m_HitCount = 8;
                protoData.m_MaxMultHitCount = 32;
                protoData.m_Hp = 1000;
                protoData.m_Mp = 1000;
                protoData.m_Gold = 0;

                stageClearMsg.m_ProtoData = protoData;
                SendLobbyMessage(stageClearMsg);

                m_CurScene = 1010;
                LogSystem.Info("Robot {0} SendMessage StageClear to lobby {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void StartGame()
        {
            try
            {
                JsonMessage startGameMsg = new JsonMessage(JsonMessageID.StartGame);
                startGameMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                SendLobbyMessage(startGameMsg);

                LogSystem.Info("Robot {0} StartGame {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void MountEquipment(int item_id, int property_id, int equipment_pos)
        {
            try
            {
                JsonMessage mountEquipMsg = new JsonMessage(JsonMessageID.MountEquipment);
                mountEquipMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_MountEquipment protoData = new ArkCrossEngineMessage.Msg_CL_MountEquipment();
                protoData.m_ItemID = item_id;
                protoData.m_PropertyID = property_id;
                protoData.m_EquipPos = equipment_pos;

                mountEquipMsg.m_ProtoData = protoData;
                SendLobbyMessage(mountEquipMsg);

                LogSystem.Info("Robot {0} MountEquipment {1} {2} {3} {4}", LobbyNetworkSystem.User, item_id, property_id, equipment_pos, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void UnmountEquipment(int equipment_pos)
        {
            try
            {
                JsonMessage unmountEquipMsg = new JsonMessage(JsonMessageID.UnmountEquipment);
                unmountEquipMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_UnmountEquipment protoData = new ArkCrossEngineMessage.Msg_CL_UnmountEquipment();
                protoData.m_EquipPos = equipment_pos;

                unmountEquipMsg.m_ProtoData = protoData;
                SendLobbyMessage(unmountEquipMsg);

                LogSystem.Info("Robot {0} UnmountEquipment {1} {2}", LobbyNetworkSystem.User, equipment_pos, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void MountSkill(int preset_index, int skill_id, SlotPosition slot_pos)
        {
            try
            {
                JsonMessage mountSkillMsg = new JsonMessage(JsonMessageID.MountSkill);
                mountSkillMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_MountSkill protoData = new ArkCrossEngineMessage.Msg_CL_MountSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_SlotPos = (int)slot_pos;

                mountSkillMsg.m_ProtoData = protoData;
                SendLobbyMessage(mountSkillMsg);

                LogSystem.Info("Robot {0} MountSkill {1} {2} {3} {4}", LobbyNetworkSystem.User, preset_index, skill_id, slot_pos, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void UnmountSkill(int preset_index, SlotPosition slot_pos)
        {
            try
            {
                JsonMessage unmountSkillMsg = new JsonMessage(JsonMessageID.UnmountSkill);
                unmountSkillMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_UnmountSkill protoData = new ArkCrossEngineMessage.Msg_CL_UnmountSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SlotPos = (int)slot_pos;

                unmountSkillMsg.m_ProtoData = protoData;
                SendLobbyMessage(unmountSkillMsg);

                LogSystem.Info("Robot {0} UnmountSkill {1} {2}", LobbyNetworkSystem.User, slot_pos, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void UpgradeSkill(int preset_index, int skill_id, bool allow_cost_gold)
        {
            try
            {
                JsonMessage upgradeSkillMsg = new JsonMessage(JsonMessageID.UpgradeSkill);
                upgradeSkillMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_UpgradeSkill protoData = new ArkCrossEngineMessage.Msg_CL_UpgradeSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_AllowCostGold = allow_cost_gold;

                upgradeSkillMsg.m_ProtoData = protoData;
                SendLobbyMessage(upgradeSkillMsg);

                LogSystem.Info("Robot {0} UpgradeSkill {1} {2} {3} {4}", LobbyNetworkSystem.User, preset_index, skill_id, allow_cost_gold, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void UnlockSkill(int preset_index, int skill_id)
        {
            try
            {
                JsonMessage unlockSkillMsg = new JsonMessage(JsonMessageID.UnlockSkill);
                unlockSkillMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_UnlockSkill protoData = new ArkCrossEngineMessage.Msg_CL_UnlockSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_UserLevel = 100;

                unlockSkillMsg.m_ProtoData = protoData;
                SendLobbyMessage(unlockSkillMsg);

                LogSystem.Info("Robot {0} UnlockSkill {1} {2} {3} {4}", LobbyNetworkSystem.User, preset_index, skill_id, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void SwapSkill(int preset_index, int skill_id, SlotPosition source_pos, SlotPosition target_pos)
        {
            try
            {
                JsonMessage swapSkillMsg = new JsonMessage(JsonMessageID.SwapSkill);
                swapSkillMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_SwapSkill protoData = new ArkCrossEngineMessage.Msg_CL_SwapSkill();
                protoData.m_PresetIndex = preset_index;
                protoData.m_SkillID = skill_id;
                protoData.m_SourcePos = (int)source_pos;
                protoData.m_TargetPos = (int)target_pos;

                swapSkillMsg.m_ProtoData = protoData;
                SendLobbyMessage(swapSkillMsg);

                LogSystem.Info("Robot {0} SwapSkill {1} {2} {3} {4} {5}", LobbyNetworkSystem.User, preset_index, skill_id, source_pos, target_pos, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void LiftSkill(int skill_id)
        {
            try
            {
                if (skill_id > 0)
                {
                    JsonMessage liftSkillMsg = new JsonMessage(JsonMessageID.LiftSkill);
                    liftSkillMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                    ArkCrossEngineMessage.Msg_CL_LiftSkill protoData = new ArkCrossEngineMessage.Msg_CL_LiftSkill();
                    protoData.m_SkillId = skill_id;

                    liftSkillMsg.m_ProtoData = protoData;
                    SendLobbyMessage(liftSkillMsg);

                    LogSystem.Info("Robot {0} LiftSkill {1} {2}", LobbyNetworkSystem.User, skill_id, Robot.GetDateTime());
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void UpgradeItem(int equipment_pos, int item_id, bool allow_cost_gold)
        {
            try
            {
                JsonMessage upgradeItemMsg = new JsonMessage(JsonMessageID.UpgradeItem);
                upgradeItemMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_UpgradeItem protoData = new ArkCrossEngineMessage.Msg_CL_UpgradeItem();
                protoData.m_Position = equipment_pos;
                protoData.m_ItemId = item_id;
                protoData.m_AllowCostGold = allow_cost_gold;

                upgradeItemMsg.m_ProtoData = protoData;
                SendLobbyMessage(upgradeItemMsg);

                LogSystem.Info("Robot {0} UpgradeItem {1} {2} {3} {4}", LobbyNetworkSystem.User, equipment_pos, item_id, allow_cost_gold, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void AddAssets(int money = 0, int gold = 0, int exp = 0, int stamina = 0)
        {
            try
            {
                if (0 == money && 0 == gold && 0 == exp && 0 == stamina)
                    return;
                JsonMessage swapSkillMsg = new JsonMessage(JsonMessageID.AddAssets);
                swapSkillMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_AddAssets protoData = new ArkCrossEngineMessage.Msg_CL_AddAssets();
                protoData.m_Money = money;
                protoData.m_Gold = gold;
                protoData.m_Exp = exp;
                protoData.m_Stamina = stamina;

                swapSkillMsg.m_ProtoData = protoData;
                SendLobbyMessage(swapSkillMsg);

                LogSystem.Info("Robot {0} AddAssets {1} {2} {3} {4} {5}", LobbyNetworkSystem.User, money, gold, exp, stamina, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void AddItem(int item_id)
        {
            try
            {
                if (item_id > 0)
                {
                    JsonData addItemMsg = new JsonData();
                    addItemMsg.Set("m_Guid", LobbyNetworkSystem.Guid);
                    addItemMsg.Set("m_ItemId", item_id);
                    addItemMsg.Set("m_ItemNum", 1);
                    SendLobbyMessage(JsonMessageID.AddItem, addItemMsg);

                    //LogSystem.Info("Robot {0} AddItem {1}", LobbyNetworkSystem.User, item_id);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ExpeditionReset(int hp, int mp, int request_num, bool allow_cost_gold)
        {
            try
            {
                JsonMessage expeditionResetMsg = new JsonMessage(JsonMessageID.ExpeditionReset);
                expeditionResetMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);

                ArkCrossEngineMessage.Msg_CL_ExpeditionReset protoData = new ArkCrossEngineMessage.Msg_CL_ExpeditionReset();
                protoData.m_AllowCostGold = allow_cost_gold;
                protoData.m_Hp = hp;
                protoData.m_Mp = mp;
                protoData.m_RequestNum = request_num;

                expeditionResetMsg.m_ProtoData = protoData;
                SendLobbyMessage(expeditionResetMsg);

                LogSystem.Info("Robot {0} ExpeditionReset {1} {2} {3} {4} {5}", LobbyNetworkSystem.User, hp, mp, request_num, allow_cost_gold, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void RequestExpedition(int scene_id, int tollgate_num)
        {
            try
            {
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(scene_id);
                if (null != cfg && cfg.m_SubType == (int)SceneSubTypeEnum.TYPE_EXPEDITION)
                {

                    JsonMessage expeditionMsg = new JsonMessage(JsonMessageID.RequestExpedition);
                    expeditionMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                    ArkCrossEngineMessage.Msg_CL_RequestExpedition protoData = new ArkCrossEngineMessage.Msg_CL_RequestExpedition();
                    protoData.m_SceneId = scene_id;
                    protoData.m_TollgateNum = tollgate_num;

                    expeditionMsg.m_ProtoData = protoData;
                    SendLobbyMessage(expeditionMsg);

                    LogSystem.Info("Robot {0} RequestExpedition {1} {2} {3}", LobbyNetworkSystem.User, scene_id, tollgate_num, Robot.GetDateTime());
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void FinishExpedition(int scene_id, int tollgate_num, int hp, int mp)
        {
            try
            {
                JsonMessage finishExpeditionMsg = new JsonMessage(JsonMessageID.FinishExpedition);
                finishExpeditionMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_FinishExpedition protoData = new ArkCrossEngineMessage.Msg_CL_FinishExpedition();
                protoData.m_SceneId = scene_id;
                protoData.m_TollgateNum = tollgate_num;
                protoData.m_Hp = hp;
                protoData.m_Mp = mp;

                finishExpeditionMsg.m_ProtoData = protoData;
                SendLobbyMessage(finishExpeditionMsg);

                LogSystem.Info("Robot {0} FinishExpedition {1} {2} {3} {4}", LobbyNetworkSystem.User, scene_id, tollgate_num, hp, mp, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void ExpeditionAward(int tollgate_num)
        {
            try
            {
                JsonMessage expeditionAwardMsg = new JsonMessage(JsonMessageID.ExpeditionAward);
                expeditionAwardMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_ExpeditionAward protoData = new ArkCrossEngineMessage.Msg_CL_ExpeditionAward();
                protoData.m_TollgateNum = tollgate_num;

                expeditionAwardMsg.m_ProtoData = protoData;
                SendLobbyMessage(expeditionAwardMsg);

                LogSystem.Info("Robot {0} ExpeditionAward {1} {2}", LobbyNetworkSystem.User, tollgate_num, Robot.GetDateTime());
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        internal void UpdateMaxUserCount(int maxUserCount, int maxUserCountPerLogicServer, int maxQueueingCount)
        {
            try
            {
                JsonMessage updateMaxUserCountMsg = new JsonMessage(JsonMessageID.GmUpdateMaxUserCount);
                updateMaxUserCountMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                ArkCrossEngineMessage.Msg_CL_GmUpdateMaxUserCount protoData = new ArkCrossEngineMessage.Msg_CL_GmUpdateMaxUserCount();
                protoData.m_MaxUserCount = maxUserCount;
                protoData.m_MaxUserCountPerLogicServer = maxUserCountPerLogicServer;
                protoData.m_MaxQueueingCount = maxQueueingCount;

                updateMaxUserCountMsg.m_ProtoData = protoData;
                SendLobbyMessage(updateMaxUserCountMsg);
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal void NotifyUserEnter()
        {
            StorySystem.SendMessage("pvpstart");
            LogSystem.Info("Robot {0} enter pvp scene {1} {2}", LobbyNetworkSystem.User, m_CurScene, Robot.GetDateTime());
        }
        internal void NotifyNpcSkill(int npcId, int skillId, float x, float z, float dir)
        {
            StorySystem.SendMessage("npcskill", npcId, skillId, x, z, dir);
            LogSystem.Info("Robot {0} does npc {1} skill {2} {3}", LobbyNetworkSystem.User, npcId, skillId, Robot.GetDateTime());
        }
        internal void NotifyMissionCompleted(int mainSceneId)
        {
            StorySystem.SendMessage("missioncompleted", mainSceneId);
            LogSystem.Info("Robot {0} pvp end (completed), goto {1} {2}", LobbyNetworkSystem.User, mainSceneId, Robot.GetDateTime());
        }
        internal void NotifyChangeScene(int mainSceneId)
        {
            StorySystem.SendMessage("changescene", mainSceneId);
            LogSystem.Info("Robot {0} pvp end (failed), goto {1} {2}", LobbyNetworkSystem.User, mainSceneId, Robot.GetDateTime());
        }
        internal int GetRandSkill()
        {
            int ct = m_SkillIds.Count;
            int ix = ArkCrossEngine.CrossEngineHelper.Random.Next(ct);
            if (ix >= 0 && ix < ct)
            {
                return m_SkillIds[ix];
            }
            else if (ct > 0)
            {
                return m_SkillIds[0];
            }
            else
            {
                return 0;
            }
        }

        private void HandleQueueingCountResult(JsonMessage msg)
        {
            JsonData jsonData = msg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_QueueingCountResult msgData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_QueueingCountResult;
            if (null != msgData)
            {
                LogSystem.Info("Robot {0} queueing {1} {2}", LobbyNetworkSystem.User, msgData.m_QueueingCount, Robot.GetDateTime());
            }
        }

        private void HandleTooManyOperations(JsonMessage msg)
        {
            LogSystem.Warn("Robot {0} received 'too many operations' {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
        }

        private void HandleAccountLoginResult(JsonMessage msg)
        {
            JsonData jsonData = msg.m_JsonData;
            int ret = jsonData.GetInt("m_Result");
            LobbyNetworkSystem.IsQueueing = false;
            if (ret == (int)AccountLoginResult.Success)
            {
                LogSystem.Info("Login success.");
                //登录成功，向服务器请求玩家角色
                JsonMessage sendMsg = new JsonMessage(JsonMessageID.RoleList);
                sendMsg.m_JsonData["m_Account"] = m_LobbyNetworkSystem.User;
                SendLobbyMessage(sendMsg);

                LogSystem.Error("Robot {0} Request Role List {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
            }
            else if (ret == (int)AccountLoginResult.FirstLogin)
            {
                //账号首次登录，需要验证激活码   
                LogSystem.Error("Robot {0} AccountLogin need activate code ! {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
            }
            else if (ret == (int)AccountLoginResult.Wait)
            {
                LogSystem.Error("Robot {0} AccountLogin wait ... {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
                Thread.Sleep(5000);
                JsonMessage loginMsg = new JsonMessage(JsonMessageID.DirectLogin);
                loginMsg.m_JsonData["m_Account"] = LobbyNetworkSystem.User;
                SendLobbyMessage(loginMsg);
                LogSystem.Error("Robot {0} AccountLogin retry . {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
            }
            else if (ret == (int)AccountLoginResult.Queueing)
            {
                LogSystem.Info("Robot {0} start queueing. {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
                LobbyNetworkSystem.IsQueueing = true;
            }
            else
            {
                //账号登录失败
                LogSystem.Error("Robot {0} AccountLogin failed ! {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
            }
        }
        private void HandleRoleListResult(JsonMessage msg)
        {
            JsonData jsonData = msg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RoleListResult msgData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RoleListResult;
            if (null != msgData)
            {
                int ret = msgData.m_Result;
                if (ret == (int)RoleListResult.Success)
                {
                    //获取玩家角色数据列表 
                    int userinfoCount = msgData.m_UserInfoCount;
                    List<ArkCrossEngineMessage.Msg_LC_RoleListResult.UserInfoForMessage> userInfos = msgData.m_UserInfos;
                    if (userInfos.Count > 0)
                    {
                        for (int i = 0; i < userInfos.Count; ++i)
                        {
                            ArkCrossEngineMessage.Msg_LC_RoleListResult.UserInfoForMessage ui = userInfos[i];
                            ulong guid = ui.m_UserGuid;

                            JsonMessage sendMsg = new JsonMessage(JsonMessageID.RoleEnter);
                            sendMsg.m_JsonData["m_Account"] = m_LobbyNetworkSystem.User;
                            ArkCrossEngineMessage.Msg_CL_RoleEnter protoData = new ArkCrossEngineMessage.Msg_CL_RoleEnter();
                            protoData.m_Guid = guid;
                            sendMsg.m_ProtoData = protoData;
                            SendLobbyMessage(sendMsg);

                            LogSystem.Info("Role Enter {0} {1} {2}", m_LobbyNetworkSystem.User, guid, Robot.GetDateTime());
                            if (i == 0)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        JsonMessage sendMsg = new JsonMessage(JsonMessageID.CreateRole);
                        sendMsg.m_JsonData["m_Account"] = m_LobbyNetworkSystem.User;
                        sendMsg.m_JsonData["m_HeroId"] = 1;// Helper.Random.Next(1, 3);
                        sendMsg.m_JsonData["m_Nickname"] = "gm007_" + m_LobbyNetworkSystem.User;
                        SendLobbyMessage(sendMsg);

                        LogSystem.Info("Create Role {0} {1}", m_LobbyNetworkSystem.User, Robot.GetDateTime());
                    }
                }
                else
                {
                    LogSystem.Info("Robot {0} Request RoleList failed {1}", m_LobbyNetworkSystem.User, Robot.GetDateTime());
                }
            }
        }
        private void HandleCreateRoleResult(JsonMessage msg)
        {
            JsonData jsonData = msg.m_JsonData;

            int ret = jsonData.GetInt("m_Result");
            if (ret == (int)CreateRoleResult.Success)
            {
                JsonData userInfo = jsonData["m_UserInfo"];
                ulong userGuid = userInfo.GetUlong("m_UserGuid");

                LogSystem.Info("Role Create {0} {1} {2}", LobbyNetworkSystem.User, userGuid, Robot.GetDateTime());
                /*
                JsonMessage sendMsg = new JsonMessage(JsonMessageID.RoleEnter);
                sendMsg.m_JsonData["m_Account"] = m_LobbyNetworkSystem.User;
                ArkCrossEngineMessage.Msg_CL_RoleEnter protoData = new ArkCrossEngineMessage.Msg_CL_RoleEnter();
                protoData.m_Guid = userGuid;
                sendMsg.m_ProtoData = protoData;
                SendLobbyMessage(sendMsg);

                LogSystem.Info("Role Enter {0} {1}", LobbyNetworkSystem.User, userGuid);
                */
            }
            else
            {
                LogSystem.Info("Role {0} HandleCreateRoleResult failed ! {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
            }
        }
        private void HandleRoleEnterResult(JsonMessage msg)
        {
            JsonData jsonData = msg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RoleEnterResult protoData = msg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RoleEnterResult;
            if (null != protoData)
            {
                int ret = protoData.m_Result;
                ulong userGuid = jsonData.GetUlong("m_Guid");
                if (ret == (int)RoleEnterResult.Wait)
                {
                    m_LobbyNetworkSystem.Guid = userGuid;

                    Thread.Sleep(2000);

                    JsonMessage sendMsg = new JsonMessage(JsonMessageID.RoleEnter);
                    sendMsg.m_JsonData["m_Account"] = LobbyNetworkSystem.User;
                    ArkCrossEngineMessage.Msg_CL_RoleEnter sendProtoData = new ArkCrossEngineMessage.Msg_CL_RoleEnter();
                    sendProtoData.m_Guid = userGuid;

                    sendMsg.m_ProtoData = sendProtoData;
                    SendLobbyMessage(sendMsg);

                    LogSystem.Debug("Retry RoleEnter {0} {1} {2}", LobbyNetworkSystem.User, userGuid, Robot.GetDateTime());
                    return;
                }
                else if (ret == (int)RoleEnterResult.Success)
                {
                    m_LobbyNetworkSystem.Guid = userGuid;
                    if (!m_LobbyNetworkSystem.HasLoggedOn)
                    {
                        GlobalInfo.Instance.FinishLogin();
                        m_StorySystem.StartStory(1, this);
                    }
                    m_LobbyNetworkSystem.IsLogining = false;
                    m_LobbyNetworkSystem.HasLoggedOn = true;

                    LogSystem.Info("Robot {0} is logged. {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
                }
                else
                {
                    LogSystem.Info("Robot {0} HandleRoleEnterResult failed ! {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
                }
            }
        }
        private void HandleMatchResult(JsonMessage lobbyMsg)
        {
            ArkCrossEngineMessage.Msg_LC_MatchResult matchResult = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MatchResult;
            if (null != matchResult)
            {
                if (matchResult.m_Result == 0)
                {
                    StorySystem.SendMessage("matchsuccess");
                    LogSystem.Info("Robot {0} match success. {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
                    StartGame();
                }
                else
                {
                    LogSystem.Info("Robot {0} match failed {1}. {2}", LobbyNetworkSystem.User, matchResult.m_Result, Robot.GetDateTime());
                }
            }
        }
        private void HandleStartGameResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_StartGameResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_StartGameResult;
            if (null != protoData)
            {
                GeneralOperationResult result = (GeneralOperationResult)protoData.result;
                if (GeneralOperationResult.LC_Succeed == result)
                {
                    uint key = protoData.key;
                    string ip = protoData.server_ip;
                    int port = (int)protoData.server_port;
                    int heroId = protoData.hero_id;
                    int campId = protoData.camp_id;
                    int sceneId = protoData.scene_type;
                    m_MatchKey = protoData.match_key;

                    m_CurScene = sceneId;

                    Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
                    if (null == cfg || cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE)
                    {
                        StorySystem.SendMessage("pvestart", sceneId);

                        LogSystem.Info("Robot {0} enter pve scene {1}. {2}", LobbyNetworkSystem.User, sceneId, Robot.GetDateTime());
                    }
                    else if (cfg.m_Type == (int)SceneTypeEnum.TYPE_PVP || cfg.m_Type == (int)SceneTypeEnum.TYPE_MULTI_PVE)
                    {
                        StorySystem.SendMessage("pvptrystart");
                        RoomNetworkSystem.Start(key, ip, port);

                        LogSystem.Info("Robot {0} try to connect room {1}. {2}", LobbyNetworkSystem.User, sceneId, Robot.GetDateTime());
                    }
                    else
                    {
                        LogSystem.Info("Robot {0} try to enter unexpected scene {1} {2}", LobbyNetworkSystem.User, sceneId, Robot.GetDateTime());
                    }
                }
                else
                {
                    LogSystem.Info("Robot {0} StartGameResult failed. {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
                }
            }
        }
        private void HandleMountEquipmentResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_MountEquipmentResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MountEquipmentResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int item_id = protoMsg.m_ItemID;
                int item_property = protoMsg.m_PropertyID;
                int equipment_pos = protoMsg.m_EquipPos;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleMountEquipmentResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleUnmountEquipmentResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UnmountEquipmentResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UnmountEquipmentResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int equipment_pos = protoMsg.m_EquipPos;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleUnmountEquipmentResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleMountSkillResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_MountSkillResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_MountSkillResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int preset_index = protoMsg.m_PresetIndex;
                int skill_id = protoMsg.m_SkillID;
                SlotPosition slot_position = (SlotPosition)protoMsg.m_SlotPos;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleMountSkillResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleUnmountSkillResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UnmountSkillResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UnmountSkillResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int preset_index = protoMsg.m_PresetIndex;
                SlotPosition slot_position = (SlotPosition)protoMsg.m_SlotPos;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleUnmountSkillResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleUpgradeSkillResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UpgradeSkillResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UpgradeSkillResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int preset_index = protoMsg.m_PresetIndex;
                int skill_id = protoMsg.m_SkillID;
                bool allow_cost_gold = protoMsg.m_AllowCostGold;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleUpgradeSkillResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleUnlockSkillResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UnlockSkillResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UnlockSkillResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int preset_index = protoMsg.m_PresetIndex;
                int skill_id = protoMsg.m_SkillID;
                int user_level = protoMsg.m_UserLevel;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleUnlockSkillResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleSwapSkillResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SwapSkillResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SwapSkillResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int preset_index = protoMsg.m_PresetIndex;
                int skill_id = protoMsg.m_SkillID;
                SlotPosition source_pos = (SlotPosition)protoMsg.m_SourcePos;
                SlotPosition target_pos = (SlotPosition)protoMsg.m_TargetPos;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleSwapSkillResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleLiftSkillResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_LiftSkillResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_LiftSkillResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int skill_id = protoMsg.m_SkillID;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleLiftSkillResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleUpgradeItemResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_UpgradeItemResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UpgradeItemResult;
            if (null != protoMsg)
            {
                ulong guid = jsonData.GetUlong("m_Guid");
                int equip_position = protoMsg.m_Position;
                GeneralOperationResult result = (GeneralOperationResult)protoMsg.m_Result;

                LogSystem.Info("Robot {0} HandleUpgradeItemResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleUserLevelup(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            ArkCrossEngineMessage.Msg_LC_UserLevelup protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_UserLevelup;
            if (null != protoData)
            {
                int user_id = protoData.m_UserId;
                int user_level = protoData.m_UserLevel;

                LogSystem.Info("Robot {0} level {1} {2}", LobbyNetworkSystem.User, user_level, Robot.GetDateTime());
            }
        }
        private void HandleSyncStamina(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            ArkCrossEngineMessage.Msg_LC_SyncStamina protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncStamina;
            if (null != protoData)
            {
                int stamina = protoData.m_Stamina;

                LogSystem.Info("Robot {0} stamina {1} {2}", LobbyNetworkSystem.User, stamina, Robot.GetDateTime());
            }
        }
        private void HandleSyncMpveBattleResult(JsonMessage lobbyMsg)
        {
            //m_Result: 0--win 1--lost 2--unfinish
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncMpveBattleResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncMpveBattleResult;
            if (null != protoMsg)
            {
                if (protoMsg.m_Result == 0)
                {//多人pve只处理成功情形，奖励在StagClearResult里处理。失败处理不走大厅流程。
                    JsonMessage stageClearMsg = new JsonMessage(JsonMessageID.StageClear);
                    stageClearMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Guid);
                    ArkCrossEngineMessage.Msg_CL_StageClear protoData = new ArkCrossEngineMessage.Msg_CL_StageClear();
                    protoData.m_HitCount = 1;
                    protoData.m_MaxMultHitCount = 100;
                    protoData.m_Hp = 10000;
                    protoData.m_Mp = 1000;

                    stageClearMsg.m_ProtoData = protoData;
                    SendLobbyMessage(stageClearMsg);

                    LogSystem.Info("Robot {0} SendMessage StageClear to lobby {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
                }
            }
        }
        private void HandleSyncGowBattleResult(JsonMessage lobbyMsg)
        {
            //m_Result: 0--win 1--lost 2--unfinish
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_SyncGowBattleResult protoMsg = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_SyncGowBattleResult;
            if (null != protoMsg)
            {
                int result = protoMsg.m_Result;
                int oldelo = protoMsg.m_OldGowElo;
                int elo = protoMsg.m_GowElo;
                int enemyoldelo = protoMsg.m_EnemyOldGowElo;
                int enemyelo = protoMsg.m_EnemyGowElo;

                LogSystem.Info("Robot {0} Gow result:{1}, my elo:{2}->{3} other elo:{4}->{5} {6}", LobbyNetworkSystem.User, result, oldelo, elo, enemyoldelo, enemyelo, Robot.GetDateTime());
            }
        }
        private void HandleStageClearResult(JsonMessage lobbyMsg)
        {
            if (null == lobbyMsg) return;
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_StageClearResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_StageClearResult;
            if (null != protoData)
            {
                int sceneId = 1;
                ulong userGuid = jsonData.GetUlong("m_Guid");
                int hitCount = protoData.m_HitCount;
                int maxMultHitCount = protoData.m_MaxMultHitCount;
                long duration = protoData.m_Duration;
                int itemId = protoData.m_ItemId;
                int itemCount = protoData.m_ItemCount;
                int expPoint = protoData.m_ExpPoint;
                int hp = protoData.m_Hp;
                int mp = protoData.m_Mp;
                int gold = protoData.m_Gold;
                int deadCount = protoData.m_DeadCount;
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);

            }
            LogSystem.Info("Robot {0} HandleStageClearResult. {1}", LobbyNetworkSystem.User, Robot.GetDateTime());
        }
        private void HandleAddAssetsResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            ArkCrossEngineMessage.Msg_LC_AddAssetsResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_AddAssetsResult;
            if (null != protoData)
            {
                int money = protoData.m_Money;
                int gold = protoData.m_Gold;
                int exp = protoData.m_Exp;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;

                LogSystem.Info("Robot {0} HandleAddAssetsResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleAddItemResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            ArkCrossEngineMessage.Msg_LC_AddItemResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_AddItemResult;
            if (null != protoData)
            {
                int item_id = protoData.m_ItemId;
                int item_random_property = protoData.m_RandomProperty;
                int item_count = protoData.m_ItemCount;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;

                //LogSystem.Info("Robot {0} HandleAddItemResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleExpeditionResetResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult;
            if (null != protoData)
            {
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;

                LogSystem.Info("Robot {0} HandleExpeditionResetResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleRequestExpeditionResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_RequestExpeditionResult msgData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_RequestExpeditionResult;
            if (null != msgData)
            {
                int result = msgData.m_Result;

                LogSystem.Info("Robot {0} HandleRequestExpeditionResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleFinishExpeditionResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ArkCrossEngineMessage.Msg_LC_FinishExpeditionResult msgData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_FinishExpeditionResult;
            if (null != msgData)
            {
                int result = msgData.m_Result;

                LogSystem.Info("Robot {0} HandleFinishExpeditionResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void HandleExpeditionAwardResult(JsonMessage lobbyMsg)
        {
            JsonData jsonData = lobbyMsg.m_JsonData;
            ulong guid = jsonData.GetUlong("m_Guid");
            ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult protoData = lobbyMsg.m_ProtoData as ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult;
            if (null != protoData)
            {
                int tollgate_num = protoData.m_TollgateNum;
                int add_money = protoData.m_AddMoney;
                GeneralOperationResult result = (GeneralOperationResult)protoData.m_Result;

                LogSystem.Info("Robot {0} HandleExpeditionAwardResult {1}. {2}", LobbyNetworkSystem.User, result, Robot.GetDateTime());
            }
        }
        private void NetWorkMessageInit()
        {
            RegisterMsgHandler(JsonMessageID.QueueingCountResult, typeof(ArkCrossEngineMessage.Msg_LC_QueueingCountResult), HandleQueueingCountResult);
            RegisterMsgHandler(JsonMessageID.TooManyOperations, HandleTooManyOperations);
            RegisterMsgHandler(JsonMessageID.AccountLoginResult, HandleAccountLoginResult);
            RegisterMsgHandler(JsonMessageID.RoleListResult, typeof(ArkCrossEngineMessage.Msg_LC_RoleListResult), HandleRoleListResult);
            RegisterMsgHandler(JsonMessageID.CreateRoleResult, HandleCreateRoleResult);
            RegisterMsgHandler(JsonMessageID.RoleEnterResult, typeof(ArkCrossEngineMessage.Msg_LC_RoleEnterResult), HandleRoleEnterResult);

            RegisterMsgHandler(JsonMessageID.MatchResult, typeof(ArkCrossEngineMessage.Msg_LC_MatchResult), HandleMatchResult);
            RegisterMsgHandler(JsonMessageID.StartGameResult, typeof(ArkCrossEngineMessage.Msg_LC_StartGameResult), HandleStartGameResult);

            RegisterMsgHandler(JsonMessageID.MountEquipmentResult, typeof(ArkCrossEngineMessage.Msg_LC_MountEquipmentResult), HandleMountEquipmentResult);
            RegisterMsgHandler(JsonMessageID.UnmountEquipmentResult, typeof(ArkCrossEngineMessage.Msg_LC_UnmountEquipmentResult), HandleUnmountEquipmentResult);
            RegisterMsgHandler(JsonMessageID.MountSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_MountSkillResult), HandleMountSkillResult);
            RegisterMsgHandler(JsonMessageID.UnmountSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_UnmountSkillResult), HandleUnmountSkillResult);
            RegisterMsgHandler(JsonMessageID.UpgradeSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_UpgradeSkillResult), HandleUpgradeSkillResult);
            RegisterMsgHandler(JsonMessageID.UnlockSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_UnlockSkillResult), HandleUnlockSkillResult);
            RegisterMsgHandler(JsonMessageID.SwapSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_SwapSkillResult), HandleSwapSkillResult);
            RegisterMsgHandler(JsonMessageID.UpgradeItemResult, typeof(ArkCrossEngineMessage.Msg_LC_UpgradeItemResult), HandleUpgradeItemResult);
            RegisterMsgHandler(JsonMessageID.UserLevelup, typeof(ArkCrossEngineMessage.Msg_LC_UserLevelup), HandleUserLevelup);
            RegisterMsgHandler(JsonMessageID.SyncStamina, typeof(ArkCrossEngineMessage.Msg_LC_SyncStamina), HandleSyncStamina);
            RegisterMsgHandler(JsonMessageID.StageClearResult, typeof(ArkCrossEngineMessage.Msg_LC_StageClearResult), HandleStageClearResult);
            RegisterMsgHandler(JsonMessageID.AddAssetsResult, typeof(ArkCrossEngineMessage.Msg_LC_AddAssetsResult), HandleAddAssetsResult);
            RegisterMsgHandler(JsonMessageID.AddItemResult, typeof(ArkCrossEngineMessage.Msg_LC_AddItemResult), HandleAddItemResult);
            RegisterMsgHandler(JsonMessageID.LiftSkillResult, typeof(ArkCrossEngineMessage.Msg_LC_LiftSkillResult), HandleLiftSkillResult);
            RegisterMsgHandler(JsonMessageID.ExpeditionResetResult, typeof(ArkCrossEngineMessage.Msg_LC_ExpeditionResetResult), HandleExpeditionResetResult);
            RegisterMsgHandler(JsonMessageID.RequestExpeditionResult, typeof(ArkCrossEngineMessage.Msg_LC_RequestExpeditionResult), HandleRequestExpeditionResult);
            RegisterMsgHandler(JsonMessageID.FinishExpeditionResult, typeof(ArkCrossEngineMessage.Msg_LC_FinishExpeditionResult), HandleFinishExpeditionResult);
            RegisterMsgHandler(JsonMessageID.ExpeditionAwardResult, typeof(ArkCrossEngineMessage.Msg_LC_ExpeditionAwardResult), HandleExpeditionAwardResult);
            RegisterMsgHandler(JsonMessageID.SyncMpveBattleResult, typeof(ArkCrossEngineMessage.Msg_LC_SyncMpveBattleResult), HandleSyncMpveBattleResult);
            RegisterMsgHandler(JsonMessageID.SyncGowBattleResult, typeof(ArkCrossEngineMessage.Msg_LC_SyncGowBattleResult), HandleSyncGowBattleResult);
        }

        private void RegisterMsgHandler(JsonMessageID id, JsonMessageHandlerDelegate handler)
        {
            m_LobbyNetworkSystem.RegisterMsgHandler(id, null, handler);
        }

        private void RegisterMsgHandler(JsonMessageID id, Type protoType, JsonMessageHandlerDelegate handler)
        {
            m_LobbyNetworkSystem.RegisterMsgHandler(id, protoType, handler);
        }

        private bool m_WaitStart = true;
        private bool m_WaitLogin = false;
        private string m_Url = "";
        private string m_User = "";
        private string m_Pass = "";

        private int m_CurScene = 1010;
        private int m_MyselfId = 0;
        private int m_ConfigId = 0;
        private int m_MyselfCampId = (int)CampIdEnum.Blue;
        private int m_MatchKey = 0;

        private List<int> m_OtherIds = new List<int>();
        private List<int> m_OwnedNpcs = new List<int>();
        private List<int> m_SkillIds = new List<int>();

        private const long c_TickLogInterval = 10000;
        private long m_LastTickLogTime = 0;
        private long m_LastTickLogicTime = 0;
        private long m_DelayTimeRamdom = m_Random.Next(0, 5000);
        private bool m_DelayMoveStart = false;
        private long m_DelayTime = 0;

        private long m_AverageRoundtripTime = 0;
        private long m_RemoteTimeOffset = 0;

        private ClientDelayActionProcessor m_DelayActionQueue = new ClientDelayActionProcessor();

        private LobbyNetworkSystem m_LobbyNetworkSystem = new LobbyNetworkSystem();
        private NetworkSystem m_RoomNetworkSystem = new NetworkSystem();
        private SkillAnalysis m_SkillAnalysis = new SkillAnalysis();
        private ClientGmStorySystem m_StorySystem = new ClientGmStorySystem();

        private static Random m_Random = new Random();

        [System.Serializable]
        public struct WayPoint
        {
            public float x, y;
        }
        private List<WayPoint> WayPoints = new List<WayPoint>();
        private int CurrentWayPointIndex = 0;

        internal static CharacterRelation GetRelation(int campA, int campB)
        {
            CharacterRelation relation = CharacterRelation.RELATION_INVALID;
            if ((int)CampIdEnum.Unkown != campA && (int)CampIdEnum.Unkown != campB)
            {
                if (campA == campB)
                    relation = CharacterRelation.RELATION_FRIEND;
                else if (campA == (int)CampIdEnum.Friendly || campB == (int)CampIdEnum.Friendly)
                    relation = CharacterRelation.RELATION_FRIEND;
                else if (campA == (int)CampIdEnum.Hostile || campB == (int)CampIdEnum.Hostile)
                    relation = CharacterRelation.RELATION_ENEMY;
                else
                    relation = CharacterRelation.RELATION_ENEMY;
            }
            return relation;
        }
        internal static string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
        }
    }
}
