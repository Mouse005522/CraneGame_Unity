@echo off
setlocal
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0StartUnityWeb.ps1" %*
set "launcherExitCode=%errorlevel%"
echo.
echo Unity Web server exited. Exit code: %launcherExitCode%
pause
endlocal & exit /b %launcherExitCode%
