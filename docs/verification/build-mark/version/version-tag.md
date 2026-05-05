### VersionTag

#### Verification Approach

`VersionTag` is tested through `VersionTagTests.cs`, which contains 19 unit tests.
The tests cover parsing of standard tags, prefixed tags (e.g., `v1.2.3`), path-prefix
tags (e.g., `mylib/1.2.3`), pre-release normalization (dots converted to hyphens),
and error handling for invalid tags.

#### Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

#### Test Scenarios

##### VersionTag_Create_ValidTag_ReturnsVersionTag

**Scenario**: `VersionTag.Create` is called with a valid tag string.

**Expected**: Returns a non-null `VersionTag` instance.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_StandardTag_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with `"1.2.3"` (no prefix).

**Expected**: `Tag` equals `"1.2.3"`; `FullVersion` equals `"1.2.3"`; `Numbers` equals `"1.2.3"`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_PrefixedTag_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with `"v1.2.3"`.

**Expected**: `Tag` equals `"v1.2.3"`; `FullVersion` equals `"1.2.3"`; `Numbers` equals `"1.2.3"`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_DotSeparatedPreRelease_NormalizesToHyphen

**Scenario**: `VersionTag.Create` is called with `"v1.2.3.alpha.1"`.

**Expected**: `Tag` equals `"v1.2.3.alpha.1"`; `FullVersion` equals `"1.2.3-alpha.1"`;
`PreRelease` equals `"alpha.1"`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_ComplexTag_ExtractsVersionCorrectly

**Scenario**: `VersionTag.Create` is called with `"Release_1.2.3.beta.5+build.123"`.

**Expected**: `Tag` equals `"Release_1.2.3.beta.5+build.123"`; `FullVersion` equals
`"1.2.3-beta.5+build.123"`; `Numbers` equals `"1.2.3"`; `PreRelease` equals `"beta.5"`;
`Metadata` equals `"build.123"`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Properties_ExposeOriginalAndParsed_Correctly

**Scenario**: `VersionTag.Create` is called with `"v1.2.3-alpha"` and the `Tag`,
`FullVersion`, `Numbers`, and `PreRelease` properties are read.

**Expected**: `Tag` equals `"v1.2.3-alpha"`; `FullVersion` equals `"1.2.3-alpha"`;
`Numbers` equals `"1.2.3"`; `PreRelease` equals `"alpha"`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_ToString_ReturnsOriginalTag

**Scenario**: `ToString` is called on a `VersionTag` created from `"v1.2.3-alpha+build.123"`.

**Expected**: Returns the original tag string unchanged.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_SimpleVPrefix_ParsesVersion

**Scenario**: `VersionTag.Create` is called with `"v1.2.3"`.

**Expected**: `Tag` equals `"v1.2.3"`; `FullVersion` equals `"1.2.3"`; `Numbers` equals
`"1.2.3"`; `PreRelease` equals `""`; `CompareVersion` equals `"1.2.3"`; `Metadata` equals
`""`; `IsPreRelease` is false.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_ComplexVersionWithMetadata_ParsesVersion

**Scenario**: `VersionTag.Create` is called with `"Rel_1.2.3.rc.4+build.5"`.

**Expected**: `Tag` equals `"Rel_1.2.3.rc.4+build.5"`; `FullVersion` equals
`"1.2.3-rc.4+build.5"`; `Numbers` equals `"1.2.3"`; `PreRelease` equals `"rc.4"`;
`CompareVersion` equals `"1.2.3-rc.4"`; `Metadata` equals `"build.5"`; `IsPreRelease`
is true.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_TryCreate_InvalidTag_ReturnsNull

**Scenario**: `VersionTag.TryCreate` is called with `"not-a-version"`.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_InvalidTag_ThrowsArgumentException

**Scenario**: `VersionTag.Create` is called with `"not-a-version"`.

**Expected**: `ArgumentException` is thrown with a message containing
`"does not match version format"`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_NoPrefix_ParsesVersion

**Scenario**: `VersionTag.Create` is called with `"1.0.0"`.

**Expected**: `Tag` equals `"1.0.0"`; `FullVersion` equals `"1.0.0"`; `IsPreRelease` is false.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_HyphenPreReleaseWithMetadata_ParsesVersion

**Scenario**: `VersionTag.Create` is called with `"Rel_1.2.3-rc.4+build.5"`.

**Expected**: `Tag` equals `"Rel_1.2.3-rc.4+build.5"`; `FullVersion` equals
`"1.2.3-rc.4+build.5"`; `Numbers` equals `"1.2.3"`; `PreRelease` equals `"rc.4"`;
`CompareVersion` equals `"1.2.3-rc.4"`; `Metadata` equals `"build.5"`; `IsPreRelease`
is true.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Semantic_AllowsComparison

**Scenario**: `Semantic.Comparable` is compared for tags `"v1.2.3"` and `"v1.11.2"`.

**Expected**: `tag1.Semantic.Comparable < tag2.Semantic.Comparable` is true (i.e.,
`"v1.2.3"` sorts before `"v1.11.2"`).

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionComparable_Equals_DifferentPrefixesSameVersion_ReturnsTrue

**Scenario**: `Semantic.Comparable` is extracted from `"v1.2.3"`, `"VER1.2.3"`,
`"Release_1.2.3"`, and `"release/1.2.3"` and the four values are compared for equality.

**Expected**: Comparable values are equal.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_GetVersionComparable_SemanticTags_ReturnsCorrectComparison

**Scenario**: `Semantic.Comparable` is compared for `"v1.0.0-alpha"`, `"v1.0.0-beta"`,
and `"v1.0.0"`.

**Expected**: alpha < beta < release: `"v1.0.0-alpha"` sorts before `"v1.0.0-beta"`,
which sorts before `"v1.0.0"`.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_PathSeparatorPrefix_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with `"release/1.2.3"`.

**Expected**: `Tag` equals `"release/1.2.3"`; `FullVersion` equals `"1.2.3"`; `Numbers`
equals `"1.2.3"`; `PreRelease` equals `""`; `IsPreRelease` is false.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_PathSeparatorPrefixWithPreRelease_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with `"release/1.2.3-rc.4"`.

**Expected**: `Tag` equals `"release/1.2.3-rc.4"`; `FullVersion` equals `"1.2.3-rc.4"`;
`Numbers` equals `"1.2.3"`; `PreRelease` equals `"rc.4"`; `CompareVersion` equals
`"1.2.3-rc.4"`; `IsPreRelease` is true.

**Requirement coverage**: `BuildMark-Version-VersionTag`

##### VersionTag_Create_MultiLevelPathPrefix_ParsesCorrectly

**Scenario**: `VersionTag.Create` is called with `"builds/release/1.2.3-beta.1+build.99"`.

**Expected**: `Tag` equals `"builds/release/1.2.3-beta.1+build.99"`; `FullVersion` equals
`"1.2.3-beta.1+build.99"`; `Numbers` equals `"1.2.3"`; `PreRelease` equals `"beta.1"`;
`Metadata` equals `"build.99"`; `IsPreRelease` is true.

**Requirement coverage**: `BuildMark-Version-VersionTag`

#### Requirements Coverage

- **BuildMark-Version-VersionTag**: All 19 tests in `VersionTagTests.cs`
