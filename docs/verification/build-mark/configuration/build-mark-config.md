# BuildMarkConfig

## Verification Approach

`BuildMarkConfig` is a data model class with no dedicated test class. It is verified
indirectly through program-level and connector factory tests that exercise the
configuration pipeline. When `BuildMarkConfigReader.ReadAsync` returns a
`ConfigurationLoadResult`, the embedded `BuildMarkConfig` instance drives connector
creation and report configuration.

## Dependencies

| Mock / Stub | Reason                                                     |
| ----------- | ---------------------------------------------------------- |
| File system | Integration tests may create temporary configuration files |

## Test Scenarios (Integration)

### Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero

**Scenario**: `Program.Run` processes a lint request; `BuildMarkConfig` is loaded
as an empty/default instance when no file is present.

**Expected**: Default `BuildMarkConfig` is handled gracefully; exit code is 0.

**Requirement coverage**: `BuildMark-Configuration-BuildMarkConfig`

### RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration

**Scenario**: `BuildMarkConfig.Connector` field is used by the factory to create
a connector with GitHub settings.

**Expected**: Connector reflects the `BuildMarkConfig` connector settings.

**Requirement coverage**: `BuildMark-Configuration-BuildMarkConfig`

## Requirements Coverage

- **BuildMark-Configuration-BuildMarkConfig**:
  Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero,
  RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration
