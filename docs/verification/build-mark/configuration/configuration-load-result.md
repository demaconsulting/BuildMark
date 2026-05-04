# ConfigurationLoadResult

## Verification Approach

`ConfigurationLoadResult` is verified with dedicated unit tests in `ConfigurationTests.cs`. Tests
construct `ConfigurationLoadResult` instances directly with controlled `ConfigurationIssue` entries
and assert on the behavior of `ReportTo(context)`. No mocking is required.

## Dependencies

| Mock / Stub | Reason          |
| ----------- | --------------- |
| None        | No mocks needed |

## Test Scenarios

### ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode

**Scenario**: A `ConfigurationLoadResult` containing one `Error`-severity issue is created;
`ReportTo` is called on a silent `Context`.

**Expected**: `context.ExitCode` equals 1.

**Requirement coverage**: `BuildMark-ConfigLoadResult-ReportTo`.

### ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode

**Scenario**: A `ConfigurationLoadResult` containing one `Warning`-severity issue is created;
`ReportTo` is called on a silent `Context`.

**Expected**: `context.ExitCode` remains 0.

**Requirement coverage**: `BuildMark-ConfigLoadResult-ReportTo`.

### ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber

**Scenario**: A `ConfigurationLoadResult` containing an `Error`-severity issue at `FilePath`
`"/repo/.buildmark.yaml"`, `Line` 7, with description `"Unexpected value"` is created; `ReportTo`
is called.

**Expected**: `result.HasErrors` is true; `context.ExitCode` is 1;
`result.Issues[0].FilePath` equals `"/repo/.buildmark.yaml"`; `result.Issues[0].Line` equals 7;
`result.Issues[0].Description` equals `"Unexpected value"`.

**Requirement coverage**: `BuildMark-ConfigLoadResult-ReportTo`.

## Requirements Coverage

- **`BuildMark-ConfigLoadResult-ReportTo`**:
  - ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode
  - ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode
  - ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber
