//todo:node <-> lobbySrv
var MsgDefine = {
  Zero: 0,
  Logout: 1,
  RequestMatch: 2,
  CancelMatch: 3,
  MatchResult: 4,
  StartGame: 5,
  StartGameResult: 6,
  NodeJsRegister: 7,
  NodeJsRegisterResult: 8,
  QuitRoom: 9,
  UserHeartbeat: 10,
  SyncPrepared: 11,
  SyncQuitRoom: 12,
  AddFriend: 13,
  AddFriendResult: 14,
  ConfirmFriend: 15,
  DelFriend: 16,
  DelFriendResult: 17,
  FriendList: 18,
  SyncFriendList: 19,
  SyncCancelFindTeam: 20,
  AddBlack: 21,
  AddBlackResult: 22,
  DelBlack: 23,
  RefuseFriend: 24,
  SyncTeamingState: 25,
  SinglePVE: 26,
  SyncMpveBattleResult: 27,
  SyncGowBattleResult: 28,
  DiscardItem: 29,
  DiscardItemResult: 30,
  MountEquipment: 31,
  MountEquipmentResult: 32,
  UnmountEquipment: 33,
  UnmountEquipmentResult: 34,
  AccountLogin: 35,
  AccountLoginResult: 36,
  AccountLogout: 37,
  RoleList: 38,
  RoleListResult: 39,
  CreateNickname: 40,
  CreateNicknameResult: 41,
  CreateRole: 42,
  CreateRoleResult: 43,
  RoleEnter: 44,
  RoleEnterResult: 45,
  MountSkill: 46,
  MountSkillResult: 47,
  UnmountSkill: 48,
  UnmountSkillResult: 49,
  UpgradeSkill: 50,
  UpgradeSkillResult: 51,
  UnlockSkill: 52,
  UnlockSkillResult: 53,
  SwapSkill: 54,
  SwapSkillResult: 55,
  UpgradeItem: 56,
  UpgradeItemResult: 57,
  UserLevelup: 58,
  SaveSkillPreset: 59,
  ActivateAccount: 60,
  ActivateAccountResult: 61,
  SyncStamina: 62,
  StageClear: 63,
  StageClearResult: 64,
  AddAssets: 65,
  AddAssetsResult: 66,
  AddItem: 67,
  AddItemResult: 68,
  LiftSkill: 69,
  LiftSkillResult: 70,
  BuyStamina: 71,
  BuyStaminaResult: 72,
  FinishMission: 73,
  FinishMissionResult: 74,
  BuyLife: 75,
  BuyLifeResult: 76,
  UnlockLegacy: 77,
  UnlockLegacyResult: 78,
  UpgradeLegacy: 79,
  UpgradeLegacyResult: 80,
  UpdateFightingScore: 81,
  NotifyNewMail: 82,
  GetMailList: 83,
  ReceiveMail: 84,
  SyncMailList: 85,
  ExpeditionReset: 86,
  ExpeditionResetResult: 87,
  VersionVerify: 88,
  VersionVerifyResult: 89,
  RequestExpedition: 90,
  RequestExpeditionResult: 91,
  FinishExpedition: 92,
  FinishExpeditionResult: 93,
  ExpeditionAward: 94,
  ExpeditionAwardResult: 95,
  GetGowStarList: 96,
  SyncGowStarList: 97,
  DirectLogin: 98,
  QueryExpeditionInfo: 99,
  ReadMail: 100,
  SendMail: 101,
  ExpeditionFailure: 102,
  MidasTouch: 103,
  MidasTouchResult: 104,
  ResetDailyMissions: 105,
  GMResetDailyMissions: 106,
  QueryFriendInfo: 107,
  QueryFriendInfoResult: 108,
  MissionCompleted: 109,
  PublishNotice: 110,
  SyncNoticeContent: 111,
  KickUser: 112,
  FriendOnline: 113,
  FriendOffline: 114,
  SyncGroupUsers: 115,
  RequestJoinGroupResult: 116,
  ConfirmJoinGroupResult: 117,
  PinviteTeam: 118,
  RequestJoinGroup: 119,
  ConfirmJoinGroup: 120,
  QuitGroup: 121,
  RequestGroupInfo: 122,
  SyncPinviteTeam: 123,
  AddXSoulExperience: 124,
  AddXSoulExperienceResult: 125,
  SyncLeaveGroup: 126,
  RefuseGroupRequest: 127,
  SelectPartner : 128,
  SelectPartnerResult : 129,
  UpgradePartnerLevel : 130,
  UpgradePartnerLevelResult : 131,
  UpgradePartnerStage : 132,
  UpgradePartnerStageResult : 133,
  GetPartner : 134,
  ChangeCaptain: 135,
  StartMpve: 136,
  QuitPve: 137,
  RequestMpveAward: 138,
  RequestMpveAwardResult: 139,
  UpdatePosition: 140,
  RequestUsers: 141,
  RequestUsersResult: 142,
  RequestUserPosition: 143,
  RequestUserPositionResult: 144,
  XSoulChangeShowModel: 145,
  XSoulChangeShowModelResult: 146,
  ChangeCityScene: 147,
  MpveResult: 148,
  CompoundPartner: 149,
  CompoundPartnerResult : 150,
  SweepStage : 151,
  SweepStageResult : 152,
  RequestPlayerInfo: 153,
  SyncPlayerInfo: 154,
  ExchangeGoods: 155,
  ExchangeGoodsResult: 156,
  RequestVigor: 157,
  SyncVigor: 158,
  TooManyOperations: 159,
  SignInAndGetReward: 160,
  SignInAndGetRewardResult: 161,
  SyncSignInCount: 162,
  SyncAttemptInfo: 163,
  SetNewbieFlag: 164,
  SyncNewbieFlag: 165,
  QueryArenaInfo: 166,
  ArenaInfoResult: 167,
  QueryArenaMatchGroup: 168,
  ArenaMatchGroupResult: 169,
  GmKickUser: 170,
  GmLockUser: 171,
  GmUnlockUser: 172,
  MonthCardExpired: 173,
  SyncGoldTollgateInfo: 174,
  RequestRefreshExchange: 175,
  RefreshExchangeResult: 176,
  ArenaStartChallenge: 177,
  ArenaStartChallengeResult: 178,
  ArenaChallengeOver: 179,
  ArenaChallengeResult: 180,
  ExchangeGift :181,
  ExchangeGiftResult :182,
  ArenaQueryRank: 183,
  ArenaQueryRankResult: 184,
  GmAddExp: 185,
  ArenaChangePartners: 186,
  ArenaChangePartnersResult: 187,
  ArenaQueryHistory: 188,
  ArenaQueryHistoryResult: 189,
  CompoundEquip: 190,
  CompoundEquipResult: 191,
  SetNewbieActionFlag: 192,
  SyncNewbieActionFlag: 193,
  ArenaBuyFightCount: 194,
  ArenaBuyFightCountResult: 195,
  WeeklyLoginReward: 196,
  WeeklyLoginRewardResult: 197,
  GetQueueingCount: 198,
  QueueingCountResult: 199,
  GmUpdateMaxUserCount: 200,
  ArenaBeginFight: 201,
  GmQueryInfoByGuidOrNickname: 202,
  GmQueryInfosByDimNickname: 203,
  GetOnlineTimeReward: 204,
  GetOnlineTimeRewardResult: 205,
  ResetOnlineTimeRewardData: 206,
  ResetWeeklyLoginRewardData: 207,
  RecordNewbieFlag: 208,
  SyncCombatData : 209,
  ServerShutdown : 210,
  UploadFPS : 211,
  SyncGuideFlag : 212,
  RequestDare : 213,
  RequestDareResult : 214,
  AcceptedDare : 215,
  RequestDareByGuid : 216,
  SyncResetGowPrize : 217,
  RequestGowPrize : 218,
  MaxNum: 219,
};

