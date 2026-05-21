### RuleConfig

#### Verification Approach

`RuleConfig` is verified through `ConfigurationTests.cs`. The test
`BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection` calls
`BuildMarkConfig.CreateDefault()` and asserts on the `Route` and `Match` properties of the
returned rules. No mocking is required.

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

**Scenario**: `BuildMarkConfig.CreateDefault()` is called; the returned `Rules` collection is
inspected.

**Expected**: Six rules are present; the first three rules have `dependency-updates`,
`bugs-fixed`, and `bugs-fixed` as routes with appropriate label and work-item-type match
conditions; the fourth and fifth rules have `suppressed` routes; the sixth rule has `changes` as
route with a null match.

**Requirement coverage**: `BuildMark-RuleConfig-Properties`.

#### Requirements Coverage

- **`BuildMark-RuleConfig-Properties`**:
  - BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection
