# GitHub

## Verification Approach

The GitHub sub-subsystem is verified through `GitHubTests.cs` (6 subsystem-level
tests), `GitHubRepoConnectorTests.cs` (22 unit tests), and 5 `GitHubGraphQLClient*Tests.cs`
files (41 tests). The subsystem tests exercise the full GitHub data pipeline through
mock HTTP responses. The unit tests are described in the individual unit chapters.

## Dependencies

| Mock / Stub              | Reason                                          |
| ------------------------ | ----------------------------------------------- |
| `MockHttpMessageHandler` | Intercepts HTTP calls to the GitHub GraphQL API |

## Test Scenarios (Subsystem-Level, GitHubTests.cs)

### GitHub_ImplementsInterface_ReturnsTrue

**Scenario**: `GitHubRepoConnector` is checked against `IRepoConnector`.

**Expected**: Implements the interface.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

### GitHub_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: GitHub connector receives mocked GraphQL data.

**Expected**: Returns valid `BuildInformation` with correct fields.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### GitHub_GetBuildInformation_WithMultipleVersions_SelectsCorrectBaseline

**Scenario**: Multiple tags exist; connector selects the correct prior release.

**Expected**: Baseline version is the most recent release before the build version.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### GitHub_GetBuildInformation_WithPullRequests_GathersChanges

**Scenario**: Mock data contains pull requests merged since the baseline.

**Expected**: `BuildInformation.Changes` contains entries for each pull request.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### GitHub_GetBuildInformation_WithOpenIssues_IdentifiesKnownIssues

**Scenario**: Mock data contains open issues.

**Expected**: `BuildInformation.KnownIssues` contains entries for each open issue.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### GitHub_GetBuildInformation_ReleaseVersion_SkipsPreReleases

**Scenario**: Build version is a release; pre-release tags in the history are skipped.

**Expected**: Baseline is the previous release tag.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

## Requirements Coverage

- **BuildMark-RepoConnectors-IRepoConnector**: GitHub_ImplementsInterface_ReturnsTrue
- **BuildMark-RepoConnectors-GitHub**: GitHub_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation,
  GitHub_GetBuildInformation_WithMultipleVersions_SelectsCorrectBaseline,
  GitHub_GetBuildInformation_WithPullRequests_GathersChanges,
  GitHub_GetBuildInformation_WithOpenIssues_IdentifiesKnownIssues,
  GitHub_GetBuildInformation_ReleaseVersion_SkipsPreReleases