MsgDefine.Responses = {
  LOGIN_SUCCESS: 0,
  LOGIN_FAIL: 1,
  LOGIN_USER_ERROR: 2,
  LOGIN_PWD_ERROR: 3,
  ERROR: 4
};

var fs = require('fs');
var hash = require("./hash");
var config = require("./config");
var PORT = parseInt(core_query_config("nodejsport"));
var WebSocketServer = require('ws').Server;

var cfg = {
    ssl: false,
    port: PORT,
    ssl_key: './nodejs/key.pem',
    ssl_cert: './nodejs/cert.pem'
};

var httpServ = ( cfg.ssl ) ? require('https') : require('http');
var app      = null;
var processRequest = function( req, res ) {

    res.writeHead(200);
    res.end("All glory to WebSockets!\n");
};

if ( cfg.ssl ) {
    app = httpServ.createServer({
        key: fs.readFileSync( cfg.ssl_key ),
        cert: fs.readFileSync( cfg.ssl_cert )

    }, processRequest ).listen( cfg.port );

} else {
    app = httpServ.createServer( processRequest ).listen( cfg.port );
}

var ioServer = new WebSocketServer( { server: app } );
console.log("Server runing at port: " + PORT + ".");

var accountList = new hash.HashTable();
var guidList = new hash.HashTable();

