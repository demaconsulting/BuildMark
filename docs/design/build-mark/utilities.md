## Utilities Subsystem

### Overview

The Utilities subsystem provides the shared helper classes used across the
BuildMark system:

- `PathHelpers` for safe path combination with traversal prevention
- `ProcessRunner` for executing external shell commands

Version parsing, comparison, tag handling, and interval logic are implemented in
the separate `Version` subsystem.

### Units

| Unit            | File                        | Responsibility             |
|-----------------|-----------------------------|----------------------------|
| `PathHelpers`   | `Utilities/PathHelpers.cs`  | Safe path combination      |
| `ProcessRunner` | `Utilities/ProcessRunner.cs`| External process execution |

### Interfaces

`PathHelpers` exposes the following static method:

| Member                            | Kind   | Description                                     |
|-----------------------------------|--------|-------------------------------------------------|
| `SafePathCombine(base, relative)` | Method | Safely combine a base path with a relative path |

`ProcessRunner` exposes the following static methods:

| Member                                  | Kind   | Description                                             |
|-----------------------------------------|--------|---------------------------------------------------------|
| `RunAsync(command, params arguments)`   | Method | Run a process and return stdout; throws on failure      |
| `TryRunAsync(command, params arguments)`| Method | Run a process and return stdout, or null on any failure |

### Design

The Utilities subsystem contains two independent, stateless units with no
dependencies on each other or on any other BuildMark subsystem:

- `PathHelpers.SafePathCombine` is a pure function that combines a base path and
  a relative path, normalizes both to absolute form, and rejects the result if the
  combined path escapes the base directory. It is consumed by any unit that needs
  to write output files to user-supplied paths safely.

- `ProcessRunner.RunAsync` and `TryRunAsync` are async wrappers over
  `System.Diagnostics.Process` that capture stdout and return it as a trimmed
  string. On Windows, commands are routed through `cmd /c` to handle `.cmd`
  and `.bat` scripts; on other platforms they are invoked directly. All connector
  and factory code that needs to run Git, `gh`, or `az` CLI commands delegates
  to `ProcessRunner` via `RepoConnectorBase.RunCommandAsync`.

### Interactions

`PathHelpers` and `ProcessRunner` have no dependencies on other BuildMark
subsystems. They are consumed by any unit that needs safe path combination or
external process execution.

Version-specific consumers should use the separate `Version` subsystem.
