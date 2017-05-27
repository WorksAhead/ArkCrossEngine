#
# Update SQL
#
#author: Sirius
#
#version notes:
#  0.1.0
#    Initial version. Date:2014-09-09
#  0.1.1
#    玩家数据增加伙伴PartnerInfo和好友FriendInfo. Date:2014-09-10
#  0.1.2
#    邮件表MailInfo中的Text字段大小设为4096字节，数据类型改为text。 Date:2014-09-10
#  0.1.3
#    X魂表XSoulInfo中增加一列XSoulModelLevel。 Date:2014-09-12
#  0.1.4
#    Account表增加IsValid;UserInfoExtra表增加AttemptAward相关;ExpeditionInfo表增加Rage. Date:2014-09-17
#  0.1.5
#    增加数据库版本验证机制。 Date:2014-09-18
#  0.1.6
#    完善版本验证机制；数据库表字符串类型修改；存储商品兑换数据。 Date:2014-09-20
#  0.1.7
#    存储精力值、每日通关数据和签到数据。 Date:2014-09-25
#  0.1.8
#    Account表添加封停标识，新手教学、月卡相关数据，礼包码 Date:2014-09-26
#  0.1.9
#    商店刷新计数，刷金副本计数，竞技场数据 Date:2014-10-08
#  0.1.10
#    Guid存储修改， Date:2014-10-09
#  0.1.11
#    竞技场战斗记录，竞技场排行榜修改，UserInfo，UserInfoExtra，LegacyInfo表修改 Date:2014-10-10

use dsnode;
# 修改数据库版本号
call SetDSNodeVersion('0.1.11');

alter table UserInfo add column Vip int not null; 
update UserInfo set Vip = 0 where Guid > 0;
alter table UserInfo add column NewbieActionFlag int not null; 
update UserInfo set NewbieActionFlag = 0 where Guid > 0;
alter table UserInfoExtra modify column ExchangeGoodRefreshCount char(255) not null;
update UserInfoExtra set ExchangeGoodRefreshCount = '' where Guid > 0;
alter table UserInfoExtra add column GowHistroyTimeList char(255) not null; 
update UserInfoExtra set GowHistroyTimeList = '' where Guid > 0;
alter table UserInfoExtra add column GowHistroyEloList char(255) not null; 
update UserInfoExtra set GowHistroyEloList = '' where Guid > 0;
alter table LegacyInfo drop column LegacyNum; 
alter table LegacyInfo change column Level LegacyLevel int not null; 

drop table if exists ArenaInfo;
create table ArenaInfo
(
  UserGuid bigint not null,
  Rank int not null,
  IsValid boolean not null,
  IsRobot boolean not null,
  ArenaBytes blob(8192) not null,
  primary key (UserGuid)
) ENGINE=MyISAM;

drop table if exists ArenaRecord;
create table ArenaRecord
(
  Guid char(16) not null,
  IsValid boolean not null,
  UserGuid bigint not null,
  IsChallengerSuccess boolean not null,
  BeginTime char(24) not null,
  EndTime char(24) not null,
  CGuid bigint not null,
  CHeroId int not null,
  CLevel int not null,
  CFightScore int not null,
  CNickname char(32) not null,
  CRank int not null,
  CUserDamage int not null,
  CPartnerId1 int not null,
  CPartnerDamage1 int not null,
  CPartnerId2 int not null,
  CPartnerDamage2 int not null,
  CPartnerId3 int not null,
  CPartnerDamage3 int not null,
  TGuid bigint not null,
  THeroId int not null,
  TLevel int not null,
  TFightScore int not null,
  TNickname char(32) not null,
  TRank int not null,
  TUserDamage int not null,
  TPartnerId1 int not null,
  TPartnerDamage1 int not null,
  TPartnerId2 int not null,
  TPartnerDamage2 int not null,
  TPartnerId3 int not null,
  TPartnerDamage3 int not null,
  primary key (Guid)
) ENGINE=MyISAM;


