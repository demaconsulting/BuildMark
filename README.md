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

For detailed documentation, see the [User Guide](https://github.com/demaconsulting/BuildMark/blob/main/docs/user_guide/introduction.md).
For command-line options, see the [CLI Reference](https://github.com/demaconsulting/BuildMark/blob/main/docs/user_guide/cli-reference.md).

## Features

- 📄 **Git Integration** — Analyze repository history, tags, and branches
- 📝 **Markdown Reports** — Generate structured build notes
- 🐛 **Issue Tracking** — Pull changes and bugs from GitHub and Azure DevOps
- 🔀 **Configurable Routing** — Route items to sections by label or type
- 🎯 **Customizable Output** — Control report depth, sections, and content
- 🚀 **CI/CD Ready** — Integrate with GitHub Actions and Azure Pipelines
- 🌐 **Multi-Platform** — Windows, Linux, and macOS on .NET 8, 9, and 10
- ✅ **Self-Validation** — Built-in qualification tests
- 📊 **Dependency Updates** — Track changes from Dependabot and Renovate

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
  --depth <depth>              Specify the markdown heading depth (default: 1)
  --include-known-issues       Include known issues in the report
```

### Quick Start Examples

**Generate build notes for the current version:**

```bash
buildmark --build-version v1.2.3 --report build-notes.md
```

**Generate build notes with custom markdown depth:**

```bash
buildmark --build-version v1.2.3 --report build-notes.md --depth 2
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

BuildMark can be configured via a `.buildmark.yaml` file placed in the repository root, with
sections for connector settings, report sections, routing rules, and report options.

When no configuration file is present, BuildMark applies built-in defaults that include
Changes, Bugs Fixed, and Dependency Updates sections with pre-wired routing rules for
common label and work-item patterns.

For configuration details and examples, see the
[Configuration Guide](https://github.com/demaconsulting/BuildMark/blob/main/docs/user_guide/configuration.md).

### Authentication

Authentication tokens are not configured in `.buildmark.yaml`. BuildMark resolves them automatically
from environment variables at runtime.

**GitHub**: resolves a token from `GH_TOKEN`, then `GITHUB_TOKEN`, then `gh auth token`.

**Azure DevOps**: resolves a token from `AZURE_DEVOPS_PAT`, then `AZURE_DEVOPS_TOKEN`, then
`AZURE_DEVOPS_EXT_PAT`, then `SYSTEM_ACCESSTOKEN` (Azure Pipelines), then
`az account get-access-token` (Azure CLI).

For more detail see the [Authentication Guide](https://github.com/demaconsulting/BuildMark/blob/main/docs/user_guide/introduction.md#with-github-token).

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

See the [CLI Reference][cli-ref] for more details on the self-validation tests.

[cli-ref]: https://github.com/demaconsulting/BuildMark/blob/main/docs/user_guide/cli-reference.md#self-validation

On validation failure the tool will exit with a non-zero exit code.

## Extended Item Controls

BuildMark supports an optional `buildmark` code block in issue and pull request descriptions
to control visibility, type classification, and affected-version ranges. Azure DevOps work items
additionally support native custom fields for the same controls.

For details, see the [Item Controls Guide](https://github.com/demaconsulting/BuildMark/blob/main/docs/user_guide/item-controls.md).

## Report Format

The generated markdown report includes:

1. **Build Report** — title heading
2. **Version Information** — current version, baseline version, and commit hashes
3. **Routed Sections** — items distributed by routing rules (e.g., Changes, Bugs Fixed,
   Dependency Updates), or legacy Changes/Bugs Fixed sections when no rules are configured
4. **Known Issues** — open bugs (when `--include-known-issues` is specified)
5. **Full Changelog** — link to the platform compare view between versions (when available)

Sections with no items are omitted. When routing rules are active, the section order and titles
are determined by the configuration.

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
