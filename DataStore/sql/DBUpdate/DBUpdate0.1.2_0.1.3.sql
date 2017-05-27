#
# Update SQL
#
#date: 2014/9/12 
#author: Sirius
#
#version: 0.1.1
#version notes:
#  0.1.0
#    Initial version. Date:2014-09-09
#  0.1.1
#    玩家数据增加伙伴PartnerInfo和好友FriendInfo. Date:2014-09-10
#  0.1.2
#    邮件表MailInfo中的Text字段大小设为4096字节，数据类型改为text。 Date:2014-09-10
#  0.1.3
#    X魂表XSoulInfo中增加一列XSoulModelLevel。 Date:2014-09-12

use dsnode;

# X魂表XSoulInfo中增加一列XSoulModelLevel
alter table XSoulInfo add column XSoulModelLevel int; 
update XSoulInfo set XSoulModelLevel = -1 where IsValid = '1';