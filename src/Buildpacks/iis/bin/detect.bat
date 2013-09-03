@echo off
powershell "& %~dp0\detect.ps1 %1"
exit /b %errorlevel%