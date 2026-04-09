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

# Continuous Compliance

BuildMark follows the [Continuous Compliance][continuous-compliance] methodology, which ensures
compliance evidence is generated automatically on every CI run.

## Key Practices

- **Requirements Traceability**: Every requirement is linked to passing tests, and a trace matrix is
  auto-generated on each release
- **Linting Enforcement**: markdownlint, cspell, and yamllint are enforced before any build proceeds
- **Automated Audit Documentation**: Each release ships with generated requirements, justifications,
  trace matrix, and quality reports
- **CodeQL and SonarCloud**: Security and quality analysis runs on every build

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

# Configuration File

BuildMark can be configured with a `.buildmark.yaml` file placed in the repository root. This file
separates persistent repository settings from runtime arguments, simplifying CI invocations and
enabling version-controlled configuration.

Example `.buildmark.yaml`:

```yaml
# Repository Connector Settings
connector:
  # Type of repository
  type: github

  # GitHub settings
  github:
    url: https://github.mycompany.com   # optional; defaults to https://api.github.com
    repository: owner/repo

# Build Notes sections
sections:
  - id: changes
    title: Changes
  - id: bugs-fixed
    title: Bugs Fixed
  - id: dependency-updates
    title: Dependency Updates

# Item routing rules
rules:
  # Labels of 'dependencies', 'renovate', or 'dependabot' get routed to the 'dependency-updates' section
  - match:
      label: [dependencies, renovate, dependabot]
    route: dependency-updates

  # Bug work-items get routed to the 'bugs-fixed' section
  - match:
      work-item-type: [Bug]
    route: bugs-fixed

  # Labels of 'bug', 'defect', or 'regression' get routed to the 'bugs-fixed' section
  - match:
      label: [bug, defect, regression]
    route: bugs-fixed

  # Labels of 'internal' or 'chore' get suppressed
  - match:
      label: [internal, chore]
    route: suppressed

  # Task and Epic work-items get suppressed
  - match:
      work-item-type: [Task, Epic]
    route: suppressed

  # Everything else gets routed to the 'changes' section
  - route: changes
```

## Connector Settings

The `connector` section declares how BuildMark connects to source-control and work-item systems.

The `type` key currently supports only the GitHub connector. Azure DevOps values are reserved for
future support and are not yet implemented.

| Value | Description |
| :---- | :---------- |
| `github` | GitHub only |

### GitHub connector settings

BuildMark currently resolves the GitHub access token automatically from `GH_TOKEN`, then
`GITHUB_TOKEN`, then `gh auth token`.

| Key | Required | Description |
| :-- | :------- | :---------- |
| `url` | No | Base URL of the GitHub instance. Defaults to `https://api.github.com`. |
| `repository` | Yes | Repository in `owner/repo` format. |

## Report Sections

The `sections` sequence defines which sections appear in the generated build notes and in what
order. Each entry has two keys:

| Key | Description |
| :-- | :---------- |
| `id` | Unique identifier used to reference this section in routing rules. |
| `title` | Human-readable heading that appears in the generated report. |

Sections are rendered in the order they are listed. Any section that receives no items is omitted
from the output.

## Item Routing Rules

The `rules` sequence controls how individual work items are categorized into report sections.
Rules are evaluated in order and the **first matching rule wins**.

Each rule may contain:

| Key | Description |
| :-- | :---------- |
| `match` | Optional. Criteria to test against each item (see below). |
| `route` | Required. The section `id` to place matched items in, or `suppressed` to exclude them. |

### Match criteria

| Criterion | Description |
| :-------- | :---------- |
| `label` | A label name or list of label names. Matches if the item carries any of the listed labels. |
| `work-item-type` | A work-item type name or list of names (e.g., `Bug`, `Task`, `Epic`). |

Multiple criteria within a single `match` block are combined with AND logic — the item must satisfy
all specified criteria to match that rule.

A rule with no `match` key is a **catch-all** and matches every item that has not already been
routed by an earlier rule. Place the catch-all last to act as a default.

### The `suppressed` route

Setting `route: suppressed` excludes matched items from the report entirely. Use this to hide
internal tasks, dependency-update noise, or any other items that should not appear in the published
build notes.

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

Self-validation produces a report demonstrating that BuildMark is functioning correctly. This is useful in
regulated industries where tool validation evidence is required.

### Running Validation

To perform self-validation:

```bash
buildmark --validate
```

To save validation results to a file:

```bash
buildmark --validate --results results.trx
```

The results file format is determined by the file extension: `.trx` for TRX (MSTest) format,
or `.xml` for JUnit format.

