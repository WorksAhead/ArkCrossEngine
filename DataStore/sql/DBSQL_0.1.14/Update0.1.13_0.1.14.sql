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
#  0.1.12
#    ���������а��޸� Date:2014-10-11
#  0.1.13
#    ���������а��޸� Date:2014-10-11
#  0.1.14
#    ������ս����¼�޸� Date:2014-10-16

use dsnode;
# �޸����ݿ�汾��
call SetDSNodeVersion('0.1.14');

truncate table ArenaRecord;

alter table ArenaRecord add column Rank int; 
update ArenaRecord set Rank = '-1' where UserGuid > 0;

create index ArenaRecordIndex on ArenaRecord (UserGuid);

