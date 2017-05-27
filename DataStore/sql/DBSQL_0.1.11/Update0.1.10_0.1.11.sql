#
# Update SQL
#
#author: Sirius
#
#version notes:
#  0.1.0
#    Initial version. Date:2014-09-09
#  0.1.1
#    ����������ӻ��PartnerInfo�ͺ���FriendInfo. Date:2014-09-10
#  0.1.2
#    �ʼ���MailInfo�е�Text�ֶδ�С��Ϊ4096�ֽڣ��������͸�Ϊtext�� Date:2014-09-10
#  0.1.3
#    X���XSoulInfo������һ��XSoulModelLevel�� Date:2014-09-12
#  0.1.4
#    Account������IsValid;UserInfoExtra������AttemptAward���;ExpeditionInfo������Rage. Date:2014-09-17
#  0.1.5
#    �������ݿ�汾��֤���ơ� Date:2014-09-18
#  0.1.6
#    ���ư汾��֤���ƣ����ݿ���ַ��������޸ģ��洢��Ʒ�һ����ݡ� Date:2014-09-20
#  0.1.7
#    �洢����ֵ��ÿ��ͨ�����ݺ�ǩ�����ݡ� Date:2014-09-25
#  0.1.8
#    Account����ӷ�ͣ��ʶ�����ֽ�ѧ���¿�������ݣ������ Date:2014-09-26
#  0.1.9
#    �̵�ˢ�¼�����ˢ�𸱱����������������� Date:2014-10-08
#  0.1.10
#    Guid�洢�޸ģ� Date:2014-10-09
#  0.1.11
#    ������ս����¼�����������а��޸ģ�UserInfo��UserInfoExtra��LegacyInfo���޸� Date:2014-10-10

use dsnode;
# �޸����ݿ�汾��
call SetDSNodeVersion('0.1.11');

alter table UserInfo add column Vip int not null; 
update UserInfo set Vip = 0 where Guid > 0;
alter table UserInfo add column NewbieActionFlag int not null; 
update UserInfo set NewbieActionFlag = 0 where Guid > 0;
alter table UserInfoExtra modify column ExchangeGoodRefreshCount char(255) not null;
update UserInfoExtra set ExchangeGoodRefreshCount = '' where Guid > 0;
alter table UserInfoExtra add column GowHistroyTimeList char(255) not null; 
update UserInfoExtra set GowHistroyTimeList = '' where Guid > 0;
alter table UserInfoExtra add column GowHistroyEloList char(255) not null; 
update UserInfoExtra set GowHistroyEloList = '' where Guid > 0;
alter table LegacyInfo drop column LegacyNum; 
alter table LegacyInfo change column Level LegacyLevel int not null; 

drop table if exists ArenaInfo;
create table ArenaInfo
(
  UserGuid bigint not null,
  Rank int not null,
  IsValid boolean not null,
  IsRobot boolean not null,
  ArenaBytes blob(8192) not null,
  primary key (UserGuid)
) ENGINE=MyISAM;

drop table if exists ArenaRecord;
create table ArenaRecord
(
  Guid char(16) not null,
  IsValid boolean not null,
  UserGuid bigint not null,
  IsChallengerSuccess boolean not null,
  BeginTime char(24) not null,
  EndTime char(24) not null,
  CGuid bigint not null,
  CHeroId int not null,
  CLevel int not null,
  CFightScore int not null,
  CNickname char(32) not null,
  CRank int not null,
  CUserDamage int not null,
  CPartnerId1 int not null,
  CPartnerDamage1 int not null,
  CPartnerId2 int not null,
  CPartnerDamage2 int not null,
  CPartnerId3 int not null,
  CPartnerDamage3 int not null,
  TGuid bigint not null,
  THeroId int not null,
  TLevel int not null,
  TFightScore int not null,
  TNickname char(32) not null,
  TRank int not null,
  TUserDamage int not null,
  TPartnerId1 int not null,
  TPartnerDamage1 int not null,
  TPartnerId2 int not null,
  TPartnerDamage2 int not null,
  TPartnerId3 int not null,
  TPartnerDamage3 int not null,
  primary key (Guid)
) ENGINE=MyISAM;


