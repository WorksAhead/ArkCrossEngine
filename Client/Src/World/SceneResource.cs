using System.Collections.Generic;
using ArkCrossEngine.Network;

namespace ArkCrossEngine
{
    internal class SceneResource
    {
        internal int ResId
        {
            get
            {
                return m_SceneResId;
            }
        }
        internal MapDataProvider StaticData
        {
            get
            {
                return m_SceneStaticData;
            }
        }
        internal Data_SceneConfig SceneConfig
        {
            get
            {
                return m_SceneConfig;
            }
        }
        internal Data_SceneDropOut SceneDropOut
        {
            get
            {
                return m_SceneDropOut;
            }
        }

        internal bool IsPureClientScene
        {
            get
            {
                if (null == m_SceneConfig)
                    return true;
                else
                    return m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_PURE_CLIENT_SCENE;
            }
        }
        internal bool IsPve
        {
            get
            {
                if (null == m_SceneConfig)
                    return false;
                else
                    return m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_PVE;
            }
        }
        internal bool IsPvap
        {
            get
            {
                if (null == m_SceneConfig)
                {
                    return false;
                }
                else
                {
                    return m_SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_PVAP;
                }
            }
        }
        internal bool IsMultiPve
        {
            get
            {
                if (null == m_SceneConfig)
                    return false;
                else
                    return m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_MULTI_PVE;
            }
        }
        internal bool IsPvp
        {
            get
            {
                if (null == m_SceneConfig)
                    return false;
                else
                    return m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_PVP;
            }
        }
        internal bool IsAttempt
        {
            get
            {
                if (null == m_SceneConfig)
                    return false;
                else
                    return m_SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_ATTEMPT;
            }
        }
        internal bool IsGold
        {
            get
            {
                if (null == m_SceneConfig)
                    return false;
                else
                    return m_SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_GOLD;
            }
        }
        internal bool IsExpedition
        {
            get
            {
                if (null == m_SceneConfig)
                    return false;
                else
                    return m_SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_EXPEDITION;
            }
        }
        internal bool IsELite
        {
            get
            {
                if (null == m_SceneConfig)
                    return false;
                else
                    return m_SceneConfig.m_SubType == (int)SceneSubTypeEnum.TYPE_ELITE;
            }
        }
        internal bool IsServerSelectScene
        {
            get
            {
                if (null == m_SceneConfig)
                    return false;
                else
                    return m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_SERVER_SELECT;
            }
        }
        internal bool IsWaitSceneLoad
        {
            get { return m_IsWaitSceneLoad; }
        }
        internal bool IsWaitRoomServerConnect
        {
            get { return m_IsWaitRoomServerConnect; }
            set { m_IsWaitRoomServerConnect = value; }
        }
        internal bool IsNoticeException
        {
            get { return m_IsNoticeException; }
            set { m_IsNoticeException = value; }
        }
        internal bool IsSuccessEnter
        {
            get { return m_IsSuccessEnter; }
        }
        internal void Init(int resId)
        {
            m_SceneResId = resId;
            LoadSceneData(resId);
            WorldSystem.Instance.SceneContext.SceneResId = resId;
            WorldSystem.Instance.SceneContext.IsRunWithRoomServer = (IsPvp || IsMultiPve);
            m_IsWaitSceneLoad = true;
            m_IsWaitRoomServerConnect = true;
            m_IsNoticeException = false;
            m_IsSuccessEnter = false;

            Data_Unit unit = m_SceneStaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(NetworkSystem.Instance.CampId)) as Data_Unit;
            if (null != unit)
            {
                m_CameraLookAtX = unit.m_Pos.X;
                m_CameraLookAtY = unit.m_Pos.Y;
                m_CameraLookAtZ = unit.m_Pos.Z;
            }
            CalculateDropOut();

