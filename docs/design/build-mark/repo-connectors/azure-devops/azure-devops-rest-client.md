#### AzureDevOpsRestClient

##### Purpose

`AzureDevOpsRestClient` is the Azure DevOps subsystem unit responsible for issuing
paginated REST API requests to the Azure DevOps API and translating the responses
into typed records for connector consumption. `AzureDevOpsRepoConnector` delegates
all Azure DevOps API communication to this client.

##### Authentication

The client authenticates using either:

- **Basic authentication** - the PAT is supplied as the password field of a `Basic`
  authorization header (with an empty username), which is the standard Azure DevOps
  PAT authentication scheme.
- **Bearer authentication** - an Entra ID (Azure AD) access token is supplied as a
  `Bearer` authorization header, used when authenticating via `az account get-access-token`
  or the `SYSTEM_ACCESSTOKEN` Azure Pipelines variable with OAuth scope.

The authentication scheme is selected automatically based on the token source
resolved by `AzureDevOpsRepoConnector`.

##### JSON Deserialization

The client uses `System.Net.Http.Json` extension methods (part of the .NET runtime) to
deserialize Azure DevOps REST API responses. Specifically, each HTTP response body is
decoded by calling `HttpContent.ReadFromJsonAsync<T>()` with a shared
`JsonSerializerOptions` instance configured with
`PropertyNamingPolicy = JsonNamingPolicy.CamelCase` and
`NumberHandling = JsonNumberHandling.AllowReadingFromString`. The camelCase policy
matches the camelCase field names returned by the Azure DevOps API without requiring
per-property `[JsonPropertyName]` attributes on the response records. The
`AllowReadingFromString` setting handles numeric fields (such as work item IDs) that
the API may return as JSON string values rather than JSON numbers.

The sole exception is the `AzureDevOpsWorkItem.Fields` dictionary - its keys are
Azure DevOps field reference names (e.g. `System.WorkItemType`, `Custom.Visibility`)
and are preserved as-is without any naming transformation.

##### Data Model

`AzureDevOpsRestClient` holds a single `HttpClient` instance configured with the
organization URL as the base address and the resolved authentication header (Basic for
PAT tokens, Bearer for Entra ID tokens).

##### Key Methods

The client provides the following methods for retrieving the repository data needed
to build a `BuildInformation` record:

###### `GetRepositoryAsync(repository)`

Fetches repository metadata for the specified repository.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{repository}?api-version=6.0`

Returns an `AzureDevOpsRepository` record containing the repository id, name, and
remote URL. Returns `null` when the repository cannot be found (HTTP 404).

###### `GetTagsAsync(repositoryId)`

Fetches all tag references for the specified repository.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/refs?filter=tags&peelTags=true&api-version=6.0`

Returns a list of `AzureDevOpsRef` records, each containing the full reference name
(e.g. `refs/tags/v1.0.0`), the object SHA it points to, and for annotated tags the
peeled commit SHA. The `peelTags=true` query parameter instructs the API to resolve
annotated tag objects to their underlying commit SHA, which is returned in the
`peeledObjectId` field.

###### `GetCommitsAsync(repositoryId)`

Fetches the complete paginated commit history for the repository.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/commits?api-version=6.0`

Returns a list of `AzureDevOpsCommit` records. Automatically paginates using
`$top` and `$skip` query parameters to retrieve all pages.

###### `GetPullRequestsAsync(repositoryId, status)`

Fetches all pull requests with the specified status for the repository. Supports
`all`, `active`, `completed`, and `abandoned` status values.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/pullrequests?searchCriteria.status={status}&api-version=6.0`

Returns a list of `AzureDevOpsPullRequest` records. Automatically paginates using
`$top` and `$skip` query parameters to retrieve all pages.

###### `GetPullRequestWorkItemsAsync(repositoryId, pullRequestId)`

Fetches the work items linked to a specific pull request.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/pullrequests/{prId}/workitems?api-version=6.0`

Returns a list of work item id references from the
`AzureDevOpsCollectionResponse<WorkItemRef>`.

###### `GetWorkItemsAsync(workItemIds)`

Batch-fetches work item details for a list of work item ids. Splits requests into
batches of 200 ids as required by the Azure DevOps API.

Endpoint: `GET /{organization}/{project}/_apis/wit/workitems?ids={ids}&$expand=all&api-version=6.0`

Returns a list of `AzureDevOpsWorkItem` records with all fields expanded.

###### `QueryWorkItemsAsync(wiql)`

Executes a WIQL (Work Item Query Language) query and returns the matching work item
id references.

Endpoint: `POST /{organization}/{project}/_apis/wit/wiql?api-version=6.0`

Returns an `AzureDevOpsWorkItemQuery` record containing the list of matching work
item id references.

##### Error Handling

HTTP and deserialization errors from the underlying `HttpClient` are propagated to
`AzureDevOpsRepoConnector`. Network failures, authentication errors (HTTP 401/403), and
malformed JSON responses result in exceptions that propagate up to
`Program.ProcessBuildNotes`.

`GetRepositoryAsync` additionally throws `InvalidOperationException` when the HTTP
response is successful but JSON deserialization returns `null` (the `result ?? throw`
pattern). This guards against a response body that is valid JSON but does not
deserialize into the expected `AzureDevOpsRepository` record.

When an HTTP error response includes a JSON body in the Azure DevOps error format, the
internal `TryReadAdoErrorMessageAsync` helper deserializes the body into an
`AzureDevOpsApiError` record and extracts the `message` field to include in the thrown
`InvalidOperationException`. This provides a human-readable error description (e.g.,
`"The area path does not exist."`) rather than a raw HTTP status code.

##### Interactions

- `AzureDevOpsRepoConnector` creates and calls `AzureDevOpsRestClient`.
- `AzureDevOpsApiTypes` provides the request and response record types used for
  serialization and deserialization.
- The Azure DevOps REST API endpoint provides the remote repository data queried by
  the client.
