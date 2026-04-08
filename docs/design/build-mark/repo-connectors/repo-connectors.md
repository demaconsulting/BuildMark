# RepoConnectors Subsystem

## Overview

The RepoConnectors subsystem abstracts access to repository metadata for BuildMark.
It provides an interface and base class for repository connectors, a factory for
creating the appropriate connector, a shared `ItemRouter` for routing items to
report sections, and `ItemControlsParser` with `ItemControlsInfo` for parsing
metadata embedded in repository item descriptions.

Concrete connector implementations are organized into subsystems: the `GitHub`
subsystem provides the production GitHub GraphQL connector, and the `Mock`
subsystem provides the in-memory connector used by the built-in `--validate`
self-test. Future connectors (e.g. Azure DevOps) will each have their own
subsystem alongside `GitHub` and `Mock`.

## Units

- `IRepoConnector` — `RepoConnectors/IRepoConnector.cs` — interface for all
  repository connectors
- `RepoConnectorBase` — `RepoConnectors/RepoConnectorBase.cs` — base class with
  common connector logic
- `RepoConnectorFactory` — `RepoConnectors/RepoConnectorFactory.cs` — creates
  the appropriate connector
- `ItemRouter` — `RepoConnectors/ItemRouter.cs` — shared item-routing logic for
  all connectors
- `ItemControlsParser` — `ItemControls/ItemControlsParser.cs` — parses
  buildmark blocks from item description bodies
- `ItemControlsInfo` — `ItemControls/ItemControlsInfo.cs` — data record holding
  visibility, type, and version-set values

## Subsystems

| Subsystem | Folder                      | Contents                                                           |
|-----------|-----------------------------|--------------------------------------------------------------------|
| `GitHub`  | `RepoConnectors/GitHub/`    | `GitHubRepoConnector`, `GitHubGraphQLClient`, `GitHubGraphQLTypes` |
| `Mock`    | `RepoConnectors/Mock/`      | `MockRepoConnector` (used by `--validate` self-test)               |

## Interfaces

`IRepoConnector` defines the contract for all connectors:

| Member                              | Kind   | Description                      |
|-------------------------------------|--------|----------------------------------|
| `GetBuildInformationAsync(version)` | Method | Fetch complete build information |

## Interactions

| Unit / Subsystem      | Role                                                                   |
|-----------------------|------------------------------------------------------------------------|
| `Program`             | Creates a connector via `RepoConnectorFactory` and calls it            |
| `Program`             | Passes `GitHubConnectorConfig` (from `result.Config.Connector.GitHub`) |
|                       | to `GitHubRepoConnector` for owner, repo, and base-URL settings        |
| `Validation`          | Uses `MockRepoConnector` directly for self-tests                       |
| `ItemControlsParser`  | Called by `GitHubRepoConnector` on each description body               |
| `BuildInformation`    | The data record returned by connectors                                 |
