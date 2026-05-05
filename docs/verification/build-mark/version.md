## Version

### Verification Approach

The Version subsystem is verified through `VersionTests.cs` (subsystem integration
tests), plus dedicated unit test files for each version class. The subsystem tests
exercise the interaction between version types - creating `VersionTag` instances,
extracting `VersionComparable`, and comparing via `VersionInterval`. The unit tests
are described in the individual unit chapters.

### Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

### Test Scenarios

#### VersionComparable_Create_ValidVersions_ReturnsVersionComparable

**Scenario**: `VersionComparable.Create` is called with a valid version string.

**Expected**: Returns a non-null `VersionComparable` instance.

**Requirement coverage**: `BuildMark-Version-VersionComparable`

#### VersionSemantic_Create_ValidSemanticVersion_ReturnsVersionSemantic

**Scenario**: `VersionSemantic.Create` is called with a valid semantic version string.

**Expected**: Returns a non-null `VersionSemantic` instance.

**Requirement coverage**: `BuildMark-Version-VersionSemantic`

#### VersionTag_Create_ValidTag_ReturnsVersionTag

**Scenario**: `VersionTag.Create` is called with a valid tag string.

**Expected**: Returns a non-null `VersionTag` instance.

**Requirement coverage**: `BuildMark-Version-VersionTag`

#### VersionInterval_Create_ValidInterval_ReturnsVersionInterval

**Scenario**: `VersionInterval.Parse` is called with a valid interval string.

**Expected**: Returns a non-null `VersionInterval` instance.

**Requirement coverage**: `BuildMark-Version-VersionInterval`

#### VersionCommitTag_Constructor_ValidParameters_CreatesInstance

**Scenario**: `VersionCommitTag` is constructed with a valid tag and commit hash.

**Expected**: Instance is created with the provided tag and commit hash properties set.

**Requirement coverage**: `BuildMark-Version-VersionCommitTag`

#### Version_Subsystem_CreateAllVersionTypes_WorksCorrectly

**Scenario**: All version type factory methods are invoked in sequence.

**Expected**: All instances are created without error.

**Requirement coverage**: `BuildMark-Version-VersionComparable`, `BuildMark-Version-VersionSemantic`,
`BuildMark-Version-VersionTag`

#### Version_Subsystem_SemanticVersioningCompliance_WorksCorrectly

**Scenario**: Version strings from the semver specification are parsed and compared.

**Expected**: Ordering follows the semver specification.

**Requirement coverage**: `BuildMark-Version-VersionComparable`, `BuildMark-Version-VersionSemantic`

#### Version_Subsystem_TagToComparableIntegration_WorksCorrectly

**Scenario**: A `VersionTag` is created and its comparable representation is extracted.

**Expected**: The `VersionComparable` extracted from the tag matches the expected version.

**Requirement coverage**: `BuildMark-Version-VersionTag`, `BuildMark-Version-VersionComparable`

### Requirements Coverage

- **BuildMark-Version-VersionComparable**: VersionComparable_Create_ValidVersions_ReturnsVersionComparable,
  Version_Subsystem_CreateAllVersionTypes_WorksCorrectly,
  Version_Subsystem_SemanticVersioningCompliance_WorksCorrectly,
  Version_Subsystem_TagToComparableIntegration_WorksCorrectly
- **BuildMark-Version-VersionSemantic**: VersionSemantic_Create_ValidSemanticVersion_ReturnsVersionSemantic,
  Version_Subsystem_CreateAllVersionTypes_WorksCorrectly,
  Version_Subsystem_SemanticVersioningCompliance_WorksCorrectly
- **BuildMark-Version-VersionTag**: VersionTag_Create_ValidTag_ReturnsVersionTag,
  Version_Subsystem_CreateAllVersionTypes_WorksCorrectly,
  Version_Subsystem_TagToComparableIntegration_WorksCorrectly
- **BuildMark-Version-VersionInterval**: VersionInterval_Create_ValidInterval_ReturnsVersionInterval
- **BuildMark-Version-VersionCommitTag**: VersionCommitTag_Constructor_ValidParameters_CreatesInstance
