# RepoConnectorFactory

## Overview

`RepoConnectorFactory` is a static factory class that creates the appropriate
`IRepoConnector` implementation based on the runtime environment.

## Methods

### `Create(ConnectorConfig? config) → IRepoConnector`

Accepts an optional `ConnectorConfig` from the parsed `.buildmark.yaml` file and
returns the appropriate `IRepoConnector` implementation:

- If `config?.Type` is `"azure-devops"`, a `NotSupportedException` is thrown
  (Azure DevOps support is not yet implemented).
- Otherwise, returns a new `GitHubRepoConnector` initialized with
  `config?.GitHub` (which may be `null` if no connector config was supplied).

In the absence of a `ConnectorConfig`, the factory also auto-detects the
environment to confirm GitHub is appropriate, using the following signals:

1. The `GITHUB_ACTIONS` environment variable is non-empty.
2. The `GITHUB_WORKSPACE` environment variable is non-empty.
3. The git remote URL (obtained by running `git remote get-url origin`) contains
   `github.com`.

## Interactions

| Unit / Subsystem        | Role                                                                  |
|-------------------------|-----------------------------------------------------------------------|
| `IRepoConnector`        | Return type of `Create`                                               |
| `ConnectorConfig`       | Optional envelope passed to `Create`; type discriminates the result   |
| `GitHubConnectorConfig` | Forwarded to `GitHubRepoConnector` as `config?.GitHub`                |
| `GitHubRepoConnector`   | The concrete connector returned by `Create` for GitHub repositories   |
| `ProcessRunner`         | Used via `TryRunAsync` to inspect the git remote URL                  |
| `Program`               | Calls `RepoConnectorFactory.Create(result.Config?.Connector)`         |
