## Utilities

### Verification Approach

The Utilities subsystem is verified through `UtilitiesTests.cs`, which contains six
integration tests. These tests exercise `PathHelpers.SafePathCombine` for valid path
handling and rejection of unsafe inputs, and they exercise `ProcessRunner.TryRunAsync`
and `ProcessRunner.RunAsync` for successful command execution and failure handling.

No mocks or stubs are required at the subsystem boundary because `ProcessRunner` uses
real OS processes and `PathHelpers` is pure logic.

### Test Environment

`ProcessRunner` integration tests invoke real OS commands through portable shell
constructs (`cmd /c echo` on Windows and `echo` on Unix) and therefore require a
working shell on the host. Tests run on Windows, Ubuntu, and macOS in the CI matrix.
`PathHelpers` integration tests have no external dependencies.

### Acceptance Criteria

- All six tests in `UtilitiesTests.cs` pass with zero failures on all supported
  operating systems.
- All `BuildMark-Utilities-*` requirements have at least one test in the ReqStream
  trace matrix.

### Test Scenarios

**Valid paths combine correctly**: This scenario verifies that
`PathHelpers.SafePathCombine` accepts a valid base path and relative path, then returns
one combined path that ends with the expected relative portion. This scenario is tested
by `Utilities_SafePaths_ValidPaths_CombinesCorrectly`.

**Traversal paths are rejected**: This scenario verifies that
`PathHelpers.SafePathCombine` rejects `"../../etc/passwd"` so path traversal cannot
escape the intended base path. The expected outcome is an `ArgumentException`. This
scenario is tested by `Utilities_SafePaths_TraversalPath_ThrowsException`.

**Absolute paths are rejected**: This scenario verifies that
`PathHelpers.SafePathCombine` rejects a platform absolute path passed as the relative
argument so callers cannot bypass the base path restriction. The expected outcome is an
`ArgumentException`. This scenario is tested by
`Utilities_SafePaths_AbsolutePath_ThrowsException`.

**Valid commands return output**: This scenario verifies that `ProcessRunner.RunAsync`
executes a portable echo command and returns non-null output containing the expected
text. This matters because the subsystem must capture command output correctly during
normal execution. This scenario is tested by
`Utilities_ProcessRunner_ValidCommand_ReturnsOutput`.

**Invalid commands return null**: This scenario verifies that
`ProcessRunner.TryRunAsync` handles a non-existent command gracefully and returns `null`
without throwing. This matters because callers rely on the non-throwing path for
optional command execution. This scenario is tested by
`Utilities_ProcessRunner_InvalidCommand_ReturnsNull`.

**Failing commands throw exceptions**: This scenario verifies that
`ProcessRunner.RunAsync` throws an `InvalidOperationException` containing
`"failed with exit code"` when a command exits with code 1. This matters because
callers must receive a clear failure signal for mandatory command execution. This
scenario is tested by `Utilities_ProcessRunner_FailingCommand_ThrowsException`.
