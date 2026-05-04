# Configuration

## Verification Approach

The Configuration subsystem is verified with dedicated subsystem tests in
`ConfigurationSubsystemTests.cs`. Tests create temporary `.buildmark.yaml` files, call
`BuildMarkConfigReader.ReadAsync`, and assert on the returned `ConfigurationLoadResult`. No mocking
is required; the real file system is used with temporary directories.

## Dependencies

| Mock / Stub | Reason                                                               |
| ----------- | -------------------------------------------------------------------- |
| File system | Tests create temporary `.buildmark.yaml` files in `Path.GetTempPath` |

## Test Scenarios

### Configuration_ReadAsync_ValidFile_ReturnsConfiguration

**Scenario**: A valid `.buildmark.yaml` containing a GitHub connector, one section, and one rule
is written to a temp directory; `BuildMarkConfigReader.ReadAsync` is called.

**Expected**: Result has no errors; `Config.Connector.Type` is `"github"`;
`Config.Connector.GitHub.Owner` is `"test-owner"`; one section and one rule are parsed.

**Requirement coverage**: `BuildMark-Configuration-Read`.

### Configuration_ReadAsync_MissingFile_ReturnsEmptyResult

**Scenario**: `BuildMarkConfigReader.ReadAsync` is called on a temp directory containing no
`.buildmark.yaml`.

**Expected**: `Config` is null; `HasErrors` is false; `Issues` is empty.

**Requirement coverage**: `BuildMark-Configuration-Read`.

### Configuration_ReadAsync_MalformedFile_ReportsError

**Scenario**: A `.buildmark.yaml` with a tab character (malformed YAML) is written;
`BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config` is null; `HasErrors` is true; `Issues` contains at least one entry with
`Error` severity.

**Requirement coverage**: `BuildMark-Configuration-Read`.

### Configuration_Issues_ErrorIssue_SetsExitCode

**Scenario**: A `ConfigurationLoadResult` containing one `Error`-severity `ConfigurationIssue` is
created; `ReportTo(context)` is called on a silent context.

**Expected**: `context.ExitCode` equals 1.

**Requirement coverage**: `BuildMark-Configuration-Issues`.

### Configuration_Issues_WarningIssue_DoesNotSetExitCode

**Scenario**: A `ConfigurationLoadResult` containing one `Warning`-severity `ConfigurationIssue`
is created; `ReportTo(context)` is called on a silent context.

**Expected**: `context.ExitCode` remains 0.

**Requirement coverage**: `BuildMark-Configuration-Issues`.

### Configuration_Issues_ValidationError_ReportsAccurateLine

**Scenario**: A `.buildmark.yaml` with an unsupported key on line 3 is written;
`BuildMarkConfigReader.ReadAsync` is called.

**Expected**: The resulting issue reports `Line` equal to 3.

**Requirement coverage**: `BuildMark-Configuration-Issues`.

### Configuration_ConnectorConfig_ValidFile_ParsesConnectorSettings

**Scenario**: A `.buildmark.yaml` with a GitHub connector block (owner, repo, base-url) is
written; `BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config.Connector.Type` is `"github"`; `GitHub.Owner`, `GitHub.Repo`, and
`GitHub.BaseUrl` match the file values.

**Requirement coverage**: `BuildMark-Configuration-ConnectorConfig`.

### Configuration_ConnectorConfig_ValidFile_ParsesAzureDevOpsSettings

**Scenario**: A `.buildmark.yaml` with an Azure DevOps connector block (url, organization,
project, repository) is written; `BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config.Connector.Type` is `"azure-devops"`; `AzureDevOps.OrganizationUrl`,
`AzureDevOps.Organization`, `AzureDevOps.Project`, and `AzureDevOps.Repository` match the file
values.

**Requirement coverage**: `BuildMark-Configuration-ConnectorConfig`.

## Requirements Coverage

- **`BuildMark-Configuration-Read`**:
  - Configuration_ReadAsync_ValidFile_ReturnsConfiguration
  - Configuration_ReadAsync_MissingFile_ReturnsEmptyResult
  - Configuration_ReadAsync_MalformedFile_ReportsError
- **`BuildMark-Configuration-Issues`**:
  - Configuration_Issues_ErrorIssue_SetsExitCode
  - Configuration_Issues_WarningIssue_DoesNotSetExitCode
  - Configuration_Issues_ValidationError_ReportsAccurateLine
- **`BuildMark-Configuration-ConnectorConfig`**:
  - Configuration_ConnectorConfig_ValidFile_ParsesConnectorSettings
  - Configuration_ConnectorConfig_ValidFile_ParsesAzureDevOpsSettings
