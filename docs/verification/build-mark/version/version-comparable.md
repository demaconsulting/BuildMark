### VersionComparable

#### Verification Approach

`VersionComparable` is tested through `VersionComparableTests.cs`, which contains
26 unit tests. The tests cover creation (valid, invalid, null, empty), comparison
operators, and numeric-vs-lexicographic pre-release ordering rules that follow the
Semantic Versioning specification.

#### Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

#### Test Scenarios

##### VersionComparable_Create_ValidVersion_ReturnsInstance

**Scenario**: `VersionComparable.Create` is called with a valid version string.

**Expected**: Returns a non-null instance.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_Create_SimpleVersion_ParsesVersion

**Scenario**: `VersionComparable.Create` is called with a simple `major.minor.patch` string.

**Expected**: Major, minor, and patch properties reflect parsed values.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_Create_PreReleaseVersion_ParsesVersion

**Scenario**: `VersionComparable.Create` is called with a pre-release version string.

**Expected**: Pre-release identifiers are parsed correctly.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_TryCreate_InvalidVersion_ReturnsNull

**Scenario**: `VersionComparable.TryCreate` is called with an invalid version string.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_TryCreate_NullInput_ReturnsNull

**Scenario**: `VersionComparable.TryCreate` is called with a `null` argument.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_TryCreate_EmptyInput_ReturnsNull

**Scenario**: `VersionComparable.TryCreate` is called with an empty string.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_Create_InvalidVersion_ThrowsArgumentException

**Scenario**: `VersionComparable.Create` is called with an invalid version string.

**Expected**: `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_SameMajorMinorPatch_ReturnsZero

**Scenario**: Two instances with identical major.minor.patch are compared.

**Expected**: `CompareTo` returns 0.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_DifferentMajor_ReturnsCorrectOrder

**Scenario**: Two instances with different major versions are compared.

**Expected**: Higher major version compares as greater.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_DifferentMinor_ReturnsCorrectOrder

**Scenario**: Two instances with different minor versions are compared.

**Expected**: Higher minor version compares as greater.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_DifferentPatch_ReturnsCorrectOrder

**Scenario**: Two instances with different patch versions are compared.

**Expected**: Higher patch version compares as greater.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_PreReleaseVsRelease_ReturnsCorrectOrder

**Scenario**: A pre-release version is compared to its release counterpart.

**Expected**: Pre-release is less than release per semver rules.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_PreReleaseVersions_ReturnsLexicographicOrder

**Scenario**: Two pre-release versions with non-numeric identifiers are compared.

**Expected**: Comparison follows lexicographic order.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_Operators_LessThan_WorksCorrectly

**Scenario**: The `<` operator is applied to two version instances.

**Expected**: Returns the correct boolean result.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_Operators_GreaterThan_WorksCorrectly

**Scenario**: The `>` operator is applied to two version instances.

**Expected**: Returns the correct boolean result.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_Operators_LessThanOrEqual_WorksCorrectly

**Scenario**: The `<=` operator is applied to two version instances.

**Expected**: Returns the correct boolean result.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_Operators_GreaterThanOrEqual_WorksCorrectly

**Scenario**: The `>=` operator is applied to two version instances.

**Expected**: Returns the correct boolean result.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_SemanticVersions_ReturnsCorrectOrder

**Scenario**: A series of semver-compliant versions is compared.

**Expected**: Ordering matches the semver specification.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_NumericComparison_CorrectOrdering

**Scenario**: Numeric pre-release identifiers are compared.

**Expected**: Numeric comparison is used (11 > 9).

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_ReleaseGreaterThanPreRelease_CorrectOrdering

**Scenario**: Release version `1.0.0` is compared to `1.0.0-alpha`.

**Expected**: Release is greater than pre-release.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_PreReleaseLexicographic_CorrectOrdering

**Scenario**: Pre-release identifiers with alphabetic content are compared.

**Expected**: Lexicographic ordering is applied.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_PreReleaseNumeric_ComparesNumerically

**Scenario**: Pre-release identifiers that are purely numeric are compared.

**Expected**: Numeric comparison is used.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_PreReleaseSemVerRules_CorrectOrdering

**Scenario**: Pre-release versions are compared following all semver rules.

**Expected**: Ordering matches semver 2.0.0 specification section 11.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_NumericVsNonNumeric_NumericIsLess

**Scenario**: A numeric pre-release identifier is compared to a non-numeric one.

**Expected**: Numeric identifier has lower precedence.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_ShorterPreRelease_IsLess

**Scenario**: Two pre-release versions with different numbers of identifiers are compared.

**Expected**: Shorter pre-release is less than longer when all common fields are equal.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

##### VersionComparable_CompareTo_ComplexPreRelease_CorrectOrdering

**Scenario**: Complex multi-identifier pre-release versions are compared.

**Expected**: Correct ordering following semver field-by-field rules.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

#### Requirements Coverage

- **BuildMark-Version-VersionComparable**: All 26 tests in `VersionComparableTests.cs`
