### BuildMarkConfigReader

#### Verification Approach

`BuildMarkConfigReader` is verified with unit tests in `ConfigurationTests.cs`. Tests write
`.buildmark.yaml` files to temporary directories using `TemporaryDirectory` and call
`BuildMarkConfigReader.ReadAsync`, asserting on the returned `ConfigurationLoadResult`. The real
file system is used; no mocking is required. Tests cover valid GitHub and Azure DevOps
configurations, alias key support, token-variable validation, area-path parsing, and all malformed
input error paths.

#### Test Environment

Tests write temporary `.buildmark.yaml` files to directories created by `TemporaryDirectory`.
Write access to the current working directory is required. No network access or external services
are needed.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` targeting `BuildMarkConfigReader` pass with zero
  failures.

#### Test Scenarios

**BuildMarkConfigReader_ReadAsync_MissingFile_ReturnsEmptyResult**: `BuildMarkConfigReader.ReadAsync`
is called on a temporary directory containing no `.buildmark.yaml`. The result must have a null
`Config`, `HasErrors` false, and an empty `Issues` collection, confirming graceful handling of an
absent configuration file. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_MissingFile_ReturnsEmptyResult`.

**BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration**: A `.buildmark.yaml` with
a GitHub connector block, one section, and one rule is written and read via
`BuildMarkConfigReader.ReadAsync`. The result must be non-null, free of errors, and correctly
reflect all connector fields, section id, and rule route. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_InvalidRepositoryValue_ReturnsErrorIssue**: A `.buildmark.yaml`
with `github.repository: invalid` (not in `owner/repo` format) is written and read via
`BuildMarkConfigReader.ReadAsync`. The result must have a null `Config`, `HasErrors` true, and an
issue description containing `"owner/repo"`. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_InvalidRepositoryValue_ReturnsErrorIssue`.

**BuildMarkConfigReader_ReadAsync_MalformedFile_ReturnsErrorIssue**: A `.buildmark.yaml`
containing a tab character (invalid YAML) is written and read via `BuildMarkConfigReader.ReadAsync`.
The result must have a null `Config`, `HasErrors` true, and an issue description containing
`"tab"` (case-insensitive). This scenario is tested by
`BuildMarkConfigReader_ReadAsync_MalformedFile_ReturnsErrorIssue`.

**BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with an Azure DevOps connector block using canonical keys (organization,
repository) is written and read via `BuildMarkConfigReader.ReadAsync`. All Azure DevOps fields
including `OrganizationUrl`, `Organization`, `Project`, and `Repository` must be parsed correctly.
This scenario is tested by
`BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with Azure DevOps connector using alias keys (`org`, `repo`) is written and read
via `BuildMarkConfigReader.ReadAsync`. All Azure DevOps fields must be populated correctly from the
alias keys, confirming alias support is equivalent to the canonical keys. This scenario is tested
by `BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAreaPath_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with an Azure DevOps connector block containing an `area-path` value is written
and read via `BuildMarkConfigReader.ReadAsync`. The `AreaPath` property must equal the configured
path string. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAreaPath_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorEmptyAreaPath_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with `azure-devops.area-path: ""` is written and read via
`BuildMarkConfigReader.ReadAsync`. The `AreaPath` property must equal `string.Empty` (not `null`),
confirming that an explicit empty value is preserved to disable area-path filtering. This scenario
is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorEmptyAreaPath_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsUnsupportedKey_ReturnsErrorIssue**: A `.buildmark.yaml`
with an unknown key inside the Azure DevOps connector block is written and read via
`BuildMarkConfigReader.ReadAsync`. The result must have a null `Config`, `HasErrors` true, and an
issue description containing `"Unsupported Azure DevOps connector key"`. This scenario is tested
by `BuildMarkConfigReader_ReadAsync_AzureDevOpsUnsupportedKey_ReturnsErrorIssue`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsNonMapping_ReturnsErrorIssue**: A `.buildmark.yaml`
where `azure-devops:` is a scalar value instead of a mapping is written and read via
`BuildMarkConfigReader.ReadAsync`. The result must have a null `Config`, `HasErrors` true, and an
issue description containing `"YAML mapping"`. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsNonMapping_ReturnsErrorIssue`.

**BuildMarkConfigReader_ReadAsync_GitHubEmptyTokenVariable_ReturnsErrorIssue**: A `.buildmark.yaml`
with `github.token-variable: ""` is written and read via `BuildMarkConfigReader.ReadAsync`. The
result must have a null `Config`, `HasErrors` true, and an issue description containing
`"token-variable"`. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_GitHubEmptyTokenVariable_ReturnsErrorIssue`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsEmptyTokenVariable_ReturnsErrorIssue**: A
`.buildmark.yaml` with `azure-devops.token-variable: ""` is written and read via
`BuildMarkConfigReader.ReadAsync`. The result must have a null `Config`, `HasErrors` true, and an
issue description containing `"token-variable"`. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsEmptyTokenVariable_ReturnsErrorIssue`.

**BuildMarkConfigReader_ReadAsync_AzureDevOpsNonScalarAreaPath_ReturnsErrorIssue**: A
`.buildmark.yaml` with `azure-devops.area-path` set to a YAML sequence instead of a scalar string
is written and read via `BuildMarkConfigReader.ReadAsync`. The result must have a null `Config`,
`HasErrors` true, and an issue description containing
`"Azure DevOps area-path must be a scalar string value"`. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_AzureDevOpsNonScalarAreaPath_ReturnsErrorIssue`.
