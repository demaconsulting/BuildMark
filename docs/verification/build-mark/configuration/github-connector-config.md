### GitHubConnectorConfig

#### Verification Approach

`GitHubConnectorConfig` is verified through `ConfigurationTests.cs`. Tests write `.buildmark.yaml`
files with GitHub connector blocks and call `BuildMarkConfigReader.ReadAsync`, asserting that
`Owner`, `Repo`, `BaseUrl`, and `TokenVariable` are correctly parsed or rejected. The real file
system is used via `TemporaryDirectory`; no mocking is required.

#### Test Environment

Tests write temporary `.buildmark.yaml` files to directories created by `TemporaryDirectory`.
Write access to the current working directory is required. No network access or external services
are needed.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` targeting `GitHubConnectorConfig` pass with zero
  failures.

#### Test Scenarios

**BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration**: A `.buildmark.yaml` with
`github.owner`, `github.repo`, and `github.base-url` fields is written and read via
`BuildMarkConfigReader.ReadAsync`; `Config.Connector.GitHub` is inspected. The `Owner`, `Repo`,
and `BaseUrl` properties must match the values written to the file, confirming all fields are
mapped correctly. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_GitHubEmptyTokenVariable_ReturnsErrorIssue**: A `.buildmark.yaml`
with `github.token-variable: ""` is written and read via `BuildMarkConfigReader.ReadAsync`. The
result must have a null `Config`, `HasErrors` true, and an issue description containing
`"token-variable"`, confirming that an empty token-variable value is rejected as invalid. This
scenario is tested by
`BuildMarkConfigReader_ReadAsync_GitHubEmptyTokenVariable_ReturnsErrorIssue`.
