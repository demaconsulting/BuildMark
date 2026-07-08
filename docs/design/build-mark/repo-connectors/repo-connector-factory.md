### RepoConnectorFactory

![RepoConnectors Structure](../../generated/RepoConnectorsView.svg)

#### Purpose

`RepoConnectorFactory` is a static factory class that creates the appropriate `IRepoConnector`
implementation based on the runtime environment. It examines explicit configuration, environment
variables, and the git remote URL in a fixed priority order, then returns the matching connector so
that callers never need to know which platform is in use.

#### Data Model

N/A — `RepoConnectorFactory` is a static class with no instance state.

#### Key Methods

**Create**: Creates a repository connector based on the current environment and optional
configuration.

- *Parameters*: `ConnectorConfig? config` — optional connector configuration parsed from
  `.buildmark.yaml`.
- *Returns*: `IRepoConnector` — the appropriate connector instance.
- *Preconditions*: None; null `config` is treated as unconfigured.
- *Postconditions*: Returns a non-null `IRepoConnector`; never throws.

Selection order: (1) if `config.Type` equals `"azure-devops"`, return
`AzureDevOpsRepoConnector(config.AzureDevOps)`; (2) if `TF_BUILD` environment variable is
non-empty, return `AzureDevOpsRepoConnector(config?.AzureDevOps)`; (3) if `GITHUB_ACTIONS` or
`GITHUB_WORKSPACE` is non-empty, return `GitHubRepoConnector(config?.GitHub)`; (4) read the git
remote URL once via `ProcessRunner.TryRunAsync("git", "remote", "get-url", "origin")` using
sync-over-async (safe in a console application with no synchronization context); delegate to
`CreateFromRemoteUrl` with the result. If the git process is unavailable, `TryRunAsync` returns
`null` and `CreateFromRemoteUrl` defaults to `GitHubRepoConnector`.

**CreateFromRemoteUrl**: Internal helper that selects a connector based on the git remote URL,
bypassing environment-variable checks.

- *Parameters*: `ConnectorConfig? config` — optional configuration; `string? remoteUrl` — the git
  remote URL, or `null` if unavailable.
- *Returns*: `IRepoConnector` — `AzureDevOpsRepoConnector` when `remoteUrl` contains
  `dev.azure.com` or `visualstudio.com`; `GitHubRepoConnector` when `remoteUrl` contains
  `github.com`; `GitHubRepoConnector` as the default when `remoteUrl` is null or unrecognized.
- *Preconditions*: None.
- *Postconditions*: Returns a non-null `IRepoConnector`.
- *Note*: GitHub Enterprise Cloud (`*.ghe.com`) and GitHub Enterprise Server (on-premises)
  remotes do not match the `github.com` substring check and therefore fall through to the
  default `GitHubRepoConnector`. This is correct and expected behavior: `GitHubRepoConnector`
  is host-agnostic and handles any GitHub remote regardless of hostname; the factory default
  ensures that enterprise remotes are processed by the same connector as public GitHub.

Exposed internally so that unit tests can exercise URL-based detection logic without requiring a
real git process.

#### Error Handling

`Create` never throws. If the git remote URL cannot be determined (e.g., git is unavailable),
`ProcessRunner.TryRunAsync` returns `null` and the factory silently defaults to a
`GitHubRepoConnector`. Connector type detection errors are suppressed to avoid failing tool startup
on environment differences.

#### Dependencies

- **IRepoConnector** — the return type of both factory methods.
- **ConnectorConfig** — optional envelope passed to `Create`; its `Type` field discriminates the
  result.
- **GitHubConnectorConfig** — forwarded to `GitHubRepoConnector` as `config?.GitHub`.
- **AzureDevOpsConnectorConfig** — forwarded to `AzureDevOpsRepoConnector` as
  `config?.AzureDevOps`.
- **GitHubRepoConnector** — returned for GitHub repositories and as the default fallback.
- **AzureDevOpsRepoConnector** — returned for Azure DevOps repositories.
- **ProcessRunner** — used via sync-over-async by `Create` to inspect the git remote URL.

#### Callers

- **Program** — calls `RepoConnectorFactory.Create(result.Config?.Connector)` to obtain the
  connector before processing build notes.
