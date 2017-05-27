using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire;

namespace Lobby
{
  internal enum ItemLogicId
  {
    ItemLogicStart = 10000,                     //道具逻辑ID基数
    ItemLogic_Exchange = ItemLogicStart + 1,    //兑换逻辑   
    MaxNum,
  }
  internal class ItemLogicManager
  {    
    #region Singleton
    internal static ItemLogicManager Instance
    {
      get { return s_Instance; }
    }
    private static ItemLogicManager s_Instance = new ItemLogicManager();
    #endregion    

    internal IItemUseLogic GetItemUseLogic(int id)
    {
      IItemUseLogic logic;
      m_ItemUseLogics.TryGetValue(id, out logic);
      return logic;
    }

    private ItemLogicManager()
    {      
      RegisterItemLogic(new ItemLogicExchange());
    }

    private Dictionary<int, IItemUseLogic> m_ItemUseLogics = new Dictionary<int, IItemUseLogic>();

    private void RegisterItemLogic(IItemUseLogic itemLogic)
    {
      if (m_ItemUseLogics.ContainsKey((int)itemLogic.ItemLogicId))
      {
        return;
      }
      m_ItemUseLogics.Add((int)itemLogic.ItemLogicId, itemLogic);
    }
  }
}
