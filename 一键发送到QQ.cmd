@echo off
rem One-click: send the newest GamblerMod zip to QQ "My iPhone".
rem Usage: double-click, or drag a file onto this cmd.
if "%~1"=="" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\send-to-qq.ps1"
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\send-to-qq.ps1" "%~1"
)
echo.
pause
