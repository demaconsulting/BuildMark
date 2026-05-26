### PathHelpers

#### Verification Approach

`PathHelpers` is a pure logic unit with no dependencies to mock or stub. It is verified through
`PathHelpersTests.cs`, which contains 7 unit tests covering valid path combination, null argument
rejection, path-traversal prevention, absolute path rejection, and acceptance of valid dot-prefixed
names such as `"..data"`.

#### Test Environment

N/A - standard test environment. No external dependencies or environment setup required.

#### Acceptance Criteria

- All 7 tests in `PathHelpersTests.cs` pass with zero failures.

#### Test Scenarios

**PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly**: This scenario verifies that
`SafePathCombine` accepts a valid base path and relative path and returns
`Path.Combine(basePath, relativePath)`. This scenario is tested by
`PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly`.

**PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException**: This scenario verifies
that a `null` base path is rejected so callers receive a clear contract violation with
`ParamName` set to `"basePath"`. This scenario is tested by
`PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException`.

**PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException**: This scenario
verifies that a `null` relative path is rejected so callers receive a clear contract violation with
`ParamName` set to `"relativePath"`. This scenario is tested by
`PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException`.

**PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException**: This
scenario verifies that a traversal attempt such as `"../etc/passwd"` is rejected to prevent access
outside the intended base path. The expected outcome is an `ArgumentException` containing
`"Invalid path component"`. This scenario is tested by
`PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException`.

**PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException**: This scenario verifies
that traversal segments embedded within a longer relative path are also rejected. The expected
outcome is an `ArgumentException` containing `"Invalid path component"`. This scenario is tested by
`PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException`.

**PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException**: This scenario verifies that
an absolute path supplied as the relative argument is rejected so the caller cannot escape the base
path. The expected outcome is an `ArgumentException` containing `"Invalid path component"`. This
scenario is tested by `PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException`.

**PathHelpers_SafePathCombine_PathStartingWithDots_CombinesCorrectly**: This scenario verifies
that a valid dot-prefixed directory name such as `"..data/file.txt"` is accepted when it is not a
traversal component. The expected outcome is successful combination without an exception. This
scenario is tested by `PathHelpers_SafePathCombine_PathStartingWithDots_CombinesCorrectly`.
