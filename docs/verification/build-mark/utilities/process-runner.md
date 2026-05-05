### ProcessRunner

#### Verification Approach

`ProcessRunner` is tested through `RepoConnectorsTests.cs`. The five ProcessRunner
tests exercise `TryRunAsync` (which returns `null` on failure) and `RunAsync` (which
throws on failure) using real OS processes to confirm the process execution logic is
correct on the target operating system.

#### Dependencies

| Mock / Stub | Reason                                                      |
| ----------- | ----------------------------------------------------------- |
| None        | Real OS processes are used to test actual process execution |

#### Test Scenarios

##### RepoConnectors_ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput

**Scenario**: `ProcessRunner.TryRunAsync` is called with a valid system command.

**Expected**: Returns a non-null output string.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

##### RepoConnectors_ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull

**Scenario**: `ProcessRunner.TryRunAsync` is called with an invalid command name.

**Expected**: Returns `null` without throwing.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

##### RepoConnectors_ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull

**Scenario**: `ProcessRunner.TryRunAsync` is called with a command that exits with
a non-zero code.

**Expected**: Returns `null`.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

##### RepoConnectors_ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput

**Scenario**: `ProcessRunner.RunAsync` is called with a valid command.

**Expected**: Returns the process standard output as a string.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

##### RepoConnectors_ProcessRunner_RunAsync_WithFailingCommand_ThrowsException

**Scenario**: `ProcessRunner.RunAsync` is called with a command that returns a
non-zero exit code.

**Expected**: An exception is thrown.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

#### Requirements Coverage

- **BuildMark-Utilities-ProcessRunner**:
  RepoConnectors_ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput,
  RepoConnectors_ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull,
  RepoConnectors_ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull,
  RepoConnectors_ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput,
  RepoConnectors_ProcessRunner_RunAsync_WithFailingCommand_ThrowsException
