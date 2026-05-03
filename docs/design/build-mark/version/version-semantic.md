# Version Semantic

## Purpose

The `VersionSemantic` record type extends `VersionComparable` with semantic version metadata
support. As a C# `record`, it provides structural equality by default - two `VersionSemantic`
instances are equal when all their properties compare equal. It provides the full semantic
version structure including build metadata while preserving comparison functionality.

## Structure

| Property | Type | Description |
| -------- | ---- | ----------- |
| Comparable | VersionComparable | Core comparison logic and version components |
| Metadata | string? | Build metadata (+metadata), or `null` when absent |
| FullVersion | string | Complete version string (major.minor.patch\[-pre-release\]\[+metadata\]) |

## Delegated Properties

For convenience, the following properties delegate to the `Comparable` instance:

- `Major`, `Minor`, `Patch` - Version number components
- `Numbers` - Core semantic numbers (major.minor.patch)
- `PreRelease` - Pre-release identifier (`null` when no pre-release)
- `IsPreRelease` - Whether this is a pre-release version
- `CompareVersion` - Comparison string (excludes metadata)

## Comparison

Comparison operations are performed on the `Comparable` instance, following
semantic version rules where build metadata does not affect precedence.

## Factory Methods

- `Create(string version)` - Creates instance, throws on invalid input
- `TryCreate(string version)` - Returns null for invalid input

### TryCreate Parsing Algorithm

1. Return `null` if the input is null or whitespace.
2. Split on `+` using `Split('+', 2)` to separate the version string from optional build metadata.
3. Normalize empty metadata to `null` (metadata is `null`, not an empty string, when absent).
4. Delegate the version part to `VersionComparable.TryCreate`; return `null` if it returns `null`.
5. Return a new `VersionSemantic(comparable, metadata)`.

## Example

```csharp
var version = VersionSemantic.Create("1.2.3-beta.1+build.123");
// version.Major = 1
// version.Comparable.Major = 1
// version.Metadata = "build.123"
// version.FullVersion = "1.2.3-beta.1+build.123"
// version.CompareVersion = "1.2.3-beta.1"
```
