### SectionConfig

#### Verification Approach

`SectionConfig` is verified through `ConfigurationTests.cs`. The test
`BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection` calls
`BuildMarkConfig.CreateDefault()` and asserts on the `Id` and `Title` properties of the returned
sections. No mocking is required.

#### Dependencies

| Mock / Stub | Reason          |
| ----------- | --------------- |
| None        | No mocks needed |

#### Test Environment

Standard dotnet test host; no external dependencies or environment setup required.

#### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

#### Test Scenarios

##### BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection

**Scenario**: `BuildMarkConfig.CreateDefault()` is called; the returned `Sections` collection is
inspected.

**Expected**: Three sections are present with ids `"changes"`, `"bugs-fixed"`, and
`"dependency-updates"` and corresponding titles `"Changes"`, `"Bugs Fixed"`, and
`"Dependency Updates"`.

**Requirement coverage**: `BuildMark-SectionConfig-Properties`.

#### Requirements Coverage

- **`BuildMark-SectionConfig-Properties`**:
  - BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection
