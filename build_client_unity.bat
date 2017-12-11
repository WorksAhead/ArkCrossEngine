@echo off

if NOT "%1" EQU "" (
  set cfg=%1
) else (
  set cfg=UnityDebug
)

if NOT "%2" EQU "" (
  set is_pause=%2
) else (
  set is_pause=True
)

rem commandlien parameters
rem %0 --rebuild --configuration Debug|Release --copy-only

rem working directory
set workdir=%~dp0
set plugindir=%workdir%\Client\Publish\Assets\Plugins
set logdir=%workdir%\BuildLog

rem xbuild is copy from mono-3.0.3/lib/mono/4.5
rem this xbuild will probably not work in a clean machine
set xbuild=%workdir%\Tools\xbuild\xbuild.exe

rem mdb generator
set pdb2mdb=%workdir%\Tools\mono\mono.exe %workdir%\Tools\lib\mono\4.0\pdb2mdb.exe

rem show xbuild version
%xbuild% /version
echo.

rem make build log dir
mkdir %logdir%

echo building Client.sln ...
%xbuild% /nologo /noconsolelogger /property:Configuration=%cfg% ^
         /flp:LogFile=%logdir%\Client.sln.log;Encoding=UTF-8 ^
     /t:clean;rebuild ^
         %workdir%\Client\Client.sln
if NOT %ERRORLEVEL% EQU 0 (
  echo build failed, check %logdir%\Client.sln.log.
  goto error_end
) else (
  echo done.
)

echo [client]: generate *mdb debug files for mono

pushd %workdir%\Bin
for /r %%i in (*.pdb) do (
  echo %%~dpni.dll
  %pdb2mdb% %%~dpni.dll
)
popd
echo done. & echo.

rem copy dll to unity3d's plugin directory
echo "update binaries"
xcopy %workdir%\Bin\*.dll %plugindir% /y /q
xcopy %workdir%\Bin\*.mdb %plugindir% /y /q
del /a /f %plugindir%\UnityEngine.dll
del /a /f %plugindir%\UnityEngine.AIModule.dll
del /a /f %plugindir%\UnityEngine.AnimationModule.dll
del /a /f %plugindir%\UnityEngine.AudioModule.dll
del /a /f %plugindir%\UnityEngine.ClothModule.dll
del /a /f %plugindir%\UnityEngine.CoreModule.dll
del /a /f %plugindir%\UnityEngine.ParticleSystemModule.dll
del /a /f %plugindir%\UnityEngine.PhysicsModule.dll
del /a /f %plugindir%\UnityEngine.TerrainModule.dll
if NOT %ERRORLEVEL% EQU 0 (
  echo copy failed, exclusive access error? check your running process and retry.
  goto error_end
) else (
  echo done.
)

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
