### TemporaryDirectory

#### Verification Approach

`TemporaryDirectory` uses the real file system, so no dependencies are mocked or stubbed. It is
verified through `TemporaryDirectoryTests.cs`, which contains 7 unit tests covering directory
creation, uniqueness, path resolution, intermediate-directory creation, traversal prevention,
disposal cleanup, and idempotent disposal when the directory has already been deleted.

#### Test Environment

N/A - standard dotnet test host. No external dependencies or environment setup are required. Tests
create and delete real directories on the host file system.

#### Acceptance Criteria

- All 7 tests in `TemporaryDirectoryTests.cs` pass with zero failures.

#### Test Scenarios

**TemporaryDirectory_Constructor_CreatesDirectory**: This scenario verifies that constructing a
`TemporaryDirectory` immediately creates a backing directory on disk. This matters because later
operations depend on `DirectoryPath` referring to an existing location. This scenario is tested by
`TemporaryDirectory_Constructor_CreatesDirectory`.

**TemporaryDirectory_Constructor_CreatesUniqueDirectories**: This scenario verifies that two
instances created in sequence receive distinct directory paths with no collision. This matters
because each test or caller must get an isolated workspace. This scenario is tested by
`TemporaryDirectory_Constructor_CreatesUniqueDirectories`.

**TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory**: This scenario verifies
that `GetFilePath("output.md")` returns a path rooted under `DirectoryPath` and ending with the
requested file name. This matters because callers rely on the helper to keep files inside the
temporary workspace. This scenario is tested by
`TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory`.

**TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories**: This scenario
verifies that `GetFilePath` accepts a nested relative path, returns the expected absolute path, and
creates the intermediate `sub/nested` directories on disk. This scenario is tested by
`TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories`.

**TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException**: This scenario
verifies that a traversal attempt such as `"../escaped.txt"` is rejected with an
`ArgumentException`. This matters because the helper must not allow file creation outside the
temporary directory. This scenario is tested by
`TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException`.

**TemporaryDirectory_Dispose_DeletesDirectory**: This scenario verifies that disposing the
instance removes the temporary directory and its contents from disk. This matters because the class
must clean up temporary artifacts reliably. This scenario is tested by
`TemporaryDirectory_Dispose_DeletesDirectory`.

**TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow**: This scenario verifies that `Dispose`
remains safe when the directory has already been deleted manually. This matters because cleanup code
should be idempotent and resilient to prior deletion. This scenario is tested by
`TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow`.
