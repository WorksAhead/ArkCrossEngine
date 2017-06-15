using System;
using System.Collections.Generic;
using System.IO;
using SkillSystem;
using ArkCrossEngine;
using UnityEngine;
using UnityEngine.Profiling;

namespace GfxModule.Skill
{
    public delegate void GfxSkillStartHandler(GameObject obj, int skillid);

    public sealed class GfxSkillSystem
    {

        public static GfxSkillStartHandler OnGfxShillStart = null;

        private class SkillInstanceInfo
        {
            public int m_SkillId;
            public SkillInstance m_SkillInstance;
            public bool m_IsUsed;
        }
        private class SkillLogicInfo
        {
            public GameObject Sender
            {
                get
                {
                    return m_Sender;
                }
            }
            public int SkillId
            {
                get
                {
                    return m_SkillInfo.m_SkillId;
                }
            }
            public SkillInstance SkillInst
            {
                get
                {
                    return m_SkillInfo.m_SkillInstance;
                }
            }
            public SkillInstanceInfo Info
            {
                get
                {
                    return m_SkillInfo;
                }
            }

            public SkillLogicInfo(GameObject obj, SkillInstanceInfo info)
            {
                m_Sender = obj;
                m_SkillInfo = info;
            }

            private GameObject m_Sender;
            private SkillInstanceInfo m_SkillInfo;
        }
        public void Init()
        {
            GfxSystem.OnFingerDown += Skill.Trigers.TriggerUtil.OnFingerDown;
            GfxSystem.OnFingerUp += Skill.Trigers.TriggerUtil.OnFingerUp;
            //注册技能触发器
            SkillTrigerManager.Instance.RegisterTrigerFactory("movecontrol", new SkillTrigerFactoryHelper<Trigers.MoveControlTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("animation", new SkillTrigerFactoryHelper<Trigers.AnimationTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("areadamage", new SkillTrigerFactoryHelper<Trigers.AreaDamageTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("colliderdamage", new SkillTrigerFactoryHelper<Trigers.ColliderDamageTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("playsound", new SkillTrigerFactoryHelper<Trigers.PlaySoundTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("stopsound", new SkillTrigerFactoryHelper<Trigers.StopSoundTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("charactereffect", new SkillTrigerFactoryHelper<Trigers.CharacterEffectTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("sceneeffect", new SkillTrigerFactoryHelper<Trigers.SceneEffectTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("charge", new SkillTrigerFactoryHelper<Trigers.ChargeTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("jump", new SkillTrigerFactoryHelper<Trigers.JumpTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("timescale", new SkillTrigerFactoryHelper<Trigers.TimeScaleTriger>());

            SkillTrigerManager.Instance.RegisterTrigerFactory("addimpacttoself", new SkillTrigerFactoryHelper<Trigers.AddImpactToSelfTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("lockframe", new SkillTrigerFactoryHelper<Trigers.LockFrameTriger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("movechild", new SkillTrigerFactoryHelper<Trigers.MoveChildTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("addbreaksection", new SkillTrigerFactoryHelper<Trigers.BreakSectionTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("startcurvemove", new SkillTrigerFactoryHelper<Trigers.CurveMovementTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("shakecamera2", new SkillTrigerFactoryHelper<Trigers.ShakeCamera2Trigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setanimspeed", new SkillTrigerFactoryHelper<Trigers.AnimationSpeedTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setcross2othertime", new SkillTrigerFactoryHelper<Trigers.SetCrossFadeTimeTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("findmovetarget", new SkillTrigerFactoryHelper<Trigers.ChooseTargetTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("addlockinputtime", new SkillTrigerFactoryHelper<Trigers.AddLockInputTimeTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("summonnpc", new SkillTrigerFactoryHelper<Trigers.SummonObjectTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("settransform", new SkillTrigerFactoryHelper<Trigers.SetTransformTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("destroyself", new SkillTrigerFactoryHelper<Trigers.DestroySelfTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("destroysummonnpc", new SkillTrigerFactoryHelper<Trigers.DestroySummonObjectTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setchildvisible", new SkillTrigerFactoryHelper<Trigers.SetChildVisibleTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("rotate", new SkillTrigerFactoryHelper<Trigers.RotateTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setenable", new SkillTrigerFactoryHelper<Trigers.SetEnableTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("gotosection", new SkillTrigerFactoryHelper<Trigers.GotoSectionTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("createshadow", new SkillTrigerFactoryHelper<Trigers.CreateShadowTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("cleardamagepool", new SkillTrigerFactoryHelper<Trigers.ClearDamagePoolTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("cleardamagestate", new SkillTrigerFactoryHelper<Trigers.ClearDamageStateTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("checkonground", new SkillTrigerFactoryHelper<Trigers.CheckOnGroundTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("stopcursection", new SkillTrigerFactoryHelper<Trigers.StopCurSectionTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("enablechangedir", new SkillTrigerFactoryHelper<Trigers.EnableChangeDirTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("stopeffect", new SkillTrigerFactoryHelper<Trigers.StopEffectTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setcamerafollowspeed", new SkillTrigerFactoryHelper<Trigers.SetCameraFollowSpeed>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("resetcamerafollowspeed", new SkillTrigerFactoryHelper<Trigers.ResetCameraFollowSpeed>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("movecamera", new SkillTrigerFactoryHelper<Trigers.MoveCameraTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("facetotarget", new SkillTrigerFactoryHelper<Trigers.FaceToTargetTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("rotatecamera", new SkillTrigerFactoryHelper<Trigers.RotateCameraTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setlifetime", new SkillTrigerFactoryHelper<Trigers.SetlifeTimeTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("simulatemove", new SkillTrigerFactoryHelper<Trigers.SimulateMoveTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("exchangeposition", new SkillTrigerFactoryHelper<Trigers.ExchangePositionTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("fruitninjia", new SkillTrigerFactoryHelper<Trigers.FruitNinjiaTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("oninput", new SkillTrigerFactoryHelper<Trigers.OnInputTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("move2targetpos", new SkillTrigerFactoryHelper<Trigers.Move2TargetPosTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("crosssummonmove", new SkillTrigerFactoryHelper<Trigers.CrossSummonMoveTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("storepos", new SkillTrigerFactoryHelper<Trigers.StorePosTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("restorepos", new SkillTrigerFactoryHelper<Trigers.RestorePosTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("blackscene", new SkillTrigerFactoryHelper<Trigers.BlackSceneTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setuivisible", new SkillTrigerFactoryHelper<Trigers.SetUIVisibleTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("addimpacttotarget", new SkillTrigerFactoryHelper<Trigers.AddImpactToTargetTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("grabtarget", new SkillTrigerFactoryHelper<Trigers.GrabTargetTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("oncross", new SkillTrigerFactoryHelper<Trigers.OnCrossTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("fieldofview", new SkillTrigerFactoryHelper<Trigers.FieldOfViewTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("forbidnextskill", new SkillTrigerFactoryHelper<Trigers.ForbidNextSkillTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("playpartanimation", new SkillTrigerFactoryHelper<Trigers.PlayPartAnimTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("cullingmask", new SkillTrigerFactoryHelper<Trigers.CullingMaskTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("parrycheck", new SkillTrigerFactoryHelper<Trigers.ParryCheckTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("parrycheckover", new SkillTrigerFactoryHelper<Trigers.ParryCheckOverTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("facetoattacker", new SkillTrigerFactoryHelper<Trigers.FaceToAttackerTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("removebreaksection", new SkillTrigerFactoryHelper<Trigers.RemoveBreakSectionTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("skyboxmaterial", new SkillTrigerFactoryHelper<Trigers.SkyboxMaterialTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("sonreleaseskill", new SkillTrigerFactoryHelper<Trigers.SonReleaseSkillTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setmaincamera", new SkillTrigerFactoryHelper<Trigers.SetMainCamera>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("setlayer", new SkillTrigerFactoryHelper<Trigers.SetLayerTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("palmhit", new SkillTrigerFactoryHelper<Trigers.PalmHitTrigger>());
            SkillTrigerManager.Instance.RegisterTrigerFactory("shadercolor", new SkillTrigerFactoryHelper<Trigers.ShaderPropertyTrigger>());
        }
        public void PreLoadAllSkills()
        {
            MyDictionary<int, object> datacont = SkillConfigProvider.Instance.skillLogicDataMgr.GetData();

            foreach (KeyValuePair<int, object> pair in datacont)
            {
                int id = pair.Key;
                SkillLogicData skillData = pair.Value as SkillLogicData;

                string filePath = HomePath.GetAbsolutePath(FilePathDefine_Client.C_SkillDslPath + skillData.SkillDataFile);
                if (!String.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            PreloadNewSkillInstance(id);
                        }
                    }
                }
            }
        }
        public void PreLoadRoleSkills(List<SkillInfo> skilllist)
        {
            if (skilllist != null)
            {
                foreach (SkillInfo skillinfo in skilllist)
                {
                    if (skillinfo != null)
                    {
                        PreloadNewSkillInstance(skillinfo.SkillId);
                    }
                }
            }
        }
        public void PreLoadSceneNpcSkills(int sceneid)
        {
            MapDataProvider mdp = SceneConfigProvider.Instance.GetMapDataBySceneResId(sceneid);
            if (mdp != null)
            {
                DataDictionaryMgr<Data_Unit> ddm = mdp.m_UnitMgr;
                if (ddm != null)
                {
                    MyDictionary<int, object> dataDic = ddm.GetData();
                    if (dataDic != null)
                    {
                        List<int> linkidlist = new List<int>();
                        linkidlist.Clear();
                        List<int> skilllist = new List<int>();
                        skilllist.Clear();
                        foreach (object obj in dataDic.Values)
                        {
                            Data_Unit du = obj as Data_Unit;
                            if (du != null)
                            {
                                if (!linkidlist.Contains(du.m_LinkId))
                                {
                                    linkidlist.Add(du.m_LinkId);
                                }
                            }
                        }
                        foreach (int linkid in linkidlist)
                        {
                            Data_NpcConfig dnc = NpcConfigProvider.Instance.GetNpcConfigById(linkid);
                            if (dnc != null && dnc.m_SkillList != null)
                            {
                                foreach (int skillid in dnc.m_SkillList)
                                {
                                    if (!skilllist.Contains(skillid))
                                    {
                                        skilllist.Add(skillid);
                                    }
                                }
                            }
                        }
                        foreach (int skillid in skilllist)
                        {
                            PreloadNewSkillInstance(skillid);
                        }
                    }
                }
            }
        }
        public void Reset()
        {
            int count = m_SkillLogicInfos.Count;
            for (int index = count - 1; index >= 0; --index)
            {
                SkillLogicInfo info = m_SkillLogicInfos[index];
                if (null != info)
                {
                    info.SkillInst.OnSkillStop(info.Sender, 0);
                    StopSkillInstance(info);
                    m_SkillLogicInfos.RemoveAt(index);
                }
            }
            m_SkillLogicInfos.Clear();
        }
        public void PreloadSkillInstance(int skillId)
        {
            SkillInstanceInfo info = NewSkillInstance(skillId);
            if (null != info)
            {
                RecycleSkillInstance(info);
            }
        }
        public void ClearSkillInstancePool()
        {
            m_SkillInstancePool.Clear();
        }

        public void PushSkill(GameObject obj, SkillCategory category, UnityEngine.Vector3 targetpos)
        {
            LogicSystem.NotifyGfxStartSkill(obj, category, targetpos);
        }

        public void BreakSkill(GameObject obj, SkillCategory category)
        {
            LogicSystem.NotifyGfxBreakSkill(obj, category);
        }

        public void ChangeSkillControlMode(GameObject obj, SkillControlMode mode)
        {
            if (obj == null)
            {
                return;
            }
            LogicSystem.NotifyGfxChangeSkillControlMode(obj, mode);
        }

        public void StartAttack(GameObject obj, UnityEngine.Vector3 targetpos)
        {
            LogicSystem.NotifyGfxStartAttack(obj, targetpos.x, targetpos.y, targetpos.z);
        }

        public void StopAttack(GameObject obj)
        {
            LogicSystem.NotifyGfxStopAttack(obj);
        }

        //用于上层逻辑检查通过后调用
        public void StartSkill(int actorId, int skillId, float facedir)
        {
            GameObject obj = LogicSystem.GetGameObject(actorId);
            if (null != obj)
            {
                SkillLogicInfo logicInfo = m_SkillLogicInfos.Find(info => info.Sender == obj && info.SkillId == skillId);
                if (logicInfo != null)
                {
                    return;
                }
                ChangeDir(obj, facedir);
                SkillInstanceInfo inst = NewSkillInstance(skillId);
                if (null != inst)
                {
                    m_SkillLogicInfos.Add(new SkillLogicInfo(obj, inst));
                }
                else
                {
                    LogicSystem.NotifyGfxStopSkill(obj, skillId);
                    return;
                }

                logicInfo = m_SkillLogicInfos.Find(info => info.Sender == obj && info.SkillId == skillId);
                if (null != logicInfo)
                {
                    if (OnGfxShillStart != null)
                    {
                        OnGfxShillStart(obj, skillId);
                    }
                    LogicSystem.NotifyGfxAnimationStart(obj, true);
                    logicInfo.SkillInst.Start(logicInfo.Sender);
                }
            }
        }
        //在技能未开始时取消技能（用于上层逻辑检查失败时）
        public void CancelSkill(int actorId, int skillId)
        {
            GameObject obj = LogicSystem.GetGameObject(actorId);
            if (null != obj)
            {
                SkillLogicInfo logicInfo = m_SkillLogicInfos.Find(info => info.Sender == obj && info.SkillId == skillId);
                if (null != logicInfo)
                {
                    if (logicInfo.SkillInst.IsControlMove)
                    {
                        LogicSystem.NotifyGfxMoveControlFinish(obj, skillId, true);
                        logicInfo.SkillInst.IsControlMove = false;
                    }
                    LogicSystem.NotifyGfxAnimationFinish(obj, true);
                    RecycleSkillInstance(logicInfo.Info);
                    m_SkillLogicInfos.Remove(logicInfo);
                }
            }
        }
        public void StopSkill(int actorId, bool isinterrupt)
        {
            GameObject obj = LogicSystem.GetGameObject(actorId);
            if (null == obj)
            {
                return;
            }
            int count = m_SkillLogicInfos.Count;
            for (int index = count - 1; index >= 0; --index)
            {
                SkillLogicInfo info = m_SkillLogicInfos[index];
                if (info != null)
                {
                    if (info.Sender == obj)
                    {
                        if (isinterrupt)
                        {
                            info.SkillInst.OnInterrupt(obj, 0);
                        }
                        else
                        {
                            info.SkillInst.OnSkillStop(obj, 0);
                        }
                        StopSkillInstance(info, isinterrupt);
                        m_SkillLogicInfos.RemoveAt(index);
                    }
                }
            }
        }
        public void SendMessage(int actorId, int skillId, string msgId)
        {
            GameObject obj = LogicSystem.GetGameObject(actorId);
            if (null != obj)
            {
                SkillLogicInfo logicInfo = m_SkillLogicInfos.Find(info => info.Sender == obj && info.SkillId == skillId);
                if (null != logicInfo && null != logicInfo.SkillInst)
                {
                    logicInfo.SkillInst.SendMessage(msgId);
                }
            }
        }
        public void Tick()
        {
            try
            {
                Profiler.BeginSample("GfxSkillSystem.Tick");
                int ct = m_SkillLogicInfos.Count;
                long delta = (long)(Time.deltaTime * 1000 * 1000);
                for (int ix = ct - 1; ix >= 0; --ix)
                {
                    SkillLogicInfo info = m_SkillLogicInfos[ix];
                    bool exist = LogicSystem.ExistGameObject(info.Sender);
                    if (exist)
                    {
                        info.SkillInst.Tick(info.Sender, delta);
                    }
                    if (!exist || info.SkillInst.IsFinished)
                    {
                        if (!exist)
                        {
                            info.SkillInst.OnSkillStop(info.Sender, 0);
                        }
                        StopSkillInstance(info);
                        m_SkillLogicInfos.RemoveAt(ix);
                    }
                }

                int product_count = m_SkillProducts.Count;
                for (int i = product_count - 1; i >= 0; --i)
                {
                    ISkillProduct product = m_SkillProducts[i];
                    product.Tick(delta);
                    if (product.IsStoped())
                    {
                        m_SkillProducts.RemoveAt(i);
                    }
                }
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        public void AddSkillProduct(ISkillProduct product)
        {
            m_SkillProducts.Add(product);
        }

        private void StopSkillInstance(SkillLogicInfo info)
        {
            StopSkillInstance(info, false);
        }

        private void StopSkillInstance(SkillLogicInfo info, bool isInterrupt)
        {
            //LogSystem.Debug("Skill {0} finished.", info.SkillId);
            if (!isInterrupt)
            {
                if (info.SkillInst.IsControlMove)
                {
                    LogicSystem.NotifyGfxMoveControlFinish(info.Sender, info.SkillId, true);
                    info.SkillInst.IsControlMove = false;
                }
                LogicSystem.NotifyGfxAnimationFinish(info.Sender, true);
                LogicSystem.NotifyGfxStopSkill(info.Sender, info.SkillId);
            }
            else
            {
                if (info.SkillInst.IsControlMove)
                {
                    info.SkillInst.IsControlMove = false;
                }
            }
            RecycleSkillInstance(info.Info);
        }

        public void OnCollider(GameObject one, GameObject two)
        {

        }

        public static void ChangeDir(GameObject obj, float direction)
        {
            UnityEngine.Vector3 rotate = new UnityEngine.Vector3(0, direction * 180 / UnityEngine.Mathf.PI, 0);
            obj.transform.eulerAngles = rotate;
            LogicSystem.NotifyGfxUpdatePosition(obj, obj.transform.position.x, obj.transform.position.y,
                                                obj.transform.position.z, 0, direction, 0);
        }

        public static void ChangeAllDir(GameObject obj, float direction)
        {
            UnityEngine.Vector3 rotate = new UnityEngine.Vector3(0, direction * 180 / UnityEngine.Mathf.PI, 0);
            obj.transform.eulerAngles = rotate;
            LogicSystem.NotifyGfxUpdatePositionAndDir(obj, obj.transform.position.x, obj.transform.position.y,
                                                obj.transform.position.z, 0, direction, 0);
        }

        public static void ChangeDir(GameObject obj, UnityEngine.Vector3 dir)
        {
            dir.y = 0;
            obj.transform.forward = dir;
            UnityEngine.Vector3 rotate = obj.transform.rotation.eulerAngles;
            LogicSystem.NotifyGfxUpdatePosition(obj, obj.transform.position.x, obj.transform.position.y,
                                                obj.transform.position.z, 0, rotate.y * UnityEngine.Mathf.PI / 180, 0);
        }

        private void PreloadNewSkillInstance(int skillId)
        {
            SkillLogicData skillData = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skillId) as SkillLogicData;
            if (null != skillData)
            {
                string filePath = HomePath.GetAbsolutePath(FilePathDefine_Client.C_SkillDslPath + skillData.SkillDataFile);
                SkillConfigManager.Instance.LoadSkillIfNotExist(skillId, filePath);
                SkillInstance inst = SkillConfigManager.Instance.NewSkillInstance(skillId);
                if (inst != null)
                {
                    SkillInstanceInfo res = new SkillInstanceInfo();
                    res.m_SkillId = skillId;
                    res.m_SkillInstance = inst;
                    res.m_IsUsed = true;
                    AddSkillInstanceInfoToPool(skillId, res);
                    RecycleSkillInstance(res);
                }
            }
        }


        private SkillInstanceInfo NewSkillInstance(int skillId)
        {
            SkillInstanceInfo instInfo = GetUnusedSkillInstanceInfoFromPool(skillId);
            if (null == instInfo)
            {
                SkillLogicData skillData = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skillId) as SkillLogicData;
                if (null != skillData)
                {
                    string filePath = HomePath.GetAbsolutePath(FilePathDefine_Client.C_SkillDslPath + skillData.SkillDataFile);
                    SkillConfigManager.Instance.LoadSkillIfNotExist(skillId, filePath);
                    SkillInstance inst = SkillConfigManager.Instance.NewSkillInstance(skillId);

                    if (inst == null)
                    {
                        LogSystem.Error("Can't load skill config, skill:{0} !", skillId);
                        return null;
                    }
                    SkillInstanceInfo res = new SkillInstanceInfo();
                    res.m_SkillId = skillId;
                    res.m_SkillInstance = inst;
                    res.m_IsUsed = true;

                    AddSkillInstanceInfoToPool(skillId, res);
                    return res;
                }
                else
                {
                    LogSystem.Error("Can't find skill config, skill:{0} !", skillId);
                    return null;
                }
            }
            else
            {
                instInfo.m_IsUsed = true;
                return instInfo;
            }
        }
        private void RecycleSkillInstance(SkillInstanceInfo info)
        {
            info.m_SkillInstance.Reset();
            info.m_IsUsed = false;
        }
        private void AddSkillInstanceInfoToPool(int skillId, SkillInstanceInfo info)
        {
            List<SkillInstanceInfo> infos;
            if (m_SkillInstancePool.TryGetValue(skillId, out infos))
            {
                infos.Add(info);
            }
            else
            {
                infos = new List<SkillInstanceInfo>();
                infos.Add(info);
                m_SkillInstancePool.Add(skillId, infos);
            }
        }
        private SkillInstanceInfo GetUnusedSkillInstanceInfoFromPool(int skillId)
        {
            SkillInstanceInfo info = null;
            List<SkillInstanceInfo> infos;
            if (m_SkillInstancePool.TryGetValue(skillId, out infos))
            {
                int ct = infos.Count;
                for (int ix = 0; ix < ct; ++ix)
                {
                    if (!infos[ix].m_IsUsed)
                    {
                        info = infos[ix];
                        break;
                    }
                }
            }
            return info;
        }

        private List<SkillLogicInfo> m_SkillLogicInfos = new List<SkillLogicInfo>();
        private Dictionary<int, List<SkillInstanceInfo>> m_SkillInstancePool = new Dictionary<int, List<SkillInstanceInfo>>();
        private List<ISkillProduct> m_SkillProducts = new List<ISkillProduct>();

        public static GfxSkillSystem Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static GfxSkillSystem s_Instance = new GfxSkillSystem();
    }
}
