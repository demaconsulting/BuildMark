# Utilities Subsystem

## Overview

The Utilities subsystem provides shared helper classes used across the BuildMark
system. It contains `PathHelpers` for safe path combination with traversal
prevention, `ProcessRunner` for executing external shell commands, and a 
comprehensive version handling hierarchy:

- `VersionComparable` for core integer-based version comparison
- `VersionSemantic` for semantic versioning with build metadata support  
- `VersionTag` for parsing repository tags into normalized version data
- `VersionInfo` for legacy compatibility and semantic version representation
- `VersionInterval`/`VersionIntervalSet` for parsing mathematical version interval
  expressions and testing whether specific versions fall inside them

## Units

| Unit                 | File                              | Responsibility                        |
|----------------------|-----------------------------------|---------------------------------------|
| `PathHelpers`        | `Utilities/PathHelpers.cs`        | Safe path combination                 |
| `ProcessRunner`      | `Utilities/ProcessRunner.cs`      | External process execution            |
| `VersionComparable`  | `Utilities/VersionComparable.cs`  | Core version comparison logic         |
| `VersionSemantic`    | `Utilities/VersionSemantic.cs`    | Semantic version with metadata        |
| `VersionTag`         | `Utilities/VersionTag.cs`         | Repository tag parsing                |
| `VersionInfo`        | `Utilities/VersionInfo.cs`        | Legacy semantic version interface     |
| `VersionInterval`    | `Utilities/VersionInterval.cs`    | Version interval parser               |
| `VersionIntervalSet` | `Utilities/VersionIntervalSet.cs` | Ordered version interval collection   |

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

`VersionComparable` exposes the following methods:

| Member                            | Kind   | Description                                     |
|-----------------------------------|--------|-------------------------------------------------|
| `Create(versionString)`           | Method | Parse a version string into VersionComparable   |
| `TryCreate(versionString)`        | Method | Try parse version string; returns null on failure |
| `CompareTo(other)`                | Method | Compare this version to another analytically    |

`VersionSemantic` exposes the following methods:

| Member                            | Kind   | Description                                     |
|-----------------------------------|--------|-------------------------------------------------|
| `Create(versionString)`           | Method | Parse a version string into VersionSemantic     |
| `TryCreate(versionString)`        | Method | Try parse version string; returns null on failure |

`VersionTag` exposes the following methods:

| Member                            | Kind   | Description                                     |
|-----------------------------------|--------|-------------------------------------------------|
| `Create(tagString)`               | Method | Parse a repository tag into VersionTag          |
| `TryCreate(tagString)`            | Method | Try parse repository tag; returns null on failure |

`VersionInfo` exposes the following static methods:

| Member           | Kind   | Description                                                             |
|------------------|--------|-------------------------------------------------------------------------|
| `TryCreate(tag)` | Method | Parse a repository tag and return a normalized `VersionInfo`, or null   |
| `Create(tag)`    | Method | Parse a repository tag and throw if the tag is not recognized           |

`VersionInterval` exposes the following methods:

| Member                         | Kind   | Description                                                   |
|--------------------------------|--------|---------------------------------------------------------------|
| `Parse(text)`                  | Method | Parse a single interval token; returns null if invalid        |
| `Contains(version)`            | Method | Test whether a semantic version string falls inside interval  |
| `Contains(versionInfo)`        | Method | Test whether a BuildMark `VersionInfo` falls inside interval  |
| `Contains(versionComparable)`  | Method | Test whether a `VersionComparable` falls inside interval     |

`VersionIntervalSet` exposes the following methods:

| Member                         | Kind   | Description                                                             |
|--------------------------------|--------|-------------------------------------------------------------------------|
| `Parse(text)`                  | Method | Parse a comma-separated interval string into an ordered collection      |
| `Contains(version)`            | Method | Test whether a semantic version string falls inside any contained range |
| `Contains(versionInfo)`        | Method | Test whether a BuildMark `VersionInfo` falls inside any contained range |
| `Contains(versionComparable)`  | Method | Test whether a `VersionComparable` falls inside any contained range    |

## Interactions

`PathHelpers` and `ProcessRunner` have no dependencies on other BuildMark 
subsystems. The version hierarchy follows these relationships:

- `VersionComparable` provides core comparison logic (no dependencies)
- `VersionSemantic` builds on `VersionComparable` adding metadata support
- `VersionTag` uses `VersionComparable` for parsing repository tags
- `VersionInfo` provides legacy compatibility by delegating to `VersionSemantic`
- `VersionInterval` and `VersionIntervalSet` consume `VersionComparable` for 
  version comparison and containment checks

The subsystem is consumed by any unit that needs safe path combination, external
process execution, version parsing, version interval parsing, or version
containment checks.
