@echo off

set current_dir=%CD%
set sam_path=%~1
set src_path=%~2
set lang=%~3
set cleanCMD=%~4
set buildCMD=%~5

if "%sam_path%"=="help" (
	echo sam_run ^<sam_path^> ^<source_code_path^> ^<language^> ^<clean_cmd^> ^<build_cmd^>
	echo #Eg: sam_run "D:\Programs\SAM_v7.3.0-windows\SAM_v7.3.0-windows\bin" "C:\SCS\CyberTool\CyberTool" cs "dotnet clean CyberTool.sln -c Release" "dotnet build cyber_core\cyber_core.csproj -c Release --no-restore"
	exit /b 0
)

if "%sam_path%"=="--help" (
	echo sam_run ^<sam_path^> ^<source_code_path^> ^<language^> ^<clean_cmd^> ^<build_cmd^>
	echo #Eg: sam_run "D:\Programs\SAM_v7.3.0-windows\SAM_v7.3.0-windows\bin" "C:\SCS\CyberTool\CyberTool" cs "dotnet clean CyberTool.sln -c Release" "dotnet build cyber_core\cyber_core.csproj -c Release --no-restore"
	exit /b 0
)

if "%sam_path%"=="-h" (
	echo sam_run ^<sam_path^> ^<source_code_path^> ^<language^> ^<clean_cmd^> ^<build_cmd^>
	echo #Eg: sam_run "D:\Programs\SAM_v7.3.0-windows\SAM_v7.3.0-windows\bin" "C:\SCS\CyberTool\CyberTool" cs "dotnet clean CyberTool.sln -c Release" "dotnet build cyber_core\cyber_core.csproj -c Release --no-restore"
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
