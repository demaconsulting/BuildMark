#### GitHubRepoConnector

##### Verification Approach

`GitHubRepoConnector` is tested through `GitHubRepoConnectorTests.cs`, which contains
35 unit tests. The tests exercise constructor behavior (with and without config),
the full `GetBuildInformationAsync` pipeline with various scenarios, URL parsing for
github.com, GitHub Enterprise Cloud, and GitHub Enterprise Server remotes (SSH and HTTPS),
visibility and type overrides, routing configuration, known issues filtering by affected
versions, and edge cases such as duplicate commit SHAs and substring label matching.

##### Dependencies

| Mock / Stub              | Reason                                                   |
| ------------------------ | -------------------------------------------------------- |
| `MockHttpMessageHandler` | Intercepts all HTTP calls to the GitHub GraphQL endpoint |

##### Test Environment

Tests use `MockHttpMessageHandler` to intercept HTTP calls. No real network access or GitHub token is required.

##### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

##### Test Scenarios

###### GitHubRepoConnector_Constructor_CreatesInstance

**Scenario**: `GitHubRepoConnector` is constructed with no configuration.

**Expected**: Instance is created without error.

**Requirement coverage**: `BuildMark-GitHub-ConnectorConfig`

###### GitHubRepoConnector_Constructor_WithConfig_StoresConfigurationOverrides

**Scenario**: `GitHubRepoConnector` is constructed with a `GitHubConnectorConfig`.

**Expected**: `ConfigurationOverrides.Owner` equals `"example-owner"`;
`ConfigurationOverrides.Repo` equals `"example-repo"`; `ConfigurationOverrides.BaseUrl`
equals `"https://api.github.com"`.

**Requirement coverage**: `BuildMark-GitHub-ConnectorConfig`

###### GitHubRepoConnector_ImplementsInterface_ReturnsTrue

**Scenario**: Connector is checked against `IRepoConnector`.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectorBase-Interface`

###### GitHubRepoConnector_GetBuildInformationAsync_WithMockedData_ReturnsValidBuildInformation

**Scenario**: `GetBuildInformationAsync` processes mocked GraphQL responses.

**Expected**: Returns a `BuildInformation` instance with correct version, baseline,
changes, and known issues.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_WithMultipleVersions_SelectsCorrectPreviousVersionAndGeneratesChangelogLink

**Scenario**: Multiple version tags exist in the mocked response.

**Expected**: Selects the correct previous release and generates a GitHub changelog link.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_WithPullRequests_GathersChangesCorrectly

**Scenario**: Mocked data includes pull requests with labels.

**Expected**: Each pull request is represented as a change item with correct type
classification.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_WithOpenIssues_IdentifiesKnownIssues

**Scenario**: Mocked data includes open issues.

**Expected**: Open issues appear in `BuildInformation.KnownIssues`.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_PreReleaseWithSameCommitHash_SkipsToNextDifferentHash

**Scenario**: A pre-release tag shares the same commit hash as the build version.

**Expected**: Connector skips to the next tag with a different commit hash.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_ReleaseVersion_SkipsAllPreReleases

**Scenario**: Build version is a release; prior history contains pre-release tags.

**Expected**: Pre-release tags are skipped; baseline is the previous release.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_PreReleaseNotInHistory_UsesLatestDifferentHash

**Scenario**: Pre-release tag is not found in commit history; connector falls back.

**Expected**: Uses the latest tag with a different commit hash as baseline.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_PreReleaseAllPreviousSameHash_ReturnsNullBaseline

**Scenario**: All previous tags share the same commit hash as the build version.

**Expected**: `BuildInformation.BaselineVersion` is `null`.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_WithDuplicateMergeCommitSha_DoesNotThrow

**Scenario**: Mocked data contains pull requests with duplicate merge commit SHAs.

**Expected**: No exception is thrown; result is returned normally.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_PrWithSubstringMatchLabel_NotClassifiedAsBug

**Scenario**: A pull request has a label that is a substring of `"bug"` (e.g., `"b"`).

**Expected**: Pull request is not classified as a bug.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_IssueWithSubstringMatchLabel_NotClassifiedAsKnownIssue

**Scenario**: An issue has a label that is a substring of a known issue label.

**Expected**: Issue is not classified as a known issue.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_VisibilityInternal_ExcludesItem

**Scenario**: An item has `visibility: internal` in its buildmark block.

**Expected**: Item is excluded from the public output.

**Requirement coverage**: `BuildMark-GitHub-ItemControls`

###### GitHubRepoConnector_GetBuildInformationAsync_VisibilityPublic_IncludesItem

**Scenario**: An item has `visibility: public` in its buildmark block.

**Expected**: Item is included in the output.

**Requirement coverage**: `BuildMark-GitHub-ItemControls`

###### GitHubRepoConnector_GetBuildInformationAsync_TypeBugOverride_ClassifiesAsBug

**Scenario**: An item has `type: bug` in its buildmark block.

**Expected**: Item is classified as a bug regardless of labels.

**Requirement coverage**: `BuildMark-GitHub-ItemControls`

###### GitHubRepoConnector_GetBuildInformationAsync_TypeFeatureOverride_ClassifiesAsFeature

**Scenario**: An item has `type: feature` in its buildmark block.

**Expected**: Item is classified as a feature regardless of labels.

**Requirement coverage**: `BuildMark-GitHub-ItemControls`

###### GitHubRepoConnector_Configure_WithRules_HasRulesReturnsTrue

**Scenario**: `Configure` is called with routing rules.

**Expected**: `HasRules` returns `true`.

**Requirement coverage**: `BuildMark-GitHub-Rules`

###### GitHubRepoConnector_GetBuildInformationAsync_WithConfiguredRules_PopulatesRoutedSections

**Scenario**: Connector is configured with routing rules and run with mock data.

**Expected**: `BuildInformation.RoutedSections` is populated with items routed per rules.

**Requirement coverage**: `BuildMark-GitHub-Rules`

###### GitHubRepoConnector_GetBuildInformationAsync_KnownIssues_FilteredByAffectedVersions

**Scenario**: Known issues have `affected-versions` set; build version is outside the range.

**Expected**: Issues outside the affected version range are excluded.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_ClosedBugWithMatchingAffectedVersions_IsKnownIssue

**Scenario**: A closed bug has `affected-versions` that includes the build version.

**Expected**: Closed bug appears in `KnownIssues`.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`

