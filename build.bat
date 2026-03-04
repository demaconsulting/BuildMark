@echo off
REM Build and test BuildMark (Windows)
setlocal

echo Building BuildMark...
dotnet build --configuration Release
if %errorlevel% neq 0 exit /b %errorlevel%

echo Running unit tests...
dotnet test --configuration Release
if %errorlevel% neq 0 exit /b %errorlevel%

echo Running self-validation...
dotnet run --project src/DemaConsulting.BuildMark --configuration Release --framework net10.0 --no-build -- --validate
if %errorlevel% neq 0 exit /b %errorlevel%

echo Build, tests, and validation completed successfully!