            LogicSystem.LogFromLogic("SceneResource.Init {0}", resId);
        }
        internal void Release()
        {
            LogicSystem.LogFromLogic("SceneResource.Release");
        }
        internal void GetLookAt(out float x, out float y, out float z)
        {
            x = m_CameraLookAtX;
            y = m_CameraLookAtY;
            z = m_CameraLookAtZ;
        }

        private void CalculateDropOut()
        {
            if (null != m_SceneDropOut && IsPve)
            {
                m_DropMoneyData.Clear();
                m_DropHpData.Clear();
                m_DropMpData.Clear();
                List<int> npcList = new List<int>();
                foreach (Data_Unit npcUnit in m_SceneStaticData.m_UnitMgr.GetData().Values)
                {
                    if (npcUnit.GetId() < 10000)
                    {
                        npcList.Add(npcUnit.GetId());
                    }
                }
                if (npcList.Count <= 0) return;
                List<int> addIndex = new List<int>();
                // calculate money
                if (m_SceneDropOut.m_GoldMin > 0)
                {
                    int dropCount = m_SceneDropOut.m_GoldSum / m_SceneDropOut.m_GoldMin;
                    int curMoney = m_SceneDropOut.m_GoldSum;
                    int npcCount = npcList.Count;
                    dropCount = npcCount > dropCount ? dropCount : npcCount;
                    while (dropCount > addIndex.Count)
                    {
                        int index = CrossEngineHelper.Random.Next(0, npcList.Count);
                        if (addIndex.IndexOf(index) == -1)
                        {
                            int dropMoney = CrossEngineHelper.Random.Next(m_SceneDropOut.m_GoldMin, m_SceneDropOut.m_GoldMax);
                            if (dropMoney > curMoney) dropMoney = curMoney;
                            curMoney -= dropMoney;
                            m_DropMoneyData.Add(npcList[index], dropMoney);
                            addIndex.Add(index);
                            if (curMoney <= 0) break;
                        }
                    }
                }
                // calculate hp
                addIndex.Clear();
                while (m_SceneDropOut.m_HpCount > addIndex.Count)
                {
                    int index = CrossEngineHelper.Random.Next(0, npcList.Count);
                    if (addIndex.IndexOf(index) == -1)
                    {
                        //LogSystem.Debug("npcList count = {0} index = {1}", npcList.Count, index);
                        m_DropHpData.Add(npcList[index], m_SceneDropOut.m_HpPercent);
                        addIndex.Add(index);
                    }
                }
                // calculate mp
                addIndex.Clear();
                while (m_SceneDropOut.m_HpCount > addIndex.Count)
                {
                    int index = CrossEngineHelper.Random.Next(0, npcList.Count);
                    if (addIndex.IndexOf(index) == -1)
                    {
                        m_DropMpData.Add(npcList[index], m_SceneDropOut.m_HpPercent);
                        addIndex.Add(index);
                    }
                }
            }
        }

