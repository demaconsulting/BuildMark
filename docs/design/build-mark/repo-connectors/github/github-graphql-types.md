#### GitHubGraphQLTypes

![GitHub Structure](../../../generated/GitHubView.svg)

##### Purpose

`GitHubGraphQLTypes` is the collection of internal C# record types used by the GitHub subsystem
to represent GraphQL request and response payloads. These types allow `GitHubGraphQLClient` to
deserialize GitHub API responses into strongly typed objects that `GitHubRepoConnector` can process
safely and predictably.

##### Data Model

**PageInfo.HasNextPage**: `bool` — Indicates whether additional pages are available in a paginated
connection.

**PageInfo.EndCursor**: `string?` — Cursor value to supply in the next page request.

**CommitNode.Oid**: `string?` — Git object ID (SHA) of the commit; used to match commits in the
target range.

**TagNode.Name**: `string?` — Tag name (e.g. `v1.0.0`).

**TagNode.Target**: `TagTargetData?` — Target object for the tag, carrying the commit SHA.

**TagTargetData.Oid**: `string?` — Commit SHA the tag points to.

**ReleaseNode.TagName**: `string?` — Tag name associated with the GitHub release.

**PullRequestNode.Number**: `int?` — Pull request number.

**PullRequestNode.Title**: `string?` — Pull request title.

**PullRequestNode.Url**: `string?` — HTML URL of the pull request.

**PullRequestNode.Merged**: `bool` — Whether the pull request has been merged.

**PullRequestNode.MergeCommit**: `PullRequestMergeCommit?` — Merge commit reference; `null` when
unmerged.

**PullRequestNode.HeadRefOid**: `string?` — Commit SHA of the pull request head; used to identify
commit ranges for unmerged pull requests.

**PullRequestNode.Labels**: `PullRequestLabelsConnection?` — Labels assigned to the pull request;
used for type normalization.

**PullRequestNode.Body**: `string?` — Pull request description body; passed to
`ItemControlsParser.Parse`.

**PullRequestMergeCommit.Oid**: `string?` — Commit SHA of the merge commit.

**PullRequestLabel.Name**: `string?` — Label name (e.g. `bug`, `feature`).

**IssueNode.Number**: `int?` — Issue number.

**IssueNode.Title**: `string?` — Issue title.

**IssueNode.Url**: `string?` — HTML URL of the issue.

**IssueNode.State**: `string?` — Issue state (`OPEN` or `CLOSED`).

**IssueNode.Labels**: `IssueLabelsConnection?` — Labels assigned to the issue; used for type
normalization.

**IssueNode.Body**: `string?` — Issue description body; passed to `ItemControlsParser.Parse`.

**IssueLabel.Name**: `string?` — Label name.

**LinkedIssueReference.Number**: `int?` — Issue number of a linked closing issue reference.

##### Key Methods

N/A — `GitHubGraphQLTypes` is a collection of immutable C# record types used purely for GraphQL
deserialization. No methods beyond C#-generated record members are defined.

##### Error Handling

N/A — These are immutable record types used purely for deserialization. No methods detect or
propagate errors.

##### Dependencies

- **GraphQL.Client.Serializer.SystemTextJson** — runtime library used by `GitHubGraphQLClient` to
  deserialize these records from GraphQL HTTP responses.

##### Callers

- **GitHubGraphQLClient** — uses these records as deserialization targets for GraphQL HTTP
  responses.
- **GitHubRepoConnector** — consumes `PullRequestNode` and `IssueNode` data returned by
  `GitHubGraphQLClient`.
