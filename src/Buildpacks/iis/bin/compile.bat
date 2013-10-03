@echo off
powershell -ExecutionPolicy bypass "& %~dp0\compile.ps1 %1 %2"
exit /b %errorlevel%