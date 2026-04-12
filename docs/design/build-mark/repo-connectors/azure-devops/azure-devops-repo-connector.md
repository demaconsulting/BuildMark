# AzureDevOpsRepoConnector

## Overview

`AzureDevOpsRepoConnector` is the production unit in the RepoConnectors/AzureDevOps
subsystem. It implements `RepoConnectorBase` and uses `AzureDevOpsRestClient` to
query the Azure DevOps REST API for commits, tags, pull requests, and work items.

The unit reads the repository URL and current commit hash from Git, resolves the
Azure DevOps token from environment variables, and fetches all data needed to
construct a `BuildInformation` record.

## Data Model

### Authentication

The connector resolves the Azure DevOps token using the following priority order:

1. `AZURE_DEVOPS_PAT` environment variable — authenticated as Basic (PAT)
2. `AZURE_DEVOPS_TOKEN` environment variable — authenticated as Basic (PAT)
3. `AZURE_DEVOPS_EXT_PAT` environment variable — authenticated as Basic (PAT)
4. `SYSTEM_ACCESSTOKEN` environment variable (set automatically by Azure Pipelines)
   — authenticated as Bearer
5. Output of `az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv`
   — authenticated as Bearer

If no token is found, the connector throws `InvalidOperationException`.

### Work Item Type Mapping

Azure DevOps work item types are mapped to normalized types:

| Azure DevOps Work Item Types        | Normalized Type |
|-------------------------------------|-----------------|
| `Bug`, `Issue`                      | `"bug"`         |
| `User Story`, `Feature`, `Epic`     | `"feature"`     |
| `Task`, `Test Case`, etc.           | work item type  |

Work items of type `"bug"` are placed in the `Bugs` list; all others go to `Changes`.

When routing rules are configured via `.buildmark.yaml`, the type-derived
categorization is overridden by `RepoConnectorBase.ApplyRules`, which delegates to
`ItemRouter` to distribute all collected items into the configured report sections.

### Item Controls Override

After the work-item-type-derived categorization is determined, the connector calls
`ItemControlsParser.Parse(description)` on the description body of each work item
and pull request. If the parser returns a non-null `ItemControlsInfo`, the following
overrides are applied:

1. **`visibility: internal`** — The item is excluded from all report sections,
   regardless of its type.
2. **`visibility: public`** — The item is included in the report even if its
   type-derived category would otherwise suppress it.
3. **`type: bug`** — The item is placed in the `Bugs` list regardless of work item
   type.
4. **`type: feature`** — The item is placed in the `Changes` list regardless of
   work item type.
5. **`affected-versions`** — The parsed `VersionIntervalSet` is stored on the
   `ItemInfo.AffectedVersions` property.

In addition, the connector reads the following Azure DevOps custom fields from each
work item:

- `Custom.Visibility` — overrides the `visibility` control when present.
- `Custom.AffectedVersions` — overrides the `affected-versions` control when present.

Custom fields take precedence over buildmark blocks when both are present.

When neither a `buildmark` block nor custom fields are present, the existing
work-item-type-based rules apply unchanged.

### REST API Response Types

The `AzureDevOpsRestClient` returns the following record types:

- **`AzureDevOpsRepository`** — repository metadata including id, name, and remoteUrl.
- **`AzureDevOpsCommit`** — commit data including commitId and comment.
- **`AzureDevOpsGitCommitRef`** — minimal commit reference containing only commitId.
- **`AzureDevOpsPullRequest`** — pull request data including pullRequestId, title,
  url, status, lastMergeCommit (an `AzureDevOpsGitCommitRef` object representing
  the most recent merge commit), sourceRefName, and description. Exposes a computed
  `MergeCommitId` property that returns `LastMergeCommit?.CommitId`.
- **`AzureDevOpsWorkItem`** — work item data including id and a fields dictionary
  containing System.Title, System.WorkItemType, System.State, System.Description,
  Custom.Visibility, and Custom.AffectedVersions.
- **`AzureDevOpsWorkItemQuery`** — result of a WIQL query, containing a list of
  work item id references.
- **`AzureDevOpsRef`** — git reference including name, objectId, and
  peeledObjectId. Exposes a computed `CommitId` property that returns
  `PeeledObjectId ?? ObjectId`, resolving annotated tags to their commit SHA.
- **`AzureDevOpsCollectionResponse<T>`** — wraps paginated responses with a count
  and value list.

## Methods

### `GetBuildInformationAsync(Version? version) → BuildInformation`

Main entry point. Performs the following steps:

1. Get repository metadata (URL, branch, current commit hash) from Git.
2. Determine the organization URL, project, and repository name — from
   `AzureDevOpsConnectorConfig` if provided, otherwise parsed from the Git remote
   URL (supports `dev.azure.com`, `visualstudio.com`, and on-premises Azure
   DevOps Server URL formats by locating the `_git` path segment).
3. Resolve the Azure DevOps authentication token (see Authentication above).
4. Create an `AzureDevOpsRestClient` with the resolved organization URL and token.
5. Fetch all tags via `GET /git/repositories/{id}/refs?filter=tags&peelTags=true`.
   The `peelTags=true` parameter resolves annotated tags to their underlying commit
   SHA (returned in the `peeledObjectId` field). Using the REST API bypasses
   shallow-checkout limitations that would otherwise prevent Git from enumerating
   remote tags.
6. Fetch the complete commit history via `GET /git/repositories/{id}/commits`.
7. Fetch all pull requests via
   `GET /git/repositories/{id}/pullrequests?searchCriteria.status=all`.
8. Determine the target version tag (highest tag matching `version`, or latest).
9. Determine the baseline version tag (highest tag below the target).
10. Get all commits between the baseline and target tags.
11. Collect changes from pull requests merged in the commit range, applying item
    controls from description bodies and custom fields (`Custom.Visibility`,
    `Custom.AffectedVersions`).
12. Fetch linked work items for each PR via
    `GET /git/repositories/{id}/pullrequests/{prId}/workitems` and batch-fetch
    work item details via `GET /wit/workitems?ids={ids}&$expand=all`.
13. Collect known issues (open bugs not resolved at the time of the build) via a
    WIQL query, applying item controls from description bodies and custom fields.
    For each candidate bug, if `AffectedVersions` is declared on the `ItemInfo`,
    the bug is included as a known issue only when
    `AffectedVersions.Contains(toVersion)` is true. When no `AffectedVersions`
    is declared, the open/closed status is the sole indicator.
14. If routing rules are configured, call `ApplyRules` (inherited from
    `RepoConnectorBase`) to distribute all collected items into the configured
    report sections and populate `BuildInformation.RoutedSections`. If no rules
    are configured, items remain in the legacy `Changes`, `Bugs`, and `KnownIssues`
    lists. Return the assembled `BuildInformation` record.

## Interactions

- `AzureDevOpsConnectorConfig` is received from `RepoConnectorFactory` and overrides
  the organization URL, project, and repository name.
- `AzureDevOpsRestClient` executes REST API requests against the Azure DevOps API.
- `ProcessRunner` runs Git and `az` CLI commands to get repository metadata and
  fall-back authentication tokens.
- `ItemRouter` routes assembled items into report sections.
- `ItemControlsParser` parses buildmark blocks from work item and pull request
  description bodies.
- `WorkItemMapper` maps `AzureDevOpsWorkItem` records to `ItemInfo` records.
- `BuildInformation` is the output record assembled from fetched data.
