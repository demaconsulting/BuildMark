### VersionSemantic

![Version Structure](VersionView.svg)

#### Purpose

`VersionSemantic` is a record that extends version comparison with optional build metadata
support. It wraps a `VersionComparable` and stores the `+metadata` component of a full
semantic version string. Build metadata is preserved for display but excluded from all
ordering comparisons, as specified by SemVer 2.0.0.

#### Data Model

**Comparable**: `VersionComparable` — Core comparable portion of the version
(`major.minor.patch[-pre-release]`); used for all ordering operations.

**Metadata**: `string?` — Build metadata string (the portion after `+`), or `null` when no
metadata is present.

**FullVersion**: `string` — Derived property; returns `major.minor.patch[-pre-release]` when
there is no metadata, or `major.minor.patch[-pre-release]+metadata` when metadata is present.

**Major**: `int` — Delegated from `Comparable.Major`.

**Minor**: `int` — Delegated from `Comparable.Minor`.

**Patch**: `int` — Delegated from `Comparable.Patch`.

**Numbers**: `string` — Delegated from `Comparable.Numbers`; returns `major.minor.patch`.

**PreRelease**: `string` — Delegated from `Comparable.PreRelease`; returns empty string when
no pre-release is present.

**IsPreRelease**: `bool` — Delegated from `Comparable.IsPreRelease`.

**CompareVersion**: `string` — Delegated from `Comparable.CompareVersion`; build metadata is
excluded from this string per SemVer 2.0.0.

#### Key Methods

**Create**: Parses a full semantic version string (including optional `+metadata`) and returns
a `VersionSemantic`; throws `ArgumentException` on invalid input.

- *Parameters*: `string versionString` — the version string to parse.
- *Returns*: `VersionSemantic` — the parsed version.
- *Preconditions*: Input is a non-null, non-whitespace string containing a valid
  `major.minor.patch` triple.
- *Postconditions*: Returns a valid `VersionSemantic`; throws `ArgumentException` on invalid
  input.

**TryCreate**: Parses a full semantic version string; returns `null` on invalid input instead
of throwing.

- *Parameters*: `string versionString` — the version string to parse.
- *Returns*: `VersionSemantic?` — the parsed version, or `null` if the input is invalid.
- *Preconditions*: None — null and whitespace inputs return `null`.
- *Postconditions*: Returns a valid `VersionSemantic` or `null`; never throws.

Both methods split the input on `+` (using `Split('+', 2)`) to separate the core version from
optional build metadata. The core version part is forwarded to `VersionComparable.TryCreate`;
an empty or absent metadata segment is normalized to `null`.

#### Error Handling

`Create` throws `ArgumentException` when the input string does not match the semantic version
format. `TryCreate` returns `null` instead of throwing. Once a valid instance is constructed,
property access and delegation to `Comparable` cannot fail.

#### Dependencies

- **VersionComparable** — wrapped as the `Comparable` property; provides all ordering and
  comparison operations.

#### Callers

- **VersionTag** — creates a `VersionSemantic` during tag parsing and stores it as the
  `Semantic` property.
