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
#    ����������ӻ��PartnerInfo�ͺ���FriendInfo. Date:2014-09-10

use dsnode;

# ������ű�PartnerInfo��FriendInfo
create table PartnerInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  PartnerId int not null,
  AdditionLevel int not null,
  SkillLevel int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index PartnerInfoIndex on PartnerInfo (UserGuid);

create table FriendInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  FriendGuid bigint not null,
  FriendNickname char(255) not null,
  HeroId int not null,
  Level int not null,
  FightingScore int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index FriendInfoIndex on FriendInfo (UserGuid);

# ��UserInfoExtra����һ��ActivePartnerId
alter table UserInfoExtra add column ActivePartnerId int; 
# ActivePartnerIdĬ��ֵΪ0
update UserInfoExtra set ActivePartnerId = 0 where Guid > 0;