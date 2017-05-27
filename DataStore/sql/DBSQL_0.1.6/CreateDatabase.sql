##############################################################################
# create database dsnode 
##############################################################################
drop database if exists dsnode;
create database dsnode default character set utf8 collate utf8_general_ci;

grant all privileges on dsnode.* to 
'dfds'@'%' identified by 'dfds';

# Initialize table DSNodeInfo and procedures.
use dsnode;
create table DSNodeInfo
(
  DSKey char(64) not null,
  DSValue char(255) not null,  
  primary key (DSKey)
) ENGINE=MyISAM;

delimiter $$
create procedure GetDSNodeVersion(out dsversion char(10))
begin
  select DSValue from DSNodeInfo where DSKey = 'DSNodeVersion' limit 1 into dsversion;
end $$

create procedure SetDSNodeVersion(in dsversion char(10))
begin
  replace into DSNodeInfo(DSKey, DSValue) values ('DSNodeVersion', dsversion); 
end $$
delimiter ;
