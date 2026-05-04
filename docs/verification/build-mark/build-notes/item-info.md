# ItemInfo

## Verification Approach

`ItemInfo` is a record type that carries no logic of its own. It is verified through
`BuildInformationTests.cs`, which asserts on the ordering, identity, and link properties of
`ItemInfo` entries returned by `MockRepoConnector`. No mocking beyond `MockRepoConnector` is
needed.

## Dependencies

| Mock / Stub         | Reason                                                               |
| ------------------- | -------------------------------------------------------------------- |
| `MockRepoConnector` | Provides `BuildInformation` instances with known `ItemInfo` entries. |

## Test Scenarios

### BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex

**Scenario**: `MockRepoConnector.GetBuildInformationAsync(VersionTag.Create("ver-1.1.0"))` is
called; the `Changes` collection is inspected.

**Expected**: `ItemInfo` entries in `Changes` are ordered by `Index` in ascending order; the first
entry has `Index` 10 and `Id` `"1"`; the second has `Index` 13.

**Requirement coverage**: `BuildMark-ItemInfo-Record`.

### BuildInformation_ToMarkdown_UsesBulletLists

**Scenario**: `ToMarkdown(includeKnownIssues: true)` is called on `BuildInformation` for `v2.0.0`;
the rendered bullet list items are inspected.

**Expected**: Each `ItemInfo` entry is rendered as a `- [id](url)` bullet; no table-row format is
present.

**Requirement coverage**: `BuildMark-ItemInfo-Record`.

## Requirements Coverage

- **`BuildMark-ItemInfo-Record`**:
  - BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex
  - BuildInformation_ToMarkdown_UsesBulletLists
