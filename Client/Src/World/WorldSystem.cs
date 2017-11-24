using System;
using System.Collections.Generic;
using System.Text;
using ArkCrossEngine.Network;
using ArkCrossEngineMessage;
using ArkCrossEngineSpatial;

namespace ArkCrossEngine
{
    internal class WorldSystemProfiler
    {
        internal long sceneTickTime;
        internal long entityMgrTickTime;
        internal long controlSystemTickTime;
        internal long movementSystemTickTime;
        internal long spatialSystemTickTime;
        internal long aiSystemTickTime;
        internal long sceneLogicSystemTickTime;
        internal long storySystemTickTime;
        internal long usersTickTime;
        internal long npcsTickTime;
        internal long combatSystemTickTime;

        internal string GenerateLogString(long tickTime)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("WorldSystem.Tick consume ").Append(tickTime).AppendLine();

            builder.Append("=>sceneTickTime:").Append(sceneTickTime).AppendLine();
            builder.Append("=>entityMgrTickTime:").Append(entityMgrTickTime).AppendLine();
            builder.Append("=>controlSystemTickTime:").Append(controlSystemTickTime).AppendLine();
            builder.Append("=>movementSystemTickTime:").Append(movementSystemTickTime).AppendLine();
            builder.Append("=>spatialSystemTickTime:").Append(spatialSystemTickTime).AppendLine();
            builder.Append("=>aiSystemTickTime:").Append(aiSystemTickTime).AppendLine();
            builder.Append("=>sceneLogicSystemTickTime:").Append(sceneLogicSystemTickTime).AppendLine();
            builder.Append("=>storySystemTickTime:").Append(storySystemTickTime).AppendLine();
            builder.Append("=>usersTickTime:").Append(usersTickTime).AppendLine();
            builder.Append("=>npcsTickTime:").Append(npcsTickTime).AppendLine();
            builder.Append("=>combatSystemTickTime:").Append(combatSystemTickTime).AppendLine();

