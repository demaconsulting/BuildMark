# AzureDevOpsRestClient

## Overview

`AzureDevOpsRestClient` is the Azure DevOps subsystem unit responsible for issuing
paginated REST API requests to the Azure DevOps API and translating the responses
into typed records for connector consumption. `AzureDevOpsRepoConnector` delegates
all Azure DevOps API communication to this client.

## Authentication

The client authenticates using either:

- **Basic authentication** — the PAT is supplied as the password field of a `Basic`
  authorization header (with an empty username), which is the standard Azure DevOps
  PAT authentication scheme.
- **Bearer authentication** — an Entra ID (Azure AD) access token is supplied as a
  `Bearer` authorization header, used when authenticating via `az account get-access-token`
  or the `SYSTEM_ACCESSTOKEN` Azure Pipelines variable with OAuth scope.

The authentication scheme is selected automatically based on the token source
resolved by `AzureDevOpsRepoConnector`.

## JSON Deserialization

The client uses `System.Net.Http.Json` extension methods (part of the .NET runtime) to
deserialize Azure DevOps REST API responses. Specifically, each HTTP response body is
decoded by calling `HttpContent.ReadFromJsonAsync<T>()` with a shared
`JsonSerializerOptions` instance configured with
`PropertyNamingPolicy = JsonNamingPolicy.CamelCase`. This matches the camelCase
field names returned by the Azure DevOps API without requiring per-property
`[JsonPropertyName]` attributes on the response records.

The sole exception is the `AzureDevOpsWorkItem.Fields` dictionary — its keys are
Azure DevOps field reference names (e.g. `System.WorkItemType`, `Custom.Visibility`)
and are preserved as-is without any naming transformation.

## Methods

The client provides the following methods for retrieving the repository data needed
to build a `BuildInformation` record:

### `GetRepositoryAsync(repository)`

Fetches repository metadata for the specified repository.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{repository}?api-version=7.1`

Returns an `AzureDevOpsRepository` record containing the repository id, name, and
remote URL.

### `GetTagsAsync(repositoryId)`

Fetches all tag references for the specified repository.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/refs?filter=tags&api-version=7.1`

Returns a list of `AzureDevOpsRef` records, each containing the full reference name
(e.g. `refs/tags/v1.0.0`) and the commit SHA it points to.

### `GetCommitsAsync(repositoryId)`

Fetches the complete paginated commit history for the repository.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/commits?api-version=7.1`

Returns a list of `AzureDevOpsCommit` records. Automatically follows continuation
tokens to retrieve all pages.

### `GetPullRequestsAsync(repositoryId, status)`

Fetches all pull requests with the specified status for the repository. Supports
`all`, `active`, `completed`, and `abandoned` status values.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/pullrequests?searchCriteria.status={status}&api-version=7.1`

Returns a list of `AzureDevOpsPullRequest` records. Automatically follows
continuation tokens to retrieve all pages.

### `GetPullRequestWorkItemsAsync(repositoryId, pullRequestId)`

Fetches the work items linked to a specific pull request.

Endpoint: `GET /{organization}/{project}/_apis/git/repositories/{id}/pullrequests/{prId}/workitems?api-version=7.1`

Returns a list of work item id references from the
`AzureDevOpsCollectionResponse<WorkItemRef>`.

### `GetWorkItemsAsync(workItemIds)`

Batch-fetches work item details for a list of work item ids. Splits requests into
batches of 200 ids as required by the Azure DevOps API.

Endpoint: `GET /{organization}/{project}/_apis/wit/workitems?ids={ids}&$expand=all&api-version=7.1`

Returns a list of `AzureDevOpsWorkItem` records with all fields expanded.

### `QueryWorkItemsAsync(wiql)`

Executes a WIQL (Work Item Query Language) query and returns the matching work item
id references.

Endpoint: `POST /{organization}/{project}/_apis/wit/wiql?api-version=7.1`

Returns an `AzureDevOpsWorkItemQuery` record containing the list of matching work
item id references.

## Interactions

- `AzureDevOpsRepoConnector` creates and calls `AzureDevOpsRestClient`.
- `AzureDevOpsApiTypes` provides the request and response record types used for
  serialization and deserialization.
- The Azure DevOps REST API endpoint provides the remote repository data queried by
  the client.
