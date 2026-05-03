# BuildMarkConfigReader

## Verification Approach

`BuildMarkConfigReader` is verified indirectly through `ProgramTests.cs`. The lint
and report tests exercise `BuildMarkConfigReader.ReadAsync` as part of the program
execution pipeline. When `Lint = true`, `ReadAsync` is called and the result is
reported to the context. When a report is generated, `ReadAsync` is called to load
optional configuration before connector creation.

## Dependencies

| Mock / Stub | Reason                                                  |
| ----------- | ------------------------------------------------------- |
| File system | `ReadAsync` attempts to read `.buildmark.yaml` from the |
|             | current directory; tests control file presence          |

## Test Scenarios (Integration)

### Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero

**Scenario**: `Program.Run` calls `BuildMarkConfigReader.ReadAsync` when no
`.buildmark.yaml` is present.

**Expected**: Reader returns a result with no errors; exit code is 0.

**Requirement coverage**: `BuildMark-Configuration-BuildMarkConfigReader`

### Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues

**Scenario**: `Program.Run` calls `BuildMarkConfigReader.ReadAsync` before report
generation; configuration may or may not be present.

**Expected**: Reader returns without error; report generation proceeds.

**Requirement coverage**: `BuildMark-Configuration-BuildMarkConfigReader`

## Requirements Coverage

- **BuildMark-Configuration-BuildMarkConfigReader**:
  Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero,
  Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues
