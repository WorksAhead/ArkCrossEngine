#
# Update SQL
#
#date: 2014/9/12 
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
#  0.1.3
#    X���XSoulInfo������һ��XSoulModelLevel�� Date:2014-09-12

use dsnode;

# X���XSoulInfo������һ��XSoulModelLevel
alter table XSoulInfo add column XSoulModelLevel int; 
update XSoulInfo set XSoulModelLevel = -1 where IsValid = '1';