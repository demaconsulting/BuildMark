#### MockRepoConnector

##### Verification Approach

`MockRepoConnector` is tested through `MockRepoConnectorTests.cs`, which contains
11 unit tests. The tests verify that the connector correctly returns in-memory data
supplied via its configuration API, handles all routing and `HasRules` logic, and
correctly implements all members of `IRepoConnector`.

##### Dependencies

| Mock / Stub | Reason                             |
| ----------- | ---------------------------------- |
| None        | Self-contained in-memory connector |

##### Test Scenarios

###### MockRepoConnector_ImplementsInterface

**Scenario**: `MockRepoConnector` is checked against `IRepoConnector`.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

###### MockRepoConnector_Constructor_CreatesInstance

**Scenario**: `MockRepoConnector` is constructed with no arguments.

**Expected**: Instance is created without error.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_NoData_ReturnsEmptyBuildInformation

**Scenario**: `GetBuildInformationAsync` is called on an unconfigured connector.

**Expected**: Returns an empty `BuildInformation` instance.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_SetBuildVersion_StoresVersion

**Scenario**: `SetBuildVersion` is called with a version string.

**Expected**: `BuildInformation.Version` equals the supplied version.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_SetBaselineVersion_StoresBaseline

**Scenario**: `SetBaselineVersion` is called with a version string.

**Expected**: `BuildInformation.BaselineVersion` equals the supplied version.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_AddChange_AddsItemToChanges

**Scenario**: `AddChange` is called with a change item.

**Expected**: `BuildInformation.Changes` contains the added item.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_AddKnownIssue_AddsItemToKnownIssues

**Scenario**: `AddKnownIssue` is called with a known issue item.

**Expected**: `BuildInformation.KnownIssues` contains the added item.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_Configure_WithRules_HasRulesReturnsTrue

**Scenario**: `Configure` is called with a non-empty rules list.

**Expected**: `HasRules` returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_Configure_EmptyRules_HasRulesReturnsFalse

**Scenario**: `Configure` is called with an empty rules list.

**Expected**: `HasRules` returns `false`.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_WithConfiguredRules_PopulatesRoutedSections

**Scenario**: Connector is configured with routing rules; changes and known issues are
added; `GetBuildInformationAsync` is called.

**Expected**: `BuildInformation.RoutedSections` is populated according to the rules.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_WithChangelogLink_StoresLink

**Scenario**: A changelog link is set on the connector.

**Expected**: `BuildInformation.ChangelogLink` equals the supplied link.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_WithRulesAndKnownIssues_KnownIssuesNotInRoutedSections

**Scenario**: Connector is configured with routing rules; `GetBuildInformationAsync` is called for a
version that has known issues; routing rules include a rule that would match bug items.

**Expected**: `BuildInformation.KnownIssues` is non-empty and `BuildInformation.RoutedSections` is
populated, but no item ID that appears in `KnownIssues` also appears in any routed section, proving
that known issues are excluded from the routing path and kept in their dedicated collection.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

##### Requirements Coverage

- **BuildMark-RepoConnectors-IRepoConnector**: MockRepoConnector_ImplementsInterface
- **BuildMark-RepoConnectors-Mock**: All remaining 11 tests in `MockRepoConnectorTests.cs`
