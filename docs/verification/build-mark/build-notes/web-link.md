# WebLink

## Verification Approach

`WebLink` is a utility data model with no dedicated test class. It is verified
indirectly through connector tests that assert on web link properties within
`ItemInfo` and `BuildInformation` instances. The `WebLink` type is used to attach
URLs to items, releases, and changelog links.

## Dependencies

| Mock / Stub         | Reason                                                     |
| ------------------- | ---------------------------------------------------------- |
| `MockRepoConnector` | Returns `BuildInformation` with `WebLink` entries on items |

## Test Scenarios (Integration)

### RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation

**Scenario**: Mock connector returns complete build information including items with web links.

**Expected**: `ItemInfo` entries contain `WebLink` instances with non-null URL and label.

**Requirement coverage**: `BuildMark-BuildNotes-WebLink`

### RepoConnectors_GitHubConnector_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: GitHub connector populates change items with links to GitHub pull request URLs.

**Expected**: `ItemInfo.Link` is a `WebLink` with a valid GitHub URL.

**Requirement coverage**: `BuildMark-BuildNotes-WebLink`

### RepoConnectors_AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: Azure DevOps connector populates change items with links to Azure DevOps URLs.

**Expected**: `ItemInfo.Link` is a `WebLink` with a valid Azure DevOps URL.

**Requirement coverage**: `BuildMark-BuildNotes-WebLink`

## Requirements Coverage

- **BuildMark-BuildNotes-WebLink**:
  RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation,
  RepoConnectors_GitHubConnector_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation,
  RepoConnectors_AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation
