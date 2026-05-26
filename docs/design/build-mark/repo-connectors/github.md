### GitHub

#### Overview

The GitHub subsystem groups the units responsible for querying the GitHub GraphQL API. It sits
within the RepoConnectors subsystem and provides the production connector used when the repository
host is GitHub or GitHub Enterprise Server.

The subsystem contains the following units:

- `GitHubRepoConnector` — implements `IRepoConnector` for GitHub; orchestrates authentication,
  GraphQL queries, and assembly of the `BuildInformation` record.
- `GitHubGraphQLClient` — issues paginated GraphQL requests to the GitHub API and deserializes
  responses into typed records; implements `IDisposable` and owns its `HttpClient`.
- `GitHubGraphQLTypes` — C# record definitions that mirror GitHub GraphQL request and response
  payloads; used as deserialization targets by `GitHubGraphQLClient`.

#### Interfaces

**GitHubRepoConnector**: The production `IRepoConnector` implementation for GitHub repositories.
All other types in the subsystem are internal.

- *Type*: In-process .NET public API.
- *Role*: Provider — exposed to `RepoConnectorFactory` and callers of `IRepoConnector`.
- *Contract*: Constructor `GitHubRepoConnector(GitHubConnectorConfig?)` accepts optional
  configuration overrides; `GetBuildInformationAsync(VersionTag? version)` fetches complete build
  information from the GitHub GraphQL API and returns a `BuildInformation` record.
- *Constraints*: Requires a valid GitHub authentication token resolvable from environment variables
  or the `gh` CLI; throws `InvalidOperationException` when no token is found or when no release
  matches the current commit hash and no version is supplied.

#### Design

`GitHubRepoConnector` orchestrates the subsystem's data flow:

1. Read the git remote URL and current commit hash via `RunCommandAsync` (inherited from
   `RepoConnectorBase`).
2. Determine the owner and repository name from `GitHubConnectorConfig` or by parsing the remote
   URL.
3. Resolve a GitHub authentication token (`GH_TOKEN`, `GITHUB_TOKEN`, or `gh auth token`).
4. Create a `GitHubGraphQLClient` with the resolved token, using `GitHubConnectorConfig.BaseUrl`
   as the GraphQL endpoint when set (supports GitHub Enterprise Server); fetch tags, releases,
   commits, pull requests (with `body`), and issues (with `body`) via GraphQL, using
   `GitHubGraphQLTypes` records as deserialization targets.
5. Call `ItemControlsParser.Parse` on the `body` of each pull request and issue; apply visibility,
   type, and affected-version overrides from the returned `ItemControlsInfo`.
6. Collect changes and known issues; if routing rules are configured, call `ApplyRules` (inherited
   from `RepoConnectorBase`) to distribute all items into the configured sections and populate
   `BuildInformation.RoutedSections`. If no rules are configured, items remain in the legacy
   `Changes`, `Bugs`, and `KnownIssues` lists.
7. Return the assembled `BuildInformation` record.
