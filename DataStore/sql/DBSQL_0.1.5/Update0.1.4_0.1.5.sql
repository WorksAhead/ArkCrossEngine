#
# Update SQL
#
#date: 2014/9/18 
#author: Sirius
#
#version: 0.1.5
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

# Initialize table DSNodeInfo and procedures.
use dsnode;
create table DSNodeInfo
(
  Version char(10) not null,  
  primary key (Version)
) ENGINE=MyISAM;

delimiter $$
create procedure GetDSNodeVersion(out dsversion char(10))
begin
  select Version from DSNodeInfo limit 1 into dsversion;
end $$

create procedure SetDSNodeVersion(in dsversion char(10))
begin
  replace into DSNodeInfo(Version) values (dsversion); 
end $$
delimiter ;

call SetDSNodeVersion('0.1.5');