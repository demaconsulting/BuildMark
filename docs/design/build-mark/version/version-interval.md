### VersionInterval

![Version Structure](VersionView.svg)

#### Purpose

`VersionInterval` represents a single mathematical version interval with optional inclusive
or exclusive lower and upper bounds. Either bound may be omitted to indicate an unbounded
range. It parses interval expressions in standard bracket notation (e.g., `[1.1.0,1.2.0)`,
`(,1.0.1]`) and tests whether a given version falls within the interval.

#### Data Model

**LowerBound**: `string?` — Lower boundary version string; `null` means the interval is
unbounded below.

**LowerInclusive**: `bool` — `true` when the lower bound is inclusive (`[`); `false` when
exclusive (`(`).

**UpperBound**: `string?` — Upper boundary version string; `null` means the interval is
unbounded above.

**UpperInclusive**: `bool` — `true` when the upper bound is inclusive (`]`); `false` when
exclusive (`)`).

#### Key Methods

**Parse**: Parses a single interval token such as `(,1.0.1]` or `[1.1.0,1.2.0)` and returns
a `VersionInterval`; returns `null` for malformed input.

- *Parameters*: `string? text` — the interval expression to parse.
- *Returns*: `VersionInterval?` — the parsed interval, or `null` if the input is not a valid
  bracket-bound interval expression.
- *Preconditions*: None — null, whitespace, and non-bracket-bounded strings return `null`.
- *Postconditions*: Returns a valid `VersionInterval` with inclusivity and bound strings set,
  or `null`.

The algorithm verifies the first character is `[` or `(` and the last is `]` or `)`,
determines inclusivity from those characters, then splits the interior on the first `,` to
obtain lower and upper bound strings. An empty bound string is treated as `null` (unbounded).

**Contains(string)**: Tests whether a semantic version string falls within the interval.

- *Parameters*: `string version` — the candidate semantic version string.
- *Returns*: `bool` — `true` if the version is within the interval; `false` if the version is
  outside the interval or is not a valid semantic version.
- *Preconditions*: None.
- *Postconditions*: Returns `false` for invalid version strings rather than throwing.

**Contains(VersionComparable)**: Tests whether a `VersionComparable` instance falls within
the interval.

- *Parameters*: `VersionComparable version` — the candidate version.
- *Returns*: `bool` — `true` if the version is within the interval.
- *Preconditions*: `version` is non-null.
- *Postconditions*: Never throws.

**Contains(VersionTag)**: Convenience overload; delegates to `Contains(VersionComparable)`
using `version.Semantic.Comparable`.

- *Parameters*: `VersionTag version` — the candidate version tag.
- *Returns*: `bool` — `true` if the version is within the interval.
- *Preconditions*: `version` is non-null.
- *Postconditions*: Never throws.

#### Error Handling

`Parse` returns `null` for null, empty, whitespace, or bracket-malformed input rather than
throwing. `Contains(string)` returns `false` when the candidate version string cannot be
parsed by `VersionComparable.TryCreate`, rather than propagating a parse error. Once a valid
instance is constructed, `Contains(VersionComparable)` and `Contains(VersionTag)` never throw.

#### Dependencies

- **VersionComparable** — used by `Contains` overloads to parse bound strings and compare
  versions.
- **VersionTag** — accepted by the `Contains(VersionTag)` convenience overload.

#### Callers

- **VersionIntervalSet** — holds a list of `VersionInterval` instances and delegates `Contains`
  calls to each element.
