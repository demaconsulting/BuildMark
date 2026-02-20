<!-- markdownlint-disable MD025 -->

# Introduction

BuildMark is a .NET command-line tool that generates comprehensive markdown build notes reports from Git repository
history and GitHub issues. It analyzes commits, pull requests, and issues to create human-readable build notes,
making it easy to integrate release documentation into your CI/CD pipelines and documentation workflows.

## Key Features

- **Git Integration**: Analyze Git repository history and tags
- **GitHub Integration**: Extract bug fixes and changes from GitHub issues and pull requests
- **Markdown Reports**: Generate human-readable build notes from repository data
- **Customizable Output**: Configure report depth and version ranges
- **CI/CD Integration**: Automate build notes generation in your pipelines
- **Multi-Platform**: Works on Windows, Linux, and macOS with .NET 8, 9, or 10
- **Self-Validation**: Built-in tests without requiring external tools
- **Detailed Reporting**: Track changes, bug fixes, and known issues between versions

# Installation

## Prerequisites

- [.NET SDK][dotnet-download] 8.0, 9.0, or 10.0

## Global Installation

Install BuildMark as a global .NET tool for system-wide access:

```bash
dotnet tool install --global DemaConsulting.BuildMark
```

Verify the installation:

```bash
buildmark --version
```

## Local Installation

For team projects, install BuildMark as a local tool to ensure version consistency:

```bash
# Create tool manifest if it doesn't exist
dotnet new tool-manifest

# Install the tool
dotnet tool install DemaConsulting.BuildMark
```

Run the locally installed tool:

```bash
dotnet buildmark --version
```

## Update

To update to the latest version:

```bash
# Global installation
dotnet tool update --global DemaConsulting.BuildMark

# Local installation
dotnet tool update DemaConsulting.BuildMark
```

# Getting Started

## Basic Usage

The most basic usage requires specifying a build version and report file:

```bash
buildmark --build-version v1.2.3 --report build-notes.md
```

This will analyze the Git repository in the current directory, find the previous version tag, and generate a
markdown report with all changes, bug fixes, and other relevant information.

## With GitHub Token

For accessing private repositories or to avoid GitHub API rate limits, provide a GitHub token:

```bash
# Using environment variable
export GH_TOKEN=ghp_abc123...
buildmark --build-version v1.2.3 --report build-notes.md

# Or using GITHUB_TOKEN
export GITHUB_TOKEN=ghp_abc123...
buildmark --build-version v1.2.3 --report build-notes.md
```

## Including Known Issues

To include known issues in the report:

```bash
buildmark --build-version v1.2.3 --report build-notes.md --include-known-issues
```

# Command-Line Options

## Display Options

### `--version`, `-v`

Display version information and exit.

```bash
buildmark --version
```

### `--help`, `-h`, `-?`

Display help message with all available options.

```bash
buildmark --help
```

## Output Control

### `--silent`

Suppress console output. Useful in automated scripts where only the exit code matters.

```bash
buildmark --build-version v1.2.3 --report build-notes.md --silent
```

### `--log <file>`

Write all output to a log file in addition to console output.

```bash
buildmark --build-version v1.2.3 --report build-notes.md --log analysis.log
```

## Build Version Options

### `--build-version <version>` (Required for report generation)

Specify the build version for which to generate the report. This should be a version tag in your Git repository
(e.g., `v1.2.3`, `1.2.3`).

```bash
buildmark --build-version v1.2.3 --report build-notes.md
```

BuildMark will automatically find the previous version tag and generate a report covering all changes between
the two versions.

## Report Generation

### `--report <file>`

Export build notes to a markdown file. The file will contain version information, changes, bug fixes, and
optionally known issues.

```bash
buildmark --build-version v1.2.3 --report build-notes.md
```

### `--report-depth <depth>`

Set the markdown header depth for the report. Default is 1. Use this when embedding the report in larger documents.

```bash
# Use level 2 headers (##) instead of level 1 (#)
buildmark --build-version v1.2.3 --report build-notes.md --report-depth 2
```

### `--include-known-issues`

