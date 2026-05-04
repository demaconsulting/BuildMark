# BuildMarkConfigReader

## Verification Approach

`BuildMarkConfigReader` is verified with dedicated unit tests in `ConfigurationTests.cs`. Tests
write `.buildmark.yaml` files to temporary directories and call `BuildMarkConfigReader.ReadAsync`,
asserting on the returned `ConfigurationLoadResult`. No mocking is required; the real file system
is used.

## Dependencies

| Mock / Stub | Reason                                                               |
| ----------- | -------------------------------------------------------------------- |
| File system | Tests create temporary `.buildmark.yaml` files in `Path.GetTempPath` |

## Test Scenarios

### BuildMarkConfigReader_ReadAsync_MissingFile_ReturnsEmptyResult

**Scenario**: `BuildMarkConfigReader.ReadAsync` is called on a temp directory containing no
`.buildmark.yaml`.

**Expected**: `Config` is null; `HasErrors` is false; `Issues` is empty.

**Requirement coverage**: `BuildMark-ConfigReader-MissingFile`.

### BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with a GitHub connector, sections, and rules is written;
`BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config` is non-null; `HasErrors` is false; connector type, owner, repo, base-url,
sections, and rules are parsed correctly.

**Requirement coverage**: `BuildMark-ConfigReader-ReadAsync`.

### BuildMarkConfigReader_ReadAsync_InvalidRepositoryValue_ReturnsErrorIssue

**Scenario**: A `.buildmark.yaml` with `repository: invalid` (not in `owner/repo` format) is
written; `BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config` is null; `HasErrors` is true; issue description contains `"owner/repo"`.

**Requirement coverage**: `BuildMark-ConfigReader-MalformedFile`.

### BuildMarkConfigReader_ReadAsync_MalformedFile_ReturnsErrorIssue

**Scenario**: A `.buildmark.yaml` containing a tab character (invalid YAML) is written;
`BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config` is null; `HasErrors` is true; issue description contains `"tab"`
(case-insensitive).

**Requirement coverage**: `BuildMark-ConfigReader-MalformedFile`.

### BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with an Azure DevOps connector block (url, organization,
project, repository) is written; `BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config.Connector.Type` is `"azure-devops"`; all Azure DevOps fields are parsed
correctly.

**Requirement coverage**: `BuildMark-ConfigReader-ReadAsync`.

### BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with Azure DevOps connector using alias keys (`org`, `repo`) is
written; `BuildMarkConfigReader.ReadAsync` is called.

**Expected**: All Azure DevOps fields are parsed correctly using the alias keys.

**Requirement coverage**: `BuildMark-ConfigReader-ReadAsync`.

### BuildMarkConfigReader_ReadAsync_AzureDevOpsUnsupportedKey_ReturnsErrorIssue

**Scenario**: A `.buildmark.yaml` with an unknown key inside the Azure DevOps connector block is
written; `BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config` is null; `HasErrors` is true; issue description contains
`"Unsupported Azure DevOps connector key"`.

**Requirement coverage**: `BuildMark-ConfigReader-MalformedFile`.

### BuildMarkConfigReader_ReadAsync_AzureDevOpsNonMapping_ReturnsErrorIssue

**Scenario**: A `.buildmark.yaml` where `azure-devops:` is a scalar value instead of a mapping is
written; `BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config` is null; `HasErrors` is true; issue description contains `"YAML mapping"`.

**Requirement coverage**: `BuildMark-ConfigReader-MalformedFile`.

## Requirements Coverage

- **`BuildMark-ConfigReader-ReadAsync`**:
  - BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration
- **`BuildMark-ConfigReader-MissingFile`**:
  - BuildMarkConfigReader_ReadAsync_MissingFile_ReturnsEmptyResult
- **`BuildMark-ConfigReader-MalformedFile`**:
  - BuildMarkConfigReader_ReadAsync_InvalidRepositoryValue_ReturnsErrorIssue
  - BuildMarkConfigReader_ReadAsync_MalformedFile_ReturnsErrorIssue
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsUnsupportedKey_ReturnsErrorIssue
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsNonMapping_ReturnsErrorIssue
