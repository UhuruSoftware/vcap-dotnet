@echo off
powershell -ExecutionPolicy bypass "& %~dp0\detect.ps1 %1"
exit /b %errorlevel%