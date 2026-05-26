### ConnectorConfig

#### Verification Approach

`ConnectorConfig` is verified through `ConfigurationTests.cs`. Tests write `.buildmark.yaml` files
with GitHub and Azure DevOps connector blocks and call `BuildMarkConfigReader.ReadAsync`, asserting
that `ConnectorConfig.Type`, `ConnectorConfig.GitHub`, and `ConnectorConfig.AzureDevOps` are
populated correctly. The real file system is used via `TemporaryDirectory`; no mocking is required.

#### Test Environment

Tests write temporary `.buildmark.yaml` files to directories created by `TemporaryDirectory`.
Write access to the current working directory is required. No network access or external services
are needed.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` targeting `ConnectorConfig` pass with zero failures.

#### Test Scenarios

**BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration**: A `.buildmark.yaml` with
a GitHub connector block is written and read via `BuildMarkConfigReader.ReadAsync`; the
`Config.Connector` is inspected. `ConnectorConfig.Type` must equal `"github"`, `GitHub` must be
non-null, and `AzureDevOps` must be null, confirming the connector discriminator is set correctly.
This scenario is tested by `BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration`.

**BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration**: A
`.buildmark.yaml` with an Azure DevOps connector block is written and read via
`BuildMarkConfigReader.ReadAsync`; the `Config.Connector` is inspected. `ConnectorConfig.Type`
must equal `"azure-devops"`, `AzureDevOps` must be non-null with all fields set, and `GitHub` must
be null. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration`.
