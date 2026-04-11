# RepoConnectorFactory

## Overview

`RepoConnectorFactory` is a static factory class that creates the appropriate
`IRepoConnector` implementation based on the runtime environment.

## Methods

### `Create(ConnectorConfig? config) → IRepoConnector`

The factory method accepts an optional `ConnectorConfig` from the
parsed `.buildmark.yaml` file and returns the appropriate
`IRepoConnector` implementation:

- If `config?.Type` is `"azure-devops"`, returns a new `AzureDevOpsRepoConnector`
  initialized with `config?.AzureDevOps`.
- Otherwise, returns a new `GitHubRepoConnector` initialized with
  `config?.GitHub` (which may be `null` if no connector config was supplied).

In the absence of a `ConnectorConfig`, the method auto-detects the environment
using the following signals, checked in order:

1. The `TF_BUILD` environment variable is non-empty — indicates Azure DevOps
   Pipelines; creates an `AzureDevOpsRepoConnector`.
2. The git remote URL contains `dev.azure.com` or `visualstudio.com` — creates
   an `AzureDevOpsRepoConnector`.
3. The `GITHUB_ACTIONS` environment variable is non-empty — creates a
   `GitHubRepoConnector`.
4. The `GITHUB_WORKSPACE` environment variable is non-empty — creates a
   `GitHubRepoConnector`.
5. The git remote URL contains `github.com` — creates a `GitHubRepoConnector`.

The git remote URL is obtained using the sync-over-async pattern via
`ProcessRunner.TryRunAsync("git", "remote", "get-url", "origin").GetAwaiter().GetResult()`.
BuildMark is distributed as a .NET tool and is not intended for consumption as a
library by external callers, so this design reflects the tool-oriented execution
model rather than a guarantee that synchronization-context-related deadlocks are
impossible in every host.

## Interactions

| Unit / Subsystem             | Role                                                                   |
| ---------------------------- | ---------------------------------------------------------------------- |
| `IRepoConnector`             | Return type of `Create`                                                |
| `ConnectorConfig`            | Optional envelope passed to `Create`; type discriminates result        |
| `GitHubConnectorConfig`      | Forwarded to `GitHubRepoConnector` as `config?.GitHub`                 |
| `AzureDevOpsConnectorConfig` | Forwarded to `AzureDevOpsRepoConnector` as `config?.AzureDevOps`       |
| `GitHubRepoConnector`        | The concrete connector returned for GitHub repositories                |
| `AzureDevOpsRepoConnector`   | The concrete connector returned for Azure DevOps repositories          |
| `ProcessRunner`              | Used via sync-over-async by `Create` to inspect git remote URL         |
| `Program`                    | Calls `RepoConnectorFactory.Create(result.Config?.Connector)`          |
