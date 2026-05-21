### VersionIntervalSet

#### Purpose

`VersionIntervalSet` is an ordered, immutable collection of one or more `VersionInterval`
instances. It is parsed from the `affected-versions` field of a `buildmark` block and tests
whether a version falls inside any contained interval. The full parsing algorithm and detailed
method descriptions are documented together with `VersionInterval` in
_VersionInterval and VersionIntervalSet Design_.

#### Data Model

```csharp
public record VersionIntervalSet(
    IReadOnlyList<VersionInterval> Intervals);
```

| Property    | Type                             | Description                      |
|-------------|----------------------------------|----------------------------------|
| `Intervals` | `IReadOnlyList<VersionInterval>` | Ordered list of parsed intervals |

#### Key Methods

- `Parse(string text)` — Parses a comma-separated list of interval expressions into a
  `VersionIntervalSet`; discards unrecognized tokens silently
- `Contains(string version)` — Tests whether a semantic version string falls within
  any interval in the set; returns `false` for invalid version strings
- `Contains(VersionComparable version)` — Tests whether a `VersionComparable` instance
  falls within any interval in the set
- `Contains(VersionTag version)` — Convenience overload delegating to
  `Contains(VersionComparable)` using `version.Semantic.Comparable`

See _VersionInterval and VersionIntervalSet Design_ for the full algorithmic descriptions of
each method.

#### Error Handling

`Parse` silently discards interval tokens that do not conform to the expected bracket-bound
format, returning a set containing only the valid intervals. `Contains(string version)` returns
`false` when the candidate version string is not a valid semantic version, rather than
propagating a parse error.

#### Interactions

| Unit / Subsystem     | Role                                                                     |
|----------------------|--------------------------------------------------------------------------|
| `VersionInterval`    | Each element of `Intervals`; `Contains` overloads delegate to it         |
| `VersionComparable`  | Used by `Contains(VersionComparable)` for ordered semantic comparison    |
| `VersionTag`         | Accepted by the `Contains(VersionTag)` convenience overload              |
| `ItemControlsParser` | Creates `VersionIntervalSet` from the `affected-versions` field value    |
| `ItemControlsInfo`   | Holds the `VersionIntervalSet` for the `affected-versions` field         |
