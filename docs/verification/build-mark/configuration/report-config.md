# ReportConfig

## Verification Approach

`ReportConfig` is a data model class verified indirectly through `ProgramTests.cs`.
Report configuration fields (`File`, `Depth`, `IncludeKnownIssues`) influence the
behavior of `Program.ProcessBuildNotes`. The test
`Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues`
exercises the `IncludeKnownIssues` field path.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios (Integration)

### Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues

**Scenario**: `ReportConfig.IncludeKnownIssues` is set; report generation includes
known issues.

**Expected**: Generated report contains a known issues section.

**Requirement coverage**: `BuildMark-Configuration-ReportConfig`

## Requirements Coverage

- **BuildMark-Configuration-ReportConfig**:
  Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues
