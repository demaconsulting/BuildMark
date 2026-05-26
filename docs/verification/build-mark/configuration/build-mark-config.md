### BuildMarkConfig

#### Verification Approach

`BuildMarkConfig` is a data model unit with no external dependencies. It is verified with unit
tests in `ConfigurationTests.cs` that call `BuildMarkConfig.CreateDefault()` and assert on the
returned sections and routing rules. No mocking or file system access is required.

#### Test Environment

N/A - standard test environment.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` targeting `BuildMarkConfig` pass with zero failures.

#### Test Scenarios

**BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection**: `BuildMarkConfig.CreateDefault()`
is called and the returned configuration is inspected for correctness. Three sections with ids
`changes`, `bugs-fixed`, and `dependency-updates` and their corresponding titles must be present;
six routing rules must exist with correct routes, label conditions, work-item-type conditions, and
a null match on the final catch-all rule that routes to `changes`. This scenario is tested by
`BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection`.
