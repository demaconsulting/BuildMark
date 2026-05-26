### SectionConfig

#### Verification Approach

`SectionConfig` is a data model unit with no external dependencies. It is verified through
`ConfigurationTests.cs` via `BuildMarkConfig.CreateDefault()`, which constructs a `SectionConfig`
collection and allows `Id` and `Title` properties to be asserted. No mocking is required.

#### Test Environment

N/A - standard test environment.

#### Acceptance Criteria

- All unit tests in `ConfigurationTests.cs` that inspect `SectionConfig` instances pass with zero
  failures.

#### Test Scenarios

**BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection**: `BuildMarkConfig.CreateDefault()`
is called and the returned `Sections` collection is inspected. Three sections must be present with
ids `"changes"`, `"bugs-fixed"`, and `"dependency-updates"` and their corresponding titles
`"Changes"`, `"Bugs Fixed"`, and `"Dependency Updates"`, confirming that all section id and title
fields are mapped correctly. This scenario is tested by
`BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection`.
