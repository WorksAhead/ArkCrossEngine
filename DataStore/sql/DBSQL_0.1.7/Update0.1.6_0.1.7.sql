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

use dsnode;
# 修改数据库版本号
call SetDSNodeVersion('0.1.7');

# 存储精力值、每日通关数据和签到数据。
alter table UserInfoExtra add column CompleteSceneList char(255) not null; 
update UserInfoExtra set CompleteSceneList = '' where Guid > 0;
alter table UserInfoExtra add column CompleteSceneNumber char(64) not null; 
update UserInfoExtra set CompleteSceneNumber = '' where Guid > 0;
alter table UserInfoExtra add column Vigor int not null; 
update UserInfoExtra set Vigor = 0 where Guid > 0;
alter table UserInfoExtra add column LastAddVigorTimestamp double not null; 
update UserInfoExtra set LastAddVigorTimestamp = 0.0 where Guid > 0;
alter table UserInfoExtra add column UsedStamina int not null; 
update UserInfoExtra set UsedStamina = 0 where Guid > 0;
alter table UserInfoExtra add column DayRestSignCount int not null; 
update UserInfoExtra set DayRestSignCount = 0 where Guid > 0;
alter table UserInfoExtra add column LastResetDaySignCountTime char(24) not null; 
update UserInfoExtra set LastResetDaySignCountTime = '1984/1/1 0:00:00' where Guid > 0;
alter table UserInfoExtra add column MonthSignCount int not null; 
update UserInfoExtra set MonthSignCount = 0 where Guid > 0;
alter table UserInfoExtra add column LastResetMonthSignCountTime char(24) not null; 
update UserInfoExtra set LastResetMonthSignCountTime = '1984/1/1 0:00:00' where Guid > 0;