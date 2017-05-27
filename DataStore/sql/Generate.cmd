@echo off
rem 1. get current path
rem 2. derived proto path
rem 3. generate descriptor set file
rem 4. generate sql 

set work_path=%~dp0
set proto_path=%work_path%..\..\Public\Messages\PBMessages\ProtoFiles\
set proto_gen=%work_path%..\..\Public\Messages\PBMessages\DashFireProtoGen.exe
set proto_assembly=%proto_path%..\Generated\DashFire.DataStore.dll
set protoc="protoc.exe"
set proto_name=Data
set desc_name=%proto_name%.descriptor

rem run DashFireProtoGen.exe first
%proto_gen%

%protoc% -I%proto_path% -o%desc_name% %proto_path%DataStore\%proto_name%.proto
if NOT %errorlevel% EQU 0 (
  pause
  exit /b -1
)

ProtoSql.exe --dap="%proto_assembly%" --ddp=%desc_name% --np=%proto_path%DataStore\%proto_name%.version.notes
if NOT %errorlevel% EQU 0 (
  echo error occured during the generation process.
  pause
  exit /b -1
)

del /f /q %desc_name%
