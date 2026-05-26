### AzureDevOpsConnectorConfig

#### Verification Approach

`AzureDevOpsConnectorConfig` is verified through `ConfigurationTests.cs` and the
`AzureDevOpsRepoConnector` integration tests. The `ConfigurationTests.cs` tests write
`.buildmark.yaml` files with Azure DevOps connector blocks and call `BuildMarkConfigReader.ReadAsync`,
asserting that `OrganizationUrl`, `Organization`, `Project`, `Repository`, `AreaPath`, and
`TokenVariable` are correctly parsed, including alias key support and empty/non-scalar area-path
edge cases. Integration tests in the RepoConnectors test suite verify that the `AreaPath` property
is applied correctly to WIQL queries at runtime. The real file system is used via
`TemporaryDirectory`; no mocking is required for the configuration parsing tests.

#### Test Environment

Tests write temporary `.buildmark.yaml` files to directories created by `TemporaryDirectory`.
Write access to the current working directory is required. Integration tests that verify `AreaPath`
runtime behavior use a mock HTTP client to capture WIQL requests; no real Azure DevOps endpoint is
needed.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` targeting `AzureDevOpsConnectorConfig` pass with zero
  failures.
- All `AzureDevOpsRepoConnector` area-path integration tests pass with zero failures.

#### Test Scenarios

**BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with canonical Azure DevOps connector keys (organization, repository) is written
and read via `BuildMarkConfigReader.ReadAsync`; `Config.Connector.AzureDevOps` is inspected. All
fields (`OrganizationUrl`, `Organization`, `Project`, `Repository`) must match the values written
to the file. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with Azure DevOps connector alias keys (`org`, `repo`) is written and read via
`BuildMarkConfigReader.ReadAsync`; all fields must be populated correctly from the alias keys,
confirming alias support is equivalent to canonical keys. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAreaPath_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with an Azure DevOps connector block containing an `area-path` scalar value is
written and read via `BuildMarkConfigReader.ReadAsync`. The `AreaPath` property must equal the
configured path string. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAreaPath_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorEmptyAreaPath_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with `azure-devops.area-path: ""` is written and read via
`BuildMarkConfigReader.ReadAsync`. The `AreaPath` property must equal `string.Empty` (not `null`),
confirming that an explicit empty value is preserved to allow area-path filtering to be disabled
at runtime. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorEmptyAreaPath_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsNonScalarAreaPath_ReturnsErrorIssue**: A
`.buildmark.yaml` with `azure-devops.area-path` set to a YAML sequence instead of a scalar string
is written and read via `BuildMarkConfigReader.ReadAsync`. The result must have a null `Config`,
`HasErrors` true, and an issue description containing
`"Azure DevOps area-path must be a scalar string value"`. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsNonScalarAreaPath_ReturnsErrorIssue`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsEmptyTokenVariable_ReturnsErrorIssue**: A
`.buildmark.yaml` with `azure-devops.token-variable: ""` is written and read via
`BuildMarkConfigReader.ReadAsync`. The result must have a null `Config`, `HasErrors` true, and an
issue description containing `"token-variable"`, confirming that an empty token-variable is
rejected. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsEmptyTokenVariable_ReturnsErrorIssue`.

**AzureDevOpsRepoConnector_GetBuildInformationAsync_WithAreaPath_ScopesWiqlQueryToAreaPath**: A
connector is created with `AreaPath` set to `"MyProject\\MyRepo"` and
`GetBuildInformationAsync` is called; the captured WIQL request body is inspected. The request
body must contain `System.AreaPath` and the configured area path value, confirming that known-issue
queries are scoped to the specified area. This scenario is tested by
`AzureDevOpsRepoConnector_GetBuildInformationAsync_WithAreaPath_ScopesWiqlQueryToAreaPath`.

**AzureDevOpsRepoConnector_GetBuildInformationAsync_WithoutAreaPath_DefaultsToProject**: A
connector is created with no `AreaPath` configured and `GetBuildInformationAsync` is called; the
captured WIQL request body is inspected. The request body must contain `System.AreaPath` scoped to
the parsed project name, confirming that the default area path is the project name rather than the
repository name. This scenario is tested by
`AzureDevOpsRepoConnector_GetBuildInformationAsync_WithoutAreaPath_DefaultsToProject`.

**AzureDevOpsRepoConnector_GetBuildInformationAsync_WithEmptyAreaPath_DisablesAreaPathFilter**: A
connector is created with `AreaPath` set to an empty string and `GetBuildInformationAsync` is
called; the captured WIQL request body is inspected. The request body must not contain
`System.AreaPath`, confirming that an empty area path disables area-path filtering and produces a
project-wide query. This scenario is tested by
`AzureDevOpsRepoConnector_GetBuildInformationAsync_WithEmptyAreaPath_DisablesAreaPathFilter`.

**AzureDevOpsRepoConnector_GetBuildInformationAsync_WithInvalidAreaPath_ThrowsWithAdoErrorMessage**:
A connector is created with `AreaPath` set to a non-existent path; the mock WIQL endpoint returns
HTTP 400 with an ADO-style JSON error body; `GetBuildInformationAsync` is called. An
`InvalidOperationException` must be thrown and its message must contain the ADO error description
from the 400 response body, so the user receives a meaningful diagnostic. This scenario is tested
by
`AzureDevOpsRepoConnector_GetBuildInformationAsync_WithInvalidAreaPath_ThrowsWithAdoErrorMessage`.
