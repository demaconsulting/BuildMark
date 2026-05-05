### ConnectorConfig

#### Verification Approach

`ConnectorConfig` is verified through `ConfigurationTests.cs`. Tests write `.buildmark.yaml` files
with connector blocks and assert that `ConnectorConfig.Type`, `ConnectorConfig.GitHub`, and
`ConnectorConfig.AzureDevOps` are correctly populated by `BuildMarkConfigReader.ReadAsync`. No
mocking is required.

#### Dependencies

| Mock / Stub | Reason                                                               |
| ----------- | -------------------------------------------------------------------- |
| File system | Tests create temporary `.buildmark.yaml` files in `Path.GetTempPath` |

#### Test Scenarios

##### BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with a GitHub connector block is written;
`BuildMarkConfigReader.ReadAsync` is called; `Config.Connector` is inspected.

**Expected**: `Config.Connector.Type` equals `"github"`; `Config.Connector.GitHub` is non-null;
`Config.Connector.AzureDevOps` is null.

**Requirement coverage**: `BuildMark-ConnectorConfig-Properties`.

##### BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with an Azure DevOps connector block is written;
`BuildMarkConfigReader.ReadAsync` is called; `Config.Connector` is inspected.

**Expected**: `Config.Connector.Type` equals `"azure-devops"`;
`Config.Connector.AzureDevOps` is non-null with all fields set; `Config.Connector.GitHub` is null.

**Requirement coverage**: `BuildMark-ConnectorConfig-Properties`.

#### Requirements Coverage

- **`BuildMark-ConnectorConfig-Properties`**:
  - BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration
