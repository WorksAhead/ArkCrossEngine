using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Google.ProtocolBuffers;
using ArkCrossEngine;

using DashFire;

namespace RoomServer
{
    internal class RoomPeer
    {
        private IList<Observer> m_Observers = null;
        private List<RoomPeer> m_SameRoomPeerList = new List<RoomPeer>();
        private List<RoomPeer> m_CareList = new List<RoomPeer>();
        private NetConnection m_Connection;
        private object m_LockObj = new object();
        private ConcurrentQueue<object> m_LogicQueue = new ConcurrentQueue<object>();
        private uint m_Key = 0;
        private RoomPeerMgr m_PeerMgr = RoomPeerMgr.Instance;
        private long m_LastPingTime;
        private long m_EnterRoomTime;        // 进入房间的时间
        private const int m_ConnectionOverTime = 15000;
        private const int m_FirstEnterWaitTime = 20000;    //第一次接入等待时间，不计算超时

        internal void RegisterObservers(IList<Observer> observers)
        {
            m_Observers = observers;
        }

        internal uint GetKey()
        {
            return m_Key;
        }

        internal ulong Guid { set; get; }

        internal int RoleId
        {
            set;
            get;
        }

        internal Vector3 Position
        {
            get;
            set;
        }

        internal float FaceDir
        {
            get;
            set;
        }

        //这里没有加锁但是会在多个线程操作（写的时间是错开的）
        internal long EnterRoomTime
        {
            get { return m_EnterRoomTime; }
            set { m_EnterRoomTime = value; }
        }

        internal bool IsTimeout()
        {
            long current_time = TimeUtility.GetServerMilliseconds();
            if (current_time <= m_EnterRoomTime + m_FirstEnterWaitTime)
            {
                return false;
            }
            if (current_time - m_LastPingTime >= m_ConnectionOverTime)
            {
                return true;
            }
            return false;
        }

        internal long GetElapsedDroppedTime()
        {
            long time = 0;
            if (IsTimeout())
            {
                long current_time = TimeUtility.GetServerMilliseconds();
                time = current_time - m_LastPingTime - m_ConnectionOverTime;
            }
            return time;
        }

        internal bool IsConnected()
        {
            bool ret = false;
            if (null != m_Connection)
                ret = (NetConnectionStatus.Connected == m_Connection.Status);
            return ret;
        }

        internal void Disconnect()
        {
            if (null != m_Connection && NetConnectionStatus.Connected == m_Connection.Status)
            {
                m_Connection.Disconnect("disconnect");
                SetLastPingTime(TimeUtility.GetServerMilliseconds() - m_ConnectionOverTime);
            }
        }

        internal void SetLastPingTime(long pingtime)
        {
            m_LastPingTime = pingtime;
        }

        internal void Init(NetConnection conn)
        {
            m_EnterRoomTime = TimeUtility.GetServerMilliseconds();
            m_Connection = conn;
        }

        internal void Reset()
        {
            Disconnect();
            // this call must before other operation
            m_PeerMgr.OnPeerDestroy(this);
            m_Observers = null;
            m_LastPingTime = 0;
            m_EnterRoomTime = 0;
            m_Connection = null;
            m_Key = 0;
            m_SameRoomPeerList.Clear();
            m_CareList.Clear();
            ClearLogicQueue();
        }

        internal NetConnection GetConnection()
        {
            return m_Connection;
        }

        internal void SendMessage(object msg)
        {
            IOManager.Instance.SendPeerMessage(this, msg);
        }

        internal void BroadCastMsgToCareList(object msg, bool exclude_me = true)
        {
            lock (m_LockObj)
            {
                if (!exclude_me)
                    SendMessage(msg);

                foreach (RoomPeer peer in m_CareList)
                {
                    if (peer.GetConnection() != null)
                    {
                        peer.SendMessage(msg);
                    }
                }
            }
            NotifyObservers(msg);
        }

        internal void BroadCastMsgToRoom(object msg, bool exclude_me = true)
        {
            lock (m_LockObj)
            {
                if (!exclude_me)
                    SendMessage(msg);

                foreach (RoomPeer peer in m_SameRoomPeerList)
                {
                    if (peer.GetConnection() != null)
                    {
                        peer.SendMessage(msg);
                    }
                }
            }
            NotifyObservers(msg);
        }

        internal void NotifyObservers(object msg)
        {
            if (null != m_Observers)
            {
                IList<Observer> observers = m_Observers;
                for (int i = 0; i < observers.Count; ++i)
                {
                    Observer observer = observers[i];
                    if (null != observer && !observer.IsIdle)
                    {
                        observer.SendMessage(msg);
                    }
                }
            }
        }

        internal bool SetKey(uint key)
        {
            if (m_PeerMgr.OnSetKey(key, this))
            {
                m_Key = key;
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool UpdateKey(uint key)
        {
            if (m_PeerMgr.OnUpdateKey(key, this))
            {
                m_Key = key;
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void AddSameRoomPeer(RoomPeer peer)
        {
            lock (m_LockObj)
            {
                m_SameRoomPeerList.Add(peer);
            }
        }

        internal void RemoveSameRoomPeer(RoomPeer peer)
        {
            lock (m_LockObj)
            {
                m_SameRoomPeerList.Remove(peer);
            }
        }

        internal void ClearSameRoomPeer()
        {
            lock (m_LockObj)
            {
                m_SameRoomPeerList.Clear();
            }
        }

        internal void AddCareMePeer(RoomPeer peer)
        {
            lock (m_LockObj)
            {
                m_CareList.Add(peer);
            }
        }

        internal void RemoveCareMePeer(RoomPeer peer)
        {
            lock (m_LockObj)
            {
                m_CareList.Remove(peer);
            }
        }

        internal void InsertLogicMsg(object msg)
        {
            m_LogicQueue.Enqueue(msg);
        }

        internal object PeekLogicMsg()
        {
            if (m_LogicQueue.Count <= 0)
                return null;
            object msg;
            m_LogicQueue.TryDequeue(out msg);
            return msg;
        }

        private void ClearLogicQueue()
        {
            while (!m_LogicQueue.IsEmpty)
            {
                object msg;
                m_LogicQueue.TryDequeue(out msg);
            }
        }
    }
}
