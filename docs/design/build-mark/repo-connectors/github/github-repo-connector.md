# GitHubRepoConnector

## Overview

`GitHubRepoConnector` is the production unit in the RepoConnectors/GitHub
sub-subsystem. It implements `RepoConnectorBase` and uses `GitHubGraphQLClient` to
query the GitHub GraphQL API for issues, pull requests, version tags, and commits.

The unit reads the repository URL and current commit hash from Git, resolves the
GitHub token from environment variables, and fetches all data needed to construct
a `BuildInformation` record.

## Data Model

### Authentication

The connector resolves the GitHub token using the following priority order:

1. `GH_TOKEN` environment variable
2. `GITHUB_TOKEN` environment variable
3. Output of `gh auth token` command

If no token is found, the connector throws `InvalidOperationException`.

### Label Mapping

GitHub issue and pull request labels are mapped to normalized types:

| GitHub Labels                              | Normalized Type |
|--------------------------------------------|-----------------|
| `bug`, `defect`                            | `"bug"`         |
| `feature`, `enhancement`                   | `"feature"`     |
| `documentation`, `performance`, `security` | label name      |

Items labelled as `"bug"` are placed in the `Bugs` list; all others go to `Changes`.

### GraphQL Response Types

The `GitHubGraphQLClient` returns `PullRequestNode` and `IssueNode` records that
must include the `body` field so the connector can pass description text to
`ItemControlsParser`:

**`PullRequestNode`** (updated to include `Body`):

```csharp
internal record PullRequestNode(
    int? Number,
    string? Title,
    string? Url,
    bool Merged,
    PullRequestMergeCommit? MergeCommit,
    string? HeadRefOid,
    PullRequestLabelsConnection? Labels,
    string? Body);
```

**`IssueNode`** (updated to include `Body`):

```csharp
internal record IssueNode(
    int? Number,
    string? Title,
    string? Url,
    string? State,
    IssueLabelsConnection? Labels,
    string? Body);
```

Both `GetPullRequestsAsync` and `GetAllIssuesAsync` must include `body` in their
GraphQL field selections.

### Item Controls Override

After the label-derived type is determined, the connector calls
`ItemControlsParser.Parse(body)` on the description body of each issue and pull
request. If the parser returns a non-null `ItemControlsInfo`, the following
overrides are applied:

1. **`visibility: internal`** — The item is excluded from all report sections,
   regardless of its labels or type.
2. **`visibility: public`** — The item is included in the report even if its
   label-derived type is `"other"`.
3. **`type: bug`** — The item is placed in the `Bugs` list regardless of labels.
4. **`type: feature`** — The item is placed in the `Changes` list regardless of
   labels.
5. **`affected-versions`** — The parsed `VersionIntervalSet` is stored on the
   `ItemInfo.AffectedVersions` property.

When no `buildmark` block is present, the existing label-based rules apply
unchanged.

## Methods

### `GetBuildInformationAsync(Version? version) → BuildInformation`

Main entry point. Performs the following steps:

1. Get repository metadata (URL, branch, current commit hash) from Git.
2. Determine the owner and repository name — from `GitHubConnectorConfig.Owner`
   and `GitHubConnectorConfig.Repo` if provided, otherwise parsed from the Git
   remote URL.
3. Resolve the GitHub authentication token (see Authentication above).
4. Create a `GitHubGraphQLClient` with the resolved token. If
   `GitHubConnectorConfig.BaseUrl` is set, use that URL as the GraphQL endpoint
   instead of the default `https://api.github.com/graphql` (supports GitHub
   Enterprise).
5. Fetch all tags, commits, releases, issues (with body), and pull requests (with
   body) via GraphQL.
6. Determine the target version tag (highest tag matching `version`, or latest).
7. Determine the baseline version tag (highest tag below the target).
8. Get all commits between the baseline and target.
9. Collect changes and bugs from pull requests merged in the commit range,
   applying item controls overrides from description bodies.
10. Collect known issues (open issues not included in this build), applying item
    controls overrides from description bodies.
11. Sort all lists chronologically.
12. Generate the full changelog URL from the baseline and target tags.
13. Return the assembled `BuildInformation` record.

## Interactions

| Unit / Subsystem        | Role                                                               |
|-------------------------|--------------------------------------------------------------------|
| `GitHubConnectorConfig` | Received from `RepoConnectorFactory`; overrides owner, repo, URL  |
| `GitHubGraphQLClient`   | Executes GraphQL queries against the GitHub API                    |
| `ProcessRunner`         | Runs Git commands to get repository metadata (via Utilities)       |
| `ItemRouter`            | Routes assembled items into report sections                        |
| `ItemControlsParser`    | Parses buildmark blocks from issue and PR description bodies       |
| `BuildInformation`      | The output record assembled from fetched data                      |
