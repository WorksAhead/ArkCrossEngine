using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    internal class GameLogicNotification : IGameLogicNotification
    {
        public void OnGfxStartStory(int id)
        {
            if (WorldSystem.Instance.IsPveScene())
            {
                ClientScriptSystem.Instance.StartStory(id);
            }
            else
            {
                // OnGfxStartStory 只会在单人pve场景中被调用
            }
        }
        public void OnGfxSendStoryMessage(string msgId, object[] args)
        {
            if (WorldSystem.Instance.IsPureClientScene() || WorldSystem.Instance.IsPveScene())
            {
                ClientScriptSystem.Instance.SendMessage(msgId, args);
            }
            else
            {
                //通知服务器
                string msgIdPrefix = "dialogover:";
                if (msgId.StartsWith(msgIdPrefix))
                {
                    ArkCrossEngineMessage.Msg_CR_DlgClosed msg = new ArkCrossEngineMessage.Msg_CR_DlgClosed();
                    msg.dialog_id = int.Parse(msgId.Substring(msgIdPrefix.Length));
                    Network.NetworkSystem.Instance.SendMessage(msg);
                }
            }
        }
        public void OnGfxControlMoveStart(int objId, int id, bool isSkill)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(objId);
            if (null != charObj)
            {
                charObj.GetMovementStateInfo().IsSkillMoving = true;
                if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
                {
                    if (objId == WorldSystem.Instance.PlayerSelfId || charObj.OwnerId == WorldSystem.Instance.PlayerSelfId)
                    {
                        Network.NetworkSystem.Instance.SyncGfxMoveControlStart(charObj, id, isSkill);
                    }
                }
            }
        }
        public void OnGfxControlMoveStop(int objId, int id, bool isSkill)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(objId);
            if (null != charObj)
            {
                charObj.GetMovementStateInfo().IsSkillMoving = false;
                if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
                {
                    if (objId == WorldSystem.Instance.PlayerSelfId || charObj.OwnerId == WorldSystem.Instance.PlayerSelfId)
                    {
                        Network.NetworkSystem.Instance.SyncGfxMoveControlStop(charObj, id, isSkill);
                    }
                }
            }
        }
        public void OnGfxHitTarget(int id, int impactId, int targetId, int hitCount,
                                   int skillId, int duration, float x,
                                   float y, float z, float dir, long hit_count_id)
        {
            CharacterInfo sender = WorldSystem.Instance.GetCharacterById(id);
            CharacterInfo target = WorldSystem.Instance.GetCharacterById(targetId);
            UserInfo playerSelf = WorldSystem.Instance.GetPlayerSelf();
            bool hitCountChanged = false;
            bool playerHitCountChange = false;
            long curTime = TimeUtility.GetLocalMilliseconds();
            UpdateHitTalent(sender, hit_count_id);
            // 攻击
            if (hitCount > 0)
            {
                CombatStatisticInfo senderCombatInfo = sender.GetCombatStatisticInfo();
                int last_count = senderCombatInfo.MultiHitCount;
                if (IsImpactHaveDamage(impactId))
                {
                    hitCountChanged = senderCombatInfo.UpdateMultiHitCount(hit_count_id, hitCount, curTime);
                }
                if (id == WorldSystem.Instance.PlayerSelfId && null != playerSelf)
                {
                    if (senderCombatInfo.MultiHitCount >= 1 && last_count != senderCombatInfo.MultiHitCount)
                    {
                        GfxSystem.PublishGfxEvent("ge_hitcount", "ui", senderCombatInfo.MultiHitCount);
                        playerHitCountChange = hitCountChanged;
                    }
                }
                CombatStatisticInfo targetCombatInfo = target.GetCombatStatisticInfo();
                targetCombatInfo.HitCount += hitCount;
                if (targetId == WorldSystem.Instance.PlayerSelfId && null != playerSelf)
                {
                    if (WorldSystem.Instance.IsELiteScene())
                    {
                        RoleInfo roleInfo = LobbyClient.Instance.CurrentRole;
                        SceneResource curScene = WorldSystem.Instance.GetCurScene();
                        if (null != roleInfo && null != curScene && roleInfo.GetSceneInfo(WorldSystem.Instance.GetCurSceneId()) == 2)
                        { //当前在挑战3星通关
                            GfxSystem.PublishGfxEvent("ge_pve_fightinfo", "ui", 0, targetCombatInfo.HitCount, curScene.SceneConfig.m_CompletedHitCount, 0);
                        }
                        playerHitCountChange = true;
                    }
                }
            }
            if (playerHitCountChange && null != playerSelf && (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene()))
            {
                CombatStatisticInfo combatInfo = playerSelf.GetCombatStatisticInfo();
                ArkCrossEngineMessage.Msg_CR_HitCountChanged msg = new ArkCrossEngineMessage.Msg_CR_HitCountChanged();
                msg.max_multi_hit_count = combatInfo.MaxMultiHitCount;
                msg.hit_count = combatInfo.HitCount;
                Network.NetworkSystem.Instance.SendMessage(msg);
            }
            if (null != sender && null != target)
            {
                int hit_count = 0;
                hit_count = sender.GetCombatStatisticInfo().MultiHitCount;
                OnGfxStartImpact(sender.GetId(), impactId, target.GetId(), skillId, duration, new Vector3(x, y, z), dir,
                  hit_count);
            }
        }

        private void UpdateHitTalent(CharacterInfo character, long hit_count_id)
        {
            if (character == null)
            {
                return;
            }
            HitAddDamageRate attr = character.TalentManager.GetTalentAttribute(AttributeId.kHitAddDamageRate) as HitAddDamageRate;
            if (attr != null)
            {
                attr.OnHit(hit_count_id);
            }
        }

        public void OnGfxMoveMeetObstacle(int id)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj)
            {
                charObj.GetMovementStateInfo().IsMoveMeetObstacle = true;
                WorldSystem.Instance.NotifyMoveMeetObstacle(false);
            }
        }

        public void OnGfxStartImpact(int sender, int impactId, int target, int skillId, int duration, Vector3 srcPos, float dir, int hit_count)
        {
            CharacterInfo senderObj = WorldSystem.Instance.GetCharacterById(sender);
            if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
            {
                if (null != senderObj)
                {
                    bool isSend = false;
                    if (senderObj.GetId() == WorldSystem.Instance.PlayerSelfId)
                    {
                        isSend = true;
                    }
                    if (senderObj is NpcInfo)
                    {
                        if (senderObj.OwnerId == WorldSystem.Instance.GetPlayerSelf().GetId())
                        {
                            isSend = true;
                        }
                    }
                    if (isSend)
                    {
                        bool ret = ImpactSystem.Instance.SendImpactToCharacter(senderObj, impactId, target, skillId, duration, srcPos, dir);
                        if (ret)
                            Network.NetworkSystem.Instance.SyncSendImpact(senderObj, impactId, target, skillId, duration, srcPos, dir, hit_count);
                    }
                }
            }
            else
            {
                bool ret = ImpactSystem.Instance.SendImpactToCharacter(senderObj, impactId, target, skillId, duration, srcPos, dir);
            }
        }

        public void OnGfxStartSkill(int id, SkillCategory category, float x, float y, float z)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj)
            {
                if (charObj.SkillController != null)
                {
                    charObj.SkillController.PushSkill(category, new Vector3(x, y, z));
                }
                //LogSystem.Debug("OnGfxStartSkill");
            }
        }

        public void OnGfxBreakSkill(int id, SkillCategory category)
        {
            try
            {
                CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
                if (null != charObj && charObj.SkillController != null)
                {
                    charObj.SkillController.BreakSkill(category);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void OnGfxForceStartSkill(int id, int skillId)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj)
            {
                if (charObj.SkillController != null)
                {
                    charObj.SkillController.ForceStartSkill(skillId);
                }
                //LogSystem.Debug("OnGfxStartSkill");
            }
        }

        public void OnGfxStopSkill(int id, int skillId)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj)
            {
                if ((WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene()) && skillId != charObj.Combat2IdleSkill)
                {
                    if (charObj.GetId() == WorldSystem.Instance.PlayerSelfId || charObj.OwnerId == WorldSystem.Instance.PlayerSelfId)
                    {
                        //LogSystem.Debug("---ongfxstopskill id={0}, skillid=", id, skillId);
                        Network.NetworkSystem.Instance.SyncPlayerStopSkill(charObj, skillId);
                    }
                }
                if (skillId == charObj.Combat2IdleSkill)
                {
                    CharacterView userview = EntityManager.Instance.GetCharacterViewById(id);
                    if (userview != null)
                    {
                        userview.OnCombat2IdleSkillOver();
                    }
                }
                SkillInfo skillInfo = charObj.GetSkillStateInfo().GetSkillInfoById(skillId);
                if (null != skillInfo)
                {
                    skillInfo.IsSkillActivated = false;
                    //LogSystem.Debug("-----OnGfxStopSkill " + skillId);
                }
            }
        }

        public void OnGfxStartAttack(int id, float x, float y, float z)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj)
            {
                charObj.SkillController.StartAttack(new Vector3(x, y, z));
            }
        }

        public void OnGfxStopAttack(int id)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj)
            {
                charObj.SkillController.StopAttack();
            }
        }

        public void OnGfxSkillBreakSection(int objid, int skillid, int breaktype, int startime, int endtime, bool isinterrupt, string skillmessage)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(objid);
            if (null != charObj)
            {
                charObj.SkillController.AddBreakSection(skillid, breaktype, startime, endtime, isinterrupt, skillmessage);
            }
        }

        public void OnGfxRemoveSkillBreakSection(int objid, int skillid, int breaktype)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(objid);
            if (null != charObj)
            {
                charObj.SkillController.RemoveBreakSectionByType(skillid, breaktype);
            }
        }

        public void OnGfxStopImpact(int id, int impactId)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj)
            {
                ImpactSystem.Instance.OnGfxStopImpact(charObj, impactId);
            }
        }

        public void OnGfxSetCrossFadeTime(int id, string fadeTargetAnim, float crossTime)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj && charObj.GetSkillStateInfo() != null)
            {
                if (fadeTargetAnim == "stand")
                {
                    charObj.GetSkillStateInfo().CrossToStandTime = crossTime;
                }
                else if (fadeTargetAnim == "run")
                {
                    charObj.GetSkillStateInfo().CrossToRunTime = crossTime;
                }
            }
        }

        public void OnGfxAddLockInputTime(int id, SkillCategory category, float lockinputtime)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (null != charObj && charObj.GetSkillStateInfo() != null)
            {
                SkillInfo curskill = charObj.GetSkillStateInfo().GetCurSkillInfo();
                if (curskill != null)
                {
                    curskill.AddLockInputTime(category, lockinputtime);
                }
            }
        }

        public void OnGfxSummonNpc(int owner_id, int owner_skillid, int npc_type_id, string modelPrefab, int skillid, int ailogicid, bool followsummonerdead,
                                   float x, float y, float z, string aiparamstr, int signforskill, bool isSimulate)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(owner_id);
            if (charObj == null)
            {
                return;
            }
            //LogSystem.Debug("----summon npc: ownerid={0}, playerselfid={1}", charObj.OwnerId, WorldSystem.Instance.PlayerSelfId);
            if (WorldSystem.Instance.IsMultiPveScene() &&
                ailogicid > 0)
            {
                if (charObj.OwnerId == WorldSystem.Instance.PlayerSelfId)
                {
                    ArkCrossEngineMessage.Msg_CRC_SummonNpc msg = new ArkCrossEngineMessage.Msg_CRC_SummonNpc();
                    msg.npc_id = -1;
                    msg.owner_id = -1;
                    msg.summon_owner_id = owner_id;
                    msg.owner_skillid = owner_skillid;
                    msg.link_id = npc_type_id;
                    msg.model_prefab = modelPrefab;
                    msg.skill_id = skillid;
                    msg.ai_id = ailogicid;
                    msg.follow_dead = followsummonerdead;
                    msg.pos_x = x;
                    msg.pos_y = y;
                    msg.pos_z = z;
                    msg.ai_params = aiparamstr;
                    Network.NetworkSystem.Instance.SendMessage(msg);
                    //LogSystem.Debug("----summon npc: send summon npc msg to server!");
                }
                return;
            }
            SummonNpc(-1, owner_id, owner_skillid, npc_type_id, modelPrefab, skillid,
                      ailogicid, followsummonerdead, x, y, z, aiparamstr, signforskill, isSimulate);
        }

        public static NpcInfo SummonNpc(int id, int owner_id, int owner_skillid, int npc_type_id,
                                        string modelPrefab, int skillid,
                                        int ailogicid, bool followsummonerdead,
                                        float x, float y, float z, string aiparamstr, int signforskill, bool isSimulate = false,
                                        bool is_start_skill = true)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(owner_id);
            if (charObj == null)
            {
                return null;
            }
            Data_Unit data = new Data_Unit();
            data.m_Id = -1;
            data.m_LinkId = npc_type_id;
            data.m_CampId = charObj.GetCampId();
            data.m_IsEnable = true;
            data.m_Pos = new Vector3(x, y, z);
            data.m_RotAngle = 0;
            data.m_AiLogic = ailogicid;
            if (!string.IsNullOrEmpty(aiparamstr))
            {
                string[] strarry = aiparamstr.Split(new char[] { ',' }, 8);
                int i = 0;
                foreach (string str in strarry)
                {
                    data.m_AiParam[i++] = str;
                }
            }
            NpcInfo npc;
            if (id <= 0)
            {
                npc = WorldSystem.Instance.NpcManager.AddNpc(data);
            }
            else
            {
                npc = WorldSystem.Instance.NpcManager.AddNpc(id, data);
            }
            if (WorldSystem.Instance.IsExpeditionScene() || WorldSystem.Instance.IsMultiPveScene())
            {
                float fightscore = 0;
                CharacterProperty info = charObj.GetActualProperty();
                if (null != info)
                {
                    CharacterProperty base_info = charObj.GetBaseProperty();
                    float assit_critical_pow = (null != base_info ? info.CriticalPow - base_info.CriticalPow : info.CriticalPow);
                    float assit_critical_backhit_pow = (null != base_info ? info.CriticalBackHitPow - base_info.CriticalBackHitPow : info.CriticalBackHitPow);
                    float assit_critical_crack_pow = (null != base_info ? info.CriticalCrackPow - base_info.CriticalCrackPow : info.CriticalCrackPow);
                    fightscore = AttributeScoreConfigProvider.Instance.CalcAttributeScore(info.HpMax,
                      info.EnergyMax, info.AttackBase, info.ADefenceBase, info.MDefenceBase, info.Critical,
                      assit_critical_pow, assit_critical_backhit_pow, assit_critical_crack_pow, info.FireDamage,
                      info.IceDamage, info.PoisonDamage, info.FireERD, info.IceERD, info.PoisonERD) + charObj.GetSkillStateInfo().GetSkillAppendScore();
                }
                WorldSystem.Instance.AdditionNpcAttr(npc, Mathf.FloorToInt(fightscore));
            }
            if (!string.IsNullOrEmpty(modelPrefab))
            {
                npc.SetModel(modelPrefab);
            }
            npc.FollowSummonerDead = followsummonerdead;
            SkillInfo skillinfo = new SkillInfo(skillid);
            npc.GetSkillStateInfo().AddSkill(skillinfo);
            npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
            npc.GetMovementStateInfo().SetPosition(data.m_Pos);
            npc.SummonOwnerId = charObj.GetId();
            npc.SignForSkill = signforskill;
            EntityManager.Instance.CreateNpcView(npc.GetId());
            charObj.GetSkillStateInfo().AddSummonObject(npc);
            NpcView npcview = EntityManager.Instance.GetNpcViewById(npc.GetId());
            CharacterView owner_view = EntityManager.Instance.GetCharacterViewById(charObj.GetId());
            npcview.ObjectInfo.SummonOwnerActorId = owner_view.Actor;
            npcview.ObjectInfo.SummonOwnerSkillId = owner_skillid;
            npcview.ObjectInfo.SignForSkill = signforskill;
            npcview.ObjectInfo.IsSimulate = isSimulate;
            if (ailogicid > 0)
            {
                npcview.ObjectInfo.IsSummonWithAI = true;
            }
            //LogSystem.Debug("---summon npc: issimulate=" + isSimulate);
            owner_view.ObjectInfo.Summons.Add(npcview.Actor);

            if (is_start_skill)
            {
                npc.SkillController.ForceStartSkill(skillid);
            }
            //LogSystem.Debug("---create summon npc over!");
            return npc;
        }
        public void OnGfxSonsReleaseSkill(int owner_id, int signskill, int skillid)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(owner_id);
            if (charObj != null)
            {
                List<NpcInfo> npclist = charObj.GetSkillStateInfo().GetSummonObject();
                if (npclist != null)
                {
                    int count = npclist.Count;
                    for (int i = 0; i < count; ++i)
                    {
                        NpcInfo ni = npclist[i];
                        if (ni != null && ni.SignForSkill == signskill && ni.SkillController != null)
                        {
                            //               List<SkillInfo> silist = ni.GetSkillStateInfo().GetAllSkill();
                            //               if (silist != null) {
                            //                 if (silist.Find(s => s.SkillId == skillid) == null) {
                            //                   SkillInfo skillinfo = new SkillInfo(skillid);
                            //                   ni.GetSkillStateInfo().AddSkill(skillinfo);
                            //                 }
                            ni.SkillController.ForceStartSkill(skillid);
                            //}
                        }
                    }
                }
            }
        }
        public void OnGfxDestroyObj(int id)
        {
            CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
            if (charObj == null)
            {
                return;
            }
            EntityManager.Instance.DestroyNpcView(charObj.GetId());
            WorldSystem.Instance.DestroyCharacterById(charObj.GetId());
        }

        public void OnGfxSetObjLifeTime(int id, long life_remaint_time)
        {
            NpcInfo npcinfo = WorldSystem.Instance.GetCharacterById(id) as NpcInfo;
            if (npcinfo == null)
            {
                return;
            }
            npcinfo.LifeEndTime = TimeUtility.GetServerMilliseconds() + life_remaint_time;
        }

        public void OnGfxDestroySummonObject(int id)
        {
            CharacterInfo character = WorldSystem.Instance.GetCharacterById(id);
            if (character == null)
            {
                return;
            }
            List<NpcInfo> summon_pool = character.GetSkillStateInfo().GetSummonObject();
            List<int> summon_id_list = new List<int>();
            foreach (NpcInfo so in summon_pool)
            {
                summon_id_list.Add(so.GetId());
            }
            foreach (int summon_id in summon_id_list)
            {
                EntityManager.Instance.DestroyNpcView(summon_id);
                WorldSystem.Instance.DestroyCharacterById(summon_id);
            }
            summon_pool.Clear();
            summon_id_list.Clear();
        }

        public void OnGfxSimulateMove(int id)
        {
            NpcInfo npc = WorldSystem.Instance.GetCharacterById(id) as NpcInfo;
            if (npc == null)
            {
                return;
            }
            if (npc.SummonOwnerId < 0)
            {
                return;
            }
            CharacterInfo owner = WorldSystem.Instance.GetCharacterById(npc.SummonOwnerId);
            if (owner == null)
            {
                return;
            }
            CharacterView owner_view = EntityManager.Instance.GetCharacterViewById(npc.SummonOwnerId);
            if (owner_view == null)
            {
                return;
            }
            npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, owner.GetActualProperty().MoveSpeed);
            npc.VelocityCoefficient = owner.VelocityCoefficient;
            npc.Combat2IdleSkill = owner.Combat2IdleSkill;
            npc.Combat2IdleTime = owner.Combat2IdleTime;
            npc.Idle2CombatWeaponMoves = owner.Idle2CombatWeaponMoves;
            List<SkillInfo> skills = owner.GetSkillStateInfo().GetAllSkill();
            npc.GetSkillStateInfo().AddSkill(new SkillInfo(npc.Combat2IdleSkill));
            foreach (SkillInfo si in skills)
            {
                npc.GetSkillStateInfo().AddSkill(new SkillInfo(si.SkillId));
            }
            npc.SkillController.Init();
            npc.IsSimulateMove = true;
        }

        public void OnGfxChangeSkillControlMode(int id, SkillControlMode mode)
        {
            CharacterInfo character = WorldSystem.Instance.GetCharacterById(id);
            if (character != null && character.SkillController != null)
            {
                character.SkillController.SetSkillControlMode(mode);
            }
        }

        public void OnGfxForbidNextSkill(int logicid)
        {
            CharacterInfo owner = WorldSystem.Instance.GetCharacterById(logicid);
            if (owner != null)
            {
                SkillInfo skillinfo = owner.GetSkillStateInfo().GetCurSkillInfo();
                if (skillinfo != null)
                {
                    skillinfo.IsForbidNextSkill = true;
                }
            }
        }

        private bool IsImpactHaveDamage(int impactid)
        {
            ImpactLogicData impact_config = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_IMPACT, impactid) as ImpactLogicData;
            if (impact_config == null)
            {
                return false;
            }
            if (impact_config.DamageRate > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static GameLogicNotification Instance
        {
            get { return s_Instance; }
        }
        private static GameLogicNotification s_Instance = new GameLogicNotification();
    }
}
