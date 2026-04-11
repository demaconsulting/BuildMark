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

+------------------------------------------+-------------------+-------------------------------------------------------+
| Member                                   | Kind              | Description                                           |
+==========================================+===================+=======================================================+
| `Configure(rules, sections)`            | Public method     | Stores routing rules and section definitions         |
+------------------------------------------+-------------------+-------------------------------------------------------+
| `HasRules`                               | Protected bool    | True when at least one rule has been configured      |
+------------------------------------------+-------------------+-------------------------------------------------------+
| `ApplyRules(allItems)`                   | Protected method  | Routes items into sections using configured rules    |
+------------------------------------------+-------------------+-------------------------------------------------------+
| `RunCommandAsync(command, params arguments)` | Protected virtual | Delegates shell commands to ProcessRunner            |
+------------------------------------------+-------------------+-------------------------------------------------------+
| `FindVersionIndex(versions, target)`     | Protected static  | Locates version using semantic equality              |
+------------------------------------------+-------------------+-------------------------------------------------------+

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

### `FindVersionIndex(versions, targetVersion)`

Protected static method that finds the index of a `targetVersion` within a list of
`VersionTag` instances using **semantic VersionComparable equality**. This design
ensures that version tags with different prefixes but identical semantic versions
are considered equal:

- `"v1.2.3"` matches `"VER1.2.3"` and `"Release-1.2.3"`
- Comparison uses `versions[i].Semantic.Comparable.Equals(targetVersion.Semantic.Comparable)`
- This prevents version matching failures across different repository tag conventions

Returns the zero-based index if found, or -1 if no semantically equivalent version exists.

The `RunCommandAsync` method is `virtual` so that test subclasses can override it
with mock implementations that return fixed strings without spawning real processes.

## Interactions

- `ProcessRunner` is used by `RunCommandAsync` to execute shell commands in the
  Utilities subsystem.
- `GitHubRepoConnector` is a concrete implementation that inherits this base.
- `AzureDevOpsRepoConnector` is a concrete implementation that inherits this base.
- `MockRepoConnector` is a test implementation that overrides
  `RunCommandAsync`.
