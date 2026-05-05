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

### Interactions

`PathHelpers` and `ProcessRunner` have no dependencies on other BuildMark
subsystems. They are consumed by any unit that needs safe path combination or
external process execution.

Version-specific consumers should use the separate `Version` subsystem.
