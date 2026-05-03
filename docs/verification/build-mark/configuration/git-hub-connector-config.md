# GitHubConnectorConfig

## Verification Approach

`GitHubConnectorConfig` is a data model class verified indirectly through
`RepoConnectorFactoryTests.cs`. Tests that supply GitHub-specific configuration
within a `ConnectorConfig` confirm that the settings are forwarded to the created
`GitHubRepoConnector`.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios (Integration)

### RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration

**Scenario**: A `ConnectorConfig` with a `GitHubConnectorConfig` is passed to the factory.

**Expected**: The resulting connector incorporates the GitHub-specific configuration.

**Requirement coverage**: `BuildMark-Configuration-GitHubConnectorConfig`

## Requirements Coverage

- **BuildMark-Configuration-GitHubConnectorConfig**:
  RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration
