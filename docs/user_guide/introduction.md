<!-- markdownlint-disable MD025 -->

# Introduction

BuildMark is a .NET command-line tool that generates comprehensive markdown build notes reports from Git repository
history and GitHub issues. It analyzes commits, pull requests, and issues to create human-readable build notes,
making it easy to integrate release documentation into your CI/CD pipelines and documentation workflows.

## Key Features

- **Git Integration** — Analyze Git repository history, tags, and branches
- **Markdown Reports** — Generate structured build notes from repository data
- **Issue Tracking** — Extract changes and bug fixes from GitHub and Azure DevOps
- **Configurable Routing** — Route items into custom report sections by label or work-item type
- **Customizable Output** — Configure report depth, sections, and known-issue inclusion
- **CI/CD Ready** — Automate build notes generation in GitHub Actions and Azure Pipelines
- **Multi-Platform** — Windows, Linux, and macOS with .NET 8, 9, and 10
- **Self-Validation** — Built-in qualification tests without external tooling
- **Dependency Updates** — Built-in tracking of dependency changes from Dependabot and Renovate

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

## With Azure DevOps Token

For Azure DevOps repositories, provide a Personal Access Token (PAT):

```bash
# Using Personal Access Token
export AZURE_DEVOPS_PAT=your-pat-token...
buildmark --build-version v1.2.3 --report build-notes.md
```

In Azure Pipelines, the pipeline service connection token is picked up automatically from
`SYSTEM_ACCESSTOKEN` when you grant the pipeline permission to access it.

## Including Known Issues

To include known issues in the report:

```bash
buildmark --build-version v1.2.3 --report build-notes.md --include-known-issues
```

[dotnet-download]: https://dotnet.microsoft.com/download
[continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
