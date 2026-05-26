#### MockRepoConnector

##### Verification Approach

`MockRepoConnector` is tested through `MockRepoConnectorTests.cs`, which contains 12 unit tests.
The tests verify that the connector correctly returns in-memory data supplied via its configuration
API, handles all routing and `HasRules` logic, and correctly implements all members of
`IRepoConnector`.

##### Dependencies

| Mock / Stub | Reason                             |
| ----------- | ---------------------------------- |
| None        | Self-contained in-memory connector |

##### Test Environment

Standard dotnet test host; no external dependencies or environment setup required.

##### Acceptance Criteria

All 12 tests in `MockRepoConnectorTests.cs` pass with no errors or warnings.

##### Test Scenarios

###### MockRepoConnector_ImplementsInterface

**Scenario**: `MockRepoConnector` is checked against `IRepoConnector`.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

###### MockRepoConnector_Constructor_CreatesInstance

**Scenario**: `MockRepoConnector` is constructed with no arguments.

**Expected**: Instance is created without error.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_ReturnsExpectedVersion

**Scenario**: `GetBuildInformationAsync` is called with an explicit version tag.

**Expected**: `BuildInformation.CurrentVersionTag` reflects the supplied version and commit hash.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_WithValidVersionFromTags_ReturnsCorrectBaseline

**Scenario**: `GetBuildInformationAsync` is called for a version that exists in the mock tag data.

**Expected**: Build information is returned with the correct version tag and a non-null commit hash.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_ReturnsCompleteInformation

**Scenario**: `GetBuildInformationAsync` is called for a version that has associated changes.

**Expected**: All expected components (`Changes`, `Bugs`, `KnownIssues`, `CurrentVersionTag`) are
present and non-null.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_CategorizesChangesCorrectly

**Scenario**: `GetBuildInformationAsync` is called for a version that has both bug and non-bug
changes.

**Expected**: Items with type `bug` appear only in `Bugs`; all items in `Bugs` have type `bug`.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_Configure_StoresRulesAndSections

**Scenario**: `Configure` is called with a non-empty rules and sections list, then
`GetBuildInformationAsync` is called.

**Expected**: `BuildInformation.RoutedSections` is non-null, confirming rules were stored and
applied.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_WithRules_ReturnsRoutedSections

**Scenario**: Connector is configured with two routing rules (`bug` label → `bugs`, catch-all
→ `features`).

**Expected**: `RoutedSections` contains exactly two sections with titles `Features` and `Bugs`.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_WithoutRules_ReturnsNullRoutedSections

**Scenario**: `GetBuildInformationAsync` is called on a connector with no rules configured.

**Expected**: `RoutedSections` is null (legacy mode).

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_KnownIssues_FilteredByAffectedVersions

**Scenario**: Version v2.0.0 is requested; issue 5 has affected-versions `[5.0.0,)` (outside
range) while issues 4 and 6 have no affected-versions restriction.

**Expected**: Issues 4 and 6 appear in `KnownIssues`; issue 5 does not.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_ClosedBugWithMatchingAffectedVersions_IsKnownIssue

**Scenario**: Issue 7 is a closed bug with affected-versions `[1.0.0,1.0.0]`. Build is requested
for v1.0.0 and then v2.0.0.

**Expected**: Issue 7 appears in `KnownIssues` for v1.0.0 (in-range); issue 7 does not appear for
v2.0.0 (out-of-range).

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

###### MockRepoConnector_GetBuildInformationAsync_WithRulesAndKnownIssues_KnownIssuesNotInRoutedSections

**Scenario**: Connector is configured with routing rules; `GetBuildInformationAsync` is called for
a version that has known issues; routing rules include a rule that would match bug items.

**Expected**: `BuildInformation.KnownIssues` is non-empty and `BuildInformation.RoutedSections` is
populated, but no item ID that appears in `KnownIssues` also appears in any routed section, proving
that known issues are excluded from the routing path and kept in their dedicated collection.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`
