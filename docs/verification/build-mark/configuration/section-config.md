# SectionConfig

## Verification Approach

`SectionConfig` is a data model class verified indirectly through
`RepoConnectorsTests.cs` and connector-specific tests. When `Configure` is called
on a connector with `SectionConfig` entries, the connector creates named sections in
the `BuildInformation.RoutedSections` output. Tests that assert on routed sections
exercise `SectionConfig` indirectly.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios (Integration)

### RepoConnectors_GitHubConnector_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: A connector configured with `SectionConfig` entries processes items and
populates `RoutedSections`.

**Expected**: Routed sections are present in the result as configured.

**Requirement coverage**: `BuildMark-Configuration-SectionConfig`

### MockRepoConnector_GetBuildInformationAsync_WithRules_ReturnsRoutedSections

**Scenario**: `MockRepoConnector` is configured with sections; `GetBuildInformationAsync`
populates routed sections.

**Expected**: `RoutedSections` contains the configured section names.

**Requirement coverage**: `BuildMark-Configuration-SectionConfig`

## Requirements Coverage

- **BuildMark-Configuration-SectionConfig**:
  RepoConnectors_GitHubConnector_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation,
  MockRepoConnector_GetBuildInformationAsync_WithRules_ReturnsRoutedSections
