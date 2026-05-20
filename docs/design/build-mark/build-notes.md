## BuildNotes Subsystem

### Overview

The BuildNotes subsystem holds the output data model shared by all connectors and
by `Program`. It defines the records that represent a build's version tags,
changed items, bugs, and known issues, together with the logic to render them as
a markdown report.

All connectors produce a `BuildInformation` record from these types, and `Program`
calls `BuildInformation.ToMarkdown` to write the final report file.

### Units

| Unit               | File                             | Responsibility                                   |
|--------------------|----------------------------------|--------------------------------------------------|
| `BuildInformation` | `BuildNotes/BuildInformation.cs` | Top-level build data model and markdown renderer |
| `ItemInfo`         | `BuildNotes/ItemInfo.cs`         | Single issue or pull request in the report       |
| `WebLink`          | `BuildNotes/WebLink.cs`          | Hyperlink used for the full-changelog entry      |

### Interfaces

The primary interface consumed by `Program` is:

| Member                                   | Kind   | Description                                                       |
|------------------------------------------|--------|-------------------------------------------------------------------|
| `BuildInformation.ToMarkdown(depth, includeKnownIssues)` | Method | Renders assembled build data as a markdown string |

The data records `BuildInformation`, `ItemInfo`, and `WebLink` are the shared
output types exposed to all connectors for assembly, and to `Program` and
`SelfTest` for consumption.

### Design

The BuildNotes subsystem is a pure data and rendering layer. Connectors in the
`RepoConnectors` subsystem assemble a `BuildInformation` record by populating its
version tags and item lists (`Changes`, `Bugs`, `KnownIssues`, and optionally
`RoutedSections`). `Program` then calls `BuildInformation.ToMarkdown` to convert
the in-memory model into the final markdown report file.

`ItemInfo` and `WebLink` are simple immutable records; their construction and
consumption require no additional coordination. The rendering logic in
`ToMarkdown` selects between routed and legacy output modes based on whether
`RoutedSections` is populated, requiring no knowledge of which connector produced
the data.

### Interactions

| Unit / Subsystem    | Role                                                               |
|---------------------|--------------------------------------------------------------------|
| `Version`           | Supplies the version types used by `VersionCommitTag`              |
| `RepoConnectors`    | Connectors construct and populate `BuildInformation` records       |
| `Program`           | Calls `BuildInformation.ToMarkdown` to produce the report file     |
| `SelfTest`          | `Validation` creates `BuildInformation` records during self-tests  |