Include known issues in the generated report. Known issues are open bugs in the GitHub repository.

```bash
buildmark --build-version v1.2.3 --report build-notes.md --include-known-issues
```

## Self-Validation

### `--validate`

Run built-in self-validation tests. These tests verify BuildMark functionality without requiring access to a real
Git repository or GitHub.

```bash
buildmark --validate
```

### `--results <file>`

Write validation results to a file. Supports TRX (`.trx`) and JUnit XML (`.xml`) formats. Requires `--validate`.

```bash
# TRX format
buildmark --validate --results validation-results.trx

# JUnit XML format
buildmark --validate --results validation-results.xml
```

# Common Use Cases

## CI/CD Integration

Integrate BuildMark into your CI/CD pipeline to automatically generate build notes:

```yaml
# GitHub Actions example
- name: Generate Build Notes
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
    buildmark \
      --build-version ${{ inputs.version }} \
      --report docs/build-notes.md

- name: Upload Build Notes
  uses: actions/upload-artifact@v6
  with:
    name: build-notes
    path: docs/build-notes.md
```

## Release Documentation

Generate build notes for a specific release:

```bash
# Generate build notes for version 2.0.0
buildmark --build-version v2.0.0 --report release-notes-2.0.0.md --include-known-issues
```

## Integration Testing

Run self-validation tests in your CI/CD pipeline:

```bash
buildmark --validate --results validation-results.trx
```

## Automated Reporting

Generate timestamped build notes for archival purposes:

```bash
#!/bin/bash
# Generate timestamped build notes
VERSION=$(git describe --tags --abbrev=0)
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
buildmark --build-version "$VERSION" \
  --report "build-notes-${TIMESTAMP}.md" \
  --log "analysis-${TIMESTAMP}.log"
```

# Report Format

The generated markdown report includes the following sections:

## Build Report Header

The report begins with a title showing the version:

```markdown
# Build Report
```

## Version Information

Shows the current version, baseline version (previous version), and commit information:

```markdown
## Version Information

**Version:** 1.2.3
**Baseline Version:** 1.2.0
**Commit:** abc123def456
**Commit Date:** 2024-01-15
```

## Changes

Lists all non-bug changes implemented in this build, extracted from GitHub pull requests and issues:

```markdown
## Changes

- [#42](https://github.com/owner/repo/pull/42): Add new feature X
- [#43](https://github.com/owner/repo/pull/43): Improve performance of Y
- [#44](https://github.com/owner/repo/issues/44): Update documentation
```

Each change entry includes:

- **Issue/PR number**: Linked to the GitHub issue or pull request
- **Description**: Title of the issue or pull request

## Bugs Fixed

Lists all bugs resolved in this build, extracted from GitHub issues labeled as bugs:

```markdown
## Bugs Fixed

- [#40](https://github.com/owner/repo/issues/40): Fix crash when Z is null
- [#41](https://github.com/owner/repo/issues/41): Correct validation logic
```

## Known Issues

When `--include-known-issues` is specified, lists currently open bugs:

```markdown
## Known Issues

- [#50](https://github.com/owner/repo/issues/50): Performance degradation on large datasets
- [#51](https://github.com/owner/repo/issues/51): UI glitch in dark mode
```

## Complete Changelog

Provides a link to the full changelog on GitHub comparing the baseline and current versions:

```markdown
## Complete Changelog

[View Full Changelog](https://github.com/owner/repo/compare/v1.2.0...v1.2.3)
```

# Running Self-Validation

BuildMark includes built-in self-validation tests to verify functionality without requiring access to a real
Git repository or GitHub. The validation uses mock data to test core features.

## Running Validation

```bash
buildmark --validate
```

## Validation Tests

The self-validation suite includes the following tests that verify core functionality:

| Test Name | Description |
| :-------- | :---------- |
| `BuildMark_MarkdownReportGeneration` | Verifies generating markdown build notes reports |
| `BuildMark_GitIntegration` | Verifies git integration for version and commit information |
| `BuildMark_IssueTracking` | Verifies issue tracking for changes and bugs |
| `BuildMark_KnownIssuesReporting` | Verifies known issues reporting functionality |

