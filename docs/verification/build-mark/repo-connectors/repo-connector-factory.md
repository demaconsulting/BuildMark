# RepoConnectorFactory

## Verification Approach

`RepoConnectorFactory` is tested through `RepoConnectorFactoryTests.cs`, which contains
11 unit tests. The tests cover connector creation from default settings, from explicit
connector type configuration, from environment variables (GitHub Actions, Azure DevOps),
and from remote URL detection.

## Dependencies

| Mock / Stub              | Reason                                                   |
| ------------------------ | -------------------------------------------------------- |
| Environment variables    | Tests set/clear `GITHUB_ACTIONS` and `TF_BUILD` env vars |
| Git remote URL (process) | Factory may invoke Git to detect the remote URL          |

## Test Scenarios

### RepoConnectorFactory_Create_ReturnsConnector

**Scenario**: `RepoConnectorFactory.Create` is called with `null` configuration.

**Expected**: Returns a non-null `IRepoConnector` instance.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_ReturnsGitHubConnectorForThisRepo

**Scenario**: Factory is invoked in the GitHub Actions CI environment.

**Expected**: Returns a `GitHubRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration

**Scenario**: Factory is called with a `ConnectorConfig` specifying GitHub settings.

**Expected**: Returns a connector with GitHub settings applied.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithAzureDevOpsType_CreatesAzureDevOpsConnector

**Scenario**: Factory is called with `ConnectorConfig.Type = "azure-devops"`.

**Expected**: Returns an `AzureDevOpsRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration

**Scenario**: Factory is called with Azure DevOps connector configuration.

**Expected**: Returns an `AzureDevOpsRepoConnector` with the supplied settings.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithTfBuildEnv_ReturnsAzureDevOpsConnector

**Scenario**: `TF_BUILD` environment variable is set to `"True"`.

**Expected**: Factory returns an `AzureDevOpsRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithGitHubActionsEnv_ReturnsGitHubConnector

**Scenario**: `GITHUB_ACTIONS` environment variable is set to `"true"`.

**Expected**: Factory returns a `GitHubRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithAzureDevOpsRemoteUrl_ReturnsAzureDevOpsConnector

**Scenario**: Git remote URL matches an Azure DevOps `dev.azure.com` pattern.

**Expected**: Factory returns an `AzureDevOpsRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithVisualStudioRemoteUrl_ReturnsAzureDevOpsConnector

**Scenario**: Git remote URL matches a `visualstudio.com` pattern.

**Expected**: Factory returns an `AzureDevOpsRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithGitHubRemoteUrl_ReturnsGitHubConnector

**Scenario**: Git remote URL matches a `github.com` pattern.

**Expected**: Factory returns a `GitHubRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectorFactory_Create_WithNullRemoteUrl_DefaultsToGitHubConnector

**Scenario**: Git remote URL cannot be determined (null/empty).

**Expected**: Factory defaults to returning a `GitHubRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

## Requirements Coverage

- **BuildMark-RepoConnectors-RepoConnectorFactory**: All 11 tests in
  `RepoConnectorFactoryTests.cs`
