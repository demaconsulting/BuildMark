### AzureDevOpsConnectorConfig

#### Verification Approach

`AzureDevOpsConnectorConfig` is verified through `ConfigurationTests.cs`. Tests write
`.buildmark.yaml` files with Azure DevOps connector blocks and assert that `OrganizationUrl`,
`Organization`, `Project`, `Repository`, and `AreaPath` properties are correctly parsed, including
alias key support. No mocking is required.

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

##### BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAreaPath_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with an Azure DevOps connector block containing `area-path` is
written; `BuildMarkConfigReader.ReadAsync` is called; `Config.Connector.AzureDevOps` is inspected.

**Expected**: `AreaPath` equals the configured area path string.

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

##### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithAreaPath_ScopesWiqlQueryToAreaPath

**Scenario**: A connector is created with `AreaPath` set to `"MyProject\\MyRepo"`;
`GetBuildInformationAsync` is called; the captured WIQL request body is inspected.

**Expected**: The WIQL request body contains `System.AreaPath` and the configured area path value,
confirming that known-issues queries are scoped to the specified area.

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

#### Requirements Coverage

- **`BuildMark-AzureDevOpsConnectorConfig-Properties`**:
  - BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAreaPath_ReturnsParsedConfiguration
  - AzureDevOpsRepoConnector_GetBuildInformationAsync_WithAreaPath_ScopesWiqlQueryToAreaPath
