### ConfigurationLoadResult

#### Verification Approach

`ConfigurationLoadResult` is verified with unit tests in `ConfigurationTests.cs`. Tests construct
`ConfigurationLoadResult` instances directly with controlled `ConfigurationIssue` entries and call
`ReportTo` on a silent `Context`. No mocking is required; the real `Context` is used in silent
mode.

#### Test Environment

N/A - standard test environment.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` targeting `ConfigurationLoadResult` pass with zero
  failures.

#### Test Scenarios

**ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode**: A `ConfigurationLoadResult`
containing one `Error`-severity `ConfigurationIssue` is created and `ReportTo` is called on a
silent `Context`. The context exit code must equal 1, confirming that error-severity issues cause
the process to report a failure. This scenario is tested by
`ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode`.

**ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode**: A `ConfigurationLoadResult`
containing one `Warning`-severity `ConfigurationIssue` is created and `ReportTo` is called on a
silent `Context`. The context exit code must remain 0, confirming that warning-severity issues do
not escalate to a process failure. This scenario is tested by
`ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode`.

**ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber**: A `ConfigurationLoadResult`
containing an `Error`-severity issue with `FilePath` `"/repo/.buildmark.yaml"`, `Line` 7, and
description `"Unexpected value"` is created and `ReportTo` is called. The `HasErrors` flag must
be true, exit code must be 1, and the issue record must expose the exact `FilePath`, `Line`,
`Severity`, and `Description` values supplied at construction. This scenario is tested by
`ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber`.
