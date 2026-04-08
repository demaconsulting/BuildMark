# Utilities Subsystem

## Overview

The Utilities subsystem provides shared helper classes used across the BuildMark
system. It contains `PathHelpers` for safe path combination with traversal
prevention, `ProcessRunner` for executing external shell commands, and
`VersionInterval`/`VersionIntervalSet` for parsing mathematical version interval
expressions.

The subsystem has no dependencies on other BuildMark subsystems.

## Units

| Unit                  | File                                  | Responsibility                                        |
|-----------------------|---------------------------------------|-------------------------------------------------------|
| `PathHelpers`         | `Utilities/PathHelpers.cs`            | Safe path combination with traversal checks           |
| `ProcessRunner`       | `Utilities/ProcessRunner.cs`          | Executes external shell commands                      |
| `VersionInterval`     | `Utilities/VersionInterval.cs`        | Single mathematical version interval model and parser |
| `VersionIntervalSet`  | `Utilities/VersionIntervalSet.cs`     | Ordered collection of version intervals               |

## Interfaces

`PathHelpers` exposes the following static method:

| Member                            | Kind   | Description                                     |
|-----------------------------------|--------|-------------------------------------------------|
| `SafePathCombine(base, relative)` | Method | Safely combine a base path with a relative path |

`ProcessRunner` exposes the following static methods:

| Member                            | Kind   | Description                                             |
|-----------------------------------|--------|---------------------------------------------------------|
| `RunAsync(command, arguments)`    | Method | Run a process and return stdout; throws on failure      |
| `TryRunAsync(command, arguments)` | Method | Run a process and return stdout, or null on any failure |

`VersionInterval` exposes the following static method:

| Member         | Kind   | Description                                              |
|----------------|--------|----------------------------------------------------------|
| `Parse(text)`  | Method | Parse a single interval token; returns null if invalid   |

`VersionIntervalSet` exposes the following static method:

| Member         | Kind   | Description                                                        |
|----------------|--------|--------------------------------------------------------------------|
| `Parse(text)`  | Method | Parse a comma-separated interval string into an ordered collection |

## Interactions

All units in this subsystem have no dependencies on other BuildMark subsystems.
They are consumed by any unit that needs safe path combination, external process
execution, or version interval parsing.