            return builder.ToString();
        }
    }
    /**
     * @brief 游戏系统
     */
    public partial class WorldSystem
    {

        /**
         * @brief Singleton
         *
         * @return 
         */
        #region Singleton
        private static WorldSystem s_Instance = new WorldSystem();
        public static WorldSystem Instance
        {
            get { return s_Instance; }
        }

        #endregion

        //---------------------------------------------------------
        // 标准接口
        //---------------------------------------------------------

        /**
         * @brief 初始化
         *
         * @return 
         */
        internal void Init()
        {
            m_IsObserver = false;
            m_CurScene = null;

            GfxSystem.EventChannelForLogic.Subscribe("ge_change_hero", "game", ChangeHeroFromGfx);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_change_player_movemode", "game", ChangePlayerMoveMode);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_change_npc_movemode", "game", ChangeNpcMoveMode);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_change_scene", "game", ChangeSceneFromGfx);
            GfxSystem.EventChannelForLogic.Subscribe("ge_resetdsl", "game", ResetDsl);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_execscript", "game", ExecScript);
            GfxSystem.EventChannelForLogic.Subscribe<string>("ge_execcommand", "game", ExecCommand);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_switch_actorid", "game", SwitchUnitId2ActorId);
            GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_switch_combatstate", "game", SwitchCombatState);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_set_story_state", "game", SetStoryState);
            GfxSystem.EventChannelForLogic.Subscribe<int>("ge_area_clear", "game", OnAreaClear);

            NpcManager.OnDamage = new DamageDelegation(NpcManager_OnDamage);
            UserManager.OnDamage = new DamageDelegation(UserManager_OnDamage);
            UserManager.OnGainMoney = new GainMoneyDelegation(UserManager_GainMoney);

            ResUpdateHandler.HandleGetPlayerCurSkillInfo = GetPlayerInfoForCache;
        }
        public bool GetLookAt(out float x, out float y, out float z)
        {
            bool ret = false;
            if (null != m_CurScene)
            {
                ret = true;
                m_CurScene.GetLookAt(out x, out y, out z);
            }
            else
            {
                x = y = z = 0;
            }
            return ret;
        }

        private void AddToDamageData(bool isOrdinaryDamage, bool isCritical, int hpDamage, int npDamage, Vector3 pos, bool isMonster = false, int hp = 0, int maxHp = 0, string name = "")
        {
            DamageData dD = new DamageData();
            dD.m_Id = m_CurIdx;
            dD.m_IsMonster = isMonster;
            dD.m_IsCritical = isCritical;
            dD.m_IsOrdinaryDamage = isOrdinaryDamage;
            dD.m_Pos = pos;
            dD.m_HpDamage = hpDamage;
            dD.m_NpDamage = npDamage;
            dD.m_Hp = hp;
            dD.m_MaxHp = maxHp;
            dD.m_Name = name;
            m_DamageDatas.AddLast(m_CurIdx++, dD);
            if (m_CurIdx >= m_MaxIdx)
            {
                m_CurIdx = 0;
            }
        }
        private void PublishGfxEventToDamageData(bool isOrdinaryDamage, bool isCritical, int hpDamage, int npDamage, Vector3 pos, bool isMonster, int hp, int maxHp, string name)
        {
            if (isCritical)
            {
                GfxSystem.PublishGfxEvent("ge_npc_cdamage", "ui", pos.X, pos.Y, pos.Z, hpDamage, isOrdinaryDamage);
            }
            else
            {
                GfxSystem.PublishGfxEvent("ge_npc_odamage", "ui", pos.X, pos.Y, pos.Z, hpDamage, isOrdinaryDamage);
            }
            if (isMonster)
            {
                GfxSystem.PublishGfxEvent("ge_small_monster_healthbar", "ui", hp, maxHp, hpDamage, name);
            }
        }

        private void TickDamageDatas()
        {
            m_CurTime += 1;
            if (m_CurTime < m_Speed)
            {
                return;
            }
            m_CurTime = 0;
            for (LinkedListNode<DamageData> node = m_DamageDatas.FirstValue; null != node;)
            {
                DamageData dD = node.Value;
                m_CurPush++;
                PublishGfxEventToDamageData(dD.m_IsOrdinaryDamage, dD.m_IsCritical, dD.m_HpDamage, dD.m_NpDamage, dD.m_Pos, dD.m_IsMonster, dD.m_Hp, dD.m_MaxHp, dD.m_Name);
                m_DamageDatas.Remove(dD.m_Id);
                if (m_CurPush >= m_MaxPush)
                {
                    m_CurPush = 0;
                    break;
                }
                node = node.Next;
            }
        }

        private void UserManager_OnDamage(int receiver, int caster, bool /*isShootDamage*/isOrdinaryDamage, bool isCritical, int hpDamage, int npDamage)
        {
            CharacterInfo caster_info = WorldSystem.Instance.GetCharacterById(caster);
            if (IsPvpScene())
            {
                if (caster == PlayerSelfId || receiver == PlayerSelfId || caster == m_PlayerSelf.PartnerId)
                {
                    UserInfo charObj = WorldSystem.Instance.GetCharacterById(receiver) as UserInfo;
                    if (null != charObj)
                    {
                        Vector3 pos = charObj.GetMovementStateInfo().GetPosition3D();
                        if (hpDamage <= 0)
                        {
                            /*if (isCritical) {
                              GfxSystem.PublishGfxEvent("ge_npc_cdamage", "ui", pos.X, pos.Y, pos.Z, hpDamage, isOrdinaryDamage);
                            } else {
                              GfxSystem.PublishGfxEvent("ge_npc_odamage", "ui", pos.X, pos.Y, pos.Z, hpDamage, isOrdinaryDamage);
                            }
                            */
                            NpcInfo npcObj = WorldSystem.Instance.GetCharacterById(caster) as NpcInfo;
                            if (npcObj != null)
                            {
                                if (npcObj.NpcType == (int)NpcTypeEnum.Partner)
                                {
                                    AddToDamageData(false, isCritical, hpDamage, npDamage, pos);
                                }
                                else
                                {
                                    AddToDamageData(true, isCritical, hpDamage, npDamage, pos);
                                }
                            }
                            else
                            {
                                AddToDamageData(true, isCritical, hpDamage, npDamage, pos);
                            }
                        }
                        else
                        {
                            GfxSystem.PublishGfxEvent("ge_hero_blood", "ui", pos.X, pos.Y, pos.Z, hpDamage);
                        }
                        if (npDamage > 0)
                        {
                            GfxSystem.PublishGfxEvent("ge_hero_energy", "ui", pos.X, pos.Y, pos.Z, npDamage);
                        }
                    }
                }
            }
            else
            {
                if (receiver == PlayerSelfId || caster == PlayerSelfId || (caster_info != null && caster_info.OwnerId == PlayerSelfId))
                {
                    UserInfo charObj = WorldSystem.Instance.GetCharacterById(receiver) as UserInfo;
                    if (null != charObj)
                    {
                        if (receiver == PlayerSelfId && (IsPveScene() || IsPureClientScene()))
                        {
                            if (charObj.IsHaveStoryFlag(StoryListenFlagEnum.Damage))
                            {
                                ClientStorySystem.Instance.SendMessage("objdamage", receiver, caster, hpDamage, npDamage, isCritical ? 1 : 0);
                                ClientStorySystem.Instance.SendMessage("playerselfdamage", receiver, caster, hpDamage, npDamage, isCritical ? 1 : 0);
                            }
                        }

                        Vector3 pos = charObj.GetMovementStateInfo().GetPosition3D();
                        if (hpDamage != 0)
                        {
                            GfxSystem.PublishGfxEvent("ge_hero_blood", "ui", pos.X, pos.Y, pos.Z, hpDamage);
                        }
                        if (npDamage != 0)
                        {
                            GfxSystem.PublishGfxEvent("ge_hero_energy", "ui", pos.X, pos.Y, pos.Z, npDamage);
                        }
                    }
                }
            }
        }
        private void NpcManager_OnDamage(int receiver, int caster, bool /*isShootDamage*/isOrdinaryDamage, bool isCritical, int hpDamage, int npDamage)
        {
            CharacterInfo caster_info = WorldSystem.Instance.GetCharacterById(caster);
            if (caster == PlayerSelfId || (caster_info != null && caster_info.OwnerId == PlayerSelfId))
            {
                NpcInfo charObj = NpcManager.GetNpcInfo(receiver);
                if (null != charObj && (IsPveScene() || IsPureClientScene()))
                {
                    if (charObj.IsHaveStoryFlag(StoryListenFlagEnum.Damage))
                    {
                        ClientStorySystem.Instance.SendMessage("objdamage", receiver, caster, hpDamage, npDamage, isCritical ? 1 : 0);
                        ClientStorySystem.Instance.SendMessage("npcdamage:" + charObj.GetUnitId(), receiver, caster, hpDamage, npDamage, isCritical ? 1 : 0);
                    }

                    int estimateDamage = hpDamage;
                    if (isCritical)
                        estimateDamage /= 2;

                    if (charObj.Hp + estimateDamage <= 0)
                    {
                        if (GetBattleNpcCount() == 1)
                        {
                            ClientStorySystem.Instance.SendMessage("finalblow", receiver);
                        }
                    }
                }

                if (null != charObj && (int)NpcTypeEnum.SceneObject != charObj.NpcType)
                {
                    Vector3 pos = charObj.GetMovementStateInfo().GetPosition3D();
                    /*if (isCritical) {
                      GfxSystem.PublishGfxEvent("ge_npc_cdamage", "ui", pos.X, pos.Y, pos.Z, hpDamage, isOrdinaryDamage);
                    } else {
                      GfxSystem.PublishGfxEvent("ge_npc_odamage", "ui", pos.X, pos.Y, pos.Z, hpDamage, isOrdinaryDamage);
                    }
                    GfxSystem.PublishGfxEvent("ge_small_monster_healthbar", "ui", charObj.Hp, charObj.GetActualProperty().HpMax, hpDamage);
                    */
                    NpcInfo npcObj = WorldSystem.Instance.GetCharacterById(caster) as NpcInfo;
                    if (npcObj != null)
                    {
                        if (npcObj.NpcType == (int)NpcTypeEnum.Partner)
                        {
                            AddToDamageData(false, isCritical, hpDamage, npDamage, pos);
                        }
                        else
                        {
                            AddToDamageData(true, isCritical, hpDamage, npDamage, pos);
                        }
                    }
                    else
                    {
                        AddToDamageData(true, isCritical, hpDamage, npDamage, pos, true, charObj.Hp, charObj.GetActualProperty().HpMax, charObj.GetName());
                    }
                }
            }
        }

        private void UserManager_GainMoney(int receiver, int money)
        {
            UserInfo charObj = WorldSystem.Instance.GetCharacterById(receiver) as UserInfo;
            if (null != charObj)
            {
                Vector3 pos = charObj.GetMovementStateInfo().GetPosition3D();
                GfxSystem.PublishGfxEvent("ge_gain_money", "ui", pos.X, pos.Y, pos.Z, money);
            }
        }

        private void ChangeHeroFromGfx()
        {
            try
            {
                ChangeHero();
            }
            catch (Exception ex)
            {
                LogSystem.Error("ExecCommand exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void ChangePlayerMoveMode(int mode)
        {
            try
            {
                UserInfo player = WorldSystem.Instance.GetPlayerSelf();
                if (null != player)
                {
                    if ((int)MovementMode.HighSpeed == mode)
                    {
                        player.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, player.GetBaseProperty().RunSpeed);
                        player.GetMovementStateInfo().MovementMode = MovementMode.Normal;
                    }
                    else if ((int)MovementMode.LowSpeed == mode)
                    {
                        player.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, player.GetBaseProperty().WalkSpeed);
                        player.GetMovementStateInfo().MovementMode = MovementMode.LowSpeed;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("ExecCommand exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void ChangeNpcMoveMode(int objId, int mode)
        {
            CharacterInfo character = WorldSystem.Instance.GetCharacterById(objId);
            if (null != character)
            {
                if ((int)MovementMode.HighSpeed == mode)
                {
                    character.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, character.GetBaseProperty().RunSpeed);
                    character.GetMovementStateInfo().MovementMode = MovementMode.Normal;
                }
                else if ((int)MovementMode.LowSpeed == mode)
                {
                    character.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, character.GetBaseProperty().WalkSpeed);
                    character.GetMovementStateInfo().MovementMode = MovementMode.LowSpeed;
                }
            }
        }

        private void ChangeSceneFromGfx(int sceneId)
        {
            try
            {
                bool targetIsServerSelect = false;
                Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
                if (null != cfg && cfg.m_Type == (int)SceneTypeEnum.TYPE_SERVER_SELECT)
                    targetIsServerSelect = true;
                if (null == m_CurScene || m_CurScene.IsSuccessEnter || targetIsServerSelect)
                {
                    if (targetIsServerSelect)
                    {
                        if (IsPvpScene())
                        {
                            NetworkSystem.Instance.QuitBattle(false);
                        }
                        else if (IsMultiPveScene())
                        {
                            NetworkSystem.Instance.QuitBattle(false);
                            NetworkSystem.Instance.ShutdownNet();
                        }
                        LobbyNetworkSystem.Instance.QuitClient();
                    }
                    ChangeScene(sceneId);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("ExecCommand exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void ResetDsl()
        {
            try
            {
                if (IsPureClientScene() || IsPveScene())
                {
                    StorySystem.StoryConfigManager.Instance.Clear();
                    ClientStorySystem.Instance.ClearStoryInstancePool();
                    for (int i = 1; i < 10; ++i)
                    {
                        ClientStorySystem.Instance.PreloadStoryInstance(i);
                    }
                    ClientStorySystem.Instance.StartStory(1);
                }
                else if (IsPvpScene() || IsMultiPveScene())
                {
                    Msg_CR_GmCommand cmdMsg = new Msg_CR_GmCommand();
                    cmdMsg.type = 0;
                    NetworkSystem.Instance.SendMessage(cmdMsg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void ExecScript(string scriptFile)
        {
            try
            {
                if (IsPureClientScene() || IsPveScene())
                {
                    if (GlobalVariables.Instance.IsDevice)
                    {
                        scriptFile = HomePath.GetAbsolutePath(FilePathDefine_Client.C_RootPath + scriptFile);
                    }
                    GmCommands.ClientGmStorySystem.Instance.Reset();
                    GmCommands.ClientGmStorySystem.Instance.LoadStory(scriptFile);
                    GmCommands.ClientGmStorySystem.Instance.StartStory(1);
                }
                else if (IsPvpScene() || IsMultiPveScene())
                {
                    Msg_CR_GmCommand cmdMsg = new Msg_CR_GmCommand();
                    cmdMsg.type = 1;
                    cmdMsg.content = scriptFile;
                    NetworkSystem.Instance.SendMessage(cmdMsg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("ExecScript exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void ExecCommand(string cmd)
        {
            try
            {
#if DEBUG
                GmCommands.ClientGmStorySystem.Instance.Reset();
                GmCommands.ClientGmStorySystem.Instance.LoadStoryText("script(1){onmessage(\"start\"){" + cmd + "}}");
                GmCommands.ClientGmStorySystem.Instance.StartStory(1);
#else
        LogSystem.Error("GM command can't used in RELEASE !");
#endif
                if (IsPvpScene() || IsMultiPveScene())
                {
                    Msg_CR_GmCommand cmdMsg = new Msg_CR_GmCommand();
                    cmdMsg.type = 2;
                    cmdMsg.content = cmd;
                    NetworkSystem.Instance.SendMessage(cmdMsg);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("ExecCommand exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void SwitchUnitId2ActorId(int unitId)
        {
            try
            {
                CharacterInfo obj_info = GetCharacterByUnitId(unitId);
                if (obj_info != null)
                {
                    NpcView npc_view = EntityManager.Instance.GetNpcViewById(obj_info.GetId());
                    if (npc_view != null)
                        GfxSystem.PublishGfxEvent("ge_actor_id", "ui", npc_view.Actor);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("ExecCommand exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void SwitchCombatState(int objId, int timeCoefficient)
        {
            try
            {
                if (timeCoefficient >= 1)
                {
                    CharacterInfo charObj = GetCharacterById(objId);
                    CharacterView view = EntityManager.Instance.GetCharacterViewById(objId);
                    if (null != charObj && null != view)
                    {
                        charObj.Combat2IdleCoefficient = timeCoefficient;
                        view.EnterCombatState();
                        view.UpdateLastLeaveCombatTime();
                    }
                }
                else
                {
                    CharacterInfo charObj = GetCharacterById(objId);
                    if (null != charObj)
                    {
                        charObj.Combat2IdleCoefficient = timeCoefficient;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("ExecCommand exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
        private void SetStoryState(int state)
        {
            if (state == 0)
            {
                m_IsStroyProceed = false;
            }
            else
            {
                m_IsStroyProceed = true;
            }
            m_ForceUpdateCamerState = true;
            SceneResource data = GetCurScene();
            if (null != data && null != data.SceneConfig
              && 1 == data.SceneConfig.m_Type && 0 == data.SceneConfig.m_SubType)
            {
                GfxSystem.PublishGfxEvent("ge_pause_or_resume_backgroud_music", "music", state > 0 ? 1 : 0, 1);
            }
        }

        private void OnAreaClear(int clearType)
        {
            GfxSystem.PublishGfxEvent("pve_area_clear", "ui_effect", clearType);
            m_IsAreaClear = true;
        }

        internal void OnRoomServerDisconnected()
        {
            if (m_CurScene.IsSuccessEnter && (IsPvpScene() || IsMultiPveScene()))
            {
                GfxSystem.PublishGfxEvent("ge_show_dialog", "ui", Dict.Get(44), Dict.Get(4), null, null, (MyAction<int>)((int btn) =>
                {
                    QueueAction((MyFunc<int, bool, bool>)this.ChangeScene, GetCurSceneId(), true);
                }), true);
            }
        }

        internal void OnRoomServerConnected()
        {
        }

        /**
         * @brief 释放
         *
         * @return 
         */
        internal void Release()
        {
            if (null != m_CurScene)
            {
                m_CurScene.Release();
            }
        }

        internal void QuitBattle()
        {
            OnRoomServerConnected();
            NetworkSystem.Instance.QuitBattle(false);
        }

        internal void QuitClient()
        {
            NetworkSystem.Instance.QuitClient();
            LobbyNetworkSystem.Instance.QuitClient();
        }

        public void ReturnToLogin()
        {
            try
            {
                NetworkSystem.Instance.QuitBattle(false);
                LobbyNetworkSystem.Instance.QuitClient();
                GfxSystem.PublishGfxEvent("ge_return_login", "ui");
            }
            catch (Exception ex)
            {
                LogSystem.Error("ReturnToLogin exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public bool IsPureClientScene()
        {
            if (null == m_CurScene)
                return true;
            else
                return m_CurScene.IsPureClientScene;
        }

        public bool IsPveScene()
        {
            if (null == m_CurScene)
                return false;
            else
                return m_CurScene.IsPve;
        }

        public bool IsPvapScene()
        {
            if (null == m_CurScene)
            {
                return false;
            }
            else
            {
                return m_CurScene.IsPvap;
            }
        }

        public bool IsMultiPveScene()
        {
            if (null == m_CurScene)
                return false;
            else
                return m_CurScene.IsMultiPve;
        }

        public bool IsPvpScene()
        {
            if (null == m_CurScene)
                return false;
            else
                return m_CurScene.IsPvp;
        }

        public bool IsExpeditionScene()
        {
            if (null == m_CurScene)
                return false;
            else
                return m_CurScene.IsExpedition;
        }
        public bool IsELiteScene()
        {
            if (null == m_CurScene)
                return false;
            else
                return m_CurScene.IsELite;
        }
        public bool IsAttemptScene()
        {
            if (null == m_CurScene)
                return false;
            else
                return m_CurScene.IsAttempt;
        }
        public bool IsGoldScene()
        {
            if (null == m_CurScene)
                return false;
            else
                return m_CurScene.IsGold;
        }
        public bool IsServerSelectScene()
        {
            //第一次加载时没通知逻辑层场景类型性，该情况下为空的时候返回true
            if (null == m_CurScene)
                return true;
            else
                return m_CurScene.IsServerSelectScene;
        }
        
        // main ticks
        internal void Tick()
        {
            // tick logic every 40ms
            long curTime = TimeUtility.GetLocalMilliseconds();
            if (m_LastLogicTickTime + 40 <= curTime)
            {
                m_LastLogicTickTime = curTime;
            }
            else
            {
                return;
            }

            // return if no scene avalible
            if (m_CurScene == null)
            {
                return;
            }

            TimeSnapshot.Start();
            TimeSnapshot.DoCheckPoint();
            
            // first time player enter a new scene
            if (!m_CurScene.IsWaitSceneLoad && m_CurScene.IsWaitRoomServerConnect)
            {
                if (this.IsPureClientScene() || this.IsPveScene() || this.IsServerSelectScene() || NetworkSystem.Instance.CanSendMessage)
                {
                    ArkProfiler.Start("WorldSystem.FirstEnterScene");

                    // load scene relative ui resources
                    GfxSystem.PublishGfxEvent("ge_enter_scene", "ui", m_CurScene.ResId);

                    // clear story pool
                    StorySystem.StoryConfigManager.Instance.Clear();
                    ClientStorySystem.Instance.ClearStoryInstancePool();
                    for (int i = 1; i < 10; ++i)
                    {
                        ClientStorySystem.Instance.PreloadStoryInstance(i);
                    }

                    // reset cached states
                    PlayerControl.Instance.Reset();
                    m_IsStroyProceed = false;
                    m_IsStageClearing = false;
                    m_IsCameraInCombatState = true;
                    m_ForceUpdateCamerState = true;
                    m_IsAreaClear = true;

                    // in observer mode
                    if (IsObserver)
                    {
                        // destroy self
                        DestroyHero();
                        // create scene logics
                        CreateSceneLogics();
                        // create use by fake id
                        UserInfo myself = CreatePlayerSelf(0x0ffffffe, 1);
                        if (null != myself)
                        {
                            myself.SetLevel(16);
                            myself.SetHp(Operate_Type.OT_Absolute, 999999);
                        }
                        // mark player entered
                        m_CurScene.NotifyUserEnter();
                        // send observer msg to server
                        ArkCrossEngineMessage.Msg_CR_Observer build = new ArkCrossEngineMessage.Msg_CR_Observer();
                        NetworkSystem.Instance.SendMessage(build);
                        LogSystem.Debug("send Msg_CR_Observer to roomserver");
                    }
                    else if (this.IsPureClientScene() || IsPveScene())
                    {
                        // create scene logics
                        CreateSceneLogics();
                        
                        if (IsExpeditionScene())
                        {
                            // expedition mode
                            ExpeditionStartGame();
                        }
                        else
                        {
                            // normal mode
                            StartGame();
                        }
                        // mark player entered
                        m_CurScene.NotifyUserEnter();
                        ClientStorySystem.Instance.StartStory(1);
                    }
                    else
                    {
                        // player guid may change via pvp scene, so destroy self first
                        DestroyHero();
                        // create scene logics
                        CreateSceneLogics();
                        // pvp or multi pve scene
                        if (IsPvpScene() || IsMultiPveScene())
                        {
                            ArkCrossEngineMessage.Msg_CRC_Create build = new ArkCrossEngineMessage.Msg_CRC_Create();
                            NetworkSystem.Instance.SendMessage(build);
                            LogSystem.Debug("send Msg_CRC_Create to roomserver");
                        }
                    }
                    // request city users
                    if (IsPureClientScene())
                    {
                        RequestCityUsers();
                    }
                    // sync all user info from userView to GfxUser info, display nickname board
                    if ((IsPveScene() || IsPureClientScene()) && !IsPvapScene())
                    {
                        SyncGfxUsersInfo();
                    }

                    m_CurScene.IsWaitRoomServerConnect = false;
                    ArkProfiler.Stop("WorldSystem.FirstEnterScene");
                }
            }
            if (IsMultiPveScene() && m_CurScene.IsWaitRoomServerConnect && !m_CurScene.IsNoticeException)
            {
                if (curTime > m_LastTryChangeSceneTime + c_ExceptionGotoMainCityTimeout)
                {
                    m_CurScene.IsNoticeException = true;
                    m_LastTryChangeSceneTime = curTime;
                    GfxSystem.PublishGfxEvent("ge_show_exception_dialog", "ui", Dict.Get(34), Dict.Get(4), null, null, true);
                }
                return;
            }

            // if enter failed
            if (!m_CurScene.IsSuccessEnter)
            {
                // timeout, promt exceptin and quit
                if (curTime > m_LastTryChangeSceneTime + c_ChangeSceneTimeout)
                {
                    m_LastTryChangeSceneTime = curTime;
                    PromptExceptionAndGotoMainCity();
                }
                // try again next frame
                else if (m_LastPromptExceptionTime + c_PromptExceptionTimeout < curTime)
                {
                    return;
                }
            }

            m_Profiler.sceneTickTime = TimeSnapshot.DoCheckPoint();

            // handle delay action
            m_DelayActionProcessor.HandleActions(100);
            // if enter scene is fired by DelayAction above, wait for ticks finish loading
            if (!m_CurScene.IsSuccessEnter)
            {
                return;
            }
            // tick user views and npc views
            EntityManager.Instance.Tick();
            m_Profiler.entityMgrTickTime = TimeSnapshot.DoCheckPoint();
            // tick character adjust system
            ControlSystemOperation.Tick();
            m_Profiler.controlSystemTickTime = TimeSnapshot.DoCheckPoint();
            m_Profiler.movementSystemTickTime = TimeSnapshot.DoCheckPoint();
            // tick spatial system
            m_SpatialSystem.Tick();
            m_Profiler.spatialSystemTickTime = TimeSnapshot.DoCheckPoint();
            // tick ai system
            m_AiSystem.AlwaysClientTick();

#if DEBUG
            if (m_Profiler.spatialSystemTickTime > 50000)
            {
                LogSystem.Warn("*** SpatialSystem tick time is {0}", m_Profiler.spatialSystemTickTime);
                for (LinkedListNode<UserInfo> node = UserManager.Users.FirstValue; null != node; node = node.Next)
                {
                    UserInfo userInfo = node.Value;
                    if (null != userInfo)
                    {
                        LogSystem.Warn("===>User:{0} Pos:{1}", userInfo.GetId(), userInfo.GetMovementStateInfo().GetPosition3D().ToString());
                    }
                }
                for (LinkedListNode<NpcInfo> node = NpcManager.Npcs.FirstValue; null != node; node = node.Next)
                {
                    NpcInfo npcInfo = node.Value;
                    if (null != npcInfo)
                    {
                        LogSystem.Warn("===>Npc:{0} Pos:{1}", npcInfo.GetId(), npcInfo.GetMovementStateInfo().GetPosition3D().ToString());
                    }
                }
            }
#endif
            // handle move meet obstacle
            TickMoveMeetObstacle();
            // handle special delay logic ie expedition scene.
            TickDelayLogic();
            // handle arena logic
            TickArenaLogic();
            // tick per user operations
            TickUsers();
            m_Profiler.usersTickTime = TimeSnapshot.DoCheckPoint();
            // tick per npc operations
            TickNpcs();
            m_Profiler.npcsTickTime = TimeSnapshot.DoCheckPoint();

            // update camera parameters
            UpdateCamera();

            // tick impact system
            try
            {
                TickSystemByCharacters();
            }
            catch (Exception e)
            {
                LogSystem.Error("Exception:{0}\n{1}", e.Message, e.StackTrace);
            }
            m_Profiler.combatSystemTickTime = TimeSnapshot.DoCheckPoint();

            // tick scoll messages
            TickScrollMessage();

            // tick city users
            if (IsPureClientScene())
            {
                TickCityUsers();
                m_AiSystem.Tick();
            }

            // tick pve
            if ((IsPureClientScene() || IsPveScene()) && !IsPvapScene())
            {
                TickPve();
            }
            // tick pvp
            if (IsPveScene())
            {
                TickRecover();
            }

            // tick story system
            GmCommands.ClientGmStorySystem.Instance.Tick();

            // tick scene logic system
            if (!IsPvapScene())
            {
                m_SceneLogicSystem.Tick();
            }
            m_Profiler.sceneLogicSystemTickTime = TimeSnapshot.DoCheckPoint();

            long tickTime = TimeSnapshot.End();
            if (tickTime > 30000)
            {
                LogSystem.Debug("*** PerformanceWarning: {0}", m_Profiler.GenerateLogString(tickTime));
            }

            // handle damage data
            TickDamageDatas();
        }

        private void TickDelayLogic()
        {
            UserInfo user = GetPlayerSelf();
            if (null == user)
                return;
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (null != role_info)
            {
                ///
                if (IsExpeditionScene())
                {
                    ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
                    if (null != expedition && expedition.StartTime > 0
                      && null != expedition.Tollgates && expedition.ActiveTollgate >= 0)
                    {
                        ExpeditionPlayerInfo.TollgateData cur_tollgate_data = expedition.Tollgates[expedition.ActiveTollgate];
                        if (null != cur_tollgate_data)
                        {
                            double elapse_time = TimeUtility.CurTimestamp - expedition.StartTime;
                            if (!cur_tollgate_data.IsDelayRefreshed && elapse_time >= ExpeditionPlayerInfo.c_DelayFlushTime)
                            {
                                GfxSystem.PublishGfxEvent("pve_checkpoint_begin", "ui_effect", "", 1, expedition.ActiveTollgate + 1);
                                DelayFlushMonster(user);
                            }
                        }
                    }
                }
            }
        }

        internal void SwitchDebug()
        {
            GlobalVariables.Instance.IsDebug = !GlobalVariables.Instance.IsDebug;
            if (IsPvpScene() || IsMultiPveScene())
            {
                Msg_CR_SwitchDebug builder = new Msg_CR_SwitchDebug();
                builder.is_debug = GlobalVariables.Instance.IsDebug;
                NetworkSystem.Instance.SendMessage(builder);
            }
            if (!GlobalVariables.Instance.IsDebug)
            {
                ShowObstacle(false);
            }
            else
            {
                ShowObstacle(true);
            }
        }
        internal void SwitchObserver()
        {
            if (m_IsObserver)
            {
                m_IsFollowObserver = !m_IsFollowObserver;
                if (m_IsFollowObserver)
                {
                    LinkedListNode<UserInfo> node = null;
                    for (node = UserManager.Users.FirstValue; null != node; node = node.Next)
                    {
                        if (node.Value == m_PlayerSelf)
                            continue;
                        if (node.Value.GetId() == m_FollowTargetId)
                        {
                            break;
                        }
                    }
                    if (null == node)
                    {
                        for (node = UserManager.Users.FirstValue; null != node; node = node.Next)
                        {
                            if (node.Value == m_PlayerSelf)
                                continue;
                            else
                                break;
                        }
                    }
                    if (null != node)
                    {
                        m_FollowTargetId = node.Value.GetId();
                        if (!node.Value.IsDead())
                        {
                            UserView view = EntityManager.Instance.GetUserViewById(m_FollowTargetId);
                            if (null != view)
                            {
                                GfxSystem.SendMessage("GfxGameRoot", "CameraFollow", view.Actor);
                            }
                        }
                    }
                }
            }
        }
        internal bool InteractObject()
        {
            if (m_IsObserver)
            {
                if (m_IsFollowObserver)
                {
                    LinkedListNode<UserInfo> node = null;
                    for (node = UserManager.Users.FirstValue; null != node; node = node.Next)
                    {
                        if (node.Value == m_PlayerSelf)
                            continue;
                        if (node.Value.GetId() == m_FollowTargetId)
                        {
                            node = node.Next;
                            break;
                        }
                    }
                    if (null == node)
                    {
                        for (node = UserManager.Users.FirstValue; null != node; node = node.Next)
                        {
                            if (node.Value == m_PlayerSelf)
                                continue;
                            else
                                break;
                        }
                    }
                    if (null != node)
                    {
                        m_FollowTargetId = node.Value.GetId();
                    }
                }
                return true;
            }
            bool sendMsg = false;
            if (null != m_PlayerSelf)
            {
                int initiator = m_PlayerSelf.GetId();
                int receiver = 0;

                //释放控制为低优先级操作
                if (!sendMsg && null != m_PlayerSelf.ControlledObject)
                {
                    receiver = m_PlayerSelf.ControlledObject.GetId();
                    sendMsg = true;
                }

                if (sendMsg)
                {
                    Msg_CRC_InteractObject builder = new Msg_CRC_InteractObject();
                    //builder.InitiatorId = initiator;
                    builder.receiver_id = receiver;
                    NetworkSystem.Instance.SendMessage(builder);
                }
            }
            return sendMsg;
        }

        internal bool ChangeScene(int sceneId)
        {
            return ChangeScene(sceneId, false);
        }

        internal bool ChangeScene(int sceneId, bool forceEnter)
        {
            try
            {
                ArkProfiler.Start("ChangeScene");

                // cache scene id
                GfxSystem.PublishGfxEvent("ge_change_sceneId_ui", "ui_data", sceneId);

                if (null != m_CurScene)
                {
                    // already in target scene
                    if (m_CurScene.ResId == sceneId && !forceEnter && !this.IsServerSelectScene())
                    {
                        return true;
                    }
                    // in error states, return failed
                    else if (!m_CurScene.IsSuccessEnter && !this.IsServerSelectScene() && !m_CurScene.IsWaitRoomServerConnect)
                    {
                        return false;
                    }
                    // release all subsystem managed resource relative to scene
                    Reset();
                    // notify scene release
                    m_CurScene.Release();
                    m_CurScene = null;
                }
                else
                {
                    Reset();
                }

                m_LastTryChangeSceneTime = TimeUtility.GetLocalMilliseconds();

                m_CurScene = new SceneResource();
                if (null != m_CurScene)
                {
                    // load scene info
                    m_CurScene.Init(sceneId);
                    if (null != m_CurScene.SceneConfig)
                    {
                        // if in server select scene, no role replied from server
                        if (IsServerSelectScene())
                        {
                            LobbyClient.Instance.CurrentRole = null;
                        }
                        if (IsPureClientScene())
                        {
                            // notify server enter scene
                            LobbyNetworkSystem.Instance.ChangeCityScene(sceneId);
                            GfxSystem.PublishGfxEvent("ge_enter_city", "ui");
                        }
                        // read obstacle data
                        Data_SceneConfig scene_config = SceneConfigProvider.Instance.GetSceneConfigById(m_CurScene.ResId);
                        m_SpatialSystem.Init(FilePathDefine_Client.C_RootPath + scene_config.m_BlockInfoFile, scene_config.m_ReachableSet);
                        m_SpatialSystem.LoadPatch(FilePathDefine_Client.C_RootPath + scene_config.m_BlockInfoFile + ".patch");
                        m_SpatialSystem.LoadObstacle(FilePathDefine_Client.C_RootPath + scene_config.m_ObstacleFile, 1 / scene_config.m_TiledDataScale);

                        LogSystem.Debug("init SpatialSystem:{0}", FilePathDefine_Client.C_RootPath + scene_config.m_BlockInfoFile);
                        LogSystem.Debug("GameSystem.ChangeNextScene:{0}", m_CurScene.ResId);

                        ArkProfiler.Stop("ChangeScene");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
            return false;
        }

        public int GetCurSceneId()
        {
            if (m_CurScene != null)
            {
                return m_CurScene.ResId;
            }
            return 0;
        }
        internal SceneResource GetCurScene()
        {
            return m_CurScene;
        }

        internal void AddCityUser(ulong guid, int heroid, string nick, float x, float z, float facedir, int xsoulItemId, int xsoulLevel, int xsoulExp, int xsoulShowLevel, int wingItemId, int wingLevel)
        {
            if (!IsPureClientScene())
                return;
            if (null != m_PlayerSelf)
            {
                //float mx = m_PlayerSelf.GetMovementStateInfo().PositionX;
                //float mz = m_PlayerSelf.GetMovementStateInfo().PositionZ;
                //if (x < 10) x = mx + CrossEngineHelper.Random.Next(10) - 5;
                //if (z < 10) z = mz + CrossEngineHelper.Random.Next(10) - 5;
                int id;
                if (m_CityUsers.TryGetValue(guid, out id))
                {
                    UserInfo user = m_UserMgr.GetUserInfo(id);
                    if (null != user)
                    {
                        MovementStateInfo msi = user.GetMovementStateInfo();
                        msi.SetWantFaceDir(facedir);

                        ClientStorySystem.Instance.SendMessage("cityusermove", id, x, z);
                    }
                }
                else
                {
                    id = m_NextCityUserId;
                    ++m_NextCityUserId;
                    if (m_CurCityUserWithModelCount < c_MaxCityUserWithModel)
                    {
                        ++m_CurCityUserWithModelCount;

                        heroid += c_LowModelIdDeviation;//显示替代模型1
                    }
                    else
                    {
                        heroid += c_LowModelId2Deviation;//显示替代模型2
                    }
                    UserInfo user = CreateUser(id, heroid);
                    if (null != user)
                    {
                        m_CityUsers.Add(guid, id);

                        user.SetCampId((int)CampIdEnum.Blue);
                        user.SetNickName(nick);
                        user.GetMovementStateInfo().SetPosition2D(x, z);
                        user.GetMovementStateInfo().SetFaceDir(facedir);

                        if (xsoulItemId > 0)
                        {
                            ItemDataInfo itemInfo = new ItemDataInfo();
                            itemInfo.ItemConfig = ItemConfigProvider.Instance.GetDataById(xsoulItemId);
                            itemInfo.Level = xsoulLevel;
                            itemInfo.Experience = xsoulExp;
                            XSoulPartInfo xsoul = new XSoulPartInfo(XSoulPart.kWeapon, itemInfo);
                            xsoul.ShowModelLevel = xsoulShowLevel;
                            user.GetXSoulInfo().SetXSoulPartData(XSoulPart.kWeapon, xsoul);
                        }

                        /*
                        if (wingItemId > 0)
                        {
                            ItemDataInfo itemInfo2 = new ItemDataInfo();
                            itemInfo2.ItemConfig = ItemConfigProvider.Instance.GetDataById(wingItemId);
                            itemInfo2.Level = wingLevel;
                            user.GetEquipmentStateInfo().SetEquipmentData((int)EquipmentType.E_Wing, itemInfo2);
                        }
                        */

                        EntityManager.Instance.CreateUserView(id);

                        // disable name plate if too many player
                        if (m_CurCityUserWithModelCount < c_MaxCityUserWithModel)
                        {
                            SyncGfxUserInfo(id);
                        }
                    }
                }
            }
        }
        internal void RemoveCityUser(ulong guid)
        {
            if (!IsPureClientScene())
                return;
            int objId;
            if (m_CityUsers.TryGetValue(guid, out objId))
            {
                m_CityUsers.Remove(guid);

                UserInfo user = m_UserMgr.GetUserInfo(objId);
                if (null != user && user.GetLinkId() >= c_LowModelIdDeviation && user.GetLinkId() < c_LowModelId2Deviation)
                {
                    --m_CurCityUserWithModelCount;
                }

                UserView view = EntityManager.Instance.GetUserViewById(objId);
                if (null != view)
                {
                    GfxSystem.PublishGfxEvent("ge_hide_name_plate", "ui", view.Actor);
                }
                EntityManager.Instance.DestroyUserView(objId);
                m_UserMgr.RemoveUser(objId);
            }
        }
        internal void UpdateCityUser(ulong guid, float x, float z, float facedir)
        {
            if (!IsPureClientScene())
                return;
            if (null != m_PlayerSelf)
            {
                float mx = m_PlayerSelf.GetMovementStateInfo().PositionX;
                float mz = m_PlayerSelf.GetMovementStateInfo().PositionZ;
                //if (x < 10) x = mx + CrossEngineHelper.Random.Next(10) - 5;
                //if (z < 10) z = mz + CrossEngineHelper.Random.Next(10) - 5;
                int objId;
                if (m_CityUsers.TryGetValue(guid, out objId))
                {

                    UserInfo user = m_UserMgr.GetUserInfo(objId);
                    if (null != user)
                    {
                        MovementStateInfo msi = user.GetMovementStateInfo();
                        msi.SetWantFaceDir(facedir);
                        float distSqr = Geometry.DistanceSquare(msi.GetPosition3D(), new Vector3(x, 0, z));
                        if (distSqr >= 1)
                        {
                            ClientStorySystem.Instance.SendMessage("cityusermove", objId, x, z);
                        }
                        else
                        {
                            msi.SetFaceDir(facedir);
                        }
                    }
                }
            }
        }

        internal void SetBlockedShader(uint rimColor1, float rimPower1, float rimCutValue1, uint rimColor2, float rimPower2, float rimCutValue2)
        {
            if (!GlobalVariables.Instance.IsHD)
                return;//低配机不开启遮挡半透
            UserInfo myself = GetPlayerSelf();
            if (null != myself)
            {
                UserView myselfView = EntityManager.Instance.GetUserViewById(myself.GetId());
                if (null != myselfView)
                {
                    GfxSystem.SetBlockedShader(myselfView.Actor, rimColor1, rimPower1, rimCutValue1);
                }
                LinkedListDictionary<int, UserInfo> users = UserManager.Users;
                for (LinkedListNode<UserInfo> node = users.FirstValue; null != node; node = node.Next)
                {
                    UserInfo user = node.Value;
                    if (null != user && user != myself)
                    {
                        UserView view = EntityManager.Instance.GetUserViewById(user.GetId());
                        if (null != view)
                        {
                            if (CharacterInfo.GetRelation(myself, user) == CharacterRelation.RELATION_FRIEND)
                            {
                                GfxSystem.SetBlockedShader(view.Actor, rimColor1, rimPower1, rimCutValue1);

                            }
                            else
                            {
                                GfxSystem.SetBlockedShader(view.Actor, rimColor2, rimPower2, rimCutValue2);
                            }
                        }
                    }
                }
                LinkedListDictionary<int, NpcInfo> npcs = NpcManager.Npcs;
                for (LinkedListNode<NpcInfo> node = npcs.FirstValue; null != node; node = node.Next)
                {
                    NpcInfo npc = node.Value;
                    if (null != npc)
                    {
                        NpcView view = EntityManager.Instance.GetNpcViewById(npc.GetId());
                        if (null != view)
                        {
                            if (CharacterInfo.GetRelation(myself, npc) == CharacterRelation.RELATION_FRIEND)
                            {
                                GfxSystem.SetBlockedShader(view.Actor, rimColor1, rimPower1, rimCutValue1);

                            }
                            else
                            {
                                GfxSystem.SetBlockedShader(view.Actor, rimColor2, rimPower2, rimCutValue2);
                            }
                        }
                    }
                }
            }
        }

        internal void Reset()
        {
            LogSystem.Debug("WorldSystem.Reset Destory Objects...");

            DestroyObstacleObjects();

            for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo info = linkNode.Value;
                if (null != info)
                {
                    EntityManager.Instance.DestroyUserView(info.GetId());
                }
            }
            for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (null != info)
                {
                    EntityManager.Instance.DestroyNpcView(info.GetId());
                }
            }
            LogSystem.Debug("WorldSystem.Reset Destory Objects Finish.");

            m_UserMgr.Reset();
            m_NpcMgr.Reset();
            m_SceneLogicInfoMgr.Reset();

            m_SceneLogicSystem.Reset();
            m_SpatialSystem.Reset();
            m_AiSystem.Reset();
            m_BlackBoard.Reset();

            m_CityUsers.Clear();
            m_LastCityUserTickTime = 0;
            m_CurCityUserTickCount = 0;
            m_NextCityUserId = 2;
            m_CurCityUserWithModelCount = 0;
            m_WaitMatchSceneId = -1;

            ControlSystemOperation.Reset();

            ClientStorySystem.Instance.Reset();
            ClientStorySystem.Instance.ClearStoryInstancePool();
            StorySystem.StoryConfigManager.Instance.Clear();
        }
        internal void LoadData()
        {
            SceneConfigProvider.Instance.Load(FilePathDefine_Client.C_SceneConfig, "ScenesConfigs");
            SceneConfigProvider.Instance.LoadDropOutConfig(FilePathDefine_Client.C_SceneDropOut, "SceneDropOut");
            UiConfigProvider.Instance.Load(FilePathDefine_Client.C_UiConfig, "UiConfig");
            SceneConfigProvider.Instance.LoadAllSceneConfig(FilePathDefine_Client.C_RootPath);
            
            ActionConfigProvider.Instance.Load(FilePathDefine_Client.C_ActionConfig, "ActionConfig");
            NpcConfigProvider.Instance.LoadNpcConfig(FilePathDefine_Client.C_NpcConfig, "NpcConfig");
            NpcConfigProvider.Instance.LoadNpcLevelupConfig(FilePathDefine_Client.C_NpcLevelupConfig, "NpcLevelupConfig");
            PlayerConfigProvider.Instance.LoadPlayerConfig(FilePathDefine_Client.C_PlayerConfig, "PlayerConfig");
            PlayerConfigProvider.Instance.LoadPlayerLevelupConfig(FilePathDefine_Client.C_PlayerLevelupConfig, "PlayerLevelupConfig");
            PlayerConfigProvider.Instance.LoadPlayerLevelupExpConfig(FilePathDefine_Client.C_PlayerLevelupExpConfig, "PlayerLevelupExpConfig");
            //CriticalConfigProvider.Instance.Load(FilePathDefine_Client.C_CriticalConfig, "CriticalConfig");
            AttributeScoreConfigProvider.Instance.Load(FilePathDefine_Client.C_AttributeScoreConfig, "AttributeScoreConfig");

            AiActionConfigProvider.Instance.Load(FilePathDefine_Client.C_AiActionConfig, "AiActionConfig");
            AiConfigProvider.Instance.Load(FilePathDefine_Client.C_AiConfig, "AiConfig");
            AiSkillComboListProvider.Instance.Load(FilePathDefine_Client.C_AiSkillComboList, "AiSkillComboList");

            ItemConfigProvider.Instance.Load(FilePathDefine_Client.C_ItemConfig, "ItemConfig");
            ItemAttributeConfigProvider.Instance.Load(FilePathDefine_Client.C_ItemAttributeConfig, "ItemAttributeConfig");
            ItemLevelupConfigProvider.Instance.Load(FilePathDefine_Client.C_ItemLevelupConfig, "ItemLevelupConfig");
            ItemCompoundConfigProvider.Instance.Load(FilePathDefine_Client.C_ItemCompoundConfig, "ItemCompoundConfig");
            XSoulLevelConfigProvider.Instance.Load(FilePathDefine_Client.C_XSoulLevelConfig, "XSoulLevelConfig");
            EquipmentConfigProvider.Instance.LoadEquipmentConfig(FilePathDefine_Client.C_EquipmentConfig, "EquipmentConfig");
            BuyStaminaConfigProvider.Instance.Load(FilePathDefine_Client.C_BuyStaminaConfig, "BuyStaminaConfig");
            AppendAttributeConfigProvider.Instance.Load(FilePathDefine_Client.C_AppendAttributeConfig, "AppendAttributeConfig");
            LegacyLevelupConfigProvider.Instance.Load(FilePathDefine_Client.C_LegacyLevelupConfig, "LegacyLevelupConfig");
            LegacyComplexAttrConifgProvider.Instance.Load(FilePathDefine_Client.C_LegacyComplexAttrConifg, "LegacyComplexAttrConifg");
            ArenaConfigProvider.Instance.LoadClientConfig();
            SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_SOUND, FilePathDefine_Client.C_SoundConfig, "SoundConfig");
            SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_SKILL, FilePathDefine_Client.C_SkillSystemConfig, "SkillConfig");
            SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_IMPACT, FilePathDefine_Client.C_ImpactSystemConfig, "ImpactConfig");
            SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_EFFECT, FilePathDefine_Client.C_EffectConfig, "EffectConfig");
            SkillLevelupConfigProvider.Instance.Load(FilePathDefine_Client.C_SkillLevelupConfig, "SkillLevelupConfig");

            BuffConfigProvider.Instance.Load(FilePathDefine_Client.C_BuffConfig, "BuffConfig");

            SoundConfigProvider.Instance.Load(FilePathDefine_Client.C_GlobalSoundConfig, "C_GlobalSoundConfig");
            StrDictionaryProvider.Instance.Load(FilePathDefine_Client.C_StrDictionary, "StrDictionary");

            NewbieGuideProvider.Instance.Load(FilePathDefine_Client.C_NewbieGuide, "NewbieGuide");
            SystemGuideConfigProvider.Instance.Load(FilePathDefine_Client.C_SystemGuideConfig, "SystemGuideConfig");

            
            LoadingChangeProvider.Instance.Load(FilePathDefine_Client.C_LoadingChange, "LoadingChange");
            LevelLockProvider.Instance.Load(FilePathDefine_Client.C_LevelLock, "LevelLock");
            ServerConfigProvider.Instance.Load(FilePathDefine_Client.C_ServerConfig, "ServerConfig");
            MissionConfigProvider.Instance.Load(FilePathDefine_Client.C_MissionConfig, "MissionConfig");
            PartnerConfigProvider.Instance.Load(FilePathDefine_Client.C_PartnerConfig, "PartnerConfig");
            PartnerLevelUpConfigProvider.Instance.Load(FilePathDefine_Client.C_PartnerLevelUpConfig, "PartnerLevelUpConfig");
            PartnerStageUpConfigProvider.Instance.Load(FilePathDefine_Client.C_PartnerStageUpConfig, "PartnerStageUpConfig");
            WordFilter.Instance.Load(FilePathDefine_Client.C_SensitiveDictionary);
            DynamicSceneConfigProvider.Instance.CollectData(FilePathDefine_Client.C_DynamicSceneConfig, "DynamicSceneConfig");

            ExpeditionMonsterAttrConfigProvider.Instance.Load(FilePathDefine_Client.C_ExpeditionMonsterAttrConfig, "ExpeditionMonsterAttrConfig");
            ExpeditionTollgateConfigProvider.Instance.Load(FilePathDefine_Client.C_ExpeditionTollgateConfig, "ExpeditionTollgateConfig");
            ExpeditionMonsterConfigProvider.Instance.Load(FilePathDefine_Client.C_ExpeditionMonsterConfig, "ExpeditionMonsterConfig");

            BuyMoneyConfigProvider.Instance.Load(FilePathDefine_Client.C_BuyMoneyConfig, "BuyMoneyConfig");
            GowConfigProvider.Instance.LoadForClient();

            VipConfigProvider.Instance.Load(FilePathDefine_Client.C_VipConfig, "VipConfig");
            VersionConfigProvider.Instance.Load(FilePathDefine_Client.C_VersionConfig, "VersionConfig");

            MpveMonsterConfigProvider.Instance.Load(FilePathDefine_Client.C_MpveMonsterConfig, "MpveMonsterConfig");
            AttemptTollgateConfigProvider.Instance.Load(FilePathDefine_Client.C_AttemptTollgateConfig, "AttemptTollgateConfig");
            MpveTimeConfigProvider.Instance.Load(FilePathDefine_Client.C_MpveTimeConfig, "MpveTimeConfig");
            // 活动
            SignInRewardConfigProvider.Instance.Load(FilePathDefine_Client.C_SignInRewardConfig, "SignInRewardConfig");
            MonthCardConfigProvider.Instacne.Load(FilePathDefine_Client.C_MonthCardConfig, "MonthCardConfig");
            GiftConfigProvider.Instance.Load(FilePathDefine_Client.C_GiftConfig, "GiftConfig");
            WeeklyLoginConfigProvider.Instance.Load(FilePathDefine_Client.C_WeeklyLoginConfig, "WeeklyLoginConfig");
            OnlineDurationRewardConfigProvider.Instance.Load(FilePathDefine_Client.C_OnlineDurationRewardConfig, "OnlineDurationRewardConfig");

            ScrollMessageConfigProvider.Instance.LoadForClient();

            StoreConfigProvider.Instance.Load(FilePathDefine_Client.C_ExchangeShopConfig, "ExchangeShop");
            NotificationConfigProvider.Instance.Load(FilePathDefine_Client.C_NotificationConfig, "NotificationConfig");
            CameraConfigProvider.Instance.Load(FilePathDefine_Client.C_CameraConfig, "CameraConfig");
            HitSoundConfigProvider.Instance.Load(FilePathDefine_Client.C_HitSoundConfig, "HitSoundConfig");

            MainCityConfigProvider.Instance.Load(FilePathDefine_Client.C_MainCityConfig, "MainCityConfig");

            foreach (ScrollMessageConfig cfg in ScrollMessageConfigProvider.Instance.ScrollMessageConfigMgr.GetData())
            {
                ScrollMessageInfo info = new ScrollMessageInfo();
                info.m_Week = cfg.m_Week;
                info.m_Start = new DateTime(1970, 1, 1, cfg.m_StartHour, cfg.m_StartMinute, 0);
                info.m_End = new DateTime(1970, 1, 1, cfg.m_EndHour, cfg.m_EndMinute, 0);
                info.m_Interval = cfg.m_Interval * 1000;
                info.m_Message = cfg.m_Message;
                info.m_ScrollCount = cfg.m_ScrollCount;
                info.m_LastSendTime = 0;

                m_ScrollMessageInfos.Add(info);
            }
        }
        internal void ReloadNpc()
        {
            foreach (Data_Unit npcUnit in m_CurScene.StaticData.m_UnitMgr.GetData().Values)
            {
                if (npcUnit.m_IsEnable)
                {
                    NpcInfo npc = m_NpcMgr.GetNpcInfoByUnitId(npcUnit.GetId());
                    if (null == npc)
                    {
                        npc = m_NpcMgr.AddNpc(npcUnit);
                    }
                    if (null != npc)
                    {
                        npc.SetAIEnable(true);
                        EntityManager.Instance.CreateNpcView(npc.GetId());
                    }
                }
            }
        }
        internal void ChangeHero()
        {
            try
            {
                UserInfo user = GetPlayerSelf();
                if (null != user)
                {
                    Vector3 pos = user.GetMovementStateInfo().GetPosition3D();
                    float dir = user.GetMovementStateInfo().GetFaceDir();
                    int hp = user.Hp;
                    int rage = user.Rage;

                    EntityManager.Instance.DestroyUserView(user.GetId());
                    DestroyCharacterById(user.GetId());

                    NetworkSystem.Instance.HeroId = (NetworkSystem.Instance.HeroId + 1) % 4;
                    if (NetworkSystem.Instance.HeroId == 0)
                        NetworkSystem.Instance.HeroId = 1;

                    user = CreatePlayerSelf(1, NetworkSystem.Instance.HeroId);
                    user.SetCampId(NetworkSystem.Instance.CampId);
                    /*Data_Unit unit = m_CurScene.StaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(NetworkSystem.Instance.CampId)) as Data_Unit;
                    if (null != unit) {
                      user.GetMovementStateInfo().SetPosition(unit.m_Pos);
                      user.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
                      user.SetHp(Operate_Type.OT_Absolute, 1000);
                    }*/
                    user.GetMovementStateInfo().SetPosition(pos);
                    user.GetMovementStateInfo().SetFaceDir(dir);
                    user.SetHp(Operate_Type.OT_Absolute, hp);
                    user.SetRage(Operate_Type.OT_Absolute, rage);
                    EntityManager.Instance.CreatePlayerSelfView(1);
                    UserView view = EntityManager.Instance.GetUserViewById(1);
                    if (null != view)
                    {
                        view.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Error("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal void StartGame()
        {
            m_SceneStartTime = TimeUtility.GetServerMilliseconds();
            UserInfo user = GetPlayerSelf();
            if (null != user)
            {
                EntityManager.Instance.DestroyUserView(user.GetId());
                DestroyCharacterById(user.GetId());
            }
            user = CreatePlayerSelf(1, NetworkSystem.Instance.HeroId);
            user.SetAIEnable(true);
            user.SetCampId(NetworkSystem.Instance.CampId);
            Data_Unit unit = m_CurScene.StaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(NetworkSystem.Instance.CampId)) as Data_Unit;
            if (null != unit)
            {
                user.GetMovementStateInfo().SetPosition(unit.m_Pos);
                user.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
                user.SetHp(Operate_Type.OT_Absolute, user.GetActualProperty().HpMax);
                user.SetEnergy(Operate_Type.OT_Absolute, user.GetActualProperty().EnergyMax);
            }

            if (null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (role_info.Nickname.Length > 0)
                {
                    user.SetNickName(role_info.Nickname);
                }
                /// level
                if (role_info.Level > 0)
                {
                    user.SetLevel(role_info.Level);
                }
                /// equips
                if (null != role_info.Equips)
                {
                    for (int i = 0; i < role_info.Equips.Length; i++)
                    {
                        if (null != role_info.Equips[i])
                        {
                            int item_id = role_info.Equips[i].ItemId;
                            if (item_id > 0)
                            {
                                ItemDataInfo info = new ItemDataInfo();
                                info.Level = role_info.Equips[i].Level;
                                info.ItemNum = role_info.Equips[i].ItemNum;
                                info.RandomProperty = role_info.Equips[i].RandomProperty;
                                info.ItemConfig = ItemConfigProvider.Instance.GetDataById(item_id);
                                if (null != info.ItemConfig)
                                {
                                    user.GetEquipmentStateInfo().SetEquipmentData(i, info);
                                }
                            }
                        }
                    }
                }
                /// skills
                RefixSkills(user);
                /// legacys
                if (null != role_info.Legacys)
                {
                    for (int i = 0; i < role_info.Legacys.Length; i++)
                    {
                        if (null != role_info.Legacys[i] && role_info.Legacys[i].IsUnlock)
                        {
                            user.GetLegacyStateInfo().ResetLegacyData(i);
                            int item_id = role_info.Legacys[i].ItemId;
                            if (item_id > 0)
                            {
                                ItemDataInfo info = new ItemDataInfo();
                                info.Level = role_info.Legacys[i].Level;
                                info.ItemNum = role_info.Legacys[i].ItemNum;
                                info.RandomProperty = role_info.Legacys[i].RandomProperty;
                                info.ItemConfig = ItemConfigProvider.Instance.GetDataById(item_id);
                                if (null != info.ItemConfig)
                                {
                                    user.GetLegacyStateInfo().SetLegacyData(i, info);
                                }
                            }
                        }
                    }
                    user.GetLegacyStateInfo().UpdateLegacyComplexAttr();
                }
                // xsoul
                InitUserXSoulInfo(user, role_info);
                user.TalentManager = role_info.TalentManager;
                /// partner
                if (null != role_info.PartnerStateInfo)
                {
                    user.SetPartnerInfo(role_info.PartnerStateInfo.GetActivePartner());
                }
                ///
                if (IsPvapScene())
                {
                    SetArenaCharacterCoefficient(user);
                }
                UserAttrCalculator.Calc(user);
                user.SetHp(Operate_Type.OT_Absolute, user.GetActualProperty().HpMax);
                user.SetEnergy(Operate_Type.OT_Absolute, user.GetActualProperty().EnergyMax);
            }

            EntityManager.Instance.CreatePlayerSelfView(1);
            UserView view = EntityManager.Instance.GetUserViewById(1);
            if (null != view)
            {
                view.Visible = true;
            }
            view.UpdateEquipment();
            /// create npc
            foreach (Data_Unit npcUnit in m_CurScene.StaticData.m_UnitMgr.GetData().Values)
            {
                if (npcUnit.m_IsEnable)
                {
                    NpcInfo npc = m_NpcMgr.GetNpcInfoByUnitId(npcUnit.GetId());
                    if (null == npc)
                    {
                        npc = m_NpcMgr.AddNpc(npcUnit);
                        if (null != npc)
                        {
                            npc.SetAIEnable(true);
                            npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                            EntityManager.Instance.CreateNpcView(npc.GetId());
                            CustomNpcByUnitId(npc, npcUnit.GetId());
                        }
                    }
                }
            }
            /// scene tips 
            if (IsELiteScene())
            {
                RoleInfo curRole = LobbyClient.Instance.CurrentRole;
                int stars = curRole.GetSceneInfo(m_CurScene.ResId);
                if (stars == 1)
                {
                    GfxSystem.PublishGfxEvent("ge_pve_fightinfo", "ui", 2, m_CurScene.SceneConfig.m_CompletedTime, 0, 0);
                }
                else if (stars == 2)
                {
                    GfxSystem.PublishGfxEvent("ge_pve_fightinfo", "ui", 0, 0, m_CurScene.SceneConfig.m_CompletedHitCount, 0);
                }
            }
            LogSystem.Debug("start game");
        }
        internal void ExpeditionStartGame()
        {
            UserInfo user = GetPlayerSelf();
            if (null != user)
            {
                EntityManager.Instance.DestroyUserView(user.GetId());
                DestroyCharacterById(user.GetId());
            }
            user = CreatePlayerSelf(1, NetworkSystem.Instance.HeroId);
            user.SetAIEnable(true);
            user.SetCampId(NetworkSystem.Instance.CampId);
            Data_Unit unit = m_CurScene.StaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(NetworkSystem.Instance.CampId)) as Data_Unit;
            if (null != unit)
            {
                user.GetMovementStateInfo().SetPosition(unit.m_Pos);
                user.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
                user.SetHp(Operate_Type.OT_Absolute, user.GetActualProperty().HpMax);
                user.SetEnergy(Operate_Type.OT_Absolute, user.GetActualProperty().EnergyMax);
            }

            if (null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (role_info.Nickname.Length > 0)
                {
                    user.SetNickName(role_info.Nickname);
                }
                /// level
                if (role_info.Level > 0)
                {
                    user.SetLevel(role_info.Level);
                }
                /// equips
                if (null != role_info.Equips)
                {
                    for (int i = 0; i < role_info.Equips.Length; i++)
                    {
                        if (null != role_info.Equips[i])
                        {
                            int item_id = role_info.Equips[i].ItemId;
                            if (item_id > 0)
                            {
                                ItemDataInfo info = new ItemDataInfo();
                                info.Level = role_info.Equips[i].Level;
                                info.ItemNum = role_info.Equips[i].ItemNum;
                                info.RandomProperty = role_info.Equips[i].RandomProperty;
                                info.ItemConfig = ItemConfigProvider.Instance.GetDataById(item_id);
                                if (null != info.ItemConfig)
                                {
                                    user.GetEquipmentStateInfo().SetEquipmentData(i, info);
                                }
                            }
                        }
                    }
                }
                /// skills
                RefixSkills(user);
                /// legacys
                if (null != role_info.Legacys)
                {
                    for (int i = 0; i < role_info.Legacys.Length; i++)
                    {
                        if (null != role_info.Legacys[i] && role_info.Legacys[i].IsUnlock)
                        {
                            user.GetLegacyStateInfo().ResetLegacyData(i);
                            int item_id = role_info.Legacys[i].ItemId;
                            if (item_id > 0)
                            {
                                ItemDataInfo info = new ItemDataInfo();
                                info.Level = role_info.Legacys[i].Level;
                                info.ItemNum = role_info.Legacys[i].ItemNum;
                                info.RandomProperty = role_info.Legacys[i].RandomProperty;
                                info.ItemConfig = ItemConfigProvider.Instance.GetDataById(item_id);
                                if (null != info.ItemConfig)
                                {
                                    user.GetLegacyStateInfo().SetLegacyData(i, info);
                                }
                            }
                        }
                    }
                    user.GetLegacyStateInfo().UpdateLegacyComplexAttr();
                }
                /// partner
                if (null != role_info.PartnerStateInfo)
                {
                    user.SetPartnerInfo(role_info.PartnerStateInfo.GetActivePartner());
                }
                ///
                InitUserXSoulInfo(user, role_info);
                user.TalentManager = role_info.TalentManager;
                ///
                UserAttrCalculator.Calc(user);
                ///
                ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
                if (null != expedition && null != expedition.Tollgates && expedition.ActiveTollgate >= 0)
                {
                    int hpmax = user.GetActualProperty().HpMax;
                    int mpmax = user.GetActualProperty().EnergyMax;
                    int ragemax = user.GetActualProperty().RageMax;

                    if (0 == expedition.Schedule)
                    {
                        user.SetHp(Operate_Type.OT_Absolute, hpmax);
                        user.SetEnergy(Operate_Type.OT_Absolute, mpmax);
                        user.SetRage(Operate_Type.OT_Absolute, 0);
                    }
                    else
                    {
                        float blurhp = (float)expedition.Hp / 1000f * hpmax;
                        float blurmp = (float)expedition.Mp / 1000f * mpmax;
                        float blurrage = (float)expedition.Rage / 1000f * ragemax;
                        user.SetHp(Operate_Type.OT_Absolute, blurhp > (int)blurhp ? (int)blurhp + 1 : (int)blurhp);
                        user.SetEnergy(Operate_Type.OT_Absolute, blurmp > (int)blurmp ? (int)blurmp + 1 : (int)blurmp);
                        user.SetRage(Operate_Type.OT_Absolute, blurrage > (int)blurrage ? (int)blurrage + 1 : (int)blurrage);
                    }
                    expedition.MonsterDeadTime = -1;
                    expedition.UserDeadTime = -1;
                    expedition.StartTime = TimeUtility.CurTimestamp;
                    ///
                    ExpeditionPlayerInfo.TollgateData cur_tollgate_data = expedition.Tollgates[expedition.ActiveTollgate];
                    if (null != cur_tollgate_data)
                    {
                        cur_tollgate_data.IsDelayRefreshed = false;
                        if (EnemyType.ET_Monster == cur_tollgate_data.Type)
                        {
                            cur_tollgate_data.FlushNum = 2;
                        }
                        else
                        {
                            cur_tollgate_data.FlushNum = 1;
                        }
                    }
                }
            }

            EntityManager.Instance.CreatePlayerSelfView(1);
            UserView view = EntityManager.Instance.GetUserViewById(1);
            if (null != view)
            {
                view.Visible = true;
            }
            view.UpdateEquipment();
        }
        public void ExpeditionErrorCheck()
        {
            if (null == LobbyClient.Instance.CurrentRole)
                return;
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
            if (null != expedition && null != expedition.Tollgates && expedition.Schedule >= 0)
            {
                ExpeditionPlayerInfo.TollgateData cur_tollgate_data = expedition.Tollgates[expedition.Schedule];
                if (null != cur_tollgate_data)
                {
                    /// exception check
                    bool is_exception = false;
                    if (cur_tollgate_data.Type == EnemyType.ET_Monster || cur_tollgate_data.Type == EnemyType.ET_Boss)
                    {
                        int ct = cur_tollgate_data.EnemyList.Count;
                        if (ct <= 0)
                        {
                            is_exception = true;
                        }
                    }
                    else
                    {
                        int ct = cur_tollgate_data.UserImageList.Count;
                        if (ct <= 0)
                        {
                            is_exception = true;
                        }
                    }
                    if (is_exception)
                    {
                        cur_tollgate_data.Type = EnemyType.ET_Boss;
                        List<MatchExpeditionArg> monster_match_list = expedition.MatchMonster((int)role_info.FightingScore, MonsterType.MT_Boss);
                        if (null != monster_match_list)
                        {
                            cur_tollgate_data.EnemyList.Clear();
                            cur_tollgate_data.EnemyAttrList.Clear();
                            int mmlct = monster_match_list.Count;
                            for (int assit_index = 0; assit_index < mmlct; assit_index++)
                            {
                                cur_tollgate_data.EnemyList.Add(monster_match_list[assit_index].Id);
                                cur_tollgate_data.EnemyAttrList.Add(monster_match_list[assit_index].Attr);
                            }
                        }
                    }
                }
            }
        }
        private void DelayFlushMonster(UserInfo user)
        {
            if (null == user)
                return;
            if (null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
                if (null != expedition && null != expedition.Tollgates && expedition.ActiveTollgate >= 0)
                {
                    ExpeditionPlayerInfo.TollgateData cur_tollgate_data = expedition.Tollgates[expedition.ActiveTollgate];
                    if (null != cur_tollgate_data)
                    {
                        /*
                        if (EnemyType.ET_Monster == cur_tollgate_data.Type) {
                          cur_tollgate_data.FlushNum = 2;
                        } else {
                          cur_tollgate_data.FlushNum = 1;
                        } */
                        if (EnemyType.ET_Monster == cur_tollgate_data.Type || EnemyType.ET_Boss == cur_tollgate_data.Type)
                        {
                            if (null != cur_tollgate_data.EnemyList && cur_tollgate_data.EnemyList.Count > 0
                              && null != cur_tollgate_data.EnemyAttrList && cur_tollgate_data.EnemyAttrList.Count > 0
                              && cur_tollgate_data.EnemyList.Count == cur_tollgate_data.EnemyAttrList.Count)
                            {
                                int ct = cur_tollgate_data.EnemyList.Count;
                                //m_NpcMgr.Reset();
                                for (int index = 0; index < ct; index++)
                                {
                                    int enemy_id = cur_tollgate_data.EnemyList[index];
                                    if (enemy_id > 0)
                                    {
                                        foreach (Data_Unit npcUnit in m_CurScene.StaticData.m_UnitMgr.GetData().Values)
                                        {
                                            if (npcUnit.GetId() == enemy_id)
                                            {
                                                NpcInfo npc = m_NpcMgr.AddNpc(npcUnit);
                                                if (null != npc)
                                                {
                                                    ExpeditionTollgateConfig tollgate_config = ExpeditionTollgateConfigProvider.Instance.GetDataById(expedition.ActiveTollgate + 1);
                                                    if (null != tollgate_config && null != tollgate_config.m_Pos)
                                                    {
                                                        int pos_ct = tollgate_config.m_Pos.Count;
                                                        int pos_index = CrossEngineHelper.Random.Next(0, pos_ct);
                                                        Vector3 npc_pos = Converter.ConvertVector3D(tollgate_config.m_Pos[pos_index]);
                                                        if (Vector3.Zero != npc_pos)
                                                        {
                                                            npc.GetMovementStateInfo().SetPosition(npc_pos);
                                                        }
                                                    }
                                                    npc.SetLevel(user.GetLevel());
                                                    npc.SetAIEnable(true);
                                                    npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                                                    EntityManager.Instance.CreateNpcView(npc.GetId());
                                                    CustomNpcByUnitId(npc, npcUnit.GetId());
                                                    ///
                                                    int enemy_attr = cur_tollgate_data.EnemyAttrList[index];
                                                    if (enemy_attr > 0)
                                                    {
                                                        AddExpeditionAttr(npc, enemy_attr);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                --cur_tollgate_data.FlushNum;
                            }
                        }
                        else
                        {
                            /// user
                            if (null != cur_tollgate_data.UserImageList && cur_tollgate_data.UserImageList.Count > 0)
                            {
                                int ct = cur_tollgate_data.UserImageList.Count;
                                for (int index = 0; index < ct; index++)
                                {
                                    ExpeditionImageInfo image_info = cur_tollgate_data.UserImageList[index];
                                    if (null != image_info)
                                    {
                                        int image_res_id = image_info.HeroId;
                                        UserInfo image_player = m_UserMgr.AddUser(image_res_id);
                                        if (null != image_player)
                                        {
                                            image_info.Guid = (ulong)image_player.GetId();
                                            image_player.SceneContext = m_SceneContext;
                                            image_player.SetCampId((int)CampIdEnum.Hostile);
                                            Vector3 image_pos = user.GetMovementStateInfo().GetPosition3D();
                                            float image_dir = user.GetMovementStateInfo().GetFaceDir() - (float)Math.PI;
                                            ExpeditionTollgateConfig tollgate_config = ExpeditionTollgateConfigProvider.Instance.GetDataById(expedition.ActiveTollgate);
                                            if (null != tollgate_config && null != tollgate_config.m_Pos)
                                            {
                                                image_player.GetAiStateInfo().AiLogic = tollgate_config.m_ImageAiLogic;
                                                image_player.SetAIEnable(true);
                                                int pos_ct = tollgate_config.m_Pos.Count - 1;
                                                int pos_index = CrossEngineHelper.Random.Next(0, pos_ct);
                                                Vector3 user_image_pos = Converter.ConvertVector3D(tollgate_config.m_Pos[pos_index]);
                                                if (Vector3.Zero != user_image_pos)
                                                {
                                                    image_pos = user_image_pos;
                                                }
                                            }
                                            image_player.GetMovementStateInfo().SetPosition(image_pos.X, image_pos.Y, image_pos.Z);
                                            image_player.GetMovementStateInfo().SetFaceDir(image_dir);
                                            EntityManager.Instance.CreateUserView(image_player.GetId());
                                            image_player.SetLevel(image_info.Level);
                                            image_player.SetNickName(image_info.Nickname);
                                            image_player.SkillController = new SwordManSkillController(image_player, GfxModule.Skill.GfxSkillSystem.Instance);
                                            if (null != m_SpatialSystem)
                                            {
                                                m_SpatialSystem.AddObj(image_player.SpaceObject);
                                            }
                                            UserView image_view = EntityManager.Instance.GetUserViewById(image_player.GetId());
                                            if (image_view != null)
                                            {
                                            }
                                            /// skills
                                            if (null != image_info.Skills)
                                            {
                                                image_player.GetSkillStateInfo().RemoveAllSkill();
                                                int skill_ct = image_info.Skills.Count;
                                                for (int i = 0; i < skill_ct; i++)
                                                {
                                                    SkillInfo info = image_info.Skills[i];
                                                    if (null != info)
                                                    {
                                                        SkillCategory cur_skill_pos = SkillCategory.kNone;
                                                        if (info.Postions.Presets[0] == SlotPosition.SP_A)
                                                        {
                                                            cur_skill_pos = SkillCategory.kSkillA;
                                                        }
                                                        else if (info.Postions.Presets[0] == SlotPosition.SP_B)
                                                        {
                                                            cur_skill_pos = SkillCategory.kSkillB;
                                                        }
                                                        else if (info.Postions.Presets[0] == SlotPosition.SP_C)
                                                        {
                                                            cur_skill_pos = SkillCategory.kSkillC;
                                                        }
                                                        else if (info.Postions.Presets[0] == SlotPosition.SP_D)
                                                        {
                                                            cur_skill_pos = SkillCategory.kSkillD;
                                                        }
                                                        info.Category = cur_skill_pos;
                                                        image_player.GetSkillStateInfo().AddSkill(info);
                                                        WorldSystem.Instance.AddSubSkill(image_player, info.SkillId, cur_skill_pos, info.SkillLevel);
                                                    }
                                                }
                                            }
                                            Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(image_player.GetLinkId());
                                            if (null != playerData && null != playerData.m_FixedSkillList
                                              && playerData.m_FixedSkillList.Count > 0)
                                            {
                                                foreach (int skill_id in playerData.m_FixedSkillList)
                                                {
                                                    SkillInfo info = new SkillInfo(skill_id, 1);
                                                    image_player.GetSkillStateInfo().AddSkill(info);
                                                }
                                            }
                                            image_player.ResetSkill();
                                            /// equips
                                            if (null != image_info.Equips)
                                            {
                                                int equip_ct = image_info.Equips.Length;
                                                for (int i = 0; i < equip_ct; i++)
                                                {
                                                    ItemDataInfo image_equip = image_info.Equips[i];
                                                    if (null != image_equip)
                                                    {
                                                        image_player.GetEquipmentStateInfo().ResetEquipmentData(i);
                                                        image_equip.ItemConfig = ItemConfigProvider.Instance.GetDataById(image_equip.ItemId);
                                                        if (null != image_equip.ItemConfig)
                                                        {
                                                            image_player.GetEquipmentStateInfo().SetEquipmentData(i, image_equip);
                                                        }
                                                    }
                                                }
                                            }
                                            /// 
                                            if (null != image_info.Legacys)
                                            {
                                                int legacy_ct = image_info.Legacys.Length;
                                                for (int i = 0; i < legacy_ct; i++)
                                                {
                                                    ItemDataInfo image_legacy = image_info.Legacys[i];
                                                    if (null != image_legacy && image_legacy.IsUnlock)
                                                    {
                                                        image_player.GetLegacyStateInfo().ResetLegacyData(i);
                                                        image_legacy.ItemConfig = ItemConfigProvider.Instance.GetDataById(image_legacy.ItemId);
                                                        if (null != image_legacy.ItemConfig)
                                                        {
                                                            image_player.GetLegacyStateInfo().SetLegacyData(i, image_legacy);
                                                        }
                                                    }
                                                }
                                                image_player.GetLegacyStateInfo().UpdateLegacyComplexAttr();
                                            }
                                            if (Geometry.IsSameFloat(image_player.HpMaxCoefficient, 1))
                                            {
                                                float hpper = user.Hp / (float)user.GetActualProperty().HpMax;
                                                float mpper = user.Energy / (float)user.GetActualProperty().EnergyMax;
                                                ExpeditionPlayerInfo.CaclCoefficient(user, image_player);
                                                UserAttrCalculator.Calc(user);
                                                user.SetHp(Operate_Type.OT_Absolute, (int)(user.GetActualProperty().HpMax * hpper));
                                                user.SetEnergy(Operate_Type.OT_Absolute, (int)(user.GetActualProperty().EnergyMax * mpper));
                                            }
                                            UserAttrCalculator.Calc(image_player);
                                            image_player.SetHp(Operate_Type.OT_Absolute, image_player.GetActualProperty().HpMax);
                                            image_player.SetRage(Operate_Type.OT_Absolute, 0);
                                            image_player.SetEnergy(Operate_Type.OT_Absolute, image_player.GetActualProperty().EnergyMax);
                                        }
                                    }
                                }
                                --cur_tollgate_data.FlushNum;
                                SyncGfxUsersInfo();
                            }
                        }
                        cur_tollgate_data.IsDelayRefreshed = true;
                    }
                }
            }
        }

        internal void SyncGfxUsersInfo()
        {
            List<GfxUserInfo> gfxUsers = new List<GfxUserInfo>();
            LinkedListDictionary<int, UserInfo> users = UserManager.Users;
            for (LinkedListNode<UserInfo> node = users.FirstValue; null != node; node = node.Next)
            {
                UserInfo user = node.Value;
                if (null != user)
                {
                    UserView view = EntityManager.Instance.GetUserViewById(user.GetId());
                    if (null != view)
                    {
                        GfxUserInfo gfxUser = new GfxUserInfo();
                        gfxUser.m_ActorId = view.Actor;
                        gfxUser.m_HeroId = user.GetLinkId();
                        gfxUser.m_Level = user.GetLevel();
                        gfxUser.m_Nick = user.GetNickName();
                        gfxUsers.Add(gfxUser);
                    }
                }
            }
            GfxSystem.PublishGfxEvent("ge_show_name_plates", "ui", gfxUsers);
            
            // temp: gain newbie tools
            ArkCrossEngine.RoleInfo ri = ArkCrossEngine.LobbyClient.Instance.CurrentRole;
            if (ri != null && ri.Level < 10)
            {
                PlayerControl.Instance.ToolPool(0, (int)Keyboard.Event.Up);
            }
        }
        internal void SyncGfxUserInfo(int objId)
        {
            UserInfo user = UserManager.GetUserInfo(objId);
            if (null != user)
            {
                UserView view = EntityManager.Instance.GetUserViewById(user.GetId());
                if (null != view)
                {
                    GfxUserInfo gfxUser = new GfxUserInfo();
                    gfxUser.m_ActorId = view.Actor;
                    gfxUser.m_HeroId = user.GetLinkId();
                    gfxUser.m_Level = user.GetLevel();
                    gfxUser.m_Nick = user.GetNickName();

                    GfxSystem.PublishGfxEvent("ge_show_name_plate", "ui", gfxUser);
                }
            }
        }

        internal void SyncGfxNpcInfo(int objId)
        {
            NpcInfo npc = NpcManager.GetNpcInfo(objId);
            if (null != npc)
            {
                NpcView view = EntityManager.Instance.GetNpcViewById(npc.GetId());
                if (null != view)
                {
                    GfxUserInfo gfxUser = new GfxUserInfo();
                    gfxUser.m_ActorId = view.Actor;
                    gfxUser.m_HeroId = npc.GetLinkId();
                    gfxUser.m_Level = npc.GetLevel();
                    gfxUser.m_Nick = "";

                    GfxSystem.PublishGfxEvent("ge_show_npc_name_plate", "ui", gfxUser);
                }
            }
        }

        internal void AdditionNpcAttr(NpcInfo npc, int player_score)
        {
            if (null == npc)
                return;
            int npc_id = npc.GetId();
            int match_attr = -1;
            int min_monster_score = 10000;
            for (int i = 0; i < ExpeditionMonsterConfigProvider.Instance.GetDataCount(); ++i)
            {
                ExpeditionMonsterConfig monster_data = ExpeditionMonsterConfigProvider.Instance.GetDataById(i) as ExpeditionMonsterConfig;
                if (null != monster_data)
                {
                    int cur_score = monster_data.m_FightingScore;
                    if (cur_score < min_monster_score && cur_score > player_score)
                    {
                        min_monster_score = cur_score;
                    }
                }
            }
            if (min_monster_score > 0)
            {
                ExpeditionMonsterConfig monster_data = ExpeditionMonsterConfigProvider.Instance.GetDataById(min_monster_score) as ExpeditionMonsterConfig;
                if (null != monster_data && null != monster_data.m_LinkId && null != monster_data.m_AttributeId)
                {
                    int ct = monster_data.m_LinkId.Count;
                    for (int i = 0; i < ct; i++)
                    {
                        if (monster_data.m_LinkId[i] == npc_id)
                        {
                            match_attr = monster_data.m_AttributeId[i];
                            break;
                        }
                    }
                }
            }
            if (match_attr > 0)
            {
                AddExpeditionAttr(npc, match_attr);
            }
        }

        private void InitUserXSoulInfo(CharacterInfo user, RoleInfo role)
        {
            XSoulInfo<XSoulPartInfo> xsoul_info = role.GetXSoulInfo();
            if (xsoul_info == null)
            {
                return;
            }
            foreach (var pair in xsoul_info.GetAllXSoulPartData())
            {
                XSoulPartInfo part_info = pair.Value;
                user.GetXSoulInfo().SetXSoulPartData(pair.Key, part_info);
                foreach (int impactid in part_info.GetActiveImpacts())
                {
                    Vector3 user_pos = user.GetMovementStateInfo().GetPosition3D();
                    //LogSystem.Debug("-----add impact {0} to user!", impactid);
                    ImpactSystem.Instance.SendImpactToCharacter(user, impactid, user.GetId(),
                                  -1, -1, user_pos, user.GetMovementStateInfo().GetFaceDir());
                }
            }
        }

        private void AddExpeditionAttr(NpcInfo npc, int attr_id)
        {
            if (null == npc || attr_id <= 0)
                return;
            ExpeditionMonsterAttrConfig attr = ExpeditionMonsterAttrConfigProvider.Instance.GetExpeditionMonsterAttrConfigById(attr_id);
            if (null != attr && null != attr.m_AttrData)
            {
                npc.GetBaseProperty().SetHpMax(Operate_Type.OT_Relative, (int)attr.m_AttrData.GetAddHpMax(0, npc.GetLevel()));
                npc.GetBaseProperty().SetEnergyMax(Operate_Type.OT_Relative, (int)attr.m_AttrData.GetAddEpMax(0, npc.GetLevel()));
                npc.GetBaseProperty().SetAttackBase(Operate_Type.OT_Relative, (int)attr.m_AttrData.GetAddAd(0, npc.GetLevel()));
                npc.GetBaseProperty().SetADefenceBase(Operate_Type.OT_Relative, (int)attr.m_AttrData.GetAddADp(0, npc.GetLevel()));
                npc.GetBaseProperty().SetMDefenceBase(Operate_Type.OT_Relative, (int)attr.m_AttrData.GetAddMDp(0, npc.GetLevel()));
                npc.SetHp(Operate_Type.OT_Absolute, (int)attr.m_AttrData.GetAddHpMax(0, npc.GetLevel()));
                npc.SetEnergy(Operate_Type.OT_Absolute, (int)attr.m_AttrData.GetAddEpMax(0, npc.GetLevel()));
            }
        }
        private int GenImageId(int image_id)
        {
            UserInfo info = m_UserMgr.GetUserInfo(image_id);
            if (null == info)
            {
                return image_id;
            }
            else
            {
                int incr_id = image_id * 10000 + CrossEngineHelper.Random.Next(0, 100);
                return GenImageId(incr_id);
            }
        }
        private void CustomNpcByUnitId(NpcInfo npc, int unitId)
        {
            Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, unitId) as Data_Unit;
            if (null != mapUnit)
            {
                NpcView view = EntityManager.Instance.GetCharacterViewById(npc.GetId()) as NpcView;
                if (null != view)
                {
                    view.SetIdleAnim(mapUnit.m_IdleAnims);
                }
            }
        }
        private void DestroyHero()
        {
            if (null != m_PlayerSelf)
            {
                EntityManager.Instance.DestroyUserView(m_PlayerSelfId);
                DestroyCharacterById(m_PlayerSelfId);
            }
        }
        private void CreateSceneLogics()
        {
            MyDictionary<int, object> slogics = m_CurScene.StaticData.m_SceneLogicMgr.GetData();
            foreach (SceneLogicConfig sc in slogics.Values)
            {
                if (null != sc)
                {
                    if (sc.m_IsClient)
                    {
                        m_SceneLogicInfoMgr.AddSceneLogicInfo(sc.GetId(), sc);
                    }
                    else if (sc.m_IsServer)
                    {
                        if (IsPureClientScene() || IsPveScene())
                            m_SceneLogicInfoMgr.AddSceneLogicInfo(sc.GetId(), sc);
                    }
                }
            }
        }

        private void TickInteraction()
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (m_LastInteractionCheckTime + c_InteractionCheckInterval < curTime)
            {
                m_LastInteractionCheckTime = curTime;

                if (null != m_PlayerSelf)
                {
                    bool sendMsg = false;
                    int initiator = m_PlayerSelf.GetId();
                    int receiver = 0;

                    List<ISpaceObject> objs = m_SpatialSystem.GetObjectInCircle(m_PlayerSelf.GetMovementStateInfo().GetPosition3D(), 2);
                    foreach (ISpaceObject obj in objs)
                    {
                        if (obj.GetObjType() == SpatialObjType.kNPC)
                        {
                            NpcInfo npc = obj.RealObject as NpcInfo;
                            if (null != npc)
                            {
                                //LogSystem.Debug("TickInteraction id:{0} owner:{1} type:{2} self id:{3}", npc.GetId(), npc.OwnerId, npc.NpcType, m_PlayerSelf.GetId());
                                if (npc.OwnerId == m_PlayerSelf.GetId() && npc.NpcType == (int)NpcTypeEnum.InteractiveNpc)
                                {
                                    receiver = (int)obj.GetID();
                                    sendMsg = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (sendMsg)
                    {
                        //todo:通知UI切换普攻图标为对话图标

                    }
                }
            }
        }
        private void TickScrollMessage()
        {
            long curTime = TimeUtility.GetServerMilliseconds();
            if (curTime > m_LastScrollMessageTickTime + c_ScrollMessageTickInterval)
            {
                m_LastScrollMessageTickTime = curTime;

                DateTime dt = DateTime.Now;
                int ct = m_ScrollMessageInfos.Count;
                for (int i = 0; i < ct; ++i)
                {
                    ScrollMessageInfo info = m_ScrollMessageInfos[i];
                    if (info.m_Week >= 0)
                    {
                        if (info.m_Week != (int)dt.DayOfWeek)
                            continue;
                    }
                    TimeSpan ts = dt.TimeOfDay;
                    TimeSpan ts1 = info.m_Start.TimeOfDay;
                    TimeSpan ts2 = info.m_End.TimeOfDay;
                    if (ts >= ts1 && ts < ts2)
                    {
                        if (info.m_LastSendTime + info.m_Interval < curTime)
                        {
                            info.m_LastSendTime = curTime;

                            GfxSystem.PublishGfxEvent("ge_notice", "notice", info.m_Message, info.m_ScrollCount);
                        }
                    }
                }
            }
        }
        private void TickCityUsers()
        {
            if (!LobbyNetworkSystem.Instance.IsConnected || LobbyNetworkSystem.Instance.IsLogining)
            {
                //连接断开或登录过程中不进行tick
                return;
            }
            long curTime = TimeUtility.GetServerMilliseconds();
            if (curTime > m_LastCityUserTickTime + c_CityUserTickInterval)
            {
                m_LastCityUserTickTime = curTime;

                if (/*0 == m_CurCityUserTickCount % c_CityUserUpdatePositionTickNum &&*/ null != m_PlayerSelf)
                {
                    MovementStateInfo msi = m_PlayerSelf.GetMovementStateInfo();

                    JsonMessage updateMsg = new JsonMessage(JsonMessageID.UpdatePosition);
                    updateMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                    ArkCrossEngineMessage.Msg_CL_UpdatePosition protoData = new Msg_CL_UpdatePosition();
                    protoData.m_X = msi.PositionX;
                    protoData.m_Z = msi.PositionZ;
                    protoData.m_FaceDir = msi.GetFaceDir();

                    updateMsg.m_ProtoData = protoData;
                    JsonMessageDispatcher.SendMessage(updateMsg);

                    RequestCityUsers();
                }
                //请求某个玩家的位置信息
                int ix = 0;
                foreach (ulong guid in m_CityUsers.Keys)
                {
                    if (ix == m_CurCityUserTickCount)
                    {
                        JsonMessage reqMsg = new JsonMessage(JsonMessageID.RequestUserPosition);
                        reqMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                        ArkCrossEngineMessage.Msg_CL_RequestUserPosition reqProtoData = new Msg_CL_RequestUserPosition();
                        reqProtoData.m_User = guid;

                        reqMsg.m_ProtoData = reqProtoData;
                        JsonMessageDispatcher.SendMessage(reqMsg);
                    }
                    ++ix;
                }

                m_CurCityUserTickCount = (m_CurCityUserTickCount + 1) % c_MaxCityUsers;
            }
        }
        private void TickPve()
        {
            m_AiSystem.Tick();
            m_Profiler.aiSystemTickTime = TimeSnapshot.DoCheckPoint();

            ClientStorySystem.Instance.Tick();
            m_Profiler.storySystemTickTime = TimeSnapshot.DoCheckPoint();
        }
        private void TickRecover()
        {
            float hp_coefficient = 1.0f;
            float mp_coefficient = 1.0f;
            if (null != m_CurScene && null != m_CurScene.SceneConfig)
            {
                Data_SceneConfig scene_data = m_CurScene.SceneConfig;
                hp_coefficient = scene_data.m_RecoverHpCoefficient;
                mp_coefficient = scene_data.m_RecoverMpCoefficient;
            }
            long curTime = TimeUtility.GetServerMilliseconds();
            if (curTime > m_LastTickTimeForTickPerSecond + c_IntervalPerSecond)
            {
                m_LastTickTimeForTickPerSecond = curTime;
                ///
                for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                {
                    UserInfo info = linkNode.Value;
                    if (!info.IsDead())
                    {
                        float hpRecover = info.GetActualProperty().HpRecover * hp_coefficient;
                        float epRecover = info.GetActualProperty().EnergyRecover * mp_coefficient;
                        if (hpRecover > 0.0001)
                        {
                            if (info.Hp + (int)hpRecover >= info.GetActualProperty().HpMax)
                                info.SetHp(Operate_Type.OT_Absolute, (int)info.GetActualProperty().HpMax);
                            else
                                info.SetHp(Operate_Type.OT_Relative, (int)hpRecover);
                        }
                        if (epRecover > 0.0001)
                        {
                            if (info.Energy + (int)epRecover >= info.GetActualProperty().EnergyMax)
                                info.SetEnergy(Operate_Type.OT_Absolute, (int)info.GetActualProperty().EnergyMax);
                            else
                                info.SetEnergy(Operate_Type.OT_Relative, (int)epRecover);
                        }
                    }
                }
                for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
                {
                    NpcInfo info = linkNode.Value;
                    if (!info.IsDead())
                    {
                        float hpRecover = info.GetActualProperty().HpRecover;
                        float npRecover = info.GetActualProperty().EnergyRecover;
                        if (hpRecover > 0.0001)
                        {
                            if (info.Hp + (int)hpRecover >= info.GetActualProperty().HpMax)
                                info.SetHp(Operate_Type.OT_Absolute, (int)info.GetActualProperty().HpMax);
                            else
                                info.SetHp(Operate_Type.OT_Relative, (int)hpRecover);
                        }
                        if (npRecover > 0.0001)
                        {
                            if (info.Energy + (int)npRecover >= info.GetActualProperty().EnergyMax)
                                info.SetEnergy(Operate_Type.OT_Absolute, (int)info.GetActualProperty().EnergyMax);
                            else
                                info.SetEnergy(Operate_Type.OT_Relative, (int)npRecover);
                        }
                    }
                }
            }
        }
        private void TickUsers()
        {
            List<NpcInfo> deletes = new List<NpcInfo>();
            for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                UserInfo info = linkNode.Value;
                if (info.SkillController != null)
                {
                    info.SkillController.OnTick();
                }
                if (info.LevelChanged || info.GetSkillStateInfo().BuffChanged || info.GetEquipmentStateInfo().EquipmentChanged || info.GetLegacyStateInfo().LegacyChanged || info.ParterChanged)
                {
                    //LogSystem.Debug("UserAttrCalculate LevelChanged:{0} BuffChanged:{1} EquipmentChanged:{2} LegacyChanged:{3}", info.LevelChanged, info.GetSkillStateInfo().BuffChanged, info.GetEquipmentStateInfo().EquipmentChanged, info.GetLegacyStateInfo().LegacyChanged);
                    UserAttrCalculator.Calc(info);
                    info.LevelChanged = false;
                    info.ParterChanged = false;
                    info.GetSkillStateInfo().BuffChanged = false;
                    info.GetEquipmentStateInfo().EquipmentChanged = false;
                    info.GetLegacyStateInfo().LegacyChanged = false;
                }
                if (IsPvpScene() || IsMultiPveScene())
                {
                    if (info.IsGfxStateFlagChanged && info.GetId() == PlayerSelfId)
                    {
                        info.IsGfxStateFlagChanged = false;
                        NetworkSystem.Instance.SyncCharacterGfxState(info);
                    }
                }
                // 只显示自己的方向指示。
                if (info.GetId() == m_PlayerSelfId)
                {
                    UserView view = EntityManager.Instance.GetUserViewById(info.GetId());
                    if (null != view)
                    {
                        if (IsPveScene() || IsMultiPveScene())
                        {
                            int battleNpcCount = GetBattleNpcCount(m_PlayerSelf, CharacterRelation.RELATION_ENEMY);
                            if (battleNpcCount <= 0)
                            {
                                view.SetIndicatorInfo(true, 0, 0);
                            }
                            else if (battleNpcCount <= 3)
                            {
                                float minPowDist = 99999;
                                NpcInfo npc = GetNearestNpc(m_PlayerSelf, CharacterRelation.RELATION_ENEMY, out minPowDist);
                                if (null != npc && minPowDist > info.IndicatorDis * info.IndicatorDis)
                                {
                                    float dir = Geometry.GetDirFromVector(npc.GetMovementStateInfo().GetPosition3D() - info.GetMovementStateInfo().GetPosition3D());
                                    view.SetIndicatorInfo(true, dir, 1);
                                }
                                else
                                {
                                    view.SetIndicatorInfo(false, 0.0f, 1);
                                }
                            }
                            else
                            {
                                view.SetIndicatorInfo(false, 0.0f, 1);
                            }
                        }
                        else
                        {
                            view.SetIndicatorInfo(false, 0.0f, 1);
                        }
                    }
                }
                if (info.IsDead() && info.GetId() != m_PlayerSelfId && info.DeadTime <= 0)
                {
                    info.DeadTime = TimeUtility.GetServerMilliseconds();
                    // summoner
                    List<NpcInfo> nil = info.GetSkillStateInfo().GetSummonObject();
                    if (nil != null)
                    {
                        foreach (NpcInfo ni in nil)
                        {
                            if (ni.FollowSummonerDead)
                            {
                                deletes.Add(ni);
                            }
                        }
                    }
                    if (!IsPvapScene())
                    {
                        //partner
                        NpcInfo npc = NpcManager.GetNpcInfo(info.PartnerId);
                        if (null != npc)
                        {
                            npc.NeedDelete = true;
                        }
                    }
                }
            }
            if (deletes.Count > 0)
            {
                foreach (NpcInfo ni in deletes)
                {
                    PlayNpcDeadEffect(ni);
                    DestroyNpc(ni);
                }
                deletes.Clear();
            }
            // 连击
            if (null != m_PlayerSelf)
            {
                long curTime = TimeUtility.GetLocalMilliseconds();
                CombatStatisticInfo combatInfo = m_PlayerSelf.GetCombatStatisticInfo();
                if (combatInfo.MultiHitCount > 0 && combatInfo.HitCountBreakTime < curTime)
                {
                    combatInfo.MultiHitCount = 0;
                    GfxSystem.PublishGfxEvent("ge_hitcount", "ui", 0);
                }
            }
            UserInfo player = WorldSystem.Instance.GetPlayerSelf();
            if (null != player && player.Hp <= 0)
            {
                if (player.DeadTime <= 0)
                {
                    player.GetCombatStatisticInfo().AddDeadCount(1);  //死亡计数+1
                    if (player.SkillController != null)
                    {
                        player.SkillController.ForceInterruptCurSkill();
                    }

                    if (IsPveScene() || IsPureClientScene())
                    {
                        ClientStorySystem.Instance.SendMessage("userkilled", player.GetId(), 0);
                        ClientStorySystem.Instance.SendMessage("playerselfkilled", player.GetId());
                        FireAllUserKilled();
                    }
                    player.DeadTime = TimeUtility.GetServerMilliseconds();
                    if (WorldSystem.Instance.IsPveScene() && !IsExpeditionScene() && !IsPvapScene())
                    {
                        GfxSystem.PublishGfxEvent("ge_role_dead", "ui");
                    }
                    if (WorldSystem.Instance.IsMultiPveScene())
                    {
                        if (IsAttemptScene())
                        {
                            int ct = m_UserMgr.Users.Count;
                            if (ct > 1)
                            {
                                GfxSystem.PublishGfxEvent("ge_mpve_role_dead", "ui");
                            }
                        }
                    }
                    // 禁止输入
                    PlayerControl.Instance.EnableMoveInput = false;
                    PlayerControl.Instance.EnableRotateInput = false;
                    PlayerControl.Instance.EnableSkillInput = false;
                }
                if (!IsPvapScene())
                {
                    //partner
                    NpcInfo npc = NpcManager.GetNpcInfo(player.PartnerId);
                    if (null != npc)
                    {
                        npc.NeedDelete = true;
                        RoleInfo roleInfo = LobbyClient.Instance.CurrentRole;
                        if (null != roleInfo)
                        {
                            roleInfo.PartnerStateInfo.ActivePartnerHpPercent = 0;
                        }
                    }
                }
                ///
                ExpeditionUserDeadHandle();
            }
            ///
            ExpeditionImageDeadHandle();
        }
        private void TickNpcs()
        {
            List<NpcInfo> deletes = new List<NpcInfo>();
            List<NpcInfo> deletes2 = new List<NpcInfo>();
            List<NpcInfo> deletes3 = new List<NpcInfo>();
            for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (info.SkillController != null)
                {
                    info.SkillController.OnTick();
                }
                if (info.LevelChanged || info.GetSkillStateInfo().BuffChanged || info.GetEquipmentStateInfo().EquipmentChanged || info.GetLegacyStateInfo().LegacyChanged)
                {
                    NpcAttrCalculator.Calc(info);
                    info.LevelChanged = false;
                    info.GetSkillStateInfo().BuffChanged = false;
                    info.GetEquipmentStateInfo().EquipmentChanged = false;
                    info.GetLegacyStateInfo().LegacyChanged = false;
                }
                if (IsPvpScene() || IsMultiPveScene())
                {
                    if (info.IsGfxStateFlagChanged && info.OwnerId == PlayerSelfId)
                    {
                        info.IsGfxStateFlagChanged = false;
                        NetworkSystem.Instance.SyncCharacterGfxState(info);
                    }
                    if (info.IsDead() && info.DeadTime > 0 && TimeUtility.GetServerMilliseconds() - info.DeadAnimTime * 1000 - info.DeadTime > info.ReleaseTime)
                    {
                        if (info.OwnerId == WorldSystem.Instance.PlayerSelfId)
                        {
                            NetworkSystem.Instance.SyncDeleteDeadNpc(info.GetId());
                        }
                    }
                }
                if (info.IsDead() && info.EmptyBloodTime <= 0)
                {
                    CharacterView view = EntityManager.Instance.GetCharacterViewById(info.GetId());
                    if (null != view)
                    {
                        GfxSystem.SendMessage(view.Actor, "OnEventEmptyBlood", null);
                        int soundCount = info.DeadSounds.Count;
                        if (soundCount > 0)
                        {
                            string deadSound = info.DeadSounds[CrossEngineHelper.Random.Next(soundCount)];
                            if (!String.IsNullOrEmpty(deadSound))
                            {
                                GfxSystem.PlaySound(view.Actor, deadSound, info.GetMovementStateInfo().GetFaceDir());
                            }
                        }
                    }
                    info.EmptyBloodTime = TimeUtility.GetServerMilliseconds();
                    OnNpcKilled(info);
                }
                if (info.IsDead() && info.DeadTime <= 0)
                {
                    if (info.DeadSkillId > 0)
                    {
                        info.DeadTime = TimeUtility.GetServerMilliseconds();
                        info.GetSkillStateInfo().RemoveAllImpact();
                        info.UltraArmor = true;
                        info.SkillController.ForceStartSkill(info.DeadSkillId);
                    }
                    else
                    {
                        CharacterView view = EntityManager.Instance.GetCharacterViewById(info.GetId());
                        if (null != view)
                        {
                            if (view.ObjectInfo.IsDead || !info.GetSkillStateInfo().IsImpactControl())
                            {
                                info.DeadTime = TimeUtility.GetServerMilliseconds();
                                info.UltraArmor = false;
                                if (info.GetSkillStateInfo().IsSkillActivated())
                                {
                                    info.SkillController.ForceInterruptCurSkill();
                                }
                                info.GetSkillStateInfo().RemoveAllImpact();
                                //SendDeadImpact(info);
                            }
                            else
                            {
                                if (!IsPvpScene() && !IsMultiPveScene())
                                {
                                    if (info.IsHaveGfxStateFlag(GfxCharacterState_Type.Stiffness))
                                    {
                                        SendDeadImpact(info);
                                    }
                                }
                            }
                        }
                    }
                }
                if (info.IsBorning && IsNpcBornOver(info))
                {
                    info.IsBorning = false;
                    info.SetAIEnable(true);
                    info.SetStateFlag(Operate_Type.OT_RemoveBit, CharacterState_Type.CST_Invincible);
                    if (info.NpcType == (int)NpcTypeEnum.Partner)
                    {
                        CharacterView view = EntityManager.Instance.GetCharacterViewById(info.GetId());
                        if (null != view)
                        {
                            GfxSystem.SetLayer(view.Actor, "Monster");
                        }
                    }
                }
                if (info.LifeEndTime > 0 && TimeUtility.GetServerMilliseconds() > info.LifeEndTime)
                {
                    OnNpcKilled(info);
                    deletes.Add(info);
                }
                if ((int)NpcTypeEnum.Partner == info.NpcType && !IsPvapScene())
                {
                    if (null != m_PlayerSelf && info.OwnerId == m_PlayerSelfId && info.GetId() == m_PlayerSelf.PartnerId)
                    {
                        PartnerInfo pi = m_PlayerSelf.GetPartnerInfo();
                        if (null != pi && TimeUtility.GetServerMilliseconds() - pi.LastTickTime > pi.TickInterval)
                        {
                            info.SetHp(Operate_Type.OT_Relative, (int)pi.GetHpCostPerTick(info.GetActualProperty().HpMax));
                            pi.LastTickTime = TimeUtility.GetServerMilliseconds();
                            RoleInfo roleInfo = LobbyClient.Instance.CurrentRole;
                            if (null != roleInfo)
                            {
                                roleInfo.PartnerStateInfo.ActivePartnerHpPercent = 1.0f * info.Hp / info.GetActualProperty().HpMax;
                            }
                            if (info.Hp <= 0)
                            {
                                m_PlayerSelf.PartnerId = 0;
                            }
                        }
                    }
                }
                if (IsPureClientScene() || IsPveScene())
                {
                    if (info.NeedDelete)
                    {
                        deletes3.Add(info);
                    }
                    else
                    {
                        // 约定npc的高度低于130时，直接判定npc死亡。
                        /*
                        if (130.0f > info.GetMovementStateInfo().GetPosition3D().Y)
                        {
                            info.NeedDelete = true;
                            OnNpcKilled(info);
                        }
                        */
                        if (info.NeedDelete)
                        {
                            deletes.Add(info);
                        }
                        else if (info.IsDead() && info.DeadTime > 0 &&
                                TimeUtility.GetServerMilliseconds() - info.DeadAnimTime * 1000 - info.DeadTime > info.ReleaseTime)
                        {
                            if (!IsPvapScene())
                            {
                                if (info.OwnerId == WorldSystem.Instance.PlayerSelfId)
                                {
                                    NetworkSystem.Instance.SyncDeleteDeadNpc(info.GetId());
                                }
                                deletes.Add(info);
                            }
                            else
                            {
                                if (info.SkillController != null)
                                {
                                    info.SkillController.ForceInterruptCurSkill();
                                }
                            }
                        }
                    }
                }
                if (info.IsDead())
                {
                    List<NpcInfo> nil = info.GetSkillStateInfo().GetSummonObject();
                    if (nil != null)
                    {
                        foreach (NpcInfo ni in nil)
                        {
                            if (ni.FollowSummonerDead)
                            {
                                deletes2.Add(ni);
                            }
                        }
                    }
                }
            }
            if (deletes2.Count > 0)
            {
                for (int i = 0; i < deletes2.Count; ++i)
                {
                    NpcInfo ni = deletes2[i];
                    PlayNpcDeadEffect(ni);
                    DestroyNpc(ni);
                }
                deletes2.Clear();
            }
            if (deletes3.Count > 0)
            {
                for (int i = 0; i < deletes3.Count; ++i)
                {
                    DestroyNpc(deletes3[i]);
                }
                deletes3.Clear();
            }
            if (deletes.Count > 0)
            {
                for (int i = 0; i < deletes.Count; ++i)
                {
                    NpcInfo ni = deletes[i];
                    PlayNpcDeadEffect(ni);
                    DestroyNpc(ni);
                }
            }
            ///
            ExpeditionNpcDeadHandle();
        }


        private NpcInfo GetNearestNpc(CharacterInfo src, CharacterRelation relation, out float minPowDist)
        {
            NpcInfo result = null;
            float powDist = 0.0f;
            minPowDist = 99999.0f;
            Vector3 pos = src.GetMovementStateInfo().GetPosition3D();
            for (LinkedListNode<NpcInfo> linkNode = NpcManager.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo npc = linkNode.Value;
                if (null != npc && npc.IsCombatNpc() && CharacterInfo.GetRelation(src, npc) == relation)
                {
                    powDist = Geometry.DistanceSquare(pos, npc.GetMovementStateInfo().GetPosition3D());
                    if (minPowDist > powDist)
                    {
                        result = npc;
                        minPowDist = powDist;
                    }
                }
            }
            return result;
        }
        private void UpdateCamera()
        {
            if (m_IsStroyProceed)
                return;
            // 测试摄像机根据是否进入战斗调整距离，后面会把参数开放到脚本层.
            // 目前只考虑了单人pve情况.
            if (IsPveScene() && !IsExpeditionScene() && !IsPvapScene())
            {
                /*
                bool isCameraInCombatState = m_IsCameraInCombatState;
                if (IsPlayerInCombatState() && NpcManager.HasCombatNpcAlive())
                {
                    isCameraInCombatState = true;
                }
                else if (!NpcManager.HasCombatNpcAlive() && m_IsAreaClear || m_ForceUpdateCamerState)
                {
                    isCameraInCombatState = false;
                    m_IsAreaClear = false;
                }
                if (m_IsCameraInCombatState != isCameraInCombatState || m_ForceUpdateCamerState)
                {
                    m_ForceUpdateCamerState = false;
                    m_IsCameraInCombatState = isCameraInCombatState;
                    CameraConfig cameraConfig = CameraConfigProvider.Instance.GetCameraConfig();
                    if (null == cameraConfig || !cameraConfig.IsActive) return;
                    float[] disArgs = new float[2];
                    float[] heightArgs = new float[2];
                    float[] initArgs = new float[2];
                    if (m_IsCameraInCombatState)
                    {
                        disArgs[0] = cameraConfig.DisInCombat;
                        disArgs[1] = cameraConfig.EnterBattleTime;
                        heightArgs[0] = cameraConfig.HeightInCombat;
                        heightArgs[1] = cameraConfig.EnterBattleTime;
                        initArgs[0] = cameraConfig.DisInCombat;
                        initArgs[1] = cameraConfig.HeightInCombat;
                    }
                    else
                    {
                        disArgs[0] = cameraConfig.DisInIdle;
                        disArgs[1] = cameraConfig.LeaveBattleTime;
                        heightArgs[0] = cameraConfig.HeightInIdle;
                        heightArgs[1] = cameraConfig.LeaveBattleTime;
                        initArgs[0] = cameraConfig.DisInIdle;
                        initArgs[1] = cameraConfig.HeightInIdle;
                    }
                    GfxSystem.SendMessage("GfxGameRoot", "CameraDistance", disArgs);
                    GfxSystem.SendMessage("GfxGameRoot", "CameraHeight", heightArgs);
                    GfxSystem.SendMessage("GfxGameRoot", "CameraInit", initArgs);
                }
                */
            }
            else
            {
                CameraConfig cameraConfig = CameraConfigProvider.Instance.GetCameraConfig();
                if (null == cameraConfig || !cameraConfig.IsActive) return;
                m_IsCameraInCombatState = !m_IsCameraInCombatState;
                float[] disArgs = new float[2];
                float[] heightArgs = new float[2];
                float[] initArgs = new float[2];
                disArgs[0] = cameraConfig.DisInIdle;
                disArgs[1] = 10.0f;
                GfxSystem.SendMessage("GfxGameRoot", "CameraDistance", disArgs);
                heightArgs[0] = cameraConfig.HeightInIdle;
                heightArgs[1] = 10.0f;
                GfxSystem.SendMessage("GfxGameRoot", "CameraHeight", heightArgs);
                initArgs[0] = cameraConfig.DisInMainCity;
                initArgs[1] = cameraConfig.HeightInMainCity;
                GfxSystem.SendMessage("GfxGameRoot", "CameraInit", initArgs);
            }
            if (IsPureClientScene())
            {
                if (NpcManager.HasCombatNpc() != m_IsCameraInCombatState)
                {
                    CameraConfig cameraConfig = CameraConfigProvider.Instance.GetCameraConfig();
                    if (null == cameraConfig || !cameraConfig.IsActive) return;
                    m_IsCameraInCombatState = !m_IsCameraInCombatState;
                    float[] disArgs = new float[2];
                    float[] heightArgs = new float[2];
                    float[] initArgs = new float[2];
                    disArgs[0] = cameraConfig.DisInMainCity;
                    disArgs[1] = 10.0f;
                    GfxSystem.SendMessage("GfxGameRoot", "CameraDistance", disArgs);
                    heightArgs[0] = cameraConfig.HeightInMainCity;
                    heightArgs[1] = 10.0f;
                    GfxSystem.SendMessage("GfxGameRoot", "CameraHeight", heightArgs);
                    initArgs[0] = cameraConfig.DisInMainCity;
                    initArgs[1] = cameraConfig.HeightInMainCity;
                    GfxSystem.SendMessage("GfxGameRoot", "CameraInit", initArgs);
                }
            }
        }
        private bool IsPlayerInCombatState()
        {
            UserView view = EntityManager.Instance.GetUserViewById(m_PlayerSelfId);
            if (null != view)
            {
                return view.IsCombatState;
            }
            return false;
        }
        private void TickSystemByCharacters()
        {
            for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                CharacterInfo character = linkNode.Value;
                ImpactSystem.Instance.Tick(character);
            }

            for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                CharacterInfo character = linkNode.Value;
                ImpactSystem.Instance.Tick(character);
            }
        }
        private void TickMoveMeetObstacle()
        {
            UserInfo myself = GetPlayerSelf();
            if (null != myself && myself.GetMovementStateInfo().IsMoving && myself.GetMovementStateInfo().IsMoveMeetObstacle)
            {
                long curTime = TimeUtility.GetLocalMilliseconds();
                if (m_LastNotifyMoveMeetObstacleTime + 100 <= curTime)
                {
                    NotifyMoveMeetObstacle(true);
                    m_LastNotifyMoveMeetObstacleTime = TimeUtility.GetLocalMilliseconds();
                }
            }
        }

        private void RequestCityUsers()
        {
            if (m_CityUsers.Count < c_MaxCityUsers)
            {
                int count = c_MaxCityUsers - m_CityUsers.Count;
                JsonMessage reqMsg = new JsonMessage(JsonMessageID.RequestUsers);
                reqMsg.m_JsonData.Set("m_Guid", LobbyNetworkSystem.Instance.Guid);
                Msg_CL_RequestUsers protoData = new Msg_CL_RequestUsers();
                protoData.m_Count = count;
                protoData.m_AlreadyExists.AddRange(m_CityUsers.Keys);

                reqMsg.m_ProtoData = protoData;
                JsonMessageDispatcher.SendMessage(reqMsg);
            }
        }
        private void DestroyNpc(NpcInfo ni)
        {
            if (ni.IsCombatNpc())
            {
                CalcKillIncome(ni);
                ni.DeadTime = 0;
                if (ni.SkillController != null)
                {
                    ni.SkillController.ForceInterruptCurSkill();
                }
            }
            EntityManager.Instance.DestroyNpcView(ni.GetId());
            WorldSystem.Instance.DestroyCharacterById(ni.GetId());
        }
        private void PlayNpcDeadEffect(NpcInfo ni)
        {
            if (null != ni)
            {
                CharacterView view = EntityManager.Instance.GetCharacterViewById(ni.GetId());
                if (null != view)
                {
                    GfxSystem.SendMessage(view.Actor, "OnEventDead", ni.NpcType);
                }
            }
        }
        private void SendDeadImpact(NpcInfo npc)
        {
            if (npc.IsCombatNpc())
            {
                CharacterView view = EntityManager.Instance.GetCharacterViewById(npc.GetId());
                if (null != view)
                {
                    GfxSystem.QueueGfxAction(GfxModule.Impact.GfxImpactSystem.Instance.SendDeadImpact, view.Actor);
                }
            }
        }
        private void OnNpcKilled(NpcInfo info)
        {
            if (IsPveScene() || IsPureClientScene())
            {
                if (info.IsCombatNpc())
                {
                    ClientStorySystem.Instance.SendMessage("objkilled", info.GetId(), GetBattleNpcCount());
                    ClientStorySystem.Instance.SendMessage(string.Format("npckilled:{0}", info.GetUnitId()), info.GetId(), GetBattleNpcCount());
                    TryFireAllNpcKilled(info.GetId());
                }
            }
        }
        private void TryFireAllNpcKilled(int deadNpcId)
        {
            int ct = GetBattleNpcCount();
            LogSystem.Debug("npc {0} killed, left {1}", deadNpcId, ct);
            if (0 == ct)
            {
                ClientStorySystem.Instance.SendMessage("allnpckilled");
            }
        }
        private void FireAllUserKilled()
        {
            ClientStorySystem.Instance.SendMessage("alluserkilled");
        }
        internal int GetBattleNpcCount()
        {
            int ct = 0;
            for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (null != info && info.EmptyBloodTime <= 0 && (info.NpcType == (int)NpcTypeEnum.Normal || info.NpcType == (int)NpcTypeEnum.BigBoss || info.NpcType == (int)NpcTypeEnum.LittleBoss))
                {
                    ++ct;
                }
            }
            return ct;
        }
        internal int GetBattleNpcCount(CharacterInfo src, CharacterRelation relation)
        {
            int ct = 0;
            for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next)
            {
                NpcInfo info = linkNode.Value;
                if (null != info && info.EmptyBloodTime <= 0 && (info.NpcType == (int)NpcTypeEnum.Normal || info.NpcType == (int)NpcTypeEnum.BigBoss || info.NpcType == (int)NpcTypeEnum.LittleBoss) && CharacterInfo.GetRelation(src, info) == relation)
                {
                    ++ct;
                }
            }
            return ct;
        }
        internal int GetLivingUserCount()
        {
            UserInfo user = WorldSystem.Instance.GetPlayerSelf();
            return (null != user && !user.IsDead() ? 1 : 0);
        }
        private void TryFlushExpeditionMonster()
        {
            UserInfo player = WorldSystem.Instance.GetPlayerSelf();
            if (null != player && null != LobbyClient.Instance.CurrentRole)
            {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (null != role_info)
                {
                    ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
                    if (null != expedition && expedition.ActiveTollgate >= 0
                      && null != expedition.Tollgates && expedition.ActiveTollgate < expedition.Tollgates.Length)
                    {
                        ExpeditionPlayerInfo.TollgateData tollgate_data = expedition.Tollgates[expedition.ActiveTollgate];
                        if (null != tollgate_data && tollgate_data.FlushNum > 0)
                        {
                            if (EnemyType.ET_Monster == tollgate_data.Type)
                            {
                                if (null != tollgate_data.EnemyList && tollgate_data.EnemyList.Count > 0
                                  && null != tollgate_data.EnemyAttrList && tollgate_data.EnemyAttrList.Count > 0
                                  && tollgate_data.EnemyList.Count == tollgate_data.EnemyAttrList.Count)
                                {
                                    int ct = tollgate_data.EnemyList.Count;
                                    //m_NpcMgr.Reset();
                                    for (int index = 0; index < ct; index++)
                                    {
                                        int enemy_id = tollgate_data.EnemyList[index];
                                        if (enemy_id > 0)
                                        {
                                            foreach (Data_Unit npcUnit in m_CurScene.StaticData.m_UnitMgr.GetData().Values)
                                            {
                                                if (npcUnit.GetId() == enemy_id)
                                                {
                                                    NpcInfo npc = m_NpcMgr.AddNpc(npcUnit);
                                                    if (null != npc)
                                                    {
                                                        ExpeditionTollgateConfig tollgate_config = ExpeditionTollgateConfigProvider.Instance.GetDataById(expedition.ActiveTollgate + 1);
                                                        if (null != tollgate_config && null != tollgate_config.m_Pos)
                                                        {
                                                            int pos_ct = tollgate_config.m_Pos.Count - 1;
                                                            int pos_index = CrossEngineHelper.Random.Next(0, pos_ct);
                                                            Vector3 npc_pos = Converter.ConvertVector3D(tollgate_config.m_Pos[pos_index]);
                                                            if (Vector3.Zero != npc_pos)
                                                            {
                                                                npc.GetMovementStateInfo().SetPosition(npc_pos);
                                                            }
                                                        }
                                                        npc.SetLevel(player.GetLevel());
                                                        npc.SetAIEnable(true);
                                                        npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                                                        EntityManager.Instance.CreateNpcView(npc.GetId());
                                                        CustomNpcByUnitId(npc, npcUnit.GetId());
                                                        ///
                                                        int enemy_attr = tollgate_data.EnemyAttrList[index];
                                                        if (enemy_attr > 0)
                                                        {
                                                            AddExpeditionAttr(npc, enemy_attr);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    --tollgate_data.FlushNum;
                                    tollgate_data.IsDoClear = false;
                                }
                            }
                        }
                        else
                        {
                            double interval_from_start = TimeUtility.CurTimestamp - expedition.StartTime;
                            if (interval_from_start > 1 && !tollgate_data.IsPostResult)
                            {
                                /// expedition finish
                                int scene_id = GetCurSceneId();
                                int tollgate_num = expedition.ActiveTollgate;
                                UserInfo user = GetPlayerSelf();
                                int hp = user.Hp;
                                int mp = user.Energy;
                                int rage = user.Rage;
                                int hp_max = user.GetActualProperty().HpMax;
                                int mp_max = user.GetActualProperty().EnergyMax;
                                int rage_max = user.GetActualProperty().RageMax;
                                if (tollgate_data.Type == EnemyType.ET_OnePlayer || tollgate_data.Type == EnemyType.ET_TwoPlayer)
                                {
                                    if (user.HpMaxCoefficient > 0)
                                    {
                                        hp = (int)(hp / user.HpMaxCoefficient);
                                        hp_max = (int)(hp_max / user.HpMaxCoefficient);
                                    }
                                    if (user.EnergyMaxCoefficient > 0)
                                    {
                                        mp = (int)(mp / user.EnergyMaxCoefficient);
                                        mp_max = (int)(mp_max / user.EnergyMaxCoefficient);
                                    }
                                    user.HpMaxCoefficient = 1;
                                    user.EnergyMaxCoefficient = 1;
                                }
                                int hp_per = (int)((float)hp / (float)(hp_max > 0 ? hp_max : 1) * 1000);
                                int mp_per = (int)((float)mp / (float)(mp_max > 0 ? mp_max : 1) * 1000);
                                int rage_per = (int)((float)rage / (float)(rage_max > 0 ? rage_max : 1) * 1000);
                                bool isWinner = hp > 0 ? true : false;
                                GfxSystem.EventChannelForLogic.Publish("ge_finish_expedition", "lobby", scene_id, tollgate_num, hp_per, mp_per, rage_per);
                                GfxSystem.PublishGfxEvent("ge_finish_expedition", "expedition", tollgate_num, isWinner, hp, mp, hp_max, mp_max, rage);
                                tollgate_data.IsPostResult = true;
                            }
                        }
                    }
                }
            }
        }
        ///
        private void ExpeditionUserDeadHandle()
        {
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (IsExpeditionScene() && null != role_info)
            {
                ExpeditionPlayerInfo expedition = role_info.GetExpeditionInfo();
                if (null != expedition && expedition.ActiveTollgate >= 0
                  && null != expedition.Tollgates && expedition.ActiveTollgate < expedition.Tollgates.Length)
                {
                    ExpeditionPlayerInfo.TollgateData tollgate_data = expedition.Tollgates[expedition.ActiveTollgate];
                    if (null != tollgate_data)
                    {
                        if (expedition.UserDeadTime < 0)
                        {
                            expedition.UserDeadTime = TimeUtility.CurTimestamp;
                        }
                        if (expedition.UserDeadTime > 0
                          && TimeUtility.CurTimestamp - expedition.UserDeadTime > ExpeditionPlayerInfo.c_FlushInterval && !tollgate_data.IsPostResult)
                        {
                            expedition.UserDeadTime = -1;
                            expedition.Hp = 0;
                            expedition.Mp = 0;
                            expedition.Rage = 0;
                            int hp = expedition.Hp;
                            int mp = expedition.Mp;
                            int rage = expedition.Rage;
                            UserInfo user = GetPlayerSelf();
                            int hp_max = user.GetActualProperty().HpMax;
                            int mp_max = user.GetActualProperty().EnergyMax;
                            if (tollgate_data.Type == EnemyType.ET_OnePlayer || tollgate_data.Type == EnemyType.ET_TwoPlayer)
                            {
                                if (user.HpMaxCoefficient > 0)
                                {
                                    hp_max = (int)(hp_max / user.HpMaxCoefficient);
                                }
                                if (user.EnergyMaxCoefficient > 0)
                                {
                                    mp_max = (int)(mp_max / user.EnergyMaxCoefficient);
                                }
                                user.HpMaxCoefficient = 1;
                                user.EnergyMaxCoefficient = 1;
                            }
                            bool isWinner = hp > 0 ? true : false;
                            int tollgate_num = expedition.ActiveTollgate;
                            GfxSystem.PublishGfxEvent("ge_finish_expedition", "expedition", tollgate_num, isWinner, hp, mp, hp_max, mp_max, rage);
                            GfxSystem.EventChannelForLogic.Publish("ge_expedition_failure", "lobby");
                            tollgate_data.IsPostResult = true;
                        }
                    }
                }
            }
        }
        ///
        private bool PlayMixCool()
        {
            /*
            bool ret = false;
            UserInfo player = WorldSystem.Instance.GetPlayerSelf();
            if (null != player) {
              if (!player.GetSkillStateInfo().IsSkillActivated()) {
                CharacterView player_view = EntityManager.Instance.GetCharacterViewById(player.GetId());
                if (null != player_view) {
                  player_view.PlayBeCoolAnimation();
                  ret = true;
                }
              }
            }
            return ret;*/
            return true;
        }
        ///
        private void ExpeditionImageDeadHandle()
        {
            if (IsExpeditionScene() && null != LobbyClient.Instance.CurrentRole)
            {
                ExpeditionPlayerInfo expedition = LobbyClient.Instance.CurrentRole.GetExpeditionInfo();
                if (null != expedition.Tollgates && expedition.ActiveTollgate >= 0
                  && expedition.ActiveTollgate < expedition.Tollgates.Length && expedition.StartTime > 0
                  && TimeUtility.CurTimestamp - expedition.StartTime > ExpeditionPlayerInfo.c_DelayFlushTime)
                {
                    ExpeditionPlayerInfo.TollgateData tollgate_data = expedition.Tollgates[expedition.ActiveTollgate];
                    if (null != tollgate_data)
                    {
                        if (EnemyType.ET_OnePlayer == tollgate_data.Type || EnemyType.ET_TwoPlayer == tollgate_data.Type)
                        {
                            int ct = 0;
                            int mark_id = 0;
                            for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next)
                            {
                                UserInfo info = linkNode.Value;
                                if (null != info && info.Hp > 0)
                                {
                                    mark_id = info.GetId();
                                    ++ct;
                                }
                            }
                            UserInfo player = WorldSystem.Instance.GetPlayerSelf();
                            if (null != player && 1 == ct && null != player && mark_id == player.GetId())
                            {
                                if (expedition.UserDeadTime < 0)
                                {
                                    ClientStorySystem.Instance.SendMessage("expeditioncompleted");
                                    expedition.UserDeadTime = TimeUtility.CurTimestamp;
                                    GfxSystem.PublishGfxEvent("ge_hide_input_ui", "ui");
                                }
                                double interval_time = TimeUtility.CurTimestamp - expedition.UserDeadTime;
                                if (expedition.UserDeadTime > 0
                                && interval_time > ExpeditionPlayerInfo.c_MixCoolTimePoint)
                                {
                                    if (interval_time > ExpeditionPlayerInfo.c_FlushInterval + 1)
                                    {
                                        if (!tollgate_data.IsPlayAnim)
                                        {
                                            if (PlayMixCool())
                                            {
                                                tollgate_data.IsPlayAnim = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!tollgate_data.IsPlayAnim && tollgate_data.FlushNum <= 0)
                                        {
                                            if (PlayMixCool())
                                            {
                                                tollgate_data.IsPlayAnim = true;
                                            }
                                        }
                                    }
                                }
                                if (expedition.UserDeadTime > 0)
                                {
                                    if (!tollgate_data.IsDoClear)
                                    {
                                        int clear_type = tollgate_data.FlushNum > 0 ? 0 : 1;
                                        GfxSystem.PublishGfxEvent("pve_area_clear", "ui_effect", clear_type);
                                        tollgate_data.IsDoClear = true;
                                    }
                                    if (TimeUtility.CurTimestamp - expedition.UserDeadTime > ExpeditionPlayerInfo.c_FlushInterval)
                                    {
                                        TryFlushExpeditionMonster();
                                        expedition.UserDeadTime = -1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        ///
        private void ExpeditionNpcDeadHandle()
        {
            if (IsExpeditionScene())
            {
                int ct = GetBattleNpcCount();
                if (ct > 0)
                    return;
                if (null != LobbyClient.Instance.CurrentRole)
                {
                    ExpeditionPlayerInfo expedition = LobbyClient.Instance.CurrentRole.GetExpeditionInfo();
                    if (null != expedition && null != expedition.Tollgates && expedition.StartTime > 0
                      && TimeUtility.CurTimestamp - expedition.StartTime > ExpeditionPlayerInfo.c_DelayFlushTime)
                    {
                        ExpeditionPlayerInfo.TollgateData tollgate_data = expedition.Tollgates[expedition.ActiveTollgate];
                        if (null != tollgate_data
                          && (tollgate_data.Type == EnemyType.ET_Boss || tollgate_data.Type == EnemyType.ET_Monster))
                        {
                            if (expedition.MonsterDeadTime < 0)
                            {
                                expedition.MonsterDeadTime = TimeUtility.CurTimestamp;
                                if (tollgate_data.FlushNum <= 0)
                                {
                                    ClientStorySystem.Instance.SendMessage("expeditioncompleted");
                                    GfxSystem.PublishGfxEvent("ge_hide_input_ui", "ui");
                                }
                            }
                            double interval_time = TimeUtility.CurTimestamp - expedition.MonsterDeadTime;
                            if (expedition.MonsterDeadTime > 0 && interval_time > ExpeditionPlayerInfo.c_MixCoolTimePoint)
                            {
                                if (interval_time > ExpeditionPlayerInfo.c_FlushInterval + 1)
                                {
                                    if (!tollgate_data.IsPlayAnim)
                                    {
                                        if (PlayMixCool())
                                        {
                                            tollgate_data.IsPlayAnim = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!tollgate_data.IsPlayAnim && tollgate_data.FlushNum <= 0)
                                    {
                                        if (PlayMixCool())
                                        {
                                            tollgate_data.IsPlayAnim = true;
                                        }
                                    }
                                }
                            }
                            if (expedition.MonsterDeadTime > 0)
                            {
                                if (!tollgate_data.IsDoClear)
                                {
                                    int clear_type = tollgate_data.FlushNum > 0 ? 0 : 1;
                                    GfxSystem.PublishGfxEvent("pve_area_clear", "ui_effect", clear_type);
                                    tollgate_data.IsDoClear = true;
                                }
                                if (TimeUtility.CurTimestamp - expedition.MonsterDeadTime > ExpeditionPlayerInfo.c_FlushInterval)
                                {
                                    TryFlushExpeditionMonster();
                                    expedition.MonsterDeadTime = -1;
                                }
                            }
                        }
                    }
                }
            }
        }
        ///
        private void CalcKillIncome(NpcInfo npc)
        {
            if (this.IsPureClientScene() || IsPveScene())
            {
                if (npc.KillerId == WorldSystem.Instance.PlayerSelfId)
                {
                    UserInfo userKiller = WorldSystem.Instance.GetPlayerSelf();
                    Data_SceneDropOut dropOutInfo = m_CurScene.SceneDropOut;
                    if (null != dropOutInfo)
                    {
                        if (m_CurScene.GetDropMoney(npc.GetUnitId()) > 0)
                        {
                            DropNpc(DropOutType.GOLD, dropOutInfo.m_GoldModel, dropOutInfo.m_GoldParticle, m_CurScene.GetDropMoney(npc.GetUnitId()), npc);
                        }
                        if (m_CurScene.GetDropHp(npc.GetUnitId()) > 0)
                        {
                            DropNpc(DropOutType.HP, dropOutInfo.m_HpModel, dropOutInfo.m_HpParticle, m_CurScene.GetDropHp(npc.GetUnitId()), npc);
                        }
                        if (m_CurScene.GetDropMp(npc.GetUnitId()) > 0)
                        {
                            DropNpc(DropOutType.MP, dropOutInfo.m_MpModel, dropOutInfo.m_MpParticle, m_CurScene.GetDropMp(npc.GetUnitId()), npc);
                        }
                    }
                }
            }
        }

        private void GetPlayerInfoForCache(ref List<int> userList, ref List<int> userSkillList)
        {
            UserInfo playerSelf = GetPlayerSelf();
            if (playerSelf != null)
            {
                userList.Add(playerSelf.GetLinkId());
                List<SkillInfo> allSkills = playerSelf.GetSkillStateInfo().GetAllSkill();
                foreach (SkillInfo skill in allSkills)
                {
                    if (skill.ConfigData.Category != SkillCategory.kNone)
                    {
                        userSkillList.Add(skill.ConfigData.GetId());
                    }
                }
            }

            //List<SkillInfo> userSkills = null;
            //for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
            //  UserInfo info = linkNode.Value;
            //  if (info != null) {
            //    userList.Add(info.GetLinkId());
            //    userSkills = info.GetSkillStateInfo().GetAllSkill();
            //    foreach (SkillInfo skill in userSkills) {
            //      if (skill.ConfigData.Category != SkillCategory.kNone) {
            //        userSkillList.Add(skill.ConfigData.GetId());
            //      }
            //    }
            //  }
            //}
        }
        private void DropNpc(DropOutType dropType, string model, string particle, int value, NpcInfo npc)
        {
            DropNpc(dropType, npc.GetId(), model, particle, value);
        }
        internal void DropNpc(DropOutType dropType, int fromNpcId, string model, string particle, int value)
        {
            Data_Unit unit = new Data_Unit();
            unit.m_Id = -1;
            switch (dropType)
            {
                case DropOutType.GOLD:
                    unit.m_LinkId = (int)DropNpcTypeEnum.GOLD;
                    break;
                case DropOutType.HP:
                    unit.m_LinkId = (int)DropNpcTypeEnum.HP;
                    break;
                case DropOutType.MP:
                    unit.m_LinkId = (int)DropNpcTypeEnum.MP;
                    break;
                case DropOutType.MULT_GOLD:
                    unit.m_LinkId = (int)DropNpcTypeEnum.MUTI_GOLD;
                    break;
            }
            unit.m_AiLogic = (int)AiStateLogicId.DropOut_AutoPick;
            unit.m_RotAngle = 0;
            UserInfo userInfo = WorldSystem.Instance.GetPlayerSelf();
            CharacterInfo fromNpc = WorldSystem.Instance.GetCharacterById(fromNpcId);
            if (null != userInfo && fromNpcId > 0)
            {
                NpcInfo npcInfo = NpcManager.AddNpc(unit);
                npcInfo.GetMovementStateInfo().SetPosition(fromNpc.GetMovementStateInfo().GetPosition3D());
                npcInfo.GetMovementStateInfo().SetFaceDir(0);
                npcInfo.GetMovementStateInfo().IsMoving = false;
                npcInfo.SetAIEnable(true);
                npcInfo.SetCampId(userInfo.GetCampId());
                npcInfo.OwnerId = userInfo.GetId();
                DropOutInfo dropInfo = new DropOutInfo();
                dropInfo.DropType = dropType;
                dropInfo.Model = model;
                dropInfo.Particle = particle;
                dropInfo.Value = value;
                npcInfo.GetAiStateInfo().AiDatas.AddData<DropOutInfo>(dropInfo);
                npcInfo.SetModel(model);
                EntityManager.Instance.CreateNpcView(npcInfo.GetId());
            }
        }
        private bool IsNpcBornOver(NpcInfo npc)
        {
            if (npc == null)
            {
                return false;
            }
            long cur_time = TimeUtility.GetServerMilliseconds();
            long born_anim_time = npc.BornAnimTimeMs;
            if ((npc.BornTime + born_anim_time) > cur_time)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void ShowObstacle(bool isShow)
        {
            if (!m_IsDebugObstacleCreated)
            {
                if (null != m_SpatialSystem && null != m_PlayerSelf)
                {
                    CellManager cellMgr = m_SpatialSystem.GetCellManager();
                    if (null != cellMgr)
                    {
                        List<float> vertices = new List<float>();
                        List<int> triangles = new List<int>();

                        int maxRows = cellMgr.GetMaxRow();
                        int maxCols = cellMgr.GetMaxCol();
                        float w = cellMgr.GetCellWidth() / 2;

                        uint color = 0xccff0000;

                        for (int i = 0; i < maxRows; ++i)
                        {
                            for (int j = 0; j < maxCols; ++j)
                            {
                                Vector3 pt = cellMgr.GetCellCenter(i, j);
                                byte status = cellMgr.GetCellStatus(i, j);
                                if (BlockType.GetBlockType(status) != BlockType.NOT_BLOCK)
                                {
                                    int stIndex = vertices.Count / 3;
                                    //---------------------
                                    //左上
                                    vertices.Add(pt.X - w);
                                    vertices.Add(0);
                                    vertices.Add(pt.Z - w);
                                    //右上
                                    vertices.Add(pt.X + w);
                                    vertices.Add(0);
                                    vertices.Add(pt.Z - w);
                                    //左下
                                    vertices.Add(pt.X - w);
                                    vertices.Add(0);
                                    vertices.Add(pt.Z + w);
                                    //右下
                                    vertices.Add(pt.X + w);
                                    vertices.Add(0);
                                    vertices.Add(pt.Z + w);
                                    //---------------------
                                    //三角形1
                                    triangles.Add(stIndex);
                                    triangles.Add(stIndex + 2);
                                    triangles.Add(stIndex + 1);
                                    //三角形2
                                    triangles.Add(stIndex + 1);
                                    triangles.Add(stIndex + 2);
                                    triangles.Add(stIndex + 3);

                                    if (stIndex >= 10000)
                                    {
                                        int actor = GameObjectIdManager.Instance.GenNextId();
                                        GfxSystem.CreateGameObjectWithMeshData(actor, vertices, triangles, "Obstacle", true);
                                        //GfxSystem.CreateGameObjectWithMeshData(actor, vertices, triangles, color, "Transparent/Diffuse", true);
                                        m_DebugObstacleActors.Add(actor);
                                        vertices = new List<float>();
                                        triangles = new List<int>();
                                    }
                                }
                            }
                        }

                        if (vertices.Count > 0)
                        {
                            int actor = GameObjectIdManager.Instance.GenNextId();
                            GfxSystem.CreateGameObjectWithMeshData(actor, vertices, triangles, "Obstacle", true);
                            //GfxSystem.CreateGameObjectWithMeshData(actor, vertices, triangles, color, "Transparent/Diffuse", true);
                            m_DebugObstacleActors.Add(actor);
                        }
                        m_IsDebugObstacleCreated = true;
                    }
                }
                return;
            }
            foreach (int actor in m_DebugObstacleActors)
            {
                GfxSystem.SetGameObjectVisible(actor, isShow);
            }
        }
        private void DestroyObstacleObjects()
        {
            foreach (int actor in m_DebugObstacleActors)
            {
                GfxSystem.DestroyGameObject(actor);
            }
            m_IsDebugObstacleCreated = false;
        }

        internal CharacterInfo GetCharacterById(int id)
        {
            CharacterInfo obj = null;
            if (null != m_NpcMgr)
                obj = m_NpcMgr.GetNpcInfo(id);
            if (null != m_UserMgr && null == obj)
                obj = m_UserMgr.GetUserInfo(id);
            return obj;
        }
        internal CharacterInfo GetCharacterByUnitId(int unitId)
        {
            CharacterInfo obj = null;
            if (null != m_NpcMgr)
                obj = m_NpcMgr.GetNpcInfoByUnitId(unitId);
            return obj;
        }
        internal UserInfo GetPlayerSelf()
        {
            return m_PlayerSelf;
        }
        internal void DestroyCharacterById(int id)
        {
            if (m_NpcMgr.Npcs.Contains(id))
            {
                m_NpcMgr.RemoveNpc(id);
            }
            if (m_PlayerSelfId == id)
            {
                m_PlayerSelf = null;
            }
            UserInfo info;
            if (m_UserMgr.Users.TryGetValue(id, out info))
            {
                if (null != m_SpatialSystem)
                {
                    if (null != info)
                    {
                        info.SceneContext = null;
                        m_SpatialSystem.RemoveObj(info.SpaceObject);
                    }
                }
                m_UserMgr.RemoveUser(id);
            }
        }
        internal void SetAIEnable(int characterId, bool enable)
        {
            CharacterInfo info = WorldSystem.Instance.GetCharacterById(characterId);
            if (null != info)
            {
                info.SetAIEnable(enable);
                info.GetMovementStateInfo().IsMoving = false;
            }
        }
        internal int CreateNpcEntity(int unitId)
        {
            int objId = 0;
            Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, unitId) as Data_Unit;
            if (null != mapUnit)
            {
                NpcInfo npc = m_NpcMgr.AddNpc(mapUnit);
                if (null != npc)
                {
                    npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                    npc.SkillController.Init();
                    EntityManager.Instance.CreateNpcView(npc.GetId());
                    CustomNpcByUnitId(npc, unitId);
                    objId = npc.GetId();
                }
            }
            return objId;
        }
        internal int CreateNpcEntity(int unitId, float rnd)
        {
            int objId = 0;
            Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, unitId) as Data_Unit;
            if (null != mapUnit)
            {
                Vector3 pos;
                if (Geometry.IsSamePoint(mapUnit.m_Pos2, Vector3.Zero))
                {
                    pos = mapUnit.m_Pos;
                }
                else
                {
                    pos = mapUnit.m_Pos * (1.0f - rnd) + mapUnit.m_Pos2 * rnd;
                }
                NpcInfo npc = m_NpcMgr.AddNpc(mapUnit);
                if (null != npc)
                {
                    npc.GetMovementStateInfo().SetPosition(pos);
                    npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                    npc.SkillController.Init();
                    EntityManager.Instance.CreateNpcView(npc.GetId());
                    CustomNpcByUnitId(npc, unitId);
                    objId = npc.GetId();
                }
            }
            return objId;
        }
        internal int CreateNpcEntityWithPos(int unitId, float x, float y, float z, float rotateAngle)
        {
            int objId = 0;
            Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, unitId) as Data_Unit;
            if (null != mapUnit)
            {
                NpcInfo npc = m_NpcMgr.AddNpc(mapUnit);
                if (null != npc)
                {
                    npc.GetMovementStateInfo().SetPosition(x, y, z);
                    npc.GetMovementStateInfo().SetFaceDir(rotateAngle);
                    npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                    npc.SkillController.Init();
                    EntityManager.Instance.CreateNpcView(npc.GetId());
                    objId = npc.GetId();
                }
            }
            return objId;
        }

        internal NpcInfo CreateNpc(int id, int unitId)
        {
            NpcInfo ret = null;
            Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, unitId) as Data_Unit;
            if (null != mapUnit)
            {
                ret = m_NpcMgr.AddNpc(id, mapUnit);
                if (null != ret)
                {
                    ret.SkillController = new SwordManSkillController(ret, GfxModule.Skill.GfxSkillSystem.Instance);
                    ret.SkillController.Init();
                }
            }
            return ret;
        }
        internal NpcInfo CreateNpcByLinkId(int id, int linkId)
        {
            Data_Unit mapUnit = new Data_Unit();
            mapUnit.m_Id = -1;
            mapUnit.m_LinkId = linkId;
            NpcInfo npc = m_NpcMgr.AddNpc(id, mapUnit);
            if (null != npc)
            {
                npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                npc.SkillController.Init();
            }
            return npc;
        }
        internal int CreateNpcAndAddAttr(int unitId, int attrId, int level)
        {
            int objId = 0;
            Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, unitId) as Data_Unit;
            if (null != mapUnit)
            {
                NpcInfo npc = m_NpcMgr.AddNpc(mapUnit);
                if (null != npc)
                {
                    objId = npc.GetId();
                    npc.SetLevel(level);
                    if (attrId > 0)
                    {
                        ExpeditionMonsterAttrConfig attr = ExpeditionMonsterAttrConfigProvider.Instance.GetExpeditionMonsterAttrConfigById(attrId);
                        if (null != attr && null != attr.m_AttrData)
                        {
                            int addHp = (int)attr.m_AttrData.GetAddHpMax(0, npc.GetLevel());
                            int addEnergy = (int)attr.m_AttrData.GetAddEpMax(0, npc.GetLevel());
                            int addAd = (int)attr.m_AttrData.GetAddAd(0, npc.GetLevel());
                            int addAdp = (int)attr.m_AttrData.GetAddADp(0, npc.GetLevel());
                            int addMdp = (int)attr.m_AttrData.GetAddMDp(0, npc.GetLevel());
                            int addHpRecover = (int)attr.m_AttrData.GetAddHpRecover(0, npc.GetLevel());
                            int addEpRecover = (int)attr.m_AttrData.GetAddEpRecover(0, npc.GetLevel());
                            npc.GetBaseProperty().SetHpMax(Operate_Type.OT_Relative, addHp);
                            npc.GetBaseProperty().SetEnergyMax(Operate_Type.OT_Relative, addEnergy);
                            npc.GetBaseProperty().SetAttackBase(Operate_Type.OT_Relative, addAd);
                            npc.GetBaseProperty().SetADefenceBase(Operate_Type.OT_Relative, addAdp);
                            npc.GetBaseProperty().SetMDefenceBase(Operate_Type.OT_Relative, addMdp);
                            npc.GetBaseProperty().SetHpRecover(Operate_Type.OT_Relative, addHpRecover);
                            npc.GetBaseProperty().SetEnergyRecover(Operate_Type.OT_Relative, addEpRecover);
                            ///
                            int curHp = (int)attr.m_AttrData.GetAddHpMax(0, npc.GetLevel());
                            int curEnergy = (int)attr.m_AttrData.GetAddEpMax(0, npc.GetLevel());
                            npc.SetHp(Operate_Type.OT_Absolute, curHp);
                            npc.SetEnergy(Operate_Type.OT_Absolute, curEnergy);
                        }
                    }
                    npc.SkillController = new SwordManSkillController(npc, GfxModule.Skill.GfxSkillSystem.Instance);
                    npc.SkillController.Init();
                    EntityManager.Instance.CreateNpcView(npc.GetId());
                    CustomNpcByUnitId(npc, unitId);
                }
            }
            return objId;
        }
        internal UserInfo CreateUser(int id, int resId)
        {
            UserInfo info = m_UserMgr.AddUser(id, resId);
            if (null != info)
            {
                info.SceneContext = m_SceneContext;
                info.SkillController = new SwordManSkillController(info, GfxModule.Skill.GfxSkillSystem.Instance);
                info.SkillController.Init();
                if (null != m_SpatialSystem)
                {
                    m_SpatialSystem.AddObj(info.SpaceObject);
                }
            }
            return info;
        }
        internal void RelivePlayer()
        {
            UserInfo player = GetPlayerSelf();
            if (null != player)
            {
                player.SetHp(Operate_Type.OT_Absolute, player.GetActualProperty().HpMax);
                player.SetEnergy(Operate_Type.OT_Absolute, player.GetActualProperty().EnergyMax);
                player.DeadTime = 0;
                player.GfxDead = false;
                List<ImpactInfo> impactInfos = player.GetSkillStateInfo().GetAllImpact();
                for (int i = 0; i < impactInfos.Count; ++i)
                {
                    IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(impactInfos[i].ConfigData.ImpactLogicId);
                    if (null != logic && impactInfos[i].ConfigData.ImpactType != (int)ImpactType.PASSIVE)
                    {
                        logic.StopImpact(player, impactInfos[i].m_ImpactId);
                    }
                }
                player.GetSkillStateInfo().RemoveAllImpact();
                player.GfxStateFlag = 0;
                CharacterView view = EntityManager.Instance.GetCharacterViewById(player.GetId());
                if (null != view)
                {
                    view.ObjectInfo.IsDead = false;
                    view.ObjectInfo.GfxStateFlag = 0;
                }
                PlayerControl.Instance.EnableMoveInput = true;
                PlayerControl.Instance.EnableRotateInput = true;
                PlayerControl.Instance.EnableSkillInput = true;
                GfxSystem.PublishGfxEvent("ge_on_role_relive", "ui");
            }
        }
        internal void AddFightingScoreCB(UserInfo user)
        {
            if (null != user && null != LobbyClient.Instance.CurrentRole)
            {
                user.AddPropertyInfoChangeCB(LobbyClient.Instance.CurrentRole.FightingScoreChangeCB);
            }
        }
        internal UserInfo CreatePlayerSelf(int id, int resId)
        {
            m_PlayerSelf = CreateUser(id, resId);
            m_PlayerSelfId = id;
            AddFightingScoreCB(m_PlayerSelf);
            return m_PlayerSelf;
        }

        internal void NotifyMoveMeetObstacle(bool force)
        {
            if (!IsAlreadyNotifyMeetObstacle || force)
            {
                if (null != m_PlayerSelf)
                {
                    MovementStateInfo msi = m_PlayerSelf.GetMovementStateInfo();

                    Msg_CRC_MoveMeetObstacle msg = new Msg_CRC_MoveMeetObstacle();
                    msg.cur_pos_x = msi.PositionX;
                    msg.cur_pos_z = msi.PositionZ;
                    msg.send_time = TimeUtility.GetServerMilliseconds();
                    NetworkSystem.Instance.SendMessage(msg);

                    IsAlreadyNotifyMeetObstacle = true;
                }
            }
        }
        internal void UpdateObserverCamera(float x, float y)
        {
            if (!m_IsFollowObserver)
            {
                GetCurScene().UpdateObserverCamera(x, y);
            }
        }
        internal void HighlightPrompt(int id, params object[] args)
        {
            string info = Dict.Format(id, args);
            GfxSystem.PublishGfxEvent("ge_highlight_prompt", "ui", info);
        }
        internal void RefixSkills(UserInfo user)
        {
            if (null != user)
            {
                if (null != LobbyClient.Instance.CurrentRole && null != LobbyClient.Instance.CurrentRole.SkillInfos)
                {
                    List<SkillInfo> skill_info_list = LobbyClient.Instance.CurrentRole.SkillInfos;
                    SkillInfo[] skill_assit = new SkillInfo[] { new SkillInfo(0), new SkillInfo(0), new SkillInfo(0), new SkillInfo(0) };
                    int cur_preset_index = 0;
                    if (cur_preset_index >= 0)
                    {
                        for (int i = 0; i < skill_assit.Length; i++)
                        {
                            for (int j = 0; j < skill_info_list.Count; j++)
                            {
                                if (skill_info_list[j].Postions.Presets[cur_preset_index] == (SlotPosition)(i + 1))
                                {
                                    skill_assit[i].SkillId = skill_info_list[j].SkillId;
                                    skill_assit[i].SkillLevel = skill_info_list[j].SkillLevel;
                                    break;
                                }
                            }
                        }
                    }
                    ///
                    user.GetSkillStateInfo().RemoveAllSkill();
                    for (int i = 0; i < skill_assit.Length; i++)
                    {
                        if (skill_assit[i].SkillId > 0)
                        {
                            SkillInfo info = new SkillInfo(skill_assit[i].SkillId);
                            info.SkillLevel = skill_assit[i].SkillLevel;
                            info.Postions.SetCurSkillSlotPos(0, (SlotPosition)(i + 1));
                            SkillCategory cur_skill_pos = SkillCategory.kNone;
                            if ((i + 1) == (int)SlotPosition.SP_A)
                            {
                                cur_skill_pos = SkillCategory.kSkillA;
                            }
                            else if ((i + 1) == (int)SlotPosition.SP_B)
                            {
                                cur_skill_pos = SkillCategory.kSkillB;
                            }
                            else if ((i + 1) == (int)SlotPosition.SP_C)
                            {
                                cur_skill_pos = SkillCategory.kSkillC;
                            }
                            else if ((i + 1) == (int)SlotPosition.SP_D)
                            {
                                cur_skill_pos = SkillCategory.kSkillD;
                            }
                            info.Category = cur_skill_pos;
                            user.GetSkillStateInfo().AddSkill(info);
                            ///
                            LogSystem.Debug("RefixSkills number:{0},skillid:{1},pos:{2}", i + 1, info.SkillId, info.ConfigData.Category);
                            ///
                            AddSubSkill(user, info.SkillId, cur_skill_pos, info.SkillLevel);
                        }
                    }
                    Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(user.GetLinkId());
                    if (null != playerData && null != playerData.m_FixedSkillList
                      && playerData.m_FixedSkillList.Count > 0)
                    {
                        foreach (int skill_id in playerData.m_FixedSkillList)
                        {
                            if (null == user.GetSkillStateInfo().GetSkillInfoById(skill_id))
                            {
                                SkillInfo info = new SkillInfo(skill_id, 1);
                                user.GetSkillStateInfo().AddSkill(info);
                            }
                        }
                    }
                    user.ResetSkill();
                }
            }
        }
        internal void PromptExceptionAndGotoMainCity()
        {
            if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
            {
                OnRoomServerConnected();
            }
            if (WorldSystem.Instance.IsPveScene())
            {
                GfxSystem.PublishGfxEvent("ge_show_dialog", "ui", Dict.Get(11), Dict.Get(4), null, null, (MyAction<int>)((int btn) =>
                {
                    LobbyNetworkSystem.Instance.QuitRoom(false);
                    NetworkSystem.Instance.QuitBattle(false);
                    m_LastPromptExceptionTime = TimeUtility.GetLocalMilliseconds();
                    QueueAction((MyFunc<int, bool>)this.ChangeScene, LobbyClient.Instance.CurrentRole.CitySceneId);
                }), true);
            }
            else if (WorldSystem.Instance.IsMultiPveScene())
            {
                GfxSystem.PublishGfxEvent("ge_show_dialog", "ui", Dict.Get(10), Dict.Get(4), null, null, (MyAction<int>)((int btn) =>
                {
                    LobbyNetworkSystem.Instance.QuitRoom(false);
                    NetworkSystem.Instance.QuitBattle(true);
                    m_LastPromptExceptionTime = TimeUtility.GetLocalMilliseconds();
                    QueueAction((MyFunc<int, bool>)this.ChangeScene, LobbyClient.Instance.CurrentRole.CitySceneId);
                }), true);
            }
            else if (WorldSystem.Instance.IsPvpScene())
            {
                if (!NetworkSystem.Instance.IsWaitStart)
                {
                    GfxSystem.PublishGfxEvent("ge_show_dialog", "ui", Dict.Get(10), Dict.Get(4), null, null, (MyAction<int>)((int btn) =>
                    {
                        LobbyNetworkSystem.Instance.QuitRoom(false);
                        NetworkSystem.Instance.QuitBattle(false);
                        m_LastPromptExceptionTime = TimeUtility.GetLocalMilliseconds();
                        QueueAction((MyFunc<int, bool>)this.ChangeScene, LobbyClient.Instance.CurrentRole.CitySceneId);
                    }), true);
                }
            }
            else
            {
                // 不在副本里就不弹提示了。
            }
        }
        internal bool AddSubSkill(UserInfo user, int skill_id, SkillCategory pos, int level)
        {
            if (null == user)
                return false;
            SkillLogicData skill_data = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skill_id) as SkillLogicData;
            if (null != skill_data && skill_data.NextSkillId > 0)
            {
                SkillInfo info = new SkillInfo(skill_data.NextSkillId);
                info.SkillLevel = level;
                info.Category = pos;
                user.GetSkillStateInfo().AddSkill(info);
                AddSubSkill(user, info.SkillId, pos, level);
            }
            return true;
        }

        internal void QueueAction(MyAction action)
        {
            m_DelayActionProcessor.QueueAction(action);
        }
        internal void QueueAction<T1>(MyAction<T1> action, T1 t1)
        {
            QueueActionWithDelegation(action, t1);
        }
        internal void QueueAction<T1, T2>(MyAction<T1, T2> action, T1 t1, T2 t2)
        {
            QueueActionWithDelegation(action, t1, t2);
        }
        internal void QueueAction<T1, T2, T3>(MyAction<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            QueueActionWithDelegation(action, t1, t2, t3);
        }
        internal void QueueAction<T1, T2, T3, T4>(MyAction<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4);
        }
        internal void QueueAction<T1, T2, T3, T4, T5>(MyAction<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6>(MyAction<T1, T2, T3, T4, T5, T6> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7>(MyAction<T1, T2, T3, T4, T5, T6, T7> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16);
        }
        internal void QueueAction<R>(MyFunc<R> action)
        {
            QueueActionWithDelegation(action);
        }
        internal void QueueAction<T1, R>(MyFunc<T1, R> action, T1 t1)
        {
            QueueActionWithDelegation(action, t1);
        }
        internal void QueueAction<T1, T2, R>(MyFunc<T1, T2, R> action, T1 t1, T2 t2)
        {
            QueueActionWithDelegation(action, t1, t2);
        }
        internal void QueueAction<T1, T2, T3, R>(MyFunc<T1, T2, T3, R> action, T1 t1, T2 t2, T3 t3)
        {
            QueueActionWithDelegation(action, t1, t2, t3);
        }
        internal void QueueAction<T1, T2, T3, T4, R>(MyFunc<T1, T2, T3, T4, R> action, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, R>(MyFunc<T1, T2, T3, T4, T5, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, R>(MyFunc<T1, T2, T3, T4, T5, T6, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15);
        }
        internal void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>(MyFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16)
        {
            QueueActionWithDelegation(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16);
        }
        internal void QueueActionWithDelegation(Delegate action, params object[] args)
        {
            if (null != m_DelayActionProcessor)
            {
                m_DelayActionProcessor.QueueActionWithDelegation(action, args);
            }
        }
        internal int PlayerSelfId
        {
            get { return m_PlayerSelfId; }
            set { m_PlayerSelfId = value; }
        }
        internal int HeroId
        {
            get { return NetworkSystem.Instance.HeroId; }
        }
        internal int CampId
        {
            get { return NetworkSystem.Instance.CampId; }
        }
        internal SceneContextInfo SceneContext
        {
            get { return m_SceneContext; }
        }
        internal ISpatialSystem SpatialSystem
        {
            get
            {
                return m_SpatialSystem;
            }
        }
        internal SceneLogicInfoManager SceneLogicInfoManager
        {
            get
            {
                return m_SceneLogicInfoMgr;
            }
        }
        internal NpcManager NpcManager
        {
            get
            {
                return m_NpcMgr;
            }
        }
        internal UserManager UserManager
        {
            get
            {
                return m_UserMgr;
            }
        }

        internal long SceneStartTime
        {
            get { return m_SceneStartTime; }
            set
            {
                m_SceneStartTime = value;
                m_SceneContext.StartTime = m_SceneStartTime;
            }
        }
        internal bool IsObserver
        {
            get { return m_IsObserver; }
        }
        internal bool IsFollowObserver
        {
            get { return m_IsFollowObserver; }
            set { m_IsFollowObserver = value; }
        }
        internal int FollowTargetId
        {
            get { return m_FollowTargetId; }
            set { m_FollowTargetId = value; }
        }

        internal bool IsAlreadyNotifyMeetObstacle
        {
            get { return m_IsAlreadyNotifyMeetObstacle; }
            set { m_IsAlreadyNotifyMeetObstacle = value; }
        }

        public int WaitMatchSceneId
        {
            get { return m_WaitMatchSceneId; }
            set { m_WaitMatchSceneId = value; }
        }

        public bool IsStageClearing
        {
            get { return m_IsStageClearing; }
            set { m_IsStageClearing = value; }
        }

        /**
         * @brief 构造函数
         *
         * @return 
         */
        private WorldSystem()
        {
            m_SceneContext.OnHighlightPrompt = (int userId, int dict, object[] args) =>
            {
                WorldSystem.Instance.HighlightPrompt(dict, args);
            };

            m_SceneContext.SpatialSystem = m_SpatialSystem;
            m_SceneContext.SceneLogicInfoManager = m_SceneLogicInfoMgr;
            m_SceneContext.NpcManager = m_NpcMgr;
            m_SceneContext.UserManager = m_UserMgr;
            m_SceneContext.BlackBoard = m_BlackBoard;

            m_NpcMgr.SetSceneContext(m_SceneContext);
            m_SceneLogicInfoMgr.SetSceneContext(m_SceneContext);

            m_AiSystem.SetNpcManager(m_NpcMgr);
            m_AiSystem.SetUserManager(m_UserMgr);
            m_SceneLogicSystem.SetSceneLogicInfoManager(m_SceneLogicInfoMgr);
        }

        private class ScrollMessageInfo
        {
            internal int m_Day;
            internal int m_Week;
            internal DateTime m_Start;
            internal DateTime m_End;
            internal long m_Interval;
            internal string m_Message;
            internal int m_ScrollCount;
            internal long m_LastSendTime;
        }

        private const long c_ScrollMessageTickInterval = 60000;
        private long m_LastScrollMessageTickTime = 0;
        private List<ScrollMessageInfo> m_ScrollMessageInfos = new List<ScrollMessageInfo>();

        private NpcManager m_NpcMgr = new NpcManager(256);
        private UserManager m_UserMgr = new UserManager(16);
        private SceneLogicInfoManager m_SceneLogicInfoMgr = new SceneLogicInfoManager(256);

        private AiSystem m_AiSystem = new AiSystem();
        private SceneLogicSystem m_SceneLogicSystem = new SceneLogicSystem();
        private SpatialSystem m_SpatialSystem = new SpatialSystem();
        private BlackBoard m_BlackBoard = new BlackBoard();

        private UserInfo m_PlayerSelf = null;
        private int m_PlayerSelfId = -1;

        private SceneContextInfo m_SceneContext = new SceneContextInfo();

        private const long c_InteractionCheckInterval = 1000;
        private long m_LastInteractionCheckTime = 0;
        private long m_SceneStartTime = 0;
        private long m_LastLogicTickTime = 0;

        private const long c_ExceptionGotoMainCityTimeout = 15000;
        private const long c_ChangeSceneTimeout = 60000;
        private const long c_PromptExceptionTimeout = 10000;
        private long m_LastTryChangeSceneTime = 0;
        private long m_LastPromptExceptionTime = 0;

        private bool m_IsObserver = false;
        private bool m_IsFollowObserver = false;
        private int m_FollowTargetId = 0;

        private int m_WaitMatchSceneId = -1;

        private WorldSystemProfiler m_Profiler = new WorldSystemProfiler();
        private SceneResource m_CurScene;

        private Dictionary<ulong, int> m_CityUsers = new Dictionary<ulong, int>();
        private const long c_CityUserTickInterval = 30;
        private long m_LastCityUserTickTime = 0;
        private int m_CurCityUserTickCount = 0;

        private const int c_CityUserUpdatePositionTickNum = 1;
        private const int c_MaxCityUsers = 500;
        private int m_NextCityUserId = 2;

        private const int c_MaxCityUserWithModel = 30;
        private const int c_LowModelIdDeviation = 100;
        private const int c_LowModelId2Deviation = 200;
        private int m_CurCityUserWithModelCount = 0;

        private bool m_IsAlreadyNotifyMeetObstacle = false;
        private long m_LastNotifyMoveMeetObstacleTime = 0;

        private bool m_IsDebugObstacleCreated = false;
        private List<int> m_DebugObstacleActors = new List<int>();

        private ClientDelayActionProcessor m_DelayActionProcessor = new ClientDelayActionProcessor();

        private const long c_IntervalPerSecond = 5000;
        private long m_LastTickTimeForTickPerSecond = 0;

        private bool m_IsCameraInCombatState = true;
        private bool m_ForceUpdateCamerState = false;
        private bool m_IsAreaClear = true;

        //冒血缓存处理数据
        private int m_CurIdx = 0;
        private int m_MaxIdx = 1000;
        private int m_CurPush = 0;
        //每次出几个
        private int m_MaxPush = 8;
        private int m_CurTime = 0;
        //多少帧出一次
        private int m_Speed = 1;
        private LinkedListDictionary<int, DamageData> m_DamageDatas = new LinkedListDictionary<int, DamageData>();
        // 是否在进行剧情
        private bool m_IsStroyProceed = false;
        private bool m_IsStageClearing = false;
    }
    internal class DamageData
    {
        internal int m_Id;
        internal int m_Hp;
        internal int m_MaxHp;
        internal int m_HpDamage;
        internal int m_NpDamage;
        internal bool m_IsCritical;
        internal bool m_IsOrdinaryDamage;
        internal bool m_IsMonster;
        internal Vector3 m_Pos;
        internal string m_Name;
    }
}
