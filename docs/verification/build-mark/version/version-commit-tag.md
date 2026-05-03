# VersionCommitTag

## Verification Approach

`VersionCommitTag` is a simple data class with no dedicated test class. It is verified
through `VersionTests.cs` via the test
`VersionCommitTag_Constructor_ValidParameters_CreatesInstance`, which constructs an
instance and asserts that the tag and commit-hash properties are stored correctly.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios

### VersionCommitTag_Constructor_ValidParameters_CreatesInstance

**Scenario**: `VersionCommitTag` is constructed with a `VersionTag` and a commit hash
string.

**Expected**: The `Tag` and `CommitHash` properties return the supplied values.

**Requirement coverage**: `BuildMark-Version-VersionCommitTag`

## Requirements Coverage

- **BuildMark-Version-VersionCommitTag**: VersionCommitTag_Constructor_ValidParameters_CreatesInstance
