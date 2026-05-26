### ReportConfig

#### Verification Approach

`ReportConfig` is verified through `ConfigurationTests.cs`. Tests write `.buildmark.yaml` files
with a `report:` block and call `BuildMarkConfigReader.ReadAsync`, asserting that `File`, `Depth`,
and `IncludeKnownIssues` are parsed correctly or that invalid values produce error issues. The real
file system is used via `TemporaryDirectory`; no mocking is required.

#### Test Environment

Tests write temporary `.buildmark.yaml` files to directories created by `TemporaryDirectory`.
Write access to the current working directory is required. No network access or external services
are needed.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` targeting `ReportConfig` pass with zero failures.

#### Test Scenarios

**BuildMarkConfigReader_ReadAsync_ValidReportSection_ReturnsParsedReportConfig**: A `.buildmark.yaml`
with `report.file`, `report.depth: 2`, and `report.include-known-issues: true` is written and read
via `BuildMarkConfigReader.ReadAsync`; `Config.Report` is inspected. The `File` property must equal
`"build-notes.md"`, `Depth` must equal 2, and `IncludeKnownIssues` must be true. This scenario is
tested by `BuildMarkConfigReader_ReadAsync_ValidReportSection_ReturnsParsedReportConfig`.

**BuildMarkConfigReader_ReadAsync_InvalidReportDepth_ReturnsErrorIssue**: A `.buildmark.yaml` with
`report.depth: -1` is written and read via `BuildMarkConfigReader.ReadAsync`. The result must have
a null `Config`, `HasErrors` true, and an issue description containing `"positive integer"`,
confirming that a negative depth value is rejected. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_InvalidReportDepth_ReturnsErrorIssue`.
