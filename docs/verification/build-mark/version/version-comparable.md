### VersionComparable

#### Verification Approach

`VersionComparable` is a pure logic unit with no external dependencies, so no mocks or stubs are
needed. The unit is exercised through `VersionComparableTests.cs`, which contains 26 unit tests
covering creation through `Create` and `TryCreate`, invalid, null, and empty input handling, the
four comparison operators, and Semantic Versioning pre-release ordering rules.

#### Test Environment

N/A - standard test environment. No external dependencies or environment setup required.

#### Acceptance Criteria

- All 26 tests in `VersionComparableTests.cs` pass with zero failures.

#### Test Scenarios

**VersionComparable_Create_ValidVersion_ReturnsInstance**: This scenario verifies that creating a
comparable version from a valid version string succeeds and returns a non-null instance. This
scenario is tested by `VersionComparable_Create_ValidVersion_ReturnsInstance`.

**VersionComparable_Create_SimpleVersion_ParsesVersion**: This scenario verifies that a simple
`major.minor.patch` version string is parsed into the expected numeric properties so later
comparisons use the correct values. This scenario is tested by
`VersionComparable_Create_SimpleVersion_ParsesVersion`.

**VersionComparable_Create_PreReleaseVersion_ParsesVersion**: This scenario verifies that a
pre-release version string is parsed correctly so pre-release identifiers participate in ordering as
intended. This scenario is tested by `VersionComparable_Create_PreReleaseVersion_ParsesVersion`.

**VersionComparable_TryCreate_InvalidVersion_ReturnsNull**: This scenario verifies that
`TryCreate` rejects an invalid version string without throwing and reports the failure with a null
result. This scenario is tested by `VersionComparable_TryCreate_InvalidVersion_ReturnsNull`.

**VersionComparable_TryCreate_NullInput_ReturnsNull**: This scenario verifies that `TryCreate`
handles a null input safely and returns null instead of creating an invalid instance. This scenario
is tested by `VersionComparable_TryCreate_NullInput_ReturnsNull`.

**VersionComparable_TryCreate_EmptyInput_ReturnsNull**: This scenario verifies that `TryCreate`
rejects an empty version string and returns null so empty input does not enter comparison logic.
This scenario is tested by `VersionComparable_TryCreate_EmptyInput_ReturnsNull`.

**VersionComparable_Create_InvalidVersion_ThrowsArgumentException**: This scenario verifies that
`Create` fails fast for an invalid version string by throwing `ArgumentException`, which protects
callers that require a valid instance. This scenario is tested by
`VersionComparable_Create_InvalidVersion_ThrowsArgumentException`.

**VersionComparable_CompareTo_SameMajorMinorPatch_ReturnsZero**: This scenario verifies that two
versions with the same major, minor, and patch values compare as equal so equality checks remain
stable. This scenario is tested by
`VersionComparable_CompareTo_SameMajorMinorPatch_ReturnsZero`.

**VersionComparable_CompareTo_DifferentMajor_ReturnsCorrectOrder**: This scenario verifies that a
higher major version compares as greater, which is the highest-priority numeric ordering rule.
This scenario is tested by `VersionComparable_CompareTo_DifferentMajor_ReturnsCorrectOrder`.

**VersionComparable_CompareTo_DifferentMinor_ReturnsCorrectOrder**: This scenario verifies that a
higher minor version compares as greater when the major version matches, preserving expected semver
ordering. This scenario is tested by `VersionComparable_CompareTo_DifferentMinor_ReturnsCorrectOrder`.

**VersionComparable_CompareTo_DifferentPatch_ReturnsCorrectOrder**: This scenario verifies that a
higher patch version compares as greater when major and minor values match. This scenario is tested
by `VersionComparable_CompareTo_DifferentPatch_ReturnsCorrectOrder`.

**VersionComparable_CompareTo_PreReleaseVsRelease_ReturnsCorrectOrder**: This scenario verifies
that a pre-release version sorts before the corresponding release version, which is a core Semantic
Versioning rule. This scenario is tested by
`VersionComparable_CompareTo_PreReleaseVsRelease_ReturnsCorrectOrder`.

