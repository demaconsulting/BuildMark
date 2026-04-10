# RepoConnectorFactory

## Overview

`RepoConnectorFactory` is a static factory class that creates the appropriate
`IRepoConnector` implementation based on the runtime environment.

## Methods

### `CreateAsync(ConnectorConfig? config) → Task<IRepoConnector>`

The preferred factory method. Accepts an optional `ConnectorConfig` from the
parsed `.buildmark.yaml` file and asynchronously returns the appropriate
`IRepoConnector` implementation:

- If `config?.Type` is `"azure-devops"`, a `NotSupportedException` is thrown
  (Azure DevOps support is not yet implemented).
- Otherwise, returns a new `GitHubRepoConnector` initialized with
  `config?.GitHub` (which may be `null` if no connector config was supplied).

In the absence of a `ConnectorConfig`, the method also auto-detects the
environment to confirm GitHub is appropriate, using the following signals:

1. The `GITHUB_ACTIONS` environment variable is non-empty.
2. The `GITHUB_WORKSPACE` environment variable is non-empty.
3. The git remote URL (obtained by awaiting `ProcessRunner.TryRunAsync("git",
   "remote get-url origin")`) contains `github.com`.

### `Create(ConnectorConfig? config) → IRepoConnector`

A synchronous fallback factory method that skips the git remote URL check to
avoid sync-over-async deadlock risks. It applies the same connector-config
validation and `GITHUB_ACTIONS`/`GITHUB_WORKSPACE` environment-variable checks
as `CreateAsync`, but defaults directly to `GitHubRepoConnector` when no
environment variables are detected.

## Interactions

| Unit / Subsystem        | Role                                                                          |
|-------------------------|-------------------------------------------------------------------------------|
| `IRepoConnector`        | Return type of `Create` and `CreateAsync`                                     |
| `ConnectorConfig`       | Optional envelope passed to `Create`/`CreateAsync`; type discriminates result |
| `GitHubConnectorConfig` | Forwarded to `GitHubRepoConnector` as `config?.GitHub`                        |
| `GitHubRepoConnector`   | The concrete connector returned for GitHub repositories                        |
| `ProcessRunner`         | Used via `TryRunAsync` by `CreateAsync` to inspect the git remote URL         |
| `Program`               | Calls `RepoConnectorFactory.CreateAsync(result.Config?.Connector)`            |
