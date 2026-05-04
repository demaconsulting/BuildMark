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

When `config?.Type` is not `"azure-devops"` (including when `config` is `null`
or `Type` is `null`), the method auto-detects the environment using the
following signals, checked in order:

1. The `TF_BUILD` environment variable is non-empty - indicates Azure DevOps
   Pipelines; creates an `AzureDevOpsRepoConnector`.
2. The `GITHUB_ACTIONS` or `GITHUB_WORKSPACE` environment variable is
   non-empty - creates a `GitHubRepoConnector`.
3. The git remote URL contains `dev.azure.com` or `visualstudio.com` - creates
   an `AzureDevOpsRepoConnector`.
4. The git remote URL contains `github.com` - creates a `GitHubRepoConnector`.
5. None of the above matched - defaults to a `GitHubRepoConnector`.

The git remote URL is obtained **once** using the sync-over-async pattern via
`ProcessRunner.TryRunAsync("git", "remote", "get-url", "origin").GetAwaiter().GetResult()`,
then forwarded to `CreateFromRemoteUrl`. If the git process is unavailable or returns
no output, `TryRunAsync` returns `null`; in that case `CreateFromRemoteUrl` falls
through to the default and returns a `GitHubRepoConnector`.
BuildMark is distributed as a .NET tool and is not intended for consumption as a
library by external callers, so this design reflects the tool-oriented execution
model rather than a guarantee that synchronization-context-related deadlocks are
impossible in every host.

### `CreateFromRemoteUrl(ConnectorConfig? config, string? remoteUrl) → IRepoConnector` *(internal)*

An internal helper that selects a connector based on `remoteUrl` alone (skipping
environment-variable checks). It is exposed internally so that unit tests can
exercise the URL-based detection logic without requiring a real git process.

- If `remoteUrl` contains `dev.azure.com` or `visualstudio.com` (case-insensitive),
  returns a new `AzureDevOpsRepoConnector` initialized with `config?.AzureDevOps`.
- If `remoteUrl` contains `github.com` (case-insensitive),
  returns a new `GitHubRepoConnector` initialized with `config?.GitHub`.
- If `remoteUrl` is `null` or does not match any known host, defaults to a
  `GitHubRepoConnector` initialized with `config?.GitHub`.

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
