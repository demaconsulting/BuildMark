#### GitHubGraphQLClient

##### Verification Approach

`GitHubGraphQLClient` is tested through five dedicated test files, each covering one
query method. All tests use `MockHttpMessageHandler` to intercept HTTP requests and
return controlled JSON responses. Tests cover successful responses, empty data,
missing required properties, HTTP errors, invalid JSON, single-item responses, and
pagination.

| Test File                                    | Method Tested                          | Test Count |
| -------------------------------------------- | -------------------------------------- | ---------- |
| `GitHubGraphQLClientFindIssueIdsTests.cs`    | `FindIssueIdsLinkedToPullRequestAsync` | 8          |
| `GitHubGraphQLClientGetAllIssuesTests.cs`    | `GetAllIssuesAsync`                    | 8          |
| `GitHubGraphQLClientGetAllTagsTests.cs`      | `GetAllTagsAsync`                      | 8          |
| `GitHubGraphQLClientGetCommitsTests.cs`      | `GetCommitsAsync`                      | 8          |
| `GitHubGraphQLClientGetPullRequestsTests.cs` | `GetPullRequestsAsync`                 | 9          |
| `GitHubGraphQLClientGetReleasesTests.cs`     | `GetReleasesAsync`                     | 8          |

##### Dependencies

| Mock / Stub              | Reason                                                  |
| ------------------------ | ------------------------------------------------------- |
| `MockHttpMessageHandler` | Intercepts HTTP calls; returns controlled JSON payloads |

##### Test Scenarios

###### GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_ValidResponse_ReturnsIssueIds

**Scenario**: Valid GraphQL response is returned for a linked-issues query.

**Expected**: Returns the list of issue IDs extracted from the response.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_NoIssues_ReturnsEmptyList

**Scenario**: Response contains no linked issues.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_MissingData_ReturnsEmptyList

**Scenario**: Response JSON is missing the data structure.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_HttpError_ReturnsEmptyList

**Scenario**: HTTP request returns a non-success status code.

**Expected**: Returns an empty list without throwing.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_InvalidJson_ReturnsEmptyList

**Scenario**: Response body is not valid JSON.

**Expected**: Returns an empty list without throwing.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_SingleIssue_ReturnsOneIssueId

**Scenario**: Response contains exactly one linked issue.

**Expected**: Returns a list with one issue ID.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_MissingNumberProperty_SkipsInvalidNodes

**Scenario**: One node in the response is missing the `number` property.

**Expected**: Invalid node is skipped; valid nodes are returned.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_WithPagination_ReturnsAllIssues

**Scenario**: Response uses pagination (multiple pages of linked issues).

**Expected**: All pages are fetched and all issue IDs are returned.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllIssuesAsync_ValidResponse_ReturnsIssues

**Scenario**: Valid response for GetAllIssues query.

**Expected**: Returns all issues from the response.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllIssuesAsync_NoIssues_ReturnsEmptyList

**Scenario**: Response contains no issues.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllIssuesAsync_MissingData_ReturnsEmptyList

**Scenario**: Response is missing the data structure.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllIssuesAsync_NullNodes_ReturnsEmptyList

**Scenario**: Response has null nodes array.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllIssuesAsync_InvalidIssues_FiltersThemOut

**Scenario**: Response contains some invalid issue objects.

**Expected**: Invalid issues are filtered out; valid issues are returned.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllIssuesAsync_WithPagination_ReturnsAllIssues

**Scenario**: Issues span multiple pages.

**Expected**: All pages are fetched and all issues are returned.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllIssuesAsync_Exception_ReturnsEmptyList

**Scenario**: An exception is thrown during the HTTP request.

**Expected**: Returns an empty list without re-throwing.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllIssuesAsync_ValidResponse_ReturnsIssuesWithBody

**Scenario**: Valid response includes issues with a body field.

**Expected**: Returned issues include the body content.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllTagsAsync_ValidResponse_ReturnsTagNodes

**Scenario**: Valid response for GetAllTags query.

**Expected**: Returns all tag nodes from the response.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllTagsAsync_NoTags_ReturnsEmptyList

**Scenario**: Response contains no tags.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllTagsAsync_MissingData_ReturnsEmptyList

**Scenario**: Response is missing the data structure.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllTagsAsync_HttpError_ReturnsEmptyList

