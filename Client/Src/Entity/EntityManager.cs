using System.Collections.Generic;

namespace ArkCrossEngine
{
    /**
     * @brief 角色管理器
     * @remarks 这个类在GameObjects采取数据驱动的方式后，它的职责变为GameObjects的View层，它在每个Tick负责更新各个GameObject的显示
     */
    internal sealed class EntityManager
    {
        #region Singleton
        private static EntityManager s_instance_ = new EntityManager();
        internal static EntityManager Instance
        {
            get { return s_instance_; }
        }
        #endregion

        /**
         * @brief 构筑函数
         *
         * @return 
         */
        private EntityManager()
        {
        }

        /**
         * @brief 初始化
         *
         * @return 
         */
        internal void Init()
        {
        }

        /**
         * @brief 销毁
         *
         * @return 
         */
        internal void Release()
        {
        }

        internal void Tick()
        {
            lock (GfxSystem.SyncLock)
            {
                foreach (UserView view in m_UserViews.Values)
                {
                    view.Update();
                }

                foreach (NpcView view in m_NpcViews.Values)
                {
                    view.Update();
                }
            }
            if (m_SpaceInfoViews.Count > 0)
            {
                if (!GlobalVariables.Instance.IsDebug)
                {
                    EntityManager.Instance.MarkSpaceInfoViews();
                    EntityManager.Instance.DestroyUnusedSpaceInfoViews();
                }
            }
        }

        internal void CreatePlayerSelfView(int objId)
        {
            CreateUserView(objId);
            UserView view = GetUserViewById(objId);
            if (null != view)
            {
                view.CreateIndicatorActor();
                GfxSystem.SendMessage("GfxGameRoot", "CameraFollowImmediately", view.Actor);
                GfxSystem.ResetInputState();
            }
        }

        internal void CreateUserView(int objId)
        {
            if (!m_UserViews.ContainsKey(objId))
            {

                UserInfo obj = WorldSystem.Instance.UserManager.GetUserInfo(objId);
                if (null != obj)
                {
                    UserView view = new UserView();
                    view.Create(obj);
                    view.UpdateXSoulEquip();
                    view.UpdateEquipment();
                    m_UserViews.Add(objId, view);
                }
            }
        }

        internal void DestroyUserView(int objId)
        {
            UserView view;
            if (m_UserViews.TryGetValue(objId, out view))
            {
                if (view != null)
                {
                    view.Destroy();
                }
                m_UserViews.Remove(objId);
            }
        }

        internal void CreateNpcView(int objId)
        {
            if (!m_NpcViews.ContainsKey(objId))
            {

                NpcInfo obj = WorldSystem.Instance.NpcManager.GetNpcInfo(objId);
                if (null != obj)
                {
                    NpcView view = new NpcView();
                    view.Create(obj);
                    m_NpcViews.Add(objId, view);
                }
                WorldSystem.Instance.SyncGfxNpcInfo(objId);
            }
        }

        internal void DestroyNpcView(int objId)
        {
            NpcView view;
            if (m_NpcViews.TryGetValue(objId, out view))
            {
                if (view != null)
                {
                    view.Destroy();
                }
                m_NpcViews.Remove(objId);
            }
        }

        internal UserView GetUserViewById(int objId)
        {
            UserView view = null;
            m_UserViews.TryGetValue(objId, out view);
            return view;
        }

        internal NpcView GetNpcViewById(int objId)
        {
            NpcView view = null;
            m_NpcViews.TryGetValue(objId, out view);
            return view;
        }

        internal CharacterView GetCharacterViewById(int objId)
        {
            CharacterView view = GetUserViewById(objId);
            if (null == view)
                view = GetNpcViewById(objId);
            return view;
        }

        internal bool IsVisible(int objId)
        {
            bool ret = false;
            CharacterView view = GetCharacterViewById(objId);
            if (null != view)
            {
                ret = view.Visible;
            }
            return ret;
        }

        internal void MarkSpaceInfoViews()
        {
            foreach (SpaceInfoView view in m_SpaceInfoViews.Values)
            {
                view.NeedDestroy = true;
            }
        }

        internal void UpdateSpaceInfoView(int objId, bool isPlayer, float x, float y, float z, float dir)
        {
            SpaceInfoView view = GetSpaceInfoViewById(objId);
            if (null == view)
            {
                view = CreateSpaceInfoView(objId, isPlayer);
            }
            if (null != view)
            {
                view.NeedDestroy = false;
                view.Update(x, y, z, dir);
            }
        }

        internal void DestroyUnusedSpaceInfoViews()
        {
            List<int> deletes = new List<int>();
            foreach (SpaceInfoView view in m_SpaceInfoViews.Values)
            {
                if (view.NeedDestroy)
                    deletes.Add(view.Id);
            }
            foreach (int id in deletes)
            {
                DestroySpaceInfoView(id);
            }
            deletes.Clear();
        }

        /**
         * @brief 检验是否在某位置
         *
         * @param pos
         *
         * @return 
         */
        internal bool IsAtPosition(CharacterInfo entity, Vector2 pos)
        {
            return Vector2.Distance(entity.GetMovementStateInfo().GetPosition2D(), pos) < ClientConfig.s_PositionRefix;
        }

        /**
         * @brief 预估移动时间
         *
         * @param pos
         *
         * @return 
         */
        internal float PredictMoveDuration(CharacterInfo character, Vector2 pos)
        {
            float distance = Vector2.Distance(character.GetMovementStateInfo().GetPosition2D(), pos);
            float duration = distance / character.GetActualProperty().MoveSpeed;
            duration += ClientConfig.s_PredictMoveDurationRefix;

            return duration;
        }

        private SpaceInfoView CreateSpaceInfoView(int objId, bool isPlayer)
        {
            SpaceInfoView view = null;
            if (!m_SpaceInfoViews.ContainsKey(objId))
            {
                view = new SpaceInfoView();
                view.Create(objId, isPlayer);
                m_SpaceInfoViews.Add(objId, view);
            }
            return view;
        }

        private void DestroySpaceInfoView(int objId)
        {
            SpaceInfoView view;
            if (m_SpaceInfoViews.TryGetValue(objId, out view))
            {
                if (view != null)
                {
                    view.Destroy();
                }
                m_SpaceInfoViews.Remove(objId);
            }
        }

        private SpaceInfoView GetSpaceInfoViewById(int objId)
        {
            SpaceInfoView view = null;
            m_SpaceInfoViews.TryGetValue(objId, out view);
            return view;
        }

        private MyDictionary<int, UserView> m_UserViews = new MyDictionary<int, UserView>();
        private MyDictionary<int, NpcView> m_NpcViews = new MyDictionary<int, NpcView>();
        private MyDictionary<int, SpaceInfoView> m_SpaceInfoViews = new MyDictionary<int, SpaceInfoView>();
    }
}
