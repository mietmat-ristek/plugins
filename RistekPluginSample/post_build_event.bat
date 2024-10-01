@rem post_build_event.bat "SolutionDir" "TargetName" "ConfigurationName" "PlatformName"
echo off

rem -------------------------------------------------------------------------

echo *****
echo post_build_event.bat %SolutionDir% %TargetName% %ConfigurationName% %PlatformName%
echo *****

set DllName=AdmPlugin

set tmp=%1
set tmp=%tmp:"=%
set SolutionDir=%tmp%
set tmp=

set tmp=%2
set tmp=%tmp:"=%
rem set TargetName=%tmp%
set TargetName="RistekPluginSample"
set tmp=

set tmp=%3
set tmp=%tmp:"=%
set ConfigurationName=%tmp%
set tmp=

set tmp=%4
set tmp=%tmp:"=%
set PlatformName=%tmp%
set tmp=

rem set DestDir=%SolutionDir%bin\%ConfigurationName%\%PlatformName%
set DestDir=%SolutionDir%\%TargetName%\bin\%ConfigurationName%

set PluginsDir=c:\Users\Mateusz\Documents\3DTrussme\Plugins\

rem -------------------------------------------------------------------------

rem echo *****
rem echo ***** Creating %DestDir% ...
rem echo *****
rem md "%DestDir%"

rem -------------------------------------------------------------------------

echo "ConfigurationName==%ConfigurationName%"

IF %ConfigurationName%==Debug goto start_debug
IF %ConfigurationName%==Release goto start_release

rem -------------------------------------------------------------------------

:start_release

echo "start %ConfigurationName%"

echo *****
echo ***** "%DestDir%\libz.exe inject-dll --assembly %DllName%.dll --include include Microsoft.Xaml.Behaviors.dll --move"
echo *****
call "%DestDir%\libz.exe" inject-dll --assembly %DllName%.dll --include Microsoft.Xaml.Behaviors.dll --move

echo *****
echo ***** "%DestDir%\libz.exe inject-dll --assembly %DllName%.dll --include Microsoft.Xaml.Behaviors.xml --move"
echo *****
call "%DestDir%\libz.exe" inject-dll --assembly %DllName%.dll --include Microsoft.Xaml.Behaviors.xml --move

echo *****
echo ***** "%DestDir%\libz.exe inject-dll --assembly %DllName%.dll --include pl\*.dll --exclude %DllName%.dll --move"
echo *****
call "%DestDir%\libz.exe" inject-dll --assembly %DllName%.dll --include pl\*.dll --exclude %DllName%.dll --move

echo *****
echo ***** "%DestDir%\libz.exe inject-dll --assembly %DllName%.dll --include fi\*.dll --exclude %DllName%.dll --move"
echo *****
call "%DestDir%\libz.exe" inject-dll --assembly %DllName%.dll --include fi\*.dll --exclude %DllName%.dll --move

echo *****
echo ***** Copying %DestDir%\%DllName%.* to %PluginsDir% ...
echo *****
xcopy "%DestDir%\%DllName%.*" "%PluginsDir%\" /e /q /y /d /r

goto end

rem -------------------------------------------------------------------------

:start_debug

echo "start %ConfigurationName%"

echo LLDesigner.ProjectData.dll > ~~tmp42
echo Enterprixe.rouvgadBIM.dll > ~~tmp43
echo LLDesigner.ParametricTrusses.dll > ~~tmp44
echo LLDesigner.ProjectData.xml > ~~tmp45
echo Enterprixe.rouvgadBIM.xml > ~~tmp46
echo LLDesigner.ParametricTrusses.xml > ~~tmp47
rem echo Enterprixe.rouvgadBIM.pdb > ~~tmp48
rem echo LLDesigner.ProjectData.pdb > ~~tmp49
rem echo Microsoft.Xaml.Behaviors > ~~tmp49
rem echo libz.exe > ~~tmp50

rem echo *****
rem echo ***** Copying AllConfigs\AllPlatforms\AllLangs to %DestDir% ...
rem echo *****
rem xcopy "%SolutionDir%StageSource\AllConfigs\AllPlatforms\AllLangs\*.*" "%DestDir%\" /exclude:~~tmp42 /e /q /y /d /r

rem echo *****
rem echo ***** Copying AllConfigs\%PlatformName% to %DestDir% ...
rem echo *****
rem xcopy "%SolutionDir%StageSource\AllConfigs\%PlatformName%\*.*" "%DestDir%\" /exclude:~~tmp42 /e /q /y /d /r

rem echo *****
rem echo ***** Copying %ConfigurationName%\AllPlatforms to %DestDir% ...
rem echo *****
rem xcopy "%SolutionDir%StageSource\%ConfigurationName%\AllPlatforms\*.*" "%DestDir%\" /exclude:~~tmp42 /e /q /y /d /r

rem echo *****
rem echo ***** Copying %ConfigurationName%\%PlatformName% to %DestDir% ...
rem echo *****
rem xcopy "%SolutionDir%StageSource\%ConfigurationName%\%PlatformName%\*.*" "%DestDir%\" /exclude:~~tmp42 /e /q /y /d /r

echo *****
echo ***** Copying %DestDir% to %PluginsDir% ...
echo *****
rem xcopy "%DestDir%\*.*" "%PluginsDir%\" /e /q /y /d /r
rem xcopy "%DestDir%\*.*" "%PluginsDir%\" /e /q /y /d /r /exclude:~~tmp42+~~tmp43+~~tmp44+~~tmp45+~~tmp46+~~tmp47+~~tmp48+~~tmp49+~~tmp50
xcopy "%DestDir%\*.*" "%PluginsDir%\" /e /q /y /d /r /exclude:~~tmp42+~~tmp43+~~tmp44+~~tmp45+~~tmp46+~~tmp47

echo *****
echo ***** Removing temps
echo *****
del ~~tmp42
del ~~tmp43
del ~~tmp44
del ~~tmp45
del ~~tmp46
del ~~tmp47
rem del ~~tmp48
rem del ~~tmp49
rem del ~~tmp50
del %PluginsDir%\~~tmp42
del %PluginsDir%\~~tmp43
del %PluginsDir%\~~tmp44
del %PluginsDir%\~~tmp45
del %PluginsDir%\~~tmp46
del %PluginsDir%\~~tmp47
rem del %PluginsDir%\~~tmp48
rem del %PluginsDir%\~~tmp49
rem del %PluginsDir%\~~tmp50

rem -------------------------------------------------------------------------

goto end

:error_end

set SolutionDir=
set ConfigurationName=
set PlatformName=
set DestDir=
set PluginsDir=
exit /B 1

:end

set SolutionDir=
set ConfigurationName=
set PlatformName=
set DestDir=
set PluginsDir=
