### WebLink

#### Verification Approach

`WebLink` is a record type with no logic. It is verified through a dedicated test in
`BuildInformationTests.cs` that constructs a `WebLink` directly and asserts on its properties.
No mocking is required.

#### Dependencies

| Mock / Stub | Reason          |
| ----------- | --------------- |
| None        | No mocks needed |

#### Test Scenarios

##### WebLink_Constructor_StoresTextAndUrl

**Scenario**: A `WebLink` is constructed with display text `"v1.0.0...v2.0.0"` and a GitHub
compare URL.

**Expected**: `LinkText` equals `"v1.0.0...v2.0.0"`; `TargetUrl` equals the supplied URL.

**Requirement coverage**: `BuildMark-WebLink-Record`.

#### Requirements Coverage

- **`BuildMark-WebLink-Record`**:
  - WebLink_Constructor_StoresTextAndUrl
