# VersionCommitTag

## Overview

`VersionCommitTag` is a record in the Version subsystem that pairs a parsed
`VersionTag` value with the Git commit hash at which that tag was created. It is
used to identify the baseline and current version boundaries when assembling a
`BuildInformation` record.

## Data Model

```csharp
public record VersionCommitTag(
    VersionTag VersionTag,
    string CommitHash);
```

| Property     | Type         | Description                                    |
|--------------|--------------|------------------------------------------------|
| `VersionTag` | `VersionTag` | Parsed version information for this tag        |
| `CommitHash` | `string`     | Git commit hash at the point this tag was made |

## Interactions

- `VersionTag` supplies the parsed tag and semantic version details.
- `BuildInformation` uses `VersionCommitTag` for `BaselineVersionTag` and
  `CurrentVersionTag`.
- `RepoConnectors` construct `VersionCommitTag` records from repository data.
