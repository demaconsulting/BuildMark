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

### Interactions

| Unit / Subsystem    | Role                                                               |
|---------------------|--------------------------------------------------------------------|
| `Version`           | Supplies the version types used by `VersionCommitTag`              |
| `RepoConnectors`    | Connectors construct and populate `BuildInformation` records       |
| `Program`           | Calls `BuildInformation.ToMarkdown` to produce the report file     |
| `SelfTest`          | `Validation` creates `BuildInformation` records during self-tests  |
