## BuildNotes Subsystem

### Overview

The BuildNotes subsystem holds the shared output data model consumed by all repository
connectors and by `Program`. It defines the records that represent a build's version tags,
changed items, bugs, and known issues, together with the logic to render them as a markdown
report.

The subsystem boundaries include only the data records and the rendering method — it does not
fetch data from any repository or perform any I/O. Repository connectors populate the records;
`Program` drives rendering.

The subsystem contains three units:

- **`BuildInformation`** (`BuildNotes/BuildInformation.cs`) — top-level build data model and
  markdown renderer
- **`ItemInfo`** (`BuildNotes/ItemInfo.cs`) — a single issue or pull request entry in the
  build report
- **`WebLink`** (`BuildNotes/WebLink.cs`) — a hyperlink for the full-changelog entry

### Interfaces

**`BuildInformation.ToMarkdown`**: Primary rendering interface consumed by `Program`.

- *Type*: In-process .NET method
- *Role*: Provider — the BuildNotes subsystem exposes this method for consumers.
- *Contract*: `ToMarkdown(int headingDepth = 1, bool includeKnownIssues = false) → string`;
  returns a fully formatted markdown string from the assembled build data.
- *Constraints*: Callers must populate all required record fields before calling. The method
  does not throw under normal operation.

**`BuildInformation`, `ItemInfo`, `WebLink` record types**: Shared data types for construction
by connectors and consumption by `Program` and `SelfTest`.

- *Type*: In-process .NET record types
- *Role*: Provider — the subsystem exposes these types for construction and consumption.
- *Contract*: Immutable C# records; all fields are set at construction time. `RoutedSections`
  on `BuildInformation` is an optional init-only property populated after construction.
- *Constraints*: Records carry no invariant validation; callers are responsible for providing
  valid field values.

### Design

The BuildNotes subsystem is a pure data and rendering layer. Connectors in the RepoConnectors
subsystem assemble a `BuildInformation` record by populating its version tags and item lists
(`Changes`, `Bugs`, `KnownIssues`, and optionally `RoutedSections`). `Program` then calls
`BuildInformation.ToMarkdown` to convert the in-memory model into the final markdown report
file.

`ItemInfo` and `WebLink` are simple immutable records; their construction and consumption
require no additional coordination. The rendering logic in `ToMarkdown` selects between routed
and legacy output modes based on whether `RoutedSections` is populated, requiring no knowledge
of which connector produced the data.

`Validation` (SelfTest subsystem) also creates `BuildInformation` records during self-tests
using `MockRepoConnector` (RepoConnectors/Mock subsystem) to verify that the rendering logic
produces correct output.

The subsystem has no dependencies on other BuildMark subsystems beyond the Version subsystem,
which provides the `VersionCommitTag` and `VersionIntervalSet` types used in the records.
