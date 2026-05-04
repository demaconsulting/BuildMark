# AzureDevOpsConnectorConfig

## Verification Approach

`AzureDevOpsConnectorConfig` is a data model class verified indirectly through
`RepoConnectorFactoryTests.cs`. Tests that supply Azure DevOps-specific configuration
within a `ConnectorConfig` confirm that the settings are forwarded to the created
`AzureDevOpsRepoConnector`.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios (Integration)

### RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration

**Scenario**: A `ConnectorConfig` with an `AzureDevOpsConnectorConfig` is passed to
the factory.

**Expected**: The resulting connector incorporates the Azure DevOps-specific
configuration.

**Requirement coverage**: `BuildMark-Configuration-AzureDevOpsConnectorConfig`

## Requirements Coverage

- **BuildMark-Configuration-AzureDevOpsConnectorConfig**:
  RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration
