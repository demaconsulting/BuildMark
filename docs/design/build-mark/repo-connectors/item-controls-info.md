# ItemControlsInfo

## Overview

`ItemControlsInfo` is the data record used by the RepoConnectors subsystem to
carry the controls extracted from a `buildmark` block. It stores the optional
visibility override, type override, and affected-version interval data that
`GitHubRepoConnector` applies while constructing `ItemInfo` records.

## Data Model

```csharp
public record ItemControlsInfo(
    string? Visibility,
    string? Type,
    VersionIntervalSet? AffectedVersions);
```

- `Visibility` (`string?`) stores the optional visibility override (`public` or
  `internal`).
- `Type` (`string?`) stores the optional type override (`bug` or `feature`).
- `AffectedVersions` (`VersionIntervalSet?`) stores the optional
  affected-version interval set.

## Interactions

- `ItemControlsParser` creates an `ItemControlsInfo` instance when a `buildmark`
  block contains one or more recognized keys.
- `GitHubRepoConnector` consumes the parsed values to override item visibility,
  item type, and affected-version metadata.
- `VersionIntervalSet` carries the parsed interval representation for the
  `affected-versions` field.
