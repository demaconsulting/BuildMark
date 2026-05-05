### ItemInfo

#### Overview

`ItemInfo` is a record in the BuildNotes subsystem that represents a single issue
or pull request entry in the build report. It is produced by connectors and stored
in the `Changes`, `Bugs`, and `KnownIssues` lists of `BuildInformation`.

#### Data Model

```csharp
public record ItemInfo(
    string Id,
    string Title,
    string Url,
    string Type,
    int Index = 0,
    VersionIntervalSet? AffectedVersions = null);
```

| Property           | Type                  | Description                                                |
|--------------------|-----------------------|------------------------------------------------------------|
| `Id`               | `string`              | Human-readable identifier (e.g., `#42`)                    |
| `Title`            | `string`              | Issue or pull request title                                |
| `Url`              | `string`              | Link to the issue or pull request on the host              |
| `Type`             | `string`              | Normalized type: `"bug"`, `"feature"`, or a label name     |
| `Index`            | `int`                 | Numeric issue/PR number for deterministic sorting          |
| `AffectedVersions` | `VersionIntervalSet?` | Interval set from the `affected-versions` field, or `null` |

#### Interactions

| Unit / Subsystem    | Role                                                        |
|---------------------|-------------------------------------------------------------|
| `BuildInformation`  | Contains lists of `ItemInfo` records                        |
| `VersionIntervalSet`| Holds the optional affected-versions interval set           |
| `RepoConnectors`    | Connectors create `ItemInfo` records from repository data   |
| `ItemRouter`        | Routes `ItemInfo` records to report sections                |
