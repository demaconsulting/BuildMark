#!/bin/bash
set -e

echo "🔧 Building BuildMark..."
dotnet build --configuration Release

echo "🧪 Running unit tests..."
dotnet test --configuration Release

echo "✅ Running self-validation..."
dotnet run --project src/DemaConsulting.BuildMark --configuration Release --framework net10.0 --no-build -- --validate

echo "✨ Build, tests, and validation completed successfully!"
