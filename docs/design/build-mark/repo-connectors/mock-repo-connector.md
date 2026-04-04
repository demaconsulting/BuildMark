# MockRepoConnector

## Overview

`MockRepoConnector` is an in-memory implementation of `IRepoConnector` used for
self-validation and unit testing. It returns a fixed, deterministic dataset
without making any network or filesystem calls.

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

## Interactions

| Unit / Subsystem    | Role                                                           |
|---------------------|----------------------------------------------------------------|
| `RepoConnectorBase` | Base class providing `FindVersionIndex` and command delegation |
| `Validation`        | Instantiates `MockRepoConnector` directly for self-tests       |