        internal int GetDropMoney(int unitId)
        {
            int ret;
            m_DropMoneyData.TryGetValue(unitId, out ret);
            return ret;
        }
        internal int GetDropHp(int unitId)
        {
            int ret;
            m_DropHpData.TryGetValue(unitId, out ret);
            return ret;
        }
        internal int GetDropMp(int unitId)
        {
            int ret;
            m_DropMpData.TryGetValue(unitId, out ret);
            return ret;
        }
        internal void NotifyUserEnter()
        {
            m_IsSuccessEnter = true;
            LogSystem.Debug("SceneResource.NotifyUserEnter");
        }
        internal void UpdateObserverCamera(float x, float y)
        {
            /*const float c_drag_velocity = 0.5f;
            if (m_IsSuccessEnter) {
              Vector3 pos = GfxSystemExt.GfxSystem.Instance.MainCamera.Position;
              if (x <= 0.2f) {
                pos.X -= c_drag_velocity; 
              } else if (x >= 0.8f) {
                pos.X += c_drag_velocity;
              }
              if (y <= 0.2f) {
                pos.Z -= c_drag_velocity;
              } else if (y >= 0.8f) {
                pos.Z += c_drag_velocity;
              }
              if (pos.X < 0)
                pos.X = 0;
              if (pos.Z < 0)
                pos.Z = 0;
              float xsize = StaticData.m_MapInfo.m_MapSize.X;
              float ysize = StaticData.m_MapInfo.m_MapSize.Y;
              if (pos.X > xsize)
                pos.X = xsize;
              if (pos.Z > ysize)
                pos.Z = ysize;
              pos.Y = m_CameraLookAtHeight;
              GfxSystem.SendMessage("GfxGameRoot", "CameraLookat", new float[]{ pos.X, pos.Y, pos.Z });
            }*/
        }
        private void OnLoadFinish()
        {
            LogSystem.Debug("SceneResource.OnLoadFinish");
            m_IsWaitSceneLoad = false;
            if (null != m_SceneDropOut)
            {
                //LogSystem.Debug("{0} {1} {2}", m_SceneDropOut.m_GoldSum, m_SceneDropOut.m_GoldMin, m_SceneDropOut.m_GoldMax);
            }
            foreach (int id in m_DropMoneyData.Keys)
            {
                //LogSystem.Debug("id = {0}, monew = {1}", id, m_DropMoneyData[id]);
            }

            Data_Unit unit = m_SceneStaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(NetworkSystem.Instance.CampId)) as Data_Unit;
            if (null != unit)
            {
                GfxSystem.SendMessage("GfxGameRoot", "CameraLookatImmediately", new float[] { unit.m_Pos.X, unit.m_Pos.Y, unit.m_Pos.Z });
            }
        }
        private bool LoadSceneData(int sceneResId)
        {
            bool result = true;
            m_SceneResId = sceneResId;
            // 加载场景配置数据
            m_SceneConfig = SceneConfigProvider.Instance.GetSceneConfigById(m_SceneResId);
            if (null == m_SceneConfig)
                LogSystem.Error("LoadSceneData {0} failed!", sceneResId);

            m_SceneDropOut = SceneConfigProvider.Instance.GetSceneDropOutById(m_SceneConfig.m_DropId);
            // 加载本场景xml数据
            m_SceneStaticData = SceneConfigProvider.Instance.LoadSceneConfig(
              sceneResId, FilePathDefine_Client.C_RootPath);
            if (null == m_SceneStaticData)
                LogSystem.Error("LoadSceneConfig {0} failed!", sceneResId);
            m_SceneStaticData = SceneConfigProvider.Instance.GetMapDataBySceneResId(m_SceneResId);
            HashSet<int> monstList = null;
            if (IsExpedition)
            {
                monstList = new HashSet<int>();
                RoleInfo curRole = LobbyClient.Instance.CurrentRole;
                ExpeditionPlayerInfo expInfo = curRole.GetExpeditionInfo();
                ExpeditionPlayerInfo.TollgateData data = expInfo.Tollgates[expInfo.ActiveTollgate];
                monstList.UnionWith(data.EnemyList);
            }
            GfxSystem.LoadScene(m_SceneConfig.m_ClientSceneFile, m_SceneConfig.m_Chapter, m_SceneConfig.GetId(), monstList, OnLoadFinish);
            return result;
        }

        private int m_SceneResId;
        private MapDataProvider m_SceneStaticData;
        private Data_SceneConfig m_SceneConfig;
        private Data_SceneDropOut m_SceneDropOut;
        private Dictionary<int, int> m_DropMoneyData = new Dictionary<int, int>();
        private Dictionary<int, int> m_DropHpData = new Dictionary<int, int>();
        private Dictionary<int, int> m_DropMpData = new Dictionary<int, int>();

        private bool m_IsWaitSceneLoad = true;
        private bool m_IsWaitRoomServerConnect = true;
        private bool m_IsNoticeException = false;
        private bool m_IsSuccessEnter = false;
        private float m_CameraLookAtX = 0;
        private float m_CameraLookAtY = 0;
        private float m_CameraLookAtZ = 0;
    }
}
