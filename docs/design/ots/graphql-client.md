## GraphQL.Client Integration Design

### Purpose

GraphQL.Client is the established GraphQL client library for .NET. BuildMark uses it to query the
GitHub GraphQL API for the commits, releases, tags, pull requests, and issues of a repository. It
was chosen because it sends typed GraphQL requests with variables over a caller-supplied
`HttpClient`, which lets BuildMark set the GitHub authentication headers and lets the tests
replace the network with a mocked `HttpMessageHandler`.

### Features Used

BuildMark uses the `GraphQL.Client` package and its `GraphQL.Client.Serializer.SystemTextJson`
serializer package:

| Feature | Usage in BuildMark |
| --- | --- |
| `GraphQLHttpClient` | Sends each query to the GitHub GraphQL endpoint |
| `GraphQLHttpClientOptions.EndPoint` | Selects the public GitHub or a GitHub Enterprise endpoint |
| `SystemTextJsonSerializer` | Serializes requests and deserializes responses with System.Text.Json |
| `GraphQLRequest` | Holds the query text and its variables (owner, repository, cursor) |
| `SendQueryAsync<T>` | Sends the request and returns the typed `GraphQLResponse<T>` |
| `GraphQLResponse<T>.Data` | The typed response data read by `GitHubGraphQLClient` |

### Integration Pattern

GraphQL.Client is used only inside the `GitHubGraphQLClient` unit of the GitHub subsystem. No
GraphQL.Client type appears in any public signature, so callers see only lists of BuildMark
values built from the typed response records in `GitHubGraphQLTypes`.

1. `GitHubGraphQLClient` creates a `GraphQLHttpClient` with the `SystemTextJsonSerializer` over an
   `HttpClient` that carries the bearer token and the user agent. A second, internal constructor
   accepts an `HttpClient` from the caller so that tests can mock the endpoint.
2. Each query method builds a `GraphQLRequest` with the query text and variables and calls
   `SendQueryAsync<T>` with the response record type for that query.
3. Paginated queries pass the `endCursor` of the previous page as the `after` variable until
   `hasNextPage` is false.
4. Any exception from GraphQL.Client (an HTTP error status, invalid JSON, or a network failure)
   is caught by the query method, which returns an empty list.

`GitHubGraphQLClient` implements `IDisposable` and disposes the `GraphQLHttpClient` it created
when it is disposed; a caller that passes in its own `HttpClient` disposes that client itself.
