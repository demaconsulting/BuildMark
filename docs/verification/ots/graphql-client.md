## GraphQL.Client

### Verification Approach

GraphQL.Client is an OTS GraphQL client library used only by `GitHubGraphQLClient` to query the
GitHub GraphQL API. It is verified through the `GitHubGraphQLClient` unit tests in
`DemaConsulting.BuildMark.Tests`, which pass an `HttpClient` over a mocked
`HttpMessageHandler` to the internal constructor. GraphQL.Client and its System.Text.Json
serializer are not mocked, so each test exercises request serialization, the HTTP exchange, and
response deserialization; only the network endpoint is replaced. Because `GitHubGraphQLClient`
catches every exception, the failure behavior is verified by `GraphQLClientTests.cs` in
`OtsSoftwareTests`, which drives `GraphQLHttpClient` directly against the same mocked handler. The
tests run on every operating system and .NET version in the CI matrix.

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

**GraphQL.Client reports failures**: `GraphQLHttpClient.SendQueryAsync` is called directly, without
the wrapper, against a mocked endpoint that answers with an HTTP 404 status and then with invalid
JSON. It must throw a `GraphQLHttpRequestException` carrying the status code for the HTTP error and
a `JsonException` for the invalid JSON. This scenario is tested by
`GraphQLClient_SendQueryAsync_HttpError_ThrowsGraphQLHttpRequestException` and
`GraphQLClient_SendQueryAsync_InvalidJson_ThrowsJsonException`. That `GitHubGraphQLClient` turns
these exceptions into an empty list is verified by its own tests for `GetCommitsAsync`,
`GetReleasesAsync`, `GetAllTagsAsync`, `GetPullRequestsAsync`, `GetAllIssuesAsync`, and
`FindIssueIdsLinkedToPullRequestAsync` (see the *GitHubGraphQLClient* verification section).
