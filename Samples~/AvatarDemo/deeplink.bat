@echo off
setlocal enabledelayedexpansion

:: Get the directory where this .bat file is located
set "EXE_DIR=%~dp0"
set "APP_PATH="

:: Search for the first .exe in the folder that is NOT UnityCrashHandler64.exe or UnityPlayer.exe
for %%F in ("%EXE_DIR%*.exe") do (
    set "FILENAME=%%~nxF"
    if /I not "!FILENAME!"=="UnityCrashHandler64.exe" if /I not "!FILENAME!"=="UnityPlayer.exe" (
        set "APP_PATH=%%~fF"
        goto :found
    )
)

:: No valid executable found
echo ERROR: No valid .exe found in the folder.
pause
exit /b 1

:found
echo Found executable: %APP_PATH%

:: Register avatardemo:// protocol in the Windows registry for the current user
reg add "HKCU\Software\Classes\avatardemo" /ve /d "URL:AvatarDemo Protocol" /f
reg add "HKCU\Software\Classes\avatardemo" /v "URL Protocol" /d "" /f
reg add "HKCU\Software\Classes\avatardemo\shell\open\command" /ve /d "\"%APP_PATH%\" \"%%1\"" /f

echo avatardemo:// protocol successfully registered.
pause
