#!/bin/bash
set -e

# Restore dependencies
dotnet restore

# Build the project
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release
