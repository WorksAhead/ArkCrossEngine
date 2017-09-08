using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire;

namespace Lobby
{
    internal interface IItemUseLogic
    {
        //获取技能逻辑Id
        ItemLogicId ItemLogicId
        { get; set; }

        bool CanUse(UserInfo user, ItemInfo item);
        void Use(UserInfo user, ItemInfo item);

    }
}
