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

| Property    | Type     | Description                                  |
|-------------|----------|----------------------------------------------|
| `LinkText`  | `string` | Human-readable link label shown in the report |
| `TargetUrl` | `string` | Fully-qualified URL that the link points to   |

## Interactions

| Unit / Subsystem    | Role                                                               |
|---------------------|--------------------------------------------------------------------|
| `BuildInformation`  | Holds a `WebLink?` as `CompleteChangelogLink`                      |
| `RepoConnectors`    | Connectors construct the `WebLink` from baseline and current tags  |
