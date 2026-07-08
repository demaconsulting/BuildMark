### TemporaryDirectory

![Utilities Structure](../../generated/UtilitiesView.svg)

#### Purpose

`TemporaryDirectory` is an `internal sealed` class implementing `IDisposable` that creates a
uniquely-named directory under `Environment.CurrentDirectory` on construction and deletes it recursively
on disposal. Using `Environment.CurrentDirectory` as the base avoids the macOS `/tmp` → `/private/tmp`
symlink mismatch that can cause path-containment checks to fail when the real path and the requested path
differ.

#### Data Model

**`DirectoryPath`**: `string` — Absolute path to the temporary directory on disk. Set in the constructor
and immutable for the lifetime of the instance.

#### Key Methods

**`TemporaryDirectory`** (constructor): Generates a unique name by combining a fixed prefix with a GUID
string, creates the directory under `Environment.CurrentDirectory`, and stores the resulting path in
`DirectoryPath`.

- *Parameters*: None.
- *Returns*: A new `TemporaryDirectory` instance.
- *Preconditions*: `Environment.CurrentDirectory` must be accessible.
- *Postconditions*: `DirectoryPath` is set to the path of the newly created directory.

Uses `PathHelpers.SafePathCombine` to build the path, then calls `Directory.CreateDirectory`. Throws
`InvalidOperationException` wrapping the original exception if directory creation fails due to an
`IOException`, `UnauthorizedAccessException`, or `ArgumentException`.

**`GetFilePath`**: Returns the absolute path to a file inside the temporary directory identified by
`relativePath`.

- *Parameters*: `string relativePath` — a relative path (file name or subdirectory/file) within the
  temporary directory; must not be `null`.
- *Returns*: `string` — the combined absolute path within the temporary directory.
- *Preconditions*: `relativePath` is non-null and does not navigate above the temporary directory.
- *Postconditions*: All intermediate directories between `DirectoryPath` and the resolved path exist.

Delegates to `PathHelpers.SafePathCombine(DirectoryPath, relativePath)` to produce a validated absolute
path, then calls `Directory.CreateDirectory` on the parent directory so that callers can write the file
immediately.

**`Dispose`**: Deletes the temporary directory and all its contents.

- *Parameters*: None.
- *Returns*: `void`
- *Preconditions*: None — may be called even if the directory has already been deleted.
- *Postconditions*: The directory is deleted if it existed; cleanup errors are suppressed.

Calls `Directory.Delete(DirectoryPath, recursive: true)`. Both `IOException` and
`UnauthorizedAccessException` are caught and silently discarded so that callers in `using` statements are
not disrupted when the directory has already been removed or when a file lock prevents deletion.

#### Error Handling

Constructor throws `InvalidOperationException` (wrapping the original exception) if directory creation
fails due to an `IOException`, `UnauthorizedAccessException`, or `ArgumentException`. `GetFilePath`
propagates `ArgumentException` from `PathHelpers.SafePathCombine` when `relativePath` would escape the
temporary directory boundary. `Dispose` silently suppresses `IOException`, `UnauthorizedAccessException`,
and `DirectoryNotFoundException`; cleanup failures are non-fatal.

#### Dependencies

- **`PathHelpers`** — `SafePathCombine` is called by the constructor and `GetFilePath` to validate and
  resolve all paths within the temporary directory boundary (Utilities subsystem)

#### Callers

- **`Validation`** — creates a `TemporaryDirectory` instance per test run to isolate log and report files
  (SelfTest subsystem)