These tests provide evidence of the tool's functionality and are particularly useful for:

- Verifying the installation is working correctly on different platforms and .NET versions
- Running automated tests in CI/CD pipelines without requiring repository access
- Generating test evidence for compliance and traceability requirements
- Validating tool functionality before deployment

**Note**: The test names with the `BuildMark_` prefix are designed for clear identification in test
result files (TRX/JUnit) when integrating with larger projects or test frameworks.

## Validation Output

Example output:

```text
BuildMark version 1.0.0
Copyright (c) DEMA Consulting

# DEMA Consulting BuildMark Self-validation

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| BuildMark Version   | 1.0.0                                              |
| Machine Name        | my-machine                                         |
| OS Version          | Ubuntu 24.04.3 LTS                                 |
| DotNet Runtime      | .NET 10.0.3                                        |
| Time Stamp          | 2026-01-15 10:30:00 UTC                              |

✓ Markdown Report Generation Test - PASSED
✓ Git Integration Test - PASSED
✓ Issue Tracking Test - PASSED
✓ Known Issues Reporting Test - PASSED

Total Tests: 4
Passed: 4
Failed: 0
```

## Saving Validation Results

Save results in TRX or JUnit XML format for integration with test reporting tools:

```bash
# TRX format (for Azure DevOps, Visual Studio)
buildmark --validate --results validation-results.trx

# JUnit XML format (for Jenkins, GitLab CI)
buildmark --validate --results validation-results.xml
```

# Version Selection Rules

BuildMark automatically determines which previous version to use as the baseline when generating build notes. This
section explains how BuildMark selects the baseline version for different scenarios.

## Pre-Release Versions

For pre-release versions (e.g., `1.2.3-beta.1`, `1.2.3-rc.1`), BuildMark picks the **previous tag (release or
pre-release) that has a different commit hash**.

This behavior handles cases where multiple pre-release tags point to the same commit (re-tagging scenarios), ensuring
the generated changelog shows actual code changes rather than an empty diff.

### Example: Pre-Release with Re-Tagged Commits

Consider the following tags:

- `1.1.2-rc.1` (commit hash: `a1b2c3d4`)
- `1.1.2-beta.2` (commit hash: `a1b2c3d4`)
- `1.1.2-beta.1` (commit hash: `734713bc`)

When generating build notes for `1.1.2-rc.1`:

1. BuildMark identifies that `1.1.2-beta.2` has the same commit hash (`a1b2c3d4`)
2. BuildMark skips `1.1.2-beta.2` since it would result in an empty changelog
3. BuildMark selects `1.1.2-beta.1` as the baseline (different commit hash: `734713bc`)

The generated build notes will show changes between `1.1.2-beta.1` and `1.1.2-rc.1`.

## Release Versions

For release versions (e.g., `1.2.3`), BuildMark picks the **previous release tag**, skipping all pre-release versions.

This ensures release notes compare against the previous stable release, showing the complete set of changes since the
last production release.

### Example: Release Skipping Pre-Releases

Consider the following tags:

- `1.1.2` (release)
- `1.1.2-rc.1` (pre-release)
- `1.1.2-beta.2` (pre-release)
- `1.1.2-beta.1` (pre-release)
- `1.1.1` (release)

When generating build notes for `1.1.2`:

1. BuildMark identifies `1.1.2` as a release version (no pre-release suffix)
2. BuildMark skips all pre-release tags (`1.1.2-rc.1`, `1.1.2-beta.2`, `1.1.2-beta.1`)
3. BuildMark selects `1.1.1` as the baseline (the previous release)

The generated build notes will show all changes between `1.1.1` and `1.1.2`, including changes from all the
pre-release versions.

## No Previous Version

If no previous version is found (e.g., generating build notes for the first release), BuildMark will build the
history from the beginning of the repository, showing all commits up to the specified version.

## Version Tag Format

BuildMark recognizes version tags with various formats:

