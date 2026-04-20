# Version Comparable

## Purpose

The `VersionComparable` class provides core semantic version comparison functionality.
It handles versions in the format `major.minor.patch[-pre-release]` and implements
proper semantic version ordering rules with optimized performance for pre-release comparison.

## Structure

| Property | Type | Description |
| -------- | ---- | ----------- |
| Major | int | Major version number |
| Minor | int | Minor version number |
| Patch | int | Patch version number |
| PreRelease | string? | Pre-release identifier (null for release versions) |
| CompareVersion | string | Normalized comparison string (major.minor.patch[-pre-release]) |

## Performance Optimization

### Pre-Release Parsing

The class implements optimized pre-release comparison through:

- **PreReleaseSegment Structure**: Internal record struct representing parsed pre-release segments
- **Construction-Time Parsing**: Pre-release strings are parsed once during object construction
- **Efficient Comparison**: Comparisons use pre-parsed segments instead of repeated string operations

### Implementation Details

| Component | Description |
| --------- | ----------- |
| `PreReleaseSegment` | Internal struct storing either numeric or text segment values |
| `ParsePreReleaseSegments` | Static method parsing pre-release strings into segment arrays |
| `ComparePreReleaseSegments` | Optimized comparison using pre-parsed segment arrays |

This optimization eliminates repeated string splitting and parsing during comparison operations,
providing significant performance benefits for sorting and version range operations.

## Interface

The class implements `IComparable<VersionComparable>` providing analytical comparison:

- Numeric comparison of version numbers (1.2.3 < 1.11.2)
- Release versions > pre-release versions for same numbers
- SemVer-compliant pre-release ordering with numeric precedence

### Pre-Release Comparison Rules

Pre-release versions follow SemVer specification:

1. Numeric identifiers are compared numerically (5 < 10)
2. Non-numeric identifiers are compared lexicographically (case-insensitive by design; note the SemVer specification uses case-sensitive ASCII sort order)
3. Numeric identifiers have lower precedence than non-numeric
4. Shorter pre-release lists have lower precedence than longer ones

## Comparison Operators

Standard comparison operators are overloaded:

- `<`, `<=`, `>`, `>=` delegate to `CompareTo`
- Natural ordering for use in collections and sorting

## Factory Methods

- `Create(string version)` - Creates instance, throws on invalid input
- `TryCreate(string version)` - Returns null for invalid input

## Example

```csharp
var v1 = VersionComparable.Create("1.2.3");
var v2 = VersionComparable.Create("1.2.3-beta");
var v3 = VersionComparable.Create("1.11.0");
var v4 = VersionComparable.Create("1.2.3-alpha.5");
var v5 = VersionComparable.Create("1.2.3-alpha.10");

// v4 < v5 < v2 < v1 < v3 (numeric pre-release comparison, pre-release < release)
```

## Regex Pattern

The class uses a generated regex pattern for parsing:

```regex
^(?<numbers>\d+\.\d+\.\d+)(?:-(?<pre_release>[a-zA-Z0-9.-]+))?$
```

This pattern matches:

- Required: major.minor.patch numbers
- Optional: hyphen followed by pre-release identifier
