using System.Collections.Generic;

namespace ArkCrossEngine
{
    internal class UserView : CharacterView
    {
        internal UserInfo User
        {
            get { return m_User; }
        }

        internal void Create(UserInfo user)
        {
            Init();
            if (null != user)
            {
                m_User = user;
                ObjectInfo.UnitId = m_User.GetUnitId();
                ObjectInfo.CampId = m_User.GetCampId();
                ObjectInfo.LinkId = m_User.GetLinkId();
                MovementStateInfo msi = m_User.GetMovementStateInfo();
                Vector3 pos = msi.GetPosition3D();
                float dir = msi.GetFaceDir();
                CreateActor(m_User.GetId(), m_User.GetModel(), pos, dir, m_User.Scale);
                user.ActorId = Actor;
                UpdateXSoulEquip();
                InitAnimationSets();
                ObjectInfo.IsPlayer = true;
                if (user.GetId() == WorldSystem.Instance.PlayerSelfId)
                {
                    GfxSystem.MarkPlayerSelf(Actor);
                }
            }
        }
        internal void UpdateXSoulEquip()
        {
            XSoulInfo<XSoulPartInfo> xsoul = m_User.GetXSoulInfo();
            //LogSystem.Debug("-----UpdateXSoulEquip" + xsoul.GetAllXSoulPartData().Count);
            foreach (var pair in xsoul.GetAllXSoulPartData())
            {
                XSoulPartInfo part_info = pair.Value;
                if (part_info == null)
                {
                    //LogSystem.Debug("----UpdateXSoulEquip: part is null!");
                    continue;
                }
                //LogSystem.Debug("-----UpdateXSoulEquip: part:{0} level:{1}", pair.Key, part_info.XSoulPartItem.Level);
                if (part_info.IsEquipModelChanged())
                {
                    string wear_node_and_name = part_info.GetWearNodeAndName();
                    string new_equip = part_info.GetLevelModel();
                    if (!string.IsNullOrEmpty(new_equip))
                    {
                        GfxSystem.ChangeEquip(Actor, wear_node_and_name, new_equip);
                        part_info.SetCurShowedModel(new_equip);
                    }
                }
            }
        }

        internal void UpdateEquipment()
        {
            for (int i = 0; i < EquipmentInfo.c_MaxEquipmentNum; i++)
            {
                ItemDataInfo equip = m_User.GetEquipmentStateInfo().GetEquipmentData(i);
                UpdateEquipment(equip, false);
            }

            UpdateSuit();
        }

        internal void UpdateSuit()
        {
            var playerConfig = PlayerConfigProvider.Instance.GetPlayerConfigById(m_User.GetLinkId());
            string playerSkeleton = playerConfig.m_Skeleton;
            if (string.IsNullOrEmpty(playerSkeleton))
            {
                return;
            }

            // 华为展示不需要换装功能,暂注
            return;

            //string playerModel = playerConfig.m_Model;
            List<string> skinnedModels = new List<string>();
            
            for (int i = 0; i < EquipmentInfo.c_MaxEquipmentNum; i++)
            {
                ItemDataInfo equip = m_User.GetEquipmentStateInfo().GetEquipmentData(i);
                string defaultEquip = playerConfig.GetDefaultModelFromEquipPart((EquipmentType)i);
                if (equip != null &&
                    equip.ItemConfig.m_SkeletonSource != null &&
                    equip.ItemConfig.m_SkeletonSource == playerSkeleton)
                {
                    skinnedModels.Add(equip.ItemConfig.m_Model);
                }
                else if (!string.IsNullOrEmpty(defaultEquip))
                {
                    skinnedModels.Add(defaultEquip);
                }
            }

            /*
            if (skinnedModels.Count == 0)
            {
                return;
            }
            */

            //skinnedModels.Add(playerModel);

            GfxSystem.ChangeSuit(Actor, playerSkeleton, skinnedModels);
        }

        internal void UpdateEquipment(ItemDataInfo equip, bool bUpdateSuit = true)
        {
            if (equip == null || equip.ItemConfig == null)
            {
                return;
            }
            string wear_node_and_name = equip.ItemConfig.m_WearNodeAndName;
            string new_equip = equip.ItemConfig.m_Model;
            GfxSystem.ChangeEquip(Actor, wear_node_and_name, new_equip);

            ItemConfig currentItem = m_User.GetEquipmentStateInfo().EquipmentInfo.Armor[equip.ItemConfig.m_WearParts].ItemConfig;

            // new item has display model or origal item has display model
            if (IsItemSuitNeedRefresh(equip.ItemConfig) || 
                (currentItem != null && IsItemSuitNeedRefresh(currentItem)))
            {
                UpdateSuit();
            }
        }

        internal bool IsItemSuitNeedRefresh(ItemConfig equip)
        {
            if (equip.m_SkeletonSource != null && equip.m_Model != null)
            {
                return true;
            }
            return false;
        }

        internal void Destroy()
        {
            DestroyActor();
            Release();
        }
        internal void Update()
        {
            UpdateAttr();
            UpdateSpatial();
            UpdateAnimation();
            UpdateIndicator();
            UpdateSuperArmor();
        }
        private void UpdateSuperArmor()
        {
            if (null != m_User)
            {
                if (m_User.SuperArmor || m_User.UltraArmor)
                {
                    if (ObjectInfo.m_ShaderPath != "DFM/Basic Outline")
                    {
                        //GfxSystem.SetShader(Actor, "DFM/Basic Outline");
                        //ObjectInfo.m_ShaderPath = "DFM/Basic Outline";
                    }
                }
                else
                {
                    if (ObjectInfo.m_ShaderPath != "DFM/NormalMonster")
                    {
                        //GfxSystem.SetShader(Actor, "DFM/NormalMonster");
                        //ObjectInfo.m_ShaderPath = "DFM/NormalMonster";
                    }
                }
                m_User.IsArmorChanged = false;
            }
        }

