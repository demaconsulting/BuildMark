# RepoConnectors

## Verification Approach

The RepoConnectors subsystem is verified through `RepoConnectorsTests.cs`, which
contains 33 subsystem-level integration tests. These tests exercise the connector
factory, the GitHub connector, the Azure DevOps connector, and the Mock connector
through the full `GetBuildInformationAsync` pipeline using mock HTTP data. Individual
unit tests for sub-components are described in the unit-level chapters.

## Dependencies

| Mock / Stub              | Reason                                                        |
| ------------------------ | ------------------------------------------------------------- |
| `MockHttpMessageHandler` | Intercepts HTTP calls to GitHub GraphQL and Azure DevOps REST |
| `MockRepoConnector`      | Used directly for factory and base class tests                |
| `ProcessRunner` (real)   | Used by ProcessRunner tests with actual OS commands           |

## Test Scenarios

### RepoConnectors_GitHubConnector_ImplementsInterface_ReturnsTrue

**Scenario**: The GitHub connector instance is checked against `IRepoConnector`.

**Expected**: Implements the interface.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

### RepoConnectors_GitHubConnector_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: GitHub connector receives mocked GraphQL responses.

**Expected**: Returns a `BuildInformation` with correct version, changes, and known issues.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### RepoConnectors_GitHubConnector_GetBuildInformation_WithMultipleVersions_SelectsCorrectBaseline

**Scenario**: Multiple version tags exist; connector selects the correct baseline.

**Expected**: Baseline version is the most recent release prior to the build version.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### RepoConnectors_GitHubConnector_GetBuildInformation_WithPullRequests_GathersChanges

**Scenario**: Mock data includes pull requests; connector gathers them as changes.

**Expected**: `BuildInformation.Changes` contains entries for each pull request.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### RepoConnectors_GitHubConnector_GetBuildInformation_WithOpenIssues_IdentifiesKnownIssues

**Scenario**: Mock data includes open issues; connector identifies them as known issues.

**Expected**: `BuildInformation.KnownIssues` contains entries for open issues.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### RepoConnectors_GitHubConnector_GetBuildInformation_ReleaseVersion_SkipsPreReleases

**Scenario**: Build version is a release; pre-release tags in history are skipped.

**Expected**: Baseline is a previous release tag, not a pre-release.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

### RepoConnectors_ConnectorBase_MockConnector_ImplementsInterface

**Scenario**: Mock connector is checked against `IRepoConnector`.

**Expected**: Implements the interface.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

### RepoConnectors_ConnectorBase_GitHubConnector_ImplementsInterface

**Scenario**: GitHub connector class is checked for `RepoConnectorBase` inheritance.

**Expected**: GitHub connector extends `RepoConnectorBase`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorBase`

### RepoConnectors_MockConnector_Constructor_CreatesInstance

**Scenario**: `MockRepoConnector` is constructed.

**Expected**: Instance is created without error.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

### RepoConnectors_MockConnector_ImplementsInterface_ReturnsTrue

**Scenario**: Mock connector is checked against `IRepoConnector`.

**Expected**: Implements the interface.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

### RepoConnectors_MockConnector_GetBuildInformation_ReturnsExpectedVersion

**Scenario**: Mock connector's `GetBuildInformationAsync` is called.

**Expected**: Returns `BuildInformation` with the expected version.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

### RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation

**Scenario**: Mock connector returns complete build information.

**Expected**: All `BuildInformation` fields are populated.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

### RepoConnectors_ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput

**Scenario**: `ProcessRunner.TryRunAsync` with a valid OS command.

**Expected**: Returns non-null output.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull

**Scenario**: `ProcessRunner.TryRunAsync` with an invalid command.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull

**Scenario**: `ProcessRunner.TryRunAsync` with a command that exits non-zero.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput

**Scenario**: `ProcessRunner.RunAsync` with a valid command.

**Expected**: Returns output string.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_ProcessRunner_RunAsync_WithFailingCommand_ThrowsException

**Scenario**: `ProcessRunner.RunAsync` with a failing command.

**Expected**: Throws exception.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_Factory_Create_ReturnsConnector

**Scenario**: `RepoConnectorFactory.Create` is called with no configuration.

**Expected**: Returns a non-null connector instance.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectors_Factory_Create_ReturnsGitHubConnectorForThisRepo

**Scenario**: Factory detects GitHub Actions environment or remote URL.

**Expected**: Returns a `GitHubRepoConnector`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorFactory`

### RepoConnectors_ItemControls_VisibilityPublic_ReturnsPublicVisibility

**Scenario**: `ItemControlsParser.Parse` processes a block with `visibility: public`.

**Expected**: Returns controls with public visibility.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

### RepoConnectors_ItemControls_VisibilityInternal_ReturnsInternalVisibility

