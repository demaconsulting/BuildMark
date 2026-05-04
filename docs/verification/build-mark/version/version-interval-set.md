# VersionIntervalSet

## Verification Approach

`VersionIntervalSet` is tested through `VersionIntervalSetTests.cs`, which contains
13 unit tests. The tests cover parsing single and multiple intervals, handling of
internal commas in interval strings, empty input, discarding invalid tokens, and
`Contains` checks for strings, `VersionTag` instances, pre-release versions, and
`VersionComparable` instances.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

## Test Scenarios

### VersionIntervalSet_Parse_SingleInterval_ReturnsOneInterval

**Scenario**: `VersionIntervalSet.Parse` is called with a single interval string.

**Expected**: Returns a set containing one interval.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Parse_TwoIntervals_ReturnsTwoIntervals

**Scenario**: `VersionIntervalSet.Parse` is called with two comma-separated intervals.

**Expected**: Returns a set containing two intervals.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Parse_IntervalsWithInternalComma_ParsedCorrectly

**Scenario**: `VersionIntervalSet.Parse` is called with an interval string that
contains a comma as the separator between lower and upper bound.

**Expected**: Each interval is parsed as a unit; internal commas do not split intervals.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Parse_EmptyString_ReturnsEmptySet

**Scenario**: `VersionIntervalSet.Parse` is called with an empty string.

**Expected**: Returns an empty set.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Parse_InvalidToken_DiscardedSilently

**Scenario**: `VersionIntervalSet.Parse` is called with a string containing invalid
tokens mixed with valid intervals.

**Expected**: Invalid tokens are silently discarded; valid intervals are retained.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Contains_StringInsideFirstInterval_ReturnsTrue

**Scenario**: `Contains` is called with a string version inside the first interval.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Contains_StringInsideLaterInterval_ReturnsTrue

**Scenario**: `Contains` is called with a string version inside a later interval.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Contains_StringOutsideAllIntervals_ReturnsFalse

**Scenario**: `Contains` is called with a string version outside all intervals.

**Expected**: Returns `false`.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Contains_EmptySet_ReturnsFalse

**Scenario**: `Contains` is called on an empty set.

**Expected**: Returns `false`.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Contains_VersionTag_DelegatesToSemanticVersion

**Scenario**: `Contains` is called with a `VersionTag` argument.

**Expected**: Delegates to the tag's semantic version for comparison.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Contains_PreReleaseVersions_HandlesCorrectly

**Scenario**: `Contains` is called with pre-release version strings.

**Expected**: Correctly applies semver pre-release ordering rules.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Contains_VersionComparable_HandlesPreRelease

**Scenario**: `Contains` is called with a `VersionComparable` that has pre-release.

**Expected**: Pre-release ordering rules are applied correctly.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

### VersionIntervalSet_Parse_PreReleaseBounds_ParsesCorrectly

**Scenario**: `VersionIntervalSet.Parse` is called with intervals using pre-release
version bounds.

**Expected**: Pre-release bounds are parsed and stored correctly.

**Requirement coverage**: `BuildMark-Version-VersionIntervalSet`

## Requirements Coverage

- **BuildMark-Version-VersionIntervalSet**: All 13 tests in `VersionIntervalSetTests.cs`