        internal void SetIndicatorInfo(bool visible, float dir, int targetType)
        {
            m_IndicatorVisible = visible;
            m_IndicatorDir = dir;
            m_IndicatorTargetType = targetType;
        }
        internal void SetIndicatorTargetType(int targetType)
        {
            GfxSystem.SendMessage(m_IndicatorActor, "SetIndicatorTarget", targetType);
        }

        protected override bool UpdateVisible(bool visible)
        {
            SetVisible(visible);
            return visible;
        }
        public void CreateIndicatorActor()
        {
            string model = m_User.GetIndicatorModel();
            m_IndicatorActor = GameObjectIdManager.Instance.GenNextId();
            GfxSystem.CreateGameObject(m_IndicatorActor, model, 0, 0, 0, 0, 0, 0, false);
            GfxSystem.SetGameObjectVisible(m_IndicatorActor, false);
            GfxSystem.SendMessage(m_IndicatorActor, "SetOwner", Actor);
        }

        private void UpdateAttr()
        {
            if (null != m_User)
            {
                ObjectInfo.Blood = m_User.Hp;
                ObjectInfo.MaxBlood = m_User.GetActualProperty().HpMax;
                ObjectInfo.Energy = m_User.Energy;
                ObjectInfo.MaxEnergy = m_User.GetActualProperty().EnergyMax;
                ObjectInfo.FightingScore = m_User.FightingScore;
                ObjectInfo.Rage = m_User.Rage;
                ObjectInfo.MaxRage = m_User.GetActualProperty().RageMax;
                ObjectInfo.IsSuperArmor = (m_User.SuperArmor || m_User.UltraArmor);
                m_User.GfxStateFlag = ObjectInfo.GfxStateFlag;
            }
        }

        private void UpdateSpatial()
        {
            if (null != m_User)
            {
                MovementStateInfo msi = m_User.GetMovementStateInfo();
                if (ObjectInfo.IsGfxMoveControl)
                {
                    if (ObjectInfo.DataChangedByGfx)
                    {
                        msi.PositionX = ObjectInfo.X;
                        msi.PositionY = ObjectInfo.Y;
                        msi.PositionZ = ObjectInfo.Z;
                        msi.SetFaceDir(ObjectInfo.FaceDir);
                        ObjectInfo.DataChangedByGfx = false;
                    }
                    if (ObjectInfo.WantDirChangedByGfx)
                    {
                        msi.SetWantFaceDir(ObjectInfo.WantFaceDir);
                        ObjectInfo.WantDirChangedByGfx = false;
                    }
                }
                else
                {
                    if (ObjectInfo.DataChangedByGfx)
                    {
                        msi.PositionX = ObjectInfo.X;
                        msi.PositionY = ObjectInfo.Y;
                        msi.PositionZ = ObjectInfo.Z;
                        //msi.SetFaceDir(ObjectInfo.FaceDir);
                        ObjectInfo.DataChangedByGfx = false;
                    }
                    UpdateMovement();
                }
                ObjectInfo.WantFaceDir = msi.GetWantFaceDir();
                SimulateDir(ObjectInfo.WantFaceDir);
            }
        }

        private void SimulateDir(float dir)
        {
            List<NpcInfo> summons = m_User.GetSkillStateInfo().GetSummonObject();
            foreach (NpcInfo npc in summons)
            {
                if (npc.IsSimulateMove)
                {
                    npc.GetMovementStateInfo().SetWantFaceDir(dir);
                }
            }
        }

        private void UpdateAnimation()
        {
            if (!CanAffectPlayerSelf) return;
            UpdateStoryAnim();
            if (IsPlayingStoryAnim())
            {
                return;
            }
            if (null != m_User)
            {
                UpdateState();
                if (ObjectInfo.IsGfxAnimation)
                {
                    m_CharacterAnimationInfo.Reset();
                    m_IdleState = IdleState.kNotIdle;
                    return;
                }
                UpdateMoveAnimation();
                UpdateDead();
                UpdateIdle();
            }
        }

        private void UpdateIndicator()
        {
            if (null != m_User)
            {
                ObjectInfo.IsIndicatorVisible = m_IndicatorVisible;
                ObjectInfo.IndicatorDir = m_IndicatorDir;
                ObjectInfo.IndicatorType = m_IndicatorTargetType;
            }
        }

        private void Init()
        {
            m_User = null;
            old_color_ = Vector4.Zero;
        }

        private void Release()
        {
            XSoulInfo<XSoulPartInfo> xsoul = m_User.GetXSoulInfo();
            foreach (var pair in xsoul.GetAllXSoulPartData())
            {
                XSoulPartInfo part_info = pair.Value;
                if (part_info == null)
                {
                    continue;
                }
                part_info.SetCurShowedModel("");
            }
        }

        protected override CharacterInfo GetOwner()
        {
            return m_User;
        }

        private UserInfo m_User = null;
        private Vector4 old_color_;
        private int m_IndicatorActor = 0;
        private float m_IndicatorDir = 0;
        private bool m_IndicatorVisible = false;
        private int m_IndicatorTargetType = 1;
        private string m_LastLevelModel = "";
    }
}
