# PathHelpers

## Overview

`PathHelpers` is the sole unit in the Utilities subsystem. It provides a single
static method, `SafePathCombine`, that combines a base path with a relative path
while defending against directory traversal attacks.

The class is designed for use in contexts where user-supplied paths must be
restricted to a known base directory.

## Data Model

`PathHelpers` has no instance state. All functionality is exposed through a single
static method.

## Methods

### `SafePathCombine(string basePath, string relativePath) → string`

Combines `basePath` and `relativePath` into a single absolute path, validating
that the result remains within `basePath`.

**Preconditions (each violation throws an exception):**

| Check                        | Exception               | Message                          |
|------------------------------|-------------------------|----------------------------------|
| `basePath` is null           | `ArgumentNullException` | `paramName: "basePath"`          |
| `relativePath` is null       | `ArgumentNullException` | `paramName: "relativePath"`      |
| `relativePath` contains `..` | `ArgumentException`     | Path traversal detected          |
| `relativePath` is rooted     | `ArgumentException`     | Absolute paths are not permitted |
| Result is outside `basePath` | `ArgumentException`     | Path traversal detected          |

**Algorithm:**

1. Validate `basePath` and `relativePath` for null.
2. Reject `relativePath` values containing `..` (fast-path traversal block).
3. Reject `relativePath` values where `Path.IsPathRooted` returns `true`.
4. Combine the paths using `Path.GetFullPath(Path.Combine(basePath, relativePath))`.
5. Compute `Path.GetRelativePath(basePath, combined)`.
6. If the relative result starts with `..`, throw `ArgumentException`.
7. Return the combined full path.

## Interactions

`PathHelpers` has no dependencies on other BuildMark units or subsystems.
