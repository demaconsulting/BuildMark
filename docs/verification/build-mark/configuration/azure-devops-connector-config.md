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

#### Test Environment

Standard dotnet test host; no external dependencies or environment setup required.

#### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

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

##### BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorEmptyAreaPath_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with an Azure DevOps connector block containing `area-path: ""`
is written; `BuildMarkConfigReader.ReadAsync` is called; `Config.Connector.AzureDevOps` is inspected.

**Expected**: `AreaPath` equals `string.Empty` (not `null`), confirming that an explicit empty
value is preserved and will disable area-path filtering at runtime.

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

##### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithAreaPath_ScopesWiqlQueryToAreaPath

**Scenario**: A connector is created with `AreaPath` set to `"MyProject\\MyRepo"`;
`GetBuildInformationAsync` is called; the captured WIQL request body is inspected.

**Expected**: The WIQL request body contains `System.AreaPath` and the configured area path value,
confirming that known-issues queries are scoped to the specified area.

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

##### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithoutAreaPath_DefaultsToProject

**Scenario**: A connector is created with no `AreaPath` configured; the git origin URL resolves to
project `project`; `GetBuildInformationAsync` is called; the captured WIQL request body is inspected.

**Expected**: The WIQL request body contains `System.AreaPath` scoped to `project`, confirming that
the default area path is the parsed project name (not the repository name).

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

##### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithEmptyAreaPath_DisablesAreaPathFilter

**Scenario**: A connector is created with `AreaPath` set to an empty string;
`GetBuildInformationAsync` is called; the captured WIQL request body is inspected.

**Expected**: The WIQL request body does not contain `System.AreaPath`, confirming that an empty
area path disables area-path filtering and produces a project-wide query.

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

##### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithInvalidAreaPath_ThrowsWithAdoErrorMessage

**Scenario**: A connector is created with `AreaPath` set to a path that does not exist in ADO;
the mock WIQL endpoint returns HTTP 400 with an ADO-style JSON error body; `GetBuildInformationAsync`
is called.

**Expected**: An `InvalidOperationException` is thrown and its message contains the ADO error
description from the 400 response body, so the user receives a meaningful diagnostic rather than
a generic HTTP failure.

**Requirement coverage**: `BuildMark-AzureDevOpsConnectorConfig-Properties`.

#### Requirements Coverage

- **`BuildMark-AzureDevOpsConnectorConfig-Properties`**:
  - BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAreaPath_ReturnsParsedConfiguration
  - BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorEmptyAreaPath_ReturnsParsedConfiguration
  - AzureDevOpsRepoConnector_GetBuildInformationAsync_WithAreaPath_ScopesWiqlQueryToAreaPath
  - AzureDevOpsRepoConnector_GetBuildInformationAsync_WithoutAreaPath_DefaultsToProject
  - AzureDevOpsRepoConnector_GetBuildInformationAsync_WithEmptyAreaPath_DisablesAreaPathFilter
  - AzureDevOpsRepoConnector_GetBuildInformationAsync_WithInvalidAreaPath_ThrowsWithAdoErrorMessage
