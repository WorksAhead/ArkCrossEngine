using System;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using RoomServer;
using ArkCrossEngineMessage;
using ArkCrossEngine;

namespace DashFire
{
    internal enum UserControlState : int
    {
        User = 0,
        UserDropped,
        Ai,
        Remove,
    }
    internal class User
    {
        internal User()
        {
            peer_ = new RoomPeer();
            dispatcher_ = new Dispatcher();
            IsEntered = false;
            HasSyncInfo = false;
            IsDebug = false;
            m_UserControlState = (int)DashFire.UserControlState.User;
        }

        internal void Reset()
        {
            peer_.Reset();
            OwnRoom = null;
            user_guid_ = 0;
            user_name_ = "";
            IsEntered = false;
            HasSyncInfo = false;
            HeroId = 0;
            IsDebug = false;
            m_UserControlState = (int)DashFire.UserControlState.User;

            m_LastIsMoving = false;
            m_LastSampleTime = 0;
            m_LastClientPosition = Vector3.Zero;
            m_LastMoveVelocity = 0;
            m_LastMoveDirCosAngle = 1;
            m_LastMoveDirSinAngle = 0;
        }

        internal void Init()
        {
            dispatcher_.SetClientDefaultHandler(DefaultMsgHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_Create), EnterHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_MoveStart), MoveHandler.OnMoveStart);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_MoveStop), MoveHandler.OnMoveStop);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_Face), FaceDirHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_MoveMeetObstacle), MoveMeetObstacleHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_Skill), UseSkillHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_StopSkill), StopSkillHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_NpcStopSkill), NpcStopSkillHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_SendImpactToEntity), SendImpactToEntityHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_StopGfxImpact), StopGfxImpactHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_GfxControlMoveStart), Msg_CRC_GfxControlMoveStartHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_GfxControlMoveStop), Msg_CRC_GfxControlMoveStopHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_SwitchDebug), SwitchDebugHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_InteractObject), Msg_CRC_InteractObjectHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_Quit), Msg_CR_QuitHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_UserMoveToPos), Msg_CR_UserMoveToPosHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_UserMoveToAttack), Msg_CR_UserMoveToAttackHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_DlgClosed), Msg_CR_DlgClosedHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_GiveUpBattle), Msg_CR_GiveUpBattleHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_DeleteDeadNpc), Msg_CR_DeleteDeadNpcHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_HitCountChanged), Msg_CR_HitCountChangedHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_SyncCharacterGfxState), Msg_CR_SyncCharacterGfxStateHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_SummonPartner), Msg_CR_SummonPartnerHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CRC_SummonNpc), Msg_CRC_SummonNpcHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_GmCommand), Msg_CR_GmCommandHandler.Execute);
            dispatcher_.RegClientMsgHandler(typeof(Msg_CR_PickUpNpc), Msg_CR_PickUpNpcHandler.Execute);
        }

        internal void RegisterObservers(IList<Observer> observers)
        {
            peer_.RegisterObservers(observers);
        }

        internal bool SetKey(uint key)
        {
            return peer_.SetKey(key);
        }
        internal uint GetKey()
        {
            return peer_.GetKey();
        }

        internal bool ReplaceDroppedUser(ulong replacer, uint key)
        {
            Guid = replacer;
            return peer_.UpdateKey(key);
        }

        internal RoomPeer GetPeer()
        {
            return peer_;
        }

        internal bool IsConnected()
        {
            return peer_.IsConnected();
        }

        internal bool IsTimeout()
        {
            return peer_.IsTimeout();
        }

        internal void Disconnect()
        {
            peer_.Disconnect();
        }

        internal long GetElapsedDroppedTime()
        {
            return peer_.GetElapsedDroppedTime();
        }

        internal void SendMessage(object msg)
        {
            peer_.SendMessage(msg);
        }

        internal void BroadCastMsgToCareList(object msg, bool exclude_me = true)
        {
            peer_.BroadCastMsgToCareList(msg, exclude_me);
        }

        internal void BroadCastMsgToRoom(object msg, bool exclude_me = true)
        {
            peer_.BroadCastMsgToRoom(msg, exclude_me);
        }

        internal void AddSameRoomUser(User user)
        {
            peer_.AddSameRoomPeer(user.GetPeer());
        }

        internal void RemoveSameRoomUser(User user)
        {
            peer_.RemoveSameRoomPeer(user.GetPeer());
        }

        internal void ClearSameRoomUser()
        {
            peer_.ClearSameRoomPeer();
        }

        internal void AddCareMeUser(User user)
        {
            peer_.AddCareMePeer(user.GetPeer());
        }

        internal void RemoveCareMeUser(User user)
        {
            peer_.RemoveCareMePeer(user.GetPeer());
        }

        internal void AddICareUser(User user)
        {
            user.AddCareMeUser(this);
        }

        internal void RemoveICareUser(User user)
        {
            user.RemoveCareMeUser(this);
        }

        internal void Tick()
        {
            try
            {
                object msg = null;
                while ((msg = peer_.PeekLogicMsg()) != null)
                {
                    dispatcher_.HandleClientMsg(msg, this);
                }
            }
            catch (Exception ex)
            {
                LogSys.Log(LOG_TYPE.ERROR, "Exception {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal bool LastIsMoving
        {
            get { return m_LastIsMoving; }
            set { m_LastIsMoving = value; }
        }

        internal Vector3 LastClientPosition
        {
            get { return m_LastClientPosition; }
        }

        internal void SampleMoveData(float x, float z, float velocity, float cosDir, float sinDir, long time)
        {
            m_LastClientPosition.X = x;
            m_LastClientPosition.Y = 0;
            m_LastClientPosition.Z = z;
            m_LastMoveVelocity = velocity;
            m_LastMoveDirCosAngle = cosDir;
            m_LastMoveDirSinAngle = sinDir;
            m_LastSampleTime = time;

            //LogSys.Log(LOG_TYPE.WARN, "SampleMoveData user:{0} x:{1} z:{2} v:{3} cos:{4} sin:{5} time:{6}", RoleId, x, z, velocity, cosDir, sinDir, time);
        }

        internal bool VerifyMovingPosition(float x, float z, float velocity, long time)
        {
            bool ret = true;
            if (m_LastSampleTime > 0)
            {
                Vector3 pos = new Vector3(x, 0, z);
                float distSqr = Geometry.DistanceSquare(pos, m_LastClientPosition);
                float v = Geometry.Max(velocity, m_LastMoveVelocity);
                float t = (time - m_LastSampleTime) / 1000.0f;
                float enableDist = v * t;
                float enableDistSqr = enableDist * enableDist;
                if (distSqr > 1 && (distSqr > enableDistSqr * 2 + 1/* || distSqr < enableDistSqr / 2 - 1*/))
                {
                    ret = false;
                    float sx = m_LastClientPosition.X + enableDist * m_LastMoveDirSinAngle;
                    float sz = m_LastClientPosition.Z + enableDist * m_LastMoveDirCosAngle;

                    LogSys.Log(LOG_TYPE.ERROR, "VerifyMoveData user:{0} t:{1} v:{2} x:{3} z:{4} sx:{5} sz:{6} distSqr:{7} enableDistSqr:{8}", RoleId, t, v, x, z, sx, sz, distSqr, enableDistSqr);
                }
            }
            return ret;
        }

        internal bool VerifyNotMovingPosition(float x, float z, float maxEnabledDistSqr)
        {
            bool ret = true;
            if (m_LastSampleTime > 0)
            {
                Vector3 pos = new Vector3(x, 0, z);
                float distSqr = Geometry.DistanceSquare(pos, m_LastClientPosition);
                if (distSqr > maxEnabledDistSqr)
                {
                    ret = false;

                    LogSys.Log(LOG_TYPE.ERROR, "VerifyNoMoveData user:{0} x:{1} z:{2} sx:{3} sz:{4}", RoleId, x, z, m_LastClientPosition.X, m_LastClientPosition.Z);
                }
            }
            return ret;
        }

        internal bool VerifyPosition(float x, float z, float velocity, long time, float maxEnabledNomovingDistSqr)
        {
            bool ret = true;
            float ox = x;
            float oz = z;
            if (LastIsMoving)
            {
                if (!VerifyMovingPosition(x, z, velocity, time))
                {
                    ret = false;
                }
            }
            else
            {
                if (!VerifyNotMovingPosition(x, z, maxEnabledNomovingDistSqr))
                {
                    ret = false;
                }
            }
            return ret;
        }

        internal int Level { set; get; }
        internal int ArgFightingScore { set; get; }
        internal uint LocalID { set; get; }
        internal bool IsIdle { set; get; }
        internal Room OwnRoom { set; get; }
        internal string Name
        {
            set
            {
                user_name_ = value;
            }
            get
            {
                return user_name_;
            }
        }
        internal ulong Guid
        {
            set
            {
                user_guid_ = value;
                peer_.Guid = user_guid_;
            }
            get
            {
                return user_guid_;
            }
        }
        internal int RoleId
        {
            get
            {
                if (null != m_Info)
                {
                    return m_Info.GetId();
                }
                else
                {
                    return 0;
                }
            }
        }
        internal long EnterRoomTime
        {
            get
            {
                return peer_.EnterRoomTime;
            }
            set
            {
                peer_.EnterRoomTime = value;
            }
        }

        internal int UserControlState
        {
            get { return m_UserControlState; }
            set { m_UserControlState = value; }
        }
        internal bool IsEntered { set; get; }
        internal bool HasSyncInfo { set; get; }
        internal int HeroId { set; get; }
        internal int CampId { set; get; }
        internal bool IsDebug { set; get; }
        internal long InitialPosX
        {
            get { return m_InitialPositionX; }
            set { m_InitialPositionX = value; }
        }
        internal long InitialPosY
        {
            get { return m_InitialPositionY; }
            set { m_InitialPositionY = value; }
        }

        internal UserInfo Info
        {
            get { return m_Info; }
            set
            {
                m_Info = value;
                m_Info.CustomData = this;
                if (null != m_Info)
                {
                    peer_.RoleId = m_Info.GetId();
                }
            }
        }
        internal List<SkillTransmitArg> Skill
        {
            get { return m_Skills; }
            set { m_Skills = value; }
        }
        internal List<ItemTransmitArg> Equip
        {
            get { return m_Equips; }
            set { m_Equips = value; }
        }
        internal List<ItemTransmitArg> Legacy
        {
            get { return m_Legacys; }
            set { m_Legacys = value; }
        }
        internal XSoulInfo<XSoulPartInfo> XSouls
        {
            get { return m_XSouls; }
        }
        internal PartnerInfo Partner
        {
            get { return m_PartnerInfo; }
            set { m_PartnerInfo = value; }
        }
        internal int PresetIndex
        {
            get { return m_PresetIndex; }
            set { m_PresetIndex = value; }
        }
        internal List<int> ShopEquipmentsId
        {
            get { return m_ShopEquipmentsId; }
            set { m_ShopEquipmentsId = value; }
        }

        private List<int> m_ShopEquipmentsId = new List<int>();
        private List<SkillTransmitArg> m_Skills = new List<SkillTransmitArg>();
        private List<ItemTransmitArg> m_Equips = new List<ItemTransmitArg>();
        private List<ItemTransmitArg> m_Legacys = new List<ItemTransmitArg>();
        private XSoulInfo<XSoulPartInfo> m_XSouls = new XSoulInfo<XSoulPartInfo>();
        private PartnerInfo m_PartnerInfo;

        private int m_PresetIndex = 0;
        private RoomPeer peer_;
        private Dispatcher dispatcher_;

        private string user_name_;
        private ulong user_guid_;

        private UserInfo m_Info;
        private int m_UserControlState;

        // 移动校验数据
        private Vector3 m_LastClientPosition = Vector3.Zero;
        private float m_LastMoveVelocity = 0;
        private float m_LastMoveDirCosAngle = 0;
        private float m_LastMoveDirSinAngle = 0;
        private long m_LastSampleTime = 0;
        private bool m_LastIsMoving = false;

        // 同步初始位置
        private long m_InitialPositionX = 0;
        private long m_InitialPositionY = 0;
    }
} // namespace 
