# MockRepoConnector

## Overview

`MockRepoConnector` is an in-memory implementation of `IRepoConnector` used for
self-validation and unit testing. It returns a fixed, deterministic dataset
without making any network or filesystem calls.

`MockRepoConnector` lives in production code — not in the test project — because
the `--validate` flag must work in any deployment without requiring a separate test
assembly or external tooling.

## Data Model

The connector holds hard-coded mappings used to build the `BuildInformation` response:

| Field                | Type                               | Description                                  |
|----------------------|------------------------------------|----------------------------------------------|
| `_issueTitles`       | `Dictionary<string, string>`       | Issue ID -> title                            |
| `_issueTypes`        | `Dictionary<string, string>`       | Issue ID -> type (bug/feature/documentation) |
| `_pullRequestIssues` | `Dictionary<string, List<string>>` | PR ID -> linked issue IDs                    |
| `_tagHashes`         | `Dictionary<string, string>`       | Tag name -> commit hash                      |
| `_openIssues`        | `List<string>`                     | IDs of issues that remain open               |

## Methods

### `GetBuildInformationAsync(version?) → BuildInformation`

Resolves tag history and the current commit hash from the internal dictionaries,
determines the target and baseline versions, collects changes and known issues,
and returns a fully populated `BuildInformation` record. The logic mirrors the
production GitHubRepoConnector flow but operates entirely on in-memory data.

When routing rules have been configured via `Configure`, `GetBuildInformationAsync`
collects all items and passes them to `ApplyRules` (inherited from `RepoConnectorBase`)
to produce the `RoutedSections` list. If no rules are configured, the legacy
categorization into `Changes` and `Bugs` is used.

## Interactions

| Unit / Subsystem    | Role                                                                              |
|---------------------|-----------------------------------------------------------------------------------|
| `RepoConnectorBase` | Base class providing `FindVersionIndex` and command delegation                    |
| `Configure` (inherited) | Called by `Validation.RunRulesRouting` to test rules-based routing            |
| `Validation`        | Instantiates `MockRepoConnector` directly for self-tests                          |
