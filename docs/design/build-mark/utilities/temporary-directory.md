### TemporaryDirectory Design

#### Purpose

`TemporaryDirectory` is an `internal sealed` class implementing `IDisposable` that
creates a uniquely-named directory under `Environment.CurrentDirectory` on construction
and deletes it recursively on disposal. Using `Environment.CurrentDirectory` as the base
avoids the macOS `/tmp` → `/private/tmp` symlink mismatch that can cause path-containment
checks to fail when the real path and the requested path differ.

#### Data Model

`TemporaryDirectory` is instance-based and owns a single directory for its lifetime:

| Property        | Type     | Description                                             |
| --------------- | -------- | ------------------------------------------------------- |
| `DirectoryPath` | `string` | Absolute path to the temporary directory on disk        |

#### Key Methods

##### Constructor

```csharp
internal TemporaryDirectory()
```

Generates a unique name by combining a fixed prefix with a GUID string, creates the
directory under `Environment.CurrentDirectory`, and stores the resulting path in
`DirectoryPath`. Throws `InvalidOperationException` wrapping the original exception if
directory creation fails due to an I/O error.

##### GetFilePath Method

```csharp
internal string GetFilePath(string relativePath)
```

Returns the absolute path to a file inside the temporary directory identified by
`relativePath`.

**Steps:**

1. Delegates to `PathHelpers.SafePathCombine(DirectoryPath, relativePath)` to produce a
   validated absolute path. `SafePathCombine` rejects traversal sequences (`..`) and
   absolute-path overrides, throwing `ArgumentException` on invalid input.
2. Creates all intermediate directories between `DirectoryPath` and the resolved file path
   using `Directory.CreateDirectory` so that callers can write the file immediately.
3. Returns the validated path string.

##### Dispose Method

```csharp
public void Dispose()
```

Deletes the temporary directory and all its contents by calling
`Directory.Delete(DirectoryPath, recursive: true)`. Both `IOException` and
`UnauthorizedAccessException` are caught and silently discarded so that callers in
`using` statements are not disrupted when the directory has already been removed or
when a file lock prevents deletion.

#### Error Handling

| Situation                                  | Behavior                                           |
| ------------------------------------------ | -------------------------------------------------- |
| Directory creation fails (I/O error)       | Throws `InvalidOperationException` (wraps original)|
| Traversal or absolute path in GetFilePath  | Throws `ArgumentException` (from `SafePathCombine`)|
| Directory already deleted before Dispose   | Silently suppressed                                |
| File lock prevents deletion in Dispose     | Silently suppressed                                |

#### Interactions

- `PathHelpers.SafePathCombine` (Utilities subsystem) is called by `GetFilePath` to
  validate and resolve the caller-supplied relative path before creating intermediate
  directories.
