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

use dsnode;
# �޸����ݿ�汾��
call SetDSNodeVersion('0.1.7');

# �洢����ֵ��ÿ��ͨ�����ݺ�ǩ�����ݡ�
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