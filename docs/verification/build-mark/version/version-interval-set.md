### VersionIntervalSet

#### Verification Approach

`VersionIntervalSet` is a pure logic unit with no external dependencies, so no mocks or stubs are
needed. The unit is exercised through `VersionIntervalSetTests.cs`, which contains 13 unit tests
covering parsing of single and multiple intervals, handling of internal commas within interval
expressions, empty input, silent discarding of invalid tokens, and `Contains` checks for strings,
`VersionTag`, pre-release versions, and `VersionComparable` values.

#### Test Environment

N/A - standard test environment. No external dependencies or environment setup required.

#### Acceptance Criteria

- All 13 tests in `VersionIntervalSetTests.cs` pass with zero failures.

#### Test Scenarios

**VersionIntervalSet_Parse_SingleInterval_ReturnsOneInterval**: This scenario verifies that parsing
one interval string yields a set containing exactly one interval so simple range specifications are
stored correctly. This scenario is tested by
`VersionIntervalSet_Parse_SingleInterval_ReturnsOneInterval`.

**VersionIntervalSet_Parse_TwoIntervals_ReturnsTwoIntervals**: This scenario verifies that two
comma-separated interval expressions are both retained in the parsed set so disjoint ranges can be
represented together. This scenario is tested by
`VersionIntervalSet_Parse_TwoIntervals_ReturnsTwoIntervals`.

**VersionIntervalSet_Parse_IntervalsWithInternalComma_ParsedCorrectly**: This scenario verifies
that commas inside interval bounds are treated as part of the interval syntax and do not split the
set incorrectly. This scenario is tested by
`VersionIntervalSet_Parse_IntervalsWithInternalComma_ParsedCorrectly`.

**VersionIntervalSet_Parse_EmptyString_ReturnsEmptySet**: This scenario verifies that an empty
input string produces an empty interval set rather than an invalid interval entry. This scenario is
tested by `VersionIntervalSet_Parse_EmptyString_ReturnsEmptySet`.

**VersionIntervalSet_Parse_InvalidToken_DiscardedSilently**: This scenario verifies that invalid
interval tokens are ignored while valid intervals are preserved, which allows tolerant parsing of
mixed input. This scenario is tested by
`VersionIntervalSet_Parse_InvalidToken_DiscardedSilently`.

**VersionIntervalSet_Contains_StringInsideFirstInterval_ReturnsTrue**: This scenario verifies that
`Contains` returns true when a string version falls inside the first interval in the set. This
scenario is tested by `VersionIntervalSet_Contains_StringInsideFirstInterval_ReturnsTrue`.

**VersionIntervalSet_Contains_StringInsideLaterInterval_ReturnsTrue**: This scenario verifies that
`Contains` returns true when a string version falls inside a later interval, proving the full set
is evaluated rather than only the first entry. This scenario is tested by
`VersionIntervalSet_Contains_StringInsideLaterInterval_ReturnsTrue`.

**VersionIntervalSet_Contains_StringOutsideAllIntervals_ReturnsFalse**: This scenario verifies
that `Contains` returns false when a version is outside every interval in the set. This scenario is
tested by `VersionIntervalSet_Contains_StringOutsideAllIntervals_ReturnsFalse`.

**VersionIntervalSet_Contains_EmptySet_ReturnsFalse**: This scenario verifies that an empty set
contains no versions and therefore always returns false. This scenario is tested by
`VersionIntervalSet_Contains_EmptySet_ReturnsFalse`.

**VersionIntervalSet_Contains_VersionTag_DelegatesToSemanticVersion**: This scenario verifies that
a `VersionTag` argument is evaluated through its semantic version representation instead of raw tag
text. This scenario is tested by
`VersionIntervalSet_Contains_VersionTag_DelegatesToSemanticVersion`.

**VersionIntervalSet_Contains_PreReleaseVersions_HandlesCorrectly**: This scenario verifies that
pre-release version strings are evaluated using semantic-version ordering rules across the interval
set. This scenario is tested by
`VersionIntervalSet_Contains_PreReleaseVersions_HandlesCorrectly`.

**VersionIntervalSet_Contains_VersionComparable_HandlesPreRelease**: This scenario verifies that a
`VersionComparable` argument with pre-release identifiers is checked using semantic-version
precedence rules. This scenario is tested by
`VersionIntervalSet_Contains_VersionComparable_HandlesPreRelease`.

**VersionIntervalSet_Parse_PreReleaseBounds_ParsesCorrectly**: This scenario verifies that
intervals using pre-release version bounds are parsed and stored correctly so later membership
checks use the intended semantic range. This scenario is tested by
`VersionIntervalSet_Parse_PreReleaseBounds_ParsesCorrectly`.
