# build.ps1
#
# PURPOSE:
#   Unified cross-platform build script for BuildMark.
#   Restores dependencies, builds the solution in Release configuration, and runs all unit tests.
#
# EXTENSION POINTS:
#   Search for "[PROJECT-SPECIFIC]" comments to find the designated locations
#   for adding project-specific build or test operations.
#
# MODIFICATION POLICY:
#   Only modify this file to add project-specific operations at the designated
#   [PROJECT-SPECIFIC] extension points.

$buildError = $false

Write-Host "Restoring dependencies..."
dotnet restore
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Building..."
dotnet build --no-restore --configuration Release
if ($LASTEXITCODE -ne 0) { $buildError = $true }

# [PROJECT-SPECIFIC] Add additional build steps here.

Write-Host "Running unit tests..."
dotnet test --no-build --configuration Release --logger trx --results-directory artifacts/tests
if ($LASTEXITCODE -ne 0) { $buildError = $true }

# [PROJECT-SPECIFIC] Add additional test or post-build steps here.

if (-not $buildError) {
    Write-Host "Build and tests completed successfully!"
}

exit ($buildError ? 1 : 0)
