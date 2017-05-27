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
#    Guid存储修改，竞技场战斗记录， Date:2014-10-09

use dsnode;
# 修改数据库版本号
call SetDSNodeVersion('0.1.10');

drop table if exists Guid;
create table Guid
(
  GuidType char(24) not null,
  IsValid boolean not null,
  GuidValue bigint not null,
  primary key (GuidType)
) ENGINE=MyISAM;

drop procedure if exists RestoreGuidData;
delimiter $$
create procedure RestoreGuidData()
begin
  declare maxUserGuid bigint;
  declare maxMailGuid bigint;
  
  select max(Guid) from UserInfo into maxUserGuid;
  if maxUserGuid is not null then 
    set maxUserGuid = maxUserGuid + 1;
    replace into Guid(GuidType, IsValid, GuidValue) values ('UserGuid', '1', maxUserGuid); 
  end if;

  select max(Guid) from MailInfo into maxMailGuid;
  if maxMailGuid is not null then
    set maxMailGuid = maxMailGuid + 1;
    replace into Guid(GuidType, IsValid, GuidValue) values ('MailGuid', '1', maxMailGuid);
  end if;
end $$
delimiter ;

call RestoreGuidData();


