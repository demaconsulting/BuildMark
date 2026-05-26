### ConfigurationIssue

#### Verification Approach

`ConfigurationIssue` is a record type with no logic. It is verified through `ConfigurationTests.cs`
via `ConfigurationLoadResult`, which constructs `ConfigurationIssue` instances directly with known
`FilePath`, `Line`, `Severity`, and `Description` values, then inspects those properties after
calling `ReportTo`. No mocking is required.

#### Test Environment

N/A - standard test environment.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` that construct `ConfigurationIssue` instances pass with
  zero failures.

#### Test Scenarios

**ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode**: A `ConfigurationIssue` with `Error`
severity is constructed and placed in a `ConfigurationLoadResult`; `ReportTo` is called on a
silent `Context`. The context exit code must equal 1, confirming that the severity value carried
by the record is read and evaluated correctly. This scenario is tested by
`ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode`.

**ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode**: A `ConfigurationIssue` with
`Warning` severity is constructed and placed in a `ConfigurationLoadResult`; `ReportTo` is called
on a silent `Context`. The context exit code must remain 0, confirming the severity is preserved
and evaluated correctly. This scenario is tested by
`ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode`.

**ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber**: A `ConfigurationIssue` is
constructed with `FilePath` `"/repo/.buildmark.yaml"`, `Line` 7, `Error` severity, and description
`"Unexpected value"`; all properties are inspected. The record must expose exactly the values
supplied at construction, confirming that all four fields are stored and accessible. This scenario
is tested by `ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber`.
