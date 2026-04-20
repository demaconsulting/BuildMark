# Configuration Subsystem

## Overview

The Configuration subsystem is responsible for reading and parsing the optional
`.buildmark.yaml` file located in the repository root. It uses the YamlDotNet
library to parse the YAML content, then walks the resulting representation model
to build a strongly-typed `BuildMarkConfig` object. Any parse errors or
validation warnings are surfaced through a `ConfigurationLoadResult` record that
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
| `AzureDevOpsConnectorConfig`| `Configuration/AzureDevOpsConnectorConfig.cs` | Azure DevOps connector settings        |
| `ReportConfig`              | `Configuration/ReportConfig.cs`               | Report output settings data model      |
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
| `Severity`    | Property | `ConfigurationIssueSeverity` enum value            |
| `Description` | Property | Human-readable description of the issue            |

`ConfigurationIssueSeverity` is a public enum with two values:

| Value     | Description                                              |
|-----------|----------------------------------------------------------|
| `Warning` | Non-fatal issue; tool continues and exit code is 0       |
| `Error`   | Fatal issue; tool reports all errors, exits with code 1  |

`BuildMarkConfig` carries the following properties:

| Member      | Kind     | Description                                           |
|-------------|----------|-------------------------------------------------------|
| `Connector` | Property | Optional `ConnectorConfig`; `null` when not specified |
| `Report`    | Property | Optional `ReportConfig`; `null` when not specified    |
| `Sections`  | Property | Ordered list of `SectionConfig` objects               |
| `Rules`     | Property | List of `RuleConfig` objects                          |

`ConnectorConfig` carries the following properties:

| Member        | Kind     | Description                                                                     |
|---------------|----------|---------------------------------------------------------------------------------|
| `Type`        | Property | Connector type: `"github"` or `"azure-devops"`                                  |
| `GitHub`      | Property | Optional `GitHubConnectorConfig`; present when `Type` is `"github"`             |
| `AzureDevOps` | Property | Optional `AzureDevOpsConnectorConfig`; present when `Type` is `"azure-devops"`  |

`GitHubConnectorConfig` carries the following properties:

| Member    | Kind     | Description                                                         |
|-----------|----------|---------------------------------------------------------------------|
| `Owner`   | Property | Repository owner override                                           |
| `Repo`    | Property | Repository name override                                            |
| `BaseUrl` | Property | Optional GitHub Enterprise API base URL; `null` uses the public API |

`AzureDevOpsConnectorConfig` carries the following properties:

| Member            | Kind     | Description                                                                   |
|-------------------|----------|-------------------------------------------------------------------------------|
| `OrganizationUrl` | Property | Azure DevOps organization URL (e.g. `https://dev.azure.com/myorg`)            |
| `Organization`    | Property | Optional organization name override; `null` when not specified                |
| `Project`         | Property | Azure DevOps project name                                                     |
| `Repository`      | Property | Repository name within the project                                            |

`ReportConfig` carries the following properties:

| Member               | Kind     | Description                                                                       |
|----------------------|----------|-----------------------------------------------------------------------------------|
| `File`               | Property | Optional output file path override; `null` uses the default report path           |
| `Depth`              | Property | Optional heading depth for report sections; `null` uses the default depth         |
| `IncludeKnownIssues` | Property | Optional flag to include known issues in the report; `null` uses the default      |

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

`RuleMatchConfig` carries the following properties:

| Member         | Kind     | Description                                                       |
|----------------|----------|-------------------------------------------------------------------|
| `Label`        | Property | List of label values; the rule matches when any label is present  |
| `WorkItemType` | Property | List of work-item type values; rule matches when any type matches |

## Interactions

| Unit / Subsystem           | Role                                                                          |
|----------------------------|-------------------------------------------------------------------------------|
| `Program`                  | Calls `BuildMarkConfigReader.ReadAsync`; calls `result.ReportTo(context)`     |
| `RepoConnectorFactory`     | Receives `ConnectorConfig` from `result.Config` to select the connector       |
| `GitHubRepoConnector`      | Reads `GitHubConnectorConfig` from `result.Config.Connector.GitHub`           |
| `AzureDevOpsRepoConnector` | Reads `AzureDevOpsConnectorConfig` from `result.Config.Connector.AzureDevOps` |
