@echo off
REM Lint BuildMark (Windows)
setlocal

echo Checking code formatting...
dotnet format --verify-no-changes
if %errorlevel% neq 0 exit /b %errorlevel%

echo Checking markdown...
call npx markdownlint-cli2 "**/*.md"
if %errorlevel% neq 0 exit /b %errorlevel%

echo Checking spelling...
call npx cspell "**/*.{md,cs}"
if %errorlevel% neq 0 exit /b %errorlevel%

echo Checking YAML...
call yamllint .
if %errorlevel% neq 0 exit /b %errorlevel%

echo All linting passed!
