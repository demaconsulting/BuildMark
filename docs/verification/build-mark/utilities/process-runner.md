### ProcessRunner

#### Verification Approach

`ProcessRunner` has no mockable dependencies, so the unit tests use real OS processes to verify
actual command execution behavior. It is verified through `ProcessRunnerTests.cs`, which contains
7 unit tests covering `TryRunAsync` and `RunAsync` success paths, invalid commands, non-zero exit
codes, and exception handling by using portable echo and shell constructs.

#### Test Environment

Tests invoke real OS commands via portable shell constructs (`cmd /c echo` on Windows, `echo` on
Unix) and require a working shell on the host. Tests run on Windows, Ubuntu, and macOS in the CI
matrix.

#### Acceptance Criteria

- All 7 tests in `ProcessRunnerTests.cs` pass with zero failures on all supported operating
  systems.

#### Test Scenarios

**ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput**: This scenario verifies that
`TryRunAsync` returns captured standard output when a valid portable echo command succeeds. This
matters because callers use `TryRunAsync` as the non-throwing execution path for optional process
work. This scenario is tested by `ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput`.

**ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull**: This scenario verifies that
`TryRunAsync` returns `null` instead of throwing when the command does not exist. This matters
because the method is expected to absorb lookup failures and report them through a null result. This
scenario is tested by `ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull`.

**ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull**: This scenario verifies that
`TryRunAsync` returns `null` when a process starts but exits with a non-zero exit code. This
matters because callers should be able to treat command failure as an absent result without
exception handling. This scenario is tested by
`ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull`.

**ProcessRunner_TryRunAsync_WithException_ReturnsNull**: This scenario verifies that
`TryRunAsync` catches internal execution exceptions, such as an empty command name, and returns
`null` rather than propagating the failure. This scenario is tested by
`ProcessRunner_TryRunAsync_WithException_ReturnsNull`.

**ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput**: This scenario verifies that `RunAsync`
returns captured standard output for a successful portable echo command. This matters because
`RunAsync` is the strict execution path that should preserve successful process output. This
scenario is tested by `ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput`.

**ProcessRunner_RunAsync_WithFailingCommand_ThrowsException**: This scenario verifies that
`RunAsync` throws an `InvalidOperationException` when a command exits with a non-zero exit code.
This matters because callers rely on `RunAsync` to surface execution failures explicitly. This
scenario is tested by `ProcessRunner_RunAsync_WithFailingCommand_ThrowsException`.

**ProcessRunner_RunAsync_WithNonexistentCommand_ThrowsDescriptiveException**: This scenario
verifies that `RunAsync` throws an `InvalidOperationException` that identifies the missing command
when process startup fails. This matters because the caller needs a descriptive error for diagnosis.
This scenario is tested by
`ProcessRunner_RunAsync_WithNonexistentCommand_ThrowsDescriptiveException`.
