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
#  0.1.2
#    �ʼ���MailInfo�е�Text�ֶδ�С��Ϊ4096�ֽڣ��������͸�Ϊtext�� Date:2014-09-10

use dsnode;

# �޸ı�MailInfo��Text�е���������Ϊtext(4096)
alter table MailInfo modify Text text(4096); 
