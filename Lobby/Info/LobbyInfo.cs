using System;
using System.Collections.Generic;

namespace Lobby
{
    internal class LobbyInfo
    {
        internal LobbyInfo()
        {
        }

        internal SortedDictionary<string, RoomServerInfo> RoomServerInfos
        {
            get
            {
                return m_RoomServerInfos;
            }
        }
        internal SortedDictionary<string, NodeInfo> NodeInfos
        {
            get { return m_NodeInfos; }
        }

        internal SortedDictionary<int, RoomInfo> Rooms
        {
            get
            {
                return m_Rooms;
            }
        }

        internal RoomInfo GetRoomByID(int roomId)
        {
            RoomInfo info = null;
            m_Rooms.TryGetValue(roomId, out info);
            return info;
        }

        internal RoomInfoForMessage[] GetRoomList(int page, int countPerPage, out bool haveNextPage)
        {
            haveNextPage = false;
            int ct = m_CustomRooms.Count - page * countPerPage;
            if (ct < 0)
            {
                ct = 0;
            }
            else if (ct > countPerPage)
            {
                ct = countPerPage;
                haveNextPage = true;
            }
            RoomInfoForMessage[] infos = null;
            if (ct > 0)
            {
                infos = new RoomInfoForMessage[ct];
                int startIx = page * countPerPage;
                for (int ix = 0; ix < ct; ++ix)
                {
                    RoomInfoForMessage info = new RoomInfoForMessage();
                    int id = m_CustomRooms[startIx + ix];
                    RoomInfo room;
                    if (m_Rooms.TryGetValue(id, out room))
                    {
                        info.m_Creator = room.CreatorNick;
                        info.m_RoomId = room.RoomId;
                        info.m_Type = room.SceneType;
                        info.m_TotalCount = room.TotalCount;
                        info.m_CurCount = room.UserCount;
                    }
                    infos[ix] = info;
                }
            }
            return infos;
        }

        internal int CreateAutoRoom(ulong[] users, int type)
        {
            RoomInfo room = NewRoomInfo();
            room.RoomId = ++m_CurRoomId;
            room.SceneType = type;
            room.RoomServerName = GetIdlestRoomServer();
            room.TotalCount = users.Length;
            if (!room.IsPvp)
            {
                room.AddUsers((int)CampIdEnum.Blue, users);
            }
            else
            {
                int ct = users.Length / 2;
                ulong[] blues = new ulong[users.Length - ct];
                ulong[] reds = new ulong[ct];
                Array.Copy(users, 0, blues, 0, users.Length - ct);
                Array.Copy(users, ct, reds, 0, ct);
                room.AddUsers((int)CampIdEnum.Blue, blues);
                room.AddUsers((int)CampIdEnum.Red, reds);
            }

            if (!m_Rooms.ContainsKey(room.RoomId))
            {
                m_Rooms.Add(room.RoomId, room);
            }
            else
            {
                m_Rooms[room.RoomId] = room;
            }
            room.Creator = 0;
            return room.RoomId;
        }

        internal void StartBattleRoom(int roomId)
        {
            RoomInfo room;
            if (m_Rooms.TryGetValue(roomId, out room))
            {
                room.StartBattleRoom();
            }
        }

        internal void StopBattleRoom(int roomId)
        {
            RoomInfo room;
            if (m_Rooms.TryGetValue(roomId, out room))
            {
                room.StopBattleRoom();
            }
        }

        internal void Tick()
        {
            Queue<RoomInfo> recycles = new Queue<RoomInfo>();
            foreach (RoomInfo room in m_Rooms.Values)
            {
                room.Tick();
                if (room.CurrentState == RoomState.Close || (room.CurrentState == RoomState.Prepare && room.IsEmpty == true))
                {
                    recycles.Enqueue(room);
                }
            }
            while (recycles.Count > 0)
            {
                RoomInfo room = recycles.Dequeue();
                m_Rooms.Remove(room.RoomId);
                if (room.IsCustomRoom)
                    m_CustomRooms.Remove(room.RoomId);
                RecycleRoomInfo(room);
            }
        }

        private string GetIdlestRoomServer()
        {
            string name = "";
            int minNum = int.MaxValue;
            RoomServerInfo retInfo = null;
            foreach (RoomServerInfo info in m_RoomServerInfos.Values)
            {
                if (info.IdleRoomNum > info.AllocedRoomNum && info.UserNum < minNum)
                {

                    LogSys.Log(LOG_TYPE.DEBUG, "GetIdlestRoomServer, Bubble process, Server:{0} UserNum:{1} < {2}", info.RoomServerName, info.UserNum, minNum);

                    minNum = info.UserNum;
                    retInfo = info;
                }
            }
            if (null != retInfo)
            {
                ++retInfo.AllocedRoomNum;
                name = retInfo.RoomServerName;
            }
            LogSys.Log(LOG_TYPE.DEBUG, "GetIdlestRoomServer, Name:{0} UserNum:{1}", name, minNum);
            return name;
        }

        private RoomInfo NewRoomInfo()
        {
            RoomInfo room = null;
            if (m_UnusedRoomInfos.Count > 0)
            {
                room = m_UnusedRoomInfos.Dequeue();
            }
            else
            {
                room = new RoomInfo();
                room.LobbyInfo = this;
            }
            room.CurrentState = RoomState.Prepare;
            return room;
        }

        private void RecycleRoomInfo(RoomInfo room)
        {
            room.Reset();
            m_UnusedRoomInfos.Enqueue(room);
        }

        private SortedDictionary<string, RoomServerInfo> m_RoomServerInfos = new SortedDictionary<string, RoomServerInfo>();
        private SortedDictionary<string, NodeInfo> m_NodeInfos = new SortedDictionary<string, NodeInfo>();
        private SortedDictionary<int, RoomInfo> m_Rooms = new SortedDictionary<int, RoomInfo>();
        private List<int> m_CustomRooms = new List<int>();
        private int m_CurRoomId = 0;

        private Queue<RoomInfo> m_UnusedRoomInfos = new Queue<RoomInfo>();
    }
}
