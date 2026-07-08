## Version Subsystem

![Version Structure](VersionView.svg)

### Overview

The Version subsystem provides semantic version processing for BuildMark. It handles all
version-related operations: parsing raw repository tag strings into structured semantic
versions, comparing versions numerically, evaluating version range expressions, and
associating parsed versions with their Git commit hashes for build boundary determination.

The subsystem boundary covers version representation and operations only — it does not
interact with Git or remote APIs directly. All raw tag strings flow in from repository
connector implementations.

The subsystem contains six units:

- **VersionComparable** — core integer-based semantic version comparison engine
- **VersionSemantic** — full semantic version with optional build metadata
- **VersionTag** — repository tag parsing and normalization
- **VersionInterval** — single version interval model and parser
- **VersionIntervalSet** — ordered set of version intervals for range queries
- **VersionCommitTag** — version tag paired with a Git commit hash

### Interfaces

**Version Processing API**: Public factory methods and record types exposed to the rest of
BuildMark for tag parsing and version range evaluation.

- *Type*: In-process .NET public API.
- *Role*: Provider (other subsystems consume this).
- *Contract*: `VersionTag.Create(tag)` and `VersionTag.TryCreate(tag)` parse a repository
  tag string; `VersionComparable.Create(version)` and `VersionComparable.TryCreate(version)`
  parse a bare semantic version string; `VersionIntervalSet.Parse(text)` parses a
  comma-separated bracket-bound range expression; `VersionIntervalSet.Contains(version)` tests
  whether a version falls within any interval in the set.
- *Constraints*: `Create` factory methods throw `ArgumentException` on invalid input; all
  `TryCreate` and `Parse` methods return `null` or an empty set rather than throwing.

**Repository Tag Consumer**: Accepts raw tag strings from repo connector implementations.

- *Type*: In-process .NET call.
- *Role*: Consumer (this subsystem consumes strings provided by RepoConnectors).
- *Contract*: String tag values forwarded to `VersionTag.TryCreate` or `VersionTag.Create`.
  The parser accepts tags with arbitrary alphabetic or path-separator prefixes preceding a
  `major.minor.patch` triple.
- *Constraints*: Null or whitespace inputs return `null` from `TryCreate`; tags with no
  recognizable `major.minor.patch` triple also return `null`.

**Version Range Consumer**: Accepts interval expression strings from `ItemControlsParser`.

- *Type*: In-process .NET call.
- *Role*: Consumer (this subsystem consumes range strings provided by ItemControlsParser).
- *Contract*: Comma-separated bracket-bound interval strings such as `(,1.0.1],[1.1.0,1.2.0)`
  are parsed via `VersionIntervalSet.Parse`.
- *Constraints*: Malformed interval tokens are silently discarded; the returned set contains
  only the successfully parsed intervals.

### Design

Raw repository tags enter the subsystem as strings from repo connector implementations.
`VersionTag.TryCreate` applies a source-generated regex pattern that accepts an optional
alphabetic or path prefix, extracts the `major.minor.patch` triple, and captures optional
pre-release and build-metadata segments. The numbers and pre-release string are packaged
into a `VersionComparable` record; the `VersionComparable` and optional metadata string are
packaged into a `VersionSemantic` record; and the original tag string and the `VersionSemantic`
are packaged into the returned `VersionTag` record.

`VersionComparable` splits the pre-release string at construction time into a cached
`PreReleaseSegment[]` array, avoiding repeated string parsing on every comparison. `CompareTo`
compares major/minor/patch numerically, then falls back to the pre-release segment array for
pre-release ordering per SemVer 2.0.0 (with case-insensitive text segment comparison by
design). Comparison operator overloads (`<`, `<=`, `>`, `>=`) delegate to `CompareTo`.

Version range expressions enter via `VersionIntervalSet.Parse`, which walks the expression
character by character, tracking bracket depth to distinguish commas that separate intervals
from commas inside a single interval's bounds. Each extracted token is forwarded to
`VersionInterval.Parse`, which reads the opening and closing bracket characters to determine
inclusivity and splits the interior text on the first comma to obtain lower and upper bound
strings. `VersionInterval.Contains` calls `VersionComparable.TryCreate` on each bound string
at evaluation time and applies the inclusivity flags.

`VersionCommitTag` is a plain record pairing a `VersionTag` with a Git commit hash. It carries
no logic; repo connectors construct it and `BuildInformation` consumes it.
