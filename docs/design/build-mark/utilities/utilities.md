# Utilities Subsystem

## Overview

The Utilities subsystem provides shared helper classes used across the BuildMark
system. It contains `PathHelpers` for safe path combination with traversal
prevention, `ProcessRunner` for executing external shell commands, `Version` for
parsing repository tags into normalized semantic version data, and
`VersionInterval`/`VersionIntervalSet` for parsing mathematical version interval
expressions and testing whether specific versions fall inside them.

## Units

| Unit                  | File                                  | Responsibility                                        |
|-----------------------|---------------------------------------|-------------------------------------------------------|
| `PathHelpers`         | `Utilities/PathHelpers.cs`            | Safe path combination with traversal checks           |
| `ProcessRunner`       | `Utilities/ProcessRunner.cs`          | Executes external shell commands                      |
| `Version`             | `Utilities/Version.cs`                | Parses version tags into normalized semantic data     |
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

`Version` exposes the following static methods:

| Member             | Kind   | Description                                                       |
|--------------------|--------|-------------------------------------------------------------------|
| `TryCreate(tag)`   | Method | Parse a repository tag and return a normalized `Version`, or null |
| `Create(tag)`      | Method | Parse a repository tag and throw if the tag is not recognized     |

`VersionInterval` exposes the following methods:

| Member                    | Kind   | Description                                                   |
|---------------------------|--------|---------------------------------------------------------------|
| `Parse(text)`             | Method | Parse a single interval token; returns null if invalid        |
| `Contains(version)`       | Method | Test whether a semantic version string falls inside interval  |
| `Contains(versionInfo)`   | Method | Test whether a BuildMark `Version` falls inside interval      |

`VersionIntervalSet` exposes the following methods:

| Member                    | Kind   | Description                                                             |
|---------------------------|--------|-------------------------------------------------------------------------|
| `Parse(text)`             | Method | Parse a comma-separated interval string into an ordered collection      |
| `Contains(version)`       | Method | Test whether a semantic version string falls inside any contained range |
| `Contains(versionInfo)`   | Method | Test whether a BuildMark `Version` falls inside any contained range     |

## Interactions

`PathHelpers`, `ProcessRunner`, and `Version` have no dependencies on other
BuildMark subsystems. `VersionInterval` and `VersionIntervalSet` may consume
`Version` instances through their `Contains(Version)` overloads. The subsystem
is consumed by any unit that needs safe path combination, external process
execution, version parsing, version interval parsing, or version containment
checks.