- Simple format: `1.2.3`
- V-prefix: `v1.2.3`
- Custom prefixes: `ver-1.2.3`, `release_1.2.3`
- Pre-release suffixes: `-alpha.1`, `-beta.2`, `-rc.1`, `.pre.1`
- Build metadata: `+build.123`, `+linux.x64`

Examples of recognized version tags:

- `1.0.0`, `v1.0.0`, `ver-1.0.0`
- `2.0.0-beta.1`, `v2.0.0-rc.2`
- `1.2.3+build.456`, `v2.0.0-rc.1+linux`

# Best Practices

## Version Tagging

- **Use semantic versioning**: Follow the `vX.Y.Z` format for version tags
- **Tag releases consistently**: Ensure all releases are tagged in Git
- **Use annotated tags**: Create annotated tags with `git tag -a` for better metadata

## GitHub Integration

- **Store tokens securely**: Use environment variables or secret management systems
- **Use read-only tokens**: BuildMark only needs read access to the GitHub API
- **Don't commit tokens**: Never commit tokens to version control
- **Set appropriate rate limits**: Be aware of GitHub API rate limits

## CI/CD Best Practices

- **Generate on every release**: Automate build notes generation for every release
- **Archive reports**: Save build notes as build artifacts for historical tracking
- **Use silent mode**: Suppress unnecessary output in automated scripts with `--silent`
- **Handle failures gracefully**: Use appropriate error handling in your CI/CD scripts

## Report Best Practices

- **Use meaningful filenames**: Include version numbers or timestamps in report filenames
- **Adjust header depth**: Use `--report-depth` when embedding reports in larger documents
- **Include known issues**: Use `--include-known-issues` for comprehensive release documentation
- **Combine with logging**: Use `--log` to capture detailed execution information

# Troubleshooting

## Git Repository Issues

**Problem**: Cannot find Git repository

**Solutions**:

- Verify you're running BuildMark from within a Git repository
- Ensure the `.git` directory exists in the current or parent directories
- Check file permissions on the Git repository

## Version Tag Issues

**Problem**: Cannot find version tags

**Solutions**:

- Verify version tags exist in the Git repository with `git tag`
- Ensure tags follow a recognizable version format (e.g., `v1.2.3` or `1.2.3`)
- Check if the specified build version exists as a tag

## GitHub API Issues

**Problem**: GitHub API rate limit exceeded or authentication failures

**Solutions**:

- Set the `GH_TOKEN` or `GITHUB_TOKEN` environment variable with a valid GitHub personal access token
- Verify the token has appropriate permissions (read access to repositories)
- Wait for the rate limit to reset (typically one hour)
- Use a GitHub token to increase rate limits from 60 to 5000 requests per hour

## Report Generation Issues

**Problem**: Report file is not generated or is empty

**Solutions**:

- Check file permissions in the output directory
- Verify the output path is valid and accessible
- Ensure there's enough disk space
- Check the log output for specific error messages

## Validation Failures

**Problem**: Self-validation tests fail

**Solutions**:

- Update to the latest version of BuildMark
- Check if there are any known issues in the GitHub repository
- Report the issue with full validation output if problem persists

## Exit Codes

BuildMark uses the following exit codes:

- `0`: Success
- `1`: Error occurred

Use these exit codes in scripts for error handling:

```bash
#!/bin/bash
if buildmark --build-version v1.2.3 --report build-notes.md; then
  echo "Build notes generated successfully!"
else
  echo "Build notes generation failed!"
  exit 1
fi
```

# Additional Resources

- [GitHub Repository][github]
- [Issue Tracker][issues]
- [Security Policy][security]
- [Contributing Guide][contributing]
- [NuGet Package][nuget]

[dotnet-download]: https://dotnet.microsoft.com/download
[github]: https://github.com/demaconsulting/BuildMark
[issues]: https://github.com/demaconsulting/BuildMark/issues
[security]: https://github.com/demaconsulting/BuildMark/blob/main/SECURITY.md
[contributing]: https://github.com/demaconsulting/BuildMark/blob/main/CONTRIBUTING.md
[nuget]: https://www.nuget.org/packages/DemaConsulting.BuildMark
