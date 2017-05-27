##############################################################################
# create database dsnode 
##############################################################################
drop database if exists dsnode;
create database dsnode default character set utf8 collate utf8_general_ci;

grant all privileges on dsnode.* to 
'dfds'@'%' identified by 'dfds';
          
