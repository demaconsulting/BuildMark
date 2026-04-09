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

| Member                                    | Kind              | Description                                          |
|-------------------------------------------|-------------------|------------------------------------------------------|
| `Configure(rules, sections)`              | Public method     | Stores routing rules and section definitions         |
| `HasRules`                                | Protected bool    | True when at least one rule has been configured      |
| `ApplyRules(allItems)`                    | Protected method  | Routes items into sections using configured rules    |
| `RunCommandAsync(command, arguments)`     | Protected virtual | Delegates shell commands to `ProcessRunner.RunAsync` |
| `FindVersionIndex(versions, normalized)`  | Protected static  | Locates a version by normalized version string       |

### `Configure(rules, sections)`

Stores the routing rules and section definitions on the connector instance. Called
by `Program.ProcessBuildNotes` after the connector is created, passing `Rules` and
`Sections` from the loaded `.buildmark.yaml` configuration.

### `HasRules`

Protected boolean property that returns `true` when at least one rule has been
stored via `Configure`. Concrete connectors use this in `GetBuildInformationAsync`
to decide whether to call `ApplyRules` or use legacy categorization.

### `ApplyRules(allItems)`

Routes the provided items using `ItemRouter.Route`, then assembles an ordered list
of `(SectionId, SectionTitle, Items)` tuples following the configured section order.
Any items routed to section IDs not in the configured section list are appended at
the end using the section ID as the display title.

The `RunCommandAsync` method is `virtual` so that test subclasses can override it
with mock implementations that return fixed strings without spawning real processes.

## Interactions

- `ProcessRunner` is used by `RunCommandAsync` to execute shell commands in the
  Utilities subsystem.
- `GitHubRepoConnector` is a concrete implementation that inherits this base.
- `MockRepoConnector` is a test implementation that overrides
  `RunCommandAsync`.
