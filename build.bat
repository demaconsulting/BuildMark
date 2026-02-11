@echo off
setlocal

REM Restore dependencies
dotnet restore
if errorlevel 1 exit /b 1

REM Build the project
dotnet build --configuration Release
if errorlevel 1 exit /b 1

REM Run tests
dotnet test --configuration Release
if errorlevel 1 exit /b 1

echo Build completed successfully!
