### PathHelpers

![Utilities Structure](../../generated/UtilitiesView.svg)

#### Purpose

`PathHelpers` is a static utility class that provides a safe path-combination method. It protects callers
against path-traversal attacks by verifying the resolved combined path stays within the base directory.
Note that `Path.GetFullPath` normalizes `.`/`..` segments but does not resolve symlinks or reparse points,
so this check guards against string-level traversal only.

#### Data Model

N/A — `PathHelpers` is a static utility class with no instance state.

#### Key Methods

**`SafePathCombine`**: Combines `basePath` and `relativePath` safely, ensuring the resulting path remains
within the base directory.

- *Parameters*: `string basePath` — the base directory to resolve within; `string relativePath` — the
  path component to combine.
- *Returns*: `string` — the combined path if it remains within the base directory.
- *Preconditions*: Both arguments must be non-null; `relativePath` must not navigate above the base
  directory.
- *Postconditions*: The returned path is contained within `basePath`.

Combines the paths with `Path.Combine`, resolves both `basePath` and the candidate to absolute form with
`Path.GetFullPath`, then computes `Path.GetRelativePath(absoluteBase, absoluteCombined)` and rejects the
input if the result is exactly `".."`, starts with `".."` followed by a directory-separator character, or
is itself rooted (absolute). Using `GetRelativePath` for the containment check handles root paths,
platform case-sensitivity, and directory-separator normalization natively. The containment test treats
`..` as an escaping segment only when it is the entire relative result or is followed by a directory
separator, avoiding false positives for valid in-base names such as `..data`.

#### Error Handling

`SafePathCombine` throws `ArgumentNullException` when either argument is `null` and `ArgumentException`
(identifying `relativePath` as the problematic parameter) when the resolved combined path escapes the base
directory. `Path.GetFullPath` may additionally propagate `NotSupportedException` or `PathTooLongException`
for malformed paths.

#### Dependencies

N/A — `PathHelpers` has no dependencies on other BuildMark units or subsystems.

#### Callers

- **`TemporaryDirectory`** — calls `SafePathCombine` in the constructor and in `GetFilePath` to validate
  all paths within the temporary directory boundary
