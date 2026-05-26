### WebLink

#### Verification Approach

`WebLink` is a record type with no logic. It is verified through a dedicated test in
`BuildInformationTests.cs` that constructs a `WebLink` directly and asserts on its properties.
No mocking is required.

#### Test Environment

N/A - standard test environment. `BuildInformationTests.cs` runs within the standard `dotnet test`
host; no external dependencies or environment setup are required.

#### Acceptance Criteria

- All `WebLink`-related tests in `BuildInformationTests.cs` pass with zero failures.
- Both `LinkText` and `TargetUrl` properties are verified.

#### Test Scenarios

**WebLink_Constructor_StoresTextAndUrl**: Verifies that a `WebLink` constructed with display text
`"v1.0.0...v2.0.0"` and a GitHub compare URL stores the values in `LinkText` and `TargetUrl`
properties respectively. This scenario is tested by `WebLink_Constructor_StoresTextAndUrl`.
