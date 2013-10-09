@echo off
powershell -ExecutionPolicy bypass "& %~dp0\release.ps1 %1"