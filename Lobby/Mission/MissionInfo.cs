using System;
using System.Collections.Generic;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{

    internal class MissionInfo
    {

        internal MissionInfo(int id)
        {
            m_MissionId = id;
            m_Config = MissionConfigProvider.Instance.GetDataById(m_MissionId);
            if (null != m_Config)
            {
                m_MissionType = (MissionType)m_Config.MissionType;
                m_FinishType = m_Config.Condition;
                m_Param0 = m_Config.Args0;
                m_Param1 = m_Config.Args1;
                foreach (int missionId in m_Config.FollowMissions)
                {
                    if (!m_FollowMissions.Contains(missionId))
                        m_FollowMissions.Add(missionId);
                }
                m_SceneId = m_Config.SceneId;
                m_State = MissionStateType.UNCOMPLETED;
                m_RewardId = m_Config.DropId;
            }
            else
            {
                LogSystem.Warn("Can't find Mission Id {0}", m_MissionId);
            }
        }

        internal int MissionId
        {
            get { return m_MissionId; }
        }
        internal MissionConfig Config
        {
            get { return m_Config; }
        }

        internal MissionType Type
        {
            get { return m_MissionType; }
        }
        internal int FinishType
        {
            get { return m_FinishType; }
        }

        internal int Param0
        {
            get { return m_Param0; }
            set { m_Param0 = value; }
        }
        internal int Param1
        {
            get { return m_Param1; }
            set { m_Param1 = value; }
        }
        internal bool NeedSync
        {
            get { return m_NeedSync; }
            set { m_NeedSync = value; }
        }
        internal List<int> FollowMissions
        {
            get { return m_FollowMissions; }
        }
        internal int SceneId
        {
            get { return m_SceneId; }
        }

        internal string Progress
        {
            get { return m_Progress; }
            set { m_Progress = value; }
        }
        internal int CurValue
        {
            get { return m_CurValue; }
            set { m_CurValue = value; }
        }
        internal int RewardId
        {
            get { return m_RewardId; }
        }

        internal MissionStateType State
        {
            get { return m_State; }
            set { m_State = value; }
        }
        internal void Reset()
        {
            m_State = MissionStateType.UNCOMPLETED;
            m_CurValue = 0;
        }
        private int m_MissionId;
        private MissionType m_MissionType;
        private MissionConfig m_Config;
        private MissionStateType m_State;
        private int m_FinishType;
        private int m_Param0;
        private int m_Param1;
        private int m_CurValue;
        private bool m_NeedSync;
        private List<int> m_FollowMissions = new List<int>();
        private int m_SceneId;
        private string m_Progress;
        private int m_RewardId;
    }

    public class MissionInfoForSync
    {
        public int m_MissionId;
        public bool m_IsCompleted;
        public string m_Progress;
    }
}
