### VersionInterval

#### Verification Approach

`VersionInterval` is tested through `VersionIntervalTests.cs`, which contains 21 unit
tests. The tests cover parsing of inclusive and exclusive lower/upper bounds, unbounded
intervals, invalid input handling, and `Contains` checks for string versions, semantic
versions, and pre-release versions.

#### Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

#### Test Scenarios

##### VersionInterval_Parse_InclusiveLower_IsInclusive

**Scenario**: `VersionInterval.Parse` is called with `[1.0.0,2.0.0]`.

**Expected**: Lower bound is inclusive; `Contains("1.0.0")` returns `true`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_ExclusiveLower_IsExclusive

**Scenario**: `VersionInterval.Parse` is called with `(1.0.0,2.0.0)`.

**Expected**: Lower bound is exclusive; `Contains("1.0.0")` returns `false`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_InclusiveUpper_IsInclusive

**Scenario**: `VersionInterval.Parse` is called with an interval using `]` upper bracket.

**Expected**: Upper bound is inclusive; `Contains` at the upper bound returns `true`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_ExclusiveUpper_IsExclusive

**Scenario**: `VersionInterval.Parse` is called with an interval using `)` upper bracket.

**Expected**: Upper bound is exclusive; `Contains` at the upper bound returns `false`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_UnboundedLower_HasNullLowerBound

**Scenario**: `VersionInterval.Parse` is called with `(,2.0.0)` (no lower bound).

**Expected**: Lower bound property is `null`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_UnboundedUpper_HasNullUpperBound

**Scenario**: `VersionInterval.Parse` is called with `[1.0.0,)` (no upper bound).

**Expected**: Upper bound property is `null`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_BothBoundsPresent_ReturnsInterval

**Scenario**: `VersionInterval.Parse` is called with a fully bounded interval.

**Expected**: Both bound properties are non-null.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_InvalidFormat_ReturnsNull

**Scenario**: `VersionInterval.Parse` is called with a string that does not match
the interval format.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_NullInput_ReturnsNull

**Scenario**: `VersionInterval.Parse` is called with `null`.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Parse_EmptyString_ReturnsNull

**Scenario**: `VersionInterval.Parse` is called with an empty string.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_StringEqualToInclusiveLower_ReturnsTrue

**Scenario**: `Contains` is called with a string equal to the inclusive lower bound.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_StringEqualToExclusiveLower_ReturnsFalse

**Scenario**: `Contains` is called with a string equal to the exclusive lower bound.

**Expected**: Returns `false`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_StringEqualToInclusiveUpper_ReturnsTrue

**Scenario**: `Contains` is called with a string equal to the inclusive upper bound.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_StringEqualToExclusiveUpper_ReturnsFalse

**Scenario**: `Contains` is called with a string equal to the exclusive upper bound.

**Expected**: Returns `false`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_StringInsideUnboundedInterval_ReturnsTrue

**Scenario**: `Contains` is called with a version inside an unbounded interval.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_StringOutsideInterval_ReturnsFalse

**Scenario**: `Contains` is called with a version outside the interval bounds.

**Expected**: Returns `false`.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_Version_DelegatesToSemanticVersion

**Scenario**: `Contains` is called with a `VersionSemantic` argument.

**Expected**: Comparison delegates to the semantic version correctly.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_PreReleaseBounds_HandlesCorrectly

**Scenario**: An interval with pre-release bounds is created; `Contains` is called
with a pre-release version.

**Expected**: Correctly determines membership using semver pre-release ordering.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_PreReleaseToPreRelease_HandlesCorrectly

**Scenario**: An interval spanning two pre-release versions; `Contains` checks a
version between them.

**Expected**: Returns `true` for versions in range, `false` outside.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_PreReleaseOrdering_UsesNumericComparison

**Scenario**: Interval bounds use numeric pre-release identifiers; `Contains` is
called with intermediate numeric pre-releases.

**Expected**: Numeric comparison is applied correctly.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

##### VersionInterval_Contains_VersionComparable_HandlesPreRelease

**Scenario**: `Contains` is called with a `VersionComparable` that has a pre-release.

**Expected**: Comparison uses semver pre-release ordering rules.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

#### Requirements Coverage

- **BuildMark-Version-VersionInterval**: All 21 tests in `VersionIntervalTests.cs`
