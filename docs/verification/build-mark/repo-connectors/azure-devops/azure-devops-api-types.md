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

##### Test Scenarios (via AzureDevOpsRestClientTests.cs)

###### AzureDevOpsRestClient_GetTagsAsync_ValidResponse_ReturnsTags

**Scenario**: Tag REST response is deserialized into tag DTOs.

**Expected**: Tag DTO fields are populated correctly.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsApiTypes`

###### AzureDevOpsRestClient_GetCommitsAsync_ValidResponse_ReturnsCommits

**Scenario**: Commits REST response is deserialized into commit DTOs.

**Expected**: Commit DTO fields are populated correctly.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsApiTypes`

###### AzureDevOpsRestClient_GetWorkItemsAsync_ValidResponse_ReturnsWorkItems

**Scenario**: Work items REST response is deserialized into work item DTOs.

**Expected**: Work item DTO fields are populated correctly.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsApiTypes`

###### AzureDevOpsRestClient_GetWorkItemLinksAsync_ValidResponse_ReturnsWorkItemLinks

**Scenario**: Work item links REST response is deserialized into work item link DTOs.

**Expected**: Work item link DTO fields are populated correctly.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOpsApiTypes`

##### Requirements Coverage

- **BuildMark-RepoConnectors-AzureDevOpsApiTypes**: Verified indirectly through all
  8 tests in `AzureDevOpsRestClientTests.cs` and all 10 tests in
  `WorkItemMapperTests.cs`