var c_LoginMonitorInterval = 5;
var c_MaxLoginCountPerIp = 0;
var loginQueueForIp = new hash.HashTable();

var lockIndex = 0;
var kickedAccounts = [{},{}];

function isKickedAccount(account) {
	try {
		return kickedAccounts[lockIndex][account];
	} catch (ex) {		
    console.log("isKickedAccount exception:"+ex);
	}
}
function addKickedAccount(account) {
	try {
		kickedAccounts[lockIndex][account] = true;
	} catch (ex) {
    console.log("addKickedAccount exception:"+ex);
	}
}

function jsonParse(jsonStr) {
  try {
    var jsonObj = JSON.parse(jsonStr);
    return jsonObj;
  } catch (ex) {
    console.log("jsonParse exception:"+ex);
  }
  return null;
}

function getClientIp(req) {
  return req.headers['x-forwarded-for'] ||
    req.connection.remoteAddress ||
    req.socket.remoteAddress;
}

function canLogin(socket) {
  if(c_MaxLoginCountPerIp<=0) {
    return true;
  }
  var ret=false;
  var ip = getClientIp(socket.upgradeReq);
  var curTime = process.uptime();
  var times = loginQueueForIp.getValue(ip);
  if(!times) {
    times = new Array();
    times.push(curTime);
    loginQueueForIp.add(ip,times);
    ret = true;
  } else {
    if(times.length>0){
      while(times.length>0){
        if(times[0]+c_LoginMonitorInterval<curTime){
          times.shift();
        } else {
          break;
        }
      }
      times.push(curTime);
      
      console.log("login count: "+times.length+" from ip: "+ip);
      
      if(times.length>c_MaxLoginCountPerIp){
        ret = false;
      } else {
        ret = true;
      }
    } else {
      times.push(curTime);
      ret = true;
    }
  }
  return ret;
}

