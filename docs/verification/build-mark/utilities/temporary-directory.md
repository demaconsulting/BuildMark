### TemporaryDirectory

#### Verification Approach

`TemporaryDirectory` is verified through dedicated unit tests in
`TemporaryDirectoryTests.cs`, which contains 7 tests covering directory creation,
uniqueness, path resolution, intermediate-directory creation, traversal prevention,
disposal cleanup, and idempotent disposal when the directory has already been deleted.

#### Dependencies

| Mock / Stub | Reason                                                     |
| ----------- | ---------------------------------------------------------- |
| None        | Tests use the real file system under a temporary directory |

#### Test Environment

Standard dotnet test host; no external dependencies or environment setup required.
Tests create and delete real directories on the host file system.

#### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

#### Test Scenarios

##### TemporaryDirectory_Constructor_CreatesDirectory

**Scenario**: A `TemporaryDirectory` instance is constructed.

**Expected**: `DirectoryPath` refers to a directory that exists on disk.

**Requirement coverage**: `BuildMark-TemporaryDirectory-Creation`

##### TemporaryDirectory_Constructor_CreatesUniqueDirectories

**Scenario**: Two `TemporaryDirectory` instances are constructed in sequence.

**Expected**: Each instance has a distinct `DirectoryPath`; neither directory collides
with the other.

**Requirement coverage**: `BuildMark-TemporaryDirectory-Creation`

##### TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory

**Scenario**: `GetFilePath` is called with a simple filename such as `"file.txt"`.

**Expected**: Returns a path that starts with `DirectoryPath` and ends with the
supplied filename.

**Requirement coverage**: `BuildMark-TemporaryDirectory-FilePath`

##### TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories

**Scenario**: `GetFilePath` is called with a nested relative path such as
`"sub/dir/file.txt"`.

**Expected**: Returns the expected absolute path and the intermediate directory
`sub/dir` is created on disk.

**Requirement coverage**: `BuildMark-TemporaryDirectory-FilePath`

##### TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException

**Scenario**: `GetFilePath` is called with a traversal path such as
`"../outside.txt"`.

**Expected**: `ArgumentException` is thrown; no file is created outside the
temporary directory.

**Requirement coverage**: `BuildMark-TemporaryDirectory-Traversal`

##### TemporaryDirectory_Dispose_DeletesDirectory

**Scenario**: A `TemporaryDirectory` instance is disposed.

**Expected**: The directory referred to by `DirectoryPath` no longer exists on disk
after `Dispose` returns.

**Requirement coverage**: `BuildMark-TemporaryDirectory-Cleanup`

##### TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow

**Scenario**: The temporary directory is manually deleted before `Dispose` is called.

**Expected**: `Dispose` completes without throwing any exception.

**Requirement coverage**: `BuildMark-TemporaryDirectory-Cleanup`

#### Requirements Coverage

- **BuildMark-TemporaryDirectory-Creation**:
  TemporaryDirectory_Constructor_CreatesDirectory,
  TemporaryDirectory_Constructor_CreatesUniqueDirectories
- **BuildMark-TemporaryDirectory-FilePath**:
  TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory,
  TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories
- **BuildMark-TemporaryDirectory-Traversal**:
  TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException
- **BuildMark-TemporaryDirectory-Cleanup**:
  TemporaryDirectory_Dispose_DeletesDirectory,
  TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow
