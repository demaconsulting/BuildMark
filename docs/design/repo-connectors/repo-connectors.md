# RepoConnectors Subsystem

## Overview

The RepoConnectors subsystem abstracts access to repository metadata for BuildMark.
It provides an interface and base class for repository connectors, a factory for
creating the appropriate connector, a mock connector for testing, and a process
runner for executing Git commands.

The primary production connector is `GitHubRepoConnector`, which queries the GitHub
GraphQL API to retrieve issues, pull requests, tags, and commits.

## Units

| Unit                   | File                                           | Responsibility                          |
|------------------------|------------------------------------------------|-----------------------------------------|
| `IRepoConnector`       | `RepoConnectors/IRepoConnector.cs`             | Interface for all repository connectors |
| `RepoConnectorBase`    | `RepoConnectors/RepoConnectorBase.cs`          | Base class with common connector logic  |
| `RepoConnectorFactory` | `RepoConnectors/RepoConnectorFactory.cs`       | Creates the appropriate connector       |
| `MockRepoConnector`    | `RepoConnectors/MockRepoConnector.cs`          | In-memory connector for testing         |
| `ProcessRunner`        | `RepoConnectors/ProcessRunner.cs`              | Runs Git commands via the shell         |
| `GitHubRepoConnector`  | `RepoConnectors/GitHubRepoConnector.cs`        | GitHub GraphQL API connector            |

## Interfaces

`IRepoConnector` defines the contract for all connectors:

| Member                              | Kind   | Description                      |
|-------------------------------------|--------|----------------------------------|
| `GetBuildInformationAsync(version)` | Method | Fetch complete build information |

## Interactions

| Unit / Subsystem   | Role                                                        |
|--------------------|-------------------------------------------------------------|
| `Program`          | Creates a connector via `RepoConnectorFactory` and calls it |
| `Validation`       | Uses `MockRepoConnector` directly for self-tests            |
| `BuildInformation` | The data record returned by connectors                      |
