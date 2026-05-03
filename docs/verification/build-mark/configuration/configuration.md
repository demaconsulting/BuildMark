# Configuration

## Verification Approach

The Configuration subsystem is verified at the integration level through
`ProgramTests.cs` and `RepoConnectorFactoryTests.cs`. There is no dedicated
`ConfigurationTests.cs` file. The lint-related tests in `ProgramTests.cs` exercise
`BuildMarkConfigReader` indirectly when `Program.Run` with `Lint = true` calls
`BuildMarkConfigReader.ReadAsync`. The factory tests in `RepoConnectorFactoryTests.cs`
exercise connector configuration forwarding.

## Dependencies

| Mock / Stub | Reason                                                    |
| ----------- | --------------------------------------------------------- |
| File system | Some tests create temporary `.buildmark.yaml` files       |
| `Context`   | Provides output capture for configuration issue reporting |

## Test Scenarios (Integration)

### Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero

**Scenario**: `Program.Run` is called with `Lint = true` and no `.buildmark.yaml` present.

**Expected**: `BuildMarkConfigReader` returns a default/empty result; exit code is 0.

**Requirement coverage**: `BuildMark-Configuration-BuildMarkConfigReader`

### RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration

**Scenario**: `RepoConnectorFactory.Create` is called with a `ConnectorConfig` that
specifies GitHub settings.

**Expected**: The returned connector has the GitHub configuration applied.

**Requirement coverage**: `BuildMark-Configuration-ConnectorConfig`,
`BuildMark-Configuration-GitHubConnectorConfig`

### RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration

**Scenario**: `RepoConnectorFactory.Create` is called with Azure DevOps connector config.

**Expected**: The returned connector has the Azure DevOps configuration applied.

**Requirement coverage**: `BuildMark-Configuration-ConnectorConfig`,
`BuildMark-Configuration-AzureDevOpsConnectorConfig`

## Requirements Coverage

- **BuildMark-Configuration-BuildMarkConfigReader**:
  Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero
- **BuildMark-Configuration-ConnectorConfig**:
  RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration,
  RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration
- **BuildMark-Configuration-GitHubConnectorConfig**:
  RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration
- **BuildMark-Configuration-AzureDevOpsConnectorConfig**:
  RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration
