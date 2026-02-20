@echo off
setlocal

echo Building BuildMark...
dotnet build --configuration Release
if errorlevel 1 exit /b 1

echo Running unit tests...
dotnet test --configuration Release
if errorlevel 1 exit /b 1

echo Running self-validation...
dotnet run --project src/DemaConsulting.BuildMark --configuration Release --framework net10.0 --no-build -- --validate
if errorlevel 1 exit /b 1

echo Build, tests, and validation completed successfully!
