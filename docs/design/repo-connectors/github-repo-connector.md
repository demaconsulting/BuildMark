# GitHubRepoConnector

## Overview

`GitHubRepoConnector` is the production unit in the RepoConnectors subsystem. It
implements `RepoConnectorBase` and uses `GitHubGraphQLClient` to query the GitHub
GraphQL API for issues, pull requests, version tags, and commits.

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

## Methods

### `GetBuildInformationAsync(Version? version) → BuildInformation`

Main entry point. Performs the following steps:

1. Get repository metadata (URL, branch, current commit hash) from Git.
2. Parse the owner and repository name from the Git remote URL.
3. Resolve the GitHub authentication token.
4. Create a `GitHubGraphQLClient` with the resolved token.
5. Fetch all tags, commits, releases, issues, and pull requests via GraphQL.
6. Determine the target version tag (highest tag matching `version`, or latest).
7. Determine the baseline version tag (highest tag below the target).
8. Get all commits between the baseline and target.
9. Collect changes and bugs from pull requests merged in the commit range.
10. Collect known issues (open issues not included in this build).
11. Sort all lists chronologically.
12. Generate the full changelog URL from the baseline and target tags.
13. Return the assembled `BuildInformation` record.

## Interactions

| Unit / Subsystem      | Role                                            |
|-----------------------|-------------------------------------------------|
| `GitHubGraphQLClient` | Executes GraphQL queries against the GitHub API |
| `ProcessRunner`       | Runs Git commands to get repository metadata    |
| `BuildInformation`    | The output record assembled from fetched data   |
