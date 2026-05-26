#### AzureDevOpsRestClient

##### Verification Approach

`AzureDevOpsRestClient` is tested through `AzureDevOpsRestClientTests.cs`, which
contains 12 unit tests. The tests cover successful data retrieval for repository
metadata, tags, commits, pull requests, work items, and WIQL queries, as well as
response variants (string-valued ids) and error handling (HTTP errors, empty inputs).

##### Dependencies

| Mock / Stub              | Reason                                                       |
| ------------------------ | ------------------------------------------------------------ |
| `MockHttpMessageHandler` | Intercepts all HTTP calls to the Azure DevOps REST endpoints |

##### Test Environment

Tests use `MockHttpMessageHandler` to intercept HTTP calls. No real network access or Azure DevOps token is required.

##### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

##### Test Scenarios

###### AzureDevOpsRestClient_GetRepositoryAsync_ValidResponse_ReturnsRepository

**Scenario**: Valid REST API response for the repository metadata endpoint.

**Expected**: Returns a repository record with correct `Id`, `Name`, and `RemoteUrl`.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_GetRepositoryAsync_HttpError_ThrowsHttpRequestException

**Scenario**: Repository metadata endpoint returns an HTTP error response (e.g., 404 Not Found).

**Expected**: `GetRepositoryAsync` throws `HttpRequestException`.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_GetCommitsAsync_ValidResponse_ReturnsCommits

**Scenario**: Valid REST API response for the commits endpoint.

**Expected**: Returns the list of commits from the response.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_GetTagsAsync_ValidResponse_ReturnsTags

**Scenario**: Valid REST API response for the tags endpoint.

**Expected**: Returns the list of tags from the response.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_GetPullRequestsAsync_ValidResponse_ReturnsPullRequests

**Scenario**: Valid REST API response for the pull requests endpoint.

**Expected**: Returns the list of pull requests from the response.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_GetPullRequestWorkItemsAsync_ValidResponse_ReturnsWorkItemRefs

**Scenario**: Valid REST API response for the PR work items endpoint.

**Expected**: Returns the list of work item references from the response.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_GetPullRequestWorkItemsAsync_StringValuedIds_DeserializesCorrectly

**Scenario**: PR work items endpoint returns ids serialized as JSON strings rather than numbers.

**Expected**: Work item ids are deserialized as integers.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_GetWorkItemsAsync_ValidResponse_ReturnsWorkItems

**Scenario**: Valid REST API response for the work items endpoint.

**Expected**: Returns the list of work items from the response.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_QueryWorkItemsAsync_ValidWiql_ReturnsWorkItemIds

**Scenario**: Valid REST API response for the WIQL query endpoint.

**Expected**: Returns the list of work item ids from the WIQL query result.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_QueryWorkItemsAsync_StringValuedIds_DeserializesCorrectly

**Scenario**: WIQL query endpoint returns ids serialized as JSON strings rather than numbers.

**Expected**: Work item ids are deserialized as integers.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_QueryWorkItemsAsync_WithHttpError_ThrowsInvalidOperationException

**Scenario**: WIQL query endpoint returns an HTTP error response (e.g., 400 Bad Request).

**Expected**: `QueryWorkItemsAsync` throws `InvalidOperationException`.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`

###### AzureDevOpsRestClient_GetWorkItemsAsync_WithEmptyInput_ReturnsEmptyList

**Scenario**: `GetWorkItemsAsync` is called with an empty list of work item ids.

**Expected**: Returns an empty list without making any HTTP call.

**Requirement coverage**: `BuildMark-AzureDevOps-RestClient`
