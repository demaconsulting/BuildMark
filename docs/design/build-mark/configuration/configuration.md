# Configuration Subsystem

## Overview

The Configuration subsystem is responsible for reading and parsing the optional
`.buildmark.yaml` file located in the repository root. It deserializes the YAML
content into a strongly-typed `BuildMarkConfig` object and surfaces any parse
errors or validation warnings through a `ConfigurationLoadResult` record that
`Program` consumes during startup.

When no `.buildmark.yaml` file is present, `BuildMarkConfigReader` returns a
`ConfigurationLoadResult` with a `null` configuration and an empty issues list,
and the rest of the system operates with its default behavior unchanged. When
the file is present but malformed, the result carries `null` for the
configuration alongside a list of `ConfigurationIssue` records describing each
problem with its file path, line number, severity, and description.

## Units

| Unit                        | File                                          | Responsibility                         |
|-----------------------------|-----------------------------------------------|----------------------------------------|
| `BuildMarkConfig`           | `Configuration/BuildMarkConfig.cs`            | Top-level configuration data model     |
| `BuildMarkConfigReader`     | `Configuration/BuildMarkConfigReader.cs`      | Reads and parses `.buildmark.yaml`     |
| `ConfigurationLoadResult`   | `Configuration/ConfigurationLoadResult.cs`    | Holds config and any load issues       |
| `ConfigurationIssue`        | `Configuration/ConfigurationIssue.cs`         | Single issue with location and severity|
| `ConnectorConfig`           | `Configuration/ConnectorConfig.cs`            | Connector envelope data model          |
| `GitHubConnectorConfig`     | `Configuration/GitHubConnectorConfig.cs`      | GitHub connector settings data model   |
| `AzureDevOpsConnectorConfig`| `Configuration/AzureDevOpsConnectorConfig.cs` | Azure DevOps connector settings stub   |
| `SectionConfig`             | `Configuration/SectionConfig.cs`              | Report section definition data model   |
| `RuleConfig`                | `Configuration/RuleConfig.cs`                 | Item routing rule data model           |

## Interfaces

`BuildMarkConfigReader` exposes the following outward-facing interface consumed by
`Program`:

| Member            | Kind   | Description                                                               |
|-------------------|--------|---------------------------------------------------------------------------|
| `ReadAsync(path)` | Method | Reads and deserializes the file; always returns `ConfigurationLoadResult` |

`ConfigurationLoadResult` carries the following members:

| Member              | Kind     | Description                                              |
|---------------------|----------|----------------------------------------------------------|
| `Config`            | Property | Parsed `BuildMarkConfig`; `null` if parsing failed       |
| `Issues`            | Property | Ordered list of `ConfigurationIssue` objects             |
| `HasErrors`         | Property | `true` when any issue has `Severity` of `Error`          |
| `ReportTo(context)` | Method   | Writes all issues to `Context`; sets exit code on errors |

`ConfigurationIssue` carries the following properties:

| Member        | Kind     | Description                                        |
|---------------|----------|----------------------------------------------------|
| `FilePath`    | Property | Path to the file containing the issue              |
| `Line`        | Property | Line number (1-based) of the issue                 |
| `Severity`    | Property | `Error` or `Warning`                               |
| `Description` | Property | Human-readable description of the issue            |

`BuildMarkConfig` carries the following properties:

| Member      | Kind     | Description                                           |
|-------------|----------|-------------------------------------------------------|
| `Connector` | Property | Optional `ConnectorConfig`; `null` when not specified |
| `Sections`  | Property | Ordered list of `SectionConfig` objects               |
| `Rules`     | Property | List of `RuleConfig` objects                          |

`ConnectorConfig` carries the following properties:

| Member        | Kind     | Description                                                               |
|---------------|----------|---------------------------------------------------------------------------|
| `Type`        | Property | Connector type: `"github"`, `"azure-devops"`, or `"github+azure-devops"`  |
| `GitHub`      | Property | Optional `GitHubConnectorConfig`; present when `Type` includes `"github"` |
| `AzureDevOps` | Property | Reserved for future use; `null` in the current release                    |

`GitHubConnectorConfig` carries the following properties:

| Member    | Kind     | Description                                                        |
|-----------|----------|--------------------------------------------------------------------|
| `Owner`   | Property | Repository owner; falls back to `--owner` CLI argument if absent   |
| `Repo`    | Property | Repository name; falls back to `--repo` CLI argument if absent     |
| `BaseUrl` | Property | Optional GitHub Enterprise API base URL; `null` uses the public API|

`AzureDevOpsConnectorConfig` is a placeholder for future Azure DevOps connector settings.
No properties are defined in the current release.

`SectionConfig` carries the following properties:

| Member  | Kind     | Description                          |
|---------|----------|--------------------------------------|
| `Id`    | Property | Unique identifier for the section    |
| `Title` | Property | Display title for the report section |

`RuleConfig` carries the following properties:

| Member  | Kind     | Description                                             |
|---------|----------|---------------------------------------------------------|
| `Match` | Property | Match conditions (labels, work-item types) for the rule |
| `Route` | Property | Destination section `Id` for matched items              |

## Interactions

| Unit / Subsystem        | Role                                                                                       |
|-------------------------|--------------------------------------------------------------------------------------------|
| `Program`               | Calls `BuildMarkConfigReader.ReadAsync`; calls `result.ReportTo(context)`                  |
| `RepoConnectorFactory`  | Receives `ConnectorConfig` from `result.Config` to select the connector                    |
| `GitHubRepoConnector`   | Receives routing lists; reads `GitHubConnectorConfig` from `result.Config.Connector.GitHub`|
