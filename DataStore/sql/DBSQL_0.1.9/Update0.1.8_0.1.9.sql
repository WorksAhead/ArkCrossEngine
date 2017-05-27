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

use dsnode;
# �޸����ݿ�汾��
call SetDSNodeVersion('0.1.9');

alter table UserInfoExtra add column ExchangeGoodRefreshCount int not null; 
update UserInfoExtra set ExchangeGoodRefreshCount = '0' where Guid > 0;
alter table UserInfoExtra add column GoldTollgateCount int not null; 
update UserInfoExtra set GoldTollgateCount = '0' where Guid > 0;
alter table UserInfoExtra add column LastResetGoldTollgateCountTime char(24) not null; 
update UserInfoExtra set LastResetGoldTollgateCountTime = '1984/1/1 0:00:00' where Guid > 0;

create table ArenaInfo
(
  Rank int not null,
  IsValid boolean not null,
  UserGuid bigint not null,
  IsRobot boolean not null,
  ArenaBytes blob(8192) not null,
  primary key (Rank)
) ENGINE=MyISAM;

