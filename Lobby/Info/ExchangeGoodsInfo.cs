using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using CSharpCenterClient;
using Lobby_RoomServer;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal class ExchangeGoodsInfo
    {
        internal GeneralOperationResult CheckOKBuy(int id, StoreConfig sc, int exchangecurrency, out int currency)
        {
            currency = 0;
            if (sc != null)
            {
                if (sc.m_HaveDayLimit)
                {
                    int val;
                    if (GoodsBuyData.TryGetValue(id, out val))
                    {
                        if (sc.m_DayLimit > val)
                        {
                            if (sc.m_Price.Count > val)
                            {
                                if (exchangecurrency >= sc.m_Price[val])
                                {
                                    currency = sc.m_Price[val];
                                    return GeneralOperationResult.LC_Succeed;
                                }
                                else
                                {
                                    return GeneralOperationResult.LC_Failure_CostError;
                                }
                            }
                            else
                            {
                                return GeneralOperationResult.LC_Failure_LevelError;
                            }
                        }
                        else
                        {
                            return GeneralOperationResult.LC_Failure_Overflow;
                        }
                    }
                    else
                    {
                        if (exchangecurrency >= sc.m_Price[0])
                        {
                            currency = sc.m_Price[0];
                            return GeneralOperationResult.LC_Succeed;
                        }
                        else
                        {
                            return GeneralOperationResult.LC_Failure_CostError;
                        }
                    }
                }
                else
                {
                    if (exchangecurrency >= sc.m_Price[0])
                    {
                        currency = sc.m_Price[0];
                        return GeneralOperationResult.LC_Succeed;
                    }
                    else
                    {
                        return GeneralOperationResult.LC_Failure_CostError;
                    }
                }
            }
            else
            {
                return GeneralOperationResult.LC_Failure_Unknown;
            }
        }
        internal void BuyGoods(int id)
        {
            int val;
            if (GoodsBuyData.TryGetValue(id, out val))
            {
                GoodsBuyData[id] = val + 1;
            }
            else
            {
                GoodsBuyData.TryAdd(id, 1);
            }
        }
        internal int GetNum(int id)
        {
            int ret;
            GoodsBuyData.TryGetValue(id, out ret);
            return ret;
        }
        internal void AddGoodData(int goodId, int number)
        {
            int val;
            if (GoodsBuyData.TryGetValue(goodId, out val))
            {
                GoodsBuyData[goodId] = val + number;
            }
            else
            {
                GoodsBuyData.TryAdd(goodId, number);
            }
        }
        internal void Reset()
        {
            GoodsBuyData.Clear();
            CurrencyRefreshNum.Clear();
        }
        internal void ResetByCurrency(int currency)
        {
            List<int> removelist = new List<int>();
            StoreConfig sc = null;
            foreach (int key in GoodsBuyData.Keys)
            {
                sc = StoreConfigProvider.Instance.GetDataById(key);
                if (sc != null && sc.m_Currency == currency)
                {
                    removelist.Add(key);
                }
            }
            foreach (int remove in removelist)
            {
                int item;
                GoodsBuyData.TryRemove(remove, out item);
            }
            removelist.Clear();
        }
        internal ConcurrentDictionary<int, int> GetAllGoodsData()
        {
            return GoodsBuyData;
        }
        private ConcurrentDictionary<int, int> GoodsBuyData = new ConcurrentDictionary<int, int>();
        internal ConcurrentDictionary<int, int> CurrencyRefreshNum = new ConcurrentDictionary<int, int>();
    }
}