**Scenario**: HTTP request returns a non-success status code.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllTagsAsync_InvalidJson_ReturnsEmptyList

**Scenario**: Response body is not valid JSON.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllTagsAsync_SingleTag_ReturnsOneTagNode

**Scenario**: Response contains exactly one tag.

**Expected**: Returns a list with one tag node.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllTagsAsync_MissingNameProperty_SkipsInvalidNodes

**Scenario**: One tag node is missing the `name` property.

**Expected**: Invalid node is skipped.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetAllTagsAsync_WithPagination_ReturnsAllTags

**Scenario**: Tags span multiple pages.

**Expected**: All pages are fetched and all tags are returned.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetCommitsAsync_ValidResponse_ReturnsCommitShas

**Scenario**: Valid response for GetCommits query.

**Expected**: Returns all commit SHAs.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetCommitsAsync_NoCommits_ReturnsEmptyList

**Scenario**: Response contains no commits.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetCommitsAsync_MissingData_ReturnsEmptyList

**Scenario**: Response is missing the data structure.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetCommitsAsync_HttpError_ReturnsEmptyList

**Scenario**: HTTP error response received.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetCommitsAsync_InvalidJson_ReturnsEmptyList

**Scenario**: Invalid JSON in response body.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetCommitsAsync_SingleCommit_ReturnsOneCommitSha

**Scenario**: Response contains exactly one commit.

**Expected**: Returns a list with one commit SHA.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetCommitsAsync_MissingOidProperty_SkipsInvalidNodes

**Scenario**: One commit node is missing the `oid` property.

**Expected**: Invalid node is skipped.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetCommitsAsync_WithPagination_ReturnsAllCommits

**Scenario**: Commits span multiple pages.

**Expected**: All pages are fetched and all commit SHAs are returned.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_ValidResponse_ReturnsPullRequests

**Scenario**: Valid response for GetPullRequests query.

**Expected**: Returns all pull requests.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_NoPullRequests_ReturnsEmptyList

**Scenario**: Response contains no pull requests.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_MissingData_ReturnsEmptyList

**Scenario**: Response is missing the data structure.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_HttpError_ReturnsEmptyList

**Scenario**: HTTP error response received.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_InvalidJson_ReturnsEmptyList

**Scenario**: Invalid JSON in response body.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_SinglePullRequest_ReturnsOnePullRequest

**Scenario**: Response contains exactly one pull request.

**Expected**: Returns a list with one pull request.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_MissingNumberOrTitle_SkipsInvalidNodes

**Scenario**: One pull request node is missing `number` or `title`.

**Expected**: Invalid node is skipped.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_WithPagination_ReturnsAllPullRequests

**Scenario**: Pull requests span multiple pages.

**Expected**: All pages are fetched and all pull requests are returned.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetPullRequestsAsync_ValidResponse_ReturnsPullRequestsWithBody

**Scenario**: Valid response includes pull requests with a body field.

**Expected**: Returned pull requests include the body content.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetReleasesAsync_ValidResponse_ReturnsReleaseTagNames

**Scenario**: Valid response for GetReleases query.

**Expected**: Returns all release tag names.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetReleasesAsync_NoReleases_ReturnsEmptyList

**Scenario**: Response contains no releases.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetReleasesAsync_MissingData_ReturnsEmptyList

**Scenario**: Response is missing the data structure.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetReleasesAsync_HttpError_ReturnsEmptyList

**Scenario**: HTTP error response received.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetReleasesAsync_InvalidJson_ReturnsEmptyList

**Scenario**: Invalid JSON in response body.

**Expected**: Returns an empty list.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetReleasesAsync_SingleRelease_ReturnsOneTagName

**Scenario**: Response contains exactly one release.

**Expected**: Returns a list with one tag name.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetReleasesAsync_MissingTagNameProperty_SkipsInvalidNodes

**Scenario**: One release node is missing the `tagName` property.

**Expected**: Invalid node is skipped.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

###### GitHubGraphQLClient_GetReleasesAsync_WithPagination_ReturnsAllReleases

**Scenario**: Releases span multiple pages.

**Expected**: All pages are fetched and all release tag names are returned.

**Requirement coverage**: `BuildMark-RepoConnectors-GitHubGraphQLClient`

##### Requirements Coverage

- **BuildMark-RepoConnectors-GitHubGraphQLClient**: All 41 tests across the five
  `GitHubGraphQLClient*Tests.cs` files