var serverLogic = {};
serverLogic.init = function () {
  
  var unlockHandle = setInterval(function(){
    try {
	  	var unlockIndex = 1 - lockIndex;
	  	kickedAccounts[unlockIndex] = {};
	  	lockIndex = unlockIndex;
    } catch(ex) {
      console.log("unlock interval exception:"+ex);
    }	  	
  },60000);

  ioServer.on('connection', function (socket) {
    socket.logicLifeCount=30;
    var timeoutHandle = setInterval(function(){
      try {
        if(socket){
          --socket.logicLifeCount;
          if(socket.logicLifeCount <= 0){
            var socketKey = socket.upgradeReq.headers['sec-websocket-key'];
            if(socketKey){
      	      var account = socket.logicAccount;
      	      var guid = socket.logicGuid;
      	      if(guid) {	      	
      		      var logoutMsg = {m_Guid:guid};
      		      core_send_message_by_name("Lobby", MsgDefine.Logout+"|"+JSON.stringify(logoutMsg));    
      	      	guidList.remove(guid);
      	      }
      	      if(account) {
      		      var accountLogoutMsg={m_Account:account};
      		      core_send_message_by_name("Lobby", MsgDefine.AccountLogout+"|"+JSON.stringify(accountLogoutMsg));
      	      	accountList.remove(account);
      	      }
      	      
      	      console.log("socket " + socketKey + " disconnect for timeout, account:"+account+" guid:"+guid);	 
      	      
      	      socket.logicAccount = null;
      	      socket.logicGuid = null;
      	    }
            socket.close();
            clearInterval(timeoutHandle);
          }
        } else {
          clearInterval(timeoutHandle);
        }
      } catch(ex) {
        console.log("socket interval exception:"+ex);
      }
    },10000);    
    socket.on('message', function (arg) {
      try {
        var ix = arg.indexOf('|');
        if (ix >= 0) {
          var ix2 = arg.indexOf('|', ix + 1);
          var msgId = parseInt(arg.substr(0, ix));
          var msgBody;
          if (ix2 >= 0)
            msgBody = arg.substr(ix + 1, ix2 - ix - 1);
          else
            msgBody = arg.substr(ix + 1);
  
          switch (msgId) {
            case MsgDefine.VersionVerify: 
              {
                console.log("version verify:" + arg);
                var jsonObj = jsonParse(msgBody);
                if(null!=jsonObj) {
                  var version = jsonObj["m_Version"];
                  if ((config.version & 0xffffff00) == (version & 0xffffff00)) {
                    socket.send(MsgDefine.VersionVerifyResult + "|{\"m_Result\":1}");
                  } else {
                    socket.send(MsgDefine.VersionVerifyResult + "|{\"m_Result\":0}");
                  }
                }
              }
              break;
            case MsgDefine.AccountLogin: 
              {
                var jsonObj = jsonParse(msgBody);
                if(null!=jsonObj) {
	              	var account = jsonObj.m_Account;	              	
	              	if(!isKickedAccount(account)) {
	              	  if(canLogin(socket)) {
  		                core_send_message_by_name("Lobby", arg);
  		                console.log("!!post to lobbySrv msg: " + arg);
  	                  accountList.add(jsonObj.m_Account, socket);
  	                } else {
  	                  console.log("login refuse for too many login ! msg:" + arg);
  	                }
	                }
                }
              }
              break;
            case MsgDefine.DirectLogin: 
              {
                var jsonObj = jsonParse(msgBody);
                if(null!=jsonObj) {
	              	var account = jsonObj.m_Account;
	              	if(!isKickedAccount(account)) {
	              	  if(canLogin(socket)) {
  		                core_send_message_by_name("Lobby", arg);
  		                console.log("!!post to lobbySrv msg: " + arg);
  	                  accountList.add(jsonObj.m_Account, socket);
  	                } else {
  	                  console.log("login refuse for too many login ! msg:" + arg);
  	                }
	                }
                }
              }
              break;
            case MsgDefine.RoleEnter: 
              {
                core_send_message_by_name("Lobby", arg);
                console.log("!!post to lobbySrv msg: " + arg);
              }
              break;
            case MsgDefine.AccountLogout: 
              {
                core_send_message_by_name("Lobby", arg);
                console.log("!!post to lobbySrv msg: " + arg);
                var jsonObj = jsonParse(msgBody);
                if(null!=jsonObj) {
                  accountList.remove(jsonObj.m_Account);
                }
              }
              break;
            case MsgDefine.Logout: 
              {
                core_send_message_by_name("Lobby", arg);
                console.log("!!post to lobbySrv msg: " + arg);
                var jsonObj = jsonParse(msgBody);
                if(null!=jsonObj) {
                  guidList.remove(jsonObj.m_Guid);
                }
              }
              break;
            case MsgDefine.GetQueueingCount:
              {           
                var socketKey = socket.upgradeReq.headers['sec-websocket-key'];
                socket.logicLifeCount=6;
                core_send_message_by_name("Lobby", arg);
                console.log("!!post to lobbySrv msg: " + arg + " key:" + socketKey);
              }
              break;
            case MsgDefine.UserHeartbeat: 
              {                
                var socketKey = socket.upgradeReq.headers['sec-websocket-key'];
                socket.logicLifeCount=6;
                core_send_message_by_name("Lobby", arg);
                console.log("!!post to lobbySrv msg: " + arg + " key:" + socketKey);
              }
              break;
            default:
              var jsonObj = jsonParse(msgBody);
              if(null!=jsonObj) {
                var socketKey = socket.upgradeReq.headers['sec-websocket-key'];
                if (!socket.logicGuid && !socket.logicAccount) {
                  console.log("msg " + arg + " from unknown socket " + socketKey);
                  break;
                } else {
                  var guid = socket.logicGuid;
                  if (jsonObj["m_Guid"] && jsonObj["m_Guid"] != guid) {
                    console.log("msg " + arg + " from socket " + socketKey + " (guid " + guid + "), guid is different !");
                    break;
                  }
                }
              } else {
                break;
              }
              core_send_message_by_name("Lobby", arg);
              //console.log("!!post to lobbySrv msg: " + arg);
              break;
          }
        }
      } catch(ex) {
        console.log("onmessage exception:"+ex+" msg:"+arg);
      }
    });
    socket.on('close', function () {
      try {
        var socketKey = socket.upgradeReq.headers['sec-websocket-key'];
        if(socketKey){
  	      var account = socket.logicAccount;
  	      var guid = socket.logicGuid;
  	      if(guid) {	      	
  		      var logoutMsg = {m_Guid:guid};
  		      core_send_message_by_name("Lobby", MsgDefine.Logout+"|"+JSON.stringify(logoutMsg));    
  	      	guidList.remove(guid);
  	      }
  	      if(account) {
  		      var accountLogoutMsg={m_Account:account};
  		      core_send_message_by_name("Lobby", MsgDefine.AccountLogout+"|"+JSON.stringify(accountLogoutMsg));
  	      	accountList.remove(account);
  	      }
  	      
  	      console.log("socket " + socketKey + " disconnect, account:"+account+" guid:"+guid);	 
  	      
  	      socket.logicAccount = null;
  	      socket.logicGuid = null;
  	    }
        socket.close();
        clearInterval(timeoutHandle);
      } catch(ex) {
        console.log("onclose exception:"+ex);
      }
    });
    socket.on('error', function () {
      try {
        var socketKey = socket.upgradeReq.headers['sec-websocket-key'];
        if(socketKey){
  	      var account = socket.logicAccount;
  	      var guid = socket.logicGuid;
  	      if(guid) {	      	
  		      var logoutMsg = {m_Guid:guid};
  		      core_send_message_by_name("Lobby", MsgDefine.Logout+"|"+JSON.stringify(logoutMsg));    
  	      	guidList.remove(guid);
  	      }
  	      if(account) {
  		      var accountLogoutMsg={m_Account:account};
  		      core_send_message_by_name("Lobby", MsgDefine.AccountLogout+"|"+JSON.stringify(accountLogoutMsg));
  	      	accountList.remove(account);
  	      }
  	      
        	console.log("socket " + socketKey + " network error, account:"+account+" guid:"+guid);
        	  	      
  	      socket.logicAccount = null;
  	      socket.logicGuid = null;
  	    }  	    
        socket.close();
        clearInterval(timeoutHandle);
      } catch(ex) {
        console.log("onerror exception:"+ex);
      }
    });
  });
};