###### GitHubRepoConnector_GetBuildInformationAsync_WithTokenVariable_UsesCustomVariable

**Scenario**: `GetBuildInformationAsync` is called with a `GitHubConnectorConfig` that
specifies a `TokenVariable` name; the environment variable is set to a non-empty token.

**Expected**: Build information is returned successfully, confirming the custom token
variable was resolved and used without throwing.

**Requirement coverage**: `BuildMark-GitHub-TokenVariable`

###### GitHubRepoConnector_GetBuildInformationAsync_WithTokenVariable_EmptyValue_ThrowsInvalidOperationException

**Scenario**: `GetBuildInformationAsync` is called with a `TokenVariable` config whose
corresponding environment variable is set to an empty string.

**Expected**: `InvalidOperationException` is thrown.

**Requirement coverage**: `BuildMark-GitHub-TokenVariable`

###### GitHubRepoConnector_GetBuildInformationAsync_WithTokenVariable_NotSet_ThrowsInvalidOperationException

**Scenario**: `GetBuildInformationAsync` is called with a `TokenVariable` config whose
corresponding environment variable is not set (null).

**Expected**: `InvalidOperationException` is thrown.

**Requirement coverage**: `BuildMark-GitHub-TokenVariable`

###### GitHubRepoConnector_ParseGitHubUrl_GitHubCom_SSH_ReturnsOwnerAndRepo

**Scenario**: `ParseGitHubUrl` is called with a standard github.com SSH URL
(`git@github.com:owner/repo.git`).

**Expected**: Returns owner `"owner"` and repo `"repo"`.

**Requirement coverage**: `BuildMark-GitHub-ParseUrl-SSH`

###### GitHubRepoConnector_ParseGitHubUrl_GitHubCom_HTTPS_ReturnsOwnerAndRepo

**Scenario**: `ParseGitHubUrl` is called with a standard github.com HTTPS URL
(`https://github.com/owner/repo`).

**Expected**: Returns owner `"owner"` and repo `"repo"`.

**Requirement coverage**: `BuildMark-GitHub-ParseUrl-HTTPS`

###### GitHubRepoConnector_ParseGitHubUrl_GitHubCom_HTTPS_WithGitSuffix_ReturnsOwnerAndRepo

**Scenario**: `ParseGitHubUrl` is called with a github.com HTTPS URL that includes a `.git` suffix
(`https://github.com/owner/repo.git`).

**Expected**: Returns owner `"owner"` and repo `"repo"` with the `.git` suffix stripped.

**Requirement coverage**: `BuildMark-GitHub-ParseUrl-HTTPS`

###### GitHubRepoConnector_ParseGitHubUrl_GHECloud_HTTPS_ReturnsOwnerAndRepo

**Scenario**: `ParseGitHubUrl` is called with a GitHub Enterprise Cloud HTTPS URL
(`https://hiarc.ghe.com/BreakAway/PyStubGenerator`).

**Expected**: Returns owner `"BreakAway"` and repo `"PyStubGenerator"`.

**Requirement coverage**: `BuildMark-GitHub-ParseUrl-HTTPS`

###### GitHubRepoConnector_ParseGitHubUrl_GHEServer_HTTPS_ReturnsOwnerAndRepo

**Scenario**: `ParseGitHubUrl` is called with a GitHub Enterprise Server on-premises HTTPS URL
(`https://github.mycompany.com/myorg/myrepo`).

**Expected**: Returns owner `"myorg"` and repo `"myrepo"`.

**Requirement coverage**: `BuildMark-GitHub-ParseUrl-HTTPS`

###### GitHubRepoConnector_ParseGitHubUrl_GHEServer_SSH_ReturnsOwnerAndRepo

**Scenario**: `ParseGitHubUrl` is called with a GitHub Enterprise Server on-premises SSH URL
(`git@github.mycompany.com:myorg/myrepo.git`).

**Expected**: Returns owner `"myorg"` and repo `"myrepo"`.

**Requirement coverage**: `BuildMark-GitHub-ParseUrl-SSH`

###### GitHubRepoConnector_ParseGitHubUrl_Invalid_ThrowsArgumentException

**Scenario**: `ParseGitHubUrl` is called with an unrecognized string (`not-a-url`).

**Expected**: `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-GitHub-ParseUrl-HTTPS`

###### GitHubRepoConnector_GetBuildInformationAsync_GHERemote_ChangelogUrlUsesGHEHost

**Scenario**: `GetBuildInformationAsync` is called with a GitHub Enterprise Server HTTPS remote
URL (`https://github.mycompany.com/myorg/myrepo.git`).

**Expected**: `CompleteChangelogLink.TargetUrl` starts with `https://github.mycompany.com/`,
confirming the changelog URL uses the GHE hostname instead of the hardcoded `github.com`.

**Requirement coverage**: `BuildMark-GitHub-ParseUrl-HTTPS`
