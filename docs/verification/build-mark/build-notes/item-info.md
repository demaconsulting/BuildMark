### ItemInfo

#### Verification Approach

`ItemInfo` is a record type that carries no logic of its own. It is verified through
`BuildInformationTests.cs`, which asserts on the ordering, identity, and link properties of
`ItemInfo` entries returned by `MockRepoConnector`. `MockRepoConnector` provides `BuildInformation`
instances with known `ItemInfo` entries; no other mocking is needed.

#### Test Environment

N/A - standard test environment. `BuildInformationTests.cs` runs within the standard `dotnet test`
host; no external dependencies or environment setup beyond a `MockRepoConnector` instance are
required.

#### Acceptance Criteria

- All `ItemInfo`-related tests in `BuildInformationTests.cs` pass with zero failures.
- Both ordering and rendered output of `ItemInfo` entries are covered.

#### Test Scenarios

**BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex**: Verifies that `ItemInfo`
entries in the `Changes` collection for `ver-1.1.0` are ordered by `Index` in ascending order,
with the first entry having `Index` 10 and `Id` `"1"` and the second having `Index` 13.
This scenario is tested by `BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex`.

**BuildInformation_ToMarkdown_UsesBulletLists**: Verifies that each `ItemInfo` entry is rendered
as a `- [id](url)` bullet in the Changes, Bugs Fixed, and Known Issues sections, with no
table-row format present.
This scenario is tested by `BuildInformation_ToMarkdown_UsesBulletLists`.
