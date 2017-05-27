using System;
using StorySystem;

namespace ArkCrossEngine.GmCommands
{
    //---------------------------------------------------------------------------------------------------------------------------------
    //连接lobby时才有效的命令
    internal class SwitchDebugCommand : SimpleStoryCommandBase<SwitchDebugCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            WorldSystem.Instance.SwitchDebug();
            return false;
        }
    }
    internal class LobbyAddAssetsCommand : SimpleStoryCommandBase<LobbyAddAssetsCommand, StoryValueParam<int, int, int, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, int, int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int money = _params.Param1Value;
                int gold = _params.Param2Value;
                int exp = _params.Param3Value;
                int stamina = _params.Param4Value;
                Network.LobbyNetworkSystem.Instance.AddAssets(money, gold, exp, stamina);
            }
            return false;
        }
    }
    internal class LobbyAddItemCommand : SimpleStoryCommandBase<LobbyAddItemCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int itemId = _params.Param1Value;
                Network.LobbyNetworkSystem.Instance.AddItem(itemId);
            }
            return false;
        }
    }
    internal class LobbyResetDailyMissionsCommand : SimpleStoryCommandBase<LobbyResetDailyMissionsCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                Network.LobbyNetworkSystem.Instance.ResetDailyMissions();
            }
            return false;
        }
    }
    internal class UpdateMaxUserCountCommand : SimpleStoryCommandBase<UpdateMaxUserCountCommand, StoryValueParam<int, int, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int maxUserCount = _params.Param1Value;
                int maxUserCountPerLogicServer = _params.Param2Value;
                int maxQueueingCount = _params.Param3Value;
                Network.LobbyNetworkSystem.Instance.UpdateMaxUserCount(maxUserCount, maxUserCountPerLogicServer, maxQueueingCount);
            }
            return false;
        }
    }
    internal class SetNewbieFlagCommand : SimpleStoryCommandBase<SetNewbieFlagCommand, StoryValueParam<int, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int bit = _params.Param1Value;
                int num = _params.Param2Value;
                GfxSystem.EventChannelForLogic.Publish("ge_set_newbie_flag", "lobby", bit, num);
            }
            return false;
        }
    }
    internal class SetNewbieActionFlagCommand : SimpleStoryCommandBase<SetNewbieActionFlagCommand, StoryValueParam<int, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int bit = _params.Param1Value;
                int num = _params.Param2Value;
                GfxSystem.EventChannelForLogic.Publish("ge_set_newbie_action_flag", "lobby", bit, num);
            }
            return false;
        }
    }
    internal class MountEquipmentCommand : SimpleStoryCommandBase<LobbyAddItemCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int itemId = _params.Param1Value;
                ItemConfig cfg = ItemConfigProvider.Instance.GetDataById(itemId);
                if (null != cfg)
                {
                    GfxSystem.EventChannelForLogic.Publish("ge_unmount_equipment", "lobby", cfg.m_WearParts);
                    GfxSystem.EventChannelForLogic.Publish("ge_mount_equipment", "lobby", itemId, 0, cfg.m_WearParts);
                }
            }
            return false;
        }
    }
    internal class MountSkillCommand : SimpleStoryCommandBase<MountSkillCommand, StoryValueParam<int, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int skillId = _params.Param1Value;
                int slot = _params.Param2Value;
                GfxSystem.EventChannelForLogic.Publish("ge_unmount_skill", "lobby", 0, (SlotPosition)slot);
                GfxSystem.EventChannelForLogic.Publish("ge_mount_skill", "lobby", 0, skillId, (SlotPosition)slot);
            }
            return false;
        }
    }
    internal class UpgradeSkillCommand : SimpleStoryCommandBase<UpgradeSkillCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int skillId = _params.Param1Value;
                GfxSystem.EventChannelForLogic.Publish("ge_upgrade_skill", "lobby", 0, skillId, true);
            }
            return false;
        }
    }
    internal class UnlockSkillCommand : SimpleStoryCommandBase<UnlockSkillCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int skillId = _params.Param1Value;
                GfxSystem.EventChannelForLogic.Publish("ge_unlock_skill", "lobby", 0, skillId);
            }
            return false;
        }
    }
    internal class SwapSkillCommand : SimpleStoryCommandBase<SwapSkillCommand, StoryValueParam<int, int, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int, int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int skillId = _params.Param1Value;
                int srcPos = _params.Param2Value;
                int destPos = _params.Param3Value;
                GfxSystem.EventChannelForLogic.Publish("ge_swap_skill", "lobby", 0, skillId, (SlotPosition)srcPos, (SlotPosition)destPos);
            }
            return false;
        }
    }
    internal class LiftSkillCommand : SimpleStoryCommandBase<LiftSkillCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int skillId = _params.Param1Value;
                GfxSystem.EventChannelForLogic.Publish("ge_lift_skill", "lobby", skillId);
            }
            return false;
        }
    }
    internal class UpgradeItemCommand : SimpleStoryCommandBase<UpgradeItemCommand, StoryValueParam<int, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int equipPos = _params.Param1Value;
                int itemId = _params.Param2Value;
                GfxSystem.EventChannelForLogic.Publish("ge_upgrade_item", "lobby", equipPos, itemId, true);
            }
            return false;
        }
    }
    internal class GetMailListCommand : SimpleStoryCommandBase<GetMailListCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                GfxSystem.EventChannelForLogic.Publish("ge_get_mail_list", "lobby");
            }
            return false;
        }
    }
    internal class ReceiveMailCommand : SimpleStoryCommandBase<ReceiveMailCommand, StoryValueParam<ulong>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<ulong> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                ulong mailGuid = _params.Param1Value;
                GfxSystem.EventChannelForLogic.Publish("ge_receive_mail", "lobby", mailGuid);
            }
            return false;
        }
    }
    internal class SendMailCommand : AbstractStoryCommand
    {
        public override IStoryCommand Clone()
        {
            SendMailCommand cmd = new SendMailCommand();
            cmd.m_Receiver = m_Receiver.Clone();
            cmd.m_Title = m_Title.Clone();
            cmd.m_Text = m_Text.Clone();
            cmd.m_ExpiryDate = m_ExpiryDate.Clone();
            cmd.m_LevelDemand = m_LevelDemand.Clone();
            cmd.m_ItemId = m_ItemId.Clone();
            cmd.m_ItemNum = m_ItemNum.Clone();
            cmd.m_Money = m_Money.Clone();
            cmd.m_Gold = m_Gold.Clone();
            cmd.m_Stamina = m_Stamina.Clone();
            return cmd;
        }

        protected override void ResetState()
        { }

        protected override void UpdateArguments(object iterator, object[] args)
        {
            m_Receiver.Evaluate(iterator, args);
            m_Title.Evaluate(iterator, args);
            m_Text.Evaluate(iterator, args);
            m_ExpiryDate.Evaluate(iterator, args);
            m_LevelDemand.Evaluate(iterator, args);
            m_ItemId.Evaluate(iterator, args);
            m_ItemNum.Evaluate(iterator, args);
            m_Money.Evaluate(iterator, args);
            m_Gold.Evaluate(iterator, args);
            m_Stamina.Evaluate(iterator, args);
        }

        protected override void UpdateVariables(StoryInstance instance)
        {
            m_Receiver.Evaluate(instance);
            m_Title.Evaluate(instance);
            m_Text.Evaluate(instance);
            m_ExpiryDate.Evaluate(instance);
            m_LevelDemand.Evaluate(instance);
            m_ItemId.Evaluate(instance);
            m_ItemNum.Evaluate(instance);
            m_Money.Evaluate(instance);
            m_Gold.Evaluate(instance);
            m_Stamina.Evaluate(instance);
        }

        protected override bool ExecCommand(StoryInstance instance, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                Network.LobbyNetworkSystem.Instance.SendMail(m_Receiver.Value, m_Title.Value, m_Text.Value, m_ExpiryDate.Value, m_LevelDemand.Value, m_ItemId.Value, m_ItemNum.Value, m_Money.Value, m_Gold.Value, m_Stamina.Value);
            }
            return false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            int paramNum = callData.GetParamNum();
            if (paramNum >= 10)
            {
                m_Receiver.InitFromDsl(callData.GetParam(0));
                m_Title.InitFromDsl(callData.GetParam(1));
                m_Text.InitFromDsl(callData.GetParam(2));
                m_ExpiryDate.InitFromDsl(callData.GetParam(3));
                m_LevelDemand.InitFromDsl(callData.GetParam(4));
                m_ItemId.InitFromDsl(callData.GetParam(5));
                m_ItemNum.InitFromDsl(callData.GetParam(6));
                m_Money.InitFromDsl(callData.GetParam(7));
                m_Gold.InitFromDsl(callData.GetParam(8));
                m_Stamina.InitFromDsl(callData.GetParam(9));
            }
        }

        private IStoryValue<string> m_Receiver = new StoryValue<string>();
        private IStoryValue<string> m_Title = new StoryValue<string>();
        private IStoryValue<string> m_Text = new StoryValue<string>();
        private IStoryValue<int> m_ExpiryDate = new StoryValue<int>();
        private IStoryValue<int> m_LevelDemand = new StoryValue<int>();
        private IStoryValue<int> m_ItemId = new StoryValue<int>();
        private IStoryValue<int> m_ItemNum = new StoryValue<int>();
        private IStoryValue<int> m_Money = new StoryValue<int>();
        private IStoryValue<int> m_Gold = new StoryValue<int>();
        private IStoryValue<int> m_Stamina = new StoryValue<int>();
    }
    /*
    internal class SendMailCommand : SimpleStoryCommandBase<SendMailCommand, StoryValueParam<string, string, string, int, int, int, int, int, int, int>>
    {
      protected override bool ExecCommand(StoryInstance instance, StoryValueParam<string, string, string, int, int, int, int, int, int, int> _params, long delta)
      {
        if (null != LobbyClient.Instance.CurrentRole) {
          string receiver = _params.Param1Value;
          string title = _params.Param2Value;
          string text = _params.Param3Value;
          int expiry_date = _params.Param4Value;
          int level_demand = _params.Param5Value;
          int item_id = _params.Param6Value;
          int item_num = _params.Param7Value;
          int money = _params.Param8Value;
          int gold = _params.Param9Value;
          int stamina = _params.Param10Value;
          Network.LobbyNetworkSystem.Instance.SendMail(receiver, title, text, expiry_date, level_demand, item_id, item_num, money, gold, stamina);
        }
        return false;
      }
    }
    */
    internal class CancelMatchCommand : SimpleStoryCommandBase<CancelMatchCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                GfxSystem.EventChannelForLogic.Publish("ge_cancel_match", "lobby", MatchSceneEnum.Gow);
            }
            return false;
        }
    }
    internal class AddFriendCommand : SimpleStoryCommandBase<AddFriendCommand, StoryValueParam<string>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<string> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                string nick = _params.Param1Value;
                GfxSystem.EventChannelForLogic.Publish("ge_add_friend", "lobby", nick);
            }
            return false;
        }
    }
    internal class DelFriendCommand : SimpleStoryCommandBase<DelFriendCommand, StoryValueParam<ulong>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<ulong> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                ulong guid = _params.Param1Value;
                GfxSystem.EventChannelForLogic.Publish("ge_delete_friend", "lobby", guid);
            }
            return false;
        }
    }

    internal class QueryFriendCommand : SimpleStoryCommandBase<QueryFriendCommand, StoryValueParam<int, string, int, int, int, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int, string, int, int, int, int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                Network.QueryType type = (Network.QueryType)_params.Param1Value;
                string t_name = _params.Param2Value;
                int t_level = _params.Param3Value;
                int t_score = _params.Param4Value;
                int t_fortune = _params.Param5Value;
                int t_gender = _params.Param6Value;
                GfxSystem.EventChannelForLogic.Publish("ge_query_friend_info", "lobby", type, t_name, t_level, t_score, t_fortune, t_gender);
            }
            return false;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------
    //********************************************************分隔线*******************************************************************
    //---------------------------------------------------------------------------------------------------------------------------------
    //只在单人pve场景内有效的命令（仅修改客户端战斗相关数据）
    internal class LevelToCommand : SimpleStoryCommandBase<LevelToCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int lvl = _params.Param1Value;
                LobbyClient.Instance.CurrentRole.Level = lvl;
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                if (null != user)
                {
                    user.SetLevel(lvl);
                }
            }
            return false;
        }
    }
    internal class FullCommand : SimpleStoryCommandBase<FullCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                if (null != user)
                {
                    user.SetHp(Operate_Type.OT_Absolute, user.GetActualProperty().HpMax);
                    user.SetEnergy(Operate_Type.OT_Absolute, user.GetActualProperty().EnergyMax);
                    user.SetRage(Operate_Type.OT_Absolute, user.GetActualProperty().RageMax);
                }
            }
            return false;
        }
    }
    internal class ClearEquipmentsCommand : SimpleStoryCommandBase<ClearEquipmentsCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                LobbyClient.Instance.CurrentRole.ClearEquip();
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                if (null != user)
                {
                    for (int i = 0; i < EquipmentStateInfo.c_EquipmentCapacity; ++i)
                        user.GetEquipmentStateInfo().ResetEquipmentData(i);
                }
            }
            return false;
        }
    }
    internal class AddEquipmentCommand : SimpleStoryCommandBase<AddEquipmentCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int itemId = _params.Param1Value;
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                if (null != user)
                {
                    ItemDataInfo item = new ItemDataInfo();
                    item.ItemConfig = ItemConfigProvider.Instance.GetDataById(itemId);
                    item.ItemNum = 1;
                    if (null != item.ItemConfig)
                    {
                        user.GetEquipmentStateInfo().SetEquipmentData(item.ItemConfig.m_WearParts, item);
                    }
                    LobbyClient.Instance.CurrentRole.Equips[(int)item.ItemConfig.m_WearParts] = item;
                }
            }
            return false;
        }
    }
    internal class ClearSkillsCommand : SimpleStoryCommandBase<ClearSkillsCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                LobbyClient.Instance.CurrentRole.SkillInfos.Clear();
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                if (null != user)
                {
                    user.GetSkillStateInfo().RemoveAllSkill();
                }
            }
            return false;
        }
    }
    internal class AddSkillCommand : SimpleStoryCommandBase<AddSkillCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int skillId = _params.Param1Value;
                SkillInfo skillInfo = new SkillInfo(skillId);
                LobbyClient.Instance.CurrentRole.SkillInfos.Add(skillInfo);
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                if (null != user)
                {
                    user.GetSkillStateInfo().AddSkill(skillInfo);
                }
            }
            return false;
        }
    }
    internal class ClearBuffsCommand : SimpleStoryCommandBase<ClearBuffsCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                if (null != user)
                {
                    user.GetSkillStateInfo().RemoveAllImpact();
                }
            }
            return false;
        }
    }
    internal class AddBuffCommand : SimpleStoryCommandBase<AddBuffCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int impactId = _params.Param1Value;
                UserInfo user = WorldSystem.Instance.GetPlayerSelf();
                if (null != user)
                {
                    ImpactSystem.Instance.SendImpactToCharacter(user, impactId, user.GetId(), /* skillId*/-1, /*duration*/10000, Vector3.Zero, 0.0f);
                }
            }
            return false;
        }
    }
    internal class SetHdCommand : SimpleStoryCommandBase<SetHdCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                int isHD = _params.Param1Value;
                GfxSystem.SendMessage("GfxGameRoot", "SetHD", isHD > 0 ? true : false);
            }
            return false;
        }
    }
    internal class ShowHdCommand : SimpleStoryCommandBase<ShowHdCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            if (null != LobbyClient.Instance.CurrentRole)
            {
                GfxSystem.GfxLog("IsHD:{0}", GlobalVariables.Instance.IsHD);
            }
            return false;
        }
    }
    internal class AllocMemoryCommand : SimpleStoryCommandBase<AllocMemoryCommand, StoryValueParam<string, int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<string, int> _params, long delta)
        {
            string key = _params.Param1Value;
            int size = _params.Param2Value;
            byte[] m = new byte[size];
            if (instance.GlobalVariables.ContainsKey(key))
            {
                instance.GlobalVariables[key] = m;
            }
            else
            {
                instance.GlobalVariables.Add(key, m);
            }
            return false;
        }
    }
    internal class FreeMemoryCommand : SimpleStoryCommandBase<FreeMemoryCommand, StoryValueParam<string>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<string> _params, long delta)
        {
            string key = _params.Param1Value;
            if (instance.GlobalVariables.ContainsKey(key))
            {
                instance.GlobalVariables.Remove(key);
                GC.Collect();
            }
            else
            {
                GC.Collect();
            }
            return false;
        }
    }
    internal class ConsumeCpuCommand : SimpleStoryCommandBase<ConsumeCpuCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            int time = _params.Param1Value;
            long startTime = TimeUtility.GetElapsedTimeUs();
            while (startTime + time > TimeUtility.GetElapsedTimeUs())
            {
            }
            return false;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------
}