### Validation Report

The validation report contains the tool version, machine name, operating system version,
.NET runtime version, timestamp, and test results.

Example validation report:

```text
# DEMA Consulting BuildMark Self-validation

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| BuildMark Version   | 1.0.0                                              |
| Machine Name        | BUILD-SERVER                                       |
| OS Version          | Ubuntu 22.04.3 LTS                                 |
| DotNet Runtime      | .NET 10.0.0                                        |
| Time Stamp          | 2024-01-15 10:30:00 UTC                            |

✓ BuildMark_MarkdownReportGeneration - Passed
✓ BuildMark_GitIntegration - Passed
✓ BuildMark_IssueTracking - Passed
✓ BuildMark_KnownIssuesReporting - Passed

Total Tests: 4
Passed: 4
Failed: 0
```

### Validation Tests

Each test proves specific functionality works correctly:

- **`BuildMark_MarkdownReportGeneration`** - Markdown report is correctly generated from mock data.
- **`BuildMark_GitIntegration`** - Git repository connector reads version tags and commits.
- **`BuildMark_IssueTracking`** - GitHub issue and pull request tracking works correctly.
- **`BuildMark_KnownIssuesReporting`** - Known issues are correctly included when requested.

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

# Extended Item Controls

BuildMark supports an optional `buildmark` code block embedded in GitHub issue and pull request
descriptions. This block gives developers fine-grained control over visibility, type classification,
and affected-version ranges without requiring custom labels or special GitHub fields.

## BuildMark Code Block Format

Add a fenced `buildmark` block to the issue or PR description:

````markdown
```buildmark
visibility: public
type: feature
affected-versions: (,1.0.1],[1.1.0,1.2.0)
```
````

All fields are optional. Unrecognized fields are ignored.

The block can be concealed inside an HTML comment if you do not want it rendered in the
GitHub UI:

````markdown
<!--
```buildmark
visibility: internal
```
-->
````

## Visibility Field

The `visibility` field explicitly overrides the default visibility rules for an item.

| Value | Behavior |
| :---- | :-------- |
| `public` | Force-include the item in build notes, overriding any default exclusion. |
| `internal` | Force-exclude the item from build notes, overriding any default inclusion. |

When the `visibility` field is absent, BuildMark applies its default rules: merged pull
requests and linked issues are included in generated build notes by default. Standard GitHub
issue labels (`bug`, `defect`, `feature`, and similar) are used to classify entries such as
bug fixes versus changes, but unlabeled or non-standard-labeled items may still be included.

### Visibility Examples

Include an item that would otherwise be excluded by default:

```yaml
visibility: public
```

Suppress an item even though it carries a `bug` label:

```yaml
visibility: internal
```

## Type Field

The `type` field overrides the item classification that BuildMark uses when placing the item
into the report.

| Value | Report Section |
| :---- | :------------- |
| `bug` | **Bugs Fixed** section |
| `feature` | **Changes** section |

When `type` is absent, BuildMark infers the type from the GitHub issue or PR labels.

### Type Example

Override a pull request so it appears in the **Bugs Fixed** section regardless of its labels:

```yaml
type: bug
```

## Affected Versions Field

The `affected-versions` field records which software versions are affected by the change
using mathematical interval notation. Multiple intervals are separated by commas.

```text
affected-versions: (,1.0.1],[1.1.0,1.2.0),(1.2.5,2.0.0],[3.0.0,)
```

### Interval Syntax

| Symbol | Meaning |
| :----- | :------ |
| `[` | Inclusive lower bound |
| `(` | Exclusive lower bound |
| `]` | Inclusive upper bound |
| `)` | Exclusive upper bound |
| _(empty)_ lower bound | No minimum — all versions from the beginning |
| _(empty)_ upper bound | No maximum — all versions from the lower bound onward |

### Affected Version Examples

| Expression | Meaning |
| :--------- | :------ |
| `(,1.0.1]` | All versions up to and including `1.0.1` |
| `[1.1.0,1.2.0)` | From `1.1.0` up to (but not including) `1.2.0` |
| `(1.2.5,2.0.0]` | After `1.2.5` up to and including `2.0.0` |
| `[3.0.0,)` | `3.0.0` and all later versions |

### Multiple Ranges

Combine ranges with commas to express disjoint sets of affected versions:

```text
affected-versions: (,1.0.1],[1.1.0,1.2.0)
```

This matches all versions up to and including `1.0.1`, and also versions from `1.1.0` up to
(but not including) `1.2.0`.

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
[continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
