### VersionCommitTag

#### Verification Approach

`VersionCommitTag` is a simple data record. It has no external dependencies and no
dependencies to mock or stub. It is verified through the subsystem integration test in
`VersionTests.cs` via `VersionCommitTag_Constructor_ValidParameters_CreatesInstance`,
which constructs an instance and asserts that the version-tag and commit-hash properties are
stored correctly. No dedicated unit test class exists for this unit.

#### Test Environment

N/A - standard test environment. No external dependencies or environment setup required.

#### Acceptance Criteria

- `VersionCommitTag_Constructor_ValidParameters_CreatesInstance` in `VersionTests.cs`
  passes with zero failures.

#### Test Scenarios

**VersionCommitTag_Constructor_ValidParameters_CreatesInstance**: `VersionCommitTag` is
constructed with a `VersionTag` and a commit hash string. The `VersionTag` and
`CommitHash` properties return the supplied values, confirming the data record stores its
constructor arguments unchanged. This scenario is tested by
`VersionCommitTag_Constructor_ValidParameters_CreatesInstance`.
