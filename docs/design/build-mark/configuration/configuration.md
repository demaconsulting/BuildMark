# Configuration Subsystem

## Overview

The Configuration subsystem is responsible for reading and parsing the optional
`.buildmark.yaml` file located in the repository root. It deserializes the YAML
content into a strongly-typed `BuildMarkConfig` object that `Program` consumes
during startup to select the appropriate repository connector and to apply
custom report-section definitions and item-routing rules.

When no `.buildmark.yaml` file is present, `BuildMarkConfigReader` returns `null`
and the rest of the system operates with its default behavior unchanged.

## Units

| Unit                    | File                                       | Responsibility                           |
|-------------------------|--------------------------------------------|-----------------------------------------||
| `BuildMarkConfig`       | `Configuration/BuildMarkConfig.cs`         | Top-level configuration data model      |
| `BuildMarkConfigReader` | `Configuration/BuildMarkConfigReader.cs`   | Reads and deserializes `.buildmark.yaml` |
| `ConnectorConfig`       | `Configuration/ConnectorConfig.cs`         | Connector settings data model            |
| `SectionConfig`         | `Configuration/SectionConfig.cs`           | Report section definition data model    |
| `RuleConfig`            | `Configuration/RuleConfig.cs`              | Item routing rule data model             |

## Interfaces

`BuildMarkConfigReader` exposes the following outward-facing interface consumed by
`Program`:

| Member              | Kind   | Description                                                |
|---------------------|--------|------------------------------------------------------------|
| `ReadAsync(path)`   | Method | Reads and deserializes the file; returns `null` if absent  |

`BuildMarkConfig` carries the following properties:

| Member      | Kind     | Description                                           |
|-------------|----------|-------------------------------------------------------|
| `Connector` | Property | Optional `ConnectorConfig`; `null` when not specified |
| `Sections`  | Property | Ordered list of `SectionConfig` objects               |
| `Rules`     | Property | List of `RuleConfig` objects                          |

`ConnectorConfig` carries the following properties:

| Member | Kind     | Description                                                              |
|--------|----------|--------------------------------------------------------------------------|
| `Type` | Property | Connector type: `"github"`, `"azure-devops"`, or `"github+azure-devops"` |

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

| Unit / Subsystem       | Role                                                         |
|------------------------|--------------------------------------------------------------|
| `Program`              | Calls `BuildMarkConfigReader.ReadAsync` at startup           |
| `RepoConnectorFactory` | Receives `ConnectorConfig` to select the connector           |
| `GitHubRepoConnector`  | Receives `SectionConfig` and `RuleConfig` lists for routing  |
