#
# Update SQL
#
#date: 2014/9/20 
#author: Sirius
#
#version: 0.1.6
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

use dsnode;
# 修改数据库版本号的存储方式
drop table if exists DSNodeInfo;
drop procedure if exists GetDSNodeVersion;
drop procedure if exists SetDSNodeVersion;
create table DSNodeInfo
(
  DSKey char(64) not null,
  DSValue char(255) not null,  
  primary key (DSKey)
) ENGINE=MyISAM;

delimiter $$
create procedure GetDSNodeVersion(out dsversion char(10))
begin
  select DSValue from DSNodeInfo where DSKey = 'DSNodeVersion' limit 1 into dsversion;
end $$

create procedure SetDSNodeVersion(in dsversion char(10))
begin
  replace into DSNodeInfo(DSKey, DSValue) values ('DSNodeVersion', dsversion); 
end $$
delimiter ;

call SetDSNodeVersion('0.1.6');

# 数据库表字符串类型修改
alter table Nickname modify Nickname char(32);
alter table GowStar modify Nickname char(32);
alter table MailInfo modify Sender char(32);
alter table MailInfo modify SendDate char(24);
alter table MailInfo modify ExpiryDate char(24);
alter table MailInfo modify Title char(64);
alter table MailInfo modify Text text(1024);
alter table MailInfo modify ItemIds char(128);
alter table MailInfo modify ItemNumbers char(64);
alter table ActivationCode modify ActivationCode char(32);
alter table ActivationCode modify Account char(64);
alter table Account modify Account char(64);
alter table UserInfo modify AccountId char(64);
alter table UserInfo modify Nickname char(32);
alter table UserInfo modify LastLogoutTime char(24);
alter table UserInfo modify CreateTime char(24);
alter table UserInfoExtra modify LastResetStaminaTime char(24);
alter table UserInfoExtra modify LastResetMidasTouchTime char(24);
alter table UserInfoExtra modify LastResetSellTime char(24);
alter table UserInfoExtra modify LastResetDailyMissionTime char(24);
alter table UserInfoExtra modify LastResetSceneCountTime char(24);
alter table UserInfoExtra modify LastResetAttemptAwardCountTime char(24);
alter table EquipInfo modify Guid char(16);
alter table ItemInfo modify Guid char(16);
alter table LegacyInfo modify Guid char(16);
alter table XSoulInfo modify Guid char(16);
alter table SkillInfo modify Guid char(16);
alter table MissionInfo modify Guid char(16);
alter table LevelInfo modify Guid char(16);
alter table ExpeditionInfo modify Guid char(16);
alter table ExpeditionInfo modify Unrewarded char(64);
alter table ExpeditionInfo modify ImageA blob(8192);
alter table ExpeditionInfo modify ImageB blob(8192);
alter table MailStateInfo modify Guid char(16);
alter table PartnerInfo modify Guid char(16);
alter table FriendInfo modify Guid char(16);
alter table FriendInfo modify FriendNickname char(32);

# 存储商品兑换数据
alter table UserInfoExtra add column ExchangeGoodList char(255); 
update UserInfoExtra set ExchangeGoodList = '' where Guid > 0;
alter table UserInfoExtra add column ExchangeGoodNumber char(64); 
update UserInfoExtra set ExchangeGoodNumber = '' where Guid > 0;
alter table UserInfoExtra add column LastResetExchangeGoodTime char(24); 
update UserInfoExtra set LastResetExchangeGoodTime = '1984/1/1 0:00:00' where Guid > 0;