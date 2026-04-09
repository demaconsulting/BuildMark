# Version Semantic

## Purpose

The `VersionSemantic` class extends `VersionComparable` with semantic version metadata
support. It provides the full semantic version structure including build metadata
while preserving comparison functionality.

## Structure

| Property | Type | Description |
|----------|------|-------------|
| Comparable | VersionComparable | Core comparison logic and version components |
| Metadata | string? | Build metadata (+metadata) |
| FullVersion | string | Complete version string (major.minor.patch[-pre-release][+metadata]) |

## Delegated Properties

For convenience, the following properties delegate to the `Comparable` instance:
- `Major`, `Minor`, `Patch` - Version number components
- `PreRelease` - Pre-release identifier
- `CompareVersion` - Comparison string (excludes metadata)

## Comparison

Comparison operations are performed on the `Comparable` instance, following
semantic version rules where build metadata does not affect precedence.

## Factory Methods

- `Create(string version)` - Creates instance, throws on invalid input
- `TryCreate(string version)` - Returns null for invalid input

## Example

```csharp
var version = VersionSemantic.Create("1.2.3-beta.1+build.123");
// version.Major = 1
// version.Comparable.Major = 1
// version.Metadata = "build.123"
// version.FullVersion = "1.2.3-beta.1+build.123"
// version.CompareVersion = "1.2.3-beta.1"
```