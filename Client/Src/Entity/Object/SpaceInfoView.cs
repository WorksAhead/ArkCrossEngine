namespace ArkCrossEngine
{
    internal class SpaceInfoView
    {
        internal int Id
        {
            get
            {
                return m_ObjId;
            }
        }
        internal bool NeedDestroy
        {
            get { return m_NeedDestroy; }
            set { m_NeedDestroy = value; }
        }
        internal SharedGameObjectInfo ObjectInfo
        {
            get { return m_ObjectInfo; }
        }

        internal void Create(int objId, bool isPlayer)
        {
            m_ObjId = objId;
            m_Actor = GameObjectIdManager.Instance.GenNextId();
            if (isPlayer)
                GfxSystem.CreateGameObject(m_Actor, "BlueCylinder", m_ObjectInfo);
            else
                GfxSystem.CreateGameObject(m_Actor, "RedCylinder", m_ObjectInfo);
        }

        internal void Destroy()
        {
            GfxSystem.DestroyGameObject(m_Actor);
        }

        internal void Update(float x, float y, float z, float dir)
        {
            m_ObjectInfo.X = x;
            m_ObjectInfo.Y = 0;
            m_ObjectInfo.Z = z;
            m_ObjectInfo.FaceDir = dir;
            m_ObjectInfo.DataChangedByLogic = true;
        }

        private int m_Actor = 0;
        private int m_ObjId = 0;
        private bool m_NeedDestroy = false;
        private SharedGameObjectInfo m_ObjectInfo = new SharedGameObjectInfo();
    }
}
