# VersionCommitTag

## Overview

`VersionCommitTag` is a record in the BuildNotes subsystem that pairs a parsed
`VersionInfo` value from the Utilities subsystem with the Git commit hash at which
that version tag was created. It is used to identify the baseline and current
version boundaries when assembling a `BuildInformation` record.

## Data Model

```csharp
public record VersionCommitTag(
    VersionInfo VersionInfo,
    string CommitHash);
```

| Property      | Type          | Description                                    |
|---------------|---------------|------------------------------------------------|
| `VersionInfo` | `VersionInfo` | Parsed version information for this tag        |
| `CommitHash`  | `string`      | Git commit hash at the point this tag was made |

## Interactions

- `Utilities` supplies the `VersionInfo` type that carries parsed version details.
- `BuildInformation` uses `VersionCommitTag` for `BaselineVersionTag` and
  `CurrentVersionTag`.
- `RepoConnectors` construct `VersionCommitTag` records from repository data.
