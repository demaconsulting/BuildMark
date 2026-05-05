### RepoConnectorBase

#### Verification Approach

`RepoConnectorBase` is tested through `RepoConnectorBaseTests.cs`, which contains
5 unit tests. The tests exercise `Configure` (storing rules and sections and setting
`HasRules`), `ApplyRules` (routing items to sections), and `FindVersionIndex` (locating
a version tag in a list, including cross-prefix equality).

#### Dependencies

| Mock / Stub               | Reason                                                         |
| ------------------------- | -------------------------------------------------------------- |
| Concrete subclass fixture | Tests instantiate a minimal concrete subclass for testing base |

#### Test Scenarios

##### RepoConnectorBase_Configure_StoresRulesAndSections_HasRulesReturnsTrue

**Scenario**: `Configure` is called with a non-empty rules list.

**Expected**: `HasRules` returns `true`; rules are stored for use in `ApplyRules`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorBase`

##### RepoConnectorBase_Configure_EmptyRules_HasRulesReturnsFalse

**Scenario**: `Configure` is called with an empty rules list.

**Expected**: `HasRules` returns `false`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorBase`

##### RepoConnectorBase_ApplyRules_RoutesItemsToConfiguredSections

**Scenario**: `ApplyRules` is called with items and configured routing rules.

**Expected**: Each item is placed in the correct section as specified by the rules.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorBase`

##### RepoConnectorBase_FindVersionIndex_DifferentPrefixSameVersion_ReturnsCorrectIndex

**Scenario**: `FindVersionIndex` is called with a tag list containing a tag with a
different prefix but the same version as the search target.

**Expected**: The index of the matching tag is returned.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorBase`

##### RepoConnectorBase_FindVersionIndex_VersionNotInList_ReturnsMinusOne

**Scenario**: `FindVersionIndex` is called with a version that is not in the list.

**Expected**: Returns `-1`.

**Requirement coverage**: `BuildMark-RepoConnectors-RepoConnectorBase`

#### Requirements Coverage

- **BuildMark-RepoConnectors-RepoConnectorBase**: All 5 tests in `RepoConnectorBaseTests.cs`
