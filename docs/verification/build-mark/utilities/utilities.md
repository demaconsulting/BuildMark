# Utilities

## Verification Approach

The Utilities subsystem is verified through `RepoConnectorsTests.cs` (for
`ProcessRunner`) and indirectly through `CliTests.cs` (for `PathHelpers` via
path-related flag handling). There is no dedicated `UtilitiesTests.cs` file;
coverage is provided by integration-level tests that exercise the utility classes
as they are used by other units.

## Dependencies

| Mock / Stub   | Reason                                                                |
| ------------- | --------------------------------------------------------------------- |
| None required | `ProcessRunner` tests use real processes; `PathHelpers` is pure logic |

## Test Scenarios (Integration)

The following integration tests in `RepoConnectorsTests.cs` exercise `ProcessRunner`:

### RepoConnectors_ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput

**Scenario**: `ProcessRunner.TryRunAsync` is called with a valid system command.

**Expected**: Returns non-null output string from the process.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull

**Scenario**: `ProcessRunner.TryRunAsync` is called with an invalid/nonexistent command.

**Expected**: Returns `null` rather than throwing.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull

**Scenario**: `ProcessRunner.TryRunAsync` is called with a command that exits non-zero.

**Expected**: Returns `null` due to non-zero exit code.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput

**Scenario**: `ProcessRunner.RunAsync` is called with a valid command.

**Expected**: Returns process output string.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

### RepoConnectors_ProcessRunner_RunAsync_WithFailingCommand_ThrowsException

**Scenario**: `ProcessRunner.RunAsync` is called with a command that fails.

**Expected**: Throws an exception.

**Requirement coverage**: `BuildMark-Utilities-ProcessRunner`

## Requirements Coverage

- **BuildMark-Utilities-ProcessRunner**:
  RepoConnectors_ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput,
  RepoConnectors_ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull,
  RepoConnectors_ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull,
  RepoConnectors_ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput,
  RepoConnectors_ProcessRunner_RunAsync_WithFailingCommand_ThrowsException
