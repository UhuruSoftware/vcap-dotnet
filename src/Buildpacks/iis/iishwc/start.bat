@echo off
powershell -ExecutionPolicy bypass "& %~dp0\start.ps1"
IF %ERRORLEVEL% EQU 0 (%~dp0\iishwcx64.exe %~dp0applicationHost.config) ELSE (%~dp0\iishwcx86.exe %~dp0applicationHost.config)