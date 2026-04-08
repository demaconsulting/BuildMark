# WebLink

## Overview

`WebLink` is a record in the BuildNotes subsystem that represents a hyperlink. It
is used by `BuildInformation` to carry the optional complete-changelog link that
appears at the end of the rendered markdown report.

## Data Model

```csharp
public record WebLink(
    string LinkText,
    string TargetUrl);
```

- `LinkText` (`string`) is the human-readable link label shown in the report.
- `TargetUrl` (`string`) is the fully-qualified URL that the link points to.

## Interactions

- `BuildInformation` holds a `WebLink?` as `CompleteChangelogLink`.
- `RepoConnectors` construct the `WebLink` from baseline and current tags.