**VersionComparable_CompareTo_PreReleaseVersions_ReturnsLexicographicOrder**: This scenario
verifies that non-numeric pre-release identifiers are compared lexicographically so alphabetic
labels sort correctly. This scenario is tested by
`VersionComparable_CompareTo_PreReleaseVersions_ReturnsLexicographicOrder`.

**VersionComparable_Operators_LessThan_WorksCorrectly**: This scenario verifies that the `<`
operator returns the expected boolean result so callers can use natural relational syntax. This
scenario is tested by `VersionComparable_Operators_LessThan_WorksCorrectly`.

**VersionComparable_Operators_GreaterThan_WorksCorrectly**: This scenario verifies that the `>`
operator returns the expected boolean result for comparable versions. This scenario is tested by
`VersionComparable_Operators_GreaterThan_WorksCorrectly`.

**VersionComparable_Operators_LessThanOrEqual_WorksCorrectly**: This scenario verifies that the
`<=` operator returns the expected boolean result for equal and lower versions. This scenario is
tested by `VersionComparable_Operators_LessThanOrEqual_WorksCorrectly`.

**VersionComparable_Operators_GreaterThanOrEqual_WorksCorrectly**: This scenario verifies that the
`>=` operator returns the expected boolean result for equal and greater versions. This scenario is
tested by `VersionComparable_Operators_GreaterThanOrEqual_WorksCorrectly`.

**VersionComparable_CompareTo_SemanticVersions_ReturnsCorrectOrder**: This scenario verifies that
an ordered series of semantic versions follows the precedence rules defined by the specification.
This scenario is tested by `VersionComparable_CompareTo_SemanticVersions_ReturnsCorrectOrder`.

**VersionComparable_CompareTo_NumericComparison_CorrectOrdering**: This scenario verifies that
numeric pre-release identifiers are compared numerically rather than lexicographically, so `11`
sorts after `9`. This scenario is tested by
`VersionComparable_CompareTo_NumericComparison_CorrectOrdering`.

**VersionComparable_CompareTo_ReleaseGreaterThanPreRelease_CorrectOrdering**: This scenario
verifies the inverse release-versus-pre-release comparison and confirms the release version has
higher precedence. This scenario is tested by
`VersionComparable_CompareTo_ReleaseGreaterThanPreRelease_CorrectOrdering`.

**VersionComparable_CompareTo_PreReleaseLexicographic_CorrectOrdering**: This scenario verifies
that alphabetic pre-release identifiers use lexicographic ordering when compared to one another.
This scenario is tested by `VersionComparable_CompareTo_PreReleaseLexicographic_CorrectOrdering`.

**VersionComparable_CompareTo_PreReleaseNumeric_ComparesNumerically**: This scenario verifies that
purely numeric pre-release identifiers use integer comparison so numeric precedence is preserved.
This scenario is tested by `VersionComparable_CompareTo_PreReleaseNumeric_ComparesNumerically`.

**VersionComparable_CompareTo_PreReleaseSemVerRules_CorrectOrdering**: This scenario verifies that
full pre-release precedence matches Semantic Versioning 2.0.0 section 11 for representative
examples. This scenario is tested by
`VersionComparable_CompareTo_PreReleaseSemVerRules_CorrectOrdering`.

**VersionComparable_CompareTo_NumericVsNonNumeric_NumericIsLess**: This scenario verifies that a
numeric pre-release identifier has lower precedence than a non-numeric identifier at the same
position. This scenario is tested by
`VersionComparable_CompareTo_NumericVsNonNumeric_NumericIsLess`.

**VersionComparable_CompareTo_ShorterPreRelease_IsLess**: This scenario verifies that when common
pre-release identifiers are equal, the shorter identifier list has lower precedence. This scenario
is tested by `VersionComparable_CompareTo_ShorterPreRelease_IsLess`.

**VersionComparable_CompareTo_ComplexPreRelease_CorrectOrdering**: This scenario verifies that
multi-part pre-release identifiers are compared field by field so complex semantic versions sort
correctly. This scenario is tested by
`VersionComparable_CompareTo_ComplexPreRelease_CorrectOrdering`.
