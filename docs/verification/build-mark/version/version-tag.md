# VersionTag

## Verification Approach

`VersionTag` is tested through `VersionTagTests.cs`, which contains 19 unit tests.
The tests cover parsing of standard tags, prefixed tags (e.g., `v1.2.3`), path-prefix
tags (e.g., `mylib/1.2.3`), pre-release normalization (dots converted to hyphens),
and error handling for invalid tags.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

## Test Scenarios

### VersionTag_Create_ValidTag_ReturnsVersionTag

**Scenario**: `VersionTag.Create` is called with a valid tag string.

**Expected**: Returns a non-null `VersionTag` instance.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_StandardTag_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with a standard `v1.2.3` tag.

**Expected**: Version components are parsed correctly.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_PrefixedTag_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with a prefixed tag like `v1.2.3`.

**Expected**: `v` prefix is stripped; version parses correctly.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_DotSeparatedPreRelease_NormalizesToHyphen

**Scenario**: `VersionTag.Create` is called with a tag using dots in the pre-release
segment.

**Expected**: Dots in the pre-release are normalized to hyphens.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_ComplexTag_ExtractsVersionCorrectly

**Scenario**: `VersionTag.Create` is called with a complex tag containing prefix,
version, pre-release, and build metadata.

**Expected**: All components are extracted correctly.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Properties_ExposeOriginalAndParsed_Correctly

**Scenario**: `Tag` and `Semantic` properties are accessed after creating a tag.

**Expected**: `Tag` returns the original string; `Semantic` returns the parsed version.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_ToString_ReturnsOriginalTag

**Scenario**: `ToString` is called on a `VersionTag` instance.

**Expected**: Returns the original tag string unchanged.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_SimpleVPrefix_ParsesVersion

**Scenario**: `VersionTag.Create` is called with `v1.0.0`.

**Expected**: Instance created with version `1.0.0`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_ComplexVersionWithMetadata_ParsesVersion

**Scenario**: `VersionTag.Create` is called with a tag including build metadata.

**Expected**: Metadata is preserved in the `Semantic` property.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_TryCreate_InvalidTag_ReturnsNull

**Scenario**: `VersionTag.TryCreate` is called with an unparseable tag string.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_InvalidTag_ThrowsArgumentException

**Scenario**: `VersionTag.Create` is called with an unparseable tag string.

**Expected**: `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_NoPrefix_ParsesVersion

**Scenario**: `VersionTag.Create` is called with a tag that has no alphabetic prefix.

**Expected**: Version is parsed directly from the string.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_HyphenPreReleaseWithMetadata_ParsesVersion

**Scenario**: `VersionTag.Create` is called with a tag using hyphen pre-release and
build metadata.

**Expected**: Both pre-release and metadata are parsed correctly.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Semantic_AllowsComparison

**Scenario**: `Semantic` property is used to compare two tags.

**Expected**: Comparison behaves correctly via the underlying `VersionSemantic`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionComparable_Equals_DifferentPrefixesSameVersion_ReturnsTrue

**Scenario**: Two tags with different prefixes but the same version are compared
via `VersionComparable`.

**Expected**: Comparable values are equal.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_GetVersionComparable_SemanticTags_ReturnsCorrectComparison

**Scenario**: `VersionComparable` extracted from two semantic tags is compared.

**Expected**: Returns the correct ordering.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_PathSeparatorPrefix_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with a tag in `prefix/1.2.3` format.

**Expected**: Path prefix is stripped; version is parsed correctly.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_PathSeparatorPrefixWithPreRelease_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with `prefix/1.2.3-alpha`.

**Expected**: Path prefix is stripped; pre-release is parsed correctly.

**Requirement coverage**: `BuildMark-Version-VersionTag`

### VersionTag_Create_MultiLevelPathPrefix_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with a multi-level path prefix like
`a/b/1.2.3`.

**Expected**: All prefix segments are stripped; version parses correctly.

**Requirement coverage**: `BuildMark-Version-VersionTag`

## Requirements Coverage

- **BuildMark-Version-VersionTag**: All 19 tests in `VersionTagTests.cs`