global.onCoreMessage = function (handle, session, msg) {
  try {
    //console.log("@@node.js receive core message:" + handle + "," + session + "," + msg);
  
    var ix = msg.indexOf('|');
    if (ix >= 0) {
      var ix2 = msg.indexOf('|', ix + 1);
      var msg_id = parseInt(msg.substr(0, ix));
      var msg_tmp;
      if (ix2 >= 0)
        msg_tmp = msg.substr(ix + 1, ix2 - ix - 1);
      else
        msg_tmp = msg.substr(ix + 1);
      var msg_body = jsonParse(msg_tmp);
      if(null==msg_body) {
        return;
      }
  
      if (msg_id === MsgDefine.AccountLoginResult) {
        var _account = msg_body["m_Account"];
        var _socket = accountList.getValue(_account);
        if (_socket) {
          _socket.send(msg);
  
          var socketKey = _socket.upgradeReq.headers['sec-websocket-key'];
          console.log("Socket2Account:" + socketKey + " -> " + _account);
  
          _socket.logicAccount = _account;
        }
      } else if (msg_id === MsgDefine.QueueingCountResult) {
        var _account = msg_body["m_Account"];
        var _socket = accountList.getValue(_account);
        if (_socket) {
          _socket.send(msg);
        }        
      } else if (msg_id === MsgDefine.ActivateAccountResult) {
        var _account = msg_body["m_Account"];
        var _socket = accountList.getValue(_account);
        if (_socket) {
          _socket.send(msg);
        }
      } else if (msg_id === MsgDefine.RoleListResult) {
        var _account = msg_body["m_Account"];
        var _socket = accountList.getValue(_account);
        if (_socket) {
          _socket.send(msg);
        }
      } else if (msg_id === MsgDefine.CreateNicknameResult) {
        var _account = msg_body["m_Account"];
        var _socket = accountList.getValue(_account);
        if (_socket) {
          _socket.send(msg);
        }
      } else if (msg_id === MsgDefine.CreateRoleResult) {
        var _account = msg_body["m_Account"];
        var _socket = accountList.getValue(_account);
        if (_socket) {
          _socket.send(msg);
        }
      } else if (msg_id === MsgDefine.RoleEnterResult) {
        var _account = msg_body["m_Account"];
        var _guid = msg_body["m_Guid"];
        var _socket = accountList.getValue(_account);
        if (_socket) {
          _socket.send(msg);
  
          var socketKey = _socket.upgradeReq.headers['sec-websocket-key'];
          console.log("Socket2Guid:" + socketKey + " -> " + _guid);
  
          guidList.add(_guid, _socket);
          _socket.logicGuid = _guid;
        }
      } else if (msg_id === MsgDefine.UserHeartbeat) {
        var _guid = msg_body["m_Guid"];
        var _socket = guidList.getValue(_guid);
        if (_socket) {
          _socket.send(msg);
        }
      } else if (msg_id === MsgDefine.KickUser) {
        var _guid = msg_body["m_Guid"];
        var _socket = guidList.getValue(_guid);
        if (_socket) {
          _socket.close();
          
          console.log("kick user, guid:"+_guid);
          
  	      var account = _socket.logicAccount;
  	      var guid = _socket.logicGuid;
  	      if(guid) {	      	
  		      guidList.remove(guid);
  	      }
  	      if(account) {
  	      	addKickedAccount(account);
  		      accountList.remove(account);
  	      }
        	
        	_socket.logicGuid = null;
        	_socket.logicAccount = null;
    	  }
      } else if (msg_id === MsgDefine.NodeJsRegisterResult) {
        var _IsOk = msg_body["m_IsOk"];
        if (_IsOk === true) {
          registered = true;
          if (tick_handle !== null) {
            clearInterval(tick_handle);
            tick_handle = null;
          }
          serverLogic.init();
        }
      } else {
        var _Guid = msg_body["m_Guid"];
        var _socket = guidList.getValue(_Guid);
        if (_socket) {
          _socket.send(msg);
        }
      }
    } else if(msg=="QuitNodeJs") {
      core_quit_nodejs();
    } else if(msg.indexOf("EvalScp ")==0) {
      eval(msg.substring(8));
      console.log("oncoremessage "+msg);
    }
  } catch(ex) {
    console.log("oncoremessage exception:"+ex+" msg:"+msg);
  }
}

var registered = false;
var tick_handle = null;
var mainLogic = {};
mainLogic.Start = function () {
  tick_handle = setInterval(function () {
    try {
      if (registered === false) {
        var name = core_service_name();
        var cacheStr = JSON.stringify({ "m_Name": name });
        var jsonMsg = MsgDefine.NodeJsRegister + "|" + cacheStr;
        core_send_message_by_name("Lobby", jsonMsg);
        console.log("!!post to lobbySrv msg: " + jsonMsg);
      }
    } catch(ex) {
      console.log("ontick exception:"+ex);
    }
  }, 8000);
}

mainLogic.Start();
