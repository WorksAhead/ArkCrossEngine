﻿#
#IMPORTANT NOTE:
#  This file is generated by ProtoSql, DO NOT edit by hand.
#
#date: 2014/9/12 17:46:00
#author: Sirius
#
#version: 0.1.1
#version notes:
#  0.1.0
#    Initial version. Date:2014-09-09
#  0.1.1
#    玩家数据增加好友FriendInfo和伙伴PartnerInfo. Date:2014-09-10
#  0.1.2
#    邮件表MailInfo中的Text字段大小设为4096字节，数据类型改为text。 Date:2014-09-10
#  0.1.3
#    X魂表XSoulInfo中增加一列XSoulModelLevel。 Date:2014-09-12
use dsnode;

truncate table Guid ;
truncate table Nickname ;
truncate table GowStar ;
truncate table MailInfo ;
truncate table ActivationCode ;
truncate table Account ;
truncate table UserInfo ;
truncate table UserInfoExtra ;
truncate table EquipInfo ;
truncate table ItemInfo ;
truncate table LegacyInfo ;
truncate table XSoulInfo ;
truncate table SkillInfo ;
truncate table MissionInfo ;
truncate table LevelInfo ;
truncate table ExpeditionInfo ;
truncate table MailStateInfo ;
truncate table PartnerInfo ;
truncate table FriendInfo ;
