# GitHubGraphQLTypes

## Overview

`GitHubGraphQLTypes` is the collection of internal record types used by the
GitHub subsystem to represent GraphQL request and response payloads. These types
allow `GitHubGraphQLClient` to deserialize GitHub API responses into strongly
typed objects that `GitHubRepoConnector` can process safely and predictably.

## Responsibilities

- Represent tag, release, issue, pull request, and commit response nodes
- Carry pagination cursors and connection metadata
- Preserve issue and pull request description body fields for item-controls
  parsing

## Interactions

- `GitHubGraphQLClient` uses these records as serialization and deserialization
  targets for GraphQL HTTP traffic.
- `GitHubRepoConnector` consumes the deserialized node data returned by
  `GitHubGraphQLClient`.
