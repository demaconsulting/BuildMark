## Utilities Subsystem

### Overview

The Utilities subsystem provides the shared helper classes used across the BuildMark system. It
encapsulates safe path combination, external process execution, and disposable temporary directory
management so that the rest of the system can delegate these cross-cutting concerns to a single,
well-tested location.

Version parsing, comparison, tag handling, and interval logic are implemented in the separate Version
subsystem.

The subsystem contains three units:

- **`PathHelpers`** (`Utilities/PathHelpers.cs`) — safe path combination with traversal prevention
- **`ProcessRunner`** (`Utilities/ProcessRunner.cs`) — async wrapper for executing external shell commands
- **`TemporaryDirectory`** (`Utilities/TemporaryDirectory.cs`) — disposable temporary directory management

### Interfaces

**`PathHelpers.SafePathCombine`**: Safely combines a base path with a relative path, consumed by
`TemporaryDirectory` and any unit that writes to user-supplied paths.

- *Type*: In-process .NET static method
- *Role*: Provider — exposes safe path combination to other units
- *Contract*: `SafePathCombine(string basePath, string relativePath) → string`; returns the combined path
  if the result stays within the base directory.
- *Constraints*: Throws `ArgumentNullException` for null inputs; throws `ArgumentException` when the
  combined path escapes the base directory.

**`ProcessRunner.RunAsync`**: Runs an external process and returns its stdout, consumed by
`RepoConnectorBase` and any unit that needs to execute CLI commands.

- *Type*: In-process .NET static async method
- *Role*: Provider — exposes process execution to connector units
- *Contract*: `RunAsync(string command, params string[] arguments) → Task<string>`; captures stdout and
  returns the trimmed output string.
- *Constraints*: Throws `InvalidOperationException` on non-zero exit code or missing command.

**`ProcessRunner.TryRunAsync`**: Runs an external process and returns its stdout or null on any failure.

- *Type*: In-process .NET static async method
- *Role*: Provider — exposes fault-tolerant process execution
- *Contract*: `TryRunAsync(string command, params string[] arguments) → Task<string?>`; returns stdout on
  success or `null` on any failure.
- *Constraints*: Never throws; all exceptions are caught and `null` is returned.

**`TemporaryDirectory`**: Creates a disposable temporary directory, consumed by `Validation` and any unit
that needs isolated scratch space.

- *Type*: In-process .NET instance class implementing `IDisposable`
- *Role*: Provider — exposes temporary directory lifecycle management
- *Contract*: Constructor creates the directory; `GetFilePath(string relativePath)` returns a validated
  path within it; `Dispose()` deletes the directory and all contents.
- *Constraints*: `GetFilePath` throws `ArgumentException` if `relativePath` escapes the directory
  boundary. `Dispose` suppresses `IOException` and `UnauthorizedAccessException`.

### Design

The three units are independent of each other except that `TemporaryDirectory` depends on
`PathHelpers.SafePathCombine` for path validation in `GetFilePath`.

`PathHelpers.SafePathCombine` is a pure function that normalizes both the base path and the candidate path
to absolute form with `Path.GetFullPath`, then computes `Path.GetRelativePath(absoluteBase,
absoluteCombined)` and rejects the input if the result escapes the base directory. Using `GetRelativePath`
for the containment check handles root paths, platform case-sensitivity, and directory-separator
normalization natively.

`ProcessRunner.RunAsync` and `TryRunAsync` are async wrappers over `System.Diagnostics.Process`. On
Windows, non-empty commands are routed through `cmd /c` so that `.cmd` and `.bat` scripts (such as the
Azure CLI `az.cmd`) are resolved correctly. All connector and factory code that needs to run Git, `gh`, or
`az` CLI commands delegates to `ProcessRunner` via `RepoConnectorBase.RunCommandAsync`.

`TemporaryDirectory` creates a uniquely-named directory under `Environment.CurrentDirectory` on
construction. Using `Environment.CurrentDirectory` avoids the macOS `/tmp` → `/private/tmp` symlink
mismatch that can cause path-containment checks to fail. `GetFilePath` delegates path validation to
`PathHelpers.SafePathCombine` before creating any missing intermediate directories, ensuring all paths
remain within the temporary directory.
