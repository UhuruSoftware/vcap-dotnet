@echo off
powershell -version 2.0 -ExecutionPolicy bypass "& %~dp0\detect.ps1 %1"
exit /b %errorlevel%