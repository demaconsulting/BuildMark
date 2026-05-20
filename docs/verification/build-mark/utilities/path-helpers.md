### PathHelpers

#### Verification Approach

`PathHelpers` is a pure utility class. It is verified through dedicated unit tests in
`PathHelpersTests.cs`, which contains 7 tests covering valid path combination, null
argument rejection, path traversal prevention (double-dots), absolute path rejection,
and acceptance of valid dot-prefixed directory names such as `"..data"`.

#### Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Pure logic |

#### Test Environment

Standard dotnet test host; no external dependencies or environment setup required.

#### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

#### Test Scenarios

##### PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly

**Scenario**: `PathHelpers.SafePathCombine` is called with a valid base path and a
relative path.

**Expected**: Returns `Path.Combine(basePath, relativePath)`.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

##### PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException

**Scenario**: `PathHelpers.SafePathCombine` is called with a `null` base path.

**Expected**: `ArgumentNullException` is thrown with `ParamName` equal to `"basePath"`.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

##### PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException

**Scenario**: `PathHelpers.SafePathCombine` is called with a `null` relative path.

**Expected**: `ArgumentNullException` is thrown with `ParamName` equal to
`"relativePath"`.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

##### PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException

**Scenario**: `PathHelpers.SafePathCombine` is called with `"../etc/passwd"` as the
relative path.

**Expected**: `ArgumentException` is thrown with a message containing
`"Invalid path component"`.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

##### PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException

**Scenario**: `PathHelpers.SafePathCombine` is called with `"subfolder/../../../etc/passwd"`.

**Expected**: `ArgumentException` is thrown with a message containing
`"Invalid path component"`.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

##### PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException

**Scenario**: `PathHelpers.SafePathCombine` is called with an absolute path as the
relative argument.

**Expected**: `ArgumentException` is thrown with a message containing
`"Invalid path component"`.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

##### PathHelpers_SafePathCombine_PathStartingWithDots_CombinesCorrectly

**Scenario**: `PathHelpers.SafePathCombine` is called with `"..data/file.txt"` (a valid
directory name that begins with dots but is not a traversal component).

**Expected**: Returns `Path.Combine(basePath, "..data/file.txt")` without error.

**Requirement coverage**: `BuildMark-Utilities-PathHelpers`

#### Requirements Coverage

- **BuildMark-Utilities-PathHelpers**:
  PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly,
  PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException,
  PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException,
  PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException,
  PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException,
  PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException,
  PathHelpers_SafePathCombine_PathStartingWithDots_CombinesCorrectly
