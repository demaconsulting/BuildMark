#### AzureDevOpsRestClient

##### Verification Approach

`AzureDevOpsRestClient` is tested through `AzureDevOpsRestClientTests.cs`, which
contains 8 unit tests. The tests cover successful data retrieval for tags, commits,
work items, and work item links, as well as HTTP error handling and invalid JSON
handling.

##### Dependencies

| Mock / Stub              | Reason                                                       |
| ------------------------ | ------------------------------------------------------------ |
| `MockHttpMessageHandler` | Intercepts all HTTP calls to the Azure DevOps REST endpoints |

##### Test Scenarios

###### AzureDevOpsRestClient_GetTagsAsync_ValidResponse_ReturnsTags

**Scenario**: Valid REST API response for the tags endpoint.

**Expected**: Returns the list of tags from the response.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsRestClient`

###### AzureDevOpsRestClient_GetTagsAsync_HttpError_ReturnsEmptyList

**Scenario**: Tags endpoint returns a non-success status code.

**Expected**: Returns an empty list without throwing.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsRestClient`

###### AzureDevOpsRestClient_GetCommitsAsync_ValidResponse_ReturnsCommits

**Scenario**: Valid REST API response for the commits endpoint.

**Expected**: Returns the list of commits from the response.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsRestClient`

###### AzureDevOpsRestClient_GetCommitsAsync_HttpError_ReturnsEmptyList

**Scenario**: Commits endpoint returns a non-success status code.

**Expected**: Returns an empty list without throwing.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsRestClient`

###### AzureDevOpsRestClient_GetWorkItemsAsync_ValidResponse_ReturnsWorkItems

**Scenario**: Valid REST API response for the work items endpoint.

**Expected**: Returns the list of work items from the response.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsRestClient`

###### AzureDevOpsRestClient_GetWorkItemsAsync_HttpError_ReturnsEmptyList

**Scenario**: Work items endpoint returns a non-success status code.

**Expected**: Returns an empty list without throwing.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsRestClient`

###### AzureDevOpsRestClient_GetWorkItemLinksAsync_ValidResponse_ReturnsWorkItemLinks

**Scenario**: Valid REST API response for the work item links endpoint.

**Expected**: Returns the list of work item links from the response.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsRestClient`

###### AzureDevOpsRestClient_GetWorkItemLinksAsync_HttpError_ReturnsEmptyList

**Scenario**: Work item links endpoint returns a non-success status code.

**Expected**: Returns an empty list without throwing.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsRestClient`

##### Requirements Coverage

- **BuildMark-RepoConnectors-AzureDevOpsRestClient**: All 8 tests in
  `AzureDevOpsRestClientTests.cs`
