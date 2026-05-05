#### GitHubRepoConnector

##### Verification Approach

`GitHubRepoConnector` is tested through `GitHubRepoConnectorTests.cs`, which contains
22 unit tests. The tests exercise constructor behavior (with and without config),
the full `GetBuildInformationAsync` pipeline with various scenarios, visibility and
type overrides, routing configuration, known issues filtering by affected versions,
and edge cases such as duplicate commit SHAs and substring label matching.

##### Dependencies

| Mock / Stub              | Reason                                                   |
| ------------------------ | -------------------------------------------------------- |
| `MockHttpMessageHandler` | Intercepts all HTTP calls to the GitHub GraphQL endpoint |

##### Test Scenarios

###### GitHubRepoConnector_Constructor_CreatesInstance

**Scenario**: `GitHubRepoConnector` is constructed with no configuration.

**Expected**: Instance is created without error.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_Constructor_WithConfig_StoresConfigurationOverrides

**Scenario**: `GitHubRepoConnector` is constructed with a `GitHubConnectorConfig`.

**Expected**: `ConfigurationOverrides.Owner` equals `"example-owner"`;
`ConfigurationOverrides.Repo` equals `"example-repo"`; `ConfigurationOverrides.BaseUrl`
equals `"https://api.github.com"`.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_ImplementsInterface_ReturnsTrue

**Scenario**: Connector is checked against `IRepoConnector`.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

###### GitHubRepoConnector_GetBuildInformationAsync_WithMockedData_ReturnsValidBuildInformation

**Scenario**: `GetBuildInformationAsync` processes mocked GraphQL responses.

**Expected**: Returns a `BuildInformation` instance with correct version, baseline,
changes, and known issues.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_WithMultipleVersions_SelectsCorrectPreviousVersionAndGeneratesChangelogLink

**Scenario**: Multiple version tags exist in the mocked response.

**Expected**: Selects the correct previous release and generates a GitHub changelog link.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_WithPullRequests_GathersChangesCorrectly

**Scenario**: Mocked data includes pull requests with labels.

**Expected**: Each pull request is represented as a change item with correct type
classification.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_WithOpenIssues_IdentifiesKnownIssues

**Scenario**: Mocked data includes open issues.

**Expected**: Open issues appear in `BuildInformation.KnownIssues`.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_PreReleaseWithSameCommitHash_SkipsToNextDifferentHash

**Scenario**: A pre-release tag shares the same commit hash as the build version.

**Expected**: Connector skips to the next tag with a different commit hash.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_ReleaseVersion_SkipsAllPreReleases

**Scenario**: Build version is a release; prior history contains pre-release tags.

**Expected**: Pre-release tags are skipped; baseline is the previous release.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_PreReleaseNotInHistory_UsesLatestDifferentHash

**Scenario**: Pre-release tag is not found in commit history; connector falls back.

**Expected**: Uses the latest tag with a different commit hash as baseline.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_PreReleaseAllPreviousSameHash_ReturnsNullBaseline

**Scenario**: All previous tags share the same commit hash as the build version.

**Expected**: `BuildInformation.BaselineVersion` is `null`.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_WithDuplicateMergeCommitSha_DoesNotThrow

**Scenario**: Mocked data contains pull requests with duplicate merge commit SHAs.

**Expected**: No exception is thrown; result is returned normally.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_PrWithSubstringMatchLabel_NotClassifiedAsBug

**Scenario**: A pull request has a label that is a substring of `"bug"` (e.g., `"b"`).

**Expected**: Pull request is not classified as a bug.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_IssueWithSubstringMatchLabel_NotClassifiedAsKnownIssue

**Scenario**: An issue has a label that is a substring of a known issue label.

**Expected**: Issue is not classified as a known issue.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_VisibilityInternal_ExcludesItem

**Scenario**: An item has `visibility: internal` in its buildmark block.

**Expected**: Item is excluded from the public output.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_VisibilityPublic_IncludesItem

**Scenario**: An item has `visibility: public` in its buildmark block.

**Expected**: Item is included in the output.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_TypeBugOverride_ClassifiesAsBug

**Scenario**: An item has `type: bug` in its buildmark block.

**Expected**: Item is classified as a bug regardless of labels.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_TypeFeatureOverride_ClassifiesAsFeature

**Scenario**: An item has `type: feature` in its buildmark block.

**Expected**: Item is classified as a feature regardless of labels.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_Configure_WithRules_HasRulesReturnsTrue

**Scenario**: `Configure` is called with routing rules.

**Expected**: `HasRules` returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_WithConfiguredRules_PopulatesRoutedSections

**Scenario**: Connector is configured with routing rules and run with mock data.

**Expected**: `BuildInformation.RoutedSections` is populated with items routed per rules.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_KnownIssues_FilteredByAffectedVersions

**Scenario**: Known issues have `affected-versions` set; build version is outside the range.

**Expected**: Issues outside the affected version range are excluded.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

###### GitHubRepoConnector_GetBuildInformationAsync_ClosedBugWithMatchingAffectedVersions_IsKnownIssue

**Scenario**: A closed bug has `affected-versions` that includes the build version.

**Expected**: Closed bug appears in `KnownIssues`.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

##### Requirements Coverage

- **BuildMark-RepoConnectors-IRepoConnector**: GitHubRepoConnector_ImplementsInterface_ReturnsTrue
- **BuildMark-RepoConnectors-GitHub**: All remaining 21 tests in `GitHubRepoConnectorTests.cs`
