@echo off
ProtoGen\protogen.exe -i:.\DashFireMsg.proto -o:ProtoMessage.cs
ProtoGen\protogen.exe -i:.\DashFireLobbyMsg.proto -o:LobbyProtoMessage.cs
xcopy .\*.cs ..\..\..\Public\Common\Message\ /y /d 
pause
