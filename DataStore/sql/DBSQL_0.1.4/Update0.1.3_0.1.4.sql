#
# Update SQL
#
#date: 2014/9/12 
#author: Sirius
#
#version: 0.1.4
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

use dsnode;

# AccountInfo表增加IsValid
alter table Account add column IsValid boolean; 
update Account set IsValid = '1' where Account != '';

# UserInfoExtra表增加AttemptAward相关
alter table UserInfoExtra add column AttemptAward int; 
update UserInfoExtra set AttemptAward = 0 where Guid > 0;
alter table UserInfoExtra add column AttemptCurAcceptedCount int; 
update UserInfoExtra set AttemptCurAcceptedCount = 0 where Guid > 0;
alter table UserInfoExtra add column AttemptAcceptedAward int; 
update UserInfoExtra set AttemptAcceptedAward = 0 where Guid > 0;
alter table UserInfoExtra add column LastResetAttemptAwardCountTime char(255); 
update UserInfoExtra set LastResetAttemptAwardCountTime = '1984/9/17 0:00:00' where Guid > 0;

# ExpeditionInfo表增加Rage
alter table ExpeditionInfo add column Rage int; 
update ExpeditionInfo set Rage = 0 where UserGuid > 0;