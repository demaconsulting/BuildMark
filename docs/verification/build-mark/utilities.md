## Utilities

### Verification Strategy

The Utilities subsystem is verified through `PathHelpersTests.cs` (7 unit tests for
`PathHelpers`) and through `RepoConnectorsTests.cs` (for `ProcessRunner`). There is no
dedicated `UtilitiesTests.cs` subsystem file; unit coverage is provided by the
dedicated class-level test files described above.

### Dependencies

| Mock / Stub   | Reason                                                                |
| ------------- | --------------------------------------------------------------------- |
| None required | `ProcessRunner` tests use real processes; `PathHelpers` is pure logic |

### Test Environment

`ProcessRunner` tests invoke real OS commands (e.g., `git --version`) and therefore
require a working shell and a `git` executable on the host. Tests run on Windows,
Ubuntu, and macOS in the CI matrix. `PathHelpers` tests have no external dependencies.

### Acceptance Criteria

All `ProcessRunner` integration tests in `RepoConnectorsTests.cs` pass with zero
failures on all supported operating systems. All `BuildMark-Utilities-*` requirements
have at least one test in the Requirements Coverage mapping.

### Test Scenarios (Integration)

The following integration tests in `RepoConnectorsTests.cs` exercise `ProcessRunner`:

#### RepoConnectors_ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput

**Scenario**: `ProcessRunner.TryRunAsync` is called with a valid system command.

**Expected**: Returns non-null output string from the process.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

#### RepoConnectors_ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull

**Scenario**: `ProcessRunner.TryRunAsync` is called with an invalid/nonexistent command.

**Expected**: Returns `null` rather than throwing.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

#### RepoConnectors_ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull

**Scenario**: `ProcessRunner.TryRunAsync` is called with a command that exits non-zero.

**Expected**: Returns `null` due to non-zero exit code.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

#### RepoConnectors_ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput

**Scenario**: `ProcessRunner.RunAsync` is called with a valid command.

**Expected**: Returns process output string.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

#### RepoConnectors_ProcessRunner_RunAsync_WithFailingCommand_ThrowsException

**Scenario**: `ProcessRunner.RunAsync` is called with a command that fails.

**Expected**: Throws an exception.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### Requirements Coverage

- **BuildMark-Utilities-ProcessRunner**:
  RepoConnectors_ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput,
  RepoConnectors_ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull,
  RepoConnectors_ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull,
  RepoConnectors_ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput,
  RepoConnectors_ProcessRunner_RunAsync_WithFailingCommand_ThrowsException
- **BuildMark-Utilities-PathHelpers**:
  PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly,
  PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException,
  PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException,
  PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException,
  PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException,
  PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException,
  PathHelpers_SafePathCombine_PathStartingWithDots_CombinesCorrectly
