## Version

### Verification Approach

The Version subsystem is verified through `VersionTests.cs` with 8 integration tests. The tests
exercise all version type factory methods and constructors, semantic versioning precedence ordering,
and cross-type integration between `VersionTag` and `VersionComparable`. No mocks or stubs are
required — all version types are pure logic with no external dependencies.

### Test Environment

N/A - standard test environment. All tests in `VersionTests.cs` are pure logic tests with no
external dependencies, network access, or file system requirements.

### Acceptance Criteria

- All 8 tests in `VersionTests.cs` pass with zero failures.
- All `BuildMark-Version-*` requirements have at least one test in the ReqStream trace matrix.

### Test Scenarios

**VersionComparable_Create_ValidVersions_ReturnsVersionComparable**: Verifies that
`VersionComparable.Create` accepts valid version strings — a simple `major.minor.patch` version, a
pre-release version, and a complex pre-release — returning a non-null `VersionComparable` instance
for each input.
This scenario is tested by `VersionComparable_Create_ValidVersions_ReturnsVersionComparable`.

**VersionSemantic_Create_ValidSemanticVersion_ReturnsVersionSemantic**: Verifies that
`VersionSemantic.Create` accepts a simple version, a version with build metadata, and a complex
version combining pre-release and build metadata, returning a non-null `VersionSemantic` instance
for each input.
This scenario is tested by `VersionSemantic_Create_ValidSemanticVersion_ReturnsVersionSemantic`.

**VersionTag_Create_ValidTag_ReturnsVersionTag**: Verifies that `VersionTag.Create` correctly parses
a plain version string, a `v`-prefixed version, and a release-prefixed pre-release tag, returning a
non-null `VersionTag` instance for each input.
This scenario is tested by `VersionTag_Create_ValidTag_ReturnsVersionTag`.

**VersionInterval_Create_ValidInterval_ReturnsVersionInterval**: Verifies that `VersionInterval` can
be constructed with fully-inclusive bounds, fully-exclusive bounds, and mixed lower/upper bounds,
returning a non-null instance for each construction.
This scenario is tested by `VersionInterval_Create_ValidInterval_ReturnsVersionInterval`.

**VersionCommitTag_Constructor_ValidParameters_CreatesInstance**: Verifies that `VersionCommitTag`
constructed with a `VersionTag` and a commit hash string exposes the supplied values through its
`VersionTag` and `CommitHash` properties.
This scenario is tested by `VersionCommitTag_Constructor_ValidParameters_CreatesInstance`.

**Version_Subsystem_CreateAllVersionTypes_WorksCorrectly**: Verifies that all version type factory
methods and constructors — `VersionComparable.Create`, `VersionSemantic.Create`,
`VersionTag.Create`, `new VersionInterval`, and `new VersionCommitTag` — can be invoked in
sequence without error, and that the resulting instances expose the supplied values through their
properties.
This scenario is tested by `Version_Subsystem_CreateAllVersionTypes_WorksCorrectly`.

**Version_Subsystem_SemanticVersioningCompliance_WorksCorrectly**: Verifies that a sequence of
`VersionComparable` instances representing the SemVer 2.0.0 precedence chain
`1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.beta < 1.0.0-beta < 1.0.0-beta.2 < 1.0.0` produces a
negative value from `CompareTo` for each adjacent pair, confirming correct precedence ordering.
This scenario is tested by `Version_Subsystem_SemanticVersioningCompliance_WorksCorrectly`.

**Version_Subsystem_TagToComparableIntegration_WorksCorrectly**: Verifies that `VersionTag`
instances created from prefix forms `v1.2.3`, `VER2.0.0-rc.1`, and `release-1.5.0` each yield a
`VersionComparable` via `Semantic.Comparable`, and that the ordering
`1.2.3 < 1.5.0 < 2.0.0-rc.1` holds, confirming correct tag-to-comparable integration.
This scenario is tested by `Version_Subsystem_TagToComparableIntegration_WorksCorrectly`.
