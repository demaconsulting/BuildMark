### ItemControlsInfo

#### Verification Approach

`ItemControlsInfo` is a data model class with no dedicated test class. It is verified
indirectly through `ItemControlsTests.cs` and `ItemControlsParserTests.cs`. The parser
tests assert on the resulting `ItemControlsInfo` instances, confirming that visibility,
type, and affected versions fields are populated correctly.

#### Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

#### Test Scenarios (via ItemControlsTests.cs - 13 tests)

##### ItemControls_Parse_WithVisibilityPublic_ReturnsPublicVisibility

**Scenario**: A buildmark block with `visibility: public` is parsed.

**Expected**: `ItemControlsInfo.Visibility` is public.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsInfo`

##### ItemControls_Parse_WithVisibilityInternal_ReturnsInternalVisibility

**Scenario**: A buildmark block with `visibility: internal` is parsed.

**Expected**: `ItemControlsInfo.Visibility` is internal.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsInfo`

##### ItemControls_Parse_WithTypeBug_ReturnsBugType

**Scenario**: A buildmark block with `type: bug` is parsed.

**Expected**: `ItemControlsInfo.Type` is bug.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsInfo`

##### ItemControls_Parse_WithTypeFeature_ReturnsFeatureType

**Scenario**: A buildmark block with `type: feature` is parsed.

**Expected**: `ItemControlsInfo.Type` is feature.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsInfo`

##### ItemControls_Parse_WithAffectedVersions_ReturnsIntervalSet

**Scenario**: A buildmark block with `affected-versions` is parsed.

**Expected**: `ItemControlsInfo.AffectedVersions` contains the interval set.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsInfo`

##### ItemControls_Parse_WithHiddenBlock_ReturnsControls

**Scenario**: A hidden buildmark block is parsed.

**Expected**: `ItemControlsInfo` is non-null.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsInfo`

##### ItemControls_Parse_WithNoBlock_ReturnsNull

**Scenario**: Text with no buildmark block is parsed.

**Expected**: Returns `null` (no `ItemControlsInfo`).

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsInfo`

#### Requirements Coverage

- **BuildMark-RepoConnectors-ItemControlsInfo**: All 13 tests in `ItemControlsTests.cs`
