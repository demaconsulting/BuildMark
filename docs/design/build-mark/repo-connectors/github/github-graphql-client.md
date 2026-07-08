#### GitHubGraphQLClient

![GitHub Structure](../../../generated/GitHubView.svg)

##### Purpose

`GitHubGraphQLClient` is the unit responsible for issuing paginated GraphQL requests to the GitHub
API and deserializing responses into typed `GitHubGraphQLTypes` records for consumption by
`GitHubRepoConnector`. All GitHub API communication is delegated to this client.

The client authenticates via an `Authorization: bearer <token>` header. It accepts an optional
`graphqlEndpoint` URL to support GitHub Enterprise Server (default:
`https://api.github.com/graphql`). The client implements `IDisposable`; when created via the
public constructor, it owns its `GraphQLHttpClient` (and the underlying `HttpClient`) and disposes
them on disposal. When created via the internal constructor (for test injection), the caller retains
ownership of the `HttpClient`.

##### Data Model

**_graphqlClient**: `GraphQLHttpClient` — The GraphQL HTTP client used for all API communication;
configured with the authentication header and the GraphQL endpoint URI.

**_ownsGraphQLClient**: `bool` — Indicates whether this instance owns the `GraphQLHttpClient` and
must dispose it; `true` when constructed via the public constructor, `false` when constructed via
the internal constructor.

##### Key Methods

**GitHubGraphQLClient (public constructor)**: Creates a new client owned by this instance.

- *Parameters*: `string token` — GitHub authentication token; `string? graphqlEndpoint` —
  optional GraphQL endpoint URL; defaults to `https://api.github.com/graphql`.
- *Postconditions*: The instance owns its `HttpClient` and `GraphQLHttpClient` and must be
  disposed by the caller.

**GitHubGraphQLClient (internal constructor)**: Creates a new client using an injected
`HttpClient`; intended for test scenarios.

- *Parameters*: `HttpClient httpClient` — pre-configured HTTP client; `string? graphqlEndpoint` —
  optional GraphQL endpoint URL.
- *Postconditions*: The caller retains ownership of `httpClient`; this instance does not dispose
  it.

**GetCommitsAsync**: Returns all commit SHAs reachable from a branch within an optional date range.

- *Parameters*: `string owner`, `string repo`, `string branch` — repository coordinates; optional
  date filters.
- *Returns*: `Task<List<string>>` — list of commit SHAs; empty list on error.

**GetReleasesAsync**: Returns all release tag names for the repository.

- *Parameters*: `string owner`, `string repo`.
- *Returns*: `Task<List<string>>` — list of release tag names; empty list on error.

**GetAllTagsAsync**: Returns all tag nodes for the repository.

- *Parameters*: `string owner`, `string repo`.
- *Returns*: `Task<List<TagNode>>` — list of tag nodes with name and target commit SHA; empty list
  on error.

**GetPullRequestsAsync**: Returns all pull request nodes for the repository, including description
bodies.

- *Parameters*: `string owner`, `string repo`.
- *Returns*: `Task<List<PullRequestNode>>` — list of pull request nodes including `Body`; empty
  list on error.

**GetAllIssuesAsync**: Returns all issue nodes for the repository across all states, including
description bodies.

- *Parameters*: `string owner`, `string repo`.
- *Returns*: `Task<List<IssueNode>>` — list of issue nodes including `Body`; empty list on error.

**FindIssueIdsLinkedToPullRequestAsync**: Finds issue numbers linked to a specific pull request
via GitHub's closing-issues cross-reference.

- *Parameters*: `string owner`, `string repo`, `int pullRequestNumber`.
- *Returns*: `Task<List<int>>` — list of linked issue numbers; empty list on error.

**Dispose**: Releases resources owned by this instance.

- *Postconditions*: If `_ownsGraphQLClient` is true, disposes the `GraphQLHttpClient` (and the
  underlying `HttpClient`).

##### Error Handling

All API methods catch exceptions from the underlying `GraphQLHttpClient` and return empty lists
rather than propagating them. This allows `GitHubRepoConnector` to continue with partial data
when the GitHub API is transiently unavailable. Because exceptions are silently swallowed, runtime
failures such as network errors, authentication failures, and malformed responses are not directly
observable by the caller; an unexpectedly empty result set is the only visible symptom.

##### Dependencies

- **GraphQLHttpClient** — from the `GraphQL.Client.Http` NuGet package; sends GraphQL queries over
  HTTPS.
- **SystemTextJsonSerializer** — from the `GraphQL.Client.Serializer.SystemTextJson` NuGet
  package; serializes and deserializes GraphQL payloads.
- **GitHubGraphQLTypes** — provides the record types used as deserialization targets.

##### Callers

- **GitHubRepoConnector** — creates and calls `GitHubGraphQLClient` for all GraphQL API
  communication.
