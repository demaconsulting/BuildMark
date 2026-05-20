#### GitHubGraphQLClient

##### Purpose

`GitHubGraphQLClient` is the GitHub subsystem unit responsible for issuing
paginated GraphQL requests to the GitHub API and translating the responses into
typed records for connector consumption. `GitHubRepoConnector` delegates all
GitHub API communication to this client.

##### Constructors

The class provides two constructors:

- **Public constructor** - accepts a GitHub authentication token and an optional
  `graphqlEndpoint` URL, then creates an owned `HttpClient` configured with the
  token and wraps it in a `GraphQLHttpClient` instance (from the `GraphQL.Client.Http`
  package). Used by `GitHubRepoConnector` in production. When `graphqlEndpoint` is
  omitted, the default GitHub GraphQL API endpoint (`https://api.github.com/graphql`)
  is used. For GitHub Enterprise Server, supply the enterprise-specific endpoint
  (e.g., `https://your-github-enterprise/api/graphql`).
- **Internal constructor** - accepts an existing `HttpClient` directly and an optional
  `graphqlEndpoint` URL. Used by tests to inject a mock `HttpClient` without network
  access.

##### Lifecycle

`GitHubGraphQLClient` implements `IDisposable`. When created via the public
constructor, the instance owns its `GraphQLHttpClient` (and the `HttpClient` it wraps)
and disposes them when the client is disposed. When created via the internal
constructor, the caller retains ownership of the `HttpClient` and the client does
not dispose it.

Callers that construct `GitHubGraphQLClient` via the public constructor must
wrap usage in a `using` statement or otherwise dispose the instance to release
the underlying HTTP connection resources.

##### Error Handling

All API methods catch exceptions from the underlying `GraphQLHttpClient` and return
empty lists rather than propagating the exception to the caller. This allows
the connector to continue with partial data when the GitHub API is transiently
unavailable.

> **Note**: Because exceptions are silently swallowed and an empty list is returned,
> runtime failures (network errors, authentication failures, malformed responses) are
> not observable by the caller. Diagnostics require inspecting log output or
> correlating an unexpectedly empty result set with network or authentication issues.

##### Data Model

`GitHubGraphQLClient` holds a single `GraphQLHttpClient` instance (from the external
`GraphQL.Client.Http` NuGet package). The `GraphQLHttpClient` internally manages an
`HttpClient` for HTTPS transport. When constructed via the public constructor, the
client owns both the `GraphQLHttpClient` and the underlying `HttpClient` and disposes
them on disposal. When constructed via the internal constructor (for test injection),
the caller retains ownership of the `HttpClient` and the client does not dispose
the injected instance.

##### Dependencies

| Dependency                 | Package                                    | Purpose                                  |
| -------------------------- | ------------------------------------------ | ---------------------------------------- |
| `GraphQLHttpClient`        | `GraphQL.Client.Http`                      | Sends GraphQL queries over HTTPS         |
| `SystemTextJsonSerializer` | `GraphQL.Client.Serializer.SystemTextJson` | Serializes/deserializes GraphQL payloads |

##### Key Methods

The client provides methods for retrieving the repository data needed to build a
`BuildInformation` record:

- `GetCommitsAsync` for commit SHAs in a range
- `GetReleasesAsync` for release tag names
- `GetAllTagsAsync` for tag nodes
- `GetPullRequestsAsync` for pull request data, including description bodies
- `GetAllIssuesAsync` for issue data, including description bodies
- `FindIssueIdsLinkedToPullRequestAsync` for cross-link lookups

##### Interactions

- `GitHubRepoConnector` creates and calls `GitHubGraphQLClient`.
- `GitHubGraphQLTypes` provide the request and response record types used for
  serialization and deserialization.
- The GitHub GraphQL endpoint provides the remote repository data queried by the
  client.
