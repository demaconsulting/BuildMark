# VersionInfo

## Overview

`VersionInfo` is a record in the Utilities subsystem that parses repository tag
strings into normalized semantic version data. It accepts optional tag
prefixes, optional pre-release labels, and optional build metadata, and it
exposes the parsed result in a form reusable across connectors and reporting.

## Data Model

```csharp
public partial record VersionInfo(
    string Tag,
    string FullVersion,
    string SemanticVersion,
    string PreRelease,
    string Metadata,
    bool IsPreRelease);
```

| Property          | Type     | Description                                                     |
|-------------------|----------|-----------------------------------------------------------------|
| `Tag`             | `string` | Original tag string as it appears in the repository             |
| `FullVersion`     | `string` | Normalized semantic version with any pre-release and metadata   |
| `SemanticVersion` | `string` | Core semantic version (`major.minor.patch`)                     |
| `PreRelease`      | `string` | Parsed pre-release identifier, or empty string if not present   |
| `Metadata`        | `string` | Parsed build metadata, or empty string if not present           |
| `IsPreRelease`    | `bool`   | `true` when a pre-release label was parsed                      |

Supported tag formats include `v1.0.0`, `ver-1.1.0`, `Rel_1.2.3.rc.4+build.5`,
and bare `2.0.0-beta.1`.

## Methods

### `TryCreate(tag) → VersionInfo?`

Static factory method. Attempts to parse the supplied tag string into a
`VersionInfo` record. Returns `null` if the tag does not match a recognized version
format.

### `Create(tag) → VersionInfo`

Static factory method. Calls `TryCreate` and throws `ArgumentException` when the
tag does not match the supported version pattern.

## Interactions

| Unit / Subsystem  | Role                                                                  |
|-------------------|-----------------------------------------------------------------------|
| `RepoConnectors`  | Call `TryCreate` or `Create` when parsing repository tags             |
| `BuildNotes`      | `VersionTag` carries parsed `VersionInfo` data for report boundaries  |
| `VersionInterval` | Consumes `VersionInfo.SemanticVersion` via `Contains(VersionInfo)`    |
