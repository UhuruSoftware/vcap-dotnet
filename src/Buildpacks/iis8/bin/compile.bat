@echo off
powershell -version 2.0 -ExecutionPolicy bypass "& %~dp0\compile.ps1 %1 %2"
exit /b %errorlevel%