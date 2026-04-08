# Version

## Overview

`Version` is a record in the BuildNotes subsystem that represents a parsed semantic
version derived from a repository tag string. Tag names may use a variety of
prefixes and pre-release suffixes; `Version` normalizes them into a canonical
semver form.

## Data Model

```csharp
public record Version(
    string Tag,
    string FullVersion,
    bool IsPreRelease);
```

| Property       | Type     | Description                                                  |
|----------------|----------|--------------------------------------------------------------|
| `Tag`          | `string` | Original tag string as it appears in the repository          |
| `FullVersion`  | `string` | Normalized semantic-version string (e.g., `1.0.0-beta.1`)   |
| `IsPreRelease` | `bool`   | `true` when the version contains a pre-release label         |

Supported tag formats include `v1.0.0`, `ver-1.1.0`, and bare `2.0.0-beta.1`.

## Methods

### `TryCreate(tag) → Version?`

Static factory method. Attempts to parse the supplied tag string into a `Version`
record. Returns `null` if the tag does not match a recognized version format.

## Interactions

| Unit / Subsystem    | Role                                                          |
|---------------------|---------------------------------------------------------------|
| `VersionTag`        | Wraps a `Version` together with its commit hash               |
| `RepoConnectors`    | Connectors call `TryCreate` to parse version tags             |
