# RepoConnectorFactory

## Overview

`RepoConnectorFactory` is a static factory class that creates the appropriate
`IRepoConnector` implementation based on the runtime environment.

## Methods

### `Create() → IRepoConnector`

Returns a new `GitHubRepoConnector` if any of the following conditions are met:

1. The `GITHUB_ACTIONS` environment variable is non-empty.
2. The `GITHUB_WORKSPACE` environment variable is non-empty.
3. The git remote URL (obtained by running `git remote get-url origin`) contains `github.com`.

If none of the above conditions are met the factory defaults to returning a new
`GitHubRepoConnector` (the only connector implementation currently available).

## Interactions

| Unit / Subsystem      | Role                                                        |
|-----------------------|-------------------------------------------------------------|
| `IRepoConnector`      | Return type of `Create`                                     |
| `GitHubRepoConnector` | The concrete connector returned by `Create`                 |
| `ProcessRunner`       | Used via `TryRunAsync` to inspect the git remote URL        |
| `Program`             | Calls `RepoConnectorFactory.Create()` to obtain a connector |
