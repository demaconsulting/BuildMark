### VersionTag

#### Purpose

`VersionTag` parses a raw repository tag string and extracts the embedded semantic version.
It preserves the original tag string for display and logging while providing structured
version access through a `VersionSemantic` property. Equality between `VersionTag` instances
is based on the semantic version content (`Semantic.Comparable`), not on the raw tag string,
so tags with different prefixes but identical semantic versions compare as equal.

#### Data Model

**Tag**: `string` — Original repository tag string exactly as it appears in the repository.

**Semantic**: `VersionSemantic` — Parsed semantic version extracted from the tag.

**FullVersion**: `string` — Delegated from `Semantic.FullVersion`; the extracted semantic
version without the tag prefix.

**Major**: `int` — Delegated from `Semantic.Major`.

**Minor**: `int` — Delegated from `Semantic.Minor`.

**Patch**: `int` — Delegated from `Semantic.Patch`.

**Numbers**: `string` — Delegated from `Semantic.Numbers`; returns `major.minor.patch`.

**PreRelease**: `string` — Delegated from `Semantic.PreRelease`; returns empty string when no
pre-release is present.

**CompareVersion**: `string` — Delegated from `Semantic.CompareVersion`.

**Metadata**: `string` — Delegated from `Semantic.Metadata`; returns empty string when no
build metadata is present.

**IsPreRelease**: `bool` — Delegated from `Semantic.IsPreRelease`.

#### Key Methods

**Create**: Parses a repository tag string and returns a `VersionTag`; throws
`ArgumentException` when no recognizable semantic version can be extracted.

- *Parameters*: `string tag` — the repository tag string to parse.
- *Returns*: `VersionTag` — the parsed tag with its embedded semantic version.
- *Preconditions*: Input contains a `major.minor.patch` triple, optionally preceded by an
  alphabetic or path-separator prefix.
- *Postconditions*: Returns a valid `VersionTag`; throws `ArgumentException` on invalid input.

**TryCreate**: Parses a repository tag string; returns `null` when no semantic version can be
extracted, instead of throwing.

- *Parameters*: `string tag` — the repository tag string to parse.
- *Returns*: `VersionTag?` — the parsed tag, or `null` if no semantic version can be
  extracted.
- *Preconditions*: None — null and whitespace inputs return `null`.
- *Postconditions*: Returns a valid `VersionTag` or `null`; never throws.

Both methods apply a source-generated regex pattern that accepts an optional alphabetic or
path-separator prefix before the `major.minor.patch` triple, and captures optional pre-release
and build-metadata segments. The separator between the numbers and pre-release may be a hyphen
(`-`) or a dot (`.`). Equality between `VersionTag` instances is determined by
`Semantic.Comparable`, so `v1.2.3` and `release/1.2.3` are considered equal.

**ToString**: Returns the original `Tag` string verbatim.

- *Parameters*: None.
- *Returns*: `string` — the original repository tag string.
- *Preconditions*: None.
- *Postconditions*: Never throws; always returns the `Tag` value set at construction.

#### Error Handling

`Create` throws `ArgumentException` when the tag string does not contain a recognizable
`major.minor.patch` triple. `TryCreate` returns `null` instead of throwing. Once constructed,
property access and `ToString` cannot fail.

#### Dependencies

- **VersionSemantic** — stores the parsed semantic version as the `Semantic` property.
- **VersionComparable** — accessed indirectly through `Semantic.Comparable` for equality and
  ordering.

#### Callers

- **VersionCommitTag** — holds a `VersionTag` as its `VersionTag` property.
- **RepoConnectorBase** — uses `VersionTag` instances to locate version boundaries (e.g.,
  `FindVersionIndex`).
- **ItemControlsParser** — filters items by version using `VersionTag` values.
- **Program** — uses `VersionTag` when filtering build notes by version range.
