# Version Subsystem Design

## Overview

The Version subsystem provides comprehensive semantic version processing capabilities for BuildMark.
It encapsulates all version-related functionality including parsing, comparison, validation, and version
range operations, ensuring consistent semantic versioning behavior across all BuildMark components.

## Architecture

The Version subsystem is composed of six units:

- `VersionComparable` (Unit) - core semantic version comparison and ordering engine
- `VersionSemantic` (Unit) - semantic version parsing and validation
- `VersionTag` (Unit) - repository version tag processing and extraction
- `VersionInterval` (Unit) - version range representation and operations
- `VersionIntervalSet` (Unit) - ordered collection of version intervals for range queries
- `VersionCommitTag` (Unit) - version-to-commit association for build information

## Version Type Hierarchy

```text
VersionTag (raw repository tag)
    ↓ (extraction)
VersionSemantic (parsed semantic version)
    ↓ (normalization)
VersionComparable (optimized comparison)
    ↓ (range operations)
VersionInterval / VersionIntervalSet (version ranges)
    ↓ (build association)
VersionCommitTag (version + commit hash)
```

## Design Principles

### Semantic Versioning Compliance

All version processing strictly adheres to Semantic Versioning 2.0.0 (<https://semver.org/>) specification
to ensure predictable and industry-standard behavior.

### Performance Optimization

Version comparison operations are optimized for high-frequency usage in repository processing
through construction-time parsing and cached segment arrays.

### Type Safety

Each version type serves a specific purpose with clear boundaries:

- **VersionTag**: Raw string from repository
- **VersionSemantic**: Validated SemVer structure
- **VersionComparable**: Optimized for comparison operations
- **VersionInterval**: Range queries and filtering
- **VersionCommitTag**: Build metadata association

## External Interfaces

| Interface        | Direction  | Protocol / Format                         |
|------------------|------------|-------------------------------------------|
| Repository Tags  | Input      | String tags from GitHub/Git repositories  |
| Version Parsing  | Processing | SemVer 2.0.0 compliant parsing            |
| Version Compare  | Processing | IComparable<T> standard interface         |
| Build Info       | Output     | VersionCommitTag records for build notes  |

## Integration Points

### Repository Connectors

Version subsystem processes raw repository tags from GitHub and other sources,
extracting semantic versions for build boundary determination.

### Build Notes

Provides VersionCommitTag associations that link semantic versions to specific commit hashes for build information generation.

### Configuration

Supports version range specifications in configuration files through VersionInterval processing.

## Error Handling

Version processing provides two parsing patterns:

- `TryCreate()` factory methods return null for invalid input, enabling safe parsing without exceptions
- `Create()` factory methods throw `ArgumentException` for invalid input, enabling fail-fast behavior
- Malformed version ranges are rejected during parsing
- Version comparison operations are guaranteed to be consistent and transitive

## Performance Characteristics

### Version Comparison

- **Construction**: O(n) where n is pre-release identifier count
- **Comparison**: O(min(a,b)) where a,b are pre-release segment counts
- **Memory**: Constant per-version overhead for parsed segments

### Version Range Operations

- **Interval Creation**: O(1) for single ranges
- **Set Operations**: O(n) where n is interval count
- **Contains Checks**: O(n) because the current implementation performs a linear scan
