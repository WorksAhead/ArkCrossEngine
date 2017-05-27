@echo off

if NOT "%1" EQU "" (
  set cfg=%1
) else (
  set cfg=Debug
)
if NOT "%2" EQU "" (
  set is_pause=%2
) else (
  set is_pause=True
)

rem working directory
set workdir=%~dp0
set svrbin=%workdir%\DcoreEnv\bin
set logdir=%workdir%\BuildLog

set xbuild=%workdir%\Tools\xbuild\xbuild.exe

rem mdb generator
set pdb2mdb=%workdir%\Tools\mono\mono.exe %workdir%\Tools\lib\mono\4.0\pdb2mdb.exe

rem show xbuild version
%xbuild% /version
echo.

rem make build log dir
mkdir %logdir%

echo sync client dlls...
xcopy %workdir%\Bin\* %svrbin% /y /q

echo building DataStore.sln ...
%xbuild% /nologo /noconsolelogger ^
         /flp:LogFile=%logdir%\DataStore.sln.log;Encoding=UTF-8 ^
         %workdir%\DataStore\DataStore.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\DataStore.sln.log.
  goto error_end
) else (
  echo done.
)
echo "update binaries"
xcopy %workdir%\DataStore\bin\%cfg%\* %svrbin% /y /q
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)

echo building ServerBridge.sln ...
%xbuild% /nologo /noconsolelogger ^
         /flp:LogFile=%logdir%\ServerBridge.sln.log;Encoding=UTF-8 ^
         %workdir%\ServerBridge\ServerBridge.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\ServerBridge.sln.log.
  goto error_end
) else (
  echo done.
)
echo "update binaries"
xcopy %workdir%\ServerBridge\bin\%cfg%\* %svrbin% /y /q
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)

echo building Lobby.sln ...
%xbuild% /nologo /noconsolelogger ^
         /flp:LogFile=%logdir%\Lobby.sln.log;Encoding=UTF-8 ^
         %workdir%\Lobby\Lobby.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\Lobby.sln.log.
  goto error_end
) else (
  echo done.
)

echo "update binaries"
xcopy %workdir%\Lobby\bin\%cfg%\* %svrbin% /y /q
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)

echo building DashFireServer.sln ...
%xbuild% /nologo /noconsolelogger ^
         /flp:LogFile=%logdir%\DashFireServer.sln.log;Encoding=UTF-8 ^
         %workdir%\Server\src\DashFireServer.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\DashFireServer.sln.log.
  goto error_end
) else (
  echo done.
)

echo "update binaries"
xcopy %workdir%\Server\src\bin\%cfg%\* %svrbin% /y /q
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)

echo [server]: generate *mdb debug files for mono
pushd %svrbin%
for /r %%i in (*.pdb) do (
echo generate mdb for %%~dpni.dll
%pdb2mdb% %%~dpni.dll
)
popd
echo done. & echo.

goto good_end

:error_end
set ec=1
goto end
:good_end
set ec=0
echo All Done, Good to Go.
:end
if %is_pause% EQU True (
  pause
  exit /b %ec%
)