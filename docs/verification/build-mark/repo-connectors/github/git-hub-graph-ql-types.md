# GitHubGraphQLTypes

## Verification Approach

`GitHubGraphQLTypes` contains the data transfer object (DTO) types used to deserialize
GitHub GraphQL API responses. These types have no dedicated test class; they are
verified indirectly through all `GitHubGraphQLClient*Tests.cs` tests that exercise
JSON deserialization of mocked API responses.

## Dependencies

| Mock / Stub              | Reason                                                       |
| ------------------------ | ------------------------------------------------------------ |
| `MockHttpMessageHandler` | Provides JSON payloads whose structure matches the DTO types |

## Test Scenarios (via GitHubGraphQLClient*Tests.cs)

### GitHubGraphQLClient_GetAllIssuesAsync_ValidResponse_ReturnsIssuesWithBody

**Scenario**: GraphQL response for issues is deserialized into issue DTOs.

**Expected**: Issue DTOs contain the expected fields including `body`.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLTypes`

### GitHubGraphQLClient_GetPullRequestsAsync_ValidResponse_ReturnsPullRequestsWithBody

**Scenario**: GraphQL response for pull requests is deserialized into pull request DTOs.

**Expected**: Pull request DTOs contain the expected fields including `body`.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLTypes`

### GitHubGraphQLClient_GetAllTagsAsync_ValidResponse_ReturnsTagNodes

**Scenario**: GraphQL response for tags is deserialized into tag node DTOs.

**Expected**: Tag node DTOs contain `name` and target commit hash fields.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLTypes`

## Requirements Coverage

- **BuildMark-RepoConnectors-GitHubGraphQLTypes**: Verified indirectly through all
  41 tests in the `GitHubGraphQLClient*Tests.cs` files
