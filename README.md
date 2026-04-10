# BuildMark

[![GitHub forks](https://img.shields.io/github/forks/demaconsulting/BuildMark?style=plastic)](https://github.com/demaconsulting/BuildMark/network/members)
[![GitHub stars](https://img.shields.io/github/stars/demaconsulting/BuildMark?style=plastic)](https://github.com/demaconsulting/BuildMark/stargazers)
[![GitHub contributors](https://img.shields.io/github/contributors/demaconsulting/BuildMark?style=plastic)](https://github.com/demaconsulting/BuildMark/graphs/contributors)
[![License](https://img.shields.io/github/license/demaconsulting/BuildMark?style=plastic)](https://github.com/demaconsulting/BuildMark/blob/main/LICENSE)
[![Build](https://img.shields.io/github/actions/workflow/status/demaconsulting/BuildMark/build_on_push.yaml)](https://github.com/demaconsulting/BuildMark/actions/workflows/build_on_push.yaml)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_BuildMark&metric=alert_status)](https://sonarcloud.io/dashboard?id=demaconsulting_BuildMark)
[![Security](https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_BuildMark&metric=security_rating)](https://sonarcloud.io/dashboard?id=demaconsulting_BuildMark)
[![NuGet](https://img.shields.io/nuget/v/DemaConsulting.BuildMark?style=plastic)](https://www.nuget.org/packages/DemaConsulting.BuildMark)

Markdown Build Notes Generation Tool

## Overview

BuildMark is a .NET command-line tool that generates comprehensive markdown build notes reports from Git repository
history and issue-tracking systems. It analyzes commits, pull requests, and issues to create human-readable build
notes, making it easy to integrate release documentation into your CI/CD pipelines and documentation workflows.

For a detailed explanation of how BuildMark works internally, see the
[Theory of Operations](https://github.com/demaconsulting/BuildMark/blob/main/THEORY-OF-OPERATIONS.md).

## Features

- 📄 **Git Integration** - Analyze Git repository history and tags
- 📝 **Markdown Reports** - Generate human-readable build notes from repository data
- 🐛 **Issue Tracking** - Extract bug fixes and changes from GitHub and Azure DevOps issues and pull requests
- 🎯 **Customizable Output** - Configure report depth and version ranges
- 🚀 **CI/CD Integration** - Automate build notes generation in your pipelines
- 🌐 **Multi-Platform** - Support for Windows, Linux, macOS with .NET 8, 9, and 10
- ✅ **Self-Validation** - Built-in tests without requiring external tools
- 📊 **Detailed Reporting** - Track changes, bug fixes, and known issues between versions

## Installation

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0, 9.0, or 10.0

### Global Installation

Install BuildMark as a global .NET tool for system-wide use:

```bash
dotnet tool install --global DemaConsulting.BuildMark
```

Verify the installation:

```bash
buildmark --version
```

### Local Installation

Install BuildMark as a local tool in your project (recommended for team projects):

```bash
dotnet new tool-manifest  # if you don't have a tool manifest already
dotnet tool install DemaConsulting.BuildMark
```

Run the tool:

```bash
dotnet buildmark --version
```

### Update

To update to the latest version:

```bash
# Global installation
dotnet tool update --global DemaConsulting.BuildMark

# Local installation
dotnet tool update DemaConsulting.BuildMark
```

### Compatibility

| Component  | Version | Status       |
|------------|---------|--------------|
| .NET SDK   | 8.0     | ✅ Supported |
| .NET SDK   | 9.0     | ✅ Supported |
| .NET SDK   | 10.0    | ✅ Supported |
| OS         | Windows | ✅ Supported |
| OS         | Linux   | ✅ Supported |
| OS         | macOS   | ✅ Supported |

## Usage

### Basic Usage

Run the tool with the `--help` option to see available commands and options:

```bash
buildmark --help
```

This will display:

```text
Usage: buildmark [options]

Options:
  -v, --version                Display version information
  -?, -h, --help               Display this help message
  --silent                     Suppress console output
  --validate                   Run self-validation
  --lint                       Validate .buildmark.yaml and exit
  --results <file>             Write validation results (TRX or JUnit format)
  --log <file>                 Write output to log file
  --build-version <version>    Specify the build version
  --report <file>              Specify the report file name
  --report-depth <depth>       Specify the report markdown depth (default: 1)
  --include-known-issues       Include known issues in the report
```

### Quick Start Examples

**Generate build notes for the current version:**

```bash
buildmark --build-version v1.2.3 --report build-notes.md
```

**Generate build notes with custom markdown depth:**

```bash
buildmark --build-version v1.2.3 --report build-notes.md --report-depth 2
```

**Include known issues in the report:**

```bash
buildmark --build-version v1.2.3 --report build-notes.md --include-known-issues
```

**Run self-validation:**

```bash
buildmark --validate
```

**Run self-validation with test results output:**

```bash
buildmark --validate --results validation-results.trx
```

## Configuration File

BuildMark can be configured via a `.buildmark.yaml` file placed in the repository root. This file
separates persistent repository settings from runtime arguments, simplifying CI invocations and
enabling version-controlled configuration.

The file has three top-level sections:

- **`connector`** — declares the repository connector type and per-connector settings such as URL
  overrides and repository identifiers. Current releases support the `github` and `azure-devops`
  connectors.
- **`sections`** — defines the ordered list of sections that will appear in the generated build
  notes, each identified by an `id` and a `title`.
- **`rules`** — an ordered list of match/route rules. Each rule can match on `label` and/or
  `work-item-type`, and routes matched items to a named section or to `suppressed` to exclude them.
  Rules are evaluated in order and the first match wins. A rule with no `match` key is a catch-all.

Example `.buildmark.yaml` for GitHub:

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

Example `.buildmark.yaml` for Azure DevOps:

```yaml
# Repository Connector Settings
connector:
  type: azure-devops

  # Azure DevOps settings
  azure-devops:
    organization-url: https://dev.azure.com/myorg
    project: MyProject
    repository: MyRepo

# Build Notes sections
sections:
  - id: changes
    title: Changes
  - id: bugs-fixed
    title: Bugs Fixed

# Item routing rules
rules:
  # Bug work-items get routed to the 'bugs-fixed' section
  - match:
      work-item-type: [Bug]
    route: bugs-fixed

  # Task and Epic work-items get suppressed
  - match:
      work-item-type: [Task, Epic]
    route: suppressed

  # Everything else gets routed to the 'changes' section
  - route: changes
```

### Authentication

Authentication tokens are not configured in `.buildmark.yaml`. BuildMark resolves them automatically
from environment variables at runtime.

**GitHub**: resolves a token from `GH_TOKEN`, then `GITHUB_TOKEN`, then `gh auth token`.

**Azure DevOps**: resolves a token from `AZURE_DEVOPS_PAT`, then `AZURE_DEVOPS_TOKEN`, then
`AZURE_DEVOPS_EXT_PAT`, then `SYSTEM_ACCESSTOKEN` (Azure Pipelines), then
`az account get-access-token` (Azure CLI).

For more detail see the [User Guide](https://github.com/demaconsulting/BuildMark/blob/main/docs/user_guide/introduction.md).

## Self Validation

Running self-validation produces a report containing the following information:

```text
# DEMA Consulting BuildMark Self-validation

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| BuildMark Version   | <version>                                          |
| Machine Name        | <machine-name>                                     |
| OS Version          | <os-version>                                       |
| DotNet Runtime      | <dotnet-runtime-version>                           |
| Time Stamp          | <timestamp> UTC                                    |

✓ BuildMark_MarkdownReportGeneration - Passed
✓ BuildMark_GitIntegration - Passed
✓ BuildMark_IssueTracking - Passed
✓ BuildMark_KnownIssuesReporting - Passed
✓ BuildMark_RulesRouting - Passed

Total Tests: 5
Passed: 5
Failed: 0
```

Each test in the report proves:

- **`BuildMark_MarkdownReportGeneration`** - Markdown report is correctly generated from mock data.
- **`BuildMark_GitIntegration`** - Git repository connector reads version tags and commits.
- **`BuildMark_IssueTracking`** - GitHub issue and pull request tracking works correctly.
- **`BuildMark_KnownIssuesReporting`** - Known issues are correctly included when requested.
- **`BuildMark_RulesRouting`** - Rules-based item routing assigns items to the correct report sections.

See the [User Guide](https://github.com/demaconsulting/BuildMark/blob/main/docs/user_guide/introduction.md) for more details
on the self-validation tests.

On validation failure the tool will exit with a non-zero exit code.

## Extended Item Controls

BuildMark supports an optional `buildmark` code block in GitHub issue and pull request descriptions
to control how each item appears in the generated build notes.

### BuildMark Code Block

Add a fenced `buildmark` block anywhere in an issue or PR description:

````markdown
```buildmark
visibility: public
type: feature
affected-versions: (,1.0.1],[1.1.0,1.2.0)
```
````

The block can be hidden using HTML comment syntax if desired:

````markdown
<!--
```buildmark
visibility: internal
```
-->
````

### Visibility Control

The `visibility` field overrides the default visibility rules:

- `public` - Force-include the item in build notes regardless of other settings
- `internal` - Force-exclude the item from build notes regardless of labels or tags

When `visibility` is not specified, the default rules apply: merged pull requests and linked
issues are included in build notes. Standard issue labels (`bug`, `defect`, `feature`, etc.)
are used to classify entries, but unlabeled or non-standard-labeled items may still be included.

### Type Override

The `type` field overrides the item classification used in build notes:

- `bug` - Classify the item as a bug fix
- `feature` - Classify the item as a new feature or change

When `type` is not specified, BuildMark infers the type from GitHub issue labels.

### Affected Versions

The `affected-versions` field encodes which versions are affected by the change,
using mathematical interval notation with comma-separated ranges:

```text
affected-versions: (,1.0.1],[1.1.0,1.2.0),(1.2.5,2.0.0],[3.0.0,)
```

- `[` / `]` — inclusive bound
- `(` / `)` — exclusive bound
- Empty lower bound — no minimum version (e.g., `(,1.0.1]` means up to and including `1.0.1`)
- Empty upper bound — no maximum version (e.g., `[3.0.0,)` means `3.0.0` and later)

## Report Format

The generated markdown report includes:

1. **Build Report Header** - Title with version information
2. **Version Information** - Current version, baseline version, and commit details
3. **Changes** - List of non-bug changes implemented in this build
4. **Bugs Fixed** - List of bugs resolved in this build
5. **Known Issues** - Optional list of known issues (when `--include-known-issues` is specified)
6. **Complete Changelog** - Link to the full changelog on GitHub (when available)

Example report structure:

```markdown
# Build Report

## Version Information

**Version:** 1.2.3
**Baseline Version:** 1.2.0
**Commit:** abc123def456

## Changes

- [#42](https://github.com/owner/repo/pull/42): Add new feature X
- [#43](https://github.com/owner/repo/pull/43): Improve performance of Y

## Bugs Fixed

- [#40](https://github.com/owner/repo/issues/40): Fix crash when Z is null
- [#41](https://github.com/owner/repo/issues/41): Correct validation logic

## Complete Changelog

[View Full Changelog](https://github.com/owner/repo/compare/v1.2.0...v1.2.3)
```

## Project Practices

The BuildMark repository itself follows these development practices:

- 🔍 **Linting Enforcement** - markdownlint, cspell, and yamllint enforced on every CI run
- 📋 **Continuous Compliance** - Compliance evidence generated automatically on every CI run,
  following the [Continuous Compliance](https://github.com/demaconsulting/ContinuousCompliance) methodology
- ☁️ **SonarCloud Integration** - Quality gate and security analysis on every build
- 🔗 **Requirements Traceability** - Requirements linked to passing tests with auto-generated trace matrix

## Contributing

Contributions are welcome! We appreciate your interest in improving BuildMark.

Please see our [Contributing Guide](https://github.com/demaconsulting/BuildMark/blob/main/CONTRIBUTING.md) for
development setup, coding standards, and submission guidelines. Also review our
[Code of Conduct](https://github.com/demaconsulting/BuildMark/blob/main/CODE_OF_CONDUCT.md) for community guidelines.

For bug reports, feature requests, and questions, please use
[GitHub Issues](https://github.com/demaconsulting/BuildMark/issues).

## License

This project is licensed under the MIT License - see the
[LICENSE](https://github.com/demaconsulting/BuildMark/blob/main/LICENSE) file for details.

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

## Support

- 🐛 **Report Bugs**: [GitHub Issues](https://github.com/demaconsulting/BuildMark/issues)
- 💡 **Request Features**: [GitHub Issues](https://github.com/demaconsulting/BuildMark/issues)
- ❓ **Ask Questions**: [GitHub Discussions](https://github.com/demaconsulting/BuildMark/discussions)
- 🤝 **Contributing**: [Contributing Guide](https://github.com/demaconsulting/BuildMark/blob/main/CONTRIBUTING.md)

## Security

For security concerns and vulnerability reporting, please see our [Security Policy](https://github.com/demaconsulting/BuildMark/blob/main/SECURITY.md).

## Acknowledgements

BuildMark is built with the following open-source projects:

- [.NET](https://dotnet.microsoft.com/) - Cross-platform framework for building applications
- [Octokit](https://github.com/octokit/octokit.net) - GitHub API client library for .NET