**Scenario**: `ItemControlsParser.Parse` processes a block with `visibility: internal`.

**Expected**: Returns controls with internal visibility.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

### RepoConnectors_ItemControls_TypeBug_ReturnsBugType

**Scenario**: `ItemControlsParser.Parse` processes a block with `type: bug`.

**Expected**: Returns controls with bug type.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

### RepoConnectors_ItemControls_TypeFeature_ReturnsFeatureType

**Scenario**: `ItemControlsParser.Parse` processes a block with `type: feature`.

**Expected**: Returns controls with feature type.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

### RepoConnectors_ItemControls_AffectedVersions_ReturnsIntervalSet

**Scenario**: `ItemControlsParser.Parse` processes a block with `affected-versions`.

**Expected**: Returns controls with a non-empty `VersionIntervalSet`.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

### RepoConnectors_ItemControls_HiddenBlock_ReturnsControls

**Scenario**: `ItemControlsParser.Parse` processes a hidden buildmark block.

**Expected**: Returns non-null controls.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

### RepoConnectors_ItemControls_NoBlock_ReturnsNull

**Scenario**: `ItemControlsParser.Parse` is called with text containing no buildmark block.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

### RepoConnectors_ItemRouter_MatchingRule_RoutesToSection

**Scenario**: `ItemRouter.Route` routes an item matching a configured rule.

**Expected**: Item appears in the correct routed section.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### RepoConnectors_ItemRouter_SuppressedRoute_OmitsItem

**Scenario**: `ItemRouter.Route` suppresses an item matching a suppressed rule.

**Expected**: Item is not present in any section.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### RepoConnectors_AzureDevOps_ImplementsInterface_ReturnsTrue

**Scenario**: Azure DevOps connector is checked against `IRepoConnector`.

**Expected**: Implements the interface.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

### RepoConnectors_AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: Azure DevOps connector receives mocked REST responses.

**Expected**: Returns valid `BuildInformation`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### RepoConnectors_AzureDevOps_GetBuildInformation_WithPullRequests_GathersChanges

**Scenario**: Mock data includes Azure DevOps pull requests.

**Expected**: `BuildInformation.Changes` contains the pull requests as change items.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### RepoConnectors_AzureDevOps_GetBuildInformation_WithOpenWorkItems_IdentifiesKnownIssues

**Scenario**: Mock data includes open work items.

**Expected**: `BuildInformation.KnownIssues` contains entries for open bugs.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### RepoConnectors_AzureDevOps_GetBuildInformation_ReleaseVersion_SkipsPreReleases

**Scenario**: Release build; pre-release tags in history are skipped.

**Expected**: Baseline is a previous release tag.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

## Requirements Coverage

- **BuildMark-RepoConnectors-IRepoConnector**: RepoConnectors_GitHubConnector_ImplementsInterface_ReturnsTrue,
  RepoConnectors_ConnectorBase_MockConnector_ImplementsInterface,
  RepoConnectors_MockConnector_ImplementsInterface_ReturnsTrue,
  RepoConnectors_AzureDevOps_ImplementsInterface_ReturnsTrue
- **BuildMark-RepoConnectors-RepoConnectorBase**: RepoConnectors_ConnectorBase_GitHubConnector_ImplementsInterface
- **BuildMark-RepoConnectors-RepoConnectorFactory**: RepoConnectors_Factory_Create_ReturnsConnector,
  RepoConnectors_Factory_Create_ReturnsGitHubConnectorForThisRepo
- **BuildMark-RepoConnectors-ItemRouter**: RepoConnectors_ItemRouter_MatchingRule_RoutesToSection,
  RepoConnectors_ItemRouter_SuppressedRoute_OmitsItem
- **BuildMark-RepoConnectors-ItemControlsParser**: RepoConnectors_ItemControls_VisibilityPublic_ReturnsPublicVisibility,
  RepoConnectors_ItemControls_VisibilityInternal_ReturnsInternalVisibility,
  RepoConnectors_ItemControls_TypeBug_ReturnsBugType,
  RepoConnectors_ItemControls_TypeFeature_ReturnsFeatureType,
  RepoConnectors_ItemControls_AffectedVersions_ReturnsIntervalSet,
  RepoConnectors_ItemControls_HiddenBlock_ReturnsControls,
  RepoConnectors_ItemControls_NoBlock_ReturnsNull
- **BuildMark-RepoConnectors-GitHub**: Multiple GitHub connector tests
- **BuildMark-RepoConnectors-AzureDevOps**: Multiple Azure DevOps connector tests
- **BuildMark-RepoConnectors-Mock**: RepoConnectors_MockConnector_Constructor_CreatesInstance,
  RepoConnectors_MockConnector_GetBuildInformation_ReturnsExpectedVersion,
  RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation
