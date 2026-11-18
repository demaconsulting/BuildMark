## GraphQL.Client

### Verification Approach

GraphQL.Client is an OTS GraphQL client library used only by `GitHubGraphQLClient` to query the
GitHub GraphQL API. It is verified through the `GitHubGraphQLClient` unit tests in
`DemaConsulting.BuildMark.Tests`, which pass an `HttpClient` over a mocked
`HttpMessageHandler` to the internal constructor. GraphQL.Client and its System.Text.Json
serializer are not mocked, so each test exercises request serialization, the HTTP exchange, and
response deserialization; only the network endpoint is replaced. The tests run on every operating
system and .NET version in the CI matrix.

### Evidence

The tests run as part of `dotnet test` in `build.ps1` and in the CI `build` job. Their TRX
results are consumed by ReqStream to satisfy the GraphQL.Client OTS requirements.

### Test Scenarios

**GraphQL.Client sends query variables**: Each paginated query is answered page by page by a
handler that returns the next page only when the previous page's cursor is in the request body.
All pages must be returned, confirming that the `after` variable is serialized into the request.
This scenario is tested by the `*_WithPagination_*` tests of `GetCommitsAsync`,
`GetReleasesAsync`, `GetAllTagsAsync`, `GetPullRequestsAsync`, `GetAllIssuesAsync`, and
`FindIssueIdsLinkedToPullRequestAsync`.

**GraphQL.Client deserializes responses**: Each query is answered with a valid GitHub response.
The values must reach the returned list, confirming that the response is deserialized into the
typed response records. This scenario is tested by the `*_ValidResponse_*` tests of the same
six methods.

**GraphQL.Client reports failures**: Each query is answered with an HTTP error status or with
invalid JSON. GraphQL.Client must throw, which `GitHubGraphQLClient` converts to an empty list.
This scenario is tested by the `*_HttpError_ReturnsEmptyList` and `*_InvalidJson_ReturnsEmptyList`
tests of `GetCommitsAsync`, `GetReleasesAsync`, `GetAllTagsAsync`, `GetPullRequestsAsync`, and
`FindIssueIdsLinkedToPullRequestAsync`.
