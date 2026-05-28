@echo off
title Full Stack - Angular + .NET

echo Iniciando Backend...
start cmd /k "cd /d %~dp0backend && dotnet run"

echo Iniciando Frontend...
start cmd /k "cd /d %~dp0frontend && ng serve --open"

exit