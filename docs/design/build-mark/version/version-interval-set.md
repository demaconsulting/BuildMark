### VersionIntervalSet

![Version Structure](VersionView.svg)

#### Purpose

`VersionIntervalSet` is an ordered, immutable collection of one or more `VersionInterval`
instances parsed from a comma-separated bracket-bound range expression such as the
`affected-versions` field of a `buildmark` block. It tests whether a candidate version falls
within any contained interval.

#### Data Model

**Intervals**: `IReadOnlyList<VersionInterval>` — Ordered list of parsed version intervals.
Only intervals that successfully parsed from the input expression are included; malformed
tokens are discarded silently.

#### Key Methods

**Parse**: Parses a comma-separated list of bracket-bound interval expressions and returns a
`VersionIntervalSet`.

- *Parameters*: `string text` — the range expression to parse (e.g.,
  `(,1.0.1],[1.1.0,1.2.0)`).
- *Returns*: `VersionIntervalSet` — a set containing the successfully parsed intervals.
- *Preconditions*: None.
- *Postconditions*: Never throws; unrecognized tokens are silently discarded.

The algorithm walks the string character by character, incrementing a depth counter on `[`
or `(` and decrementing it on `]` or `)`. A complete token is extracted each time the depth
returns to zero; the token is forwarded to `VersionInterval.Parse` and appended to the list
if valid.

**Contains(string)**: Tests whether a semantic version string falls within any interval in
the set.

- *Parameters*: `string version` — the candidate semantic version string.
- *Returns*: `bool` — `true` if any interval contains the version; `false` if no interval
  matches or the version string is invalid.
- *Preconditions*: None.
- *Postconditions*: Never throws.

**Contains(VersionComparable)**: Tests whether a `VersionComparable` instance falls within
any interval in the set.

- *Parameters*: `VersionComparable version` — the candidate version.
- *Returns*: `bool` — `true` if any interval contains the version.
- *Preconditions*: `version` is non-null.
- *Postconditions*: Never throws.

**Contains(VersionTag)**: Convenience overload; delegates to `Contains(VersionComparable)`
using `version.Semantic.Comparable`.

- *Parameters*: `VersionTag version` — the candidate version tag.
- *Returns*: `bool` — `true` if any interval contains the version.
- *Preconditions*: `version` is non-null.
- *Postconditions*: Never throws.

#### Error Handling

`Parse` never throws; it silently discards interval tokens that do not conform to the
bracket-bound format, returning a set containing only the valid intervals. `Contains(string)`
returns `false` when the candidate version string is not a valid semantic version, rather than
propagating a parse error.

#### Dependencies

- **VersionInterval** — each element of `Intervals`; `Contains` overloads delegate to
  `VersionInterval.Contains`.
- **VersionComparable** — accepted by the `Contains(VersionComparable)` overload and used
  indirectly by each `VersionInterval` during bound evaluation.
- **VersionTag** — accepted by the `Contains(VersionTag)` convenience overload.

#### Callers

- **ItemControlsParser** — calls `VersionIntervalSet.Parse` to create the set from the
  `affected-versions` field value.
- **ItemControlsInfo** — stores the `VersionIntervalSet` for the `affected-versions` field.
