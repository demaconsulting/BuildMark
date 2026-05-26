### VersionTag

#### Verification Approach

`VersionTag` is a pure logic unit with no external dependencies, so no mocks or stubs are needed.
The unit is exercised through `VersionTagTests.cs`, which contains 19 unit tests covering standard
and prefixed tags, path-prefix tags, multi-level path prefixes, normalization of dot-separated
pre-release suffixes to semantic-version form, invalid input handling, and comparison through
`Semantic.Comparable`.

#### Test Environment

N/A - standard test environment. No external dependencies or environment setup required.

#### Acceptance Criteria

- All 19 tests in `VersionTagTests.cs` pass with zero failures.

#### Test Scenarios

**VersionTag_Create_ValidTag_ReturnsVersionTag**: This scenario verifies that a valid tag string
produces a non-null `VersionTag` instance so downstream code can parse repository tags safely. This
scenario is tested by `VersionTag_Create_ValidTag_ReturnsVersionTag`.

**VersionTag_Create_StandardTag_ParsesCorrectly**: This scenario verifies that a plain tag such as
`1.2.3` is parsed without normalization changes and exposes the expected original and semantic
values. This scenario is tested by `VersionTag_Create_StandardTag_ParsesCorrectly`.

**VersionTag_Create_PrefixedTag_ParsesCorrectly**: This scenario verifies that a common `v` prefix
is ignored for semantic parsing while the original tag text is preserved. This scenario is tested
by `VersionTag_Create_PrefixedTag_ParsesCorrectly`.

**VersionTag_Create_DotSeparatedPreRelease_NormalizesToHyphen**: This scenario verifies that a
pre-release suffix written with dots in the tag is normalized to semantic-version hyphen form for
comparison and display. This scenario is tested by
`VersionTag_Create_DotSeparatedPreRelease_NormalizesToHyphen`.

**VersionTag_Create_ComplexTag_ExtractsVersionCorrectly**: This scenario verifies that a tag with
an arbitrary prefix, pre-release suffix, and build metadata is reduced to the expected semantic
version fields. This scenario is tested by
`VersionTag_Create_ComplexTag_ExtractsVersionCorrectly`.

**VersionTag_Properties_ExposeOriginalAndParsed_Correctly**: This scenario verifies that the unit
retains the original tag while also exposing parsed semantic version properties for a pre-release
value. This scenario is tested by
`VersionTag_Properties_ExposeOriginalAndParsed_Correctly`.

**VersionTag_ToString_ReturnsOriginalTag**: This scenario verifies that `ToString` returns the
original tag text unchanged so the source repository tag can be displayed exactly as received. This
scenario is tested by `VersionTag_ToString_ReturnsOriginalTag`.

**VersionTag_Create_SimpleVPrefix_ParsesVersion**: This scenario verifies that a simple `v`-prefix
release tag populates all parsed properties consistently and reports that it is not a pre-release.
This scenario is tested by `VersionTag_Create_SimpleVPrefix_ParsesVersion`.

**VersionTag_Create_ComplexVersionWithMetadata_ParsesVersion**: This scenario verifies that a tag
with a custom prefix, dot-separated pre-release, and metadata populates all semantic properties
correctly after normalization. This scenario is tested by
`VersionTag_Create_ComplexVersionWithMetadata_ParsesVersion`.

**VersionTag_TryCreate_InvalidTag_ReturnsNull**: This scenario verifies that `TryCreate` safely
returns null for an invalid tag string rather than throwing. This scenario is tested by
`VersionTag_TryCreate_InvalidTag_ReturnsNull`.

**VersionTag_Create_InvalidTag_ThrowsArgumentException**: This scenario verifies that `Create`
rejects an invalid tag with an `ArgumentException` so required parsing failures are explicit. This
scenario is tested by `VersionTag_Create_InvalidTag_ThrowsArgumentException`.

**VersionTag_Create_NoPrefix_ParsesVersion**: This scenario verifies that a release tag with no
prefix is parsed directly and is not marked as a pre-release. This scenario is tested by
`VersionTag_Create_NoPrefix_ParsesVersion`.

**VersionTag_Create_HyphenPreReleaseWithMetadata_ParsesVersion**: This scenario verifies that a
hyphenated pre-release tag with build metadata is parsed without dot-normalization changes and
reports pre-release state correctly. This scenario is tested by
`VersionTag_Create_HyphenPreReleaseWithMetadata_ParsesVersion`.

**VersionTag_Semantic_AllowsComparison**: This scenario verifies that two parsed tags can be
compared numerically through `Semantic.Comparable`, so version precedence does not depend on the
original text prefix. This scenario is tested by `VersionTag_Semantic_AllowsComparison`.

**VersionComparable_Equals_DifferentPrefixesSameVersion_ReturnsTrue**: This scenario verifies that
multiple tags with different textual prefixes normalize to equal comparable semantic versions when
the underlying version is the same. This scenario is tested by
`VersionComparable_Equals_DifferentPrefixesSameVersion_ReturnsTrue`.

**VersionTag_GetVersionComparable_SemanticTags_ReturnsCorrectComparison**: This scenario verifies
that alpha, beta, and release tags compare in semantic-version precedence order through
`Semantic.Comparable`. This scenario is tested by
`VersionTag_GetVersionComparable_SemanticTags_ReturnsCorrectComparison`.

**VersionTag_Create_PathSeparatorPrefix_ParsesCorrectly**: This scenario verifies that a tag with
a path-style prefix such as `release/1.2.3` still yields the expected semantic version values.
This scenario is tested by `VersionTag_Create_PathSeparatorPrefix_ParsesCorrectly`.

**VersionTag_Create_PathSeparatorPrefixWithPreRelease_ParsesCorrectly**: This scenario verifies
that a path-style prefix combined with a pre-release suffix is parsed correctly and identified as a
pre-release version. This scenario is tested by
`VersionTag_Create_PathSeparatorPrefixWithPreRelease_ParsesCorrectly`.

**VersionTag_Create_MultiLevelPathPrefix_ParsesCorrectly**: This scenario verifies that a
multi-level path prefix does not interfere with extracting semantic numbers, pre-release content,
and metadata from the tag. This scenario is tested by
`VersionTag_Create_MultiLevelPathPrefix_ParsesCorrectly`.
