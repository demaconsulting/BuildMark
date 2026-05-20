## RepoConnectors Subsystem

### Overview

The RepoConnectors subsystem abstracts access to repository metadata for BuildMark.
It provides an interface and base class for repository connectors, a factory for
creating the appropriate connector, a shared `ItemRouter` for routing items to
report sections, and `ItemControlsParser` with `ItemControlsInfo` for parsing
metadata embedded in repository item descriptions.

Concrete connector implementations are organized into subsystems: the `GitHub`
subsystem provides the production GitHub GraphQL connector, the `AzureDevOps`
subsystem provides the production Azure DevOps REST connector, and the `Mock`
subsystem provides the in-memory connector used by the built-in `--validate`
self-test.

### Units

- `IRepoConnector` - `RepoConnectors/IRepoConnector.cs` - interface for all
  repository connectors
- `RepoConnectorBase` - `RepoConnectors/RepoConnectorBase.cs` - base class with
  common connector logic
- `RepoConnectorFactory` - `RepoConnectors/RepoConnectorFactory.cs` - creates
  the appropriate connector
- `ItemRouter` - `RepoConnectors/ItemRouter.cs` - shared item-routing logic for
  all connectors
- `ItemControlsParser` - `RepoConnectors/ItemControlsParser.cs` - parses
  buildmark blocks from item description bodies
- `ItemControlsInfo` - `RepoConnectors/ItemControlsInfo.cs` - data record holding
  visibility, type, and version-set values

### Subsystems

- **`GitHub`** - `RepoConnectors/GitHub/` - `GitHubRepoConnector`, `GitHubGraphQLClient`,
  `GitHubGraphQLTypes`
- **`AzureDevOps`** - `RepoConnectors/AzureDevOps/` - `AzureDevOpsRepoConnector`,
  `AzureDevOpsRestClient`, `AzureDevOpsApiTypes`, `WorkItemMapper`
- **`Mock`** - `RepoConnectors/Mock/` - `MockRepoConnector` (used by `--validate` self-test)

### Interfaces

`IRepoConnector` defines the contract for all connectors:

| Member                              | Kind   | Description                      |
|-------------------------------------|--------|----------------------------------|
| `GetBuildInformationAsync(version)` | Method | Fetch complete build information |

### Design

The RepoConnectors subsystem separates the connector contract, shared
infrastructure, and concrete implementations into three layers:

1. **Contract layer**: `IRepoConnector` defines the single public method all
   connectors must implement. `RepoConnectorFactory` resolves the appropriate
   concrete connector at runtime without the caller needing to know which
   platform is in use.

2. **Base layer**: `RepoConnectorBase` provides shared behavior inherited by
   all production connectors — token resolution is handled in the concrete
   classes, while `Configure`, `HasRules`, `ApplyRules`, `FindVersionIndex`, and
   `RunCommandAsync` are provided by the base. `ItemControlsParser` and
   `ItemControlsInfo` are shared utilities called by every connector to apply
   buildmark block overrides per item. `ItemRouter` is the central routing
   engine called by `RepoConnectorBase.ApplyRules` to distribute items into
   configured report sections.

3. **Implementation layer**: `GitHub`, `AzureDevOps`, and `Mock` child subsystems
   each contain a connector that inherits from `RepoConnectorBase` together with
   platform-specific client and type definitions. Each connector fetches platform
   data, normalizes it into `ItemInfo` records, applies item-controls overrides,
   calls `ApplyRules` when routing is configured, and returns a `BuildInformation`
   record.

### Interactions

| Unit / Subsystem             | Role                                                                   |
|------------------------------|------------------------------------------------------------------------|
| `Program`                    | Creates a connector via `RepoConnectorFactory` and calls it            |
| `Program`                    | Passes `ConnectorConfig` (from `result.Config.Connector`) to           |
|                              | `RepoConnectorFactory`, which forwards platform-specific config to     |
|                              | the appropriate connector implementation                               |
| `Validation`                 | Uses `MockRepoConnector` directly for self-tests                       |
| `ItemControlsParser`         | Called by `GitHubRepoConnector` and `AzureDevOpsRepoConnector`         |
|                              | on each description body                                               |
| `BuildInformation`           | The data record returned by connectors                                 |
