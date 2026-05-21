#### AzureDevOpsApiTypes

##### Verification Approach

`AzureDevOpsApiTypes` contains the DTO types used to deserialize Azure DevOps REST API
responses. These types have no dedicated test class; they are verified indirectly
through `AzureDevOpsRestClientTests.cs` tests that exercise JSON deserialization of
mocked API responses and through `WorkItemMapperTests.cs` tests that consume the
deserialized data.

##### Dependencies

| Mock / Stub              | Reason                                                       |
| ------------------------ | ------------------------------------------------------------ |
| `MockHttpMessageHandler` | Provides JSON payloads whose structure matches the DTO types |

##### Test Environment

Tests use `MockHttpMessageHandler` to intercept HTTP calls. No real network access or Azure DevOps token is required.

##### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

##### Test Scenarios (via AzureDevOpsRestClientTests.cs)

###### AzureDevOpsRestClient_GetTagsAsync_ValidResponse_ReturnsTags

**Scenario**: Tag REST response is deserialized into tag DTOs.

**Expected**: Tag DTO fields are populated correctly.

**Requirement coverage**: `BuildMark-AzureDevOps-ApiTypes`

###### AzureDevOpsRestClient_GetCommitsAsync_ValidResponse_ReturnsCommits

**Scenario**: Commits REST response is deserialized into commit DTOs.

**Expected**: Commit DTO fields are populated correctly.

**Requirement coverage**: `BuildMark-AzureDevOps-ApiTypes`

###### AzureDevOpsRestClient_GetWorkItemsAsync_ValidResponse_ReturnsWorkItems

**Scenario**: Work items REST response is deserialized into work item DTOs.

**Expected**: Work item DTO fields are populated correctly.

**Requirement coverage**: `BuildMark-AzureDevOps-ApiTypes`

###### AzureDevOpsRestClient_GetPullRequestsAsync_ValidResponse_ReturnsPullRequests

**Scenario**: Pull requests REST response is deserialized into pull request DTOs.

**Expected**: Pull request DTO fields are populated correctly.

**Requirement coverage**: `BuildMark-AzureDevOps-ApiTypes`

##### Requirements Coverage

- **BuildMark-AzureDevOps-ApiTypes**: Verified indirectly through all 12 tests in
  `AzureDevOpsRestClientTests.cs` and all 13 tests in `WorkItemMapperTests.cs`
