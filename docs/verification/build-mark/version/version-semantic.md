# VersionSemantic

## Verification Approach

`VersionSemantic` is tested through `VersionSemanticTests.cs`, which contains 12 unit
tests. The tests cover creation with and without build metadata, property delegation
to the underlying `VersionComparable`, string formatting, and comparison.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

## Test Scenarios

### VersionSemantic_Create_WithBuildMetadata_ReturnsInstance

**Scenario**: `VersionSemantic.Create` is called with `"1.2.3+build.123"`.

**Expected**: `Metadata` equals `"build.123"`; `FullVersion` equals `"1.2.3+build.123"`.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_WithoutBuildMetadata_ReturnsInstance

**Scenario**: `VersionSemantic.Create` is called with `"1.2.3"`.

**Expected**: `Metadata` is `null`; `FullVersion` equals `"1.2.3"`.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Properties_DelegateToComparable_Correctly

**Scenario**: `VersionSemantic.Create` is called with `"1.2.3-alpha"` and the `Major`,
`Minor`, `Patch`, `PreRelease`, and `CompareVersion` properties are read.

**Expected**: `Major` equals 1; `Minor` equals 2; `Patch` equals 3; `PreRelease` equals
`"alpha"`; `CompareVersion` equals `"1.2.3-alpha"`.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_ToString_FormatsCompletely_WithAllComponents

**Scenario**: `VersionSemantic.Create` is called with `"1.2.3-alpha+build.123"` and
`FullVersion` is read.

**Expected**: `FullVersion` equals `"1.2.3-alpha+build.123"`.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_PreRelease_ReturnsEmptyStringForRelease

**Scenario**: `PreRelease` property is accessed on a release version (no `-` suffix).

**Expected**: Returns empty string.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Parse_ValidSemanticVersions_ParsesCorrectly

**Scenario**: A series of standard semver strings are parsed.

**Expected**: Each parses without error with correct field values.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_SimpleVersion_ParsesVersion

**Scenario**: `VersionSemantic.Create` is called with `"1.2.3"`.

**Expected**: `Major` equals 1; `Minor` equals 2; `Patch` equals 3; `Numbers` equals
`"1.2.3"`; `PreRelease` equals `""`; `Metadata` is `null`; `CompareVersion` equals
`"1.2.3"`; `FullVersion` equals `"1.2.3"`; `IsPreRelease` is false.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_VersionWithMetadata_ParsesVersion

**Scenario**: `VersionSemantic.Create` is called with `"1.2.3+build.5"`.

**Expected**: `Numbers` equals `"1.2.3"`; `PreRelease` equals `""`; `Metadata` equals
`"build.5"`; `CompareVersion` equals `"1.2.3"`; `FullVersion` equals `"1.2.3+build.5"`;
`IsPreRelease` is false.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_PreReleaseWithMetadata_ParsesVersion

**Scenario**: `VersionSemantic.Create` is called with `"2.0.0-alpha.1+linux.x64"`.

**Expected**: `Numbers` equals `"2.0.0"`; `PreRelease` equals `"alpha.1"`; `Metadata`
equals `"linux.x64"`; `CompareVersion` equals `"2.0.0-alpha.1"`; `FullVersion` equals
`"2.0.0-alpha.1+linux.x64"`; `IsPreRelease` is true.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_TryCreate_InvalidVersion_ReturnsNull

**Scenario**: `VersionSemantic.TryCreate` is called with `"not-a-version"`.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_InvalidVersion_ThrowsArgumentException

**Scenario**: `VersionSemantic.Create` is called with `"not-a-version"`.

**Expected**: `ArgumentException` is thrown with a message containing
`"does not match semantic version format"`.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Comparable_AllowsComparison

**Scenario**: `Comparable` is compared for `"1.2.3+build1"` and `"1.2.4+build2"`.

**Expected**: `version1.Comparable < version2.Comparable` is true (i.e., `"1.2.3"` sorts
before `"1.2.4"`).

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

## Requirements Coverage

- **BuildMark-Version-VersionSemantic**: All 12 tests in `VersionSemanticTests.cs`
