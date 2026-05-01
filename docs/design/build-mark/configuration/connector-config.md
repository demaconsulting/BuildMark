# ConnectorConfig, GitHubConnectorConfig, AzureDevOpsConnectorConfig

## Overview

These three configuration data models define the connector selection and per-connector
settings read from the `connector:` section of `.buildmark.yaml`.

`ConnectorConfig` is the envelope that carries the connector type and the platform-specific
settings. `GitHubConnectorConfig` holds GitHub-specific overrides. `AzureDevOpsConnectorConfig`
holds Azure DevOps-specific connection details.

## Data Models

### ConnectorConfig

| Property      | Type                          | Description                                   |
|---------------|-------------------------------|-----------------------------------------------|
| `Type`        | `string?`                     | Connector type: `"github"` or `"azure-devops"`|
| `GitHub`      | `GitHubConnectorConfig?`      | Optional GitHub connector settings            |
| `AzureDevOps` | `AzureDevOpsConnectorConfig?` | Optional Azure DevOps connector settings      |

### GitHubConnectorConfig

| Property  | Type      | Description                                                         |
|-----------|-----------|---------------------------------------------------------------------|
| `Owner`   | `string?` | Repository owner override                                           |
| `Repo`    | `string?` | Repository name override                                            |
| `BaseUrl` | `string?` | Optional GitHub Enterprise API base URL; `null` uses the public API |

### AzureDevOpsConnectorConfig

| Property          | Type      | Description                                                               |
|-------------------|-----------|---------------------------------------------------------------------------|
| `OrganizationUrl` | `string?` | Azure DevOps organization URL (e.g. `https://dev.azure.com/myorg`)        |
| `Organization`    | `string?` | Optional organization name override; `null` when not specified            |
| `Project`         | `string?` | Azure DevOps project name                                                 |
| `Repository`      | `string?` | Repository name within the project                                        |

## Interactions

| Unit / Subsystem            | Role                                                                            |
|-----------------------------|---------------------------------------------------------------------------------|
| `BuildMarkConfig`           | Holds `ConnectorConfig` in its `Connector` property                             |
| `BuildMarkConfigReader`     | Parses the `connector:` YAML node and populates these records                   |
| `RepoConnectorFactory`      | Receives `ConnectorConfig` to select the appropriate connector implementation   |
| `GitHubRepoConnector`       | Reads `GitHubConnectorConfig` for owner, repo, and base URL overrides           |
| `AzureDevOpsRepoConnector`  | Reads `AzureDevOpsConnectorConfig` for organization URL, project, and repo      |
