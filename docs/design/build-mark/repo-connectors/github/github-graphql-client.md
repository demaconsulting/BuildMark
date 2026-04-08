# GitHubGraphQLClient

## Overview

`GitHubGraphQLClient` is the GitHub subsystem unit responsible for issuing
paginated GraphQL requests to the GitHub API and translating the responses into
typed records for connector consumption. `GitHubRepoConnector` delegates all
GitHub API communication to this client.

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
