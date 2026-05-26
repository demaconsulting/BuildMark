### VersionInterval

#### Verification Approach

`VersionInterval` is a pure logic unit with no external dependencies, so no mocks or stubs are
needed. The unit is exercised through `VersionIntervalTests.cs`, which contains 21 unit tests
covering parsing of inclusive and exclusive lower and upper bounds, bounded and unbounded
intervals, invalid input handling, and `Contains` checks for strings, `VersionSemantic`,
pre-release versions, and `VersionComparable` values.

#### Test Environment

N/A - standard test environment. No external dependencies or environment setup required.

#### Acceptance Criteria

- All 21 tests in `VersionIntervalTests.cs` pass with zero failures.

#### Test Scenarios

**VersionInterval_Parse_InclusiveLower_IsInclusive**: This scenario verifies that an interval with
an inclusive lower bound contains a version exactly equal to that bound, which confirms correct
bracket semantics. This scenario is tested by
`VersionInterval_Parse_InclusiveLower_IsInclusive`.

**VersionInterval_Parse_ExclusiveLower_IsExclusive**: This scenario verifies that an interval with
an exclusive lower bound rejects a version exactly equal to that bound. This scenario is tested by
`VersionInterval_Parse_ExclusiveLower_IsExclusive`.

**VersionInterval_Parse_InclusiveUpper_IsInclusive**: This scenario verifies that an interval with
an inclusive upper bound contains a version exactly equal to that upper limit. This scenario is
tested by `VersionInterval_Parse_InclusiveUpper_IsInclusive`.

**VersionInterval_Parse_ExclusiveUpper_IsExclusive**: This scenario verifies that an interval with
an exclusive upper bound rejects a version exactly equal to that upper limit. This scenario is
tested by `VersionInterval_Parse_ExclusiveUpper_IsExclusive`.

**VersionInterval_Parse_UnboundedLower_HasNullLowerBound**: This scenario verifies that parsing an
interval with no lower bound leaves the lower-bound property unset so the range extends downward
without restriction. This scenario is tested by
`VersionInterval_Parse_UnboundedLower_HasNullLowerBound`.

**VersionInterval_Parse_UnboundedUpper_HasNullUpperBound**: This scenario verifies that parsing an
interval with no upper bound leaves the upper-bound property unset so the range extends upward
without restriction. This scenario is tested by
`VersionInterval_Parse_UnboundedUpper_HasNullUpperBound`.

**VersionInterval_Parse_BothBoundsPresent_ReturnsInterval**: This scenario verifies that a fully
bounded interval retains both parsed bound values so closed and open range checks can be applied.
This scenario is tested by `VersionInterval_Parse_BothBoundsPresent_ReturnsInterval`.

**VersionInterval_Parse_InvalidFormat_ReturnsNull**: This scenario verifies that a malformed
interval string is rejected by returning null rather than creating an unusable interval. This
scenario is tested by `VersionInterval_Parse_InvalidFormat_ReturnsNull`.

**VersionInterval_Parse_NullInput_ReturnsNull**: This scenario verifies that null input is handled
safely and returns null so callers can probe optional interval values. This scenario is tested by
`VersionInterval_Parse_NullInput_ReturnsNull`.

**VersionInterval_Parse_EmptyString_ReturnsNull**: This scenario verifies that an empty string does
not parse into an interval and instead returns null. This scenario is tested by
`VersionInterval_Parse_EmptyString_ReturnsNull`.

**VersionInterval_Contains_StringEqualToInclusiveLower_ReturnsTrue**: This scenario verifies that
`Contains` returns true for a string version equal to an inclusive lower bound. This scenario is
tested by `VersionInterval_Contains_StringEqualToInclusiveLower_ReturnsTrue`.

**VersionInterval_Contains_StringEqualToExclusiveLower_ReturnsFalse**: This scenario verifies that
`Contains` returns false for a string version equal to an exclusive lower bound. This scenario is
tested by `VersionInterval_Contains_StringEqualToExclusiveLower_ReturnsFalse`.

**VersionInterval_Contains_StringEqualToInclusiveUpper_ReturnsTrue**: This scenario verifies that
`Contains` returns true for a string version equal to an inclusive upper bound. This scenario is
tested by `VersionInterval_Contains_StringEqualToInclusiveUpper_ReturnsTrue`.

**VersionInterval_Contains_StringEqualToExclusiveUpper_ReturnsFalse**: This scenario verifies that
`Contains` returns false for a string version equal to an exclusive upper bound. This scenario is
tested by `VersionInterval_Contains_StringEqualToExclusiveUpper_ReturnsFalse`.

**VersionInterval_Contains_StringInsideUnboundedInterval_ReturnsTrue**: This scenario verifies
that a version inside an interval with an unbounded side is accepted when it satisfies the present
bound. This scenario is tested by
`VersionInterval_Contains_StringInsideUnboundedInterval_ReturnsTrue`.

**VersionInterval_Contains_StringOutsideInterval_ReturnsFalse**: This scenario verifies that a
version outside the configured bounds is rejected so interval filtering remains reliable. This
scenario is tested by `VersionInterval_Contains_StringOutsideInterval_ReturnsFalse`.

**VersionInterval_Contains_Version_DelegatesToSemanticVersion**: This scenario verifies that a
`VersionSemantic` argument is evaluated using its semantic version information rather than string
comparison. This scenario is tested by
`VersionInterval_Contains_Version_DelegatesToSemanticVersion`.

**VersionInterval_Contains_PreReleaseBounds_HandlesCorrectly**: This scenario verifies that
interval bounds containing pre-release versions use semantic-version precedence rules when checking
membership. This scenario is tested by
`VersionInterval_Contains_PreReleaseBounds_HandlesCorrectly`.

**VersionInterval_Contains_PreReleaseToPreRelease_HandlesCorrectly**: This scenario verifies that
an interval spanning two pre-release versions accepts in-range values and rejects out-of-range
values across the pre-release boundary. This scenario is tested by
`VersionInterval_Contains_PreReleaseToPreRelease_HandlesCorrectly`.

**VersionInterval_Contains_PreReleaseOrdering_UsesNumericComparison**: This scenario verifies that
numeric pre-release identifiers inside interval bounds are ordered numerically instead of
lexicographically. This scenario is tested by
`VersionInterval_Contains_PreReleaseOrdering_UsesNumericComparison`.

**VersionInterval_Contains_VersionComparable_HandlesPreRelease**: This scenario verifies that a
`VersionComparable` argument with pre-release identifiers is evaluated using semantic-version
precedence rules. This scenario is tested by
`VersionInterval_Contains_VersionComparable_HandlesPreRelease`.
