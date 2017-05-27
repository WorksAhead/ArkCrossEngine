﻿#
#IMPORTANT NOTE:
#  This file is generated by ProtoSql, DO NOT edit by hand.
#
#date: 2014/10/9 10:52:42
#author: Sirius
#
#version: 0.1.9
#version notes:
#  0.1.0
#    Initial version. Date:2014-09-09
#  0.1.1
#    玩家数据增加好友FriendInfo和伙伴PartnerInfo. Date:2014-09-10
#  0.1.2
#    邮件表MailInfo中的Text字段大小设为4096字节，数据类型改为text。 Date:2014-09-10
#  0.1.3
#    X魂表XSoulInfo中增加一列XSoulModelLevel。 Date:2014-09-12
#  0.1.4
#    AccountInfo表增加IsValid;UserInfoExtra表增加AttemptAward相关;ExpeditionInfo表增加Rage Date:2014-09-17
#  0.1.5
#    增加数据库版本验证机制。 Date:2014-09-18
#  0.1.6
#    完善版本验证机制；数据库表字符串类型修改；存储商品兑换数据。 Date:2014-09-22
#  0.1.7
#    存储精力值、每日通关数据和签到数据。 Date:2014-09-25
#  0.1.8
#    Account表添加封停标识，新手教学、月卡相关数据，礼包码 Date:2014-09-26
#  0.1.9
#    商店刷新计数，刷金副本计数，竞技场数据 Date:2014-10-08
#  0.1.10
#    Guid表修改 Date:2014-10-09

use dsnode;
call SetDSNodeVersion('0.1.10');

create table Guid
(
  GuidType char(24) not null,
  IsValid boolean not null,
  GuidValue bigint not null,
  primary key (GuidType)
) ENGINE=MyISAM;

create table Nickname
(
  Nickname char(32) not null,
  IsValid boolean not null,
  UserGuid bigint not null,
  primary key (Nickname)
) ENGINE=MyISAM;

create table GowStar
(
  Rank int not null,
  IsValid boolean not null,
  UserGuid bigint not null,
  Nickname char(32) not null,
  HeroId int not null,
  Level int not null,
  FightingScore int not null,
  GowElo int not null,
  primary key (Rank)
) ENGINE=MyISAM;

create table MailInfo
(
  Guid bigint not null,
  IsValid boolean not null,
  ModuleTypeId int not null,
  Sender char(32) not null,
  Receiver bigint not null,
  SendDate char(24) not null,
  ExpiryDate char(24) not null,
  Title char(64) not null,
  Text text(1024) not null,
  Money int not null,
  Gold int not null,
  Stamina int not null,
  ItemIds char(128) not null,
  ItemNumbers char(64) not null,
  LevelDemand int not null,
  IsRead boolean not null,
  primary key (Guid)
) ENGINE=MyISAM;

create table ActivationCode
(
  ActivationCode char(32) not null,
  IsValid boolean not null,
  IsActivated boolean not null,
  Account char(64) not null,
  primary key (ActivationCode)
) ENGINE=MyISAM;

create table GiftCode
(
  GiftCode char(32) not null,
  IsValid boolean not null,
  GiftId int not null,
  IsUsed boolean not null,
  UserGuid bigint not null,
  primary key (GiftCode)
) ENGINE=MyISAM;

create table ArenaInfo
(
  Rank int not null,
  IsValid boolean not null,
  UserGuid bigint not null,
  IsRobot boolean not null,
  ArenaBytes blob(8192) not null,
  primary key (Rank)
) ENGINE=MyISAM;

create table Account
(
  Account char(64) not null,
  IsValid boolean not null,
  IsBanned boolean not null,
  UserGuid1 bigint not null,
  UserGuid2 bigint not null,
  UserGuid3 bigint not null,
  primary key (Account)
) ENGINE=MyISAM;

