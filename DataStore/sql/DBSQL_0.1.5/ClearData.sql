﻿#
#IMPORTANT NOTE:
#  This file is generated by ProtoSql, DO NOT edit by hand.
#
#date: 2014/9/18 16:38:59
#author: Sirius
#
#version: 0.1.5
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
