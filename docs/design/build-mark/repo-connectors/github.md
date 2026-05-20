### GitHub Subsystem

#### Overview

The GitHub subsystem groups the units responsible for querying the GitHub
GraphQL API. It sits within the RepoConnectors subsystem and provides the
production connector used when the repository host is GitHub or GitHub Enterprise.

#### Units

- `GitHubRepoConnector` - `RepoConnectors/GitHub/GitHubRepoConnector.cs` -
  implements `IRepoConnector` for GitHub.
- `GitHubGraphQLClient` - `RepoConnectors/GitHub/GitHubGraphQLClient.cs` -
  issues paginated GraphQL queries.
- `GitHubGraphQLTypes` - `RepoConnectors/GitHub/GitHubGraphQLTypes.cs` -
  provides record types for GraphQL request and response data.

##### `GitHubRepoConnector`

The primary production connector. Resolves the repository URL and GitHub token from
the environment, creates a `GitHubGraphQLClient`, fetches all required data via
GraphQL, applies item-controls overrides, calls `ItemRouter` to assign items to
sections, and assembles the `BuildInformation` record.

##### `GitHubGraphQLClient`

Handles HTTPS communication with the GitHub GraphQL endpoint. Supports paginated
queries and authenticates via an `Authorization: bearer <token>` header. Also
supports GitHub Enterprise by accepting an alternative base URL.

##### `GitHubGraphQLTypes`

Internal C# records that mirror the GraphQL schema types returned by GitHub. Used
as the deserialization target for responses from `GitHubGraphQLClient`.

#### Interfaces

The GitHub subsystem exposes `GitHubRepoConnector`, which implements
`IRepoConnector`. All other types in the subsystem are internal.

| Member | Kind | Description |
|---|---|---|
| `GitHubRepoConnector(config)` | Constructor | Create the connector with optional configuration overrides |
| `GetBuildInformationAsync(version)` | Method | Fetch complete build information from the GitHub GraphQL API |

#### Design

`GitHubRepoConnector` orchestrates the subsystem's data flow. It uses
`GitHubGraphQLClient` for all HTTPS communication, `GitHubGraphQLTypes` records
as GraphQL deserialization targets, and `ItemControlsParser` to extract buildmark
block overrides from issue and pull request description bodies.

The connector calls `ItemControlsParser.Parse` on the `body` field of each issue
and pull request. If a non-null `ItemControlsInfo` is returned, the connector
applies visibility, type, and affected-versions overrides before adding the item
to the appropriate list. When routing rules have been configured, the connector
passes all collected items to `ApplyRules` (inherited from `RepoConnectorBase`)
to populate `BuildInformation.RoutedSections`.

#### Interactions

| Unit / Subsystem        | Role                                                              |
|-------------------------|-------------------------------------------------------------------|
| `IRepoConnector`        | Interface implemented by `GitHubRepoConnector`                    |
| `RepoConnectorBase`     | Base class for `GitHubRepoConnector`                              |
| `ItemRouter`            | Called by `GitHubRepoConnector` to assign items to sections       |
| `ProcessRunner`         | Used (via `RepoConnectorBase`) to run Git and gh CLI commands     |
| `GitHubConnectorConfig` | Supplies owner, repo, and base-URL overrides                      |
| `ItemControlsParser`    | Parses buildmark blocks from issue and PR description bodies      |
| `BuildInformation`      | The output record assembled and returned by `GitHubRepoConnector` |
