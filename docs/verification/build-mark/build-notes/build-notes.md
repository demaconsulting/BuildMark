# BuildNotes

## Verification Approach

The BuildNotes subsystem is verified at the integration level through `ProgramTests.cs`
and `RepoConnectorsTests.cs`. There is no dedicated `BuildNotesTests.cs` file; the
subsystem is exercised indirectly whenever `Program.Run` generates a report or when
a connector's `GetBuildInformationAsync` is exercised with mock data.

`BuildInformation.ToMarkdown` is exercised by report generation tests.
`ItemInfo` and `WebLink` data models are exercised through the connector tests that
populate `BuildInformation` instances with change items and web links.

## Dependencies

| Mock / Stub         | Reason                                                    |
| ------------------- | --------------------------------------------------------- |
| `MockRepoConnector` | Returns deterministic `BuildInformation` with known items |
| `Context`           | Provides output capture for markdown assertion            |

## Test Scenarios (Integration)

The following integration tests in `ProgramTests.cs` exercise the BuildNotes subsystem:

### Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues

**Scenario**: A full pipeline run generates a report with known issues included.

**Expected**: `BuildInformation` is populated; markdown report is written to file.

**Requirement coverage**: `BuildMark-BuildNotes-BuildInformation`,
`BuildMark-BuildNotes-ItemInfo`

The following tests in `RepoConnectorsTests.cs` exercise the subsystem through
connector output:

### RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation

**Scenario**: Mock connector returns a fully populated `BuildInformation` instance.

**Expected**: All fields including changes, known issues, and web links are present.

**Requirement coverage**: `BuildMark-BuildNotes-BuildInformation`,
`BuildMark-BuildNotes-ItemInfo`, `BuildMark-BuildNotes-WebLink`

## Requirements Coverage

- **BuildMark-BuildNotes-BuildInformation**: Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues,
  RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation
- **BuildMark-BuildNotes-ItemInfo**: Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues,
  RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation
- **BuildMark-BuildNotes-WebLink**: RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation
