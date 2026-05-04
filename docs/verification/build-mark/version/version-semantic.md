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

**Scenario**: `VersionSemantic.Create` is called with a version string that includes
build metadata (the `+` suffix).

**Expected**: Returns a non-null instance with metadata set.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_WithoutBuildMetadata_ReturnsInstance

**Scenario**: `VersionSemantic.Create` is called with a version string that has no
build metadata.

**Expected**: Returns a non-null instance with metadata as empty string.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Properties_DelegateToComparable_Correctly

**Scenario**: Major, minor, patch, and pre-release properties are accessed on a
`VersionSemantic` instance.

**Expected**: Values match those of the underlying `VersionComparable`.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_ToString_FormatsCompletely_WithAllComponents

**Scenario**: `ToString` is called on a `VersionSemantic` with pre-release and metadata.

**Expected**: Returns a string in `major.minor.patch-pre+meta` format.

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

**Scenario**: `VersionSemantic.Create` is called with a simple `major.minor.patch`.

**Expected**: Instance created with zero pre-release and metadata.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_VersionWithMetadata_ParsesVersion

**Scenario**: `VersionSemantic.Create` is called with version plus build metadata.

**Expected**: Metadata property returns the metadata string.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_PreReleaseWithMetadata_ParsesVersion

**Scenario**: `VersionSemantic.Create` is called with pre-release and metadata.

**Expected**: Both pre-release and metadata properties are set correctly.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_TryCreate_InvalidVersion_ReturnsNull

**Scenario**: `VersionSemantic.TryCreate` is called with an invalid string.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Create_InvalidVersion_ThrowsArgumentException

**Scenario**: `VersionSemantic.Create` is called with an invalid string.

**Expected**: `ArgumentException` is thrown.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

### VersionSemantic_Comparable_AllowsComparison

**Scenario**: Two `VersionSemantic` instances are compared using comparison operators.

**Expected**: Comparison delegates to the underlying `VersionComparable` correctly.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

## Requirements Coverage

- **BuildMark-Version-VersionSemantic**: All 12 tests in `VersionSemanticTests.cs`