create table UserInfo
(
  Guid bigint not null,
  AccountId char(64) not null,
  IsValid boolean not null,
  Nickname char(32) not null,
  HeroId int not null,
  Level int not null,
  Money int not null,
  Gold int not null,
  ExpPoints int not null,
  CitySceneId int not null,
  LastLogoutTime char(24) not null,
  CreateTime char(24) not null,
  NewbieStep bigint not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index UserInfoIndex on UserInfo (AccountId);

create table UserInfoExtra
(
  Guid bigint not null,
  IsValid boolean not null,
  GowElo int not null,
  GowMatches int not null,
  GowWinMatches int not null,
  Stamina int not null,
  BuyStaminaCount int not null,
  LastAddStaminaTimestamp double not null,
  BuyMoneyCount int not null,
  LastBuyMoneyTimestamp double not null,
  SellIncome int not null,
  LastSellTimestamp double not null,
  LastResetStaminaTime char(24) not null,
  LastResetMidasTouchTime char(24) not null,
  LastResetSellTime char(24) not null,
  LastResetDailyMissionTime char(24) not null,
  ActivePartnerId int not null,
  AttemptAward int not null,
  AttemptCurAcceptedCount int not null,
  AttemptAcceptedAward int not null,
  LastResetAttemptAwardCountTime char(24) not null,
  GoldTollgateCount int not null,
  LastResetGoldTollgateCountTime char(24) not null,
  ExchangeGoodList char(255) not null,
  ExchangeGoodNumber char(64) not null,
  LastResetExchangeGoodTime char(24) not null,
  ExchangeGoodRefreshCount int not null,
  CompleteSceneList char(255) not null,
  CompleteSceneNumber char(64) not null,
  LastResetSceneCountTime char(24) not null,
  Vigor int not null,
  LastAddVigorTimestamp double not null,
  UsedStamina int not null,
  DayRestSignCount int not null,
  LastResetDaySignCountTime char(24) not null,
  MonthSignCount int not null,
  LastResetMonthSignCountTime char(24) not null,
  MonthCardExpireTime char(24) not null,
  primary key (Guid)
) ENGINE=MyISAM;

create table EquipInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  Position int not null,
  ItemId int not null,
  ItemNum int not null,
  Level int not null,
  AppendProperty int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index EquipInfoIndex on EquipInfo (UserGuid);

create table ItemInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  Position int not null,
  ItemId int not null,
  ItemNum int not null,
  Level int not null,
  AppendProperty int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index ItemInfoIndex on ItemInfo (UserGuid);

create table LegacyInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  Position int not null,
  LegacyId int not null,
  LegacyNum int not null,
  Level int not null,
  AppendProperty int not null,
  IsUnlock boolean not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index LegacyInfoIndex on LegacyInfo (UserGuid);

create table XSoulInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  Position int not null,
  XSoulType int not null,
  XSoulId int not null,
  XSoulLevel int not null,
  XSoulExp int not null,
  XSoulModelLevel int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index XSoulInfoIndex on XSoulInfo (UserGuid);

create table SkillInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  SkillId int not null,
  Level int not null,
  Preset int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index SkillInfoIndex on SkillInfo (UserGuid);

create table MissionInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  MissionId int not null,
  MissionValue int not null,
  MissionState int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index MissionInfoIndex on MissionInfo (UserGuid);

create table LevelInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  LevelId int not null,
  LevelRecord int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index LevelInfoIndex on LevelInfo (UserGuid);

create table ExpeditionInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  StartTime double not null,
  FightingScore int not null,
  HP int not null,
  MP int not null,
  Rage int not null,
  Schedule int not null,
  MonsterCount int not null,
  BossCount int not null,
  OnePlayerCount int not null,
  Unrewarded char(64) not null,
  TollgateType int not null,
  EnemyList char(255) not null,
  EnemyAttrList char(255) not null,
  ImageA blob(8192) not null,
  ImageB blob(8192) not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index ExpeditionInfoIndex on ExpeditionInfo (UserGuid);

create table MailStateInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  MailGuid bigint not null,
  IsRead boolean not null,
  IsReceived boolean not null,
  ExpiryDate char(24) not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index MailStateInfoIndex on MailStateInfo (UserGuid);

create table PartnerInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  PartnerId int not null,
  AdditionLevel int not null,
  SkillLevel int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index PartnerInfoIndex on PartnerInfo (UserGuid);

create table FriendInfo
(
  Guid char(16) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  FriendGuid bigint not null,
  FriendNickname char(32) not null,
  HeroId int not null,
  Level int not null,
  FightingScore int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index FriendInfoIndex on FriendInfo (UserGuid);
