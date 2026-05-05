### ItemControlsParser

#### Verification Approach

`ItemControlsParser` is tested through `ItemControlsParserTests.cs`, which contains
15 unit tests. The tests cover parsing `null` and empty descriptions, descriptions
with no block, and descriptions containing a buildmark block with various field
combinations (visibility, type, affected-versions, hidden). Unknown keys and
unrecognized values are tested for graceful ignorance.

#### Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

#### Test Scenarios

##### ItemControlsParser_Parse_WithNullDescription_ReturnsNull

**Scenario**: `ItemControlsParser.Parse` is called with `null`.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithEmptyDescription_ReturnsNull

**Scenario**: `ItemControlsParser.Parse` is called with an empty string.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithNoBlock_ReturnsNull

**Scenario**: `ItemControlsParser.Parse` is called with text that contains no
buildmark block.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithVisibilityPublic_ReturnsPublicVisibility

**Scenario**: Description contains a buildmark block with `visibility: public`.

**Expected**: Returns `ItemControlsInfo` with public visibility.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithVisibilityInternal_ReturnsInternalVisibility

**Scenario**: Description contains a buildmark block with `visibility: internal`.

**Expected**: Returns `ItemControlsInfo` with internal visibility.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithTypeBug_ReturnsBugType

**Scenario**: Description contains a buildmark block with `type: bug`.

**Expected**: Returns `ItemControlsInfo` with bug type.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithTypeFeature_ReturnsFeatureType

**Scenario**: Description contains a buildmark block with `type: feature`.

**Expected**: Returns `ItemControlsInfo` with feature type.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithAffectedVersions_ReturnsIntervalSet

**Scenario**: Description contains a buildmark block with `affected-versions`.

**Expected**: Returns `ItemControlsInfo` with a non-null `AffectedVersions`.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithHiddenBlock_ReturnsControls

**Scenario**: Description contains a hidden (but valid) buildmark block.

**Expected**: Returns non-null `ItemControlsInfo`.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithHiddenBlockVisibilityInternal_ReturnsInternalVisibility

**Scenario**: Hidden block contains `visibility: internal`.

**Expected**: Returns `ItemControlsInfo` with internal visibility.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithUnknownKey_IgnoresKey

**Scenario**: Buildmark block contains an unrecognized key.

**Expected**: Unknown key is ignored; other fields are parsed normally.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithUnrecognizedVisibilityValue_IgnoresValue

**Scenario**: Buildmark block contains `visibility: unknown-value`.

**Expected**: Visibility is not set; `ItemControlsInfo` has default visibility.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithUnrecognizedTypeValue_IgnoresValue

**Scenario**: Buildmark block contains `type: unknown-value`.

**Expected**: Type is not set; `ItemControlsInfo` has default type.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_AllFields_ReturnsCompleteInfo

**Scenario**: Buildmark block contains visibility, type, and affected-versions.

**Expected**: Returns `ItemControlsInfo` with all three fields populated.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

##### ItemControlsParser_Parse_WithUnrecognizedAffectedVersionsValue_IgnoresValue

**Scenario**: Buildmark block contains an invalid `affected-versions` value.

**Expected**: `AffectedVersions` is not set or is empty; no exception thrown.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemControlsParser`

#### Requirements Coverage

- **BuildMark-RepoConnectors-ItemControlsParser**: All 15 tests in
  `ItemControlsParserTests.cs`
