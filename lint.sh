#!/bin/bash
set -e

# Run markdown linter
npx markdownlint-cli2 "**/*.md"

# Run spell checker
npx cspell "**/*.{md,cs}"

# Run YAML linter
yamllint .
