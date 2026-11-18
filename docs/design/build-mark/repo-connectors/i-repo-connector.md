### IRepoConnector

#### Purpose

`IRepoConnector` is the connector contract of the RepoConnectors subsystem. It lets `Program`
collect build information without knowing which repository platform it is talking to.
`RepoConnectorFactory` returns an `IRepoConnector`, and every concrete connector
(`GitHubRepoConnector`, `AzureDevOpsRepoConnector`, and `MockRepoConnector`) implements it through
`RepoConnectorBase`.

#### Data Model

N/A — `IRepoConnector` is an interface with no state.

#### Key Methods

**GetBuildInformationAsync(version)**: Collects the build information for a release.

- *Parameters*: `VersionTag? version` — the target version; when `null`, the connector uses the
  most recent tag if it matches the current commit.
- *Returns*: `Task<BuildInformation>` — the versions, changes, bugs, and known issues for the
  release.
- *Preconditions*: The connector has been created by `RepoConnectorFactory` and, where routing
  is needed, configured through `RepoConnectorBase.Configure`.
- *Postconditions*: Returns a populated `BuildInformation`.

#### Error Handling

Implementations throw `InvalidOperationException` when the target version cannot be determined.

#### Dependencies

- **BuildInformation** — the result type of `GetBuildInformationAsync`.
- **VersionTag** — the type of the optional target version.

#### Callers

- **Program** — calls `GetBuildInformationAsync` on the connector returned by
  `RepoConnectorFactory`.
- **RepoConnectorFactory** — returns concrete connectors typed as `IRepoConnector`.
