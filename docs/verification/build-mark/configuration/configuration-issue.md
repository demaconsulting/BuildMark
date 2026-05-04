# ConfigurationIssue

## Verification Approach

`ConfigurationIssue` is a record type with no logic. It is verified through
`ConfigurationTests.cs`, which constructs `ConfigurationIssue` instances directly and asserts on
their `FilePath`, `Line`, `Severity`, and `Description` properties. No mocking is required.

## Dependencies

| Mock / Stub | Reason          |
| ----------- | --------------- |
| None        | No mocks needed |

## Test Scenarios

### ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode

**Scenario**: A `ConfigurationIssue` with `Error` severity is constructed and placed in a
`ConfigurationLoadResult`; `ReportTo` is called.

**Expected**: `context.ExitCode` equals 1, confirming the issue record carries severity correctly.

**Requirement coverage**: `BuildMark-ConfigurationIssue-Record`.

### ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode

**Scenario**: A `ConfigurationIssue` with `Warning` severity is constructed and placed in a
`ConfigurationLoadResult`; `ReportTo` is called.

**Expected**: `context.ExitCode` remains 0, confirming severity is preserved and evaluated
correctly.

**Requirement coverage**: `BuildMark-ConfigurationIssue-Record`.

### ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber

**Scenario**: A `ConfigurationIssue` is constructed with `FilePath` `"/repo/.buildmark.yaml"`,
`Line` 7, `Error` severity, and description `"Unexpected value"`; properties are inspected.

**Expected**: `FilePath` equals `"/repo/.buildmark.yaml"`; `Line` equals 7; `Severity` equals
`Error`; `Description` equals `"Unexpected value"`.

**Requirement coverage**: `BuildMark-ConfigurationIssue-Record`.

## Requirements Coverage

- **`BuildMark-ConfigurationIssue-Record`**:
  - ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode
  - ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode
  - ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber
