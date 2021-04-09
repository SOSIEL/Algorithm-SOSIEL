@echo off
if "x%1" == "x" goto print_usage
if "%1" == "latest" goto latest_ver
set _ff=SOSIEL-v%1.zip
goto make_zip
:latest_ver
set _ff=SOSIEL-latest.zip
:make_zip
if exist %_ff%  del /F %_ff%
7z a -tzip -mx9 %_ff% *.dll *.pdb *.json *.LICENSE.txt *.LICENSE.md
goto end
:print_usage
echo %~nx0 VERSION (like 2.2.2)
:end
