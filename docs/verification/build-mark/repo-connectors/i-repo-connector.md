### IRepoConnector

#### Verification Approach

`IRepoConnector` is an interface with no dedicated test class. Its contract is verified through all
tests that exercise concrete implementations: `GitHubRepoConnector`, `AzureDevOpsRepoConnector`, and
`MockRepoConnector`. Each implementation is checked against the interface via cast or type-assertion
tests that confirm the concrete class implements `IRepoConnector`.

#### Dependencies

| Mock / Stub | Reason |
| --- | --- |
| None | Interface |

#### Test Environment

Standard dotnet test host; no external dependencies or environment setup required.

#### Acceptance Criteria

All interface-implementation tests across `RepoConnectorsTests.cs`, `GitHubTests.cs`,
`AzureDevOpsTests.cs`, `MockTests.cs`, `GitHubRepoConnectorTests.cs`,
`AzureDevOpsRepoConnectorTests.cs`, and `MockRepoConnectorTests.cs` that verify
`IRepoConnector` pass with no errors or warnings.

#### Test Scenarios

##### RepoConnectors_GitHubConnector_ImplementsInterface_ReturnsTrue

**Scenario**: `GitHubRepoConnector` instance is checked for `IRepoConnector` implementation.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHub`

##### RepoConnectors_MockConnector_ImplementsInterface_ReturnsTrue

**Scenario**: `MockRepoConnector` instance is checked for `IRepoConnector` implementation.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-Mock`

##### RepoConnectors_AzureDevOps_ImplementsInterface_ReturnsTrue

**Scenario**: `AzureDevOpsRepoConnector` instance is checked for `IRepoConnector` implementation.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-AzureDevOps`

##### GitHubRepoConnector_ImplementsInterface_ReturnsTrue

**Scenario**: `GitHubRepoConnector` type check against `IRepoConnector`.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-GitHub-BuildInformation`, `BuildMark-RepoConnectorBase-Interface`

##### MockRepoConnector_ImplementsInterface

**Scenario**: `MockRepoConnector` type check against `IRepoConnector`.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-MockRepoConnector-Deterministic`, `BuildMark-RepoConnectorBase-Interface`

##### AzureDevOpsRepoConnector_ImplementsInterface_ReturnsTrue

**Scenario**: `AzureDevOpsRepoConnector` type check against `IRepoConnector`.

**Expected**: Returns `true`.

**Requirement coverage**: `BuildMark-AzureDevOps-BuildInformation`
