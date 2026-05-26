### VersionSemantic

#### Verification Approach

`VersionSemantic` is a pure logic unit with no external dependencies, so no mocks or stubs are
needed. The unit is exercised through `VersionSemanticTests.cs`, which contains 12 unit tests
covering creation with and without build metadata, delegation of comparison properties to the
underlying `VersionComparable`, formatting of the full version string, and comparison through the
`Comparable` property.

#### Test Environment

N/A - standard test environment.

#### Acceptance Criteria

- All 12 tests in `VersionSemanticTests.cs` pass with zero failures.

#### Test Scenarios

**VersionSemantic_Create_WithBuildMetadata_ReturnsInstance**: This scenario verifies that creating
`VersionSemantic` from `1.2.3+build.123` preserves the build metadata and full version string so
metadata is exposed without affecting comparison data. This scenario is tested by
`VersionSemantic_Create_WithBuildMetadata_ReturnsInstance`.

**VersionSemantic_Create_WithoutBuildMetadata_ReturnsInstance**: This scenario verifies that
creating `VersionSemantic` from `1.2.3` leaves metadata unset and preserves the original full
version string for a plain release version. This scenario is tested by
`VersionSemantic_Create_WithoutBuildMetadata_ReturnsInstance`.

**VersionSemantic_Properties_DelegateToComparable_Correctly**: This scenario verifies that major,
minor, patch, pre-release, and compare-version values are delegated correctly from the underlying
`VersionComparable`. This scenario is tested by
`VersionSemantic_Properties_DelegateToComparable_Correctly`.

**VersionSemantic_ToString_FormatsCompletely_WithAllComponents**: This scenario verifies that a
version containing both pre-release and build metadata is formatted into the expected full version
string. This scenario is tested by
`VersionSemantic_ToString_FormatsCompletely_WithAllComponents`.

**VersionSemantic_PreRelease_ReturnsEmptyStringForRelease**: This scenario verifies that a release
version reports an empty pre-release value so callers do not need null handling for normal
releases. This scenario is tested by
`VersionSemantic_PreRelease_ReturnsEmptyStringForRelease`.

**VersionSemantic_Parse_ValidSemanticVersions_ParsesCorrectly**: This scenario verifies that a
representative set of valid semantic version strings parses successfully and exposes the expected
data. This scenario is tested by
`VersionSemantic_Parse_ValidSemanticVersions_ParsesCorrectly`.

**VersionSemantic_Create_SimpleVersion_ParsesVersion**: This scenario verifies that a simple
release version populates numeric, compare, and display properties consistently and reports that it
is not a pre-release. This scenario is tested by
`VersionSemantic_Create_SimpleVersion_ParsesVersion`.

**VersionSemantic_Create_VersionWithMetadata_ParsesVersion**: This scenario verifies that a
version with build metadata preserves the metadata in `FullVersion` while keeping
`CompareVersion` limited to comparable semantic fields. This scenario is tested by
`VersionSemantic_Create_VersionWithMetadata_ParsesVersion`.

**VersionSemantic_Create_PreReleaseWithMetadata_ParsesVersion**: This scenario verifies that a
version containing both pre-release and metadata components populates all properties correctly and
reports `IsPreRelease` as true. This scenario is tested by
`VersionSemantic_Create_PreReleaseWithMetadata_ParsesVersion`.

**VersionSemantic_TryCreate_InvalidVersion_ReturnsNull**: This scenario verifies that `TryCreate`
returns null for an invalid semantic version string rather than throwing, which supports safe input
probing. This scenario is tested by `VersionSemantic_TryCreate_InvalidVersion_ReturnsNull`.

**VersionSemantic_Create_InvalidVersion_ThrowsArgumentException**: This scenario verifies that
`Create` rejects an invalid semantic version string with an `ArgumentException` that explains the
format problem. This scenario is tested by
`VersionSemantic_Create_InvalidVersion_ThrowsArgumentException`.

**VersionSemantic_Comparable_AllowsComparison**: This scenario verifies that the `Comparable`
property enables ordering between semantic versions while ignoring build metadata in precedence.
This scenario is tested by `VersionSemantic_Comparable_AllowsComparison`.
