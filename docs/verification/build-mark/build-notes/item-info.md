# ItemInfo

## Verification Approach

`ItemInfo` is a data model with no dedicated test class. It is verified indirectly
through connector tests in `RepoConnectorsTests.cs`, `GitHubRepoConnectorTests.cs`,
`AzureDevOpsRepoConnectorTests.cs`, and `MockRepoConnectorTests.cs` that assert on
the items contained in `BuildInformation` instances returned by connectors.

## Dependencies

| Mock / Stub         | Reason                                                   |
| ------------------- | -------------------------------------------------------- |
| `MockRepoConnector` | Returns `BuildInformation` with known `ItemInfo` entries |

## Test Scenarios (Integration)

### RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation

**Scenario**: Mock connector returns complete build information with change items.

**Expected**: `BuildInformation.Changes` contains `ItemInfo` entries with correct
title, type, visibility, and web link.

**Requirement coverage**: `BuildMark-BuildNotes-ItemInfo`

### RepoConnectors_GitHubConnector_GetBuildInformation_WithPullRequests_GathersChanges

**Scenario**: GitHub connector processes pull requests into change items.

**Expected**: Each pull request is represented as an `ItemInfo` in `BuildInformation.Changes`.

**Requirement coverage**: `BuildMark-BuildNotes-ItemInfo`

### RepoConnectors_AzureDevOps_GetBuildInformation_WithPullRequests_GathersChanges

**Scenario**: Azure DevOps connector processes pull requests into change items.

**Expected**: Each pull request is represented as an `ItemInfo` in `BuildInformation.Changes`.

**Requirement coverage**: `BuildMark-BuildNotes-ItemInfo`

## Requirements Coverage

- **BuildMark-BuildNotes-ItemInfo**:
  RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation,
  RepoConnectors_GitHubConnector_GetBuildInformation_WithPullRequests_GathersChanges,
  RepoConnectors_AzureDevOps_GetBuildInformation_WithPullRequests_GathersChanges
