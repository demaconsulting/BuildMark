# BuildInformation

## Verification Approach

`BuildInformation` is a data model with no dedicated test class. It is verified
indirectly through integration tests in `ProgramTests.cs` and `RepoConnectorsTests.cs`
that exercise the full pipeline and assert on the structure and content of
`BuildInformation` instances returned by connectors.

## Dependencies

| Mock / Stub         | Reason                                             |
| ------------------- | -------------------------------------------------- |
| `MockRepoConnector` | Returns deterministic `BuildInformation` instances |

## Test Scenarios (Integration)

### Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues

**Scenario**: `Program.Run` generates a report using a mock connector.

**Expected**: `BuildInformation.ToMarkdown` produces valid markdown output including
known issues section.

**Requirement coverage**: `BuildMark-BuildNotes-BuildInformation`

### RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation

**Scenario**: Mock connector's `GetBuildInformationAsync` returns a complete result.

**Expected**: `BuildInformation` instance contains version, baseline, changes,
known issues, and routed sections.

**Requirement coverage**: `BuildMark-BuildNotes-BuildInformation`

### RepoConnectors_GitHubConnector_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: GitHub connector returns `BuildInformation` from mocked API responses.

**Expected**: `BuildInformation` fields are correctly populated from the mock data.

**Requirement coverage**: `BuildMark-BuildNotes-BuildInformation`

### RepoConnectors_AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: Azure DevOps connector returns `BuildInformation` from mocked API responses.

**Expected**: `BuildInformation` fields are correctly populated from the mock data.

**Requirement coverage**: `BuildMark-BuildNotes-BuildInformation`

## Requirements Coverage

- **BuildMark-BuildNotes-BuildInformation**:
  Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues,
  RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation,
  RepoConnectors_GitHubConnector_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation,
  RepoConnectors_AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation
