using System;
using System.Text;
using DashFire;
using Messenger;
using DashFire.DataStore;
using Google.ProtocolBuffers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ArkCrossEngine;

namespace Lobby
{
  /// <summary>
  /// 数据访问层线程。此类主要用于数据存储相关的操作
  /// </summary>  
  internal sealed class DataStoreThread : MyServerThread
  {
    internal delegate void DSPLoadAccountCB(DSLoadResult ret, DSP_Account data);    
    internal delegate void DSPLoadUserCB(DSLoadResult ret, DSP_User data);
    internal delegate void DSPLoadUserArenaCB(DSLoadResult ret, ArenaInfo arenaInfo, List<ChallengeInfo> arenaRecordList);
    //全局数据加载状态
    internal enum DataInitStatus
    {
      Unload = 0,   //未加载
      Loading,      //从DS加载中
      Done          //完成加载
    }
    internal DataStoreThread()
    {      
    }
    internal DataStoreClient DataStoreClient
    {
      get { return m_DataStoreClient; }
    }
    internal bool DataStoreAvailable
    {
      get { return m_DataStoreAvailable; }
    }
    protected override void OnStart()
    {     
      //TO DO:下面两个参数决定了该线程的消息处理量      
      TickSleepTime = 10;         //Tick的时间间隔
      ActionNumPerTick = 500;     //每个Tick需要处理的消息数量
      m_DataStoreAvailable = LobbyConfig.DataStoreAvailable;     
      if (LobbyConfig.DataStoreAvailable == true) {
        LogSys.Log(LOG_TYPE.INFO, "Connect to DataStoreNode ...");
        ConnectDataStoreNode();
      }
    }
    protected override void OnTick()
    {      
      long curTime = TimeUtility.GetServerMilliseconds();
      if (m_LastLogTime + 60000 < curTime) {
        m_LastLogTime = curTime;
        DebugPoolCount((string msg) => {
          LogSys.Log(LOG_TYPE.INFO, "DataStoreThread.ActionQueue {0}", msg);
        });        
      }
      if (LobbyConfig.DataStoreAvailable == true) {
        if (m_LastTimeoutTickTime + c_TimeoutTickInterval < curTime) {
          m_LastTimeoutTickTime = curTime;
          if (m_DataStoreClient != null) {
            m_DataStoreClient.Tick();
          }
          if (m_DataStoreClient.CurrentStatus == DataStoreClient.ConnectStatus.Disconnect) {
            ConnectDataStoreNode();
          } else if (m_DataStoreClient.CurrentStatus == DataStoreClient.ConnectStatus.Connected) {
            OnConnectDataStore();
          }
        }
      } else {
        OnConnectDataStore();
      }   
    }
    internal void Init(PBChannel channel)
    {
      m_DataStoreClient = new DataStoreClient(channel,this);     
    }   
    //与DataStoreNode建立连接
    internal void ConnectDataStoreNode()
    {
      if (m_DataStoreClient.CurrentStatus == DataStoreClient.ConnectStatus.Connecting || m_DataStoreClient.CurrentStatus == DataStoreClient.ConnectStatus.Connected) {
        return;
      }     
      string clientName = "Lobby";      
      m_DataStoreClient.Connect(clientName, (ret, error) =>
      {
        if (ret == true) {          
          m_DataStoreClient.CurrentStatus = DataStoreClient.ConnectStatus.Connected;
          LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Connect to DataStoreNode Success.");
          OnConnectDataStore();
        } else {
          m_DataStoreClient.CurrentStatus = DataStoreClient.ConnectStatus.Disconnect;
          LogSys.Log(LOG_TYPE.ERROR, "Connect to DataStoreNode Failed...Error:{0}", error);          
        }
      });
    }
    private void OnConnectDataStore()
    {
      //全局数据加载的顺序：Guid->Nickname->Mail->Gowstar
      InitGuidData();
      InitActivationCodeData();
      InitNicknameData();
      InitMailData();
      InitGowStarData();
      InitGiftCodeData();
      InitArenaData();
      InitArenaRecordData();
    }
    //与DataStoreNode建立连接后加载数据
    private void InitGuidData()
    {      
      if (m_GuidInitStatus == DataInitStatus.Unload) {
        if (LobbyConfig.DataStoreAvailable == true) {
          m_GuidInitStatus = DataInitStatus.Loading;
          uint dsgMsgId = MessageMapping.Query(typeof(DSG_Guid));
          m_DataStoreClient.Load<DSG_Guid>(dsgMsgId.ToString(), ((ret, error, data) =>
          {
            if (ret == DSLoadResult.Success) {
              List<GuidInfo> guidList = new List<GuidInfo>();
              foreach (var dataGuid in data.GuidListList) {
                GuidInfo guidinfo = new GuidInfo();
                guidinfo.GuidType = dataGuid.GuidType;
                guidinfo.NextGuid = dataGuid.GuidValue;
                guidList.Add(guidinfo);
              }
              LobbyServer.Instance.GlobalDataProcessThread.InitGuidData(guidList);
              m_GuidInitStatus = DataInitStatus.Done;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data success. Table:DSG_Guid");
            } else {
              m_GuidInitStatus = DataInitStatus.Unload;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Red, "Load DataStore global data failed. Table: {0}, Error: {1}", "DSG_Guid", error);
            }
          }));
        } else {
          m_GuidInitStatus = DataInitStatus.Done;
        }
      }
    }
    private void InitActivationCodeData()
    {
      if (m_ActivationInitStatus == DataInitStatus.Unload && m_GuidInitStatus == DataInitStatus.Done) {
        if (m_DataStoreAvailable && LobbyConfig.ActivationCodeAvailable) {
          m_ActivationInitStatus = DataInitStatus.Loading;
          uint dsgMsgId = MessageMapping.Query(typeof(DSG_ActivationCode));
          List<DS_ActivationCode> activationCodeList = new List<DS_ActivationCode>();
          m_DataStoreClient.Load<DSG_ActivationCode>(dsgMsgId.ToString(), ((ret, error, data) =>
          {            
            if (ret == DSLoadResult.Success) {
              activationCodeList.AddRange(data.ActivationCodeListList);
              LobbyServer.Instance.DataProcessScheduler.InitActivationCodeData(activationCodeList);
              m_ActivationInitStatus = DataInitStatus.Done;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data success. Table: {0}", "DS_ActivationCode");
            } else if (ret == DSLoadResult.Undone) {
              activationCodeList.AddRange(data.ActivationCodeListList);
              m_ActivationInitStatus = DataInitStatus.Loading;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data undone. Continuing .... Table: {0}", "DS_ActivationCode");
            } else {
              m_ActivationInitStatus = DataInitStatus.Unload;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Red, "Load DataStore global data failed. Table: {0}, Error: {1}", "DS_ActivationCode", error);
            }
          }));
        } else {
          m_ActivationInitStatus = DataInitStatus.Done;
        }                
      }
    }    
    private void InitNicknameData()
    {
      if (m_NicknameInitStatus == DataInitStatus.Unload && m_GuidInitStatus == DataInitStatus.Done) {
        if (m_DataStoreAvailable) {
          m_NicknameInitStatus = DataInitStatus.Loading;
          uint dsgMsgId = MessageMapping.Query(typeof(DSG_Nickname));
          List<DS_Nickname> nicknameList = new List<DS_Nickname>();
          m_DataStoreClient.Load<DSG_Nickname>(dsgMsgId.ToString(), ((ret, error, data) =>
          {            
            DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
            if (ret == DSLoadResult.Success) {
              nicknameList.AddRange(data.NicknameListList);
              scheduler.InitNicknameData(nicknameList);                       
              m_NicknameInitStatus = DataInitStatus.Done;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data success. Table: {0}", "DS_Nickname");
            } else if (ret == DSLoadResult.Undone) {
              nicknameList.AddRange(data.NicknameListList);
              m_NicknameInitStatus = DataInitStatus.Loading;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data undone. Continuing .... Table: {0}", "DS_Nickname");
            } else {
              m_NicknameInitStatus = DataInitStatus.Unload;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Red, "Load DataStore global data failed. Table: {0}, Error: {1}", "DS_Nickname", error);
            }
          }));
        } else {
          List<DS_Nickname> nicknameList = new List<DS_Nickname>();
          DataProcessScheduler scheduler = LobbyServer.Instance.DataProcessScheduler;
          scheduler.InitNicknameData(nicknameList);
          m_NicknameInitStatus = DataInitStatus.Done;
        }
      }
    }    
    private void InitMailData()
    {
      if (m_MailInitStatus == DataInitStatus.Unload && m_NicknameInitStatus == DataInitStatus.Done) {
        if (m_DataStoreAvailable) {
          m_MailInitStatus = DataInitStatus.Loading;
          uint dsgMsgId = MessageMapping.Query(typeof(DSG_Mail));
          m_DataStoreClient.Load<DSG_Mail>(dsgMsgId.ToString(), ((ret, error, data) =>
          {
            if (ret == DSLoadResult.Success) {
              List<MailInfo> totalMailList = new List<MailInfo>(data.MailListCount);
              foreach (var mailData in data.MailListList) {
                MailInfo mail = new MailInfo();
                mail.m_MailGuid = (ulong)mailData.Guid;
                mail.m_Module = (ModuleMailTypeEnum)mailData.ModuleTypeId;
                mail.m_Sender = mailData.Sender;
                mail.m_Receiver = (ulong)mailData.Receiver;
                mail.m_SendTime = DateTime.Parse(mailData.SendDate);
                mail.m_ExpiryDate = DateTime.Parse(mailData.ExpiryDate);
                mail.m_Title = mailData.Title;
                mail.m_Text = mailData.Text;
                mail.m_Money = mailData.Money;
                mail.m_Gold = mailData.Gold;
                mail.m_Stamina = mailData.Stamina;
                List<int> itemIds = new List<int>();
                string[] itemIdArray = mailData.ItemIds.Split(new char[] { '|' });
                foreach (string itemIdStr in itemIdArray) {
                  if (itemIdStr.Trim() != string.Empty) {
                    int id = -1;
                    if (int.TryParse(itemIdStr, out id)) {
                      itemIds.Add(id);
                    }
                  }
                }
                List<int> itemNums = new List<int>();
                string[] itemNumArray = mailData.ItemNumbers.Split(new char[] { '|' });
                foreach (string itemNumStr in itemNumArray) {
                  if (itemNumStr.Trim() != string.Empty) {
                    int num = -1;
                    if (int.TryParse(itemNumStr, out num)) {
                      itemNums.Add(num);
                    }
                  }
                }
                for (int i = 0; i < itemIds.Count; ++i) {
                  MailItem item = new MailItem();
                  item.m_ItemId = itemIds[i];
                  item.m_ItemNum = itemNums[i];
                  mail.m_Items.Add(item);
                }
                mail.m_LevelDemand = mailData.LevelDemand;
                mail.m_AlreadyRead = mailData.IsRead;
                totalMailList.Add(mail);
              }
              LobbyServer.Instance.GlobalDataProcessThread.InitMailData(totalMailList);
              m_MailInitStatus = DataInitStatus.Done;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data success. Table: {0}", "DS_MailInfo");
            } else {
              m_MailInitStatus = DataInitStatus.Unload;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Red, "Load DataStore global data failed. Table: {0}, Error: {1}", "DS_MailInfo", error);
            }
          }));
        } else {
          List<MailInfo> totalMailList = new List<MailInfo>();
          LobbyServer.Instance.GlobalDataProcessThread.InitMailData(totalMailList);
          m_MailInitStatus = DataInitStatus.Done;
        }
      }
    }
    private void InitGowStarData()
    {
      if (m_GowStarInitStatus == DataInitStatus.Unload && m_MailInitStatus == DataInitStatus.Done) {
        if (m_DataStoreAvailable) {
          m_GowStarInitStatus = DataInitStatus.Loading;
          uint dsgMsgId = MessageMapping.Query(typeof(DSG_GowStar));
          m_DataStoreClient.Load<DSG_GowStar>(dsgMsgId.ToString(), ((ret, error, data) =>
          {
            if (ret == DSLoadResult.Success) {
              List<GowStarInfo> gowstarList = new List<GowStarInfo>(data.GowStarListCount);
              foreach (var gowstarData in data.GowStarListList) {
                GowStarInfo gsi = new GowStarInfo();
                gsi.m_Guid = (ulong)gowstarData.UserGuid;
                gsi.m_Nick = gowstarData.Nickname;
                gsi.m_HeroId = gowstarData.HeroId;
                gsi.m_Level = gowstarData.Level;
                gsi.m_FightingScore = gowstarData.FightingScore;
                gsi.m_GowElo = gowstarData.GowElo;
                gowstarList.Add(gsi);
              }
              LobbyServer.Instance.GlobalDataProcessThread.InitGowStarData(gowstarList);
              m_GowStarInitStatus = DataInitStatus.Done;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data success. Table: {0}", "DS_GowStar");
            } else {
              m_GowStarInitStatus = DataInitStatus.Unload;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Red, "Load DataStore global data failed. Table: {0}, Error: {1}", "DS_GowStar", error);
            }
          }));
        } else {
          List<GowStarInfo> gowstarList = new List<GowStarInfo>();
          LobbyServer.Instance.GlobalDataProcessThread.InitGowStarData(gowstarList);
          m_GowStarInitStatus = DataInitStatus.Done;
        }
      }
    }
    private void InitGiftCodeData()
    {
      if (m_GiftInitStatus == DataInitStatus.Unload && m_GuidInitStatus == DataInitStatus.Done) {
        if (m_DataStoreAvailable) {
          m_GiftInitStatus = DataInitStatus.Loading;
          uint dsgMsgId = MessageMapping.Query(typeof(DSG_GiftCode));
          List<GiftCodeInfo> giftcodeList = new List<GiftCodeInfo>();
          m_DataStoreClient.Load<DSG_GiftCode>(dsgMsgId.ToString(), ((ret, error, data) =>
          {            
            if (ret == DSLoadResult.Success) {              
              foreach (DS_GiftCode dataCode in data.GiftCodeListList) {
                GiftCodeInfo giftcode = new GiftCodeInfo();
                giftcode.GiftCode = dataCode.GiftCode;
                giftcode.GiftId = dataCode.GiftId;
                giftcode.IsUsed = dataCode.IsUsed;
                giftcode.UserGuid = (ulong)dataCode.UserGuid;
                giftcodeList.Add(giftcode);
              }
              LobbyServer.Instance.GlobalDataProcessThread.InitGiftCodeData(giftcodeList);
              m_GiftInitStatus = DataInitStatus.Done;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data success. Table: {0}", "DSG_GiftCode");
            } else if (ret == DSLoadResult.Undone) {
              foreach (DS_GiftCode dataCode in data.GiftCodeListList) {
                GiftCodeInfo giftcode = new GiftCodeInfo();
                giftcode.GiftCode = dataCode.GiftCode;
                giftcode.GiftId = dataCode.GiftId;
                giftcode.IsUsed = dataCode.IsUsed;
                giftcode.UserGuid = (ulong)dataCode.UserGuid;
                giftcodeList.Add(giftcode);
              }
              m_GiftInitStatus = DataInitStatus.Loading;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data undone. Continuing .... Table: {0}", "DSG_GiftCode");
            } else {
              m_GiftInitStatus = DataInitStatus.Unload;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Red, "Load DataStore global data failed. Table: {0}, Error: {1}", "DSG_GiftCode", error);
            }
          }));
        } else {
          m_GiftInitStatus = DataInitStatus.Done;
        }
      }
    }
    private void InitArenaData()
    {
      if (m_ArenaRankInitStatus == DataInitStatus.Unload && m_GiftInitStatus == DataInitStatus.Done) {
        if (m_DataStoreAvailable) {
          m_ArenaRankInitStatus = DataInitStatus.Loading;
          uint dsgMsgId = MessageMapping.Query(typeof(DSG_ArenaRank));
          List<ArenaInfo> arenaList = new List<ArenaInfo>();
          m_DataStoreClient.Load<DSG_ArenaRank>(dsgMsgId.ToString(), ((ret, error, data) =>
          {
            if (ret == DSLoadResult.Success) {
              foreach (DS_ArenaInfo dataArena in data.ArenaListList) {
                ArenaInfo arenaInfo = DeserializeArena(dataArena);
                arenaList.Add(arenaInfo);
              }
              LobbyServer.Instance.GlobalDataProcessThread.InitArenaData(arenaList);
              m_ArenaRankInitStatus = DataInitStatus.Done;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data success. Table: {0}", "DSG_ArenaRank");
            } else if (ret == DSLoadResult.Undone) {
              foreach (DS_ArenaInfo dataArena in data.ArenaListList) {
                ArenaInfo arenaInfo = DeserializeArena(dataArena);
                arenaList.Add(arenaInfo);
              }
              m_ArenaRankInitStatus = DataInitStatus.Loading;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data undone. Continuing .... Table: {0}", "DSG_ArenaRank");
            } else {
              m_ArenaRankInitStatus = DataInitStatus.Unload;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Red, "Load DataStore global data failed. Table: {0}, Error: {1}", "DSG_ArenaRank", error);
            }
          }));
        } else {
          List<ArenaInfo> arenaList = new List<ArenaInfo>();
          LobbyServer.Instance.GlobalDataProcessThread.InitArenaData(arenaList);
          m_ArenaRankInitStatus = DataInitStatus.Done;
        }
      }
    }
    private void InitArenaRecordData()
    {
      if (m_ArenaRecordInitStatus == DataInitStatus.Unload && m_ArenaRankInitStatus == DataInitStatus.Done) {
        if (m_DataStoreAvailable) {
          m_ArenaRecordInitStatus = DataInitStatus.Loading;
          uint dsgMsgId = MessageMapping.Query(typeof(DSG_ArenaRecord));
          List<Tuple<ulong, ChallengeInfo>> recordList = new List<Tuple<ulong, ChallengeInfo>>();
          m_DataStoreClient.Load<DSG_ArenaRecord>(dsgMsgId.ToString(), ((ret, error, data) =>
          {
            if (ret == DSLoadResult.Success) {
              foreach (DS_ArenaRecord dataRecord in data.RecordListList) {
                Tuple<ulong,ChallengeInfo> record =  CreateArenaRecord(dataRecord);
                recordList.Add(record);
              }
              LobbyServer.Instance.GlobalDataProcessThread.InitChallengeData(recordList);
              m_ArenaRecordInitStatus = DataInitStatus.Done;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data success. Table: {0}", "DSG_ArenaRecord");
            } else if (ret == DSLoadResult.Undone) {
              foreach (DS_ArenaRecord dataRecord in data.RecordListList) {
                Tuple<ulong, ChallengeInfo> record = CreateArenaRecord(dataRecord);
                recordList.Add(record);
              }
              m_ArenaRecordInitStatus = DataInitStatus.Loading;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Green, "Load DataStore global data undone. Continuing .... Table: {0}", "DSG_ArenaRecord");
            } else {
              m_ArenaRecordInitStatus = DataInitStatus.Unload;
              LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Red, "Load DataStore global data failed. Table: {0}, Error: {1}", "DSG_ArenaRecord", error);
            }
          }));
        } else {
          List<Tuple<ulong, ChallengeInfo>> recordList = new List<Tuple<ulong, ChallengeInfo>>();
          LobbyServer.Instance.GlobalDataProcessThread.InitChallengeData(recordList);
          m_ArenaRecordInitStatus = DataInitStatus.Done;
        }
      }
    }   
    //--------------------------------------------------------------------------------------------------------------------------
    // 供外部直接调用的方法，实际执行线程是调用线程。
    //--------------------------------------------------------------------------------------------------------------------------
    internal void DSPSaveCreateAccount(AccountInfo ai, string activationCode)
    {
      try {
        DSP_CreateAccount.Builder dsgNewAccountBuilder = DSP_CreateAccount.CreateBuilder();
        long[] userGuids = new long[3];
        for (int i = 0; i < ai.Users.Count; ++i) {
          if (ai.Users[i] != null) {
            userGuids[i] = (long)ai.Users[i].Guid;
          }
        }
        DS_Account.Builder dataAccountBuilder = DS_Account.CreateBuilder();
        dataAccountBuilder.SetAccount(ai.AccountId);
        dataAccountBuilder.SetIsValid(true);
        dataAccountBuilder.SetIsBanned(false);
        dataAccountBuilder.SetUserGuid1(userGuids[0]);
        dataAccountBuilder.SetUserGuid2(userGuids[1]);
        dataAccountBuilder.SetUserGuid3(userGuids[2]);
        DS_Account dataAccount = dataAccountBuilder.Build();
        DS_ActivationCode.Builder dataCodeBuilder = DS_ActivationCode.CreateBuilder();
        dataCodeBuilder.SetActivationCode(activationCode);
        dataCodeBuilder.SetIsValid(true);
        dataCodeBuilder.SetIsActivated(true);
        dataCodeBuilder.SetAccount(ai.AccountId);
        DS_ActivationCode dataCode = dataCodeBuilder.Build();
        dsgNewAccountBuilder.SetAccount(ai.AccountId);
        dsgNewAccountBuilder.SetAccountBasic(dataAccount);
        dsgNewAccountBuilder.SetUsedActivationCode(dataCode);
        DSP_CreateAccount dsgNewAccount = dsgNewAccountBuilder.Build();
        QueueAction(DSPSaveCreateAccountInternal, ai.AccountId, dsgNewAccount);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR. Msg:DSP_CreateAccount, Key:{0}, Error:{1},\nStacktrace:{2}", ai.AccountId, e.Message, e.StackTrace);
      }
    }
    internal void DSPSaveCreateUser(AccountInfo ai, string nickname, ulong userGuid)
    {
      try {
        DSP_CreateUser.Builder dsgNewUserBuilder = DSP_CreateUser.CreateBuilder();
        long[] userGuids = new long[3];
        for (int i = 0; i < ai.Users.Count; ++i) {
          if (ai.Users[i] != null) {
            userGuids[i] = (long)ai.Users[i].Guid;
          }
        }
        DS_Account.Builder dataAccountBuilder = DS_Account.CreateBuilder();
        dataAccountBuilder.SetAccount(ai.AccountId);
        dataAccountBuilder.SetIsBanned(false);
        dataAccountBuilder.SetIsValid(true);
        dataAccountBuilder.SetUserGuid1(userGuids[0]);
        dataAccountBuilder.SetUserGuid2(userGuids[1]);
        dataAccountBuilder.SetUserGuid3(userGuids[2]);
        DS_Account dataAccount = dataAccountBuilder.Build();
        DS_Nickname.Builder dataNicknameBuilder = DS_Nickname.CreateBuilder();
        dataNicknameBuilder.SetNickname(nickname);
        dataNicknameBuilder.SetIsValid(true);
        dataNicknameBuilder.SetUserGuid((long)userGuid);        
        DS_Nickname dataNickname = dataNicknameBuilder.Build();
        dsgNewUserBuilder.SetAccount(ai.AccountId);
        dsgNewUserBuilder.SetAccountBasic(dataAccount);
        dsgNewUserBuilder.SetUsedNickname(dataNickname);
        DSP_CreateUser dsgNewUser = dsgNewUserBuilder.Build();
        QueueAction(DSPSaveCreateUserInternal, ai.AccountId, dsgNewUser);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR. Msg:DSP_CreateUser, Key:{0}, Error:{1},\nStacktrace:{2}", ai.AccountId, e.Message, e.StackTrace);
      }
    }
    internal void DSPSaveUser(UserInfo ui, long saveCount)
    {
      try {        
        long userGuid = (long)ui.Guid;
        DSP_User.Builder dataUserBuilder = DSP_User.CreateBuilder();
        dataUserBuilder.SetUserGuid(userGuid);
        //角色基础数据
        DS_UserInfo.Builder userBasicBuilder = DS_UserInfo.CreateBuilder();
        userBasicBuilder.SetGuid(userGuid);
        userBasicBuilder.SetAccountId(ui.AccountId);
        userBasicBuilder.SetIsValid(true);
        userBasicBuilder.SetNickname(ui.Nickname);
        userBasicBuilder.SetHeroId(ui.HeroId);
        userBasicBuilder.SetLevel(ui.Level);
        userBasicBuilder.SetMoney((int)ui.Money);
        userBasicBuilder.SetGold((int)ui.Gold);
        userBasicBuilder.SetExpPoints(ui.ExpPoints);
        userBasicBuilder.SetVip(ui.Vip);
        userBasicBuilder.SetCitySceneId(ui.CitySceneId);
        userBasicBuilder.SetLastLogoutTime(ui.LastLogoutTime.ToString());        
        userBasicBuilder.SetCreateTime(ui.CreateTime.ToString());
        userBasicBuilder.SetNewbieStep(ui.NewBieGuideInfo.NewbieFlag);
        userBasicBuilder.SetNewbieActionFlag(ui.NewBieGuideInfo.NewbieActionFlag);
        dataUserBuilder.SetUserBasic(userBasicBuilder.Build());

        DS_UserInfoExtra.Builder userExtraBuilder = DS_UserInfoExtra.CreateBuilder();
        userExtraBuilder.SetGuid(userGuid);
        userExtraBuilder.SetIsValid(true);
        userExtraBuilder.SetGowElo(ui.GowInfo.GowElo);
        userExtraBuilder.SetGowMatches(ui.GowInfo.GowMatches);
        userExtraBuilder.SetGowWinMatches(ui.GowInfo.GowWinMatches);
        userExtraBuilder.SetStamina(ui.CurStamina);
        userExtraBuilder.SetBuyStaminaCount(ui.CurBuyStaminaCount);
        userExtraBuilder.SetLastAddStaminaTimestamp(ui.LastAddStaminaTimestamp);
        userExtraBuilder.SetBuyMoneyCount(ui.CurBuyMoneyCount);
        userExtraBuilder.SetLastBuyMoneyTimestamp(ui.LastBuyMoneyTimestamp);
        userExtraBuilder.SetSellIncome(ui.CurSellItemGoldIncome);
        userExtraBuilder.SetLastSellTimestamp(ui.LastSellItemGoldIncomeTimestamp);
        userExtraBuilder.SetLastResetStaminaTime(ui.LastResetStaminaTime.ToString());
        userExtraBuilder.SetLastResetMidasTouchTime(ui.LastResetMidasTouchTime.ToString());
        userExtraBuilder.SetLastResetSellTime(ui.LastResetSellItemIncomeTime.ToString());
        userExtraBuilder.SetLastResetDailyMissionTime(ui.LastResetDailyMissionTime.ToString());       
        userExtraBuilder.SetActivePartnerId(ui.PartnerStateInfo.GetActivePartnerId());
        userExtraBuilder.SetAttemptAward(ui.AttemptAward);
        userExtraBuilder.SetAttemptCurAcceptedCount(ui.AttemptCurAcceptedCount);
        userExtraBuilder.SetAttemptAcceptedAward(ui.AttemptAcceptedAward);
        userExtraBuilder.SetLastResetAttemptAwardCountTime(ui.LastResetAttemptAwardCountTime.ToString());
        userExtraBuilder.SetGoldTollgateCount(ui.GoldCurAcceptedCount);
        userExtraBuilder.SetLastResetGoldTollgateCountTime(ui.LastResetGoldAwardCountTime.ToString());
        StringBuilder exchangeGoodList = new StringBuilder();
        StringBuilder exchangeGoodNumber = new StringBuilder();
        foreach (var good in ui.ExchangeGoodsInfo.GetAllGoodsData()) {
          int goodId = good.Key;
          StoreConfig sc = StoreConfigProvider.Instance.GetDataById(goodId);
          if (sc != null) {
            if (sc.m_HaveDayLimit) {
              exchangeGoodList.Append(goodId);
              exchangeGoodList.Append('|');
              exchangeGoodNumber.Append(good.Value);
              exchangeGoodNumber.Append('|');
            }
          }
        }
        userExtraBuilder.SetExchangeGoodList(exchangeGoodList.ToString());
        userExtraBuilder.SetExchangeGoodNumber(exchangeGoodNumber.ToString());
        userExtraBuilder.SetLastResetExchangeGoodTime(ui.LastResetExchangeGoodsTime.ToString());
        StringBuilder exchangeGoodRefreshCount = new StringBuilder();
        foreach (var currencyRefresh in ui.ExchangeGoodsInfo.CurrencyRefreshNum) {
          exchangeGoodRefreshCount.Append(currencyRefresh.Key);
          exchangeGoodRefreshCount.Append('|');
          exchangeGoodRefreshCount.Append(currencyRefresh.Value);
          exchangeGoodRefreshCount.Append('&');
        }
        userExtraBuilder.SetExchangeGoodRefreshCount(exchangeGoodRefreshCount.ToString());
        StringBuilder completeSceneList = new StringBuilder();
        StringBuilder completeSceneNumber = new StringBuilder();
        foreach (var scene in ui.ScenesCompletedCountData) {
          int sceneId = scene.Key;
          Data_SceneConfig sceneConfig = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
          if (sceneConfig.m_Type == 1 && sceneConfig.m_SubType == 1) {
            completeSceneList.Append(sceneId);
            completeSceneList.Append('|');
            completeSceneNumber.Append(scene.Value);
            completeSceneNumber.Append('|');
          }          
        }
        userExtraBuilder.SetCompleteSceneList(completeSceneList.ToString());
        userExtraBuilder.SetCompleteSceneNumber(completeSceneNumber.ToString());
        userExtraBuilder.SetLastResetSceneCountTime(ui.LastResetCompletedScenesCountTime.ToString());
        userExtraBuilder.SetVigor(ui.Vigor);
        userExtraBuilder.SetLastAddVigorTimestamp(ui.LastAddVigorTimestamp);
        userExtraBuilder.SetUsedStamina(ui.UsedStamina);
        userExtraBuilder.SetDayRestSignCount(ui.RestDailySignInCount);
        userExtraBuilder.SetLastResetDaySignCountTime(ui.LastResetSignInRewardDailyCountTime.ToString());
        userExtraBuilder.SetMonthSignCount(ui.SignInCountCurMonth);
        userExtraBuilder.SetLastResetMonthSignCountTime(ui.LastResetSignInRewardMonthCountTime.ToString());
        userExtraBuilder.SetMonthCardExpireTime(ui.MonthCardExpiredTime.ToString());
        StringBuilder gowHistoryTimeList = new StringBuilder();
        StringBuilder gowHistroyEloList = new StringBuilder();
        foreach (var timeElo in ui.GowInfo.HistoryGowElos) {
          gowHistoryTimeList.Append(timeElo.Key);
          gowHistoryTimeList.Append('|');
          gowHistroyEloList.Append(timeElo.Value);
          gowHistroyEloList.Append('|');          
        }
        userExtraBuilder.SetGowHistroyTimeList(gowHistoryTimeList.ToString());
        userExtraBuilder.SetGowHistroyEloList(gowHistroyEloList.ToString());
        userExtraBuilder.SetIsWeeklyLoginRewarded(ui.LoginRewardInfo.IsGetLoginReward);
        StringBuilder weeklyLoginRewardList = new StringBuilder();
        
        {   // Add lock   
            lock (ui.LoginRewardInfo.Lock)
            {
                foreach (var reward in ui.LoginRewardInfo.WeeklyLoginRewardRecord)
                {
                    weeklyLoginRewardList.Append(reward);
                    weeklyLoginRewardList.Append('|');
                }
            }
        }
        
        userExtraBuilder.SetWeeklyLoginRewardList(weeklyLoginRewardList.ToString());
        userExtraBuilder.SetLastResetWeeklyLoginRewardTime(ui.LastResetWeeklyLoginRewardTime.ToString());
        dataUserBuilder.SetUserExtra(userExtraBuilder.Build());
        //装备数据        
        DS_EquipInfo.Builder[] equipListBuilder = new DS_EquipInfo.Builder[EquipInfo.c_MaxEquipmentNum];
        for (int i = 0; i < EquipInfo.c_MaxEquipmentNum; ++i) {
          equipListBuilder[i] = DS_EquipInfo.CreateBuilder();
          string equipGuid = string.Format("{0}:{1}", userGuid, i);
          equipListBuilder[i].SetGuid(equipGuid);
          equipListBuilder[i].SetUserGuid(userGuid);
          equipListBuilder[i].SetIsValid(true);
          equipListBuilder[i].SetPosition(i);
          ItemInfo equipment = ui.Equip.GetEquipmentData(i);
          if (equipment != null) {
            equipListBuilder[i].SetItemId(equipment.ItemId);
            equipListBuilder[i].SetItemNum(equipment.ItemNum);
            equipListBuilder[i].SetLevel(equipment.Level);
            equipListBuilder[i].SetAppendProperty(equipment.AppendProperty);
          } else {
            equipListBuilder[i].SetItemId(0);
            equipListBuilder[i].SetItemNum(1);
            equipListBuilder[i].SetLevel(1);
            equipListBuilder[i].SetAppendProperty(0);
          }
          dataUserBuilder.EquipListList.Add(equipListBuilder[i].Build());
        }
        //物品数据
        lock (ui.ItemBag.Lock) {
          DS_ItemInfo.Builder[] itemListBuilder = new DS_ItemInfo.Builder[ui.ItemBag.ItemInfos.Count];
          for (int i = 0; i < ui.ItemBag.ItemInfos.Count; ++i) {
            ItemInfo item = ui.ItemBag.ItemInfos[i];
            if (item != null) {
              itemListBuilder[i] = DS_ItemInfo.CreateBuilder();
              string itemGuid = string.Format("{0}:{1}", userGuid, i);
              itemListBuilder[i].SetGuid(itemGuid);
              itemListBuilder[i].SetUserGuid(userGuid);
              itemListBuilder[i].SetIsValid(true);
              itemListBuilder[i].SetPosition(i);
              itemListBuilder[i].SetItemId(item.ItemId);
              itemListBuilder[i].SetItemNum(item.ItemNum);
              itemListBuilder[i].SetLevel(item.Level);
              itemListBuilder[i].SetAppendProperty(item.AppendProperty);
              dataUserBuilder.ItemListList.Add(itemListBuilder[i].Build());
            }
          }
        }
        //神器数据
        int legacyCount = LegacyInfo.c_MaxLegacyNum;
        DS_LegacyInfo.Builder[] legacyListBuilder = new DS_LegacyInfo.Builder[legacyCount];
        for (int i = 0; i < legacyCount; ++i) {
          legacyListBuilder[i] = DS_LegacyInfo.CreateBuilder();
          string legacyGuid = string.Format("{0}:{1}", userGuid, i);
          legacyListBuilder[i].SetGuid(legacyGuid);
          legacyListBuilder[i].SetUserGuid(userGuid);
          legacyListBuilder[i].SetIsValid(true);
          legacyListBuilder[i].SetPosition(i);
          ItemInfo legacy = ui.Legacy.GetLegacyData(i);
          if (legacy != null) {
            legacyListBuilder[i].SetLegacyId(legacy.ItemId);
            legacyListBuilder[i].SetLegacyLevel(legacy.Level);
            legacyListBuilder[i].SetAppendProperty(legacy.AppendProperty);
            legacyListBuilder[i].SetIsUnlock(legacy.IsUnlock);
          } else {
            legacyListBuilder[i].SetLegacyId(0);
            legacyListBuilder[i].SetLegacyLevel(1);
            legacyListBuilder[i].SetAppendProperty(0);
            legacyListBuilder[i].SetIsUnlock(false);
          }
          dataUserBuilder.LegacyListList.Add(legacyListBuilder[i].Build());
        }
        //XSoul数据
        var xsoulDict = ui.XSoul.GetAllXSoulPartData();
        DS_XSoulInfo.Builder[] xsoulListBuilder = new DS_XSoulInfo.Builder[xsoulDict.Count];
        int xsoulIndex = 0;
        foreach(var kv in xsoulDict){
          xsoulListBuilder[xsoulIndex] = DS_XSoulInfo.CreateBuilder();
          string xsoulGuid = string.Format("{0}:{1}", userGuid, xsoulIndex);
          xsoulListBuilder[xsoulIndex].SetGuid(xsoulGuid);
          xsoulListBuilder[xsoulIndex].SetUserGuid(userGuid);
          xsoulListBuilder[xsoulIndex].SetIsValid(true);
          xsoulListBuilder[xsoulIndex].SetPosition(xsoulIndex);
          xsoulListBuilder[xsoulIndex].SetXSoulType((int)kv.Key);
          xsoulListBuilder[xsoulIndex].SetXSoulId(kv.Value.ItemId);
          xsoulListBuilder[xsoulIndex].SetXSoulLevel(kv.Value.Level);
          xsoulListBuilder[xsoulIndex].SetXSoulExp(kv.Value.Experience);
          xsoulListBuilder[xsoulIndex].SetXSoulModelLevel(kv.Value.ShowModelLevel);
          dataUserBuilder.XSoulListList.Add(xsoulListBuilder[xsoulIndex].Build());
          xsoulIndex++;
        }       
        //技能数据
        int skillCount = ui.Skill.Skills.Count;
        DS_SkillInfo.Builder[] skillListBuilder = new DS_SkillInfo.Builder[skillCount];
        for (int i = 0; i < skillCount; ++i) {
          SkillDataInfo skill = ui.Skill.Skills[i];
          if (skill != null) {
            skillListBuilder[i] = DS_SkillInfo.CreateBuilder();
            string skillGuid = string.Format("{0}:{1}", userGuid, i);
            skillListBuilder[i].SetGuid(skillGuid);
            skillListBuilder[i].SetUserGuid(userGuid);
            skillListBuilder[i].SetIsValid(true);
            skillListBuilder[i].SetSkillId(skill.ID);
            skillListBuilder[i].SetLevel(skill.Level);
            skillListBuilder[i].SetPreset((int)skill.Postions.Presets[0]);
            dataUserBuilder.SkillListList.Add(skillListBuilder[i].Build());
          }
        }
        //任务数据
        int missionSum = ui.Mission.MissionList.Count;
        DS_MissionInfo.Builder[] missionListBuilder = new DS_MissionInfo.Builder[missionSum];
        int missionIndex = 0;
        foreach (MissionInfo mission in ui.Mission.MissionList.Values) {
          if (mission != null) {
            missionListBuilder[missionIndex] = DS_MissionInfo.CreateBuilder();
            string missionGuid = string.Format("{0}:{1}", userGuid, missionIndex);
            missionListBuilder[missionIndex].SetGuid(missionGuid);
            missionListBuilder[missionIndex].SetUserGuid(userGuid);
            missionListBuilder[missionIndex].SetIsValid(true);
            missionListBuilder[missionIndex].SetMissionId(mission.MissionId);
            missionListBuilder[missionIndex].SetMissionValue(mission.CurValue);
            missionListBuilder[missionIndex].SetMissionState((int)mission.State);           
            dataUserBuilder.MissionListList.Add(missionListBuilder[missionIndex].Build());
            missionIndex++;
          }
        }
        //通关记录
        int levelCount = ui.SceneData.Count;
        DS_LevelInfo.Builder[] levelListBuilder = new DS_LevelInfo.Builder[levelCount];
        int levelIndex = 0;
        foreach (var level in ui.SceneData) {
          levelListBuilder[levelIndex] = new DS_LevelInfo.Builder();
          string levelGuid = string.Format("{0}:{1}", userGuid, levelIndex);
          levelListBuilder[levelIndex].SetGuid(levelGuid);
          levelListBuilder[levelIndex].SetUserGuid(userGuid);
          levelListBuilder[levelIndex].SetIsValid(true);
          levelListBuilder[levelIndex].SetLevelId(level.Key);
          levelListBuilder[levelIndex].SetLevelRecord(level.Value);
          dataUserBuilder.LevelListList.Add(levelListBuilder[levelIndex].Build());
          levelIndex++;
        }
        //远征数据
        DS_ExpeditionInfo.Builder expeditionBuilder = DS_ExpeditionInfo.CreateBuilder();
        string expeditionGuid = string.Format("{0}:{1}", userGuid, 0);
        expeditionBuilder.SetGuid(expeditionGuid);
        expeditionBuilder.SetUserGuid(userGuid);
        expeditionBuilder.SetIsValid(true);
        expeditionBuilder.SetStartTime(ui.Expedition.LastResetTimestamp);
        expeditionBuilder.SetFightingScore(ui.Expedition.ResetScore);
        expeditionBuilder.SetHP(ui.Expedition.Hp);
        expeditionBuilder.SetMP(ui.Expedition.Mp);
        expeditionBuilder.SetRage(ui.Expedition.Rage);
        expeditionBuilder.SetSchedule(ui.Expedition.Schedule);
        expeditionBuilder.SetMonsterCount(ui.Expedition.CurWeakMonsterCount);
        expeditionBuilder.SetBossCount(ui.Expedition.CurBossCount);
        expeditionBuilder.SetOnePlayerCount(ui.Expedition.CurOnePlayerCount);
        StringBuilder unrewarded = new StringBuilder();
        for (int i = 0; i < ui.Expedition.Schedule; ++i) {
          var tollgate = ui.Expedition.Tollgates[i];
          if (tollgate != null) {
            if (tollgate.IsAcceptedAward == false) {
              unrewarded.Append(i);
              unrewarded.Append('|');
            }
          }
        }
        expeditionBuilder.SetUnrewarded(unrewarded.ToString());
        int nextTollgateIndex = 0;
        if (ui.Expedition.Schedule < ExpeditionPlayerInfo.c_MaxExpeditionNum) {
          nextTollgateIndex = ui.Expedition.Schedule;
        } else {
          //已经通关
          nextTollgateIndex = ui.Expedition.Schedule - 1;
        }
        Lobby.ExpeditionPlayerInfo.TollgateData nextTollgate = ui.Expedition.Tollgates[nextTollgateIndex];
        expeditionBuilder.SetTollgateType((int)nextTollgate.Type);
        expeditionBuilder.SetEnemyList("");
        expeditionBuilder.SetEnemyAttrList("");
        byte[] imageBytesA = new byte[0];
        byte[] imageBytesB = new byte[0];
        if (nextTollgate.Type == EnemyType.ET_Monster || nextTollgate.Type == EnemyType.ET_Boss) {
          StringBuilder enemylist = new StringBuilder();
          foreach (int monster in nextTollgate.EnemyList) {
            enemylist.Append(monster);
            enemylist.Append('|');
          }
          expeditionBuilder.SetEnemyList(enemylist.ToString());
          StringBuilder enemyAttrList = new StringBuilder();
          foreach (int enemyAttr in nextTollgate.EnemyAttrList) {
            enemyAttrList.Append(enemyAttr);
            enemyAttrList.Append('|');
          }
          expeditionBuilder.SetEnemyAttrList(enemyAttrList.ToString());
        } else {
          ExpeditionImageInfo imageA = nextTollgate.UserImageList[0];
          imageBytesA = SerializeImage(imageA);          
          if (nextTollgate.UserImageList.Count > 1) {
            ExpeditionImageInfo imageB = nextTollgate.UserImageList[1];
            imageBytesB = SerializeImage(imageB);            
          }
        }
        expeditionBuilder.SetImageA(ByteString.CopyFrom(imageBytesA));
        expeditionBuilder.SetImageB(ByteString.CopyFrom(imageBytesB));
        dataUserBuilder.SetUserExpedition(expeditionBuilder.Build());
        //邮件状态数据
        lock (ui.MailStateInfo.Lock) {
          int mailStateCount = ui.MailStateInfo.WholeMailStates.Count;
          DS_MailStateInfo.Builder[] mailStateListBuilder = new DS_MailStateInfo.Builder[mailStateCount];
          int mailStateIndex = 0;
          foreach (var mailState in ui.MailStateInfo.WholeMailStates.Values) {
            mailStateListBuilder[mailStateIndex] = new DS_MailStateInfo.Builder();
            string mailStateGuid = string.Format("{0}:{1}", userGuid, mailStateIndex);
            mailStateListBuilder[mailStateIndex].SetGuid(mailStateGuid);
            mailStateListBuilder[mailStateIndex].SetUserGuid(userGuid);
            mailStateListBuilder[mailStateIndex].SetIsValid(true);
            mailStateListBuilder[mailStateIndex].SetMailGuid((long)mailState.m_MailGuid);
            mailStateListBuilder[mailStateIndex].SetIsRead(mailState.m_AlreadyRead);
            mailStateListBuilder[mailStateIndex].SetIsReceived(mailState.m_AlreadyReceived);
            mailStateListBuilder[mailStateIndex].SetExpiryDate(mailState.m_ExpiryDate.ToString());
            dataUserBuilder.MailStateListList.Add(mailStateListBuilder[mailStateIndex].Build());
            mailStateIndex++;
          }
        }
        //伙伴数据
        int partnerCount = ui.PartnerStateInfo.GetAllPartners().Count;
        DS_PartnerInfo.Builder[] partnerListBuilder = new DS_PartnerInfo.Builder[partnerCount];
        int partnerIndex = 0;
        foreach (var partner in ui.PartnerStateInfo.GetAllPartners()) {
          partnerListBuilder[partnerIndex] = new DS_PartnerInfo.Builder();
          string partnerGuid = string.Format("{0}:{1}", userGuid, partnerIndex);
          partnerListBuilder[partnerIndex].SetGuid(partnerGuid);
          partnerListBuilder[partnerIndex].SetUserGuid(userGuid);
          partnerListBuilder[partnerIndex].SetIsValid(true);
          partnerListBuilder[partnerIndex].SetPartnerId(partner.Id);
          partnerListBuilder[partnerIndex].SetAdditionLevel(partner.CurAdditionLevel);
          partnerListBuilder[partnerIndex].SetSkillLevel(partner.CurSkillStage);
          dataUserBuilder.PartnerListList.Add(partnerListBuilder[partnerIndex].Build());
          partnerIndex++;
        }
        //好友数据
        int friendCount = ui.FriendInfos.Count;
        DS_FriendInfo.Builder[] friendListBuilder = new DS_FriendInfo.Builder[friendCount];
        int friendIndex = 0;
        foreach (var friend in ui.FriendInfos.Values) {
          friendListBuilder[friendIndex] = new DS_FriendInfo.Builder();
          string firendGuid = string.Format("{0}:{1}", userGuid, friendIndex);
          friendListBuilder[friendIndex].SetGuid(firendGuid);
          friendListBuilder[friendIndex].SetUserGuid(userGuid);
          friendListBuilder[friendIndex].SetIsValid(true);
          friendListBuilder[friendIndex].SetFriendGuid((long)friend.Guid);
          friendListBuilder[friendIndex].SetFriendNickname(friend.Nickname);
          friendListBuilder[friendIndex].SetHeroId(friend.HeroId);
          friendListBuilder[friendIndex].SetLevel(friend.Level);
          friendListBuilder[friendIndex].SetFightingScore(friend.FightingScore);
          dataUserBuilder.FriendListList.Add(friendListBuilder[friendIndex].Build());
          friendIndex++;
        }
        DSP_User dataUser = dataUserBuilder.Build();
        QueueAction(DSPSaveUserInternal, userGuid, dataUser, ui, saveCount);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR. Msg:DSP_User, Key:{0}, SaveCount:{1}, Error:{2},\nStacktrace:{3}", ui.Guid, saveCount, e.Message, e.StackTrace);
      }
    }
    internal void DSPSaveUserArena(ArenaInfo arenaInfo, List<ChallengeInfo> arenaRecordList)
    {
      try {
        if (arenaInfo != null) {
          long userGuid = (long)arenaInfo.GetId();
          DSP_UserArena.Builder dspUserArenaBuilder = DSP_UserArena.CreateBuilder();
          dspUserArenaBuilder.SetUserGuid(userGuid);
          DS_ArenaInfo.Builder dataArenaBuilder = DS_ArenaInfo.CreateBuilder();
          dataArenaBuilder.SetUserGuid(userGuid);
          dataArenaBuilder.SetIsValid(true);
          dataArenaBuilder.SetRank(arenaInfo.GetRank());
          dataArenaBuilder.SetIsRobot(arenaInfo.IsRobot);
          byte[] arenaBytes = new byte[0];
          arenaBytes = SerializeArena(arenaInfo);
          dataArenaBuilder.SetArenaBytes(ByteString.CopyFrom(arenaBytes));
          dataArenaBuilder.SetLastBattleTime(arenaInfo.LastBattleTime.ToString());
          dataArenaBuilder.SetLeftFightCount(arenaInfo.LeftFightCount);
          dataArenaBuilder.SetBuyFightCount(arenaInfo.FightCountBuyTime);
          dataArenaBuilder.SetLastResetFightCountTime(arenaInfo.FightCountResetTime.ToString());
          StringBuilder arenaHistoryTimeList = new StringBuilder();
          StringBuilder arenaHistroyRankList = new StringBuilder();
          foreach (var timeRank in arenaInfo.RankHistory) {
            arenaHistoryTimeList.Append(timeRank.Key);
            arenaHistoryTimeList.Append('|');
            arenaHistroyRankList.Append(timeRank.Value);
            arenaHistroyRankList.Append('|');
          }
          dataArenaBuilder.SetArenaHistroyTimeList(arenaHistoryTimeList.ToString());
          dataArenaBuilder.SetArenaHistroyRankList(arenaHistroyRankList.ToString());
          dspUserArenaBuilder.SetArenaBasic(dataArenaBuilder.Build());
          if (arenaRecordList != null) {
            int index = 0;
            foreach (var record in arenaRecordList) {
              DS_ArenaRecord.Builder dataRecordBuilder = DS_ArenaRecord.CreateBuilder();
              dataRecordBuilder.SetGuid(string.Format("{0}:{1}", userGuid, index));
              index++;
              dataRecordBuilder.SetIsValid(true);
              dataRecordBuilder.SetUserGuid(userGuid);
              dataRecordBuilder.SetRank(arenaInfo.GetRank());
              dataRecordBuilder.SetIsChallengerSuccess(record.IsChallengerSuccess);
              dataRecordBuilder.SetBeginTime(record.ChallengeBeginTime.ToString());
              dataRecordBuilder.SetEndTime(record.ChallengeEndTime.ToString());
              dataRecordBuilder.SetCGuid((long)record.Challenger.Guid);
              dataRecordBuilder.SetCHeroId(record.Challenger.HeroId);
              dataRecordBuilder.SetCLevel(record.Challenger.Level);
              dataRecordBuilder.SetCFightScore(record.Challenger.FightScore);
              dataRecordBuilder.SetCNickname(record.Challenger.NickName);
              dataRecordBuilder.SetCRank(record.Challenger.Rank);
              dataRecordBuilder.SetCUserDamage(record.Challenger.UserDamage);
              if (record.Challenger.PartnerDamage.Count > 0) {
                dataRecordBuilder.SetCPartnerId1(record.Challenger.PartnerDamage[0].OwnerId);
                dataRecordBuilder.SetCPartnerDamage1(record.Challenger.PartnerDamage[0].Damage);
              } else {
                dataRecordBuilder.SetCPartnerId1(0);
                dataRecordBuilder.SetCPartnerDamage1(0);
              }
              if (record.Challenger.PartnerDamage.Count > 1) {
                dataRecordBuilder.SetCPartnerId2(record.Challenger.PartnerDamage[1].OwnerId);
                dataRecordBuilder.SetCPartnerDamage2(record.Challenger.PartnerDamage[1].Damage);
              } else {
                dataRecordBuilder.SetCPartnerId2(0);
                dataRecordBuilder.SetCPartnerDamage2(0);
              }
              if (record.Challenger.PartnerDamage.Count > 2) {
                dataRecordBuilder.SetCPartnerId3(record.Challenger.PartnerDamage[2].OwnerId);
                dataRecordBuilder.SetCPartnerDamage3(record.Challenger.PartnerDamage[2].Damage);
              } else {
                dataRecordBuilder.SetCPartnerId3(0);
                dataRecordBuilder.SetCPartnerDamage3(0);
              }
              dataRecordBuilder.SetTGuid((long)record.Target.Guid);
              dataRecordBuilder.SetTHeroId(record.Target.HeroId);
              dataRecordBuilder.SetTLevel(record.Target.Level);
              dataRecordBuilder.SetTFightScore(record.Target.FightScore);
              dataRecordBuilder.SetTNickname(record.Target.NickName);
              dataRecordBuilder.SetTRank(record.Target.Rank);
              dataRecordBuilder.SetTUserDamage(record.Target.UserDamage);
              if (record.Target.PartnerDamage.Count > 0) {
                dataRecordBuilder.SetTPartnerId1(record.Target.PartnerDamage[0].OwnerId);
                dataRecordBuilder.SetTPartnerDamage1(record.Target.PartnerDamage[0].Damage);
              } else {
                dataRecordBuilder.SetTPartnerId1(0);
                dataRecordBuilder.SetTPartnerDamage1(0);
              }
              if (record.Target.PartnerDamage.Count > 1) {
                dataRecordBuilder.SetTPartnerId2(record.Target.PartnerDamage[1].OwnerId);
                dataRecordBuilder.SetTPartnerDamage2(record.Target.PartnerDamage[1].Damage);
              } else {
                dataRecordBuilder.SetTPartnerId2(0);
                dataRecordBuilder.SetTPartnerDamage2(0);
              }
              if (record.Target.PartnerDamage.Count > 2) {
                dataRecordBuilder.SetTPartnerId3(record.Target.PartnerDamage[2].OwnerId);
                dataRecordBuilder.SetTPartnerDamage3(record.Target.PartnerDamage[2].Damage);
              } else {
                dataRecordBuilder.SetTPartnerId3(0);
                dataRecordBuilder.SetTPartnerDamage3(0);
              }
              dspUserArenaBuilder.ArenaRecordListList.Add(dataRecordBuilder.Build());
            }
          }
          DSP_UserArena dspUserArena = dspUserArenaBuilder.Build();
          QueueAction(DSPSaveUserArenaInternal, userGuid, dspUserArena);
        }        
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR. Msg:DSP_UserArena, Key:{0}, Error:{2},\nStacktrace:{3}", arenaInfo.GetId(), e.Message, e.StackTrace);
      }
    }
    internal void DSGSaveGuid(List<GuidInfo> guidList, long saveCount)
    {
      try {
        DSG_Guid.Builder dsgGuidBuilder = DSG_Guid.CreateBuilder();
        uint dsgMsgId = MessageMapping.Query(typeof(DSG_Guid));
        dsgGuidBuilder.SetDataMsgId((int)dsgMsgId);
        foreach (var guidinfo in guidList) {
          DS_Guid.Builder dataGuidBuider = DS_Guid.CreateBuilder();
          dataGuidBuider.SetGuidType(guidinfo.GuidType);
          dataGuidBuider.SetIsValid(true);
          dataGuidBuider.SetGuidValue(guidinfo.NextGuid);
          DS_Guid dataUserGuid = dataGuidBuider.Build();
          dsgGuidBuilder.GuidListList.Add(dataUserGuid);
        }        
        DSG_Guid dsgGuid = dsgGuidBuilder.Build();
        QueueAction(DSGSaveGuidInternal, dsgMsgId, dsgGuid, saveCount);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    internal void DSGSaveMail(List<MailInfo> totalMails, long saveCount)
    {
      try {
        DSG_Mail.Builder dsgMailBuilder = DSG_Mail.CreateBuilder();
        uint dsgMsgId = MessageMapping.Query(typeof(DSG_Mail));
        dsgMailBuilder.SetDataMsgId((int)dsgMsgId);
        foreach (var mail in totalMails) {
          DS_MailInfo.Builder dataMailBuilder = DS_MailInfo.CreateBuilder();
          dataMailBuilder.SetGuid((long)mail.m_MailGuid);
          dataMailBuilder.SetIsValid(true);
          dataMailBuilder.SetModuleTypeId((int)mail.m_Module);
          dataMailBuilder.SetSender(mail.m_Sender);
          dataMailBuilder.SetReceiver((long)mail.m_Receiver);
          dataMailBuilder.SetSendDate(mail.m_SendTime.ToString());
          dataMailBuilder.SetExpiryDate(mail.m_ExpiryDate.ToString());
          dataMailBuilder.SetTitle(mail.m_Title);
          dataMailBuilder.SetText(mail.m_Text);
          dataMailBuilder.SetMoney(mail.m_Money);
          dataMailBuilder.SetGold(mail.m_Gold);
          dataMailBuilder.SetStamina(mail.m_Stamina);
          StringBuilder sbItemId = new StringBuilder();
          StringBuilder sbItemNum = new StringBuilder();
          foreach (var item in mail.m_Items) {
            sbItemId.Append(item.m_ItemId);
            sbItemNum.Append(item.m_ItemNum);
          }
          dataMailBuilder.SetItemIds(sbItemId.ToString());
          dataMailBuilder.SetItemNumbers(sbItemNum.ToString());
          dataMailBuilder.SetLevelDemand(mail.m_LevelDemand);
          dataMailBuilder.SetIsRead(mail.m_AlreadyRead);
          DS_MailInfo dataMail = dataMailBuilder.Build();
          dsgMailBuilder.MailListList.Add(dataMail);
        }
        DSG_Mail dsgMail = dsgMailBuilder.Build();
        QueueAction(DSGSaveMailInternal, dsgMsgId, dsgMail, saveCount);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    internal void DSGSaveGowStar(List<GowStarInfo> gowstarlist,long saveCount)
    {
      try {
        DSG_GowStar.Builder dsgGowstarBuilder = DSG_GowStar.CreateBuilder();
        uint dsgMsgId = MessageMapping.Query(typeof(DSG_GowStar));
        dsgGowstarBuilder.SetDataMsgId((int)dsgMsgId);
        int rank = 1;
        foreach (var gsi in gowstarlist) {
          DS_GowStar.Builder dataGowstarBuilder = DS_GowStar.CreateBuilder();
          dataGowstarBuilder.SetRank(rank);
          dataGowstarBuilder.SetIsValid(true);
          dataGowstarBuilder.SetUserGuid((long)gsi.m_Guid);
          dataGowstarBuilder.SetNickname(gsi.m_Nick);
          dataGowstarBuilder.SetHeroId(gsi.m_HeroId);
          dataGowstarBuilder.SetLevel(gsi.m_Level);
          dataGowstarBuilder.SetFightingScore(gsi.m_FightingScore);
          dataGowstarBuilder.SetGowElo(gsi.m_GowElo);
          DS_GowStar dataGowstar = dataGowstarBuilder.Build();
          dsgGowstarBuilder.GowStarListList.Add(dataGowstar);
          rank++;
        }
        DSG_GowStar dsgGowstar = dsgGowstarBuilder.Build();
        QueueAction(DSGSaveGowStarInternal, dsgMsgId, dsgGowstar, saveCount);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);        
      }
    }
    internal int DSGSaveArenaRank(List<ArenaInfo> arenaList, long saveCount)
    {
      try {
        List<DS_ArenaInfo> dataArenaList = new List<DS_ArenaInfo>();
        foreach (var arenaInfo in arenaList) {
          if (arenaInfo != null) {
            DS_ArenaInfo.Builder dataArenaBuilder = DS_ArenaInfo.CreateBuilder();
            dataArenaBuilder.SetUserGuid((long)arenaInfo.GetId());
            dataArenaBuilder.SetIsValid(true);
            dataArenaBuilder.SetRank(arenaInfo.GetRank());
            dataArenaBuilder.SetIsRobot(arenaInfo.IsRobot);
            byte[] arenaBytes = new byte[0];
            arenaBytes = SerializeArena(arenaInfo);
            dataArenaBuilder.SetArenaBytes(ByteString.CopyFrom(arenaBytes));
            dataArenaBuilder.SetLastBattleTime(arenaInfo.LastBattleTime.ToString());
            dataArenaBuilder.SetLeftFightCount(arenaInfo.LeftFightCount);
            dataArenaBuilder.SetBuyFightCount(arenaInfo.FightCountBuyTime);
            dataArenaBuilder.SetLastResetFightCountTime(arenaInfo.FightCountResetTime.ToString());
            StringBuilder arenaHistoryTimeList = new StringBuilder();
            StringBuilder arenaHistroyRankList = new StringBuilder();
            foreach (var timeRank in arenaInfo.RankHistory) {
              arenaHistoryTimeList.Append(timeRank.Key);
              arenaHistoryTimeList.Append('|');
              arenaHistroyRankList.Append(timeRank.Value);
              arenaHistroyRankList.Append('|');
            }
            dataArenaBuilder.SetArenaHistroyTimeList(arenaHistoryTimeList.ToString());
            dataArenaBuilder.SetArenaHistroyRankList(arenaHistroyRankList.ToString());
            dataArenaList.Add(dataArenaBuilder.Build());
          }
        }
        uint dsgMsgId = MessageMapping.Query(typeof(DSG_ArenaRank));
        List<List<ArenaInfo>> pieceList = new List<List<ArenaInfo>>();
        int pieceCapacity = 200;    //单个DS_ArenaArena平均大小约为300字节，64KB/300B=218，取近似值
        int pieceCount = 0;
        int mod = dataArenaList.Count % pieceCapacity;
        if (mod == 0) {
          pieceCount = (int)(dataArenaList.Count / pieceCapacity);
        } else {
          pieceCount = (int)(dataArenaList.Count / pieceCapacity) + 1;
        }        
        for (int i = 0; i < pieceCount; ++i) {
          DSG_ArenaRank.Builder dsgArenaRankBuilder = DSG_ArenaRank.CreateBuilder();
          dsgArenaRankBuilder.SetDataMsgId((int)dsgMsgId);         
          for (int j = i * pieceCapacity; j < (i + 1) * pieceCapacity && j < dataArenaList.Count; ++j) {
            dsgArenaRankBuilder.ArenaListList.Add(dataArenaList[j]);
          }
          if (dsgArenaRankBuilder.ArenaListCount > 0) {
            DSG_ArenaRank dsgArenaRank = dsgArenaRankBuilder.Build();
            QueueAction(DSGSaveArenaRankInternal, dsgMsgId, i, dsgArenaRank, saveCount);
            LogSys.Log(LOG_TYPE.INFO, "DSGSaveArenaRank piece. PieceNumber:{0}, SaveCount:{1}, DataSize:{2}, DataCount:{3}", 
              i, saveCount, dsgArenaRank.SerializedSize, dsgArenaRank.ArenaListCount);
          }          
        }
        LogSys.Log(LOG_TYPE.INFO, "DSGSaveArenaRank is divided into {0} pieces. SaveCount:{1}", pieceCount, saveCount);
        return pieceCount;
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
        return 0;
      }
    }
    internal int DSGSaveArenaRecord(Dictionary<ulong, List<ChallengeInfo>> arenaRecordDict, Dictionary<ulong, int> userRankDict, long saveCount)
    {
      try {
        List<DS_ArenaRecord> dataRecordList = new List<DS_ArenaRecord>();        
        foreach (var userRecords in arenaRecordDict) {
          long userGuid = (long)userRecords.Key;
          int rank = -1;
          userRankDict.TryGetValue(userRecords.Key, out rank);  
          int index = 0;
          foreach (var record in userRecords.Value) {
            DS_ArenaRecord.Builder dataRecordBuilder = DS_ArenaRecord.CreateBuilder();
            dataRecordBuilder.SetGuid(string.Format("{0}:{1}", userGuid, index));
            index++;
            dataRecordBuilder.SetIsValid(true);
            dataRecordBuilder.SetUserGuid(userGuid);
            dataRecordBuilder.SetRank(rank);
            dataRecordBuilder.SetIsChallengerSuccess(record.IsChallengerSuccess);
            dataRecordBuilder.SetBeginTime(record.ChallengeBeginTime.ToString());
            dataRecordBuilder.SetEndTime(record.ChallengeEndTime.ToString());
            dataRecordBuilder.SetCGuid((long)record.Challenger.Guid);
            dataRecordBuilder.SetCHeroId(record.Challenger.HeroId);
            dataRecordBuilder.SetCLevel(record.Challenger.Level);
            dataRecordBuilder.SetCFightScore(record.Challenger.FightScore);
            dataRecordBuilder.SetCNickname(record.Challenger.NickName);
            dataRecordBuilder.SetCRank(record.Challenger.Rank);
            dataRecordBuilder.SetCUserDamage(record.Challenger.UserDamage);
            if (record.Challenger.PartnerDamage.Count > 0) {
              dataRecordBuilder.SetCPartnerId1(record.Challenger.PartnerDamage[0].OwnerId);
              dataRecordBuilder.SetCPartnerDamage1(record.Challenger.PartnerDamage[0].Damage);
            } else {
              dataRecordBuilder.SetCPartnerId1(0);
              dataRecordBuilder.SetCPartnerDamage1(0);
            }
            if (record.Challenger.PartnerDamage.Count > 1) {
              dataRecordBuilder.SetCPartnerId2(record.Challenger.PartnerDamage[1].OwnerId);
              dataRecordBuilder.SetCPartnerDamage2(record.Challenger.PartnerDamage[1].Damage);
            } else {
              dataRecordBuilder.SetCPartnerId2(0);
              dataRecordBuilder.SetCPartnerDamage2(0);
            }
            if (record.Challenger.PartnerDamage.Count > 2) {
              dataRecordBuilder.SetCPartnerId3(record.Challenger.PartnerDamage[2].OwnerId);
              dataRecordBuilder.SetCPartnerDamage3(record.Challenger.PartnerDamage[2].Damage);
            } else {
              dataRecordBuilder.SetCPartnerId3(0);
              dataRecordBuilder.SetCPartnerDamage3(0);
            }
            dataRecordBuilder.SetTGuid((long)record.Target.Guid);
            dataRecordBuilder.SetTHeroId(record.Target.HeroId);
            dataRecordBuilder.SetTLevel(record.Target.Level);
            dataRecordBuilder.SetTFightScore(record.Target.FightScore);
            dataRecordBuilder.SetTNickname(record.Target.NickName);
            dataRecordBuilder.SetTRank(record.Target.Rank);
            dataRecordBuilder.SetTUserDamage(record.Target.UserDamage);
            if (record.Target.PartnerDamage.Count > 0) {
              dataRecordBuilder.SetTPartnerId1(record.Target.PartnerDamage[0].OwnerId);
              dataRecordBuilder.SetTPartnerDamage1(record.Target.PartnerDamage[0].Damage);
            } else {
              dataRecordBuilder.SetTPartnerId1(0);
              dataRecordBuilder.SetTPartnerDamage1(0);
            }
            if (record.Target.PartnerDamage.Count > 1) {
              dataRecordBuilder.SetTPartnerId2(record.Target.PartnerDamage[1].OwnerId);
              dataRecordBuilder.SetTPartnerDamage2(record.Target.PartnerDamage[1].Damage);
            } else {
              dataRecordBuilder.SetTPartnerId2(0);
              dataRecordBuilder.SetTPartnerDamage2(0);
            }
            if (record.Target.PartnerDamage.Count > 2) {
              dataRecordBuilder.SetTPartnerId3(record.Target.PartnerDamage[2].OwnerId);
              dataRecordBuilder.SetTPartnerDamage3(record.Target.PartnerDamage[2].Damage);
            } else {
              dataRecordBuilder.SetTPartnerId3(0);
              dataRecordBuilder.SetTPartnerDamage3(0);
            }
            dataRecordList.Add(dataRecordBuilder.Build());
          }
        }
        uint dsgMsgId = MessageMapping.Query(typeof(DSG_ArenaRecord));
        List<DSG_ArenaRecord> pieceList = new List<DSG_ArenaRecord>();
        int pieceCapacity = 350;    //单个DS_ArenaRecord平均大小约为165字节，64KB/165B=397，取近似值
        int pieceCount = 0;
        int mod = dataRecordList.Count % pieceCapacity;
        if (mod == 0) {
          pieceCount = (int)(dataRecordList.Count / pieceCapacity);
        } else {
          pieceCount = (int)(dataRecordList.Count / pieceCapacity) + 1;
        }       
        for (int i = 0; i < pieceCount; ++i) {
          DSG_ArenaRecord.Builder dsgArenaRecordBuilder = DSG_ArenaRecord.CreateBuilder();          
          dsgArenaRecordBuilder.SetDataMsgId((int)dsgMsgId);
          for (int j = i * pieceCapacity; j < (i + 1) * pieceCapacity && j < dataRecordList.Count; ++j) {
            dsgArenaRecordBuilder.RecordListList.Add(dataRecordList[j]);
          }
          if (dsgArenaRecordBuilder.RecordListCount > 0) {
            DSG_ArenaRecord dsgArenaRecord = dsgArenaRecordBuilder.Build();
            QueueAction(DSGSaveArenaRecordInternal, dsgMsgId, i, dsgArenaRecord, saveCount);
            LogSys.Log(LOG_TYPE.INFO, "DSGSaveArenaRecord piece. PieceNumber:{0}, SaveCount:{1}, DataSize:{2}, DataCount:{3}", 
              i, saveCount, dsgArenaRecord.SerializedSize, dsgArenaRecord.RecordListCount);
          }          
        }
        LogSys.Log(LOG_TYPE.INFO, "DSGSaveArenaRecord is divided into {0} pieces. SaveCount:{1}", pieceCount, saveCount);
        return pieceCount;
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
        return 0;
      }
    }
    internal void DSSaveGiftCode(GiftCodeInfo giftcodeInfo, bool isUsed)
    {
      try {
        DS_GiftCode.Builder dataGiftCodeBuilder = DS_GiftCode.CreateBuilder();
        dataGiftCodeBuilder.SetGiftCode(giftcodeInfo.GiftCode);
        dataGiftCodeBuilder.SetIsValid(true);
        dataGiftCodeBuilder.SetGiftId(giftcodeInfo.GiftId);
        dataGiftCodeBuilder.SetIsUsed(isUsed);
        dataGiftCodeBuilder.SetUserGuid((long)giftcodeInfo.UserGuid);
        DS_GiftCode dataGiftCode = dataGiftCodeBuilder.Build();
        QueueAction(DSSaveInternal, giftcodeInfo.GiftCode, dataGiftCode);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    internal void DSDSaveBanAccount(string accountId, bool isBan)
    {
      try {
        DSD_BanAccount.Builder dataBanAccountBuilder = DSD_BanAccount.CreateBuilder();
        dataBanAccountBuilder.SetAccount(accountId);
        dataBanAccountBuilder.SetIsBanned(isBan);
        DSD_BanAccount dataBanAccount = dataBanAccountBuilder.Build();
        QueueAction(DSSaveInternal, accountId, dataBanAccount);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    //--------------------------------------------------------------------------------------------------------------------------
    //供内部通过QueueAction调用的方法，实际执行线程是DataStoreThread。
    //--------------------------------------------------------------------------------------------------------------------------
    private void DSSaveInternal(string key, IMessage data)
    {
      try {
        m_DataStoreClient.Save(key, data, (ret, error) =>
        {
          if (ret == DSSaveResult.Success) {
            LogSys.Log(LOG_TYPE.INFO, "Save data success. dataType:{0}, key:{1}", data.GetType().Name, key);
          } else {
            LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "Save data ERROR. dataType:{0}, key:{1}, ErrorType:{2}, ErrorMsg:{3}",
             data.GetType().Name, key, ret, error);
          }
        });
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    private void DSPSaveCreateAccountInternal(string accountId, DSP_CreateAccount dataNewAccount)
    {
      try {
        m_DataStoreClient.Save(accountId, dataNewAccount, (ret, error) =>
        {
          if (ret == DSSaveResult.Success) {
            LogSys.Log(LOG_TYPE.INFO, "Save data success. dataType:({0}) key:({1})", dataNewAccount.GetType().Name, accountId);
          } else {
            LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "Save data ERROR. dataType:({0}) key:({1}) ErrorType:({2}) ErrorMsg:({3})",
             dataNewAccount.GetType().Name, accountId, ret, error);
          }
        });
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    private void DSPSaveCreateUserInternal(string accountId, DSP_CreateUser dataNewUser)
    {
      try {
        m_DataStoreClient.Save(accountId, dataNewUser, (ret, error) =>
        {
          if (ret == DSSaveResult.Success) {
            LogSys.Log(LOG_TYPE.INFO, "Save data success. dataType:({0}) key:({1})", dataNewUser.GetType().Name, accountId);
          } else {
            LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "Save data ERROR. dataType:({0}) key:({1}) ErrorType:({2}) ErrorMsg:({3})",
             dataNewUser.GetType().Name, accountId, ret, error);
          }
        });
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    private void DSPSaveUserInternal(long userGuid, DSP_User dataUser, UserInfo ui, long saveCount)
    {
      try {
        //玩家角色数据
        m_DataStoreClient.Save(userGuid.ToString(), dataUser, (ret, error) =>
        {
          if (ret == DSSaveResult.Success) {
            ui.CurrentUserSaveCount = saveCount;
            LogSys.Log(LOG_TYPE.INFO, "Save User data success. UserGuid:{0}, SaveCount:{1}", userGuid, saveCount);
          } else {            
            LogSys.Log(LOG_TYPE.ERROR, "Save User data ERROR. UserGuid:{0}, SaveCount:{1} ErrorType:({2}) ErrorMsg:({3})",
              userGuid, saveCount, ret, error);
          }
        }, saveCount != 0);
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    private void DSPSaveUserArenaInternal(long userGuid, DSP_UserArena dataUserArena)
    {
      try {       
        m_DataStoreClient.Save(userGuid.ToString(), dataUserArena, (ret, error) =>
        {
          if (ret == DSSaveResult.Success) {
            //TODO:handle saveCount
            LogSys.Log(LOG_TYPE.INFO, "Save UserArena data success. UserGuid:{0}", userGuid);
          } else {
            LogSys.Log(LOG_TYPE.ERROR, "Save UserArena data ERROR. UserGuid:{0} ErrorType:{1} ErrorMsg:{2}", userGuid, ret, error);
          }
        });
      } catch (Exception e) {
        LogSys.Log(LOG_TYPE.ERROR, "DataStore Save ERROR:{0}, Stacktrace:{1}", e.Message, e.StackTrace);
      }
    }
    private void DSGSaveGuidInternal(uint dsgMsgId, DSG_Guid dsgGuid, long saveCount)
    {
      m_DataStoreClient.Save(dsgMsgId.ToString(), dsgGuid, (ret, error) =>
      {
        if (ret == DSSaveResult.Success) {
          GlobalDataProcessThread global_process = LobbyServer.Instance.GlobalDataProcessThread;
          global_process.QueueAction(global_process.SetCurrentGuidSaveCount, saveCount);
          LogSys.Log(LOG_TYPE.INFO, "Save global data success. Type:DSG_Guid, SaveCount:{0}, ListCount:{1}", saveCount, dsgGuid.GuidListCount);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, "Save global data failed. Type:DSG_Guid, SaveCount:{0}, ErrorType:{1}, ErrorMsg:{2}", saveCount, ret, error);
        }
      });
    }
    private void DSGSaveMailInternal(uint dsgMsgId, DSG_Mail dsgMail, long saveCount)
    {
      m_DataStoreClient.Save(dsgMsgId.ToString(), dsgMail, (ret, error) =>
      {
        if (ret == DSSaveResult.Success) {
          GlobalDataProcessThread global_process = LobbyServer.Instance.GlobalDataProcessThread;
          global_process.QueueAction(global_process.SetCurrentMailSaveCount, saveCount);
          LogSys.Log(LOG_TYPE.INFO, "Save global data success. Type:MailSystem, SaveCount:{0}, ListCount:{1}", saveCount, dsgMail.MailListCount);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, "Save global data Failed. Type:MailSystem, SaveCount:{0}, ErrorType:{1}, ErrorMsg:{2}", saveCount, ret, error);
        }
      });
    }
    private void DSGSaveGowStarInternal(uint dsgMsgId, DSG_GowStar dsgGowstar, long saveCount)
    {
      m_DataStoreClient.Save(dsgMsgId.ToString(), dsgGowstar, (ret, error) =>
      {
        if (ret == DSSaveResult.Success) {
          GlobalDataProcessThread global_process = LobbyServer.Instance.GlobalDataProcessThread;
          global_process.QueueAction(global_process.SetCurrentGowstarSaveCount, saveCount);          
          LogSys.Log(LOG_TYPE.INFO, "Save global data success. Type:GowStar, SaveCount:{0}, ListCount:{1}", saveCount, dsgGowstar.GowStarListCount);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, "Save global data Failed. Type:GowStar, SaveCount:{0}, ErrorType:{1}, ErrorMsg:{2}", saveCount, ret, error);
        }
      });
    }
    private void DSGSaveArenaRankInternal(uint dsgMsgId, int pieceNumber, DSG_ArenaRank dsgArenaRank, long saveCount)
    {
      string key = string.Format("{0}+{1}", dsgMsgId, pieceNumber);
      m_DataStoreClient.Save(key, dsgArenaRank, (ret, error) =>
      {
        if (ret == DSSaveResult.Success) {
          GlobalDataProcessThread global_process = LobbyServer.Instance.GlobalDataProcessThread;
          global_process.QueueAction(global_process.SetCurrentArenaRankSaveCount, saveCount, pieceNumber);
          LogSys.Log(LOG_TYPE.INFO, "Save global data success. Type:ArenaRank, SaveCount:{0}, ListCount:{1}", saveCount, dsgArenaRank.ArenaListCount);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, "Save global data Failed. Type:ArenaRank, SaveCount:{0}, ErrorType:{1}, ErrorMsg:{2}", saveCount, ret, error);
        }
      });
    }
    private void DSGSaveArenaRecordInternal(uint dsgMsgId, int pieceNumber, DSG_ArenaRecord dsgArenaRecord, long saveCount)
    {
      string key = string.Format("{0}+{1}", dsgMsgId, pieceNumber);
      m_DataStoreClient.Save(key, dsgArenaRecord, (ret, error) =>
      {
        if (ret == DSSaveResult.Success) {
          GlobalDataProcessThread global_process = LobbyServer.Instance.GlobalDataProcessThread;
          global_process.QueueAction(global_process.SetCurrentArenaRecordSaveCount, saveCount, pieceNumber);
          LogSys.Log(LOG_TYPE.INFO, "Save global data success. Type:ArenaRecord, SaveCount:{0}, ListCount:{1}", saveCount, dsgArenaRecord.RecordListCount);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, "Save global data Failed. Type:ArenaRecord, SaveCount:{0}, ErrorType:{1}, ErrorMsg:{2}", saveCount, ret, error);
        }
      });
    }
    //--------------------------------------------------------------------------------------------------------------------------
    //供外部通过QueueAction调用的方法，实际执行线程是DataStoreThread。
    //--------------------------------------------------------------------------------------------------------------------------
    internal void DSPLoadAccount(string accountId, DSPLoadAccountCB cb)
    {
      string key = accountId;
      m_DataStoreClient.Load<DSP_Account>(accountId, (ret, error, data) =>
      {
        if (ret == DSLoadResult.Success) {
          LogSys.Log(LOG_TYPE.INFO, "DataStore Load Success: Msg:{0}, Key:{1}", "DSP_Account", key);
        } else if (ret == DSLoadResult.NotFound) {
          LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Yellow, "DataStore Load NotFound: Msg:{0}, Key:{1}", "DSP_Account", key);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "DataStore Load Failed: Msg:{0}, Key:{1}, ERROR:{2}", "DSP_Account", key, error);
        }
        cb(ret, data);
      });
    }
    internal void DSPLoadUser(ulong userGuid, DSPLoadUserCB cb)
    {
      string key = userGuid.ToString();
      m_DataStoreClient.Load<DSP_User>(key, (ret, error, data) =>
      {
        if (ret == DSLoadResult.Success) {
          LogSys.Log(LOG_TYPE.INFO, "DataStore Load Success: Msg:{0}, Key:{1}", "DSP_User", key);          
        } else if (ret == DSLoadResult.NotFound) {
          LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Yellow, "DataStore Load NotFound: Msg:{0}, Key:{1}", "DSP_User", key);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "DataStore Load Failed: Msg:{0}, Key:{1}, ERROR:{2}", "DSP_User", key, error);
        }
        cb(ret, data);
      });      
    }
    internal void DSPLoadUserArena(ulong userGuid, DSPLoadUserArenaCB cb)
    {
      string key = userGuid.ToString();
      m_DataStoreClient.Load<DSP_UserArena>(key, (ret, error, dataUserArena) =>
      {
        ArenaInfo arenaInfo = null;
        List<ChallengeInfo> arenaRecordList = null;
        if (ret == DSLoadResult.Success) {
          arenaInfo = DeserializeArena(dataUserArena.ArenaBasic);
          arenaRecordList = new List<ChallengeInfo>();
          foreach (var dataRecord in dataUserArena.ArenaRecordListList) {
            Tuple<ulong, ChallengeInfo> record = CreateArenaRecord(dataRecord);
            arenaRecordList.Add(record.Item2);
          }          
          LogSys.Log(LOG_TYPE.INFO, "DataStore Load Success: Msg:{0}, Key:{1}", "DSP_UserArena", key);
        } else if (ret == DSLoadResult.NotFound) {
          LogSys.Log(LOG_TYPE.INFO, ConsoleColor.Yellow, "DataStore Load NotFound: Msg:{0}, Key:{1}", "DSP_UserArena", key);
        } else {
          LogSys.Log(LOG_TYPE.ERROR, ConsoleColor.Red, "DataStore Load Failed: Msg:{0}, Key:{1}, ERROR:{2}", "DSP_UserArena", key, error);
        }
        cb(ret, arenaInfo, arenaRecordList);
      });
    }   
    //辅助方法
    private byte[] SerializeImage(ExpeditionImageInfo image)
    {
      DSA_ExpeditionImage.Builder dataImageBuilder = DSA_ExpeditionImage.CreateBuilder();
      dataImageBuilder.SetGuid(image.Guid);
      dataImageBuilder.SetHeroId(image.HeroId);
      dataImageBuilder.SetLevel(image.Level);
      dataImageBuilder.SetNickname(image.Nickname);
      dataImageBuilder.SetFightingScore(image.FightingScore);
      foreach (var equip in image.Equips.Armor) {
        if (equip != null) {
          DSA_Item.Builder dataItemBuilder = DSA_Item.CreateBuilder();
          dataItemBuilder.SetItemId(equip.ItemId);
          dataItemBuilder.SetLevel(equip.Level);
          dataItemBuilder.SetNumber(equip.ItemNum);
          dataItemBuilder.SetAppendProperty(equip.AppendProperty);
          dataImageBuilder.EquipListList.Add(dataItemBuilder.Build());
        }
      }
      foreach (var legacy in image.Legacys.SevenArcs) {
        if (legacy != null) {
          DSA_Legacy.Builder dataLegacyBuilder = DSA_Legacy.CreateBuilder();
          dataLegacyBuilder.SetItemId(legacy.ItemId);
          dataLegacyBuilder.SetLevel(legacy.Level);
          dataLegacyBuilder.SetIsUnlock(legacy.IsUnlock);
          dataLegacyBuilder.SetAppendProperty(legacy.AppendProperty);
          dataImageBuilder.LegacyListList.Add(dataLegacyBuilder.Build());
        }
      }
      foreach (var skill in image.Skills.Skills) {
        if (skill != null) {
          DSA_Skill.Builder dataSkillBuilder = DSA_Skill.CreateBuilder();
          dataSkillBuilder.SetID(skill.ID);
          dataSkillBuilder.SetLevel(skill.Level);
          dataSkillBuilder.SetPostions((int)skill.Postions.Presets[0]);
          dataImageBuilder.SkillListList.Add(dataSkillBuilder.Build());
        }
      }
      return dataImageBuilder.Build().ToByteArray();
    }
    private byte[] SerializeArena(ArenaInfo arenaInfo)
    {
      DSA_ArenaInfo.Builder dsaArenaBuilder = DSA_ArenaInfo.CreateBuilder();
      dsaArenaBuilder.SetGuid(arenaInfo.GetId());
      dsaArenaBuilder.SetHeroId(arenaInfo.HeroId);
      dsaArenaBuilder.SetNickname(arenaInfo.NickName);
      dsaArenaBuilder.SetLevel(arenaInfo.Level);
      dsaArenaBuilder.SetFightingScore(arenaInfo.FightScore);     
      foreach (var equip in arenaInfo.EquipInfo) {
        if (equip != null) {
          DSA_Item.Builder dsaItemBuilder = DSA_Item.CreateBuilder();
          dsaItemBuilder.SetItemId(equip.ItemId);
          dsaItemBuilder.SetLevel(equip.Level);
          dsaItemBuilder.SetNumber(equip.ItemNum);
          dsaItemBuilder.SetAppendProperty(equip.AppendProperty);
          dsaArenaBuilder.EquipListList.Add(dsaItemBuilder.Build());
        }
      }
      foreach (var skill in arenaInfo.SkillDataInfo) {
        if (skill != null) {
          DSA_Skill.Builder dsaSkillBuilder = DSA_Skill.CreateBuilder();
          dsaSkillBuilder.SetID(skill.ID);
          dsaSkillBuilder.SetLevel(skill.Level);
          dsaSkillBuilder.SetPostions((int)skill.Postions.Presets[0]);
          dsaArenaBuilder.SkillListList.Add(dsaSkillBuilder.Build());
        }
      }     
      foreach (var legacy in arenaInfo.LegacyInfo) {
        if (legacy != null) {
          DSA_Legacy.Builder dsaLegacyBuilder = DSA_Legacy.CreateBuilder();
          dsaLegacyBuilder.SetItemId(legacy.ItemId);
          dsaLegacyBuilder.SetLevel(legacy.Level);
          dsaLegacyBuilder.SetAppendProperty(legacy.AppendProperty);
          dsaLegacyBuilder.SetIsUnlock(legacy.IsUnlocked);
          dsaArenaBuilder.LegacyListList.Add(dsaLegacyBuilder.Build());
        }
      }
      foreach (var xsoul in arenaInfo.XSoulInfo) {
        if (xsoul != null) {
          DSA_XSoul.Builder dsaXSoulBuilder = DSA_XSoul.CreateBuilder();
          dsaXSoulBuilder.SetItemId(xsoul.ItemId);
          dsaXSoulBuilder.SetLevel(xsoul.Level);
          dsaXSoulBuilder.SetExperience(xsoul.Experience);
          dsaXSoulBuilder.SetModelLevel(xsoul.ModelLevel);
          dsaArenaBuilder.XSoulListList.Add(dsaXSoulBuilder.Build());
        }
      }
      foreach (var partner in arenaInfo.FightPartners) {
        if (partner != null) {
          DSA_Partner.Builder dsaPartnerBuilder = DSA_Partner.CreateBuilder();
          dsaPartnerBuilder.SetPartnerId(partner.Id);
          dsaPartnerBuilder.SetAdditionLevel(partner.CurAdditionLevel);
          dsaPartnerBuilder.SetSkillLevel(partner.CurSkillStage);
          dsaArenaBuilder.PartnerListList.Add(dsaPartnerBuilder.Build());
        }
      }
      if (arenaInfo.ActivePartner != null) {
        DSA_Partner.Builder activePartnerBuilder = DSA_Partner.CreateBuilder();
        activePartnerBuilder.SetPartnerId(arenaInfo.ActivePartner.Id);
        activePartnerBuilder.SetAdditionLevel(arenaInfo.ActivePartner.CurAdditionLevel);
        activePartnerBuilder.SetSkillLevel(arenaInfo.ActivePartner.CurSkillStage);
        dsaArenaBuilder.SetActivePartner(activePartnerBuilder.Build());
      }      
      return dsaArenaBuilder.Build().ToByteArray();
    }
    private ArenaInfo DeserializeArena(DS_ArenaInfo dataArena)
    {
      ArenaInfo arenaInfo = new ArenaInfo();
      arenaInfo.SetRank(dataArena.Rank);
      arenaInfo.SetId((ulong)dataArena.UserGuid);
      arenaInfo.IsRobot = dataArena.IsRobot;
      arenaInfo.LastBattleTime = DateTime.Parse(dataArena.LastBattleTime);
      arenaInfo.LeftFightCount = dataArena.LeftFightCount;
      arenaInfo.FightCountBuyTime = dataArena.BuyFightCount;
      arenaInfo.FightCountResetTime = DateTime.Parse(dataArena.LastResetFightCountTime);
      arenaInfo.RankHistory.Clear();
      List<DateTime> arenaTimeList = new List<DateTime>();
      string[] arenaTimeArray = dataArena.ArenaHistroyTimeList.Split(new char[] { '|' });
      foreach (string str in arenaTimeArray) {
        if (str.Trim() != string.Empty) {
          DateTime time;
          if (DateTime.TryParse(str, out time)) {
            arenaTimeList.Add(time);
          }
        }
      }
      List<int> arenaRankList = new List<int>();
      string[] arenaRankArray = dataArena.ArenaHistroyRankList.Split(new char[] { '|' });
      foreach (string str in arenaRankArray) {
        if (str.Trim() != string.Empty) {
          int rank = 0;
          if (int.TryParse(str, out rank)) {
            arenaRankList.Add(rank);
          }
        }
      }
      if (arenaTimeList.Count == arenaRankList.Count) {       
        for (int i = 0; i < arenaTimeList.Count; ++i) {
          arenaInfo.RankHistory.Add(arenaTimeList[i], arenaRankList[i]);
        }
      }
      DSA_ArenaInfo dsaArena = DSA_ArenaInfo.ParseFrom(dataArena.ArenaBytes.ToByteArray());
      arenaInfo.HeroId = dsaArena.HeroId;
      arenaInfo.NickName = dsaArena.Nickname;
      arenaInfo.Level = dsaArena.Level;
      arenaInfo.FightScore = dsaArena.FightingScore;
      foreach (var dsaEquip in dsaArena.EquipListList) {
        ItemInfo equip = new ItemInfo();
        equip.ItemId = dsaEquip.ItemId;
        equip.Level = dsaEquip.Level;
        equip.ItemNum = dsaEquip.Number;
        equip.AppendProperty = dsaEquip.AppendProperty;
        arenaInfo.EquipInfo.Add(equip);
      }
      foreach (var dsaSkill in dsaArena.SkillListList) {
        SkillDataInfo skill = new SkillDataInfo();
        skill.ID = dsaSkill.ID;
        skill.Level = dsaSkill.Level;
        skill.Postions.Presets[0] = (SlotPosition)dsaSkill.Postions;
        arenaInfo.SkillDataInfo.Add(skill);
      }      
      foreach (var dsaLegacy in dsaArena.LegacyListList) {
        ArenaItemInfo legacy = new ArenaItemInfo();
        legacy.ItemId = dsaLegacy.ItemId;
        legacy.Level = dsaLegacy.Level;
        legacy.AppendProperty = dsaLegacy.AppendProperty;
        legacy.IsUnlocked = dsaLegacy.IsUnlock;
        arenaInfo.LegacyInfo.Add(legacy);
      }
      foreach (var dsaXSoul in dsaArena.XSoulListList) {
        ArenaXSoulInfo xsoul = new ArenaXSoulInfo();
        xsoul.ItemId = dsaXSoul.ItemId;
        xsoul.Level = dsaXSoul.Level;
        xsoul.Experience = dsaXSoul.Experience;
        xsoul.ModelLevel = dsaXSoul.ModelLevel;
        arenaInfo.XSoulInfo.Add(xsoul);
      }
      foreach (var dsaPartner in dsaArena.PartnerListList) {
        PartnerConfig config = PartnerConfigProvider.Instance.GetDataById(dsaPartner.PartnerId);
        if (null != config) {
          PartnerInfo partner = new PartnerInfo(config);
          partner.CurSkillStage = dsaPartner.SkillLevel;
          partner.CurAdditionLevel = dsaPartner.AdditionLevel;
          arenaInfo.FightPartners.Add(partner);
        }
      }
      if (dsaArena.ActivePartner != null) {
        PartnerConfig partnerConfig = PartnerConfigProvider.Instance.GetDataById(dsaArena.ActivePartner.PartnerId);
        if (null != partnerConfig) {
          arenaInfo.ActivePartner = new PartnerInfo(partnerConfig);
          arenaInfo.ActivePartner.CurSkillStage = dsaArena.ActivePartner.SkillLevel;
          arenaInfo.ActivePartner.CurAdditionLevel = dsaArena.ActivePartner.AdditionLevel;
        }
      }      
      return arenaInfo;
    }
    Tuple<ulong, ChallengeInfo> CreateArenaRecord(DS_ArenaRecord dataRecord)
    {
      ulong userGuid = (ulong)dataRecord.UserGuid;
      ChallengeInfo challengerInfo = new ChallengeInfo();
      challengerInfo.IsChallengerSuccess = dataRecord.IsChallengerSuccess;
      challengerInfo.ChallengeBeginTime = DateTime.Parse(dataRecord.BeginTime);
      challengerInfo.ChallengeEndTime = DateTime.Parse(dataRecord.EndTime);
      challengerInfo.IsDone = true;
      challengerInfo.Challenger = new ChallengeEntityInfo();
      challengerInfo.Challenger.Guid = (ulong)dataRecord.CGuid;
      challengerInfo.Challenger.HeroId = dataRecord.CHeroId;
      challengerInfo.Challenger.Level = dataRecord.CLevel;
      challengerInfo.Challenger.FightScore = dataRecord.CFightScore;
      challengerInfo.Challenger.NickName = dataRecord.CNickname;
      challengerInfo.Challenger.Rank = dataRecord.CRank;
      challengerInfo.Challenger.UserDamage = dataRecord.CUserDamage;
      if (dataRecord.CPartnerId1 > 0) {
        DamageInfo damangeInfo = new DamageInfo();
        damangeInfo.OwnerId = dataRecord.CPartnerId1;
        damangeInfo.Damage = dataRecord.CPartnerDamage1;
        challengerInfo.Challenger.PartnerDamage.Add(damangeInfo);
      }
      if (dataRecord.CPartnerId2 > 0) {
        DamageInfo damangeInfo = new DamageInfo();
        damangeInfo.OwnerId = dataRecord.CPartnerId2;
        damangeInfo.Damage = dataRecord.CPartnerDamage2;
        challengerInfo.Challenger.PartnerDamage.Add(damangeInfo);
      }
      if (dataRecord.CPartnerId3 > 0) {
        DamageInfo damangeInfo = new DamageInfo();
        damangeInfo.OwnerId = dataRecord.CPartnerId3;
        damangeInfo.Damage = dataRecord.CPartnerDamage3;
        challengerInfo.Challenger.PartnerDamage.Add(damangeInfo);
      }
      challengerInfo.Target = new ChallengeEntityInfo();
      challengerInfo.Target.Guid = (ulong)dataRecord.TGuid;
      challengerInfo.Target.HeroId = dataRecord.THeroId;
      challengerInfo.Target.Level = dataRecord.TLevel;
      challengerInfo.Target.FightScore = dataRecord.TFightScore;
      challengerInfo.Target.NickName = dataRecord.TNickname;
      challengerInfo.Target.Rank = dataRecord.TRank;
      challengerInfo.Target.UserDamage = dataRecord.TUserDamage;
      if (dataRecord.TPartnerId1 > 0) {
        DamageInfo damangeInfo = new DamageInfo();
        damangeInfo.OwnerId = dataRecord.TPartnerId1;
        damangeInfo.Damage = dataRecord.TPartnerDamage1;
        challengerInfo.Challenger.PartnerDamage.Add(damangeInfo);
      }
      if (dataRecord.TPartnerId2 > 0) {
        DamageInfo damangeInfo = new DamageInfo();
        damangeInfo.OwnerId = dataRecord.TPartnerId2;
        damangeInfo.Damage = dataRecord.TPartnerDamage2;
        challengerInfo.Challenger.PartnerDamage.Add(damangeInfo);
      }
      if (dataRecord.TPartnerId3 > 0) {
        DamageInfo damangeInfo = new DamageInfo();
        damangeInfo.OwnerId = dataRecord.TPartnerId3;
        damangeInfo.Damage = dataRecord.TPartnerDamage3;
        challengerInfo.Challenger.PartnerDamage.Add(damangeInfo);
      }
      Tuple<ulong, ChallengeInfo> arenaRecord = new Tuple<ulong, ChallengeInfo>(userGuid, challengerInfo);
      return arenaRecord;
    }

    private bool m_DataStoreAvailable = false;
    private DataStoreClient m_DataStoreClient = null;
    private DataInitStatus m_GuidInitStatus = DataInitStatus.Unload;
    private DataInitStatus m_ActivationInitStatus = DataInitStatus.Unload;
    private DataInitStatus m_NicknameInitStatus = DataInitStatus.Unload;
    private DataInitStatus m_GowStarInitStatus = DataInitStatus.Unload;
    private DataInitStatus m_MailInitStatus = DataInitStatus.Unload;
    private DataInitStatus m_GiftInitStatus = DataInitStatus.Unload;
    private DataInitStatus m_ArenaRankInitStatus = DataInitStatus.Unload;
    private DataInitStatus m_ArenaRecordInitStatus = DataInitStatus.Unload;
    private long m_LastTimeoutTickTime = 0;
    private const long c_TimeoutTickInterval = 5000;
    private long m_LastLogTime = 0;    
  }
}
