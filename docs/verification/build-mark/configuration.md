## Configuration

### Verification Approach

The Configuration subsystem is verified with dedicated subsystem-level integration tests in
`ConfigurationSubsystemTests.cs`. Tests exercise the full subsystem boundary by creating temporary
`.buildmark.yaml` files on the real file system, invoking `BuildMarkConfigReader.ReadAsync`, and
asserting on the returned `ConfigurationLoadResult`. No mocking is required; the real file system
is used with temporary directories managed by `TemporaryDirectory`. Error-reporting behavior is
verified by constructing `ConfigurationLoadResult` instances directly with controlled
`ConfigurationIssue` entries and calling `ReportTo` on a silent `Context`.

### Test Environment

Tests create temporary directories via `TemporaryDirectory`, which produces unique `tmp-*`
subdirectories under the current working directory. Write access to the current working directory
is required. No network access or external services are needed.

### Acceptance Criteria

- All tests in `ConfigurationSubsystemTests.cs` pass with zero failures.
- All `BuildMark-Configuration-*` requirements have at least one test mapped in the requirements
  traceability matrix.

### Test Scenarios

**Configuration_ReadAsync_ValidFile_ReturnsConfiguration**: A valid `.buildmark.yaml` containing a
GitHub connector, one section, and one routing rule is written to a temporary directory and read
via `BuildMarkConfigReader.ReadAsync`. The result must be non-null, free of errors, and correctly
reflect the connector type, owner, repo, section id, and rule route. This scenario is tested by
`Configuration_ReadAsync_ValidFile_ReturnsConfiguration`.

**Configuration_ReadAsync_MissingFile_ReturnsEmptyResult**: `BuildMarkConfigReader.ReadAsync` is
called on a temporary directory containing no `.buildmark.yaml`. The result must have a null
`Config`, `HasErrors` false, and an empty `Issues` collection. This scenario is tested by
`Configuration_ReadAsync_MissingFile_ReturnsEmptyResult`.

**Configuration_ReadAsync_MalformedFile_ReportsError**: A `.buildmark.yaml` containing a tab
character (invalid YAML) is written to a temporary directory and read via
`BuildMarkConfigReader.ReadAsync`. The result must have a null `Config`, `HasErrors` true, and at
least one `Error`-severity issue. This scenario is tested by
`Configuration_ReadAsync_MalformedFile_ReportsError`.

**Configuration_Issues_ErrorIssue_SetsExitCode**: A `ConfigurationLoadResult` carrying one
`Error`-severity `ConfigurationIssue` is created and `ReportTo` is called on a silent `Context`.
The context exit code must be set to 1, confirming that error issues are escalated correctly. This
scenario is tested by `Configuration_Issues_ErrorIssue_SetsExitCode`.

**Configuration_Issues_WarningIssue_DoesNotSetExitCode**: A `ConfigurationLoadResult` carrying one
`Warning`-severity `ConfigurationIssue` is created and `ReportTo` is called on a silent `Context`.
The context exit code must remain 0, confirming that warning issues do not fail the process. This
scenario is tested by `Configuration_Issues_WarningIssue_DoesNotSetExitCode`.

**Configuration_Issues_ValidationError_ReportsAccurateLine**: A `.buildmark.yaml` with an
unsupported key placed on line 3 is written and read via `BuildMarkConfigReader.ReadAsync`. The
resulting issue must report `Line` equal to 3, confirming that 1-based line numbers are preserved
through the YAML parser. This scenario is tested by
`Configuration_Issues_ValidationError_ReportsAccurateLine`.

**Configuration_ConnectorConfig_ValidFile_ParsesConnectorSettings**: A `.buildmark.yaml` with a
GitHub connector block containing owner, repo, and base-url fields is written and read via
`BuildMarkConfigReader.ReadAsync`. The connector type, owner, repo, and base URL must all be parsed
correctly into the `ConnectorConfig` model. This scenario is tested by
`Configuration_ConnectorConfig_ValidFile_ParsesConnectorSettings`.

**Configuration_ConnectorConfig_ValidFile_ParsesAzureDevOpsSettings**: A `.buildmark.yaml` with an
Azure DevOps connector block containing url, organization, project, and repository fields is written
and read via `BuildMarkConfigReader.ReadAsync`. All Azure DevOps connector fields must be parsed
correctly into the `ConnectorConfig` model. This scenario is tested by
`Configuration_ConnectorConfig_ValidFile_ParsesAzureDevOpsSettings`.
