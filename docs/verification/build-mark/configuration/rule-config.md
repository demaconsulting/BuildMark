### RuleConfig

#### Verification Approach

`RuleConfig` is a data model unit with no external dependencies. It is verified through
`ConfigurationTests.cs` via `BuildMarkConfig.CreateDefault()`, which constructs a `RuleConfig`
collection and allows `Route` and `Match` properties to be asserted. No mocking is required.

#### Test Environment

N/A - standard test environment.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` that inspect `RuleConfig` instances pass with zero
  failures.

#### Test Scenarios

**BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection**: `BuildMarkConfig.CreateDefault()`
is called and the returned `Rules` collection is inspected. Six rules must be present; the first
three rules must have routes `dependency-updates`, `bugs-fixed`, and `bugs-fixed` with appropriate
label and work-item-type match conditions; the fourth and fifth rules must have `suppressed` routes;
the sixth rule must have route `changes` with a null `Match`, confirming the catch-all rule. This
scenario is tested by `BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection`.
