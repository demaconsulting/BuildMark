#### GitHubGraphQLTypes

##### Purpose

`GitHubGraphQLTypes` is the collection of internal record types used by the
GitHub subsystem to represent GraphQL request and response payloads. These types
allow `GitHubGraphQLClient` to deserialize GitHub API responses into strongly
typed objects that `GitHubRepoConnector` can process safely and predictably.

##### Responsibilities

- Represent tag, release, issue, pull request, and commit response nodes
- Carry pagination cursors and connection metadata
- Preserve issue and pull request description body fields for item-controls
  parsing

##### Data Model

The following record types are defined for GitHub GraphQL request and response serialization:

- **Tag and release nodes**: types representing GitHub tag and release response objects,
  including tag names and associated commit SHAs
- **Issue and pull request nodes**: `IssueNode` and `PullRequestNode` records carrying
  number, title, URL, state, label connections, and description body fields
- **Commit nodes**: records carrying commit SHAs from GraphQL commit range queries
- **Pagination types**: `PageInfo`, connection, and edge wrapper types carrying `endCursor`
  and `hasNextPage` fields for cursor-based pagination across all paginated queries
- **Request payload types**: GraphQL query request wrappers used to serialize requests to
  the GitHub API endpoint

##### Key Methods

N/A — `GitHubGraphQLTypes` is a collection of record type definitions with no methods
beyond C# record-generated members.

##### Error Handling

N/A — These are immutable record types used purely for JSON deserialization. No methods
detect or propagate errors.

##### Interactions

- `GitHubGraphQLClient` uses these records as serialization and deserialization
  targets for GraphQL HTTP traffic.
- `GitHubRepoConnector` consumes the deserialized node data returned by
  `GitHubGraphQLClient`.
