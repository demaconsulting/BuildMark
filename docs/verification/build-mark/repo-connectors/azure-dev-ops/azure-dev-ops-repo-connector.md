# AzureDevOpsRepoConnector

## Verification Approach

`AzureDevOpsRepoConnector` is tested through `AzureDevOpsRepoConnectorTests.cs`,
which contains 25 unit tests. The tests exercise constructor behavior, the full
`GetBuildInformationAsync` pipeline, visibility overrides, type overrides, routing
configuration, known issues filtering by affected versions, and edge cases including
work item deduplication and version tag handling.

## Dependencies

| Mock / Stub              | Reason                                                      |
| ------------------------ | ----------------------------------------------------------- |
| `MockHttpMessageHandler` | Intercepts all HTTP calls to the Azure DevOps REST endpoint |

## Test Scenarios

### AzureDevOpsRepoConnector_Constructor_CreatesInstance

**Scenario**: Connector is constructed with no configuration.

**Expected**: Instance is created without error.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_Constructor_WithConfig_StoresConfigurationOverrides

**Scenario**: Connector is constructed with an `AzureDevOpsConnectorConfig`.

**Expected**: Configuration overrides are stored.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_ImplementsInterface_ReturnsTrue

**Scenario**: Connector is checked against `IRepoConnector`.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithMockedData_ReturnsValidBuildInformation

**Scenario**: `GetBuildInformationAsync` processes mocked REST API responses.

**Expected**: Returns a `BuildInformation` instance with correct version, baseline,
changes, and known issues.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithMultipleVersions_SelectsCorrectBaseline

**Scenario**: Multiple version tags exist in the mocked response.

**Expected**: Selects the correct previous release as baseline.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithWorkItems_GathersChangesCorrectly

**Scenario**: Mocked data includes work items linked to commits.

**Expected**: Work items appear as change items.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithOpenBugs_IdentifiesKnownIssues

**Scenario**: Mocked data includes open bug work items.

**Expected**: Open bugs appear in `KnownIssues`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_PreReleaseWithSameCommitHash_SkipsToNextDifferentHash

**Scenario**: A pre-release tag shares the same commit hash as the build version.

**Expected**: Connector skips to the next tag with a different commit hash.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_ReleaseVersion_SkipsAllPreReleases

**Scenario**: Build version is a release; prior history contains pre-release tags.

**Expected**: Pre-release tags are skipped; baseline is the previous release.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_PreReleaseNotInHistory_UsesLatestDifferentHash

**Scenario**: Pre-release tag is not found in commit history; connector falls back.

**Expected**: Uses the latest tag with a different commit hash as baseline.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_PreReleaseAllPreviousSameHash_ReturnsNullBaseline

**Scenario**: All previous tags share the same commit hash.

**Expected**: `BuildInformation.BaselineVersion` is `null`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithDuplicateWorkItem_DeduplicatesChanges

**Scenario**: Mocked data contains the same work item linked to multiple commits.

**Expected**: Work item appears only once in `Changes`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_VisibilityInternal_ExcludesItem

**Scenario**: A work item has `visibility: internal` in its buildmark block.

**Expected**: Work item is excluded from the public output.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_VisibilityPublic_IncludesItem

**Scenario**: A work item has `visibility: public` in its buildmark block.

**Expected**: Work item is included in the output.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_TypeBugOverride_ClassifiesAsBug

**Scenario**: A work item has `type: bug` in its buildmark block.

**Expected**: Work item is classified as a bug.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_TypeFeatureOverride_ClassifiesAsFeature

**Scenario**: A work item has `type: feature` in its buildmark block.

**Expected**: Work item is classified as a feature.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_Configure_WithRules_HasRulesReturnsTrue

**Scenario**: `Configure` is called with routing rules.

**Expected**: `HasRules` returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithConfiguredRules_PopulatesRoutedSections

**Scenario**: Connector is configured with routing rules and run with mock data.

**Expected**: `BuildInformation.RoutedSections` is populated correctly.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_KnownIssues_FilteredByAffectedVersions

**Scenario**: Known issues have `affected-versions` set; build version is outside the
range.

**Expected**: Issues outside the range are excluded.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_ClosedBugWithMatchingAffectedVersions_IsKnownIssue

**Scenario**: A closed bug has `affected-versions` that includes the build version.

**Expected**: Closed bug appears in `KnownIssues`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithNoTags_ReturnsEmptyBuildInformation

**Scenario**: Repository has no version tags.

**Expected**: Returns `BuildInformation` with null baseline and empty change lists.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithNoCommitsBetweenVersions_ReturnsEmptyChanges

**Scenario**: No commits exist between the build version and the baseline.

**Expected**: `Changes` is empty.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WorkItemWithNoBugType_NotKnownIssue

**Scenario**: Open work item is not of type bug.

**Expected**: Work item is not added to `KnownIssues`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WorkItemWithCompletedState_NotKnownIssue

**Scenario**: Bug work item is in a completed state.

**Expected**: Completed bug is not added to `KnownIssues`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

### AzureDevOpsRepoConnector_GetBuildInformationAsync_WithAzureDevOpsUrl_GeneratesChangelogLink

**Scenario**: Repository URL is an Azure DevOps URL.

**Expected**: Generated changelog link uses the Azure DevOps compare format.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

## Requirements Coverage

- **BuildMark-RepoConnectors-IRepoConnector**:
  AzureDevOpsRepoConnector_ImplementsInterface_ReturnsTrue
- **BuildMark-RepoConnectors-AzureDevOps**: All remaining 24 tests in
  `AzureDevOpsRepoConnectorTests.cs`
