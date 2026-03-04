#!/usr/bin/env bash
# Lint BuildMark
set -e  # Exit on error

echo "🎨 Checking code formatting..."
dotnet format --verify-no-changes

echo "📝 Checking markdown..."
npx markdownlint-cli2 "**/*.md"

echo "🔤 Checking spelling..."
npx cspell "**/*.{md,cs}"

echo "📋 Checking YAML..."
yamllint .

echo "✨ All linting passed!"
