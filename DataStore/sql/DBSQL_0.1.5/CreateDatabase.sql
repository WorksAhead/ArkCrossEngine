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
