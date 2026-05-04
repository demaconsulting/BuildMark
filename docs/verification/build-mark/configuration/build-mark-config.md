# BuildMarkConfig

## Verification Approach

`BuildMarkConfig` is verified with dedicated unit tests in `ConfigurationTests.cs`. The test
`BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection` calls
`BuildMarkConfig.CreateDefault()` and asserts on the returned sections and routing rules. No
mocking is required.

## Dependencies

| Mock / Stub | Reason          |
| ----------- | --------------- |
| None        | No mocks needed |

## Test Scenarios

### BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection

**Scenario**: `BuildMarkConfig.CreateDefault()` is called.

**Expected**: The result contains 3 sections (`changes`, `bugs-fixed`, `dependency-updates`) with
correct titles; 6 rules are present with correct routes and match conditions; the final catch-all
rule has a null match.

**Requirement coverage**: `BuildMark-SectionConfig-Properties`,
`BuildMark-RuleConfig-Properties`.

## Requirements Coverage

- **`BuildMark-SectionConfig-Properties`**:
  - BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection
- **`BuildMark-RuleConfig-Properties`**:
  - BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection
