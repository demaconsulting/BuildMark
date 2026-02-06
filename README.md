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
history and GitHub issues. It analyzes commits, pull requests, and issues to create human-readable build notes,
making it easy to integrate release documentation into your CI/CD pipelines and documentation workflows.

## Features

- 📄 **Git Integration** - Analyze Git repository history and tags
- 📝 **Markdown Reports** - Generate human-readable build notes from repository data
- 🐛 **Issue Tracking** - Extract bug fixes and changes from GitHub issues and pull requests
- 🎯 **Customizable Output** - Configure report depth and version ranges
- 🚀 **CI/CD Integration** - Automate build notes generation in your pipelines
- 🌐 **Multi-Platform** - Support for .NET 8, 9, and 10
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

### Self-Validation Tests

BuildMark includes built-in self-validation tests that verify the tool's functionality without requiring external
repositories or services. These tests use mock data to validate core features and generate test result files in TRX
or JUnit format.

The self-validation suite includes tests that verify:

- Version tag parsing and comparison
- Build information extraction from repositories
- Markdown report generation
- GitHub repository connector functionality
- Mock repository connector functionality

These tests provide evidence of the tool's functionality and are particularly useful for:

- Verifying the installation is working correctly
- Running automated tests in CI/CD pipelines without requiring repository access
- Generating test evidence for compliance and traceability requirements

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
