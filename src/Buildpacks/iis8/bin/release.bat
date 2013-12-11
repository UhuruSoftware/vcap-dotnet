@echo off
powershell -version 2.0 -ExecutionPolicy bypass "& %~dp0\release.ps1 %1"