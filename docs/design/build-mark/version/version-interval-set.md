# VersionIntervalSet

## Purpose

`VersionIntervalSet` is an immutable record in the Version subsystem that represents an
ordered collection of `VersionInterval` instances. It models the value of the
`affected-versions` field, which may contain multiple comma-separated ranges.

## Structure

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Intervals` | `IReadOnlyList<VersionInterval>` | Ordered list of parsed version intervals |

## Methods

### `Contains(string version) → bool`

Returns `true` when the semantic version text falls within any interval in the set.
Delegates to `VersionInterval.Contains(string)` for each interval and returns the
logical OR of all results.

### `Contains(VersionComparable version) → bool`

Returns `true` when the comparable version falls within any interval in the set.
Delegates to `VersionInterval.Contains(VersionComparable)` for each interval.

### `Contains(VersionTag version) → bool`

Convenience overload for `VersionTag`. Delegates to
`Contains(version.Semantic.Comparable)`.

### `Parse(string text) → VersionIntervalSet` (static factory)

Parses a comma-separated string of interval tokens into an ordered collection.

#### Parsing Algorithm

The parser walks the input character by character, tracking bracket depth:

1. Increment depth on `[` or `(`.
2. Decrement depth on `]` or `)`.
3. When a closing bracket returns depth to 0, extract the token from the last
   `tokenStart` position to the current position (inclusive).
4. Trim the token and attempt to parse it via `VersionInterval.Parse`. Tokens
   that do not parse are silently discarded.
5. Advance `tokenStart` past any trailing commas and whitespace.
6. Return a `VersionIntervalSet` wrapping the collected intervals.

This depth-tracking approach correctly handles the comma that appears inside a
single interval between its lower and upper bounds.

## Interactions

| Unit / Subsystem           | Role                                                                                   |
|----------------------------|----------------------------------------------------------------------------------------|
| `VersionInterval`          | Each interval token is parsed by `VersionInterval.Parse` and stored in the set         |
| `ItemControlsParser`       | Calls `VersionIntervalSet.Parse` to build the set from the `affected-versions` field   |
| `GitHubRepoConnector`      | Calls `Contains(VersionTag)` to decide whether a bug is a known issue                  |
| `AzureDevOpsRepoConnector` | Calls `Contains(VersionTag)` to decide whether a bug is a known issue                  |
| `MockRepoConnector`        | Uses `VersionIntervalSet` directly in the `_issueAffectedVersions` dictionary          |
