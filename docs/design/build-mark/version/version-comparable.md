### VersionComparable

![Version Structure](VersionView.svg)

#### Purpose

`VersionComparable` represents the comparable portion of a semantic version —
`major.minor.patch[-pre-release]`. It parses version strings using a source-generated regex,
caches pre-release segments at construction time for efficient repeated comparisons, and
implements `IComparable<VersionComparable>` with numeric major/minor/patch ordering and
SemVer-compliant pre-release precedence rules. Non-numeric pre-release segments are compared
case-insensitively, which differs from the ASCII case-sensitive sort defined by SemVer 2.0.0.

#### Data Model

**Major**: `int` — Major version number parsed from the version string.

**Minor**: `int` — Minor version number parsed from the version string.

**Patch**: `int` — Patch version number parsed from the version string.

**PreRelease**: `string?` — Pre-release identifier (e.g., `rc.4`, `alpha.1`), or `null`
for release versions.

**Numbers**: `string` — Derived property; returns `major.minor.patch` as a formatted string.

**IsPreRelease**: `bool` — Derived property; `true` when `PreRelease` is non-null and
non-empty.

**CompareVersion**: `string` — Derived property; returns `major.minor.patch` for release
versions and `major.minor.patch-pre-release` for pre-release versions.

#### Key Methods

**Create**: Parses a `major.minor.patch[-pre-release]` string and returns a
`VersionComparable`; throws `ArgumentException` when the input does not match the expected
format.

- *Parameters*: `string versionString` — the version string to parse.
- *Returns*: `VersionComparable` — the parsed version.
- *Preconditions*: Input is a non-null, non-whitespace string.
- *Postconditions*: Returns a valid `VersionComparable`; throws `ArgumentException` on
  invalid input.

**TryCreate**: Parses a `major.minor.patch[-pre-release]` string; returns `null` on invalid
input instead of throwing.

- *Parameters*: `string versionString` — the version string to parse.
- *Returns*: `VersionComparable?` — the parsed version, or `null` if the input is invalid.
- *Preconditions*: None — null and whitespace inputs return `null`.
- *Postconditions*: Returns a valid `VersionComparable` or `null`; never throws.

Both methods apply the source-generated regex pattern
`^(?<numbers>\d+\.\d+\.\d+)(?:-(?<pre_release>[a-zA-Z0-9.-]+))?$` to validate and extract
version components.

**CompareTo**: Implements `IComparable<VersionComparable>`; compares two versions using
numeric major/minor/patch fields, then SemVer pre-release ordering using the pre-parsed
`PreReleaseSegment[]` cache.

- *Parameters*: `VersionComparable? other` — the version to compare against.
- *Returns*: `int` — negative if this instance is less, zero if equal, positive if greater.
- *Preconditions*: None — a `null` argument is treated as less than any non-null value.
- *Postconditions*: Returns a consistent, transitive ordering; never throws.

Comparison operator overloads (`<`, `<=`, `>`, `>=`) delegate to `CompareTo`.

#### Error Handling

`Create` throws `ArgumentException` when the input string does not match the
`major.minor.patch[-pre-release]` format. `TryCreate` returns `null` instead of throwing.
Once a valid instance is constructed, `CompareTo` and the operator overloads never fail.

#### Dependencies

N/A — `VersionComparable` has no dependencies on other BuildMark units; it relies only on
.NET BCL types (`System.Text.RegularExpressions`, `System`).

#### Callers

- **VersionSemantic** — wraps a `VersionComparable` as its `Comparable` property.
- **VersionTag** — accesses `Comparable` via `Semantic.Comparable` for equality and ordering.
- **VersionInterval** — calls `VersionComparable.TryCreate` on bound strings during `Contains`
  evaluation.
- **VersionIntervalSet** — accepts a `VersionComparable` overload in `Contains`.
