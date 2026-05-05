### AzureDevOpsConnectorConfig

#### Verification Approach

`AzureDevOpsConnectorConfig` is verified through `ConfigurationTests.cs`. Tests write
`.buildmark.yaml` files with Azure DevOps connector blocks and assert that `OrganizationUrl`,
`Organization`, `Project`, and `Repository` properties are correctly parsed, including alias key
support. No mocking is required.

#### Dependencies

| Mock / Stub | Reason                                                               |
| ----------- | -------------------------------------------------------------------- |
| File system | Tests create temporary `.buildmark.yaml` files in `Path.GetTempPath` |

#### Test Scenarios

##### BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with an Azure DevOps connector block using canonical keys
(`organization`, `repository`) is written; `BuildMarkConfigReader.ReadAsync` is called;
`Config.Connector.AzureDevOps` is inspected.

**Expected**: `OrganizationUrl` equals `"https://dev.azure.com/myorg"`; `Organization` equals
`"myorg"`; `Project` equals `"myproject"`; `Repository` equals `"myrepo"`.

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

##### BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with Azure DevOps connector using alias keys (`org`, `repo`) is
written; `BuildMarkConfigReader.ReadAsync` is called; `Config.Connector.AzureDevOps` is inspected.

**Expected**: `OrganizationUrl`, `Organization`, `Project`, and `Repository` are all populated
correctly from the alias keys.

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

#### Requirements Coverage

- **`BuildMark-AzureDevOpsConnectorConfig-Properties`**:
  - BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration
