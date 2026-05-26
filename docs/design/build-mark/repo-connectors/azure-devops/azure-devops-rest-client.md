#### AzureDevOpsRestClient

##### Purpose

`AzureDevOpsRestClient` is the unit responsible for issuing paginated HTTP requests to the Azure
DevOps REST API and deserializing responses into typed `AzureDevOpsApiTypes` records for
consumption by `AzureDevOpsRepoConnector`. All Azure DevOps API communication is delegated to
this client.

The client supports both Basic authentication (PAT) and Bearer authentication (Entra ID access
token). It uses `System.Net.Http.Json` extension methods with a shared `JsonSerializerOptions`
instance (`PropertyNamingPolicy = JsonNamingPolicy.CamelCase`,
`NumberHandling = JsonNumberHandling.AllowReadingFromString`) to deserialize API responses without
per-property attributes. The sole exception is `AzureDevOpsWorkItem.Fields`, which is deserialized
as a `Dictionary<string, object?>` preserving dotted field reference keys verbatim.

When a request returns an HTTP error response, the internal helper `TryReadAdoErrorMessageAsync`
attempts to deserialize the response body as `AzureDevOpsApiError` and extracts the `message`
field to include in the thrown `InvalidOperationException`, providing a human-readable description
instead of a raw HTTP status code.

##### Data Model

**_httpClient**: `HttpClient` — HTTP client configured with the organization URL as the base
address and an `Authorization` header set to either `Basic {base64(pat)}` for PAT tokens or
`Bearer {token}` for Entra ID tokens.

##### Key Methods

**GetRepositoryAsync**: Fetches repository metadata for the specified repository name.

- *Parameters*: `string repository` — repository name.
- *Returns*: `Task<AzureDevOpsRepository>` — repository record containing `Id`, `Name`, and
  `RemoteUrl`.
- *Postconditions*: Throws `InvalidOperationException` on HTTP failure or when deserialization
  returns null.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{repository}?api-version=6.0`

**GetTagsAsync**: Fetches all tag references for the specified repository, resolving annotated tags
to their commit SHA.

- *Parameters*: `string repositoryId` — repository identifier.
- *Returns*: `Task<List<AzureDevOpsRef>>` — list of tag references; `AzureDevOpsRef.CommitId`
  returns `PeeledObjectId ?? ObjectId`.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/refs?filter=tags&peelTags=true
&api-version=6.0`

**GetCommitsAsync**: Fetches the complete paginated commit history for the repository.

- *Parameters*: `string repositoryId` — repository identifier.
- *Returns*: `Task<List<AzureDevOpsCommit>>` — all commits across all pages.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/commits?api-version=6.0`
Paginates using `$top` and `$skip` query parameters.

**GetPullRequestsAsync**: Fetches all pull requests with the specified status for the repository.

- *Parameters*: `string repositoryId` — repository identifier; `string status` — filter value
  (e.g. `all`, `completed`).
- *Returns*: `Task<List<AzureDevOpsPullRequest>>` — all pull requests across all pages.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/pullrequests?
searchCriteria.status={status}&api-version=6.0`. Paginates using `$top` and `$skip`.

**GetPullRequestWorkItemsAsync**: Fetches work item id references linked to a specific pull
request.

- *Parameters*: `string repositoryId` — repository identifier; `int pullRequestId` — pull request
  number.
- *Returns*: `Task<List<AzureDevOpsWorkItemRef>>` — list of linked work item references.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/pullrequests/{prId}/
workitems?api-version=6.0`

**GetWorkItemsAsync**: Batch-fetches work item details for a list of work item IDs. Splits
requests into batches of 200 IDs as required by the Azure DevOps API.

- *Parameters*: `IEnumerable<int> workItemIds` — work item IDs to fetch.
- *Returns*: `Task<List<AzureDevOpsWorkItem>>` — work items with all fields expanded.

Endpoint: `GET /{organization}/{project}/_apis/wit/workitems?ids={ids}&$expand=all&api-version=6.0`

**QueryWorkItemsAsync**: Executes a WIQL query and returns matching work item id references.

- *Parameters*: `string wiql` — the WIQL query string.
- *Returns*: `Task<AzureDevOpsWorkItemQuery>` — result containing the list of matching work item
  references.

Endpoint: `POST /{organization}/{project}/_apis/wit/wiql?api-version=6.0`

##### Error Handling

HTTP and deserialization errors from `HttpClient` propagate to `AzureDevOpsRepoConnector` as
exceptions. `GetRepositoryAsync` additionally throws `InvalidOperationException` when
deserialization succeeds but returns null (guard against an unexpected empty response). When an
error response body is present and parses as `AzureDevOpsApiError`, the `message` field is
included in the `InvalidOperationException` message for human-readable diagnostics.

##### Dependencies

- **AzureDevOpsApiTypes** — provides the record types used as serialization and deserialization
  targets.
- **System.Net.Http.Json** — runtime extension methods used to call `ReadFromJsonAsync<T>` on
  HTTP response content.

##### Callers

- **AzureDevOpsRepoConnector** — creates and calls `AzureDevOpsRestClient` for all REST API
  communication.
