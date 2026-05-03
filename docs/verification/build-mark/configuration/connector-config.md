# ConnectorConfig

## Verification Approach

`ConnectorConfig` is a data model class verified indirectly through
`RepoConnectorFactoryTests.cs`. Tests that pass a `ConnectorConfig` to
`RepoConnectorFactory.Create` exercise the configuration forwarding path and confirm
that the connector type and sub-configuration are interpreted correctly.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios (Integration)

### RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration

**Scenario**: `ConnectorConfig` with GitHub settings is passed to the factory.

**Expected**: Factory creates a GitHub connector with the supplied settings applied.

**Requirement coverage**: `BuildMark-Configuration-ConnectorConfig`

### RepoConnectorFactory_Create_WithAzureDevOpsType_CreatesAzureDevOpsConnector

**Scenario**: `ConnectorConfig` with Azure DevOps type is passed to the factory.

**Expected**: Factory creates an Azure DevOps connector.

**Requirement coverage**: `BuildMark-Configuration-ConnectorConfig`

### RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration

**Scenario**: `ConnectorConfig` with Azure DevOps settings is passed to the factory.

**Expected**: Factory creates Azure DevOps connector with the supplied settings applied.

**Requirement coverage**: `BuildMark-Configuration-ConnectorConfig`

## Requirements Coverage

- **BuildMark-Configuration-ConnectorConfig**:
  RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration,
  RepoConnectorFactory_Create_WithAzureDevOpsType_CreatesAzureDevOpsConnector,
  RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration
