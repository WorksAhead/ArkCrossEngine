#
# Update SQL
#
#date: 2014/9/18 
#author: Sirius
#
#version: 0.1.5
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

# Initialize table DSNodeInfo and procedures.
use dsnode;
create table DSNodeInfo
(
  Version char(10) not null,  
  primary key (Version)
) ENGINE=MyISAM;

delimiter $$
create procedure GetDSNodeVersion(out dsversion char(10))
begin
  select Version from DSNodeInfo limit 1 into dsversion;
end $$

create procedure SetDSNodeVersion(in dsversion char(10))
begin
  replace into DSNodeInfo(Version) values (dsversion); 
end $$
delimiter ;

call SetDSNodeVersion('0.1.5');