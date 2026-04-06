# ItemControls Subsystem

## Overview

The ItemControls subsystem provides parsing and data-model support for the
`buildmark` fenced code block that developers embed in GitHub issue and pull
request descriptions. It extracts optional visibility, type, and
affected-version overrides from the block and exposes them as a strongly-typed
record for the `GitHubRepoConnector` to apply when building the item lists.

The subsystem has no dependencies on other BuildMark subsystems. It receives a
plain description string and returns a data record; it performs no I/O.

## Units

| Unit                  | File                                 | Responsibility                                        |
|-----------------------|--------------------------------------|-------------------------------------------------------|
| `ItemControlsInfo`    | `ItemControls/ItemControlsInfo.cs`   | Data record holding visibility, type, and version set |
| `ItemControlsParser`  | `ItemControls/ItemControlsParser.cs` | Extracts and parses the buildmark block from a string |
| `VersionInterval`     | `ItemControls/VersionInterval.cs`    | Single mathematical version interval model and parser |
| `VersionIntervalSet`  | `ItemControls/VersionIntervalSet.cs` | Ordered collection of version intervals               |

## Interfaces

`ItemControlsParser` exposes the following static method consumed by
`GitHubRepoConnector`:

| Member               | Kind   | Description                                                   |
|----------------------|--------|---------------------------------------------------------------|
| `Parse(description)` | Method | Extract and parse the buildmark block; returns null if absent |

`VersionIntervalSet` exposes the following static method:

| Member        | Kind   | Description                                        |
|---------------|--------|----------------------------------------------------|
| `Parse(text)` | Method | Parse a comma-separated interval string into a set |

## Interactions

| Unit / Subsystem      | Role                                                              |
|-----------------------|-------------------------------------------------------------------|
| `GitHubRepoConnector` | Calls `ItemControlsParser.Parse` on each issue and PR description |
| `ItemControlsInfo`    | Returned by `ItemControlsParser`; consumed by the connector       |
| `VersionIntervalSet`  | Stored on `ItemControlsInfo.AffectedVersions`                     |
