# GitHubGraphQLClient

## Overview

`GitHubGraphQLClient` is the GitHub subsystem unit responsible for issuing
paginated GraphQL requests to the GitHub API and translating the responses into
typed records for connector consumption. `GitHubRepoConnector` delegates all
GitHub API communication to this client.

## Constructors

The class provides two constructors:

- **Public constructor** - accepts a GitHub authentication token and creates an
  owned `HttpClient` configured with the token. Used by `GitHubRepoConnector` in
  production.
- **Internal constructor** - accepts an existing `HttpClient` directly. Used by
  tests to inject a mock `HttpClient` without network access.

## Lifecycle

`GitHubGraphQLClient` implements `IDisposable`. When created via the public
constructor, the instance owns its `HttpClient` and disposes it when the client
is disposed. When created via the internal constructor, the caller retains
ownership of the `HttpClient` and the client does not dispose it.

Callers that construct `GitHubGraphQLClient` via the public constructor must
wrap usage in a `using` statement or otherwise dispose the instance to release
the underlying HTTP connection resources.

## Error Strategy

All API methods catch exceptions from the underlying `HttpClient` and return
empty lists rather than propagating the exception to the caller. This allows
the connector to continue with partial data when the GitHub API is transiently
unavailable.

## Methods

The client provides methods for retrieving the repository data needed to build a
`BuildInformation` record:

- `GetCommitsAsync` for commit SHAs in a range
- `GetReleasesAsync` for release tag names
- `GetAllTagsAsync` for tag nodes
- `GetPullRequestsAsync` for pull request data, including description bodies
- `GetAllIssuesAsync` for issue data, including description bodies
- `FindIssueIdsLinkedToPullRequestAsync` for cross-link lookups

## Interactions

- `GitHubRepoConnector` creates and calls `GitHubGraphQLClient`.
- `GitHubGraphQLTypes` provide the request and response record types used for
  serialization and deserialization.
- The GitHub GraphQL endpoint provides the remote repository data queried by the
  client.
