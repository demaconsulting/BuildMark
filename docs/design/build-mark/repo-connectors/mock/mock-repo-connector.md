# MockRepoConnector

## Overview

`MockRepoConnector` is an in-memory implementation of `IRepoConnector` used for
self-validation and unit testing. It returns a fixed, deterministic dataset
without making any network or filesystem calls.

`MockRepoConnector` lives in production code - not in the test project - because
the `--validate` flag must work in any deployment without requiring a separate test
assembly or external tooling.

## Data Model

The connector holds hard-coded mappings used to build the `BuildInformation` response:

| Field                    | Type                                      | Description                                  |
|--------------------------|-------------------------------------------|----------------------------------------------|
| `_issueTitles`           | `Dictionary<string, string>`              | Issue ID -> title                            |
| `_issueTypes`            | `Dictionary<string, string>`              | Issue ID -> type (bug/feature/documentation) |
| `_pullRequestIssues`     | `Dictionary<string, List<string>>`        | PR ID -> linked issue IDs                    |
| `_tagHashes`             | `Dictionary<string, string>`              | Tag name -> commit hash                      |
| `_openIssues`            | `List<string>`                            | IDs of issues that remain open               |
| `_issueAffectedVersions` | `Dictionary<string, VersionIntervalSet>`  | Issue ID -> declared affected-versions range |

## Methods

### `GetBuildInformationAsync(version?) → BuildInformation`

Resolves tag history and the current commit hash from the internal dictionaries,
determines the target and baseline versions, collects changes and known issues,
and returns a fully populated `BuildInformation` record. The logic mirrors the
production GitHubRepoConnector flow but operates entirely on in-memory data.

When collecting known issues, **all** issues (open and closed) are considered:

- If `AffectedVersions` is non-null, the bug is included if and only if
  `AffectedVersions.Contains(targetVersion)` is true, regardless of open/closed
  state (models a closed bug never back-ported to an older branch).
- If `AffectedVersions` is null, only open bugs are included.

When routing rules have been configured via `Configure`, `GetBuildInformationAsync`
collects all items and passes them to `ApplyRules` (inherited from `RepoConnectorBase`)
to produce the `RoutedSections` list. If no rules are configured, the legacy
categorization into `Changes` and `Bugs` is used.

## Error Conditions

`GetBuildInformationAsync` throws `InvalidOperationException` in the following scenarios:

1. **No version tags exist in data and no version argument provided** - throws with message:
   `"No tags found in repository and no version specified. Please provide a version parameter."`
2. **Current commit does not match any tag and no version argument provided** - throws with message:
   `"Target version not specified and current commit does not match any tag. Please provide a version parameter."`

These conditions mirror the equivalent error paths in the production `GitHubRepoConnector`
and `AzureDevOpsRepoConnector`, making the mock suitable for testing error-handling code
paths as well as the happy path.

## Interactions

| Unit / Subsystem         | Role                                                                              |
|--------------------------|-----------------------------------------------------------------------------------|
| `RepoConnectorBase`      | Base class providing `FindVersionIndex` and command delegation                    |
| `Configure` (inherited)  | Called by `Validation.RunRulesRouting` to test rules-based routing                |
| `Validation`             | Instantiates `MockRepoConnector` directly for self-tests                          |
