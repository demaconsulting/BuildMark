@echo off
setlocal

REM Run markdown linter
call npx markdownlint-cli2 "**/*.md"
if errorlevel 1 exit /b 1

REM Run spell checker
call npx cspell "**/*.{md,cs}"
if errorlevel 1 exit /b 1

REM Run YAML linter
call yamllint .
if errorlevel 1 exit /b 1

echo Linting completed successfully!
