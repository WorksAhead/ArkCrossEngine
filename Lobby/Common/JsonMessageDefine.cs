using System;
using Newtonsoft.Json;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    public class JsonMessage
    {
        [JsonIgnore]
        public int m_ID = -1;

        [JsonIgnore]
        public object m_ProtoData = null;

        protected JsonMessage(int id)
        {
            m_ID = id;
        }
    }

    internal delegate bool JsonMessageFilterDelegate(JsonMessage msg, int handle, uint seq);
    internal delegate void JsonMessageHandlerDelegate(JsonMessage msg, int handle, uint seq);

    //----------------------------------------------------------------------------------------------
    //新风格的消息定义，用于除登录流程外的逻辑功能消息，此类消息json部分简化为只包含一个account或guid
    //或一个account加一个guid，消息数据采用proto-buf表示
    //----------------------------------------------------------------------------------------------
    public class JsonMessageWithAccount : JsonMessage
    {
        public string m_Account = "";

        public JsonMessageWithAccount(JsonMessageID id)
          : base((int)id)
        { }
    }
    public class JsonMessageWithGuid : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageWithGuid(JsonMessageID id)
          : base((int)id)
        { }
    }
    public class JsonMessageWithAccountAndGuid : JsonMessage
    {
        public string m_Account = "";
        public ulong m_Guid = 0;

        public JsonMessageWithAccountAndGuid(JsonMessageID id)
          : base((int)id)
        { }
    }

    //----------------------------------------------------------------------------------------------
    //旧风格的消息定义，除登录流程外的消息应逐渐改成上面新风格的消息样式
    //----------------------------------------------------------------------------------------------
    public class JsonMessageZero : JsonMessage
    {
        public string m_TestMsg = "";

        public JsonMessageZero()
          : base((int)JsonMessageID.Zero)
        { }
        public override string ToString()
        {
            return "JsonMessageZero:" + m_TestMsg;
        }
    }

    public class JsonMessageLogout : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageLogout()
          : base((int)JsonMessageID.Logout)
        { }
    }

    public class JsonMessageRequestMatch : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_MemberCount = 2;
        public int m_SceneType = 0;

        public JsonMessageRequestMatch()
          : base((int)JsonMessageID.RequestMatch)
        { }
    }

    public class JsonMessageSinglePVE : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SceneType = 0;

        public JsonMessageSinglePVE()
          : base((int)JsonMessageID.SinglePVE)
        { }
    }

    public class JsonMessageCancelMatch : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageCancelMatch()
          : base((int)JsonMessageID.CancelMatch)
        { }
    }

    public class JsonMessageMatchResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public bool m_IsSucceed = false;

        public JsonMessageMatchResult()
          : base((int)JsonMessageID.MatchResult)
        { }
    }

    public class JsonMessageStartGame : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageStartGame()
          : base((int)JsonMessageID.StartGame)
        { }
    }

    public class JsonMessageNodeJsRegister : JsonMessage
    {
        public string m_Name = "";

        public JsonMessageNodeJsRegister()
          : base((int)JsonMessageID.NodeJsRegister)
        { }
    }

    public class JsonMessageNodeJsRegisterResult : JsonMessage
    {
        public bool m_IsOk = false;

        public JsonMessageNodeJsRegisterResult()
          : base((int)JsonMessageID.NodeJsRegisterResult)
        { }
    }

    public class JsonMessageQuitRoom : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageQuitRoom()
          : base((int)JsonMessageID.QuitRoom)
        { }
    }

    public class JsonMessageUserHeartbeat : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageUserHeartbeat()
          : base((int)JsonMessageID.UserHeartbeat)
        { }
    }

    public class JsonMessageSyncPrepared : JsonMessage
    {
        public ulong m_Guid = 0;
        public ulong m_PreparedGuid = 0;

        public JsonMessageSyncPrepared()
          : base((int)JsonMessageID.SyncPrepared)
        { }
    }

    public class JsonMessageSyncQuitRoom : JsonMessage
    {
        public ulong m_Guid = 0;
        public ulong m_QuitGuid = 0;

        public JsonMessageSyncQuitRoom()
          : base((int)JsonMessageID.SyncQuitRoom)
        { }
    }

    public class RoomInfoForMessage
    {
        public int m_RoomId = 0;
        public string m_Creator = "";
        public int m_Type = 0;
        public int m_TotalCount = 0;
        public int m_CurCount = 0;
    }

    public class RoomUserInfoForMessage
    {
        public ulong m_Guid = 0;
        public string m_Nick = "";
        public int m_CampId = 0;
        public int m_HeroId = 0;
        public int m_WeaponId = 0;
    }

    public class JsonMessageAddFriend : JsonMessage
    {
        public ulong m_Guid = 0;
        public string m_TargetNick = "";
        public ulong m_TargetGuid = 0;

        public JsonMessageAddFriend()
          : base((int)JsonMessageID.AddFriend)
        { }
    }

    public enum AddFriendResult : int
    {
        ADD_SUCCESS = 0,
        ADD_NONENTITY_ERROR,
        ADD_OWN_ERROR,
        ADD_PLAYERSELF_ERROR,
        ADD_NOTICE,
        ADD_OVERFLOW,
        ERROR
    }
    public class FriendInfoForMsg
    {
        public FriendInfoForMsg()
        {
            this.Guid = 0;
            this.Nickname = null;
            this.Level = 1;
            this.FightingScore = 0;
            this.IsBlack = false;
        }
        public ulong Guid { get; set; }
        public string Nickname { get; set; }
        public int Level { get; set; }
        public int FightingScore { get; set; }
        public bool IsBlack { get; set; }
        //public ItemDataMsg[] Equipments = null;
        //public SkillDataInfo[] SkillInfo = null;
    }
    public class JsonMessageAddFriendResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public ulong m_TargetGuid = 0;
        public string m_TargetNick = null;
        public FriendInfoForMsg m_FriendInfo = null;
        public int m_Result = (int)AddFriendResult.ERROR;

        public JsonMessageAddFriendResult()
          : base((int)JsonMessageID.AddFriendResult)
        { }
    }

    public class JsonMessageConfirmFriend : JsonMessage
    {
        public ulong m_Guid = 0;
        public ulong m_TargetGuid = 0;

        public JsonMessageConfirmFriend()
          : base((int)JsonMessageID.ConfirmFriend)
        { }
    }

    public class JsonMessageDelFriend : JsonMessage
    {
        public ulong m_Guid = 0;
        public ulong m_TargetGuid = 0;

        public JsonMessageDelFriend()
          : base((int)JsonMessageID.DelFriend)
        { }
    }

    public enum DelFriendResult : int
    {
        DEL_SUCCESS = 0,
        DEL_NONENTITY_ERROR,
        ERROR
    }

    public class JsonMessageDelFriendResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public ulong m_TargetGuid = 0;
        public int m_Result = (int)DelFriendResult.ERROR;

        public JsonMessageDelFriendResult()
          : base((int)JsonMessageID.DelFriendResult)
        { }
    }

    public class JsonMessageSyncCancelFindTeam : JsonMessage
    {
        public ulong m_Guid = 0;
        public string m_CancelName = "";
        public ulong m_leader = 0;
        public string m_Account = "";

        public JsonMessageSyncCancelFindTeam()
          : base((int)JsonMessageID.SyncCancelFindTeam)
        { }
    }

    public class JsonMessageFriendList : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageFriendList()
          : base((int)JsonMessageID.FriendList)
        { }
    }

    public class JsonMessageSyncFriendList : JsonMessage
    {
        public ulong m_Guid = 0;
        public FriendInfoForMsg[] m_FriendInfo = null;

        public JsonMessageSyncFriendList()
          : base((int)JsonMessageID.SyncFriendList)
        { }
    }

    public class JsonMessageSyncTeamingState : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageSyncTeamingState()
          : base((int)JsonMessageID.SyncTeamingState)
        { }
    }

    public class JsonMessageSyncMpveBattleResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Result = (int)BattleResultEnum.Unfinish;

        public JsonMessageSyncMpveBattleResult()
          : base((int)JsonMessageID.SyncMpveBattleResult)
        { }
    }

    public class JsonMessageSyncGowBattleResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Result = (int)BattleResultEnum.Unfinish;
        public int m_OldGowElo = 0;
        public int m_GowElo = 0;
        public int m_MaxMultiHitCount = 0;
        public int m_TotalDamage = 0;
        public string m_EnemyNick = "";
        public int m_EnemyHeroId = 0;
        public int m_EnemyOldGowElo = 0;
        public int m_EnemyGowElo = 0;
        public int m_EnemyMaxMultiHitCount = 0;
        public int m_EnemyTotalDamage = 0;

        public JsonMessageSyncGowBattleResult()
          : base((int)JsonMessageID.SyncGowBattleResult)
        { }
    }

    public class JsonMessageDiscardItem : JsonMessage
    {
        public ulong m_Guid = 0;
        public string m_ItemId = "";
        public string m_PropertyId = "";
        public JsonMessageDiscardItem()
          : base((int)JsonMessageID.DiscardItem)
        {
        }
    }

    public enum GeneralOperationResult : int
    {
        LC_Succeed = 0,
        LC_Failure_CostError = 1,
        LC_Failure_Position = 2,
        LC_Failure_NotUnLock = 3,
        LC_Failure_LevelError = 4,
        LC_Failure_Overflow = 5,
        LC_Failure_Time = 6,
        LC_Failure_VigorError = 7,
        LC_Failure_Unknown = 8,
        LC_Failure_Arena_NotFindTarget = 9,
        LC_Failure_Code_Used = 10,          //激活码/礼品码已经被使用
        LC_Failure_Code_Error = 11,         //激活码/礼品码有误
        LC_Failure_NotFinduser = 12,
        LC_Failure_PartNumError = 13,
        LC_Failure_MaterialNumError = 14,
        LC_Failure_InCd,
        LC_Failure_NoFightCount,
        LC_Failure_InMatching,
        LC_Failure_Full,
        LC_Failuer_NotMatch,
        LC_Failuer_Offline,
        LC_Failuer_Busy,
    }

    public class JsonMessageDiscardItemResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int[] m_ItemId = null;
        public int[] m_PropertyId = null;
        public int m_GoldIncome = 0;
        public int m_TotalIncome = 0;
        public int m_Result = 3;
        public JsonMessageDiscardItemResult()
          : base((int)JsonMessageID.DiscardItemResult)
        {
        }
    }

    public class JsonMessageMountEquipment : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_ItemID = -1;
        public int m_PropertyID = 0;
        public int m_EquipPos = -1;
        public JsonMessageMountEquipment()
          : base((int)JsonMessageID.MountEquipment)
        {
        }
    }

    public class JsonMessageMountEquipmentResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_ItemID = -1;
        public int m_PropertyID = 0;
        public int m_EquipPos = -1;
        public int m_Result = 3;
        public JsonMessageMountEquipmentResult()
          : base((int)JsonMessageID.MountEquipmentResult)
        {
        }
    }


    public class JsonMessageUnmountEquipment : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_EquipPos = -1;
        public JsonMessageUnmountEquipment()
          : base((int)JsonMessageID.UnmountEquipment)
        {
        }
    }

    public class JsonMessageUnmountEquipmentResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_EquipPos = -1;
        public int m_Result = 3;
        public JsonMessageUnmountEquipmentResult()
          : base((int)JsonMessageID.UnmountEquipmentResult)
        {
        }
    }

    public class JsonMessageDirectLogin : JsonMessage
    {
        public string m_Account = "";
        public int m_LoginServerId = 1;
        public JsonMessageDirectLogin()
          : base((int)JsonMessageID.DirectLogin)
        { }
    }
    public class JsonMessageAccountLogin : JsonMessage
    {
        public string m_Account = "";
        public int m_OpCode = 0;
        public int m_ChannelId = 0;
        public string m_Data = "";
        public int m_LoginServerId = 1;
        public string m_ClientGameVersion = "0";
        public string m_ClientLoginIp = "127.0.0.1";
        public string m_GameChannelId = "2010071003";
        public string m_UniqueIdentifier = "";
        public string m_System = "all";
        public JsonMessageAccountLogin()
          : base((int)JsonMessageID.AccountLogin)
        { }
    }
    public enum AccountLoginResult : int
    {
        Success = 0,    //登录成功
        FirstLogin,     //账号首次登录
        Error,          //登录失败
        Wait,           //登录人太多，等待
        Banned,         //账号已被封停
        AlreadyOnline,  //账号在别处已登录
        Queueing,       //排队
    }
    public class JsonMessageAccountLoginResult : JsonMessage
    {
        public string m_Account = "";
        public string m_AccountId = "";
        public int m_Result = (int)AccountLoginResult.Error;
        public JsonMessageAccountLoginResult()
          : base((int)JsonMessageID.AccountLoginResult)
        { }
    }
    public class JsonMessageAccountLogout : JsonMessage
    {
        public string m_Account = "";
        public JsonMessageAccountLogout()
          : base((int)JsonMessageID.AccountLogout)
        { }
    }
    public class JsonMessageRoleList : JsonMessage
    {
        public string m_Account = "";
        public JsonMessageRoleList()
          : base((int)JsonMessageID.RoleList)
        { }
    }
    public enum RoleListResult : int
    {
        Success = 0,      //返回角色列表成功
        AccountNotLogin,  //账号未登录或不存在
        UnknownError,     //未知错误
    }
    public class UserInfoForMessage
    {
        public string m_Nickname = "";  //角色昵称
        public int m_HeroId = 0;      //职业类型
        public int m_Level = 0;         //等级
        public ulong m_UserGuid = 0;     //角色Guid
    }
    public class JsonMessageRoleListResult : JsonMessage
    {
        public string m_Account = "";
        public int m_Result = (int)RoleListResult.UnknownError;
        public int m_UserInfoCount = 0;
        public UserInfoForMessage[] m_UserInfos = null;
        public JsonMessageRoleListResult()
          : base((int)JsonMessageID.RoleListResult)
        { }
    }
    public class JsonMessageCreateNickname : JsonMessage
    {
        public string m_Account = "";
        public JsonMessageCreateNickname()
          : base((int)JsonMessageID.CreateNickname)
        { }
    }
    public class JsonMessageCreateNicnameResult : JsonMessage
    {
        public string m_Account = "";
        public string[] m_Nicknames = null;
        public JsonMessageCreateNicnameResult()
          : base((int)JsonMessageID.CreateNicknameResult)
        { }
    }
    public class JsonMessageCreateRole : JsonMessage
    {
        public string m_Account = "";
        public int m_HeroId = 0;
        public string m_Nickname;
        public JsonMessageCreateRole()
          : base((int)JsonMessageID.CreateRole)
        { }
    }
    public enum CreateRoleResult
    {
        Success,
        NicknameError,
        UnknownError,
    }
    public class JsonMessageCreateRoleResult : JsonMessage
    {
        public string m_Account = "";
        public int m_Result = (int)CreateRoleResult.UnknownError;
        public UserInfoForMessage m_UserInfo = null;
        public JsonMessageCreateRoleResult()
          : base((int)JsonMessageID.CreateRoleResult)
        { }
    }
    public class JsonMessageRoleEnter : JsonMessage
    {
        public string m_Account = "";
        public ulong m_Guid = 0;
        public JsonMessageRoleEnter()
          : base((int)JsonMessageID.RoleEnter)
        { }
    }
    public enum RoleEnterResult
    {
        Success = 0,
        Wait,
        UnknownError,
    }
    public class GowDataMsg
    {
        public GowDataMsg()
        {
            this.GowElo = 1000;
            this.GowMatches = 0;
            this.GowWinMatches = 0;
            this.LeftMatchCount = 0;
        }
        public int GowElo { get; set; }
        public int GowMatches { get; set; }
        public int GowWinMatches { get; set; }
        public int LeftMatchCount { get; set; }
    }
    public class ItemDataMsg
    {
        public ItemDataMsg()
        {
            this.ItemId = 0;
            this.Level = 0;
            this.Num = 1;
            this.AppendProperty = 0;
        }
        public int ItemId { get; set; }
        public int Level { get; set; }
        public int Num { get; set; }
        public int AppendProperty { get; set; }
    }
    public class LegacyDataMsg
    {
        public LegacyDataMsg()
        {
            this.ItemId = 0;
            this.IsUnlock = false;
        }
        public int ItemId { get; set; }
        public int Level { get; set; }
        public int AppendProperty { get; set; }
        public bool IsUnlock { get; set; }
    }

    public class SceneDataMsg
    {
        public int m_SceneId = 0;
        public int m_Grade = 0;
    }

    public class JsonMessageRoleEnterResult : JsonMessage
    {
        public string m_Account;
        public int m_Result = (int)RoleEnterResult.UnknownError;
        public ulong m_Guid = 0;
        public ulong m_Money = 0;
        public ulong m_Gold = 0;
        public int m_Stamina = 0;
        public int m_Exp = 0;
        public int m_Level = 0;
        public int m_CitySceneId = 0;
        public int m_BuyStaminaCount = 0;
        public int m_BuyMoneyCount = 0;
        public int m_CurSellItemGoldIncome = 0;
        public int m_Vip = 0;
        public int m_NewbieGuideScene = 0;
        public GowDataMsg m_Gow = null;
        public int[] m_NewbieGuides = null;
        public ItemDataMsg[] m_BagItems = null;
        public ItemDataMsg[] m_Equipments = null;
        public SkillDataInfo[] m_SkillInfo = null;
        public MissionInfoForSync[] m_Missions = null;
        public LegacyDataMsg[] m_Legacys = null;
        public SceneDataMsg[] m_SceneData = null;
        public JsonMessageRoleEnterResult()
          : base((int)JsonMessageID.RoleEnterResult)
        { }
    }
    public class JsonMessageMountSkill : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = 0;
        public int m_SkillID = -1;
        public int m_SlotPos = 0;
        public JsonMessageMountSkill()
          : base((int)JsonMessageID.MountSkill)
        {
        }
    }
    public class JsonMessageMountSkillResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = 0;
        public int m_SkillID = -1;
        public int m_SlotPos = 0;
        public int m_Result = 3;
        public JsonMessageMountSkillResult()
          : base((int)JsonMessageID.MountSkillResult)
        {
        }
    }
    public class JsonMessageUnmountSkill : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = -1;
        public int m_SlotPos = 0;
        public JsonMessageUnmountSkill()
          : base((int)JsonMessageID.UnmountSkill)
        {
        }
    }
    public class JsonMessageUnmountSkillResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = -1;
        public int m_SlotPos = 0;
        public int m_Result = 3;
        public JsonMessageUnmountSkillResult()
          : base((int)JsonMessageID.UnmountSkillResult)
        {
        }
    }
    public class JsonMessageUpgradeSkill : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = -1;
        public int m_SkillID = -1;
        public bool m_AllowCostGold = false;
        public JsonMessageUpgradeSkill()
          : base((int)JsonMessageID.UpgradeSkill)
        {
        }
    }
    public class JsonMessageUpgradeSkillResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = -1;
        public int m_SkillID = -1;
        public bool m_AllowCostGold = false;
        public int m_Result = 3;
        public JsonMessageUpgradeSkillResult()
          : base((int)JsonMessageID.UpgradeSkillResult)
        {
        }
    }
    public class JsonMessageUnlockSkill : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = -1;
        public int m_SkillID = -1;
        public int m_UserLevel = 0;
        public JsonMessageUnlockSkill()
          : base((int)JsonMessageID.UnlockSkill)
        {
        }
    }
    public class JsonMessageUnlockSkillResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = -1;
        public int m_SkillID = -1;
        public int m_UserLevel = 0;
        public int m_Result = 3;
        public JsonMessageUnlockSkillResult()
          : base((int)JsonMessageID.UnlockSkillResult)
        {
        }
    }
    public class JsonMessageSwapSkill : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = -1;
        public int m_SkillID = -1;
        public int m_SourcePos = 0;
        public int m_TargetPos = 0;
        public JsonMessageSwapSkill()
          : base((int)JsonMessageID.SwapSkill)
        {
        }
    }
    public class JsonMessageSwapSkillResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_PresetIndex = -1;
        public int m_SkillID = -1;
        public int m_SourcePos = 0;
        public int m_TargetPos = 0;
        public int m_Result = 3;
        public JsonMessageSwapSkillResult()
          : base((int)JsonMessageID.SwapSkillResult)
        {
        }
    }
    public class JsonMessageUpgradeItem : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Position = -1;
        public int m_ItemId = -1;
        public bool m_AllowCostGold = false;
        public JsonMessageUpgradeItem()
          : base((int)JsonMessageID.UpgradeItem)
        {
        }
    }
    public class JsonMessageUpgradeItemResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Position = -1;
        public int m_ItemId = -1;
        public bool m_AllowCostGold = false;
        public int m_Result = 3;
        public JsonMessageUpgradeItemResult()
          : base((int)JsonMessageID.UpgradeItemResult)
        {
        }
    }
    public class JsonMessageUserLevelup : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_UserId = -1;
        public int m_UserLevel = -1;
        public JsonMessageUserLevelup()
          : base((int)JsonMessageID.UserLevelup)
        {
        }
    }
    public class JsonMessageSaveSkillPreset : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SelectedPresetIndex = -1;
        public JsonMessageSaveSkillPreset()
          : base((int)JsonMessageID.SaveSkillPreset)
        {
        }
    }
    public class JsonMessageActivateAccount : JsonMessage
    {
        public string m_Account = "";
        public string m_ActivationCode = "";
        public JsonMessageActivateAccount()
          : base((int)JsonMessageID.ActivateAccount)
        {
        }
    }
    public enum ActivateAccountResult : int
    {
        Success = 0,    //激活成功
        InvalidCode,    //失效的激活码（该激活码已经被使用）
        MistakenCode,   //错误的激活码（该激活码不存在）
        Error,          //激活失败(系统问题)
    }
    public class JsonMessageActivateAccountResult : JsonMessage
    {
        public string m_Account = "";
        public int m_Result = (int)ActivateAccountResult.Error;
        public JsonMessageActivateAccountResult()
          : base((int)JsonMessageID.ActivateAccountResult)
        {
        }
    }

    public class JsonMessageSyncStamina : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Stamina = 0;
        public JsonMessageSyncStamina()
          : base((int)JsonMessageID.SyncStamina)
        {
        }
    }
    public class JsonMessageStageClear : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SceneId = 0;
        public int m_HitCount = 0;
        public int m_MaxMultHitCount = 0;
        public int m_Hp = 0;
        public int m_Mp = 0;
        public int m_Gold = 0;

        public JsonMessageStageClear()
          : base((int)JsonMessageID.StageClear)
        {
        }
    }
    public class JsonMessageStageClearResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SceneId = 0;
        public int m_HitCount = 0;
        public int m_MaxMultHitCount = 0;
        public long m_Duration = 0;
        public int m_ItemId = 0;
        public int m_ItemCount = 0;
        public int m_ExpPoint = 0;
        public int m_Hp = 0;
        public int m_Mp = 0;
        public int m_Gold = 0;
        public int m_DeadCount = 0;
        public int m_CompletedRewardId = 0;
        public int m_SceneStarNum = 0;
        public MissionInfoForSync[] m_Missions;

        public JsonMessageStageClearResult()
          : base((int)JsonMessageID.StageClearResult)
        {
        }
    }
    public class JsonMessageAddAssets : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Money = 0;
        public int m_Gold = 0;
        public int m_Exp = 0;
        public int m_Stamina = 0;

        public JsonMessageAddAssets()
          : base((int)JsonMessageID.AddAssets)
        {
        }
    }
    public class JsonMessageAddAssetsResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Money = 0;
        public int m_Gold = 0;
        public int m_Exp = 0;
        public int m_Stamina = 0;
        public int m_Result = 3;

        public JsonMessageAddAssetsResult()
          : base((int)JsonMessageID.AddAssetsResult)
        {
        }
    }
    public class JsonMessageAddItem : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_ItemId = 0;
        public int m_ItemNum = 0;

        public JsonMessageAddItem()
          : base((int)JsonMessageID.AddItem)
        {
        }
    }
    public class JsonMessageAddItemResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_ItemId = 0;
        public int m_ItemCount = 1;
        public int m_RandomProperty = 0;
        public int m_Result = 3;

        public JsonMessageAddItemResult()
          : base((int)JsonMessageID.AddItemResult)
        {
        }
    }
    public class JsonMessageLiftSkill : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SkillId = 0;

        public JsonMessageLiftSkill()
          : base((int)JsonMessageID.LiftSkill)
        {
        }
    }
    public class JsonMessageLiftSkillResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SkillId = 0;
        public int m_Result = 3;

        public JsonMessageLiftSkillResult()
          : base((int)JsonMessageID.LiftSkillResult)
        {
        }
    }
    public class JsonMessageBuyStamina : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageBuyStamina()
          : base((int)JsonMessageID.BuyStamina)
        {
        }
    }
    public class JsonMessageBuyStaminaResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Result = 3;

        public JsonMessageBuyStaminaResult()
          : base((int)JsonMessageID.BuyStaminaResult)
        {
        }
    }

    public class JsonMessageFinishMission : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_MissionId = 0;

        public JsonMessageFinishMission()
          : base((int)JsonMessageID.FinishMission)
        {
        }
    }
    public class JsonMessageFinishMissionResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public bool m_Success;
        public int m_FinishMissionId;
        public int m_Gold = 0;
        public int m_Exp = 0;
        public int m_Diamond = 0;
        public MissionInfoForSync m_UnlockMission;

        public JsonMessageFinishMissionResult()
          : base((int)JsonMessageID.FinishMissionResult)
        {
        }
    }
    public class JsonMessageBuyLife : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageBuyLife()
          : base((int)JsonMessageID.BuyLife)
        {
        }
    }
    public class JsonMessageBuyLifeResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public bool m_Succeed = false;
        public int m_CurDiamond = 0;

        public JsonMessageBuyLifeResult()
          : base((int)JsonMessageID.BuyLifeResult)
        {
        }
    }
    public class JsonMessageUnlockLegacy : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Index = 0;
        public int m_ItemID = 0;

        public JsonMessageUnlockLegacy()
          : base((int)JsonMessageID.UnlockLegacy)
        {
        }
    }
    public class JsonMessageUnlockLegacyResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Index = 0;
        public int m_ItemID = 0;
        public int m_Result = 3;

        public JsonMessageUnlockLegacyResult()
          : base((int)JsonMessageID.UnlockLegacyResult)
        {
        }
    }
    public class JsonMessageUpgradeLegacy : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Index = 0;
        public int m_ItemID = 0;
        public bool m_AllowCostGold = false;

        public JsonMessageUpgradeLegacy()
          : base((int)JsonMessageID.UpgradeLegacy)
        {
        }
    }
    public class JsonMessageUpgradeLegacyResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Index = 0;
        public int m_ItemID = 0;
        public bool m_AllowCostGold = false;
        public int m_Result = 3;

        public JsonMessageUpgradeLegacyResult()
          : base((int)JsonMessageID.UpgradeLegacyResult)
        {
        }
    }
    public class JsonMessageNotifyNewMail : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageNotifyNewMail()
          : base((int)JsonMessageID.NotifyNewMail)
        { }
    }
    public class JsonMessageGetMailList : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageGetMailList()
          : base((int)JsonMessageID.GetMailList)
        {

        }
    }
    public class JsonMessageReadMail : JsonMessage
    {
        public ulong m_Guid = 0;
        public ulong m_MailGuid = 0;

        public JsonMessageReadMail()
          : base((int)JsonMessageID.ReadMail)
        {

        }
    }
    public class JsonMessageReceiveMail : JsonMessage
    {
        public ulong m_Guid = 0;
        public ulong m_MailGuid = 0;

        public JsonMessageReceiveMail()
          : base((int)JsonMessageID.ReceiveMail)
        {

        }
    }
    public class MailItemForMessage
    {
        public int m_ItemId;
        public int m_ItemNum;
    }
    public class MailInfoForMessage
    {
        public bool m_AlreadyRead;
        public ulong m_MailGuid;
        public int m_Module;
        public string m_Title;
        public string m_Sender;
        public DateTime m_SendTime;
        public string m_Text;
        public MailItemForMessage[] m_Items;
        public int m_Money;
        public int m_Gold;
        public int m_Stamina;
    }
    public class JsonMessageSyncMailList : JsonMessage
    {
        public ulong m_Guid = 0;
        public MailInfoForMessage[] m_Mails = null;

        public JsonMessageSyncMailList()
          : base((int)JsonMessageID.SyncMailList)
        {
        }
    }
    public class JsonMessageExpeditionReset : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Hp = 0;
        public int m_Mp = 0;
        public int m_RequestNum = 0;
        public bool m_AllowCostGold = false;

        public JsonMessageExpeditionReset()
          : base((int)JsonMessageID.ExpeditionReset)
        {
        }
    }
    public class SkillDataMsg
    {
        public SkillDataMsg()
        {
            this.ID = -1;
            this.Level = 0;
            this.Postion = 0;
        }
        public int ID { get; set; }
        public int Level { get; set; }
        public int Postion { get; set; }
    }
    public class ImageDataMsg
    {
        public ImageDataMsg()
        {
            this.Guid = 0;
            this.HeroId = 0;
            this.Nickname = "";
            this.Level = 0;
            this.FightingScore = 0;
        }
        public ulong Guid { get; set; }
        public int HeroId { get; set; }
        public string Nickname { get; set; }
        public int Level { get; set; }
        public int FightingScore { get; set; }
        public ItemDataMsg[] EquipInfo { get; set; }
        public SkillDataMsg[] SkillInfo { get; set; }
        public LegacyDataMsg[] LegacyInfo { get; set; }
    }
    public class TollgateDataForMsg
    {
        public TollgateDataForMsg()
        {
            this.Type = (int)EnemyType.ET_Monster;
            this.IsFinish = false;
        }
        public int Type { get; set; }
        public bool IsFinish { get; set; }
        public bool[] IsAcceptedAward { get; set; }
        public int[] EnemyArray { get; set; }
        public int[] EnemyAttrArray { get; set; }
        public ImageDataMsg[] UserImageArray { get; set; }
    }
    public class JsonMessageExpeditionResetResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Hp = 0;
        public int m_Mp = 0;
        public int m_Energy = 0;
        public int m_Schedule = 0;
        public int m_LastResetTimestamp = 0;
        public bool m_CanReset = true;
        public TollgateDataForMsg Tollgates = null;
        public bool m_AllowCostGold = false;
        public bool m_IsUnlock = false;
        public int m_Result = 3;

        public JsonMessageExpeditionResetResult()
          : base((int)JsonMessageID.ExpeditionResetResult)
        {
        }
    }
    public class JsonMessageRequestExpedition : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SceneId = 0;
        public int m_TollgateNum = 0;

        public JsonMessageRequestExpedition()
          : base((int)JsonMessageID.RequestExpedition)
        {
        }
    }
    public class JsonMessageRequestExpeditionResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public string m_ServerIp = "";
        public uint m_ServerPort = 0;
        public uint m_Key = 0;
        public int m_HeroId = 0;
        public int m_CampId = 0;
        public int m_SceneType = 0;
        public int m_ActiveTollgate = 0;
        public int m_Result = 3;

        public JsonMessageRequestExpeditionResult()
          : base((int)JsonMessageID.RequestExpeditionResult)
        {
        }
    }
    public class JsonMessageFinishExpedition : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SceneId = 0;
        public int m_TollgateNum = 0;
        public int m_Hp = 0;
        public int m_Mp = 0;

        public JsonMessageFinishExpedition()
          : base((int)JsonMessageID.FinishExpedition)
        {
        }
    }
    public class JsonMessageFinishExpeditionResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_SceneId = 0;
        public int m_TollgateNum = 0;
        public int m_Hp = 0;
        public int m_Mp = 0;
        public int m_Result = 3;

        public JsonMessageFinishExpeditionResult()
          : base((int)JsonMessageID.FinishExpeditionResult)
        {
        }
    }
    public class JsonMessageExpeditionAward : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_TollgateNum = 0;

        public JsonMessageExpeditionAward()
          : base((int)JsonMessageID.ExpeditionAward)
        {
        }
    }
    public class AwardItemInfo
    {
        public AwardItemInfo()
        {
            this.Id = 0;
            this.Num = 0;
        }
        public int Id { get; set; }
        public int Num { get; set; }
    }
    public class JsonMessageExpeditionAwardResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_TollgateNum = 0;
        public int m_AddMoney = 0;
        public AwardItemInfo[] m_Items = null;
        public int m_Result = 3;

        public JsonMessageExpeditionAwardResult()
          : base((int)JsonMessageID.ExpeditionAwardResult)
        {
        }
    }
    public class JsonMessageGetGowStarList : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Start = 0;
        public int m_Count = 0;

        public JsonMessageGetGowStarList()
          : base((int)JsonMessageID.GetGowStarList)
        {
        }
    }
    public class GowStarInfoForMessage
    {
        public ulong m_Guid;
        public int m_GowElo;
        public string m_Nick;
        public int m_HeroId;
        public int m_Level;
        public int m_FightingScore;
    }
    public class JsonMessageSyncGowStarList : JsonMessage
    {
        public ulong m_Guid = 0;
        public GowStarInfoForMessage[] m_Stars = null;

        public JsonMessageSyncGowStarList()
          : base((int)JsonMessageID.SyncGowStarList)
        {
        }
    }
    public class JsonMessageQueryExpeditionInfo : JsonMessage
    {
        public ulong m_Guid = 0;

        public JsonMessageQueryExpeditionInfo()
          : base((int)JsonMessageID.QueryExpeditionInfo)
        {
        }
    }
    public class JsonMessageSendMail : JsonMessage
    {
        public ulong m_Guid = 0;
        public string m_Receiver;
        public string m_Title = "";
        public string m_Text = "";
        public int m_ExpiryDate = 1;
        public int m_LevelDemand;
        public int m_ItemId = 0;
        public int m_ItemNum = 0;
        public int m_Money = 0;
        public int m_Gold = 0;
        public int m_Stamina = 0;
        public JsonMessageSendMail()
          : base((int)JsonMessageID.SendMail)
        {
        }
    }
    public class JsonMessageExpeditionFailure : JsonMessage
    {
        public ulong m_Guid = 0;
        public JsonMessageExpeditionFailure()
          : base((int)JsonMessageID.ExpeditionFailure)
        {
        }
    }
    public class JsonMessageMidasTouch : JsonMessage
    {
        public ulong m_Guid = 0;
        public JsonMessageMidasTouch()
          : base((int)JsonMessageID.MidasTouch)
        {
        }
    }
    public class JsonMessageMidasTouchResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_Count = 0;
        public int m_CostGlod = 0;
        public int m_GainMoney = 0;
        public int m_Result = 3;
        public JsonMessageMidasTouchResult()
          : base((int)JsonMessageID.MidasTouchResult)
        {
        }
    }
    public class JsonMessageResetDailyMissions : JsonMessage
    {
        public ulong m_Guid = 0;
        public MissionInfoForSync[] m_Missions = null;
        public JsonMessageResetDailyMissions()
          : base((int)JsonMessageID.ResetDailyMissions)
        {
        }
    }
    public class JsonMessageGMResetDailyMissions : JsonMessage
    {
        public ulong m_Guid = 0;
        public JsonMessageGMResetDailyMissions()
          : base((int)JsonMessageID.GMResetDailyMissions)
        {
        }
    }
    public enum GenderType : int
    {
        Other = 0,
        Mr = 1,
        Ms = 2,
    }
    public enum QueryType : int
    {
        Guid = 0,
        Name = 1,
        Level = 2,
        Score = 3,
        Fortune = 4,
        Gender = 5,
        Unknown = 6,
    }
    public class JsonMessageQueryFriendInfo : JsonMessage
    {
        public ulong m_Guid = 0;
        public int m_QueryType = (int)QueryType.Unknown;
        public ulong m_TargetGuid = 0;
        public string m_TargetName = null;
        public int m_TargetLevel = 0;
        public int m_TargetScore = 0;
        public int m_TargetFortune = 0;
        public int m_TargetGender = (int)GenderType.Other;

        public JsonMessageQueryFriendInfo()
          : base((int)JsonMessageID.QueryFriendInfo)
        {
        }
    }
    public class JsonMessageQueryFriendInfoResult : JsonMessage
    {
        public ulong m_Guid = 0;
        public FriendInfoForMsg m_Friend = null;

        public JsonMessageQueryFriendInfoResult()
          : base((int)JsonMessageID.QueryFriendInfoResult)
        {
        }
    }
}