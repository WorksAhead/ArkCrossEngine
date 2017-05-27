using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobby
{
  class ItemLogicExchange :IItemUseLogic
  {
    internal ItemLogicExchange()
    {
      this.ItemLogicId = ItemLogicId.ItemLogic_Exchange;
    }
    public ItemLogicId ItemLogicId
    { get; set; }

    public bool CanUse(UserInfo user, ItemInfo item)
    {
      return true;
    }

    public void Use(UserInfo user, ItemInfo item)
    {    
    }    
  }
}
