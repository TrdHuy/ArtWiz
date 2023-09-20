@echo off

set scriptName=%~nx0
set current_dir=%CD%
set sam_path=%~1
set src_path=%~2
set lang=%~3
set cleanCMD=%~4
set buildCMD=%~5

if "%sam_path%"=="help" (
	echo %scriptName% ^<sam_path^> ^<source_code_path^> ^<language^> ^<clean_cmd^> ^<build_cmd^>
	echo #Eg: %scriptName% "D:\Programs\SAM_v7.3.0-windows\SAM_v7.3.0-windows\bin" "C:\SCS\CyberTool\CyberTool" cs "dotnet clean CyberTool.sln -c Release" "dotnet build cyber_core\cyber_core.csproj -c Release --no-restore"
	exit /b 0
)

if "%sam_path%"=="--help" (
	echo %scriptName% ^<sam_path^> ^<source_code_path^> ^<language^> ^<clean_cmd^> ^<build_cmd^>
	echo #Eg: %scriptName% "D:\Programs\SAM_v7.3.0-windows\SAM_v7.3.0-windows\bin" "C:\SCS\CyberTool\CyberTool" cs "dotnet clean CyberTool.sln -c Release" "dotnet build cyber_core\cyber_core.csproj -c Release --no-restore"
	exit /b 0
)

if "%sam_path%"=="-h" (
	echo %scriptName% ^<sam_path^> ^<source_code_path^> ^<language^> ^<clean_cmd^> ^<build_cmd^>
	echo #Eg: %scriptName% "D:\Programs\SAM_v7.3.0-windows\SAM_v7.3.0-windows\bin" "C:\SCS\CyberTool\CyberTool" cs "dotnet clean CyberTool.sln -c Release" "dotnet build cyber_core\cyber_core.csproj -c Release --no-restore"
	exit /b 0
)

if "%sam_path%"=="-help" (
	echo %scriptName% ^<sam_path^> ^<source_code_path^> ^<language^> ^<clean_cmd^> ^<build_cmd^>
	echo #Eg: %scriptName% "D:\Programs\SAM_v7.3.0-windows\SAM_v7.3.0-windows\bin" "C:\SCS\CyberTool\CyberTool" cs "dotnet clean CyberTool.sln -c Release" "dotnet build cyber_core\cyber_core.csproj -c Release --no-restore"
	exit /b 0
)

if "%sam_path%"=="" (
	setlocal EnableDelayedExpansion
	for /F %%a in ('echo prompt $E ^| cmd') do (
		set "ESC=%%a"
	)
	echo. 
	echo. 
	echo !ESC![33m========WELCOME TO SAM SCRIPT=========!ESC![0m
	echo To run the script please use:
	echo    !ESC![1m%scriptName%!ESC![0m !ESC![31m^<sam_path^>!ESC![0m ^
!ESC![37m^<source_code_path^>!ESC![0m ^
!ESC![32m^<language^>!ESC![0m ^
!ESC![35m^<clean_cmd^>!ESC![0m ^
!ESC![36m^<build_cmd^>!ESC![0m

	echo    #Eg: !ESC![1m%scriptName%!ESC![0m !ESC![31m"D:\Programs\SAM_v7.3.0-windows\SAM_v7.3.0-windows\bin"!ESC![0m ^
!ESC![37m"C:\SCS\CyberTool\CyberTool"!ESC![0m ^
!ESC![32mcs!ESC![0m ^
!ESC![35m"dotnet clean CyberTool.sln -c Release"!ESC![0m ^
!ESC![36m"dotnet build cyber_core\cyber_core.csproj -c Release --no-restore"!ESC![0m

	echo. 
	
	echo  #!ESC![1mAnd please make sure to download the SAM tool at:!ESC![0m !ESC![34;4mhttps://bart.sec.samsung.net/artifactory/sar-generic-local/SAM-Tools/SAM_v7.3.0_release/SAM_v7.3.0-windows.zip!ESC![0m.
	echo  #After downloaded SAM tool for window version, extract the zip, and you will see the ./bin folder
	echo.
	echo  #!ESC![1mAbout SAM tool homepage:!ESC![0m !ESC![34;4mhttps://github.sec.samsung.net/RS7-Architectural-Refactoring/SAM-Tools!ESC![0m.
	echo.
	echo.
	echo !ESC![33m======================================!ESC![0m
	echo.
	echo.
	cmd
	exit /b 0
)


echo sam_path=%sam_path%
echo src_path=%src_path%
echo lang=%lang%
echo cleanCMD=%cleanCMD%
echo buildCMD=%buildCMD%

if "%sam_path%"=="" (
	echo SAM directory must not be empty
	exit /b 1
) else (
	if exist "%sam_path%\sam.exe" (
		echo SAM directory existed	
	) else (
		echo SAM directory not existed
		exit /b 1
	)
)

if "%src_path%"=="" (
	echo src path directory must not be empty
	exit /b 1
) else (
	if exist "%sam_path%" (
		echo src directory existed	
	) else (
		echo src directory not existed
		exit /b 1
	)
)

if "%buildCMD%"=="" (
	echo build CMD must not be empty, #Eg: dotnet build sample.sln -c Release
	exit /b 1
)

if "%cleanCMD%"=="" (
	echo clean CMD must not be empty, #Eg: dotnet clean sample.sln -c Release
	exit /b 1
)


if "%lang%"=="" (
	echo language must not be empty, #supported lang: cs,c,java
	exit /b 1
) else (
	setlocal enabledelayedexpansion
	set supportedLang=c cs java kotlin
	for %%v in (!supportedLang!) do (
		if %lang%==%%v (
			echo %lang% language is supported
			endlocal
			goto :continue_sam
		)
	)
	echo language error, #supported lang: c cs java kotlin
	exit /b 1
)

:continue_sam
set PATH=%PATH%;%sam_path%
cd /d %src_path%
sam init
%cleanCMD%
sam scan --language %lang% %buildCMD%
sam analyze --language %lang%
sam calculate --language %lang%

for /f "tokens=2-4 delims=/ " %%a in ('date /t') do (
    set day=%%a
    set month=%%b
    set year=%%c
)
set formattedDate=%year%%day%%month%
start "" "%src_path%\.sam-dir\sam-result\html\SAM_Report_SW Project_%formattedDate%.html"
cd /d %current_dir%
pause
