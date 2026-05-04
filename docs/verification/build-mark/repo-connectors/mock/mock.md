# Mock

## Verification Approach

The Mock sub-subsystem is verified through `MockTests.cs` (3 subsystem-level tests)
and `MockRepoConnectorTests.cs` (11 unit tests). The subsystem tests confirm
integration of the mock connector within the broader pipeline. The unit tests are
described in the individual unit chapter.

## Dependencies

| Mock / Stub | Reason    |
| ----------- | --------- |
| None        | Pure mock |

## Test Scenarios (Subsystem-Level, MockTests.cs)

### Mock_ImplementsInterface_ReturnsTrue

**Scenario**: `MockRepoConnector` is checked against `IRepoConnector`.

**Expected**: Implements the interface.

**Requirement coverage**: `BuildMark-RepoConnectors-IRepoConnector`

### Mock_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation

**Scenario**: Mock connector is configured with direct data and `GetBuildInformationAsync`
is called.

**Expected**: Returns the configured `BuildInformation` directly.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

### Mock_GetBuildInformation_WithNoData_ReturnsEmptyBuildInformation

**Scenario**: Mock connector is not configured; `GetBuildInformationAsync` is called.

**Expected**: Returns an empty `BuildInformation`.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

## Requirements Coverage

- **BuildMark-RepoConnectors-IRepoConnector**: Mock_ImplementsInterface_ReturnsTrue
- **BuildMark-RepoConnectors-Mock**: Mock_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation,
  Mock_GetBuildInformation_WithNoData_ReturnsEmptyBuildInformation
