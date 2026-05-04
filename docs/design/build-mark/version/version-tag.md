# Version Tag

## Purpose

The `VersionTag` class parses repository tags to extract semantic version information.
It handles various tag formats and prefixes while providing access to both the
original tag and parsed semantic version. **Critically, VersionTag instances are
compared based on their semantic version content (VersionComparable), not their
tag strings, enabling version equality across different tag formats.**

## Structure

| Property | Type            | Description                  |
|----------|-----------------|------------------------------|
| Tag      | string          | Original repository tag      |
| Semantic | VersionSemantic | Parsed semantic version info |

## Delegated Properties

For convenience, the following properties delegate to the `Semantic.Comparable` instance:

- `Major`, `Minor`, `Patch` - Version number components  
- `PreRelease` - Pre-release identifier
- `CompareVersion` - Comparison string
- `Numbers` - Version numbers only (major.minor.patch)

Additional delegated properties from `Semantic`:

- `Metadata` - Build metadata
- `FullVersion` - Complete semantic version string

## Tag Format Support

The parser supports various repository tag formats:

- Simple: `1.2.3`, `v1.2.3`, `ver1.2.3`
- Complex prefixes: `Release_1.2.3`, `MyApp-v1.2.3`
- Path-separator prefixes: `release/1.2.3`, `builds/release/1.2.3`
- Pre-release: `1.2.3-beta`, `1.2.3.rc.1` (dot becomes hyphen)
- Metadata: `1.2.3+build.123`

## Version Equality

**Important Design Principle**: VersionTag instances with different tag strings but
identical semantic versions are considered equal. This enables repository connectors
to correctly identify versions regardless of tagging conventions:

- `"v1.2.3"` equals `"VER1.2.3"` equals `"Release-1.2.3"` equals `"release/1.2.3"`
- Equality is determined by `Semantic.Comparable.Equals()`
- FindVersionIndex in RepoConnectorBase uses this semantic comparison

This design prevents version matching failures when repositories use different
tag naming conventions but identical semantic versions.

## Factory Methods

- `Create(string tag)` - Creates instance, throws on invalid tag
- `TryCreate(string tag)` - Returns null for invalid tag

## Display / ToString

The `ToString()` method is overridden to return the original `Tag` string verbatim.
This preserves the repository tag format for display and logging purposes while keeping semantic comparison
available through `Semantic.Comparable`:

```csharp
var versionTag = VersionTag.Create("release/1.2.3-rc.1");
Console.WriteLine(versionTag);             // "release/1.2.3-rc.1"
Console.WriteLine(versionTag.FullVersion); // "1.2.3-rc.1"
```

## Example

```csharp
var versionTag = VersionTag.Create("MyApp-v1.2.3-beta.1+build.123");
// versionTag.Tag = "MyApp-v1.2.3-beta.1+build.123"
// versionTag.Numbers = "1.2.3"
// versionTag.FullVersion = "1.2.3-beta.1+build.123"
// versionTag.CompareVersion = "1.2.3-beta.1"

// Version equality example
var tag1 = VersionTag.Create("v1.2.3");
var tag2 = VersionTag.Create("VER1.2.3");
// tag1.Semantic.Comparable.Equals(tag2.Semantic.Comparable) == true
```
