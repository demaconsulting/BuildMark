# Getting Started

## Basic Usage

The simplest usage requires a build version and an output report path:

```bash
buildmark --build-version v1.2.3 --report build-notes.md
```

This command analyzes the Git repository in the current directory, finds the previous version tag,
and generates a markdown report with changes, bug fixes, and other release information.

## Using a GitHub Token

For private repositories or to avoid GitHub API rate limits, provide a GitHub token through an
environment variable:

```bash
# Using GH_TOKEN
export GH_TOKEN=ghp_abc123...
buildmark --build-version v1.2.3 --report build-notes.md

# Or using GITHUB_TOKEN
export GITHUB_TOKEN=ghp_abc123...
buildmark --build-version v1.2.3 --report build-notes.md
```

## Using an Azure DevOps Token

For Azure DevOps repositories, provide a personal access token:

```bash
export AZURE_DEVOPS_PAT=your-pat-token...
buildmark --build-version v1.2.3 --report build-notes.md
```

In Azure Pipelines, the pipeline service connection token is picked up automatically from
`SYSTEM_ACCESSTOKEN` when the pipeline is granted access.

## Including Known Issues

To include known issues in the generated report:

```bash
buildmark --build-version v1.2.3 --report build-notes.md --include-known-issues
```
