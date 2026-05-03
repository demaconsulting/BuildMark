# Azure DevOps

## Verification Approach

The Azure DevOps sub-subsystem is verified through `AzureDevOpsTests.cs` (5 subsystem-
level tests), `AzureDevOpsRepoConnectorTests.cs` (25 unit tests),
`AzureDevOpsRestClientTests.cs` (8 unit tests), and `WorkItemMapperTests.cs` (10 unit
tests). The subsystem tests exercise the full Azure DevOps data pipeline through mock
HTTP responses. The unit tests are described in the individual unit chapters.

## Dependencies

| Mock / Stub              | Reason                                             |
| ------------------------ | -------------------------------------------------- |
| `MockHttpMessageHandler` | Intercepts HTTP calls to the Azure DevOps REST API |

## Test Scenarios (Subsystem-Level, AzureDevOpsTests.cs)

### AzureDevOps_ImplementsInterface_ReturnsTrue

**Scenario**: `AzureDevOpsRepoConnector` is checked against `IRepoConnector`.

**Expected**: Implements the interface.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

### AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: Azure DevOps connector receives mocked REST API data.

**Expected**: Returns valid `BuildInformation` with correct fields.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOps_GetBuildInformation_WithWorkItems_GathersChanges

**Scenario**: Mock data includes work items linked to commits.

**Expected**: Work items appear in `BuildInformation.Changes`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOps_GetBuildInformation_WithOpenBugs_IdentifiesKnownIssues

**Scenario**: Mock data includes open bug work items.

**Expected**: Bugs appear in `BuildInformation.KnownIssues`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOps_GetBuildInformation_ReleaseVersion_SkipsPreReleases

**Scenario**: Build version is a release; pre-release tags in history are skipped.

**Expected**: Baseline is the previous release tag.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

## Requirements Coverage

- **BuildMark-RepoConnectors-IRepoConnector**: AzureDevOps_ImplementsInterface_ReturnsTrue
- **BuildMark-RepoConnectors-AzureDevOps**: AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation,
  AzureDevOps_GetBuildInformation_WithWorkItems_GathersChanges,
  AzureDevOps_GetBuildInformation_WithOpenBugs_IdentifiesKnownIssues,
  AzureDevOps_GetBuildInformation_ReleaseVersion_SkipsPreReleases
