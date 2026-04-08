# Utilities Subsystem

## Overview

The Utilities subsystem provides shared helper classes used across the BuildMark
system. It contains `PathHelpers`, which provides safe path combination with
traversal prevention, and `ProcessRunner`, which executes external shell commands
and captures their output.

The subsystem has no dependencies on other BuildMark subsystems.

## Units

| Unit            | File                           | Responsibility                              |
|-----------------|--------------------------------|---------------------------------------------|
| `PathHelpers`   | `Utilities/PathHelpers.cs`     | Safe path combination with traversal checks |
| `ProcessRunner` | `Utilities/ProcessRunner.cs`   | Executes external shell commands            |

## Interfaces

`PathHelpers` exposes the following static method:

| Member                            | Kind   | Description                                     |
|-----------------------------------|--------|-------------------------------------------------|
| `SafePathCombine(base, relative)` | Method | Safely combine a base path with a relative path |

`ProcessRunner` exposes the following static methods:

| Member                              | Kind   | Description                                              |
|-------------------------------------|--------|----------------------------------------------------------|
| `RunAsync(command, arguments)`      | Method | Run a process and return stdout; throws on failure       |
| `TryRunAsync(command, arguments)`   | Method | Run a process and return stdout, or null on any failure  |

## Interactions

`PathHelpers` and `ProcessRunner` have no dependencies on other BuildMark
subsystems. They are used by any unit that needs to combine file paths safely or
execute external processes.
