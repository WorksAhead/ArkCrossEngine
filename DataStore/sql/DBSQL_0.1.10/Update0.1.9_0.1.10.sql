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
#    Guid�洢�޸ģ�������ս����¼�� Date:2014-10-09

use dsnode;
# �޸����ݿ�汾��
call SetDSNodeVersion('0.1.10');

drop table if exists Guid;
create table Guid
(
  GuidType char(24) not null,
  IsValid boolean not null,
  GuidValue bigint not null,
  primary key (GuidType)
) ENGINE=MyISAM;

drop procedure if exists RestoreGuidData;
delimiter $$
create procedure RestoreGuidData()
begin
  declare maxUserGuid bigint;
  declare maxMailGuid bigint;
  
  select max(Guid) from UserInfo into maxUserGuid;
  if maxUserGuid is not null then 
    set maxUserGuid = maxUserGuid + 1;
    replace into Guid(GuidType, IsValid, GuidValue) values ('UserGuid', '1', maxUserGuid); 
  end if;

  select max(Guid) from MailInfo into maxMailGuid;
  if maxMailGuid is not null then
    set maxMailGuid = maxMailGuid + 1;
    replace into Guid(GuidType, IsValid, GuidValue) values ('MailGuid', '1', maxMailGuid);
  end if;
end $$
delimiter ;

call RestoreGuidData();


