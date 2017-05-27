using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using DashFire.DataStore;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
  internal class GiftCodeInfo
  {
    internal string GiftCode
    { get; set; }
    internal int GiftId
    { get; set; }
    internal bool IsUsed
    { get; set; }  
    internal ulong UserGuid
    { get; set; }  
  }
  internal class GiftCodeSystem
  {
    //初始化礼品码数据
    internal void InitGiftCodeData(List<GiftCodeInfo> giftcodeList)
    {
      if (giftcodeList.Count > 0) {
        foreach (var dataCode in giftcodeList) {
          m_GiftCodes.Add(dataCode.GiftCode, dataCode);
          if (dataCode.UserGuid > 0) {
            Dictionary<int, int> giftCountDict = null;
            if (m_UserGiftCountDict.TryGetValue(dataCode.UserGuid, out giftCountDict)) {
              int count = 0;
              if (giftCountDict.TryGetValue(dataCode.GiftId, out count)) {
                giftCountDict[dataCode.GiftId]++;
              } else {
                giftCountDict.Add(dataCode.GiftId, 1);
              }
            } else {
              giftCountDict = new Dictionary<int, int>();
              giftCountDict.Add(dataCode.GiftId, 1);
              m_UserGiftCountDict.Add(dataCode.UserGuid, giftCountDict);
            }
          }         
        }
      }
      m_IsDataLoaded = true;
    }
    //领取礼品
    internal GeneralOperationResult ExchangeGift(ulong userGuid, string giftcode, out int giftId)
    {
      giftId = 0;
      GeneralOperationResult ret = GeneralOperationResult.LC_Failure_Unknown;
      DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
      UserInfo user = scheduler.GetUserInfo(userGuid);
      if (null != user) {
        GiftCodeInfo giftcodeInfo = null;
        m_GiftCodes.TryGetValue(giftcode, out giftcodeInfo);
        if (giftcodeInfo != null) {
          giftId = giftcodeInfo.GiftId;
          Dictionary<int, int> giftCountDict = null;
          if (m_UserGiftCountDict.TryGetValue(user.Guid, out giftCountDict)) {
            int count = 0;
            if (giftCountDict.TryGetValue(giftId, out count)) {
              if (count >= s_UserMaxGiftCount) {
                //该种礼包的领取次数超过限制
                ret = GeneralOperationResult.LC_Failure_Overflow;
                return ret;
              }
            }
          }
          if (giftcodeInfo.IsUsed == false) {
            //礼品码可用                         
            GiftConfig giftConfig = GiftConfigProvider.Instance.GetDataById(giftcodeInfo.GiftId);
            if (null != giftConfig) {
              //扣
              giftcodeInfo.IsUsed = true;
              giftcodeInfo.UserGuid = userGuid;
              if (giftCountDict != null) {
                 int count = 0;
                 if (giftCountDict.TryGetValue(giftId, out count)) {
                   giftCountDict[giftId]++;
                 } else {
                   giftCountDict.Add(giftId, 1);
                 }
              } else {
                giftCountDict = new Dictionary<int, int>();
                giftCountDict.Add(giftId, 1);
                m_UserGiftCountDict.Add(userGuid, giftCountDict);
              }             
              var ds_thread = LobbyServer.Instance.DataStoreThread;
              if (ds_thread.DataStoreAvailable) {
                ds_thread.DSSaveGiftCode(giftcodeInfo, true);
              }
              //给             
              for (int i = 0; i < giftConfig.ItemIdList.Count; ++i) {
                if (giftConfig.ItemIdList[i] > 0) {
                  scheduler.DispatchAction(scheduler.DoAddItem, userGuid, giftConfig.ItemIdList[i], giftConfig.ItemNumList[i], GainConsumePos.Gift.ToString());
                }
              }
              ret = GeneralOperationResult.LC_Succeed;
            }
          } else {
            //礼品码已经使用
            ret = GeneralOperationResult.LC_Failure_Code_Used;
          }
        } else {
          //礼品码错误
          ret = GeneralOperationResult.LC_Failure_Code_Error;
        }
      }
      return ret;
    }

    private Dictionary<string, GiftCodeInfo> m_GiftCodes = new Dictionary<string, GiftCodeInfo>();                      //礼品码
    private Dictionary<ulong, Dictionary<int, int>> m_UserGiftCountDict = new Dictionary<ulong, Dictionary<int, int>>();    //每个玩家所对应的领取次数
    private object m_Lock = new object();
    private bool m_IsDataLoaded = false;
    private static int s_UserMaxGiftCount = 5;
  }
}
