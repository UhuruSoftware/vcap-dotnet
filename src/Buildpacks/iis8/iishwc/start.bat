@echo off
powershell -ExecutionPolicy bypass "& %~dp0\start.ps1"
set bitness=%ERRORLEVEL%
powershell -ExecutionPolicy bypass "& %~dp0\detectVersion.ps1"
set version=%ERRORLEVEL%
IF %bitness% EQU 0 (
    IF %version% EQU 40 (
        %~dp0\iishwcx64.exe %~dp0applicationHost.config %~dp0rootWeb4064.config %PORT%
    ) ELSE (
        %~dp0\iishwcx64.exe %~dp0applicationHost.config %~dp0rootWeb2064.config %PORT%
    )
) ELSE (
    IF %version% EQU 40 (
        %~dp0\iishwcx86.exe %~dp0applicationHost.config %~dp0rootWeb4086.config %PORT%
    ) ELSE (
        %~dp0\iishwcx86.exe %~dp0applicationHost.config %~dp0rootWeb2086.config %PORT%
    )
)