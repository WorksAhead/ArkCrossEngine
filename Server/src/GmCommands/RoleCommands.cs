using System;
using System.Collections.Generic;
using StorySystem;
using ArkCrossEngine;

namespace DashFire.GmCommands
{
    //---------------------------------------------------------------------------------------------------------------------------------
    //********************************************************分隔线*******************************************************************
    //---------------------------------------------------------------------------------------------------------------------------------
    //只在场景内有效的命令（仅修改RoomServer战斗相关数据）
    internal class LevelToCommand : SimpleStoryCommandBase<LevelToCommand, StoryValueParam<int>>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam<int> _params, long delta)
        {
            object us;
            if (instance.GlobalVariables.TryGetValue("UserInfo", out us))
            {
                UserInfo user = us as UserInfo;
                if (null != user)
                {
                    int lvl = _params.Param1Value;
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
            object us;
            if (instance.GlobalVariables.TryGetValue("UserInfo", out us))
            {
                UserInfo user = us as UserInfo;
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
            object us;
            if (instance.GlobalVariables.TryGetValue("UserInfo", out us))
            {
                UserInfo user = us as UserInfo;
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
            object us;
            if (instance.GlobalVariables.TryGetValue("UserInfo", out us))
            {
                UserInfo user = us as UserInfo;
                if (null != user)
                {
                    int itemId = _params.Param1Value;
                    ItemDataInfo item = new ItemDataInfo();
                    item.ItemConfig = ItemConfigProvider.Instance.GetDataById(itemId);
                    item.ItemNum = 1;
                    if (null != item.ItemConfig)
                    {
                        user.GetEquipmentStateInfo().SetEquipmentData(item.ItemConfig.m_WearParts, item);
                    }
                }
            }
            return false;
        }
    }
    internal class ClearSkillsCommand : SimpleStoryCommandBase<ClearSkillsCommand, StoryValueParam>
    {
        protected override bool ExecCommand(StoryInstance instance, StoryValueParam _params, long delta)
        {
            object us;
            if (instance.GlobalVariables.TryGetValue("UserInfo", out us))
            {
                UserInfo user = us as UserInfo;
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
            object us;
            if (instance.GlobalVariables.TryGetValue("UserInfo", out us))
            {
                UserInfo user = us as UserInfo;
                if (null != user)
                {
                    int skillId = _params.Param1Value;
                    SkillInfo skillInfo = new SkillInfo(skillId);
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
            object us;
            if (instance.GlobalVariables.TryGetValue("UserInfo", out us))
            {
                UserInfo user = us as UserInfo;
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
            object us;
            if (instance.GlobalVariables.TryGetValue("UserInfo", out us))
            {
                UserInfo user = us as UserInfo;
                if (null != user)
                {
                    int impactId = _params.Param1Value;
                    ImpactSystem.Instance.SendImpactToCharacter(user, impactId, user.GetId(), /* skillId*/-1, /*duration*/10000, Vector3.Zero, 0.0f);
                }
            }
            return false;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------
}
