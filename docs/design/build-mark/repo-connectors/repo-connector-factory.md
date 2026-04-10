# RepoConnectorFactory

## Overview

`RepoConnectorFactory` is a static factory class that creates the appropriate
`IRepoConnector` implementation based on the runtime environment. It is
delivered exclusively as a .NET tool and is not intended for use as a library
by external callers.

## Methods

### `CreateAsync(ConnectorConfig? config) → Task<IRepoConnector>`

The factory method accepts an optional `ConnectorConfig` from the
parsed `.buildmark.yaml` file and returns the appropriate
`IRepoConnector` implementation:

- If `config?.Type` is `"azure-devops"`, a `NotSupportedException` is thrown
  (Azure DevOps support is not yet implemented).
- Otherwise, returns a new `GitHubRepoConnector` initialized with
  `config?.GitHub` (which may be `null` if no connector config was supplied).

In the absence of a `ConnectorConfig`, the method also auto-detects the
environment to confirm GitHub is appropriate, using the following signals:

1. The `GITHUB_ACTIONS` environment variable is non-empty.
2. The `GITHUB_WORKSPACE` environment variable is non-empty.
3. The git remote URL (obtained asynchronously via
   `ProcessRunner.TryRunAsync("git", "remote get-url origin")`)
   contains `github.com`.

## Interactions

| Unit / Subsystem | Role |
| ---------------- | ---- |
| `IRepoConnector` | Return type of `CreateAsync` |
| `ConnectorConfig` | Optional envelope passed to `CreateAsync`; type discriminates result |
| `GitHubConnectorConfig` | Forwarded to `GitHubRepoConnector` as `config?.GitHub` |
| `GitHubRepoConnector` | The concrete connector returned for GitHub repositories |
| `ProcessRunner` | Used by `CreateAsync` to inspect git remote URL |
| `Program` | Calls `await RepoConnectorFactory.CreateAsync(result.Config?.Connector)` |
