#
# Update SQL
#
#date: 2014/9/10 14:33:17
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

use dsnode;

# 修改表MailInfo中Text列的数据类型为text(4096)
alter table MailInfo modify Text text(4096); 
