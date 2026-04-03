# IRepoConnector and RepoConnectorBase

## Overview

`IRepoConnector` defines the contract that all repository connectors must satisfy.
`RepoConnectorBase` is an abstract class implementing `IRepoConnector` and providing
shared utilities used by concrete connectors.

## Interface

`IRepoConnector` exposes a single method:

| Member                              | Kind   | Description                      |
|-------------------------------------|--------|----------------------------------|
| `GetBuildInformationAsync(version)` | Method | Fetch complete build information |

## Base Class

`RepoConnectorBase` provides:

| Member                                       | Kind              | Description                                               |
|----------------------------------------------|-------------------|-----------------------------------------------------------|
| `RunCommandAsync(command, arguments)`        | Protected virtual | Delegates to `ProcessRunner.RunAsync` for shell commands  |
| `FindVersionIndex(versions, normalized)`     | Protected static  | Locates a version in a list by normalized version string  |

The `RunCommandAsync` method is `virtual` so that test subclasses can override it
with mock implementations that return fixed strings without spawning real processes.

## Interactions

| Unit / Subsystem    | Role                                              |
|---------------------|---------------------------------------------------|
| `ProcessRunner`     | Used by `RunCommandAsync` to execute shell commands |
| `GitHubRepoConnector` | Concrete implementation that inherits this base  |
| `MockRepoConnector` | Test implementation that overrides `RunCommandAsync` |
