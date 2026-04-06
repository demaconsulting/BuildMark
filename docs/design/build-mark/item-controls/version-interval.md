# VersionInterval and VersionIntervalSet

## Overview

`VersionInterval` represents a single mathematical version interval using
inclusive or exclusive bounds, where either bound may be omitted to indicate
an unbounded range. `VersionIntervalSet` is an ordered, immutable collection
of one or more `VersionInterval` instances parsed from the `affected-versions`
field of a `buildmark` block.

## Interval Notation

The interval format follows standard mathematical notation:

| Symbol | Meaning                    |
|--------|----------------------------|
| `[`    | Inclusive lower bound      |
| `(`    | Exclusive lower bound      |
| `]`    | Inclusive upper bound      |
| `)`    | Exclusive upper bound      |

An empty lower bound (e.g., `(,1.0.1]`) means no minimum version. An empty
upper bound (e.g., `[3.0.0,)`) means no maximum version.

Multiple intervals are separated by `,` **between** ranges. The parser
distinguishes a separating comma from the commas that appear **inside** an
interval (between the bounds) by tracking bracket depth.

## Data Model — VersionInterval

```csharp
public record VersionInterval(
    string? LowerBound,
    bool LowerInclusive,
    string? UpperBound,
    bool UpperInclusive);
```

| Property         | Type      | Description                                    |
|------------------|-----------|------------------------------------------------|
| `LowerBound`     | `string?` | Lower version string, or `null` if unbounded   |
| `LowerInclusive` | `bool`    | `true` if the lower bound is inclusive (`[`)   |
| `UpperBound`     | `string?` | Upper version string, or `null` if unbounded   |
| `UpperInclusive` | `bool`    | `true` if the upper bound is inclusive (`]`)   |

## Data Model — VersionIntervalSet

```csharp
public record VersionIntervalSet(
    IReadOnlyList<VersionInterval> Intervals);
```

| Property    | Type                              | Description                       |
|-------------|-----------------------------------|-----------------------------------|
| `Intervals` | `IReadOnlyList<VersionInterval>`  | Ordered list of parsed intervals  |

## Methods

### `VersionInterval.Parse(string text) → VersionInterval?`

Parses a single interval token such as `(,1.0.1]` or `[1.1.0,1.2.0)`:

1. Verify the first character is `[` or `(` and the last character is `]` or `)`.
2. Extract the opening bracket to determine `LowerInclusive`.
3. Extract the closing bracket to determine `UpperInclusive`.
4. Split the interior text on the first `,` to obtain the lower and upper bound
   strings.
5. Treat an empty string for either bound as `null` (unbounded).
6. Return `null` if the input does not match the expected pattern.

### `VersionIntervalSet.Parse(string text) → VersionIntervalSet`

Parses a comma-separated list of intervals:

1. Walk the string character by character, tracking the current bracket depth
   (incremented on `[` or `(`, decremented on `]` or `)`).
2. Split on `,` only when bracket depth is zero (these commas separate intervals,
   not bounds within an interval).
3. Call `VersionInterval.Parse` on each token.
4. Discard tokens that do not parse successfully.
5. Return a `VersionIntervalSet` wrapping the resulting list.

## Parsing Examples

| Input                      | Result                                            |
|----------------------------|---------------------------------------------------|
| `(,1.0.1]`                 | One interval: up to and including `1.0.1`         |
| `[1.1.0,1.2.0)`            | One interval: `1.1.0` up to `1.2.0` (exclusive)   |
| `(,1.0.1],[1.1.0,1.2.0)`   | Two intervals                                     |
| `[3.0.0,)`                 | One interval: `3.0.0` and later                   |

## Interactions

`VersionInterval` and `VersionIntervalSet` have no dependencies on other
BuildMark subsystems. They are created only by `ItemControlsParser` and stored
on `ItemControlsInfo`.
