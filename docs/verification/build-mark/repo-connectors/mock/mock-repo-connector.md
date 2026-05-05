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

##### Requirements Coverage

- **BuildMark-RepoConnectors-IRepoConnector**: MockRepoConnector_ImplementsInterface
- **BuildMark-RepoConnectors-Mock**: All remaining 10 tests in `MockRepoConnectorTests.cs`
